using UnityEngine;

/// <summary>
/// Controls the Sprites/DragSprite shader. Tracks mouse click + drag,
/// converts to UV-space delta, and animates a 2D spring bounce-back on release.
/// The drag delta itself is sprung back to zero, so the texture naturally
/// overshoots and wobbles like dough snapping back.
/// Attach to the same GameObject as the SpriteRenderer.
/// </summary>
public class DragSpriteController : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The SpriteRenderer to control. If left empty, uses GetComponent.")]
    public SpriteRenderer spriteRenderer;

    [Header("Drag Settings")]
    [Tooltip("Mouse button index (0=Left, 1=Right, 2=Middle).")]
    public int mouseButton = 0;

    [Tooltip("Minimum alpha (0-1) a pixel must have to be grabbable. Requires Read/Write enabled on the sprite texture.")]
    [Range(0f, 1f)]
    public float grabAlphaThreshold = 0.1f;

    [Tooltip("Maximum drag delta in UV space. Prevents extreme distortion.")]
    public float maxDragDistance = 0.3f;

    [Tooltip("How quickly the drag follows the mouse (higher = snappier, lower = more doughy).")]
    public float dragSmoothTime = 0.08f;

    [Header("Bounce Settings")]
    [Tooltip("Spring frequency for bounce-back (higher = faster oscillation).")]
    public float bounceFrequency = 8f;

    [Tooltip("Damping ratio. <1 = bouncy/jelly, 1 = critical (no overshoot), >1 = overdamped/sluggish.")]
    [Range(0.1f, 2f)]
    public float bounceDamping = 0.35f;

    [Tooltip("Below this magnitude, snap to zero (prevents endless micro-oscillations).")]
    public float snapThreshold = 0.001f;

    private MaterialPropertyBlock propertyBlock;
    private Camera mainCamera;

    // Drag state
    private bool isDragging;
    private bool isBouncing;
    private Vector2 grabUV;             // UV where the mouse first clicked
    private Vector2 currentDragUV;      // the animated drag delta sent to shader
    private Vector2 dragSmoothVelocity; // used by SmoothDamp during active drag

    // 2D spring state for bounce-back
    private Vector2 springVelocity;

    // Shader property IDs
    private static readonly int GrabXProp = Shader.PropertyToID("_GrabX");
    private static readonly int GrabYProp = Shader.PropertyToID("_GrabY");
    private static readonly int DragXProp = Shader.PropertyToID("_DragX");
    private static readonly int DragYProp = Shader.PropertyToID("_DragY");

    private void Awake()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        propertyBlock = new MaterialPropertyBlock();
        mainCamera = Camera.main;
    }

    private void Update()
    {
        if (spriteRenderer == null)
            return;

        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null)
                return;
        }

        float dt = Time.deltaTime;
        Bounds bounds = spriteRenderer.bounds;
        Vector3 mouseWorld = GetMouseWorldPosition();

        bool mouseOver = mouseWorld.x >= bounds.min.x && mouseWorld.x <= bounds.max.x
                      && mouseWorld.y >= bounds.min.y && mouseWorld.y <= bounds.max.y;

        // --- Mouse press: start drag ---
        if (Input.GetMouseButtonDown(mouseButton) && mouseOver)
        {
            Vector2 clickUV = WorldToSpriteUV(mouseWorld, bounds);
            if (IsPixelOpaque(clickUV))
            {
                isDragging = true;
                isBouncing = false;
                grabUV = clickUV;
                // If still bouncing from a previous drag, keep the current offset
                // so it feels like grabbing a moving object mid-motion.
                dragSmoothVelocity = springVelocity;
                springVelocity = Vector2.zero;
            }
        }

        // --- Mouse held: update drag delta ---
        if (isDragging && Input.GetMouseButton(mouseButton))
        {
            Vector2 currentMouseUV = WorldToSpriteUV(mouseWorld, bounds);
            Vector2 targetDragUV = currentMouseUV - grabUV;

            // Clamp to max distance
            if (targetDragUV.magnitude > maxDragDistance)
            {
                targetDragUV = targetDragUV.normalized * maxDragDistance;
            }

            // SmoothDamp for a viscous, doughy follow
            currentDragUV = new Vector2(
                Mathf.SmoothDamp(currentDragUV.x, targetDragUV.x, ref dragSmoothVelocity.x, dragSmoothTime, Mathf.Infinity, dt),
                Mathf.SmoothDamp(currentDragUV.y, targetDragUV.y, ref dragSmoothVelocity.y, dragSmoothTime, Mathf.Infinity, dt)
            );
        }

        // --- Mouse release: start bounce-back ---
        if (isDragging && Input.GetMouseButtonUp(mouseButton))
        {
            isDragging = false;
            isBouncing = true;
            // Carry the drag velocity into the spring so release feels continuous
            springVelocity = dragSmoothVelocity;
        }

        // --- Bounce-back: 2D damped spring on the drag delta toward (0,0) ---
        if (isBouncing)
        {
            float omega = bounceFrequency * 2f * Mathf.PI;

            // Damped harmonic oscillator per axis
            // F = -omega^2 * x - 2 * damping * omega * v
            Vector2 springForce = -omega * omega * currentDragUV;
            Vector2 dampForce = -2f * bounceDamping * omega * springVelocity;

            springVelocity += (springForce + dampForce) * dt;
            currentDragUV += springVelocity * dt;

            // Snap to rest once oscillation is negligible
            if (currentDragUV.magnitude < snapThreshold && springVelocity.magnitude < snapThreshold)
            {
                currentDragUV = Vector2.zero;
                springVelocity = Vector2.zero;
                isBouncing = false;
            }
        }

        // --- Push to shader ---
        spriteRenderer.GetPropertyBlock(propertyBlock);
        propertyBlock.SetFloat(GrabXProp, grabUV.x);
        propertyBlock.SetFloat(GrabYProp, grabUV.y);
        propertyBlock.SetFloat(DragXProp, currentDragUV.x);
        propertyBlock.SetFloat(DragYProp, currentDragUV.y);
        spriteRenderer.SetPropertyBlock(propertyBlock);
    }

    private Vector3 GetMouseWorldPosition()
    {
        Vector3 mouseScreen = Input.mousePosition;
        mouseScreen.z = -mainCamera.transform.position.z;
        return mainCamera.ScreenToWorldPoint(mouseScreen);
    }

    private Vector2 WorldToSpriteUV(Vector3 worldPos, Bounds bounds)
    {
        float u = Mathf.InverseLerp(bounds.min.x, bounds.max.x, worldPos.x);
        float v = Mathf.InverseLerp(bounds.min.y, bounds.max.y, worldPos.y);
        return new Vector2(u, v);
    }

    /// <summary>
    /// Samples the sprite texture at a 0-1 sprite UV and returns true if the
    /// pixel alpha meets the grab threshold.
    /// Requires "Read/Write Enabled" on the texture import settings.
    /// </summary>
    private bool IsPixelOpaque(Vector2 spriteUV)
    {
        if (spriteRenderer == null || spriteRenderer.sprite == null)
            return false;

        Sprite sprite = spriteRenderer.sprite;
        Texture2D tex = sprite.texture;

        if (tex == null)
            return false;

        // sprite.rect is in pixels within the full texture
        Rect rect = sprite.rect;
        int px = Mathf.RoundToInt(rect.x + spriteUV.x * rect.width);
        int py = Mathf.RoundToInt(rect.y + spriteUV.y * rect.height);

        px = Mathf.Clamp(px, (int)rect.x, (int)(rect.x + rect.width  - 1));
        py = Mathf.Clamp(py, (int)rect.y, (int)(rect.y + rect.height - 1));

        Color pixel;
        try
        {
            pixel = tex.GetPixel(px, py);
        }
        catch
        {
            // Texture not readable — fall back to allowing the grab
            return true;
        }

        return pixel.a >= grabAlphaThreshold;
    }

    /// <summary>
    /// True while the user is actively dragging.
    /// </summary>
    public bool IsDragging() => isDragging;

    /// <summary>
    /// True while the bounce-back spring animation is playing.
    /// </summary>
    public bool IsBouncing() => isBouncing;

    /// <summary>
    /// True when no drag or bounce is active (sprite is at rest).
    /// </summary>
    public bool IsIdle() => !isDragging && !isBouncing;
}
