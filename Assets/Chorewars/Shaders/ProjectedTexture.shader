Shader "Chorewars/ProjectedTexture"
{
    Properties
    {
        _MainTex ("Projected Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
        LOD 100

        Pass
        {
            ZWrite On
            Cull Back

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4x4 _ProjectVP;

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float4 proj : TEXCOORD0;
            };

            v2f vert (appdata v)
            {
                v2f o;
                float4 world = mul(unity_ObjectToWorld, v.vertex);
                o.pos = mul(UNITY_MATRIX_VP, world);
                o.proj = mul(_ProjectVP, world);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float w = max(1e-6, i.proj.w);
                float2 ndc = i.proj.xy / w;
                float2 uv = ndc * 0.5 + 0.5;

                // Outside the captured frustum: fall back to a dark neutral.
                if (uv.x < 0 || uv.x > 1 || uv.y < 0 || uv.y > 1)
                    return fixed4(0.12, 0.12, 0.12, 1);

                fixed4 c = tex2D(_MainTex, uv);
                return c;
            }
            ENDHLSL
        }
    }
}

