using Mirror;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Windows;

public class Movement : NetworkBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 5f;
    public float gravity = -20f;
    public float sprintSpeed = 7f;

    CharacterController controller;

    bool localSprint;
    Vector2 localMoveInput;

    Vector2 serverMoveInput;
    bool serverSprint;
    float serverVerticalVelocity;

    float lastSend;
    const float sendRate = 0.05f; // 0.05f = 20 times per sec

    PlayerInput pi;
    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        pi = GetComponent<PlayerInput>();
        if (pi != null) pi.enabled = false;
    }
    public override void OnStartLocalPlayer()
    {
        if (pi != null) pi.enabled = true; // ?????? ?????????? ????????
        if (Keyboard.current != null && Mouse.current != null)
            pi.SwitchCurrentControlScheme(Keyboard.current, Mouse.current);
    }
    public override void OnStopLocalPlayer()
    {
        if (pi != null) pi.enabled = false;
    }
    private void Update()
    {
        if (!isLocalPlayer) return;
        if (Time.time - lastSend >= sendRate)
        {
            lastSend = Time.time;
            CmdSetInput(localMoveInput, localSprint);
        }
    }
    public void OnMove(InputValue value)
    {
        if (!isLocalPlayer) return;
        localMoveInput = value.Get<Vector2>();
    }
    public void OnSprint(InputValue value)
    {
        if (!isLocalPlayer) return;
        localSprint = value.isPressed;
    }
    [Command]
    private void CmdSetInput(Vector2 move, bool sprint)
    {
        serverMoveInput = Vector2.ClampMagnitude(move, 1f);
        serverSprint = sprint;
    }
    [ServerCallback]
    private void FixedUpdate()
    {
        float speed = serverSprint ? sprintSpeed : walkSpeed;

        Vector3 move = new Vector3(serverMoveInput.x, 0f, serverMoveInput.y);

        if (controller.isGrounded && serverVerticalVelocity < 0f)
            serverVerticalVelocity = -2f;

        serverVerticalVelocity += gravity * Time.fixedDeltaTime;

        Vector3 velocity = move * speed + Vector3.up * serverVerticalVelocity;

        controller.Move(velocity * Time.fixedDeltaTime);
    }
}