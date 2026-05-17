using UnityEngine;

public class HitMarker : MonoBehaviour
{
    public float duration  = 0.25f;
    public float lineSize  = 11f;
    public float gap       = 6f;
    public float thickness = 2.5f;

    public static HitMarker Instance { get; private set; }

    float     _hideTime;
    bool      _headshot;
    Texture2D _tex;

    void Awake()
    {
        Instance = this;
        _tex = new Texture2D(1, 1);
        _tex.SetPixel(0, 0, Color.white);
        _tex.Apply();
    }

    public void Show(bool headshot = false)
    {
        _hideTime = Time.unscaledTime + duration;
        _headshot = headshot;
    }

    void OnGUI()
    {
        if (Time.unscaledTime > _hideTime) return;

        float cx = Screen.width  / 2f;
        float cy = Screen.height / 2f;
        float g  = gap;
        float s  = lineSize;
        float t  = thickness;

        GUI.color = _headshot ? new Color(1f, 0.85f, 0.1f, 0.95f) : new Color(1f, 1f, 1f, 0.9f);

        // Right
        GUI.DrawTexture(new Rect(cx + g,     cy - t / 2f, s, t), _tex);
        // Left
        GUI.DrawTexture(new Rect(cx - g - s, cy - t / 2f, s, t), _tex);
        // Down
        GUI.DrawTexture(new Rect(cx - t / 2f, cy + g,     t, s), _tex);
        // Up
        GUI.DrawTexture(new Rect(cx - t / 2f, cy - g - s, t, s), _tex);

        GUI.color = Color.white;
    }
}
