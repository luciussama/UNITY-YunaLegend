Shader "YunaLegend/ToyRoom_Painterly3D"
{
    Properties
    {
        _BaseMap ("Base Map", 2D) = "white" {}
        _BaseColor ("Base Color", Color) = (1, 1, 1, 1)
        _ShadowColor ("Shadow Tint", Color) = (0.28, 0.24, 0.42, 1)
        _RimColor ("Rim Color", Color) = (1, 0.92, 0.78, 1)
        _RimPower ("Rim Power", Range(1.0, 8.0)) = 3.5
        _RimStrength ("Rim Strength", Range(0.0, 1.0)) = 0.28
        _ShadowStep ("Shadow Threshold", Range(-0.5, 0.5)) = 0.08
        _ShadowFeather ("Shadow Feather", Range(0.02, 0.5)) = 0.16
        _EmissionColor ("Emission Color", Color) = (0, 0, 0, 1)
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" "Queue"="Geometry" }
        LOD 200

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
                float3 positionWS : TEXCOORD1;
                float3 normalWS : TEXCOORD2;
            };

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

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
            CBUFFER_END

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
                litColor += _RimColor.rgb * (fresnel * _RimStrength * saturate(ramp + 0.25h));
                litColor += _EmissionColor.rgb;

                return half4(litColor, tex.a);
            }
            ENDHLSL
        }

        UsePass "Universal Render Pipeline/Lit/ShadowCaster"
    }
}
