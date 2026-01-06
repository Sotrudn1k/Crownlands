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
    [SyncVar] public int stamina = 100;
    public float lockUntilTime;

    StarterAssetsInputs input;
    CharacterController controller;

    bool hasDealtDamage;
    bool hasReducedStamina;

    public enum CombatState { Free, Prepare, Attacking, Recover, Blocking }
    [SyncVar] public CombatState state;
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
            input.attack = false;
            if (state != CombatState.Free || stamina < weapon.staminaCostPerAttack) return;
            CmdAttack(); 
        }
        if (input.feint)
        {
            input.feint = false;
            CmdFeint();
        }
        if(state == CombatState.Free)
        {
            timer = 0f;
            input.block = false;
            
            lockUntilTime -= Time.fixedDeltaTime;
        }
    }
    private void FixedUpdate()
    {
        if (!isServer) return;
        
        timer += Time.fixedDeltaTime;
        switch (state)
        {
            case CombatState.Prepare:
                if (timer >= weapon.prepareTime)
                {
                    state = CombatState.Attacking;
                }
                break;

            case CombatState.Attacking:
                if (timer >= weapon.prepareTime + weapon.attackTime) state = CombatState.Recover;
                TryDealDamage();
                break;

            case CombatState.Recover:
                if (timer >= weapon.prepareTime + weapon.attackTime + weapon.recoverTime)
                {
                    
                    state = CombatState.Free;
                    if (!hasReducedStamina) ReduceStamina(weapon.staminaCostPerMiss);
                }
                break;
            case CombatState.Blocking:
                break;
        }
    }
    [Command]
    private void CmdFeint()
    {
        if(state != CombatState.Prepare) return;
        anim.SetTrigger("Feint");
        lockUntilTime = 0.4f;
        state = CombatState.Free;
        ReduceStamina(weapon.staminaCostPerAttack);
    }
    [Command]
    private void CmdBlock()
    {

    }
    [Command]
    private void CmdAttack()
    {
        if(weapon == null) return;
        if (lockUntilTime >= 0) return;

        timer = 0f;
        hasDealtDamage = false;
        anim.SetTrigger("Attack");
        hasReducedStamina = false;
        state = CombatState.Prepare;
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
    private void OnDrawGizmos()
    {
        Vector3 origin = transform.position;
        Vector3 offsetWorld = transform.TransformDirection(weapon.hitOffsetLocal);
        Vector3 center = origin + offsetWorld;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(center, weapon.radius);
    }
}