using Plus.Communication.Packets.Outgoing.Rooms.AI.Pets;
using Plus.Database;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms;

namespace Plus.Communication.Packets.Incoming.Rooms.AI.Pets.Horse;

internal class ModifyWhoCanRideHorseEvent : RoomPacketEvent
{
    private readonly IDatabase _database;
    private readonly IRoomManager _roomManager;

    public ModifyWhoCanRideHorseEvent(IDatabase database, IRoomManager roomManager)
    {
        _database = database;
        _roomManager = roomManager;
    }

    public override Task Parse(Room room, GameClient session, IIncomingPacket packet)
    {
        var petId = packet.ReadInt();
        if (!room.GetRoomUserManager().TryGetPet(petId, out var pet))
            return Task.CompletedTask;
        pet.PetData.AnyoneCanRide = pet.PetData.AnyoneCanRide == 1 ? 0 : 1;
        using (var dbClient = _database.GetQueryReactor())
        {
            dbClient.RunQuery($"UPDATE `bots_petdata` SET `anyone_ride` = '{pet.PetData.AnyoneCanRide}' WHERE `id` = '{petId}' LIMIT 1");
        }
        room.SendPacket(new PetInformationComposer(pet.PetData, _roomManager));
        return Task.CompletedTask;
    }
}