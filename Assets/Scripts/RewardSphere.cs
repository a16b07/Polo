using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(SphereCollider))]
[RequireComponent(typeof(Rigidbody))]
public class RewardSphere : MonoBehaviour
{
    public float grabRange = 3.5f;

    Camera _cam;
    Vector3 _startPos;
    bool _grabbed;
    bool _showPrompt;

    void Awake()
    {
        var rb          = GetComponent<Rigidbody>();
        rb.mass         = 0.5f;
        rb.linearDamping  = 1.5f;
        rb.angularDamping = 1.5f;
        GetComponent<SphereCollider>().isTrigger = false;
    }

    void Start()
    {
        _cam      = Camera.main;
        _startPos = transform.position;

        var aura           = gameObject.AddComponent<GunAura>();
        aura.auraColor     = new Color(0.1f, 1f, 0.2f);
        aura.baseIntensity = 3f;
        aura.range         = 4f;
        aura.pulseSpeed    = 3f;

        var mr  = GetComponent<MeshRenderer>();
        if (mr != null)
        {
            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = new Color(0.1f, 0.9f, 0.2f);
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", new Color(0f, 2f, 0.3f));
            mr.material = mat;
        }
    }

    void Update()
    {
        if (_grabbed) return;
        if (_cam == null) { _cam = Camera.main; return; }

        // SphereCast from camera — same style as GunPickup
        _showPrompt = false;
        if (Physics.SphereCast(_cam.transform.position, 0.3f, _cam.transform.forward,
                               out RaycastHit hit, grabRange)
            && hit.collider.gameObject == gameObject)
        {
            _showPrompt = true;
            if (Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame)
                Grab();
        }
    }

    void Grab()
    {
        if (_grabbed) return;
        var pm = PerkManager.Instance ?? FindFirstObjectByType<PerkManager>();
        if (pm == null) return;

        _grabbed = true;
        var perk = pm.RollNew();
        pm.ShowPerk(perk, null);
        gameObject.SetActive(false);
    }

    public void ResetSphere()
    {
        _grabbed = false;
        _showPrompt = false;
        var rb = GetComponent<Rigidbody>();
        if (rb != null) { rb.linearVelocity = Vector3.zero; rb.angularVelocity = Vector3.zero; }
        transform.position = _startPos;
        gameObject.SetActive(true);
    }

    void OnGUI()
    {
        if (!_showPrompt || _grabbed) return;
        var style = new GUIStyle(GUI.skin.label)
        {
            font      = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"),
            fontSize  = 14,
            fontStyle = FontStyle.Normal,
            alignment = TextAnchor.MiddleCenter
        };
        style.normal.textColor = new Color(0.3f, 1f, 0.4f, 0.7f);
        float w = 220, h = 22;
        GUI.Label(new Rect((Screen.width - w) / 2f, Screen.height / 2f + 28, w, h),
                  "[Right Click] claim reward", style);
    }
}
