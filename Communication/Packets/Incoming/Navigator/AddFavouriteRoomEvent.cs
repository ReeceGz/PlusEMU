using Plus.Communication.Packets.Outgoing.Navigator;
using Plus.Database;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms;

namespace Plus.Communication.Packets.Incoming.Navigator;

public class AddFavouriteRoomEvent : IPacketEvent
{
    private readonly IDatabase _database;
    private readonly IRoomDataLoader _roomDataLoader;

    public AddFavouriteRoomEvent(IDatabase database, IRoomDataLoader roomDataLoader)
    {
        _database = database;
        _roomDataLoader = roomDataLoader;
    }

    public Task Parse(GameClient session, IIncomingPacket packet)
    {
        var roomId = packet.ReadUInt();
        if (!_roomDataLoader.TryGetData(roomId, out var data))
            return Task.CompletedTask;
        if (data == null || session.GetHabbo().FavoriteRooms.Count >= 30 || session.GetHabbo().FavoriteRooms.Contains(roomId))
        {
            // send packet that favourites is full.
            return Task.CompletedTask;
        }
        session.GetHabbo().FavoriteRooms.Add(roomId);
        session.Send(new UpdateFavouriteRoomComposer(roomId, true));
        using var dbClient = _database.GetQueryReactor();
        dbClient.RunQuery($"INSERT INTO user_favorites (user_id,room_id) VALUES ({session.GetHabbo().Id},{roomId})");
        return Task.CompletedTask;
    }
}