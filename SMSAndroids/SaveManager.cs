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

namespace SMSAndroidsCore
{
    [BepInPlugin(pluginGuid, Core.pluginName + " - SaveManager", Core.pluginVersion)]
    internal class SaveManager : BaseUnityPlugin
    {
        #region Plugin Info
        public const string pluginGuid = "treboy.starmakerstory.smsandroidscore.savemanager";
        #endregion

        private static SaveManager instance;
        
        // Dictionary to cache values for the current save slot
        private Dictionary<string, object> currentSlotCache = new Dictionary<string, object>();
        private int currentSaveSlot = -1;
        private string currentSaveFilePath;
        private bool afterSleepEventsProc = false;
        private bool autosaveProcedThisSession = false;  // Tracks if autosave happened during this gameplay session
        private bool saveMenuListenersAttached = false;  // Prevents listener stacking
        private bool newGame = false;
        private bool modSaveThisSession = false;
        private int pendingOverwriteSlot = -1;

        // Default values for all mod variables
        private static readonly Dictionary<string, object> defaultValues = new Dictionary<string, object>
        {
            // Gift Shop variables
            { "GiftShop_BuildCounter", 0 },
            { "GiftShop_FirstVisited", false },

            // Harbor Home Variables
            { "HarborHome_Bought", false },
            { "HarborHome_FirstVisited", false },
            { "HarborHome_Outfit_Anis", "Default" },
            { "HarborHome_Slept", false },
            { "HarborHome_TalkSelected", "" },
            { "HarborHome_Visit_Amber", false },
            { "HarborHome_Visit_Claire", false },
            { "HarborHome_Visit_Sarah", false },
            { "HarborHome_Visit_Anis", false },
            { "HarborHome_Visit_Centi", false },
            { "HarborHome_Visit_Dorothy", false },
            { "HarborHome_Visit_Elegg", false },
            { "HarborHome_Visit_Frima", false },
            { "HarborHome_Visit_Guilty", false },
            { "HarborHome_Visit_Helm", false },
            { "HarborHome_Visit_Maiden", false },
            { "HarborHome_Visit_Mary", false },
            { "HarborHome_Visit_Mast", false },
            { "HarborHome_Visit_Neon", false },
            { "HarborHome_Visit_Pepper", false },
            { "HarborHome_Visit_Rapi", false },
            { "HarborHome_Visit_Rosanna", false },
            { "HarborHome_Visit_Sakura", false },
            { "HarborHome_Visit_Tove", false },
            { "HarborHome_Visit_Viper", false },
            { "HarborHome_Visit_Yan", false },

            // Mountain Lab variables
            { "MountainLab_FirstVisited", false },
            { "MountainLab_FirstVisitor", false },
            { "MountainLab_GKExplanation", false },

            // Secret Beach variables
            { "SecretBeach_FirstVisited", false },
            { "SecretBeach_GKSeen", false },
            { "SecretBeach_RelaxedAmount", 0 },
            { "SecretBeach_UnlockedLab", false },
            
            
            // Affection variables
            { "Affection_Amber", 0 },
            { "Affection_Claire", 0 },
            { "Affection_Sarah", 0 },
            { "Affection_Anis", 0 },
            { "Affection_Anis_Seen1", false },
            { "Affection_Anis_Seen2", false },
            { "Affection_Anis_Seen3", false },
            { "Affection_Centi", 0 },
            { "Affection_Dorothy", 0 },
            { "Affection_Elegg", 0 },
            { "Affection_Frima", 0 },
            { "Affection_Guilty", 0 },
            { "Affection_Helm", 0 },
            { "Affection_Maiden", 0 },
            { "Affection_Mary", 0 },
            { "Affection_Mast", 0 },
            { "Affection_Neon", 0 },
            { "Affection_Pepper", 0 },
            { "Affection_Rapi", 0 },
            { "Affection_Rosanna", 0 },
            { "Affection_Sakura", 0 },
            { "Affection_Tove", 0 },
            { "Affection_Viper", 0 },
            { "Affection_Yan", 0 },

            // Event variables
            { "Event_SeenAmberHospitalHallway01", false },
            { "Event_SeenAnisMall01", false },
            { "Event_SeenCentiKensHome01", false },
            { "Event_SeenDorothyPark01", false },
            { "Event_SeenEleggDowntown01", false },
            { "Event_SeenFrimaHotel01", false },
            { "Event_SeenGuiltyParkingLot01", false },
            { "Event_SeenHelmBeach01", false },
            { "Event_SeenMaidenAlley01", false },
            { "Event_SeenMaryHospitalHallway01", false },
            { "Event_SeenMastBeach01", false },
            { "Event_SeenNeonTemple01", false },
            { "Event_SeenPepperHospital01", false },
            { "Event_SeenRapiGasStation01", false },
            { "Event_SeenRosannaGabrielsMansion01", false },
            { "Event_SeenToveTrail01", false },
            { "Event_SeenSakuraForest01", false },
            { "Event_SeenViperVilla01", false },
            { "Event_SeenYanMall01", false },

            { "Event_SeenIt01", false },

            // Voyeur variables
            { "Voyeur_SeenAnis", false },
            { "Voyeur_SeenCenti", false },
            { "Voyeur_SeenDorothy", false },
            { "Voyeur_SeenElegg", false },
            { "Voyeur_SeenFrima", false },
            { "Voyeur_SeenGuilty", false },
            { "Voyeur_SeenHelm", false },
            { "Voyeur_SeenMaiden", false },
            { "Voyeur_SeenMary", false },
            { "Voyeur_SeenMast", false },
            { "Voyeur_SeenNeon", false },
            { "Voyeur_SeenPepper", false },
            { "Voyeur_SeenRapi", false },
            { "Voyeur_SeenRosanna", false },
            { "Voyeur_SeenSakura", false },
            { "Voyeur_SeenTove", false },
            { "Voyeur_SeenViper", false },
            { "Voyeur_SeenYan", false },
            
            // Item variables
            { "Gift_Action-Figure", false },
            { "Gift_Bikini", false },
            { "Gift_Bonsai-Tree", false },
            { "Gift_Parasol", false },
            { "Gift_Ring", false },
            { "Gift_Shark-Tooth-Necklace", false },
            { "Gift_Sunglasses", false },
            { "Gift_Sunscreen", false },
            { "Gift_Tropical-Flower-Bouquet", false },

            // General mod variables
            { "Wallpaper_Current", -1},
            { "Mod_Version", Core.pluginVersion }
        };

        public void Awake()
        {
            instance = this;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        public void OnSceneLoaded(Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
        {
            MainStory.relaxed = false;
            modSaveThisSession = false;
            MainStory.actionTodaySB = false;
            if (scene.name == "CoreGameScene")
            {
                Schedule.day = Core.GetVariableNumber("Day");
                MainStory.generalLotteryNumber1 = Core.GetRandomNumber(100);
                MainStory.generalLotteryNumber2 = Core.GetRandomNumber(100);
                MainStory.generalLotteryNumber3 = Core.GetRandomNumber(100);
                MainStory.voyeurLotteryNumber = Core.GetRandomNumber(100);
                Debug.Log("General Lottery Number 1: " + MainStory.generalLotteryNumber1);
                Debug.Log("General Lottery Number 2: " + MainStory.generalLotteryNumber2);
                Debug.Log("General Lottery Number 3: " + MainStory.generalLotteryNumber3);
                Debug.Log("Voyeur Lottery Number: " + MainStory.voyeurLotteryNumber);
                //Debug.Log("Day: " + Schedule.day);
                Core.RefreshDailyProxyVariables();
                Invoke(nameof(UpdateScheduleInvoke), 1.0f);
            }
            StartCoroutine(WaitAndLoadSaveFile());
        }

        private IEnumerator WaitAndLoadSaveFile()
        {
            // Wait until SaveLoadManager is initialized and SlotLoaded is valid
            while (Core.saveLoadManager == null || Core.saveLoadManager.SlotLoaded <= 0)
            {
                yield return null; // Wait one frame
            }
            currentSaveSlot = Core.saveLoadManager.SlotLoaded;
            LoadSaveFile();
            if (GetString("HarborHome_Outfit_Anis") == null || GetString("HarborHome_Outfit_Anis") == "") { SetString("HarborHome_Outfit_Anis", "Default"); }

            // Update Gift Shop texture after save variables are loaded
            Places.UpdateGiftShopTextureBasedOnBuildStatus();
        }

        public void Update()
        {
            if (Core.currentScene.name == "CoreGameScene")
            {
                if (Schedule.loadedSchedule)
                {
                    if (Core.afterSleepEvents.activeSelf && !afterSleepEventsProc)
                    {
                        afterSleepEventsProc = true;
                    }
                    if (Core.savedUI.activeSelf && afterSleepEventsProc)
                    {
                        //RefreshDailyVariables();
                        Core.RefreshDailyProxyVariables();
                        if (GetBool("GiftShop_FirstVisited") && GetInt("GiftShop_BuildCounter") < 2) { SetInt("GiftShop_BuildCounter", GetInt("GiftShop_BuildCounter") + 1); }
                        SetBool("HarborHome_Slept", Places.harborHomeBedroomLevel.activeSelf);
                        SaveToFile(1);  // Autosave to slot 1
                        Schedule.day = Core.GetVariableNumber("Day");
                        Debug.Log("Day: " + Schedule.day);

                        if (Schedule.day == 1)
                        {
                            SaveToFile(2);  // Additional Monday backup to slot 2
                            Debug.Log("[SaveManager] Monday detected - autosaved to both slot 1 and slot 2");
                        }

                        autosaveProcedThisSession = true;  // Mark that autosave happened
                        MainStory.voyeurTargetsLeft.Clear();
                        string[] currentTargets;
                        // Progression gating: unlock new tiers only after specific conditions
                        if (SaveManager.GetInt("GiftShop_BuildCounter") >= 2)
                        {
                            // Player has built the gift shop, unlock full voyeur targets
                            currentTargets = MainStory.fullVoyeurTargets;
                        }
                        else if (SaveManager.GetBool("MountainLab_GKExplanation"))
                        {
                            // Player has found all starter targets AND received GK explanation, unlock gift shop targets
                            currentTargets = MainStory.gSVoyeurTargets;
                        }
                        else
                        {
                            // Default to starter targets
                            currentTargets = MainStory.starterVoyeurTargets;
                        }
                        foreach (string character in currentTargets)
                        {
                            if (!SaveManager.GetBool($"Voyeur_Seen{character}"))
                            {
                                MainStory.voyeurTargetsLeft.Add(character);
                            }
                        }
                        MainStory.relaxed = false;
                        MainStory.actionTodaySB = false;
                        Places.UpdateGiftShopTextureBasedOnBuildStatus();
                        MainStory.generalLotteryNumber1 = Core.GetRandomNumber(100);
                        MainStory.generalLotteryNumber2 = Core.GetRandomNumber(100);
                        MainStory.generalLotteryNumber3 = Core.GetRandomNumber(100);
                        MainStory.voyeurLotteryNumber = Core.GetRandomNumber(100);
                        Debug.Log("General Lottery Number 1: " + MainStory.generalLotteryNumber1);
                        Debug.Log("General Lottery Number 2: " + MainStory.generalLotteryNumber2);
                        Debug.Log("General Lottery Number 3: " + MainStory.generalLotteryNumber3);
                        Debug.Log("Voyeur Lottery Number: " + MainStory.voyeurLotteryNumber);
                        Invoke(nameof(UpdateScheduleInvoke), 1.0f);
                        afterSleepEventsProc = false;
                    }

                    // Find NanoSave (root-level GameObject that becomes active when save menu opens)
                    Transform nanoSaveTransform = Core.currentScene.GetRootGameObjects()
                        .FirstOrDefault(go => go.name == "NanoSave")?
                        .transform;

                    // Attach listeners if NanoSave just became active
                    bool nanoSaveIsActive = nanoSaveTransform != null && nanoSaveTransform.gameObject.activeSelf;
                    bool shouldAttach = nanoSaveIsActive && !saveMenuListenersAttached;
                    
                    if (shouldAttach)
                    {
                        if (!saveMenuListenersAttached)
                        {
                            AttachSaveSlotListenersCoreGameScene(nanoSaveTransform);
                            saveMenuListenersAttached = true;
                        }
                    }
                    
                    // Reset flags when NanoSave is deactivated
                    if (!nanoSaveIsActive && saveMenuListenersAttached)
                    {
                        saveMenuListenersAttached = false;
                    }

                    if (Core.introMomentNewGame.activeSelf && !newGame)
                    {
                        ResetToDefaults();
                        newGame = true;
                    }
                }
            }

            if (Core.currentScene.name == "GameStart")
            {
                if (newGame)
                {
                    newGame = false;
                }
            }
        }

        #region Public Methods

        /// Sets a string value for the current save slot
        public static void SetString(string variableName, string value)
        {
            if (instance == null) return;
            instance.SetValueInternal(variableName, value);
        }
        /// Gets a string value for the current save slot
        public static string GetString(string variableName, string defaultValue = "")
        {
            if (instance == null) return defaultValue;
            return instance.GetValueInternal(variableName, defaultValue);
        }
        /// Sets an integer value for the current save slot
        public static void SetInt(string variableName, int value)
        {
            if (instance == null) return;
            // Clamp affection variables to a maximum of 5
            if (variableName.StartsWith("Affection_"))
            {
                value = Mathf.Clamp(value, 0, 5);
            }
            instance.SetValueInternal(variableName, value);
        }
        /// Gets an integer value for the current save slot
        public static int GetInt(string variableName, int defaultValue = 0)
        {
            if (instance == null) return defaultValue;
            return instance.GetValueInternal(variableName, defaultValue);
        }
        /// Sets a float value for the current save slot
        public static void SetFloat(string variableName, float value)
        {
            if (instance == null) return;
            instance.SetValueInternal(variableName, value);
        }
        /// Gets a float value for the current save slot
        public static float GetFloat(string variableName, float defaultValue = 0f)
        {
            if (instance == null) return defaultValue;
            return instance.GetValueInternal(variableName, defaultValue);
        }
        /// Sets a boolean value for the current save slot
        public static void SetBool(string variableName, bool value)
        {
            if (instance == null) return;
            instance.SetValueInternal(variableName, value);
        }
        /// Gets a boolean value for the current save slot
        public static bool GetBool(string variableName, bool defaultValue = false)
        {
            if (instance == null) return defaultValue;
            return instance.GetValueInternal(variableName, defaultValue);
        }
        /// Deletes a variable for the current save slot
        public static void DeleteVariable(string variableName)
        {
            if (instance == null) return;
            instance.DeleteVariableInternal(variableName);
        }
        /// Checks if a variable exists for the current save slot
        public static bool HasVariable(string variableName)
        {
            if (instance == null) return false;
            return instance.HasVariableInternal(variableName);
        }
        /// Gets all variables for the current save slot
        public static Dictionary<string, object> GetAllVariables()
        {
            if (instance == null) return new Dictionary<string, object>();
            return new Dictionary<string, object>(instance.currentSlotCache);
        }
        /// Clears all variables for the current save slot
        public static void ClearAllVariables()
        {
            if (instance == null) return;
            instance.ClearAllVariablesInternal();
        }
        /// Resets all variables to their default values for the current save slot
        public static void ResetToDefaults()
        {
            if (instance == null) return;
            instance.ResetToDefaultsInternal();
        }

        /// <summary>
        /// Refreshes all daily bool variables by setting them to false.
        /// This resets any SaveManager variables that end with "_Daily".
        /// </summary>
        public static void RefreshDailyVariables()
        {
            if (instance == null) return;
            
            var keysToReset = new List<string>();
            foreach (var kvp in instance.currentSlotCache)
            {
                if (kvp.Key.EndsWith("_Daily") && kvp.Value is bool)
                {
                    keysToReset.Add(kvp.Key);
                }
            }

            foreach (var key in keysToReset)
            {
                instance.SetValueInternal(key, false);
                Debug.Log($"[SaveManager] Reset daily variable: {key}");
            }

            Debug.Log($"[SaveManager] Refreshed {keysToReset.Count} daily variables");
        }

        /// <summary>
        /// Returns true if any bool save variable whose name contains the given substring is true.
        /// </summary>
        public static bool AnyBoolVariableWithNameContains(string substring)
        {
            if (instance == null || string.IsNullOrEmpty(substring)) return false;
            string substringLower = substring.ToLower();
            foreach (var kvp in instance.currentSlotCache)
            {
                if (kvp.Key.ToLower().Contains(substringLower) && kvp.Value is bool b && b)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Returns the count of bool save variables whose name contains the given substring and are true.
        /// </summary>
        public static int CountBoolVariablesWithNameContains(string substring)
        {
            if (instance == null || string.IsNullOrEmpty(substring)) return 0;
            string substringLower = substring.ToLower();
            int count = 0;
            foreach (var kvp in instance.currentSlotCache)
            {
                if (kvp.Key.ToLower().Contains(substringLower) && kvp.Value is bool b && b)
                {
                    count++;
                }
            }
            return count;
        }

        /// <summary>
        /// Dynamically discover and attach listeners to all save slot buttons in CoreGameScene.
        /// Navigates through NanoSave UI hierarchy to find SaveSlots(Clone) GOs.
        /// Also attaches listener to EmptySaveSlot(Clone) for creating new saves.
        /// </summary>
        private void AttachSaveSlotListenersCoreGameScene(Transform nanoSaveTransform)
        {
            if (nanoSaveTransform == null)
            {
                Debug.LogError("[SaveManager] nanoSaveTransform is null, cannot attach listeners");
                return;
            }

            if (!nanoSaveTransform.gameObject.activeSelf)
            {
                Debug.LogWarning("[SaveManager] NanoSave is not active");
                return;
            }

            Debug.Log("[SaveManager] NanoSave is active, attempting to navigate hierarchy");

            // Navigate to the Content GO containing SaveSlots(Clone) GOs
            // Path: NanoSave > Content > Content > List > Viewport > Content
            Transform contentLevel1 = nanoSaveTransform.Find("Content");
            if (contentLevel1 == null)
            {
                Debug.LogError("[SaveManager] Could not find Content (level 1) in NanoSave");
                return;
            }

            Transform contentLevel2 = contentLevel1.Find("Content");
            if (contentLevel2 == null)
            {
                Debug.LogError("[SaveManager] Could not find Content (level 2) in NanoSave");
                return;
            }

            Transform listTransform = contentLevel2.Find("List");
            if (listTransform == null)
            {
                Debug.LogError("[SaveManager] Could not find List in NanoSave hierarchy");
                return;
            }

            Transform viewportTransform = listTransform.Find("Viewport");
            if (viewportTransform == null)
            {
                Debug.LogError("[SaveManager] Could not find Viewport in NanoSave hierarchy");
                return;
            }

            Transform contentParent = viewportTransform.Find("Content");
            if (contentParent == null)
            {
                Debug.LogError("[SaveManager] Could not find Content (parent) in NanoSave hierarchy");
                return;
            }

            Debug.Log($"[SaveManager] Found content parent with {contentParent.childCount} children");

            // Iterate through all children to find EmptySaveSlot(Clone) and SaveSlots(Clone) GOs
            bool emptySlotFound = false;
            int saveSlotCount = 0;
            for (int i = 0; i < contentParent.childCount; i++)
            {
                Transform child = contentParent.GetChild(i);
                //Debug.Log($"[SaveManager] Child {i}: {child.name}");
                
                // Look for EmptySaveSlot(Clone) - attach save button listener
                if (child.name.StartsWith("EmptySaveSlot"))
                {
                    Debug.Log($"[SaveManager] Found EmptySaveSlot at index {i}, attaching listener");
                    AttachEmptySaveSlotListener(child);
                    emptySlotFound = true;
                }
                
                // Look for SaveSlots(Clone) - attach overwrite button listener
                if (child.name.StartsWith("SaveSlots"))
                {
                    Debug.Log($"[SaveManager] Found SaveSlots at index {i}, attaching overwrite listener");
                    AttachOverwriteSaveSlotListener(child);
                    saveSlotCount++;
                }
            }

            if (!emptySlotFound)
            {
                Debug.LogWarning("[SaveManager] EmptySaveSlot(Clone) not found in content parent");
            }

            Debug.Log($"[SaveManager] Attached listeners to save UI buttons in CoreGameScene (EmptySlot: {emptySlotFound}, SaveSlots: {saveSlotCount})");
        }


        /// <summary>
        /// Attach listener to EmptySaveSlot(Clone) GO to save to the next available slot.
        /// </summary>
        private void AttachEmptySaveSlotListener(Transform emptySlotTransform)
        {
            Debug.Log($"[SaveManager] AttachEmptySaveSlotListener called for {emptySlotTransform.name}");
            
            // Check if listener is already attached to prevent stacking
            if (emptySlotTransform.GetComponent<SaveMenuEmptySaveSlotMarker>() != null)
            {
                Debug.Log($"[SaveManager] Marker already exists on {emptySlotTransform.name}, listener already attached, skipping to prevent stacking");
                return;
            }

            Debug.Log($"[SaveManager] Searching for ButtonInstructions on {emptySlotTransform.name}...");
            
            ButtonInstructions saveButton = emptySlotTransform.GetComponent<ButtonInstructions>();
            if (saveButton != null)
            {
                Debug.Log($"[SaveManager] ✓ Found ButtonInstructions directly on {emptySlotTransform.name}");
            }
            else
            {
                Debug.LogWarning($"[SaveManager] ✗ ButtonInstructions NOT found directly on {emptySlotTransform.name}, searching in children...");
                
                // Try to find it in children
                ButtonInstructions[] childButtons = emptySlotTransform.GetComponentsInChildren<ButtonInstructions>();
                Debug.Log($"[SaveManager] GetComponentsInChildren returned {childButtons.Length} ButtonInstructions components");
                
                if (childButtons.Length > 0)
                {
                    saveButton = childButtons[0];
                    Debug.Log($"[SaveManager] ✓ Using ButtonInstructions from child: {saveButton.gameObject.name}");
                }
                else
                {
                    Debug.LogError($"[SaveManager] ✗ ERROR: Could not find ButtonInstructions anywhere on {emptySlotTransform.name} or its children!");
                    
                    // Debug: Log the hierarchy
                    Debug.Log($"[SaveManager] EmptySaveSlot hierarchy:");
                    foreach (Transform child in emptySlotTransform.GetComponentsInChildren<Transform>())
                    {
                        Debug.Log($"[SaveManager]   - {child.name} (components: {string.Join(", ", child.GetComponents<UnityEngine.Component>().Select(c => c.GetType().Name).Take(5))})");
                    }
                    return;
                }
            }

            // Attach listener to save to the next available slot
            Debug.Log($"[SaveManager] Attaching onClick listener to ButtonInstructions on {saveButton.gameObject.name}");
            saveButton.onClick.AddListener(() =>
            {
                Debug.Log("[SaveManager] ► EmptySaveSlot button clicked! Saving to next available slot...");
                SaveToNextAvailableSlot();
            });
            
            // Mark this transform as having a listener attached
            emptySlotTransform.gameObject.AddComponent<SaveMenuEmptySaveSlotMarker>();
            Debug.Log($"[SaveManager] Successfully attached listener to {emptySlotTransform.name}");

            Debug.Log("[SaveManager] Successfully attached save listener to EmptySaveSlot(Clone)");
        }

        /// <summary>
        /// Detect the most recently created/modified save folder and save mod data to it.
        /// This syncs with the vanilla game's save by waiting for it to complete.
        /// Only proceeds if 5_Levels > 5_MyRoom is active (player is in the correct location).
        /// Uses Invoke to avoid blocking the game flow.
        /// </summary>
        private void SaveToNextAvailableSlot()
        {
            // Check if 5_Levels > 5_MyRoom is active before saving
            GameObject levelsGO = GameObject.Find("5_Levels");
            if (levelsGO == null)
            {
                Debug.LogWarning("[SaveManager] 5_Levels not found, cannot save");
                return;
            }

            Transform myRoomTransform = levelsGO.transform.Find("5_MyRoom");
            if (myRoomTransform == null || !myRoomTransform.gameObject.activeSelf)
            {
                Debug.LogWarning("[SaveManager] 5_Levels > 5_MyRoom is not active, saving is not allowed from this location");
                return;
            }

            Debug.Log("[SaveManager] SaveToNextAvailableSlot() called, using Invoke to wait for game save");
            // Cancel any pending invokes to allow multiple clicks
            CancelInvoke(nameof(SaveToLatestSlot));
            // Wait 0.2 seconds for the vanilla game save to complete, then save our data
            Invoke(nameof(SaveToLatestSlot), 0.2f);
        }

        /// <summary>
        /// Find the most recently modified NANOSAVE_* folder and save mod data to it.
        /// Called via Invoke after button click to ensure game's save is complete.
        /// </summary>
        private void SaveToLatestSlot()
        {
            Debug.Log("[SaveManager] SaveToLatestSlot() invoked");
            
            try
            {
                // Get the base saves directory
                string localLowPath = Path.Combine(
                    Path.GetDirectoryName(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)),
                    "LocalLow",
                    "Arvus Games",
                    "Starmaker Story",
                    "Saves"
                );

                if (!Directory.Exists(localLowPath))
                {
                    Debug.LogError("[SaveManager] Saves directory does not exist");
                    return;
                }

                // Find all NANOSAVE_XXXX folders
                string[] slotFolders = Directory.GetDirectories(localLowPath, "NANOSAVE_*");
                if (slotFolders.Length == 0)
                {
                    Debug.LogError("[SaveManager] No save folders found");
                    return;
                }

                // Find the most recently modified folder
                string latestFolder = slotFolders
                    .OrderByDescending(folder => Directory.GetLastWriteTime(folder))
                    .FirstOrDefault();

                if (latestFolder == null)
                {
                    Debug.LogError("[SaveManager] Could not find latest save folder");
                    return;
                }

                // Extract the slot number from the folder name
                string folderName = Path.GetFileName(latestFolder);
                if (!folderName.StartsWith("NANOSAVE_") || !int.TryParse(folderName.Substring("NANOSAVE_".Length), out int latestSlot))
                {
                    Debug.LogError($"[SaveManager] Could not parse slot number from folder: {folderName}");
                    return;
                }

                // Determine which slot to copy from:
                // - If on slot -1 (new game): copy from auto-save (slot 1)
                // - If auto-save was triggered during this gameplay session: copy from auto-save (slot 1)
                // - Otherwise: copy from current slot
                int sourceSlot;
                string copyReason;
                
                if (currentSaveSlot == -1)
                {
                    sourceSlot = 1;
                    copyReason = "new game (slot -1)";
                }
                else if (autosaveProcedThisSession)
                {
                    sourceSlot = 1;
                    copyReason = "auto-save triggered during gameplay";
                }
                else
                {
                    sourceSlot = currentSaveSlot;
                    copyReason = $"current slot ({currentSaveSlot})";
                }

                string sourceFile = GetSaveFilePath(sourceSlot);
                string destFile = GetSaveFilePath(latestSlot);
                
                if (!File.Exists(sourceFile))
                {
                    Debug.LogError($"[SaveManager] Source file does not exist: {sourceFile}");
                    return;
                }
                
                try
                {
                    File.Copy(sourceFile, destFile, true);
                    Debug.Log($"[SaveManager] Copied slot {sourceSlot} ({copyReason}) to slot {latestSlot}");
                }
                catch (Exception copyEx)
                {
                    Debug.LogError($"[SaveManager] Error copying slot {sourceSlot} to slot {latestSlot}: {copyEx.Message}");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveManager] Error finding latest save slot: {e.Message}\n{e.StackTrace}");
            }
        }

        /// <summary>
        /// Attach listener to a SaveSlots(Clone) GO's overwrite button.
        /// Parses the slot number from the "Save XXXX" label text at SaveSlots(Clone) > Right > Save 0001,
        /// then attaches a click listener to the overwrite button at SaveSlots(Clone) > Image (1) > Button (1).
        /// </summary>
        private void AttachOverwriteSaveSlotListener(Transform saveSlotTransform)
        {
            Debug.Log($"[SaveManager] AttachOverwriteSaveSlotListener called for {saveSlotTransform.name}");

            // Check if listener is already attached to prevent stacking
            if (saveSlotTransform.GetComponent<SaveMenuOverwriteSlotMarker>() != null)
            {
                Debug.Log($"[SaveManager] Overwrite marker already exists on {saveSlotTransform.name}, skipping");
                return;
            }

            // Parse the slot number from SaveSlots(Clone) > Right > Save 0001's TextMeshProUGUI text
            Transform rightTransform = saveSlotTransform.Find("Right");
            if (rightTransform == null)
            {
                Debug.LogError($"[SaveManager] Could not find 'Right' in {saveSlotTransform.name}");
                return;
            }

            Transform saveLabelTransform = rightTransform.Find("Save 0001");
            if (saveLabelTransform == null)
            {
                Debug.LogError($"[SaveManager] Could not find 'Save 0001' in {saveSlotTransform.name} > Right");
                return;
            }

            TextMeshProUGUI saveLabelTMP = saveLabelTransform.GetComponent<TextMeshProUGUI>();
            if (saveLabelTMP == null)
            {
                Debug.LogError($"[SaveManager] Could not find TextMeshProUGUI on {saveLabelTransform.name}");
                return;
            }

            // Text format is "Save XXXX" where XXXX is the slot number (e.g. "Save 104" -> NANOSAVE_0104)
            string labelText = saveLabelTMP.text;
            Debug.Log($"[SaveManager] Save label text: '{labelText}'");

            if (!labelText.StartsWith("Save ") || !int.TryParse(labelText.Substring("Save ".Length).Trim(), out int targetSlot))
            {
                Debug.LogError($"[SaveManager] Could not parse slot number from label: '{labelText}'");
                return;
            }

            Debug.Log($"[SaveManager] Parsed target slot: {targetSlot} (NANOSAVE_{targetSlot:D4})");

            // Find the overwrite button at SaveSlots(Clone) > Image (1) > Button (1)
            Transform imageTransform = saveSlotTransform.Find("Image (1)");
            if (imageTransform == null)
            {
                Debug.LogError($"[SaveManager] Could not find 'Image (1)' in {saveSlotTransform.name}");
                return;
            }

            Transform buttonTransform = imageTransform.Find("Button (1)");
            if (buttonTransform == null)
            {
                Debug.LogError($"[SaveManager] Could not find 'Button (1)' in {saveSlotTransform.name} > Image (1)");
                return;
            }

            ButtonInstructions overwriteButton = buttonTransform.GetComponent<ButtonInstructions>();
            if (overwriteButton == null)
            {
                Debug.LogError($"[SaveManager] Could not find ButtonInstructions on {buttonTransform.name}");
                return;
            }

            // Attach listener
            overwriteButton.onClick.AddListener(() =>
            {
                Debug.Log($"[SaveManager] ► Overwrite button clicked for slot {targetSlot}!");
                OverwriteSaveSlot(targetSlot);
            });

            // Mark as having a listener attached
            saveSlotTransform.gameObject.AddComponent<SaveMenuOverwriteSlotMarker>();
            Debug.Log($"[SaveManager] Successfully attached overwrite listener to {saveSlotTransform.name} for slot {targetSlot}");
        }

        /// <summary>
        /// Overwrite the mod save file for the specified slot.
        /// Uses the same source-slot logic as SaveToLatestSlot:
        /// - If new game (slot -1): copy from auto-save (slot 1)
        /// - If auto-save was triggered this session: copy from auto-save (slot 1)
        /// - Otherwise: copy from current slot
        /// If no SMSAndroidsCore_Save file exists in the target folder, creates a new one.
        /// Delayed via Invoke to allow the vanilla game save to complete first.
        /// </summary>
        private void OverwriteSaveSlot(int targetSlot)
        {
            // Check if 5_Levels > 5_MyRoom is active before saving
            GameObject levelsGO = GameObject.Find("5_Levels");
            if (levelsGO == null)
            {
                Debug.LogWarning("[SaveManager] 5_Levels not found, cannot overwrite");
                return;
            }

            Transform myRoomTransform = levelsGO.transform.Find("5_MyRoom");
            if (myRoomTransform == null || !myRoomTransform.gameObject.activeSelf)
            {
                Debug.LogWarning("[SaveManager] 5_Levels > 5_MyRoom is not active, overwriting is not allowed from this location");
                return;
            }

            Debug.Log($"[SaveManager] OverwriteSaveSlot({targetSlot}) called, using Invoke to wait for game save");
            // Store the target slot for the delayed method
            pendingOverwriteSlot = targetSlot;
            // Cancel any pending invokes to allow multiple clicks
            CancelInvoke(nameof(PerformOverwriteSaveSlot));
            // Wait 0.2 seconds for the vanilla game save to complete
            Invoke(nameof(PerformOverwriteSaveSlot), 0.2f);
        }

        /// <summary>
        /// Performs the actual overwrite of the mod save file for the pending target slot.
        /// Called via Invoke after the overwrite button click.
        /// </summary>
        private void PerformOverwriteSaveSlot()
        {
            int targetSlot = pendingOverwriteSlot;
            Debug.Log($"[SaveManager] PerformOverwriteSaveSlot() invoked for slot {targetSlot}");

            if (targetSlot < 1)
            {
                Debug.LogError($"[SaveManager] Invalid overwrite target slot: {targetSlot}");
                return;
            }

            try
            {
                // Determine which slot to copy from (same logic as SaveToLatestSlot)
                int sourceSlot;
                string copyReason;

                if (currentSaveSlot == -1)
                {
                    sourceSlot = 1;
                    copyReason = "new game (slot -1)";
                }
                else if (autosaveProcedThisSession)
                {
                    sourceSlot = 1;
                    copyReason = "auto-save triggered during gameplay";
                }
                else
                {
                    sourceSlot = currentSaveSlot;
                    copyReason = $"current slot ({currentSaveSlot})";
                }

                string sourceFile = GetSaveFilePath(sourceSlot);
                string destFile = GetSaveFilePath(targetSlot);

                // Ensure the target directory exists
                if (!EnsureSaveDirectoryExists(targetSlot))
                {
                    Debug.LogError($"[SaveManager] Failed to create save directory for overwrite target slot {targetSlot}");
                    return;
                }

                if (File.Exists(sourceFile))
                {
                    // Source file exists, copy it to the target slot (overwrite or create)
                    try
                    {
                        File.Copy(sourceFile, destFile, true);
                        Debug.Log($"[SaveManager] Overwrote slot {targetSlot} with data from slot {sourceSlot} ({copyReason})");
                    }
                    catch (Exception copyEx)
                    {
                        Debug.LogError($"[SaveManager] Error copying slot {sourceSlot} to slot {targetSlot}: {copyEx.Message}");
                    }
                }
                else
                {
                    // No source file exists — save current cache directly to the target slot
                    Debug.Log($"[SaveManager] Source file not found at slot {sourceSlot}, saving current cache to slot {targetSlot}");
                    SaveToFile(targetSlot);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveManager] Error overwriting save slot {targetSlot}: {e.Message}\n{e.StackTrace}");
            }
        }

        #endregion

        #region Internal Methods
        private void SetValueInternal(string variableName, object value)
        {
            if (currentSaveSlot < 0) return;

            // Update cache
            currentSlotCache[variableName] = value;
            Debug.Log($"[SaveManager] Set runtime variable {variableName} = {value} for save slot {currentSaveSlot}");
        }
        private T GetValueInternal<T>(string variableName, T defaultValue)
        {
            if (currentSaveSlot < 0) return defaultValue;

            // Check cache first
            if (currentSlotCache.TryGetValue(variableName, out object cachedValue))
            {
                if (cachedValue is T typedValue)
                {
                    return typedValue;
                }
            }

            // If not in cache, return default value
            return defaultValue;
        }
        private void DeleteVariableInternal(string variableName)
        {
            if (currentSaveSlot < 0) return;
            currentSlotCache.Remove(variableName);
            Debug.Log($"[SaveManager] Deleted runtime variable {variableName} for save slot {currentSaveSlot}");
        }
        private bool HasVariableInternal(string variableName)
        {
            if (currentSaveSlot < 0) return false;
            return currentSlotCache.ContainsKey(variableName);
        }
        private void ClearAllVariablesInternal()
        {
            if (currentSaveSlot < 0) return;
            currentSlotCache.Clear();
            Debug.Log($"[SaveManager] Cleared all runtime variables for save slot {currentSaveSlot}");
        }
        private void ResetToDefaultsInternal()
        {
            if (currentSaveSlot < 0) return;
            Debug.Log($"[SaveManager] ResetToDefaultsInternal called for slot {currentSaveSlot}");
            Debug.Log($"[SaveManager] defaultValues count: {defaultValues.Count}");
            currentSlotCache.Clear();

            // Add all default values
            foreach (var kvp in defaultValues)
            {
                Debug.Log($"[SaveManager] Adding default: {kvp.Key} = {kvp.Value}");
                currentSlotCache[kvp.Key] = kvp.Value;
            }
            // We still want to save here to create the initial file
            SaveToFile(currentSaveSlot);
            modSaveThisSession = false;
            Debug.Log($"[SaveManager] Reset all variables to defaults for save slot {currentSaveSlot}");
        }
        private void LoadSaveFile()
        {
            currentSaveFilePath = GetSaveFilePath(currentSaveSlot);
            currentSlotCache.Clear();
            try
            {
                if (File.Exists(currentSaveFilePath))
                {
                    foreach (var line in File.ReadAllLines(currentSaveFilePath))
                    {
                        if (string.IsNullOrWhiteSpace(line) || !line.Contains("=")) continue;
                        var parts = line.Split(new[] { '=' }, 2);
                        var key = parts[0];
                        var value = parts[1];
                        if (key == "saveSlot" || key == "lastSaved") continue;
                        // Try to parse value type based on defaultValues
                        if (defaultValues.TryGetValue(key, out object defaultVal))
                        {
                            if (defaultVal is int && int.TryParse(value, out int intVal))
                                currentSlotCache[key] = intVal;
                            else if (defaultVal is bool && bool.TryParse(value, out bool boolVal))
                                currentSlotCache[key] = boolVal;
                            else if (defaultVal is float && float.TryParse(value, out float floatVal))
                                currentSlotCache[key] = floatVal;
                            else
                                currentSlotCache[key] = value;
                        }
                        else
                        {
                            currentSlotCache[key] = value;
                        }
                    }
                    Debug.Log($"[SaveManager] Loaded save file: {currentSaveFilePath}");
                }
                else
                {
                    // Create new save file with default values
                    ResetToDefaultsInternal();
                    // SaveToFile is already called by ResetToDefaultsInternal
                    Debug.Log($"[SaveManager] Created new save file with defaults: {currentSaveFilePath}");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveManager] Error loading save file: {e.Message}");
                // Fallback to defaults
                ResetToDefaultsInternal();
            }
        }

        private void SaveToFile(int? slotOverride = null)
        {
            int slot = slotOverride ?? currentSaveSlot;
            if (slot < 1)
            {
                Debug.LogError("[SaveManager] Invalid save slot: " + slot);
                return;
            }

            // Ensure save directory exists
            if (!EnsureSaveDirectoryExists(slot))
            {
                Debug.LogError($"[SaveManager] Failed to create save directory for slot {slot}");
                return;
            }

            string saveFilePath = GetSaveFilePath(slot);

            try
            {
                using (var writer = new StreamWriter(saveFilePath))
                {
                    writer.WriteLine($"saveSlot={slot}");
                    writer.WriteLine($"lastSaved={DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                    foreach (var kvp in currentSlotCache)
                    {
                        writer.WriteLine($"{kvp.Key}={kvp.Value}");
                    }
                }
                Debug.Log($"[SaveManager] Saved to slot {slot} at {saveFilePath}");
                modSaveThisSession = true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveManager] Error saving to slot {slot}: {e.Message}");
            }
        }

        /// <summary>
        /// Gets the full path to a save file in the game's save directory.
        /// Path: AppData\LocalLow\Arvus Games\Starmaker Story\Saves\NANOSAVE_XXXX\SMSAndroidsCore_Save.txt
        /// Where XXXX is the slot number formatted with leading zeros.
        /// </summary>
        private string GetSaveFilePath(int slot)
        {
            if (slot < 1)
            {
                Debug.LogError("[SaveManager] Invalid slot number for save path: " + slot);
                return "";
            }

            // Get the LocalLow path (not LocalApplicationData which is Local)
            string appDataPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData);
            string localLowPath = Path.Combine(Path.GetDirectoryName(appDataPath), "LocalLow");
            string gameSavePath = Path.Combine(localLowPath, "Arvus Games", "Starmaker Story", "Saves");
            
            // Format slot number with leading zeros (e.g., slot 1 -> NANOSAVE_0001)
            string slotFolder = $"NANOSAVE_{slot:D4}";
            string fullPath = Path.Combine(gameSavePath, slotFolder, "SMSAndroidsCore_Save.txt");
            
            return fullPath;
        }

        /// <summary>
        /// Ensures the directory for a save slot exists.
        /// </summary>
        private bool EnsureSaveDirectoryExists(int slot)
        {
            try
            {
                string saveFilePath = GetSaveFilePath(slot);
                string directory = Path.GetDirectoryName(saveFilePath);
                
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                    Debug.Log($"[SaveManager] Created save directory: {directory}");
                }
                
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveManager] Error ensuring save directory for slot {slot}: {e.Message}");
                return false;
            }
        }

        #endregion

        private void UpdateScheduleInvoke()
        {
            Schedule.UpdateScheduleForDay();
        }
    }

    /// <summary>
    /// Marker component used to track whether listeners have already been attached 
    /// to the EmptySaveSlot button to prevent listener stacking.
    /// </summary>
    public class SaveMenuEmptySaveSlotMarker : MonoBehaviour
    {
    }

    /// <summary>
    /// Marker component used to track whether listeners have already been attached 
    /// to a SaveSlots overwrite button to prevent listener stacking.
    /// </summary>
    public class SaveMenuOverwriteSlotMarker : MonoBehaviour
    {
    }
} 