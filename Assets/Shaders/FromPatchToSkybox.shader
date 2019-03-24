Shader "Unlit/FromPatchToSkybox"
{
	Properties{
		[NoScaleOffset]_Patch("Patch Map", 2D) = "white" {}
		_MinX("MinX", Range(0.0,1.0)) = 0.0
		_MaxX("MaxX", Range(0.0,1.0)) = 1.0
		_MinY("MinY", Range(0.0,1.0)) = 0.0
		_MaxY("MaxY", Range(0.0,1.0)) = 1.0
	}
	SubShader{
		Pass{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
			sampler2D _Patch;
			float _MinX;
			float _MaxX;
			float _MinY;
			float _MaxY;
			

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
				return float2(-0.25, 1.0) - sphereCoords;//???
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
				//how to map latlong
				latlong.x = (latlong.x - _MinX) / (_MaxX - _MinX);
				latlong.y = (latlong.y - _MinY) / (_MaxY - _MinY);
				return tex2D(_Patch, latlong);
			}
			ENDCG
		}
	}
}

