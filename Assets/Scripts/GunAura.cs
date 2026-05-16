using UnityEngine;

public class GunAura : MonoBehaviour
{
    public Color auraColor = Color.white;
    public float baseIntensity = 1.5f;
    public float pulseSpeed    = 2.5f;
    public float range         = 1.8f;

    Light _light;

    void Awake()
    {
        var go = new GameObject("Aura");
        go.transform.SetParent(transform);
        go.transform.localPosition = Vector3.zero;

        _light           = go.AddComponent<Light>();
        _light.type      = LightType.Point;
        _light.color     = auraColor;
        _light.intensity = baseIntensity;
        _light.range     = range;
        _light.shadows   = LightShadows.None;
    }

    void OnDestroy()
    {
        if (_light != null) Destroy(_light.gameObject);
    }

    void Update()
    {
        if (_light == null) return;
        _light.color     = auraColor;
        _light.range     = range;
        _light.intensity = baseIntensity + 0.6f * Mathf.Sin(Time.time * pulseSpeed);
    }
}
