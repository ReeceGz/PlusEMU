using Plus.HabboHotel.Catalog.Marketplace;
using Plus.HabboHotel.GameClients;

namespace Plus.Communication.Packets.Outgoing.Marketplace;

public class MarketplaceItemStatsComposer : IServerPacket
{
    private readonly int _itemId;
    private readonly uint _spriteId;
    private readonly int _averagePrice;
    private readonly IMarketplaceManager _marketplace;
    public uint MessageId => ServerPacketHeader.MarketplaceItemStatsComposer;

    public MarketplaceItemStatsComposer(int itemId, uint spriteId, int averagePrice, IMarketplaceManager marketplace)
    {
        _itemId = itemId;
        _spriteId = spriteId;
        _averagePrice = averagePrice;
        _marketplace = marketplace;
    }

    public void Compose(IOutgoingPacket packet)
    {
        packet.WriteInteger(_averagePrice); //Avg price in last 7 days.
        packet.WriteInteger(_marketplace.OfferCountForSprite(_spriteId));
        packet.WriteInteger(0); //No idea.
        packet.WriteInteger(0); //No idea.
        packet.WriteInteger(_itemId);
        packet.WriteUInteger(_spriteId);
    }
}