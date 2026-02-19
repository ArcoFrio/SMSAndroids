using BepInEx;
using GameCreator;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.Common.Audio;
using GameCreator.Runtime.Common.SaveSystem;
using GameCreator.Runtime.Common.UnityUI;
using GameCreator.Runtime.Dialogue;
using GameCreator.Runtime.Dialogue.UnityUI;
using GameCreator.Runtime.Variables;
using GameCreator.Runtime.VisualScripting;
using HarmonyLib;
using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Policy;
using System.Xml.XPath;
using TMPro;
using TransitionsPlusDemos;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.InputSystem;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.UIElements;
using static UnityEngine.GraphicsBuffer;
using System.Xml.Serialization;
using static System.Net.Mime.MediaTypeNames;

namespace SMSAndroidsCore
{
    [BepInPlugin(pluginGuid, Core.pluginName + " - SaveManager", Core.pluginVersion)]
    internal class Wallpaper : BaseUnityPlugin
    {
        #region Plugin Info
        public const string pluginGuid = "treboy.starmakerstory.smsandroidscore.wallpaper";

        public static bool loadedWallpaper = false;
        public static GameObject baseWallpaper;
        public static GameObject baseWallpaperButton;
        public static GameObject wallpaperSnek;
        public static GameObject wallpaperSnekButton;
        public static int vanillaWallpaperCount = 0;
        public static int wallpaperSnekIndex = -1;

        public static GameObject wallpaperButtonAnis1;
        public static GameObject wallpaperButtonDorothy1;
        public static GameObject wallpaperButtonHelm1;
        public static GameObject wallpaperButtonSolidGearOfMetal;
        #endregion
        public void Update()
        {
            if (Core.currentScene.name == "CoreGameScene")
            {
                if (!loadedWallpaper && Core.loadedBases)
                {
                    baseWallpaper = Core.mainCanvas.Find("Desktop").Find("Wallpaper").Find("Wallpaper (0)").gameObject;
                    baseWallpaperButton = Core.mainCanvas.Find("Wallpaperselection").Find("UI_Core").Find("List").Find("WallpaperButton (0)").gameObject;
                    vanillaWallpaperCount = baseWallpaper.transform.parent.childCount;

                    CreateWallpaper("AnisSwimsuit", Core.wallpaperPath + "WallpaperAnis1.PNG");
                    wallpaperButtonAnis1 = Core.mainCanvas.Find("Wallpaperselection").Find("UI_Core").Find("List").Find("WallpaperButton (AnisSwimsuit)").gameObject;
                    CreateWallpaper("DorothySwimsuit", Core.wallpaperPath + "WallpaperDorothy1.PNG");
                    wallpaperButtonDorothy1 = Core.mainCanvas.Find("Wallpaperselection").Find("UI_Core").Find("List").Find("WallpaperButton (DorothySwimsuit)").gameObject;
                    CreateWallpaper("HelmSwimsuit", Core.wallpaperPath + "WallpaperHelm1.PNG");
                    wallpaperButtonHelm1 = Core.mainCanvas.Find("Wallpaperselection").Find("UI_Core").Find("List").Find("WallpaperButton (HelmSwimsuit)").gameObject;
                    CreateWallpaper("SolidGearOfMetal", Core.wallpaperPath + "Solid.PNG");
                    wallpaperButtonSolidGearOfMetal = Core.mainCanvas.Find("Wallpaperselection").Find("UI_Core").Find("List").Find("WallpaperButton (SolidGearOfMetal)").gameObject;

                    if (SaveManager.GetInt("Wallpaper_Current") > -1)
                    {
                        foreach (Transform child in Core.mainCanvas.Find("Desktop").Find("Wallpaper"))
                        {
                            child.gameObject.SetActive(false);
                        }
                        Core.mainCanvas.Find("Desktop").Find("Wallpaper").GetChild(SaveManager.GetInt("Wallpaper_Current")).gameObject.SetActive(true);
                    }

                    loadedWallpaper = true;
                    Logger.LogInfo("----- WALLPAPERS LOADED -----");
                }
                if (loadedWallpaper)
                {
                    wallpaperButtonAnis1.SetActive((SaveManager.GetBool("Event_SeenAnisMall01")));
                    wallpaperButtonDorothy1.SetActive((SaveManager.GetBool("Event_SeenDorothyPark01")));
                    wallpaperButtonHelm1.SetActive((SaveManager.GetBool("Event_SeenHelmBeach01")));
                    wallpaperButtonSolidGearOfMetal.SetActive((SaveManager.GetBool("Event_SeenIt01")));
                    // Sync the current active wallpaper index to Wallpaper_Current
                    // Only sync when Wallpaper_Current has been explicitly set (not default -1)
                    int currentWallpaperIdx = SaveManager.GetInt("Wallpaper_Current");
                    if (currentWallpaperIdx > -1)
                    {
                        Transform wallpaperParent = Core.mainCanvas.Find("Desktop").Find("Wallpaper");
                        // Find the single active wallpaper
                        int activeCount = 0;
                        int firstActiveIdx = -1;
                        for (int i = 0; i < wallpaperParent.childCount; i++)
                        {
                            if (wallpaperParent.GetChild(i).gameObject.activeSelf)
                            {
                                activeCount++;
                                if (firstActiveIdx == -1) firstActiveIdx = i;
                            }
                        }
                        // If multiple wallpapers are active, deactivate all except the saved one
                        if (activeCount > 1)
                        {
                            for (int i = 0; i < wallpaperParent.childCount; i++)
                            {
                                wallpaperParent.GetChild(i).gameObject.SetActive(i == currentWallpaperIdx);
                            }
                        }
                        else if (activeCount == 1 && firstActiveIdx != currentWallpaperIdx)
                        {
                            SaveManager.SetInt("Wallpaper_Current", firstActiveIdx);
                        }
                    }
                }
            }
            if (Core.currentScene.name == "GameStart")
            {
                if (loadedWallpaper)
                {
                    loadedWallpaper = false;
                }
            }
        }

        /// <summary>
        /// Creates a new wallpaper with button and all required components.
        /// </summary>
        /// <param name="wallpaperName">Name for the wallpaper (e.g., "SolidGearOfMetal")</param>
        /// <param name="texturePath">Full path to the texture file</param>
        /// <returns>The index of the created wallpaper</returns>
        private int CreateWallpaper(string wallpaperName, string texturePath)
        {
            // Create wallpaper display
            GameObject wallpaper = Instantiate(baseWallpaper, baseWallpaper.transform.parent);
            wallpaper.name = wallpaperName;
            int wallpaperIndex = wallpaper.transform.GetSiblingIndex();

            // Load and apply texture
            Texture2D tex = new Texture2D(1920, 1080);
            tex.LoadImage(File.ReadAllBytes(texturePath));
            Sprite newSprite = Sprite.Create(tex, new Rect(0, 0, 1920, 1080), new Vector2(0.5f, 0.5f));
            wallpaper.GetComponent<UnityEngine.UI.Image>().sprite = newSprite;

            // Create wallpaper button
            GameObject wallpaperButton = Instantiate(baseWallpaperButton, baseWallpaperButton.transform.parent);
            wallpaperButton.name = $"WallpaperButton ({wallpaperName})";
            wallpaperButton.GetComponent<UnityEngine.UI.Image>().sprite = newSprite;

            // Remove old ButtonInstructions component
            ButtonInstructions oldButtonInstructions = wallpaperButton.GetComponent<ButtonInstructions>();
            if (oldButtonInstructions != null)
            {
                DestroyImmediate(oldButtonInstructions);
            }

            // Get or create Button component
            UnityEngine.UI.Button button = wallpaperButton.GetComponent<UnityEngine.UI.Button>();
            if (button == null)
            {
                button = wallpaperButton.AddComponent<UnityEngine.UI.Button>();
            }

            // Setup target graphic (child Image)
            Transform childImage = wallpaperButton.transform.GetChild(0);
            if (childImage != null)
            {
                UnityEngine.UI.Image childImageComponent = childImage.GetComponent<UnityEngine.UI.Image>();
                if (childImageComponent != null)
                {
                    button.targetGraphic = childImageComponent;

                    // Setup color transitions
                    ColorBlock colors = button.colors;
                    colors.normalColor = new Color(1, 1, 1, 0); // Transparent white
                    colors.highlightedColor = Color.white; // Fully white on hover
                    colors.pressedColor = new Color(0.7f, 0.7f, 0.7f, 1); // Gray on click for visual feedback
                    colors.selectedColor = Color.white;
                    button.colors = colors;

                    // Set transition type to color
                    button.transition = UnityEngine.UI.Selectable.Transition.ColorTint;
                }
            }

            // Load and setup audio
            AudioClip uiSelectClip = Core.otherBundle.LoadAsset<AudioClip>("UI_Select");
            if (uiSelectClip != null)
            {
                AudioSource audioSource = wallpaperButton.GetComponent<AudioSource>();
                if (audioSource == null)
                {
                    audioSource = wallpaperButton.AddComponent<AudioSource>();
                }
                audioSource.clip = uiSelectClip;
                audioSource.playOnAwake = false;
            }

            // Wire up the onClick listener
            int buttonIndex = wallpaperIndex;
            button.onClick.AddListener(() =>
            {
                Debug.Log($"[Wallpaper] {wallpaperName} button clicked! Setting Wallpaper variable to {buttonIndex}");
                Core.FindAndModifyVariableDouble("Wallpaper", buttonIndex);
                UpdateWallpaperDisplay(buttonIndex);

                // Play audio
                AudioSource audioSource = wallpaperButton.GetComponent<AudioSource>();
                if (audioSource != null && audioSource.clip != null)
                {
                    audioSource.PlayOneShot(audioSource.clip, 0.5f);
                }
            });
            wallpaperButton.SetActive(false);
            Debug.Log($"[Wallpaper] Created wallpaper '{wallpaperName}' at index {wallpaperIndex}");
            return wallpaperIndex;
        }
        
        private static void UpdateWallpaperDisplay(int wallpaperIndex)
        {
            // Deactivate all wallpapers first
            Transform wallpaperParent = Core.mainCanvas.Find("Desktop").Find("Wallpaper");
            if (wallpaperParent == null) return;
            
            for (int i = 0; i < wallpaperParent.childCount; i++)
            {
                wallpaperParent.GetChild(i).gameObject.SetActive(false);
            }
            
            // Activate the selected wallpaper
            Transform selectedWallpaper = wallpaperParent.GetChild(wallpaperIndex);
            if (selectedWallpaper != null)
            {
                selectedWallpaper.gameObject.SetActive(true);
                Debug.Log($"[Wallpaper] Activated wallpaper at index {wallpaperIndex}");
            }
            else
            {
                Debug.LogError($"[Wallpaper] Wallpaper at index {wallpaperIndex} not found");
            }
        }
    }
}
