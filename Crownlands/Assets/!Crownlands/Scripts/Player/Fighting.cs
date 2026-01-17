using Mirror;
using UnityEngine;
using StarterAssets;
public class Fighting : NetworkBehaviour
{
    public WeaponCO weapon;
    public Animator anim;

    public float maxStamina = 100;
    [SyncVar] public float stamina;

    [SyncVar] public int killCount;
    public AudioClip ClashSound;
    [SerializeField] private float regenRate;
    [SerializeField] private double regenDelay = 3;
    private double regenBlockedUntil;

    StarterAssetsInputs input;

    [SerializeField] [SyncVar] double lockUntil;
    [SerializeField] float timer;

    [SyncVar] bool hasReducedStamina;
    [SyncVar] bool hasDealtDamage;
    [SyncVar] bool hasBlocked;

    float stunTime = 0.6f;
    double stunUntil;
    public enum CombatState { Free, Prepare, Attacking, Recover, Blocking, Stun }
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
                if (timer >= weapon.prepareTime) state = CombatState.Attacking;
                break;

            case CombatState.Attacking:
                if (timer >= weapon.prepareTime + weapon.attackTime) state = CombatState.Recover;
                TryDealDamage();
                break;

            case CombatState.Recover:
                if (!hasReducedStamina) ReduceStamina(weapon.staminaCostPerMiss);
                if (timer >= weapon.prepareTime + weapon.attackTime + weapon.recoverTime) state = CombatState.Free;   
                
                break;
            case CombatState.Blocking:
                if (timer >= weapon.blockTime)
                {
                    RpcBlockAnim(false);
                    
                    state = CombatState.Free;
                    if (!hasBlocked) lockUntil = NetworkTime.time + 1;
                }
                break;
            case CombatState.Stun:
                if(NetworkTime.time >= stunUntil) state = CombatState.Free;
                break;
            case CombatState.Free:
                if (NetworkTime.time >= regenBlockedUntil && stamina < maxStamina) stamina = Mathf.Min(maxStamina, stamina + regenRate * Time.fixedDeltaTime);
                
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
    [Command]
    private void CmdBlock(bool isBlocking)
    {
        
        if (state != CombatState.Free) return;
        if (NetworkTime.time < lockUntil) return;
        RpcBlockAnim(isBlocking);
        hasBlocked = false;
        timer = 0f;
        
        state = CombatState.Blocking;
    }
    [Command]
    private void CmdAttack()
    {
        if (!CanAttack()) return;

        timer = 0f;
        state = CombatState.Prepare;
        
        hasReducedStamina = false;
        hasDealtDamage = false;
        RpcAttackAnim();
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
    [ClientRpc]
    public void RpcBlockAnim(bool isBlocking)
    {
        if (anim != null) anim.SetBool("Block", isBlocking);
    }
    [Server]
    public void TryDealDamage()
    {
        if (hasDealtDamage) return;


        Vector3 origin = transform.position;
        Vector3 offsetWorld = transform.TransformDirection(weapon.hitOffsetLocal);
        Vector3 center = origin + offsetWorld;

        Collider[] hits = Physics.OverlapSphere(center, weapon.radius);

        foreach (var c in hits)
        {
            if (c.transform == transform) continue;
            if(c.TryGetComponent(out Fighting target))
            {
                if (target.state == CombatState.Blocking)
                {
                    target.ReduceStamina(target.weapon.blockStaminaCost);
                    state = CombatState.Recover;
                    target.hasBlocked = true;
                    RpcClash();
                    return;
                }
            }
            if (c.TryGetComponent<IDamageable>(out IDamageable damageable))
            {
                damageable.ServerTakeDamage(weapon.damage, netIdentity);
                if (!hasReducedStamina) ReduceStamina(weapon.staminaCostPerAttack);
                hasDealtDamage = true;
                target.RpcFeintAnim();
                target.stunUntil = NetworkTime.time + stunTime;
                target.state = CombatState.Stun;
            }
        }
    }
    [ClientRpc]
    void RpcClash()
    {
        AudioSource.PlayClipAtPoint(ClashSound, transform.position);
        anim.SetTrigger("Feint");
    }
    [Server]
    private bool CanAttack()
    {
        if (weapon == null) return false;
        if (NetworkTime.time < lockUntil) return false;
        if (stamina < weapon.staminaCostPerAttack) return false;

        if (state == CombatState.Free) return true;
        if (state == CombatState.Blocking && hasBlocked)
        {
            RpcBlockAnim(false);
            return true;
        }

        return false;
    }
    [Server]
    public void ReduceStamina(int amount)
    {
        stamina = Mathf.Max(0, stamina - amount);
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