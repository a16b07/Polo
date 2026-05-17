using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float  speed          = 55f;
    public float  radius         = 0.05f;
    public int    damage         = 25;
    public float  maxRange       = 150f;
    public bool   explosiveRounds;
    public bool   piercing;
    public bool   isEnemyBullet  = false;
    public Color  bulletColor    = new Color(1f, 0.9f, 0f);

    Vector3 _start;

    void Start()
    {
        _start = transform.position;
        BuildVisual();
    }

    void BuildVisual()
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        go.transform.SetParent(transform);
        go.transform.localPosition = Vector3.zero;
        float s = radius * 2f;
        go.transform.localScale = new Vector3(s, s, s);
        Destroy(go.GetComponent<Collider>());

        var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        mat.color = bulletColor;
        mat.EnableKeyword("_EMISSION");
        mat.SetColor("_EmissionColor", bulletColor * 3f);
        go.GetComponent<MeshRenderer>().material = mat;

        var light       = go.AddComponent<Light>();
        light.type      = LightType.Point;
        light.color     = bulletColor;
        light.intensity = 1.5f;
        light.range     = 1.5f;
        light.shadows   = LightShadows.None;
    }

    void Update()
    {
        float step = speed * Time.deltaTime;

        int mask = ~(1 << 9); // ignore layer 9 (Props) so bullets pass through furniture
        if (Physics.SphereCast(transform.position, radius, transform.forward, out RaycastHit hit, step, mask))
        {
            OnHit(hit);
            if (!piercing || isEnemyBullet) { Destroy(gameObject); return; }
        }

        transform.position += transform.forward * step;

        if (Vector3.Distance(_start, transform.position) >= maxRange)
            Destroy(gameObject);
    }

    void OnHit(RaycastHit hit)
    {
        var rb = hit.collider.GetComponentInParent<Rigidbody>();
        if (rb != null) rb.AddForceAtPosition(transform.forward * 10f, hit.point, ForceMode.Impulse);

        if (!isEnemyBullet)
        {
            var hitbox = hit.collider.GetComponent<Hitbox>();
            var enemy  = hitbox != null ? hitbox.enemy : hit.collider.GetComponentInParent<EnemyAI>();
            if (enemy != null)
            {
                bool headshot = hit.point.y >= enemy.HeadThreshold;
                int dmg = headshot ? damage * 2 : damage;
                enemy.TakeDamage(dmg);
                HitMarker.Instance?.Show(headshot);
                if (explosiveRounds)
                    foreach (var col in Physics.OverlapSphere(hit.point, 2.5f))
                    {
                        var e2 = col.GetComponentInParent<EnemyAI>();
                        if (e2 != null && e2 != enemy)
                            e2.TakeDamage(Mathf.RoundToInt(damage * 0.45f));
                    }
            }
        }
        else
        {
            if (hit.collider.CompareTag("Player"))
                PlayerHealth.Instance?.TakeDamage(damage);
        }
    }
}
