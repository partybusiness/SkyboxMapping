Shader "Unlit/FromSkyboxToPatch"
{
	Properties{
		[NoScaleOffset]_MainTex("Panorama Texture", 2D) = "white" {}
		_Cube("Reflection Map", Cube) = "" {}
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
			uniform samplerCUBE _Cube;
			sampler2D _MainTex;
			float _OffsetX;
			float _OffsetY;
			float _Width;
			float _Height;

			struct vertexInput {
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};
			struct vertexOutput {
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
			};
			vertexOutput vert(vertexInput input)
			{
				vertexOutput output;

				float4x4 modelMatrix = unity_ObjectToWorld;
				//output.viewDir = mul(modelMatrix, input.vertex).xyz
				//	- _WorldSpaceCameraPos;
				output.uv = input.uv;
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
				longitude = 0.25 +longitude;//Quarter offset is arbitrary but matches how the Cubemap is sampled in Unity
				latitude = 1 + latitude;
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
				float2 uv = input.uv;
				//if ((round(uv.x*10) + round(uv.y*10))%2 == 1) {
					//return float4(uv, 1,1);
				//}
				uv.x = lerp(_OffsetX - _Width * 0.5, _OffsetX + _Width * 0.5, uv.x);
				uv.y = lerp(_OffsetY - _Height * 0.5, _OffsetY + _Height * 0.5, uv.y);
				//return float4(uv, 1, 1);
				
				return tex2D(_MainTex, uv);
				//return float4(input.uv,1,1);
				//return texCUBE(_Cube, input.viewDir);
			}
			ENDCG
		}
	}
}

