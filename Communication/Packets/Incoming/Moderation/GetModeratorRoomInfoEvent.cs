using Plus.Communication.Packets.Outgoing.Moderation;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms;

namespace Plus.Communication.Packets.Incoming.Moderation;

internal class GetModeratorRoomInfoEvent : IPacketEvent
{
    private readonly IRoomManager _roomManager;
    private readonly IRoomDataLoader _roomDataLoader;

    public GetModeratorRoomInfoEvent(IRoomManager roomManager, IRoomDataLoader roomDataLoader)
    {
        _roomManager = roomManager;
        _roomDataLoader = roomDataLoader;
    }

    public Task Parse(GameClient session, IIncomingPacket packet)
    {
        if (!session.GetHabbo().Permissions.HasRight("mod_tool"))
            return Task.CompletedTask;
        var roomId = packet.ReadUInt();
        if (!_roomDataLoader.TryGetData(roomId, out var data))
            return Task.CompletedTask;
        if (!_roomManager.TryGetRoom(roomId, out var room))
            return Task.CompletedTask;
        session.Send(new ModeratorRoomInfoComposer(data, room.GetRoomUserManager().GetRoomUserByHabbo(data.OwnerName) != null));
        return Task.CompletedTask;
    }
}