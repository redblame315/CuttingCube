//Lightsaber shader for the glow of the lightsabers
Shader "PwhSoft/Glow"
{
	Properties
	{
		_Color("Color", Color) = (1,0,0)
		_InnerGlow("Inner Glow", Range(0 , 1)) = 0.49
		_OuterGlow("Outer Glow", Range(0 , 12)) = 1.95
	}

	SubShader
	{
		Tags{ "RenderType" = "Transparent"  "Queue" = "Transparent+0" }
		Cull Off
		
		CGPROGRAM
		#pragma target 3.0
		#pragma surface surf Standard keepalpha addshadow fullforwardshadows 

		struct Input
		{
			float4 color : COLOR;
		};

		uniform float _InnerGlow;
		uniform float _OuterGlow;
		uniform float4 _Color;

		void surf(Input input , inout SurfaceOutputStandard output)
		{
			output.Emission = lerp((_Color * _OuterGlow) , _OuterGlow, _InnerGlow).rgb;
		}

		ENDCG
	}
	
	Fallback "Diffuse"
}