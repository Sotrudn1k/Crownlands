using Mirror;
using UnityEngine;
using StarterAssets;
using Mirror.Examples.BilliardsPredicted;
using UnityEngine.Windows;
public class Fighting : NetworkBehaviour
{
    public float timer;
    public Animator anim;
    public WeaponCO weapon;
    public bool isBlocking;
    public bool isAttacking;

    public int stamina = 100;

    StarterAssetsInputs input;
    CharacterController controller;

    bool hasDealtDamage;
    bool hasReducedStamina;
    private void Awake()
    {
        anim = GetComponent<Animator>();
        input = GetComponent<StarterAssetsInputs>();
        controller = GetComponent<CharacterController>();
    }
    private void Update()
    {
        if (!isLocalPlayer) return;
        if (input.attack)
        {
            if(isAttacking || isBlocking || stamina < weapon.staminaCostPerAttack) return;
            CmdAttack();
        }
    }
    private void FixedUpdate()
    {
        if (!isServer) return;
        if (!isAttacking) return;

        timer += Time.fixedDeltaTime;

        if (timer >= weapon.prepareTime && timer < weapon.prepareTime + weapon.attackTime)
        {
            TryDealDamage();
        }
        if (timer >= weapon.prepareTime + weapon.attackTime + weapon.recoverTime)
        {
            timer = 0f;
            isAttacking = false;
            input.attack = false;
            if(!hasReducedStamina) ReduceStamina(weapon.staminaCostPerMiss);
        }
    }
    [Command]
    public void CmdAttack()
    {
        if(weapon == null) return;

        timer = 0f;
        isAttacking = true;
        hasDealtDamage = false;
        hasReducedStamina = false;
        RpcPlayAttackAnim();
    }
    [Server]
    public void TryDealDamage()
    {
        if (hasDealtDamage) return;

        // Here you would implement the logic to detect if an enemy is in range and apply damage.
        // This is a placeholder for demonstration purposes.

        Vector3 origin = transform.position;
        Vector3 offsetWorld = transform.TransformDirection(weapon.hitOffsetLocal);
        Vector3 center = origin + offsetWorld;

        Collider[] hits = Physics.OverlapSphere(center, weapon.radius);

        foreach (var c in hits)
        {
            if (c.transform == transform) continue;
            if (c.TryGetComponent<IDamageable>(out IDamageable damageable))
            {
                damageable.ServerTakeDamage(weapon.damage, netIdentity);
                if(!hasReducedStamina) ReduceStamina(weapon.staminaCostPerAttack);
                hasDealtDamage = true;
                // Optionally apply push force here
            }
        }
    }
    [Server]
    public void ReduceStamina(int amount)
    {
        stamina -= amount;
        hasReducedStamina = true;
    }
    [ClientRpc]
    void RpcPlayAttackAnim()
    {
        if (anim != null) anim.SetTrigger("Attack");
    }
}