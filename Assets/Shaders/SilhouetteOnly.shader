// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Outlined/Silhouette Only" {
	Properties{
		_Color("Main Color", Color) = (0,0,0,1)
		_OutlineColor("Outline Color", Color) = (0,0,0,1)
		_Outline("Outline width", Range(0.0, 0.03)) = .005
	}

		CGINCLUDE
#include "UnityCG.cginc"

		struct appdata {
		float4 vertex : POSITION;
		float3 normal : NORMAL;
		fixed4 color : COLOR;
	};

	struct v2f {
		float4 pos : POSITION;
		float4 color : COLOR;
	};

	uniform float _Outline;
	uniform float4 _OutlineColor;

	ENDCG

		SubShader{
		Tags{ "Queue" = "Geometry" }

		Pass
	{
		Name "BASE"
		LOD 100
		Cull Back
		Lighting Off
		ZWrite On
		Blend SrcAlpha OneMinusSrcAlpha

		CGPROGRAM
#pragma vertex vert
#pragma fragment frag

#include "UnityCG.cginc"

	fixed4 _Color;

	v2f vert(appdata v) {
		v2f o;
		o.pos = UnityObjectToClipPos(v.vertex);
		o.color = v.color;
		o.color.a = _Color.a;
		return o;
	}

	fixed4 frag(v2f i) : SV_Target
	{
		return i.color;
	}
		ENDCG
	}

		// note that a vertex shader is specified here but its using the one above
		Pass{
		Name "OUTLINE"
		Tags{ "LightMode" = "Always" "Queue" = "Transparent" }
		Cull Front
		ZWrite Off
		ZTest Always

		//Offset 50, 50

		// you can choose what kind of blending mode you want for the outline
		//Blend SrcAlpha OneMinusSrcAlpha // Normal
		//Blend One One // Additive
		Blend One OneMinusDstColor // Soft Additive
								   //Blend DstColor Zero // Multiplicative
								   //Blend DstColor SrcColor // 2x Multiplicative
		Blend SrcAlpha OneMinusSrcAlpha

		CGPROGRAM
#pragma vertex vert
#pragma fragment frag

		v2f vert(appdata v) {
		// just make a copy of incoming vertex data but scaled according to normal direction
		v2f o;
		o.pos = UnityObjectToClipPos(v.vertex);

		float3 norm = mul((float3x3)UNITY_MATRIX_IT_MV, v.normal);
		float2 offset = TransformViewToProjection(norm.xy);

		o.pos.xy += offset * o.pos.z * _Outline;
		o.color = _OutlineColor;
		// As this is used for the 'connectivity' mode, where we set parts of the 
		// network to green, use the green color value to determine visability of outline
		o.color.a = v.color.g / 4;

		return o;
	}

		half4 frag(v2f i) :COLOR{
		return i.color;
	}
		ENDCG
	}

	}

		Fallback "Diffuse"
}