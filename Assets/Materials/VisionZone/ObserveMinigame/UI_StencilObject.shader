Shader "UI/StencilObject"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        [MainColor] _BaseColor ("Base Color", Color) = (1,1,1,1)
        _StencilRef ("Stencil Ref", Int) = 2
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True"
               "PreviewType"="Plane" "CanUseSpriteAtlas"="True" "RenderPipeline"="UniversalPipeline" }

        Stencil
        {
            Ref [_StencilRef]
            ReadMask 2       // ignore the world FOV bit
            WriteMask 2
            Comp Equal
            Pass Keep
            Fail Keep
            ZFail Keep
        }

        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes { float4 positionOS : POSITION; float4 color : COLOR; float2 uv : TEXCOORD0; };
            struct Varyings  { float4 positionHCS : SV_POSITION; half4 color : COLOR; float2 uv : TEXCOORD0; float4 positionOS : TEXCOORD1; };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                float4 _ClipRect;
            CBUFFER_END

            float Get2DClipping(float2 p, float4 r)
            {
                float2 inside = step(r.xy, p) * step(p, r.zw);
                return inside.x * inside.y;
            }

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionOS = IN.positionOS;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                OUT.color = IN.color * _BaseColor;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                half4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv) * IN.color;
                #ifdef UNITY_UI_CLIP_RECT
                color.a *= Get2DClipping(IN.positionOS.xy, _ClipRect);
                #endif
                return color;
            }
            ENDHLSL
        }
    }
}