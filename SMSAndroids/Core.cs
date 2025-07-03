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
    [BepInPlugin(pluginGuid, Core.pluginName, Core.pluginVersion)]
    public class Core : BaseUnityPlugin
    {
        #region Plugin Info
        public const string pluginGuid = "treboy.starmakerstory.smsandroidscore.core";
        public const string pluginName = "Androids Core";
        public const string pluginVersion = "0.4.0";
        #endregion

        public static AssetBundle dialogueBundle;
        public static AssetBundle otherBundle;
        public static bool loadedCore = false;
        public static bool loadedBases = false;
        public static GameObject afterSleepEvents;
        public static GameObject affectionIncrease;
        public static GameObject baseBust;
        public static GameObject introMomentNewGame;
        public static GameObject levelBeach;
        public static GameObject saveButton1;
        public static GameObject saveButton2;
        public static GameObject saveButton3;
        public static GameObject saveButton4;
        public static GameObject saveButton5;
        public static GameObject saveButton6;
        public static GameObject saveButton7;
        public static GameObject saveButton8;
        public static GameObject savedUI;
        public static GameObject saveLoadSystem;
        public static int loadedSaveFile;
        public static SaveLoadManager saveLoadManager;
        public static Scene currentScene;
        public static string assetPath = "BepInEx\\plugins\\SMSAndroidsCore\\Assets\\";
        public static string audioPath = "BepInEx\\plugins\\SMSAndroidsCore\\Audio\\";
        public static string bustPath = "BepInEx\\plugins\\SMSAndroidsCore\\Busts\\";
        public static string bustNikkePath = "BepInEx\\plugins\\SMSAndroidsCore\\Busts\\Nikke\\";
        public static string locationPath = "BepInEx\\plugins\\SMSAndroidsCore\\Locations\\";
        public static string savesPath = "BepInEx\\plugins\\SMSAndroidsCore\\Saves\\";
        public static string scenePath = "BepInEx\\plugins\\SMSAndroidsCore\\Scenes\\";
        public static string exePath;
        public static Transform attractorCanvas;
        public static Transform bustManager;
        public static Transform cGManagerSexy;
        public static Transform coreEvents;
        public static Transform effects;
        public static Transform gameplay;
        public static Transform level;
        public static Transform mainCanvas;
        public static Transform roomTalk;

        public void Awake()
        {
            Logger.LogInfo("Awake");

            exePath = Application.dataPath;
            if (Application.platform == RuntimePlatform.OSXPlayer)
            {
                exePath += "/../../";
            }
            else if (Application.platform == RuntimePlatform.WindowsPlayer)
            {
                exePath += "/../";
            }
            dialogueBundle = AssetBundle.LoadFromFile(exePath + assetPath + "dialoguebundle");
            otherBundle = AssetBundle.LoadFromFile(exePath + assetPath + "otherbundle");

            Logger.LogInfo("Asset Bundles loaded.");

            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        public void OnSceneLoaded(Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
        {
            currentScene = scene;
            loadedCore = false;
            Characters.loadedBusts = false;
            Dialogues.loadedDialogues = false;
            MainStory.loadedStory = false;
            Places.loadedPlaces = false;
            Scenes.loadedScenes = false;
        }
        public void Update()
        {
            if (loadedCore && Characters.loadedBusts && Dialogues.loadedDialogues && MainStory.loadedStory && Places.loadedPlaces && Scenes.loadedScenes)
            {
                loadedBases = true;
            }
            else
            {
                loadedBases = false;
            }

            if (currentScene.name == "CoreGameScene")
            {
                if (!loadedCore)
                {
                    saveLoadManager = GameObject.FindFirstObjectByType<SaveLoadManager>();
                    loadedSaveFile = saveLoadManager.SlotLoaded;

                    attractorCanvas = GameObject.Find("Attractor_Canvas").transform;
                    bustManager = GameObject.Find("2_Bust_Manager").transform;
                    cGManagerSexy = GameObject.Find("4_CG_Manager-Sexy").transform;
                    level = GameObject.Find("5_Levels").transform;
                    effects = GameObject.Find("6_Effects").transform;
                    coreEvents = GameObject.Find("8_Core_Events").transform;
                    roomTalk = GameObject.Find("8_Room_Talk").transform;
                    mainCanvas = GameObject.Find("9_MainCanvas").transform;
                    gameplay = GameObject.Find("10_Gameplay").transform;

                    baseBust = bustManager.Find("Anna_YellowSexy").gameObject;
                    levelBeach = level.Find("14_Beach").gameObject;
                    afterSleepEvents = mainCanvas.Find("AfterSleepEvents").gameObject;
                    introMomentNewGame = mainCanvas.transform.Find("Starmaker").Find("Intro_Moment").gameObject;
                    savedUI = effects.Find("Effect_Canvas").Find("Game_Saved").Find("Saved").gameObject;
                    saveLoadSystem = mainCanvas.transform.Find("SaveLoadSystem").gameObject;
                    saveButton1 = saveLoadSystem.transform.Find("ButtonList").GetChild(0).Find("Button (2)").gameObject;
                    saveButton2 = saveLoadSystem.transform.Find("ButtonList").GetChild(1).Find("Button (2)").gameObject;
                    saveButton3 = saveLoadSystem.transform.Find("ButtonList").GetChild(2).Find("Button (2)").gameObject;
                    saveButton4 = saveLoadSystem.transform.Find("ButtonList").GetChild(3).Find("Button (2)").gameObject;
                    saveButton5 = saveLoadSystem.transform.Find("ButtonList").GetChild(4).Find("Button (2)").gameObject;
                    saveButton6 = saveLoadSystem.transform.Find("ButtonList").GetChild(5).Find("Button (2)").gameObject;
                    saveButton7 = saveLoadSystem.transform.Find("ButtonList").GetChild(6).Find("Button (2)").gameObject;
                    saveButton8 = saveLoadSystem.transform.Find("ButtonList").GetChild(7).Find("Button (2)").gameObject;

                    affectionIncrease = GameObject.Instantiate(effects.Find("CFXR2 Expression Loving (Timed)").gameObject, effects);
                    affectionIncrease.name = "CFXR2 Expression Affectionate (Timed)";
                    affectionIncrease.GetComponent<ParticleSystem>().startColor = new Color(1.0f, 0.6f, 0.9f);
                    affectionIncrease.transform.Find("Hearts").gameObject.GetComponent<ParticleSystem>().startColor = new Color(1.0f, 0.35f, 0.7f);

                    Logger.LogInfo("----- CORE LOADED -----");
                    loadedCore = true;
                }
            }
            if (currentScene.name == "GameStart")
            {
                if (loadedCore)
                {
                    Logger.LogInfo("----- CORE UNLOADED -----");
                    loadedCore = false;
                }
            }
        }
        
        public static void FindAndModifyVariableDouble(string variableNameToFind, Double newValue)
        {
            var manager = GlobalNameVariablesManager.Instance;
            if (manager == null)
            {
                Debug.LogError("GlobalNameVariablesManager not initialized");
                return;
            }

            // Access private Values dictionary
            PropertyInfo valuesProp = typeof(GlobalNameVariablesManager).GetProperty(
                "Values",
                BindingFlags.NonPublic | BindingFlags.Instance
            );

            var values = valuesProp.GetValue(manager) as Dictionary<IdString, NameVariableRuntime>;
            if (values == null) return;

            bool found = false;

            foreach (var pair in values)
            {
                // Access the runtime's Variables dictionary
                PropertyInfo varsProp = typeof(NameVariableRuntime).GetProperty(
                    "Variables",
                    BindingFlags.NonPublic | BindingFlags.Instance
                );

                var variables = varsProp.GetValue(pair.Value) as Dictionary<string, NameVariable>;
                if (variables == null) continue;

                if (variables.TryGetValue(variableNameToFind, out var nameVar))
                {
                    // Get the asset name for logging
                    var repo = TRepository<VariablesRepository>.Get;
                    var asset = repo?.Variables.GetNameVariablesAsset(pair.Key);
                    string assetName = asset != null ? asset.name : $"Unknown Asset ({pair.Key})";

                    // Store old value
                    object oldValue = nameVar.Value;

                    // Modify the value
                    nameVar.Value = newValue;
                    found = true;

                    Debug.Log($"Modified {variableNameToFind} in {assetName} " +
                             $"from {oldValue} to {newValue}");

                    // Trigger change event if needed
                    MethodInfo eventMethod = typeof(NameVariableRuntime).GetMethod(
                        "OnRuntimeChange",
                        BindingFlags.NonPublic | BindingFlags.Instance
                    );
                    eventMethod?.Invoke(pair.Value, new object[] { variableNameToFind });
                }
            }

            if (!found)
            {
                Debug.LogError($"Variable '{variableNameToFind}' not found in any global variable set");
            }
        }
        public static void FindAndModifyVariableBool(string variableNameToFind, bool newValue)
        {
            var manager = GlobalNameVariablesManager.Instance;
            if (manager == null)
            {
                Debug.LogError("GlobalNameVariablesManager not initialized");
                return;
            }

            // Access private Values dictionary
            PropertyInfo valuesProp = typeof(GlobalNameVariablesManager).GetProperty(
                "Values",
                BindingFlags.NonPublic | BindingFlags.Instance
            );

            var values = valuesProp.GetValue(manager) as Dictionary<IdString, NameVariableRuntime>;
            if (values == null) return;

            bool found = false;

            foreach (var pair in values)
            {
                // Access the runtime's Variables dictionary
                PropertyInfo varsProp = typeof(NameVariableRuntime).GetProperty(
                    "Variables",
                    BindingFlags.NonPublic | BindingFlags.Instance
                );

                var variables = varsProp.GetValue(pair.Value) as Dictionary<string, NameVariable>;
                if (variables == null) continue;

                if (variables.TryGetValue(variableNameToFind, out var nameVar))
                {
                    // Get the asset name for logging
                    var repo = TRepository<VariablesRepository>.Get;
                    var asset = repo?.Variables.GetNameVariablesAsset(pair.Key);
                    string assetName = asset != null ? asset.name : $"Unknown Asset ({pair.Key})";

                    // Store old value
                    object oldValue = nameVar.Value;

                    // Modify the value
                    nameVar.Value = newValue;
                    found = true;

                    Debug.Log($"Modified {variableNameToFind} in {assetName} " +
                             $"from {oldValue} to {newValue}");

                    // Trigger change event if needed
                    MethodInfo eventMethod = typeof(NameVariableRuntime).GetMethod(
                        "OnRuntimeChange",
                        BindingFlags.NonPublic | BindingFlags.Instance
                    );
                    eventMethod?.Invoke(pair.Value, new object[] { variableNameToFind });
                }
            }

            if (!found)
            {
                Debug.LogError($"Variable '{variableNameToFind}' not found in any global variable set");
            }
        }
        public static void FindAndModifyVariableGameObject(string variableNameToFind, GameObject newValue)
        {
            var manager = GlobalNameVariablesManager.Instance;
            if (manager == null)
            {
                Debug.LogError("GlobalNameVariablesManager not initialized");
                return;
            }

            // Access private Values dictionary
            PropertyInfo valuesProp = typeof(GlobalNameVariablesManager).GetProperty(
                "Values",
                BindingFlags.NonPublic | BindingFlags.Instance
            );

            var values = valuesProp.GetValue(manager) as Dictionary<IdString, NameVariableRuntime>;
            if (values == null) return;

            bool found = false;

            foreach (var pair in values)
            {
                // Access the runtime's Variables dictionary
                PropertyInfo varsProp = typeof(NameVariableRuntime).GetProperty(
                    "Variables",
                    BindingFlags.NonPublic | BindingFlags.Instance
                );

                var variables = varsProp.GetValue(pair.Value) as Dictionary<string, NameVariable>;
                if (variables == null) continue;

                if (variables.TryGetValue(variableNameToFind, out var nameVar))
                {
                    // Get the asset name for logging
                    var repo = TRepository<VariablesRepository>.Get;
                    var asset = repo?.Variables.GetNameVariablesAsset(pair.Key);
                    string assetName = asset != null ? asset.name : $"Unknown Asset ({pair.Key})";

                    // Store old value
                    object oldValue = nameVar.Value;

                    // Modify the value
                    nameVar.Value = newValue;
                    found = true;

                    Debug.Log($"Modified {variableNameToFind} in {assetName} " +
                             $"from {oldValue} to {newValue}");

                    // Trigger change event if needed
                    MethodInfo eventMethod = typeof(NameVariableRuntime).GetMethod(
                        "OnRuntimeChange",
                        BindingFlags.NonPublic | BindingFlags.Instance
                    );
                    eventMethod?.Invoke(pair.Value, new object[] { variableNameToFind });
                }
            }

            if (!found)
            {
                Debug.LogError($"Variable '{variableNameToFind}' not found in any global variable set");
            }
        }
        public static bool GetVariableBool(string variableNameToFind)
        {
            var manager = GlobalNameVariablesManager.Instance;
            if (manager == null)
            {
                Debug.LogError("GlobalNameVariablesManager not initialized");
                return false;
            }

            // Access private Values dictionary
            PropertyInfo valuesProp = typeof(GlobalNameVariablesManager).GetProperty(
                "Values",
                BindingFlags.NonPublic | BindingFlags.Instance
            );

            var values = valuesProp.GetValue(manager) as Dictionary<IdString, NameVariableRuntime>;
            if (values == null) return false;

            foreach (var pair in values)
            {
                // Access the runtime's Variables dictionary
                PropertyInfo varsProp = typeof(NameVariableRuntime).GetProperty(
                    "Variables",
                    BindingFlags.NonPublic | BindingFlags.Instance
                );

                var variables = varsProp.GetValue(pair.Value) as Dictionary<string, NameVariable>;
                if (variables == null) continue;

                if (variables.TryGetValue(variableNameToFind, out var nameVar))
                {
                    return (bool)nameVar.Value;
                }
            }

            Debug.LogError($"Variable '{variableNameToFind}' not found in any global variable set");
            return false;
        }
        public static double GetVariableNumber(string variableNameToFind)
        {
            var manager = GlobalNameVariablesManager.Instance;
            if (manager == null)
            {
                Debug.LogError("GlobalNameVariablesManager not initialized");
                return 0.0;
            }

            // Access private Values dictionary
            PropertyInfo valuesProp = typeof(GlobalNameVariablesManager).GetProperty(
                "Values",
                BindingFlags.NonPublic | BindingFlags.Instance
            );

            var values = valuesProp.GetValue(manager) as Dictionary<IdString, NameVariableRuntime>;
            if (values == null) return 0.0;

            foreach (var pair in values)
            {
                // Access the runtime's Variables dictionary
                PropertyInfo varsProp = typeof(NameVariableRuntime).GetProperty(
                    "Variables",
                    BindingFlags.NonPublic | BindingFlags.Instance
                );

                var variables = varsProp.GetValue(pair.Value) as Dictionary<string, NameVariable>;
                if (variables == null) continue;

                if (variables.TryGetValue(variableNameToFind, out var nameVar))
                {
                    try
                    {
                        return Convert.ToDouble(nameVar.Value);
                    }
                    catch (Exception)
                    {
                        Debug.LogWarning($"Variable '{variableNameToFind}' found but could not be converted to a number. Value: {nameVar.Value}");
                        return 0.0;
                    }
                }
            }

            Debug.LogError($"Variable '{variableNameToFind}' not found in any global variable set");
            return 0.0;
        }
        public static void AddGameObjectToLocalListVariables(GameObject targetGameObject, GameObject valueToAdd)
        {
            if (targetGameObject == null)
            {
                Debug.LogError("Target GameObject is null");
                return;
            }

            LocalListVariables localListVariables = targetGameObject.GetComponent<LocalListVariables>();
            if (localListVariables == null)
            {
                Debug.LogError($"LocalListVariables component not found on GameObject: {targetGameObject.name}");
                return;
            }

            localListVariables.Push(valueToAdd);
            Debug.Log($"Added GameObject '{valueToAdd?.name ?? "null"}' to LocalListVariables on GameObject: {targetGameObject.name}. New count: {localListVariables.Count}");
        }
        public static int GetRandomNumber(int max)
        {
            return UnityEngine.Random.Range(0, max + 1);
        }
        public static float GetRandomFloat(float min, float max)
        {
            return UnityEngine.Random.Range(min, max);
        }
    }
}
