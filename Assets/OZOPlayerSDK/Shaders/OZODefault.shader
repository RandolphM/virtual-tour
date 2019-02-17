//Copyright © 2017 Nokia Corporation and/or its subsidiary(-ies). All rights reserved.
Shader "OZO/OZODefault"
{
    Properties
    {
        _MainTex("Video Texture", 2D) = "white" {}
    }

    SubShader
    {
        //Tags { "RenderType" = "Opaque" } //use this if your content does not have alpha
        Tags{ "Queue" = "Transparent" "RenderType" = "Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha

        LOD 100
        ZWrite On

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv0 : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            struct frag_out
            {
                fixed4 col : SV_Target;
            };

            sampler2D _MainTex;
            float4  _MainTex_ST;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv0 = TRANSFORM_TEX(v.uv, _MainTex);
#if UNITY_UV_STARTS_AT_TOP
                o.uv0.y = 1 - o.uv0.y;
#endif
                return o;
            }

            frag_out frag(v2f i)
            {
                frag_out o;
                o.col = tex2D(_MainTex, i.uv0);
                return o;
            }
            ENDCG
        }
    }
}
