Shader "Custom/TerrainShader" {
	Properties {
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard fullforwardshadows

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		float minHeight, maxHeight;

		sampler2D _MainTex;

		struct Input {
			float3 worldPos;
		};

		float inverseLerp(float a, float b, float value) {
			return saturate((value - a) / (b - a));
		}


		//Called for every pixel
		void surf (Input IN, inout SurfaceOutputStandard o) {

			if ( IN.worldPos.y < 0.2f * 1.8f * 3.0f * 60.0f) {
				o.Albedo = float3(0.055f, 0.094f, 0.631f);
			}
			else if (IN.worldPos.y < 0.25f * 1.8f * 3.0f * 60.0f) {
				o.Albedo = float3(0.808f, 0.839f, 0.522f);
			}
			else if (IN.worldPos.y < 0.65 * 1.8f * 3.0f * 60.0f) {
				o.Albedo = float3(0.0f, 0.478f, 0.039f); 
			}
			else {
				float heightPercent = inverseLerp(0.75f * 3.0f * 60.0f, 1.8f * 3.0f * 60.0f, IN.worldPos.y);
				o.Albedo = heightPercent;
			}
		}
		ENDCG
	}
	FallBack "Diffuse"
}
