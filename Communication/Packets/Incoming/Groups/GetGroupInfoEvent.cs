using Plus.Communication.Packets.Outgoing.Groups;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Groups;

using Plus.HabboHotel.Users.UserData;
namespace Plus.Communication.Packets.Incoming.Groups;

internal class GetGroupInfoEvent : IPacketEvent
{
    private readonly IGroupManager _groupManager;
    private readonly IUserDataFactory _userDataFactory;

    public GetGroupInfoEvent(IGroupManager groupManager, IUserDataFactory userDataFactory)
    {
        _groupManager = groupManager;
        _userDataFactory = userDataFactory;
    }

    public Task Parse(GameClient session, IIncomingPacket packet)
    {
        var groupId = packet.ReadInt();
        var newWindow = packet.ReadBool();
        if (!_groupManager.TryGetGroup(groupId, out var group))
            return Task.CompletedTask;
        session.Send(new GroupInfoComposer(group, session, _userDataFactory, newWindow));
        return Task.CompletedTask;
    }
}