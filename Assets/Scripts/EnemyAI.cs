using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

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

    NavMeshAgent       _agent;
    Transform          _player;
    GameObject         _heldWeapon;
    EnemySpriteAnimator _spriteAnim;
    int   _damage = 25;
    float _nextFireTime;
    float _patrolTimer;
    Vector3 _patrolCenter;
    bool _resized;
    bool  _dead;
    public bool  applyEntranceResize;
    public float flashStunEnd;

    public static bool DebugOutlineEnabled;
    GameObject _outlineGO;
    Material   _outlineMat;
    float      _outlinePulse;

    // Headshot threshold: top 30% of box (2.85u * 0.70 = 2.0u above root)
    public float HeadThreshold => transform.position.y + 2.0f;

    // Stuck detection
    Vector3 _lastPos;
    float   _stuckTimer;

    void Start()
    {
        _agent  = GetComponent<NavMeshAgent>();
        _player = GameObject.FindGameObjectWithTag("Player")?.transform;

        _agent.speed                 = moveSpeed;
        _agent.angularSpeed          = 0f;
        _agent.updateRotation        = false;
        _agent.stoppingDistance      = 9f;
        _agent.radius                = 2.1f;
        _agent.height                = 2.4f;
        _agent.obstacleAvoidanceType = ObstacleAvoidanceType.MedQualityObstacleAvoidance;
        _agent.avoidancePriority     = Random.Range(20, 80);

        _patrolCenter = transform.position;
        _lastPos      = transform.position;

        var oldCap = GetComponent<CapsuleCollider>();
        if (oldCap != null) Destroy(oldCap);

        // Single box covering the full sprite — headshot determined by hit.point.y in Bullet
        var box    = GetComponent<BoxCollider>() ?? gameObject.AddComponent<BoxCollider>();
        box.center = new Vector3(0f, 1.425f, 0f);
        box.size   = new Vector3(1.5f, 2.85f, 1.0f);
        box.isTrigger = false;

        var bodyHitbox   = gameObject.AddComponent<Hitbox>();
        bodyHitbox.enemy = this;

        BuildSprite();
        BuildDebugOutline();
        EquipWeapon();
        PickPatrolPoint();
        if (applyEntranceResize) Resize(1.8f, 1.3f);
    }

    void BuildDebugOutline()
    {
        var shader = Shader.Find("Custom/DoorOutline");
        if (shader == null) return;

        _outlineGO = GameObject.CreatePrimitive(PrimitiveType.Cube);
        _outlineGO.name = "DebugOutline";
        _outlineGO.transform.SetParent(transform, false);
        _outlineGO.transform.localPosition = new Vector3(0f, 1.425f, 0f);
        _outlineGO.transform.localScale    = new Vector3(1.5f, 2.85f, 1.0f);
        Destroy(_outlineGO.GetComponent<Collider>());

        _outlineMat = new Material(shader);
        _outlineMat.SetColor("_OutlineColor", new Color(1f, 0.25f, 0.15f, 0.9f));
        _outlineMat.SetFloat("_OutlineWidth", 0.022f);
        var mr = _outlineGO.GetComponent<MeshRenderer>();
        mr.material = _outlineMat;
        mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        mr.receiveShadows    = false;
        _outlineGO.SetActive(false);
    }

    void BuildSprite()
    {
        var sprites = Resources.LoadAll<Sprite>("Sprites/poloEnemyColor");
        if (sprites == null || sprites.Length < 8)
        {
            Debug.LogWarning("EnemyAI: could not load poloEnemy sprites from Resources/Sprites/");
            return;
        }

        System.Array.Sort(sprites, (a, b) =>
            string.Compare(a.name, b.name, System.StringComparison.Ordinal));

        var walkFrames  = new Sprite[] { sprites[0], sprites[1], sprites[2], sprites[3] };
        var deathFrames = new Sprite[] { sprites[4], sprites[5], sprites[6], sprites[7] };

        var spriteGO = new GameObject("Sprite");
        spriteGO.transform.SetParent(transform);
        spriteGO.transform.localPosition = Vector3.zero;
        spriteGO.transform.localScale    = Vector3.one;

        _spriteAnim = spriteGO.AddComponent<EnemySpriteAnimator>();
        _spriteAnim.Init(walkFrames, deathFrames);
    }

    void EquipWeapon()
    {
        var allWeapons = FindObjectsByType<WeaponStats>(FindObjectsSortMode.None);
        if (allWeapons.Length == 0) return;

        var chosen = allWeapons[Random.Range(0, allWeapons.Length)];
        _damage = chosen.damage;

        var clone = Instantiate(chosen.gameObject);
        clone.name = chosen.weaponName + "_AI";

        var rb   = clone.GetComponent<Rigidbody>();  if (rb   != null) Destroy(rb);
        var aura = clone.GetComponent<GunAura>();    if (aura != null) Destroy(aura);
        foreach (var c in clone.GetComponents<Collider>()) Destroy(c);

        clone.transform.SetParent(transform);
        clone.transform.localPosition = new Vector3(0.26f, 1.63f, 0.38f);
        clone.transform.localRotation = Quaternion.Euler(0f, 168.82f, 0f);
        clone.transform.localScale    = Vector3.one;

        _heldWeapon = clone;
    }

    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.pKey.wasPressedThisFrame)
            DebugOutlineEnabled = !DebugOutlineEnabled;
        if (_outlineGO != null) _outlineGO.SetActive(DebugOutlineEnabled && !_dead);
        if (DebugOutlineEnabled && _outlineMat != null)
        {
            _outlinePulse += Time.deltaTime * 4f;
            _outlineMat.SetFloat("_Pulse", Mathf.Sin(_outlinePulse) * 0.5f + 0.5f);
        }

        if (_dead || _player == null) return;

        Vector3 toPlayer = _player.position - transform.position;
        toPlayer.y = 0f;
        if (toPlayer.sqrMagnitude > 0.01f)
            transform.rotation = Quaternion.LookRotation(toPlayer);

        float dist = toPlayer.magnitude;

        if (dist <= chaseDistance)
            _agent.SetDestination(_player.position);
        else
            Patrol();

        _stuckTimer += Time.deltaTime;
        if (_stuckTimer >= 2f)
        {
            if (Vector3.Distance(transform.position, _lastPos) < 0.3f)
                PickPatrolPoint();
            _lastPos    = transform.position;
            _stuckTimer = 0f;
        }

        if (Time.time >= _nextFireTime)
        {
            Shoot();
            _nextFireTime = Time.time + fireInterval;
        }

        if (_spriteAnim != null)
        {
            bool moving = _agent.velocity.magnitude > 0.1f;
            _spriteAnim.SetState(moving
                ? EnemySpriteAnimator.State.Walking
                : EnemySpriteAnimator.State.Idle);
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
        if (Time.time < flashStunEnd) return;

        Vector3 origin = _heldWeapon != null
            ? _heldWeapon.transform.position
            : transform.position + Vector3.up * 1.0f;
        Vector3 target = _player.position   + Vector3.up * 1.2f;
        Vector3 dir    = (target - origin).normalized;

        float speed = 64f;
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

        var b           = go.AddComponent<Bullet>();
        b.damage        = _damage;
        b.speed         = speed;
        b.radius        = size;
        b.isEnemyBullet = true;
        b.bulletColor   = new Color(1f, 0.15f, 0.05f);

        StartCoroutine(MuzzleFlash(origin, dir));
    }

    System.Collections.IEnumerator MuzzleFlash(Vector3 pos, Vector3 dir)
    {
        var go = new GameObject("EnemyMuzzle");
        go.transform.position = pos + dir * 0.4f;
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
        AudioManager.Instance.PlaySFX("GreatHit");
    }

    public void Resize(float heightMult, float areaMult = 1f)
    {
        if (_resized) return;
        _resized = true;

        _agent.height = _agent.height * heightMult;
        _agent.radius = _agent.radius * areaMult;
        patrolRadius  = patrolRadius  * areaMult;

        if (_spriteAnim != null)
            _spriteAnim.transform.localScale *= heightMult;

        if (_heldWeapon != null)
            _heldWeapon.transform.localScale *= areaMult;
    }

    void Die()
    {
        if (_dead) return;
        _dead = true;
        OnAnyEnemyDied?.Invoke();

        _agent.isStopped = true;
        _agent.enabled   = false;

        if (_heldWeapon != null)
        {
            foreach (var r in _heldWeapon.GetComponentsInChildren<Renderer>()) r.enabled = true;
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

        if (_spriteAnim != null)
            _spriteAnim.PlayDeath();
        else
            Destroy(gameObject);
    }
}
