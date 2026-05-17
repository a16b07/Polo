using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public static PlayerHealth Instance { get; private set; }

    public int maxHp = 200;
    public int currentHp { get; private set; }

    PlayerStats _stats;
    float _regenAccum;

    public float HpFraction => (float)currentHp / maxHp;

    void Awake()
    {
        Instance = this;
        currentHp = maxHp;
    }

    void Start()
    {
        _stats = GetComponent<PlayerStats>();
    }

    void Update()
    {
        if (_stats == null || _stats.regeneration <= 0f || currentHp >= maxHp) return;
        _regenAccum += _stats.regeneration * Time.deltaTime;
        if (_regenAccum >= 1f)
        {
            int healed = Mathf.FloorToInt(_regenAccum);
            currentHp = Mathf.Min(currentHp + healed, maxHp);
            _regenAccum -= healed;
        }
    }

    public void TakeDamage(int dmg)
    {
        float reduction = _stats != null ? _stats.damageReduction : 0f;
        int finalDmg = Mathf.Max(1, Mathf.RoundToInt(dmg * (1f - reduction)));
        currentHp = Mathf.Max(0, currentHp - finalDmg);
        if (currentHp <= 0) Die();
    }

    public void Heal(int amount)
    {
        currentHp = Mathf.Min(currentHp + amount, maxHp);
    }

    void Die()
    {
        // TODO: proper game-over screen
        Debug.Log("Player died.");
    }
}
