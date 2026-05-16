using UnityEngine;

public class ThrownWeaponDamage : MonoBehaviour
{
    public int damage = 75;

    Rigidbody _rb;
    bool      _hit;

    void Start() => _rb = GetComponent<Rigidbody>();

    void OnCollisionEnter(Collision col)
    {
        if (_hit) return;
        if (_rb != null && _rb.linearVelocity.magnitude < 3f) return; // only damage at speed

        var enemy = col.collider.GetComponentInParent<EnemyAI>();
        if (enemy != null)
        {
            _hit = true;
            enemy.TakeDamage(damage);
        }

        Destroy(this); // one damage instance per throw
    }
}
