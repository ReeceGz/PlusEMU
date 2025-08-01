﻿using Plus.Communication.Packets.Outgoing.Catalog;
using Plus.HabboHotel.Catalog;
using Plus.HabboHotel.GameClients;

namespace Plus.Communication.Packets.Incoming.Catalog;

internal class GetClubOffersEvent : IPacketEvent
{
    private readonly ICatalogManager _catalogManager;

    public GetClubOffersEvent(ICatalogManager catalogManager)
    {
        _catalogManager = catalogManager;
    }

    public Task Parse(GameClient session, IIncomingPacket packet)
    {
        var offerId = packet.ReadInt();
        if (!_catalogManager.ItemOffers.ContainsKey(offerId))
            return Task.CompletedTask;
        var pageId = _catalogManager.ItemOffers[offerId];
        if (!_catalogManager.TryGetPage(pageId, out var page))
            return Task.CompletedTask;
        if (!page.Enabled || !page.Visible || page.MinimumRank > session.GetHabbo().Rank || page.MinimumVip > session.GetHabbo().VipRank && session.GetHabbo().Rank == 1)
            return Task.CompletedTask;
        if (!page.ItemOffers.ContainsKey(offerId))
            return Task.CompletedTask;
        var item = page.ItemOffers[offerId];
        if (item != null)
            session.Send(new CatalogOfferComposer(item, _catalogManager));
        return Task.CompletedTask;
    }
}