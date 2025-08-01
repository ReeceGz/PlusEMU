using Plus.Communication.Packets.Outgoing.FriendList;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Users.Messenger;

using Plus.HabboHotel.Cache;
namespace Plus.Communication.Packets.Incoming.FriendList;

internal class GetFriendRequestsEvent : IPacketEvent
{
    private readonly ICacheManager _cacheManager;

    public GetFriendRequestsEvent(ICacheManager cacheManager)
    {
        _cacheManager = cacheManager;
    }

    public Task Parse(GameClient session, IIncomingPacket packet)
    {
        ICollection<MessengerRequest> requests = session.GetHabbo().Messenger.Requests.Values.ToList();
        session.Send(new BuddyRequestsComposer(requests, _cacheManager));
        return Task.CompletedTask;
    }
}