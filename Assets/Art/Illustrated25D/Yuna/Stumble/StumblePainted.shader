Shader "YunaLegend/IllustratedStumbleCandidate"
{
 Properties { [PerRendererData] _MainTex ("Painting", 2D)="white" {} }
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
   #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/Core2D.hlsl"
   TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);
   struct A { float4 positionOS:POSITION; float2 uv:TEXCOORD0; };
   struct V { float4 positionCS:SV_POSITION; float2 uv:TEXCOORD0; };
   V vert(A v)
   {
    V o;
    SetUpSpriteInstanceProperties();
    o.positionCS=TransformObjectToHClip(UnityFlipSprite(v.positionOS.xyz,unity_SpriteProps.xy));
    o.uv=v.uv;
    return o;
   }
   half4 frag(V v):SV_Target
   {
    half4 c=SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,v.uv);
    // Raw generator alpha is preserved on disk. Suppress only low-alpha canvas haze.
    c.a*=smoothstep(.075,.18,c.a);
    return c;
   }
   ENDHLSL
  }
 }
}
