Shader "WetSurface/Decal" 
{
	Properties 
	{
		[Header(Water)]
		[Space]
		_WetColor("Water Color", Color) = (0.251, 0.643, 0.875, 0.25)
		_WetMap ("Wet Map", 2D) = "white" {}
		[Space]
		_WetMetallic ("Water Metallic", Range(0,1)) = 0.1
		_WetSmoothness ("Water Smoothness", Range(0,1)) = 0.95
		[Header(Wetness)]
		_Wetness ("Wetness", Range(0, 1)) = 0.0
	}
	SubShader {
		Tags { "RenderType" = "Opaque" "Queue" = "Geometry+1" "ForceNoShadowCasting" = "True"} //opaque
		LOD 200

		CGPROGRAM
		#pragma surface surf Standard decal:blend
		#pragma target 3.0

		sampler2D _WetMap;

		struct Input 
		{
			float2 uv_WetMap;
		};

		fixed4 _WetColor;
		half _WetMetallic;
		half _WetSmoothness;
		half _Wetness;

		void surf (Input IN, inout SurfaceOutputStandard o) 
		{
			fixed wetMap = dot(tex2D (_WetMap, IN.uv_WetMap).rgb, fixed3(0.3, 0.59, 0.11)); //Wet mask to grayscale
			fixed wetFlow = 1 - tex2D(_WetMap, IN.uv_WetMap).a;
			fixed wetness = (1 - wetFlow) == 0 ? 0 : max((wetMap * _Wetness) - wetFlow, 0) / (1 - wetFlow);

			fixed4 c = lerp(0, _WetColor, wetness);
			//
			o.Albedo = c.rgb; //apply c to material albedo
			o.Metallic = lerp(0, _WetMetallic, wetness);
			o.Smoothness = lerp(0, _WetSmoothness, wetness); //Lerp between Dry & Wet Smoothness by material Wetness
			//Alpha
			clip(c.a - 0.01);
			o.Alpha = c.a; //Apply alpha
		}
		ENDCG
	}
	FallBack "Diffuse"
}
