using BepInEx;
using GameCreator;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.Common.Audio;
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
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using System.Reflection;
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

        // Default values for all mod variables
        private static readonly Dictionary<string, object> defaultValues = new Dictionary<string, object>
        {
            // Secret Beach variables
            { "SecretBeach_RelaxedAmount", 0 },
            { "SecretBeach_Unlocked", false },
            { "SecretBeach_FirstVisit", false },
            
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
            { "Mod_Initialized", false },
            { "Mod_Version", "0.4.0" }
        };

        public void Awake()
        {
            instance = this;
            Logger.LogInfo("SaveManager plugin loaded");
            
            // Ensure saves directory exists
            if (!Directory.Exists(Core.savesPath))
            {
                Directory.CreateDirectory(Core.savesPath);
                Logger.LogInfo($"Created saves directory: {Core.savesPath}");
            }
        }

        public void Update()
        {
            // Update current save slot if it changed
            if (Core.saveLoadManager != null && Core.saveLoadManager.SlotLoaded != currentSaveSlot)
            {
                currentSaveSlot = Core.saveLoadManager.SlotLoaded;
                LoadSaveFile();
            }
        }

        #region Public Methods

        /// <summary>
        /// Sets a string value for the current save slot
        /// </summary>
        public static void SetString(string variableName, string value)
        {
            if (instance == null) return;
            instance.SetValueInternal(variableName, value);
        }

        /// <summary>
        /// Gets a string value for the current save slot
        /// </summary>
        public static string GetString(string variableName, string defaultValue = "")
        {
            if (instance == null) return defaultValue;
            return instance.GetValueInternal(variableName, defaultValue);
        }

        /// <summary>
        /// Sets an integer value for the current save slot
        /// </summary>
        public static void SetInt(string variableName, int value)
        {
            if (instance == null) return;
            instance.SetValueInternal(variableName, value);
        }

        /// <summary>
        /// Gets an integer value for the current save slot
        /// </summary>
        public static int GetInt(string variableName, int defaultValue = 0)
        {
            if (instance == null) return defaultValue;
            return instance.GetValueInternal(variableName, defaultValue);
        }

        /// <summary>
        /// Sets a float value for the current save slot
        /// </summary>
        public static void SetFloat(string variableName, float value)
        {
            if (instance == null) return;
            instance.SetValueInternal(variableName, value);
        }

        /// <summary>
        /// Gets a float value for the current save slot
        /// </summary>
        public static float GetFloat(string variableName, float defaultValue = 0f)
        {
            if (instance == null) return defaultValue;
            return instance.GetValueInternal(variableName, defaultValue);
        }

        /// <summary>
        /// Sets a boolean value for the current save slot
        /// </summary>
        public static void SetBool(string variableName, bool value)
        {
            if (instance == null) return;
            instance.SetValueInternal(variableName, value);
        }

        /// <summary>
        /// Gets a boolean value for the current save slot
        /// </summary>
        public static bool GetBool(string variableName, bool defaultValue = false)
        {
            if (instance == null) return defaultValue;
            return instance.GetValueInternal(variableName, defaultValue);
        }

        /// <summary>
        /// Deletes a variable for the current save slot
        /// </summary>
        public static void DeleteVariable(string variableName)
        {
            if (instance == null) return;
            instance.DeleteVariableInternal(variableName);
        }

        /// <summary>
        /// Checks if a variable exists for the current save slot
        /// </summary>
        public static bool HasVariable(string variableName)
        {
            if (instance == null) return false;
            return instance.HasVariableInternal(variableName);
        }

        /// <summary>
        /// Gets all variables for the current save slot
        /// </summary>
        public static Dictionary<string, object> GetAllVariables()
        {
            if (instance == null) return new Dictionary<string, object>();
            return new Dictionary<string, object>(instance.currentSlotCache);
        }

        /// <summary>
        /// Clears all variables for the current save slot
        /// </summary>
        public static void ClearAllVariables()
        {
            if (instance == null) return;
            instance.ClearAllVariablesInternal();
        }

        /// <summary>
        /// Resets all variables to their default values for the current save slot
        /// </summary>
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

            // Save to file
            SaveToFile();
            Debug.Log($"[SaveManager] Saved {variableName} = {value} for save slot {currentSaveSlot}");
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
            SaveToFile();
            Debug.Log($"[SaveManager] Deleted {variableName} for save slot {currentSaveSlot}");
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
            SaveToFile();
            Debug.Log($"[SaveManager] Cleared all variables for save slot {currentSaveSlot}");
        }

        private void ResetToDefaultsInternal()
        {
            if (currentSaveSlot < 0) return;

            currentSlotCache.Clear();
            
            // Add all default values
            foreach (var kvp in defaultValues)
            {
                currentSlotCache[kvp.Key] = kvp.Value;
            }

            SaveToFile();
            Debug.Log($"[SaveManager] Reset all variables to defaults for save slot {currentSaveSlot}");
        }

        private void LoadSaveFile()
        {
            if (currentSaveSlot < 0) return;

            currentSaveFilePath = Path.Combine(Core.savesPath, $"save_{currentSaveSlot}.json");
            currentSlotCache.Clear();

            try
            {
                if (File.Exists(currentSaveFilePath))
                {
                    string jsonContent = File.ReadAllText(currentSaveFilePath);
                    var loadedData = JsonUtility.FromJson<SaveData>(jsonContent);
                    
                    if (loadedData != null && loadedData.variableEntries != null)
                    {
                        foreach (var entry in loadedData.variableEntries)
                        {
                            // Convert the serialized value back to the appropriate type
                            object value = ConvertSerializedValue(entry.value, entry.type);
                            currentSlotCache[entry.key] = value;
                        }
                    }
                    
                    Debug.Log($"[SaveManager] Loaded save file: {currentSaveFilePath}");
                }
                else
                {
                    // Create new save file with default values
                    ResetToDefaultsInternal();
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

        private void SaveToFile()
        {
            if (currentSaveSlot < 0 || string.IsNullOrEmpty(currentSaveFilePath)) return;

            try
            {
                var variableEntries = new List<VariableEntry>();
                
                foreach (var kvp in currentSlotCache)
                {
                    var entry = new VariableEntry
                    {
                        key = kvp.Key,
                        value = kvp.Value.ToString(),
                        type = GetValueType(kvp.Value)
                    };
                    variableEntries.Add(entry);
                }

                var saveData = new SaveData
                {
                    saveSlot = currentSaveSlot,
                    lastSaved = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    variableEntries = variableEntries
                };

                string jsonContent = JsonUtility.ToJson(saveData, true);
                File.WriteAllText(currentSaveFilePath, jsonContent);
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveManager] Error saving to file: {e.Message}");
            }
        }

        private string GetValueType(object value)
        {
            if (value is bool) return "bool";
            if (value is int) return "int";
            if (value is float) return "float";
            if (value is string) return "string";
            return "string"; // Default fallback
        }

        private object ConvertSerializedValue(string serializedValue, string type)
        {
            try
            {
                switch (type)
                {
                    case "bool":
                        return bool.Parse(serializedValue);
                    case "int":
                        return int.Parse(serializedValue);
                    case "float":
                        return float.Parse(serializedValue);
                    case "string":
                    default:
                        return serializedValue;
                }
            }
            catch
            {
                // Return default value if parsing fails
                switch (type)
                {
                    case "bool": return false;
                    case "int": return 0;
                    case "float": return 0f;
                    case "string":
                    default: return "";
                }
            }
        }

        #endregion

        #region Global Variable Integration

        /// <summary>
        /// Syncs a saved value to a global variable for use in the game
        /// </summary>
        public static void SyncToGlobalVariable(string saveVariableName, string globalVariableName)
        {
            if (instance == null) return;

            // Get the saved value (assuming it's a boolean for now)
            bool value = GetBool(saveVariableName, false);
            
            // Set it as a global variable
            Core.FindAndModifyVariableBool(globalVariableName, value);
            Debug.Log($"[SaveManager] Synced {saveVariableName} ({value}) to global variable {globalVariableName}");
        }

        /// <summary>
        /// Syncs a global variable value back to the save system
        /// </summary>
        public static void SyncFromGlobalVariable(string globalVariableName, string saveVariableName)
        {
            if (instance == null) return;

            // Get the global variable value
            bool value = Core.GetVariableBool(globalVariableName);
            
            // Save it
            SetBool(saveVariableName, value);
            Debug.Log($"[SaveManager] Synced global variable {globalVariableName} ({value}) to {saveVariableName}");
        }

        #endregion

        #region Save Data Structure

        [Serializable]
        private class SaveData
        {
            public int saveSlot;
            public string lastSaved;
            public List<VariableEntry> variableEntries;
        }

        [Serializable]
        private class VariableEntry
        {
            public string key;
            public string value;
            public string type;
        }

        #endregion
    }
} 