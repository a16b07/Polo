using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(BoxCollider))]
public class EnemyAI : MonoBehaviour
{
    public static event System.Action OnAnyEnemyDied;

    [Header("Health")]
    public int health = 50;

    [Header("Combat")]
    public float fireInterval  = 1.4f;
    public float chaseDistance = 18f;

    [Header("Movement")]
    public float moveSpeed     = 3f;
    public float patrolRadius  = 3f;

    NavMeshAgent _agent;
    Transform    _player;
    GameObject   _heldWeapon;
    int          _damage = 25;
    float        _nextFireTime;
    float        _patrolTimer;
    Vector3      _patrolCenter;
    bool         _resized;
    bool         _dead;

    // Stuck detection
    Vector3 _lastPos;
    float   _stuckTimer;

    void Start()
    {
        _agent  = GetComponent<NavMeshAgent>();
        _player = GameObject.FindGameObjectWithTag("Player")?.transform;

        _agent.speed                  = moveSpeed;
        _agent.angularSpeed           = 0f;
        _agent.updateRotation         = false;
        _agent.stoppingDistance       = 4f;
        _agent.radius                 = 0.7f;
        _agent.height                 = 2.4f;
        _agent.obstacleAvoidanceType  = ObstacleAvoidanceType.MedQualityObstacleAvoidance;
        _agent.avoidancePriority      = Random.Range(20, 80);

        _patrolCenter = transform.position;
        _lastPos      = transform.position;

        // Replace any stale CapsuleCollider with a BoxCollider
        var oldCap = GetComponent<CapsuleCollider>();
        if (oldCap != null) Destroy(oldCap);

        var box    = GetComponent<BoxCollider>() ?? gameObject.AddComponent<BoxCollider>();
        box.center = new Vector3(0f, 1.2f, 0f);
        box.size   = new Vector3(0.55f, 2.5f, 0.55f);

        var bodyHitbox        = gameObject.AddComponent<Hitbox>();
        bodyHitbox.isHeadshot = false;
        bodyHitbox.enemy      = this;

        var headGO = new GameObject("HeadHitbox");
        headGO.transform.SetParent(transform);
        headGO.transform.localPosition = new Vector3(0f, 2.25f, 0f);
        var headCol        = headGO.AddComponent<SphereCollider>();
        headCol.radius     = 0.32f;
        var headHitbox     = headGO.AddComponent<Hitbox>();
        headHitbox.isHeadshot = true;
        headHitbox.enemy      = this;

        BuildModel();
        EquipWeapon();

        PickPatrolPoint();
    }

    void BuildModel()
    {
        var body = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        body.name = "Body";
        body.transform.SetParent(transform);
        body.transform.localPosition = new Vector3(0f, 1.2f, 0f);
        body.transform.localScale    = new Vector3(0.35f, 1.2f, 0.05f);
        Destroy(body.GetComponent<Collider>());

        var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        mat.color = new Color(0.1f, 0.3f, 1f);
        body.GetComponent<MeshRenderer>().material = mat;
    }

    void EquipWeapon()
    {
        var allWeapons = FindObjectsByType<WeaponStats>(FindObjectsSortMode.None);
        if (allWeapons.Length == 0) return;

        var chosen = allWeapons[Random.Range(0, allWeapons.Length)];
        _damage = chosen.damage;

        var clone = Instantiate(chosen.gameObject);
        clone.name = chosen.weaponName + "_AI";

        var rb   = clone.GetComponent<Rigidbody>();    if (rb   != null) Destroy(rb);
        var aura = clone.GetComponent<GunAura>();      if (aura != null) Destroy(aura);
        foreach (var c in clone.GetComponents<Collider>()) Destroy(c);

        clone.transform.SetParent(transform);
        clone.transform.localPosition = chosen.heldPosition != Vector3.zero
            ? chosen.heldPosition : new Vector3(0.3f, 1.2f, 0.4f);
        clone.transform.localRotation = Quaternion.Euler(chosen.heldRotation);
        clone.transform.localScale    = Vector3.one;

        _heldWeapon = clone;
    }

    void Update()
    {
        if (_player == null) return;

        // Always face player
        Vector3 toPlayer = _player.position - transform.position;
        toPlayer.y = 0f;
        if (toPlayer.sqrMagnitude > 0.01f)
            transform.rotation = Quaternion.LookRotation(toPlayer);

        float dist = toPlayer.magnitude;

        // Navigate
        if (dist <= chaseDistance)
            _agent.SetDestination(_player.position);
        else
            Patrol();

        // Stuck detection — pick new patrol point if barely moving for 2s
        _stuckTimer += Time.deltaTime;
        if (_stuckTimer >= 2f)
        {
            if (Vector3.Distance(transform.position, _lastPos) < 0.3f)
                PickPatrolPoint();
            _lastPos    = transform.position;
            _stuckTimer = 0f;
        }

        // Shoot
        if (Time.time >= _nextFireTime)
        {
            Shoot();
            _nextFireTime = Time.time + fireInterval;
        }
    }

    void Patrol()
    {
        _patrolTimer -= Time.deltaTime;
        if (_patrolTimer <= 0f || _agent.remainingDistance < 0.5f)
            PickPatrolPoint();
    }

    void PickPatrolPoint()
    {
        for (int i = 0; i < 10; i++)
        {
            Vector2 rand  = Random.insideUnitCircle * patrolRadius;
            Vector3 point = _patrolCenter + new Vector3(rand.x, 0f, rand.y);
            if (NavMesh.SamplePosition(point, out NavMeshHit hit, 3f, NavMesh.AllAreas))
            {
                _agent.SetDestination(hit.position);
                _patrolTimer = Random.Range(3f, 7f);
                return;
            }
        }
    }

    void Shoot()
    {
        Vector3 origin = transform.position + Vector3.up * 1.5f;
        Vector3 target = _player.position   + Vector3.up * 1.2f;
        Vector3 dir    = (target - origin).normalized;

        float speed = 32f;
        float size  = 0.06f;

        var pm = PerkManager.Instance;
        if (pm != null)
        {
            if (pm.HasFlag("SLOW_ENEMY_BULLETS"))  speed *= 0.55f;
            if (pm.HasFlag("FAST_ENEMY_BULLETS"))  speed *= 1.7f;
            if (pm.HasFlag("SMALL_ENEMY_BULLETS")) size  *= 0.35f;
            if (pm.HasFlag("BIG_ENEMY_BULLETS"))   size  *= 2.5f;
        }

        var go = new GameObject("EnemyBullet");
        go.transform.position = origin;
        go.transform.rotation = Quaternion.LookRotation(dir);

        var b             = go.AddComponent<Bullet>();
        b.damage          = _damage;
        b.speed           = speed;
        b.radius          = size;
        b.isEnemyBullet   = true;
        b.bulletColor     = new Color(1f, 0.15f, 0.05f);

        StartCoroutine(MuzzleFlash(origin));
    }

    System.Collections.IEnumerator MuzzleFlash(Vector3 pos)
    {
        var go = new GameObject("EnemyMuzzle");
        go.transform.position = pos + transform.forward * 0.5f;
        var light       = go.AddComponent<Light>();
        light.type      = LightType.Point;
        light.color     = new Color(1f, 0.3f, 0.1f);
        light.intensity = 8f;
        light.range     = 5f;
        light.shadows   = LightShadows.None;
        yield return new WaitForSeconds(0.05f);
        Destroy(go);
    }

    public void TakeDamage(int dmg)
    {
        health -= dmg;
        if (health <= 0) Die();
    }

    public void Resize(float heightMult, float areaMult = 1f)
    {
        if (_resized) return;
        _resized = true;

        var body = transform.Find("Body");
        if (body != null) body.localScale = new Vector3(body.localScale.x, body.localScale.y * heightMult, body.localScale.z);

        _agent.height = _agent.height * heightMult;
        _agent.radius = _agent.radius * areaMult;
        patrolRadius  = patrolRadius  * areaMult;

        // Scale held weapon
        if (_heldWeapon != null)
            _heldWeapon.transform.localScale = _heldWeapon.transform.localScale * areaMult;
    }

    void Die()
    {
        if (_dead) return;
        _dead = true;
        OnAnyEnemyDied?.Invoke();

        if (_heldWeapon != null)
        {
            _heldWeapon.transform.SetParent(null);
            _heldWeapon.transform.localScale = Vector3.one;

            var rb = _heldWeapon.AddComponent<Rigidbody>();
            rb.linearVelocity  = transform.forward * 9f + Vector3.up * 5f;
            rb.angularVelocity = Random.insideUnitSphere * 4f;
            _heldWeapon.AddComponent<ThrownWeaponDamage>();

            var cap = _heldWeapon.AddComponent<CapsuleCollider>();
            cap.direction = 2; cap.radius = 0.15f; cap.height = 0.6f;
            _heldWeapon.AddComponent<GunAura>();
        }
        Destroy(gameObject);
    }
}
