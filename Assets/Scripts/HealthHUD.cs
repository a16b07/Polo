using UnityEngine;

public class HealthHUD : MonoBehaviour
{
    float _elapsed;

    const float BAR_X = 12f;
    const float BAR_Y = 14f;
    const float BAR_W = 220f;
    const float BAR_H = 22f;

    static readonly Color BarColor = new Color(0.72f, 0.92f, 1f, 1f);
    static readonly Color BarBg    = new Color(0.1f, 0.1f, 0.1f, 0.55f);

    GUIStyle _timerStyle;
    GUIStyle _hpTextStyle;

    void Update() => _elapsed += Time.deltaTime;

    void OnGUI()
    {
        DrawHPBar();
        DrawTimer();
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
}
