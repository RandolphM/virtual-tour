Shader "Custom/ZED RGB" {
	Properties {
		_MainTex ("Albedo (RGB)", 2D) = "white" {}	
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		Lighting Off
		Pass {
		CGPROGRAM
#include "UnityCG.cginc"
#pragma vertex vert
#pragma fragment frag

		
		#pragma target 3.0

		sampler2D _MainTex;
		float4 _MainTex_ST;
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

		v2f vert(appdata v)
		{
			v2f o;
			o.vertex = UnityObjectToClipPos(v.vertex);
			o.uv = TRANSFORM_TEX(v.uv, _MainTex);
			return o;
		}

		fixed4 frag(v2f IN) : SV_TARGET{
			fixed4 c = tex2D(_MainTex, IN.uv*fixed2(1,-1)).bgra;
			return c;
		}
		ENDCG
		}
	}
	FallBack "Diffuse"
}
