using Plus.Communication.Packets.Outgoing.Groups;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Groups;
using Plus.HabboHotel.Users.UserData;

namespace Plus.Communication.Packets.Incoming.Groups;

internal class AcceptGroupMembershipEvent : IPacketEvent
{
    private readonly IGroupManager _groupManager;
    private readonly IUserDataFactory _userDataFactory;

    public AcceptGroupMembershipEvent(IGroupManager groupManager, IUserDataFactory userDataFactory)
    {
        _groupManager = groupManager;
        _userDataFactory = userDataFactory;
    }

    public Task Parse(GameClient session, IIncomingPacket packet)
    {
        var groupId = packet.ReadInt();
        var userId = packet.ReadInt();
        if (!_groupManager.TryGetGroup(groupId, out var group))
            return Task.CompletedTask;
        if (session.GetHabbo().Id != group.CreatorId && !group.IsAdmin(session.GetHabbo().Id) && !session.GetHabbo().Permissions.HasRight("fuse_group_accept_any"))
            return Task.CompletedTask;
        if (!group.HasRequest(userId))
            return Task.CompletedTask;
        var habbo = _userDataFactory.GetUserDataByIdAsync(userId).Result;
        if (habbo == null)
        {
            session.SendNotification("Oops, an error occurred whilst finding this user.");
            return Task.CompletedTask;
        }
        group.HandleRequest(userId, true);
        session.Send(new GroupMemberUpdatedComposer(groupId, habbo, 4));
        return Task.CompletedTask;
    }
}