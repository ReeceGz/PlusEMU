using Plus.Communication.Packets.Outgoing.Groups;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Groups;
using Plus.HabboHotel.Items;

namespace Plus.Communication.Packets.Incoming.Rooms.Furni;
using Plus.HabboHotel.Users.UserData;

internal class GetGroupFurniSettingsEvent : IPacketEvent
{
    private readonly IGroupManager _groupManager;
    private readonly IUserDataFactory _userDataFactory;

    public GetGroupFurniSettingsEvent(IGroupManager groupManager, IUserDataFactory userDataFactory)
    {
        _groupManager = groupManager;
        _userDataFactory = userDataFactory;
    }

    public Task Parse(GameClient session, IIncomingPacket packet)
    {
        if (!session.GetHabbo().InRoom)
            return Task.CompletedTask;
        var itemId = packet.ReadUInt();
        var groupId = packet.ReadInt();
        var item = session.GetHabbo().CurrentRoom.GetRoomItemHandler().GetItem(itemId);
        if (item == null)
            return Task.CompletedTask;
        if (item.Definition.InteractionType != InteractionType.GuildGate)
            return Task.CompletedTask;
        if (!_groupManager.TryGetGroup(groupId, out var group))
            return Task.CompletedTask;
        session.Send(new GroupFurniSettingsComposer(group, itemId, session.GetHabbo().Id));
        session.Send(new GroupInfoComposer(group, session, _userDataFactory));
        return Task.CompletedTask;
    }
}