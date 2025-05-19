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
        public static GameObject baseBust;
        public static int loadedSaveFile;
        public static SaveLoadManager saveLoadManager;
        public static Scene currentScene;
        public static string assetPath = "BepInEx\\plugins\\SMSAndroidsCore\\Assets\\";
        public static string audioPath = "BepInEx\\plugins\\SMSAndroidsCore\\Audio\\";
        public static string bustPath = "BepInEx\\plugins\\SMSAndroidsCore\\Busts\\";
        public static string locationPath = "BepInEx\\plugins\\SMSAndroidsCore\\Locations\\";
        public static string scenePath = "BepInEx\\plugins\\SMSAndroidsCore\\Scenes\\";
        public static string exePath;
        public static Transform bustManager;
        public static Transform cGManagerSexy;
        public static Transform coreEvents;
        public static Transform level;
        public static Transform mainCanvas;
        public static Transform roomTalk;

        private Dictionary<IdString, GlobalVariableChangeTracker> variableTrackers = new Dictionary<IdString, GlobalVariableChangeTracker>();
        private float monitoringInterval = 0.1f; // Check every 0.1 seconds
        private bool isMonitoring = false;
        private Coroutine monitoringCoroutine;


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

            StartMonitoringGlobalVariables();
        }
        public void Update()
        {
            currentScene = SceneManager.GetActiveScene();
            if (currentScene.name == "CoreGameScene")
            {
                if (!loadedCore)
                {
                    saveLoadManager = GameObject.FindFirstObjectByType<SaveLoadManager>();
                    loadedSaveFile = saveLoadManager.SlotLoaded;

                    bustManager = GameObject.Find("2_Bust_Manager").transform;
                    cGManagerSexy = GameObject.Find("4_CG_Manager-Sexy").transform;
                    level = GameObject.Find("5_Levels").transform;
                    coreEvents = GameObject.Find("8_Core_Events").transform;
                    roomTalk = GameObject.Find("8_Room_Talk").transform;
                    mainCanvas = GameObject.Find("9_MainCanvas").transform;
                    baseBust = bustManager.Find("Anna_YellowSexy").gameObject;

                    #region Initialize Keys
                    if (!PlayerPrefs.HasKey(loadedSaveFile + "_MPE_Initialized"))
                    {
                        PlayerPrefs.SetInt(loadedSaveFile + "_MPE_Initialized", 1);
                        PlayerPrefs.SetInt(loadedSaveFile + "_MPE_SecretBeach_RelaxedAmount", 0);
                        PlayerPrefs.SetInt(loadedSaveFile + "_MPE_Voyeur_SeenAnis", 0);
                        PlayerPrefs.SetInt(loadedSaveFile + "_MPE_Voyeur_SeenFrima", 0);
                        PlayerPrefs.SetInt(loadedSaveFile + "_MPE_Voyeur_SeenGuilty", 0);
                        PlayerPrefs.SetInt(loadedSaveFile + "_MPE_Voyeur_SeenHelm", 0);
                        PlayerPrefs.SetInt(loadedSaveFile + "_MPE_Voyeur_SeenMaiden", 0);
                        PlayerPrefs.SetInt(loadedSaveFile + "_MPE_Voyeur_SeenMary", 0);
                        PlayerPrefs.SetInt(loadedSaveFile + "_MPE_Voyeur_SeenMast", 0);
                        PlayerPrefs.SetInt(loadedSaveFile + "_MPE_Voyeur_SeenNeon", 0);
                        PlayerPrefs.SetInt(loadedSaveFile + "_MPE_Voyeur_SeenPepper", 0);
                        PlayerPrefs.SetInt(loadedSaveFile + "_MPE_Voyeur_SeenRapi", 0);
                        PlayerPrefs.SetInt(loadedSaveFile + "_MPE_Voyeur_SeenRosanna", 0);
                        PlayerPrefs.SetInt(loadedSaveFile + "_MPE_Voyeur_SeenSakura", 0);
                        PlayerPrefs.SetInt(loadedSaveFile + "_MPE_Voyeur_SeenViper", 0);
                        PlayerPrefs.SetInt(loadedSaveFile + "_MPE_Voyeur_SeenYan", 0);
                        PlayerPrefs.Save();
                        Logger.LogInfo("New PlayerPrefs for current save set.");
                    }
                    #endregion

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
        #region GlobalNameVariable monitoring
        public class GlobalVariableChangeTracker
        {
            private Dictionary<string, object> previousValues = new Dictionary<string, object>();
            private string assetName;

            public GlobalVariableChangeTracker(string assetName)
            {
                this.assetName = assetName;
            }

            public void TrackVariable(string variableName, object currentValue)
            {
                if (!previousValues.ContainsKey(variableName))
                {
                    previousValues[variableName] = currentValue;
                    return;
                }

                var previousValue = previousValues[variableName];
                if (!Equals(previousValue, currentValue))
                {
                    Debug.Log($"[Global Variable Changed] {assetName}.{variableName}: {previousValue} -> {currentValue}");
                    previousValues[variableName] = currentValue;
                }
            }
        }
        public void StartMonitoringGlobalVariables()
        {
            if (!isMonitoring)
            {
                isMonitoring = true;
                monitoringCoroutine = StartCoroutine(MonitorGlobalVariablesCoroutine());
                Debug.Log("Started monitoring global variables");
            }
        }
        public void StopMonitoringGlobalVariables()
        {
            if (isMonitoring)
            {
                isMonitoring = false;
                if (monitoringCoroutine != null)
                {
                    StopCoroutine(monitoringCoroutine);
                    monitoringCoroutine = null;
                }
                Debug.Log("Stopped monitoring global variables");
            }
        }
        private IEnumerator MonitorGlobalVariablesCoroutine()
        {
            var waitInterval = new WaitForSeconds(monitoringInterval);

            while (isMonitoring)
            {
                MonitorGlobalVariableChanges();
                yield return waitInterval;
            }
        }
        public void MonitorGlobalVariableChanges()
        {
            var manager = GlobalNameVariablesManager.Instance;
            if (manager == null) return;

            PropertyInfo valuesProp = typeof(GlobalNameVariablesManager).GetProperty(
                "Values",
                BindingFlags.NonPublic | BindingFlags.Instance
            );

            if (valuesProp == null) return;

            var values = valuesProp.GetValue(manager) as Dictionary<IdString, NameVariableRuntime>;
            if (values == null) return;

            var repo = TRepository<VariablesRepository>.Get;
            if (repo == null) return;

            foreach (var pair in values)
            {
                var asset = repo.Variables.GetNameVariablesAsset(pair.Key);
                string assetName = asset != null ? asset.name : $"Unknown Asset ({pair.Key})";

                // Get or create tracker for this asset
                if (!variableTrackers.TryGetValue(pair.Key, out var tracker))
                {
                    tracker = new GlobalVariableChangeTracker(assetName);
                    variableTrackers[pair.Key] = tracker;
                }

                PropertyInfo runtimeVarsProp = typeof(NameVariableRuntime).GetProperty(
                    "Variables",
                    BindingFlags.NonPublic | BindingFlags.Instance
                );

                if (runtimeVarsProp == null) continue;

                var variables = runtimeVarsProp.GetValue(pair.Value) as Dictionary<string, NameVariable>;
                if (variables == null) continue;

                foreach (var varPair in variables)
                {
                    object value = varPair.Value?.Value;
                    tracker.TrackVariable(varPair.Key, value);
                }
            }
        }
        #endregion
    }
}
