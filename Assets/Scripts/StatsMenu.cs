using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class StatsMenu : MonoBehaviour
{
    PlayerStats  _stats;
    PerkManager  _perks;
    GUIStyle     _titleStyle, _labelStyle, _valueStyle, _deltaGreen, _deltaRed;
    GUIStyle     _squareStyle, _tooltipStyle, _panelStyle;

    // Base values (what stats start at)
    const float BASE_SPEED  = 1f;
    const float BASE_DAMAGE = 1f;
    const float BASE_DR     = 0f;
    const float BASE_REGEN  = 0f;
    const float BASE_LUCK   = 0f;

    void Start()
    {
        _stats = GetComponent<PlayerStats>();
        _perks = GetComponent<PerkManager>();
    }

    void OnGUI()
    {
        if (Keyboard.current == null || !Keyboard.current.tabKey.isPressed) return;
        if (_stats == null) return;
        if (_titleStyle == null) BuildStyles();

        float sw = Screen.width, sh = Screen.height;
        float pw = 400f, ph = 400f;
        float px = (sw - pw) / 2f, py = (sh - ph) / 2f;

        GUI.Box(new Rect(px - 12, py - 12, pw + 24, ph + 24), GUIContent.none, _panelStyle);
        GUI.Label(new Rect(px, py, pw, 36), "CHARACTER STATS", _titleStyle);

        float ry = py + 44, rh = 36f;
        StatRow(px, ry,       pw, rh, "Damage Mult",     $"x{_stats.damageMultiplier:F2}",  _stats.damageMultiplier,  BASE_DAMAGE, true);
        StatRow(px, ry+rh,    pw, rh, "Speed",           $"{_stats.speedMultiplier*100f:F0}%", _stats.speedMultiplier, BASE_SPEED, true);
        StatRow(px, ry+rh*2,  pw, rh, "Damage Reduction",$"{_stats.damageReduction*100f:F0}%", _stats.damageReduction, BASE_DR, true);
        StatRow(px, ry+rh*3,  pw, rh, "Regeneration",   $"{_stats.regeneration:F1} hp/s",   _stats.regeneration,     BASE_REGEN, true);
        StatRow(px, ry+rh*4,  pw, rh, "Luck",              $"{_stats.luck:F2}",                   _stats.luck,                    BASE_LUCK,  true);
        StatRow(px, ry+rh*5,  pw, rh, "Proj. Speed",       $"x{_stats.projectileSpeedMult:F2}",   _stats.projectileSpeedMult,     1f,         true);
        StatRow(px, ry+rh*6,  pw, rh, "Proj. Size",        $"x{_stats.projectileSizeMult:F2}",    _stats.projectileSizeMult,      1f,         true);

        // Passive perk squares
        if (_perks != null)
        {
            var passives = new List<PerkDefinition>();
            foreach (var p in _perks.AcquiredPerks)
                if (p.type == PerkType.Perk) passives.Add(p);

            if (passives.Count > 0)
            {
                float sqY  = py + ph - 60f;
                float sqSz = 30f, gap = 4f;
                GUI.Label(new Rect(px, sqY - 22, pw, 20), "PASSIVES", _labelStyle);

                string tooltip = null;
                Rect   tooltipAnchor = default;

                for (int i = 0; i < passives.Count; i++)
                {
                    var p    = passives[i];
                    var rect = new Rect(px + i * (sqSz + gap), sqY, sqSz, sqSz);

                    Color rc = PerkDefinition.RarityColor(p.rarity);
                    GUI.color = rc;
                    GUI.Box(rect, GUIContent.none, _squareStyle);
                    GUI.color = Color.white;

                    // First letter of perk name
                    _labelStyle.normal.textColor = Color.black;
                    GUI.Label(new Rect(rect.x + 6, rect.y + 4, sqSz, sqSz), p.name[0].ToString(), _labelStyle);
                    _labelStyle.normal.textColor = new Color(0.75f, 0.75f, 0.75f);

                    if (rect.Contains(Event.current.mousePosition))
                    {
                        tooltip       = $"[{p.rarity}] {p.name}\n{p.description}";
                        tooltipAnchor = rect;
                    }
                }

                // Draw tooltip last so it's on top
                if (tooltip != null)
                {
                    float tw = 220f, th = 52f;
                    float tx = Mathf.Min(tooltipAnchor.x, sw - tw - 8);
                    float ty = tooltipAnchor.y - th - 4;
                    GUI.Box(new Rect(tx, ty, tw, th), GUIContent.none, _panelStyle);
                    GUI.Label(new Rect(tx + 6, ty + 4, tw - 12, th - 8), tooltip, _tooltipStyle);
                }
            }
        }
    }

    void StatRow(float x, float y, float w, float h, string label, string val, float current, float baseVal, bool higherIsBetter)
    {
        float diff  = current - baseVal;
        bool better = higherIsBetter ? diff > 0.001f : diff < -0.001f;
        bool worse  = higherIsBetter ? diff < -0.001f : diff > 0.001f;

        GUI.Label(new Rect(x, y, w * 0.45f, h), label, _labelStyle);
        GUI.Label(new Rect(x + w * 0.45f, y, w * 0.28f, h), val, _valueStyle);

        if (Mathf.Abs(diff) > 0.001f)
        {
            var style = better ? _deltaGreen : _deltaRed;
            string arrow = better ? "▲" : "▼";
            string dStr  = FormatDelta(diff, current, baseVal);
            GUI.Label(new Rect(x + w * 0.73f, y, w * 0.27f, h), arrow + " " + dStr, style);
        }
    }

    string FormatDelta(float diff, float current, float baseVal)
    {
        if (baseVal == 0f) return diff >= 0 ? $"+{diff:F2}" : $"{diff:F2}";
        float pct = (diff / Mathf.Abs(baseVal)) * 100f;
        return pct >= 0 ? $"+{pct:F0}%" : $"{pct:F0}%";
    }

    void BuildStyles()
    {
        var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        _panelStyle = new GUIStyle(GUI.skin.box);
        _panelStyle.normal.background = MakeTex(2, 2, new Color(0.04f, 0.04f, 0.08f, 0.92f));

        _titleStyle = Style(font, 22, FontStyle.Bold, TextAnchor.MiddleLeft, Color.white);
        _labelStyle = Style(font, 16, FontStyle.Normal, TextAnchor.MiddleLeft, new Color(0.75f, 0.75f, 0.75f));
        _valueStyle = Style(font, 16, FontStyle.Bold, TextAnchor.MiddleLeft, Color.white);
        _deltaGreen = Style(font, 15, FontStyle.Bold, TextAnchor.MiddleLeft, new Color(0.2f, 0.95f, 0.35f));
        _deltaRed   = Style(font, 15, FontStyle.Bold, TextAnchor.MiddleLeft, new Color(0.95f, 0.25f, 0.25f));

        _squareStyle = new GUIStyle(GUI.skin.box);
        _squareStyle.normal.background = MakeTex(2, 2, Color.white);

        _tooltipStyle = Style(font, 13, FontStyle.Normal, TextAnchor.UpperLeft, new Color(0.9f, 0.9f, 0.9f));
        _tooltipStyle.wordWrap = true;
    }

    static GUIStyle Style(Font font, int size, FontStyle fs, TextAnchor anchor, Color col)
    {
        var s = new GUIStyle { font = font, fontSize = size, fontStyle = fs, alignment = anchor };
        s.normal.textColor = col;
        return s;
    }

    static Texture2D MakeTex(int w, int h, Color col)
    {
        var t = new Texture2D(w, h);
        var px = new Color[w * h];
        for (int i = 0; i < px.Length; i++) px[i] = col;
        t.SetPixels(px); t.Apply(); return t;
    }
}
