﻿using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Quests;

namespace Plus.Communication.Packets.Outgoing.Quests;

public class QuestListComposer : IServerPacket
{
    private readonly GameClient _session;
    private readonly bool _send;
    private readonly Dictionary<string, Quest> _userQuests;
    private readonly IQuestManager _questManager;
    public uint MessageId => ServerPacketHeader.QuestListComposer;

    public QuestListComposer(GameClient session, bool send, Dictionary<string, Quest> userQuests, IQuestManager questManager)
    {
        _session = session;
        _send = send;
        _userQuests = userQuests;
        _questManager = questManager;
    }

    public void Compose(IOutgoingPacket packet)
    {
        packet.WriteInteger(_userQuests.Count);

        // Active ones first
        foreach (var userQuest in _userQuests)
        {
            if (userQuest.Value == null)
                continue;
            SerializeQuest(packet, _session, userQuest.Value, userQuest.Key);
        }

        // Dead ones last
        foreach (var userQuest in _userQuests)
        {
            if (userQuest.Value != null)
                continue;
            SerializeQuest(packet, _session, userQuest.Value, userQuest.Key);
        }
        packet.WriteBoolean(_send);
    }

    private void SerializeQuest(IOutgoingPacket message, GameClient session, Quest quest, string category)
    {
        if (message == null || session == null)
            return;
        var amountInCat = _questManager.GetAmountOfQuestsInCategory(category);
        var number = quest == null ? amountInCat : quest.Number - 1;
        var userProgress = quest == null ? 0 : session.GetHabbo().GetQuestProgress(quest.Id);
        if (quest != null && quest.IsCompleted(userProgress))
            number++;
        message.WriteString(category);
        message.WriteInteger(quest == null ? 0 : quest.Category.Contains("xmas2012") ? 0 : number); // Quest progress in this cat
        message.WriteInteger(quest == null ? 0 : quest.Category.Contains("xmas2012") ? 0 : amountInCat); // Total quests in this cat
        message.WriteInteger(quest?.RewardType ?? 3); // Reward type (1 = Snowflakes, 2 = Love hearts, 3 = Pixels, 4 = Seashells, everything else is pixels
        message.WriteInteger(quest?.Id ?? 0); // Quest id
        message.WriteBoolean(quest != null && session.GetHabbo().HabboStats.QuestId == quest.Id); // Quest started
        message.WriteString(quest == null ? string.Empty : quest.ActionName);
        message.WriteString(quest == null ? string.Empty : quest.DataBit);
        message.WriteInteger(quest?.Reward ?? 0);
        message.WriteString(quest == null ? string.Empty : quest.Name);
        message.WriteInteger(userProgress); // Current progress
        message.WriteInteger(quest?.GoalData ?? 0); // Target progress
        message.WriteInteger(quest?.TimeUnlock ?? 0); // "Next quest available countdown" in seconds
        message.WriteString("");
        message.WriteString("");
        message.WriteBoolean(true);
    }
}