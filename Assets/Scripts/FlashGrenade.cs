using System.Collections;
using UnityEngine;

public class FlashGrenade : MonoBehaviour
{
    public float fuseTime        = 3f;
    public float maxStunRange    = 66f;
    public float maxStunDuration = 6f;

    public static event System.Action OnFlashExploded;

    Camera _cam;

    void Start()
    {
        _cam = Camera.main;
        StartCoroutine(FuseCoroutine());
    }

    IEnumerator FuseCoroutine()
    {
        yield return new WaitForSeconds(fuseTime);
        Explode();
    }

    void Explode()
    {
        foreach (var enemy in FindObjectsByType<EnemyAI>(FindObjectsSortMode.None))
        {
            float dist = Vector3.Distance(transform.position, enemy.transform.position);
            if (dist > maxStunRange) continue;
            float t = 1f - (dist / maxStunRange);
            enemy.flashStunEnd = Time.time + t * maxStunDuration;
        }

        if (PlayerCanSee())
        {
            OnFlashExploded?.Invoke();
            AudioManager.Instance?.MuffleMusic(4f);
        }

        Destroy(gameObject);
    }

    bool PlayerCanSee()
    {
        if (_cam == null) return false;
        var vp = _cam.WorldToViewportPoint(transform.position);
        if (vp.z <= 0f || vp.x < 0f || vp.x > 1f || vp.y < 0f || vp.y > 1f) return false;
        Vector3 dir = transform.position - _cam.transform.position;
        return !Physics.Raycast(_cam.transform.position, dir.normalized, dir.magnitude - 0.3f, ~(1 << 9));
    }
}
