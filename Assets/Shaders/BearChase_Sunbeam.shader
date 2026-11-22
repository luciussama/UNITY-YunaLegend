Shader "YunaLegend/BearChase_Sunbeam"
{
    Properties
    {
        _BaseMap ("Gradient Map", 2D) = "white" {}
        _BaseColor ("Tint Color", Color) = (1.0, 0.92, 0.68, 0.16)
        _SoftPower ("Edge Softness", Range(0.5, 4.0)) = 1.6
        _RadialOnly ("Radial Glow Mode", Range(0.0, 1.0)) = 0.0
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "RenderPipeline"="UniversalPipeline" "Queue"="Transparent" "IgnoreProjector"="True" }
        LOD 100

        Pass
        {
            Name "SunbeamAdditive"
            Blend SrcAlpha One
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                half4 _BaseColor;
                half _SoftPower;
                half _RadialOnly;
            CBUFFER_END

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float fogFactor : TEXCOORD1;
            };

            Varyings vert(Attributes input)
            {
                Varyings output;
                VertexPositionInputs posInputs = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionHCS = posInputs.positionCS;
                output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
                output.fogFactor = ComputeFogFactor(posInputs.positionCS.z);
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                half4 tex = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv);
                half edgeFade = pow(saturate(sin(input.uv.x * 3.14159265)), _SoftPower);
                half verticalFade = smoothstep(0.0, 0.18, input.uv.y) * smoothstep(1.0, 0.15, input.uv.y);
                half mask = lerp(edgeFade * verticalFade, 1.0h, step(0.5h, _RadialOnly));
                half alpha = _BaseColor.a * tex.a * mask;
                half3 col = _BaseColor.rgb * tex.rgb;
                col = MixFog(col, input.fogFactor);
                return half4(col, alpha);
            }
            ENDHLSL
        }
    }
}
