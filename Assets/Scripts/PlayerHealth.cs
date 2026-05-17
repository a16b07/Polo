using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public static PlayerHealth Instance { get; private set; }
    public static event System.Action OnDamaged;

    public int maxHp = 200;
    public int currentHp { get; private set; }

    PlayerStats _stats;

    public float HpFraction => (float)currentHp / maxHp;

    void Awake()
    {
        Instance  = this;
        currentHp = maxHp;
    }

    void Start() => _stats = GetComponent<PlayerStats>();

    public void TakeDamage(int dmg)
    {
        float reduction = _stats != null ? _stats.damageReduction : 0f;
        int finalDmg = Mathf.Max(1, Mathf.RoundToInt(dmg * (1f - reduction)));
        currentHp = Mathf.Max(0, currentHp - finalDmg);
        AudioManager.Instance.PlaySFX("PlayerHurt");
        OnDamaged?.Invoke();
        if (currentHp <= 0) Die();
    }

    public void Heal(int amount) => currentHp = Mathf.Min(currentHp + amount, maxHp);

    public void ResetHP() => currentHp = maxHp;

    void Die()
    {
        var menu = FindFirstObjectByType<MainMenu>();
        if (menu != null) menu.TriggerGameOver();
        AudioManager.Instance.StopMusic(0.2f);
    }
}
