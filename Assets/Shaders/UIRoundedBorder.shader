Shader "UI/UIRoundedBorder"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        [HDR] _Color ("Tint", Color) = (1,1,1,1)
        
        [HDR] _BorderColor ("Border Color", Color) = (1, 1, 1, 1)
        _Dimensions ("RectTransform Dimensions (Width, Height)", Vector) = (3, 8, 0, 0)
        _Radius ("Corner Radius", Float) = 0.5
        _BorderThickness ("Inner Border Thickness", Float) = 0.2

        // Required properties for Unity UI (Masking, Clipping)
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord  : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            fixed4 _Color;
            float4 _ClipRect;

            float4 _BorderColor;
            float2 _Dimensions;
            float _Radius;
            float _BorderThickness;

            float RoundedRectSDF(float2 p, float2 b, float r)
            {
                float2 d = abs(p) - b + float2(r, r);
                return min(max(d.x, d.y), 0.0) + length(max(d, 0.0)) - r;
            }

            v2f vert(appdata_t v)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                OUT.worldPosition = v.vertex;
                OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);
                OUT.texcoord = v.texcoord;

                // Grab the UI Image color (this is what makes Button tinting work!)
                OUT.color = v.color * _Color;
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                float2 p = (IN.texcoord - 0.5) * _Dimensions;
                float2 extents = _Dimensions / 2.0;

                float dist = RoundedRectSDF(p, extents, _Radius);

                // fwidth creates a perfect 1-pixel anti-aliased edge, regardless of camera zoom!
                float pixelWidth = fwidth(dist);
                float alpha = smoothstep(pixelWidth, -pixelWidth, dist);
                
                // Border interior transition
                float borderAlpha = smoothstep(-_BorderThickness + pixelWidth, -_BorderThickness - pixelWidth, dist);

                // Lerp between the Border Color and the UI Image Color (IN.color)
                fixed4 finalColor = lerp(_BorderColor, IN.color, borderAlpha);
                finalColor.a *= alpha;

                #ifdef UNITY_UI_CLIP_RECT
                finalColor.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
                #endif

                #ifdef UNITY_UI_ALPHACLIP
                clip (finalColor.a - 0.001);
                #endif

                return finalColor;
            }
            ENDCG
        }
    }
}
