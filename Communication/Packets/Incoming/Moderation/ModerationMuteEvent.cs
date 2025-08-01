﻿using Plus.Database;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Users.UserData;

namespace Plus.Communication.Packets.Incoming.Moderation;

internal class ModerationMuteEvent : IPacketEvent
{
    public readonly IDatabase _database;
    private readonly IUserDataFactory _userDataFactory;

    public ModerationMuteEvent(IDatabase database, IUserDataFactory userDataFactory)
    {
        _database = database;
        _userDataFactory = userDataFactory;
    }

    public Task Parse(GameClient session, IIncomingPacket packet)
    {
        if (!session.GetHabbo().Permissions.HasRight("mod_mute"))
            return Task.CompletedTask;
        var userId = packet.ReadInt();
        packet.ReadString(); //message
        double length = packet.ReadInt() * 60;
        packet.ReadString(); //unk1
        packet.ReadString(); //unk2
        var habbo = _userDataFactory.GetUserDataByIdAsync(userId).Result;
        if (habbo == null)
        {
            session.SendWhisper("An error occoured whilst finding that user in the database.");
            return Task.CompletedTask;
        }
        if (habbo.Permissions.HasRight("mod_mute") && !session.GetHabbo().Permissions.HasRight("mod_mute_any"))
        {
            session.SendWhisper("Oops, you cannot mute that user.");
            return Task.CompletedTask;
        }
        using (var dbClient = _database.GetQueryReactor())
        {
            dbClient.RunQuery($"UPDATE `users` SET `time_muted` = '{length}' WHERE `id` = '{habbo.Id}' LIMIT 1");
        }
        if (habbo.Client != null)
        {
            habbo.TimeMuted = length;
            habbo.Client.SendNotification($"You have been muted by a moderator for {length} seconds!");
        }
        return Task.CompletedTask;
    }
}