//Copyright © 2017 Nokia Corporation and/or its subsidiary(-ies). All rights reserved.
Shader "OZO/OZOGreenScreen"
{
    Properties
    {
        _MainTex("Video Texture", 2D) = "white" {}
        _GreenBackgroundColor("Green replacement color", Color) = (0,0,0,0)
        _GreenDetectionFactor("Green detection factor", Range(0, 1)) = 0.35
        _GreenCorrectionFactor("Green correction factor", Range(0, 2)) = 0.015

        //for green only
        //_GreenDetectionFactor("Green detection factor", Range(0, 10)) = 5
        //_GreenCorrectionFactor("Green correction factor", Range(0, 1)) = 0.75
    }

    SubShader
    {
        Tags{ "Queue" = "Transparent" "RenderType" = "Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        LOD 100
        ZWrite Off

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
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            struct frag_out
            {
                fixed4 col : SV_Target;
                float depth : SV_Depth;
            };
                        
            sampler2D _MainTex;
            float4    _MainTex_ST;
	       
            float4 _GreenBackgroundColor;
            float _GreenDetectionFactor;
            float _GreenCorrectionFactor;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
        #if UNITY_UV_STARTS_AT_TOP
                o.uv.y = 1 - o.uv.y;
        #endif
                return o;
            }

            float compare(float3 ycbcr, float3 ycbcr_key, float min, float size)
            {
                float d = length(ycbcr.gb - ycbcr_key.gb);
                return smoothstep(min, min + size, d);
            }
            float compareDepth(float3 ycbcr, float3 ycbcr_key, float min, float size)
            {
                float d = length(ycbcr.gb - ycbcr_key.gb);
                if (d < min) return 0.0;
                else if (d < min + size) return smoothstep(min, min + size, d);
                return 1.0;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                //chroma with color pick
                float3 rgb2y = float3(0.299, 0.587, 0.114);
                float3 rgb2cb = float3(-0.168736, -0.331264, 0.5);
                float3 rgb2cr = float3(0.5, -0.418688, -0.081312);

                float3 key = _GreenBackgroundColor.rgb;
                float3 ycbcr_key;
                ycbcr_key.r = dot(rgb2y, key);
                ycbcr_key.g = dot(rgb2cb, key) + 0.5;
                ycbcr_key.b = dot(rgb2cr, key) + 0.5;

                float3 ycbcr;
                float4 fg = tex2D(_MainTex, i.uv);
                float invA = 1.0/fg.a;
                fg = float4(fg.rgb * invA, fg.a); //restore color from premultiplied alpha
                ycbcr.r = dot(rgb2y, fg.rgb);
                ycbcr.g = dot(rgb2cb, fg.rgb) + 0.5;
                ycbcr.b = dot(rgb2cr, fg.rgb) + 0.5;

                float v = compare(ycbcr, ycbcr_key, _GreenDetectionFactor, _GreenCorrectionFactor);
                return float4(lerp(fg, float4(0.0,0.0,0.0,0.0), 1.0 - v));

                ////chroma key for green only
                //float4 fg = tex2D(_MainTex, i.uv);
                //float maxrb = max(fg.r, fg.b);
                //float k = clamp((fg.g - maxrb) * _GreenDetectionFactor, 0.0, 1.0);
                //float dg = fg.g;
                //fg.g = min(fg.g, maxrb * _GreenCorrectionFactor);
                //fg.rgb += dg - fg.g;
                //return lerp(fg, _GreenBackgroundColor, k);

            }
            ENDCG
        }
    }
}
