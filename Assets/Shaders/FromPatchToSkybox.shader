Shader "Unlit/FromPatchToSkybox"
{
	Properties{
		[NoScaleOffset]_Patch("Patch Map", 2D) = "white" {}
		_OffsetX("OffsetX", float) = 0.0
		_OffsetY("OffsetY", float) = 0.0
		_Width("Width", float) = 0.3
		_Height("Height", float) = 0.3
	}
	SubShader{
		Pass{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
			sampler2D _Patch;
			float _OffsetX;
			float _OffsetY;
			float _Width;
			float _Height;
			

			struct vertexInput {
				float4 vertex : POSITION;
			};
			struct vertexOutput {
				float4 pos : SV_POSITION;
				float3 viewDir : TEXCOORD0;
			};

			inline float2 ToRadialCoords(float3 coords)
			{
				float3 normalizedCoords = normalize(coords);
				float latitude = acos(normalizedCoords.y);
				float longitude = atan2(normalizedCoords.z, normalizedCoords.x);
				float2 sphereCoords = float2(longitude, latitude) * float2(0.5 / UNITY_PI, 1.0 / UNITY_PI);
				return float2(-0.25, 1.0) - sphereCoords;//Quarter offset is arbitrary but matches how the Cubemap is sampled in Unity
			}

			vertexOutput vert(vertexInput input)
			{
				vertexOutput output;

				float4x4 modelMatrix = unity_ObjectToWorld;
				float4x4 modelMatrixInverse = unity_WorldToObject;
				// multiplication with unity_Scale.w is unnecessary
				// because we normalize transformed vectors
				output.viewDir = mul(modelMatrix, input.vertex).xyz
					- _WorldSpaceCameraPos;
				output.pos = UnityObjectToClipPos(input.vertex);
				return output;
			}

			float4 frag(vertexOutput input) : COLOR
			{
				float2 latlong = ToRadialCoords(input.viewDir);
				latlong.x = (latlong.x - _OffsetX + _Width * 0.5) / _Width;
				latlong.y = (latlong.y - _OffsetY + _Height * 0.5) / _Height;
				return tex2D(_Patch, latlong);
			}
			ENDCG
		}
	}
}

