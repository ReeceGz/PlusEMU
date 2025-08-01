using Plus.Database;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Users.UserData;
using Plus.Utilities;

namespace Plus.Communication.Packets.Incoming.Moderation;

internal class ModerationTradeLockEvent : IPacketEvent
{
    public readonly IDatabase _database;
    private readonly IUserDataFactory _userDataFactory;

    public ModerationTradeLockEvent(IDatabase database, IUserDataFactory userDataFactory)
    {
        _database = database;
        _userDataFactory = userDataFactory;
    }

    public Task Parse(GameClient session, IIncomingPacket packet)
    {
        if (!session.GetHabbo().Permissions.HasRight("mod_trade_lock"))
            return Task.CompletedTask;
        var userId = packet.ReadInt();
        var message = packet.ReadString();
        var days = packet.ReadInt() / 1440.0;
        packet.ReadString(); //unk1
        packet.ReadString(); //unk2
        var length = UnixTimestamp.GetNow() + days * 86400;
        var habbo = _userDataFactory.GetUserDataByIdAsync(userId).Result;
        if (habbo == null)
        {
            session.SendWhisper("An error occoured whilst finding that user in the database.");
            return Task.CompletedTask;
        }
        if (habbo.Permissions.HasRight("mod_trade_lock") && !session.GetHabbo().Permissions.HasRight("mod_trade_lock_any"))
        {
            session.SendWhisper("Oops, you cannot trade lock another user ranked 5 or higher.");
            return Task.CompletedTask;
        }
        if (days < 1)
            days = 1;
        if (days > 365)
            days = 365;
        using (var dbClient = _database.GetQueryReactor())
        {
            dbClient.RunQuery($"UPDATE `user_info` SET `trading_locked` = '{length}', `trading_locks_count` = `trading_locks_count` + '1' WHERE `user_id` = '{habbo.Id}' LIMIT 1");
        }
        if (habbo.Client != null)
        {
            habbo.TradingLockExpiry = length;
            habbo.Client.SendNotification($"You have been trade banned for {days} day(s)!\r\rReason:\r\r{message}");
        }
        return Task.CompletedTask;
    }
}