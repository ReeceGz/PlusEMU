using Plus.Database;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Moderation;
using Plus.HabboHotel.Users.UserData;
using Plus.Utilities;

namespace Plus.Communication.Packets.Incoming.Moderation;

internal class ModerationBanEvent : IPacketEvent
{
    private readonly IGameClientManager _clientManager;
    private readonly IModerationManager _moderationManager;
    private readonly IDatabase _database;
    private readonly IUserDataFactory _userDataFactory;

    public ModerationBanEvent(IGameClientManager clientManager, IModerationManager moderationManager, IDatabase database, IUserDataFactory userDataFactory)
    {
        _clientManager = clientManager;
        _moderationManager = moderationManager;
        _database = database;
        _userDataFactory = userDataFactory;
    }

    public Task Parse(GameClient session, IIncomingPacket packet)
    {
        if (!session.GetHabbo().Permissions.HasRight("mod_soft_ban"))
            return Task.CompletedTask;
        var userId = packet.ReadInt();
        var message = packet.ReadString();
        var length = packet.ReadInt() * 3600 + UnixTimestamp.GetNow();
        packet.ReadString(); //unk1
        packet.ReadString(); //unk2
        var ipBan = packet.ReadBool();
        var machineBan = packet.ReadBool();
        if (machineBan)
            ipBan = false;
        var habbo = _userDataFactory.GetUserDataByIdAsync(userId).Result;
        if (habbo == null)
        {
            session.SendWhisper("An error occoured whilst finding that user in the database.");
            return Task.CompletedTask;
        }
        if (habbo.Permissions.HasRight("mod_tool") && !session.GetHabbo().Permissions.HasRight("mod_ban_any"))
        {
            session.SendWhisper("Oops, you cannot ban that user.");
            return Task.CompletedTask;
        }
        message = message ?? "No reason specified.";
        using (var dbClient = _database.GetQueryReactor())
        {
            dbClient.RunQuery($"UPDATE `user_info` SET `bans` = `bans` + '1' WHERE `user_id` = '{habbo.Id}' LIMIT 1");
        }
        if (ipBan == false && machineBan == false)
            _moderationManager.BanUser(session.GetHabbo().Username, ModerationBanType.Username, habbo.Username, message, length);
        else if (ipBan)
            _moderationManager.BanUser(session.GetHabbo().Username, ModerationBanType.Ip, habbo.Username, message, length);
        else
        {
            _moderationManager.BanUser(session.GetHabbo().Username, ModerationBanType.Ip, habbo.Username, message, length);
            _moderationManager.BanUser(session.GetHabbo().Username, ModerationBanType.Username, habbo.Username, message, length);
            _moderationManager.BanUser(session.GetHabbo().Username, ModerationBanType.Machine, habbo.Username, message, length);
        }
        var targetClient = _clientManager.GetClientByUsername(habbo.Username);
        if (targetClient != null)
            targetClient.Disconnect();
        return Task.CompletedTask;
    }
}