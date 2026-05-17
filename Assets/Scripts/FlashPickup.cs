using UnityEngine;
using UnityEngine.InputSystem;

public class FlashPickup : MonoBehaviour
{
    public float grabRange  = 4f;
    public float tubeRadius = 0.8f;

    Camera _cam;
    bool   _collected;
    bool   _showPrompt;
    Vector3 _startPos;
    GUIStyle _style;

    void Start()
    {
        _cam      = Camera.main;
        _startPos = transform.position;

        if (GetComponent<Collider>() == null)
        {
            var cap       = gameObject.AddComponent<CapsuleCollider>();
            cap.radius    = 0.25f;
            cap.height    = 0.6f;
            cap.direction = 1;
        }

        if (GetComponent<GunAura>() == null)
        {
            var aura           = gameObject.AddComponent<GunAura>();
            aura.auraColor     = new Color(1f, 0.95f, 0.4f);
            aura.baseIntensity = 2.5f;
            aura.range         = 3f;
            aura.pulseSpeed    = 3f;
        }
    }

    void Update()
    {
        if (_collected || _cam == null) return;

        Vector3 camPos     = _cam.transform.position;
        Vector3 camFwd     = _cam.transform.forward;
        Vector3 toSelf     = transform.position - camPos;
        float   along      = Vector3.Dot(toSelf, camFwd);
        float   perpDist   = Vector3.Cross(toSelf, camFwd).magnitude;

        _showPrompt = along >= 0f && along <= grabRange && perpDist <= tubeRadius;

        if (_showPrompt && Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
            Collect();
    }

    void Collect()
    {
        _collected = true;
        FlashManager.Instance?.AddFlash();
        gameObject.SetActive(false);
    }

    public void ResetPickup()
    {
        _collected = false;
        transform.position = _startPos;
        gameObject.SetActive(true);
    }

    void OnGUI()
    {
        if (!_showPrompt || _collected) return;
        if (_style == null)
        {
            _style = new GUIStyle(GUI.skin.label)
            {
                font      = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"),
                fontSize  = 11,
                fontStyle = FontStyle.Normal,
                alignment = TextAnchor.MiddleCenter
            };
            _style.normal.textColor = Color.white;
        }
        GUI.Label(new Rect((Screen.width - 220f) / 2f, Screen.height / 2f + 28f, 220f, 20f),
                  "[R] Pick up flash", _style);
    }
}
