Shader "Custom/TerrainShader" {
	Properties {
		testTexture("Texture", 2D) = "white"{}
		testScale("Scale", Float) = 1
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard fullforwardshadows

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		const static int maxLayers = 10;
		const static float tinyValue = 1E-4;  //0.0001;

		int layerCount;
		//I det här shaderspråket måste arrayerna skapas med en storlek. Därför är alla arrays satta till max tio. Varsågod att ändra om du vill :-]
		float3 layerColorWashes[maxLayers];
		float layerStartHeights[maxLayers];
		float layerBlendStrengths[maxLayers];
		float layerWashIntensity[maxLayers];
		float layerTextureScale[maxLayers];

		float lowest;
		float highest;

		sampler2D testTexture;
		float testScale;

		UNITY_DECLARE_TEX2DARRAY(layerTextures);

		struct Input {
			float3 worldPos;
			float3 worldNormal;
		};

		float inverseLerp(float a, float b, float value) {
			return saturate((value-a)/(b-a));
		}

			//triplanar-mapping. För att undvika att texturerna blir otydliga eller distortade efter att ha applicerats så blandar vi alla tre håll. 
			//Alltså uppifrån (IN.worldPos.xz), sidan (IN.worldPos.xy), andra sidan(IN.worldPos.yz) där de sista bokstäverna står för de tredimensionella axlarna
		float3 triplanar(float3 worldPos, float scale, float3 blendAxes, int textureIndex){
			float3 scaledWorldPosition = worldPos / scale;

			
			float3 fromX = UNITY_SAMPLE_TEX2DARRAY(layerTextures, float3(scaledWorldPosition.y, scaledWorldPosition.z, textureIndex)) * blendAxes.x;
			float3 fromY = UNITY_SAMPLE_TEX2DARRAY(layerTextures, float3(scaledWorldPosition.x, scaledWorldPosition.z, textureIndex)) * blendAxes.y;
			float3 fromZ = UNITY_SAMPLE_TEX2DARRAY(layerTextures, float3(scaledWorldPosition.x, scaledWorldPosition.y, textureIndex)) * blendAxes.z;
			return fromX + fromY + fromZ;
		}

		void surf (Input IN, inout SurfaceOutputStandard o) {
			float heightPercent = inverseLerp(lowest,highest, IN.worldPos.y);

			//följande uträkning är till för att undvika att texturens färg distortas när den appliceras
			float3 blendAxes = abs(IN.worldNormal);
			blendAxes /= blendAxes.x + blendAxes.y + blendAxes.z;

			for (int i = 0; i < layerCount; i ++) {
				//tinyValue är bara med här för att undvika buggar om användaren matar in ett nollvärde
				float drawStrength = inverseLerp(-layerBlendStrengths[i]/2-tinyValue, layerBlendStrengths[i]/2
				, heightPercent - layerStartHeights[i]);

				float3 baseColor = layerColorWashes[i] * layerWashIntensity[i];
				float3 textureColor = triplanar(IN.worldPos, layerTextureScale[i], blendAxes, i) * (1- layerWashIntensity[i]);

				o.Albedo = o.Albedo * (1-drawStrength) + (baseColor + textureColor) * drawStrength;
			}
		}
		ENDCG
	}
	FallBack "Diffuse"
}