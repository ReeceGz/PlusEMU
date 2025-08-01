using Plus.Communication.Packets.Outgoing.FriendList;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Users.Messenger;
using Plus.Utilities;

namespace Plus.Communication.Packets.Incoming.FriendList;

internal class HabboSearchEvent : IPacketEvent
{
    private readonly ISearchResultFactory _searchResultFactory;
    private readonly IGameClientManager _clientManager;

    public HabboSearchEvent(ISearchResultFactory searchResultFactory, IGameClientManager clientManager)
    {
        _searchResultFactory = searchResultFactory;
        _clientManager = clientManager;
    }

    public Task Parse(GameClient session, IIncomingPacket packet)
    {
        var query = StringCharFilter.Escape(packet.ReadString().Replace("%", ""));
        if (query.Length < 1 || query.Length > 100)
            return Task.CompletedTask;
        var friends = new List<SearchResult>();
        var othersUsers = new List<SearchResult>();
        var results = _searchResultFactory.GetSearchResult(query);
        foreach (var result in results.ToList())
        {
            if (session.GetHabbo().Messenger.FriendshipExists(result.UserId))
                friends.Add(result);
            else
                othersUsers.Add(result);
        }
        session.Send(new HabboSearchResultComposer(friends, othersUsers, _clientManager));
        return Task.CompletedTask;
    }
}