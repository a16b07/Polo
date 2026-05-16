using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class GunPickup : MonoBehaviour
{
    const int WeaponLayer = 31;

    [Header("References")]
    public Transform weaponRoot;
    public Camera    cam;
    public Text      promptText;

    [Header("Throw")]
    public float baseThrowForce = 9f;

    [Header("Pickup")]
    public float pickupRange  = 3.5f;
    public float sphereRadius = 0.25f;

    [Header("Weapon View")]
    public float weaponFOV = 50f;

    public int CurrentAmmo => _held != null ? (_held.GetComponent<WeaponStats>()?.currentAmmo ?? -1) : -1;
    public int MaxAmmo     => _held != null ? (_held.GetComponent<WeaponStats>()?.maxAmmo     ??  0) :  0;
    public bool IsEmpty    => _held != null && CurrentAmmo == 0;

    GameObject    _held;
    Shooter       _shooter;
    Camera        _weaponCam;
    RenderTexture _weaponRT;

    [Header("Weapon Bob")]
    public float bobFrequency  = 8f;
    public float bobAmplitudeY = 0.025f;
    public float bobAmplitudeX = 0.012f;

    float               _bobT;
    float               _bobAmp;
    Vector3             _weaponOrigin;
    Vector3             _prevBobPos;
    CharacterController _cc;

    static PhysicsMaterial _deadBounce;

void Start()
    {
        _shooter = GetComponent<Shooter>();
        if (cam        == null) cam        = Camera.main;
        if (weaponRoot == null) weaponRoot = GameObject.Find("WeaponRoot")?.transform;

        if (promptText == null)
        {
            var hud = GameObject.Find("HUD");
            if (hud != null)
            {
                var t = hud.transform.Find("PickupPrompt");
                if (t != null) promptText = t.GetComponent<Text>();
            }
        }

        SetupWeaponUI();
        _weaponOrigin = weaponRoot.localPosition;
        _cc           = GetComponent<CharacterController>();
        _prevBobPos   = transform.position;;

        // Add aura to any gun already sitting on the floor
        foreach (var ws in FindObjectsByType<WeaponStats>(FindObjectsSortMode.None))
            if (!ws.transform.IsChildOf(weaponRoot))
                if (ws.GetComponent<GunAura>() == null)
                    ws.gameObject.AddComponent<GunAura>();

        foreach (Transform t in weaponRoot)
            if (t.GetComponent<WeaponStats>() != null) { _held = t.gameObject; break; }

        if (_held != null) { EnsurePillCollider(_held); SetLayerRecursive(_held, WeaponLayer); }
        if (promptText != null) promptText.gameObject.SetActive(false);
        if (_shooter != null) _shooter.enabled = _held != null;
        CreateBounceMat();
    }

    // Dedicated weapon camera renders only WeaponLayer to a RenderTexture shown as a
    // transparent UI overlay. Camera never rotates - gun is locked to a fixed screen position.
    void SetupWeaponUI()
    {
        cam.cullingMask &= ~(1 << WeaponLayer);

        _weaponRT = new RenderTexture(Screen.width, Screen.height, 24, RenderTextureFormat.ARGB32);
        _weaponRT.Create();

        var camGO = new GameObject("WeaponCamera");

        _weaponCam                 = camGO.AddComponent<Camera>();
        _weaponCam.clearFlags      = CameraClearFlags.SolidColor;
        _weaponCam.backgroundColor = Color.clear;
        _weaponCam.cullingMask     = 1 << WeaponLayer;
        _weaponCam.fieldOfView     = weaponFOV;
        _weaponCam.nearClipPlane   = 0.01f;
        _weaponCam.farClipPlane    = 100f;
        _weaponCam.depth           = cam.depth - 1;
        _weaponCam.targetTexture   = _weaponRT;

        var wData = camGO.GetComponent<UniversalAdditionalCameraData>();
        if (wData != null) wData.renderPostProcessing = false;

        // Detach WeaponRoot from main camera and pin it to the weapon camera.
        // Fixed local position = fixed screen position no matter where the player looks.
        weaponRoot.SetParent(camGO.transform, false);
        weaponRoot.localPosition = new Vector3(0.26f, -0.16f, 0.55f);
        weaponRoot.localRotation = Quaternion.identity;
        weaponRoot.localScale    = Vector3.one;

        var hud = GameObject.Find("HUD");
        if (hud != null)
        {
            var go = new GameObject("WeaponOverlay");
            go.transform.SetParent(hud.transform, false);
            go.transform.SetAsFirstSibling();

            var img = go.AddComponent<RawImage>();
            img.texture = _weaponRT;
            img.color   = Color.white;

            var rect = img.rectTransform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }
    }

void Update()
    {
        if (Mouse.current == null) return;

        if (_weaponCam != null)
            _weaponCam.transform.position = cam.transform.position;

        ApplyWeaponBob();

        bool rightClick    = Mouse.current.rightButton.wasPressedThisFrame;
        GameObject target  = FindLookTarget();
        bool canPickup     = target != null && _held == null;
        bool canSwap       = target != null && _held != null;

        if (promptText != null)
        {
            bool showEmpty = _held != null && IsEmpty && target == null;
            promptText.gameObject.SetActive(canPickup || canSwap || showEmpty);
            if (canPickup)  promptText.text = "[Right Click] Pick Up";
            if (canSwap)    promptText.text = "[Right Click] Swap";
            if (showEmpty)  promptText.text = "Empty — [Right Click] throw";
        }

        if (rightClick)
        {
            if (canPickup)
                PickUp(target);
            else if (canSwap)
            {
                var swapTarget = target;
                Drop();
                if (swapTarget != null) PickUp(swapTarget);
            }
            else if (_held != null)
                Drop();
        }
    }

void ApplyWeaponBob()
    {
        Vector3 pos   = transform.position;
        float   speed = new Vector3(pos.x - _prevBobPos.x, 0f, pos.z - _prevBobPos.z).magnitude
                        / Time.deltaTime;
        _prevBobPos = pos;

        bool grounded = Physics.Raycast(transform.position + Vector3.up * 0.1f, Vector3.down, 1.5f);
        bool moving   = grounded && speed > 0.5f;

        _bobAmp = Mathf.Lerp(_bobAmp, moving ? 1f : 0f, Time.deltaTime * 10f);
        if (_bobAmp > 0.001f) _bobT += Time.deltaTime * bobFrequency;

        weaponRoot.localPosition = _weaponOrigin + new Vector3(
            Mathf.Sin(_bobT * 0.5f) * bobAmplitudeX * _bobAmp,
            Mathf.Sin(_bobT)        * bobAmplitudeY * _bobAmp,
            0f
        );
    }


    GameObject FindLookTarget()
    {
        RaycastHit hit;
        if (!Physics.SphereCast(cam.transform.position, sphereRadius,
                                 cam.transform.forward, out hit, pickupRange))
            return null;

        Transform t = hit.transform;
        WeaponStats stats = null;
        while (t != null && stats == null) { stats = t.GetComponent<WeaponStats>(); t = t.parent; }
        if (stats == null) return null;

        var root = stats.gameObject;
        if (root.transform.IsChildOf(weaponRoot)) return null;
        return root;
    }

void PickUp(GameObject gun)
    {
        if (_held != null) Drop();

        // Remove floor aura before parenting
        var aura = gun.GetComponent<GunAura>();
        if (aura != null) UnityEngine.Object.Destroy(aura);

        var rb = gun.GetComponent<Rigidbody>();
        if (rb != null) UnityEngine.Object.Destroy(rb);
        foreach (var c in gun.GetComponents<Collider>()) UnityEngine.Object.Destroy(c);

        gun.transform.SetParent(weaponRoot);
        gun.transform.localScale = Vector3.one;

        var stats = gun.GetComponent<WeaponStats>();
        gun.transform.localPosition = stats != null ? stats.heldPosition : Vector3.zero;
        gun.transform.localRotation = Quaternion.Euler(
            stats != null ? stats.heldRotation : Vector3.zero);

        SetLayerRecursive(gun, WeaponLayer);

        _held = gun;
        if (promptText != null) promptText.gameObject.SetActive(false);
        if (_shooter   != null) _shooter.enabled = true;
    }

void Drop()
    {
        if (_held == null) return;
        if (_shooter != null) _shooter.enabled = false;

        SetLayerRecursive(_held, 0);
        _held.transform.SetParent(null);
        _held.transform.localScale = Vector3.one;
        EnsurePillCollider(_held);

        if (_held.GetComponent<GunAura>() == null)
            _held.AddComponent<GunAura>();

        var stats  = _held.GetComponent<WeaponStats>();
        float mass  = stats != null ? stats.weight : 1f;
        float force = baseThrowForce * 2.5f / Mathf.Sqrt(mass);

        // Inherit player XZ movement so the gun carries momentum,
        // but always throw forward — backward movement just reduces forward speed, never reverses it.
        var cc = GetComponent<CharacterController>();
        Vector3 playerFlat = cc != null ? new Vector3(cc.velocity.x, 0f, cc.velocity.z) : Vector3.zero;

        bool isEmpty = stats != null && stats.currentAmmo <= 0;

        var rb = _held.AddComponent<Rigidbody>();
        rb.mass           = mass;
        rb.linearDamping  = 0.1f;
        rb.angularDamping = 0.5f;
        rb.linearVelocity  = cam.transform.forward * force + playerFlat;
        rb.angularVelocity = Random.insideUnitSphere * 3f;

        if (isEmpty)
            _held.AddComponent<BreakOnImpact>();
        else
        {
            _held.AddComponent<ThrownWeaponDamage>();
            _held.AddComponent<GunAura>();
        }

        _held = null;
    }

    void SetLayerRecursive(GameObject go, int layer)
    {
        go.layer = layer;
        foreach (Transform child in go.transform)
            SetLayerRecursive(child.gameObject, layer);
    }

    void EnsurePillCollider(GameObject go)
    {
        foreach (var c in go.GetComponents<Collider>()) UnityEngine.Object.DestroyImmediate(c);
        var cap = go.AddComponent<CapsuleCollider>();
        cap.direction = 2; cap.radius = 0.15f; cap.height = 0.6f;
        if (_deadBounce != null) cap.material = _deadBounce;
    }

    static void CreateBounceMat()
    {
        if (_deadBounce != null) return;
        _deadBounce = new PhysicsMaterial("GunBounce")
        {
            bounciness      = 0.25f,
            dynamicFriction = 1.5f,
            staticFriction  = 1.5f,
            bounceCombine   = PhysicsMaterialCombine.Average,
            frictionCombine = PhysicsMaterialCombine.Maximum
        };
    }
}
