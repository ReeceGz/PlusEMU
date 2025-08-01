using Plus.HabboHotel.Achievements;
using Plus.HabboHotel.GameClients;
using Plus.Utilities;
using Plus.HabboHotel.Users.UserData;

namespace Plus.Communication.Packets.Incoming.Rooms.Action;

internal class MuteUserEvent : IPacketEvent
{
    private readonly IAchievementManager _achievementManager;
    private readonly IUserDataFactory _userDataFactory;

    public MuteUserEvent(IAchievementManager achievementManager, IUserDataFactory userDataFactory)
    {
        _achievementManager = achievementManager;
        _userDataFactory = userDataFactory;
    }

    public async Task Parse(GameClient session, IIncomingPacket packet)
    {
        if (!session.GetHabbo().InRoom)
            return;
        var userId = packet.ReadInt();
        packet.ReadInt(); //roomId
        var time = packet.ReadInt();
        var room = session.GetHabbo().CurrentRoom;
        if (room == null)
            return;
        if (room.WhoCanMute == 0 && !room.CheckRights(session, true) && room.Group == null || room.WhoCanMute == 1 && !room.CheckRights(session) && room.Group == null ||
            room.Group != null && !room.CheckRights(session, false, true))
            return;
        var username = await _userDataFactory.GetUsernameForHabboById(userId);
        var target = room.GetRoomUserManager().GetRoomUserByHabbo(username);
        if (target == null)
            return;
        if (target.GetClient().GetHabbo().Permissions.HasRight("mod_tool"))
            return;
        if (room.MutedUsers.ContainsKey(userId))
        {
            if (room.MutedUsers[userId] < UnixTimestamp.GetNow())
                room.MutedUsers.Remove(userId);
            else
                return;
        }
        room.MutedUsers.Add(userId, UnixTimestamp.GetNow() + time * 60);
        target.GetClient().SendWhisper($"The room owner has muted you for {time} minutes!");
        _achievementManager.ProgressAchievement(session, "ACH_SelfModMuteSeen", 1);
        return;
    }
}