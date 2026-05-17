using UnityEngine;

public class HealthHUD : MonoBehaviour
{
    float _elapsed;
    float _damageVignette;
    float _whiteFlash;

    const float BAR_X = 12f;
    const float BAR_Y = 14f;
    const float BAR_W = 220f;
    const float BAR_H = 22f;

    static readonly Color BarColor = new Color(0.72f, 0.92f, 1f, 1f);
    static readonly Color BarBg    = new Color(0.1f, 0.1f, 0.1f, 0.55f);

    GUIStyle _timerStyle;
    GUIStyle _hpTextStyle;

    void OnEnable()
    {
        PlayerHealth.OnDamaged      += OnDamaged;
        FlashGrenade.OnFlashExploded += OnFlashExploded;
    }
    void OnDisable()
    {
        PlayerHealth.OnDamaged      -= OnDamaged;
        FlashGrenade.OnFlashExploded -= OnFlashExploded;
    }
    void OnDamaged()      => _damageVignette = 1f;
    void OnFlashExploded() => _whiteFlash    = 1f;

    void Update()
    {
        if (MainMenu.IsOpen) return;

        _elapsed        += Time.deltaTime;
        _damageVignette  = Mathf.Max(0f, _damageVignette - Time.deltaTime * 1.8f);
        _whiteFlash      = Mathf.Max(0f, _whiteFlash     - Time.deltaTime * 0.32f);
    }

    void OnGUI()
    {
        if (MainMenu.IsOpen) return;

        DrawHPBar();
        DrawFlashCount();
        DrawTimer();
        DrawDamageVignette();
        DrawWhiteFlash();
    }

    void DrawHPBar()
    {
        float fraction  = PlayerHealth.Instance != null ? PlayerHealth.Instance.HpFraction : 1f;
        int   currentHp = PlayerHealth.Instance != null ? PlayerHealth.Instance.currentHp  : 0;
        int   maxHp     = PlayerHealth.Instance != null ? PlayerHealth.Instance.maxHp      : 200;

        // Dark grey tray
        GUI.color = BarBg;
        GUI.DrawTexture(new Rect(BAR_X, BAR_Y, BAR_W, BAR_H), Texture2D.whiteTexture);

        // Light-blue fill
        GUI.color = BarColor;
        GUI.DrawTexture(new Rect(BAR_X, BAR_Y, BAR_W * fraction, BAR_H), Texture2D.whiteTexture);

        GUI.color = Color.white;

        if (_hpTextStyle == null)
        {
            _hpTextStyle = new GUIStyle(GUI.skin.label)
            {
                font      = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"),
                fontSize  = 13,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
            };
            _hpTextStyle.normal.textColor = Color.white;
        }
        GUI.Label(new Rect(BAR_X, BAR_Y, BAR_W, BAR_H), $"{currentHp} / {maxHp}", _hpTextStyle);
    }

    void DrawTimer()
    {
        int minutes = (int)(_elapsed / 60f);
        int seconds = (int)(_elapsed % 60f);

        if (_timerStyle == null)
        {
            _timerStyle = new GUIStyle(GUI.skin.label)
            {
                font      = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"),
                fontSize  = 26,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.UpperRight,
            };
            _timerStyle.normal.textColor = Color.white;
        }

        GUI.Label(new Rect(Screen.width - 162f, 12f, 150f, 40f),
                  $"{minutes:D2}:{seconds:D2}", _timerStyle);
    }

    GUIStyle _flashStyle;
    void DrawFlashCount()
    {
        int count = FlashManager.Instance != null ? FlashManager.Instance.flashCount : 0;
        if (_flashStyle == null)
        {
            _flashStyle = new GUIStyle(GUI.skin.label)
            {
                font      = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"),
                fontSize  = 14,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft,
            };
            _flashStyle.normal.textColor = new Color(1f, 0.95f, 0.35f);
        }
        GUI.Label(new Rect(BAR_X, BAR_Y + BAR_H + 4f, 120f, 18f),
                  $"FLASH  x{count}", _flashStyle);
    }

    void DrawDamageVignette()
    {
        if (_damageVignette <= 0f) return;
        float a = _damageVignette * 0.55f;
        int   e = 80; // edge thickness
        // Draw 4 red edge strips
        GUI.color = new Color(1f, 0f, 0f, a);
        GUI.DrawTexture(new Rect(0, 0, Screen.width, e), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(0, Screen.height - e, Screen.width, e), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(0, 0, e, Screen.height), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(Screen.width - e, 0, e, Screen.height), Texture2D.whiteTexture);
        GUI.color = Color.white;
    }

    void DrawWhiteFlash()
    {
        if (_whiteFlash <= 0f) return;
        GUI.color = new Color(1f, 1f, 1f, _whiteFlash);
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
        GUI.color = Color.white;
    }
}
