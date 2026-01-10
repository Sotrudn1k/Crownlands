using Mirror;
using UnityEngine;

public class Health : NetworkBehaviour, IDamageable
{
    public int MaxHealth = 100;
    [SyncVar] public int currentHealth;


    public AudioClip HitSound;
    GameManager GameManager;
    Animator anim;
    PlayerUI playerUI;
    private void Awake()
    {
        GameManager = FindAnyObjectByType<GameManager>();
        playerUI = GetComponent<PlayerUI>();
        anim = GetComponent<Animator>();
    }
    public override void OnStartServer()
    {
        base.OnStartServer();
        currentHealth = MaxHealth;
    }
    [Server]
    public void ServerTakeDamage(int damageAmount, NetworkIdentity Attacker)
    {
        currentHealth -= damageAmount;
        RpcHit(damageAmount);
        if (currentHealth <= 0)
        {
            if (Attacker != null)
            {
                Fighting fighting = Attacker.GetComponent<Fighting>();
                if (fighting != null)
                {
                    fighting.killCount++;
                    Health attackerHealth = Attacker.GetComponent<Health>();
                    if (attackerHealth != null)
                    {
                        attackerHealth.currentHealth = attackerHealth.MaxHealth;
                    }
                }
            }

            GameManager.RespawnPlayer(this);
        }
    }
    [ClientRpc]
    void RpcHit(int damageAmount)
    {
        AudioSource.PlayClipAtPoint(HitSound, transform.position);
        anim.SetTrigger("Hit");
    }
    [TargetRpc]
    public void TargetRespawn(NetworkConnectionToClient conn, Vector3 spawnPos)
    {
        var controller = GetComponent<CharacterController>();
        if (controller != null) controller.enabled = false;

        transform.position = spawnPos;

        if (controller != null) controller.enabled = true;
    }
}
