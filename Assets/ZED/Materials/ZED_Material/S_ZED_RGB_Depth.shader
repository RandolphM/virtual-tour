Shader "Custom/ZED RGB-Depth"
{
	Properties
	{
		[HideInInspector] _MainTex("Base (RGB) Trans (A)", 2D) = "" {}
	_DepthXYZTex("Depth texture", 2D) = "" {}
	_CameraTex("Texture from ZED", 2D) = "" {}
	_Threshold("Threshold", Range(0, 1)) = 0.0
		_GreenLimit("GreenLimit", Range(0, 1)) = 0.0
	}

		SubShader
	{
		// No culling or depth
		ZWrite On
		//ZTest Always
		//Blend OneMinusSrcAlpha SrcAlpha// Alpha blending
		//Blend One One // Additive
		//Blend OneMinusDstColor One // Soft Additive
		//Blend DstColor Zero // Multiplicative
		//Blend DstColor SrcColor // 2x Multiplicative

		Tags{
		"Queue" = "Background"
	}

		Pass
	{
		CGPROGRAM
#pragma vertex vert
#pragma fragment frag

#include "UnityCG.cginc"
#include "S_ZED_Utils.cginc"
		struct appdata
	{
		float4 vertex : POSITION;
		float2 uv : TEXCOORD0;
	};

	struct Input {
		float2 uv_MainTex;
	};

	struct v2f
	{
		float2 uv : TEXCOORD0;
		float4 vertex : SV_POSITION;

	};

	struct fragOut {

		float depth : SV_Depth;
		fixed4 color : SV_Target;
	};

	uniform float light_x;
	uniform float light_y;
	uniform float light_z;
	uniform float time_step;
	uniform float A_GREEN_WEIGHT;
	uniform float B_GREEN_WEIGHT;
	uniform float GREEN_THRESH;
	uniform float human_scale;
	uniform float4x4 _ProjectionMatrix;
	sampler2D _DepthXYZTex;
	sampler2D _CameraTex;
	uniform float4 _MaskTex_ST;
	float4 _MainTex_ST;
	uniform int _isGrey;

	v2f vert(appdata v)
	{
		v2f o;
		o.vertex = mul(mul(mul(_ProjectionMatrix, UNITY_MATRIX_V), UNITY_MATRIX_M), v.vertex);
		o.uv = TRANSFORM_TEX(v.uv, _MainTex);
		return o;
	}


	uniform float _Threshold;
	uniform float _GreenLimit;


	float4 GetPosition(float3 colorXYZ)
	{
		//colorXYZ.b = -colorXYZ.b;
		//colorXYZ.g = -colorXYZ.g;

		//If the depth is inf or nan
		/*if (colorXYZ.b != colorXYZ.b) return 0.999f;
		if (isinf(colorXYZ.b)) {
			if (colorXYZ.b > 0)
				return 0.01f;
			else
				return 0.999f;
		}*/

		float4 v = float4(colorXYZ, 1);

		return v;
	}

	

	fragOut frag(v2f i)
	{

		//Get the depth in XYZ format
		
		//Compute the depth to work with Unity (1m real = 1m to Unity)
		float2 centerUV = (i.uv*fixed2(1, -1));
		
		float3 colorXYZ = tex2D(_DepthXYZTex, centerUV).rgb;
		float depthReal = computeDepthXYZ(colorXYZ, human_scale);

		//Color from the camera
		fragOut o = (fragOut)0;

		float color_count = 1.0f;

		float3 colorCamera = tex2D(_CameraTex, i.uv*fixed2(1, -1)).bgr;

		o.color.rgb = colorCamera.rgb;

		o.depth = depthReal;

		/*Formula 1*/
		//float A = 1.0;
		//float B = 1.0;
		float res = A_GREEN_WEIGHT*(colorCamera.b + colorCamera.r) - B_GREEN_WEIGHT*(colorCamera.g);

		float res2;
		if (res < GREEN_THRESH)
			discard;

		/*Formuala 2*/
		/*float _Threshold = 0.12;
		fixed4 col1 = fixed4(colorCamera, 1.0);
		fixed4 val = ceil(saturate(col1.g - col1.r - _Threshold)) * ceil(saturate(col1.g - col1.b - _Threshold));
		float4 alf = lerp(col1, fixed4(0., 0., 0., 0.), val);
		if (alf.a < 0.01)
		discard;*/

		//if (_GreenLimit < 0.1)
		//	discard;

		/*Aslong as we are computing a color*/

		/*COMPUTE NORMALS*/
		//reverse Y and Z axes

		float4 center = GetPosition(colorXYZ);
		float u_ratio = 1.0f / 1920.0f;
		float v_ratio = 1.0f / 1080.0f;
		float3 normal = float3(0, 0, 0);

		float3 colors[9] = { float3(0,0,0), float3(0,0,0), float3(0,0,0),
							 float3(0,0,0), float3(0,0,0), float3(0,0,0),
							 float3(0,0,0), float3(0,0,0), float3(0,0,0) };

		colorCamera = tex2D(_CameraTex, (i.uv*fixed2(1, -1))).bgr;/*center position*/
		colors[0].rgb = colorCamera.rgb;
		colorCamera = tex2D(_CameraTex, ((i.uv + float2(0, v_ratio))*fixed2(1, -1))).bgr;/*Top*/
		colors[1].rgb = colorCamera.rgb;
		colorCamera = tex2D(_CameraTex, ((i.uv + float2(u_ratio, v_ratio))*fixed2(1, -1))).bgr;/*Top Right*/
		colors[2].rgb = colorCamera.rgb;
		colorCamera = tex2D(_CameraTex, ((i.uv + float2(u_ratio, 0))*fixed2(1, -1))).bgr;/*Right*/
		colors[3].rgb = colorCamera.rgb;
		colorCamera = tex2D(_CameraTex, ((i.uv + float2(u_ratio, -v_ratio))*fixed2(1, -1))).bgr;/*Bottom Right*/
		colors[4].rgb = colorCamera.rgb;
		colorCamera = tex2D(_CameraTex, ((i.uv + float2(0, -v_ratio))*fixed2(1, -1))).rgb;/*Bottom*/
		colors[5].rgb = colorCamera.rgb;
		colorCamera = tex2D(_CameraTex, ((i.uv + float2(-u_ratio, -v_ratio))*fixed2(1, -1))).bgr;/*Bottom Left*/
		colors[6].rgb = colorCamera.rgb;
		colorCamera = tex2D(_CameraTex, ((i.uv + float2(-u_ratio, 0))*fixed2(1, -1))).bgr;/*Left*/
		colors[7].rgb = colorCamera.rgb;
		colorCamera = tex2D(_CameraTex, ((i.uv + float2(-u_ratio, v_ratio))*fixed2(1, -1))).bgr;/*Top left*/
		colors[8].rgb = colorCamera.rgb;

		float3 c[9] = { float3(0,0,0), float3(0,0,0), float3(0,0,0),
						float3(0,0,0), float3(0,0,0), float3(0,0,0),
						float3(0,0,0), float3(0,0,0), float3(0,0,0) };

		float3 totop;
		float3 toright;
		float3 cr;

		c[0] = GetPosition(tex2D(_DepthXYZTex, (i.uv*fixed2(1, -1))).rgb).xyz;/*center position*/
		c[1] = GetPosition(tex2D(_DepthXYZTex, ((i.uv + float2(0, v_ratio))*fixed2(1, -1))).rgb).xyz;/*Top*/
		c[2] = GetPosition(tex2D(_DepthXYZTex, ((i.uv + float2(u_ratio, v_ratio))*fixed2(1, -1))).rgb).xyz;/*Top Right*/
		c[3] = GetPosition(tex2D(_DepthXYZTex, ((i.uv + float2(u_ratio, 0))*fixed2(1, -1))).rgb).xyz;/*Right*/
		c[4] = GetPosition(tex2D(_DepthXYZTex, ((i.uv + float2(u_ratio, -v_ratio))*fixed2(1, -1))).rgb).xyz;/*Bottom Right*/
		c[5] = GetPosition(tex2D(_DepthXYZTex, ((i.uv + float2(0, -v_ratio))*fixed2(1, -1))).rgb).xyz;/*Bottom*/
		c[6] = GetPosition(tex2D(_DepthXYZTex, ((i.uv + float2(-u_ratio, -v_ratio))*fixed2(1, -1))).rgb).xyz;/*Bottom Left*/
		c[7] = GetPosition(tex2D(_DepthXYZTex, ((i.uv + float2(-u_ratio, 0))*fixed2(1, -1))).rgb).xyz;/*Left*/
		c[8] = GetPosition(tex2D(_DepthXYZTex, ((i.uv + float2(-u_ratio, v_ratio))*fixed2(1, -1))).rgb).xyz;/*Top left*/

		res = A_GREEN_WEIGHT*(colors[1].b + colors[1].r) - B_GREEN_WEIGHT*(colors[1].g);
		res2 = A_GREEN_WEIGHT*(colors[2].b + colors[2].r) - B_GREEN_WEIGHT*(colors[2].g);

		if (res >= GREEN_THRESH)
		{
			o.color.rgb += colors[1].rgb;
			color_count += 1.0f;
		}
			

		if (res2 >= GREEN_THRESH)
		{
			o.color.rgb += colors[2].rgb;
			color_count += 1.0f;
		}

		if (res >= GREEN_THRESH && res2 >= GREEN_THRESH)
		{
			totop = c[1] - c[0];
			toright = c[2] - c[0];
			cr = cross(toright, totop);
			normal += normalize(cr);
		}

		res = A_GREEN_WEIGHT*(colors[3].b + colors[3].r) - B_GREEN_WEIGHT*(colors[3].g);

		if (res >= GREEN_THRESH)
		{
			o.color.rgb += colors[3].rgb;
			color_count += 1.0f;
		}

		if (res >= GREEN_THRESH && res2 >= GREEN_THRESH)
		{
			totop = c[2] - c[0];
			toright = c[3] - c[0];
			cr = cross(toright, totop);
			normal += normalize(cr);
		}

		res2 = A_GREEN_WEIGHT*(colors[4].b + colors[4].r) - B_GREEN_WEIGHT*(colors[4].g);

		if (res2 >= GREEN_THRESH)
		{
			o.color.rgb += colors[4].rgb;
			color_count += 1.0f;
		}

		if (res >= GREEN_THRESH && res2 >= GREEN_THRESH)
		{
			totop = c[3] - c[0];
			toright = c[4] - c[0];
			cr = cross(toright, totop);
			normal += normalize(cr);
		}

		res = A_GREEN_WEIGHT*(colors[5].b + colors[5].r) - B_GREEN_WEIGHT*(colors[5].g);

		if (res >= GREEN_THRESH)
		{
			o.color.rgb += colors[5].rgb;
			color_count += 1.0f;
		}

		if (res >= GREEN_THRESH && res2 >= GREEN_THRESH)
		{
			totop = c[4] - c[0];
			toright = c[5] - c[0];
			cr = cross(toright, totop);
			normal += normalize(cr);
		}

		res2 = A_GREEN_WEIGHT*(colors[6].b + colors[6].r) - B_GREEN_WEIGHT*(colors[6].g);

		if (res2 >= GREEN_THRESH)
		{
			o.color.rgb += colors[6].rgb;
			color_count += 1.0f;
		}

		if (res >= GREEN_THRESH && res2 >= GREEN_THRESH)
		{
			totop = c[5] - c[0];
			toright = c[6] - c[0];
			cr = cross(toright, totop);
			normal += normalize(cr);
		}

		res = A_GREEN_WEIGHT*(colors[7].b + colors[7].r) - B_GREEN_WEIGHT*(colors[7].g);

		if (res >= GREEN_THRESH)
		{
			o.color.rgb += colors[7].rgb;
			color_count += 1.0f;
		}

		if (res >= GREEN_THRESH && res2 >= GREEN_THRESH)
		{
			totop = c[6] - c[0];
			toright = c[7] - c[0];
			cr = cross(toright, totop);
			normal += normalize(cr);
		}

		res2 = A_GREEN_WEIGHT*(colors[8].b + colors[8].r) - B_GREEN_WEIGHT*(colors[8].g);

		if (res2 >= GREEN_THRESH)
		{
			o.color.rgb += colors[8].rgb;
			color_count += 1.0f;
		}

		if (color_count < 8.5f)
			o.color.rgb /= color_count;
		else
			o.color.rgb = colors[0].rgb;

		if (res >= GREEN_THRESH && res2 >= GREEN_THRESH)
		{
			totop = c[7] - c[0];
			toright = c[8] - c[0];
			cr = cross(toright, totop);
			normal += normalize(cr);
		}

		res = A_GREEN_WEIGHT*(colors[1].b + colors[1].r) - B_GREEN_WEIGHT*(colors[1].g);

		if (res >= GREEN_THRESH && res2 >= GREEN_THRESH)
		{
			totop = c[8] - c[0];
			toright = c[1] - c[0];
			cr = cross(toright, totop);
			normal += normalize(cr);
		}

		normal = normalize(normal);

		float angle = acos(dot(float3(0, 0, 1), normal));

		

		if ( angle > 3.14f * ( 75.0f / 180.0f )/* && o.color.g > 0.75f*/ )
		{
			//float change = angle - 3.14f*0.45f;
			//o.color.rgb = float3(1,0,0);// *(change / (3.14f*0.5f - 3.14f*0.45f));

			o.color.g *= 0.9f;
			//return o;
		}

		float3 light_dir = float3(0, cos(time_step), sin(time_step));
		float3 light_pos = float3(light_x, light_y, light_z);
		light_dir = light_pos - c[0];
		float d = length(light_dir);
		light_dir = normalize(light_dir);
		float mag = saturate(dot(light_dir, -normal));

		float atten = (1.0f - saturate(d / 1.0f));
		o.color.rgb = saturate(o.color.rgb*0.5f + 0.5f*saturate(o.color.rgb)*float3(1.0f,0.5f,0.25f)*atten*mag);

		return o;
	}
	ENDCG
	}
	}
}

