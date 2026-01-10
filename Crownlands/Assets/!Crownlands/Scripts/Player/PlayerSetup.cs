using UnityEngine;
using Mirror;
using UnityEngine.InputSystem;
using StarterAssets;
using Cinemachine;
public class PlayerSetup : NetworkBehaviour
{
    public PlayerInput input;
    public ThirdPersonController controller;
    [SerializeField] private Transform followTarget;
    public override void OnStartLocalPlayer()
    {
        if (input != null) input.enabled = true;
        if (controller != null) controller.enabled = true;
        var vcam = FindAnyObjectByType<CinemachineVirtualCamera>();
        if (vcam != null)
        {
            vcam.Follow = followTarget;
            vcam.LookAt = followTarget;
        }
    }
}
