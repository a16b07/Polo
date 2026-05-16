using System.Collections.Generic;
using UnityEngine;

public enum PerkRarity { Common, Uncommon, Rare, Epic, Legendary }
public enum PerkType   { Buff, Nerf, Perk }

[System.Serializable]
public class PerkDefinition
{
    public string     id;
    public string     name;
    public string     description;
    public PerkRarity rarity;
    public PerkType   type;

    public float dSpeed;
    public float dDamage;
    public float dDamageReduction;
    public float dRegen;
    public float dLuck;
    public float dProjSpeed;
    public float dProjSize;
    public float dMaxAmmoMult;
    public string specialFlag;

    public PerkDefinition(string id, string name, string desc, PerkRarity r, PerkType t,
        float spd=0, float dmg=0, float dr=0, float reg=0, float lck=0,
        float ps=0, float pz=0, float am=0, string flag="")
    {
        this.id=id; this.name=name; description=desc; rarity=r; type=t;
        dSpeed=spd; dDamage=dmg; dDamageReduction=dr; dRegen=reg; dLuck=lck;
        dProjSpeed=ps; dProjSize=pz; dMaxAmmoMult=am; specialFlag=flag;
    }

    public static Color RarityColor(PerkRarity r) => r switch
    {
        PerkRarity.Common    => new Color(0.70f, 0.70f, 0.70f),
        PerkRarity.Uncommon  => new Color(0.30f, 0.90f, 0.30f),
        PerkRarity.Rare      => new Color(0.30f, 0.60f, 1.00f),
        PerkRarity.Epic      => new Color(0.80f, 0.30f, 1.00f),
        PerkRarity.Legendary => new Color(1.00f, 0.65f, 0.00f),
        _                    => Color.white
    };
}

public static class PerkDatabase
{
    // ── STAT NERFS (65%) ────────────────────────────────────────────────────
    public static readonly List<PerkDefinition> Nerfs = new List<PerkDefinition>
    {
        new("slow_feet",   "Slow Feet",         "Speed -6%",                           PerkRarity.Common,    PerkType.Nerf, spd:-0.06f),
        new("rusty_aim",   "Rusty Aim",          "Damage -5%",                          PerkRarity.Common,    PerkType.Nerf, dmg:-0.05f),
        new("thin_skin",   "Thin Skin",          "DR -4%",                              PerkRarity.Common,    PerkType.Nerf, dr:-0.04f),
        new("fumbler",     "Fumbler",            "Luck -0.2",                           PerkRarity.Common,    PerkType.Nerf, lck:-0.2f),
        new("soft_hands",  "Soft Hands",         "Regen -0.2 hp/s",                     PerkRarity.Common,    PerkType.Nerf, reg:-0.2f),
        new("heavy_legs",  "Heavy Legs",         "Speed -14%",                          PerkRarity.Uncommon,  PerkType.Nerf, spd:-0.14f),
        new("dull_blade",  "Dull Blade",          "Damage -12%",                         PerkRarity.Uncommon,  PerkType.Nerf, dmg:-0.12f),
        new("slow_ammo",   "Slow Ammo",           "Projectile speed -20%",               PerkRarity.Common,    PerkType.Nerf, ps:-0.20f),
        new("bb_pellets",  "BB Pellets",          "Projectile speed -35%, size -25%",    PerkRarity.Rare,      PerkType.Nerf, ps:-0.35f, pz:-0.25f),
        new("micro_shot",  "Micro Shot",          "Projectile size -30%",                PerkRarity.Uncommon,  PerkType.Nerf, pz:-0.30f),
        new("thin_clip",   "Thin Clip",           "Max ammo -20%",                       PerkRarity.Common,    PerkType.Nerf, am:-0.20f),
        new("half_mag",    "Half Mag",            "Max ammo -35%",                       PerkRarity.Uncommon,  PerkType.Nerf, am:-0.35f),
        new("empty_handed","Empty Handed",        "Max ammo -55%",                       PerkRarity.Rare,      PerkType.Nerf, am:-0.55f),
        new("brittle",     "Brittle",            "DR -10%",                             PerkRarity.Uncommon,  PerkType.Nerf, dr:-0.10f),
        new("cursed",      "Cursed",             "Luck -0.6",                           PerkRarity.Uncommon,  PerkType.Nerf, lck:-0.6f),
        new("lead_boots",  "Lead Boots",         "Speed -10%, Luck -0.3",               PerkRarity.Uncommon,  PerkType.Nerf, spd:-0.10f, lck:-0.3f),
        new("crippled",    "Crippled",           "Speed -24%",                          PerkRarity.Rare,      PerkType.Nerf, spd:-0.24f),
        new("weakened",    "Weakened",           "Damage -22%",                         PerkRarity.Rare,      PerkType.Nerf, dmg:-0.22f),
        new("exposed",     "Exposed",            "DR -20%",                             PerkRarity.Rare,      PerkType.Nerf, dr:-0.20f),
        new("bad_luck",    "Bad Luck",           "Luck -1.2",                           PerkRarity.Rare,      PerkType.Nerf, lck:-1.2f),
        new("shattered",   "Shattered",          "DR -38%, Speed -12%",                 PerkRarity.Epic,      PerkType.Nerf, spd:-0.12f, dr:-0.38f),
        new("broken",      "Broken",             "Damage -38%",                         PerkRarity.Epic,      PerkType.Nerf, dmg:-0.38f),
        new("marked",      "Marked for Death",   "Luck -2.0, DR -18%, Speed -8%",       PerkRarity.Epic,      PerkType.Nerf, spd:-0.08f, dr:-0.18f, lck:-2.0f),
        new("doomed",      "Doomed",             "Speed -55%, Damage -45%, DR -30%",    PerkRarity.Legendary, PerkType.Nerf, spd:-0.55f, dmg:-0.45f, dr:-0.30f),
        new("forsaken",    "Forsaken",           "ALL stats -35%",                      PerkRarity.Legendary, PerkType.Nerf, spd:-0.35f, dmg:-0.35f, dr:-0.35f, reg:-2.0f, lck:-1.5f),
        new("paralyzed",   "Paralyzed",          "Speed -70%",                          PerkRarity.Legendary, PerkType.Nerf, spd:-0.70f),
        new("cursed_soul", "Cursed Soul",        "Damage -65%, Luck -3.0",              PerkRarity.Legendary, PerkType.Nerf, dmg:-0.65f, lck:-3.0f),
    };

    // ── STAT BUFFS (25%) ────────────────────────────────────────────────────
    public static readonly List<PerkDefinition> Buffs = new List<PerkDefinition>
    {
        new("light_feet",  "Light Feet",         "Speed +7%",                           PerkRarity.Common,    PerkType.Buff, spd:0.07f),
        new("sharp_eye",   "Sharp Eye",          "Damage +7%",                          PerkRarity.Common,    PerkType.Buff, dmg:0.07f),
        new("thick_skin",  "Thick Skin",         "DR +5%",                              PerkRarity.Common,    PerkType.Buff, dr:0.05f),
        new("fortune",     "Fortune's Smile",    "Luck +0.2",                           PerkRarity.Common,    PerkType.Buff, lck:0.2f),
        new("trickle",     "Trickle Heal",       "Regen +0.3 hp/s",                     PerkRarity.Common,    PerkType.Buff, reg:0.3f),
        new("swift",       "Swift",              "Speed +16%",                          PerkRarity.Uncommon,  PerkType.Buff, spd:0.16f),
        new("precision",   "Precision",          "Damage +16%",                         PerkRarity.Uncommon,  PerkType.Buff, dmg:0.16f),
        new("reinforced",  "Reinforced",         "DR +12%",                             PerkRarity.Uncommon,  PerkType.Buff, dr:0.12f),
        new("lucky",       "Lucky",              "Luck +0.5",                           PerkRarity.Uncommon,  PerkType.Buff, lck:0.5f),
        new("field_medic",   "Field Medic",         "Regen +1.0 hp/s",                     PerkRarity.Uncommon,  PerkType.Buff, reg:1.0f),
        new("quick_draw",    "Quick Draw",          "Projectile speed +25%",               PerkRarity.Common,    PerkType.Buff, ps:0.25f),
        new("large_caliber", "Large Caliber",       "Projectile size +30%",                PerkRarity.Common,    PerkType.Buff, pz:0.30f),
        new("high_velocity", "High Velocity",       "Projectile speed +55%",               PerkRarity.Rare,      PerkType.Buff, ps:0.55f),
        new("heavy_round",   "Heavy Round",         "Proj size +60%, speed +20%",          PerkRarity.Rare,      PerkType.Buff, ps:0.20f, pz:0.60f),
        new("railgun_round", "Railgun Round",       "Projectile speed +120%",              PerkRarity.Legendary, PerkType.Buff, ps:1.20f),
        new("well_stocked",  "Well Stocked",        "Max ammo +25%",                       PerkRarity.Common,    PerkType.Buff, am:0.25f),
        new("ammo_cache",    "Ammo Cache",          "Max ammo +45%",                       PerkRarity.Rare,      PerkType.Buff, am:0.45f),
        new("arsenal",       "Arsenal",             "Max ammo +80%",                       PerkRarity.Epic,      PerkType.Buff, am:0.80f),
        new("blazing",     "Blazing",            "Speed +28%",                          PerkRarity.Rare,      PerkType.Buff, spd:0.28f),
        new("deadly",      "Deadly",             "Damage +28%",                         PerkRarity.Rare,      PerkType.Buff, dmg:0.28f),
        new("armored",     "Armored",            "DR +22%",                             PerkRarity.Rare,      PerkType.Buff, dr:0.22f),
        new("blessed",     "Blessed",            "Luck +1.2",                           PerkRarity.Rare,      PerkType.Buff, lck:1.2f),
        new("regeneration","Regeneration",       "Regen +2.2 hp/s",                     PerkRarity.Rare,      PerkType.Buff, reg:2.2f),
        new("phantom",     "Phantom",            "Speed +45%",                          PerkRarity.Epic,      PerkType.Buff, spd:0.45f),
        new("explosive_d", "Explosive",          "Damage +45%",                         PerkRarity.Epic,      PerkType.Buff, dmg:0.45f),
        new("fortress",    "Fortress",           "DR +35%",                             PerkRarity.Epic,      PerkType.Buff, dr:0.35f),
        new("unstoppable", "Unstoppable",        "Speed +30%, Damage +25%",             PerkRarity.Epic,      PerkType.Buff, spd:0.30f, dmg:0.25f),
        new("god_speed",   "God Speed",          "Speed +90%",                          PerkRarity.Legendary, PerkType.Buff, spd:0.90f),
        new("one_shot",    "One Shot",           "Damage +100%",                        PerkRarity.Legendary, PerkType.Buff, dmg:1.00f),
        new("immortal",    "Immortal Skin",      "DR +60%",                             PerkRarity.Legendary, PerkType.Buff, dr:0.60f),
        new("apex",        "Apex Predator",      "ALL stats +30%",                      PerkRarity.Legendary, PerkType.Buff, spd:0.30f, dmg:0.30f, dr:0.30f, reg:3.0f, lck:1.5f),
    };

    // ── NEGATIVE PASSIVES (6% of total) ─────────────────────────────────────
    // Real gameplay effects, not just stat changes
    public static readonly List<PerkDefinition> NegativePerks = new List<PerkDefinition>
    {
        new("frenzy",      "Frenzy",             "Weapon spread never recovers — always at max bloom",
            PerkRarity.Uncommon,  PerkType.Perk, flag:"FRENZY"),
        new("heavy_ammo",  "Heavy Ammo",         "Spread builds 2x faster with every shot",
            PerkRarity.Common,    PerkType.Perk, flag:"HEAVY_AMMO"),
        new("paranoia",    "Paranoia",           "Your aim drifts and shakes uncontrollably",
            PerkRarity.Rare,      PerkType.Perk, flag:"PARANOIA"),
        new("glass_bones", "Glass Bones",        "Jumping deals 5 damage to yourself",
            PerkRarity.Uncommon,  PerkType.Perk, flag:"GLASS_BONES"),
        new("fumble",      "Fumble Hands",       "30% chance thrown weapons fly backward",
            PerkRarity.Rare,      PerkType.Perk, flag:"FUMBLE_HANDS"),
        new("scatter_shot", "Scatter Shot",       "Max spread multiplied by 1.9x — bloom goes wild",
            PerkRarity.Epic,      PerkType.Perk, flag:"SCATTER_SHOT"),
        new("hex",          "Hex",               "Spread builds 3x faster AND never recovers",
            PerkRarity.Legendary, PerkType.Perk, flag:"HEX"),
        new("double_consume","Trigger Happy",      "Each shot uses 2 ammo instead of 1",
            PerkRarity.Rare,      PerkType.Perk, flag:"DOUBLE_CONSUME"),
        new("warp_ammo",    "Warp Ammo",         "Enemy projectiles are 60% faster",
            PerkRarity.Rare,      PerkType.Perk, flag:"FAST_ENEMY_BULLETS"),
        new("fat_bullets",  "Fat Bullets",       "Enemy projectiles are 2.5x larger",
            PerkRarity.Epic,      PerkType.Perk, flag:"BIG_ENEMY_BULLETS"),
    };

    // ── POSITIVE PASSIVES (4% of total) ─────────────────────────────────────
    // Real gameplay effects, fun and impactful
    public static readonly List<PerkDefinition> PositivePerks = new List<PerkDefinition>
    {
        new("explosive_r", "Explosive Rounds",   "Bullets explode on impact, damaging nearby enemies",
            PerkRarity.Rare,      PerkType.Perk, flag:"EXPLOSIVE_ROUNDS"),
        new("piercing",    "Piercing",           "Bullets pass through enemies, hitting multiple",
            PerkRarity.Uncommon,  PerkType.Perk, flag:"PIERCING"),
        new("double_jump", "Double Jump",        "You can jump a second time in mid-air",
            PerkRarity.Rare,      PerkType.Perk, flag:"DOUBLE_JUMP"),
        new("leech",       "Leech",              "Hitting enemies slowly boosts your regen",
            PerkRarity.Uncommon,  PerkType.Perk, reg:0.5f, flag:"LEECH"),
        new("glass_cannon2","Glass Cannon",      "+50% damage but DR -35% — high risk, high reward",
            PerkRarity.Rare,      PerkType.Perk, dmg:0.50f, dr:-0.35f),
        new("iron_skin",   "Iron Skin",          "+40% DR but -15% speed — slow but tough",
            PerkRarity.Rare,      PerkType.Perk, spd:-0.15f, dr:0.40f),
        new("inf_ammo",     "Infinite Magazine",  "30 seconds of infinite ammo — go wild",
            PerkRarity.Epic,      PerkType.Perk, flag:"INF_AMMO_30"),
        new("ammo_conserve","Ammo Saver",         "30% chance each shot doesn't consume ammo",
            PerkRarity.Uncommon,  PerkType.Perk, flag:"AMMO_CONSERVE"),
        new("last_shot",    "Last Shot",          "When on your last bullet, deal 2x damage",
            PerkRarity.Rare,      PerkType.Perk, flag:"LAST_SHOT"),
        new("slow_field",   "Slow Field",         "Enemy projectiles are 45% slower",
            PerkRarity.Uncommon,  PerkType.Perk, flag:"SLOW_ENEMY_BULLETS"),
        new("shrink_ray",   "Shrink Ray",         "Enemy projectiles are 65% smaller",
            PerkRarity.Rare,      PerkType.Perk, flag:"SMALL_ENEMY_BULLETS"),
        new("big_shot",    "Big Shot",           "Projectiles are 2.5x larger — much easier to hit",
            PerkRarity.Uncommon,  PerkType.Perk, flag:"PROJ_SIZE_X25"),
        new("hypersonic",  "Hypersonic",         "Projectiles travel 80% faster",
            PerkRarity.Rare,      PerkType.Perk, flag:"PROJ_SPEED_180"),
        new("cannonball",  "Cannonball",         "Projectiles are 4x larger but 50% slower",
            PerkRarity.Epic,      PerkType.Perk, flag:"CANNONBALL"),
        new("sniper_perk", "Sniper",             "Tiny fast projectile — x0.3 size, x2 speed, +30% damage",
            PerkRarity.Rare,      PerkType.Perk, dmg:0.30f, flag:"SNIPER"),
        new("adrenaline",  "Adrenaline Rush",    "Killing an enemy gives a brief massive speed boost",
            PerkRarity.Epic,      PerkType.Perk, flag:"ADRENALINE"),
        new("gods_touch",  "God's Touch",        "+18% to ALL stats simultaneously",
            PerkRarity.Epic,      PerkType.Perk, spd:0.18f, dmg:0.18f, dr:0.18f, reg:2.0f, lck:0.8f),
        new("transcend",   "Transcendence",      "Every stat boosted by 35% — pure ascension",
            PerkRarity.Legendary, PerkType.Perk, spd:0.35f, dmg:0.35f, dr:0.35f, reg:4.0f, lck:2.0f),
        new("one_man_army","One Man Army",       "Each kill permanently stacks +3% damage (max 45%)",
            PerkRarity.Legendary, PerkType.Perk, flag:"ONE_MAN_ARMY"),
    };

    static readonly float[] RarityWeights = { 0.40f, 0.30f, 0.20f, 0.08f, 0.02f };

    public static PerkDefinition Roll()
    {
        float r = Random.value;
        List<PerkDefinition> pool;
        if      (r < 0.65f) pool = Nerfs;
        else if (r < 0.90f) pool = Buffs;
        else if (r < 0.96f) pool = NegativePerks;
        else                pool = PositivePerks;
        return PickFromPool(pool);
    }

    static PerkDefinition PickFromPool(List<PerkDefinition> pool)
    {
        var byRarity = new List<PerkDefinition>[5];
        for (int i = 0; i < 5; i++) byRarity[i] = new List<PerkDefinition>();
        foreach (var p in pool) byRarity[(int)p.rarity].Add(p);

        float roll = Random.value, cumul = 0f;
        for (int i = 4; i >= 0; i--)
        {
            cumul += RarityWeights[i];
            if (roll < cumul && byRarity[i].Count > 0)
                return byRarity[i][Random.Range(0, byRarity[i].Count)];
        }
        return byRarity[0].Count > 0 ? byRarity[0][Random.Range(0, byRarity[0].Count)] : pool[0];
    }
}
