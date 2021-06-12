// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Invisible"
{
	Properties
	{
		_MaskClipValue( "Mask Clip Value", Float ) = 0.5
		[HideInInspector] __dirty( "", Int ) = 1
		_DistortionMap("DistortionMap", 2D) = "bump" {}
		/*_Noise("Noise", 2D) = "white" {}
		_EdgeSize("EdgeSize", Range(0,1)) = 0.2
		_cutoff("cutoff", Range(0,1)) = 0.0*/
		_RippleScale("Ripple Scale", Range( 0 , 20)) = 1.5
		_DisturbanceScale("Disturbance Scale", Range( 0 , 1)) = 0.19
		_RippleSpeed("RippleSpeed", Range( 0 , 1)) = 0.3
		_Blending("Blending", Range( 0 , 1)) = 0.5393515
	}

	SubShader
	{
		Tags{ "RenderType" = "Overlay"  "Queue" = "Transparent+1" "IsEmissive" = "true"  }
		Cull Back
		GrabPass{ "_GrabScreen0" }
		CGPROGRAM
		#include "UnityShaderVariables.cginc"
		#pragma target 3.0
		#pragma surface surf Standard alpha:fade  noshadow noforwardadd	// keepalpha  vertex:vert
		struct Input
		{
			float4 screenPos;
			float3 worldNormal;
			float3 viewDir;
			float2 uv_Noise;
			//float4 vHeight;
		};

		uniform sampler2D _GrabScreen0;
		uniform sampler2D _DistortionMap;
		//sampler2D _Noise;
		//float _EdgeSize, _cutoff;
		uniform float _RippleScale;
		uniform float _RippleSpeed;
		uniform float _DisturbanceScale;
		uniform float _Blending;
		uniform float _MaskClipValue = 0.5;

		/*void vert(inout appdata_full v, out Input o) 
		{
			float height = v.vertex.y;
			UNITY_INITIALIZE_OUTPUT(Input, o);
			o.vHeight = height;
		}*/

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			/*half3 Noise = tex2D(_Noise, i.uv_Noise);
			Noise.r = lerp(0, 1, Noise.r);
			_cutoff = lerp(0, _cutoff + _EdgeSize, _cutoff);*/

			o.Albedo = float3(0,0,0);
			float4 screenPos3 = i.screenPos;
			#if UNITY_UV_STARTS_AT_TOP
			float scale3 = -1.0;
			#else
			float scale3 = 1.0;
			#endif
			float halfPosW3 = screenPos3.w * 0.5;
			screenPos3.y = ( screenPos3.y - halfPosW3 ) * _ProjectionParams.x* scale3 + halfPosW3;
			screenPos3.w += 0.00000000001;
			screenPos3.xyzw /= screenPos3.w;
			float4 temp_cast_0 = ( _Time.y * _RippleSpeed );
			float4 temp_cast_3 = 1.0;
			float3 invis = lerp( tex2Dproj( _GrabScreen0, UNITY_PROJ_COORD( ( float4( ( UnpackNormal( tex2D( _DistortionMap,( _RippleScale * float2( ( temp_cast_0 + screenPos3 ).x , ( temp_cast_0 + screenPos3 ).y ) ).xy) ) * _DisturbanceScale ) , 0.0 ) + screenPos3 ) ) ) , temp_cast_3 , _Blending ).rgb;
			float fresnel = pow((1.0 - saturate(dot(i.worldNormal, i.viewDir))), 5.0);

			//float dissolve = step(0.0, Noise - _cutoff);

			o.Emission = invis - (fresnel * 0.25); // *dissolve + float3(1.0, 1.0, 1.0) * (1 - dissolve);
			float temp_output_20_0 = 1.0;
			o.Metallic = temp_output_20_0;
			o.Alpha = 1.0;
			
			/* this code can be used to cut the object based on its vertex heights
			float splitValue = 0.3;
			float visibility = 1 - step(i.vHeight, splitValue);
			o.Alpha = 1.0 - visibility;*/
		}

		ENDCG
	}
}