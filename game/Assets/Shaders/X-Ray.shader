Shader "Custom/X-Ray"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
		_AlphaBlending ("Alpha Blending", Range(0,1)) = 0.5
    }
    SubShader
    {

        Tags { "RenderType"="Opaque" }

		// dont apply x-ray pass over basic pass if it already rendered
		// NOTE: only rendered fragments have a ref value of 4, others have the default (0)
		Stencil
		{
			Ref 4			// reference value of 4
			Comp always		// always pass this test (so render when you should render normally)
			Pass replace	// replace the stencil value (null in the first pass here) with 4
			ZFail keep		// use the z-buffer
		}

		// Actual main shader
        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex;

        struct Input
        {
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

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Albedo comes from a texture tinted by color
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = c.rgb;
            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = c.a;
        }
		ENDCG

			// this second pass is executed AFTER the standard pass
			Pass
		{
			Tags {"Queue" = "Transparent"}

			ZWrite Off	// you don't need to write on the z-buffer anyway
			ZTest Always

			Stencil
			{
				Ref 3
				Comp Greater	// pass when 3 is greater than the value that is already in the buffer (4 for already rendered pixels)
				Pass replace	// if the comparison test pass, replace the value
				Fail keep		// else dont draw it
			}

			Blend SrcAlpha OneMinusSrcAlpha

			CGPROGRAM
			#pragma vertex vert 
			#pragma fragment frag

			float _AlphaBlending;

			struct vertexInput
			{
				float4 vertex : POSITION;
			};

			struct vertexOutput
			{
				float4 pos : SV_POSITION;
			};

			vertexOutput vert(vertexInput input)
			{
				vertexOutput output;
				output.pos = UnityObjectToClipPos(input.vertex);
				return output;
			}

			float4 frag(vertexOutput input) : COLOR
			{
				return float4(1.0, 0.0, 0.0, _AlphaBlending);
			}

			ENDCG
		}
    }
    FallBack "Diffuse"
}
