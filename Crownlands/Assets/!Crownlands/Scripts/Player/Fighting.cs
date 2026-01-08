using Mirror;
using UnityEngine;
using StarterAssets;
public class Fighting : NetworkBehaviour
{
    public WeaponCO weapon;
    public Animator anim;

    public float maxStamina = 100;
    [SyncVar] public float stamina;

    public AudioClip ClashSound;
    public AudioClip HitSound;
    [SerializeField] private float regenRate;
    [SerializeField] private double regenDelay = 3;
    private double regenBlockedUntil;

    StarterAssetsInputs input;

    [SerializeField] [SyncVar] double lockUntil;
    [SerializeField] float timer;

    bool hasReducedStamina;
    bool hasDealtDamage;
    bool hasBlocked;
    public enum CombatState { Free, Prepare, Attacking, Recover, Blocking }
    [SyncVar] public CombatState state;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        input = GetComponent<StarterAssetsInputs>();
        stamina = maxStamina;
    }
    private void Update()
    {
        if (!isLocalPlayer) return;
        Inputs();
    }
    private void FixedUpdate()
    {
        if (!isServer) return;

        StateMachine();
    }
    private void Inputs()
    {
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
        if (input.block)
        {
            input.block = false;
            CmdBlock(true);
        }
    }
    private void StateMachine()
    {
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
                if (!hasReducedStamina) ReduceStamina(weapon.staminaCostPerMiss);
                if (timer >= weapon.prepareTime + weapon.attackTime + weapon.recoverTime)
                {
                    state = CombatState.Free;   
                }
                break;
            case CombatState.Blocking:
                if (timer >= weapon.blockTime)
                {
                    anim.SetBool("Block", false);
                    state = CombatState.Free;
                    if (!hasBlocked) lockUntil = NetworkTime.time + 1;
                }
                break;
            case CombatState.Free:
                if (NetworkTime.time >= regenBlockedUntil && stamina < maxStamina)
                {
                    stamina = Mathf.Min(maxStamina, stamina + regenRate * Time.fixedDeltaTime);
                }
                break;
        }
    }
    [Command]
    private void CmdFeint()
    {
        if(state != CombatState.Prepare) return;
        ReduceStamina(weapon.staminaCostPerAttack);
        lockUntil = NetworkTime.time + 0.4;
        state = CombatState.Free;
        RpcFeintAnim();
    }
    [ClientRpc]
    public void RpcAttackAnim()
    {
        anim.SetTrigger("Attack");
    }
    [ClientRpc]
    public void RpcFeintAnim()
    {
        anim.SetTrigger("Feint");
    }
    [Command]
    private void CmdBlock(bool isBlocking)
    {
        if(state != CombatState.Free) return;
        if (NetworkTime.time < lockUntil) return;
        timer = 0f;
        hasBlocked = false;
        anim.SetBool("Block", true);
        state = CombatState.Blocking;
    }
    [Command]
    private void CmdAttack()
    {
        if(weapon == null) return;
        if (NetworkTime.time < lockUntil) return;

        timer = 0f;
        state = CombatState.Prepare;
        hasReducedStamina = false;
        hasDealtDamage = false;
        RpcAttackAnim();
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
            if(c.TryGetComponent(out Fighting component))
            {
                if (component.state == CombatState.Blocking)
                {
                    component.ReduceStamina(component.weapon.blockStaminaCost);
                    state = CombatState.Recover;
                    AudioSource.PlayClipAtPoint(ClashSound, transform.forward.normalized);
                    component.hasBlocked = true;
                    anim.SetTrigger("Feint");
                    return;
                }
            }
            if (c.TryGetComponent<IDamageable>(out IDamageable damageable))
            {
                damageable.ServerTakeDamage(weapon.damage, netIdentity);
                AudioSource.PlayClipAtPoint(HitSound, transform.forward.normalized);
                if (!hasReducedStamina) ReduceStamina(weapon.staminaCostPerAttack);
                hasDealtDamage = true;
                // Optionally apply push force here
            }
        }
    }
    [Server]
    public void ReduceStamina(int amount)
    {
        stamina = Mathf.Max(0, stamina - amount); ;
        hasReducedStamina = true;
        regenBlockedUntil = NetworkTime.time + regenDelay;
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