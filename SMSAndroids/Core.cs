using BepInEx;
using GameCreator.Runtime.Common;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SMSAndroidsCore
{
    /// <summary>
    /// Root-transform cache and load chain for the massage minigame, the one part of the mod that is still C#.
    /// Everything else is SMSAndroidsPack's (docs/PLUGIN-TO-PACK-2026-11-02.md).
    /// </summary>
    [BepInPlugin(pluginGuid, Core.pluginName, Core.pluginVersion)]
    public class Core : BaseUnityPlugin
    {
        #region Plugin Info
        public const string pluginGuid = "treboy.starmakerstory.smsandroidscore.core";
        public const string pluginName = "Androids Core";
        public const string pluginVersion = "0.7.0";
        #endregion

        public static AssetBundle minigameBundle;
        public static bool loadedCore = false;
        public static bool loadedBases = false;
        public static GameObject mainCamera;
        public static Scene currentScene;
        public static string assetPath = "BepInEx\\plugins\\SMSAndroidsCore\\Assets\\";
        public static string minigamePath = "BepInEx\\plugins\\SMSAndroidsCore\\Minigame\\";
        public static string bustPath = "BepInEx\\plugins\\SMSAndroidsCore\\Busts\\";
        public static string exePath;
        public static Transform bustManager;
        public static Transform cGManagerSexy;
        public static Transform level;

        public void Awake()
        {
            exePath = Application.dataPath;
            if (Application.platform == RuntimePlatform.OSXPlayer)
            {
                exePath += "/../../";
            }
            else if (Application.platform == RuntimePlatform.WindowsPlayer)
            {
                exePath += "/../";
            }

            // Minigame bundle is optional — visual assets fall back to procedural if absent
            string minigameBundlePath = exePath + assetPath + "minigamebundle";
            if (System.IO.File.Exists(minigameBundlePath))
                minigameBundle = AssetBundle.LoadFromFile(minigameBundlePath);

            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        public void OnSceneLoaded(Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
        {
            currentScene = scene;
            loadedCore = false;
            loadedBases = false;
            Characters.loadedBusts = false;
            MainStory.loadedStory = false;
            Minigames.loadedMinigame = false;
            Schedule.loadedSchedule = false;
        }
        public void Update()
        {
            loadedBases = loadedCore && Characters.loadedBusts && MainStory.loadedStory;

            if (currentScene.name == "CoreGameScene")
            {
                if (!loadedCore)
                {
                    bustManager = GameObject.Find("2_Bust_Manager").transform;
                    cGManagerSexy = GameObject.Find("4_CG_Manager-Sexy").transform;
                    level = GameObject.Find("5_Levels").transform;
                    mainCamera = GameObject.Find("Main Camera");

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
    }
}
