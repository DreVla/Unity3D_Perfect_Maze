Shader "WetSurface/Main" {
	Properties 
	{
		_Color ("Color", Color) = (1, 1, 1, 1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		[Normal]
		_NormalTex ("Normalmap", 2D) = "bump" {}
		_NormalScale ("Normalmap Scale", Range(0, 1)) = 1
		_MetallicTex ("Metallic", 2D) = "white" {}
		[Space]
		_Metallic ("Metallic", Range(0, 1)) = 0.0
		_Smoothness ("Smoothness", Range(0, 1)) = 0.5
		[Header(Water)]
		[Space]
		_WetColor("Water Color", Color) = (1, 1, 1, 1)
		_WetMap ("Wet Map", 2D) = "white" {}
		[Space]
		_WetMetallic ("Water Metallic", Range(0, 1)) = 0.1
		_WetSmoothness ("Water Smoothness", Range(0, 1)) = 0.95
		[Header(Wetness)]
		_Wetness ("Wetness", Range(0, 1)) = 0.0
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		#pragma surface surf Standard fullforwardshadows
		#pragma target 3.0

		sampler2D _MainTex;
		sampler2D _NormalTex;
		sampler2D _MetallicTex;
		sampler2D _WetMap;

		struct Input 
		{
			float2 uv_MainTex;
			float2 uv_NormalTex;
			float2 uv_MetallicTex;
			float2 uv_WetMap;
		};

		fixed4 _Color;
		half _NormalScale;
		half _Metallic;
		half _Smoothness;
		fixed4 _WetColor;
		half _WetMetallic;
		half _WetSmoothness;
		half _Wetness;

		void surf (Input IN, inout SurfaceOutputStandard o) 
		{
			fixed wetMap = dot(tex2D (_WetMap, IN.uv_WetMap).rgb, fixed3(0.3, 0.59, 0.11)); //Wet mask to grayscale
			fixed wetFlow = 1 - tex2D(_WetMap, IN.uv_WetMap).a;
			fixed wetness = (1 - wetFlow) == 0 ? 0 : max((wetMap * _Wetness) - wetFlow, 0) / (1 - wetFlow);

			fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * lerp(_Color, _WetColor, wetness);
			//
			o.Albedo = c.rgb; //apply c to material albedo
			o.Normal = lerp(UnpackNormal(tex2D(_NormalTex, IN.uv_NormalTex)), half3(0, 0, 1), saturate(wetness - _NormalScale + 1)); //material wetness == 0 - apply Normalmap; material wetnes == 1 - dont apply normals
			//Metallic
			fixed4 m = tex2D(_MetallicTex, IN.uv_MetallicTex);
			o.Metallic = lerp(dot(m.rgb, fixed3(0.3, 0.59, 0.11)) * _Metallic, _WetMetallic, wetness);
			//Smoothness
			fixed minS = m.a * _Smoothness; //Min Smoothness(Dry)
			fixed maxS = max(_WetSmoothness, minS); //Max Smoothness(Wet)
			o.Smoothness = lerp(minS, maxS, wetness); //Lerp between Dry & Wet Smoothness by material Wetness
			//
			o.Alpha = c.a; //Apply alpha
		}
		ENDCG
	}
	FallBack "Diffuse"
}
