using Plus.Communication.Packets.Outgoing.FriendList;
using Plus.Core.Settings;
using Plus.HabboHotel.Friends;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Users.Messenger;

namespace Plus.Communication.Packets.Incoming.FriendList;

internal class MessengerInitEvent : IPacketEvent
{
    private readonly IMessengerDataLoader _messengerDataLoader;
    private readonly ISettingsManager _settingsManager;

    public MessengerInitEvent(IMessengerDataLoader messengerDataLoader, ISettingsManager settingsManager)
    {
        _messengerDataLoader = messengerDataLoader;
        _settingsManager = settingsManager;
    }

    public async Task Parse(GameClient session, IIncomingPacket packet)
    {
        var friends = session.GetHabbo().Messenger.Friends.Values.ToList();
        session.Send(new MessengerInitComposer(_settingsManager));
        var page = 0;
        if (!friends.Any())
            session.Send(new BuddyListComposer(friends, 1, 0));
        else
        {
            var pages = (friends.Count - 1) / 500 + 1;
            foreach (ICollection<MessengerBuddy> batch in friends.Chunk(500))
            {
                session.Send(new BuddyListComposer(batch.ToList(), pages, page));
                page++;
            }
        }

        var messages = await _messengerDataLoader.GetAndDeleteOfflineMessages(session.GetHabbo().Id);
        foreach (var (userId, report) in messages)
            foreach (var (message, secondsAgo) in report)
                session.Send(new NewConsoleMessageComposer(userId, message, secondsAgo));
    }
}