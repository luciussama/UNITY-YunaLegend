Shader "YunaLegend/ToyRoom_Painterly3D"
{
    Properties
    {
        _BaseMap ("Base Map", 2D) = "white" {}
        _BaseColor ("Base Color", Color) = (1, 1, 1, 1)
        _ShadowColor ("Shadow Tint", Color) = (0.34, 0.28, 0.44, 1)
        _RimColor ("Rim Color", Color) = (1, 0.92, 0.74, 1)
        _RimPower ("Rim Power", Range(1.0, 8.0)) = 3.2
        _RimStrength ("Rim Strength", Range(0.0, 1.0)) = 0.24
        _ShadowStep ("Shadow Threshold", Range(-0.5, 0.5)) = 0.05
        _ShadowFeather ("Shadow Feather", Range(0.02, 0.5)) = 0.18
        _EmissionColor ("Emission Color", Color) = (0, 0, 0, 1)
        _OutlineColor ("Outline Color", Color) = (0.22, 0.13, 0.09, 1)
        _OutlineWidth ("Outline Width", Range(0.0, 0.05)) = 0.012
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" "Queue"="Geometry" }
        LOD 200

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

        CBUFFER_START(UnityPerMaterial)
            float4 _BaseMap_ST;
            half4 _BaseColor;
            half4 _ShadowColor;
            half4 _RimColor;
            half _RimPower;
            half _RimStrength;
            half _ShadowStep;
            half _ShadowFeather;
            half4 _EmissionColor;
            half4 _OutlineColor;
            float _OutlineWidth;
        CBUFFER_END
        ENDHLSL

        Pass
        {
            Name "UniversalForward"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _SHADOWS_SOFT
            #pragma multi_compile _ _ADDITIONAL_LIGHTS

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
                float3 positionWS : TEXCOORD1;
                float3 normalWS : TEXCOORD2;
            };

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            Varyings vert(Attributes input)
            {
                Varyings output;
                VertexPositionInputs posInputs = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs normInputs = GetVertexNormalInputs(input.normalOS);

                output.positionHCS = posInputs.positionCS;
                output.positionWS = posInputs.positionWS;
                output.normalWS = normalize(normInputs.normalWS);
                output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                half4 tex = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv) * _BaseColor;
                float3 normalWS = normalize(input.normalWS);
                float3 viewDirWS = GetWorldSpaceNormalizeViewDir(input.positionWS);

                float4 shadowCoord = TransformWorldToShadowCoord(input.positionWS);
                Light mainLight = GetMainLight(shadowCoord);

                half ndl = dot(normalWS, mainLight.direction);
                half ramp = smoothstep(_ShadowStep - _ShadowFeather, _ShadowStep + _ShadowFeather, ndl * mainLight.shadowAttenuation);
                half3 litColor = lerp(_ShadowColor.rgb * tex.rgb, tex.rgb * mainLight.color, ramp);

                #ifdef _ADDITIONAL_LIGHTS
                uint lightCount = GetAdditionalLightsCount();
                for (uint i = 0u; i < lightCount; ++i)
                {
                    Light addLight = GetAdditionalLight(i, input.positionWS);
                    half addNdl = saturate(dot(normalWS, addLight.direction));
                    litColor += tex.rgb * addLight.color * (addNdl * addLight.distanceAttenuation * addLight.shadowAttenuation * 0.75h);
                }
                #endif

                half fresnel = pow(1.0h - saturate(dot(normalWS, viewDirWS)), _RimPower);
                litColor += _RimColor.rgb * (fresnel * _RimStrength * saturate(ramp + 0.30h));
                litColor += _EmissionColor.rgb;

                return half4(litColor, tex.a);
            }
            ENDHLSL
        }

        Pass
        {
            Name "Outline"
            Tags { "LightMode"="SRPDefaultUnlit" }
            Cull Front
            ZWrite On

            HLSLPROGRAM
            #pragma vertex vertOutline
            #pragma fragment fragOutline

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
            };

            Varyings vertOutline(Attributes input)
            {
                Varyings output;
                float3 posWS = TransformObjectToWorld(input.positionOS.xyz);
                float3 normWS = TransformObjectToWorldNormal(input.normalOS);
                posWS += normalize(normWS) * _OutlineWidth;
                output.positionHCS = TransformWorldToHClip(posWS);
                return output;
            }

            half4 fragOutline(Varyings input) : SV_Target
            {
                return _OutlineColor;
            }
            ENDHLSL
        }

        UsePass "Universal Render Pipeline/Lit/ShadowCaster"
    }
}
