using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RawImage))]
public class Crosshair : MonoBehaviour
{
    public Color color = Color.white;

    RawImage _img;
    Shooter  _shooter;
    Texture2D _blank;
    Texture2D _circleTex;
    int       _cachedRadius = -1;


    void Start()
    {
        _img    = GetComponent<RawImage>();
        _img.enabled = false; // we draw in OnGUI
        _shooter = FindFirstObjectByType<Shooter>();

        _blank = new Texture2D(1, 1);
        _blank.SetPixel(0, 0, Color.white);
        _blank.Apply();
    }

void OnDestroy()
    {
        if (_circleTex != null) Destroy(_circleTex);
    }


    bool IsShotgun()
    {
        if (_shooter == null || _shooter.weaponRoot == null) return false;
        var s = _shooter.weaponRoot.GetComponentInChildren<WeaponStats>();
        return s != null && s.pelletCount > 1;
    }

    void OnGUI()
    {
        float heat   = _shooter != null ? _shooter.Heat : 0f;
        float cx     = Screen.width  / 2f;
        float cy     = Screen.height / 2f;

        GUI.color = new Color(color.r, color.g, color.b, 0.85f);

        if (IsShotgun())
            DrawCircle(cx, cy, heat);
        else
            DrawCross(cx, cy, heat);

        GUI.color = Color.white;
    }

    void DrawCross(float cx, float cy, float heat)
    {
        float gap     = Mathf.Lerp(4f, 22f, heat);
        float lineLen = 10f;
        float thick   = 2f;

        // Right
        Rect(cx + gap,             cy - thick / 2f, lineLen, thick);
        // Left
        Rect(cx - gap - lineLen,   cy - thick / 2f, lineLen, thick);
        // Up
        Rect(cx - thick / 2f,      cy - gap - lineLen, thick, lineLen);
        // Down
        Rect(cx - thick / 2f,      cy + gap,           thick, lineLen);

        // Center dot
        Rect(cx - 1f, cy - 1f, 2f, 2f);
    }

void DrawCircle(float cx, float cy, float heat)
    {
        int radius = Mathf.RoundToInt(Mathf.Lerp(10f, 36f, heat));
        int thick  = 2;

        if (radius != _cachedRadius)
        {
            if (_circleTex != null) Destroy(_circleTex);
            int size = (radius + thick + 2) * 2;
            _circleTex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            var px = new Color32[size * size];
            int c  = size / 2;
            for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float d     = Mathf.Sqrt((x - c) * (x - c) + (y - c) * (y - c));
                float alpha = 1f - Mathf.Clamp01(Mathf.Abs(d - radius) - thick + 1f);
                px[y * size + x] = new Color32(255, 255, 255, (byte)(alpha * 220));
            }
            _circleTex.SetPixels32(px);
            _circleTex.Apply();
            _cachedRadius = radius;
        }

        int half = _circleTex.width / 2;
        GUI.DrawTexture(new UnityEngine.Rect(cx - half, cy - half, _circleTex.width, _circleTex.height), _circleTex);
    }

    void Rect(float x, float y, float w, float h) =>
        GUI.DrawTexture(new UnityEngine.Rect(x, y, w, h), _blank);
}
