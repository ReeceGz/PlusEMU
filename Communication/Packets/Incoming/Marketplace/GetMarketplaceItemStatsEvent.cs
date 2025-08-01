using System.Data;
using Plus.Communication.Packets.Outgoing.Marketplace;
using Plus.Database;
using Plus.HabboHotel.Catalog.Marketplace;
using Plus.HabboHotel.GameClients;

namespace Plus.Communication.Packets.Incoming.Marketplace;

internal class GetMarketplaceItemStatsEvent : IPacketEvent
{
    private readonly IDatabase _database;
    private readonly IMarketplaceManager _marketplaceManager;

    public GetMarketplaceItemStatsEvent(IDatabase database, IMarketplaceManager marketplaceManager)
    {
        _database = database;
        _marketplaceManager = marketplaceManager;
    }

    public Task Parse(GameClient session, IIncomingPacket packet)
    {
        var itemId = packet.ReadInt();
        var spriteId = packet.ReadUInt();
        DataRow row;
        using (var dbClient = _database.GetQueryReactor())
        {
            dbClient.SetQuery("SELECT `avgprice` FROM `catalog_marketplace_data` WHERE `sprite` = @SpriteId LIMIT 1");
            dbClient.AddParameter("SpriteId", spriteId);
            row = dbClient.GetRow();
        }
        session.Send(new MarketplaceItemStatsComposer(itemId, spriteId, row != null ? Convert.ToInt32(row["avgprice"]) : 0, _marketplaceManager));
        return Task.CompletedTask;
    }
}