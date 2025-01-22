Shader "Custom/HologramFresnelURP_Mobile"
{
    Properties
    {
        _BaseColor ("Base Color", Color) = (0, 1, 1, 1)
        _MainTex ("Main Texture", 2D) = "white" {}
        _NoiseTex ("Noise Texture (for scan lines)", 2D) = "white" {}
        _NoiseIntensity ("Noise Intensity", Range(0,1)) = 0.3
        _ScrollSpeed ("Noise Scroll Speed", Range(-5,5)) = 1.0

        _FresnelColor ("Fresnel Color", Color) = (0.0, 1.0, 1.0, 1.0)
        _FresnelPower ("Fresnel Power", Range(0.1,8.0)) = 2.0
        _FresnelStrength ("Fresnel Strength", Range(0,1)) = 0.5

        _Alpha ("Overall Alpha", Range(0,1)) = 0.5
    }

    SubShader
    {
        Tags
        {
            "RenderType"="Transparent"
            "Queue"="Transparent"
        }

        Pass
        {
            Name "ForwardLit"
            Tags{"LightMode"="UniversalForward"}

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Back

            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_fog
            #pragma multi_compile _ _ADDITIONAL_LIGHTS
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float3 viewDirWS : TEXCOORD2;
            };

            sampler2D _MainTex;
            float4 _BaseColor;
            float _Alpha;

            sampler2D _NoiseTex;
            float _NoiseIntensity;
            float _ScrollSpeed;

            float4 _FresnelColor;
            float _FresnelPower;
            float _FresnelStrength;

            Varyings Vert(Attributes v)
            {
                Varyings o;
                o.positionHCS = TransformObjectToHClip(v.positionOS.xyz);
                float3 worldPos = TransformObjectToWorld(v.positionOS.xyz);
                o.normalWS = normalize(TransformObjectToWorldNormal(v.normalOS));
                float3 cameraPosWS = GetCameraPositionWS();
                o.viewDirWS = normalize(cameraPosWS - worldPos);
                o.uv = v.uv;
                return o;
            }

            float4 Frag(Varyings i) : SV_Target
            {
                // Base texture and color
                float4 baseCol = _BaseColor;
                float2 noiseUV = float2(i.uv.x, i.uv.y + _Time.y * _ScrollSpeed);
                float noiseVal = tex2D(_NoiseTex, noiseUV).r * _NoiseIntensity;

                float4 mainTexCol = tex2D(_MainTex, i.uv);
                float4 combinedColor = baseCol * mainTexCol;
                combinedColor.rgb += noiseVal;

                // Fresnel calculation
                float fresnelFactor = pow(1.0 - saturate(dot(i.normalWS, i.viewDirWS)), _FresnelPower);
                fresnelFactor *= _FresnelStrength;
                combinedColor.rgb = lerp(combinedColor.rgb, _FresnelColor.rgb, fresnelFactor);

                // Set alpha
                combinedColor.a = _Alpha;

                return combinedColor;
            }
            ENDHLSL
        }
    }
    FallBack Off
}
