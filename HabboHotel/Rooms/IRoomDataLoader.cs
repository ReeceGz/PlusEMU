using Plus.Utilities.DependencyInjection;

namespace Plus.HabboHotel.Rooms;

[Singleton]
public interface IRoomDataLoader
{
    List<RoomData> GetRoomsDataByOwnerSortByName(int ownerId);
    bool TryGetData(uint roomId, out RoomData data);
}
