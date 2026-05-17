using UnityEngine;

[RequireComponent(typeof(Light))]
public class LightBlinker : MonoBehaviour
{
    [Header("Blink")]
    public float bpm           = 165f;
    public float maxIntensity  = 10f;

    [Header("Color")]
    public float colorCycleSpeed = 0.04f;  // hue units per second (slow drift)
    [Range(0f, 1f)]
    public float hueOffset = 0f;           // stagger per object so they differ
    public bool keepColor = false;

    Light        _light;
    MeshRenderer _renderer;
    Material     _mat;
    Color        _originalColor;

    void Start()
    {
        _light = GetComponent<Light>();
        _originalColor = _light.color;
        _light.range  = 8f;
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
    }

    void Update()
    {
        float freq  = bpm / 60f;
        float pulse = Mathf.Pow(Mathf.Abs(Mathf.Sin(Time.time * Mathf.PI * freq)), 6f);

        Color col;
        if (keepColor)
        {
            col = _originalColor;
        }
        else
        {
            float hue = (hueOffset + Time.time * colorCycleSpeed) % 1f;
            col = Color.HSVToRGB(hue, 1f, 1f);
        }

        _light.color     = col;
        _light.intensity = pulse * maxIntensity;

        if (_mat != null)
            _mat.SetColor("_EmissionColor", col * (pulse * 6f));
    }
}
