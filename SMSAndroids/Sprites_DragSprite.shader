Shader "Sprites/DragSprite"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _MaskTex ("Drag Mask (R=Drag, G=Stretch, B=Wobble, A=Range)", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)

        [Header(Mouse Input  set by script)]
        _GrabX ("Grab Origin X (UV)", Float) = 0.5
        _GrabY ("Grab Origin Y (UV)", Float) = 0.5
        _DragX ("Drag Delta X (UV)", Float) = 0.0
        _DragY ("Drag Delta Y (UV)", Float) = 0.0

        [Header(Drag Range)]
        _DragRadius ("Drag Radius (UV distance)", Range(0.01, 2.0)) = 0.25
        _DragAspect ("Drag Aspect (X/Y ratio)", Range(0.1, 4.0)) = 1.0

        [Header(Pull  Red Channel)]
        _PullAmount ("Pull Amount (multiplier)", Range(0.0, 2.0)) = 1.0

        [Header(Stretch  Green Channel)]
        _StretchAmount ("Stretch Amount", Range(0.0, 1.0)) = 0.3
        _StretchMaxOffset ("Stretch Max UV Offset", Range(0.01, 0.5)) = 0.15

        [Header(Wobble  Blue Channel)]
        _WobbleFrequency ("Wobble Frequency", Float) = 15.0
        _WobbleStrength ("Wobble Strength", Float) = 0.02

        [Header(Smoothing)]
        _EdgeSmoothness ("Edge Smoothness", Range(0.001, 0.5)) = 0.08
        _CenterRadius ("Center Dead Zone Radius", Range(-0.5, 0.5)) = 0.0
        _CenterSmoothness ("Center Smoothness (ramp width)", Range(0.0, 0.5)) = 0.02

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

            float _GrabX;
            float _GrabY;
            float _DragX;
            float _DragY;
            float _DragRadius;
            float _DragAspect;
            float _PullAmount;
            float _StretchAmount;
            float _StretchMaxOffset;
            float _WobbleFrequency;
            float _WobbleStrength;
            float _EdgeSmoothness;
            float _CenterRadius;
            float _CenterSmoothness;

            // Distance from pixel to grab point, with aspect ratio control
            float grabDistance(float2 uv, float2 grab)
            {
                float2 delta = uv - grab;
                delta.x *= _DragAspect;
                return length(delta);
            }

            // Smooth falloff: 0 inside centerRadius, ramps up over centerSmooth,
            // holds until the outer edge, then fades over edgeSmooth.
            // Uses a cubic ease for a fluid, organic feel.
            float dragCurve(float dist, float radius, float edgeSmooth, float centerRadius, float centerSmooth)
            {
                // Outer edge: smooth fade to zero
                float outerInner = radius - edgeSmooth;
                float outerFalloff = 1.0 - smoothstep(max(outerInner, 0.0), radius, dist);
                // Center: hard dead zone of size centerRadius, then ramp up over centerSmooth
                // Negative centerRadius shifts the ramp start below zero,
                // meaning the effect is already easing in at the grab point itself.
                float centerStart = centerRadius;
                float centerEnd   = centerStart + max(centerSmooth, 0.0001);
                float centerRamp  = smoothstep(centerStart, centerEnd, dist);
                // Combine and apply cubic ease for extra smoothness
                float f = outerFalloff * centerRamp;
                return f * f * (3.0 - 2.0 * f);
            }

            // Attenuate an offset near UV edges to prevent sampling outside [0,1]
            float edgeAttenuation(float2 uv, float margin)
            {
                float2 d = min(uv, 1.0 - uv);
                return saturate(min(d.x, d.y) / max(margin, 0.001));
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
                float2 grab = float2(_GrabX, _GrabY);
                float2 drag = float2(_DragX, _DragY);
                float dragLen = length(drag);

                // Sample the mask at original UVs
                fixed4 mask = tex2D(_MaskTex, uv);
                // Alpha channel controls the effective range of the effect
                float rangeMask = mask.a;

                // Distance from this pixel to the grab origin
                float dist = grabDistance(uv, grab);

                // Effective radius is modulated by the mask alpha:
                // where alpha is low, the radius shrinks (effect doesn't reach)
                float effectiveRadius = _DragRadius * rangeMask;

                // Smooth falloff within the effective radius
                float falloff = dragCurve(dist, effectiveRadius, _EdgeSmoothness, _CenterRadius, _CenterSmoothness);

                // --- Red Channel: Direct Pull ---
                // Pixels near the grab point follow the drag direction.
                // Shift UVs opposite to drag so the texture appears to move WITH the cursor.
                float pullWeight = falloff * mask.r * _PullAmount;
                uv -= drag * pullWeight;

                // --- Green Channel: Stretch ---
                // Taffy-like elongation: pixels behind the drag get pulled along,
                // creating a stretched wake behind the grab point.
                float stretchBase = falloff * mask.g * _StretchAmount;
                float2 toGrab = uv - grab;
                float toGrabLen = max(length(toGrab), 0.001);
                float2 toGrabDir = toGrab / toGrabLen;

                // Use safe drag direction (avoid zero-length normalize)
                float2 dragDir = dragLen > 0.001 ? drag / dragLen : float2(0, 0);

                // Pixels on the opposite side of the drag feel the stretch most
                float behindFactor = saturate(-dot(dragDir, toGrabDir) * 0.5 + 0.5);

                // Scale by distance from grab (normalized by radius) so pixels
                // right at the grab point don't get wild offsets
                float distScale = saturate(toGrabLen / max(effectiveRadius, 0.001));

                // Compute stretch offset, clamped to prevent extreme UV jumps
                float stretchMag = stretchBase * behindFactor * min(dragLen, _StretchMaxOffset) * distScale;
                float2 stretchOffset = toGrabDir * stretchMag;

                // Attenuate near UV edges to prevent wrapping/copies
                stretchOffset *= edgeAttenuation(uv, 0.08);

                uv += stretchOffset;

                // --- Blue Channel: Wobble ---
                // Oscillating jelly-like distortion, proportional to current drag.
                // Naturally active during drag and during bounce-back.
                float wobblePhase = dist * _WobbleFrequency - _Time.y * 6.0;
                float2 toGrabNow = uv - grab;
                float toGrabNowLen = max(length(toGrabNow), 0.001);
                float2 wobbleDir = toGrabNow / toGrabNowLen;
                float wobble = sin(wobblePhase) * _WobbleStrength * falloff * mask.b * dragLen;
                uv += wobbleDir * wobble;

                // If distorted UVs are outside [0,1], the texture was pulled
                // away from here — render transparent (like pulling dough away
                // leaves an empty space behind).
                if (uv.x < 0.0 || uv.x > 1.0 || uv.y < 0.0 || uv.y > 1.0)
                {
                    return fixed4(0, 0, 0, 0);
                }

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
