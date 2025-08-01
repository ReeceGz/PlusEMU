using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Quests;

namespace Plus.Communication.Packets.Outgoing.Quests;

public class QuestCompletedComposer : IServerPacket
{
    private readonly GameClient _session;
    private readonly Quest _quest;
    private readonly IQuestManager _questManager;

    public uint MessageId => ServerPacketHeader.QuestCompletedComposer;

    public QuestCompletedComposer(GameClient session, Quest quest, IQuestManager questManager)
    {
        _session = session;
        _quest = quest;
        _questManager = questManager;
    }

    public void Compose(IOutgoingPacket packet)
    {
        var amountInCat = _questManager.GetAmountOfQuestsInCategory(_quest.Category);
        var number = _quest?.Number ?? amountInCat;
        var userProgress = _quest == null ? 0 : _session.GetHabbo().GetQuestProgress(_quest.Id);
        packet.WriteString(_quest.Category);
        packet.WriteInteger(number); // Quest progress in this cat
        packet.WriteInteger(_quest.Name.Contains("xmas2012") ? 1 : amountInCat); // Total quests in this cat
        packet.WriteInteger(_quest?.RewardType ?? 3); // Reward type (1 = Snowflakes, 2 = Love hearts, 3 = Pixels, 4 = Seashells, everything else is pixels
        packet.WriteInteger(_quest?.Id ?? 0); // Quest id
        packet.WriteBoolean(_quest == null ? false : _session.GetHabbo().HabboStats.QuestId == _quest.Id); // Quest started
        packet.WriteString(_quest == null ? string.Empty : _quest.ActionName);
        packet.WriteString(_quest == null ? string.Empty : _quest.DataBit);
        packet.WriteInteger(_quest?.Reward ?? 0);
        packet.WriteString(_quest == null ? string.Empty : _quest.Name);
        packet.WriteInteger(userProgress); // Current progress
        packet.WriteInteger(_quest?.GoalData ?? 0); // Target progress
        packet.WriteInteger(_quest?.TimeUnlock ?? 0); // "Next quest available countdown" in seconds
        packet.WriteString("");
        packet.WriteString("");
        packet.WriteBoolean(true); // ?
        packet.WriteBoolean(true); // Activate next quest..
    }
}