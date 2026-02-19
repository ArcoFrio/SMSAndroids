Shader "Sprites/JiggleSprite"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _MaskTex ("Jiggle Mask (R=Bounce, G=Wave, B=Noise, A=Intensity)", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)

        [Header(Bounce Jiggle (Red Channel))]
        _JiggleSpeed ("Bounce Speed", Float) = 1.0
        _JiggleStrength ("Bounce Strength", Float) = 0.05

        [Header(Wave Jiggle (Green Channel))]
        _JiggleFrequency ("Wave Frequency", Float) = 10.0

        [Header(Noise Jiggle (Blue Channel))]
        _NoiseScale ("Noise Scale", Float) = 20.0
        _NoiseSpeed ("Noise Speed", Float) = 0.5
        _NoiseStrength ("Noise Strength", Float) = 0.03

        [Header(Rendering)]
        [Toggle] _PixelSnap ("Pixel Snap", Float) = 0
        [HideInInspector] _RendererColor ("RendererColor", Color) = (1,1,1,1)
        [HideInInspector] _Flip ("Flip", Vector) = (1,1,1,1)
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

        Cull Off
        Lighting Off
        ZWrite Off
        Blend One OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #pragma multi_compile _ PIXELSNAP_ON

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            sampler2D _MaskTex;
            fixed4 _Color;
            fixed4 _RendererColor;
            float4 _Flip;
            float4 _MainTex_ST;

            float _JiggleSpeed;
            float _JiggleStrength;
            float _JiggleFrequency;
            float _NoiseScale;
            float _NoiseSpeed;
            float _NoiseStrength;

            // Simple hash-based noise function
            float hash(float2 p)
            {
                float3 p3 = frac(float3(p.xyx) * 0.1031);
                p3 += dot(p3, p3.yzx + 33.33);
                return frac((p3.x + p3.y) * p3.z);
            }

            float noise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                f = f * f * (3.0 - 2.0 * f);

                float a = hash(i);
                float b = hash(i + float2(1.0, 0.0));
                float c = hash(i + float2(0.0, 1.0));
                float d = hash(i + float2(1.0, 1.0));

                return lerp(lerp(a, b, f.x), lerp(c, d, f.x), f.y);
            }

            v2f vert(appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                v.vertex.xy *= _Flip.xy;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
                o.color = v.color * _Color * _RendererColor;

                #ifdef PIXELSNAP_ON
                o.vertex = UnityPixelSnap(o.vertex);
                #endif

                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 uv = i.texcoord;

                // Sample the jiggle mask
                fixed4 mask = tex2D(_MaskTex, uv);
                float intensity = mask.a;

                // Bounce jiggle (Red channel) - up-down movement
                float bounce = sin(_Time.y * _JiggleSpeed) * _JiggleStrength * mask.r * intensity;

                // Wave jiggle (Green channel) - left-right movement
                float wave = sin(uv.y * _JiggleFrequency + _Time.y * _JiggleSpeed) * _JiggleStrength * mask.g * intensity;

                // Noise jiggle (Blue channel) - noise-based distortion
                float2 noiseUV = uv * _NoiseScale + _Time.y * _NoiseSpeed;
                float noiseX = (noise(noiseUV) - 0.5) * 2.0 * _NoiseStrength * mask.b * intensity;
                float noiseY = (noise(noiseUV + float2(43.0, 17.0)) - 0.5) * 2.0 * _NoiseStrength * mask.b * intensity;

                // Apply distortion to UVs
                uv.x += wave + noiseX;
                uv.y += bounce + noiseY;

                // Sample and output
                fixed4 c = tex2D(_MainTex, uv) * i.color;
                c.rgb *= c.a;
                return c;
            }
            ENDCG
        }
    }

    Fallback "Sprites/Default"
}
