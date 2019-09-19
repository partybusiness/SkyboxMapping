Shader "Unlit/HoverPatch"
{
	Properties{
		[NoScaleOffset]_PatchLeft("Patch Map Left", 2D) = "white" {}
		[NoScaleOffset]_PatchRight("Patch Map Right", 2D) = "white" {}
		_OffsetX("OffsetX", float) = 0.0
		_OffsetY("OffsetY", float) = 0.0
		_Width("Width", float) = 0.3
		_Height("Height", float) = 0.3
	}
		SubShader{
			Tags{ "Queue" = "Transparent" "RenderType" = "Transparent" }
			ZWrite Off
			Blend SrcAlpha OneMinusSrcAlpha
		Pass{
		CGPROGRAM
#pragma vertex vert
#pragma fragment frag
#include "UnityCG.cginc"
		sampler2D _PatchLeft;
		sampler2D _PatchRight;
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

	inline float2 ToRadialCoords(float3 coords) {
		float3 normalizedCoords = normalize(coords);
		float latitude = acos(normalizedCoords.y);
		float longitude = atan2(normalizedCoords.z, normalizedCoords.x);
		float2 sphereCoords = float2(longitude, latitude) * float2(0.5 / UNITY_PI, 1.0 / UNITY_PI);
		return float2(-0.25, 1.0) - sphereCoords;//Quarter offset is arbitrary but matches how the Cubemap is sampled in Unity
	}

	vertexOutput vert(vertexInput input) {
		vertexOutput output;

		float4x4 modelMatrix = unity_ObjectToWorld;
		float4x4 modelMatrixInverse = unity_WorldToObject;
		// multiplication with unity_Scale.w is unnecessary
		// because we normalize transformed vectors

		//float4 q = 0;
		//half_angle = angle / 2
		//q.x = axis.x * sin(half_angle);
		//q.y = axis.y * sin(half_angle);
		//q.z = 0;
		//q.w = cos(half_angle);

		output.viewDir = (mul(modelMatrix, input.vertex).xyz
			- _WorldSpaceCameraPos);
		output.pos = UnityObjectToClipPos(input.vertex);
		return output;
	}

	inline float3 FromLatLongToDirection(float latitude, float longitude) {
		longitude = -0.5 + longitude;//Quarter offset is arbitrary but matches how the Cubemap is sampled in Unity
		latitude = latitude;
		longitude *= UNITY_PI * 2.0;
		latitude *= UNITY_PI;

		float3 coords = float3(0, 0, 0);
		coords.y = -cos(latitude);
		coords.z = cos(longitude)*sin(latitude);
		coords.x = sin(longitude)*sin(latitude);

		return normalize(coords);
	}

	float4 RotateAroundY(float4 vertex, float alpha) {
		float sina, cosa;
		sincos(alpha, sina, cosa);
		float2x2 m = float2x2(cosa, -sina, sina, cosa);
		return float4(mul(m, vertex.xz), vertex.yw).xzyw;
	}

	matrix rotationX(float angle) {
		return matrix(1.0, 0, 0, 0,
			0, cos(angle), -sin(angle), 0,
			0, sin(angle), cos(angle), 0,
			0, 0, 0, 1);
	}

	matrix rotationY(float angle) {
		return matrix(cos(angle), 0, sin(angle), 0,
			0, 1.0, 0, 0,
			-sin(angle), 0, cos(angle), 0,
			0, 0, 0, 1);
	}

	float4 frag(vertexOutput input) : COLOR
	{
		float3 centreDirection = FromLatLongToDirection(_OffsetY, _OffsetX);

		float3 orthoX = normalize(cross(-centreDirection,float3(0,1,0)));
		float3 orthoY = normalize(cross(centreDirection,orthoX));

		float3 viewDir = normalize(input.viewDir);

		float2 planePosition = float2(dot(viewDir, orthoX), dot(viewDir, orthoY));
		float ratio = dot(viewDir, centreDirection);
		planePosition /= ratio;
		planePosition /= float2(_Width, _Height);
		planePosition += 0.5;
		float4 col;
		if (unity_StereoEyeIndex == 0)
			col = tex2D(_PatchLeft, planePosition);
		else
			col = tex2D(_PatchRight, planePosition);

		col.a = saturate((sin(_Time.z * 5.0)*0.3 + 0.7 - 1.0) * 3.0 + 1.3);
		return col;
	}
		ENDCG
	}
		}
}