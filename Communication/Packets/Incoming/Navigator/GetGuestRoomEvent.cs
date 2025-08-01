using Plus.Communication.Packets.Outgoing.Navigator;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms;

namespace Plus.Communication.Packets.Incoming.Navigator;
using Plus.HabboHotel.Rooms;
internal class GetGuestRoomEvent : IPacketEvent
{
    private readonly IRoomDataLoader _roomDataLoader;

    public GetGuestRoomEvent(IRoomDataLoader roomDataLoader)
    {
        _roomDataLoader = roomDataLoader;
    }

    public Task Parse(GameClient session, IIncomingPacket packet)
    {
        var roomId = packet.ReadUInt();
        if (!_roomDataLoader.TryGetData(roomId, out var data))
            return Task.CompletedTask;
        var enter = packet.ReadInt() == 1;
        var forward = packet.ReadInt() == 1;
        session.Send(new GetGuestRoomResultComposer(session, data, enter, forward));
        return Task.CompletedTask;
    }
}