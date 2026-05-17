using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class KnifeController : MonoBehaviour
{
    const int   WEAPON_LAYER    = 31;
    const float ATTACK_RANGE    = 4.4f;
    const float ATTACK_RADIUS   = 0.9f;
    const float ATTACK_COOLDOWN = 0.45f;

    // Idle held pose
    static readonly Vector3    HOLD_POS = new Vector3(0.06f, -0.14f, 0.28f);
    static readonly Quaternion HOLD_ROT = Quaternion.Euler(90f, 175f, 0f);

    GunPickup  _gunPickup;
    Camera     _cam;
    GameObject _knife;
    bool       _knifeVisible;
    float      _nextAttack;
    bool       _attacking;

    void Start()
    {
        _gunPickup = GetComponent<GunPickup>();
        _cam       = Camera.main;

        _knife = GameObject.Find("Knife");
        if (_knife == null) { Debug.LogWarning("KnifeController: Knife not found"); return; }

        Transform root = _gunPickup?.weaponRoot
                      ?? GameObject.Find("WeaponRoot")?.transform;
        if (root != null)
        {
            _knife.transform.SetParent(root, false);
            _knife.transform.localPosition = HOLD_POS;
            _knife.transform.localRotation = HOLD_ROT;
            _knife.transform.localScale    = Vector3.one;
        }

        SetLayerRecursive(_knife, WEAPON_LAYER);
        _knife.SetActive(false);
    }

    void Update()
    {
        if (_knife == null) return;

        bool shouldShow = _gunPickup != null && !_gunPickup.HasWeapon && !MainMenu.IsOpen;
        if (shouldShow != _knifeVisible)
        {
            _knifeVisible = shouldShow;
            _knife.SetActive(shouldShow);
            if (shouldShow) ResetPose();
        }

        if (!_knifeVisible || _attacking) return;

        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            MeleeAttack();
    }

    void MeleeAttack()
    {
        if (Time.time < _nextAttack) return;
        _nextAttack = Time.time + ATTACK_COOLDOWN;

        // Damage check first
        Vector3 origin = _cam.transform.position + _cam.transform.forward * ATTACK_RANGE;
        foreach (var col in Physics.OverlapSphere(origin, ATTACK_RADIUS))
        {
            var hitbox = col.GetComponent<Hitbox>();
            var enemy  = hitbox != null ? hitbox.enemy : col.GetComponentInParent<EnemyAI>();
            if (enemy == null) continue;
            bool headshot = origin.y >= enemy.HeadThreshold;
            enemy.TakeDamage(headshot ? 200 : 100);
            HitMarker.Instance?.Show(headshot);
            AudioManager.Instance?.PlaySFX("GreatHit");
            break;
        }

        StartCoroutine(SlashAnim());
    }

    IEnumerator SlashAnim()
    {
        _attacking = true;

        // Phase 1 — rotate X: 90 → 0  (quick)
        float dur1 = 0.07f;
        Quaternion fromRot = HOLD_ROT;
        Quaternion toRot   = Quaternion.Euler(0f, 175f, 0f);
        for (float t = 0; t < dur1; t += Time.deltaTime)
        {
            _knife.transform.localRotation = Quaternion.Lerp(fromRot, toRot, t / dur1);
            yield return null;
        }
        _knife.transform.localRotation = toRot;

        // Phase 2 — thrust Z: 0.28 → -0.4  (quick, after rotation)
        float dur2 = 0.07f;
        Vector3 fromPos = HOLD_POS;
        Vector3 toPos   = new Vector3(HOLD_POS.x, HOLD_POS.y, 0.9f);
        for (float t = 0; t < dur2; t += Time.deltaTime)
        {
            _knife.transform.localPosition = Vector3.Lerp(fromPos, toPos, t / dur2);
            yield return null;
        }
        _knife.transform.localPosition = toPos;

        // Phase 3 — return both to idle  (slightly slower)
        float dur3 = 0.12f;
        Vector3    startPos = toPos;
        Quaternion startRot = toRot;
        for (float t = 0; t < dur3; t += Time.deltaTime)
        {
            float s = t / dur3;
            _knife.transform.localPosition = Vector3.Lerp(startPos, HOLD_POS, s);
            _knife.transform.localRotation = Quaternion.Lerp(startRot, HOLD_ROT, s);
            yield return null;
        }

        ResetPose();
        _attacking = false;
    }

    void ResetPose()
    {
        if (_knife == null) return;
        _knife.transform.localPosition = HOLD_POS;
        _knife.transform.localRotation = HOLD_ROT;
    }

    void SetLayerRecursive(GameObject go, int layer)
    {
        go.layer = layer;
        foreach (Transform child in go.transform)
            SetLayerRecursive(child.gameObject, layer);
    }
}
