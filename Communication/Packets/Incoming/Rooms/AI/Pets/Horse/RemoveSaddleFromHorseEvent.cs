﻿using Plus.Communication.Packets.Outgoing.Catalog;
using Plus.Communication.Packets.Outgoing.Inventory.Furni;
using Plus.Communication.Packets.Outgoing.Rooms.AI.Pets;
using Plus.Communication.Packets.Outgoing.Rooms.Engine;
using Plus.Database;
using Plus.HabboHotel.Catalog.Utilities;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Items;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Groups;
using Plus.HabboHotel.Users.UserData;

namespace Plus.Communication.Packets.Incoming.Rooms.AI.Pets.Horse;

internal class RemoveSaddleFromHorseEvent : IPacketEvent
{
    private readonly IRoomManager _roomManager;
    private readonly IItemDataManager _itemDataManager;
    private readonly IDatabase _database;
    private readonly IItemFactory _itemFactory;
    private readonly IGroupManager _groupManager;
    private readonly IUserDataFactory _userDataFactory;

    public RemoveSaddleFromHorseEvent(IRoomManager roomManager, IItemDataManager itemDataManager, IDatabase database, IItemFactory itemFactory, IGroupManager groupManager, IUserDataFactory userDataFactory)
    {
        _roomManager = roomManager;
        _itemDataManager = itemDataManager;
        _database = database;
        _itemFactory = itemFactory;
        _groupManager = groupManager;
        _userDataFactory = userDataFactory;
    }

    public Task Parse(GameClient session, IIncomingPacket packet)
    {
        if (!session.GetHabbo().InRoom)
            return Task.CompletedTask;
        if (!_roomManager.TryGetRoom(session.GetHabbo( ).CurrentRoom!.Id, out var room))
            return Task.CompletedTask;
        if (!room.GetRoomUserManager().TryGetPet(packet.ReadInt(), out var petUser))
            return Task.CompletedTask;
        if (petUser.PetData == null || petUser.PetData.OwnerId != session.GetHabbo().Id)
            return Task.CompletedTask;

        //Fetch the furniture Id for the pets current saddle.
        var saddleId = ItemUtility.GetSaddleId(petUser.PetData.Saddle);

        //Remove the saddle from the pet.
        petUser.PetData.Saddle = 0;
        using (var dbClient = _database.GetQueryReactor())
        {
            dbClient.RunQuery($"UPDATE `bots_petdata` SET `have_saddle` = '0' WHERE `id` = '{petUser.PetData.PetId}' LIMIT 1");
        }

        //Give the saddle back to the user.
        if (!_itemDataManager.Items.TryGetValue(saddleId, out var itemData))
            return Task.CompletedTask;
        var item = _itemFactory.CreateSingleItemNullable(itemData, session.GetHabbo(), "", "").ToInventoryItem();
        if (item != null)
        {
            session.GetHabbo().Inventory.Furniture.AddItem(item);
            session.Send(new FurniListNotificationComposer(item.Id, 1));
            session.Send(new PurchaseOkComposer());
            session.Send(new FurniListAddComposer(item));
            session.Send(new FurniListUpdateComposer());
        }

        //Update the Pet and the Pet figure information.
        room.SendPacket(new UsersComposer(petUser, _groupManager, _userDataFactory));
        room.SendPacket(new PetHorseFigureInformationComposer(petUser));
        return Task.CompletedTask;
    }
}