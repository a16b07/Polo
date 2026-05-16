Shader "Custom/Bitcrush"
{
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        ZWrite Off ZTest Always Blend Off Cull Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            float _PixelSize;
            float _ColorBits;

            half4 Frag(Varyings i) : SV_Target
            {
                float2 res = _ScreenParams.xy / _PixelSize;
                float2 uv = floor(i.texcoord * res) / res;
                half3 col = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv).rgb;
                float levels = pow(2.0, _ColorBits) - 1.0;
                col = floor(col * levels + 0.5) / levels;
                return half4(col, 1.0);
            }
            ENDHLSL
        }
    }
}
