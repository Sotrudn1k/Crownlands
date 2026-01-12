using Mirror;
using Steamworks;
using UnityEngine;

public class SteamMirrorConnectUI : MonoBehaviour
{
    [Header("Paste Host SteamID here for client")]
    public string hostSteamIdString;

    public void StartHost()
    {
        NetworkManager.singleton.StartHost();
    }

    public void StartClient()
    {
        NetworkManager.singleton.networkAddress = hostSteamIdString.Trim();

        Debug.Log($"Starting CLIENT. Connecting to host SteamID: {NetworkManager.singleton.networkAddress}");
        NetworkManager.singleton.StartClient();
    }

    public void StopAll()
    {
        if (NetworkServer.active && NetworkClient.isConnected)
            NetworkManager.singleton.StopHost();
        else if (NetworkClient.isConnected)
            NetworkManager.singleton.StopClient();
        else if (NetworkServer.active)
            NetworkManager.singleton.StopServer();
    }
}
