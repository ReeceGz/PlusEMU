using Plus.Communication.Packets.Outgoing.Rooms.Settings;
using Plus.HabboHotel.GameClients;

using Plus.HabboHotel.Cache;
namespace Plus.Communication.Packets.Incoming.Rooms.Settings;

internal class GetRoomRightsEvent : IPacketEvent
{
    private readonly ICacheManager _cacheManager;

    public GetRoomRightsEvent(ICacheManager cacheManager)
    {
        _cacheManager = cacheManager;
    }

    public Task Parse(GameClient session, IIncomingPacket packet)
    {
        if (!session.GetHabbo().InRoom)
            return Task.CompletedTask;
        var instance = session.GetHabbo().CurrentRoom;
        if (instance == null)
            return Task.CompletedTask;
        if (!instance.CheckRights(session))
            return Task.CompletedTask;
        session.Send(new RoomRightsListComposer(instance, _cacheManager));
        return Task.CompletedTask;
    }
}