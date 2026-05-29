using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Soft breast physics with actual mesh deformation. Converts the sprite's
/// PolygonCollider2D into a deformable mesh where each vertex has its own
/// spring. When the contour follower (hand) sweeps nearby while clicking,
/// vertices get pushed in the hand's direction of movement, creating a
/// soft lift/droop with jiggle.
///
/// Works alongside Sprites/SqueezeSprite shader — vertex deformation
/// handles the physical lift/droop motion, while the shader handles
/// squeeze/wobble/zoom effects via _MouseX/_MouseY/_Clicked.
///
/// Required on the same GameObject:
///   - SpriteRenderer (with sprite + SqueezeSprite material)
///   - PolygonCollider2D (defines the breast shape; can be trigger)
///
/// Setup:
///   1. Place breast sprite as child of body, position over the body
///   2. Add PolygonCollider2D, shape it to the breast outline
///   3. Attach this script, assign the contour follower reference
///   4. Set pinEdge to the side that attaches to the body
///   5. Tune pushStrength, stiffness, damping to taste
/// </summary>
public class BreastPhysics : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The contour follower (hand) that drives interaction.")]
    public SqueezeContourFollower contourFollower;

    [Tooltip("PolygonCollider2D defining the breast shape. Auto-detected if empty.")]
    public PolygonCollider2D shapeCollider;

    [Tooltip("SpriteRenderer for the breast. Auto-detected if empty.")]
    public SpriteRenderer spriteRenderer;

    [Header("Mesh")]
    [Tooltip("Max edge length for boundary subdivision (smaller = more vertices = softer).")]
    public float maxEdgeLength = 0.15f;

    [Tooltip("Local-space points defining the pin curve (attachment line to body). " +
             "Uses 5 points so you can shape it to match the breast's curved attachment. " +
             "Drag these in the Scene view gizmo via the Inspector.")]
    public Vector2[] pinPoints = new Vector2[]
    {
        new Vector2(-0.5f,  0.40f),
        new Vector2(-0.55f, 0.20f),
        new Vector2(-0.58f, 0.00f),
        new Vector2(-0.55f,-0.20f),
        new Vector2(-0.5f, -0.40f),
    };

    [Tooltip("Distance from the pin curve within which vertices are locked.")]
    public float pinMargin = 0.15f;

    [Tooltip("Expand the mesh outward from the collider shape by this amount (local units). " +
             "Gives the shader effects extra room so they don't get clipped at the mesh edge.")]
    public float meshExpand = 0.08f;

    [Header("Interaction")]
    [Tooltip("Mouse button that must be held (0=Left, 1=Right, 2=Middle).")]
    public int mouseButton = 0;

    [Tooltip("Collider2D on the hand that physically pushes the breast. " +
             "Add a CircleCollider2D (trigger) + kinematic Rigidbody2D to the contour follower. " +
             "Auto-detected from contourFollower if empty.")]
    public Collider2D handCollider;

    [Tooltip("Extra padding beyond the hand collider surface (world units). " +
             "Keeps vertices from touching the exact edge, reducing jitter.")]
    public float handPadding = 0.02f;

    [Tooltip("Distance beyond the hand collider surface where soft repulsion begins (world units). " +
             "Creates visible lift/deformation before the hand actually touches. " +
             "At the collider surface the push is full strength; at liftRadius it fades to zero.")]
    public float liftRadius = 0.25f;

    [Tooltip("Strength of the proximity lift force (units/s^2). " +
             "Higher = breast deforms more before the hand reaches it.")]
    public float liftStrength = 60f;

    [Tooltip("How strongly the hand's movement direction pushes the breast globally. " +
             "This is what makes downward hand motion push the breast down, etc.")]
    public float handVelocityPush = 0.3f;

    [Tooltip("How much the hand's travel direction overrides the radial escape direction " +
             "during penetration. 0 = pure radial (old behavior), 1 = pure hand velocity direction. " +
             "At ~0.6, the breast gets pushed primarily where the hand is going.")]
    [Range(0f, 1f)]
    public float velocityDirectionBias = 0.6f;

    [Tooltip("How much the breast can be pushed outward (away from pin curve) vs inward. " +
             "0 = no outward push at all, 1 = full outward push allowed.")]
    [Range(0f, 1f)]
    public float outwardLimit = 0.15f;

    [Tooltip("Fraction of the hand's per-vertex push that is distributed to the whole breast. " +
             "0 = only touched vertices move (local deformation), " +
             "1 = entire breast moves as a single unit. " +
             "Values around 0.4-0.7 give a natural 'lifting' feel.")]
    [Range(0f, 1f)]
    public float globalPushRatio = 0.5f;

    [Header("Tilt")]
    [Tooltip("Maximum positive Z-axis rotation (degrees).")]
    public float tiltMaxAngle = 10f;

    [Tooltip("Maximum negative Z-axis rotation (degrees). Use a positive number — it will be negated internally.")]
    public float tiltMinAngle = 10f;

    [Tooltip("How quickly the tilt responds to deformation changes.")]
    public float tiltSpeed = 12f;

    [Tooltip("Reverse the tilt spin direction. Use for mirrored breasts so both tilt outward.")]
    public bool tiltInvert = false;

    [Tooltip("Local X position shift per degree of tilt. " +
             "Positive values shift right when tilted positively, left when negative.")]
    public float tiltXShift = 0.005f;

    [Tooltip("Local Y position shift per degree of tilt. " +
             "Positive values shift up when tilted positively, down when negative.")]
    public float tiltYShift = 0f;

    [Header("Spring")]
    [Tooltip("Stiffness pulling vertices back to rest. Higher = snappier return.")]
    public float springStiffness = 45f;

    [Tooltip("Damping reducing jiggle. Higher = less jiggle.")]
    public float springDamping = 10f;

    [Tooltip("Maximum displacement from rest position (local units).")]
    public float maxDisplacement = 0.3f;

    [Header("Propagation")]
    [Tooltip("How much displacement smooths to neighboring boundary vertices.")]
    [Range(0f, 0.95f)]
    public float propagation = 0.85f;

    [Tooltip("Number of smoothing passes per frame.")]
    [Range(0, 10)]
    public int propagationPasses = 4;

    [Header("Debug")]
    [Tooltip("Use Sprites/Default shader instead of the sprite's material. " +
             "If the mesh becomes visible with this ON, the issue is with the original material/shader setup.")]
    public bool debugUseDefaultShader = false;

    // ===================== Internal =====================

    // Mesh data
    private int vertCount;
    private Vector2[] restLocal;      // vertex positions WITH collider offset (for mesh positioning)
    private Vector2[] restLocalNoOff; // vertex positions WITHOUT collider offset (for UV computation)
    private Vector2[] displacements;
    private Vector2[] velocities;
    private Vector2[] tempDisp; // propagation scratch buffer
    private bool[] pinned;
    private float[] leverArm; // 0 at pin line, 1 at furthest point
    private Vector2 outwardDir; // normalized direction from pin line toward breast center
    private bool[] touching;  // per-vertex: currently overlapping the hand collider
    private bool[] resolved;  // per-vertex: was touched, hand moved past — skip until release
    private int[] tris;
    private Mesh mesh;
    private Vector3[] meshVerts3;
    private Vector2 meshCenter; // bounding-box center of restLocal; meshChild sits here
    private float currentTiltAngle; // smoothed Z rotation

    // Rendering
    private GameObject meshChild;
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private MaterialPropertyBlock propBlock;
    private Camera mainCamera;

    // Shader property IDs
    private static readonly int PropMouseX = Shader.PropertyToID("_MouseX");
    private static readonly int PropMouseY = Shader.PropertyToID("_MouseY");
    private static readonly int PropClicked = Shader.PropertyToID("_Clicked");
    private float smoothMX = 0.5f, smoothMY = 0.5f, smoothClick;

    // Hand velocity tracking
    private Vector3 prevHandPos;
    private bool hasPrevHandPos;

    // ===================== Lifecycle =====================

    private void Start()
    {
        if (shapeCollider == null) shapeCollider = GetComponent<PolygonCollider2D>();
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        if (handCollider == null && contourFollower != null)
            handCollider = contourFollower.GetComponent<Collider2D>();
        mainCamera = Camera.main;
        propBlock = new MaterialPropertyBlock();

        if (shapeCollider == null || spriteRenderer == null || spriteRenderer.sprite == null)
        {
            Debug.LogError("BreastPhysics: Missing PolygonCollider2D or SpriteRenderer.");
            enabled = false;
            return;
        }

        if (!BuildMesh())
        {
            enabled = false;
            return;
        }

        CreateMeshChild();

        Debug.Log($"BreastPhysics: {vertCount} vertices, " +
                  $"{tris.Length / 3} triangles, " +
                  $"pinned={System.Array.FindAll(pinned, p => p).Length}");
    }

    private void LateUpdate()
    {
        float dt = Time.deltaTime;
        if (dt <= 0f || restLocal == null) return;

        bool interacting = Input.GetMouseButton(mouseButton) && handCollider != null;

        // --- Per-vertex spring simulation ---
        for (int i = 0; i < vertCount; i++)
        {
            if (pinned[i]) continue;

            Vector2 force = Vector2.zero;

            // Spring: pull back to rest
            force -= springStiffness * displacements[i];
            force -= springDamping * velocities[i];

            // Integrate
            velocities[i] += force * dt;
            displacements[i] += velocities[i] * dt;
        }

        // --- Propagation: smooth displacements along the boundary ring ---
        for (int pass = 0; pass < propagationPasses; pass++)
        {
            System.Array.Copy(displacements, tempDisp, vertCount);

            for (int i = 0; i < vertCount; i++)
            {
                if (pinned[i]) continue;

                int prev = (i == 0) ? vertCount - 1 : i - 1;
                int next = (i == vertCount - 1) ? 0 : i + 1;

                Vector2 avg = (tempDisp[prev] + tempDisp[i] + tempDisp[next]) / 3f;
                displacements[i] = Vector2.Lerp(tempDisp[i], avg, propagation);
            }
        }

        // --- Proximity lift + collider penetration resolution ---
        if (interacting)
        {
            Vector2 handCenter = (Vector2)handCollider.bounds.center;

            // Compute hand velocity for directional push + lift gating
            Vector2 handVel = Vector2.zero;
            if (hasPrevHandPos && dt > 0f)
                handVel = ((Vector2)(handCollider.transform.position - prevHandPos)) / dt;
            prevHandPos = handCollider.transform.position;
            hasPrevHandPos = true;

            // Accumulators for global push distribution
            Vector2 penPushSum  = Vector2.zero;
            float   penPushW    = 0f;
            Vector2 liftPushSum = Vector2.zero;
            float   liftPushW   = 0f;
            bool    anyContact  = false;

            for (int i = 0; i < vertCount; i++)
            {
                if (pinned[i]) continue;

                // Skip vertices the hand already passed over this interaction.
                if (resolved[i]) continue;

                Vector2 vWorld = (Vector2)transform.TransformPoint(
                    restLocal[i].x + displacements[i].x,
                    restLocal[i].y + displacements[i].y, 0f);

                bool inside = handCollider.OverlapPoint(vWorld);
                Vector2 closest = handCollider.ClosestPoint(vWorld);

                Vector2 pushDir;
                if (inside && Vector2.Distance(closest, vWorld) < 0.0001f)
                    pushDir = (vWorld - handCenter).normalized;
                else if (inside)
                    pushDir = (closest - vWorld).normalized;
                else
                    pushDir = (vWorld - closest).normalized;

                // --- Resolved tracking ---
                // If the vertex WAS touching the hand last frame but ISN'T
                // anymore, it means the hand has swept past it. Mark it
                // resolved so it can't collide again until mouse release.
                if (touching[i] && !inside)
                {
                    resolved[i] = true;
                    touching[i] = false;
                    continue;
                }
                touching[i] = inside;

                if (inside)
                {
                    anyContact = true;
                    Vector2 prevDisp = displacements[i];

                    // Blend push direction: mix radial escape with hand travel direction.
                    Vector2 effectiveDir = pushDir;
                    if (handVel.sqrMagnitude > 0.01f && velocityDirectionBias > 0f)
                    {
                        Vector2 velDir = handVel.normalized;
                        effectiveDir = Vector2.Lerp(pushDir, velDir, velocityDirectionBias).normalized;
                    }

                    // How deep is the vertex inside? Use that as push magnitude.
                    float depth = Vector2.Distance(vWorld, closest) + handPadding;

                    Vector2 disp = effectiveDir * depth;
                    float outComp = Vector2.Dot(disp, outwardDir);
                    if (outComp > 0f)
                        disp -= outwardDir * outComp * (1f - outwardLimit);
                    disp *= leverArm[i];

                    // Convert to local space displacement
                    Vector2 newWorld = vWorld + disp;
                    Vector2 newLocal = (Vector2)transform.InverseTransformPoint(newWorld);
                    displacements[i] = newLocal - restLocal[i];

                    // Kill velocity going against the effective push
                    float velIntoHand = Vector2.Dot(velocities[i], -effectiveDir);
                    if (velIntoHand > 0f)
                        velocities[i] += effectiveDir * velIntoHand;

                    // Accumulate for global distribution
                    penPushSum += displacements[i] - prevDisp;
                    penPushW   += leverArm[i];
                }
                else if (liftRadius > 0f)
                {
                    float dist = Vector2.Distance(vWorld, closest);
                    if (dist < liftRadius)
                    {
                        float t = 1f - (dist / liftRadius);
                        Vector2 liftForce = pushDir * (liftStrength * t * leverArm[i]);

                        float outComp = Vector2.Dot(liftForce, outwardDir);
                        if (outComp > 0f)
                            liftForce -= outwardDir * outComp * (1f - outwardLimit);

                        velocities[i] += liftForce * dt;

                        // Accumulate for global distribution
                        liftPushSum += liftForce * dt;
                        liftPushW   += leverArm[i];
                    }
                }
            }

            // --- Hand velocity impulse ---
            // Makes the breast move in the hand's direction of travel,
            // so sweeping down pushes down, sweeping up pushes up, etc.
            if (anyContact && handVel.sqrMagnitude > 0.01f && handVelocityPush > 0f)
            {
                for (int i = 0; i < vertCount; i++)
                {
                    if (pinned[i]) continue;
                    velocities[i] += handVel * handVelocityPush * leverArm[i] * dt;
                }
            }

            // --- Global push distribution ---
            // Spreads the hand's effect across the whole breast so it
            // moves as a single unit instead of deforming only locally.
            if (globalPushRatio > 0f)
            {
                if (penPushW > 0f)
                {
                    Vector2 avgPen = penPushSum / penPushW;
                    for (int i = 0; i < vertCount; i++)
                    {
                        if (pinned[i]) continue;
                        displacements[i] += avgPen * globalPushRatio * leverArm[i];
                    }
                }

                if (liftPushW > 0f)
                {
                    Vector2 avgLift = liftPushSum / liftPushW;
                    for (int i = 0; i < vertCount; i++)
                    {
                        if (pinned[i]) continue;
                        velocities[i] += avgLift * globalPushRatio * leverArm[i];
                    }
                }
            }
        }
        else
        {
            // Mouse released or no hand collider — clear all contact state
            // so the next interaction starts fresh.
            if (touching != null)
            {
                System.Array.Clear(touching, 0, vertCount);
                System.Array.Clear(resolved, 0, vertCount);
            }
            hasPrevHandPos = false;
        }

        // --- Clamp + pin enforcement ---
        for (int i = 0; i < vertCount; i++)
        {
            if (pinned[i])
            {
                displacements[i] = Vector2.zero;
                velocities[i] = Vector2.zero;
                continue;
            }

            float mag = displacements[i].magnitude;
            if (mag > maxDisplacement)
            {
                displacements[i] *= maxDisplacement / mag;
                // Kill outward velocity to prevent bounce at the limit
                float dot = Vector2.Dot(velocities[i], displacements[i].normalized);
                if (dot > 0f)
                    velocities[i] -= displacements[i].normalized * dot;
            }
        }

        // --- Update mesh vertices (relative to meshCenter) ---
        for (int i = 0; i < vertCount; i++)
        {
            meshVerts3[i].x = restLocal[i].x + displacements[i].x - meshCenter.x;
            meshVerts3[i].y = restLocal[i].y + displacements[i].y - meshCenter.y;
        }
        mesh.vertices = meshVerts3;
        mesh.RecalculateBounds();

        // --- Tilt: rotate mesh child based on hand proximity + displacement ---
        if (tiltMaxAngle > 0f && meshChild != null)
        {
            float targetAngle = 0f;

            // Component 1: vertex displacement-driven tilt
            float avgY = 0f;
            int count = 0;
            for (int i = 0; i < vertCount; i++)
            {
                if (pinned[i]) continue;
                avgY += displacements[i].y;
                count++;
            }
            if (count > 0) avgY /= count;
            float dispLimit = Mathf.Max(tiltMaxAngle, tiltMinAngle);
            float dispTilt = avgY / maxDisplacement * dispLimit;

            // Component 2: hand proximity-driven tilt (tension before contact)
            float proxTilt = 0f;
            if (interacting && handCollider != null)
            {
                Vector2 breastWorldCenter = (Vector2)transform.TransformPoint(meshCenter);
                Vector2 handCenter = (Vector2)handCollider.bounds.center;

                // Find closest vertex distance to hand surface
                float closestDist = float.MaxValue;
                for (int i = 0; i < vertCount; i++)
                {
                    if (pinned[i]) continue;
                    Vector2 vWorld = (Vector2)transform.TransformPoint(
                        restLocal[i].x + displacements[i].x,
                        restLocal[i].y + displacements[i].y, 0f);
                    Vector2 closest = handCollider.ClosestPoint(vWorld);
                    float d = Vector2.Distance(vWorld, closest);
                    if (handCollider.OverlapPoint(vWorld)) d = 0f;
                    if (d < closestDist) closestDist = d;
                }

                // Within liftRadius, the Y offset of the hand from breast
                // center drives a tilt proportional to proximity.
                if (closestDist < liftRadius)
                {
                    float proximity = 1f - (closestDist / liftRadius); // 0 at edge, 1 at contact
                    float yOffset = handCenter.y - breastWorldCenter.y;
                    // Scale so that even small Y offsets can reach the full limit
                    float maxLimit = Mathf.Max(tiltMaxAngle, tiltMinAngle);
                    proxTilt = Mathf.Clamp(yOffset * 30f, -maxLimit, maxLimit) * proximity;
                }
            }

            // Use whichever component is stronger, then optionally invert
            targetAngle = Mathf.Abs(proxTilt) > Mathf.Abs(dispTilt) ? proxTilt : dispTilt;
            if (tiltInvert) targetAngle = -targetAngle;
            // After inversion, clamp with swapped limits so the correct
            // limit applies to the correct direction.
            float clampMin = tiltInvert ? -tiltMaxAngle : -tiltMinAngle;
            float clampMax = tiltInvert ? tiltMinAngle  : tiltMaxAngle;
            targetAngle = Mathf.Clamp(targetAngle, clampMin, clampMax);

            currentTiltAngle = Mathf.Lerp(currentTiltAngle, targetAngle, dt * tiltSpeed);

            Vector3 euler = meshChild.transform.localEulerAngles;
            euler.z = currentTiltAngle;
            meshChild.transform.localEulerAngles = euler;

            Vector3 pos = meshChild.transform.localPosition;
            pos.x = meshCenter.x + currentTiltAngle * tiltXShift;
            pos.y = meshCenter.y + currentTiltAngle * tiltYShift;
            meshChild.transform.localPosition = pos;
        }

        // --- Drive squeeze shader uniforms ---
        UpdateShaderUniforms(dt);
    }

    // ===================== Mesh Generation =====================

    private bool BuildMesh()
    {
        // Read collider path (raw points are in local space relative to
        // the collider's own origin, WITHOUT the collider offset).
        Vector2[] raw = shapeCollider.GetPath(0);
        if (raw == null || raw.Length < 3)
        {
            Debug.LogError("BreastPhysics: Collider path has fewer than 3 points.");
            return false;
        }

        Vector2 offset = shapeCollider.offset;

        // Work with offset-applied points for mesh vertex positions.
        Vector2[] contour = new Vector2[raw.Length];
        for (int i = 0; i < raw.Length; i++)
            contour[i] = raw[i] + offset;

        // Also keep a parallel contour WITHOUT offset for UV computation.
        // The sprite's texture is anchored to the pivot, not to the collider offset.
        Vector2[] contourNoOff = new Vector2[raw.Length];
        for (int i = 0; i < raw.Length; i++)
            contourNoOff[i] = raw[i];

        // Ensure counter-clockwise winding (required for ear-clipping).
        // SignedArea uses the surveyor's formula: positive = CW, negative = CCW.
        // Reverse only when CW (positive) so the ear-clipper gets CCW input.
        if (SignedArea(contour) > 0f)
        {
            System.Array.Reverse(contour);
            System.Array.Reverse(contourNoOff);
        }

        // Expand each vertex outward from the polygon centroid so the
        // mesh is slightly larger than the collider. This gives the shader
        // effects (squeeze, wobble) room to breathe without being clipped.
        if (meshExpand > 0f)
        {
            Vector2 centroid = Vector2.zero;
            for (int i = 0; i < contour.Length; i++) centroid += contour[i];
            centroid /= contour.Length;

            for (int i = 0; i < contour.Length; i++)
            {
                Vector2 dir = contour[i] - centroid;
                float len = dir.magnitude;
                if (len > 0.0001f)
                {
                    Vector2 expansion = dir * (meshExpand / len);
                    contour[i]      += expansion;
                    contourNoOff[i] += expansion;
                }
            }
        }

        // Subdivide long edges for mesh resolution.
        // Must subdivide both arrays identically so they stay in sync.
        if (maxEdgeLength > 0f)
        {
            contour = Subdivide(contour, maxEdgeLength);
            contourNoOff = Subdivide(contourNoOff, maxEdgeLength);
        }

        // Ear-clip triangulate (using the offset contour for geometry)
        var verts = new List<Vector2>(contour);
        var triList = EarClip(verts);
        if (triList == null || triList.Count < 3)
        {
            Debug.LogError("BreastPhysics: Triangulation failed.");
            return false;
        }

        vertCount = verts.Count;
        restLocal = verts.ToArray();
        // restLocalNoOff has the same vertex count (same topology) but without offset
        restLocalNoOff = new List<Vector2>(contourNoOff).ToArray();
        tris = triList.ToArray();

        Debug.Log($"[BreastPhysics] Collider offset=({offset.x:F3},{offset.y:F3}), " +
                  $"raw verts={raw.Length}, subdivided={vertCount}");

        // Allocate physics arrays
        displacements = new Vector2[vertCount];
        velocities = new Vector2[vertCount];
        tempDisp = new Vector2[vertCount];
        touching = new bool[vertCount];
        resolved = new bool[vertCount];

        // Determine which vertices are pinned (body attachment edge)
        ComputePins();

        // Compute UVs mapped to the full sprite texture
        Vector2[] uvs = ComputeUVs();

        // Compute bounding-box center so the mesh child can sit at the
        // breast content's position instead of at the parent GO's pivot.
        {
            float cMinX = float.MaxValue, cMaxX = float.MinValue;
            float cMinY = float.MaxValue, cMaxY = float.MinValue;
            for (int i = 0; i < vertCount; i++)
            {
                if (restLocal[i].x < cMinX) cMinX = restLocal[i].x;
                if (restLocal[i].x > cMaxX) cMaxX = restLocal[i].x;
                if (restLocal[i].y < cMinY) cMinY = restLocal[i].y;
                if (restLocal[i].y > cMaxY) cMaxY = restLocal[i].y;
            }
            meshCenter = new Vector2((cMinX + cMaxX) * 0.5f, (cMinY + cMaxY) * 0.5f);
        }

        // Build Unity Mesh — vertices are relative to meshCenter
        meshVerts3 = new Vector3[vertCount];
        for (int i = 0; i < vertCount; i++)
            meshVerts3[i] = new Vector3(restLocal[i].x - meshCenter.x, restLocal[i].y - meshCenter.y, 0f);

        mesh = new Mesh();
        mesh.name = "BreastMesh";
        mesh.MarkDynamic();
        mesh.vertices = meshVerts3;
        mesh.uv = uvs;
        mesh.triangles = tris;

        // The SqueezeSprite shader multiplies by vertex color (v.color).
        // SpriteRenderer fills this automatically; MeshRenderer does not.
        // Without this, everything is multiplied by (0,0,0,0) → invisible.
        Color32 white = new Color32(255, 255, 255, 255);
        Color32[] colors = new Color32[vertCount];
        for (int i = 0; i < vertCount; i++)
            colors[i] = white;
        mesh.colors32 = colors;

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        return true;
    }

    private void ComputePins()
    {
        pinned = new bool[vertCount];
        leverArm = new float[vertCount];

        // Compute perpendicular distance from each vertex to the pin line segment.
        // Vertices within pinMargin are locked; lever arm is normalized distance.
        float maxDist = 0f;
        float[] dists = new float[vertCount];
        for (int i = 0; i < vertCount; i++)
        {
            dists[i] = DistToPolyline(restLocal[i], pinPoints);
            if (dists[i] > maxDist) maxDist = dists[i];
        }

        for (int i = 0; i < vertCount; i++)
        {
            pinned[i] = dists[i] <= pinMargin;
            leverArm[i] = (maxDist > 0f) ? dists[i] / maxDist : 0f;
            if (pinned[i]) leverArm[i] = 0f;
        }

        // Compute outward direction: from pin curve midpoint toward breast center.
        Vector2 pinMid = pinPoints[pinPoints.Length / 2];
        outwardDir = (meshCenter - pinMid);
        if (outwardDir.sqrMagnitude > 0.0001f)
            outwardDir.Normalize();
        else
            outwardDir = Vector2.right; // fallback
    }

    /// <summary>
    /// Shortest distance from point p to any segment of a polyline.
    /// </summary>
    private static float DistToPolyline(Vector2 p, Vector2[] pts)
    {
        float best = float.MaxValue;
        for (int i = 0; i < pts.Length - 1; i++)
        {
            float d = DistToSegment(p, pts[i], pts[i + 1]);
            if (d < best) best = d;
        }
        return best;
    }

    /// <summary>
    /// Shortest distance from point p to the line segment a→b.
    /// </summary>
    private static float DistToSegment(Vector2 p, Vector2 a, Vector2 b)
    {
        Vector2 ab = b - a;
        float lenSq = ab.sqrMagnitude;
        if (lenSq < 0.00001f) return Vector2.Distance(p, a);
        float t = Mathf.Clamp01(Vector2.Dot(p - a, ab) / lenSq);
        return Vector2.Distance(p, a + ab * t);
    }

    private Vector2[] ComputeUVs()
    {
        Sprite spr = spriteRenderer.sprite;
        Texture2D tex = spr.texture;
        Rect rect = spr.rect;
        Vector2 pivot = spr.pivot;
        float ppu = spr.pixelsPerUnit;
        float texW = tex.width;
        float texH = tex.height;

        Vector2[] uvs = new Vector2[vertCount];
        float uMin = float.MaxValue, uMax = float.MinValue;
        float vMin = float.MaxValue, vMax = float.MinValue;

        for (int i = 0; i < vertCount; i++)
        {
            // Use restLocalNoOff (without collider offset) for UVs.
            // The collider offset shifts WHERE the mesh appears, but the
            // texture content is anchored to the sprite pivot — so UVs
            // must correspond to where each vertex falls relative to
            // the pivot, not where the mesh is rendered.
            float px = rect.x + pivot.x + restLocalNoOff[i].x * ppu;
            float py = rect.y + pivot.y + restLocalNoOff[i].y * ppu;
            uvs[i] = new Vector2(px / texW, py / texH);

            if (uvs[i].x < uMin) uMin = uvs[i].x;
            if (uvs[i].x > uMax) uMax = uvs[i].x;
            if (uvs[i].y < vMin) vMin = uvs[i].y;
            if (uvs[i].y > vMax) vMax = uvs[i].y;
        }

        Debug.Log($"[BreastPhysics] Sprite: rect=({rect.x},{rect.y},{rect.width},{rect.height}), " +
                  $"pivot=({pivot.x},{pivot.y}), ppu={ppu}, tex=({texW}x{texH})");
        Debug.Log($"[BreastPhysics] UV range: U=[{uMin:F4}, {uMax:F4}], V=[{vMin:F4}, {vMax:F4}]");

        if (uMin < -0.01f || uMax > 1.01f || vMin < -0.01f || vMax > 1.01f)
            Debug.LogWarning("[BreastPhysics] UVs extend outside [0,1]! " +
                             "The mesh will sample transparent texture pixels and be invisible. " +
                             "Check sprite pivot, PPU, and collider offset.");

        return uvs;
    }

    // ===================== Rendering =====================

    private void CreateMeshChild()
    {
        // MeshFilter can't coexist with SpriteRenderer on the same GO,
        // so we create a child for the deformable mesh.
        meshChild = new GameObject("BreastMeshRenderer");
        meshChild.transform.SetParent(transform, false);
        meshChild.transform.localPosition = new Vector3(meshCenter.x, meshCenter.y, 0f);
        meshChild.transform.localRotation = Quaternion.identity;
        meshChild.transform.localScale = Vector3.one;
        meshChild.layer = gameObject.layer; // Must match camera culling mask

        meshFilter = meshChild.AddComponent<MeshFilter>();
        meshRenderer = meshChild.AddComponent<MeshRenderer>();
        meshFilter.mesh = mesh;

        // --- Build material ---
        Texture2D tex = spriteRenderer.sprite.texture;
        Material mat;

        if (debugUseDefaultShader)
        {
            // Fallback: Sprites/Default proves whether the mesh geometry + UVs
            // are correct independently of the custom shader.
            Shader fallback = Shader.Find("Sprites/Default");
            mat = new Material(fallback);
            mat.SetTexture("_MainTex", tex);
            Debug.Log("[BreastPhysics] DEBUG: Using Sprites/Default for visibility test.");
        }
        else
        {
            // Clone the SpriteRenderer's material and explicitly set every
            // property that SpriteRenderer normally handles internally.
            mat = new Material(spriteRenderer.sharedMaterial);

            // _MainTex must be set ON THE MATERIAL (not just propBlock)
            // so that _MainTex_ST (used by TRANSFORM_TEX in the shader)
            // has correct tiling (1,1) and offset (0,0).
            mat.SetTexture("_MainTex", tex);
            mat.SetTextureScale("_MainTex", Vector2.one);
            mat.SetTextureOffset("_MainTex", Vector2.zero);

            // Ensure color multipliers are identity — SpriteRenderer sets
            // these per-renderer; MeshRenderer does not.
            mat.SetColor("_Color", Color.white);
            mat.SetColor("_RendererColor", Color.white);
            mat.SetVector("_Flip", new Vector4(1, 1, 1, 1));
        }

        meshRenderer.material = mat;
        meshRenderer.sortingLayerID = spriteRenderer.sortingLayerID;
        meshRenderer.sortingOrder = spriteRenderer.sortingOrder;

        // Per-renderer property block (belt and suspenders).
        propBlock.SetTexture("_MainTex", tex);
        propBlock.SetColor("_RendererColor", Color.white);
        propBlock.SetVector("_Flip", new Vector4(1, 1, 1, 1));
        meshRenderer.SetPropertyBlock(propBlock);

        // Hide the SpriteRenderer — the mesh replaces it visually
        spriteRenderer.enabled = false;

        Debug.Log($"[BreastPhysics] Rendering: shader={mat.shader.name}, " +
                  $"tex={tex.name} ({tex.width}x{tex.height}), " +
                  $"layer={LayerMask.LayerToName(meshChild.layer)} ({meshChild.layer}), " +
                  $"sorting={meshRenderer.sortingLayerName}:{meshRenderer.sortingOrder}, " +
                  $"verts={mesh.vertexCount}, tris={mesh.triangles.Length / 3}");
    }

    private void UpdateShaderUniforms(float dt)
    {
        if (meshRenderer == null) return;
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null) return;
        }

        Vector3 mouseScreen = Input.mousePosition;
        mouseScreen.z = -mainCamera.transform.position.z;
        Vector3 mouseWorld = mainCamera.ScreenToWorldPoint(mouseScreen);

        Bounds bounds = meshRenderer.bounds;
        bool hovered = mouseWorld.x >= bounds.min.x && mouseWorld.x <= bounds.max.x
                    && mouseWorld.y >= bounds.min.y && mouseWorld.y <= bounds.max.y;

        float targetX = hovered ? Mathf.InverseLerp(bounds.min.x, bounds.max.x, mouseWorld.x) : 0.5f;
        float targetY = hovered ? Mathf.InverseLerp(bounds.min.y, bounds.max.y, mouseWorld.y) : 0.5f;
        float targetClick = (Input.GetMouseButton(mouseButton) && hovered) ? 1f : 0f;

        smoothMX = Mathf.Lerp(smoothMX, targetX, dt * 14f);
        smoothMY = Mathf.Lerp(smoothMY, targetY, dt * 14f);
        smoothClick = Mathf.Lerp(smoothClick, targetClick, dt * 10f);

        meshRenderer.GetPropertyBlock(propBlock);
        propBlock.SetFloat(PropMouseX, smoothMX);
        propBlock.SetFloat(PropMouseY, smoothMY);
        propBlock.SetFloat(PropClicked, smoothClick);
        propBlock.SetColor("_RendererColor", Color.white);
        propBlock.SetVector("_Flip", new Vector4(1, 1, 1, 1));
        if (spriteRenderer != null && spriteRenderer.sprite != null)
            propBlock.SetTexture("_MainTex", spriteRenderer.sprite.texture);
        meshRenderer.SetPropertyBlock(propBlock);
    }

    // ===================== Geometry Utilities =====================

    private static float SignedArea(Vector2[] poly)
    {
        float area = 0f;
        for (int i = 0; i < poly.Length; i++)
        {
            Vector2 a = poly[i];
            Vector2 b = poly[(i + 1) % poly.Length];
            area += (b.x - a.x) * (b.y + a.y);
        }
        return area;
    }

    private static Vector2[] Subdivide(Vector2[] contour, float maxLen)
    {
        var result = new List<Vector2>();
        for (int i = 0; i < contour.Length; i++)
        {
            Vector2 a = contour[i];
            Vector2 b = contour[(i + 1) % contour.Length];
            result.Add(a);

            float len = Vector2.Distance(a, b);
            if (len > maxLen)
            {
                int segs = Mathf.CeilToInt(len / maxLen);
                for (int s = 1; s < segs; s++)
                    result.Add(Vector2.Lerp(a, b, (float)s / segs));
            }
        }
        return result.ToArray();
    }

    private static List<int> EarClip(List<Vector2> verts)
    {
        var triList = new List<int>();
        var indices = new List<int>();
        for (int i = 0; i < verts.Count; i++)
            indices.Add(i);

        int safety = verts.Count * verts.Count;
        int iter = 0;

        while (indices.Count > 2 && iter++ < safety)
        {
            bool earFound = false;

            for (int i = 0; i < indices.Count; i++)
            {
                int iPrev = (i == 0) ? indices.Count - 1 : i - 1;
                int iNext = (i == indices.Count - 1) ? 0 : i + 1;

                int idxA = indices[iPrev];
                int idxB = indices[i];
                int idxC = indices[iNext];

                Vector2 a = verts[idxA], b = verts[idxB], c = verts[idxC];

                // Must be convex (positive cross product for CCW polygon)
                float cross = (b.x - a.x) * (c.y - a.y) - (c.x - a.x) * (b.y - a.y);
                if (cross <= 0f) continue;

                // No other vertex inside this ear triangle
                bool pointInside = false;
                for (int j = 0; j < indices.Count; j++)
                {
                    if (j == iPrev || j == i || j == iNext) continue;
                    if (PointInTriangle(verts[indices[j]], a, b, c))
                    {
                        pointInside = true;
                        break;
                    }
                }
                if (pointInside) continue;

                triList.Add(idxA);
                triList.Add(idxB);
                triList.Add(idxC);
                indices.RemoveAt(i);
                earFound = true;
                break;
            }

            if (!earFound) break;
        }

        return triList;
    }

    private static bool PointInTriangle(Vector2 p, Vector2 a, Vector2 b, Vector2 c)
    {
        float d1 = CrossSign(p, a, b);
        float d2 = CrossSign(p, b, c);
        float d3 = CrossSign(p, c, a);
        bool hasNeg = (d1 < 0) || (d2 < 0) || (d3 < 0);
        bool hasPos = (d1 > 0) || (d2 > 0) || (d3 > 0);
        return !(hasNeg && hasPos);
    }

    private static float CrossSign(Vector2 p1, Vector2 p2, Vector2 p3)
    {
        return (p1.x - p3.x) * (p2.y - p3.y) - (p2.x - p3.x) * (p1.y - p3.y);
    }

    // ===================== Debug =====================

    private void OnDrawGizmosSelected()
    {
        // Draw pin curve — visible even before play mode so you can position it
        if (pinPoints != null && pinPoints.Length > 0)
        {
            Gizmos.color = Color.magenta;
            for (int i = 0; i < pinPoints.Length; i++)
            {
                Vector3 pw = transform.TransformPoint(pinPoints[i].x, pinPoints[i].y, 0f);
                Gizmos.DrawWireSphere(pw, 0.04f);
                if (i > 0)
                {
                    Vector3 prev = transform.TransformPoint(pinPoints[i - 1].x, pinPoints[i - 1].y, 0f);
                    Gizmos.DrawLine(prev, pw);
                }
            }
        }

        if (restLocal == null) return;

        for (int i = 0; i < vertCount; i++)
        {
            Vector3 world = transform.TransformPoint(
                restLocal[i].x + displacements[i].x,
                restLocal[i].y + displacements[i].y,
                0f);

            if (pinned[i])
                Gizmos.color = Color.red;
            else
                Gizmos.color = Color.green;

            Gizmos.DrawWireSphere(world, 0.02f);
        }

        if (handCollider != null)
        {
            Gizmos.color = new Color(1f, 1f, 0f, 0.3f);
            Gizmos.DrawWireSphere(handCollider.bounds.center, handCollider.bounds.extents.magnitude);
        }
    }
}
