﻿using Plus.Communication.Packets.Outgoing.Groups;
using Plus.Communication.Packets.Outgoing.Rooms.Engine;
using Plus.Database;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Groups;
using Plus.HabboHotel.Items;
using Dapper;

using Plus.HabboHotel.Users.UserData;
namespace Plus.Communication.Packets.Incoming.Groups;

internal class UpdateGroupColoursEvent : IPacketEvent
{
    private readonly IGroupManager _groupManager;
    private readonly IDatabase _database;
    private readonly IUserDataFactory _userDataFactory;

    public UpdateGroupColoursEvent(IGroupManager groupManager, IDatabase database, IUserDataFactory userDataFactory)
    {
        _groupManager = groupManager;
        _database = database;
        _userDataFactory = userDataFactory;
    }
    public Task Parse(GameClient session, IIncomingPacket packet)
    {
        var groupId = packet.ReadInt();
        var mainColour = packet.ReadInt();
        var secondaryColour = packet.ReadInt();
        if (!_groupManager.TryGetGroup(groupId, out var group))
            return Task.CompletedTask;
        if (group.CreatorId != session.GetHabbo().Id)
            return Task.CompletedTask;
        using (var connection = _database.Connection())
        {
            connection.Execute("UPDATE `groups` SET `colour1` = @colour1, `colour2` = @colour2 WHERE `id` = @groupId LIMIT 1",
                new { colour1 = mainColour, colour2 = secondaryColour, groupId = group.Id });
        }
        group.Colour1 = mainColour;
        group.Colour2 = secondaryColour;
        session.Send(new GroupInfoComposer(group, session, _userDataFactory));
        if (session.GetHabbo().CurrentRoom != null)
        {
            foreach (var item in session.GetHabbo().CurrentRoom.GetRoomItemHandler().GetFloor.ToList())
            {
                if (item == null || item.Definition== null)
                    continue;
                if (item.Definition.InteractionType != InteractionType.GuildItem && item.Definition.InteractionType != InteractionType.GuildGate ||
                    item.Definition.InteractionType != InteractionType.GuildForum)
                    continue;
                session.GetHabbo().CurrentRoom.SendPacket(new ObjectUpdateComposer(item));
            }
        }
        return Task.CompletedTask;
    }
}