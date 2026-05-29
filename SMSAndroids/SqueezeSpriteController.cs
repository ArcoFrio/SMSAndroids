using UnityEngine;

/// <summary>
/// Controls the Sprites/SqueezeSprite shader. Passes mouse position and
/// click state. The effect only activates while the mouse button is held.
/// Attach to the same GameObject as the SpriteRenderer.
/// </summary>
public class SqueezeSpriteController : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The SpriteRenderer to control. If left empty, uses GetComponent.")]
    public SpriteRenderer spriteRenderer;

    [Header("Mouse Settings")]
    [Tooltip("Mouse button index (0=Left, 1=Right, 2=Middle).")]
    public int mouseButton = 0;

    [Tooltip("Only activate when the mouse is hovering over the sprite.")]
    public bool onlyWhenHovered = true;

    [Tooltip("Default mouse X (0-1) when not hovering.")]
    [Range(0f, 1f)]
    public float defaultMouseX = 0.5f;

    [Tooltip("Default mouse Y (0-1) when not hovering.")]
    [Range(0f, 1f)]
    public float defaultMouseY = 0.5f;

    [Header("Smoothing")]
    [Tooltip("How fast the mouse position follows the cursor.")]
    public float positionSmoothSpeed = 14f;

    [Tooltip("How fast the click strength transitions (higher = snappier).")]
    public float clickSmoothSpeed = 10f;

    private MaterialPropertyBlock propertyBlock;
    private Camera mainCamera;
    private float currentMouseX;
    private float currentMouseY;
    private float currentClicked;
    private bool isHovered;

    private static readonly int MouseXProp = Shader.PropertyToID("_MouseX");
    private static readonly int MouseYProp = Shader.PropertyToID("_MouseY");
    private static readonly int ClickedProp = Shader.PropertyToID("_Clicked");

    private void Awake()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        propertyBlock = new MaterialPropertyBlock();
        currentMouseX = defaultMouseX;
        currentMouseY = defaultMouseY;
        currentClicked = 0f;
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

        float targetX = defaultMouseX;
        float targetY = defaultMouseY;

        // Convert mouse screen position to world position
        Vector3 mouseScreen = Input.mousePosition;
        mouseScreen.z = -mainCamera.transform.position.z;
        Vector3 mouseWorld = mainCamera.ScreenToWorldPoint(mouseScreen);

        Bounds bounds = spriteRenderer.bounds;
        isHovered = mouseWorld.x >= bounds.min.x && mouseWorld.x <= bounds.max.x
                 && mouseWorld.y >= bounds.min.y && mouseWorld.y <= bounds.max.y;

        if (!onlyWhenHovered || isHovered)
        {
            targetX = Mathf.InverseLerp(bounds.min.x, bounds.max.x, mouseWorld.x);
            targetY = Mathf.InverseLerp(bounds.min.y, bounds.max.y, mouseWorld.y);
        }

        // Determine click target: 1 when button held (and hovering if required), 0 otherwise
        bool clicking = Input.GetMouseButton(mouseButton) && (!onlyWhenHovered || isHovered);
        float targetClicked = clicking ? 1f : 0f;

        // Smooth everything
        currentMouseX = Mathf.Lerp(currentMouseX, targetX, dt * positionSmoothSpeed);
        currentMouseY = Mathf.Lerp(currentMouseY, targetY, dt * positionSmoothSpeed);
        currentClicked = Mathf.Lerp(currentClicked, targetClicked, dt * clickSmoothSpeed);

        // Push to shader
        spriteRenderer.GetPropertyBlock(propertyBlock);
        propertyBlock.SetFloat(MouseXProp, currentMouseX);
        propertyBlock.SetFloat(MouseYProp, currentMouseY);
        propertyBlock.SetFloat(ClickedProp, currentClicked);
        spriteRenderer.SetPropertyBlock(propertyBlock);
    }

    /// <summary>
    /// Returns true if the mouse is currently over the sprite.
    /// </summary>
    public bool IsHovered() => isHovered;
}
