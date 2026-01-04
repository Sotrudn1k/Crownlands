using System.Runtime.Serialization;
using UnityEngine;

[CreateAssetMenu(fileName = "WeaponCO", menuName = "Scriptable Objects/WeaponCO")]
public class WeaponCO : ScriptableObject
{
    [Header("Weapon Stats")]
    public int range;
    public int damage;
    public float radius;
    public float pushForce;
    public Vector3 hitOffsetLocal;


    [Header("Timings")]
    public float prepareTime;
    public float attackTime;
    public float recoverTime;
    public float blockTime;

    [Header("Stamina")]
    public int blockStaminaCost;
    public int staminaCostPerMiss;
    public int staminaCostPerAttack;

    [Header("Animation")]
    public AnimatorOverrideController overrideController;

}
