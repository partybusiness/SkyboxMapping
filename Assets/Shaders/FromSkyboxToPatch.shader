Shader "Unlit/FromSkyboxToPatch"
{
	Properties{
		[NoScaleOffset]_MainTex("Panorama Texture", 2D) = "white" {}
		_Cube("Reflection Map", Cube) = "" {}
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
			uniform samplerCUBE _Cube;
			sampler2D _MainTex;
			float _MinX;
			float _MaxX;
			float _MinY;
			float _MaxY;

			struct vertexInput {
				float4 vertex : POSITION;
				float3 normal : NORMAL;
			};
			struct vertexOutput {
				float4 pos : SV_POSITION;
				float3 viewDir : TEXCOORD0;
			};
			vertexOutput vert(vertexInput input)
			{
				vertexOutput output;

				float4x4 modelMatrix = unity_ObjectToWorld;
				output.viewDir = mul(modelMatrix, input.vertex).xyz
					- _WorldSpaceCameraPos;
				output.pos = UnityObjectToClipPos(input.vertex);
				return output;
			}


			inline float2 ToRadialCoords(float3 coords)
			{
				float3 normalizedCoords = normalize(coords);
				float latitude = acos(normalizedCoords.y);
				float longitude = atan2(normalizedCoords.z, normalizedCoords.x);
				float2 sphereCoords = float2(longitude, latitude) * float2(0.5 / UNITY_PI, 1.0 / UNITY_PI);
				return float2(-0.25, 1.0) - sphereCoords;//Quarter offset is arbitrary but matches how the Cubemap is sampled in Unity
			}

			inline float3 FromLatLongToDirection(float latitude, float longitude)
			{
				longitude = -longitude + 0.25;//Quarter offset is arbitrary but matches how the Cubemap is sampled in Unity
				latitude = 1 - latitude;
				longitude /= (0.5 / UNITY_PI);
				latitude /= (1.0 / UNITY_PI);

				float3 coords = float3(0, 0, 0);
				coords.y = cos(latitude);
				coords.z = 0;
				coords.x = 0;
				//float longitude = atan2(normalizedCoords.z, normalizedCoords.x);
				
				return normalize(coords);
			}

			float4 frag(vertexOutput input) : COLOR
			{
				float2 uv = input.viewDir.xy;
				uv.x = lerp(_MinX, _MaxX, uv.x);
				uv.y = lerp(_MinY, _MaxY, uv.y);
				return tex2D(_MainTex, uv);
				//float4(input.viewDir.xy,input.viewDir.z,1);// texCUBE(_Cube, input.viewDir);
			}
			ENDCG
		}
	}
}

