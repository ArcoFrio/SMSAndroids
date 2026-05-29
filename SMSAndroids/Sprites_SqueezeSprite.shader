Shader "Sprites/SqueezeSprite"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _MaskTex ("Squeeze Mask (R=Body Area, G=Zoom, B=Stretch, A=Intensity)", 2D) = "white" {}
        _WobbleTex ("Wobble Mask (R=Horizontal Drift, G=Vertical Drift, B=Delayed Drift, A=Intensity)", 2D) = "black" {}
        _Color ("Tint", Color) = (1,1,1,1)

        [Header(Mouse Input  set by script)]
        _MouseX ("Mouse X Position (0-1 UV)", Range(0, 1)) = 0.5
        _MouseY ("Mouse Y Position (0-1 UV)", Range(0, 1)) = 0.5
        _Clicked ("Click Strength (0-1)", Range(0, 1)) = 0.0

        [Header(Expand  Horizontal Push Red Channel)]
        _SqueezeRadius ("Expand Radius (Y distance from mouse)", Range(0.01, 1.0)) = 0.15
        _SqueezeStrength ("Expand Strength", Range(0.0, 1.0)) = 0.3
        _ExpandUpperLimit ("Expand Upper Limit (max scale)", Range(1.0, 5.0)) = 2.5

        [Header(Push  Vertical Displacement)]
        _PushStrength ("Push Strength", Range(0.0, 0.3)) = 0.06
        _PushRadius ("Push Radius (Y distance from mouse)", Range(0.01, 1.0)) = 0.3

        [Header(Bulge  Horizontal Expansion)]
        _BulgeStrength ("Bulge Strength", Range(0.0, 0.3)) = 0.04

        [Header(Zoom  Green Channel)]
        _ZoomSpeed ("Zoom Speed", Float) = 1.5
        _ZoomStrength ("Zoom Strength", Range(0.0, 0.5)) = 0.08

        [Header(Stretch  Blue Channel  inverse of Zoom)]
        _StretchStrength ("Stretch Strength", Range(0.0, 0.5)) = 0.08

        [Header(Wobble  Second Mask)]
        _WobbleSpeed ("Drift Speed (R and G channels)", Float) = 0.3
        _WobbleStrength ("Drift Strength (R and G channels)", Range(0.0, 0.15)) = 0.02
        _WobbleDelay ("Delayed Drift Lag (B channel, seconds)", Range(0.05, 2.0)) = 0.35
        _WobbleElasticity ("Delayed Drift Elasticity (B channel bounce)", Range(0.0, 1.0)) = 0.3

        [Header(Smoothing)]
        _EdgeSmoothness ("Edge Smoothness", Range(0.001, 0.5)) = 0.06

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

        // Write stencil 255 on every visible pixel so the lotion trail
        // (Sprites/LotionTrail) can mask itself to the body silhouette.
        Stencil
        {
            Ref 255
            Comp Always
            Pass Replace
        }

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
            sampler2D _WobbleTex;
            fixed4 _Color;
            fixed4 _RendererColor;
            float4 _Flip;
            float4 _MainTex_ST;

            float _MouseX;
            float _MouseY;
            float _Clicked;
            float _SqueezeRadius;
            float _SqueezeStrength;
            float _ExpandUpperLimit;
            float _PushStrength;
            float _PushRadius;
            float _BulgeStrength;
            float _ZoomSpeed;
            float _ZoomStrength;
            float _StretchStrength;
            float _WobbleSpeed;
            float _WobbleStrength;
            float _WobbleDelay;
            float _WobbleElasticity;
            float _EdgeSmoothness;

            // Smooth bell-shaped falloff: 1.0 at center, 0.0 at radius
            float falloff(float dist, float radius, float edgeSmooth)
            {
                float inner = radius - edgeSmooth;
                float f = 1.0 - smoothstep(max(inner, 0.0), radius, dist);
                // Extra cubic ease for organic feel
                return f * f * (3.0 - 2.0 * f);
            }

            // Smooth value noise for organic wobble drift
            float hash(float2 p)
            {
                float3 p3 = frac(float3(p.xyx) * 0.1031);
                p3 += dot(p3, p3.yzx + 33.33);
                return frac((p3.x + p3.y) * p3.z);
            }

            float smoothNoise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                f = f * f * (3.0 - 2.0 * f);
                return lerp(
                    lerp(hash(i), hash(i + float2(1,0)), f.x),
                    lerp(hash(i + float2(0,1)), hash(i + float2(1,1)), f.x),
                    f.y);
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

                // Sample the red channel mask and alpha intensity at original UVs
                fixed4 mask = tex2D(_MaskTex, uv);
                float bodyMask = mask.r;
                float intensity = mask.a;

                // Y distance from this pixel to the mouse
                float dy = uv.y - _MouseY;
                float absDy = abs(dy);

                // === Horizontal Expand ===
                // Bell-shaped falloff centered at mouse Y.
                // Scale > 1 compresses the UV range, pushing texture content outward
                // toward the edges — the opposite of pinching.
                float squeezeFall = falloff(absDy, _SqueezeRadius, _EdgeSmoothness);

                // Expand weight: red mask gates effect, alpha scales it, _Clicked enables it
                float squeezeWeight = squeezeFall * bodyMask * intensity * _Clicked * _SqueezeStrength;

                // Scale factor: 1.0 = no effect, _ExpandUpperLimit = max expansion
                float scale = lerp(1.0, _ExpandUpperLimit, squeezeWeight);

                // Push texture outward from horizontal center
                uv.x = 0.5 + (uv.x - 0.5) / max(scale, 0.001);

                // === Vertical Push ===
                // Above mouse center: push the texture edges upward
                //   (shift UV down so texture appears displaced up)
                // Below mouse center: push edges downward
                //   (shift UV up so texture appears displaced down)
                float pushFall = falloff(absDy, _PushRadius, _EdgeSmoothness);
                float pushDir = sign(dy); // +1 above mouse, -1 below
                // Stronger near the squeeze zone but not right at center
                float pushProfile = pushFall * saturate(absDy / max(_PushRadius, 0.001));
                float pushOffset = pushDir * pushProfile * bodyMask * intensity * _Clicked * _PushStrength;
                uv.y -= pushOffset;

                // === Horizontal Bulge ===
                // The pushed area expands outward horizontally.
                // Pixels away from center X get nudged further out.
                float bulgeWeight = pushProfile * bodyMask * intensity * _Clicked * _BulgeStrength;
                float xFromCenter = uv.x - 0.5;
                uv.x += xFromCenter * bulgeWeight;

                // Clamp to prevent tearing — only stretch, never tear
                uv = clamp(uv, 0.001, 0.999);

                // === Wobble Mask (sampled early so B delay can influence Zoom/Stretch) ===
                fixed4 wobble = tex2D(_WobbleTex, i.texcoord);
                float wobbleIntensity = wobble.a;
                float delayMask = wobble.b * wobbleIntensity;

                // Elastic delay helpers for Zoom/Stretch and the B drift below.
                // Phase cycles through 0-1 over _WobbleDelay seconds; bounce
                // produces a damped sine ring that overshoots the target.
                float _phase = frac(_Time.y / max(_WobbleDelay, 0.001));
                float _bounce = sin(_phase * 3.14159 * 2.0) * exp(-_phase * 3.0);

                // Effective time for Zoom/Stretch: blend between current time
                // and a delayed+elastic time based on B mask presence.
                float tCurrent = _Time.y;
                float tLag = _Time.y - _WobbleDelay;
                float tElastic = tLag + (tCurrent - tLag) * _WobbleElasticity * _bounce;
                float tEffective = lerp(tCurrent, tElastic, delayMask);

                // === Zoom (Green Channel) ===
                // Time-driven UV scale centered on the texture center (0.5, 0.5).
                // Positive zoom factor contracts UVs (zooms in), negative expands (zooms out),
                // creating a rhythmic back-and-forth depth illusion.
                float zoomMask = mask.g * intensity;
                if (zoomMask > 0.0)
                {
                    float2 zoomPivot = float2(0.5, 0.5);
                    float zoomAmount = sin(tEffective * _ZoomSpeed) * _ZoomStrength * zoomMask;
                    // Scale UVs around the texture center
                    float zoomScale = 1.0 + zoomAmount;
                    uv = zoomPivot + (uv - zoomPivot) / max(zoomScale, 0.01);
                    uv = clamp(uv, 0.001, 0.999);
                }

                // === Stretch (Blue Channel of first mask) ===
                // Horizontal expansion/contraction inverse of the green Zoom.
                float stretchMask = mask.b * intensity;
                if (stretchMask > 0.0)
                {
                    // Horizontal stretch — inverse phase of green zoom
                    float stretchAmount = -sin(tEffective * _ZoomSpeed) * _StretchStrength * stretchMask;
                    float stretchScale = 1.0 + stretchAmount;
                    uv.x = 0.5 + (uv.x - 0.5) / max(stretchScale, 0.01);

                    uv = clamp(uv, 0.001, 0.999);
                }

                // === Wobble R/G/B Drift ===

                // R channel: horizontal drift (noise-based X wander)
                float hDrift = wobble.r * wobbleIntensity;
                if (hDrift > 0.0)
                {
                    float t = _Time.y * _WobbleSpeed;
                    float nx = (smoothNoise(float2(t, 0.0)) - 0.5) * 2.0;
                    uv.x += nx * _WobbleStrength * hDrift;
                }

                // G channel: vertical drift (noise-based Y wander)
                float vDrift = wobble.g * wobbleIntensity;
                if (vDrift > 0.0)
                {
                    float t = _Time.y * _WobbleSpeed;
                    float ny = (smoothNoise(float2(t + 31.7, 17.3)) - 0.5) * 2.0;
                    uv.y += ny * _WobbleStrength * vDrift;
                }

                // B channel: delayed drift (same noise as R+G but lagging + elastic)
                if (delayMask > 0.0)
                {
                    float tNow = _Time.y * _WobbleSpeed;
                    float2 current = float2(
                        (smoothNoise(float2(tNow, 0.0)) - 0.5) * 2.0,
                        (smoothNoise(float2(tNow + 31.7, 17.3)) - 0.5) * 2.0);

                    float tDelayed = (_Time.y - _WobbleDelay) * _WobbleSpeed;
                    float2 delayed = float2(
                        (smoothNoise(float2(tDelayed, 0.0)) - 0.5) * 2.0,
                        (smoothNoise(float2(tDelayed + 31.7, 17.3)) - 0.5) * 2.0);

                    float2 diff = current - delayed;
                    float2 elastic = delayed + diff * _WobbleElasticity * _bounce;

                    uv += (delayed + elastic) * 0.5 * _WobbleStrength * delayMask;
                }

                uv = clamp(uv, 0.001, 0.999);

                // Sample and output
                fixed4 c = tex2D(_MainTex, uv) * i.color;

                // Discard fully transparent pixels so they don't write
                // to the stencil buffer (keeps trail masked to body only)
                clip(c.a - 0.01);

                c.rgb *= c.a;
                return c;
            }
            ENDCG
        }
    }

    Fallback "Sprites/Default"
}
