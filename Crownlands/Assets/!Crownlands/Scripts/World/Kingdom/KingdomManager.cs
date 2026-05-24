 using Mirror;
using System;
using System.Collections.Generic;
using UnityEngine;

public class KingdomManager : NetworkBehaviour
{
    public static KingdomManager instance;
    [SerializeField] private Dictionary<uint, KingdomData> kingdoms = new Dictionary<uint, KingdomData>();

    private void Awake()
    {
        instance = this;
    }

    [Server]
    public void CreateKingdom(PlayerKingdom creator, string kingdomName, byte kingdomColorId)
    {
        uint newKingdomID = (uint)kingdoms.Count + 1;
        KingdomData newKingdom = new KingdomData
        {
            kingdomName = kingdomName,
            kingdomColorId = kingdomColorId,
            kingdomID = newKingdomID,
        };
        newKingdom.memberIDs.Add(creator.netId);
        kingdoms.Add(newKingdomID, newKingdom);
        creator.kingdomID = newKingdomID;

        RpcUpdateKingdomList();
    }

    private KingdomInfo[] GetKingdomInfos()
    {
        KingdomInfo[] infos = new KingdomInfo[kingdoms.Count];
        int i = 0;
        foreach (var k in kingdoms.Values)
        {
            infos[i++] = new KingdomInfo
            {
                id = k.kingdomID,
                name = k.kingdomName,
                colorId = k.kingdomColorId
            };
        }
        return infos;
    }

    [Server]
    public void JoinKingdom(PlayerKingdom player, uint kingdomID)
    {
        if (kingdoms.ContainsKey(kingdomID))
        {
            RpcUpdatePlayerKingdom(player, kingdomID);
        }
    }
    [Server]
    public void LeaveKingdom(PlayerKingdom player)
    {
        uint kingdomID = player.kingdomID;
        if (kingdomID == 0) return;

        if (kingdoms.ContainsKey(kingdomID))
        {
            kingdoms[kingdomID].memberIDs.Remove(player.netId);
            player.kingdomID = 0;
        }
    }

    [ClientRpc]
    public void RpcUpdateKingdomList()
    {
        KingdomMenuUI.Instance.RefreshList(GetKingdomInfos());
    }

    [ClientRpc]
    public void RpcUpdatePlayerKingdom(PlayerKingdom player, uint kingdomId)
    {
        // This can be used to update the player's kingdom info on the client side if needed
        kingdoms[kingdomId].memberIDs.Add(player.netId);
        player.kingdomID = kingdomId;
    }
}
    public class KingdomData
{
    public byte kingdomColorId;
    public string kingdomName;
    public uint kingdomID;
    public uint leaderID;
    public HashSet<uint> memberIDs = new HashSet<uint>();
}
