﻿using System.Globalization;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Groups;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Rooms.AI;
using Plus.HabboHotel.Users.UserData;

namespace Plus.Communication.Packets.Outgoing.Rooms.Engine;

public class UsersComposer : IServerPacket
{
    private readonly ICollection<RoomUser> _users;
    private readonly IGroupManager _groupManager;
    private readonly IUserDataFactory _userDataFactory;

    public uint MessageId => ServerPacketHeader.UsersComposer;

    public UsersComposer(ICollection<RoomUser> users, IGroupManager groupManager, IUserDataFactory userDataFactory)
    {
        _users = users;
        _groupManager = groupManager;
        _userDataFactory = userDataFactory;
    }

    public void Compose(IOutgoingPacket packet)
    {
        packet.WriteInteger(_users.Count);
        foreach (var user in _users.ToList()) WriteUser(packet, user);
    }

    public UsersComposer(RoomUser user, IGroupManager groupManager, IUserDataFactory userDataFactory)
    {
        _users = new List<RoomUser>() { user };
        _groupManager = groupManager;
        _userDataFactory = userDataFactory;
    }

    private void WriteUser(IOutgoingPacket packet, RoomUser user)
    {
        if (!user.IsPet && !user.IsBot)
        {
            var habbo = user.GetClient().GetHabbo();
            Group group = null;
            if (habbo != null)
            {
                if (habbo.HabboStats != null)
                {
                    if (habbo.HabboStats.FavouriteGroupId > 0)
                    {
                        if (!_groupManager.TryGetGroup(habbo.HabboStats.FavouriteGroupId, out group))
                            group = null;
                    }
                }
            }
            //if (habbo.PetId == 0)
            //{
                packet.WriteInteger(habbo.Id);
                packet.WriteString(habbo.Username);
                packet.WriteString(habbo.Motto);
                packet.WriteString(habbo.Look);
                packet.WriteInteger(user.VirtualId);
                packet.WriteInteger(user.X);
                packet.WriteInteger(user.Y);
                packet.WriteString(user.Z.ToString(CultureInfo.InvariantCulture));
                packet.WriteInteger(user.RotBody); //2 for user, 4 for bot.
                packet.WriteInteger(1); //1 for user, 2 for pet, 3 for bot.
                packet.WriteString(habbo.Gender.ToLower());
                if (group != null)
                {
                    packet.WriteInteger(group.Id);
                    packet.WriteInteger(0);
                    packet.WriteString(group.Name);
                }
                else
                {
                    packet.WriteInteger(0);
                    packet.WriteInteger(0);
                    packet.WriteString("");
                }
                packet.WriteString(""); //Whats this? TG: Swim Figure
                packet.WriteInteger(habbo.HabboStats.AchievementPoints); //Achievement score
                packet.WriteBoolean(false); //Builders club? TG: Is Moderator
            //}
            //else if (habbo.PetId > 0 && habbo.PetId != 100)
            //{
            //    packet.WriteInteger(habbo.Id);
            //    packet.WriteString(habbo.Username);
            //    packet.WriteString(habbo.Motto);
            //    packet.WriteString(PetFigureForType(habbo.PetId));
            //    packet.WriteInteger(user.VirtualId);
            //    packet.WriteInteger(user.X);
            //    packet.WriteInteger(user.Y);
            //    WriteDouble(user.Z);
            //    packet.WriteInteger(user.RotBody);
            //    packet.WriteInteger(2); //Pet.
            //    packet.WriteInteger(habbo.PetId); //pet type.
            //    packet.WriteInteger(habbo.Id); //UserId of the owner.
            //    packet.WriteString(habbo.Username); //Username of the owner.
            //    packet.WriteInteger(1);
            //    packet.WriteBoolean(false); //Has saddle.
            //    packet.WriteBoolean(false); //Is someone riding this horse?
            //    packet.WriteInteger(0);
            //    packet.WriteInteger(0);
            //    packet.WriteString("");
            //}
            //else if (habbo.PetId > 0 && habbo.PetId == 100)
            //{
            //    packet.WriteInteger(habbo.Id);
            //    packet.WriteString(habbo.Username);
            //    packet.WriteString(habbo.Motto);
            //    packet.WriteString(habbo.Look.ToLower());
            //    packet.WriteInteger(user.VirtualId);
            //    packet.WriteInteger(user.X);
            //    packet.WriteInteger(user.Y);
            //    WriteDouble(user.Z);
            //    packet.WriteInteger(0);
            //    packet.WriteInteger(4);
            //    packet.WriteString(habbo.Gender.ToLower()); // ?
            //    packet.WriteInteger(habbo.Id); //Owner Id
            //    packet.WriteString(habbo.Username); // Owner name
            //    packet.WriteInteger(0); //Action Count
            //}
        }
        else if (user.IsPet)
        {
            packet.WriteInteger(user.BotAi.BaseId);
            packet.WriteString(user.BotData.Name);
            packet.WriteString(user.BotData.Motto);

            //base.WriteString("26 30 ffffff 5 3 302 4 2 201 11 1 102 12 0 -1 28 4 401 24");
            packet.WriteString(user.BotData.Look.ToLower() + (user.PetData.Saddle > 0
                ? $" 3 2 {user.PetData.PetHair} {user.PetData.HairDye} 3 {user.PetData.PetHair} {user.PetData.HairDye} 4 {user.PetData.Saddle} 0"
                : $" 2 2 {user.PetData.PetHair} {user.PetData.HairDye} 3 {user.PetData.PetHair} {user.PetData.HairDye}"));
            packet.WriteInteger(user.VirtualId);
            packet.WriteInteger(user.X);
            packet.WriteInteger(user.Y);
            packet.WriteString(user.Z.ToString(CultureInfo.InvariantCulture));
            packet.WriteInteger(0);
            packet.WriteInteger(user.BotData.AiType == BotAiType.Pet ? 2 : 4);
            packet.WriteInteger(user.PetData.Type);
            packet.WriteInteger(user.PetData.OwnerId); // userid
            packet.WriteString(user.PetData.OwnerName); // username
            packet.WriteInteger(1);
            packet.WriteBoolean(user.PetData.Saddle > 0);
            packet.WriteBoolean(user.RidingHorse);
            packet.WriteInteger(0);
            packet.WriteInteger(0);
            packet.WriteString("");
        }
        else if (user.IsBot)
        {
            packet.WriteInteger(user.BotAi.BaseId);
            packet.WriteString(user.BotData.Name);
            packet.WriteString(user.BotData.Motto);
            packet.WriteString(user.BotData.Look.ToLower());
            packet.WriteInteger(user.VirtualId);
            packet.WriteInteger(user.X);
            packet.WriteInteger(user.Y);
            packet.WriteString(user.Z.ToString(CultureInfo.InvariantCulture));
            packet.WriteInteger(0);
            packet.WriteInteger(user.BotData.AiType == BotAiType.Pet ? 2 : 4);
            packet.WriteString(user.BotData.Gender.ToLower()); // ?
            packet.WriteInteger(user.BotData.OwnerId); //Owner Id
            packet.WriteString(_userDataFactory.GetUsernameForHabboById(user.BotData.OwnerId).GetAwaiter().GetResult()); // Owner name
            packet.WriteInteger(5); //Action Count
            packet.WriteShort(1); //Copy looks
            packet.WriteShort(2); //Setup speech
            packet.WriteShort(3); //Relax
            packet.WriteShort(4); //Dance
            packet.WriteShort(5); //Change name
        }
    }

    public string PetFigureForType(int type)
    {
        switch (type)
        {
            default:
            case 60:
            {
                var randomNumber = Random.Shared.Next(1, 5);
                switch (randomNumber)
                {
                    default:
                    case 1:
                        return "0 0 f08b90 2 2 -1 1 3 -1 1";
                    case 2:
                        return "0 15 ffffff 2 2 -1 0 3 -1 0";
                    case 3:
                        return "0 20 d98961 2 2 -1 0 3 -1 0";
                    case 4:
                        return "0 21 da9dbd 2 2 -1 0 3 -1 0";
                }
            }
            case 1:
            {
                var randomNumber = Random.Shared.Next(1, 6);
                switch (randomNumber)
                {
                    default:
                    case 1:
                        return "1 18 d5b35f 2 2 -1 0 3 -1 0";
                    case 2:
                        return "1 0 ff7b3a 2 2 -1 0 3 -1 0";
                    case 3:
                        return "1 18 d98961 2 2 -1 0 3 -1 0";
                    case 4:
                        return "1 0 ff7b3a 2 2 -1 0 3 -1 1";
                    case 5:
                        return "1 24 d5b35f 2 2 -1 0 3 -1 0";
                }
            }
            case 2:
            {
                var randomNumber = Random.Shared.Next(1, 7);
                switch (randomNumber)
                {
                    default:
                    case 1:
                        return "3 3 eeeeee 2 2 -1 0 3 -1 0";
                    case 2:
                        return "3 0 ffffff 2 2 -1 0 3 -1 0";
                    case 3:
                        return "3 5 eeeeee 2 2 -1 0 3 -1 0";
                    case 4:
                        return "3 6 eeeeee 2 2 -1 0 3 -1 0";
                    case 5:
                        return "3 4 dddddd 2 2 -1 0 3 -1 0";
                    case 6:
                        return "3 5 dddddd 2 2 -1 0 3 -1 0";
                }
            }
            case 3:
            {
                var randomNumber = Random.Shared.Next(1, 6);
                switch (randomNumber)
                {
                    default:
                    case 1:
                        return "2 10 84ce84 2 2 -1 0 3 -1 0";
                    case 2:
                        return "2 8 838851 2 2 0 0 3 -1 0";
                    case 3:
                        return "2 11 b99105 2 2 -1 0 3 -1 0";
                    case 4:
                        return "2 3 e8ce25 2 2 -1 0 3 -1 0";
                    case 5:
                        return "2 2 fcfad3 2 2 -1 0 3 -1 0";
                }
            }
            case 4:
            {
                var randomNumber = Random.Shared.Next(1, 5);
                switch (randomNumber)
                {
                    default:
                    case 1:
                        return "4 2 e4feff 2 2 -1 0 3 -1 0";
                    case 2:
                        return "4 3 e4feff 2 2 -1 0 3 -1 0";
                    case 3:
                        return "4 1 eaeddf 2 2 -1 0 3 -1 0";
                    case 4:
                        return "4 0 ffffff 2 2 -1 0 3 -1 0";
                }
            }
            case 5:
            {
                var randomNumber = Random.Shared.Next(1, 8);
                switch (randomNumber)
                {
                    default:
                    case 1:
                        return "5 2 ffffff 2 2 -1 0 3 -1 0";
                    case 2:
                        return "5 0 ffffff 2 2 -1 0 3 -1 0";
                    case 3:
                        return "5 3 ffffff 2 2 -1 0 3 -1 0";
                    case 4:
                        return "5 5 ffffff 2 2 -1 0 3 -1 0";
                    case 5:
                        return "5 7 ffffff 2 2 -1 0 3 -1 0";
                    case 6:
                        return "5 1 ffffff 2 2 -1 0 3 -1 0";
                    case 7:
                        return "5 8 ffffff 2 2 -1 0 3 -1 0";
                }
            }
            case 6:
            {
                var randomNumber = Random.Shared.Next(1, 12);
                switch (randomNumber)
                {
                    default:
                    case 1:
                        return "6 0 ffffff 2 2 -1 0 3 -1 0";
                    case 2:
                        return "6 1 ffffff 2 2 -1 0 3 -1 0";
                    case 3:
                        return "6 2 ffffff 2 2 -1 0 3 -1 0";
                    case 4:
                        return "6 3 ffffff 2 2 -1 0 3 -1 0";
                    case 5:
                        return "6 4 ffffff 2 2 -1 0 3 -1 0";
                    case 6:
                        return "6 0 ffd8c9 2 2 -1 0 3 -1 0";
                    case 7:
                        return "6 5 ffffff 2 2 -1 0 3 -1 0";
                    case 8:
                        return "6 11 ffffff 2 2 -1 0 3 -1 0";
                    case 9:
                        return "6 2 ffe49d 2 2 -1 0 3 -1 0";
                    case 10:
                        return "6 11 ff9ae 2 2 -1 0 3 -1 0";
                    case 11:
                        return "6 2 ff9ae 2 2 -1 0 3 -1 0";
                }
            }
            case 7:
            {
                var randomNumber = Random.Shared.Next(1, 8);
                switch (randomNumber)
                {
                    default:
                    case 1:
                        return "7 5 aeaeae 2 2 -1 0 3 -1 0";
                    case 2:
                        return "7 7 ffc99a 2 2 -1 0 3 -1 0";
                    case 3:
                        return "7 5 cccccc 2 2 -1 0 3 -1 0";
                    case 4:
                        return "7 5 9adcff 2 2 -1 0 3 -1 0";
                    case 5:
                        return "7 5 ff7d6a 2 2 -1 0 3 -1 0";
                    case 6:
                        return "7 6 cccccc 2 2 -1 0 3 -1 0";
                    case 7:
                        return "7 0 cccccc 2 2 -1 0 3 -1 0";
                }
            }
            case 8:
            {
                var randomNumber = Random.Shared.Next(1, 14);
                switch (randomNumber)
                {
                    default:
                    case 1:
                        return "8 0 ffffff 2 2 -1 0 3 -1 0";
                    case 2:
                        return "8 1 ffffff 2 2 -1 0 3 -1 0";
                    case 3:
                        return "8 2 ffffff 2 2 -1 0 3 -1 0";
                    case 4:
                        return "8 3 ffffff 2 2 -1 0 3 -1 0";
                    case 5:
                        return "8 4 ffffff 2 2 -1 0 3 -1 0";
                    case 6:
                        return "8 14 ffffff 2 2 -1 0 3 -1 0";
                    case 7:
                        return "8 11 ffffff 2 2 -1 0 3 -1 0";
                    case 8:
                        return "8 8 ffffff 2 2 -1 0 3 -1 0";
                    case 9:
                        return "8 6 ffffff 2 2 -1 0 3 -1 0";
                    case 10:
                        return "8 5 ffffff 2 2 -1 0 3 -1 0";
                    case 11:
                        return "8 9 ffffff 2 2 -1 0 3 -1 0";
                    case 12:
                        return "8 10 ffffff 2 2 -1 0 3 -1 0";
                    case 13:
                        return "8 7 ffffff 2 2 -1 0 3 -1 0";
                }
            }
            case 9:
            {
                var randomNumber = Random.Shared.Next(1, 10);
                switch (randomNumber)
                {
                    default:
                    case 1:
                        return "9 0 ffffff 2 2 -1 0 3 -1 0";
                    case 2:
                        return "9 1 ffffff 2 2 -1 0 3 -1 0";
                    case 3:
                        return "9 2 ffffff 2 2 -1 0 3 -1 0";
                    case 4:
                        return "9 3 ffffff 2 2 -1 0 3 -1 0";
                    case 5:
                        return "9 4 ffffff 2 2 -1 0 3 -1 0";
                    case 6:
                        return "9 5 ffffff 2 2 -1 0 3 -1 0";
                    case 7:
                        return "9 6 ffffff 2 2 -1 0 3 -1 0";
                    case 8:
                        return "9 7 ffffff 2 2 -1 0 3 -1 0";
                    case 9:
                        return "9 8 ffffff 2 2 -1 0 3 -1 0";
                }
            }
            case 10:
            {
                        return "10 0 ffffff 2 2 -1 0 3 -1 0";
            }
            case 11:
            {
                var randomNumber = Random.Shared.Next(1, 14);
                switch (randomNumber)
                {
                    default:
                    case 1:
                        return "11 1 ffffff 2 2 -1 0 3 -1 0";
                    case 2:
                        return "11 2 ffffff 2 2 -1 0 3 -1 0";
                    case 3:
                        return "11 3 ffffff 2 2 -1 0 3 -1 0";
                    case 4:
                        return "11 4 ffffff 2 2 -1 0 3 -1 0";
                    case 5:
                        return "11 5 ffffff 2 2 -1 0 3 -1 0";
                    case 6:
                        return "11 9 ffffff 2 2 -1 0 3 -1 0";
                    case 7:
                        return "11 10 ffffff 2 2 -1 0 3 -1 0";
                    case 8:
                        return "11 6 ffffff 2 2 -1 0 3 -1 0";
                    case 9:
                        return "11 12 ffffff 2 2 -1 0 3 -1 0";
                    case 10:
                        return "11 11 ffffff 2 2 -1 0 3 -1 0";
                    case 11:
                        return "11 15 ffffff 2 2 -1 0 3 -1 0";
                    case 12:
                        return "11 13 ffffff 2 2 -1 0 3 -1 0";
                    case 13:
                        return "11 18 ffffff 2 2 -1 0 3 -1 0";
                }
            }
            case 12:
            {
                var randomNumber = Random.Shared.Next(1, 7);
                switch (randomNumber)
                {
                    default:
                    case 1:
                        return "12 0 ffffff 2 2 -1 0 3 -1 0";
                    case 2:
                        return "12 1 ffffff 2 2 -1 0 3 -1 0";
                    case 3:
                        return "12 2 ffffff 2 2 -1 0 3 -1 0";
                    case 4:
                        return "12 3 ffffff 2 2 -1 0 3 -1 0";
                    case 5:
                        return "12 4 ffffff 2 2 -1 0 3 -1 0";
                    case 6:
                        return "12 5 ffffff 2 2 -1 0 3 -1 0";
                }
            }
            case 14:
            {
                var randomNumber = Random.Shared.Next(1, 15);
                switch (randomNumber)
                {
                    default:
                    case 1:
                        return "14 0 ffffff 2 2 -1 0 3 -1 0";
                    case 2:
                        return "14 1 ffffff 2 2 -1 0 3 -1 0";
                    case 3:
                        return "14 2 ffffff 2 2 -1 0 3 -1 0";
                    case 4:
                        return "14 3 ffffff 2 2 -1 0 3 -1 0";
                    case 5:
                        return "14 6 ffffff 2 2 -1 0 3 -1 0";
                    case 6:
                        return "14 4 ffffff 2 2 -1 0 3 -1 0";
                    case 7:
                        return "14 5 ffffff 2 2 -1 0 3 -1 0";
                    case 8:
                        return "14 7 ffffff 2 2 -1 0 3 -1 0";
                    case 9:
                        return "14 8 ffffff 2 2 -1 0 3 -1 0";
                    case 10:
                        return "14 9 ffffff 2 2 -1 0 3 -1 0";
                    case 11:
                        return "14 10 ffffff 2 2 -1 0 3 -1 0";
                    case 12:
                        return "14 11 ffffff 2 2 -1 0 3 -1 0";
                    case 13:
                        return "14 12 ffffff 2 2 -1 0 3 -1 0";
                    case 14:
                        return "14 13 ffffff 2 2 -1 0 3 -1 0";
                }
            }
            case 15:
            {
                var randomNumber = Random.Shared.Next(1, 21);
                switch (randomNumber)
                {
                    default:
                    case 1:
                        return "15 2 ffffff 2 2 -1 0 3 -1 0";
                    case 2:
                        return "15 3 ffffff 2 2 -1 0 3 -1 0";
                    case 3:
                        return "15 4 ffffff 2 2 -1 0 3 -1 0";
                    case 4:
                        return "15 5 ffffff 2 2 -1 0 3 -1 0";
                    case 5:
                        return "15 6 ffffff 2 2 -1 0 3 -1 0";
                    case 6:
                        return "15 7 ffffff 2 2 -1 0 3 -1 0";
                    case 7:
                        return "15 8 ffffff 2 2 -1 0 3 -1 0";
                    case 8:
                        return "15 9 ffffff 2 2 -1 0 3 -1 0";
                    case 9:
                        return "15 10 ffffff 2 2 -1 0 3 -1 0";
                    case 10:
                        return "15 11 ffffff 2 2 -1 0 3 -1 0";
                    case 11:
                        return "15 12 ffffff 2 2 -1 0 3 -1 0";
                    case 12:
                        return "15 13 ffffff 2 2 -1 0 3 -1 0";
                    case 13:
                        return "15 14 ffffff 2 2 -1 0 3 -1 0";
                    case 14:
                        return "15 15 ffffff 2 2 -1 0 3 -1 0";
                    case 15:
                        return "15 16 ffffff 2 2 -1 0 3 -1 0";
                    case 16:
                        return "15 17 ffffff 2 2 -1 0 3 -1 0";
                    case 17:
                        return "15 78 ffffff 2 2 -1 0 3 -1 0";
                    case 18:
                        return "15 77 ffffff 2 2 -1 0 3 -1 0";
                    case 19:
                        return "15 79 ffffff 2 2 -1 0 3 -1 0";
                    case 20:
                        return "15 80 ffffff 2 2 -1 0 3 -1 0";
                }
            }
            case 17:
            {
                var randomNumber = Random.Shared.Next(1, 9);
                switch (randomNumber)
                {
                    default:
                    case 1:
                        return "17 1 ffffff";
                    case 2:
                        return "17 2 ffffff";
                    case 3:
                        return "17 3 ffffff";
                    case 4:
                        return "17 4 ffffff";
                    case 5:
                        return "17 5 ffffff";
                    case 6:
                        return "18 0 ffffff";
                    case 7:
                        return "19 0 ffffff";
                    case 8:
                        return "20 0 ffffff";
                }
            }
            case 21:
            {
                var randomNumber = Random.Shared.Next(1, 3);
                switch (randomNumber)
                {
                    default:
                    case 1:
                        return "21 0 ffffff";
                    case 2:
                        return "22 0 ffffff";
                }
            }
            case 23:
            {
                var randomNumber = Random.Shared.Next(1, 4);
                switch (randomNumber)
                {
                    default:
                    case 1:
                        return "23 0 ffffff";
                    case 2:
                        return "23 1 ffffff";
                    case 3:
                        return "23 3 ffffff";
                }
            }
            case 26:
            {
                var randomNumber = Random.Shared.Next(1, 5);
                switch (randomNumber)
                {
                    default:
                    case 1:
                        return "26 1 ffffff 5 0 -1 0 4 402 5 3 301 4 1 101 2 2 201 3";
                    case 2:
                        return "26 1 ffffff 5 0 -1 0 1 102 13 3 301 4 4 401 5 2 201 3";
                    case 3:
                        return "26 6 ffffff 5 1 102 8 2 201 16 4 401 9 3 303 4 0 -1 6";
                    case 4:
                        return "26 30 ffffff 5 0 -1 0 3 303 4 4 401 5 1 101 2 2 201 3";
                }
            }
        }
    }
}