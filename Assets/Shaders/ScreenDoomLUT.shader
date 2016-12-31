Shader "Hidden/DoomLUT"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_LUTTex ("LUT", 2D) = "white" {}
		_CurPal ("Current Palette", Int) = 0
		_NumPals ("Number of Palettes", Int) = 14
		_NumMaps ("Number of Colormaps", Int) = 34
		_NumCols ("Number of Colors", Int) = 256
	}
	SubShader
	{
		// No culling or depth
		Cull Off ZWrite Off ZTest Always

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				o.uv = v.uv;
				return o;
			}
			
			sampler2D _MainTex;
			sampler2D _LUTTex;
			uint _CurPal;
			uint _NumPals;
			uint _NumMaps;
			uint _NumCols;

			fixed4 frag (v2f i) : SV_Target
			{
				float4 tex = tex2D(_MainTex, i.uv);

				float map = (lerp(0, _NumMaps - 2, tex.g) + _CurPal * (_NumMaps + 1)) / (_NumMaps * _NumPals);
				float col = lerp(0, _NumCols, tex.r) / _NumCols;

				return tex2D(_LUTTex, float2(map, col));
			}
			ENDCG
		}
	}
}
