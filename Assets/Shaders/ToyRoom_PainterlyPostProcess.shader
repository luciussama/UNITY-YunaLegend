Shader "YunaLegend/ToyRoom_PainterlyPostProcess"
{
    Properties
    {
        _KuwaharaBlend ("Kuwahara Painterly Blend", Range(0.0, 1.0)) = 0.18
        _KuwaharaRadius ("Brush Radius (Texels)", Range(0.5, 2.5)) = 0.85
        _InkColor ("Sepia Ink Color", Color) = (0.18, 0.10, 0.07, 1.0)
        _InkStrength ("Ink Line Strength", Range(0.0, 1.0)) = 0.28
        _DepthThreshold ("Depth Edge Threshold", Range(0.05, 2.0)) = 0.42
        _NormalThreshold ("Normal Edge Threshold", Range(0.1, 1.5)) = 0.58
        _PaperGrainStrength ("Watercolor Paper Grain", Range(0.0, 0.15)) = 0.024
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
        LOD 100
        ZTest Always
        ZWrite Off
        Cull Off

        Pass
        {
            Name "ToyRoomPainterlyPass"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareNormalsTexture.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half _KuwaharaBlend;
                float _KuwaharaRadius;
                half4 _InkColor;
                half _InkStrength;
                float _DepthThreshold;
                float _NormalThreshold;
                half _PaperGrainStrength;
            CBUFFER_END

            float Hash21(float2 p)
            {
                p = frac(p * float2(123.34, 456.21));
                p += dot(p, p + 45.32);
                return frac(p.x * p.y);
            }

            float ValueNoise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                float2 u = f * f * (3.0 - 2.0 * f);
                float a = Hash21(i);
                float b = Hash21(i + float2(1.0, 0.0));
                float c = Hash21(i + float2(0.0, 1.0));
                float d = Hash21(i + float2(1.0, 1.0));
                return lerp(lerp(a, b, u.x), lerp(c, d, u.x), u.y);
            }

            half3 SampleSource(float2 uv)
            {
                return SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, saturate(uv)).rgb;
            }

            // Anisotropic / Oriented 4-Quadrant Polynomial Kuwahara Filter
            half3 EvaluateAnisotropicKuwahara(float2 uv, float2 texelSize)
            {
                half3 cL = SampleSource(uv - float2(texelSize.x, 0.0));
                half3 cR = SampleSource(uv + float2(texelSize.x, 0.0));
                half3 cD = SampleSource(uv - float2(0.0, texelSize.y));
                half3 cU = SampleSource(uv + float2(0.0, texelSize.y));

                float3 gx = cR - cL;
                float3 gy = cU - cD;
                float angle = atan2(dot(gy, float3(0.299, 0.587, 0.114)), dot(gx, float3(0.299, 0.587, 0.114)) + 1e-5);
                float s = sin(angle);
                float c = cos(angle);
                float2x2 rot = float2x2(c, -s, s, c);

                float2 stepUV = texelSize * _KuwaharaRadius;

                half3 mean[4] = { half3(0,0,0), half3(0,0,0), half3(0,0,0), half3(0,0,0) };
                half3 sqSum[4] = { half3(0,0,0), half3(0,0,0), half3(0,0,0), half3(0,0,0) };

                // 4 overlapping 2x2 quadrants rotated along local gradient orientation
                [unroll]
                for (int q = 0; q < 4; ++q)
                {
                    float2 qSign = float2((q & 1) ? 1.0 : -1.0, (q & 2) ? 1.0 : -1.0);
                    [unroll]
                    for (int ix = 0; ix <= 1; ++ix)
                    {
                        [unroll]
                        for (int iy = 0; iy <= 1; ++iy)
                        {
                            float2 localOffset = float2(ix, iy) * qSign;
                            float2 rotatedOffset = mul(rot, localOffset) * stepUV;
                            half3 col = SampleSource(uv + rotatedOffset);
                            mean[q] += col;
                            sqSum[q] += col * col;
                        }
                    }
                    mean[q] *= 0.25h;
                    sqSum[q] = abs(sqSum[q] * 0.25h - mean[q] * mean[q]);
                }

                half3 weightedColor = half3(0, 0, 0);
                float totalWeight = 0.0;

                [unroll]
                for (int k = 0; k < 4; ++k)
                {
                    float variance = sqSum[k].r + sqSum[k].g + sqSum[k].b;
                    float w = 1.0 / (1.0 + pow(variance * 35.0, 4.0));
                    weightedColor += mean[k] * w;
                    totalWeight += w;
                }

                return weightedColor / max(totalWeight, 1e-4);
            }

            // Sobel Depth + Normal Edge Detection with organic hand-drawn brush jitter
            half EvaluateHandDrawnInkEdge(float2 uv, float2 texelSize)
            {
                float jitter = (ValueNoise(uv * 240.0) - 0.5) * 0.45;
                float2 d = texelSize * (1.0 + jitter * 0.35);

                float dC = LinearEyeDepth(SampleSceneDepth(uv), _ZBufferParams);
                float dL = LinearEyeDepth(SampleSceneDepth(uv + float2(-d.x, 0.0)), _ZBufferParams);
                float dR = LinearEyeDepth(SampleSceneDepth(uv + float2(d.x, 0.0)), _ZBufferParams);
                float dU = LinearEyeDepth(SampleSceneDepth(uv + float2(0.0, d.y)), _ZBufferParams);
                float dB = LinearEyeDepth(SampleSceneDepth(uv + float2(0.0, -d.y)), _ZBufferParams);

                float depthDiff = abs(dL - dR) + abs(dU - dB);
                float depthEdge = smoothstep(_DepthThreshold, _DepthThreshold * 2.2, depthDiff / max(1.0, dC * 0.15));

                float3 nL = SampleSceneNormals(uv + float2(-d.x, 0.0));
                float3 nR = SampleSceneNormals(uv + float2(d.x, 0.0));
                float3 nU = SampleSceneNormals(uv + float2(0.0, d.y));
                float3 nB = SampleSceneNormals(uv + float2(0.0, -d.y));

                float normDiff = length(nL - nR) + length(nU - nB);
                float normEdge = smoothstep(_NormalThreshold, _NormalThreshold * 1.8, normDiff);

                float brushMod = lerp(0.72, 1.15, ValueNoise(uv * 160.0));
                return saturate(max(depthEdge, normEdge * 0.75) * brushMod);
            }

            half4 Frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                float2 uv = input.texcoord;
                float2 texelSize = _BlitTexture_TexelSize.xy;
                if (texelSize.x <= 0.0)
                {
                    texelSize = 1.0 / max(_ScreenParams.xy, float2(1280.0, 720.0));
                }

                half3 rawColor = SampleSource(uv);
                half3 kuwaharaColor = EvaluateAnisotropicKuwahara(uv, texelSize);
                half3 painted = lerp(rawColor, kuwaharaColor, _KuwaharaBlend);

                // Cold-press watercolor paper grain
                float2 paperCoord = uv * float2(420.0, 240.0);
                float grain = (ValueNoise(paperCoord) * 0.65 + ValueNoise(paperCoord * 2.3) * 0.35) - 0.5;
                painted += grain * _PaperGrainStrength;

                // Hand-inked sepia contour lines
                half inkEdge = EvaluateHandDrawnInkEdge(uv, texelSize) * _InkStrength;
                painted = lerp(painted, _InkColor.rgb, inkEdge);

                return half4(saturate(painted), 1.0);
            }
            ENDHLSL
        }
    }
}
