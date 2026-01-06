using Mirror;
using UnityEngine;

public class Health : NetworkBehaviour, IDamageable
{
    [SyncVar] public int currentHealth = 100;
    int MaxHealth;

    [Server]
    public void ServerTakeDamage(int damageAmount, NetworkIdentity Attacker)
    {
        currentHealth -= damageAmount;
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Debug.Log("Player " + netIdentity.netId + " has died.");
        }
    }
}
