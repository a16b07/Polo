Shader "Custom/BuckshotRouletteFilter"
{
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
        LOD 100
        ZWrite Off ZTest Always Blend Off Cull Off

        // Pass 0 – full filter
        Pass
        {
            Name "BuckshotFilter"
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            float  _Exposure; float _Contrast; float _Saturation;
            float  _EnablePixelate; float _PixelFactor;
            float  _EnablePosterize; float _ColorLevels; float _DitherStrength;
            float  _ChromAberration; float _VignetteStrength; float _GrainStrength;
            float4 _TintColor; float _TintStrength;

            float BayerDither(float2 uv, float2 res)
            {
                int2 p = int2(fmod(uv * res, 4.0));
                int idx = p.y * 4 + p.x;
                float bayer[16] = { 0,8,2,10, 12,4,14,6, 3,11,1,9, 15,7,13,5 };
                return bayer[idx] / 16.0 - 0.5;
            }
            float Hash(float2 p, float t) { return frac(sin(dot(p + frac(t), float2(127.1,311.7))) * 43758.5453); }
            float3 Sat(float3 c, float s) { float l = dot(c, float3(0.299,0.587,0.114)); return lerp(l.xxx, c, s); }

            half4 Frag(Varyings i) : SV_Target
            {
                float2 uv = i.texcoord; float2 res = _ScreenParams.xy;
                float2 sUV = uv;
                if (_EnablePixelate > 0.5) { float2 ps = _PixelFactor / res; sUV = floor(uv/ps)*ps + ps*0.5; }
                float2 off = sUV - 0.5; float dist = length(off);
                float2 dir = dist > 0.0001 ? normalize(off) : float2(0,0);
                float  amt = _ChromAberration * 0.006 * dist;
                float r = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, sUV + dir*amt).r;
                float g = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, sUV).g;
                float b = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, sUV - dir*amt).b;
                float3 col = float3(r,g,b) * _Exposure;
                col = (col - 0.5) * _Contrast + 0.5;
                col = Sat(col, _Saturation);
                if (_EnablePosterize > 0.5) { float d = BayerDither(uv,res)*_DitherStrength/max(_ColorLevels,1); col = floor((col+d)*_ColorLevels+0.5)/_ColorLevels; }
                float2 vUV = uv - 0.5; col *= saturate(1.0 - dot(vUV,vUV)*_VignetteStrength*0.8);
                col += (Hash(uv,_Time.y) - 0.5) * _GrainStrength;
                col = lerp(col, col * _TintColor.rgb, _TintStrength);
                return half4(saturate(col), 1.0);
            }
            ENDHLSL
        }

        // Pass 1 – plain copy (used for copy-back step)
        Pass
        {
            Name "BuckshotCopy"
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment FragCopy
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
            half4 FragCopy(Varyings i) : SV_Target
            {
                return SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, i.texcoord);
            }
            ENDHLSL
        }
    }
}
