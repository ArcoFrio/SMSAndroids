using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Paints a uniform varnish/lotion layer along the contour-follower path.
/// While clicking the layer is fully opaque; on release the entire layer
/// fades out uniformly over <see cref="fadeOutTime"/> like lotion absorbing.
/// 
/// Built as a quad‐strip mesh — positions and perpendiculars are locked at
/// sample time so nothing ever reshapes, and multiple layers stack via
/// standard alpha blending (overlapping = more opaque).
///
/// Uses the Sprites/LotionTrail shader for stencil masking + soft edges.
/// Attach to the same GameObject as SqueezeContourFollower.
/// </summary>
public class LotionTrail : MonoBehaviour
{
    // ──────────────────────────────────────────────────────────────
    //  Inspector
    // ──────────────────────────────────────────────────────────────

    [Header("Brush")]
    [Tooltip("Ribbon width in world units.")]
    public float width = 1.5f;

    [Tooltip("Minimum distance before a new point is sampled.")]
    public float minVertexDistance = 0.01f;

    [Header("Color")]
    [Tooltip("Varnish tint. Alpha controls how opaque the layer is while painting.")]
    public Color color = new Color(1f, 1f, 0.92f, 0.6f);

    [Header("Fade")]
    [Tooltip("Seconds after release before the layer fully vanishes.")]
    public float fadeOutTime = 2.5f;

    [Header("Material")]
    [Tooltip("Material using Sprites/LotionTrail shader. Auto-created if empty.")]
    public Material trailMaterial;

    [Tooltip("Cross-width edge softness.")]
    [Range(0.01f, 0.5f)]
    public float edgeSoftness = 0.5f;

    [Header("Smoothing")]
    [Tooltip("Position smoothing — lower values smooth more. 1 = no smoothing.")]
    [Range(0.1f, 1f)]
    public float smoothing = 1f;

    [Tooltip("Minimum perpendicular scale at sharp corners (fraction of full width).")]
    [Range(0.1f, 1f)]
    public float minCornerWidth = 0.3f;

    [Tooltip("Minimum Y-delta to register a direction change and split the strip.")]
    public float directionChangeThreshold = 0.03f;

    [Tooltip("Segments in each rounded end-cap (0 = flat).")]
    [Range(0, 16)]
    public int capSegments = 8;

    [Header("Sorting")]
    [Tooltip("Sorting layer — match the body sprite.")]
    public string sortingLayerName = "Default";

    [Tooltip("Order in layer (must be > body sprite order).")]
    public int sortingOrder = 2;

    [Header("Offset")]
    [Tooltip("Horizontal offset (world units) applied to every sampled position.\n" +
             "Useful for nudging the trail left or right relative to the follower pivot.")]
    public float offsetX = 0f;

    [Header("Emit Control")]
    [Tooltip("Paint only while the mouse button is held.")]
    public bool emitOnlyWhileClicking = true;

    [Tooltip("Mouse button index (0=Left, 1=Right, 2=Middle).")]
    public int mouseButton = 0;

    // ──────────────────────────────────────────────────────────────
    //  Shader property IDs
    // ──────────────────────────────────────────────────────────────

    private static readonly int EdgeSoftnessProp = Shader.PropertyToID("_EdgeSoftness");
    private static readonly int StencilRefProp   = Shader.PropertyToID("_StencilRef");

    private const int Queue_Trail = 3001; // Transparent+1

    // ──────────────────────────────────────────────────────────────
    //  Internal data
    // ──────────────────────────────────────────────────────────────

    private struct StripPoint
    {
        public Vector3 position;       // world — locked
        public Vector3 perpendicular;  // extrusion dir — locked
    }

    private class VarnishStrip
    {
        public List<StripPoint> points = new List<StripPoint>();
        public GameObject       go;
        public MeshFilter       filter;
        public MeshRenderer     renderer;
        public Mesh             mesh;
        public Material         mat;
        public bool             fading;     // released — fading out
        public float            fadeStart;  // Time.time when fading began
    }

    private VarnishStrip activeStrip;
    private readonly List<VarnishStrip> allStrips = new List<VarnishStrip>();
    private bool wasClicking;

    // Smoothing state — reset on each new click
    private Vector3 smoothedPos;
    private bool    hasSmoothedPos;

    // Stencil: each directional sub-strip gets a unique ref to prevent self-overlap
    private int nextStencilRef = 254;

    // Y-direction tracking for strip splitting
    private int lastYDirection; // +1 = up, -1 = down, 0 = undetermined

    // ──────────────────────────────────────────────────────────────
    //  Lifecycle
    // ──────────────────────────────────────────────────────────────

    private void Update()
    {
        if (!emitOnlyWhileClicking)
        {
            if (activeStrip == null) SpawnStrip();
            SamplePoint(activeStrip);
        }
        else
        {
            bool clicking = Input.GetMouseButton(mouseButton);

            if (clicking && !wasClicking)
            {
                SpawnStrip();
                lastYDirection = 0;
                hasSmoothedPos = false;
            }
            if (clicking && activeStrip != null) SamplePoint(activeStrip);
            if (!clicking && wasClicking) FreezeActiveStrip();

            wasClicking = clicking;
        }

        UpdateAllStrips();

        if (activeStrip != null && activeStrip.mat != null)
            SyncMaterial(activeStrip.mat);
    }

    private void OnDestroy()
    {
        foreach (var s in allStrips) DestroyStrip(s);
        allStrips.Clear();
    }

    // ──────────────────────────────────────────────────────────────
    //  Strip lifecycle
    // ──────────────────────────────────────────────────────────────

    private void SpawnStrip()
    {
        if (activeStrip != null) FreezeActiveStrip();

        // Reset stencil counter when no other strips are alive
        if (allStrips.Count == 0)
            nextStencilRef = 254;

        var s   = new VarnishStrip();
        s.go    = new GameObject("LotionVarnish");
        s.go.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
        s.go.transform.localScale = Vector3.one;

        s.filter   = s.go.AddComponent<MeshFilter>();
        s.renderer = s.go.AddComponent<MeshRenderer>();
        s.mesh     = new Mesh { name = "VarnishMesh" };
        s.mesh.MarkDynamic();
        s.filter.mesh = s.mesh;

        // Each strip gets a unique stencil ref to prevent pixel-level self-overlap
        int stencilRef = nextStencilRef;
        nextStencilRef = Mathf.Max(nextStencilRef - 1, 1);

        s.mat = CreateMaterial(stencilRef);
        s.renderer.sharedMaterial    = s.mat;
        s.renderer.sortingLayerName  = sortingLayerName;
        s.renderer.sortingOrder      = sortingOrder;
        s.renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        s.renderer.receiveShadows    = false;

        s.fading = false;
        activeStrip = s;
        allStrips.Add(s);
    }

    private void FreezeActiveStrip()
    {
        if (activeStrip == null) return;
        activeStrip.fading    = true;
        activeStrip.fadeStart = Time.time;
        activeStrip = null;
    }

    private void DestroyStrip(VarnishStrip s)
    {
        if (s.mesh != null) Destroy(s.mesh);
        if (s.mat  != null) Destroy(s.mat);
        if (s.go   != null) Destroy(s.go);
    }

    // ──────────────────────────────────────────────────────────────
    //  Sampling — positions + perps locked once
    // ──────────────────────────────────────────────────────────────

    private void SamplePoint(VarnishStrip strip)
    {
        // ── Smooth the raw cursor position ──
        Vector3 rawPos = transform.position;
        rawPos.x += offsetX;
        if (!hasSmoothedPos) { smoothedPos = rawPos; hasSmoothedPos = true; }
        else                  smoothedPos = Vector3.Lerp(smoothedPos, rawPos, smoothing);

        Vector3 pos = smoothedPos;
        int n = strip.points.Count;

        if (n == 0)
        {
            // First point — cursor. Perp is zero (sentinel); will be
            // finalized when the second point arrives.
            strip.points.Add(new StripPoint { position = pos, perpendicular = Vector3.zero });
            return;
        }

        Vector3 last  = strip.points[n - 1].position;
        Vector3 delta = pos - last;
        if (delta.sqrMagnitude < minVertexDistance * minVertexDistance) return;

        Vector3 dir  = delta.normalized;
        Vector3 perp = new Vector3(-dir.y, dir.x, 0f);

        // ── Y-direction change detection ──
        // When the trail reverses vertical direction, freeze the current
        // strip and start a new one.  Each strip uses a unique stencil ref
        // so it can’t overdraw itself, but CAN overlap previous strips.
        if (Mathf.Abs(delta.y) > directionChangeThreshold)
        {
            int newDir = delta.y > 0f ? 1 : -1;

            if (lastYDirection == 0)
            {
                // First meaningful Y movement — establish direction
                lastYDirection = newDir;
            }
            else if (newDir != lastYDirection)
            {
                // Direction reversed — split into a new strip
                FinalizeCursorPerp(strip, perp);
                Vector3 bridgePos = strip.points[strip.points.Count - 1].position;

                FreezeActiveStrip();
                SpawnStrip();

                // Bridge: seed the new strip at the split position so
                // the next sample connects seamlessly
                activeStrip.points.Add(new StripPoint
                    { position = bridgePos, perpendicular = Vector3.zero });

                lastYDirection = newDir;
                return; // next frame continues sampling into the new strip
            }
        }

        // Finalize the previous cursor point’s perpendicular
        FinalizeCursorPerp(strip, perp);

        // Add the new point as the current cursor (invisible head).
        // Its perp will be finalized when the NEXT point is sampled.
        strip.points.Add(new StripPoint { position = pos, perpendicular = perp });
    }

    /// <summary>
    /// Writes the averaged perpendicular to the last point in the strip.
    /// The average is intentionally NOT re-normalized: its natural length
    /// is cos(halfAngle), which narrows the ribbon at sharp turns and
    /// prevents quads from overlapping on the inner edge.
    /// </summary>
    private void FinalizeCursorPerp(VarnishStrip strip, Vector3 newPerp)
    {
        int idx = strip.points.Count - 1;
        if (idx < 0) return;

        var pt = strip.points[idx];

        if (pt.perpendicular.sqrMagnitude < 0.001f)
        {
            pt.perpendicular = newPerp;
        }
        else
        {
            Vector3 avg = (pt.perpendicular + newPerp) * 0.5f;
            float   len = avg.magnitude;
            if (len < 0.001f)
                pt.perpendicular = pt.perpendicular.normalized * minCornerWidth;
            else
            {
                float scale = Mathf.Max(len, minCornerWidth);
                pt.perpendicular = (avg / len) * scale;
            }
        }

        strip.points[idx] = pt;
    }

    // ──────────────────────────────────────────────────────────────
    //  Update — rebuild meshes, handle fading, prune dead strips
    // ──────────────────────────────────────────────────────────────

    private void UpdateAllStrips()
    {
        float now = Time.time;

        for (int i = allStrips.Count - 1; i >= 0; i--)
        {
            var s = allStrips[i];

            // Compute global alpha for this strip
            float alpha;
            if (!s.fading)
            {
                alpha = 1f; // full strength while painting
            }
            else
            {
                float elapsed = now - s.fadeStart;
                alpha = 1f - Mathf.Clamp01(elapsed / fadeOutTime);
                if (alpha <= 0f)
                {
                    DestroyStrip(s);
                    allStrips.RemoveAt(i);
                    continue;
                }
            }

            // Active strips: skip the last point (the cursor) so the
            // spinning head quad never renders. Frozen strips: show all.
            int drawCount = s.fading ? s.points.Count : s.points.Count - 1;

            if (drawCount < 2)
            {
                s.mesh.Clear();
                continue;
            }

            BuildMesh(s, alpha, drawCount);
        }
    }

    // ──────────────────────────────────────────────────────────────
    //  Mesh generation — uniform width, uniform color, locked geometry
    // ──────────────────────────────────────────────────────────────

    private void BuildMesh(VarnishStrip strip, float alpha, int count)
    {
        int C = (count >= 2) ? capSegments : 0; // cap segments (needs 2+ points)
        // Each cap: 1 center + (C+1) arc verts = C+2 verts, C triangles
        int vertCount = count * 2 + (C > 0 ? 2 * (C + 2) : 0);
        int triCount  = (count - 1) * 6 + (C > 0 ? 2 * C * 3 : 0);

        var verts  = new Vector3[vertCount];
        var uvs    = new Vector2[vertCount];
        var colors = new Color[vertCount];
        var tris   = new int[triCount];

        float halfW = width * 0.5f;

        // Uniform vertex color — alpha modulated by global fade
        Color vc = color;
        vc.a *= alpha;

        // ── Ribbon quad-strip ──
        for (int i = 0; i < count; i++)
        {
            StripPoint pt = strip.points[i];
            Vector3 perp  = pt.perpendicular;

            int vi = i * 2;
            verts[vi]     = pt.position + perp * halfW;
            verts[vi + 1] = pt.position - perp * halfW;

            uvs[vi]     = new Vector2(0.5f, 0f);
            uvs[vi + 1] = new Vector2(0.5f, 1f);

            colors[vi]     = vc;
            colors[vi + 1] = vc;
        }

        int t = 0;
        for (int i = 0; i < count - 1; i++)
        {
            int vi = i * 2;
            tris[t++] = vi;     tris[t++] = vi + 2; tris[t++] = vi + 1;
            tris[t++] = vi + 1; tris[t++] = vi + 2; tris[t++] = vi + 3;
        }

        // ── Rounded caps ──
        if (C > 0)
        {
            // Helper: writes a semicircle fan into the arrays.
            // sweepForward=false → cap points backward (start cap)
            // sweepForward=true  → cap points forward  (end cap)
            // Winding for start cap must be reversed to face the camera (+Z).
            void WriteCap(Vector3 center, Vector3 perp, Vector3 fwd,
                          bool sweepForward, int vBase, ref int tIdx)
            {
                // Normalise perp so the cap is always circular regardless of miter scale
                Vector3 p = perp.sqrMagnitude > 0.001f ? perp.normalized : Vector3.right;
                Vector3 f = sweepForward ? fwd : -fwd;

                // Center
                verts[vBase]  = center;
                uvs[vBase]    = new Vector2(0.5f, 0.5f);
                colors[vBase] = vc;

                // Arc: θ 0→π, arc[0]=+p, arc[C]=−p, midpoint points in f direction
                for (int i = 0; i <= C; i++)
                {
                    float theta = i * Mathf.PI / C;
                    verts[vBase + 1 + i]  = center + halfW * (Mathf.Cos(theta) * p
                                                              + Mathf.Sin(theta) * f);
                    uvs[vBase + 1 + i]    = new Vector2(0.5f, 0.5f);
                    colors[vBase + 1 + i] = vc;
                }

                // Triangle fan — winding differs by cap side
                for (int i = 0; i < C; i++)
                {
                    if (sweepForward) // end cap: normal CW winding
                    {
                        tris[tIdx++] = vBase;
                        tris[tIdx++] = vBase + 1 + i;
                        tris[tIdx++] = vBase + 1 + i + 1;
                    }
                    else // start cap: reversed winding so face is visible
                    {
                        tris[tIdx++] = vBase;
                        tris[tIdx++] = vBase + 1 + i + 1;
                        tris[tIdx++] = vBase + 1 + i;
                    }
                }
            }

            // Start cap — points backward along the strip
            Vector3 startPerp = strip.points[0].perpendicular;
            Vector3 startFwd  = (strip.points[1].position
                                 - strip.points[0].position).normalized;
            WriteCap(strip.points[0].position, startPerp, startFwd,
                     sweepForward: false, vBase: count * 2, tIdx: ref t);

            // End cap — points forward along the strip
            Vector3 endPerp = strip.points[count - 1].perpendicular;
            Vector3 endFwd  = (strip.points[count - 1].position
                               - strip.points[count - 2].position).normalized;
            WriteCap(strip.points[count - 1].position, endPerp, endFwd,
                     sweepForward: true, vBase: count * 2 + C + 2, tIdx: ref t);
        }

        strip.mesh.Clear();
        strip.mesh.vertices  = verts;
        strip.mesh.uv        = uvs;
        strip.mesh.colors    = colors;
        strip.mesh.triangles = tris;
    }

    // ──────────────────────────────────────────────────────────────
    //  Material
    // ──────────────────────────────────────────────────────────────

    private Material CreateMaterial(int stencilRef)
    {
        Material src = trailMaterial;
        if (src == null)
        {
            Shader shader = Shader.Find("Sprites/LotionTrail");
            if (shader == null)
            {
                Debug.LogWarning("LotionTrail: 'Sprites/LotionTrail' shader not found.");
                return new Material(Shader.Find("Sprites/Default"));
            }
            src = new Material(shader);
        }
        else
        {
            src = new Material(src);
        }

        // Render queue increases with each strip so earlier strips
        // render first and later strips can draw on top of them.
        src.renderQueue = Queue_Trail + (254 - stencilRef);
        src.SetFloat(StencilRefProp, stencilRef);
        SyncMaterial(src);
        return src;
    }

    private void SyncMaterial(Material mat)
    {
        mat.SetFloat(EdgeSoftnessProp, edgeSoftness);
    }

    // ──────────────────────────────────────────────────────────────
    //  Public API
    // ──────────────────────────────────────────────────────────────

    public void ClearActiveTrail()
    {
        if (activeStrip != null) { activeStrip.points.Clear(); activeStrip.mesh.Clear(); }
    }

    public void ClearAllFrozenTrails()
    {
        for (int i = allStrips.Count - 1; i >= 0; i--)
        {
            if (allStrips[i].fading) { DestroyStrip(allStrips[i]); allStrips.RemoveAt(i); }
        }
    }

    public void ClearAll()
    {
        foreach (var s in allStrips) DestroyStrip(s);
        allStrips.Clear();
        activeStrip = null;
    }
}
