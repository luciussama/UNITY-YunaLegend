Shader "YunaLegend/Arena_MetallicSword"
{
    Properties
    {
        _BaseColor ("Metal Tint", Color) = (0.86, 0.91, 0.97, 1)
        _ShadowTint ("Metal Dark Contrast", Color) = (0.24, 0.30, 0.40, 1)
        _SkyReflectColor ("Upper Window Reflection", Color) = (0.94, 0.98, 1.0, 1)
        _GroundReflectColor ("Warm Wood Bounce", Color) = (0.58, 0.42, 0.28, 1)
        _Metallic ("Metallic", Range(0.0, 1.0)) = 0.95
        _Smoothness ("Smoothness", Range(0.1, 1.0)) = 0.90
        _ReflectionStrength ("Reflection Strength", Range(0.0, 2.0)) = 1.15
        _FresnelPower ("Fresnel Power", Range(1.0, 6.0)) = 2.8
        _FresnelStrength ("Fresnel Rim Strength", Range(0.0, 2.0)) = 0.85
        _GlintColor ("Glint Reflection Color", Color) = (1.0, 0.99, 0.95, 1)
        _GlintIntensity ("Glint Intensity", Range(0.0, 3.0)) = 1.65
        _GlintSpeed ("Glint Sweep Speed", Range(0.0, 2.0)) = 0.42
        _GlintWidth ("Glint Band Width", Range(0.02, 0.4)) = 0.11
        _GlintPhase ("Glint Phase Offset", Range(0.0, 1.0)) = 0.48
        _BrushStrength ("Brushed Metal Grain", Range(0.0, 0.3)) = 0.08
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
            #pragma multi_compile_fragment _ _SHADOWS_SOFT

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                half4 _ShadowTint;
                half4 _SkyReflectColor;
                half4 _GroundReflectColor;
                half _Metallic;
                half _Smoothness;
                half _ReflectionStrength;
                half _FresnelPower;
                half _FresnelStrength;
                half4 _GlintColor;
                half _GlintIntensity;
                half _GlintSpeed;
                half _GlintWidth;
                half _GlintPhase;
                half _BrushStrength;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 positionWS  : TEXCOORD0;
                float3 normalWS    : TEXCOORD1;
                float3 positionOS  : TEXCOORD2;
                float2 uv          : TEXCOORD3;
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                VertexPositionInputs posInputs = GetVertexPositionInputs(IN.positionOS.xyz);
                VertexNormalInputs normInputs = GetVertexNormalInputs(IN.normalOS);
                OUT.positionHCS = posInputs.positionCS;
                OUT.positionWS = posInputs.positionWS;
                OUT.normalWS = normalize(normInputs.normalWS);
                OUT.positionOS = IN.positionOS.xyz;
                OUT.uv = IN.uv;
                return OUT;
            }

            half3 EvaluateStylizedRoomReflection(half3 R)
            {
                // Faixa de horizonte escura -> contraste classico de metal polido / cromo
                half upMask = smoothstep(0.02, 0.55, R.y);
                half downMask = smoothstep(-0.02, -0.45, R.y);
                half horizonBand = 1.0 - smoothstep(0.0, 0.16, abs(R.y - 0.03));

                // Reflexo da janela azulada / luz superior
                half3 env = lerp(_ShadowTint.rgb, _SkyReflectColor.rgb, upMask);
                env = lerp(env, _GroundReflectColor.rgb, downMask * 0.72);
                env *= lerp(1.0, 0.42, horizonBand);

                // Dois "softboxes" de reflexo de estudio/janela para leitura metalica imediata
                half3 windowDir1 = normalize(half3(0.35, 0.78, -0.52));
                half3 windowDir2 = normalize(half3(-0.62, 0.48, -0.62));
                half box1 = pow(saturate(dot(R, windowDir1)), 18.0);
                half box2 = pow(saturate(dot(R, windowDir2)), 10.0) * 0.55;
                env += _SkyReflectColor.rgb * (box1 * 1.15 + box2 * 0.65);

                return env;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                half3 N = normalize(IN.normalWS);
                half3 V = normalize(GetWorldSpaceViewDir(IN.positionWS));
                half3 R = reflect(-V, N);

                // Micro-escovado metalico sutil ao longo do eixo da lamina
                half brush = sin(IN.positionOS.x * 160.0 + IN.positionOS.z * 90.0) * 0.5 + 0.5;
                half brushMod = lerp(1.0 - _BrushStrength, 1.0 + _BrushStrength, brush);

                // Luz principal URP + especular metalico agudo (Blinn-Phong / GGX estilizado)
                Light mainLight = GetMainLight();
                half3 L = normalize(mainLight.direction);
                half3 H = normalize(L + V);

                half NdotL = saturate(dot(N, L));
                half NdotV = saturate(dot(N, V));
                half NdotH = saturate(dot(N, H));

                half specPower = lerp(16.0, 140.0, _Smoothness);
                half specular = pow(NdotH, specPower) * (0.85 + _Smoothness * 1.35);

                // Reflexo de ambiente (probe URP + reflexo de sala estilizado para nunca escurecer)
                half mip = (1.0 - _Smoothness) * 5.0;
                half3 probeEnv = DecodeHDREnvironment(
                    SAMPLE_TEXTURECUBE_LOD(unity_SpecCube0, samplerunity_SpecCube0, R, mip),
                    unity_SpecCube0_HDR);
                half3 studioEnv = EvaluateStylizedRoomReflection(R);
                half3 envReflect = lerp(studioEnv, max(studioEnv, probeEnv), 0.35) * _ReflectionStrength;

                // Combinacao metalica: difuso reduzido + reflexo tintado pelo metal
                half3 diffuse = _BaseColor.rgb * (1.0 - _Metallic * 0.78) * (0.35 + 0.65 * NdotL) * mainLight.color;
                half3 metalReflect = envReflect * lerp(half3(1.0, 1.0, 1.0), _BaseColor.rgb, _Metallic * 0.82) * brushMod;
                half3 specColor = lerp(half3(1.0, 1.0, 1.0), _BaseColor.rgb, _Metallic * 0.45) * mainLight.color * specular;

                // Borda Fresnel nitida nos chanfros da lamina
                half fresnel = pow(1.0 - NdotV, _FresnelPower) * _FresnelStrength;
                half3 rimGlow = lerp(_SkyReflectColor.rgb, _GlintColor.rgb, 0.5) * fresnel;

                // Efeito de reflexo diagonal (glint sweep animado + brilho estatico dependente da camera)
                half bladeCoord = IN.positionOS.y * 1.15 + IN.positionOS.x * 1.35 + IN.positionOS.z * 0.85;
                half sweepCycle = frac(_Time.y * _GlintSpeed + _GlintPhase);
                // Mapeia o ciclo para varrer a lamina e fazer breve pausa entre reflexos
                half sweepPos = lerp(-0.35, 1.45, smoothstep(0.0, 0.62, sweepCycle));
                half sweepDist = abs(bladeCoord - sweepPos);
                half sweepBand = smoothstep(_GlintWidth, 0.0, sweepDist);
                // Segunda linha fina acompanhando o brilho principal (efeito reflexo anime/storybook)
                half subBand = smoothstep(_GlintWidth * 0.45, 0.0, abs(bladeCoord - (sweepPos - _GlintWidth * 1.35))) * 0.55;
                half sweepVisibility = 1.0 - smoothstep(0.58, 0.70, sweepCycle);
                half glintMask = (sweepBand + subBand) * sweepVisibility;

                // Reflexo especular adicional ligado ao angulo da camera (sempre vivo ao mover/atacar)
                half viewSheen = pow(saturate(1.0 - abs(dot(N, V) - 0.58) * 3.2), 3.0) * 0.38;

                half3 glintContrib = _GlintColor.rgb * (glintMask * _GlintIntensity + viewSheen * _GlintIntensity * 0.45);

                half3 finalColor = diffuse + metalReflect + specColor + rimGlow + glintContrib;
                return half4(finalColor, 1.0);
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
            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

            float3 _LightDirection;

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
            };

            Varyings ShadowPassVertex(Attributes input)
            {
                Varyings output;
                float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
                float3 normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.positionCS = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, _LightDirection));
                return output;
            }

            half4 ShadowPassFragment(Varyings input) : SV_Target
            {
                return 0;
            }
            ENDHLSL
        }
    }
    FallBack "Universal Render Pipeline/Lit"
}
