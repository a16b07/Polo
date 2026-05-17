Shader "Custom/DoorOutline"
{
    Properties
    {
        _OutlineColor ("Color", Color) = (0.2, 0.9, 1.0, 1.0)
        _OutlineWidth ("Width", Float) = 0.018
        _Pulse        ("Pulse", Float) = 0.0
    }

    SubShader
    {
        Tags { "Queue"="Overlay+1" "RenderType"="Transparent" }

        Pass
        {
            ZTest    Always
            ZWrite   Off
            Cull     Front
            Blend    SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            #pragma vertex   vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            float4 _OutlineColor;
            float  _OutlineWidth;
            float  _Pulse;

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
            };

            v2f vert(appdata v)
            {
                v2f o;
                float4 clip = UnityObjectToClipPos(v.vertex);
                float3 cn   = mul((float3x3)UNITY_MATRIX_VP,
                                  mul((float3x3)unity_ObjectToWorld, v.normal));
                float w = _OutlineWidth + _Pulse * 0.006;
                clip.xy += normalize(cn.xy) * w;
                o.pos = clip;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                return _OutlineColor;
            }
            ENDCG
        }
    }
}
