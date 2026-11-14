Shader "YunaLegend/DresserFloorReceiver"
{
    Properties { _MainTex("Approved floor painting",2D)="white" {} }
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
            float4 _RoomBounds; float _Strength,_FloorY;
            struct A { float4 positionOS:POSITION;float2 uv:TEXCOORD0; };
            struct V { float4 positionCS:SV_POSITION;float2 uv:TEXCOORD0;float2 world:TEXCOORD1; };
            V vert(A v) { V o;float3 w=TransformObjectToWorld(v.positionOS.xyz);o.world=w.xy;
                o.positionCS=TransformWorldToHClip(w);o.uv=v.uv;return o; }
            half4 frag(V v):SV_Target
            {
                float2 uv=(v.world-_RoomBounds.xy)/_RoomBounds.zw;
                half3 floor=SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,uv).rgb;
                float2 d=(v.uv-.5)*2;
                float mask=pow(saturate(1-dot(d,d)),2);
                mask*=1-smoothstep(_FloorY-.04,_FloorY+.06,v.world.y);
                return half4(floor*float3(.24,.29,.38),mask*.75*_Strength);
            }
            ENDHLSL
        }
    }
}
