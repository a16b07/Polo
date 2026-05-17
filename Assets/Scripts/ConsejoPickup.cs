using UnityEngine;
using UnityEngine.InputSystem;

public class ConsejoPickup : MonoBehaviour
{
    const float GRAB_RANGE   = 3.5f;
    const float TUBE_RADIUS  = 0.9f;
    const float SHOW_SECS    = 3f;
    const float FADE_SECS    = 1f;

    public static int  CollectedCount { get; private set; }
    public static bool AllCollected  => CollectedCount >= 3;

    // Static so all instances share one display slot
    static Texture2D _dispTex;
    static float     _showUntil;
    static float     _fadeUntil;

    Texture2D _myTex;
    Camera    _cam;
    bool      _collected;
    bool      _showPrompt;
    GUIStyle  _promptStyle;

    void Start()
    {
        _cam = Camera.main;

        var mr = GetComponentInChildren<MeshRenderer>();
        if (mr != null && mr.sharedMaterial != null)
            _myTex = mr.sharedMaterial.GetTexture("_BaseMap") as Texture2D;
    }

    void Update()
    {
        if (_collected || _cam == null) return;

        Vector3 toSelf   = transform.position - _cam.transform.position;
        float   along    = Vector3.Dot(toSelf, _cam.transform.forward);
        float   perpDist = Vector3.Cross(toSelf, _cam.transform.forward).magnitude;

        _showPrompt = along >= 0f && along <= GRAB_RANGE && perpDist <= TUBE_RADIUS;

        if (_showPrompt && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
            Collect();
    }

    void Collect()
    {
        _collected  = true;
        _showPrompt = false;

        // Hide mesh — keep script alive so OnGUI can draw the display
        foreach (var mr in GetComponentsInChildren<MeshRenderer>())
            mr.enabled = false;
        foreach (var col in GetComponentsInChildren<Collider>())
            col.enabled = false;

        CollectedCount++;

        if (_myTex != null)
        {
            _dispTex   = _myTex;
            _showUntil = Time.time + SHOW_SECS;
            _fadeUntil = _showUntil + FADE_SECS;
        }
    }

    void OnGUI()
    {
        DrawPrompt();
        DrawDisplay();
    }

    void DrawPrompt()
    {
        if (!_showPrompt || _collected) return;
        if (_promptStyle == null)
        {
            _promptStyle = new GUIStyle(GUI.skin.label)
            {
                font      = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"),
                fontSize  = 13,
                fontStyle = FontStyle.Normal,
                alignment = TextAnchor.MiddleCenter,
            };
            _promptStyle.normal.textColor = Color.white;
        }
        GUI.Label(new Rect((Screen.width - 220f) / 2f, Screen.height / 2f + 28f, 220f, 20f),
                  "[E] pick up", _promptStyle);
    }

    static void DrawDisplay()
    {
        if (_dispTex == null) return;
        float now = Time.time;
        if (now >= _fadeUntil) return;

        float alpha = now <= _showUntil
            ? 1f
            : 1f - (now - _showUntil) / FADE_SECS;

        float h  = Screen.height / 5f;
        float w  = h * ((float)_dispTex.width / _dispTex.height);
        float px = 16f;
        float py = Screen.height / 5f;   // top of image = 1/5 from top of screen

        GUI.color = new Color(1f, 1f, 1f, alpha);
        GUI.DrawTexture(new Rect(px, py, w, h), _dispTex, ScaleMode.ScaleToFit, true);
        GUI.color = Color.white;
    }
}
