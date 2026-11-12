Shader "YunaLegend/IllustratedIdleCandidate"
{
    Properties
    {
        _MainTex ("Approved painting", 2D) = "white" {}
        _BlinkTex ("Painted closed eye", 2D) = "white" {}
        _Blink ("Blink", Range(0,1)) = 0
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "RenderPipeline"="UniversalPipeline" }
        Pass
        {
            Tags { "LightMode"="UniversalForward" }
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);
            TEXTURE2D(_BlinkTex); SAMPLER(sampler_BlinkTex);
            CBUFFER_START(UnityPerMaterial)
            float _Blink;
            CBUFFER_END
            struct Attributes { float4 positionOS : POSITION; float2 uv : TEXCOORD0; };
            struct Varyings { float4 positionCS : SV_POSITION; float2 uv : TEXCOORD0; };
            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                return output;
            }
            half4 frag(Varyings input) : SV_Target
            {
                half4 original = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
                half4 closed = SAMPLE_TEXTURE2D(_BlinkTex, sampler_BlinkTex, input.uv);
                // Use generated paint only inside the eye. Outside: original RGB and alpha.
                float2 pixel = float2(input.uv.x * 1024, (1 - input.uv.y) * 1536);
                float mask = smoothstep(643,653,pixel.x) * (1-smoothstep(729,739,pixel.x))
                           * smoothstep(351,361,pixel.y) * (1-smoothstep(458,468,pixel.y));
                original.rgb = lerp(original.rgb, closed.rgb, saturate(_Blink) * mask);
                return original;
            }
            ENDHLSL
        }
    }
}
