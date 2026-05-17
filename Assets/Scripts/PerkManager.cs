using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PerkManager : MonoBehaviour
{
    public static bool IsMenuOpen { get; private set; }
    public static PerkManager Instance { get; private set; }

    public List<PerkDefinition> AcquiredPerks { get; } = new List<PerkDefinition>();

    // Special flag tracking
    public bool HasFlag(string flag) => _flags.Contains(flag);
    readonly HashSet<string> _flags = new HashSet<string>();

    PlayerStats _stats;
    GUIStyle _notifStyle;

    void Awake()
    {
        Instance   = this;
        IsMenuOpen = false; // always reset on scene load
        _stats     = GetComponent<PlayerStats>();
    }

    public PerkDefinition RollNew()
    {
        for (int i = 0; i < 30; i++)
        {
            var p = PerkDatabase.Roll();
            if (p.type != PerkType.Perk) return p; // stat changes can repeat freely
            if (string.IsNullOrEmpty(p.specialFlag) || !_flags.Contains(p.specialFlag))
                return p;
        }
        return PerkDatabase.Roll();
    }

    public void ShowPerk(PerkDefinition perk, System.Action onClose)
    {
        ApplyPerk(perk);
        onClose?.Invoke(); // immediately continue — no screen block
    }

    void ApplyPerk(PerkDefinition p)
    {
        if (_stats == null) return;
        _stats.speedMultiplier  = Mathf.Clamp(_stats.speedMultiplier  + p.dSpeed, 0.40f, 3.0f);
        _stats.damageMultiplier = Mathf.Max(0.05f, _stats.damageMultiplier + p.dDamage);
        _stats.damageReduction += p.dDamageReduction; // no clamp — can go negative (more damage taken)
        _stats.luck                  += p.dLuck;
        _stats.projectileSpeedMult = Mathf.Max(0.05f, _stats.projectileSpeedMult + p.dProjSpeed);
        _stats.projectileSizeMult  = Mathf.Max(0.05f, _stats.projectileSizeMult  + p.dProjSize);
        _stats.maxAmmoMult         = Mathf.Max(0.05f, _stats.maxAmmoMult         + p.dMaxAmmoMult);
        if (!string.IsNullOrEmpty(p.specialFlag))
        {
            _flags.Add(p.specialFlag);
            if (p.specialFlag == "INF_AMMO_30")
            {
                var shooter = FindFirstObjectByType<Shooter>();
                if (shooter != null) shooter._infAmmoTimer = 30f;
            }
            else if (p.specialFlag == "FIFTY_FIFTY")
            {
                bool win   = Random.value < 0.5f;
                var  pool  = win ? PerkDatabase.Buffs : PerkDatabase.Nerfs;
                var  rolled = pool[Random.Range(0, pool.Count)];
                _stats.speedMultiplier     = Mathf.Clamp(_stats.speedMultiplier     + rolled.dSpeed, 0.40f, 3.0f);
                _stats.damageMultiplier    = Mathf.Max(0.05f, _stats.damageMultiplier    + rolled.dDamage);
                _stats.damageReduction    += rolled.dDamageReduction;
                _stats.luck               += rolled.dLuck;
                _stats.projectileSpeedMult = Mathf.Max(0.05f, _stats.projectileSpeedMult + rolled.dProjSpeed);
                _stats.projectileSizeMult  = Mathf.Max(0.05f, _stats.projectileSizeMult  + rolled.dProjSize);
                _stats.maxAmmoMult         = Mathf.Max(0.05f, _stats.maxAmmoMult         + rolled.dMaxAmmoMult);
                string arrow = win ? "▲" : "▼";
                string result = win ? "LUCKY" : "UNLUCKY";
                _notes.Add(new Notification { text = $"★ 50/50 {result}: {arrow} {rolled.name}", col = new Color(1f, 0.65f, 0f), t = 5f });
                AcquiredPerks.Add(rolled);
                return;
            }
        }
        AcquiredPerks.Add(p);
        AddNotification(p);
        AudioManager.Instance.PlaySFX("Glup");
    }

    // ── Popup notifications ──────────────────────────────────────────────────
    struct Notification { public string text; public Color col; public float t; }
    readonly System.Collections.Generic.List<Notification> _notes = new();

    void AddNotification(PerkDefinition p)
    {
        string sign = p.type == PerkType.Nerf ? "▼" : p.type == PerkType.Buff ? "▲" : "★";
        _notes.Add(new Notification { text = $"{sign} {p.name}", col = PerkDefinition.RarityColor(p.rarity), t = 3.5f });
    }

    void Update()
    {
        for (int i = _notes.Count - 1; i >= 0; i--)
        {
            var n = _notes[i]; n.t -= Time.deltaTime; _notes[i] = n;
            if (n.t <= 0) _notes.RemoveAt(i);
        }
    }

    void OnGUI()
    {
        if (_notes.Count == 0) return;
        if (_notifStyle == null)
        {
            _notifStyle = new GUIStyle
            {
                font      = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"),
                fontSize  = 16,
                fontStyle = FontStyle.Bold
            };
        }
        float nx = 18f, ny = Screen.height * 0.38f, nh = 24f;
        for (int i = 0; i < _notes.Count; i++)
        {
            var n = _notes[i];
            var c = n.col; c.a = Mathf.Clamp01(n.t);
            _notifStyle.normal.textColor = c;
            GUI.Label(new Rect(nx, ny + i * nh, 260f, nh), n.text, _notifStyle);
        }
    }

    static Texture2D MakeTex(int w, int h, Color col)
    {
        var t = new Texture2D(w, h);
        var px = new Color[w * h];
        for (int i = 0; i < px.Length; i++) px[i] = col;
        t.SetPixels(px); t.Apply(); return t;
    }
}
