﻿using System.Data;
using Plus.HabboHotel.GameClients;
using Plus.Utilities;

namespace Plus.Communication.Packets.Outgoing.Moderation;

public class ModeratorUserInfoComposer : IServerPacket
{
    private readonly DataRow _user;
    private readonly DataRow _info;
    private readonly IGameClientManager _clientManager;
    public uint MessageId => ServerPacketHeader.ModeratorUserInfoComposer;

    public ModeratorUserInfoComposer(DataRow user, DataRow info, IGameClientManager clientManager)
    {
        _user = user;
        _info = info;
        _clientManager = clientManager;
    }

    public void Compose(IOutgoingPacket packet)
    {
        var origin = new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds(Convert.ToDouble(_info["trading_locked"]));
        packet.WriteInteger(_user != null ? Convert.ToInt32(_user["id"]) : 0);
        packet.WriteString(_user != null ? Convert.ToString(_user["username"]) : "Unknown");
        packet.WriteString(_user != null ? Convert.ToString(_user["look"]) : "Unknown");
        packet.WriteInteger(_user != null ? Convert.ToInt32(Math.Ceiling((UnixTimestamp.GetNow() - Convert.ToDouble(_user["account_created"])) / 60)) : 0);
        packet.WriteInteger(_user != null ? Convert.ToInt32(Math.Ceiling((UnixTimestamp.GetNow() - Convert.ToDouble(_user["last_online"])) / 60)) : 0);
        packet.WriteBoolean(_user != null ? _clientManager.GetClientByUserId(Convert.ToInt32(_user["id"])) != null : false);
        packet.WriteInteger(_info != null ? Convert.ToInt32(_info["cfhs"]) : 0);
        packet.WriteInteger(_info != null ? Convert.ToInt32(_info["cfhs_abusive"]) : 0);
        packet.WriteInteger(_info != null ? Convert.ToInt32(_info["cautions"]) : 0);
        packet.WriteInteger(_info != null ? Convert.ToInt32(_info["bans"]) : 0);
        packet.WriteInteger(_info != null ? Convert.ToInt32(_info["trading_locks_count"]) : 0); //Trading lock counts
        packet.WriteString(Convert.ToDouble(_info["trading_locked"]) != 0 ? origin.ToString("dd/MM/yyyy HH:mm:ss") : "0"); //Trading lock
        packet.WriteString(""); //Purchases
        packet.WriteInteger(0); //Itendity information tool
        packet.WriteInteger(0); //Id bans.
        packet.WriteString(_user != null ? Convert.ToString(_user["mail"]) : "Unknown");
        packet.WriteString(""); //user_classification
    }
}