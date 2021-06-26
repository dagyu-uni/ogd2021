Shader "Unlit/Distortion"
{
    Properties
    {
		[NoScaleOffset] _MainTex("Main Texture", 2D) = "white" {}
		_MainTiling ("Main Tiling", Float) = 1.0
		[NoScaleOffset] _NoiseTex("Noise Texture", 2D) = "white" {}
		_NoiseTiling("Noise Tiling", Vector) = (1,1,1,1)
		_NoiseSpeed("Noise Speed", Vector) = (1,1,1,1)
		_NoiseColor("Noise Color", Color) = (1,1,1,1)
		_NoiseBrightness("Noise Brightness", Float) = 2.0
		_FresnelPower("Fresnel Power", Float) = 1.0
		[NoScaleOffset] _DistortionTex("Distortion Texture", 2D) = "white" {}
		_DistortionTiling("Distortion Tiling", Vector) = (1,1,1,1)
		_DistortionSpeed("Distortion Speed", Vector) = (0.1, 0.1, 1, 1)
		_DistortionDirection("Distortion Direction", Vector) = (0.1, 0.1, 0.1, 0.1)
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        LOD 100

		GrabPass { "_GrabTexture"}

        Pass
        {
			ZWrite On
			Blend One Zero
			Lighting Off
			Cull Off
			Fog { Mode Off }

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
				float3 normal : NORMAL;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
				half3 worldNormal : TEXCOORD1;
				half3 worldPos : TEXCOORD2;
				float4 grabUV : TEXCOORD3;
                float4 vertex : SV_POSITION;
            };

			sampler2D _MainTex;
			sampler2D _NoiseTex;
			float4 _NoiseTex_ST;
			sampler2D _DistortionTex;
			sampler2D _GrabTexture;

			half _MainTiling;
			half4 _NoiseTiling;
			half4 _NoiseSpeed;
			half4 _NoiseColor;
			half _NoiseBrightness;
			half _FresnelPower;
			half4 _DistortionTiling;
			half4 _DistortionSpeed;
			half4 _DistortionDirection;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
				o.worldNormal = normalize(UnityObjectToWorldNormal(v.normal));
				o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.uv = TRANSFORM_TEX(v.uv, _NoiseTex);
				o.grabUV = ComputeGrabScreenPos(o.vertex);
                return o;
            }

			fixed4 frag(v2f i) : SV_Target
			{
				half4 mainTex = tex2D(_MainTex, i.uv * _MainTiling);

				// offset based on time and speed
				half2 timeSpeed = half2(_NoiseSpeed.r, _NoiseSpeed.g) * _Time.y;
				half2 tiling = half2(_NoiseTiling.r, _NoiseTiling.g);
				half4 noise = tex2D(_NoiseTex, i.uv * tiling + timeSpeed) * _NoiseColor * _NoiseBrightness;
				// viewDir
				half3 worldViewDir = normalize(UnityWorldSpaceViewDir(i.worldPos));

				// Fresnel Effect
				half fresnel = pow((1.0 - saturate(dot(i.worldNormal, worldViewDir))), _FresnelPower);
                
				half4 finalNoise = noise * fresnel * mainTex;

				// Distortion
				half2 distortionSpeed = half2(_DistortionSpeed.r, _DistortionSpeed.g) * _Time.y;
				half2 distortionTiling = half2(_DistortionTiling.r, _DistortionTiling.g);

				half4 bump = tex2D(_DistortionTex, i.uv * distortionTiling + distortionSpeed);
				half2 distortion = UnpackNormal(bump).rg;

				half2 distortionDir = half2(_DistortionDirection.r, _DistortionDirection.g);
			
				// Grab Texture
				i.grabUV.xy += distortion * distortionDir;
				half4 grabTex = tex2Dproj(_GrabTexture, i.grabUV);
                

				return grabTex + finalNoise;
            }
            ENDCG
        }
    }
}
