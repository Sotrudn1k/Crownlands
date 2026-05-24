using Mirror;
using StarterAssets;
using UnityEngine;

public class PlayerKingdom : NetworkBehaviour
{
    [SyncVar] public uint kingdomID;
    public void CreateKingdom(string kingdomName, byte kingdomColorId) => CmdCreateKingdom(kingdomName, kingdomColorId);
    public void JoinKingdom(uint kingdomID) => CmdJoinKingdom(kingdomID);
    public void LeaveKingdom() => CmdLeaveKingdom();

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        KingdomMenuUI.Instance.SetLocalPlayer(this);
    }

    [Command]
    public void CmdCreateKingdom(string kingdomName, byte kingdomColorId)
    {
        if(!isLocalPlayer) return;
        KingdomManager.instance.CreateKingdom(this, kingdomName, kingdomColorId);
    }
    [Command]
    public void CmdJoinKingdom(uint kingdomID)
    {
        if (!isLocalPlayer) return;
        KingdomManager.instance.JoinKingdom(this, kingdomID);
    }
    [Command]
    public void CmdLeaveKingdom()
    {
        if (!isLocalPlayer) return;
        KingdomManager.instance.LeaveKingdom(this);
    }
}
