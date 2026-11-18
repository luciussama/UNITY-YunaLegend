Shader "YunaLegend/ToyRoom_Painterly3D"
{
    Properties
    {
        _BaseMap ("Base Map", 2D) = "white" {}
        _BaseColor ("Base Color", Color) = (1, 1, 1, 1)
        _ShadowColor ("Shadow Tint", Color) = (0.66, 0.56, 0.64, 1)
        _RimColor ("Rim Color", Color) = (1, 0.94, 0.80, 1)
        _RimPower ("Rim Power", Range(1.0, 8.0)) = 4.2
        _RimStrength ("Rim Strength", Range(0.0, 1.0)) = 0.14
        _ShadowStep ("Shadow Threshold", Range(-0.5, 0.5)) = -0.04
        _ShadowFeather ("Shadow Feather", Range(0.02, 0.5)) = 0.18
        _BrushNoiseStrength ("Brush Terminator Noise", Range(0.0, 0.35)) = 0.045
        _EmissionColor ("Emission Color", Color) = (0, 0, 0, 1)
        _OutlineColor ("Outline Color", Color) = (0.22, 0.12, 0.09, 1)
        _OutlineWidth ("Outline Width", Range(0.0, 0.05)) = 0.0045
        _OcclusionFade ("Occlusion Fade", Range(0.15, 1.0)) = 1.0
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
            half _OcclusionFade;
        CBUFFER_END

        float BayerDither4x4(float2 screenPos)
        {
            static const float dither[16] = {
                0.0 / 16.0,  8.0 / 16.0,  2.0 / 16.0, 10.0 / 16.0,
               12.0 / 16.0,  4.0 / 16.0, 14.0 / 16.0,  6.0 / 16.0,
                3.0 / 16.0, 11.0 / 16.0,  1.0 / 16.0,  9.0 / 16.0,
               15.0 / 16.0,  7.0 / 16.0, 13.0 / 16.0,  5.0 / 16.0
            };
            int2 p = int2(screenPos) & 3;
            return dither[p.y * 4 + p.x];
        }

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
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION

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
                if (_OcclusionFade < 0.99h)
                {
                    clip(_OcclusionFade - (half)BayerDither4x4(input.positionHCS.xy) - 0.001h);
                }

                half4 tex = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv) * _BaseColor;
                float3 normalWS = normalize(input.normalWS);
                float3 viewDirWS = GetWorldSpaceNormalizeViewDir(input.positionWS);
                float2 screenUV = GetNormalizedScreenSpaceUV(input.positionHCS);

                half directAO = 1.0h;
                half indirectAO = 1.0h;
                #if defined(_SCREEN_SPACE_OCCLUSION)
                    AmbientOcclusionFactor aoFactor = GetScreenSpaceAmbientOcclusion(screenUV);
                    directAO = aoFactor.directAmbientOcclusion;
                    indirectAO = aoFactor.indirectAmbientOcclusion;
                #endif

                // Indirect bounce GI (SH) modulated by SSAO for rich ray-traced-like contact crevices
                half3 indirectGI = SampleSH(normalWS) * indirectAO * 0.65h;
                half3 litColor = tex.rgb * indirectGI;

                // Main light (only contributes when enabled/non-zero)
                float4 shadowCoord = TransformWorldToShadowCoord(input.positionWS);
                Light mainLight = GetMainLight(shadowCoord);
                half mainLum = dot(mainLight.color, half3(0.3h, 0.59h, 0.11h));
                if (mainLum > 0.001h)
                {
                    float strokeNoise = (BrushNoise(input.uv * float2(28.0, 18.0) + input.positionWS.xz * 2.5) - 0.5) * _BrushNoiseStrength;
                    half rawNdl = saturate(dot(normalWS, mainLight.direction) + strokeNoise);
                    half atten = mainLight.distanceAttenuation * mainLight.shadowAttenuation * directAO;
                    litColor += tex.rgb * mainLight.color * (rawNdl * atten);
                }

                // Additional lights (the 2 windows + Nanda) with full real-time soft shadows
                #ifdef _ADDITIONAL_LIGHTS
                half4 shadowMask = half4(1.0h, 1.0h, 1.0h, 1.0h);
                uint lightCount = GetAdditionalLightsCount();
                half totalDirectMask = 0.0h;
                for (uint i = 0u; i < lightCount; ++i)
                {
                    Light addLight = GetAdditionalLight(i, input.positionWS, shadowMask);
                    half ndl = saturate(dot(normalWS, addLight.direction) * 0.65h + 0.35h);
                    half atten = addLight.distanceAttenuation * addLight.shadowAttenuation * directAO;
                    half3 contrib = tex.rgb * addLight.color * (ndl * atten);
                    litColor += contrib;
                    totalDirectMask = saturate(totalDirectMask + ndl * atten);
                }
                #else
                half totalDirectMask = 0.0h;
                #endif

                // Subtle rim on lateral silhouettes where light hits
                half lateralMask = saturate(1.0h - abs(normalWS.y) * 0.75h);
                half fresnel = pow(1.0h - saturate(dot(normalWS, viewDirWS)), _RimPower) * lateralMask;
                litColor += _RimColor.rgb * (fresnel * _RimStrength * saturate(totalDirectMask + 0.15h));
                litColor += _EmissionColor.rgb;

                // Soft-knee highlight compression so diffused daylight never blows out flat tops
                litColor = litColor / (1.0h + litColor * 0.22h) * 1.16h;

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
                if (_OcclusionFade < 0.99h)
                {
                    clip(_OcclusionFade - (half)BayerDither4x4(input.positionHCS.xy) - 0.001h);
                }
                return _OutlineColor;
            }
            ENDHLSL
        }

        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode"="ShadowCaster" }
            ZWrite On
            ZTest LEqual
            ColorMask 0

            HLSLPROGRAM
            #pragma vertex vertShadow
            #pragma fragment fragShadow

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
            };

            Varyings vertShadow(Attributes input)
            {
                Varyings output;
                float3 posWS = TransformObjectToWorld(input.positionOS.xyz);
                output.positionHCS = TransformWorldToHClip(posWS);
                return output;
            }

            half4 fragShadow(Varyings input) : SV_Target
            {
                return 0;
            }
            ENDHLSL
        }

        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode"="DepthOnly" }
            ZWrite On
            ColorMask R

            HLSLPROGRAM
            #pragma vertex vertDepth
            #pragma fragment fragDepth

            struct Attributes
            {
                float4 positionOS : POSITION;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
            };

            Varyings vertDepth(Attributes input)
            {
                Varyings output;
                output.positionHCS = TransformObjectToHClip(input.positionOS.xyz);
                return output;
            }

            half4 fragDepth(Varyings input) : SV_Target
            {
                return 0;
            }
            ENDHLSL
        }

        Pass
        {
            Name "DepthNormals"
            Tags { "LightMode"="DepthNormals" }
            ZWrite On

            HLSLPROGRAM
            #pragma vertex vertDepthNormals
            #pragma fragment fragDepthNormals

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 normalWS : TEXCOORD0;
            };

            Varyings vertDepthNormals(Attributes input)
            {
                Varyings output;
                output.positionHCS = TransformObjectToHClip(input.positionOS.xyz);
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                return output;
            }

            half4 fragDepthNormals(Varyings input) : SV_Target
            {
                return half4(NormalizeNormalPerPixel(input.normalWS), 0.0);
            }
            ENDHLSL
        }
    }
}
