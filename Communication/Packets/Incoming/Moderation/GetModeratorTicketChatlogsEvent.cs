using Plus.Communication.Packets.Outgoing.Moderation;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Moderation;
using Plus.HabboHotel.Rooms;

namespace Plus.Communication.Packets.Incoming.Moderation;

internal class GetModeratorTicketChatlogsEvent : IPacketEvent
{
    private readonly IModerationManager _moderationManager;
    private readonly IRoomDataLoader _roomDataLoader;

    public GetModeratorTicketChatlogsEvent(IModerationManager moderationManager, IRoomDataLoader roomDataLoader)
    {
        _moderationManager = moderationManager;
        _roomDataLoader = roomDataLoader;
    }

    public Task Parse(GameClient session, IIncomingPacket packet)
    {
        if (!session.GetHabbo().Permissions.HasRight("mod_tickets"))
            return Task.CompletedTask;
        var ticketId = packet.ReadInt();
        if (!_moderationManager.TryGetTicket(ticketId, out var ticket) || ticket.Room == null)
            return Task.CompletedTask;
        if (!_roomDataLoader.TryGetData(ticket.Room.Id, out var data))
            return Task.CompletedTask;
        session.Send(new ModeratorTicketChatlogComposer(ticket, data, ticket.Timestamp));
        return Task.CompletedTask;
    }
}