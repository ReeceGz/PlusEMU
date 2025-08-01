using Plus.Communication.Packets.Outgoing.Rooms.Settings;
using Plus.HabboHotel.GameClients;

using Plus.HabboHotel.Cache;
namespace Plus.Communication.Packets.Incoming.Rooms.Settings;

internal class GetRoomBannedUsersEvent : IPacketEvent
{
    private readonly ICacheManager _cacheManager;

    public GetRoomBannedUsersEvent(ICacheManager cacheManager)
    {
        _cacheManager = cacheManager;
    }

    public Task Parse(GameClient session, IIncomingPacket packet)
    {
        if (!session.GetHabbo().InRoom)
            return Task.CompletedTask;
        var instance = session.GetHabbo().CurrentRoom;
        if (instance == null || !instance.CheckRights(session, true))
            return Task.CompletedTask;
        if (instance.GetBans().BannedUsers().Count > 0)
            session.Send(new GetRoomBannedUsersComposer(instance, _cacheManager));
        return Task.CompletedTask;
    }
}