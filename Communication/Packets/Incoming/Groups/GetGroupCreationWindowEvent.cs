using Plus.Communication.Packets.Outgoing.Groups;
using Plus.Core.Settings;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms;

namespace Plus.Communication.Packets.Incoming.Groups;

internal class GetGroupCreationWindowEvent : IPacketEvent
{
    private readonly ISettingsManager _settingsManager;

    public GetGroupCreationWindowEvent(ISettingsManager settingsManager)
    {
        _settingsManager = settingsManager;
    }

    public Task Parse(GameClient session, IIncomingPacket packet)
    {
        var rooms = RoomFactory.GetRoomsDataByOwnerSortByName(session.GetHabbo().Id).Where(x => x.Group == null).ToList();
        session.Send(new GroupCreationWindowComposer(rooms, _settingsManager));
        return Task.CompletedTask;
    }
}