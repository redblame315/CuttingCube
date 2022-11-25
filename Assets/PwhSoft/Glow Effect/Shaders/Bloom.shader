﻿Shader "PwhSoft/Bloom" {
	Properties {
		_MainTex ("Texture", 2D) = "white" {}
	}

	CGINCLUDE
		#include "UnityCG.cginc"

		sampler2D _MainTex, _SourceTex;
		float4 _MainTex_TexelSize;

		half4 _Filter;

		half _Intensity;

		struct VertexData {
			float4 vertex : POSITION;
			float2 uv : TEXCOORD0;
		};

		struct Interpolators {
			float4 pos : SV_POSITION;
			float2 uv : TEXCOORD0;
		};

		Interpolators VertexProgram (VertexData v) {
			Interpolators i;
			i.pos = UnityObjectToClipPos(v.vertex);
			i.uv = v.uv;
			return i;
		}

		half3 ExecuteSampling (float2 uv) {
			return tex2D(_MainTex, uv).rgb;
		}

		half3 CreateSamplingBox (float2 uv, float delta) {
			float4 o = _MainTex_TexelSize.xyxy * float2(-delta, delta).xxyy;
			half3 s =
				ExecuteSampling(uv + o.xy) + ExecuteSampling(uv + o.zy) +
				ExecuteSampling(uv + o.xw) + ExecuteSampling(uv + o.zw);
			return s * 0.25f;
		}

		half3 Prefilter (half3 c) {
			half brightness = max(c.r, max(c.g, c.b));
			half soft = brightness - _Filter.y;
			soft = clamp(soft, 0, _Filter.z);
			soft = soft * soft * _Filter.w;
			half contribution = max(soft, brightness - _Filter.x);
			contribution /= max(brightness, 0.00001);
			return c * contribution;
		}

	ENDCG

	SubShader {
		Cull Off
		ZTest Always
		ZWrite Off

		Pass { // 0
			CGPROGRAM
				#pragma vertex VertexProgram
				#pragma fragment FragmentProgram

				half4 FragmentProgram (Interpolators i) : SV_Target {
					return half4(Prefilter(CreateSamplingBox(i.uv, 1)), 1);
				}
			ENDCG
		}

		Pass { // 1
			CGPROGRAM
				#pragma vertex VertexProgram
				#pragma fragment FragmentProgram

				half4 FragmentProgram (Interpolators i) : SV_Target {
					return half4(CreateSamplingBox(i.uv, 1), 1);
				}
			ENDCG
		}

		Pass { // 2
			Blend One One

			CGPROGRAM
				#pragma vertex VertexProgram
				#pragma fragment FragmentProgram

				half4 FragmentProgram (Interpolators i) : SV_Target {
					return half4(CreateSamplingBox(i.uv, 0.5), 1);
				}
			ENDCG
		}

		Pass { // 3 - Hier überprüfen TODO
			CGPROGRAM
				#pragma vertex VertexProgram
				#pragma fragment FragmentProgram

				half4 FragmentProgram (Interpolators i) : SV_Target {
				    half glow = 2;
//				    half contrast =1;
					half4 c = tex2D(_SourceTex, i.uv);
					c.rgb += _Intensity * CreateSamplingBox(i.uv, 0.5);
//					c.rgb = lerp(_Intensity, c.rgb, glow); //glow
//					c.rgb = (c.rgb - 0.5f) * (contrast)+0.5f;
					return c;
				}
			ENDCG
		}

		Pass { // 4
			CGPROGRAM
				#pragma vertex VertexProgram
				#pragma fragment FragmentProgram

				half4 FragmentProgram (Interpolators i) : SV_Target {
					return half4(_Intensity * CreateSamplingBox(i.uv, 0.5), 1);
				}
			ENDCG
		}
	}
}