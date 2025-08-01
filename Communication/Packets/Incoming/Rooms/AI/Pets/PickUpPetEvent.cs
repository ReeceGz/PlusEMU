﻿using Plus.Communication.Packets.Outgoing.Inventory.Pets;
using Plus.Communication.Packets.Outgoing.Rooms.Engine;
using Plus.Database;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Groups;
using Plus.HabboHotel.Users.UserData;

namespace Plus.Communication.Packets.Incoming.Rooms.AI.Pets;

internal class PickUpPetEvent : RoomPacketEvent
{
    private readonly IGameClientManager _clientManager;
    private readonly IDatabase _database;
    private readonly IGroupManager _groupManager;
    private readonly IUserDataFactory _userDataFactory;

    public PickUpPetEvent(IGameClientManager clientManager, IDatabase database, IGroupManager groupManager, IUserDataFactory userDataFactory)
    {
        _clientManager = clientManager;
        _database = database;
        _groupManager = groupManager;
        _userDataFactory = userDataFactory;
    }

    public override Task Parse(Room room, GameClient session, IIncomingPacket packet)
    {
        var petId = packet.ReadInt();
        if (!room.GetRoomUserManager().TryGetPet(petId, out var pet))
        {
            //Check kick rights, just because it seems most appropriate.
            if (!room.CheckRights(session) && room.WhoCanKick != 2 && room.Group == null || room.Group != null && !room.CheckRights(session, false, true))
                return Task.CompletedTask;

            //Okay so, we've established we have no pets in this room by this virtual Id, let us check out users, maybe they're creeping as a pet?!
            var targetUser = session.GetHabbo().CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(petId);
            if (targetUser == null)
                return Task.CompletedTask;

            //Check some values first, please!
            if (targetUser.GetClient() == null || targetUser.GetClient().GetHabbo() == null)
                return Task.CompletedTask;

            //Update the targets PetId.
            targetUser.GetClient().GetHabbo().PetId = 0;

            //Quickly remove the old user instance.
            room.SendPacket(new UserRemoveComposer(targetUser.VirtualId));

            //Add the new one, they won't even notice a thing!!11 8-)
            room.SendPacket(new UsersComposer(targetUser, _groupManager, _userDataFactory));
            return Task.CompletedTask;
        }
        if (session.GetHabbo().Id != pet.PetData.OwnerId && !room.CheckRights(session, true))
        {
            session.SendWhisper("You can only pickup your own pets, to kick a pet you must have room rights.");
            return Task.CompletedTask;
        }
        if (pet.RidingHorse)
        {
            var userRiding = room.GetRoomUserManager().GetRoomUserByVirtualId(pet.HorseId);
            if (userRiding != null)
            {
                userRiding.RidingHorse = false;
                userRiding.ApplyEffect(-1);
                userRiding.MoveTo(new(userRiding.X + 1, userRiding.Y + 1));
            }
            else
                pet.RidingHorse = false;
        }
        var data = pet.PetData;
        if (data != null)
        {
            using var dbClient = _database.GetQueryReactor();
            dbClient.RunQuery($"UPDATE `bots` SET `room_id` = '0', `x` = '0', `Y` = '0', `Z` = '0' WHERE `id` = '{data.PetId}' LIMIT 1");
            dbClient.RunQuery(
                $"UPDATE `bots_petdata` SET `experience` = '{data.Experience}', `energy` = '{data.Energy}', `nutrition` = '{data.Nutrition}', `respect` = '{data.Respect}' WHERE `id` = '{data.PetId}' LIMIT 1");
        }
        if (data.OwnerId != session.GetHabbo().Id)
        {
            var target = _clientManager.GetClientByUserId(data.OwnerId);
            if (target != null)
            {
                if (target.GetHabbo().Inventory.Pets.AddPet(pet.PetData))
                {
                    pet.PetData.RoomId = 0;
                    pet.PetData.PlacedInRoom = false;
                    room.GetRoomUserManager().RemoveBot(pet.VirtualId, false);
                    target.Send(new PetInventoryComposer(target.GetHabbo().Inventory.Pets.Pets.Values.ToList()));
                    return Task.CompletedTask;
                }
            }
        }
        room.GetRoomUserManager().RemoveBot(pet.VirtualId, false);
        return Task.CompletedTask;
    }
}