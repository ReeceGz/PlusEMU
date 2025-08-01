using Plus.Communication.Packets.Outgoing.Groups;
using Plus.Communication.Packets.Outgoing.Rooms.Permissions;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Groups;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Users.UserData;

namespace Plus.Communication.Packets.Incoming.Groups;

internal class TakeAdminRightsEvent : IPacketEvent
{
    private readonly IGroupManager _groupManager;
    private readonly IRoomManager _roomManager;
    private readonly IUserDataFactory _userDataFactory;

    public TakeAdminRightsEvent(IGroupManager groupManager, IRoomManager roomManager, IUserDataFactory userDataFactory)
    {
        _groupManager = groupManager;
        _roomManager = roomManager;
        _userDataFactory = userDataFactory;
    }

    public Task Parse(GameClient session, IIncomingPacket packet)
    {
        var groupId = packet.ReadInt();
        var userId = packet.ReadInt();
        if (!_groupManager.TryGetGroup(groupId, out var group))
            return Task.CompletedTask;
        if (session.GetHabbo().Id != group.CreatorId || !group.IsMember(userId))
            return Task.CompletedTask;
        var habbo = _userDataFactory.GetUserDataByIdAsync(userId).Result;
        if (habbo == null)
        {
            session.SendNotification("Oops, an error occurred whilst finding this user.");
            return Task.CompletedTask;
        }
        group.TakeAdmin(userId);
        if (_roomManager.TryGetRoom(group.RoomId, out var room))
        {
            var user = room.GetRoomUserManager().GetRoomUserByHabbo(userId);
            if (user != null)
            {
                if (user.Statusses.ContainsKey("flatctrl 3"))
                    user.RemoveStatus("flatctrl 3");
                user.UpdateNeeded = true;
                if (user.GetClient() != null)
                    user.GetClient().Send(new YouAreControllerComposer(0));
            }
        }
        session.Send(new GroupMemberUpdatedComposer(groupId, habbo, 2));
        return Task.CompletedTask;
    }
}