using UnityEngine;

public class AmmoHUD : MonoBehaviour
{
    [Tooltip("Assign bala.png here once added to the project.")]
    public Sprite bulletSprite;

    GunPickup _pickup;
    Texture2D _tex;

    void Start()
    {
        _pickup = FindFirstObjectByType<GunPickup>();
        BuildTexture();
    }

    void BuildTexture()
    {
        if (bulletSprite != null)
        {
            _tex = bulletSprite.texture;
            return;
        }
        // Procedural bullet shape (small yellow rectangle with rounded tip)
        int w = 5, h = 13;
        _tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        var px = new Color32[w * h];
        for (int y = 0; y < h; y++)
        for (int x = 0; x < w; x++)
        {
            bool inBody = y < h - 3;
            bool inTip  = y >= h - 3 && Mathf.Abs(x - w / 2) <= (h - 1 - y);
            px[y * w + x] = (inBody || inTip)
                ? new Color32(255, 210, 30, 255)
                : new Color32(0, 0, 0, 0);
        }
        _tex.SetPixels32(px);
        _tex.Apply();
        _tex.filterMode = FilterMode.Point;
    }

    void OnGUI()
    {
        if (_pickup == null || _tex == null) return;
        int ammo = _pickup.CurrentAmmo;
        if (ammo <= 0) return;

        // Scale sprites to fit in max 280px wide
        float sprW = 10f, sprH = 22f, gap = 3f;
        float totalW = ammo * (sprW + gap) - gap;
        float maxW   = 280f;
        if (totalW > maxW)
        {
            float s = maxW / totalW;
            sprW *= s; sprH *= s; gap *= s;
        }

        float startX = 14f;
        float startY = Screen.height - sprH - 14f;

        for (int i = 0; i < ammo; i++)
            GUI.DrawTexture(new Rect(startX + i * (sprW + gap), startY, sprW, sprH), _tex);
    }
}
