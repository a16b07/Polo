using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;

public class BitcrushFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class Settings
    {
        public RenderPassEvent renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
        [Range(1f, 8f)] public float pixelSize = 3f;
        [Range(2f, 8f)] public float colorBits = 5f;
    }

    public Settings settings = new Settings();
    BitcrushPass _pass;
    Material _mat;

    public override void Create()
    {
        var s = Shader.Find("Custom/Bitcrush");
        if (s == null) { Debug.LogError("Bitcrush shader not found!"); return; }
        _mat = CoreUtils.CreateEngineMaterial(s);
        _pass = new BitcrushPass(_mat);
        _pass.renderPassEvent = settings.renderPassEvent;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (_mat == null || _pass == null) return;
        _pass.Setup(settings);
        renderer.EnqueuePass(_pass);
    }

    protected override void Dispose(bool disposing) => CoreUtils.Destroy(_mat);

    class BitcrushPass : ScriptableRenderPass
    {
        readonly Material _mat;
        Settings _s;

        static readonly int ID_Tex       = Shader.PropertyToID("_BlitTexture");
        static readonly int ID_PixelSize = Shader.PropertyToID("_PixelSize");
        static readonly int ID_ColorBits = Shader.PropertyToID("_ColorBits");

        class PassData { public TextureHandle src; public TextureHandle dst; public Material mat; }

        public BitcrushPass(Material mat) => _mat = mat;
        public void Setup(Settings s) => _s = s;

        public override void RecordRenderGraph(RenderGraph rg, ContextContainer frameData)
        {
            var res = frameData.Get<UniversalResourceData>();
            var cam = frameData.Get<UniversalCameraData>();
            if (cam.cameraType == CameraType.Preview) return;

            _mat.SetFloat(ID_PixelSize, _s.pixelSize);
            _mat.SetFloat(ID_ColorBits, _s.colorBits);

            TextureHandle src = res.activeColorTexture;
            var desc = cam.cameraTargetDescriptor;
            desc.depthBufferBits = 0;
            TextureHandle temp = UniversalRenderer.CreateRenderGraphTexture(rg, desc, "_BitcrushTemp", false, FilterMode.Point);

            using (var b = rg.AddUnsafePass<PassData>("BitcrushFilter", out var d))
            {
                d.src = src; d.dst = temp; d.mat = _mat;
                b.UseTexture(src, AccessFlags.Read);
                b.UseTexture(temp, AccessFlags.Write);
                b.SetRenderFunc((PassData data, UnsafeGraphContext ctx) =>
                {
                    var cmd = CommandBufferHelpers.GetNativeCommandBuffer(ctx.cmd);
                    cmd.SetRenderTarget(data.dst, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store);
                    cmd.SetGlobalTexture(ID_Tex, data.src);
                    Blitter.BlitTexture(cmd, new Vector4(1, 1, 0, 0), data.mat, 0);
                });
            }

            using (var b = rg.AddUnsafePass<PassData>("BitcrushCopyBack", out var d))
            {
                d.src = temp; d.dst = src; d.mat = _mat;
                b.UseTexture(temp, AccessFlags.Read);
                b.UseTexture(src, AccessFlags.Write);
                b.SetRenderFunc((PassData data, UnsafeGraphContext ctx) =>
                {
                    var cmd = CommandBufferHelpers.GetNativeCommandBuffer(ctx.cmd);
                    cmd.SetRenderTarget(data.dst, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store);
                    cmd.SetGlobalTexture(ID_Tex, data.src);
                    Blitter.BlitTexture(cmd, new Vector4(1, 1, 0, 0), data.mat, 0);
                });
            }
        }
    }
}
