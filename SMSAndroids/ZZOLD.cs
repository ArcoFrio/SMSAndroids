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
    // Helper class to track variable changes
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

    [BepInPlugin(pluginGuid, pluginName, pluginVersion)]
    public class ZZOLD : BaseUnityPlugin
    {
        #region Plugin Info
        public const string pluginGuid = "treboy.starmakerstory.smsandroidscore.core";
        public const string pluginName = "Androids Core";
        public const string pluginVersion = "0.4.0";
        #endregion

        #region Global Variables
        public GlobalNameVariables globalNameVariablesCore;
        public GlobalNameVariablesManager globalNameVariablesManager;
        public GameObject moddingGO;
        public LocalNameVariables localNameVariables;
        public SaveLoadManager saveLoadManager;
        public int loadedSaveFile;
        #endregion
        #region Generic Variables
        public static AssetBundle dialogueBundle;
        public static AssetBundle otherBundle;
        public GameObject afterSleepEvents;
        public GameObject roomTalk;
        public GameObject bustManager;
        public GameObject baseBust;
        public GameObject beach;
        public GameObject basePicScene;
        public GameObject baseDialogueObject;
        public GameObject baseLevel;
        public GameObject savedUI;
        public GameObject defaultDialogueClone;
        public GameObject navigator;
        public GameObject baseMapButton;
        public GameObject wetParticleSystem;
        public List<(GameObject bust, float wght)> totalVoyeurBusts = new List<(GameObject, float)>();
        public List<GameObject> totalVoyeurDialogues = new List<GameObject>();
        public Transform baseBustParent;
        public Transform basePicParent;
        public static string assetPath = "BepInEx\\plugins\\SMSAndroidsCore\\Assets\\";
        public static string audioPath = "BepInEx\\plugins\\SMSAndroidsCore\\Audio\\";
        public static string bustPath = "BepInEx\\plugins\\SMSAndroidsCore\\Busts\\";
        public static string locationPath = "BepInEx\\plugins\\SMSAndroidsCore\\Locations\\";
        public static string scenePath = "BepInEx\\plugins\\SMSAndroidsCore\\Scenes\\";
        public static string exePath;
        public static Scene currentScene;
        public bool activated = false;
        public bool loadedBases = false;
        public bool dialogueStarted = false;
        public bool afterSleepEventsProc = false;
        #endregion
        #region Save & Load
        public GameObject introMomentNewGame;
        public GameObject saveLoadSystem;
        public GameObject saveButton1;
        public GameObject saveButton2;
        public GameObject saveButton3;
        public GameObject saveButton4;
        public GameObject saveButton5;
        public GameObject saveButton6;
        public GameObject saveButton7;
        public GameObject saveButton8;
        public string voyeurTarget = "None";
        public bool isNewGame = false;
        public bool hasNewGameSaved = false;
        public bool hasSaved = false;
        public bool hasQuickSaved = false;
        public float saveCounter = 0;
        #endregion
        #region Secret Beach Variables
        public AudioClip portalBoom;
        public GameObject secretBeachRoomTalk;
        public GameObject secretBeachMapButton;
        public GameObject secretBeachMapButtonText;
        public GameObject secretBeachMapButtonImage;
        public GameObject secretBeachMapButtonImage1;
        public GameObject secretBeachMapButtonKBNumber;
        public static GameObject secretBeachLevel;
        public GameObject secretBeachSky;
        public GameObject secretBeachGatekeeper;
        public GameObject secretBeachGatekeeperB;
        public GameObject secretBeachFlash;
        public GameObject transitionBlinkObject;
        public GameObject transitionFadeObject;
        public GameObject dialogueBeachMain;
        public static GameObject dialogueBeachMainInstance;
        public GameObject beachMainDialogueScene1;
        public GameObject beachMainDialogueScene2;
        public GameObject beachMainDialogueScene3;
        public GameObject beachMainDialogueScene4;
        public GameObject beachMainDialogueActivator;
        public GameObject beachMainDialogueFinisher;
        public GameObject beachMainDialogueSpriteFocus;
        public GameObject dialogueBeachMainGK;
        public GameObject dialogueBeachMainGKInstance;
        public GameObject beachMainDialogueGKScene1;
        public GameObject beachMainDialogueGKScene2;
        public GameObject beachMainDialogueGKScene3;
        public GameObject beachMainDialogueGKScene4;
        public GameObject beachMainDialogueGKScene5;
        public GameObject beachMainDialogueGKActivator;
        public GameObject beachMainDialogueGKFinisher;
        public GameObject beachMainDialogueGKSpriteFocus;
        public GameObject beachMainDialogueGKBlinkActivator;
        public Dialogue dialogueBeachMainDialogue;
        public Dialogue dialogueBeachMainDialogueGK;
        public bool hasSBLoaded = false;
        public bool hasSBAnimationPlayed = false;
        public bool hasSBBlinkPlayed = false;
        public bool hasSBDialogueLoaded = false;
        public bool hasSBRelaxed = false;
        public bool hasSBMainDialoguePlayed = false;
        public bool hasSBMainDialogueGKPlayed = false;
        public bool sBAnimTimerRunning = false;
        public bool sBBlinkTimerRunning = false;
        public bool sBDialogueTimerRunning = false;
        public int sBRelaxedAmount = 0;
        public float sBAnimTimer = 0;
        public float sBBlinkTimer = 0;
        public float sBDialogueTimer = 0;
        public Vector2 refVelocity = Vector2.zero;
        public Vector2 originLevelPos = Vector2.zero;
        #endregion
        #region Anis Variables
        public GameObject anisSwim;
        public GameObject anisBeachScene1;
        public GameObject anisBeachScene2;
        public GameObject anisBeachScene3;
        public GameObject anisBeachScene4;
        public GameObject anisBeachDialogueScene1;
        public GameObject anisBeachDialogueScene2;
        public GameObject anisBeachDialogueScene3;
        public GameObject anisBeachDialogueScene4;
        public GameObject anisBeachDialogueActivator;
        public GameObject anisBeachDialogueFinisher;
        public GameObject anisBeachDialogueSpriteFocus;
        public GameObject anisSwimMBase;
        public GameObject anisSwimWetParticles;
        public GameObject dialogueAnisBeach01;
        public GameObject dialogueAnisBeach01Instance;
        public Dialogue dialogueAnisBeach01Dialogue;
        public bool anisSwimWetParticlesActive = false;
        #endregion
        #region Frima Variables
        public GameObject frimaSwim;
        public GameObject frimaBeachScene1;
        public GameObject frimaBeachScene2;
        public GameObject frimaBeachScene3;
        public GameObject frimaBeachScene4;
        public GameObject frimaBeachDialogueScene1;
        public GameObject frimaBeachDialogueScene2;
        public GameObject frimaBeachDialogueScene3;
        public GameObject frimaBeachDialogueScene4;
        public GameObject frimaBeachDialogueActivator;
        public GameObject frimaBeachDialogueFinisher;
        public GameObject frimaBeachDialogueSpriteFocus;
        public GameObject frimaSwimMBase;
        public GameObject frimaSwimWetParticles;
        public GameObject dialogueFrimaBeach01;
        public GameObject dialogueFrimaBeach01Instance;
        public Dialogue dialogueFrimaBeach01Dialogue;
        public bool frimaSwimWetParticlesActive = false;
        #endregion
        #region Guilty Variables
        public GameObject guiltySwim;
        public GameObject guiltyBeachScene1;
        public GameObject guiltyBeachScene2;
        public GameObject guiltyBeachScene3;
        public GameObject guiltyBeachScene4;
        public GameObject guiltyBeachDialogueScene1;
        public GameObject guiltyBeachDialogueScene2;
        public GameObject guiltyBeachDialogueScene3;
        public GameObject guiltyBeachDialogueScene4;
        public GameObject guiltyBeachDialogueActivator;
        public GameObject guiltyBeachDialogueFinisher;
        public GameObject guiltyBeachDialogueSpriteFocus;
        public GameObject guiltySwimMBase;
        public GameObject guiltySwimWetParticles;
        public GameObject dialogueGuiltyBeach01;
        public GameObject dialogueGuiltyBeach01Instance;
        public Dialogue dialogueGuiltyBeach01Dialogue;
        public bool guiltySwimWetParticlesActive = false;
        #endregion
        #region Helm Variables
        public GameObject helmSwim;
        public GameObject helmBeachScene1;
        public GameObject helmBeachScene2;
        public GameObject helmBeachScene3;
        public GameObject helmBeachScene4;
        public GameObject helmBeachDialogueScene1;
        public GameObject helmBeachDialogueScene2;
        public GameObject helmBeachDialogueScene3;
        public GameObject helmBeachDialogueScene4;
        public GameObject helmBeachDialogueActivator;
        public GameObject helmBeachDialogueFinisher;
        public GameObject helmBeachDialogueSpriteFocus;
        public GameObject helmSwimMBase;
        public GameObject helmSwimWetParticles;
        public GameObject dialogueHelmBeach01;
        public GameObject dialogueHelmBeach01Instance;
        public Dialogue dialogueHelmBeach01Dialogue;
        public bool helmSwimWetParticlesActive = false;
        #endregion
        #region Maiden Variables
        public GameObject maidenSwim;
        public GameObject maidenBeachScene1;
        public GameObject maidenBeachScene2;
        public GameObject maidenBeachScene3;
        public GameObject maidenBeachScene4;
        public GameObject maidenBeachDialogueScene1;
        public GameObject maidenBeachDialogueScene2;
        public GameObject maidenBeachDialogueScene3;
        public GameObject maidenBeachDialogueScene4;
        public GameObject maidenBeachDialogueActivator;
        public GameObject maidenBeachDialogueFinisher;
        public GameObject maidenBeachDialogueSpriteFocus;
        public GameObject maidenSwimMBase;
        public GameObject maidenSwimWetParticles;
        public GameObject dialogueMaidenBeach01;
        public GameObject dialogueMaidenBeach01Instance;
        public Dialogue dialogueMaidenBeach01Dialogue;
        public bool maidenSwimWetParticlesActive = false;
        #endregion
        #region Mary Variables
        public GameObject marySwim;
        public GameObject maryBeachScene1;
        public GameObject maryBeachScene2;
        public GameObject maryBeachScene3;
        public GameObject maryBeachScene4;
        public GameObject maryBeachDialogueScene1;
        public GameObject maryBeachDialogueScene2;
        public GameObject maryBeachDialogueScene3;
        public GameObject maryBeachDialogueScene4;
        public GameObject maryBeachDialogueActivator;
        public GameObject maryBeachDialogueFinisher;
        public GameObject maryBeachDialogueSpriteFocus;
        public GameObject marySwimMBase;
        public GameObject marySwimWetParticles;
        public GameObject dialogueMaryBeach01;
        public GameObject dialogueMaryBeach01Instance;
        public Dialogue dialogueMaryBeach01Dialogue;
        public bool marySwimWetParticlesActive = false;
        #endregion
        #region Mast Variables
        public GameObject mastSwim;
        public GameObject mastBeachScene1;
        public GameObject mastBeachScene2;
        public GameObject mastBeachScene3;
        public GameObject mastBeachScene4;
        public GameObject mastBeachDialogueScene1;
        public GameObject mastBeachDialogueScene2;
        public GameObject mastBeachDialogueScene3;
        public GameObject mastBeachDialogueScene4;
        public GameObject mastBeachDialogueActivator;
        public GameObject mastBeachDialogueFinisher;
        public GameObject mastBeachDialogueSpriteFocus;
        public GameObject mastSwimMBase;
        public GameObject mastSwimWetParticles;
        public GameObject dialogueMastBeach01;
        public GameObject dialogueMastBeach01Instance;
        public Dialogue dialogueMastBeach01Dialogue;
        public bool mastSwimWetParticlesActive = false;
        #endregion
        #region Neon Variables
        public GameObject neonSwim;
        public GameObject neonBeachScene1;
        public GameObject neonBeachScene2;
        public GameObject neonBeachScene3;
        public GameObject neonBeachScene4;
        public GameObject neonBeachDialogueScene1;
        public GameObject neonBeachDialogueScene2;
        public GameObject neonBeachDialogueScene3;
        public GameObject neonBeachDialogueScene4;
        public GameObject neonBeachDialogueActivator;
        public GameObject neonBeachDialogueFinisher;
        public GameObject neonBeachDialogueSpriteFocus;
        public GameObject neonSwimMBase;
        public GameObject neonSwimWetParticles;
        public GameObject dialogueNeonBeach01;
        public GameObject dialogueNeonBeach01Instance;
        public Dialogue dialogueNeonBeach01Dialogue;
        public bool neonSwimWetParticlesActive = false;
        #endregion
        #region Pepper Variables
        public GameObject pepperSwim;
        public GameObject pepperBeachScene1;
        public GameObject pepperBeachScene2;
        public GameObject pepperBeachScene3;
        public GameObject pepperBeachScene4;
        public GameObject pepperBeachDialogueScene1;
        public GameObject pepperBeachDialogueScene2;
        public GameObject pepperBeachDialogueScene3;
        public GameObject pepperBeachDialogueScene4;
        public GameObject pepperBeachDialogueActivator;
        public GameObject pepperBeachDialogueFinisher;
        public GameObject pepperBeachDialogueSpriteFocus;
        public GameObject pepperSwimMBase;
        public GameObject pepperSwimWetParticles;
        public GameObject dialoguePepperBeach01;
        public GameObject dialoguePepperBeach01Instance;
        public Dialogue dialoguePepperBeach01Dialogue;
        public bool pepperSwimWetParticlesActive = false;
        #endregion
        #region Rapi Variables
        public GameObject rapiSwim;
        public GameObject rapiBeachScene1;
        public GameObject rapiBeachScene2;
        public GameObject rapiBeachScene3;
        public GameObject rapiBeachScene4;
        public GameObject rapiBeachDialogueScene1;
        public GameObject rapiBeachDialogueScene2;
        public GameObject rapiBeachDialogueScene3;
        public GameObject rapiBeachDialogueScene4;
        public GameObject rapiBeachDialogueActivator;
        public GameObject rapiBeachDialogueFinisher;
        public GameObject rapiBeachDialogueSpriteFocus;
        public GameObject rapiSwimMBase;
        public GameObject rapiSwimWetParticles;
        public GameObject dialogueRapiBeach01;
        public GameObject dialogueRapiBeach01Instance;
        public Dialogue dialogueRapiBeach01Dialogue;
        public bool rapiWetSwimParticlesActive = false;
        #endregion
        #region Rosanna Variables
        public GameObject rosannaSwim;
        public GameObject rosannaBeachScene1;
        public GameObject rosannaBeachScene2;
        public GameObject rosannaBeachScene3;
        public GameObject rosannaBeachScene4;
        public GameObject rosannaBeachDialogueScene1;
        public GameObject rosannaBeachDialogueScene2;
        public GameObject rosannaBeachDialogueScene3;
        public GameObject rosannaBeachDialogueScene4;
        public GameObject rosannaBeachDialogueActivator;
        public GameObject rosannaBeachDialogueFinisher;
        public GameObject rosannaBeachDialogueSpriteFocus;
        public GameObject rosannaSwimMBase;
        public GameObject rosannaSwimWetParticles;
        public GameObject dialogueRosannaBeach01;
        public GameObject dialogueRosannaBeach01Instance;
        public Dialogue dialogueRosannaBeach01Dialogue;
        public bool rosannaSwimWetParticlesActive = false;
        #endregion
        #region Sakura Variables
        public GameObject sakuraSwim;
        public GameObject sakuraBeachScene1;
        public GameObject sakuraBeachScene2;
        public GameObject sakuraBeachScene3;
        public GameObject sakuraBeachScene4;
        public GameObject sakuraBeachDialogueScene1;
        public GameObject sakuraBeachDialogueScene2;
        public GameObject sakuraBeachDialogueScene3;
        public GameObject sakuraBeachDialogueScene4;
        public GameObject sakuraBeachDialogueActivator;
        public GameObject sakuraBeachDialogueFinisher;
        public GameObject sakuraBeachDialogueSpriteFocus;
        public GameObject sakuraSwimMBase;
        public GameObject sakuraSwimWetParticles;
        public GameObject dialogueSakuraBeach01;
        public GameObject dialogueSakuraBeach01Instance;
        public Dialogue dialogueSakuraBeach01Dialogue;
        public bool sakuraSwimWetParticlesActive = false;
        #endregion
        #region Viper Variables
        public GameObject viperSwim;
        public GameObject viperBeachScene1;
        public GameObject viperBeachScene2;
        public GameObject viperBeachScene3;
        public GameObject viperBeachScene4;
        public GameObject viperBeachDialogueScene1;
        public GameObject viperBeachDialogueScene2;
        public GameObject viperBeachDialogueScene3;
        public GameObject viperBeachDialogueScene4;
        public GameObject viperBeachDialogueActivator;
        public GameObject viperBeachDialogueFinisher;
        public GameObject viperBeachDialogueSpriteFocus;
        public GameObject viperSwimMBase;
        public GameObject viperSwimWetParticles;
        public GameObject dialogueViperBeach01;
        public GameObject dialogueViperBeach01Instance;
        public Dialogue dialogueViperBeach01Dialogue;
        public bool viperSwimWetParticlesActive = false;
        #endregion
        #region Yan Variables
        public GameObject yanSwim;
        public GameObject yanBeachScene1;
        public GameObject yanBeachScene2;
        public GameObject yanBeachScene3;
        public GameObject yanBeachScene4;
        public GameObject yanBeachDialogueScene1;
        public GameObject yanBeachDialogueScene2;
        public GameObject yanBeachDialogueScene3;
        public GameObject yanBeachDialogueScene4;
        public GameObject yanBeachDialogueActivator;
        public GameObject yanBeachDialogueFinisher;
        public GameObject yanBeachDialogueSpriteFocus;
        public GameObject yanSwimMBase;
        public GameObject yanSwimWetParticles;
        public GameObject dialogueYanBeach01;
        public GameObject dialogueYanBeach01Instance;
        public Dialogue dialogueYanBeach01Dialogue;
        public bool yanSwimWetParticlesActive = false;
        #endregion

        private Dictionary<IdString, GlobalVariableChangeTracker> variableTrackers = new Dictionary<IdString, GlobalVariableChangeTracker>();
        private float monitoringInterval = 0.1f; // Check every 0.1 seconds
        private bool isMonitoring = false;
        private Coroutine monitoringCoroutine;

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
                #region Load Bases
                if (roomTalk != GameObject.Find("8_Room_Talk"))
                {
                    loadedBases = false;
                    totalVoyeurBusts.Clear();
                    totalVoyeurDialogues.Clear();
                }
                if (loadedBases == false)
                {
                    #region Core
                    Logger.LogInfo("CoreGameScene loaded.");
                    afterSleepEvents = GameObject.Find("9_MainCanvas").transform.Find("AfterSleepEvents").gameObject;
                    roomTalk = GameObject.Find("8_Room_Talk");
                    bustManager = GameObject.Find("2_Bust_Manager");
                    beach = roomTalk.transform.Find("Beach").gameObject;
                    baseBust = FindInActiveObjectByName("Anna_YellowSexy");
                    baseBustParent = GameObject.Find("2_Bust_Manager").transform;
                    basePicScene = FindInActiveObjectByName("Samanthabeach");
                    basePicParent = GameObject.Find("4_CG_Manager-Sexy").transform;
                    baseLevel = GameObject.Find("5_Levels").transform.Find("14_Beach").gameObject;
                    savedUI = FindInActiveObjectByName("Saved");
                    baseDialogueObject = FindInActiveObjectByName("FailedGroceries").transform.Find("GameObject").gameObject;
                    navigator = FindInActiveObjectByName("Navigator");
                    baseMapButton = navigator.transform.Find("MapButtons").Find("14_beach").gameObject;
                    wetParticleSystem = baseBustParent.transform.Find("Anna_Towel").Find("MBase1").Find("Particle System").gameObject;
                    transitionBlinkObject = GameObject.Find("Transition_Blink");
                    transitionFadeObject = GameObject.Find("Transition_FadeBlackDefault");
                    introMomentNewGame = GameObject.Find("9_MainCanvas").transform.Find("Starmaker").Find("Intro_Moment").gameObject;

                    saveLoadSystem = GameObject.Find("9_MainCanvas").transform.Find("SaveLoadSystem").gameObject;
                    saveButton1 = saveLoadSystem.transform.Find("ButtonList").GetChild(0).Find("Button (2)").gameObject;
                    saveButton2 = saveLoadSystem.transform.Find("ButtonList").GetChild(1).Find("Button (2)").gameObject;
                    saveButton3 = saveLoadSystem.transform.Find("ButtonList").GetChild(2).Find("Button (2)").gameObject;
                    saveButton4 = saveLoadSystem.transform.Find("ButtonList").GetChild(3).Find("Button (2)").gameObject;
                    saveButton5 = saveLoadSystem.transform.Find("ButtonList").GetChild(4).Find("Button (2)").gameObject;
                    saveButton6 = saveLoadSystem.transform.Find("ButtonList").GetChild(5).Find("Button (2)").gameObject;
                    saveButton7 = saveLoadSystem.transform.Find("ButtonList").GetChild(6).Find("Button (2)").gameObject;
                    saveButton8 = saveLoadSystem.transform.Find("ButtonList").GetChild(7).Find("Button (2)").gameObject;
                    voyeurTarget = "None";

                    Logger.LogInfo("Variables assigned.");

                    moddingGO = GameObject.Find("99_Modding");
                    localNameVariables = moddingGO.GetComponent<LocalNameVariables>();
                    globalNameVariablesManager = GameObject.FindFirstObjectByType<GlobalNameVariablesManager>();
                    globalNameVariablesCore = FindGlobalNameVariable("Core");
                    saveLoadManager = GameObject.FindFirstObjectByType<SaveLoadManager>();
                    loadedSaveFile = saveLoadManager.SlotLoaded;
                    Logger.LogInfo("Globals assigned.");
                    Logger.LogInfo("Save file: " + loadedSaveFile);
                    #endregion

                    #region Secret Beach
                    secretBeachMapButton = GameObject.Instantiate(otherBundle.LoadAsset<GameObject>("999_SecretBeach"), baseMapButton.transform.parent);
                    secretBeachMapButtonText = GameObject.Instantiate(baseMapButton.transform.GetChild(0).gameObject, secretBeachMapButton.transform);
                    secretBeachMapButtonImage = GameObject.Instantiate(baseMapButton.transform.GetChild(1).gameObject, secretBeachMapButton.transform);
                    secretBeachMapButtonImage1 = GameObject.Instantiate(baseMapButton.transform.GetChild(2).gameObject, secretBeachMapButton.transform);
                    secretBeachMapButtonKBNumber = GameObject.Instantiate(baseMapButton.transform.GetChild(3).gameObject, secretBeachMapButton.transform);
                    secretBeachMapButton.name = "999_SecretBeach";
                    secretBeachMapButtonText.name = "Text (TMP)";
                    secretBeachMapButtonImage.name = "Image";
                    secretBeachMapButtonImage1.name = "Image (1)";
                    secretBeachMapButtonKBNumber.name = "keyboardnumber";
                    secretBeachMapButton.GetComponent<UnityEngine.UI.Image>().sprite = baseMapButton.GetComponent<UnityEngine.UI.Image>().sprite;
                    secretBeachMapButtonText.GetComponent<TextMeshProUGUI>().text = "Remote area";
                    secretBeachMapButtonImage.GetComponent<UnityEngine.UI.Image>().sprite = baseMapButton.transform.GetChild(1).gameObject.GetComponent<UnityEngine.UI.Image>().sprite;
                    secretBeachMapButtonKBNumber.GetComponent<TextMeshProUGUI>().text = "2";
                    secretBeachLevel = CreateNewLevel("999_SecretBeach", locationPath, "SecretBeach.png", "SecretBeachB.png", "SecretBeachMask.png");
                    secretBeachRoomTalk = GameObject.Instantiate(beach, beach.transform.parent);
                    secretBeachRoomTalk.name = "SecretBeach";
                    secretBeachSky = GameObject.Instantiate(secretBeachLevel.transform.GetChild(1).gameObject, secretBeachLevel.transform);
                    secretBeachFlash = GameObject.Instantiate(secretBeachLevel.transform.GetChild(1).gameObject, secretBeachLevel.transform);
                    secretBeachGatekeeper = GameObject.Instantiate(secretBeachLevel.transform.GetChild(1).gameObject, secretBeachLevel.transform);
                    secretBeachGatekeeperB = GameObject.Instantiate(secretBeachLevel.transform.GetChild(1).gameObject, secretBeachGatekeeper.transform);
                    SetNewLevelSprite(secretBeachSky, locationPath, "SecretBeachUp.PNG", 2048, 1729);
                    SetNewLevelSprite(secretBeachFlash, locationPath, "Flash.PNG", 2048, 1729);
                    SetNewLevelSprite(secretBeachGatekeeper, locationPath, "Gatekeeper.PNG", 1024, 783);
                    SetNewLevelSprite(secretBeachGatekeeperB, locationPath, "GatekeeperB.PNG", 1024, 783);
                    secretBeachSky.name = "Sky";
                    secretBeachFlash.name = "Flash";
                    secretBeachGatekeeper.name = "Gatekeeper";
                    secretBeachGatekeeperB.name = "Portal";
                    secretBeachSky.transform.position = new Vector2(secretBeachSky.transform.position.x, 15);
                    secretBeachFlash.transform.position = new Vector2(secretBeachFlash.transform.position.x, 15);
                    secretBeachGatekeeper.transform.position = new Vector2(secretBeachSky.transform.position.x, 18);
                    secretBeachLevel.transform.GetChild(1).GetComponent<MoveRelative2Mouse>().enabled = false;
                    secretBeachSky.GetComponent<MoveRelative2Mouse>().enabled = false;
                    secretBeachFlash.GetComponent<MoveRelative2Mouse>().enabled = false;
                    secretBeachFlash.GetComponent<SpriteRenderer>().sortingOrder = -9;
                    secretBeachGatekeeper.GetComponent<MoveRelative2Mouse>().enabled = false;
                    secretBeachGatekeeper.GetComponent<SpriteRenderer>().sortingOrder = -10;
                    secretBeachGatekeeperB.GetComponent<MoveRelative2Mouse>().enabled = false;
                    secretBeachGatekeeperB.GetComponent<SpriteRenderer>().sortingOrder = -11;
                    secretBeachFlash.SetActive(false);
                    secretBeachGatekeeperB.SetActive(false);
                    secretBeachGatekeeperB.GetComponent<SpriteRenderer>().color = new Color(secretBeachGatekeeperB.GetComponent<SpriteRenderer>().color.r,
                        secretBeachGatekeeperB.GetComponent<SpriteRenderer>().color.g, secretBeachGatekeeperB.GetComponent<SpriteRenderer>().color.b, 0);
                    secretBeachGatekeeperB.AddComponent<FadeInSprite2>();
                    secretBeachGatekeeperB.GetComponent<FadeInSprite2>().fadeInDuration = 1f;
                    secretBeachFlash.AddComponent<FadeOutSprite>();
                    originLevelPos = secretBeachLevel.transform.position;
                    Destroy(secretBeachRoomTalk.transform.GetChild(1).gameObject);
                    Destroy(secretBeachRoomTalk.transform.GetChild(2).gameObject);
                    Destroy(secretBeachRoomTalk.transform.GetChild(3).gameObject);
                    Destroy(secretBeachRoomTalk.transform.GetChild(4).gameObject);
                    Destroy(secretBeachRoomTalk.transform.GetChild(5).gameObject);
                    Destroy(secretBeachRoomTalk.transform.GetChild(6).gameObject);
                    Destroy(secretBeachRoomTalk.transform.GetChild(7).gameObject);

                    dialogueBeachMain = dialogueBundle.LoadAsset<GameObject>("SBDialogueMain");
                    dialogueBeachMainDialogue = dialogueBeachMain.GetComponent<Dialogue>();
                    dialogueBeachMainDialogue.Story.Content.DialogueSkin = baseDialogueObject.GetComponent<Dialogue>().Story.Content.DialogueSkin;
                    dialogueBeachMainInstance = GameObject.Instantiate(dialogueBeachMain, beach.transform);
                    beachMainDialogueScene1 = dialogueBeachMainInstance.transform.Find("Scene1").gameObject;
                    beachMainDialogueScene2 = dialogueBeachMainInstance.transform.Find("Scene2").gameObject;
                    beachMainDialogueScene3 = dialogueBeachMainInstance.transform.Find("Scene3").gameObject;
                    beachMainDialogueScene4 = dialogueBeachMainInstance.transform.Find("Scene4").gameObject;
                    beachMainDialogueActivator = dialogueBeachMainInstance.transform.Find("DialogueActivator").gameObject;
                    beachMainDialogueFinisher = dialogueBeachMainInstance.transform.Find("DialogueFinisher").gameObject;
                    beachMainDialogueSpriteFocus = dialogueBeachMainInstance.transform.Find("SpriteFocus").gameObject;
                    dialogueBeachMainGK = dialogueBundle.LoadAsset<GameObject>("SBDialogueMainGatekeeper");
                    dialogueBeachMainDialogueGK = dialogueBeachMainGK.GetComponent<Dialogue>();
                    dialogueBeachMainDialogueGK.Story.Content.DialogueSkin = baseDialogueObject.GetComponent<Dialogue>().Story.Content.DialogueSkin;
                    dialogueBeachMainGKInstance = GameObject.Instantiate(dialogueBeachMainGK, beach.transform);
                    beachMainDialogueGKScene1 = dialogueBeachMainGKInstance.transform.Find("Scene1").gameObject;
                    beachMainDialogueGKScene2 = dialogueBeachMainGKInstance.transform.Find("Scene2").gameObject;
                    beachMainDialogueGKScene3 = dialogueBeachMainGKInstance.transform.Find("Scene3").gameObject;
                    beachMainDialogueGKScene4 = dialogueBeachMainGKInstance.transform.Find("Scene4").gameObject;
                    beachMainDialogueGKScene5 = dialogueBeachMainGKInstance.transform.Find("Scene5").gameObject;
                    beachMainDialogueGKBlinkActivator = dialogueBeachMainGKInstance.transform.Find("MouthActivator").gameObject;
                    beachMainDialogueGKActivator = dialogueBeachMainGKInstance.transform.Find("DialogueActivator").gameObject;
                    beachMainDialogueGKFinisher = dialogueBeachMainGKInstance.transform.Find("DialogueFinisher").gameObject;
                    beachMainDialogueGKSpriteFocus = dialogueBeachMainGKInstance.transform.Find("SpriteFocus").gameObject;
                    Logger.LogInfo("Secret Beach ready.");
                    #endregion

                    #region Anis
                    anisSwim = CreateNewBust("Anis", bustPath, "Anis\\AnisSwim00.PNG", "Anis\\AnisBlink.PNG", "Anis\\AnisSwim00Mask.PNG", "Anis\\AnisMouth");
                    anisSwimMBase = anisSwim.transform.Find("MBase1").gameObject;
                    anisBeachScene1 = CreateNewPicScene("AnisBeachScene01", scenePath + "Anis\\AnisBeachScene01.PNG");
                    anisBeachScene2 = CreateNewPicScene("AnisBeachScene02", scenePath + "Anis\\AnisBeachScene02.PNG");
                    anisBeachScene3 = CreateNewPicScene("AnisBeachScene03", scenePath + "Anis\\AnisBeachScene03.PNG");
                    anisBeachScene4 = CreateNewPicScene("AnisBeachScene04", scenePath + "Anis\\AnisBeachScene04.PNG");
                    anisSwimWetParticles = GameObject.Instantiate(wetParticleSystem, anisSwimMBase.transform);
                    anisSwimWetParticles.SetActive(false);
                    if (PlayerPrefs.GetInt(loadedSaveFile + "_MPE_Voyeur_SeenAnis") != 1) { totalVoyeurBusts.Add((anisSwim, 4f)); }
                    Logger.LogInfo("Anis bust ready!");

                    dialogueAnisBeach01 = dialogueBundle.LoadAsset<GameObject>("AnisDialogueBeach01");
                    dialogueAnisBeach01Dialogue = dialogueAnisBeach01.GetComponent<Dialogue>();
                    dialogueAnisBeach01Dialogue.Story.Content.DialogueSkin = baseDialogueObject.GetComponent<Dialogue>().Story.Content.DialogueSkin;
                    dialogueAnisBeach01Instance = GameObject.Instantiate(dialogueAnisBeach01, beach.transform);
                    if (PlayerPrefs.GetInt(loadedSaveFile + "_MPE_Voyeur_SeenAnis") != 1) { totalVoyeurDialogues.Add(dialogueAnisBeach01Instance); }
                    anisBeachDialogueScene1 = dialogueAnisBeach01Instance.transform.Find("Scene1").gameObject;
                    anisBeachDialogueScene2 = dialogueAnisBeach01Instance.transform.Find("Scene2").gameObject;
                    anisBeachDialogueScene3 = dialogueAnisBeach01Instance.transform.Find("Scene3").gameObject;
                    anisBeachDialogueScene4 = dialogueAnisBeach01Instance.transform.Find("Scene4").gameObject;
                    anisBeachDialogueActivator = dialogueAnisBeach01Instance.transform.Find("DialogueActivator").gameObject;
                    anisBeachDialogueFinisher = dialogueAnisBeach01Instance.transform.Find("DialogueFinisher").gameObject;
                    anisBeachDialogueSpriteFocus = dialogueAnisBeach01Instance.transform.Find("SpriteFocus").gameObject;
                    Logger.LogInfo("Anis Dialogue ready!");
                    #endregion
                    #region Frima
                    frimaSwim = CreateNewBust("Frima", bustPath, "Frima\\FrimaSwim00.PNG", "Frima\\FrimaBlink.PNG", "Frima\\FrimaSwim00Mask.PNG", "Frima\\FrimaMouth");
                    frimaSwimMBase = frimaSwim.transform.Find("MBase1").gameObject;
                    frimaBeachScene1 = CreateNewPicScene("FrimaBeachScene01", scenePath + "Frima\\FrimaBeachScene01.PNG");
                    frimaBeachScene2 = CreateNewPicScene("FrimaBeachScene02", scenePath + "Frima\\FrimaBeachScene02.PNG");
                    frimaBeachScene3 = CreateNewPicScene("FrimaBeachScene03", scenePath + "Frima\\FrimaBeachScene03.PNG");
                    frimaBeachScene4 = CreateNewPicScene("FrimaBeachScene04", scenePath + "Frima\\FrimaBeachScene04.PNG");
                    frimaSwimWetParticles = GameObject.Instantiate(wetParticleSystem, frimaSwimMBase.transform);
                    frimaSwimWetParticles.SetActive(false);
                    if (PlayerPrefs.GetInt(loadedSaveFile + "_MPE_Voyeur_SeenFrima") != 1) { totalVoyeurBusts.Add((frimaSwim, 4f)); }
                    Logger.LogInfo("Frima bust ready!");

                    dialogueFrimaBeach01 = dialogueBundle.LoadAsset<GameObject>("FrimaDialogueBeach01");
                    dialogueFrimaBeach01Dialogue = dialogueFrimaBeach01.GetComponent<Dialogue>();
                    dialogueFrimaBeach01Dialogue.Story.Content.DialogueSkin = baseDialogueObject.GetComponent<Dialogue>().Story.Content.DialogueSkin;
                    dialogueFrimaBeach01Instance = GameObject.Instantiate(dialogueFrimaBeach01, beach.transform);
                    if (PlayerPrefs.GetInt(loadedSaveFile + "_MPE_Voyeur_SeenFrima") != 1) { totalVoyeurDialogues.Add(dialogueFrimaBeach01Instance); }
                    frimaBeachDialogueScene1 = dialogueFrimaBeach01Instance.transform.Find("Scene1").gameObject;
                    frimaBeachDialogueScene2 = dialogueFrimaBeach01Instance.transform.Find("Scene2").gameObject;
                    frimaBeachDialogueScene3 = dialogueFrimaBeach01Instance.transform.Find("Scene3").gameObject;
                    frimaBeachDialogueScene4 = dialogueFrimaBeach01Instance.transform.Find("Scene4").gameObject;
                    frimaBeachDialogueActivator = dialogueFrimaBeach01Instance.transform.Find("DialogueActivator").gameObject;
                    frimaBeachDialogueFinisher = dialogueFrimaBeach01Instance.transform.Find("DialogueFinisher").gameObject;
                    frimaBeachDialogueSpriteFocus = dialogueFrimaBeach01Instance.transform.Find("SpriteFocus").gameObject;
                    Logger.LogInfo("Frima Dialogue ready!");
                    #endregion
                    #region Guilty
                    guiltySwim = CreateNewBust("Guilty", bustPath, "Guilty\\GuiltySwim00.PNG", "Guilty\\GuiltyBlink.PNG", "Guilty\\GuiltySwim00Mask.PNG", "Guilty\\GuiltyMouth");
                    guiltySwimMBase = guiltySwim.transform.Find("MBase1").gameObject;
                    guiltyBeachScene1 = CreateNewPicScene("GuiltyBeachScene01", scenePath + "Guilty\\GuiltyBeachScene01.PNG");
                    guiltyBeachScene2 = CreateNewPicScene("GuiltyBeachScene02", scenePath + "Guilty\\GuiltyBeachScene02.PNG");
                    guiltyBeachScene3 = CreateNewPicScene("GuiltyBeachScene03", scenePath + "Guilty\\GuiltyBeachScene03.PNG");
                    guiltyBeachScene4 = CreateNewPicScene("GuiltyBeachScene04", scenePath + "Guilty\\GuiltyBeachScene04.PNG");
                    guiltySwimWetParticles = GameObject.Instantiate(wetParticleSystem, guiltySwimMBase.transform);
                    guiltySwimWetParticles.SetActive(false);
                    if (PlayerPrefs.GetInt(loadedSaveFile + "_MPE_Voyeur_SeenGuilty") != 1) { totalVoyeurBusts.Add((guiltySwim, 4f)); }
                    Logger.LogInfo("Guilty bust ready!");

                    dialogueGuiltyBeach01 = dialogueBundle.LoadAsset<GameObject>("GuiltyDialogueBeach01");
                    dialogueGuiltyBeach01Dialogue = dialogueGuiltyBeach01.GetComponent<Dialogue>();
                    dialogueGuiltyBeach01Dialogue.Story.Content.DialogueSkin = baseDialogueObject.GetComponent<Dialogue>().Story.Content.DialogueSkin;
                    dialogueGuiltyBeach01Instance = GameObject.Instantiate(dialogueGuiltyBeach01, beach.transform);
                    if (PlayerPrefs.GetInt(loadedSaveFile + "_MPE_Voyeur_SeenGuilty") != 1) { totalVoyeurDialogues.Add(dialogueGuiltyBeach01Instance); }
                    guiltyBeachDialogueScene1 = dialogueGuiltyBeach01Instance.transform.Find("Scene1").gameObject;
                    guiltyBeachDialogueScene2 = dialogueGuiltyBeach01Instance.transform.Find("Scene2").gameObject;
                    guiltyBeachDialogueScene3 = dialogueGuiltyBeach01Instance.transform.Find("Scene3").gameObject;
                    guiltyBeachDialogueScene4 = dialogueGuiltyBeach01Instance.transform.Find("Scene4").gameObject;
                    guiltyBeachDialogueActivator = dialogueGuiltyBeach01Instance.transform.Find("DialogueActivator").gameObject;
                    guiltyBeachDialogueFinisher = dialogueGuiltyBeach01Instance.transform.Find("DialogueFinisher").gameObject;
                    guiltyBeachDialogueSpriteFocus = dialogueGuiltyBeach01Instance.transform.Find("SpriteFocus").gameObject;
                    Logger.LogInfo("Guilty Dialogue ready!");
                    #endregion
                    #region Helm
                    helmSwim = CreateNewBust("Helm", bustPath, "Helm\\HelmSwim00.PNG", "Helm\\HelmBlink.PNG", "Helm\\HelmSwim00.PNG", "Helm\\HelmMouth");
                    helmSwimMBase = helmSwim.transform.Find("MBase1").gameObject;
                    helmBeachScene1 = CreateNewPicScene("HelmBeachScene01", scenePath + "Helm\\HelmBeachScene01.PNG");
                    helmBeachScene2 = CreateNewPicScene("HelmBeachScene02", scenePath + "Helm\\HelmBeachScene02.PNG");
                    helmBeachScene3 = CreateNewPicScene("HelmBeachScene03", scenePath + "Helm\\HelmBeachScene03.PNG");
                    helmBeachScene4 = CreateNewPicScene("HelmBeachScene04", scenePath + "Helm\\HelmBeachScene04.PNG");
                    helmSwimWetParticles = GameObject.Instantiate(wetParticleSystem, helmSwimMBase.transform);
                    helmSwimWetParticles.SetActive(false);
                    if (PlayerPrefs.GetInt(loadedSaveFile + "_MPE_Voyeur_SeenHelm") != 1) { totalVoyeurBusts.Add((helmSwim, 4f)); }
                    Logger.LogInfo("Helm bust ready!");

                    dialogueHelmBeach01 = dialogueBundle.LoadAsset<GameObject>("HelmDialogueBeach01");
                    dialogueHelmBeach01Dialogue = dialogueHelmBeach01.GetComponent<Dialogue>();
                    dialogueHelmBeach01Dialogue.Story.Content.DialogueSkin = baseDialogueObject.GetComponent<Dialogue>().Story.Content.DialogueSkin;
                    dialogueHelmBeach01Instance = GameObject.Instantiate(dialogueHelmBeach01, beach.transform);
                    if (PlayerPrefs.GetInt(loadedSaveFile + "_MPE_Voyeur_SeenHelm") != 1) { totalVoyeurDialogues.Add(dialogueHelmBeach01Instance); }
                    helmBeachDialogueScene1 = dialogueHelmBeach01Instance.transform.Find("Scene1").gameObject;
                    helmBeachDialogueScene2 = dialogueHelmBeach01Instance.transform.Find("Scene2").gameObject;
                    helmBeachDialogueScene3 = dialogueHelmBeach01Instance.transform.Find("Scene3").gameObject;
                    helmBeachDialogueScene4 = dialogueHelmBeach01Instance.transform.Find("Scene4").gameObject;
                    helmBeachDialogueActivator = dialogueHelmBeach01Instance.transform.Find("DialogueActivator").gameObject;
                    helmBeachDialogueFinisher = dialogueHelmBeach01Instance.transform.Find("DialogueFinisher").gameObject;
                    helmBeachDialogueSpriteFocus = dialogueHelmBeach01Instance.transform.Find("SpriteFocus").gameObject;
                    Logger.LogInfo("Helm Dialogue ready!");
                    #endregion
                    #region Maiden
                    maidenSwim = CreateNewBust("Maiden", bustPath, "Maiden\\MaidenSwim00.PNG", "Maiden\\MaidenBlink.PNG", "Maiden\\MaidenSwim00Mask.PNG", "Maiden\\MaidenMouth");
                    maidenSwimMBase = maidenSwim.transform.Find("MBase1").gameObject;
                    maidenBeachScene1 = CreateNewPicScene("MaidenBeachScene01", scenePath + "Maiden\\MaidenBeachScene01.PNG");
                    maidenBeachScene2 = CreateNewPicScene("MaidenBeachScene02", scenePath + "Maiden\\MaidenBeachScene02.PNG");
                    maidenBeachScene3 = CreateNewPicScene("MaidenBeachScene03", scenePath + "Maiden\\MaidenBeachScene03.PNG");
                    maidenBeachScene4 = CreateNewPicScene("MaidenBeachScene04", scenePath + "Maiden\\MaidenBeachScene04.PNG");
                    maidenSwimWetParticles = GameObject.Instantiate(wetParticleSystem, maidenSwimMBase.transform);
                    maidenSwimWetParticles.SetActive(false);
                    if (PlayerPrefs.GetInt(loadedSaveFile + "_MPE_Voyeur_SeenMaiden") != 1) { totalVoyeurBusts.Add((maidenSwim, 4f)); }
                    Logger.LogInfo("Maiden bust ready!");

                    dialogueMaidenBeach01 = dialogueBundle.LoadAsset<GameObject>("MaidenDialogueBeach01");
                    dialogueMaidenBeach01Dialogue = dialogueMaidenBeach01.GetComponent<Dialogue>();
                    dialogueMaidenBeach01Dialogue.Story.Content.DialogueSkin = baseDialogueObject.GetComponent<Dialogue>().Story.Content.DialogueSkin;
                    dialogueMaidenBeach01Instance = GameObject.Instantiate(dialogueMaidenBeach01, beach.transform);
                    if (PlayerPrefs.GetInt(loadedSaveFile + "_MPE_Voyeur_SeenMaiden") != 1) { totalVoyeurDialogues.Add(dialogueMaidenBeach01Instance); }
                    maidenBeachDialogueScene1 = dialogueMaidenBeach01Instance.transform.Find("Scene1").gameObject;
                    maidenBeachDialogueScene2 = dialogueMaidenBeach01Instance.transform.Find("Scene2").gameObject;
                    maidenBeachDialogueScene3 = dialogueMaidenBeach01Instance.transform.Find("Scene3").gameObject;
                    maidenBeachDialogueScene4 = dialogueMaidenBeach01Instance.transform.Find("Scene4").gameObject;
                    maidenBeachDialogueActivator = dialogueMaidenBeach01Instance.transform.Find("DialogueActivator").gameObject;
                    maidenBeachDialogueFinisher = dialogueMaidenBeach01Instance.transform.Find("DialogueFinisher").gameObject;
                    maidenBeachDialogueSpriteFocus = dialogueMaidenBeach01Instance.transform.Find("SpriteFocus").gameObject;
                    Logger.LogInfo("Maiden Dialogue ready!");
                    #endregion
                    #region Mary
                    marySwim = CreateNewBust("Mary", bustPath, "Mary\\MarySwim00.PNG", "Mary\\MaryBlink.PNG", "Mary\\MarySwim00Mask.PNG", "Mary\\MaryMouth");
                    marySwimMBase = marySwim.transform.Find("MBase1").gameObject;
                    maryBeachScene1 = CreateNewPicScene("MaryBeachScene01", scenePath + "Mary\\MaryBeachScene01.PNG");
                    maryBeachScene2 = CreateNewPicScene("MaryBeachScene02", scenePath + "Mary\\MaryBeachScene02.PNG");
                    maryBeachScene3 = CreateNewPicScene("MaryBeachScene03", scenePath + "Mary\\MaryBeachScene03.PNG");
                    maryBeachScene4 = CreateNewPicScene("MaryBeachScene04", scenePath + "Mary\\MaryBeachScene04.PNG");
                    marySwimWetParticles = GameObject.Instantiate(wetParticleSystem, marySwimMBase.transform);
                    marySwimWetParticles.SetActive(false);
                    if (PlayerPrefs.GetInt(loadedSaveFile + "_MPE_Voyeur_SeenMary") != 1) { totalVoyeurBusts.Add((marySwim, 4f)); }
                    Logger.LogInfo("Mary bust ready!");

                    dialogueMaryBeach01 = dialogueBundle.LoadAsset<GameObject>("MaryDialogueBeach01");
                    dialogueMaryBeach01Dialogue = dialogueMaryBeach01.GetComponent<Dialogue>();
                    dialogueMaryBeach01Dialogue.Story.Content.DialogueSkin = baseDialogueObject.GetComponent<Dialogue>().Story.Content.DialogueSkin;
                    dialogueMaryBeach01Instance = GameObject.Instantiate(dialogueMaryBeach01, beach.transform);
                    if (PlayerPrefs.GetInt(loadedSaveFile + "_MPE_Voyeur_SeenMary") != 1) { totalVoyeurDialogues.Add(dialogueMaryBeach01Instance); }
                    maryBeachDialogueScene1 = dialogueMaryBeach01Instance.transform.Find("Scene1").gameObject;
                    maryBeachDialogueScene2 = dialogueMaryBeach01Instance.transform.Find("Scene2").gameObject;
                    maryBeachDialogueScene3 = dialogueMaryBeach01Instance.transform.Find("Scene3").gameObject;
                    maryBeachDialogueScene4 = dialogueMaryBeach01Instance.transform.Find("Scene4").gameObject;
                    maryBeachDialogueActivator = dialogueMaryBeach01Instance.transform.Find("DialogueActivator").gameObject;
                    maryBeachDialogueFinisher = dialogueMaryBeach01Instance.transform.Find("DialogueFinisher").gameObject;
                    maryBeachDialogueSpriteFocus = dialogueMaryBeach01Instance.transform.Find("SpriteFocus").gameObject;
                    Logger.LogInfo("Mary Dialogue ready!");
                    #endregion
                    #region Mast
                    mastSwim = CreateNewBust("Mast", bustPath, "Mast\\MastSwim00.PNG", "Mast\\MastBlink.PNG", "Mast\\MastSwim00Mask.PNG", "Mast\\MastMouth");
                    mastSwimMBase = mastSwim.transform.Find("MBase1").gameObject;
                    mastBeachScene1 = CreateNewPicScene("MastBeachScene01", scenePath + "Mast\\MastBeachScene01.PNG");
                    mastBeachScene2 = CreateNewPicScene("MastBeachScene02", scenePath + "Mast\\MastBeachScene02.PNG");
                    mastBeachScene3 = CreateNewPicScene("MastBeachScene03", scenePath + "Mast\\MastBeachScene03.PNG");
                    mastBeachScene4 = CreateNewPicScene("MastBeachScene04", scenePath + "Mast\\MastBeachScene04.PNG");
                    mastSwimWetParticles = GameObject.Instantiate(wetParticleSystem, mastSwimMBase.transform);
                    mastSwimWetParticles.SetActive(false);
                    if (PlayerPrefs.GetInt(loadedSaveFile + "_MPE_Voyeur_SeenMast") != 1) { totalVoyeurBusts.Add((mastSwim, 4f)); }
                    Logger.LogInfo("Mast bust ready!");

                    dialogueMastBeach01 = dialogueBundle.LoadAsset<GameObject>("MastDialogueBeach01");
                    dialogueMastBeach01Dialogue = dialogueMastBeach01.GetComponent<Dialogue>();
                    dialogueMastBeach01Dialogue.Story.Content.DialogueSkin = baseDialogueObject.GetComponent<Dialogue>().Story.Content.DialogueSkin;
                    dialogueMastBeach01Instance = GameObject.Instantiate(dialogueMastBeach01, beach.transform);
                    if (PlayerPrefs.GetInt(loadedSaveFile + "_MPE_Voyeur_SeenMast") != 1) { totalVoyeurDialogues.Add(dialogueMastBeach01Instance); }
                    mastBeachDialogueScene1 = dialogueMastBeach01Instance.transform.Find("Scene1").gameObject;
                    mastBeachDialogueScene2 = dialogueMastBeach01Instance.transform.Find("Scene2").gameObject;
                    mastBeachDialogueScene3 = dialogueMastBeach01Instance.transform.Find("Scene3").gameObject;
                    mastBeachDialogueScene4 = dialogueMastBeach01Instance.transform.Find("Scene4").gameObject;
                    mastBeachDialogueActivator = dialogueMastBeach01Instance.transform.Find("DialogueActivator").gameObject;
                    mastBeachDialogueFinisher = dialogueMastBeach01Instance.transform.Find("DialogueFinisher").gameObject;
                    mastBeachDialogueSpriteFocus = dialogueMastBeach01Instance.transform.Find("SpriteFocus").gameObject;
                    Logger.LogInfo("Mast Dialogue ready!");
                    #endregion
                    #region Neon
                    neonSwim = CreateNewBust("Neon", bustPath, "Neon\\NeonSwim00.PNG", "Neon\\NeonBlink.PNG", "Neon\\NeonSwim00Mask.PNG", "Neon\\NeonMouth");
                    neonSwimMBase = neonSwim.transform.Find("MBase1").gameObject;
                    neonBeachScene1 = CreateNewPicScene("NeonBeachScene01", scenePath + "Neon\\NeonBeachScene01.PNG");
                    neonBeachScene2 = CreateNewPicScene("NeonBeachScene02", scenePath + "Neon\\NeonBeachScene02.PNG");
                    neonBeachScene3 = CreateNewPicScene("NeonBeachScene03", scenePath + "Neon\\NeonBeachScene03.PNG");
                    neonBeachScene4 = CreateNewPicScene("NeonBeachScene04", scenePath + "Neon\\NeonBeachScene04.PNG");
                    neonSwimWetParticles = GameObject.Instantiate(wetParticleSystem, neonSwimMBase.transform);
                    neonSwimWetParticles.SetActive(false);
                    if (PlayerPrefs.GetInt(loadedSaveFile + "_MPE_Voyeur_SeenNeon") != 1) { totalVoyeurBusts.Add((neonSwim, 4f)); }
                    Logger.LogInfo("Neon bust ready!");

                    dialogueNeonBeach01 = dialogueBundle.LoadAsset<GameObject>("NeonDialogueBeach01");
                    dialogueNeonBeach01Dialogue = dialogueNeonBeach01.GetComponent<Dialogue>();
                    dialogueNeonBeach01Dialogue.Story.Content.DialogueSkin = baseDialogueObject.GetComponent<Dialogue>().Story.Content.DialogueSkin;
                    dialogueNeonBeach01Instance = GameObject.Instantiate(dialogueNeonBeach01, beach.transform);
                    if (PlayerPrefs.GetInt(loadedSaveFile + "_MPE_Voyeur_SeenNeon") != 1) { totalVoyeurDialogues.Add(dialogueNeonBeach01Instance); }
                    neonBeachDialogueScene1 = dialogueNeonBeach01Instance.transform.Find("Scene1").gameObject;
                    neonBeachDialogueScene2 = dialogueNeonBeach01Instance.transform.Find("Scene2").gameObject;
                    neonBeachDialogueScene3 = dialogueNeonBeach01Instance.transform.Find("Scene3").gameObject;
                    neonBeachDialogueScene4 = dialogueNeonBeach01Instance.transform.Find("Scene4").gameObject;
                    neonBeachDialogueActivator = dialogueNeonBeach01Instance.transform.Find("DialogueActivator").gameObject;
                    neonBeachDialogueFinisher = dialogueNeonBeach01Instance.transform.Find("DialogueFinisher").gameObject;
                    neonBeachDialogueSpriteFocus = dialogueNeonBeach01Instance.transform.Find("SpriteFocus").gameObject;
                    Logger.LogInfo("Neon Dialogue ready!");
                    #endregion
                    #region Pepper
                    pepperSwim = CreateNewBust("Pepper", bustPath, "Pepper\\PepperSwim00.PNG", "Pepper\\PepperBlink.PNG", "Pepper\\PepperSwim00Mask.PNG", "Pepper\\PepperMouth");
                    pepperSwimMBase = pepperSwim.transform.Find("MBase1").gameObject;
                    pepperBeachScene1 = CreateNewPicScene("PepperBeachScene01", scenePath + "Pepper\\PepperBeachScene01.PNG");
                    pepperBeachScene2 = CreateNewPicScene("PepperBeachScene02", scenePath + "Pepper\\PepperBeachScene02.PNG");
                    pepperBeachScene3 = CreateNewPicScene("PepperBeachScene03", scenePath + "Pepper\\PepperBeachScene03.PNG");
                    pepperBeachScene4 = CreateNewPicScene("PepperBeachScene04", scenePath + "Pepper\\PepperBeachScene04.PNG");
                    pepperSwimWetParticles = GameObject.Instantiate(wetParticleSystem, pepperSwimMBase.transform);
                    pepperSwimWetParticles.SetActive(false);
                    if (PlayerPrefs.GetInt(loadedSaveFile + "_MPE_Voyeur_SeenPepper") != 1) { totalVoyeurBusts.Add((pepperSwim, 4f)); }
                    Logger.LogInfo("Pepper bust ready!");

                    dialoguePepperBeach01 = dialogueBundle.LoadAsset<GameObject>("PepperDialogueBeach01");
                    dialoguePepperBeach01Dialogue = dialoguePepperBeach01.GetComponent<Dialogue>();
                    dialoguePepperBeach01Dialogue.Story.Content.DialogueSkin = baseDialogueObject.GetComponent<Dialogue>().Story.Content.DialogueSkin;
                    dialoguePepperBeach01Instance = GameObject.Instantiate(dialoguePepperBeach01, beach.transform);
                    if (PlayerPrefs.GetInt(loadedSaveFile + "_MPE_Voyeur_SeenPepper") != 1) { totalVoyeurDialogues.Add(dialoguePepperBeach01Instance); }
                    pepperBeachDialogueScene1 = dialoguePepperBeach01Instance.transform.Find("Scene1").gameObject;
                    pepperBeachDialogueScene2 = dialoguePepperBeach01Instance.transform.Find("Scene2").gameObject;
                    pepperBeachDialogueScene3 = dialoguePepperBeach01Instance.transform.Find("Scene3").gameObject;
                    pepperBeachDialogueScene4 = dialoguePepperBeach01Instance.transform.Find("Scene4").gameObject;
                    pepperBeachDialogueActivator = dialoguePepperBeach01Instance.transform.Find("DialogueActivator").gameObject;
                    pepperBeachDialogueFinisher = dialoguePepperBeach01Instance.transform.Find("DialogueFinisher").gameObject;
                    pepperBeachDialogueSpriteFocus = dialoguePepperBeach01Instance.transform.Find("SpriteFocus").gameObject;
                    Logger.LogInfo("Pepper Dialogue ready!");
                    #endregion
                    #region Rapi
                    rapiSwim = CreateNewBust("Rapi", bustPath, "Rapi\\RapiSwim00.PNG", "Rapi\\RapiBlink.PNG", "Rapi\\RapiSwim00Mask.PNG", "Rapi\\RapiMouth");
                    rapiSwimMBase = rapiSwim.transform.Find("MBase1").gameObject;
                    rapiBeachScene1 = CreateNewPicScene("RapiBeachScene01", scenePath + "Rapi\\RapiBeachScene01.PNG");
                    rapiBeachScene2 = CreateNewPicScene("RapiBeachScene02", scenePath + "Rapi\\RapiBeachScene02.PNG");
                    rapiBeachScene3 = CreateNewPicScene("RapiBeachScene03", scenePath + "Rapi\\RapiBeachScene03.PNG");
                    rapiBeachScene4 = CreateNewPicScene("RapiBeachScene04", scenePath + "Rapi\\RapiBeachScene04.PNG");
                    rapiSwimWetParticles = GameObject.Instantiate(wetParticleSystem, rapiSwimMBase.transform);
                    rapiSwimWetParticles.SetActive(false);
                    if (PlayerPrefs.GetInt(loadedSaveFile + "_MPE_Voyeur_SeenRapi") != 1) { totalVoyeurBusts.Add((rapiSwim, 4f)); }
                    Logger.LogInfo("Rapi bust ready!");

                    dialogueRapiBeach01 = dialogueBundle.LoadAsset<GameObject>("RapiDialogueBeach01");
                    dialogueRapiBeach01Dialogue = dialogueRapiBeach01.GetComponent<Dialogue>();
                    dialogueRapiBeach01Dialogue.Story.Content.DialogueSkin = baseDialogueObject.GetComponent<Dialogue>().Story.Content.DialogueSkin;
                    dialogueRapiBeach01Instance = GameObject.Instantiate(dialogueRapiBeach01, beach.transform);
                    if (PlayerPrefs.GetInt(loadedSaveFile + "_MPE_Voyeur_SeenRapi") != 1) { totalVoyeurDialogues.Add(dialogueRapiBeach01Instance); }
                    rapiBeachDialogueScene1 = dialogueRapiBeach01Instance.transform.Find("Scene1").gameObject;
                    rapiBeachDialogueScene2 = dialogueRapiBeach01Instance.transform.Find("Scene2").gameObject;
                    rapiBeachDialogueScene3 = dialogueRapiBeach01Instance.transform.Find("Scene3").gameObject;
                    rapiBeachDialogueScene4 = dialogueRapiBeach01Instance.transform.Find("Scene4").gameObject;
                    rapiBeachDialogueActivator = dialogueRapiBeach01Instance.transform.Find("DialogueActivator").gameObject;
                    rapiBeachDialogueFinisher = dialogueRapiBeach01Instance.transform.Find("DialogueFinisher").gameObject;
                    rapiBeachDialogueSpriteFocus = dialogueRapiBeach01Instance.transform.Find("SpriteFocus").gameObject;
                    Logger.LogInfo("Rapi Dialogue ready!");
                    #endregion
                    #region Rosanna
                    rosannaSwim = CreateNewBust("Rosanna", bustPath, "Rosanna\\RosannaSwim00.PNG", "Rosanna\\RosannaBlink.PNG", "Rosanna\\RosannaSwim00Mask.PNG", "Rosanna\\RosannaMouth");
                    rosannaSwimMBase = rosannaSwim.transform.Find("MBase1").gameObject;
                    rosannaBeachScene1 = CreateNewPicScene("RosannaBeachScene01", scenePath + "Rosanna\\RosannaBeachScene01.PNG");
                    rosannaBeachScene2 = CreateNewPicScene("RosannaBeachScene02", scenePath + "Rosanna\\RosannaBeachScene02.PNG");
                    rosannaBeachScene3 = CreateNewPicScene("RosannaBeachScene03", scenePath + "Rosanna\\RosannaBeachScene03.PNG");
                    rosannaBeachScene4 = CreateNewPicScene("RosannaBeachScene04", scenePath + "Rosanna\\RosannaBeachScene04.PNG");
                    rosannaSwimWetParticles = GameObject.Instantiate(wetParticleSystem, rosannaSwimMBase.transform);
                    rosannaSwimWetParticles.SetActive(false);
                    if (PlayerPrefs.GetInt(loadedSaveFile + "_MPE_Voyeur_SeenRosanna") != 1) { totalVoyeurBusts.Add((rosannaSwim, 4f)); }
                    Logger.LogInfo("Rosanna bust ready!");

                    dialogueRosannaBeach01 = dialogueBundle.LoadAsset<GameObject>("RosannaDialogueBeach01");
                    dialogueRosannaBeach01Dialogue = dialogueRosannaBeach01.GetComponent<Dialogue>();
                    dialogueRosannaBeach01Dialogue.Story.Content.DialogueSkin = baseDialogueObject.GetComponent<Dialogue>().Story.Content.DialogueSkin;
                    dialogueRosannaBeach01Instance = GameObject.Instantiate(dialogueRosannaBeach01, beach.transform);
                    if (PlayerPrefs.GetInt(loadedSaveFile + "_MPE_Voyeur_SeenRosanna") != 1) { totalVoyeurDialogues.Add(dialogueRosannaBeach01Instance); }
                    rosannaBeachDialogueScene1 = dialogueRosannaBeach01Instance.transform.Find("Scene1").gameObject;
                    rosannaBeachDialogueScene2 = dialogueRosannaBeach01Instance.transform.Find("Scene2").gameObject;
                    rosannaBeachDialogueScene3 = dialogueRosannaBeach01Instance.transform.Find("Scene3").gameObject;
                    rosannaBeachDialogueScene4 = dialogueRosannaBeach01Instance.transform.Find("Scene4").gameObject;
                    rosannaBeachDialogueActivator = dialogueRosannaBeach01Instance.transform.Find("DialogueActivator").gameObject;
                    rosannaBeachDialogueFinisher = dialogueRosannaBeach01Instance.transform.Find("DialogueFinisher").gameObject;
                    rosannaBeachDialogueSpriteFocus = dialogueRosannaBeach01Instance.transform.Find("SpriteFocus").gameObject;
                    Logger.LogInfo("Rosanna Dialogue ready!");
                    #endregion
                    #region Sakura
                    sakuraSwim = CreateNewBust("Sakura", bustPath, "Sakura\\SakuraSwim00.PNG", "Sakura\\SakuraBlink.PNG", "Sakura\\SakuraSwim00Mask.PNG", "Sakura\\SakuraMouth");
                    sakuraSwimMBase = sakuraSwim.transform.Find("MBase1").gameObject;
                    sakuraBeachScene1 = CreateNewPicScene("SakuraBeachScene01", scenePath + "Sakura\\SakuraBeachScene01.PNG");
                    sakuraBeachScene2 = CreateNewPicScene("SakuraBeachScene02", scenePath + "Sakura\\SakuraBeachScene02.PNG");
                    sakuraBeachScene3 = CreateNewPicScene("SakuraBeachScene03", scenePath + "Sakura\\SakuraBeachScene03.PNG");
                    sakuraBeachScene4 = CreateNewPicScene("SakuraBeachScene04", scenePath + "Sakura\\SakuraBeachScene04.PNG");
                    sakuraSwimWetParticles = GameObject.Instantiate(wetParticleSystem, sakuraSwimMBase.transform);
                    sakuraSwimWetParticles.SetActive(false);
                    if (PlayerPrefs.GetInt(loadedSaveFile + "_MPE_Voyeur_SeenSakura") != 1) { totalVoyeurBusts.Add((sakuraSwim, 4f)); }
                    Logger.LogInfo("Sakura bust ready!");

                    dialogueSakuraBeach01 = dialogueBundle.LoadAsset<GameObject>("SakuraDialogueBeach01");
                    dialogueSakuraBeach01Dialogue = dialogueSakuraBeach01.GetComponent<Dialogue>();
                    dialogueSakuraBeach01Dialogue.Story.Content.DialogueSkin = baseDialogueObject.GetComponent<Dialogue>().Story.Content.DialogueSkin;
                    dialogueSakuraBeach01Instance = GameObject.Instantiate(dialogueSakuraBeach01, beach.transform);
                    if (PlayerPrefs.GetInt(loadedSaveFile + "_MPE_Voyeur_SeenSakura") != 1) { totalVoyeurDialogues.Add(dialogueSakuraBeach01Instance); }
                    sakuraBeachDialogueScene1 = dialogueSakuraBeach01Instance.transform.Find("Scene1").gameObject;
                    sakuraBeachDialogueScene2 = dialogueSakuraBeach01Instance.transform.Find("Scene2").gameObject;
                    sakuraBeachDialogueScene3 = dialogueSakuraBeach01Instance.transform.Find("Scene3").gameObject;
                    sakuraBeachDialogueScene4 = dialogueSakuraBeach01Instance.transform.Find("Scene4").gameObject;
                    sakuraBeachDialogueActivator = dialogueSakuraBeach01Instance.transform.Find("DialogueActivator").gameObject;
                    sakuraBeachDialogueFinisher = dialogueSakuraBeach01Instance.transform.Find("DialogueFinisher").gameObject;
                    sakuraBeachDialogueSpriteFocus = dialogueSakuraBeach01Instance.transform.Find("SpriteFocus").gameObject;
                    Logger.LogInfo("Sakura Dialogue ready!");
                    #endregion
                    #region Viper
                    viperSwim = CreateNewBust("Viper", bustPath, "Viper\\ViperSwim00.PNG", "Viper\\ViperBlink.PNG", "Viper\\ViperSwim00Mask.PNG", "Viper\\ViperMouth");
                    viperSwimMBase = viperSwim.transform.Find("MBase1").gameObject;
                    viperBeachScene1 = CreateNewPicScene("ViperBeachScene01", scenePath + "Viper\\ViperBeachScene01.PNG");
                    viperBeachScene2 = CreateNewPicScene("ViperBeachScene02", scenePath + "Viper\\ViperBeachScene02.PNG");
                    viperBeachScene3 = CreateNewPicScene("ViperBeachScene03", scenePath + "Viper\\ViperBeachScene03.PNG");
                    viperBeachScene4 = CreateNewPicScene("ViperBeachScene04", scenePath + "Viper\\ViperBeachScene04.PNG");
                    viperSwimWetParticles = GameObject.Instantiate(wetParticleSystem, viperSwimMBase.transform);
                    viperSwimWetParticles.SetActive(false);
                    if (PlayerPrefs.GetInt(loadedSaveFile + "_MPE_Voyeur_SeenViper") != 1) { totalVoyeurBusts.Add((viperSwim, 4f)); }
                    Logger.LogInfo("Viper bust ready!");

                    dialogueViperBeach01 = dialogueBundle.LoadAsset<GameObject>("ViperDialogueBeach01");
                    dialogueViperBeach01Dialogue = dialogueViperBeach01.GetComponent<Dialogue>();
                    dialogueViperBeach01Dialogue.Story.Content.DialogueSkin = baseDialogueObject.GetComponent<Dialogue>().Story.Content.DialogueSkin;
                    dialogueViperBeach01Instance = GameObject.Instantiate(dialogueViperBeach01, beach.transform);
                    if (PlayerPrefs.GetInt(loadedSaveFile + "_MPE_Voyeur_SeenViper") != 1) { totalVoyeurDialogues.Add(dialogueViperBeach01Instance); }
                    viperBeachDialogueScene1 = dialogueViperBeach01Instance.transform.Find("Scene1").gameObject;
                    viperBeachDialogueScene2 = dialogueViperBeach01Instance.transform.Find("Scene2").gameObject;
                    viperBeachDialogueScene3 = dialogueViperBeach01Instance.transform.Find("Scene3").gameObject;
                    viperBeachDialogueScene4 = dialogueViperBeach01Instance.transform.Find("Scene4").gameObject;
                    viperBeachDialogueActivator = dialogueViperBeach01Instance.transform.Find("DialogueActivator").gameObject;
                    viperBeachDialogueFinisher = dialogueViperBeach01Instance.transform.Find("DialogueFinisher").gameObject;
                    viperBeachDialogueSpriteFocus = dialogueViperBeach01Instance.transform.Find("SpriteFocus").gameObject;
                    Logger.LogInfo("Viper Dialogue ready!");
                    #endregion
                    #region Yan
                    yanSwim = CreateNewBust("Yan", bustPath, "Yan\\YanSwim00.PNG", "Yan\\YanBlink.PNG", "Yan\\YanSwim00Mask.PNG", "Yan\\YanMouth");
                    yanSwimMBase = yanSwim.transform.Find("MBase1").gameObject;
                    yanBeachScene1 = CreateNewPicScene("YanBeachScene01", scenePath + "Yan\\YanBeachScene01.PNG");
                    yanBeachScene2 = CreateNewPicScene("YanBeachScene02", scenePath + "Yan\\YanBeachScene02.PNG");
                    yanBeachScene3 = CreateNewPicScene("YanBeachScene03", scenePath + "Yan\\YanBeachScene03.PNG");
                    yanBeachScene4 = CreateNewPicScene("YanBeachScene04", scenePath + "Yan\\YanBeachScene04.PNG");
                    yanSwimWetParticles = GameObject.Instantiate(wetParticleSystem, yanSwimMBase.transform);
                    yanSwimWetParticles.SetActive(false);
                    if (PlayerPrefs.GetInt(loadedSaveFile + "_MPE_Voyeur_SeenYan") != 1) { totalVoyeurBusts.Add((yanSwim, 4f)); }
                    Logger.LogInfo("Yan bust ready!");

                    dialogueYanBeach01 = dialogueBundle.LoadAsset<GameObject>("YanDialogueBeach01");
                    dialogueYanBeach01Dialogue = dialogueYanBeach01.GetComponent<Dialogue>();
                    dialogueYanBeach01Dialogue.Story.Content.DialogueSkin = baseDialogueObject.GetComponent<Dialogue>().Story.Content.DialogueSkin;
                    dialogueYanBeach01Instance = GameObject.Instantiate(dialogueYanBeach01, beach.transform);
                    if (PlayerPrefs.GetInt(loadedSaveFile + "_MPE_Voyeur_SeenYan") != 1) { totalVoyeurDialogues.Add(dialogueYanBeach01Instance); }
                    yanBeachDialogueScene1 = dialogueYanBeach01Instance.transform.Find("Scene1").gameObject;
                    yanBeachDialogueScene2 = dialogueYanBeach01Instance.transform.Find("Scene2").gameObject;
                    yanBeachDialogueScene3 = dialogueYanBeach01Instance.transform.Find("Scene3").gameObject;
                    yanBeachDialogueScene4 = dialogueYanBeach01Instance.transform.Find("Scene4").gameObject;
                    yanBeachDialogueActivator = dialogueYanBeach01Instance.transform.Find("DialogueActivator").gameObject;
                    yanBeachDialogueFinisher = dialogueYanBeach01Instance.transform.Find("DialogueFinisher").gameObject;
                    yanBeachDialogueSpriteFocus = dialogueYanBeach01Instance.transform.Find("SpriteFocus").gameObject;
                    Logger.LogInfo("Yan Dialogue ready!");
                    #endregion

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

                    PrintAllVariables();
                    PrintAllGlobalVariables();

                    sBRelaxedAmount = PlayerPrefs.GetInt(loadedSaveFile + "_MPE_SecretBeach_RelaxedAmount");
                    loadedBases = true;
                }



                #endregion
                if (loadedBases)
                {
                    #region Secret Beach
                    if (baseLevel.activeSelf)
                    {
                        secretBeachMapButton.SetActive(true);
                    }
                    else
                    {
                        secretBeachMapButton.SetActive(false);
                    }
                    if (secretBeachMapButton.transform.GetChild(0).gameObject.activeSelf)
                    {
                        FindAndModifyVariable("Active-Level", 999);
                        PrintAllGlobalVariables();

                        transitionFadeObject.GetComponent<DemoStartTransition>().Play();
                        sBAnimTimer = 0.0f;
                        sBAnimTimerRunning = true;
                        secretBeachMapButton.transform.GetChild(0).gameObject.SetActive(false);
                    }
                    if (sBAnimTimerRunning)
                    {
                        sBAnimTimer += Time.deltaTime;
                    }
                    if (sBAnimTimer >= 0.7f)
                    {
                        sBAnimTimer = 0.0f;
                        sBAnimTimerRunning = false;
                        StartCoroutine(TravelSecretBeach(0.7f));
                    }
                    if (sBBlinkTimerRunning)
                    {
                        sBBlinkTimer += Time.deltaTime;
                    }
                    if (sBBlinkTimer >= 1f)
                    {
                        sBBlinkTimer = 0.0f;
                        sBBlinkTimerRunning = false;
                        hasSBBlinkPlayed = true;
                    }
                    if (hasSBBlinkPlayed)
                    {
                        transitionBlinkObject.GetComponent<DemoStartTransition>().PlayBackwards();
                        hasSBBlinkPlayed = false;
                    }
                    if (sBDialogueTimerRunning)
                    {
                        sBDialogueTimer += Time.deltaTime;
                    }
                    if (sBDialogueTimer >= 0.7f)
                    {
                        sBDialogueTimer = 0.0f;
                        sBDialogueTimerRunning = false;
                        //FindInActiveObjectByName("Dialogue_Default_Dialogue(Clone)").SetActive(true);
                    }
                    #endregion


                    #region Randomizer
                    if (secretBeachLevel.activeSelf)
                    {
                        sBDialogueTimerRunning = true;
                        secretBeachGatekeeper.transform.Rotate(0, 0, 1f * Time.deltaTime);
                        #region Relax Dialogue
                        if (!hasSBRelaxed)
                        {
                            if (!hasSBMainDialoguePlayed && !hasSBMainDialogueGKPlayed)
                            {
                                if (sBRelaxedAmount == 2)
                                {
                                    secretBeachLevel.GetComponent<MoveRelative2Mouse>().enabled = false;
                                    StartCoroutine(StartDialogueNoBust(dialogueBeachMainGKInstance, 1.5f));
                                    hasSBMainDialogueGKPlayed = true;
                                }
                                else
                                {
                                    StartCoroutine(StartDialogueNoBust(dialogueBeachMainInstance, 1.5f));
                                    hasSBMainDialoguePlayed = true;
                                }
                            }
                            if (beachMainDialogueScene1.activeSelf || beachMainDialogueGKScene5.activeSelf)
                            {
                                sBRelaxedAmount++;
                                Logger.LogInfo("sBRelaxedAmount = " + sBRelaxedAmount);
                                hasSBRelaxed = true;
                            }
                        }
                        if (beachMainDialogueGKScene2.activeSelf)
                        {
                            if (secretBeachLevel.transform.position.y > -17)
                            {
                                secretBeachLevel.transform.position = Vector2.SmoothDamp(secretBeachLevel.transform.position, new Vector2(0, -17), ref refVelocity, 1.5f);
                            }
                        }
                        if (beachMainDialogueGKScene3.activeSelf && !beachMainDialogueGKScene4.activeSelf)
                        {
                            secretBeachLevel.transform.position = new Vector2(secretBeachLevel.transform.position.x, -17);
                            secretBeachGatekeeperB.SetActive(true);
                        }
                        if (beachMainDialogueGKScene4.activeSelf)
                        {
                            secretBeachFlash.SetActive(true);
                            secretBeachGatekeeper.SetActive(false);
                            secretBeachGatekeeperB.SetActive(false);

                        }
                        if (beachMainDialogueGKScene5.activeSelf)
                        {
                            if (secretBeachLevel.transform.position.y < 0)
                            {
                                secretBeachLevel.transform.position = Vector2.SmoothDamp(secretBeachLevel.transform.position, new Vector2(0, 7), ref refVelocity, 1f);
                            }
                        }
                        if (beachMainDialogueFinisher.activeSelf)
                        {
                            FinishDialogueNoBust(dialogueBeachMainInstance);
                            beachMainDialogueActivator.SetActive(false);
                            beachMainDialogueFinisher.SetActive(false);
                            transitionBlinkObject.GetComponent<DemoStartTransition>().Play();
                            sBBlinkTimerRunning = true;
                        }
                        if (beachMainDialogueGKFinisher.activeSelf)
                        {
                            FinishDialogueNoBust(dialogueBeachMainGKInstance);
                            secretBeachLevel.transform.position = new Vector2(secretBeachLevel.transform.position.x, 0);
                            beachMainDialogueGKActivator.SetActive(false);
                            beachMainDialogueGKFinisher.SetActive(false);
                            transitionBlinkObject.GetComponent<DemoStartTransition>().Play();
                            sBBlinkTimerRunning = true;
                            secretBeachLevel.GetComponent<MoveRelative2Mouse>().enabled = true;
                        }


                        #endregion
                        if (hasSBLoaded)
                        {
                            if (hasSBAnimationPlayed)
                            {
                                transitionFadeObject.GetComponent<DemoStartTransition>().PlayBackwards();
                                hasSBAnimationPlayed = false;
                            }
                        }
                        else
                        {
                            hasSBLoaded = false;
                        }
                        if (hasSBLoaded && !activated && sBRelaxedAmount >= 3 && hasSBRelaxed && !hasSBMainDialogueGKPlayed)
                        {
                            activated = true;

                            float randomizer = UnityEngine.Random.Range(0.0f, 100.0f);
                            if (totalVoyeurDialogues.Count > 0)
                            {
                                if (randomizer <= 100.0f)
                                {
                                    int randomIndex = GetWeightedRandomIndex(totalVoyeurBusts);
                                    StartCoroutine(StartDialogue(totalVoyeurBusts[randomIndex].bust, totalVoyeurDialogues[randomIndex], 2f));
                                    Logger.LogInfo(randomizer + ". Bullseye! " + totalVoyeurBusts[randomIndex].bust.name + " time!");
                                }
                                else
                                {
                                    Logger.LogInfo(randomizer + ". No dice.");
                                }
                            }
                            else
                            {
                                Logger.LogInfo(randomizer + ". No dice.");
                            }
                        }
                    }
                    else
                    {
                        dialogueStarted = false;
                    }
                    #endregion
                    #region Save & Load
                    if (afterSleepEvents.activeSelf && !afterSleepEventsProc)
                    {
                        afterSleepEventsProc = true;
                        SetNewSpriteWithMask(anisSwim, bustPath, "AnisSwim00");
                        SetNewSpriteWithMask(frimaSwim, bustPath, "FrimaSwim00");
                        SetNewSpriteWithMask(guiltySwim, bustPath, "GuiltySwim00");
                        SetNewSpriteWithMask(helmSwim, bustPath, "HelmSwim00");
                        SetNewSpriteWithMask(maidenSwim, bustPath, "MaidenSwim00");
                        SetNewSpriteWithMask(marySwim, bustPath, "MarySwim00");
                        SetNewSpriteWithMask(mastSwim, bustPath, "MastSwim00");
                        SetNewSpriteWithMask(neonSwim, bustPath, "NeonSwim00");
                        SetNewSpriteWithMask(pepperSwim, bustPath, "PepperSwim00");
                        SetNewSpriteWithMask(rapiSwim, bustPath, "RapiSwim00");
                        SetNewSpriteWithMask(rosannaSwim, bustPath, "RosannaSwim00");
                        SetNewSpriteWithMask(sakuraSwim, bustPath, "SakuraSwim00");
                        SetNewSpriteWithMask(viperSwim, bustPath, "ViperSwim00");
                        SetNewSpriteWithMask(yanSwim, bustPath, "YanSwim00");

                        activated = false;
                        hasSBRelaxed = false;
                        hasSBMainDialoguePlayed = false;
                        hasSBMainDialogueGKPlayed = false;
                        Logger.LogInfo("Reset encounter.");
                    }

                    if (afterSleepEventsProc && savedUI.activeSelf)
                    {
                        if (isNewGame && !hasNewGameSaved)
                        {
                            SaveFile(loadedSaveFile, 1, true);
                            hasNewGameSaved = true;
                        }
                        else
                        {
                            SaveFile(loadedSaveFile, 1, false);
                        }
                        afterSleepEventsProc = false;
                        hasQuickSaved = true;
                    }
                    if (saveLoadSystem.activeSelf)
                    {
                        saveButton1.GetComponent<ButtonInstructions>().onClick.AddListener(() => {
                            if (!hasSaved)
                            {
                                SaveFile(loadedSaveFile, 2, false);
                                hasSaved = true;
                            }
                        });
                        saveButton2.GetComponent<ButtonInstructions>().onClick.AddListener(() => {
                            if (!hasSaved)
                            {
                                SaveFile(loadedSaveFile, 3, false);
                                hasSaved = true;
                            }
                        });
                        saveButton3.GetComponent<ButtonInstructions>().onClick.AddListener(() => {
                            if (!hasSaved)
                            {
                                SaveFile(loadedSaveFile, 4, false);
                                hasSaved = true;
                            }
                        });
                        saveButton4.GetComponent<ButtonInstructions>().onClick.AddListener(() => {
                            if (!hasSaved)
                            {
                                SaveFile(loadedSaveFile, 5, false);
                                hasSaved = true;
                            }
                        });
                        saveButton5.GetComponent<ButtonInstructions>().onClick.AddListener(() => {
                            if (!hasSaved)
                            {
                                SaveFile(loadedSaveFile, 6, false);
                                hasSaved = true;
                            }
                        });
                        saveButton6.GetComponent<ButtonInstructions>().onClick.AddListener(() => {
                            if (!hasSaved)
                            {
                                SaveFile(loadedSaveFile, 7, false);
                                hasSaved = true;
                            }
                        });
                        saveButton7.GetComponent<ButtonInstructions>().onClick.AddListener(() => {
                            if (!hasSaved)
                            {
                                SaveFile(loadedSaveFile, 8, false);
                                hasSaved = true;
                            }
                        });
                        saveButton8.GetComponent<ButtonInstructions>().onClick.AddListener(() => {
                            if (!hasSaved)
                            {
                                SaveFile(loadedSaveFile, 9, false);
                                hasSaved = true;
                            }
                        });
                    }
                    if (hasSaved)
                    {
                        saveCounter += Time.deltaTime;
                    }
                    if (saveCounter >= 0.5f)
                    {
                        hasSaved = false;
                        saveCounter = 0;
                    }
                    if (introMomentNewGame.activeSelf)
                    {
                        isNewGame = true;
                    }
                    #endregion

                    #region Anis Dialogue
                    if (anisBeachDialogueScene1.activeSelf && !anisBeachDialogueScene4.activeSelf)
                    {
                        anisBeachScene1.SetActive(true);
                    }
                    if (anisBeachDialogueScene2.activeSelf)
                    {
                        anisBeachScene2.SetActive(true);
                        SetNewSpriteWithMask(anisSwim, bustPath, "Anis\\AnisSwim01");
                        anisSwimWetParticles.SetActive(true);
                    }
                    if (anisBeachDialogueScene3.activeSelf)
                    {
                        anisBeachScene3.SetActive(true);
                        SetNewSpriteWithMask(anisSwim, bustPath, "Anis\\AnisSwim02");
                    }
                    if (anisBeachDialogueScene4.activeSelf)
                    {
                        anisBeachScene4.SetActive(true);
                        anisBeachScene1.SetActive(false);
                    }
                    if (anisBeachDialogueFinisher.activeSelf)
                    {
                        anisBeachScene1.SetActive(false);
                        anisBeachScene2.SetActive(false);
                        anisBeachScene3.SetActive(false);
                        anisBeachScene4.SetActive(false);
                        StartCoroutine(FinishDialogue(anisSwimMBase, dialogueAnisBeach01Instance));
                        anisBeachDialogueActivator.SetActive(false);
                        anisBeachDialogueFinisher.SetActive(false);
                        voyeurTarget = "Anis";
                        totalVoyeurBusts.Remove((anisSwim, 4f));
                        totalVoyeurDialogues.Remove(dialogueAnisBeach01Instance);
                    }
                    if (anisBeachDialogueSpriteFocus.activeSelf)
                    {
                        anisSwimMBase.GetComponent<SpriteRenderer>().sortingOrder = 16;
                        anisSwimWetParticles.GetComponent<ParticleSystemRenderer>().sortingOrder = 17;
                    }
                    else
                    {
                        anisSwimMBase.GetComponent<SpriteRenderer>().sortingOrder = 0;
                        anisSwimWetParticles.GetComponent<ParticleSystemRenderer>().sortingOrder = 10;
                    }
                    #endregion
                    #region Frima Dialogue
                    if (frimaBeachDialogueScene1.activeSelf && !frimaBeachDialogueScene4.activeSelf)
                    {
                        frimaBeachScene1.SetActive(true);
                    }
                    if (frimaBeachDialogueScene2.activeSelf)
                    {
                        frimaBeachScene2.SetActive(true);
                        SetNewSpriteWithMask(frimaSwim, bustPath, "FrimaSwim01");
                    }
                    if (frimaBeachDialogueScene3.activeSelf)
                    {
                        frimaBeachScene3.SetActive(true);
                    }
                    if (frimaBeachDialogueScene4.activeSelf)
                    {
                        frimaBeachScene4.SetActive(true);
                        SetNewSpriteWithMask(frimaSwim, bustPath, "FrimaSwim02");
                        frimaBeachScene1.SetActive(false);
                    }
                    if (frimaBeachDialogueFinisher.activeSelf)
                    {
                        frimaBeachScene1.SetActive(false);
                        frimaBeachScene2.SetActive(false);
                        frimaBeachScene3.SetActive(false);
                        frimaBeachScene4.SetActive(false);
                        StartCoroutine(FinishDialogue(frimaSwimMBase, dialogueFrimaBeach01Instance));
                        frimaBeachDialogueActivator.SetActive(false);
                        frimaBeachDialogueFinisher.SetActive(false);
                        voyeurTarget = "Frima";
                        totalVoyeurBusts.Remove((frimaSwim, 4f));
                        totalVoyeurDialogues.Remove(dialogueFrimaBeach01Instance);
                    }
                    if (frimaBeachDialogueSpriteFocus.activeSelf)
                    {
                        frimaSwimMBase.GetComponent<SpriteRenderer>().sortingOrder = 16;
                        frimaSwimWetParticles.GetComponent<ParticleSystemRenderer>().sortingOrder = 17;
                    }
                    else
                    {
                        frimaSwimMBase.GetComponent<SpriteRenderer>().sortingOrder = 0;
                        frimaSwimWetParticles.GetComponent<ParticleSystemRenderer>().sortingOrder = 10;
                    }
                    #endregion
                    #region Guilty Dialogue
                    if (guiltyBeachDialogueScene1.activeSelf && !guiltyBeachDialogueScene4.activeSelf)
                    {
                        guiltyBeachScene1.SetActive(true);
                    }
                    if (guiltyBeachDialogueScene2.activeSelf)
                    {
                        guiltyBeachScene2.SetActive(true);
                    }
                    if (guiltyBeachDialogueScene3.activeSelf)
                    {
                        guiltyBeachScene3.SetActive(true);
                    }
                    if (guiltyBeachDialogueScene4.activeSelf)
                    {
                        guiltyBeachScene4.SetActive(true);
                        SetNewSpriteWithMask(guiltySwim, bustPath, "GuiltySwim01");
                        guiltyBeachScene1.SetActive(false);
                    }
                    if (guiltyBeachDialogueFinisher.activeSelf)
                    {
                        guiltyBeachScene1.SetActive(false);
                        guiltyBeachScene2.SetActive(false);
                        guiltyBeachScene3.SetActive(false);
                        guiltyBeachScene4.SetActive(false);
                        StartCoroutine(FinishDialogue(guiltySwimMBase, dialogueGuiltyBeach01Instance));
                        guiltyBeachDialogueActivator.SetActive(false);
                        guiltyBeachDialogueFinisher.SetActive(false);
                        voyeurTarget = "Guilty";
                        totalVoyeurBusts.Remove((guiltySwim, 4f));
                        totalVoyeurDialogues.Remove(dialogueGuiltyBeach01Instance);
                    }
                    if (guiltyBeachDialogueSpriteFocus.activeSelf)
                    {
                        guiltySwimMBase.GetComponent<SpriteRenderer>().sortingOrder = 16;
                        guiltySwimWetParticles.GetComponent<ParticleSystemRenderer>().sortingOrder = 17;
                    }
                    else
                    {
                        guiltySwimMBase.GetComponent<SpriteRenderer>().sortingOrder = 0;
                        guiltySwimWetParticles.GetComponent<ParticleSystemRenderer>().sortingOrder = 10;
                    }
                    #endregion
                    #region Helm Dialogue
                    if (helmBeachDialogueScene1.activeSelf && !helmBeachDialogueScene4.activeSelf)
                    {
                        helmBeachScene1.SetActive(true);
                    }
                    if (helmBeachDialogueScene2.activeSelf)
                    {
                        helmBeachScene2.SetActive(true);
                        SetNewSpriteWithMask(helmSwim, bustPath, "HelmSwim01");
                        helmSwimWetParticles.SetActive(true);
                    }
                    if (helmBeachDialogueScene3.activeSelf)
                    {
                        helmBeachScene3.SetActive(true);
                        SetNewSpriteWithMask(helmSwim, bustPath, "HelmSwim02");
                    }
                    if (helmBeachDialogueScene4.activeSelf)
                    {
                        helmBeachScene4.SetActive(true);
                        SetNewSpriteWithMask(helmSwim, bustPath, "HelmSwim03");
                        helmBeachScene1.SetActive(false);
                    }
                    if (helmBeachDialogueFinisher.activeSelf)
                    {
                        helmBeachScene1.SetActive(false);
                        helmBeachScene2.SetActive(false);
                        helmBeachScene3.SetActive(false);
                        helmBeachScene4.SetActive(false);
                        StartCoroutine(FinishDialogue(helmSwimMBase, dialogueHelmBeach01Instance));
                        helmBeachDialogueActivator.SetActive(false);
                        helmBeachDialogueFinisher.SetActive(false);
                        voyeurTarget = "Helm";
                        totalVoyeurBusts.Remove((helmSwim, 4f));
                        totalVoyeurDialogues.Remove(dialogueHelmBeach01Instance);
                    }
                    if (helmBeachDialogueSpriteFocus.activeSelf)
                    {
                        helmSwimMBase.GetComponent<SpriteRenderer>().sortingOrder = 16;
                        helmSwimWetParticles.GetComponent<ParticleSystemRenderer>().sortingOrder = 17;
                    }
                    else
                    {
                        helmSwimMBase.GetComponent<SpriteRenderer>().sortingOrder = 0;
                        helmSwimWetParticles.GetComponent<ParticleSystemRenderer>().sortingOrder = 10;
                    }
                    #endregion
                    #region Maiden Dialogue
                    if (maidenBeachDialogueScene1.activeSelf && !maidenBeachDialogueScene4.activeSelf)
                    {
                        maidenBeachScene1.SetActive(true);
                    }
                    if (maidenBeachDialogueScene2.activeSelf)
                    {
                        maidenBeachScene2.SetActive(true);
                    }
                    if (maidenBeachDialogueScene3.activeSelf)
                    {
                        maidenBeachScene3.SetActive(true);
                    }
                    if (maidenBeachDialogueScene4.activeSelf)
                    {
                        maidenBeachScene4.SetActive(true);
                        SetNewSpriteWithMask(maidenSwim, bustPath, "MaidenSwim01");
                        maidenBeachScene1.SetActive(false);
                    }
                    if (maidenBeachDialogueFinisher.activeSelf)
                    {
                        maidenBeachScene1.SetActive(false);
                        maidenBeachScene2.SetActive(false);
                        maidenBeachScene3.SetActive(false);
                        maidenBeachScene4.SetActive(false);
                        StartCoroutine(FinishDialogue(maidenSwimMBase, dialogueMaidenBeach01Instance));
                        maidenBeachDialogueActivator.SetActive(false);
                        maidenBeachDialogueFinisher.SetActive(false);
                        voyeurTarget = "Maiden";
                        totalVoyeurBusts.Remove((maidenSwim, 4f));
                        totalVoyeurDialogues.Remove(dialogueMaidenBeach01Instance);
                    }
                    if (maidenBeachDialogueSpriteFocus.activeSelf)
                    {
                        maidenSwimMBase.GetComponent<SpriteRenderer>().sortingOrder = 16;
                        maidenSwimWetParticles.GetComponent<ParticleSystemRenderer>().sortingOrder = 17;
                    }
                    else
                    {
                        maidenSwimMBase.GetComponent<SpriteRenderer>().sortingOrder = 0;
                        maidenSwimWetParticles.GetComponent<ParticleSystemRenderer>().sortingOrder = 10;
                    }
                    #endregion
                    #region Mary Dialogue
                    if (maryBeachDialogueScene1.activeSelf && !maryBeachDialogueScene4.activeSelf)
                    {
                        maryBeachScene1.SetActive(true);
                    }
                    if (maryBeachDialogueScene2.activeSelf)
                    {
                        maryBeachScene2.SetActive(true);
                    }
                    if (maryBeachDialogueScene3.activeSelf)
                    {
                        maryBeachScene3.SetActive(true);
                        SetNewSpriteWithMask(marySwim, bustPath, "MarySwim01");
                    }
                    if (maryBeachDialogueScene4.activeSelf)
                    {
                        maryBeachScene4.SetActive(true);
                        maryBeachScene1.SetActive(false);
                    }
                    if (maryBeachDialogueFinisher.activeSelf)
                    {
                        maryBeachScene1.SetActive(false);
                        maryBeachScene2.SetActive(false);
                        maryBeachScene3.SetActive(false);
                        maryBeachScene4.SetActive(false);
                        StartCoroutine(FinishDialogue(marySwimMBase, dialogueMaryBeach01Instance));
                        maryBeachDialogueActivator.SetActive(false);
                        maryBeachDialogueFinisher.SetActive(false);
                        voyeurTarget = "Mary";
                        totalVoyeurBusts.Remove((marySwim, 4f));
                        totalVoyeurDialogues.Remove(dialogueMaryBeach01Instance);
                    }
                    if (maryBeachDialogueSpriteFocus.activeSelf)
                    {
                        marySwimMBase.GetComponent<SpriteRenderer>().sortingOrder = 16;
                        marySwimWetParticles.GetComponent<ParticleSystemRenderer>().sortingOrder = 17;
                    }
                    else
                    {
                        marySwimMBase.GetComponent<SpriteRenderer>().sortingOrder = 0;
                        marySwimWetParticles.GetComponent<ParticleSystemRenderer>().sortingOrder = 10;
                    }
                    #endregion
                    #region Mast Dialogue
                    if (mastBeachDialogueScene1.activeSelf && !mastBeachDialogueScene4.activeSelf)
                    {
                        mastBeachScene1.SetActive(true);
                    }
                    if (mastBeachDialogueScene2.activeSelf)
                    {
                        mastBeachScene2.SetActive(true);
                    }
                    if (mastBeachDialogueScene3.activeSelf)
                    {
                        mastBeachScene3.SetActive(true);
                    }
                    if (mastBeachDialogueScene4.activeSelf)
                    {
                        mastBeachScene4.SetActive(true);
                        SetNewSpriteWithMask(mastSwim, bustPath, "MastSwim01");
                        mastBeachScene1.SetActive(false);
                    }
                    if (mastBeachDialogueFinisher.activeSelf)
                    {
                        mastBeachScene1.SetActive(false);
                        mastBeachScene2.SetActive(false);
                        mastBeachScene3.SetActive(false);
                        mastBeachScene4.SetActive(false);
                        StartCoroutine(FinishDialogue(mastSwimMBase, dialogueMastBeach01Instance));
                        mastBeachDialogueActivator.SetActive(false);
                        mastBeachDialogueFinisher.SetActive(false);
                        voyeurTarget = "Mast";
                        totalVoyeurBusts.Remove((mastSwim, 4f));
                        totalVoyeurDialogues.Remove(dialogueMastBeach01Instance);
                    }
                    if (mastBeachDialogueSpriteFocus.activeSelf)
                    {
                        mastSwimMBase.GetComponent<SpriteRenderer>().sortingOrder = 16;
                        mastSwimWetParticles.GetComponent<ParticleSystemRenderer>().sortingOrder = 17;
                    }
                    else
                    {
                        mastSwimMBase.GetComponent<SpriteRenderer>().sortingOrder = 0;
                        mastSwimWetParticles.GetComponent<ParticleSystemRenderer>().sortingOrder = 10;
                    }
                    #endregion
                    #region Neon Dialogue
                    if (neonBeachDialogueScene1.activeSelf && !neonBeachDialogueScene4.activeSelf)
                    {
                        neonBeachScene1.SetActive(true);
                    }
                    if (neonBeachDialogueScene2.activeSelf)
                    {
                        neonBeachScene2.SetActive(true);
                    }
                    if (neonBeachDialogueScene3.activeSelf)
                    {
                        neonBeachScene3.SetActive(true);
                        SetNewSpriteWithMask(neonSwim, bustPath, "NeonSwim01");
                    }
                    if (neonBeachDialogueScene4.activeSelf)
                    {
                        neonBeachScene4.SetActive(true);
                        SetNewSpriteWithMask(neonSwim, bustPath, "NeonSwim02");
                        neonBeachScene1.SetActive(false);
                    }
                    if (neonBeachDialogueFinisher.activeSelf)
                    {
                        neonBeachScene1.SetActive(false);
                        neonBeachScene2.SetActive(false);
                        neonBeachScene3.SetActive(false);
                        neonBeachScene4.SetActive(false);
                        StartCoroutine(FinishDialogue(neonSwimMBase, dialogueNeonBeach01Instance));
                        neonBeachDialogueActivator.SetActive(false);
                        neonBeachDialogueFinisher.SetActive(false);
                        voyeurTarget = "Neon";
                        totalVoyeurBusts.Remove((neonSwim, 4f));
                        totalVoyeurDialogues.Remove(dialogueNeonBeach01Instance);
                    }
                    if (neonBeachDialogueSpriteFocus.activeSelf)
                    {
                        neonSwimMBase.GetComponent<SpriteRenderer>().sortingOrder = 16;
                        neonSwimWetParticles.GetComponent<ParticleSystemRenderer>().sortingOrder = 17;
                    }
                    else
                    {
                        neonSwimMBase.GetComponent<SpriteRenderer>().sortingOrder = 0;
                        neonSwimWetParticles.GetComponent<ParticleSystemRenderer>().sortingOrder = 10;
                    }
                    #endregion
                    #region Pepper Dialogue
                    if (pepperBeachDialogueScene1.activeSelf && !pepperBeachDialogueScene4.activeSelf)
                    {
                        pepperBeachScene1.SetActive(true);
                    }
                    if (pepperBeachDialogueScene2.activeSelf)
                    {
                        pepperBeachScene2.SetActive(true);
                    }
                    if (pepperBeachDialogueScene3.activeSelf)
                    {
                        pepperBeachScene3.SetActive(true);
                        SetNewSpriteWithMask(pepperSwim, bustPath, "PepperSwim01");
                    }
                    if (pepperBeachDialogueScene4.activeSelf)
                    {
                        pepperBeachScene4.SetActive(true);
                        pepperBeachScene1.SetActive(false);
                    }
                    if (pepperBeachDialogueFinisher.activeSelf)
                    {
                        pepperBeachScene1.SetActive(false);
                        pepperBeachScene2.SetActive(false);
                        pepperBeachScene3.SetActive(false);
                        pepperBeachScene4.SetActive(false);
                        StartCoroutine(FinishDialogue(pepperSwimMBase, dialoguePepperBeach01Instance));
                        pepperBeachDialogueActivator.SetActive(false);
                        pepperBeachDialogueFinisher.SetActive(false);
                        voyeurTarget = "Pepper";
                        totalVoyeurBusts.Remove((pepperSwim, 4f));
                        totalVoyeurDialogues.Remove(dialoguePepperBeach01Instance);
                    }
                    if (pepperBeachDialogueSpriteFocus.activeSelf)
                    {
                        pepperSwimMBase.GetComponent<SpriteRenderer>().sortingOrder = 16;
                        pepperSwimWetParticles.GetComponent<ParticleSystemRenderer>().sortingOrder = 17;
                    }
                    else
                    {
                        pepperSwimMBase.GetComponent<SpriteRenderer>().sortingOrder = 0;
                        pepperSwimWetParticles.GetComponent<ParticleSystemRenderer>().sortingOrder = 10;
                    }
                    #endregion
                    #region Rapi Dialogue
                    if (rapiBeachDialogueScene1.activeSelf && !rapiBeachDialogueScene4.activeSelf)
                    {
                        rapiBeachScene1.SetActive(true);
                    }
                    if (rapiBeachDialogueScene2.activeSelf)
                    {
                        rapiBeachScene2.SetActive(true);
                    }
                    if (rapiBeachDialogueScene3.activeSelf)
                    {
                        rapiBeachScene3.SetActive(true);
                    }
                    if (rapiBeachDialogueScene4.activeSelf)
                    {
                        rapiBeachScene4.SetActive(true);
                        SetNewSpriteWithMask(rapiSwim, bustPath, "RapiSwim01");
                        rapiBeachScene1.SetActive(false);
                    }
                    if (rapiBeachDialogueFinisher.activeSelf)
                    {
                        rapiBeachScene1.SetActive(false);
                        rapiBeachScene2.SetActive(false);
                        rapiBeachScene3.SetActive(false);
                        rapiBeachScene4.SetActive(false);
                        StartCoroutine(FinishDialogue(rapiSwimMBase, dialogueRapiBeach01Instance));
                        rapiBeachDialogueActivator.SetActive(false);
                        rapiBeachDialogueFinisher.SetActive(false);
                        voyeurTarget = "Rapi";
                        totalVoyeurBusts.Remove((rapiSwim, 4f));
                        totalVoyeurDialogues.Remove(dialogueRapiBeach01Instance);
                    }
                    if (rapiBeachDialogueSpriteFocus.activeSelf)
                    {
                        rapiSwimMBase.GetComponent<SpriteRenderer>().sortingOrder = 16;
                        rapiSwimWetParticles.GetComponent<ParticleSystemRenderer>().sortingOrder = 17;
                    }
                    else
                    {
                        rapiSwimMBase.GetComponent<SpriteRenderer>().sortingOrder = 0;
                        rapiSwimWetParticles.GetComponent<ParticleSystemRenderer>().sortingOrder = 10;
                    }
                    #endregion
                    #region Rosanna Dialogue
                    if (rosannaBeachDialogueScene1.activeSelf && !rosannaBeachDialogueScene4.activeSelf)
                    {
                        rosannaBeachScene1.SetActive(true);
                    }
                    if (rosannaBeachDialogueScene2.activeSelf)
                    {
                        rosannaBeachScene2.SetActive(true);
                    }
                    if (rosannaBeachDialogueScene3.activeSelf)
                    {
                        rosannaBeachScene3.SetActive(true);
                    }
                    if (rosannaBeachDialogueScene4.activeSelf)
                    {
                        rosannaBeachScene4.SetActive(true);
                        SetNewSpriteWithMask(rosannaSwim, bustPath, "RosannaSwim01");
                        rosannaBeachScene1.SetActive(false);
                    }
                    if (rosannaBeachDialogueFinisher.activeSelf)
                    {
                        rosannaBeachScene1.SetActive(false);
                        rosannaBeachScene2.SetActive(false);
                        rosannaBeachScene3.SetActive(false);
                        rosannaBeachScene4.SetActive(false);
                        StartCoroutine(FinishDialogue(rosannaSwimMBase, dialogueRosannaBeach01Instance));
                        rosannaBeachDialogueActivator.SetActive(false);
                        rosannaBeachDialogueFinisher.SetActive(false);
                        voyeurTarget = "Rosanna";
                        totalVoyeurBusts.Remove((rosannaSwim, 4f));
                        totalVoyeurDialogues.Remove(dialogueRosannaBeach01Instance);
                    }
                    if (rosannaBeachDialogueSpriteFocus.activeSelf)
                    {
                        rosannaSwimMBase.GetComponent<SpriteRenderer>().sortingOrder = 16;
                        rosannaSwimWetParticles.GetComponent<ParticleSystemRenderer>().sortingOrder = 17;
                    }
                    else
                    {
                        rosannaSwimMBase.GetComponent<SpriteRenderer>().sortingOrder = 0;
                        rosannaSwimWetParticles.GetComponent<ParticleSystemRenderer>().sortingOrder = 10;
                    }
                    #endregion
                    #region Sakura Dialogue
                    if (sakuraBeachDialogueScene1.activeSelf && !sakuraBeachDialogueScene4.activeSelf)
                    {
                        sakuraBeachScene1.SetActive(true);
                    }
                    if (sakuraBeachDialogueScene2.activeSelf)
                    {
                        sakuraBeachScene2.SetActive(true);
                    }
                    if (sakuraBeachDialogueScene3.activeSelf)
                    {
                        sakuraBeachScene3.SetActive(true);
                    }
                    if (sakuraBeachDialogueScene4.activeSelf)
                    {
                        sakuraBeachScene4.SetActive(true);
                        SetNewSpriteWithMask(sakuraSwim, bustPath, "SakuraSwim01");
                        sakuraBeachScene1.SetActive(false);
                    }
                    if (sakuraBeachDialogueFinisher.activeSelf)
                    {
                        sakuraBeachScene1.SetActive(false);
                        sakuraBeachScene2.SetActive(false);
                        sakuraBeachScene3.SetActive(false);
                        sakuraBeachScene4.SetActive(false);
                        StartCoroutine(FinishDialogue(sakuraSwimMBase, dialogueSakuraBeach01Instance));
                        sakuraBeachDialogueActivator.SetActive(false);
                        sakuraBeachDialogueFinisher.SetActive(false);
                        voyeurTarget = "Sakura";
                        totalVoyeurBusts.Remove((sakuraSwim, 4f));
                        totalVoyeurDialogues.Remove(dialogueSakuraBeach01Instance);
                    }
                    if (sakuraBeachDialogueSpriteFocus.activeSelf)
                    {
                        sakuraSwimMBase.GetComponent<SpriteRenderer>().sortingOrder = 16;
                        sakuraSwimWetParticles.GetComponent<ParticleSystemRenderer>().sortingOrder = 17;
                    }
                    else
                    {
                        sakuraSwimMBase.GetComponent<SpriteRenderer>().sortingOrder = 0;
                        sakuraSwimWetParticles.GetComponent<ParticleSystemRenderer>().sortingOrder = 10;
                    }
                    #endregion
                    #region Viper Dialogue
                    if (viperBeachDialogueScene1.activeSelf && !viperBeachDialogueScene4.activeSelf)
                    {
                        viperBeachScene1.SetActive(true);
                        SetNewSpriteWithMask(viperSwim, bustPath, "ViperSwim01");
                    }
                    if (viperBeachDialogueScene2.activeSelf)
                    {
                        viperBeachScene2.SetActive(true);
                        SetNewSpriteWithMask(viperSwim, bustPath, "ViperSwim02");
                        viperSwimWetParticles.SetActive(true);
                    }
                    if (viperBeachDialogueScene3.activeSelf)
                    {
                        viperBeachScene3.SetActive(true);
                    }
                    if (viperBeachDialogueScene4.activeSelf)
                    {
                        viperBeachScene4.SetActive(true);
                        SetNewSpriteWithMask(viperSwim, bustPath, "ViperSwim03");
                        viperBeachScene1.SetActive(false);
                    }
                    if (viperBeachDialogueFinisher.activeSelf)
                    {
                        viperBeachScene1.SetActive(false);
                        viperBeachScene2.SetActive(false);
                        viperBeachScene3.SetActive(false);
                        viperBeachScene4.SetActive(false);
                        StartCoroutine(FinishDialogue(viperSwimMBase, dialogueViperBeach01Instance));
                        viperBeachDialogueActivator.SetActive(false);
                        viperBeachDialogueFinisher.SetActive(false);
                        voyeurTarget = "Viper";
                        totalVoyeurBusts.Remove((viperSwim, 4f));
                        totalVoyeurDialogues.Remove(dialogueViperBeach01Instance);
                    }
                    if (viperBeachDialogueSpriteFocus.activeSelf)
                    {
                        viperSwimMBase.GetComponent<SpriteRenderer>().sortingOrder = 16;
                        viperSwimWetParticles.GetComponent<ParticleSystemRenderer>().sortingOrder = 17;
                    }
                    else
                    {
                        viperSwimMBase.GetComponent<SpriteRenderer>().sortingOrder = 0;
                        viperSwimWetParticles.GetComponent<ParticleSystemRenderer>().sortingOrder = 10;
                    }
                    #endregion
                    #region Yan Dialogue
                    if (yanBeachDialogueScene1.activeSelf && !yanBeachDialogueScene4.activeSelf)
                    {
                        yanBeachScene1.SetActive(true);
                    }
                    if (yanBeachDialogueScene2.activeSelf)
                    {
                        yanBeachScene2.SetActive(true);
                    }
                    if (yanBeachDialogueScene3.activeSelf)
                    {
                        yanBeachScene3.SetActive(true);
                    }
                    if (yanBeachDialogueScene4.activeSelf)
                    {
                        yanBeachScene4.SetActive(true);
                        SetNewSpriteWithMask(yanSwim, bustPath, "YanSwim01");
                        yanBeachScene1.SetActive(false);
                    }
                    if (yanBeachDialogueFinisher.activeSelf)
                    {
                        yanBeachScene1.SetActive(false);
                        yanBeachScene2.SetActive(false);
                        yanBeachScene3.SetActive(false);
                        yanBeachScene4.SetActive(false);
                        StartCoroutine(FinishDialogue(yanSwimMBase, dialogueYanBeach01Instance));
                        yanBeachDialogueActivator.SetActive(false);
                        yanBeachDialogueFinisher.SetActive(false);
                        voyeurTarget = "Yan";
                        totalVoyeurBusts.Remove((yanSwim, 4f));
                        totalVoyeurDialogues.Remove(dialogueYanBeach01Instance);
                    }
                    if (yanBeachDialogueSpriteFocus.activeSelf)
                    {
                        yanSwimMBase.GetComponent<SpriteRenderer>().sortingOrder = 16;
                        yanSwimWetParticles.GetComponent<ParticleSystemRenderer>().sortingOrder = 17;
                    }
                    else
                    {
                        yanSwimMBase.GetComponent<SpriteRenderer>().sortingOrder = 0;
                        yanSwimWetParticles.GetComponent<ParticleSystemRenderer>().sortingOrder = 10;
                    }
                    #endregion
                }
            }

            if (currentScene.name == "GameStart")
            {
                #region Unload Bases
                if (loadedBases == true)
                {
                    Logger.LogInfo("GameStart loaded");

                    roomTalk = GameObject.Find("EventSystem");
                    bustManager = GameObject.Find("EventSystem");
                    beach = GameObject.Find("EventSystem");
                    baseBust = GameObject.Find("EventSystem");
                    baseBustParent = GameObject.Find("EventSystem").transform;
                    basePicScene = GameObject.Find("EventSystem");
                    savedUI = GameObject.Find("EventSystem");
                    dialogueAnisBeach01 = GameObject.Find("EventSystem");
                    baseDialogueObject = GameObject.Find("EventSystem");
                    dialogueAnisBeach01Instance = GameObject.Find("EventSystem");
                    basePicParent = GameObject.Find("EventSystem").transform;
                    navigator = GameObject.Find("EventSystem");
                    baseMapButton = GameObject.Find("EventSystem");
                    wetParticleSystem = GameObject.Find("EventSystem");
                    transitionFadeObject = GameObject.Find("EventSystem");

                    saveLoadSystem = GameObject.Find("Part_One").transform.Find("Canvas_MM").Find("MainMenu").Find("SaveLoadSystem").gameObject;
                    saveButton1 = GameObject.Find("EventSystem");
                    saveButton2 = GameObject.Find("EventSystem");
                    saveButton3 = GameObject.Find("EventSystem");
                    saveButton4 = GameObject.Find("EventSystem");
                    saveButton5 = GameObject.Find("EventSystem");
                    saveButton6 = GameObject.Find("EventSystem");
                    saveButton7 = GameObject.Find("EventSystem");
                    saveButton8 = GameObject.Find("EventSystem");

                    loadedBases = false;
                    activated = false;
                    loadedSaveFile = -1;
                }
                #endregion
            }
        }

        #region Functions
        public void SetNewSprite(GameObject gO, string pathToCG, string baseSprite)
        {
            Material mat = new Material(gO.GetComponent<SpriteRenderer>().material);

            Texture2D tex = new Texture2D(256, 256, TextureFormat.RGBA32, false);
            var rawData = System.IO.File.ReadAllBytes(pathToCG + baseSprite);
            tex.LoadImage(rawData);
            tex.filterMode = FilterMode.Point;
            Sprite newSprite = Sprite.Create(tex, new Rect(0, 0, 256, 256), new Vector2(0.5f, 0.5f));
            gO.GetComponent<SpriteRenderer>().sprite = newSprite;
        }
        public void SetNewLevelSprite(GameObject gO, string pathToCG, string baseSprite, int width, int height)
        {
            Material mat = new Material(gO.GetComponent<SpriteRenderer>().material);

            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            var rawData = System.IO.File.ReadAllBytes(pathToCG + baseSprite);
            tex.LoadImage(rawData);
            tex.filterMode = FilterMode.Point;
            Sprite newSprite = Sprite.Create(tex, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 70.32f);
            gO.GetComponent<SpriteRenderer>().sprite = newSprite;
        }
        public void SetNewSpriteWithMask(GameObject mainBust, string pathToCG, string baseSprite)
        {
            GameObject mBase = mainBust.transform.Find("MBase1").gameObject;
            Material mat = new Material(mBase.GetComponent<SpriteRenderer>().material);

            Texture2D tex = new Texture2D(256, 256, TextureFormat.RGBA32, false);
            var rawData = System.IO.File.ReadAllBytes(pathToCG + baseSprite + ".PNG");
            tex.LoadImage(rawData);
            tex.filterMode = FilterMode.Point;
            Sprite newSprite = Sprite.Create(tex, new Rect(0, 0, 256, 256), new Vector2(0.5f, 0.5f));
            mBase.GetComponent<SpriteRenderer>().sprite = newSprite;

            tex = new Texture2D(256, 256, TextureFormat.RGBA32, false);
            rawData = System.IO.File.ReadAllBytes(pathToCG + baseSprite + "Mask.PNG");
            tex.LoadImage(rawData);
            tex.filterMode = FilterMode.Point;
            mat.SetTexture("_MaskTex", tex);
            mBase.GetComponent<SpriteRenderer>().material = mat;
            mBase.GetComponent<SpriteRenderer>().material.SetTexture("_MaskTex", tex);
        }
        public void FinishDialogueNoBust(GameObject dialogueInstance)
        {
            if (dialogueInstance.transform.Find("Scene1").gameObject != null)
            {
                dialogueInstance.transform.Find("Scene1").gameObject.SetActive(false);
            }
            if (dialogueInstance.transform.Find("Scene2").gameObject != null)
            {
                dialogueInstance.transform.Find("Scene2").gameObject.SetActive(false);
            }
            if (dialogueInstance.transform.Find("Scene3").gameObject != null)
            {
                dialogueInstance.transform.Find("Scene3").gameObject.SetActive(false);
            }
            if (dialogueInstance.transform.Find("Scene4").gameObject != null)
            {
                dialogueInstance.transform.Find("Scene4").gameObject.SetActive(false);
            }
            if (dialogueInstance.transform.Find("Scene5").gameObject != null)
            {
                dialogueInstance.transform.Find("Scene5").gameObject.SetActive(false);
            }

            navigator.SetActive(true);
        }
        public void SaveFile(int currentSave, int destinationSave, bool isNewGame)
        {
            if (isNewGame)
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
            }
            else
            {
                PlayerPrefs.SetInt(destinationSave + "_MPE_Initialized", PlayerPrefs.GetInt(currentSave + "_MPE_Initialized"));
                PlayerPrefs.SetInt(destinationSave + "_MPE_SecretBeach_RelaxedAmount", sBRelaxedAmount);
                PlayerPrefs.SetInt(destinationSave + "_MPE_Voyeur_SeenAnis", PlayerPrefs.GetInt(currentSave + "_MPE_Voyeur_SeenAnis"));
                PlayerPrefs.SetInt(destinationSave + "_MPE_Voyeur_SeenFrima", PlayerPrefs.GetInt(currentSave + "_MPE_Voyeur_SeenFrima"));
                PlayerPrefs.SetInt(destinationSave + "_MPE_Voyeur_SeenGuilty", PlayerPrefs.GetInt(currentSave + "_MPE_Voyeur_SeenGuilty"));
                PlayerPrefs.SetInt(destinationSave + "_MPE_Voyeur_SeenHelm", PlayerPrefs.GetInt(currentSave + "_MPE_Voyeur_SeenHelm"));
                PlayerPrefs.SetInt(destinationSave + "_MPE_Voyeur_SeenMaiden", PlayerPrefs.GetInt(currentSave + "_MPE_Voyeur_SeenMaiden"));
                PlayerPrefs.SetInt(destinationSave + "_MPE_Voyeur_SeenMary", PlayerPrefs.GetInt(currentSave + "_MPE_Voyeur_SeenMary"));
                PlayerPrefs.SetInt(destinationSave + "_MPE_Voyeur_SeenMast", PlayerPrefs.GetInt(currentSave + "_MPE_Voyeur_SeenMast"));
                PlayerPrefs.SetInt(destinationSave + "_MPE_Voyeur_SeenNeon", PlayerPrefs.GetInt(currentSave + "_MPE_Voyeur_SeenNeon"));
                PlayerPrefs.SetInt(destinationSave + "_MPE_Voyeur_SeenPepper", PlayerPrefs.GetInt(currentSave + "_MPE_Voyeur_SeenPepper"));
                PlayerPrefs.SetInt(destinationSave + "_MPE_Voyeur_SeenRapi", PlayerPrefs.GetInt(currentSave + "_MPE_Voyeur_SeenRapi"));
                PlayerPrefs.SetInt(destinationSave + "_MPE_Voyeur_SeenRosanna", PlayerPrefs.GetInt(currentSave + "_MPE_Voyeur_SeenRosanna"));
                PlayerPrefs.SetInt(destinationSave + "_MPE_Voyeur_SeenSakura", PlayerPrefs.GetInt(currentSave + "_MPE_Voyeur_SeenSakura"));
                PlayerPrefs.SetInt(destinationSave + "_MPE_Voyeur_SeenViper", PlayerPrefs.GetInt(currentSave + "_MPE_Voyeur_SeenViper"));
                PlayerPrefs.SetInt(destinationSave + "_MPE_Voyeur_SeenYan", PlayerPrefs.GetInt(currentSave + "_MPE_Voyeur_SeenYan"));
                if (voyeurTarget != "None")
                {
                    PlayerPrefs.SetInt(destinationSave + "_MPE_Voyeur_Seen" + voyeurTarget, 1);
                }
            }
            PlayerPrefs.Save();

            Logger.LogInfo("sBRelaxedAmount = " + sBRelaxedAmount);
            Logger.LogInfo("Saved to slot " + destinationSave + ".");
            Logger.LogInfo(destinationSave + "_MPE_SecretBeach_RelaxedAmount = " + PlayerPrefs.GetInt(destinationSave + "_MPE_SecretBeach_RelaxedAmount"));
        }
        public int GetWeightedRandomIndex(List<(GameObject obj, float weight)> weightedGameObjects)
        {
            // Ensure there are GameObjects to select from
            if (weightedGameObjects == null || weightedGameObjects.Count == 0)
            {
                Debug.LogError("No GameObjects provided.");
                return -1;
            }

            // Calculate the total weight
            float totalWeight = 0f;
            foreach (var (obj, weight) in weightedGameObjects)
            {
                totalWeight += weight;
            }

            // Generate a random number between 0 and the total weight
            float randomValue = UnityEngine.Random.Range(0f, totalWeight);

            // Determine which GameObject to select based on the random value
            float cumulativeWeight = 0f;
            for (int i = 0; i < weightedGameObjects.Count; i++)
            {
                cumulativeWeight += weightedGameObjects[i].weight;
                if (randomValue < cumulativeWeight)
                {
                    return i; // Return the index of the selected GameObject
                }
            }

            // If no GameObject was selected (should not happen)
            return -1;
        }
        public GameObject CreateNewPicScene(string name, string pathToCG)
        {
            GameObject newPicScene = GameObject.Instantiate(basePicScene, basePicParent);
            newPicScene.name = name;
            GameObject core = newPicScene.transform.Find("Core").gameObject;
            GameObject art = core.transform.Find("Art").gameObject;

            var rawData = System.IO.File.ReadAllBytes(pathToCG);
            Texture2D tex = new Texture2D(256, 256);
            tex.filterMode = FilterMode.Point;
            ImageConversion.LoadImage(tex, rawData);
            Sprite newSprite = Sprite.Create(tex, new Rect(0, 0, 256, 256), new Vector2(0.5f, 0.5f));
            art.GetComponent<SpriteRenderer>().sprite = newSprite;

            basePicParent.GetComponent<SpriteRendererLayoutManager>().targetObjects.Add(newPicScene);

            newPicScene.SetActive(false);
            return newPicScene;
        }
        public GameObject CreateNewBust(string name, string pathToCG, string baseSprite, string blinkSprite, string maskSprite, string mouthSprite)
        {
            GameObject newBust = GameObject.Instantiate(baseBust, baseBustParent);
            newBust.name = name;
            GameObject mBase = newBust.transform.Find("MBase1").gameObject;
            GameObject blink = mBase.transform.Find("Blink").gameObject;
            GameObject mouth1 = mBase.transform.Find("Mouth").Find("1").gameObject;
            GameObject mouth2 = mBase.transform.Find("Mouth").Find("2").gameObject;
            GameObject mouth3 = mBase.transform.Find("Mouth").Find("3").gameObject;
            GameObject mouth4 = mBase.transform.Find("Mouth").Find("4").gameObject;
            Material mat = new Material(mBase.GetComponent<SpriteRenderer>().material);

            Texture2D tex = new Texture2D(256, 256, TextureFormat.RGBA32, false);
            var rawData = System.IO.File.ReadAllBytes(pathToCG + baseSprite);
            tex.LoadImage(rawData);
            tex.filterMode = FilterMode.Point;
            Sprite newSprite = Sprite.Create(tex, new Rect(0, 0, 256, 256), new Vector2(0.5f, 0.5f));
            mBase.GetComponent<SpriteRenderer>().sprite = newSprite;

            tex = new Texture2D(256, 256, TextureFormat.RGBA32, false);
            rawData = System.IO.File.ReadAllBytes(pathToCG + blinkSprite);
            tex.LoadImage(rawData);
            tex.filterMode = FilterMode.Point;
            newSprite = Sprite.Create(tex, new Rect(0, 0, 256, 256), new Vector2(0.5f, 0.5f));
            blink.GetComponent<SpriteRenderer>().sprite = newSprite;

            tex = new Texture2D(256, 256, TextureFormat.RGBA32, false);
            rawData = System.IO.File.ReadAllBytes(pathToCG + maskSprite);
            tex.LoadImage(rawData);
            tex.filterMode = FilterMode.Point;
            mat.SetTexture("_MaskTex", tex);
            mBase.GetComponent<SpriteRenderer>().material = mat;
            mBase.GetComponent<SpriteRenderer>().material.SetTexture("_MaskTex", tex);

            baseBustParent.GetComponent<SpriteRendererLayoutManager>().targetObjects.Add(newBust);

            newBust.SetActive(false);
            return newBust;
        }
        public GameObject CreateNewLevel(string name, string pathToCG, string baseSprite, string secondarySprite, string maskSprite)
        {
            GameObject newLevel = GameObject.Instantiate(baseLevel, baseLevel.transform.parent);
            newLevel.name = name;
            GameObject secondaryTex = newLevel.transform.GetChild(1).gameObject;
            secondaryTex.name = name;
            GameObject NPCs = newLevel.transform.GetChild(2).gameObject;
            // Destroy all children of NPCs
            foreach (Transform child in NPCs.transform)
            {
                Destroy(child.gameObject);
            }
            Destroy(NPCs);
            Material mat = new Material(newLevel.GetComponent<SpriteRenderer>().material);

            var rawData = System.IO.File.ReadAllBytes(pathToCG + baseSprite);
            Texture2D tex = new Texture2D(2048, 1136);
            tex.filterMode = FilterMode.Bilinear;
            ImageConversion.LoadImage(tex, rawData);
            Sprite newSprite = Sprite.Create(tex, new Rect(0, 0, 2048, 1136), new Vector2(0.5f, 0.5f), 70.32f);
            newLevel.GetComponent<SpriteRenderer>().sprite = newSprite;

            rawData = System.IO.File.ReadAllBytes(pathToCG + secondarySprite);
            tex = new Texture2D(2048, 1136);
            tex.filterMode = FilterMode.Point;
            ImageConversion.LoadImage(tex, rawData);
            newSprite = Sprite.Create(tex, new Rect(0, 0, 2048, 1136), new Vector2(0.5f, 0.5f), 70.32f);
            secondaryTex.GetComponent<SpriteRenderer>().sprite = newSprite;

            rawData = System.IO.File.ReadAllBytes(pathToCG + maskSprite);
            tex = new Texture2D(256, 143, TextureFormat.RGBA32, false);
            tex.LoadImage(rawData);
            tex.filterMode = FilterMode.Point;
            mat.SetTexture("_MaskTex", tex);
            newLevel.GetComponent<SpriteRenderer>().material = mat;
            newLevel.GetComponent<SpriteRenderer>().material.SetTexture("_MaskTex", tex);

            newLevel.SetActive(false);
            return newLevel;
        }
        public GameObject FindInActiveObjectByName(string name)
        {
            Transform[] objs = Resources.FindObjectsOfTypeAll<Transform>() as Transform[];
            for (int i = 0; i < objs.Length; i++)
            {
                if (objs[i].hideFlags == HideFlags.None)
                {
                    if (objs[i].name == name)
                    {
                        return objs[i].gameObject;
                    }
                }
            }
            return null;
        }
        public GlobalNameVariables FindGlobalNameVariable(string name)
        {
            var valuesField = typeof(GlobalNameVariablesManager).GetProperty("Values", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var values = valuesField.GetValue(globalNameVariablesManager) as Dictionary<IdString, NameVariableRuntime>;
            GlobalNameVariables result = null;

            foreach (var kvp in values)
            {
                IdString assetId = kvp.Key;
                NameVariableRuntime runtime = kvp.Value;
                GlobalNameVariables globalNameVariables = TRepository<VariablesRepository>.Get.Variables.GetNameVariablesAsset(assetId);

                if (globalNameVariables.name == name)
                {
                    Logger.LogInfo($"Found GlobalNameVariables asset: {globalNameVariables.name} (ID: {assetId})");
                }
            }
            return result;
        }
        IEnumerator FinishDialogue(GameObject bust, GameObject dialogueInstance)
        {
            if (dialogueInstance.transform.Find("Scene1").gameObject != null)
            {
                dialogueInstance.transform.Find("Scene1").gameObject.SetActive(false);
            }
            if (dialogueInstance.transform.Find("Scene2").gameObject != null)
            {
                dialogueInstance.transform.Find("Scene2").gameObject.SetActive(false);
            }
            if (dialogueInstance.transform.Find("Scene3").gameObject != null)
            {
                dialogueInstance.transform.Find("Scene3").gameObject.SetActive(false);
            }
            if (dialogueInstance.transform.Find("Scene4").gameObject != null)
            {
                dialogueInstance.transform.Find("Scene4").gameObject.SetActive(false);
            }
            if (dialogueInstance.transform.Find("Scene5").gameObject != null)
            {
                dialogueInstance.transform.Find("Scene5").gameObject.SetActive(false);
            }

            navigator.SetActive(true);

            bust.transform.Find("Leave").gameObject.SetActive(true);
            yield return new WaitForSecondsRealtime(2f);
            bust.transform.Find("Leave").gameObject.SetActive(false);
        }
        IEnumerator StartDialogue(GameObject bust, GameObject dialogueInstance, float time)
        {
            Logger.LogInfo("Waiting");
            yield return new WaitForSecondsRealtime(time);
            Logger.LogInfo("Waited");

            navigator.SetActive(false);
            dialogueInstance.transform.GetChild(0).gameObject.SetActive(true);
            bust.SetActive(true);

            sBDialogueTimerRunning = true;
            dialogueStarted = true;
        }
        IEnumerator StartDialogueNoBust(GameObject dialogueInstance, float time)
        {
            Logger.LogInfo("Waiting");
            yield return new WaitForSecondsRealtime(time);
            Logger.LogInfo("Waited");

            navigator.SetActive(false);
            dialogueInstance.transform.GetChild(0).gameObject.SetActive(true);
            yield return new WaitForSecondsRealtime(time / 2);

            sBDialogueTimerRunning = true;
            dialogueStarted = true;
        }
        IEnumerator TravelSecretBeach(float waitTime)
        {
            baseLevel.SetActive(false);
            secretBeachLevel.SetActive(true);
            baseMapButton.SetActive(true);
            yield return new WaitForSeconds(waitTime);
            hasSBLoaded = true;
            hasSBAnimationPlayed = true;
        }
        IEnumerator Wait(float waitTime)
        {
            yield return new WaitForSeconds(waitTime);
        }

        public void PrintAllVariables()
        {
            if (!moddingGO) return;

            var localVars = moddingGO.GetComponent<LocalNameVariables>();
            if (!localVars) return;

            // Access private m_Runtime field
            FieldInfo runtimeField = typeof(LocalNameVariables).GetField(
                "m_Runtime",
                BindingFlags.NonPublic | BindingFlags.Instance
            );

            if (runtimeField == null)
            {
                Debug.LogError("Couldn't find m_Runtime field");
                return;
            }

            var runtime = runtimeField.GetValue(localVars) as NameVariableRuntime;
            if (runtime == null) return;

            // Access internal Variables property
            PropertyInfo variablesProp = typeof(NameVariableRuntime).GetProperty(
                "Variables",
                BindingFlags.NonPublic | BindingFlags.Instance
            );

            if (variablesProp == null)
            {
                Debug.LogError("Couldn't find Variables property");
                return;
            }

            var variables = variablesProp.GetValue(runtime) as Dictionary<string, NameVariable>;
            if (variables == null) return;

            Debug.Log($"Found {variables.Count} variables on {moddingGO.name}:");
            foreach (var pair in variables)
            {
                object value = pair.Value?.Value;
                Debug.Log($"- {pair.Key} = {value} (Type: {value?.GetType()?.Name ?? "null"})");
            }
        }
        public void PrintAllGlobalVariables()
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

            if (valuesProp == null)
            {
                Debug.LogError("Couldn't find Values property");
                return;
            }

            var values = valuesProp.GetValue(manager) as Dictionary<IdString, NameVariableRuntime>;
            if (values == null) return;

            // Get VariablesRepository to map IdString back to assets
            var repo = TRepository<VariablesRepository>.Get;
            if (repo == null)
            {
                Debug.LogError("Couldn't find VariablesRepository");
                return;
            }

            foreach (var pair in values)
            {
                var asset = repo.Variables.GetNameVariablesAsset(pair.Key);
                string assetName = asset != null ? asset.name : $"Unknown Asset ({pair.Key})";

                Debug.Log($"Global Variable Set from " + manager.name + ": {assetName}");

                // Now access the NameVariableRuntime's variables
                PropertyInfo runtimeVarsProp = typeof(NameVariableRuntime).GetProperty(
                    "Variables",
                    BindingFlags.NonPublic | BindingFlags.Instance
                );

                if (runtimeVarsProp == null) continue;

                var variables = runtimeVarsProp.GetValue(pair.Value) as Dictionary<string, NameVariable>;
                if (variables == null) continue;

                // Create a sorted list of variables before printing
                var sortedVariables = variables.OrderBy(v =>
                {
                    // Try to parse the key as a number if it starts with a digit
                    if (v.Key.Length > 0 && char.IsDigit(v.Key[0]))
                    {
                        if (int.TryParse(v.Key, out int numericValue))
                            return numericValue;
                    }
                    return int.MaxValue; // Non-numeric keys go to the end
                }).ThenBy(v => v.Key); // Secondary sort by string value

                foreach (var varPair in sortedVariables)
                {
                    object value = varPair.Value?.Value;
                    Debug.Log($"- {varPair.Key} = {value} (Type: {value?.GetType()?.Name ?? "null"})");
                }
            }
        }
        public void FindAndModifyVariable(string variableNameToFind, int newValue)
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
        #endregion
    }
}
