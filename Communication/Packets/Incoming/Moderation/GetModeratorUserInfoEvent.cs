﻿using System.Data;
using Plus.Communication.Packets.Outgoing.Moderation;
using Plus.Core.Language;
using Plus.Database;
using Plus.HabboHotel.GameClients;

namespace Plus.Communication.Packets.Incoming.Moderation;

internal class GetModeratorUserInfoEvent : IPacketEvent
{
    private readonly ILanguageManager _languageManager;
    private readonly IDatabase _database;
    private readonly IGameClientManager _clientManager;

    public GetModeratorUserInfoEvent(ILanguageManager languageManager, IDatabase database, IGameClientManager clientManager)
    {
        _languageManager = languageManager;
        _database = database;
        _clientManager = clientManager;
    }

    public Task Parse(GameClient session, IIncomingPacket packet)
    {
        if (!session.GetHabbo().Permissions.HasRight("mod_tool"))
            return Task.CompletedTask;
        var userId = packet.ReadInt();
        DataRow user;
        DataRow info;
        using (var dbClient = _database.GetQueryReactor())
        {
            dbClient.SetQuery($"SELECT `id`,`username`,`online`,`mail`,`ip_last`,`look`,`account_created`,`last_online` FROM `users` WHERE `id` = '{userId}' LIMIT 1");
            user = dbClient.GetRow();
            if (user == null)
            {
                session.SendNotification(_languageManager.TryGetValue("user.not_found"));
                return Task.CompletedTask;
            }
            dbClient.SetQuery($"SELECT `cfhs`,`cfhs_abusive`,`cautions`,`bans`,`trading_locked`,`trading_locks_count` FROM `user_info` WHERE `user_id` = '{userId}' LIMIT 1");
            info = dbClient.GetRow();
            if (info == null)
            {
                dbClient.RunQuery($"INSERT INTO `user_info` (`user_id`) VALUES ('{userId}')");
                dbClient.SetQuery($"SELECT `cfhs`,`cfhs_abusive`,`cautions`,`bans`,`trading_locked`,`trading_locks_count` FROM `user_info` WHERE `user_id` = '{userId}' LIMIT 1");
                info = dbClient.GetRow();
            }
        }
        session.Send(new ModeratorUserInfoComposer(user, info, _clientManager));
        return Task.CompletedTask;
    }
}