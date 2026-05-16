using UnityEngine;

[RequireComponent(typeof(Light))]
public class DiscoController : MonoBehaviour
{
    [Header("Color")]
    public float colorCycleSpeed = 0.08f;

    [Header("Strobe")]
    public float bpm = 165f;
    public float maxIntensity = 10f;
    public float lightRange = 40f;

    Light _light;
    MeshRenderer _renderer;
    Material _mat;
    float _hue;

    void Start()
    {
        _light = GetComponent<Light>();
        _light.range   = lightRange;
        _light.shadows = LightShadows.None;

        _renderer = GetComponent<MeshRenderer>();
        if (_renderer != null)
        {
            _mat = new Material(_renderer.sharedMaterial != null
                ? _renderer.sharedMaterial
                : new Material(Shader.Find("Universal Render Pipeline/Lit")));
            _mat.EnableKeyword("_EMISSION");
            _renderer.material = _mat;
        }

        // Disable LightBlinker so they don't fight each other
        var blinker = GetComponent<LightBlinker>();
        if (blinker != null) blinker.enabled = false;
    }

    void Update()
    {
        _hue = (_hue + colorCycleSpeed * Time.deltaTime) % 1f;

        float freq  = bpm / 60f;
        float pulse = Mathf.Pow(Mathf.Abs(Mathf.Sin(Time.time * Mathf.PI * freq)), 6f);

        Color col = Color.HSVToRGB(_hue, 1f, 1f);
        _light.range     = lightRange;
        _light.color     = col;
        _light.intensity = pulse * maxIntensity;

        if (_mat != null)
            _mat.SetColor("_EmissionColor", col * (pulse * 6f));
    }
}
