﻿using Plus.Communication.Packets.Outgoing.Rooms.AI.Pets;
using Plus.Communication.Packets.Outgoing.Rooms.Engine;
using Plus.Database;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Items;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Groups;
using Plus.HabboHotel.Users.UserData;

namespace Plus.Communication.Packets.Incoming.Rooms.AI.Pets.Horse;

internal class ApplyHorseEffectEvent : RoomPacketEvent
{
    private readonly IDatabase _database;
    private readonly IGroupManager _groupManager;
    private readonly IUserDataFactory _userDataFactory;

    public ApplyHorseEffectEvent(IDatabase database, IGroupManager groupManager, IUserDataFactory userDataFactory)
    {
        _database = database;
        _groupManager = groupManager;
        _userDataFactory = userDataFactory;
    }

    public override Task Parse(Room room, GameClient session, IIncomingPacket packet)
    {
        var itemId = packet.ReadUInt();
        var item = room.GetRoomItemHandler().GetItem(itemId);
        if (item == null)
            return Task.CompletedTask;
        var petId = packet.ReadInt();
        if (!room.GetRoomUserManager().TryGetPet(petId, out var petUser))
            return Task.CompletedTask;
        if (petUser.PetData == null || petUser.PetData.OwnerId != session.GetHabbo().Id)
            return Task.CompletedTask;
        if (item.Definition.InteractionType == InteractionType.HorseSaddle1)
        {
            petUser.PetData.Saddle = 9;
            using (var dbClient = _database.GetQueryReactor())
            {
                dbClient.RunQuery($"UPDATE `bots_petdata` SET `have_saddle` = '9' WHERE `id` = '{petUser.PetData.PetId}' LIMIT 1");
                dbClient.RunQuery($"DELETE FROM `items` WHERE `id` = '{item.Id}' LIMIT 1");
            }

            //We only want to use this if we're successful.
            room.GetRoomItemHandler().RemoveFurniture(session, item.Id);
        }
        else if (item.Definition.InteractionType == InteractionType.HorseSaddle2)
        {
            petUser.PetData.Saddle = 10;
            using (var dbClient = _database.GetQueryReactor())
            {
                dbClient.RunQuery($"UPDATE `bots_petdata` SET `have_saddle` = '10' WHERE `id` = '{petUser.PetData.PetId}' LIMIT 1");
                dbClient.RunQuery($"DELETE FROM `items` WHERE `id` = '{item.Id}' LIMIT 1");
            }

            //We only want to use this if we're successful.
            room.GetRoomItemHandler().RemoveFurniture(session, item.Id);
        }
        else if (item.Definition.InteractionType == InteractionType.HorseHairstyle)
        {
            var parse = 100;
            var hairType = item.Definition.ItemName.Split('_')[2];
            parse = parse + int.Parse(hairType);
            petUser.PetData.PetHair = parse;
            using (var dbClient = _database.GetQueryReactor())
            {
                dbClient.RunQuery($"UPDATE `bots_petdata` SET `pethair` = '{petUser.PetData.PetHair}' WHERE `id` = '{petUser.PetData.PetId}' LIMIT 1");
                dbClient.RunQuery($"DELETE FROM `items` WHERE `id` = '{item.Id}' LIMIT 1");
            }

            //We only want to use this if we're successful.
            room.GetRoomItemHandler().RemoveFurniture(session, item.Id);
        }
        else if (item.Definition.InteractionType == InteractionType.HorseHairDye)
        {
            var hairDye = 48;
            var hairType = item.Definition.ItemName.Split('_')[2];
            hairDye = hairDye + int.Parse(hairType);
            petUser.PetData.HairDye = hairDye;
            using (var dbClient = _database.GetQueryReactor())
            {
                dbClient.RunQuery($"UPDATE `bots_petdata` SET `hairdye` = '{petUser.PetData.HairDye}' WHERE `id` = '{petUser.PetData.PetId}' LIMIT 1");
                dbClient.RunQuery($"DELETE FROM `items` WHERE `id` = '{item.Id}' LIMIT 1");
            }

            //We only want to use this if we're successful.
            room.GetRoomItemHandler().RemoveFurniture(session, item.Id);
        }
        else if (item.Definition.InteractionType == InteractionType.HorseBodyDye)
        {
            var race = item.Definition.ItemName.Split('_')[2];
            var parse = int.Parse(race);
            var raceLast = 2 + parse * 4 - 4;
            if (parse == 13)
                raceLast = 61;
            else if (parse == 14)
                raceLast = 65;
            else if (parse == 15)
                raceLast = 69;
            else if (parse == 16)
                raceLast = 73;
            petUser.PetData.Race = raceLast.ToString();
            using (var dbClient = _database.GetQueryReactor())
            {
                dbClient.RunQuery($"UPDATE `bots_petdata` SET `race` = '{petUser.PetData.Race}' WHERE `id` = '{petUser.PetData.PetId}' LIMIT 1");
                dbClient.RunQuery($"DELETE FROM `items` WHERE `id` = '{item.Id}' LIMIT 1");
            }

            //We only want to use this if we're successful.
            room.GetRoomItemHandler().RemoveFurniture(session, item.Id);
        }

        //Update the Pet and the Pet figure information.
        room.SendPacket(new UsersComposer(petUser, _groupManager, _userDataFactory));
        room.SendPacket(new PetHorseFigureInformationComposer(petUser));
        return Task.CompletedTask;
    }
}