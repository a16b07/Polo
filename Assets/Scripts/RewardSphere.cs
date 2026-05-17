using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CapsuleCollider))]
[RequireComponent(typeof(Rigidbody))]
public class RewardSphere : MonoBehaviour
{
    public float grabRange   = 5f;
    public float tubeRadius  = 1f;

    Camera  _cam;
    Vector3 _startPos;
    bool    _grabbed;
    bool    _showPrompt;

    void Awake()
    {
        // Remove any leftover SphereCollider from before
        var old = GetComponent<SphereCollider>();
        if (old != null) Destroy(old);

        var rb           = GetComponent<Rigidbody>();
        rb.mass          = 0.5f;
        rb.linearDamping  = 1.5f;
        rb.angularDamping = 1.5f;

        // CapsuleCollider matches the cylinder mesh (Y axis, full height)
        var cap       = GetComponent<CapsuleCollider>() ?? gameObject.AddComponent<CapsuleCollider>();
        cap.direction = 1;
        cap.radius    = 0.5f;
        cap.height    = 2f;
        cap.isTrigger = false;
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

        var mr = GetComponent<MeshRenderer>();
        if (mr != null) ApplyTexture(mr);
    }

    void ApplyTexture(MeshRenderer mr)
    {
        var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        mat.color = Color.white;

#if UNITY_EDITOR
        var tex = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/text/fourloko.png");
#else
        Texture2D tex = null;
#endif
        if (tex != null)
        {
            mat.SetTexture("_BaseMap", tex);
            mat.EnableKeyword("_EMISSION");
            mat.SetTexture("_EmissionMap", tex);
            mat.SetColor("_EmissionColor", Color.white * 0.4f);

            // Crop UV to non-transparent region so transparent margins don't show
            CropToOpaque(mat, tex);
        }
        else
        {
            mat.color = new Color(0.1f, 0.9f, 0.2f);
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", new Color(0f, 2f, 0.3f));
        }

        mr.material = mat;
    }

    static void CropToOpaque(Material mat, Texture2D tex)
    {
        Color[] pixels = tex.GetPixels();
        int w = tex.width, h = tex.height;
        int minX = w, minY = h, maxX = 0, maxY = 0;

        for (int y = 0; y < h; y++)
        for (int x = 0; x < w; x++)
        {
            if (pixels[y * w + x].a > 0.05f)
            {
                if (x < minX) minX = x;
                if (y < minY) minY = y;
                if (x > maxX) maxX = x;
                if (y > maxY) maxY = y;
            }
        }

        if (maxX <= minX || maxY <= minY) return; // fully transparent — leave as-is

        float offX  = (float)minX / w;
        float offY  = (float)minY / h;
        float scaleX = (float)(maxX - minX + 1) / w;
        float scaleY = (float)(maxY - minY + 1) / h;

        mat.mainTextureOffset = new Vector2(offX, offY);
        mat.mainTextureScale  = new Vector2(scaleX, scaleY);
    }

    void Update()
    {
        if (_grabbed) return;
        if (_cam == null) { _cam = Camera.main; return; }

        // Ray-distance check — independent of physics collider size
        _showPrompt = false;
        Vector3 camPos     = _cam.transform.position;
        Vector3 camForward = _cam.transform.forward;
        Vector3 toSphere   = transform.position - camPos;
        float   along      = Vector3.Dot(toSphere, camForward);
        float   perpDist   = Vector3.Cross(toSphere, camForward).magnitude;

        if (along >= 0f && along <= grabRange && perpDist <= tubeRadius)
        {
            _showPrompt = true;
            if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
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
        if (PlayerHealth.Instance != null)
            PlayerHealth.Instance.Heal(Mathf.RoundToInt(PlayerHealth.Instance.maxHp * 0.15f));
        gameObject.SetActive(false);
    }

    public void ResetSphere()
    {
        _grabbed    = false;
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
                  "[R] claim reward", style);
    }
}
