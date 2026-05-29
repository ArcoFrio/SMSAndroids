using UnityEngine;

/// <summary>
/// Positions this GameObject along the edge contour of the red channel
/// in the squeeze mask texture, or along the sprite's alpha edge.
/// Follows the mouse vertically while snapping horizontally to the
/// left or right boundary. Attach to the cursor/indicator GO;
/// reference the SpriteRenderer whose material has the squeeze mask.
/// </summary>
public enum ContourSource
{
    MaskRedChannel,
    SpriteAlphaEdge
}

public class SqueezeContourFollower : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The SpriteRenderer with the squeeze material/mask. If empty, tries parent.")]
    public SpriteRenderer targetSprite;

    [Tooltip("Which contour to follow: the mask's R channel or the sprite's alpha edge.")]
    public ContourSource contourSource = ContourSource.MaskRedChannel;

    [Tooltip("The mask texture to sample (R channel). If empty, reads _MaskTex from the material. Only used when contourSource is MaskRedChannel.")]
    public Texture2D maskTexture;

    [Header("Click Sprite")]
    [Tooltip("SpriteRenderer on this GO to swap. If empty, uses GetComponent.")]
    public SpriteRenderer ownRenderer;

    [Tooltip("Sprite shown while no mouse button is held.")]
    public Sprite defaultSprite;

    [Tooltip("Sprite shown while the mouse button is held.")]
    public Sprite clickSprite;

    [Tooltip("Optional GameObject activated only while the mouse button is held (e.g. a press effect or extra hand sprite).")]
    public GameObject clickActiveObject;

    [Tooltip("Mouse button index (0=Left, 1=Right, 2=Middle).")]
    public int mouseButton = 0;

    [Header("Edge Settings")]
    [Tooltip("Follow the left edge (true) or right edge (false) of the contour.")]
    public bool followLeftEdge = true;

    [Tooltip("Red channel threshold to consider a pixel 'inside' the body area (MaskRedChannel mode).")]
    [Range(0.01f, 1f)]
    public float redThreshold = 0.01f;

    [Tooltip("Alpha threshold to consider a pixel 'inside' the sprite (SpriteAlphaEdge mode).")]
    [Range(0.01f, 1f)]
    public float alphaThreshold = 0.01f;

    [Tooltip("Horizontal offset from the detected edge (in world units). Positive = outward.")]
    public float edgeOffset = -0.9f;

    [Header("Smoothing")]
    [Tooltip("Skip horizontal smoothing and snap directly to the detected edge each frame.")]
    public bool instantHorizontalSnap = false;

    [Tooltip("Smooth the vertical position for fluid tracking.")]
    public float verticalSmoothSpeed = 14f;

    [Tooltip("Smooth the horizontal position so it doesn't jitter along the edge. Ignored when instantHorizontalSnap is true.")]
    public float horizontalSmoothSpeed = 10f;

    [Header("Constraints")]
    [Tooltip("Clamp the follower within the sprite bounds vertically.")]
    public bool clampToSprite = true;

    [Tooltip("Z position of this object (leave at 0 for 2D).")]
    public float zPosition = 0f;

    private Camera mainCamera;
    private float currentX;
    private float currentY;
    private bool initialized;
    private float previousY;
    private float verticalVelocity;

    // Cached mask pixel data for fast row scanning
    private Color32[] maskPixels;
    private int maskWidth;
    private int maskHeight;

    // Cached sprite pixel data for alpha edge scanning
    private Color32[] spritePixels;
    private int spriteTexWidth;
    private int spriteTexHeight;
    private Rect spriteTexRect;

    private void Awake()
    {
        mainCamera = Camera.main;

        if (targetSprite == null)
        {
            targetSprite = GetComponentInParent<SpriteRenderer>();
        }

        if (ownRenderer == null)
        {
            ownRenderer = GetComponent<SpriteRenderer>();
        }

        // Capture the current sprite as the default if none is assigned
        if (defaultSprite == null && ownRenderer != null)
        {
            defaultSprite = ownRenderer.sprite;
        }

        CacheMaskData();
        CacheSpriteData();
    }

    private void CacheSpriteData()
    {
        if (contourSource != ContourSource.SpriteAlphaEdge)
            return;

        if (targetSprite == null || targetSprite.sprite == null)
        {
            Debug.LogWarning("SqueezeContourFollower: No target sprite for alpha edge mode.");
            return;
        }

        Texture2D tex = targetSprite.sprite.texture;
        try
        {
            spritePixels = tex.GetPixels32();
            spriteTexWidth = tex.width;
            spriteTexHeight = tex.height;
            spriteTexRect = targetSprite.sprite.rect;
        }
        catch
        {
            Debug.LogError("SqueezeContourFollower: Sprite texture is not readable. Enable Read/Write in import settings.");
            spritePixels = null;
        }
    }

    private void CacheMaskData()
    {
        if (maskTexture == null && targetSprite != null)
        {
            // Try to get it from the material
            Material mat = targetSprite.sharedMaterial;
            if (mat != null && mat.HasProperty("_MaskTex"))
            {
                maskTexture = mat.GetTexture("_MaskTex") as Texture2D;
            }
        }

        if (maskTexture == null)
        {
            Debug.LogWarning("SqueezeContourFollower: No mask texture assigned or found on material.");
            return;
        }

        try
        {
            maskPixels = maskTexture.GetPixels32();
            maskWidth = maskTexture.width;
            maskHeight = maskTexture.height;
        }
        catch
        {
            Debug.LogError("SqueezeContourFollower: Mask texture is not readable. Enable Read/Write in import settings.");
            maskPixels = null;
        }
    }

    private void Update()
    {
        if (targetSprite == null)
            return;

        bool hasMask = contourSource == ContourSource.MaskRedChannel && maskPixels != null;
        bool hasAlpha = contourSource == ContourSource.SpriteAlphaEdge && spritePixels != null;
        if (!hasMask && !hasAlpha)
            return;

        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null)
                return;
        }

        float dt = Time.deltaTime;

        // Compute full sprite-rect bounds in world space so the UV mapping
        // is correct even when Mesh Type is Tight (which trims transparent edges
        // and shrinks SpriteRenderer.bounds).
        Bounds bounds = GetFullSpriteWorldBounds();

        // Get mouse world position
        Vector3 mouseScreen = Input.mousePosition;
        mouseScreen.z = -mainCamera.transform.position.z;
        Vector3 mouseWorld = mainCamera.ScreenToWorldPoint(mouseScreen);

        // Target Y follows the mouse
        float targetY = mouseWorld.y;
        if (clampToSprite)
        {
            targetY = Mathf.Clamp(targetY, bounds.min.y, bounds.max.y);
        }

        // Convert target Y to UV space (0-1)
        float uvY = Mathf.InverseLerp(bounds.min.y, bounds.max.y, targetY);

        // Scan the row to find the edge
        float edgeUVX;
        if (contourSource == ContourSource.SpriteAlphaEdge)
        {
            int pixelY = Mathf.RoundToInt(spriteTexRect.y + uvY * spriteTexRect.height);
            pixelY = Mathf.Clamp(pixelY, (int)spriteTexRect.y, (int)(spriteTexRect.y + spriteTexRect.height - 1));
            edgeUVX = FindAlphaEdgeInRow(pixelY);
        }
        else
        {
            // Map UV directly into mask pixel space (mask covers full 0-1 UV range)
            int pixelY = Mathf.Clamp(Mathf.RoundToInt(uvY * (maskHeight - 1)), 0, maskHeight - 1);
            edgeUVX = FindEdgeInRow(pixelY);
        }

        // Convert edge UV X back to world X
        float targetX = Mathf.Lerp(bounds.min.x, bounds.max.x, edgeUVX);

        // Apply offset (outward from the body)
        if (followLeftEdge)
        {
            targetX -= edgeOffset;
        }
        else
        {
            targetX += edgeOffset;
        }

        // Smooth
        if (!initialized)
        {
            currentX = targetX;
            currentY = targetY;
            initialized = true;
        }
        else
        {
            currentX = instantHorizontalSnap ? targetX : Mathf.Lerp(currentX, targetX, dt * horizontalSmoothSpeed);
            currentY = Mathf.Lerp(currentY, targetY, dt * verticalSmoothSpeed);
        }

        // Track vertical velocity for external consumers
        if (dt > 0f)
        {
            verticalVelocity = (currentY - previousY) / dt;
        }
        previousY = currentY;

        transform.position = new Vector3(currentX, currentY, zPosition);

        // --- Click sprite swap ---
        if (ownRenderer != null)
        {
            bool clicking = Input.GetMouseButton(mouseButton);
            Sprite target = clicking && clickSprite != null ? clickSprite : defaultSprite;
            if (ownRenderer.sprite != target)
            {
                ownRenderer.sprite = target;
            }

            // Toggle the optional click-only GameObject
            if (clickActiveObject != null)
            {
                bool shouldBeActive = clicking;
                if (clickActiveObject.activeSelf != shouldBeActive)
                    clickActiveObject.SetActive(shouldBeActive);
            }
        }
    }

    /// <summary>
    /// Returns world-space bounds derived from the sprite's full texture rect,
    /// pivot, and pixelsPerUnit. Unlike SpriteRenderer.bounds this is not
    /// affected by the Tight mesh type trimming transparent edges.
    /// </summary>
    private Bounds GetFullSpriteWorldBounds()
    {
        Sprite spr = targetSprite.sprite;
        float ppu = spr.pixelsPerUnit;
        Vector2 pivot = spr.pivot;
        Rect rect = spr.rect;
        Vector3 scale = targetSprite.transform.lossyScale;
        Vector3 pos = targetSprite.transform.position;

        float worldW = (rect.width / ppu) * Mathf.Abs(scale.x);
        float worldH = (rect.height / ppu) * Mathf.Abs(scale.y);
        float pivotX = (pivot.x / ppu) * scale.x;
        float pivotY = (pivot.y / ppu) * scale.y;

        float minX = pos.x - pivotX;
        float minY = pos.y - pivotY;

        Vector3 center = new Vector3(minX + worldW * 0.5f, minY + worldH * 0.5f, pos.z);
        Vector3 size = new Vector3(worldW, worldH, 0f);
        return new Bounds(center, size);
    }

    /// <summary>
    /// Scans a row of pixels in the mask to find the left or right edge
    /// of the red channel. The row is in mask pixel space (0 to maskHeight-1).
    /// Returns the edge position as a 0-1 UV X coordinate.
    /// </summary>
    private float FindEdgeInRow(int pixelY)
    {
        int rowStart = 0;
        int rowEnd = maskWidth - 1;
        byte threshold = (byte)(redThreshold * 255f);

        if (followLeftEdge)
        {
            // Scan left to right, find first pixel above threshold
            for (int x = rowStart; x <= rowEnd; x++)
            {
                int index = pixelY * maskWidth + x;
                if (index >= 0 && index < maskPixels.Length && maskPixels[index].r >= threshold)
                {
                    // Left edge of the first red pixel = outer boundary
                    return (float)x / maskWidth;
                }
            }
        }
        else
        {
            // Scan right to left, find first pixel above threshold
            for (int x = rowEnd; x >= rowStart; x--)
            {
                int index = pixelY * maskWidth + x;
                if (index >= 0 && index < maskPixels.Length && maskPixels[index].r >= threshold)
                {
                    // Right edge of the last red pixel = outer boundary (+1)
                    return (float)(x + 1) / maskWidth;
                }
            }
        }

        // No red found in this row — return center
        return 0.5f;
    }

    /// <summary>
    /// Scans a row of sprite pixels to find the left or right alpha edge.
    /// Returns the edge position as a 0-1 UV X coordinate.
    /// </summary>
    private float FindAlphaEdgeInRow(int pixelY)
    {
        int rowStart = (int)spriteTexRect.x;
        int rowEnd = (int)(spriteTexRect.x + spriteTexRect.width - 1);
        byte threshold = (byte)(alphaThreshold * 255f);

        if (followLeftEdge)
        {
            for (int x = rowStart; x <= rowEnd; x++)
            {
                int index = pixelY * spriteTexWidth + x;
                if (index >= 0 && index < spritePixels.Length && spritePixels[index].a >= threshold)
                {
                    // Left edge of the first opaque pixel = outer boundary
                    return (x - spriteTexRect.x) / spriteTexRect.width;
                }
            }
        }
        else
        {
            for (int x = rowEnd; x >= rowStart; x--)
            {
                int index = pixelY * spriteTexWidth + x;
                if (index >= 0 && index < spritePixels.Length && spritePixels[index].a >= threshold)
                {
                    // Right edge of the last opaque pixel = outer boundary (+1)
                    return (x - spriteTexRect.x + 1) / spriteTexRect.width;
                }
            }
        }

        return 0.5f;
    }

    /// <summary>
    /// Call this if you change the mask texture at runtime.
    /// </summary>
    public void RefreshMask()
    {
        CacheMaskData();
    }

    /// <summary>
    /// Refreshes cached sprite pixel data (call after sprite swap at runtime).
    /// </summary>
    public void RefreshSpriteData()
    {
        CacheSpriteData();
    }

    /// <summary>
    /// Call this if you swap the mask texture at runtime.
    /// </summary>
    public void SetMaskTexture(Texture2D newMask)
    {
        maskTexture = newMask;
        CacheMaskData();
    }

    /// <summary>
    /// Current smoothed vertical velocity in world units per second.
    /// Positive = moving up, negative = moving down.
    /// </summary>
    public float VerticalVelocity => verticalVelocity;

    /// <summary>
    /// Current smoothed Y position in world space.
    /// </summary>
    public float CurrentY => currentY;
}
