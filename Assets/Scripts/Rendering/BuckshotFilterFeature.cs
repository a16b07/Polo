using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;

public class BuckshotFilterFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class Settings
    {
        public RenderPassEvent renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
        [Range(0.5f,2f)] public float exposure = 1f;
        [Range(0.5f,2f)] public float contrast = 1.05f;
        [Range(0f,2f)]   public float saturation = 0.9f;
        public bool enablePixelate = true;
        [Range(0.5f,6f)] public float pixelFactor = 2f;
        public bool enablePosterize = true;
        [Range(2f,16f)]  public float colorLevels = 8f;
        [Range(0f,1f)]   public float ditherStrength = 0.35f;
        [Range(0f,2f)]   public float chromAberration = 0.35f;
        [Range(0f,2f)]   public float vignetteStrength = 1f;
        [Range(0f,1f)]   public float grainStrength = 0.2f;
        public Color tintColor = new Color(0.95f, 1.03f, 0.9f, 1f);
        [Range(0f,1f)]   public float tintStrength = 0.15f;
        [Range(0.1f,3f)] public float gamma = 1f;
    }

    public Settings settings = new Settings();
    BuckshotPass _pass;
    Material _mat;

    public override void Create()
    {
        var s = Shader.Find("Custom/BuckshotRouletteFilter");
        if (s == null) { Debug.LogError("BuckshotRouletteFilter shader not found!"); return; }
        _mat = CoreUtils.CreateEngineMaterial(s);
        _pass = new BuckshotPass(_mat);
        _pass.renderPassEvent = settings.renderPassEvent;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (_mat == null || _pass == null) return;
        _pass.Setup(settings);
        renderer.EnqueuePass(_pass);
    }

    protected override void Dispose(bool disposing) => CoreUtils.Destroy(_mat);

    class BuckshotPass : ScriptableRenderPass
    {
        readonly Material _mat;
        Settings _s;

        static readonly int ID_Tex            = Shader.PropertyToID("_BlitTexture");
        static readonly int ID_Exposure       = Shader.PropertyToID("_Exposure");
        static readonly int ID_Contrast       = Shader.PropertyToID("_Contrast");
        static readonly int ID_Saturation     = Shader.PropertyToID("_Saturation");
        static readonly int ID_Pixelate       = Shader.PropertyToID("_EnablePixelate");
        static readonly int ID_PixelFactor    = Shader.PropertyToID("_PixelFactor");
        static readonly int ID_Posterize      = Shader.PropertyToID("_EnablePosterize");
        static readonly int ID_ColorLevels    = Shader.PropertyToID("_ColorLevels");
        static readonly int ID_Dither         = Shader.PropertyToID("_DitherStrength");
        static readonly int ID_Chrom          = Shader.PropertyToID("_ChromAberration");
        static readonly int ID_Vignette       = Shader.PropertyToID("_VignetteStrength");
        static readonly int ID_Grain          = Shader.PropertyToID("_GrainStrength");
        static readonly int ID_TintColor      = Shader.PropertyToID("_TintColor");
        static readonly int ID_TintStrength   = Shader.PropertyToID("_TintStrength");
        static readonly int ID_Gamma          = Shader.PropertyToID("_Gamma");

        class PassData { public TextureHandle src; public TextureHandle dst; public Material mat; }

        public BuckshotPass(Material mat) => _mat = mat;
        public void Setup(Settings s) => _s = s;

        void SetProps()
        {
            _mat.SetFloat(ID_Exposure,    _s.exposure);
            _mat.SetFloat(ID_Contrast,    _s.contrast);
            _mat.SetFloat(ID_Saturation,  _s.saturation);
            _mat.SetFloat(ID_Pixelate,    _s.enablePixelate  ? 1f : 0f);
            _mat.SetFloat(ID_PixelFactor, _s.pixelFactor);
            _mat.SetFloat(ID_Posterize,   _s.enablePosterize ? 1f : 0f);
            _mat.SetFloat(ID_ColorLevels, _s.colorLevels);
            _mat.SetFloat(ID_Dither,      _s.ditherStrength);
            _mat.SetFloat(ID_Chrom,       _s.chromAberration);
            _mat.SetFloat(ID_Vignette,    _s.vignetteStrength);
            _mat.SetFloat(ID_Grain,       _s.grainStrength);
            _mat.SetVector(ID_TintColor,  (Vector4)_s.tintColor);
            _mat.SetFloat(ID_TintStrength,_s.tintStrength);
            _mat.SetFloat(ID_Gamma,       _s.gamma);
        }

public override void RecordRenderGraph(RenderGraph rg, ContextContainer frameData)
{
    var res = frameData.Get<UniversalResourceData>();
    var cam = frameData.Get<UniversalCameraData>();
    if (cam.cameraType == CameraType.Preview) return;

    SetProps();
    TextureHandle src = res.activeColorTexture;
    var desc = cam.cameraTargetDescriptor;
    desc.depthBufferBits = 0;
    TextureHandle temp = UniversalRenderer.CreateRenderGraphTexture(rg, desc, "_BuckshotTemp", false, FilterMode.Bilinear);

    using (var b = rg.AddUnsafePass<PassData>("BuckshotFilter", out var d))
    {
        d.src = src; d.dst = temp; d.mat = _mat;
        b.UseTexture(src, AccessFlags.Read);
        b.UseTexture(temp, AccessFlags.Write);
        b.SetRenderFunc((PassData data, UnsafeGraphContext ctx) =>
        {
            var cmd = CommandBufferHelpers.GetNativeCommandBuffer(ctx.cmd);
            cmd.SetRenderTarget(data.dst, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store);
            cmd.SetGlobalTexture(ID_Tex, data.src);
            Blitter.BlitTexture(cmd, new Vector4(1,1,0,0), data.mat, 0);
        });
    }

    res.cameraColor = temp;
}
    }
}
