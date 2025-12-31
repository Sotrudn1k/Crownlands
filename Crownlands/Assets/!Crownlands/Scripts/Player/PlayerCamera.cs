using Mirror;
using Mirror.Examples.Common;
using UnityEngine;

public class PlayerCamera : NetworkBehaviour
{
    [SerializeField] Camera playerCamera;
    [SerializeField] AudioListener audioListener;

    public override void OnStartLocalPlayer()
    {
        if (playerCamera) playerCamera.enabled = true;
        if (audioListener) audioListener.enabled = true;
    }

    public override void OnStartClient()
    {
        if (playerCamera) playerCamera.enabled = false;
        if (audioListener) audioListener.enabled = false;
    }
}
