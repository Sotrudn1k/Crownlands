using Mirror;
using UnityEngine;

public interface IDamageable
{
    public void ServerTakeDamage(int damageAmount, NetworkIdentity Attacker);
}
