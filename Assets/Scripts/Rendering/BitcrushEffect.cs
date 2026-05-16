using UnityEngine;

[ExecuteAlways, RequireComponent(typeof(Camera))]
public class BitcrushEffect : MonoBehaviour
{
    [Range(1f, 8f)] public float pixelSize = 3f;
    [Range(2f, 8f)] public float colorBits = 5f;

    Material _mat;

    void OnEnable()
    {
        var s = Shader.Find("Custom/Bitcrush");
        if (s) _mat = new Material(s);
        else Debug.LogError("Bitcrush shader not found");
    }

    void OnDisable() { if (_mat) DestroyImmediate(_mat); _mat = null; }

    void OnRenderImage(RenderTexture src, RenderTexture dst)
    {
        if (!_mat) { Graphics.Blit(src, dst); return; }
        _mat.SetFloat("_PixelSize", pixelSize);
        _mat.SetFloat("_ColorBits", colorBits);
        Graphics.Blit(src, dst, _mat);
    }
}
