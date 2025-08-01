using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Groups;
using Plus.HabboHotel.Users;
using Plus.Utilities;

namespace Plus.Communication.Packets.Outgoing.Users;

public class ProfileInformationComposer : IServerPacket
{
    private readonly Habbo _habbo;
    private readonly GameClient _session;
    private readonly IGameClientManager _clientManager;
    private readonly IGroupManager _groupManager;
    private readonly List<Group> _groups;
    private readonly int _friendCount;
    private readonly HabboStats _habboStats;

    public uint MessageId => ServerPacketHeader.ProfileInformationComposer;

    public ProfileInformationComposer(Habbo habbo, GameClient session, List<Group> groups, int friendCount, HabboStats habboStats, IGameClientManager clientManager, IGroupManager groupManager)
    {
        _habbo = habbo;
        _session = session;
        _groups = groups;
        _friendCount = friendCount;
        _habboStats = habboStats;
        _clientManager = clientManager;
        _groupManager = groupManager;
    }

    public void Compose(IOutgoingPacket packet)
    {
        var origin = new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds(_habbo.AccountCreated);
        packet.WriteInteger(_habbo.Id);
        packet.WriteString(_habbo.Username);
        packet.WriteString(_habbo.Look);
        packet.WriteString(_habbo.Motto);
        packet.WriteString(origin.ToString("dd/MM/yyyy"));
        packet.WriteInteger(_habboStats?.AchievementPoints ?? 0);
        packet.WriteInteger(_friendCount); // Friend Count
        packet.WriteBoolean(_habbo.Id != _session.GetHabbo().Id && _session.GetHabbo().Messenger.FriendshipExists(_habbo.Id)); //  Is friend
        packet.WriteBoolean(_habbo.Id != _session.GetHabbo().Id && !_session.GetHabbo().Messenger.FriendshipExists(_habbo.Id) &&
                            _session.GetHabbo().Messenger.OutstandingFriendRequests.Contains(_habbo.Id)); // Sent friend request
        packet.WriteBoolean(_clientManager.GetClientByUserId(_habbo.Id) != null);
        packet.WriteInteger(_groups.Count);
        foreach (var group in _groups)
        {
            packet.WriteInteger(group.Id);
            packet.WriteString(group.Name);
            packet.WriteString(group.Badge);
            packet.WriteString(_groupManager.GetColourCode(group.Colour1, true));
            packet.WriteString(_groupManager.GetColourCode(group.Colour2, false));
            packet.WriteBoolean(_habboStats?.FavouriteGroupId == group.Id); // todo favs
            packet.WriteInteger(0); //what the fuck
            packet.WriteBoolean(group?.ForumEnabled ?? true); //HabboTalk
        }
        packet.WriteInteger(Convert.ToInt32(UnixTimestamp.GetNow() - _habbo.LastOnline)); // Last online
        packet.WriteBoolean(true); // Show the profile
    }
}
