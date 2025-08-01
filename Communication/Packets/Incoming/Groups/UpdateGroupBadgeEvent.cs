﻿using Plus.Communication.Packets.Outgoing.Groups;
using Plus.Database;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Groups;
using Dapper;

namespace Plus.Communication.Packets.Incoming.Groups;
using Plus.HabboHotel.Users.UserData;

internal class UpdateGroupBadgeEvent : IPacketEvent
{
    private readonly IGroupManager _groupManager;
    private readonly IDatabase _database;
    private readonly IUserDataFactory _userDataFactory;

    public UpdateGroupBadgeEvent(IGroupManager groupManager, IDatabase database, IUserDataFactory userDataFactory)
    {
        _groupManager = groupManager;
        _database = database;
        _userDataFactory = userDataFactory;
    }

    public Task Parse(GameClient session, IIncomingPacket packet)
    {
        var groupId = packet.ReadInt();
        if (!_groupManager.TryGetGroup(groupId, out var group))
            return Task.CompletedTask;
        if (group.CreatorId != session.GetHabbo().Id)
            return Task.CompletedTask;
        var count = packet.ReadInt();
        var badge = "";
        for (var i = 0; i < count; i++) badge += BadgePartUtility.WorkBadgeParts(i == 0, packet.ReadInt().ToString(), packet.ReadInt().ToString(), packet.ReadInt().ToString());
        group.Badge = string.IsNullOrWhiteSpace(badge) ? "b05114s06114" : badge;
        using (var connection = _database.Connection())
        {
            connection.Execute("UPDATE `groups` SET `badge` = @badge WHERE `id` = @groupId LIMIT 1", new { badge = group.Badge, groupId = group.Id });
        }
        session.Send(new GroupInfoComposer(group, session, _userDataFactory));
        return Task.CompletedTask;
    }
}