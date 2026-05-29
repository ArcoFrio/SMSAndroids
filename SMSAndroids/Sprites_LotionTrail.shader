Shader "Sprites/LotionTrail"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color   ("Tint", Color) = (1,1,1,0.6)

        [Header(Edge Softness)]
        _EdgeSoftness ("Cross-Width Softness", Range(0.01, 0.5)) = 0.15

        [Header(Stencil)]
        _StencilRef  ("Stencil Ref", Float) = 254
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent+1"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
        }

        Cull Off
        Lighting Off
        ZWrite Off

        // Standard alpha blend: overlapping layers build up opacity
        // like successive coats of varnish / lotion.
        Blend SrcAlpha OneMinusSrcAlpha

        Stencil
        {
            Ref  [_StencilRef]
            Comp Less
            Pass Replace
        }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                fixed4 color  : COLOR;
                float2 uv     : TEXCOORD0;
            };

            sampler2D _MainTex;
            float4    _MainTex_ST;
            fixed4    _Color;
            float     _EdgeSoftness;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv     = TRANSFORM_TEX(v.texcoord, _MainTex);
                o.color  = v.color;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 tex = tex2D(_MainTex, i.uv);
                fixed4 c   = tex * _Color * i.color;

                // Cross-width softness (V = 0..1 across the ribbon)
                float v    = i.uv.y;
                float fade = smoothstep(0.0, _EdgeSoftness, v)
                           * smoothstep(1.0, 1.0 - _EdgeSoftness, v);
                fade = fade * fade * (3.0 - 2.0 * fade);
                c.a *= fade;

                return c;
            }
            ENDCG
        }
    }

    Fallback "Sprites/Default"
}
