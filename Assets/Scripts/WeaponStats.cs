using UnityEngine;

public class WeaponStats : MonoBehaviour
{
    public string  weaponName   = "Unknown";
    public int     damage       = 25;
    public float   rpm          = 450f;
    public float   weight       = 1f;
    public float   recoilKick   = 4f;
    public Vector3 heldPosition = new Vector3(0f, 0f, 0f);
    public Vector3 heldRotation = new Vector3(0f, 180f, 0f);

    [Header("Shotgun")]
    public int   pelletCount  = 1;
    public float pelletSpread = 0f;

    [Header("Spread")]
    public float baseSpread   = 0f;
    public float maxSpread    = 0f;
    public float heatPerShot  = 0f;
    public float heatCooldown = 0f;

    [Header("Projectile")]
    public float projectileSpeed = 55f;
    public float projectileSize  = 0.05f;

    [Header("Ammo")]
    public int maxAmmo = 30;
    [HideInInspector] public int currentAmmo;

    [Header("Audio")]
    public string shootSFX = "ShootLight";

    void Awake()
    {
        int min = Mathf.Max(1, Mathf.CeilToInt(maxAmmo * 0.20f));
        currentAmmo = Random.Range(min, maxAmmo + 1);
    }
}
