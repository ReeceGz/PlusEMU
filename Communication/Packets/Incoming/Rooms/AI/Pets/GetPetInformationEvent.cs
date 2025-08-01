using Plus.Communication.Packets.Outgoing.Rooms.AI.Pets;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms;

namespace Plus.Communication.Packets.Incoming.Rooms.AI.Pets;

internal class GetPetInformationEvent : IPacketEvent
{
    private readonly IRoomManager _roomManager;

    public GetPetInformationEvent(IRoomManager roomManager)
    {
        _roomManager = roomManager;
    }

    public Task Parse(GameClient session, IIncomingPacket packet)
    {
        if (!session.GetHabbo().InRoom)
            return Task.CompletedTask;
        var petId = packet.ReadInt();
        if (!session.GetHabbo().CurrentRoom.GetRoomUserManager().TryGetPet(petId, out var pet))
        {
            //Okay so, we've established we have no pets in this room by this virtual Id, let us check out users, maybe they're creeping as a pet?!
            var user = session.GetHabbo().CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(petId);
            if (user == null)
                return Task.CompletedTask;

            //Check some values first, please!
            if (user.GetClient() == null || user.GetClient().GetHabbo() == null)
                return Task.CompletedTask;

            //And boom! Let us send the information composer 8-).
            session.Send(new PetInformationComposer(user.GetClient().GetHabbo()));
            return Task.CompletedTask;
        }

        //Continue as a regular pet..
        if (pet.RoomId != session.GetHabbo().CurrentRoom?.RoomId || pet.PetData == null)
            return Task.CompletedTask;
        session.Send(new PetInformationComposer(pet.PetData, _roomManager));
        return Task.CompletedTask;
    }
}