// Upgrade NOTE: upgraded instancing buffer 'Props' to new syntax.

Shader "Custom/HSVTextureVariationLight" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_AmbientLightColor("Ambient Light Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		
		_Cutoff("Alpha Cutoff", Range(0.0, 1.0)) = 0.5

		_Glossiness ("Smoothness", Range(0,1)) = 0.0
		_Metallic ("Metallic", Range(0,1)) = 0.0
		
		_BumpScale("Scale", Float) = 1.0
        _BumpMap("Normal Map", 2D) = "bump" {}

        _EmissionColor("Color", Color) = (0,0,0)
        
		_AlphaMask("Alpha Mask", 2D) = "white" {}

		// Blending state
        [HideInInspector] _Mode ("__mode", Float) = 0.0
        
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		
		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard fullforwardshadows

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		sampler2D _MainTex;
		sampler2D _BumpMap;
		sampler2D _AlphaMask;

		struct Input {
			float2 uv_MainTex;
		};

		half _Glossiness;
		half _Metallic;
		fixed4 _Color;
		
		// Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
		// See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
		// #pragma instancing_options assumeuniformscaling
		UNITY_INSTANCING_BUFFER_START(Props)
			// put more per-instance properties here
		UNITY_INSTANCING_BUFFER_END(Props)

		void surf(Input IN, inout SurfaceOutputStandard o) {
			fixed4 c = tex2D(_MainTex, IN.uv_MainTex);

			// Albedo comes from a texture tinted by color
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
}
