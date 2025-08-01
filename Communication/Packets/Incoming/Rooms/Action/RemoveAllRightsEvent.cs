using Plus.Communication.Packets.Outgoing.Rooms.Engine;
using Plus.Communication.Packets.Outgoing.Rooms.Permissions;
using Plus.Communication.Packets.Outgoing.Rooms.Settings;
using Plus.Database;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms;

using Plus.HabboHotel.Cache;
namespace Plus.Communication.Packets.Incoming.Rooms.Action;

internal class RemoveAllRightsEvent : RoomPacketEvent
{
    private readonly IRoomManager _roomManager;
    private readonly IDatabase _database;
    private readonly ICacheManager _cacheManager;

    public RemoveAllRightsEvent(IRoomManager roomManager, IDatabase database, ICacheManager cacheManager)
    {
        _roomManager = roomManager;
        _database = database;
        _cacheManager = cacheManager;
    }

    public override Task Parse(Room room, GameClient session, IIncomingPacket packet)
    {
        var instance = room;
        if (!instance.CheckRights(session, true))
            return Task.CompletedTask;
        foreach (var userId in new List<int>(instance.UsersWithRights))
        {
            var user = instance.GetRoomUserManager().GetRoomUserByHabbo(userId);
            if (user != null && !user.IsBot)
            {
                user.RemoveStatus("flatctrl 1");
                user.UpdateNeeded = true;
                user.GetClient().Send(new YouAreControllerComposer(0));
            }
            using (var dbClient = _database.GetQueryReactor())
            {
                dbClient.SetQuery("DELETE FROM `room_rights` WHERE `user_id` = @uid AND `room_id` = @rid LIMIT 1");
                dbClient.AddParameter("uid", userId);
                dbClient.AddParameter("rid", instance.Id);
                dbClient.RunQuery();
            }
            session.Send(new FlatControllerRemovedComposer(instance, userId));
            session.Send(new RoomRightsListComposer(instance, _cacheManager));
            session.Send(new UserUpdateComposer(instance.GetRoomUserManager().GetUserList().ToList()));
        }
        if (instance.UsersWithRights.Count > 0)
            instance.UsersWithRights.Clear();
        return Task.CompletedTask;
    }
}