﻿using System.Data;
using Plus.Communication.Packets.Outgoing.Moderation;
using Plus.Database;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Rooms.Chat.Logs;
using Plus.HabboHotel.Users.UserData;

namespace Plus.Communication.Packets.Incoming.Moderation;

internal class GetModeratorRoomChatlogEvent : IPacketEvent
{
    private readonly IRoomManager _roomManager;
    private readonly IChatlogManager _chatlogManager;
    private readonly IDatabase _database;
    private readonly IUserDataFactory _userDataFactory;

    public GetModeratorRoomChatlogEvent(IRoomManager roomManager, IChatlogManager chatlogManager, IDatabase database, IUserDataFactory userDataFactory)
    {
        _roomManager = roomManager;
        _chatlogManager = chatlogManager;
        _database = database;
        _userDataFactory = userDataFactory;
    }

    public Task Parse(GameClient session, IIncomingPacket packet)
    {
        if (!session.GetHabbo().Permissions.HasRight("mod_tool"))
            return Task.CompletedTask;
        packet.ReadInt(); //junk
        var roomId = packet.ReadUInt();
        if (!_roomManager.TryGetRoom(roomId, out var room)) return Task.CompletedTask;
        _chatlogManager.FlushAndSave();
        var chats = new List<ChatlogEntry>();
        using (var dbClient = _database.GetQueryReactor())
        {
            dbClient.SetQuery("SELECT * FROM `chatlogs` WHERE `room_id` = @id ORDER BY `id` DESC LIMIT 100");
            dbClient.AddParameter("id", roomId);
            var data = dbClient.GetTable();
            if (data != null)
            {
                foreach (DataRow row in data.Rows)
                {
                    var habbo = _userDataFactory.GetUserDataByIdAsync(Convert.ToInt32(row["user_id"])).Result;
                    if (habbo != null) chats.Add(new(Convert.ToInt32(row["user_id"]), roomId, Convert.ToString(row["message"]), Convert.ToDouble(row["timestamp"]), habbo));
                }
            }
        }
        session.Send(new ModeratorRoomChatlogComposer(room, chats));
        return Task.CompletedTask;
    }
}