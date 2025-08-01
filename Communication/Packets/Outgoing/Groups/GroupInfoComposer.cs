using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Groups;
using Plus.HabboHotel.Users.UserData;

namespace Plus.Communication.Packets.Outgoing.Groups;

public class GroupInfoComposer : IServerPacket
{
    private readonly Group _group;
    private readonly GameClient _session;
    private readonly bool _newWindow;
    private readonly IUserDataFactory _userDataFactory;

    public uint MessageId => ServerPacketHeader.GroupInfoComposer;

    public GroupInfoComposer(Group group, GameClient session, IUserDataFactory userDataFactory, bool newWindow = false)
    {
        _group = @group;
        _session = session;
        _userDataFactory = userDataFactory;
        _newWindow = newWindow;
    }

    public void Compose(IOutgoingPacket packet)
    {
        var origin = DateTime.UnixEpoch.AddSeconds(_group.CreateTime);
        packet.WriteInteger(_group.Id);
        packet.WriteBoolean(true);
        packet.WriteInteger(_group.Type == GroupType.Open ? 0 : _group.Type == GroupType.Locked ? 1 : 2);
        packet.WriteString(_group.Name);
        packet.WriteString(_group.Description);
        packet.WriteString(_group.Badge);
        packet.WriteUInteger(_group.RoomId);
        packet.WriteString(_group.GetRoom() != null ? _group.GetRoom().Name : "No room found.."); // room name
        packet.WriteInteger(_group.CreatorId == _session.GetHabbo().Id ? 3 : _group.HasRequest(_session.GetHabbo().Id) ? 2 : _group.IsMember(_session.GetHabbo().Id) ? 1 : 0);
        packet.WriteInteger(_group.MemberCount); // Members
        packet.WriteBoolean(false); //?? CHANGED
        packet.WriteString($"{origin.Day}-{origin.Month}-{origin.Year}");
        packet.WriteBoolean(_group.CreatorId == _session.GetHabbo().Id);
        packet.WriteBoolean(_group.IsAdmin(_session.GetHabbo().Id)); // admin
        packet.WriteString(_userDataFactory.GetUsernameForHabboById(_group.CreatorId).GetAwaiter().GetResult());
        packet.WriteBoolean(_newWindow); // Show group info
        packet.WriteBoolean(_group.AdminOnlyDeco == 0); // Any user can place furni in home room
        packet.WriteInteger(_group.CreatorId == _session.GetHabbo().Id ? _group.RequestCount :
            _group.IsAdmin(_session.GetHabbo().Id) ? _group.RequestCount :
            _group.IsMember(_session.GetHabbo().Id) ? 0 : 0); // Pending users
        //base.WriteInteger(0);//what the fuck
        packet.WriteBoolean(_group?.ForumEnabled ?? true); //HabboTalk.
    }
}