﻿using System.Data;
using Microsoft.Extensions.Logging;
using Plus.Communication.Packets.Incoming;
using Plus.Communication.Packets.Outgoing.Inventory.Purse;
using Plus.Communication.Packets.Outgoing.Quests;
using Plus.Database;
using Plus.HabboHotel.Friends;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Users.Messenger;

namespace Plus.HabboHotel.Quests;

public class QuestManager : IQuestManager
{
    private readonly IDatabase _database;
    private readonly IMessengerDataLoader _messengerDataLoader;
    private readonly ILogger<QuestManager> _logger;
    private readonly Dictionary<string, int> _questCount;

    private readonly Dictionary<int, Quest> _quests;

    public QuestManager(IDatabase database, IMessengerDataLoader messengerDataLoader, ILogger<QuestManager> logger)
    {
        _database = database;
        _messengerDataLoader = messengerDataLoader;
        _logger = logger;
        _quests = new();
        _questCount = new();
    }

    public void Init()
    {
        if (_quests.Count > 0)
            _quests.Clear();
        using (var dbClient = _database.GetQueryReactor())
        {
            dbClient.SetQuery("SELECT `id`,`type`,`level_num`,`goal_type`,`goal_data`,`action`,`pixel_reward`,`data_bit`,`reward_type`,`timestamp_unlock`,`timestamp_lock` FROM `quests`");
            var dTable = dbClient.GetTable();
            if (dTable != null)
            {
                foreach (DataRow dRow in dTable.Rows)
                {
                    var id = Convert.ToInt32(dRow["id"]);
                    var category = Convert.ToString(dRow["type"]);
                    var num = Convert.ToInt32(dRow["level_num"]);
                    var type = Convert.ToInt32(dRow["goal_type"]);
                    var goalData = Convert.ToInt32(dRow["goal_data"]);
                    var name = Convert.ToString(dRow["action"]);
                    var reward = Convert.ToInt32(dRow["pixel_reward"]);
                    var dataBit = Convert.ToString(dRow["data_bit"]);
                    var rewardtype = Convert.ToInt32(dRow["reward_type"].ToString());
                    var time = Convert.ToInt32(dRow["timestamp_unlock"]);
                    var locked = Convert.ToInt32(dRow["timestamp_lock"]);
                    _quests.Add(id, new(id, category, num, (QuestType)type, goalData, name, reward, dataBit, rewardtype, time, locked));
                    AddToCounter(category);
                }
            }
        }
        _logger.LogInformation("Quest Manager -> LOADED");
    }

    private void AddToCounter(string category)
    {
        var count = 0;
        if (_questCount.TryGetValue(category, out count))
            _questCount[category] = count + 1;
        else
            _questCount.Add(category, 1);
    }

    public Quest GetQuest(int id)
    {
        _quests.TryGetValue(id, out var quest);
        return quest;
    }

    public int GetAmountOfQuestsInCategory(string category)
    {
        _questCount.TryGetValue(category, out var count);
        return count;
    }

    public void ProgressUserQuest(GameClient session, QuestType type, int data = 0)
    {
        if (session == null || session.GetHabbo() == null || session.GetHabbo().HabboStats.QuestId <= 0) return;
        var quest = GetQuest(session.GetHabbo().HabboStats.QuestId);
        if (quest == null || quest.GoalType != type) return;
        var currentProgress = session.GetHabbo().GetQuestProgress(quest.Id);
        var totalProgress = currentProgress;
        var completeQuest = false;
        switch (type)
        {
            default:
                totalProgress++;
                if (totalProgress >= quest.GoalData) completeQuest = true;
                break;
            case QuestType.ExploreFindItem:
                if (data != quest.GoalData)
                    return;
                totalProgress = Convert.ToInt32(quest.GoalData);
                completeQuest = true;
                break;
            case QuestType.StandOn:
                if (data != quest.GoalData)
                    return;
                totalProgress = Convert.ToInt32(quest.GoalData);
                completeQuest = true;
                break;
            case QuestType.XmasParty:
                totalProgress++;
                if (totalProgress == quest.GoalData)
                    completeQuest = true;
                break;
            case QuestType.GiveItem:
                if (data != quest.GoalData)
                    return;
                totalProgress = Convert.ToInt32(quest.GoalData);
                completeQuest = true;
                break;
        }
        using (var dbClient = _database.GetQueryReactor())
        {
            dbClient.RunQuery($"UPDATE `user_quests` SET `progress` = '{totalProgress}' WHERE `user_id` = '{session.GetHabbo().Id}' AND `quest_id` = '{quest.Id}' LIMIT 1");
            if (completeQuest)
                dbClient.RunQuery($"UPDATE `user_statistics` SET `quest_id` = '0' WHERE `id` = '{session.GetHabbo().Id}' LIMIT 1");
        }
        session.GetHabbo().Quests[session.GetHabbo().HabboStats.QuestId] = totalProgress;
        session.Send(new QuestStartedComposer(session, quest, this));
        if (completeQuest)
        {
            _messengerDataLoader.BroadcastStatusUpdate(session.GetHabbo(), MessengerEventTypes.QuestCompleted, $"{quest.Category}.{quest.Name}");
            session.GetHabbo().HabboStats.QuestId = 0;
            session.GetHabbo().QuestLastCompleted = quest.Id;
            session.Send(new QuestCompletedComposer(session, quest, this));
            session.GetHabbo().Duckets += quest.Reward;
            session.Send(new HabboActivityPointNotificationComposer(session.GetHabbo().Duckets, quest.Reward));
            GetList(session, null);
        }
    }

    public Quest GetNextQuestInSeries(string category, int number)
    {
        foreach (var quest in _quests.Values)
            if (quest.Category == category && quest.Number == number)
                return quest;
        return null;
    }

    public void GetList(GameClient session, ClientPacket message)
    {
        var userQuestGoals = new Dictionary<string, int>();
        var userQuests = new Dictionary<string, Quest>();
        foreach (var quest in _quests.Values.ToList())
        {
            if (quest.Category.Contains("xmas2012"))
                continue;
            if (!userQuestGoals.ContainsKey(quest.Category))
            {
                userQuestGoals.Add(quest.Category, 1);
                userQuests.Add(quest.Category, null);
            }
            if (quest.Number >= userQuestGoals[quest.Category])
            {
                var userProgress = session.GetHabbo().GetQuestProgress(quest.Id);
                if (session.GetHabbo().HabboStats.QuestId != quest.Id && userProgress >= quest.GoalData) userQuestGoals[quest.Category] = quest.Number + 1;
            }
        }
        foreach (var quest in _quests.Values.ToList())
        {
            foreach (var goal in userQuestGoals)
            {
                if (quest.Category.Contains("xmas2012"))
                    continue;
                if (quest.Category == goal.Key && quest.Number == goal.Value)
                {
                    userQuests[goal.Key] = quest;
                    break;
                }
            }
        }
        session.Send(new QuestListComposer(session, message != null, userQuests, this));
    }

    public void QuestReminder(GameClient session, int questId)
    {
        var quest = GetQuest(questId);
        if (quest == null)
            return;
        session.Send(new QuestStartedComposer(session, quest, this));
    }
}