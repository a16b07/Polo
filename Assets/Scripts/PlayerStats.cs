using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [Header("Offense")]
    [Tooltip("Multiplies all outgoing damage.")]
    public float damageMultiplier = 1f;

    [Header("Speed")]
    [Tooltip("1 = 100% speed.")]
    public float speedMultiplier = 1f;

    [Header("Defense")]
    [Tooltip("0 = no reduction. Negative = take MORE damage. Not clamped.")]
    public float damageReduction = 0f; // no [Range] — can go negative

    [Header("Luck")]
    public float luck = 0f;

    [Header("Ammo")]
    [Tooltip("Multiplies max ammo for all weapons.")]
    public float maxAmmoMult = 1f;

    [Header("Projectile")]
    [Tooltip("Multiplies projectile speed for all weapons.")]
    public float projectileSpeedMult = 1f;
    [Tooltip("Multiplies projectile size for all weapons.")]
    public float projectileSizeMult  = 1f;
}
