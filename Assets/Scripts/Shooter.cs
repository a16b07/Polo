using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class Shooter : MonoBehaviour
{
    [Header("References")]
    public Camera    cam;
    public Transform weaponRoot;

    [Header("Spread")]
    public float baseSpread   = 0f;
    public float maxSpread    = 5f;
    public float heatPerShot  = 0.18f;
    public float heatCooldown = 4f;

    [Header("Muzzle Flash")]
    public float flashDuration  = 0.06f;
    public float flashIntensity = 22f;
    public float flashRange     = 8f;

    [Header("Recoil")]
    public float recoilReturn = 60f;
    public float recoilMax    = 4f;

    public float Heat => _heat;

    Light       _flash;
    float       _nextFireTime;
    PerkManager _pm;
    PlayerStats _ps;
    public float _infAmmoTimer; // set externally by perk

    bool InfiniteAmmo => _infAmmoTimer > 0f || (_pm != null && _pm.HasFlag("INFINITE_AMMO"));
    Vector3     _recoilOriginPos;
    Quaternion  _recoilOriginRot;
    float       _recoilPitch;
    float       _heat;

    void Start()
    {
        if (cam        == null) cam        = Camera.main;
        if (weaponRoot == null) weaponRoot = GameObject.Find("WeaponRoot")?.transform;

        _recoilOriginPos = weaponRoot.localPosition;
        _recoilOriginRot = weaponRoot.localRotation;

        var go           = new GameObject("MuzzleFlash");
        go.transform.SetParent(weaponRoot);
        go.transform.localPosition = new Vector3(0f, 0f, 0.35f);
        _flash           = go.AddComponent<Light>();
        _flash.type      = LightType.Point;
        _flash.color     = new Color(1f, 0.6f, 0.1f);
        _flash.range     = flashRange;
        _flash.intensity = 0f;
        _flash.shadows   = LightShadows.None;
    }

    void OnEnable()
    {
        if (weaponRoot == null) return;
        _recoilOriginPos = weaponRoot.localPosition;
        _recoilOriginRot = weaponRoot.localRotation;
    }

    void Update()
    {
        if (Mouse.current == null) return;

        if (_pm == null) _pm = PerkManager.Instance ?? FindFirstObjectByType<PerkManager>();
        if (_ps == null) _ps = GetComponent<PlayerStats>();

        // Tick infinite ammo timer
        if (_infAmmoTimer > 0f) _infAmmoTimer -= Time.deltaTime;

        var activeStats = weaponRoot != null ? weaponRoot.GetComponentInChildren<WeaponStats>() : null;
        bool hasFrenzy  = _pm != null && (_pm.HasFlag("FRENZY") || _pm.HasFlag("HEX"));
        float cooldown  = hasFrenzy ? 0f
            : (activeStats != null && activeStats.heatCooldown > 0) ? activeStats.heatCooldown : heatCooldown;
        _heat = Mathf.Max(0f, _heat - Time.deltaTime * cooldown);

        if (_recoilPitch > 0f)
        {
            _recoilPitch = Mathf.Max(0f, _recoilPitch - recoilReturn * Time.deltaTime);
            ApplyPose();
        }

        if (Mouse.current.leftButton.isPressed && Time.time >= _nextFireTime)
            Shoot();
    }

    void Shoot()
    {
        int   damage     = 25;
        float rpm        = 450f;
        float kick       = 3f;
        int   pellets    = 1;
        float pelletSpread = 0f;
        float projSpeed  = 55f;
        float projSize   = 0.05f;

        float wBaseSpread  = baseSpread;
        float wMaxSpread   = maxSpread;
        float wHeatPerShot = heatPerShot;

        var stats = weaponRoot.GetComponentInChildren<WeaponStats>();

        // Ammo check
        if (stats != null && !InfiniteAmmo && stats.currentAmmo <= 0) return;

        if (stats != null)
        {
            damage        = stats.damage;
            rpm           = stats.rpm;
            kick          = stats.recoilKick;
            pellets       = stats.pelletCount;
            pelletSpread  = stats.pelletSpread;
            projSpeed     = stats.projectileSpeed;
            projSize      = stats.projectileSize;
            if (stats.baseSpread  > 0) wBaseSpread  = stats.baseSpread;
            if (stats.maxSpread   > 0) wMaxSpread   = stats.maxSpread;
            if (stats.heatPerShot > 0) wHeatPerShot = stats.heatPerShot;
        }

        // Apply perk/stat modifiers
        if (_pm != null && (_pm.HasFlag("HEAVY_AMMO") || _pm.HasFlag("HEX"))) wHeatPerShot *= 2f;
        if (_pm != null && _pm.HasFlag("SCATTER_SHOT")) wMaxSpread *= 1.9f;
        wBaseSpread = 0f; // always start at 0
        if (_ps != null) { projSpeed *= _ps.projectileSpeedMult; projSize *= _ps.projectileSizeMult; }
        int finalDmg = Mathf.RoundToInt(damage * (_ps != null ? _ps.damageMultiplier : 1f));

        _nextFireTime = Time.time + 60f / rpm;
        _recoilPitch  = Mathf.Min(_recoilPitch + kick, recoilMax);
        ApplyPose();

        float spread = Mathf.Lerp(wBaseSpread, wMaxSpread, _heat);
        _heat = Mathf.Min(1f, _heat + wHeatPerShot);

        // Consume ammo (once per trigger pull regardless of pellet count)
        if (stats != null && !InfiniteAmmo)
        {
            bool conserve = _pm != null && _pm.HasFlag("AMMO_CONSERVE") && Random.value < 0.30f;
            int  consume  = (!conserve && _pm != null && _pm.HasFlag("DOUBLE_CONSUME")) ? 2 : 1;
            if (!conserve) stats.currentAmmo = Mathf.Max(0, stats.currentAmmo - consume);
        }
        // Last Shot: double damage when that was the last bullet
        if (stats != null && stats.currentAmmo == 0 && _pm != null && _pm.HasFlag("LAST_SHOT"))
            finalDmg *= 2;

        for (int i = 0; i < pellets; i++)
        {
            float totalSpread = spread + (pellets > 1 ? pelletSpread : 0f);
            Vector2 sp  = Random.insideUnitCircle * totalSpread;
            Vector3 dir = Quaternion.Euler(sp.x, sp.y, 0f) * cam.transform.forward;

            SpawnBullet(cam.transform.position, dir, finalDmg, projSpeed, projSize);
        }

        StartCoroutine(Flash());
    }

    void SpawnBullet(Vector3 origin, Vector3 dir, int dmg, float speed, float size)
    {
        var go     = new GameObject("Bullet");
        go.transform.position = origin;
        go.transform.rotation = Quaternion.LookRotation(dir);

        // Apply projectile perks
        if (_pm != null)
        {
            if (_pm.HasFlag("PROJ_SIZE_X25"))  size  *= 2.5f;
            if (_pm.HasFlag("PROJ_SPEED_180")) speed *= 1.8f;
            if (_pm.HasFlag("CANNONBALL"))   { size  *= 4.0f; speed *= 0.5f; }
            if (_pm.HasFlag("SNIPER"))       { size  *= 0.3f; speed *= 2.0f; }
        }

        var b             = go.AddComponent<Bullet>();
        b.damage          = dmg;
        b.speed           = speed;
        b.radius          = size;
        b.explosiveRounds = _pm != null && _pm.HasFlag("EXPLOSIVE_ROUNDS");
        b.piercing        = _pm != null && _pm.HasFlag("PIERCING");
    }

    void ApplyPose()
    {
        weaponRoot.localRotation = _recoilOriginRot * Quaternion.Euler(-_recoilPitch, 0f, 0f);
        weaponRoot.localPosition = _recoilOriginPos + Vector3.back * (_recoilPitch * 0.001f);
    }

    IEnumerator Flash()
    {
        _flash.intensity = flashIntensity;
        yield return new WaitForSeconds(flashDuration);
        _flash.intensity = 0f;
        yield return new WaitForSeconds(flashDuration * 0.4f);
        _flash.intensity = flashIntensity * 0.45f;
        yield return new WaitForSeconds(flashDuration * 0.8f);
        _flash.intensity = 0f;
    }
}
