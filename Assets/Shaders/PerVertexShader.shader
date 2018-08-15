Shader "Unlit/PerVertexShader"
{
	Properties
	{
		_Color("Main Color", Color) = (1,1,1,1)
	}

		SubShader
	{
		Tags { "RenderType" = "Opaque" "Queue" = "Transparent" }
		LOD 100
		Cull Back
		Lighting Off
		ZWrite On
		Blend SrcAlpha OneMinusSrcAlpha

		Pass
		{
			Name "BASE"
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"
			
			struct appdata {
				float4 vertex : POSITION;
				fixed4 color : COLOR;
			};

			struct v2f {
				float4 pos : SV_POSITION;
				fixed4 color : COLOR;
			};

			fixed4 _Color;

			v2f vert(appdata v) {
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.color = v.color;
				o.color.a = _Color.a;
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{ 
				return i.color; 
			}
			ENDCG
		}
	}
}
