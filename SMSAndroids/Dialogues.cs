using BepInEx;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.Common.Audio;
using GameCreator.Runtime.Common.UnityUI;  // ButtonInstructions
using GameCreator.Runtime.VisualScripting; // Trigger
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SMSAndroidsCore
{
    /// <summary>
    /// Gift UI + Harbor Home Talk panel + dialogue-playing mirror. Used to
    /// also instantiate every authored dialogue prefab (~3000 lines of static
    /// GameObject field declarations + an Update body that loaded the dialogue
    /// bundle); that whole layer is now ModForge's job. The pack ships the
    /// dialogue definitions, ModForge spawns them under each roomtalk, and the
    /// SMSAndroids side just keeps the small UI bits that don't fit a pack-
    /// driven model:
    /// <list type="bullet">
    ///   <item>Gift-giving UI (UI_GiftItem_Androids_Canvas) -- a cloned-and-
    ///   reshaped vanilla canvas listing every registered gift; visible when
    ///   the player triggers a gift-give. Opened from packs via the
    ///   OpenGiftUI signal (handled in <see cref="GiftUIBridge"/>).</item>
    ///   <item>Harbor Home Talk panel -- per-room replacement for the vanilla
    ///   TalkButton; shows up to two character buttons for whoever's in the
    ///   active HH room. Pure UI on top of <see cref="Schedule"/> state.</item>
    ///   <item><see cref="audioShower"/> / <see cref="audioShowerQuiet"/> /
    ///   <see cref="audioHarborHomeMusic"/> -- vanilla AudioSource lookups
    ///   under 12_AudioPlayer that Schedule and the HH transitions still
    ///   toggle.</item>
    ///   <item><see cref="dialoguePlaying"/> / <see cref="dialoguePlayingVanilla"/>
    ///   -- mirrored into the proxy variable Checks_Dialogue-is-playing so
    ///   vanilla GC2 conditions can branch on it; the ModForge dispatcher
    ///   keeps the pack-driven flag in sync via reflection.</item>
    /// </list>
    /// </summary>
    [BepInPlugin(pluginGuid, Core.pluginName + " - Dialogues", Core.pluginVersion)]
    internal class Dialogues : BaseUnityPlugin
    {
        #region Plugin Info
        public const string pluginGuid = "treboy.starmakerstory.smsandroidscore.dialogues";
        #endregion

        // -- Gift UI fields ------------------------------------------------
        public static GameObject giftUI;
        public static GameObject giftItemTemplate;
        public static Dictionary<string, bool> giftVanillaMap = new Dictionary<string, bool>();

        // -- Audio sources under 12_AudioPlayer ----------------------------
        // audioShower / audioShowerQuiet are gated by Schedule based on which
        // HH room the player is in vs. where Anis is. audioHarborHomeMusic is
        // the per-house music swap track loaded from the other bundle.
        public static GameObject audioHarborHomeMusic;
        public static GameObject audioShower;
        public static GameObject audioShowerQuiet;

        // -- Harbor Home Talk Panel ----------------------------------------
        public static GameObject hhTalkPanel;
        public static GameObject hhTalkButton1;
        public static GameObject hhTalkButton2;
        private static GameObject vanillaTalkButton;
        private static bool hhTalkPanelInitialized = false;
        private static string hhTalkLastActiveRoom = null;
        private static float hhTalkLastUpdateTime = 0f;
        private static float hhTalkUpdateInterval = 0.3f;
        private static int hhTalkLastScheduleVersion = -1;

        // Cached rounded-corner sprites for the HHTalk UI
        private static Sprite hhPanelSprite;
        private static Sprite hhButtonLeftSprite;
        private static Sprite hhButtonRightSprite;
        private static Sprite hhButtonFullSprite;
        private static Sprite hhLabelLeftSprite;
        private static Sprite hhLabelRightSprite;
        private static Sprite hhLabelFullSprite;

        private static readonly string[] hhTalkRoomNames = new string[]
        {
            "HarborHomeBathroom", "HarborHomeBedroom", "HarborHomeCloset",
            "HarborHomeKitchen", "HarborHomeLivingRoom", "HarborHomePool"
        };

        private static readonly string[] hhTalkCharacterNames = new string[]
        {
            "Amber", "Claire", "Sarah",
            "Anis", "Centi", "Dorothy", "Elegg", "Frima", "Guilty", "Helm",
            "Maiden", "Mary", "Mast", "Neon", "Pepper", "Rapi", "Rosanna",
            "Sakura", "Tove", "Viper", "Yan"
        };

        // -- Dialogue-playing flags ----------------------------------------
        public static bool loadedDialogues = false;
        public static bool dialoguePlaying = false;
        public static bool dialoguePlayingVanilla = false;
        private static bool dialoguePlayingVanillaInvokeRepeatingRunning = false;

        private void MonitorRoomTalkChildren()
        {
            if (Core.roomTalk == null)
            {
                Debug.LogWarning("Core.roomTalk is null. Cannot monitor children.");
                return;
            }

            bool anyChildActive = false;
            foreach (Transform child in Core.roomTalk)
            {
                if (child.name != "Always_Active" && child.gameObject.activeSelf)
                {
                    anyChildActive = true;
                    break;
                }
            }

            //Debug.Log("Monitoring.");
            dialoguePlayingVanilla = anyChildActive;
            //Debug.Log("dialoguePlayingVanilla = " + dialoguePlayingVanilla);
        }

        public void Update()
        {
            if (Core.currentScene.name == "CoreGameScene")
            {
                if (!loadedDialogues && Places.loadedPlaces)
                {
                    // The dialogue-prefab init block that used to live here
                    // (~1000 lines spawning every authored dialogue under its
                    // matching roomtalk) is gone. ModForge spawns those now
                    // -- see SMSModForge.PackPlugin.DialogueBuilder.

                    audioHarborHomeMusic = CreateMusicPlayer("HarborHomeMusic");
                    audioShower          = Core.audioPlayer.Find("Shower").gameObject;
                    audioShowerQuiet     = Core.audioPlayer.Find("ShowerQuiet").gameObject;

                    giftUI = InitializeGiftUICanvas();
                    AddGiftItem("Gift_Action-Figure", "Action Figure", false, "Figure.PNG");
                    AddGiftItem("Beer", "Beer", true, "Beer.PNG");
                    AddGiftItem("Gift_Bikini", "Bikini", false, "Bikini.PNG");
                    AddGiftItem("Body-Oil", "Body Oil", true, "Body Oil.PNG");
                    AddGiftItem("Gift_Bonsai-Tree", "Bonsai Tree", false, "Bonsai.PNG");
                    AddGiftItem("Chocolate", "Chocolate", true, "Chocolate.PNG");
                    AddGiftItem("Inv-energydrink", "Energy Drink", true, "Energy Drink.PNG");
                    AddGiftItem("Flowers", "Flowers", true, "Flowers.PNG");
                    AddGiftItem("inv-lovegum", "Love Gum", true, "Love Gum.PNG");
                    AddGiftItem("Gift_Parasol", "Parasol", false, "Parasol.PNG");
                    AddGiftItem("red-meat", "Red Meat", true, "Red Meat.PNG");
                    AddGiftItem("Gift_Ring", "Ring", false, "Ring.PNG");
                    AddGiftItem("Gift_Shark-Tooth-Necklace", "Shark Tooth Necklace", false, "Necklace.PNG");
                    AddGiftItem("Gift_Sunglasses", "Sunglasses", false, "Sunglasses.PNG");
                    AddGiftItem("Gift_Sunscreen", "Sunscreen", false, "Sunscreen.PNG");
                    AddGiftItem("Gift_Tropical-Flower-Bouquet", "Tropical Flower Bouquet", false, "Bouquet.PNG");
                    AddGiftItem("inv-vape", "Vape", true, "Vape.PNG");
                    AddGiftItem("Whiskey", "Whiskey", true, "Whiskey.PNG");
                    AddGiftItem("Wine", "Wine", true, "Wine.PNG");

                    InitializeHHTalkPanel();
                    GiftUIBridge.EnsureSubscribed();

                    Logger.LogInfo("----- DIALOGUES LOADED -----");
                    loadedDialogues = true;
                }
            }
            if (Core.currentScene.name == "GameStart")
            {
                if (loadedDialogues)
                {
                    hhTalkPanelInitialized = false;
                    hhTalkLastActiveRoom = null;
                    Logger.LogInfo("----- DIALOGUES UNLOADED -----");
                    loadedDialogues = false;
                }
            }

            if (loadedDialogues)
            {
                if (!dialoguePlayingVanillaInvokeRepeatingRunning)
                {
                    InvokeRepeating("MonitorRoomTalkChildren", 0f, 0.5f);
                    dialoguePlayingVanillaInvokeRepeatingRunning = true;
                }
                // Pack dialogues run inside ModForge; read its generic
                // "any dialogue playing" query each frame so our
                // !dialoguePlaying gates back off while one is in flight.
                // (Dependency points SMSAndroids -> ModForge.) OR with our own
                // vanilla-roomtalk scan as belt-and-suspenders.
                dialoguePlaying = ModForgeBridge.IsAnyDialoguePlaying() || dialoguePlayingVanilla;
                if (dialoguePlaying  && !Core.GetProxyVariableBool("Checks_Dialogue-is-playing")) { Core.FindAndModifyProxyVariableBool("Checks_Dialogue-is-playing", true); }
                if (!dialoguePlaying &&  Core.GetProxyVariableBool("Checks_Dialogue-is-playing")) { Core.FindAndModifyProxyVariableBool("Checks_Dialogue-is-playing", false); }

                UpdateHHTalkPanel();
                UpdateGiftUIVisibility();
            }
            else
            {
                if (dialoguePlayingVanillaInvokeRepeatingRunning)
                {
                    CancelInvoke("MonitorRoomTalkChildren");
                    dialoguePlayingVanillaInvokeRepeatingRunning = false;
                }
            }
        }

        private static float lastGiftUICheckTime = 0f;
        private static float giftUICheckInterval = 0.5f;

        private void UpdateGiftUIVisibility()
        {
            // Throttle checks every 0.5 seconds
            if (Time.time - lastGiftUICheckTime < giftUICheckInterval)
            {
                return;
            }
            lastGiftUICheckTime = Time.time;

            // Find the gift canvas
            Transform giftCanvasTransform = Core.FindInActiveObjectByName("UI_GiftItem_Androids_Canvas");
            if (giftCanvasTransform == null)
            {
                return;
            }

            // Check all three gift lists
            Transform[] giftLists = new Transform[]
            {
                giftCanvasTransform.Find("gift_list"),
                giftCanvasTransform.Find("gift_list (1)"),
                giftCanvasTransform.Find("gift_list (2)")
            };

            foreach (Transform giftList in giftLists)
            {
                if (giftList == null) continue;

                for (int i = 0; i < giftList.childCount; i++)
                {
                    Transform giftItem = giftList.GetChild(i);
                    string giftName = giftItem.name;

                    // Check if we know about this gift's vanilla/proxy status
                    if (!giftVanillaMap.ContainsKey(giftName))
                    {
                        continue;
                    }

                    bool isVanilla = giftVanillaMap[giftName];
                    bool giftValue = false;

                    if (isVanilla)
                    {
                        // Get vanilla GNV value
                        giftValue = Core.GetVariableBool(giftName);
                    }
                    else
                    {
                        // Get proxy GNV value
                        if (Core.proxyVariables != null && Core.proxyVariables.Exists(giftName))
                        {
                            giftValue = (bool)Core.proxyVariables.Get(giftName);
                        }
                    }

                    // Set active state based on GNV value (true = active, false = inactive)
                    giftItem.gameObject.SetActive(giftValue);
                }
            }
        }

        private GameObject InitializeGiftUICanvas()
        {
            // Find or copy the UI_GiftItem_Canvas (disabled)
            Transform vanillaGiftCanvasTransform = Core.FindInActiveObjectByName("UI_GiftItem_Canvas");
            if (vanillaGiftCanvasTransform == null)
            {
                Debug.LogError("[InitializeGiftUICanvas] Could not find UI_GiftItem_Canvas");
                return null;
            }

            GameObject vanillaGiftCanvas = vanillaGiftCanvasTransform.gameObject;

            // Instantiate and set as root with no parent
            GameObject giftCanvasClone = GameObject.Instantiate(vanillaGiftCanvas);
            giftCanvasClone.transform.SetParent(null, worldPositionStays: false);
            giftCanvasClone.name = "UI_GiftItem_Androids_Canvas";
            Debug.Log("[InitializeGiftUICanvas] Created UI_GiftItem_Androids_Canvas");

            // Remove Trigger component
            Trigger triggerComponent = giftCanvasClone.GetComponent<Trigger>();
            if (triggerComponent != null)
            {
                GameObject.Destroy(triggerComponent);
                Debug.Log("[InitializeGiftUICanvas] Removed Trigger component from canvas");
            }

            // Define gift item mappings
            Dictionary<string, string> giftMappings = new Dictionary<string, string>
            {
                { "gift_chocolate", "Chocolate" },
                { "gift_wine", "Wine" },
                { "gift_EnergyDrink", "Inv-energydrink" },
                { "gift_Vape", "inv-vape" },
                { "gift_lovegum", "inv-lovegum" }
            };

            // Load audio clip
            AudioClip kissClip = Core.otherBundle.LoadAsset<AudioClip>("kiss1");

            // Process gift_list and gift_list (1)
            Transform giftList = giftCanvasClone.transform.Find("gift_list");
            Transform giftList2 = giftCanvasClone.transform.Find("gift_list (1)");

            if (giftList != null)
            {
                // Store template before deleting anything
                if (giftList.childCount > 0)
                {
                    giftItemTemplate = GameObject.Instantiate(giftList.GetChild(0).gameObject);
                    giftItemTemplate.SetActive(false);
                    giftItemTemplate.name = "GiftItemTemplate";
                    
                    // Remove Trigger component from template root (controls visibility based on vanilla GNVs)
                    Trigger rootTrigger = giftItemTemplate.GetComponent<Trigger>();
                    if (rootTrigger != null)
                    {
                        GameObject.DestroyImmediate(rootTrigger);
                        Debug.Log("[InitializeGiftUICanvas] Removed Trigger from template root");
                    }
                    
                    // Delete Text (TMP) (1) child from template
                    Transform textChild = giftItemTemplate.transform.Find("Text (TMP) (1)");
                    if (textChild != null)
                    {
                        GameObject.DestroyImmediate(textChild.gameObject);
                        Debug.Log("[InitializeGiftUICanvas] Deleted Text (TMP) (1) from template");
                    }
                    
                    // Remove Trigger from Image (1) child
                    Transform image1Child = giftItemTemplate.transform.Find("Image (1)");
                    if (image1Child != null)
                    {
                        Trigger image1Trigger = image1Child.GetComponent<Trigger>();
                        if (image1Trigger != null)
                        {
                            GameObject.DestroyImmediate(image1Trigger);
                            Debug.Log("[InitializeGiftUICanvas] Removed Trigger from template Image (1)");
                        }
                    }

                    // Find and configure Button child of template
                    Transform buttonChild = giftItemTemplate.transform.Find("Button");
                    if (buttonChild != null)
                    {
                        // Remove ButtonInstructions component
                        ButtonInstructions buttonInstructions = buttonChild.GetComponent<ButtonInstructions>();
                        if (buttonInstructions != null)
                        {
                            GameObject.DestroyImmediate(buttonInstructions);
                            Debug.Log("[InitializeGiftUICanvas] Destroyed ButtonInstructions from template Button");
                        }

                        // Remove all Trigger components from Button
                        Trigger[] triggers = buttonChild.GetComponents<Trigger>();
                        foreach (Trigger trigger in triggers)
                        {
                            GameObject.DestroyImmediate(trigger);
                        }
                        Debug.Log($"[InitializeGiftUICanvas] Removed {triggers.Length} Trigger components from template Button");

                        // Add Unity Button component if it doesn't exist
                        UnityEngine.UI.Button button = buttonChild.GetComponent<UnityEngine.UI.Button>();
                        if (button == null)
                        {
                            button = buttonChild.gameObject.AddComponent<UnityEngine.UI.Button>();
                        }

                        // Configure ColorBlock for red on hover
                        var colors = button.colors;
                        colors.normalColor = new Color(1f, 1f, 1f, 1f); // White for default
                        colors.highlightedColor = new Color(1f, 0f, 0f, 1f); // Bright red for hover
                        colors.pressedColor = new Color(0.8f, 0f, 0f, 1f); // Darker red for press
                        colors.selectedColor = new Color(1f, 1f, 1f, 1f); // White
                        colors.disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.5f); // Dark grey
                        colors.colorMultiplier = 1f;
                        colors.fadeDuration = 0.1f;
                        button.colors = colors;
                        button.transition = UnityEngine.UI.Selectable.Transition.ColorTint;

                        Debug.Log("[InitializeGiftUICanvas] Configured Button on template");
                    }
                    
                    // Add LayoutElement to template to enforce size
                    LayoutElement layoutElement = giftItemTemplate.GetComponent<LayoutElement>();
                    if (layoutElement == null)
                    {
                        layoutElement = giftItemTemplate.AddComponent<LayoutElement>();
                    }
                    layoutElement.preferredWidth = 175f;
                    layoutElement.preferredHeight = 175f;
                    Debug.Log("[InitializeGiftUICanvas] Added LayoutElement to template with size 175x175");
                    
                    Debug.Log("[InitializeGiftUICanvas] Stored gift item template");
                }

                // Create copies of gift_list for gift_list (1) and gift_list (2)
                if (giftList2 != null)
                {
                    // Delete the original gift_list (1)
                    GameObject.Destroy(giftList2.gameObject);
                    Debug.Log("[InitializeGiftUICanvas] Deleted original gift_list (1)");

                    // Create gift_list (1) as a copy of gift_list
                    GameObject giftList1Clone = GameObject.Instantiate(giftList.gameObject);
                    giftList1Clone.name = "gift_list (1)";
                    giftList1Clone.transform.SetParent(giftCanvasClone.transform, worldPositionStays: false);
                    giftList1Clone.transform.SetSiblingIndex(giftList.GetSiblingIndex() + 1);
                    
                    // Delete children from gift_list (1) copy
                    Transform giftList1Transform = giftList1Clone.transform;
                    while (giftList1Transform.childCount > 0)
                    {
                        GameObject.DestroyImmediate(giftList1Transform.GetChild(0).gameObject);
                    }
                    Debug.Log("[InitializeGiftUICanvas] Created gift_list (1) as copy of gift_list and deleted its children");

                    // Create gift_list (2) as a copy of gift_list
                    GameObject giftList2Clone = GameObject.Instantiate(giftList.gameObject);
                    giftList2Clone.name = "gift_list (2)";
                    giftList2Clone.transform.SetParent(giftCanvasClone.transform, worldPositionStays: false);
                    giftList2Clone.transform.SetSiblingIndex(giftList1Clone.transform.GetSiblingIndex() + 1);
                    
                    // Delete children from gift_list (2) copy
                    Transform giftList2Transform = giftList2Clone.transform;
                    while (giftList2Transform.childCount > 0)
                    {
                        GameObject.DestroyImmediate(giftList2Transform.GetChild(0).gameObject);
                    }
                    Debug.Log("[InitializeGiftUICanvas] Created gift_list (2) as copy of gift_list and deleted its children");
                }

                // Re-find the lists after creating the copies
                Transform giftList1 = giftCanvasClone.transform.Find("gift_list (1)");
                Transform giftList3 = giftCanvasClone.transform.Find("gift_list (2)");

                // Set RectTransform positions and scales
                RectTransform giftListRect = giftList.GetComponent<RectTransform>();
                if (giftListRect != null)
                {
                    giftListRect.anchoredPosition = new Vector2(giftListRect.anchoredPosition.x, 225f);
                    giftList.localScale = new Vector3(0.8f, 0.8f, 0.8f);
                    Debug.Log("[InitializeGiftUICanvas] Set gift_list anchoredPosition.y to 225 and scale to 0.8");
                }

                if (giftList1 != null)
                {
                    RectTransform giftList1Rect = giftList1.GetComponent<RectTransform>();
                    if (giftList1Rect != null)
                    {
                        giftList1.localPosition = new Vector3(0f, 0f, giftList1.localPosition.z);
                        giftList1.localScale = new Vector3(0.8f, 0.8f, 0.8f);
                        Debug.Log("[InitializeGiftUICanvas] Set gift_list (1) localPosition to (0, 0) and scale to 0.8");
                    }
                }

                if (giftList3 != null)
                {
                    RectTransform giftList3Rect = giftList3.GetComponent<RectTransform>();
                    if (giftList3Rect != null)
                    {
                        giftList3Rect.anchoredPosition = new Vector2(giftList3Rect.anchoredPosition.x, -225f);
                        giftList3.localScale = new Vector3(0.8f, 0.8f, 0.8f);
                        Debug.Log("[InitializeGiftUICanvas] Set gift_list (2) anchoredPosition.y to -225 and scale to 0.8");
                    }
                }

                // Delete all gift items from gift_list (template already stored)
                while (giftList.childCount > 0)
                {
                    GameObject.DestroyImmediate(giftList.GetChild(0).gameObject);
                }
                Debug.Log("[InitializeGiftUICanvas] Deleted all gift items from gift_list");
                
                // Remove EnableChildren components from all gift lists (they control visibility based on vanilla GNVs)
                RemoveEnableChildrenComponents(giftCanvasClone);
                
                // Configure HorizontalLayoutGroup components for all three lists
                ConfigureGiftListLayouts(giftCanvasClone);
            }

            // Delete Close_GiftUI and Gift_Dialogue GameObjects
            Transform closeGiftUI = giftCanvasClone.transform.Find("Close_GiftUI");
            if (closeGiftUI != null)
            {
                GameObject.Destroy(closeGiftUI.gameObject);
                Debug.Log("[InitializeGiftUICanvas] Deleted Close_GiftUI");
            }

            Transform giftDialogue = giftCanvasClone.transform.Find("Gift_Dialogue");
            if (giftDialogue != null)
            {
                GameObject.Destroy(giftDialogue.gameObject);
                Debug.Log("[InitializeGiftUICanvas] Deleted Gift_Dialogue");
            }

            // Setup the Button GO
            Transform buttonGO = giftCanvasClone.transform.Find("Button");
            if (buttonGO != null)
            {
                // Remove ButtonInstructions component
                ButtonInstructions buttonInstructions = buttonGO.GetComponent<ButtonInstructions>();
                if (buttonInstructions != null)
                {
                    GameObject.DestroyImmediate(buttonInstructions);
                    Debug.Log("[InitializeGiftUICanvas] Destroyed ButtonInstructions from Button");
                }

                // Remove all Trigger components from Button
                Trigger[] triggers = buttonGO.GetComponents<Trigger>();
                foreach (Trigger trigger in triggers)
                {
                    GameObject.Destroy(trigger);
                }
                Debug.Log($"[InitializeGiftUICanvas] Removed {triggers.Length} Trigger components from Button");

                // Add Unity Button component
                UnityEngine.UI.Button button = buttonGO.gameObject.AddComponent<UnityEngine.UI.Button>();

                // Set targetGraphic to the Text (TMP) child
                Transform textChild = buttonGO.Find("Text (TMP)");
                if (textChild != null)
                {
                    UnityEngine.UI.Image textImage = textChild.GetComponent<UnityEngine.UI.Image>();
                    if (textImage != null)
                    {
                        button.targetGraphic = textImage;
                        Debug.Log("[InitializeGiftUICanvas] Set Button targetGraphic to Text (TMP)");
                    }
                }

                // Configure ColorBlock for highlighted color (light pink on hover)
                var colors = button.colors;
                colors.normalColor = new Color(1f, 1f, 1f, 1f); // White for default
                colors.highlightedColor = new Color(1f, 0.75f, 0.8f, 1f); // Light pink for hover
                colors.pressedColor = new Color(0.9f, 0.5f, 0.6f, 1f); // Darker pink for press
                colors.selectedColor = new Color(1f, 1f, 1f, 1f); // White
                colors.disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.5f); // Dark grey
                colors.colorMultiplier = 1f;
                colors.fadeDuration = 0.1f;
                button.colors = colors;

                // Set transition to ColorTint
                button.transition = UnityEngine.UI.Selectable.Transition.ColorTint;

                // Add onClick listener
                button.onClick.AddListener(() => {
                    Debug.Log("Click!");

                    // Play kiss1 SFX
                    if (kissClip != null)
                    {
                        Singleton<AudioManager>.Instance.UserInterface.Play(kissClip, AudioConfigSoundUI.Default, Args.EMPTY);
                    }

                    // Disable UI_GiftItem_Androids_Canvas
                    Signals.Emit(MainStory.fadeUISignal);
                    giftCanvasClone.SetActive(false);
                    Debug.Log("[GiftUI] Disabled UI_GiftItem_Androids_Canvas");
                });

                Debug.Log("[InitializeGiftUICanvas] Configured Button component");
            }

            Debug.Log("[InitializeGiftUICanvas] Gift UI canvas initialization complete");
            return giftCanvasClone;
        }

        /// <summary>
        /// Removes EnableChildren components from all gift lists.
        /// These components control child visibility based on vanilla GNVs, which breaks our custom gift items.
        /// </summary>
        private void RemoveEnableChildrenComponents(GameObject giftCanvasClone)
        {
            Transform[] giftLists = new Transform[]
            {
                giftCanvasClone.transform.Find("gift_list"),
                giftCanvasClone.transform.Find("gift_list (1)"),
                giftCanvasClone.transform.Find("gift_list (2)")
            };

            foreach (Transform giftList in giftLists)
            {
                if (giftList == null) continue;

                // Find and destroy EnableChildren component by name (GameCreator component)
                // Use GetComponents and check type name since we don't have direct reference
                var components = giftList.GetComponents<UnityEngine.Component>();
                foreach (var component in components)
                {
                    if (component != null && component.GetType().Name == "EnableChildren")
                    {
                        GameObject.DestroyImmediate(component);
                        Debug.Log($"[RemoveEnableChildrenComponents] Removed EnableChildren from {giftList.name}");
                    }
                }
            }
        }

        private void ConfigureGiftListLayouts(GameObject giftCanvasClone)
        {
            // The LayoutElement components on children will control their sizes
            // Just log that we've set up the layout
            Transform[] giftLists = new Transform[]
            {
                giftCanvasClone.transform.Find("gift_list"),
                giftCanvasClone.transform.Find("gift_list (1)"),
                giftCanvasClone.transform.Find("gift_list (2)")
            };

            foreach (Transform giftList in giftLists)
            {
                if (giftList != null)
                {
                    Debug.Log($"[ConfigureGiftListLayouts] {giftList.name} ready for gift items with LayoutElement sizing");
                }
            }
        }

        public static void AddGiftItem(string name, string displayName, bool isVanillaGNV, string textureFileName)
        {
            // Find UI_GiftItem_Androids_Canvas (may be disabled)
            Transform giftCanvasTransform = Core.FindInActiveObjectByName("UI_GiftItem_Androids_Canvas");
            if (giftCanvasTransform == null)
            {
                Debug.LogError("[AddGiftItem] UI_GiftItem_Androids_Canvas not found");
                return;
            }
            GameObject giftCanvas = giftCanvasTransform.gameObject;

            // Find a gift_list with space (max 7 children per list)
            Transform targetGiftList = null;
            Transform giftList = giftCanvas.transform.Find("gift_list");
            Transform giftList2 = giftCanvas.transform.Find("gift_list (1)");
            Transform giftList3 = giftCanvas.transform.Find("gift_list (2)");

            if (giftList != null && giftList.childCount < 7)
                targetGiftList = giftList;
            else if (giftList2 != null && giftList2.childCount < 7)
                targetGiftList = giftList2;
            else if (giftList3 != null && giftList3.childCount < 7)
                targetGiftList = giftList3;

            if (targetGiftList == null)
            {
                Debug.LogError("[AddGiftItem] All gift lists are full (7 items each)");
                return;
            }

            // Get template
            if (giftItemTemplate == null)
            {
                Debug.LogError("[AddGiftItem] Gift item template not initialized");
                return;
            }

            // Instantiate gift item
            GameObject newGiftItem = GameObject.Instantiate(giftItemTemplate, targetGiftList);
            newGiftItem.SetActive(true);
            newGiftItem.name = name;
            
            // Force size to 175x175
            RectTransform giftItemRect = newGiftItem.GetComponent<RectTransform>();
            if (giftItemRect != null)
            {
                giftItemRect.sizeDelta = new Vector2(175f, 175f);
                Debug.Log($"[AddGiftItem] Set {name} size to 175x175");
            }
            
            // Store vanilla/proxy mapping for visibility checks
            giftVanillaMap[name] = isVanillaGNV;
            
            Debug.Log($"[AddGiftItem] Created gift item: {name}");

            // Update Text (TMP) display name
            Transform textChild = newGiftItem.transform.Find("Text (TMP)");
            if (textChild != null)
            {
                TextMeshProUGUI textComponent = textChild.GetComponent<TextMeshProUGUI>();
                if (textComponent != null)
                {
                    textComponent.text = displayName;
                    Debug.Log($"[AddGiftItem] Set display name to: {displayName}");
                }
            }

            // Update Image (2) sprite
            Transform imageChild = newGiftItem.transform.Find("Image (2)");
            if (imageChild != null)
            {
                UnityEngine.UI.Image imageComponent = imageChild.GetComponent<UnityEngine.UI.Image>();
                if (imageComponent != null)
                {
                    // Load texture from Core.itemsPath using file I/O
                    string texturePath = Core.itemsPath + textureFileName;
                    if (System.IO.File.Exists(texturePath))
                    {
                        try
                        {
                            byte[] fileData = System.IO.File.ReadAllBytes(texturePath);
                            Texture2D texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
                            texture.LoadImage(fileData);
                            
                            // Create sprite from texture
                            Sprite newSprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
                            imageComponent.sprite = newSprite;
                            Debug.Log($"[AddGiftItem] Set image sprite to: {textureFileName}");
                        }
                        catch (Exception e)
                        {
                            Debug.LogError($"[AddGiftItem] Error loading texture: {e.Message}");
                        }
                    }
                    else
                    {
                        Debug.LogWarning($"[AddGiftItem] Texture file not found at: {texturePath}");
                    }
                }
            }

            // Configure Button
            Transform buttonChild = newGiftItem.transform.Find("Button");
            if (buttonChild != null)
            {
                UnityEngine.UI.Button button = buttonChild.GetComponent<UnityEngine.UI.Button>();
                if (button == null)
                {
                    button = buttonChild.gameObject.AddComponent<UnityEngine.UI.Button>();
                }

                // Clear existing listeners
                button.onClick.RemoveAllListeners();

                // Capture variables for the lambda
                string giftName = name;
                bool vanillaGNV = isVanillaGNV;
                GameObject giftCanvasRef = giftCanvas;

                // Load audio clip
                AudioClip kissClip = Core.otherBundle.LoadAsset<AudioClip>("kiss1");

                // Add onClick listener
                button.onClick.AddListener(() => {
                    Debug.Log("Click!");

                    // Play kiss1 SFX
                    if (kissClip != null)
                    {
                        Singleton<AudioManager>.Instance.UserInterface.Play(kissClip, AudioConfigSoundUI.Default, Args.EMPTY);
                    }

                    // Increment affection if character likes this gift
                    string giftingTarget = Core.GetProxyVariableString("Gifting_Target", "");
                    if (!string.IsNullOrEmpty(giftingTarget))
                    {
                        Core.IncrementAffectionForGiftIfLiked(giftName, giftingTarget);
                    }
                    else
                    {
                        Debug.LogWarning("[GiftUI] Gifting_Target proxy variable not found or empty");
                    }

                    // Set Gifting_Gift to this gift's name
                    Core.FindAndModifyProxyVariableString("Gifting_Gift", giftName);

                    // Set Gifting_Gifted to true
                    Core.FindAndModifyProxyVariableBool("Gifting_Gifted", true);

                    // Set DailyProc_Gifting_[CHARNAME] to true
                    string giftingTargetForDaily = Core.GetProxyVariableString("Gifting_Target", "");
                    if (!string.IsNullOrEmpty(giftingTargetForDaily))
                    {
                        string dailyProcVarName = $"DailyProc_Gifting_{giftingTargetForDaily}";
                        Core.FindAndModifyProxyVariableBool(dailyProcVarName, true);
                    }

                    // Set GNV to False (vanilla or proxy)
                    if (vanillaGNV)
                    {
                        Core.FindAndModifyVariableBool(giftName, false);
                        Debug.Log($"[GiftUI] Set vanilla GNV {giftName} to false");
                    }
                    else
                    {
                        if (Core.proxyVariables != null && Core.proxyVariables.Exists(giftName))
                        {
                            Core.SetAndSyncGiftVariable(giftName, false);
                            Debug.Log($"[GiftUI] Set and synced proxy GNV {giftName} to false");
                        }
                        else
                        {
                            Debug.LogWarning($"[GiftUI] Proxy variable '{giftName}' not found");
                        }
                    }

                    // Disable UI_GiftItem_Androids_Canvas
                    giftCanvasRef.SetActive(false);
                    Debug.Log("[GiftUI] Disabled UI_GiftItem_Androids_Canvas");
                });

                Debug.Log($"[AddGiftItem] Configured button for {name}");
            }

            Debug.Log($"[AddGiftItem] Successfully added gift: {name} to {targetGiftList.name}");
        }

        public static GameObject CreateMusicPlayer(string assetName)
        {
            try
            {
                // Find 12_AudioPlayer in the scene
                GameObject audioPlayerParent = GameObject.Find("12_AudioPlayer");
                if (audioPlayerParent == null)
                {
                    Debug.LogError("[CreateMusicPlayer] Could not find 12_AudioPlayer in the scene");
                    return null;
                }

                // Find the Music child object
                Transform musicTransform = audioPlayerParent.transform.Find("Beach");
                if (musicTransform == null)
                {
                    Debug.LogError("[CreateMusicPlayer] Could not find Music object under 12_AudioPlayer");
                    return null;
                }

                // Load the audio asset from OtherBundle
                AudioClip audioClip = Core.otherBundle.LoadAsset<AudioClip>(assetName);
                if (audioClip == null)
                {
                    Debug.LogError($"[CreateMusicPlayer] Could not load audio asset '{assetName}' from OtherBundle");
                    return null;
                }

                // Instantiate a copy of the Music object (disabled to prevent auto-play)
                GameObject newMusicPlayer = GameObject.Instantiate(musicTransform.gameObject, audioPlayerParent.transform);
                newMusicPlayer.SetActive(false);
                
                // Set the name to match the asset name
                newMusicPlayer.name = assetName;

                // Get the AudioSource component and set the clip
                AudioSource audioSource = newMusicPlayer.GetComponent<AudioSource>();
                if (audioSource != null)
                {
                    audioSource.clip = audioClip;
                    Debug.Log($"[CreateMusicPlayer] Created music player '{assetName}' (disabled) with AudioClip successfully");
                }
                else
                {
                    Debug.LogWarning($"[CreateMusicPlayer] Created music player '{assetName}' but AudioSource component not found");
                }

                return newMusicPlayer;
            }
            catch (Exception e)
            {
                Debug.LogError($"[CreateMusicPlayer] Error creating music player for '{assetName}': {e.Message}");
                return null;
            }
        }

        #region Harbor Home Talk Panel

        /// <summary>
        /// Initializes the Harbor Home Talk Panel UI that replaces the vanilla TalkButton
        /// when the player is in a Harbor Home room (excluding the entrance).
        /// The panel contains up to 2 character buttons for characters currently in that room.
        /// </summary>
        private void InitializeHHTalkPanel()
        {
            // Get reference to the vanilla TalkButton
            vanillaTalkButton = Core.mainCanvas.Find("TalkButton")?.gameObject;
            if (vanillaTalkButton == null)
            {
                Debug.LogError("[HHTalkPanel] Could not find TalkButton under mainCanvas");
                return;
            }

            // Generate rounded-corner sprites for panel, buttons, and labels (radius = 24)
            int cornerRadius = 24;
            hhPanelSprite = CreateRoundedRectSprite(128, 128, cornerRadius, true, true, true, true);
            hhButtonLeftSprite = CreateRoundedRectSprite(128, 128, cornerRadius, true, false, true, false);   // TL, BL rounded
            hhButtonRightSprite = CreateRoundedRectSprite(128, 128, cornerRadius, false, true, false, true);  // TR, BR rounded
            hhButtonFullSprite = CreateRoundedRectSprite(128, 128, cornerRadius, true, true, true, true);     // all corners rounded
            hhLabelLeftSprite = CreateRoundedRectSprite(128, 128, cornerRadius, false, false, true, false);   // BL rounded only
            hhLabelRightSprite = CreateRoundedRectSprite(128, 128, cornerRadius, false, false, false, true);  // BR rounded only
            hhLabelFullSprite = CreateRoundedRectSprite(128, 128, cornerRadius, false, false, true, true);    // both bottom corners

            // Create the parent panel as a sibling of TalkButton under mainCanvas
            hhTalkPanel = new GameObject("HHTalkPanel");
            hhTalkPanel.transform.SetParent(Core.mainCanvas, false);

            // Copy RectTransform position from TalkButton
            RectTransform talkButtonRect = vanillaTalkButton.GetComponent<RectTransform>();
            RectTransform panelRect = hhTalkPanel.AddComponent<RectTransform>();
            panelRect.anchorMin = talkButtonRect.anchorMin;
            panelRect.anchorMax = talkButtonRect.anchorMax;
            panelRect.pivot = talkButtonRect.pivot;
            panelRect.anchoredPosition = talkButtonRect.anchoredPosition + new Vector2(0f, -20f);
            panelRect.sizeDelta = new Vector2(300f, 200f);

            // Add a rounded-corner background image to the panel
            UnityEngine.UI.Image panelBG = hhTalkPanel.AddComponent<UnityEngine.UI.Image>();
            panelBG.sprite = hhPanelSprite;
            panelBG.type = UnityEngine.UI.Image.Type.Sliced;
            panelBG.color = new Color(0.08f, 0.06f, 0.12f, 0.80f);
            panelBG.raycastTarget = false;

            // Add Shadow component (black 50% alpha, offset 5, -5)
            Shadow panelShadow = hhTalkPanel.AddComponent<Shadow>();
            panelShadow.effectColor = new Color(0f, 0f, 0f, 0.5f);
            panelShadow.effectDistance = new Vector2(5f, -5f);

            // Add a horizontal layout group to arrange buttons side by side
            UnityEngine.UI.HorizontalLayoutGroup hlg = hhTalkPanel.AddComponent<UnityEngine.UI.HorizontalLayoutGroup>();
            hlg.spacing = 0f;
            hlg.padding = new RectOffset(0, 0, 0, 0);
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = true;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;

            // Place the panel right after TalkButton in sibling order
            hhTalkPanel.transform.SetSiblingIndex(vanillaTalkButton.transform.GetSiblingIndex() + 1);

            // Create the two character button slots (left and right)
            hhTalkButton1 = CreateHHTalkButton("HHTalkButton1", hhTalkPanel.transform, hhButtonLeftSprite, hhLabelLeftSprite);
            hhTalkButton2 = CreateHHTalkButton("HHTalkButton2", hhTalkPanel.transform, hhButtonRightSprite, hhLabelRightSprite);

            // Start hidden
            hhTalkPanel.SetActive(false);
            hhTalkPanelInitialized = true;

            Debug.Log("[HHTalkPanel] Initialized successfully");
        }

        /// <summary>
        /// Creates a single HH talk button with a background and semi-transparent character portrait.
        /// Uses the provided sprites for asymmetric rounded corners on button and label.
        /// </summary>
        private GameObject CreateHHTalkButton(string name, Transform parent, Sprite cornerSprite, Sprite labelSprite)
        {
            GameObject buttonGO = new GameObject(name);
            buttonGO.transform.SetParent(parent, false);

            RectTransform rect = buttonGO.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(130f, 180f);

            // LayoutElement so the HorizontalLayoutGroup gives each button equal flexible width
            UnityEngine.UI.LayoutElement layoutElem = buttonGO.AddComponent<UnityEngine.UI.LayoutElement>();
            layoutElem.flexibleWidth = 1f;

            // Background image with rounded corners
            UnityEngine.UI.Image bgImage = buttonGO.AddComponent<UnityEngine.UI.Image>();
            bgImage.sprite = cornerSprite;
            bgImage.type = UnityEngine.UI.Image.Type.Sliced;
            bgImage.color = new Color(0.15f, 0.12f, 0.2f, 0.85f);

            // Add Unity Button component
            UnityEngine.UI.Button button = buttonGO.AddComponent<UnityEngine.UI.Button>();
            var colors = button.colors;
            colors.normalColor = new Color(1f, 1f, 1f, 0.5f);
            colors.highlightedColor = new Color(0.85f, 0.75f, 1f, 1f);
            colors.pressedColor = new Color(0.7f, 0.55f, 0.9f, 1f);
            colors.selectedColor = new Color(1f, 1f, 1f, 0.8f);
            colors.disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
            colors.colorMultiplier = 1f;
            colors.fadeDuration = 0.1f;
            button.colors = colors;
            button.transition = UnityEngine.UI.Selectable.Transition.ColorTint;
            button.targetGraphic = bgImage;

            // Character portrait (height = button height, width adjusts to aspect ratio, centered, 90% opaque)
            GameObject portraitGO = new GameObject("Portrait");
            portraitGO.transform.SetParent(buttonGO.transform, false);
            RectTransform portraitRect = portraitGO.AddComponent<RectTransform>();
            // Anchor to center, size controlled by AspectRatioFitter
            portraitRect.anchorMin = new Vector2(0.5f, 0f);
            portraitRect.anchorMax = new Vector2(0.5f, 1f);
            portraitRect.offsetMin = new Vector2(-65f, 0f); // fallback width before fitter kicks in
            portraitRect.offsetMax = new Vector2(65f, 0f);
            UnityEngine.UI.Image portraitImage = portraitGO.AddComponent<UnityEngine.UI.Image>();
            portraitImage.color = new Color(1f, 1f, 1f, 0.9f);
            portraitImage.raycastTarget = false;
            portraitImage.preserveAspect = false;

            // AspectRatioFitter ensures height always matches button, width scales proportionally
            UnityEngine.UI.AspectRatioFitter arf = portraitGO.AddComponent<UnityEngine.UI.AspectRatioFitter>();
            arf.aspectMode = UnityEngine.UI.AspectRatioFitter.AspectMode.HeightControlsWidth;
            arf.aspectRatio = 0.72f; // sensible default; updated when sprite is assigned in ConfigureHHTalkButton

            // Character name label at the bottom with matching rounded corners
            GameObject labelGO = new GameObject("Label");
            labelGO.transform.SetParent(buttonGO.transform, false);
            RectTransform labelRect = labelGO.AddComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0f, 0f);
            labelRect.anchorMax = new Vector2(1f, 0.2f);
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;

            // Label background with matching rounded bottom corners
            UnityEngine.UI.Image labelBG = labelGO.AddComponent<UnityEngine.UI.Image>();
            labelBG.sprite = labelSprite;
            labelBG.type = UnityEngine.UI.Image.Type.Sliced;
            labelBG.color = new Color(0f, 0f, 0f, 0.5f);
            labelBG.raycastTarget = false;

            GameObject textGO = new GameObject("Text");
            textGO.transform.SetParent(labelGO.transform, false);
            RectTransform textRect = textGO.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            TextMeshProUGUI tmpText = textGO.AddComponent<TextMeshProUGUI>();
            tmpText.text = "";
            tmpText.fontSize = 16;
            tmpText.alignment = TextAlignmentOptions.Center;
            tmpText.color = Color.white;
            tmpText.raycastTarget = false;

            buttonGO.SetActive(false);
            return buttonGO;
        }

        /// <summary>
        /// Called every frame when dialogues are loaded. Manages visibility and content
        /// of the Harbor Home talk panel based on which HH room level is currently active.
        /// </summary>
        /// <param name="immediate">If true, bypasses throttle and forces immediate button rebuild</param>
        public static void UpdateHHTalkPanel(bool immediate = false)
        {
            if (!hhTalkPanelInitialized || hhTalkPanel == null || vanillaTalkButton == null)
                return;

            // Throttle updates unless immediate is true
            if (!immediate && Time.time - hhTalkLastUpdateTime < hhTalkUpdateInterval)
                return;
            hhTalkLastUpdateTime = Time.time;

            // Determine which HH room (non-entrance) is currently active
            string activeRoom = GetActiveHHRoom();

            if (activeRoom != null)
            {
                // We're in a Harbor Home room - hide vanilla talk button, show our panel
                if (vanillaTalkButton.activeSelf)
                {
                    vanillaTalkButton.SetActive(false);
                }

                if (!hhTalkPanel.activeSelf)
                {
                    hhTalkPanel.SetActive(true);
                }

                // Check if room changed or schedule updated
                bool roomChanged = activeRoom != hhTalkLastActiveRoom;
                bool scheduleChanged = Schedule.scheduleVersion != hhTalkLastScheduleVersion;
                
                if (roomChanged)
                {
                    hhTalkLastActiveRoom = activeRoom;
                }
                
                if (scheduleChanged)
                {
                    hhTalkLastScheduleVersion = Schedule.scheduleVersion;
                }
                
                // Rebuild buttons when room or schedule changes, or when immediate is requested
                if (roomChanged || scheduleChanged || immediate)
                {
                    PopulateHHTalkButtons(activeRoom);
                }

                // Update button interactability based on HarborHome_TalkSelected and Lock-Game
                bool canInteract = string.IsNullOrEmpty(SaveManager.GetString("HarborHome_TalkSelected")) && !Core.GetVariableBool("Lock-Game");
                if (hhTalkButton1 != null && hhTalkButton1.activeSelf)
                {
                    UnityEngine.UI.Button btn1 = hhTalkButton1.GetComponent<UnityEngine.UI.Button>();
                    if (btn1 != null) btn1.interactable = canInteract;
                }
                if (hhTalkButton2 != null && hhTalkButton2.activeSelf)
                {
                    UnityEngine.UI.Button btn2 = hhTalkButton2.GetComponent<UnityEngine.UI.Button>();
                    if (btn2 != null) btn2.interactable = canInteract;
                }
            }
            else
            {
                // Not in a Harbor Home room - restore vanilla talk button
                if (hhTalkPanel.activeSelf)
                {
                    hhTalkPanel.SetActive(false);
                }

                // Only re-enable the vanilla TalkButton if it was disabled by us
                // (it has its own Conditions component that controls visibility)
                if (hhTalkLastActiveRoom != null)
                {
                    vanillaTalkButton.SetActive(true);
                    hhTalkLastActiveRoom = null;
                }
            }
        }

        /// <summary>
        /// Returns the room name of the currently active HH level (excluding entrance), or null.
        /// </summary>
        private static string GetActiveHHRoom()
        {
            if (Places.harborHomeBathroomLevel != null && Places.harborHomeBathroomLevel.activeSelf)
                return "HarborHomeBathroom";
            if (Places.harborHomeBedroomLevel != null && Places.harborHomeBedroomLevel.activeSelf)
                return "HarborHomeBedroom";
            if (Places.harborHomeClosetLevel != null && Places.harborHomeClosetLevel.activeSelf)
                return "HarborHomeCloset";
            if (Places.harborHomeKitchenLevel != null && Places.harborHomeKitchenLevel.activeSelf)
                return "HarborHomeKitchen";
            if (Places.harborHomeLivingroomLevel != null && Places.harborHomeLivingroomLevel.activeSelf)
                return "HarborHomeLivingRoom";
            if (Places.harborHomePoolLevel != null && Places.harborHomePoolLevel.activeSelf)
                return "HarborHomePool";
            return null;
        }

        /// <summary>
        /// Finds characters whose HH location is in the given room and populates up to 2 buttons.
        /// </summary>
        private static void PopulateHHTalkButtons(string activeRoom)
        {
            List<string> charactersInRoom = new List<string>();

            foreach (string charName in hhTalkCharacterNames)
            {
                if (!SaveManager.GetBool("HarborHome_Visit_" + charName))
                    continue;

                // The main location variable is already set to the HH position by the roaming system
                string charLocation = Schedule.GetCharacterLocation(charName);
                if (string.IsNullOrEmpty(charLocation))
                    continue;

                // Extract the room name from the full location (e.g. "HarborHomeBathroomShower" -> "HarborHomeBathroom")
                string charRoom = GetHHTalkRoomNameFromLocation(charLocation);
                if (charRoom == activeRoom)
                {
                    charactersInRoom.Add(charName);
                    if (charactersInRoom.Count >= 2)
                        break; // Max 2 buttons
                }
            }

            // Configure button 1 (always left slot)
            if (charactersInRoom.Count >= 1)
            {
                ConfigureHHTalkButton(hhTalkButton1, charactersInRoom[0]);
                hhTalkButton1.SetActive(true);

                if (charactersInRoom.Count == 1)
                {
                    // Single button fills the whole panel - use fully rounded corners
                    hhTalkButton1.GetComponent<UnityEngine.UI.Image>().sprite = hhButtonFullSprite;
                    Transform label1 = hhTalkButton1.transform.Find("Label");
                    if (label1 != null) label1.GetComponent<UnityEngine.UI.Image>().sprite = hhLabelFullSprite;
                }
                else
                {
                    // Two buttons - left button gets left-rounded corners
                    hhTalkButton1.GetComponent<UnityEngine.UI.Image>().sprite = hhButtonLeftSprite;
                    Transform label1 = hhTalkButton1.transform.Find("Label");
                    if (label1 != null) label1.GetComponent<UnityEngine.UI.Image>().sprite = hhLabelLeftSprite;
                }
            }
            else
            {
                hhTalkButton1.SetActive(false);
            }

            // Configure button 2 (right slot)
            if (charactersInRoom.Count >= 2)
            {
                ConfigureHHTalkButton(hhTalkButton2, charactersInRoom[1]);
                hhTalkButton2.SetActive(true);
            }
            else
            {
                hhTalkButton2.SetActive(false);
            }

            // If no characters found, hide the panel entirely
            if (charactersInRoom.Count == 0)
            {
                hhTalkPanel.SetActive(false);
                // Restore vanilla talk button when no chars are present
                vanillaTalkButton.SetActive(true);
            }

            Debug.Log($"[HHTalkPanel] Room '{activeRoom}': {charactersInRoom.Count} character(s) found: {string.Join(", ", charactersInRoom)}");
        }

        /// <summary>
        /// Configures a single HH talk button with the given character's portrait and click handler.
        /// </summary>
        private static void ConfigureHHTalkButton(GameObject buttonGO, string characterName)
        {
            // Update the portrait image with the character's bust texture
            UnityEngine.UI.Image portraitImage = buttonGO.transform.Find("Portrait")?.GetComponent<UnityEngine.UI.Image>();
            if (portraitImage != null)
            {
                Sprite bustSprite = GetCharacterBustSprite(characterName);
                if (bustSprite != null)
                {
                    portraitImage.sprite = bustSprite;
                    portraitImage.color = new Color(1f, 1f, 1f, 0.9f);

                    // Update AspectRatioFitter with the actual sprite's aspect ratio
                    UnityEngine.UI.AspectRatioFitter arf = portraitImage.GetComponent<UnityEngine.UI.AspectRatioFitter>();
                    if (arf != null)
                    {
                        arf.aspectRatio = bustSprite.rect.width / bustSprite.rect.height;
                    }
                }
                else
                {
                    portraitImage.sprite = null;
                    portraitImage.color = new Color(1f, 1f, 1f, 0f);
                }
            }

            // Update the label text
            TextMeshProUGUI labelText = buttonGO.transform.Find("Label")?.Find("Text")?.GetComponent<TextMeshProUGUI>();
            if (labelText != null)
            {
                labelText.text = characterName;
            }

            // Set up click handler
            UnityEngine.UI.Button button = buttonGO.GetComponent<UnityEngine.UI.Button>();
            if (button != null)
            {
                // Only allow interaction if no character is currently selected
                button.interactable = string.IsNullOrEmpty(SaveManager.GetString("HarborHome_TalkSelected"));

                button.onClick.RemoveAllListeners();
                string capturedName = characterName;
                button.onClick.AddListener(() =>
                {
                    Debug.Log($"[HHTalkPanel] Selected character: {capturedName}");
                    SaveManager.SetString("HarborHome_TalkSelected", capturedName);

                    // Play UI button sound
                    AudioClip buttonSound = Core.otherBundle?.LoadAsset<AudioClip>("Button-1");
                    if (buttonSound != null)
                    {
                        Singleton<AudioManager>.Instance.UserInterface.Play(buttonSound, AudioConfigSoundUI.Default, Args.EMPTY);
                    }
                });
            }
        }

        /// <summary>
        /// Gets the base bust sprite for a character (used for the portrait in HH talk buttons).
        /// Returns the sprite from the character's default bust GameObject's MBase1 SpriteRenderer.
        /// </summary>
        private static Sprite GetCharacterBustSprite(string characterName)
        {
            GameObject bust = GetDefaultBustForCharacter(characterName);
            if (bust == null)
                return null;

            Transform mBase = bust.transform.Find("MBase1");
            if (mBase == null)
                return null;

            SpriteRenderer sr = mBase.GetComponent<SpriteRenderer>();
            return sr?.sprite;
        }

        /// <summary>
        /// Returns the default bust GameObject for a character by name.
        /// </summary>
        private static GameObject GetDefaultBustForCharacter(string characterName)
        {
            switch (characterName)
            {
                case "Amber": return Characters.amber;
                case "Claire": return Characters.claire;
                case "Sarah": return Characters.sarah;
                case "Anis": return Characters.anis;
                case "Centi": return Characters.centi;
                case "Dorothy": return Characters.dorothy;
                case "Elegg": return Characters.elegg;
                case "Frima": return Characters.frima;
                case "Guilty": return Characters.guilty;
                case "Helm": return Characters.helm;
                case "Maiden": return Characters.maiden;
                case "Mary": return Characters.mary;
                case "Mast": return Characters.mast;
                case "Neon": return Characters.neon;
                case "Pepper": return Characters.pepper;
                case "Rapi": return Characters.rapi;
                case "Rosanna": return Characters.rosanna;
                case "Sakura": return Characters.sakura;
                case "Tove": return Characters.tove;
                case "Viper": return Characters.viper;
                case "Yan": return Characters.yan;
                default: return null;
            }
        }

        /// <summary>
        /// Extracts the room name from a full HH location string.
        /// E.g. "HarborHomeBathroomShower" -> "HarborHomeBathroom"
        /// </summary>
        private static string GetHHTalkRoomNameFromLocation(string location)
        {
            if (string.IsNullOrEmpty(location))
                return null;

            foreach (string roomName in hhTalkRoomNames)
            {
                if (location.StartsWith(roomName))
                    return roomName;
            }
            return location;
        }

        /// <summary>
        /// Creates a rounded-rect Sprite at runtime with selectable per-corner rounding.
        /// The sprite is set up with 9-slice borders so Unity's Sliced image mode stretches it properly.
        /// </summary>
        /// <param name="width">Texture width in pixels</param>
        /// <param name="height">Texture height in pixels</param>
        /// <param name="radius">Corner radius in pixels</param>
        /// <param name="roundTL">Round top-left corner</param>
        /// <param name="roundTR">Round top-right corner</param>
        /// <param name="roundBL">Round bottom-left corner</param>
        /// <param name="roundBR">Round bottom-right corner</param>
        private static Sprite CreateRoundedRectSprite(int width, int height, int radius, bool roundTL, bool roundTR, bool roundBL, bool roundBR)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            Color32 white = new Color32(255, 255, 255, 255);
            Color32 clear = new Color32(0, 0, 0, 0);

            Color32[] pixels = new Color32[width * height];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    bool inside = true;

                    // Bottom-left corner
                    if (roundBL && x < radius && y < radius)
                    {
                        float dist = Mathf.Sqrt((x - radius) * (x - radius) + (y - radius) * (y - radius));
                        if (dist > radius) inside = false;
                    }
                    // Bottom-right corner
                    if (roundBR && x >= width - radius && y < radius)
                    {
                        float dist = Mathf.Sqrt((x - (width - radius - 1)) * (x - (width - radius - 1)) + (y - radius) * (y - radius));
                        if (dist > radius) inside = false;
                    }
                    // Top-left corner
                    if (roundTL && x < radius && y >= height - radius)
                    {
                        float dist = Mathf.Sqrt((x - radius) * (x - radius) + (y - (height - radius - 1)) * (y - (height - radius - 1)));
                        if (dist > radius) inside = false;
                    }
                    // Top-right corner
                    if (roundTR && x >= width - radius && y >= height - radius)
                    {
                        float dist = Mathf.Sqrt((x - (width - radius - 1)) * (x - (width - radius - 1)) + (y - (height - radius - 1)) * (y - (height - radius - 1)));
                        if (dist > radius) inside = false;
                    }

                    pixels[y * width + x] = inside ? white : clear;
                }
            }

            tex.SetPixels32(pixels);
            tex.Apply();
            tex.filterMode = FilterMode.Bilinear;

            // 9-slice borders: the corner radius determines the border size
            Vector4 border = new Vector4(radius, radius, radius, radius); // left, bottom, right, top
            Sprite sprite = Sprite.Create(tex, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, border);
            return sprite;
        }
        #endregion
    }
}
