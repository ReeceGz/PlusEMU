using Plus.Communication.Packets.Outgoing.Marketplace;
using Plus.HabboHotel.GameClients;

using Plus.Database;
namespace Plus.Communication.Packets.Incoming.Marketplace;

internal class GetOwnOffersEvent : IPacketEvent
{
    private readonly IDatabase _database;

    public GetOwnOffersEvent(IDatabase database)
    {
        _database = database;
    }

    public Task Parse(GameClient session, IIncomingPacket packet)
    {
        session.Send(new MarketPlaceOwnOffersComposer(session.GetHabbo().Id, _database));
        return Task.CompletedTask;
    }
}