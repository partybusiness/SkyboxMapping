Shader "Unlit/FromSkyboxToPatch"
{
	Properties{
		[NoScaleOffset]_MainTex("Ignore Texture", 2D) = "white" {}
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
			float4 frag(vertexOutput input) : COLOR
			{
				return texCUBE(_Cube, input.viewDir);
			}
			ENDCG
		}
	}
}

