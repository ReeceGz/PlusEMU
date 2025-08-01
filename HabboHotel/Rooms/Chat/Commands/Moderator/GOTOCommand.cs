﻿using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Moderator;

internal class GotoCommand : IChatCommand
{
    private readonly IRoomDataLoader _roomDataLoader;

    public GotoCommand(IRoomDataLoader roomDataLoader)
    {
        _roomDataLoader = roomDataLoader;
    }

    public string Key => "goto";
    public string PermissionRequired => "command_goto";

    public string Parameters => "%room_id%";

    public string Description => "";

    public void Execute(GameClient session, Room room, string[] parameters)
    {
        if (!parameters.Any())
        {
            session.SendWhisper("You must specify a room id!");
            return;
        }
        if (!uint.TryParse(parameters[0], out var roomId))
            session.SendWhisper("You must enter a valid room ID");
        else
        {
            if (!_roomDataLoader.TryGetData(roomId, out var data))
            {
                session.SendWhisper("This room does not exist!");
                return;
            }
            session.GetHabbo().PrepareRoom(roomId, "");
        }
    }
}