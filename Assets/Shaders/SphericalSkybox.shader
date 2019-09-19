Shader "Unlit/SphericalSkybox"
{
	Properties
	{
		_Cube("Cube Map", Cube) = "" {}
		_Radius("Radius", float) = 20
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
			};

			struct v2f
			{
				float3 worldPos : TEXCOORD0;
				float4 vertex : SV_POSITION;
				float3 viewDir : TEXCOORD1;
			};

			uniform samplerCUBE _Cube;
			float _Radius;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.worldPos = mul(unity_ObjectToWorld, v.vertex);
				o.viewDir = mul(unity_ObjectToWorld, v.vertex).xyz - _WorldSpaceCameraPos;
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				float3 viewDir = normalize(i.viewDir);
				float t = dot(-_WorldSpaceCameraPos, viewDir);
				float3 p = _WorldSpaceCameraPos + viewDir * t;
				float y = length(p);
				float x = sqrt(_Radius*_Radius + y * y);
				//_WorldSpaceCameraPos
				fixed4 col = texCUBE(_Cube, (p + (viewDir*x)));
				return col;
			}
			ENDCG
		}
	}
}
