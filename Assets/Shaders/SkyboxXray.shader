Shader "Unlit/SkyboxXray"
{
	Properties{
		_Left("Left Map", Cube) = "" {}
		_Right("Right Map", Cube) = "" {}
		_Centre("Centre of Wipe", Vector) = (0,0,0,0)
		_Radius("Wipe Radius", Float) = 0
		_Feather("Wipe Feather", Float) = 0.2
	}

		SubShader{
			Tags{ "Queue" = "Transparent" "RenderType" = "Transparent" }
			ZWrite Off
			Blend SrcAlpha OneMinusSrcAlpha
			Pass {
				CGPROGRAM
					#pragma vertex vert
					#pragma fragment frag
					#include "UnityCG.cginc"
					uniform samplerCUBE _Left;
					uniform samplerCUBE _Right;
					float4 _Centre;
					float _Radius;
					float _Feather;
					struct vertexInput {
						float4 vertex : POSITION;
					};
					struct vertexOutput {
						float4 pos : SV_POSITION;
						float3 viewDir : TEXCOORD1;
						float4 worldPos : TEXCOORD2;
					};
					vertexOutput vert(vertexInput input) {
						vertexOutput output;

						float4x4 modelMatrix = unity_ObjectToWorld;
						// multiplication with unity_Scale.w is unnecessary
						// because we normalize transformed vectors
						output.viewDir = mul(modelMatrix, input.vertex).xyz - _WorldSpaceCameraPos;
						output.pos = UnityObjectToClipPos(input.vertex);
						output.worldPos = mul(modelMatrix, input.vertex);
						return output;
					}
					float4 frag(vertexOutput input) : COLOR
					{
						float4 col = lerp(texCUBE(_Left, input.viewDir),texCUBE(_Right, input.viewDir),unity_StereoEyeIndex);
						col.a = saturate((_Radius - distance(input.worldPos,_Centre)) / _Feather);
						return col;
					}
				ENDCG
			}
		}
}