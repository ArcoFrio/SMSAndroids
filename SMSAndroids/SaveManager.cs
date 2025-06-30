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
        private bool saveListenersAdded = false;
        private bool newGame = false;
        private bool modSaveThisSession = false;

        // Default values for all mod variables
        private static readonly Dictionary<string, object> defaultValues = new Dictionary<string, object>
        {
            // Secret Beach variables
            { "SecretBeach_FirstVisited", false },
            { "SecretBeach_GKSeen", false },
            { "SecretBeach_RelaxedAmount", 0 },
            { "SecretBeach_UnlockedLab", false },
            
            // Voyeur variables for each character
            { "Voyeur_SeenAnis", false },
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
            { "Voyeur_SeenViper", false },
            { "Voyeur_SeenYan", false },
            
            // General mod variables
            { "Mod_Version", Core.pluginVersion }
        };

        public void Awake()
        {
            instance = this;
            Logger.LogInfo("SaveManager plugin loaded");

            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        public void OnSceneLoaded(Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
        {
            MainStory.relaxed = false;
            modSaveThisSession = false;
            MainStory.actionTodaySB = false;
            MainStory.voyeurLotteryNumber = Core.GetRandomNumber(100);
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
        }

        public void Update()
        {
            if (Core.currentScene.name == "CoreGameScene")
            {

                if (Core.afterSleepEvents.activeSelf && !afterSleepEventsProc)
                {
                    afterSleepEventsProc = true;
                }
                if (Core.savedUI.activeSelf && afterSleepEventsProc)
                {
                    SaveToFile();
                    MainStory.voyeurTargetsLeft.Clear();
                    foreach (string character in MainStory.voyeurTargets)
                    {
                        if (!SaveManager.GetBool($"Voyeur_Seen{character}"))
                        {
                            MainStory.voyeurTargetsLeft.Add(character);
                        }
                    }
                    MainStory.relaxed = false;
                    MainStory.actionTodaySB = false;
                    MainStory.voyeurLotteryNumber = Core.GetRandomNumber(100);
                    afterSleepEventsProc = false;
                }


                if (Core.saveLoadSystem.activeSelf && !saveListenersAdded)
                {
                    Core.saveButton1.GetComponent<ButtonInstructions>().onClick.AddListener(() => {
                        OverwriteSaveSlotWithCurrentFile(2);
                    });
                    Core.saveButton2.GetComponent<ButtonInstructions>().onClick.AddListener(() => {
                        OverwriteSaveSlotWithCurrentFile(3);
                    });
                    Core.saveButton3.GetComponent<ButtonInstructions>().onClick.AddListener(() => {
                        OverwriteSaveSlotWithCurrentFile(4);
                    });
                    Core.saveButton4.GetComponent<ButtonInstructions>().onClick.AddListener(() => {
                        OverwriteSaveSlotWithCurrentFile(5);
                    });
                    Core.saveButton5.GetComponent<ButtonInstructions>().onClick.AddListener(() => {
                        OverwriteSaveSlotWithCurrentFile(6);
                    });
                    Core.saveButton6.GetComponent<ButtonInstructions>().onClick.AddListener(() => {
                        OverwriteSaveSlotWithCurrentFile(7);
                    });
                    Core.saveButton7.GetComponent<ButtonInstructions>().onClick.AddListener(() => {
                        OverwriteSaveSlotWithCurrentFile(8);
                    });
                    Core.saveButton8.GetComponent<ButtonInstructions>().onClick.AddListener(() => {
                        OverwriteSaveSlotWithCurrentFile(9);
                    });
                    saveListenersAdded = true;
                }
                if (!Core.saveLoadSystem.activeSelf && saveListenersAdded)
                {
                    Core.saveButton1.GetComponent<ButtonInstructions>().onClick.AddListener(() => {
                        OverwriteSaveSlotWithCurrentFile(2);
                    });
                    Core.saveButton2.GetComponent<ButtonInstructions>().onClick.AddListener(() => {
                        OverwriteSaveSlotWithCurrentFile(3);
                    });
                    Core.saveButton3.GetComponent<ButtonInstructions>().onClick.AddListener(() => {
                        OverwriteSaveSlotWithCurrentFile(4);
                    });
                    Core.saveButton4.GetComponent<ButtonInstructions>().onClick.AddListener(() => {
                        OverwriteSaveSlotWithCurrentFile(5);
                    });
                    Core.saveButton5.GetComponent<ButtonInstructions>().onClick.AddListener(() => {
                        OverwriteSaveSlotWithCurrentFile(6);
                    });
                    Core.saveButton6.GetComponent<ButtonInstructions>().onClick.AddListener(() => {
                        OverwriteSaveSlotWithCurrentFile(7);
                    });
                    Core.saveButton7.GetComponent<ButtonInstructions>().onClick.AddListener(() => {
                        OverwriteSaveSlotWithCurrentFile(8);
                    });
                    Core.saveButton8.GetComponent<ButtonInstructions>().onClick.AddListener(() => {
                        OverwriteSaveSlotWithCurrentFile(9);
                    });
                    saveListenersAdded = false;
                }

                if (Core.introMomentNewGame.activeSelf && !newGame)
                {
                    ResetToDefaults();
                    newGame = true;
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
            currentSaveFilePath = Path.Combine(Core.savesPath, $"save_{currentSaveSlot}.txt");
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
            int slot = slotOverride ?? 1;
            string saveFilePath = Path.Combine(Core.savesPath, $"save_{slot}.txt");

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

        #endregion
        /// Overwrites the specified save slot's file with the contents of the current save slot's file.
        /// This copies only the serialized file data, not the current in-memory variables.
        /// <param name="targetSlot">The save slot to overwrite (e.g., 2, 3, ...)</param>
        public static void OverwriteSaveSlotWithCurrentFile(int targetSlot)
        {
            if (instance == null) return;
            if (instance.currentSaveSlot < 0)
            {
                Debug.LogError("[SaveManager] No current save slot loaded.");
                return;
            }

            string sourceFile;
            if (instance.modSaveThisSession)
            {
                sourceFile = Path.Combine(Core.savesPath, "save_1.txt");
            }
            else
            {
                sourceFile = Path.Combine(Core.savesPath, $"save_{instance.currentSaveSlot}.txt");
            }
            string destFile = Path.Combine(Core.savesPath, $"save_{targetSlot}.txt");

            if (!File.Exists(sourceFile))
            {
                Debug.LogError($"[SaveManager] Source save file does not exist: {sourceFile}");
                return;
            }

            try
            {
                File.Copy(sourceFile, destFile, true);
                Debug.Log($"[SaveManager] Overwrote save slot {targetSlot} with data from {(instance.modSaveThisSession ? "slot 1" : $"slot {instance.currentSaveSlot}")}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveManager] Error overwriting save slot {targetSlot}: {e.Message}");
            }
        }

        // Add utility method to check if a variable has changed between runtime and save file
        public static bool HasVariableChanged(string variableName)
        {
            if (instance == null || instance.currentSaveSlot < 0) return false;

            // Get runtime value
            if (!instance.currentSlotCache.TryGetValue(variableName, out object runtimeValue))
                return false; // Variable doesn't exist in runtime

            // Get saved value
            string saveFilePath = Path.Combine(Core.savesPath, $"save_{instance.currentSaveSlot}.txt");
            if (!File.Exists(saveFilePath))
                return true; // No save file means runtime value is different

            try
            {
                foreach (var line in File.ReadAllLines(saveFilePath))
                {
                    if (string.IsNullOrWhiteSpace(line) || !line.Contains("=")) continue;
                    var parts = line.Split(new[] { '=' }, 2);
                    if (parts[0] == variableName)
                    {
                        var value = parts[1];
                        // Try to parse value based on defaultValues
                        if (defaultValues.TryGetValue(variableName, out object defaultVal))
                        {
                            object savedValue = null;
                            if (defaultVal is int && int.TryParse(value, out int intVal))
                                savedValue = intVal;
                            else if (defaultVal is bool && bool.TryParse(value, out bool boolVal))
                                savedValue = boolVal;
                            else if (defaultVal is float && float.TryParse(value, out float floatVal))
                                savedValue = floatVal;
                            else
                                savedValue = value;

                            return !Equals(runtimeValue, savedValue);
                        }
                        return !Equals(runtimeValue.ToString(), value);
                    }
                }
                return true; // Variable not found in save file means it's different
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveManager] Error checking variable change: {e.Message}");
                return false;
            }
        }
    }
} 