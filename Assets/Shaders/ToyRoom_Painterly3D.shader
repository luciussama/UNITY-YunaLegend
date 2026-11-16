Shader "YunaLegend/ToyRoom_Painterly3D"
{
    Properties
    {
        _BaseMap ("Base Map", 2D) = "white" {}
        _BaseColor ("Base Color", Color) = (1, 1, 1, 1)
        _ShadowColor ("Shadow Tint", Color) = (0.42, 0.34, 0.50, 1)
        _RimColor ("Rim Color", Color) = (1, 0.92, 0.76, 1)
        _RimPower ("Rim Power", Range(1.0, 8.0)) = 4.2
        _RimStrength ("Rim Strength", Range(0.0, 1.0)) = 0.14
        _ShadowStep ("Shadow Threshold", Range(-0.5, 0.5)) = 0.02
        _ShadowFeather ("Shadow Feather", Range(0.02, 0.5)) = 0.14
        _BrushNoiseStrength ("Brush Terminator Noise", Range(0.0, 0.35)) = 0.11
        _EmissionColor ("Emission Color", Color) = (0, 0, 0, 1)
        _OutlineColor ("Outline Color", Color) = (0.20, 0.11, 0.08, 1)
        _OutlineWidth ("Outline Width", Range(0.0, 0.05)) = 0.006
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
            half _BrushNoiseStrength;
            half4 _EmissionColor;
            half4 _OutlineColor;
            float _OutlineWidth;
        CBUFFER_END

        float Hash21(float2 p)
        {
            p = frac(p * float2(127.1, 311.7));
            p += dot(p, p + 34.5);
            return frac(p.x * p.y);
        }

        float BrushNoise(float2 p)
        {
            float2 i = floor(p);
            float2 f = frac(p);
            float2 u = f * f * (3.0 - 2.0 * f);
            return lerp(
                lerp(Hash21(i), Hash21(i + float2(1, 0)), u.x),
                lerp(Hash21(i + float2(0, 1)), Hash21(i + float2(1, 1)), u.x),
                u.y);
        }
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

                // Hand-brushed terminator noise breaks up mathematical cel-shading bands
                float strokeNoise = (BrushNoise(input.uv * float2(24.0, 14.0) + input.positionWS.xz * 3.5) - 0.5) * _BrushNoiseStrength;
                half rawNdl = dot(normalWS, mainLight.direction) * mainLight.shadowAttenuation + strokeNoise;

                // 3-step quantized storybook cel-ramp (Shadow -> Warm Midtone -> Soft Lit)
                half shadowBand = smoothstep(_ShadowStep - _ShadowFeather, _ShadowStep + _ShadowFeather, rawNdl);
                half highlightBand = smoothstep(0.55h, 0.72h, rawNdl);

                half3 shadowTone = _ShadowColor.rgb * tex.rgb;
                half3 midTone = tex.rgb * lerp(half3(0.92h, 0.88h, 0.86h), mainLight.color, 0.75h);
                half3 highTone = tex.rgb * mainLight.color * 1.04h;

                half3 litColor = lerp(shadowTone, midTone, shadowBand);
                litColor = lerp(litColor, highTone, highlightBand * 0.45h);

                #ifdef _ADDITIONAL_LIGHTS
                uint lightCount = GetAdditionalLightsCount();
                for (uint i = 0u; i < lightCount; ++i)
                {
                    Light addLight = GetAdditionalLight(i, input.positionWS);
                    half addNdl = smoothstep(0.0h, 0.45h, saturate(dot(normalWS, addLight.direction)));
                    litColor += tex.rgb * addLight.color * (addNdl * addLight.distanceAttenuation * addLight.shadowAttenuation * 0.65h);
                }
                #endif

                // Suppress rim on upward-facing horizontal surfaces so isometric view never washes out
                half lateralMask = saturate(1.0h - abs(normalWS.y) * 0.75h);
                half fresnel = pow(1.0h - saturate(dot(normalWS, viewDirWS)), _RimPower) * lateralMask;
                litColor += _RimColor.rgb * (fresnel * _RimStrength * saturate(shadowBand + 0.25h));
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
                float3 radialOS = length(input.positionOS.xyz) > 0.0001 ? normalize(input.positionOS.xyz) : input.normalOS;
                float3 smoothNormalOS = normalize(lerp(input.normalOS, radialOS, 0.35));
                float3 posWS = TransformObjectToWorld(input.positionOS.xyz);
                float3 normWS = TransformObjectToWorldNormal(smoothNormalOS);
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
