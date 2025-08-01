using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Ambassadors;
using Plus.HabboHotel.Users.UserData;

namespace Plus.Communication.Packets.Incoming.Rooms.Action;

internal class AmbassadorAlertEvent : IPacketEvent
{
    private readonly IAmbassadorsManager _ambassadorsManager;
    private readonly IUserDataFactory _userDataFactory;

    public AmbassadorAlertEvent(IAmbassadorsManager ambassadorsManager, IUserDataFactory userDataFactory)
    {
        _ambassadorsManager = ambassadorsManager;
        _userDataFactory = userDataFactory;
    }

    public async Task Parse(GameClient session, IIncomingPacket packet)
    {
        var userid = packet.ReadInt();
        var target = await _userDataFactory.GetUserDataByIdAsync(userid);
        if (target != null)
            await _ambassadorsManager.Warn(session.GetHabbo(), target, "Alert");
    }
}
