// Upgrade NOTE: upgraded instancing buffer 'Props' to new syntax.

Shader "Custom/HSVTextureVariation" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		
		_Cutoff("Alpha Cutoff", Range(0.0, 1.0)) = 0.5

		_Glossiness ("Smoothness", Range(0,1)) = 0.0
		_Metallic ("Metallic", Range(0,1)) = 0.0
		
		_BumpScale("Scale", Float) = 1.0
        _BumpMap("Normal Map", 2D) = "bump" {}

        _EmissionColor("Color", Color) = (0,0,0)
        
		_AlphaMask("Alpha Mask", 2D) = "white" {}

		//_HSVoffset("HSV Offset", Color) = (0.5,0.5,0.5)
		_HSVoffsetMap("HSV Offset", 2D) = "gray" {}

		_DoVariation("Do Variation", Range(0.0, 1.0)) = 1.0
		
		// Blending state
        [HideInInspector] _Mode ("__mode", Float) = 0.0
        
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard fullforwardshadows

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		sampler2D _MainTex;
		sampler2D _BumpMap;
		sampler2D _AlphaMask;
		sampler2D _HSVoffsetMap;

		struct Input {
			float2 uv_MainTex;
		};

		half _Glossiness;
		half _Metallic;
		fixed4 _Color;
		
		float _DoVariation;
	
		// Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
		// See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
		// #pragma instancing_options assumeuniformscaling
		UNITY_INSTANCING_BUFFER_START(Props)
			// put more per-instance properties here
		UNITY_INSTANCING_BUFFER_END(Props)

		float3 rgb2hsv(float3 c)
		{
			float4 K = float4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
			float4 p = c.g < c.b ? float4(c.bg, K.wz) : float4(c.gb, K.xy);
			float4 q = c.r < p.x ? float4(p.xyw, c.r) : float4(c.r, p.yzx);

			float d = q.x - min(q.w, q.y);
			float e = 1.0e-10;
			return float3(abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
		}

		float3 hsv2rgb(float3 c)
		{
			float4 K = float4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
			float3 p = abs(frac(c.xxx + K.xyz) * 6.0 - K.www);
			return c.z * lerp(K.xxx, saturate(p - K.xxx), c.y);
		}

		void surf(Input IN, inout SurfaceOutputStandard o) {
			// Albedo comes from a texture tinted by color
			fixed4 c = tex2D(_MainTex, IN.uv_MainTex);
			
			if (_DoVariation > 0.0)
			{
				float channelVariationID = tex2D(_AlphaMask, IN.uv_MainTex).x;
				float2 variationTexCoord = float2(channelVariationID, 0.0);

				fixed3 _HSVoffset = tex2D(_HSVoffsetMap, variationTexCoord);
				_HSVoffset = (_HSVoffset * 2.0 - 1.0);
				_HSVoffset.x = _HSVoffset.x * 0.5;
				
				fixed3 hsv = rgb2hsv(c.rgb) +_HSVoffset;
				if (hsv.r > 1.0)
					hsv.r -= 1.0;
				if (hsv.r < 0)
					hsv.r += 1.0;
				hsv.g = clamp(hsv.g, 0.0, 1.0);
				hsv.b = clamp(hsv.b, 0.0, 1.0);
				c.rgb = hsv2rgb(hsv);
			}

			o.Albedo = c * _Color;

			// Metallic and smoothness come from slider variables
			o.Normal = UnpackNormal(tex2D(_BumpMap, IN.uv_MainTex));
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			o.Alpha = c.a;
		}

		ENDCG
	}
	FallBack "Diffuse"
    CustomEditor "HSVTextureVariationGUI"
}
