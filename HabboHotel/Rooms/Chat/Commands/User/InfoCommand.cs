using Plus.Communication.Packets.Outgoing.Rooms.Notifications;
using Plus.HabboHotel.GameClients;
using Plus;

namespace Plus.HabboHotel.Rooms.Chat.Commands.User;

internal class InfoCommand : IChatCommand
{
    private readonly IGameClientManager _gameClientManager;
    private readonly IRoomManager _roomManager;
    private readonly IPlusEnvironment _environment;
    public string Key => "about";
    public string PermissionRequired => "command_info";

    public string Parameters => "";

    public string Description => "Displays generic information that everybody loves to see.";

    public InfoCommand(IGameClientManager gameClientManager, IRoomManager roomManager, IPlusEnvironment environment)
    {
        _gameClientManager = gameClientManager;
        _roomManager = roomManager;
        _environment = environment;
    }
    public void Execute(GameClient session, Room room, string[] parameters)
    {
        var uptime = DateTime.Now - _environment.ServerStarted;
        var onlineUsers = _gameClientManager.Count;
        var roomCount = _roomManager.Count;
        session.Send(new RoomNotificationComposer("Powered by Plus++ Emulator",
            $"<b>Created by the Habbo Hotel Community</b>\n\n<b>Current run time information</b>:\nOnline Users: {onlineUsers}\nRooms Loaded: {roomCount}\nUptime: {uptime.Days} day(s), {uptime.Hours} hours and {uptime.Minutes} minutes.\n\n", "plus", "View on github >", "https://github.com/80O/PlusEmu"));
    }
}