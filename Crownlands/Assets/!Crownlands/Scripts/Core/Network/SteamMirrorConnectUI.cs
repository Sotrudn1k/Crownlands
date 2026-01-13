using Mirror;
using Steamworks;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SteamMirrorConnectUI : MonoBehaviour
{
    public string hostSteamId;
    public TMP_InputField hostSteamIdInputField;
    public GameObject menuPanel;


    public void StartHost()
    {
        if (!SteamManager.Initialized)
        {
            Debug.LogError("Steam is not initialized");
            return;
        }

        Debug.Log("My SteamID = " + SteamUser.GetSteamID());
        NetworkManager.singleton.StartHost();
        menuPanel.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void StartClient()
    {
        if (!SteamManager.Initialized)
        {
            Debug.LogError("Steam is not initialized");
            return;
        }

        string hostSteamId = hostSteamIdInputField.text.Trim();

        if (string.IsNullOrWhiteSpace(hostSteamId))
        {
            Debug.LogError("hostSteamId is empty");
            return;
        }
        NetworkManager.singleton.networkAddress = hostSteamId.Trim();

        Debug.Log($"Starting CLIENT. Connecting to host SteamID: {NetworkManager.singleton.networkAddress}");
        NetworkManager.singleton.StartClient();
        menuPanel.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
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
