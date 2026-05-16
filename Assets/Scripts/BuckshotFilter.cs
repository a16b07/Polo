using UnityEngine;

[ExecuteAlways, RequireComponent(typeof(Camera))]
public class BuckshotFilter : MonoBehaviour
{
    [Range(0.5f,2f)] public float exposure         = 1f;
    [Range(0.5f,2f)] public float contrast         = 1.05f;
    [Range(0f,2f)]   public float saturation       = 0.9f;
    public bool enablePixelate                     = true;
    [Range(0.5f,6f)] public float pixelFactor      = 2f;
    public bool enablePosterize                    = true;
    [Range(2f,16f)]  public float colorLevels      = 8f;
    [Range(0f,1f)]   public float ditherStrength   = 0.35f;
    [Range(0f,2f)]   public float chromAberration  = 0.35f;
    [Range(0f,2f)]   public float vignetteStrength = 1f;
    [Range(0f,1f)]   public float grainStrength    = 0.2f;
    public Color     tintColor                     = new Color(0.95f, 1.03f, 0.9f, 1f);
    [Range(0f,1f)]   public float tintStrength     = 0.15f;

    Material _mat;

    void OnEnable()
    {
        var s = Shader.Find("Custom/BuckshotRouletteFilter");
        if (s) _mat = new Material(s);
        else Debug.LogError("BuckshotRouletteFilter shader not found");
    }

    void OnDisable() { if (_mat) DestroyImmediate(_mat); _mat = null; }

    void OnRenderImage(RenderTexture src, RenderTexture dst)
    {
        if (!_mat) { Graphics.Blit(src, dst); return; }
        _mat.SetFloat("_Exposure",        exposure);
        _mat.SetFloat("_Contrast",        contrast);
        _mat.SetFloat("_Saturation",      saturation);
        _mat.SetFloat("_EnablePixelate",  enablePixelate  ? 1f : 0f);
        _mat.SetFloat("_PixelFactor",     pixelFactor);
        _mat.SetFloat("_EnablePosterize", enablePosterize ? 1f : 0f);
        _mat.SetFloat("_ColorLevels",     colorLevels);
        _mat.SetFloat("_DitherStrength",  ditherStrength);
        _mat.SetFloat("_ChromAberration", chromAberration);
        _mat.SetFloat("_VignetteStrength",vignetteStrength);
        _mat.SetFloat("_GrainStrength",   grainStrength);
        _mat.SetVector("_TintColor",      (Vector4)tintColor);
        _mat.SetFloat("_TintStrength",    tintStrength);
        Graphics.Blit(src, dst, _mat);
    }
}
