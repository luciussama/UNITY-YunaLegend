Shader "YunaLegend/RoomPenumbraT054"
{
    Properties
    {
        [PerRendererData] _MainTex ("Approved painting", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _Sprite ("Sprite geometry", Float) = 0
        _Role ("0 background, 1 platform, 2 Yuna, 3 Nanda", Float) = 0
        _ZWrite ("Depth write", Float) = 0
        _BlinkTex ("Approved closed eye", 2D) = "white" {}
        _BlinkEnabled ("Use approved eye mask", Float) = 0
        _SuppressHaze ("Use approved stumble alpha", Float) = 0
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "RenderPipeline"="UniversalPipeline" }
        Pass
        {
            Tags { "LightMode"="UniversalForward" }
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite [_ZWrite]
            Cull Off
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/Core2D.hlsl"
            TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);
            TEXTURE2D(_BlinkTex); SAMPLER(sampler_BlinkTex);
            float _Blink, _BlinkEnabled, _SuppressHaze;
            float4 _Color, _MoonPosition, _NandaPosition;
            float _Sprite, _Role, _MoonStrength, _NandaStrength, _Phosphor;
            float4 _FloorContact0, _FloorContact1, _FloorContact2;
            float FloorContact(float2 world, float4 contact)
            {
                float2 delta = (world - float2(contact.x, contact.y - .23))
                    / float2(max(.1, contact.z) * 1.25, .65);
                // Reflected light on the painted floor, never on the wall.
                return exp(-dot(delta, delta) * 1.5)
                    * (1-smoothstep(contact.y-.04, contact.y+.08, world.y)) * contact.w;
            }
            struct A { float4 positionOS:POSITION; float2 uv:TEXCOORD0; float4 color:COLOR; };
            struct V { float4 positionCS:SV_POSITION; float2 uv:TEXCOORD0; float3 world:TEXCOORD1; float4 color:COLOR; };
            V vert(A v)
            {
                V o;
                SetUpSpriteInstanceProperties();
                float3 p = v.positionOS.xyz;
                if (_Sprite > .5) p = UnityFlipSprite(p, unity_SpriteProps.xy);
                o.world = TransformObjectToWorld(p);
                o.positionCS = TransformWorldToHClip(o.world);
                o.uv = v.uv;
                o.color = _Color;
                if (_Sprite > .5) o.color *= v.color;
                return o;
            }
            half4 frag(V v):SV_Target
            {
                half4 c = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, v.uv) * v.color;
                if (_BlinkEnabled > .5)
                {
                    float2 pixel = float2(v.uv.x * 1024, (1-v.uv.y) * 1536);
                    float mask = smoothstep(643,653,pixel.x) * (1-smoothstep(729,739,pixel.x))
                        * smoothstep(351,361,pixel.y) * (1-smoothstep(458,468,pixel.y));
                    c.rgb = lerp(c.rgb, SAMPLE_TEXTURE2D(_BlinkTex,sampler_BlinkTex,v.uv).rgb,
                        saturate(_Blink) * mask);
                }
                if (_SuppressHaze > .5) c.a *= smoothstep(.075,.18,c.a);
                if (_Role > 2.5) return half4(c.rgb * float3(.88,1.02,1.18),c.a);
                // These are projected illustration lights, in the existing 3D world.
                // They never move the physical support or modify the source painting.
                float below = _MoonPosition.y - v.world.y;
                float width = 1.15 + max(0,below) * .27;
                float dx = v.world.x - (_MoonPosition.x + max(0,below) * .12);
                float moon = exp(-dx*dx/(width*width)) * smoothstep(-.45,.35,below);
                moon *= .55 / (1 + max(0,below) * .12);
                float2 fairyDelta = (v.world.xy - _NandaPosition.xy) / 3.0;
                float fairy = exp(-dot(fairyDelta,fairyDelta)*2.2) * 1.05 * _NandaStrength;
                // Very low reflected moonlight, extinguished with the moon source.
                float3 light = float3(.018,.022,.035) * _MoonStrength;
                light += moon * float3(.68,.79,1) * _MoonStrength;
                light += fairy * float3(.42,.65,1);
                float3 emission = 0;
                if (_Role > .5 && _Role < 1.5)
                {
                    light += _Phosphor * float3(.67,.85,1);
                    float value = dot(c.rgb,float3(.2126,.7152,.0722));
                    emission = _Phosphor * float3(.12,.21,.22) * saturate(value*3);
                }
                // The visible window aperture remains the source, not a new lamp.
                if (_Role < .5)
                {
                    float contact = max(FloorContact(v.world.xy, _FloorContact0),
                        max(FloorContact(v.world.xy, _FloorContact1), FloorContact(v.world.xy, _FloorContact2)));
                    light += contact * float3(.24,.29,.38) * _MoonStrength;
                    float2 window = abs((v.uv-float2(.564,.558))/float2(.038,.064));
                    float aperture = (1-smoothstep(.88,1,window.x)) * (1-smoothstep(.88,1,window.y));
                    light = lerp(light,float3(.8,.9,1.05)*_MoonStrength,aperture);
                }
                return half4(c.rgb * light + emission,c.a);
            }
            ENDHLSL
        }
    }
}
