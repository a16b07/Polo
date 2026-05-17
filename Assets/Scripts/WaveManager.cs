using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

public class WaveManager : MonoBehaviour
{
    public static WaveManager Instance { get; private set; }

    struct EnemySpawn { public Vector3 pos; public Quaternion rot; }

    readonly List<EnemySpawn>   _enemySpawns  = new List<EnemySpawn>();
    readonly List<RewardSphere> _spheres       = new List<RewardSphere>();

    int   _aliveCount;
    bool  _waveClear;
    public bool WaveClear => _waveClear;

    void Awake() => Instance = this;

    void Start()
    {
        // Record all enemies
        foreach (var e in FindObjectsByType<EnemyAI>(FindObjectsSortMode.None))
            _enemySpawns.Add(new EnemySpawn { pos = e.transform.position, rot = e.transform.rotation });

        // Record all spheres
        foreach (var s in FindObjectsByType<RewardSphere>(FindObjectsSortMode.None))
            _spheres.Add(s);

        _aliveCount = _enemySpawns.Count;
        EnemyAI.OnAnyEnemyDied += OnEnemyDied;
    }

    void OnDestroy() => EnemyAI.OnAnyEnemyDied -= OnEnemyDied;

    void OnEnemyDied()
    {
        _aliveCount = Mathf.Max(0, _aliveCount - 1);
        if (_aliveCount == 0) _waveClear = true;
    }

    public void ForceWaveClear() { _waveClear = true; _aliveCount = 0; }

    public void StartNextWave()
    {
        _waveClear  = false;
        _aliveCount = _enemySpawns.Count;

        // Respawn enemies
        foreach (var sd in _enemySpawns)
        {
            var go = new GameObject("Enemy");
            go.transform.position = sd.pos;
            go.transform.rotation = sd.rot;
            var cc    = go.AddComponent<CharacterController>();
            cc.height = 2.4f;
            cc.center = new Vector3(0f, 1.2f, 0f);
            cc.radius = 0.35f;
            var ai = go.AddComponent<EnemyAI>();
            ai.applyEntranceResize = InteractiveEntrance.WasActivated;
        }

        // Reset spheres
        foreach (var s in _spheres)
            if (s != null) s.ResetSphere();

        // TP player back to combat area
        var dest   = GameObject.Find("TPDestination");
        var player = GameObject.FindGameObjectWithTag("Player");
        if (dest != null && player != null)
        {
            var cc = player.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;
            player.transform.position = dest.transform.position + Vector3.up * 0.5f;
            if (cc != null) cc.enabled = true;
        }
    }

}
