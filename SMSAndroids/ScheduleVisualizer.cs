using BepInEx;
using GameCreator.Runtime.VisualScripting;
using System;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace SMSAndroidsCore
{
    [BepInPlugin(pluginGuid, Core.pluginName + " - Schedule Visualizer", Core.pluginVersion)]
    internal class ScheduleVisualizer : BaseUnityPlugin
    {
        #region Plugin Info
        public const string pluginGuid = "treboy.starmakerstory.smsandroidscore.schedulevisualizer";
        #endregion

        public static bool loadedVisualizer = false;
        public static Transform radialButtons;
        public static Transform districtButtons;
        public static Transform worldMapCanvas;
        public static GameObject worldMapGameObject;
        
        // Track the last known schedule version to detect changes
        private static int lastKnownScheduleVersion = -1;
        
        // Track World_Map active state to detect when it's opened
        private static bool wasWorldMapActive = false;

        // Dictionary to cache loaded character icons
        private static Dictionary<string, Sprite> characterIconCache = new Dictionary<string, Sprite>();

        // Dictionary to store created overlay containers for each location button (Radial_Buttons)
        private static Dictionary<string, GameObject> locationOverlays = new Dictionary<string, GameObject>();
        
        // Dictionary to store created overlay containers for each district button (District_Buttons)
        private static Dictionary<string, GameObject> districtOverlays = new Dictionary<string, GameObject>();
        
        // Pre-created icon pools for each overlay (key = overlay path, value = list of icon GameObjects)
        private static Dictionary<string, List<GameObject>> locationIconPools = new Dictionary<string, List<GameObject>>();
        private static Dictionary<string, List<GameObject>> districtIconPools = new Dictionary<string, List<GameObject>>();
        
        // Max icons per overlay (3 rows * 8 columns)
        private const int maxIconsPerOverlay = 24;
        
        // Pre-cached sprites for each district overlay at each row count (1-4 rows)
        // Key = district name, Value = array of sprites indexed by row count (index 0 = 1 row, index 3 = 4 rows)
        private static Dictionary<string, Sprite[]> districtSpritesPerRowCount = new Dictionary<string, Sprite[]>();
        
        // Store the scale used for each district overlay for height calculations
        private static Dictionary<string, float> districtOverlayScales = new Dictionary<string, float>();
        
        // Store pre-calculated heights for each row count (scaled)
        private static Dictionary<string, float[]> districtHeightsPerRowCount = new Dictionary<string, float[]>();

        // ============================================================================
        // LOCATION MAPPING CONFIGURATION
        // ============================================================================
        // This dictionary maps Schedule location strings to World Map Radial Button paths
        // Format: "ScheduleLocationName" => "RadialButtonParent/LocationButtonName"
        // 
        // Radial Button parents (districts): Seaside, Badlands, Suburbs, TheLine, TheHills, 
        //                                     Downtown, NeonRow, Shopside, Evergreen, Plainview, Foundry
        //
        // To add new mappings, simply add a new entry here.
        // If a location doesn't have a corresponding button, map it to null or omit it.
        // ============================================================================
        public static readonly Dictionary<string, string> LocationToButtonMapping = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            // Seaside District
            { "Beach", "Seaside/Beach" },
            { "Sofia", "Seaside/Sofia" },
            
            // Badlands District
            { "Parkinglot", "Badlands/Parkinglotbutton" },
            { "Tonihome", "Badlands/tonihomeworldmap" },
            { "AmusementPark", "Badlands/amusementparkworldmap" },
            { "Alley", "Badlands/Parkinglotbutton" },
            
            // Suburbs District
            { "Kenshome", "Suburbs/Ken" },
            { "KenhouseOutside", "Suburbs/Ken" },
            { "Pool", "Suburbs/Pool" },
            { "Park", "Suburbs/Park" },
            
            // The Line District
            { "TashaStore", "TheLine/tashastore" },
            { "FlowerStore", "TheLine/Flowerstoreworldmap" },
            { "Kate", "TheLine/Kate" },
            
            // The Hills District
            { "Gabrielsmansion", "TheHills/Gabriel" },
            { "Mario", "TheHills/Mario" },
            { "Villa", "TheHills/Mario" },
            { "Chloe", "TheHills/Chole" },
            
            // Downtown District
            { "Home", "Downtown/Home" },
            { "Downtown", "Downtown/Streets" },
            { "Streets", "Downtown/Streets" },
            { "Hospital", "Downtown/Hospital" },
            { "Hospitalhallway", "Downtown/Hospital" },
            { "Hotel", "Downtown/Streets" },
            
            // Neon Row / Nightlife District
            { "Club", "NeonRow/Club" },
            { "Neonstreet", "NeonRow/Neonstreet" },
            
            // Shopping District
            { "Mall", "Shopside/Mall" },
            { "Gasstation", "Shopside/Gasstation" },
            
            // Evergreen District
            { "Forest", "Evergreen/Forest" },
            { "Temple", "Evergreen/Forest" },
            { "Trail", "Evergreen/Trail" },
            
            // Plainview District
            { "Farm", "Plainview/Farm" },
            
            // Harbor / Foundry District
            { "Harbor", "Foundry/Harbor" },
            { "hauhou", "Foundry/hauhou" },
            { "HarborHouseEntrance", "Foundry/HarborHouseEntrance" },
            { "HarborHome*", "Foundry/HarborHouseEntrance" }, 
            
            // Locations that don't show on world map (internal/mod areas)
            { "GiftShop", "Seaside/Beach" },
            { "GiftShopInterior", "Seaside/Beach" },
            { "MountainLab", "Seaside/Beach" },
            { "MountainLabRoomNikkeAnis", "Seaside/Beach" },
            { "MountainLabRoomNikkeCenti", "Seaside/Beach" },
            { "MountainLabRoomNikkeDorothy", "Seaside/Beach" },
            { "MountainLabRoomNikkeElegg", "Seaside/Beach" },
            { "MountainLabRoomNikkeFrima", "Seaside/Beach" },
            { "MountainLabRoomNikkeGuilty", "Seaside/Beach" },
            { "MountainLabRoomNikkeHelm", "Seaside/Beach" },
            { "MountainLabRoomNikkeMaiden", "Seaside/Beach" },
            { "MountainLabRoomNikkeMary", "Seaside/Beach" },
            { "MountainLabRoomNikkeMast", "Seaside/Beach" },
            { "MountainLabRoomNikkeNeon", "Seaside/Beach" },
            { "MountainLabRoomNikkePepper", "Seaside/Beach" },
            { "MountainLabRoomNikkeRapi", "Seaside/Beach" },
            { "MountainLabRoomNikkeRosanna", "Seaside/Beach" },
            { "MountainLabRoomNikkeSakura", "Seaside/Beach" },
            { "MountainLabRoomNikkeTove", "Seaside/Beach" },
            { "MountainLabRoomNikkeViper", "Seaside/Beach" },
            { "MountainLabRoomNikkeYan", "Seaside/Beach" },
            { "SecretBeach", "Seaside/Beach" },
            { "Unknown", null },
        };

        // ============================================================================
        // RADIAL BUTTON TO DISTRICT BUTTON MAPPING
        // ============================================================================
        // Maps Radial_Button parent names to District_Button names
        // Format: "RadialButtonParentName" => "DistrictButtonName"
        // 
        // This allows characters shown on radial buttons to also appear on their
        // parent district button. Names may differ between the two hierarchies.
        // ============================================================================
        public static readonly Dictionary<string, string> RadialToDistrictMapping = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "Seaside", "Seaside" },
            { "Suburbs", "Suburbs" },
            { "Downtown", "Downtown" },
            { "TheHills", "TheHills" },
            { "TheLine", "The Line" },       // Note: District button has space
            { "Evergreen", "Evergreen" },
            { "Badlands", "Badlands" },
            { "Plainview", "Plainview" },
            { "Shopside", "Shoppingdistrict" }, // Different names
            { "NeonRow", "Nightlife" },       // Different names
            { "Foundry", "Harbor" },          // Different names
        };

        // ============================================================================
        // CHARACTER NAMES LIST
        // ============================================================================
        // All characters that can appear on the schedule
        // Icon files should be named Icon{CharacterName}.png (e.g., IconAnis.png)
        // ============================================================================
        public static readonly string[] AllCharacters = new string[]
        {
            "Amber", "Claire",
            "Anis", "Centi", "Dorothy", "Elegg", "Frima", "Guilty", "Helm", "Maiden",
            "Mary", "Mast", "Neon", "Pepper", "Rapi", "Rosanna", "Sakura", "Tove", "Viper", "Yan",
            "Snek"
        };

        // ============================================================================
        // VISUAL SETTINGS
        // ============================================================================
        public static float iconSize = 18f;               // Size of each character icon
        public static float overlayPadding = 4f;          // Padding around the icons
        public static float iconSpacing = 2f;             // Spacing between icons
        public static int maxIconsPerRow = 8;             // Maximum icons per row (8 slots per row)
        public static int maxRows = 3;                    // Maximum number of rows
        public static float leftPadding = 40f;            // Extra padding on left so icons don't render under location button
        public static float cornerRadius = 12f;           // Radius for rounded corners on the right side
        public static float leftCutoutRadius = 38f;       // Radius of the circular cutout on the left (matches parent button size)
        public static Color overlayBackgroundColor = new Color(0f, 0f, 0f, 0.6f); // Semi-transparent black
        public static int borderThickness = 2;            // Thickness of the white border in pixels
        public static Color borderColor = new Color(1f, 1f, 1f, 0.9f);    // Color of the border (90% opaque white)
        
        // District-specific settings (rendered below button, centered)
        public static int districtMaxIconsPerRow = 6;     // Max icons per row for district overlays
        public static int districtMaxRows = 4;            // Max rows for district overlays
        public static int districtMaxIcons = 24;          // Max total icons (6 * 4)
        public static float districtTopPadding = 40f;     // Extra padding on top so icons don't render under button
        public static float topCutoutRadius = 38f;        // Radius of the circular cutout on top
        // ============================================================================

        public void Update()
        {
            if (Core.currentScene.name == "CoreGameScene")
            {
                if (!loadedVisualizer && Core.loadedCore && Schedule.loadedSchedule)
                {
                    InitializeVisualizer();
                    Logger.LogInfo("----- SCHEDULE VISUALIZER LOADED -----");
                    loadedVisualizer = true;
                }

                // Update visuals when World_Map is activated (opened)
                if (loadedVisualizer && Schedule.loadedSchedule && worldMapGameObject != null)
                {
                    bool isWorldMapActive = worldMapGameObject.activeInHierarchy;
                    
                    // Check if World_Map just became active (was closed, now open)
                    if (isWorldMapActive && !wasWorldMapActive)
                    {
                        UpdateAllCharacterIcons();
                        lastKnownScheduleVersion = Schedule.scheduleVersion;
                        Debug.Log($"[ScheduleVisualizer] World_Map opened, updating icons");
                    }
                    
                    wasWorldMapActive = isWorldMapActive;
                }
            }
            if (Core.currentScene.name == "GameStart")
            {
                if (loadedVisualizer)
                {
                    CleanupVisualizer();
                    Logger.LogInfo("----- SCHEDULE VISUALIZER UNLOADED -----");
                    loadedVisualizer = false;
                }
            }
        }

        private void InitializeVisualizer()
        {
            try
            {
                // Find World_Map > Canvas > Core > Radial_Buttons
                // Note: World_Map is initially disabled, so we can't use GameObject.Find()
                // Instead, search through all root objects in the scene including inactive ones
                GameObject worldMap = FindInactiveGameObject("World_Map");
                if (worldMap == null)
                {
                    Debug.LogError("[ScheduleVisualizer] Could not find World_Map");
                    return;
                }
                
                // Store reference for active state tracking
                worldMapGameObject = worldMap;
                wasWorldMapActive = worldMap.activeInHierarchy;

                worldMapCanvas = worldMap.transform.Find("Canvas");
                if (worldMapCanvas == null)
                {
                    Debug.LogError("[ScheduleVisualizer] Could not find World_Map > Canvas");
                    return;
                }

                Transform core = worldMapCanvas.Find("Core");
                if (core == null)
                {
                    Debug.LogError("[ScheduleVisualizer] Could not find World_Map > Canvas > Core");
                    return;
                }

                radialButtons = core.Find("Radial_Buttons");
                if (radialButtons == null)
                {
                    Debug.LogError("[ScheduleVisualizer] Could not find Radial_Buttons");
                    return;
                }
                
                districtButtons = core.Find("District_Buttons");
                if (districtButtons == null)
                {
                    Debug.LogError("[ScheduleVisualizer] Could not find District_Buttons");
                    return;
                }

                // Pre-load character icons
                PreloadCharacterIcons();

                // Create overlays for all location buttons (Radial_Buttons)
                CreateAllLocationOverlays();
                
                // Create overlays for all district buttons (District_Buttons)
                CreateAllDistrictOverlays();

                // Initial update of character positions
                UpdateAllCharacterIcons();

                Debug.Log("[ScheduleVisualizer] Initialization complete");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ScheduleVisualizer] Error during initialization: {ex.Message}\n{ex.StackTrace}");
            }
        }

        /// <summary>
        /// Finds a GameObject by name, including inactive objects.
        /// Searches through all root objects in the active scene.
        /// </summary>
        private GameObject FindInactiveGameObject(string name)
        {
            // Get all root objects in the scene (including inactive)
            GameObject[] rootObjects = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
            
            foreach (GameObject root in rootObjects)
            {
                if (root.name == name)
                {
                    return root;
                }
                
                // Also search children recursively
                Transform found = FindInChildren(root.transform, name);
                if (found != null)
                {
                    return found.gameObject;
                }
            }
            
            return null;
        }

        /// <summary>
        /// Recursively searches for a child transform by name.
        /// </summary>
        private Transform FindInChildren(Transform parent, string name)
        {
            foreach (Transform child in parent)
            {
                if (child.name == name)
                {
                    return child;
                }
                
                Transform found = FindInChildren(child, name);
                if (found != null)
                {
                    return found;
                }
            }
            return null;
        }

        private void CleanupVisualizer()
        {
            // Destroy all location overlay objects
            foreach (var overlay in locationOverlays.Values)
            {
                if (overlay != null)
                {
                    Destroy(overlay);
                }
            }
            locationOverlays.Clear();
            
            // Destroy all district overlay objects
            foreach (var overlay in districtOverlays.Values)
            {
                if (overlay != null)
                {
                    Destroy(overlay);
                }
            }
            districtOverlays.Clear();
            
            characterIconCache.Clear();
        }

        private void PreloadCharacterIcons()
        {
            string iconFolder = Core.exePath + Core.uiPath;

            foreach (string character in AllCharacters)
            {
                string iconFileName = $"Icon{character}.png";
                string fullPath = iconFolder + iconFileName;

                if (File.Exists(fullPath))
                {
                    Sprite sprite = LoadSpriteFromFile(fullPath);
                    if (sprite != null)
                    {
                        characterIconCache[character.ToLower()] = sprite;
                        Debug.Log($"[ScheduleVisualizer] Loaded icon for {character}");
                    }
                }
            }

            // Ensure we have a fallback icon (Anis)
            if (!characterIconCache.ContainsKey("anis"))
            {
                Debug.LogWarning("[ScheduleVisualizer] No fallback icon (IconAnis.png) found!");
            }
        }

        private Sprite LoadSpriteFromFile(string fullPath)
        {
            try
            {
                byte[] rawData = File.ReadAllBytes(fullPath);
                Texture2D tex = new Texture2D(64, 64, TextureFormat.RGBA32, false);
                tex.LoadImage(rawData);
                tex.filterMode = FilterMode.Trilinear;  // Best for scaled-down UI icons
                tex.mipMapBias = -1f;  // Sharper mip levels
                Sprite sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
                return sprite;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ScheduleVisualizer] Error loading sprite from {fullPath}: {ex.Message}");
                return null;
            }
        }

        private Sprite GetCharacterIcon(string characterName)
        {
            string key = characterName.ToLower();
            if (characterIconCache.TryGetValue(key, out Sprite sprite))
            {
                return sprite;
            }

            // Fallback to Anis icon
            if (characterIconCache.TryGetValue("anis", out Sprite fallback))
            {
                return fallback;
            }

            return null;
        }

        /// <summary>
        /// Creates a sprite with rounded corners on the right side and a circular cutout on the left
        /// to accommodate the circular parent location button. Also draws a white border.
        /// </summary>
        /// <param name="width">Texture width</param>
        /// <param name="height">Texture height</param>
        /// <param name="rightRadius">Radius for rounded corners on right side</param>
        /// <param name="leftCutoutRadiusPixels">Radius of circular cutout on left (in pixels)</param>
        /// <param name="borderThicknessPixels">Thickness of the border in pixels</param>
        private Sprite CreateRoundedRightWithLeftCutoutSprite(int width, int height, int rightRadius, int leftCutoutRadiusPixels, int borderThicknessPixels)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            Color32[] pixels = new Color32[width * height];
            Color32 clear = new Color32(0, 0, 0, 0);
            Color32 bgColor = new Color32((byte)(overlayBackgroundColor.r * 255), (byte)(overlayBackgroundColor.g * 255), (byte)(overlayBackgroundColor.b * 255), (byte)(overlayBackgroundColor.a * 255));
            Color32 borderCol = new Color32((byte)(borderColor.r * 255), (byte)(borderColor.g * 255), (byte)(borderColor.b * 255), 255);

            // Center of the left cutout circle (at the left edge, vertically centered)
            float cutoutCenterX = 0f;
            float cutoutCenterY = height / 2f;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    bool inside = true;
                    bool isBorder = false;

                    // Check if inside the left circular cutout (should be transparent)
                    float dxLeft = x - cutoutCenterX;
                    float dyLeft = y - cutoutCenterY;
                    float distToLeftCutout = Mathf.Sqrt(dxLeft * dxLeft + dyLeft * dyLeft);
                    
                    if (distToLeftCutout < leftCutoutRadiusPixels)
                    {
                        inside = false;
                    }
                    // Check if on the border of left cutout (circular border)
                    else if (distToLeftCutout < leftCutoutRadiusPixels + borderThicknessPixels)
                    {
                        inside = true;
                        isBorder = true;
                    }

                    // Check top-right corner (rounded)
                    if (inside && !isBorder && x > width - rightRadius - 1 && y > height - rightRadius - 1)
                    {
                        int dx = x - (width - rightRadius - 1);
                        int dy = y - (height - rightRadius - 1);
                        float distToCorner = Mathf.Sqrt(dx * dx + dy * dy);
                        if (distToCorner > rightRadius)
                        {
                            inside = false;
                        }
                        // Check if on the border of rounded corner
                        else if (distToCorner > rightRadius - borderThicknessPixels)
                        {
                            isBorder = true;
                        }
                    }
                    // Check bottom-right corner (rounded)
                    else if (inside && !isBorder && x > width - rightRadius - 1 && y < rightRadius)
                    {
                        int dx = x - (width - rightRadius - 1);
                        int dy = y - rightRadius;
                        float distToCorner = Mathf.Sqrt(dx * dx + dy * dy);
                        if (distToCorner > rightRadius)
                        {
                            inside = false;
                        }
                        // Check if on the border of rounded corner
                        else if (distToCorner > rightRadius - borderThicknessPixels)
                        {
                            isBorder = true;
                        }
                    }

                    // Check if on border of top or bottom edge (for non-rounded sections)
                    if (inside && !isBorder)
                    {
                        if (y < borderThicknessPixels || y >= height - borderThicknessPixels)
                        {
                            // Only apply border if not in the rounded corner regions
                            if (!((x > width - rightRadius - 1 && y > height - rightRadius - 1) || (x > width - rightRadius - 1 && y < rightRadius)))
                            {
                                isBorder = true;
                            }
                        }
                        // Check right edge border
                        if (x > width - borderThicknessPixels)
                        {
                            // Only apply border if not in the left cutout region
                            if (!(distToLeftCutout < leftCutoutRadiusPixels))
                            {
                                isBorder = true;
                            }
                        }
                    }

                    if (inside)
                    {
                        pixels[y * width + x] = isBorder ? borderCol : bgColor;
                    }
                    else
                    {
                        pixels[y * width + x] = clear;
                    }
                }
            }

            tex.SetPixels32(pixels);
            tex.Apply();
            tex.filterMode = FilterMode.Bilinear;

            // Create sprite (no 9-slice since left cutout makes it non-sliceable)
            Sprite sprite = Sprite.Create(tex, new Rect(0, 0, width, height), new Vector2(0f, 0.5f), 100f);
            return sprite;
        }

        private void CreateAllLocationOverlays()
        {
            // Get unique button paths from the mapping
            HashSet<string> uniqueButtonPaths = new HashSet<string>();
            foreach (var mapping in LocationToButtonMapping)
            {
                if (!string.IsNullOrEmpty(mapping.Value))
                {
                    uniqueButtonPaths.Add(mapping.Value);
                }
            }

            // Create overlay for each unique button
            foreach (string buttonPath in uniqueButtonPaths)
            {
                CreateOverlayForButton(buttonPath, radialButtons, locationOverlays, locationIconPools);
            }
        }

        private void CreateAllDistrictOverlays()
        {
            // Create overlay for each district button based on the mapping
            // District overlays render below and centered, with 4 rows x 6 columns
            foreach (var mapping in RadialToDistrictMapping)
            {
                string districtButtonName = mapping.Value;
                if (!string.IsNullOrEmpty(districtButtonName))
                {
                    CreateDistrictOverlayForButton(districtButtonName, districtButtons, districtOverlays, districtIconPools, scale: 1.4f);
                    
                    // Add hover event to move button to front
                    Transform buttonTransform = districtButtons.Find(districtButtonName);
                    if (buttonTransform != null)
                    {
                        AddDistrictButtonHoverHandler(buttonTransform.gameObject);
                    }
                }
            }
        }

        private void AddDistrictButtonHoverHandler(GameObject buttonGameObject)
        {
            // Get or add EventTrigger component
            EventTrigger eventTrigger = buttonGameObject.GetComponent<EventTrigger>();
            if (eventTrigger == null)
            {
                eventTrigger = buttonGameObject.AddComponent<EventTrigger>();
            }

            // Create and add PointerEnter event
            EventTrigger.Entry pointerEnterEntry = new EventTrigger.Entry();
            pointerEnterEntry.eventID = EventTriggerType.PointerEnter;
            pointerEnterEntry.callback.AddListener((data) =>
            {
                // Move this button to the end (front) so its overlay renders on top
                buttonGameObject.transform.SetAsLastSibling();
            });
            eventTrigger.triggers.Add(pointerEnterEntry);
        }

        private void CreateOverlayForButton(string buttonPath, Transform parentContainer, Dictionary<string, GameObject> overlayDictionary, Dictionary<string, List<GameObject>> iconPoolDictionary, float scale = 1f, float horizontalOffset = 0f)
        {
            if (string.IsNullOrEmpty(buttonPath) || parentContainer == null)
                return;

            Transform buttonTransform = parentContainer.Find(buttonPath);
            if (buttonTransform == null)
            {
                Debug.LogWarning($"[ScheduleVisualizer] Button not found: {buttonPath} in {parentContainer.name}");
                return;
            }

            // Create overlay container as a child of the button
            GameObject overlay = new GameObject($"CharacterOverlay_{buttonPath.Replace("/", "_")}");
            overlay.transform.SetParent(buttonTransform, false);

            // Add RectTransform
            RectTransform rectTransform = overlay.AddComponent<RectTransform>();
            // Anchor to center of parent (left edge of overlay at center of location button)
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0f, 0.5f);      // Pivot on left side
            rectTransform.anchoredPosition = new Vector2(horizontalOffset, 0f);    // Left edge at center + offset
            
            // Apply scale to icon sizes and spacing for this overlay
            float scaledIconSize = iconSize * scale;
            float scaledLeftPadding = leftPadding * scale;
            float scaledOverlayPadding = overlayPadding * scale;
            float scaledIconSpacing = iconSpacing * scale;
            
            // Calculate the required size based on max content
            // Width: leftPadding + (maxIconsPerRow * iconSize) + ((maxIconsPerRow-1) * iconSpacing) + overlayPadding
            // Height: (maxRows * iconSize) + ((maxRows-1) * iconSpacing) + (overlayPadding * 2)
            float contentWidth = scaledLeftPadding + (maxIconsPerRow * scaledIconSize) + ((maxIconsPerRow - 1) * scaledIconSpacing) + scaledOverlayPadding;
            float contentHeight = (maxRows * scaledIconSize) + ((maxRows - 1) * scaledIconSpacing) + (scaledOverlayPadding * 2);
            rectTransform.sizeDelta = new Vector2(contentWidth, contentHeight);

            // Add background image with rounded right corners and left circular cutout
            Image bgImage = overlay.AddComponent<Image>();
            // Create sprite at the exact size we need (accounting for cutout)
            int spriteWidth = (int)contentWidth;
            int spriteHeight = (int)contentHeight;
            int cutoutPixelRadius = (int)(leftCutoutRadius * scale);  // Scale the cutout radius
            bgImage.sprite = CreateRoundedRightWithLeftCutoutSprite(spriteWidth, spriteHeight, (int)(cornerRadius * scale), cutoutPixelRadius, (int)(borderThickness * scale));
            bgImage.color = Color.white;  // Use white to show full colors from the sprite
            bgImage.raycastTarget = false;

            // Add GridLayoutGroup for 3 rows x 8 columns layout
            UnityEngine.UI.GridLayoutGroup glg = overlay.AddComponent<UnityEngine.UI.GridLayoutGroup>();
            glg.cellSize = new Vector2(scaledIconSize, scaledIconSize);
            glg.spacing = new Vector2(scaledIconSpacing, scaledIconSpacing);
            glg.startCorner = UnityEngine.UI.GridLayoutGroup.Corner.UpperLeft;
            glg.startAxis = UnityEngine.UI.GridLayoutGroup.Axis.Horizontal;
            glg.childAlignment = TextAnchor.MiddleLeft;
            glg.constraint = UnityEngine.UI.GridLayoutGroup.Constraint.FixedColumnCount;
            glg.constraintCount = maxIconsPerRow;  // 8 columns
            // Left padding is larger to avoid rendering under the location button
            glg.padding = new RectOffset((int)scaledLeftPadding, (int)scaledOverlayPadding, (int)scaledOverlayPadding, (int)scaledOverlayPadding);

            // Pre-create icon pool for this overlay
            List<GameObject> iconPool = new List<GameObject>();
            for (int i = 0; i < maxIconsPerOverlay; i++)
            {
                GameObject iconGO = new GameObject($"Icon_{i}");
                iconGO.transform.SetParent(overlay.transform, false);

                RectTransform iconRect = iconGO.AddComponent<RectTransform>();
                iconRect.sizeDelta = new Vector2(scaledIconSize, scaledIconSize);

                Image iconImage = iconGO.AddComponent<Image>();
                iconImage.preserveAspect = true;
                iconImage.raycastTarget = false;

                // Start hidden
                iconGO.SetActive(false);
                iconPool.Add(iconGO);
            }
            iconPoolDictionary[buttonPath] = iconPool;

            // Start with overlay hidden
            overlay.SetActive(false);

            overlayDictionary[buttonPath] = overlay;
            Debug.Log($"[ScheduleVisualizer] Created overlay for {buttonPath} in {parentContainer.name}");
        }

        /// <summary>
        /// Creates an overlay for a district button, rendered below and centered with top cutout.
        /// Pre-caches sprites for each possible row count (1-4) to allow dynamic height adjustment.
        /// </summary>
        private void CreateDistrictOverlayForButton(string buttonPath, Transform parentContainer, Dictionary<string, GameObject> overlayDictionary, Dictionary<string, List<GameObject>> iconPoolDictionary, float scale = 1f)
        {
            if (string.IsNullOrEmpty(buttonPath) || parentContainer == null)
                return;

            Transform buttonTransform = parentContainer.Find(buttonPath);
            if (buttonTransform == null)
            {
                Debug.LogWarning($"[ScheduleVisualizer] Button not found: {buttonPath} in {parentContainer.name}");
                return;
            }

            // Store the scale for later use
            districtOverlayScales[buttonPath] = scale;

            // Create overlay container as a child of the button
            GameObject overlay = new GameObject($"CharacterOverlay_{buttonPath.Replace("/", "_")}");
            overlay.transform.SetParent(buttonTransform, false);

            // Add RectTransform
            RectTransform rectTransform = overlay.AddComponent<RectTransform>();
            // Anchor to center of parent, positioned below (top edge of overlay at center of button)
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 1f);      // Pivot on top center
            rectTransform.anchoredPosition = Vector2.zero;    // Top edge at center of button
            
            // Apply scale to icon sizes and spacing for this overlay
            float scaledIconSize = iconSize * scale;
            float scaledTopPadding = districtTopPadding * scale;
            float scaledOverlayPadding = overlayPadding * scale;
            float scaledIconSpacing = iconSpacing * scale;
            
            // Calculate width (constant regardless of row count)
            float contentWidth = (districtMaxIconsPerRow * scaledIconSize) + ((districtMaxIconsPerRow - 1) * scaledIconSpacing) + (scaledOverlayPadding * 2);
            int spriteWidth = (int)contentWidth;
            int cutoutPixelRadius = (int)(topCutoutRadius * scale);
            int scaledCornerRadius = (int)(cornerRadius * scale);
            int scaledBorderThickness = (int)(borderThickness * scale);
            
            // Pre-cache sprites and heights for each row count (1-4)
            Sprite[] spritesForRows = new Sprite[districtMaxRows];
            float[] heightsForRows = new float[districtMaxRows];
            
            for (int rowCount = 1; rowCount <= districtMaxRows; rowCount++)
            {
                // Height: topPadding + (rowCount * iconSize) + ((rowCount-1) * iconSpacing) + overlayPadding
                float heightForRows = scaledTopPadding + (rowCount * scaledIconSize) + ((rowCount - 1) * scaledIconSpacing) + scaledOverlayPadding;
                heightsForRows[rowCount - 1] = heightForRows;
                
                int spriteHeight = (int)heightForRows;
                spritesForRows[rowCount - 1] = CreateRoundedBottomWithTopCutoutSprite(spriteWidth, spriteHeight, scaledCornerRadius, cutoutPixelRadius, scaledBorderThickness);
            }
            
            districtSpritesPerRowCount[buttonPath] = spritesForRows;
            districtHeightsPerRowCount[buttonPath] = heightsForRows;
            
            // Start with max height (4 rows)
            float contentHeight = heightsForRows[districtMaxRows - 1];
            rectTransform.sizeDelta = new Vector2(contentWidth, contentHeight);

            // Add background image - start with max rows sprite
            Image bgImage = overlay.AddComponent<Image>();
            bgImage.sprite = spritesForRows[districtMaxRows - 1];
            bgImage.color = Color.white;
            bgImage.raycastTarget = false;

            // Add GridLayoutGroup for 4 rows x 6 columns layout
            UnityEngine.UI.GridLayoutGroup glg = overlay.AddComponent<UnityEngine.UI.GridLayoutGroup>();
            glg.cellSize = new Vector2(scaledIconSize, scaledIconSize);
            glg.spacing = new Vector2(scaledIconSpacing, scaledIconSpacing);
            glg.startCorner = UnityEngine.UI.GridLayoutGroup.Corner.UpperLeft;
            glg.startAxis = UnityEngine.UI.GridLayoutGroup.Axis.Horizontal;
            glg.childAlignment = TextAnchor.UpperCenter;
            glg.constraint = UnityEngine.UI.GridLayoutGroup.Constraint.FixedColumnCount;
            glg.constraintCount = districtMaxIconsPerRow;  // 6 columns
            // Top padding is larger to avoid rendering under the button
            glg.padding = new RectOffset((int)scaledOverlayPadding, (int)scaledOverlayPadding, (int)scaledTopPadding, (int)scaledOverlayPadding);

            // Pre-create icon pool for this overlay
            List<GameObject> iconPool = new List<GameObject>();
            for (int i = 0; i < districtMaxIcons; i++)
            {
                GameObject iconGO = new GameObject($"Icon_{i}");
                iconGO.transform.SetParent(overlay.transform, false);

                RectTransform iconRect = iconGO.AddComponent<RectTransform>();
                iconRect.sizeDelta = new Vector2(scaledIconSize, scaledIconSize);

                Image iconImage = iconGO.AddComponent<Image>();
                iconImage.preserveAspect = true;
                iconImage.raycastTarget = false;

                // Start hidden
                iconGO.SetActive(false);
                iconPool.Add(iconGO);
            }
            iconPoolDictionary[buttonPath] = iconPool;

            // Start with overlay hidden
            overlay.SetActive(false);

            overlayDictionary[buttonPath] = overlay;
            Debug.Log($"[ScheduleVisualizer] Created district overlay for {buttonPath} with {districtMaxRows} pre-cached row sprites");
        }

        /// <summary>
        /// Creates a sprite with rounded bottom corners and a circular cutout at the top center.
        /// </summary>
        private Sprite CreateRoundedBottomWithTopCutoutSprite(int width, int height, int bottomRadius, int topCutoutRadiusPixels, int borderThicknessPixels)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            Color32[] pixels = new Color32[width * height];
            Color32 clear = new Color32(0, 0, 0, 0);
            Color32 bgColor = new Color32((byte)(overlayBackgroundColor.r * 255), (byte)(overlayBackgroundColor.g * 255), (byte)(overlayBackgroundColor.b * 255), (byte)(overlayBackgroundColor.a * 255));
            Color32 borderCol = new Color32((byte)(borderColor.r * 255), (byte)(borderColor.g * 255), (byte)(borderColor.b * 255), 255);

            // Center of the top cutout circle (at the top edge, horizontally centered)
            float cutoutCenterX = width / 2f;
            float cutoutCenterY = height;  // At top edge

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    bool inside = true;
                    bool isBorder = false;

                    // Check if inside the top circular cutout (should be transparent, no border)
                    float dxTop = x - cutoutCenterX;
                    float dyTop = y - cutoutCenterY;
                    float distToTopCutout = Mathf.Sqrt(dxTop * dxTop + dyTop * dyTop);
                    
                    if (distToTopCutout < topCutoutRadiusPixels)
                    {
                        inside = false;
                        // No border around cutout - just transparent
                    }

                    // Check bottom-left corner (rounded)
                    if (inside && !isBorder && x < bottomRadius && y < bottomRadius)
                    {
                        int dx = x - bottomRadius;
                        int dy = y - bottomRadius;
                        float distToCorner = Mathf.Sqrt(dx * dx + dy * dy);
                        if (distToCorner > bottomRadius)
                        {
                            inside = false;
                        }
                        else if (distToCorner > bottomRadius - borderThicknessPixels)
                        {
                            isBorder = true;
                        }
                    }
                    // Check bottom-right corner (rounded)
                    else if (inside && !isBorder && x > width - bottomRadius - 1 && y < bottomRadius)
                    {
                        int dx = x - (width - bottomRadius - 1);
                        int dy = y - bottomRadius;
                        float distToCorner = Mathf.Sqrt(dx * dx + dy * dy);
                        if (distToCorner > bottomRadius)
                        {
                            inside = false;
                        }
                        else if (distToCorner > bottomRadius - borderThicknessPixels)
                        {
                            isBorder = true;
                        }
                    }

                    // Check if on border of left or right edge
                    if (inside && !isBorder)
                    {
                        // Left edge border
                        if (x < borderThicknessPixels)
                        {
                            // Only apply if not in bottom-left rounded corner
                            if (!(x < bottomRadius && y < bottomRadius))
                            {
                                isBorder = true;
                            }
                        }
                        // Right edge border
                        else if (x >= width - borderThicknessPixels)
                        {
                            // Only apply if not in bottom-right rounded corner
                            if (!(x > width - bottomRadius - 1 && y < bottomRadius))
                            {
                                isBorder = true;
                            }
                        }
                        
                        // Bottom edge border
                        if (y < borderThicknessPixels)
                        {
                            // Only apply if not in bottom rounded corners
                            if (!((x < bottomRadius && y < bottomRadius) || (x > width - bottomRadius - 1 && y < bottomRadius)))
                            {
                                isBorder = true;
                            }
                        }
                        
                        // Top edge border - only for areas completely outside the cutout region
                        // Check if this x position is outside the cutout's horizontal influence at the top
                        float horizontalDistFromCutoutCenter = Mathf.Abs(x - cutoutCenterX);
                        bool outsideCutoutHorizontally = horizontalDistFromCutoutCenter > topCutoutRadiusPixels;
                        if (y >= height - borderThicknessPixels && outsideCutoutHorizontally)
                        {
                            isBorder = true;
                        }
                    }

                    if (inside)
                    {
                        pixels[y * width + x] = isBorder ? borderCol : bgColor;
                    }
                    else
                    {
                        pixels[y * width + x] = clear;
                    }
                }
            }

            tex.SetPixels32(pixels);
            tex.Apply(false, false);  // Don't build mipmaps, keep readable
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Clamp;  // Prevent texture bleeding at edges

            // Create sprite with pivot at top center, no border for 9-slicing
            Sprite sprite = Sprite.Create(tex, new Rect(0, 0, width, height), new Vector2(0.5f, 1f), 100f, 0, SpriteMeshType.FullRect);
            return sprite;
        }

        /// <summary>
        /// Looks up a button path for a location, supporting wildcard matching.
        /// Keys ending with '*' will match any location starting with the prefix.
        /// </summary>
        private static bool TryGetButtonPath(string location, out string buttonPath)
        {
            // Try exact match first
            if (LocationToButtonMapping.TryGetValue(location, out buttonPath))
            {
                return true;
            }

            // Try wildcard matching
            foreach (var kvp in LocationToButtonMapping)
            {
                if (kvp.Key.EndsWith("*"))
                {
                    string prefix = kvp.Key.Substring(0, kvp.Key.Length - 1);
                    if (location.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                    {
                        buttonPath = kvp.Value;
                        return true;
                    }
                }
            }

            buttonPath = null;
            return false;
        }

        /// <summary>
        /// Updates all character icons based on current schedule locations.
        /// Call this after schedule changes (e.g., when day changes).
        /// </summary>
        public static void UpdateAllCharacterIcons()
        {
            if (!loadedVisualizer || radialButtons == null)
                return;

            // Clear all existing icons first
            ClearAllIcons();

            // Dictionary to group characters by their mapped button path (for radial buttons)
            Dictionary<string, List<string>> buttonToCharacters = new Dictionary<string, List<string>>();
            
            // Dictionary to group characters by their district (for district buttons)
            Dictionary<string, List<string>> districtToCharacters = new Dictionary<string, List<string>>();

            // Go through all characters and get their current locations
            foreach (string character in AllCharacters)
            {
                string location = Schedule.GetCharacterLocation(character);
                if (string.IsNullOrEmpty(location))
                    continue;

                // Get the button path for this location (supports wildcards)
                if (TryGetButtonPath(location, out string buttonPath))
                {
                    if (string.IsNullOrEmpty(buttonPath))
                        continue; // Location doesn't show on world map

                    // Add to radial button mapping
                    if (!buttonToCharacters.ContainsKey(buttonPath))
                    {
                        buttonToCharacters[buttonPath] = new List<string>();
                    }
                    buttonToCharacters[buttonPath].Add(character);
                    
                    // Also add to district mapping
                    // Extract the district name from the button path (e.g., "Seaside/Beach" -> "Seaside")
                    string radialParent = buttonPath.Contains("/") ? buttonPath.Split('/')[0] : buttonPath;
                    if (RadialToDistrictMapping.TryGetValue(radialParent, out string districtName))
                    {
                        if (!string.IsNullOrEmpty(districtName))
                        {
                            if (!districtToCharacters.ContainsKey(districtName))
                            {
                                districtToCharacters[districtName] = new List<string>();
                            }
                            // Only add if not already in the list (avoid duplicates)
                            if (!districtToCharacters[districtName].Contains(character))
                            {
                                districtToCharacters[districtName].Add(character);
                            }
                        }
                    }
                }
                else
                {
                    Debug.LogWarning($"[ScheduleVisualizer] No mapping found for location: {location}");
                }
            }

            // Update overlays for each radial button
            foreach (var kvp in buttonToCharacters)
            {
                UpdateOverlayForButton(kvp.Key, kvp.Value, locationOverlays, locationIconPools);
            }

            // Hide radial overlays with no characters
            foreach (var kvp in locationOverlays)
            {
                if (!buttonToCharacters.ContainsKey(kvp.Key))
                {
                    kvp.Value.SetActive(false);
                }
            }
            
            // Update overlays for each district button (with dynamic height)
            foreach (var kvp in districtToCharacters)
            {
                UpdateDistrictOverlayForButton(kvp.Key, kvp.Value);
            }

            // Hide district overlays with no characters
            foreach (var kvp in districtOverlays)
            {
                if (!districtToCharacters.ContainsKey(kvp.Key))
                {
                    kvp.Value.SetActive(false);
                }
            }
        }
        
        /// <summary>
        /// Updates a district overlay with dynamic height based on character count.
        /// </summary>
        private static void UpdateDistrictOverlayForButton(string buttonPath, List<string> characters)
        {
            if (!districtOverlays.TryGetValue(buttonPath, out GameObject overlay) || overlay == null)
                return;
            
            if (!districtIconPools.TryGetValue(buttonPath, out List<GameObject> iconPool))
                return;
            
            // Calculate how many rows we need
            int characterCount = characters.Count;
            int rowsNeeded = Mathf.CeilToInt((float)characterCount / districtMaxIconsPerRow);
            rowsNeeded = Mathf.Clamp(rowsNeeded, 1, districtMaxRows);  // At least 1 row, max 4
            
            // Update the sprite and height based on rows needed
            if (districtSpritesPerRowCount.TryGetValue(buttonPath, out Sprite[] sprites) &&
                districtHeightsPerRowCount.TryGetValue(buttonPath, out float[] heights))
            {
                RectTransform rectTransform = overlay.GetComponent<RectTransform>();
                Image bgImage = overlay.GetComponent<Image>();
                
                if (rectTransform != null && bgImage != null)
                {
                    // Get current width (stays the same)
                    float currentWidth = rectTransform.sizeDelta.x;
                    
                    // Set new height and sprite based on rows needed
                    int rowIndex = rowsNeeded - 1;  // Convert to 0-based index
                    rectTransform.sizeDelta = new Vector2(currentWidth, heights[rowIndex]);
                    bgImage.sprite = sprites[rowIndex];
                }
            }
            
            // Update icons from the pool
            for (int i = 0; i < iconPool.Count; i++)
            {
                if (i < characters.Count)
                {
                    UpdateIconFromPool(iconPool[i], characters[i]);
                    iconPool[i].SetActive(true);
                }
                else
                {
                    iconPool[i].SetActive(false);
                }
            }

            // Show the overlay if it has characters
            overlay.SetActive(characters.Count > 0);
        }

        private static void ClearAllIcons()
        {
            // Hide all icons in radial button overlays (don't destroy)
            foreach (var kvp in locationIconPools)
            {
                foreach (var icon in kvp.Value)
                {
                    if (icon != null) icon.SetActive(false);
                }
            }
            
            // Hide all icons in district button overlays (don't destroy)
            foreach (var kvp in districtIconPools)
            {
                foreach (var icon in kvp.Value)
                {
                    if (icon != null) icon.SetActive(false);
                }
            }
        }

        private static void UpdateOverlayForButton(string buttonPath, List<string> characters, Dictionary<string, GameObject> overlayDictionary, Dictionary<string, List<GameObject>> iconPoolDictionary)
        {
            if (!overlayDictionary.TryGetValue(buttonPath, out GameObject overlay) || overlay == null)
                return;

            // Get or create the icon pool for this overlay
            if (!iconPoolDictionary.TryGetValue(buttonPath, out List<GameObject> iconPool))
                return;

            // Update icons from the pool
            for (int i = 0; i < iconPool.Count; i++)
            {
                if (i < characters.Count)
                {
                    // Show and update this icon
                    UpdateIconFromPool(iconPool[i], characters[i]);
                    iconPool[i].SetActive(true);
                }
                else
                {
                    // Hide unused icons
                    iconPool[i].SetActive(false);
                }
            }

            // Show the overlay if it has characters
            overlay.SetActive(characters.Count > 0);
        }
        
        private static void UpdateIconFromPool(GameObject iconGO, string characterName)
        {
            if (iconGO == null) return;
            
            // Get cached sprite (cache uses lowercase keys)
            string key = characterName.ToLower();
            if (!characterIconCache.TryGetValue(key, out Sprite icon) || icon == null)
            {
                // Try fallback to anis
                if (!characterIconCache.TryGetValue("anis", out icon) || icon == null)
                    return;
            }
            
            Image iconImage = iconGO.GetComponent<Image>();
            if (iconImage != null)
            {
                iconImage.sprite = icon;
            }
        }

        private static void CreateCharacterIcon(Transform parent, string characterName)
        {
            // Get the visualizer instance to access GetCharacterIcon
            ScheduleVisualizer instance = FindFirstObjectByType<ScheduleVisualizer>();
            if (instance == null) return;

            Sprite icon = instance.GetCharacterIcon(characterName);
            if (icon == null)
            {
                Debug.LogWarning($"[ScheduleVisualizer] No icon available for {characterName}");
                return;
            }

            GameObject iconGO = new GameObject($"Icon_{characterName}");
            iconGO.transform.SetParent(parent, false);

            RectTransform rectTransform = iconGO.AddComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(iconSize, iconSize);

            Image iconImage = iconGO.AddComponent<Image>();
            iconImage.sprite = icon;
            iconImage.preserveAspect = true;
            iconImage.raycastTarget = false;

            // Add orange outline for first character (to match reference image)
            if (parent.childCount == 1)
            {
                Outline outline = iconGO.AddComponent<Outline>();
                outline.effectColor = new Color(1f, 0.5f, 0f, 1f); // Orange
                outline.effectDistance = new Vector2(1f, -1f);
            }
        }

        /// <summary>
        /// Call this method when the schedule is updated (e.g., when day changes)
        /// </summary>
        public static void OnScheduleUpdated()
        {
            UpdateAllCharacterIcons();
        }
    }
}
