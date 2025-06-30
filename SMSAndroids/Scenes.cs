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
    [BepInPlugin(pluginGuid, Core.pluginName + " - Scenes", Core.pluginVersion)]
    internal class Scenes : BaseUnityPlugin
    {
        #region Plugin Info
        public const string pluginGuid = "treboy.starmakerstory.smsandroidscore.scenes";
        #endregion

        public static GameObject amberStareScene1;

        #region Voyeur Scene Variables
        public static GameObject anisBeachScene1;
        public static GameObject anisBeachScene2;
        public static GameObject anisBeachScene3;
        public static GameObject anisBeachScene4;

        public static GameObject frimaBeachScene1;
        public static GameObject frimaBeachScene2;
        public static GameObject frimaBeachScene3;
        public static GameObject frimaBeachScene4;

        public static GameObject guiltyBeachScene1;
        public static GameObject guiltyBeachScene2;
        public static GameObject guiltyBeachScene3;
        public static GameObject guiltyBeachScene4;

        public static GameObject helmBeachScene1;
        public static GameObject helmBeachScene2;
        public static GameObject helmBeachScene3;
        public static GameObject helmBeachScene4;

        public static GameObject maidenBeachScene1;
        public static GameObject maidenBeachScene2;
        public static GameObject maidenBeachScene3;
        public static GameObject maidenBeachScene4;

        public static GameObject maryBeachScene1;
        public static GameObject maryBeachScene2;
        public static GameObject maryBeachScene3;
        public static GameObject maryBeachScene4;

        public static GameObject mastBeachScene1;
        public static GameObject mastBeachScene2;
        public static GameObject mastBeachScene3;
        public static GameObject mastBeachScene4;

        public static GameObject neonBeachScene1;
        public static GameObject neonBeachScene2;
        public static GameObject neonBeachScene3;
        public static GameObject neonBeachScene4;

        public static GameObject pepperBeachScene1;
        public static GameObject pepperBeachScene2;
        public static GameObject pepperBeachScene3;
        public static GameObject pepperBeachScene4;

        public static GameObject rapiBeachScene1;
        public static GameObject rapiBeachScene2;
        public static GameObject rapiBeachScene3;
        public static GameObject rapiBeachScene4;

        public static GameObject rosannaBeachScene1;
        public static GameObject rosannaBeachScene2;
        public static GameObject rosannaBeachScene3;
        public static GameObject rosannaBeachScene4;

        public static GameObject sakuraBeachScene1;
        public static GameObject sakuraBeachScene2;
        public static GameObject sakuraBeachScene3;
        public static GameObject sakuraBeachScene4;

        public static GameObject viperBeachScene1;
        public static GameObject viperBeachScene2;
        public static GameObject viperBeachScene3;
        public static GameObject viperBeachScene4;

        public static GameObject yanBeachScene1;
        public static GameObject yanBeachScene2;
        public static GameObject yanBeachScene3;
        public static GameObject yanBeachScene4;
        #endregion

        public static bool loadedScenes = false;
        public void Update()
        {
            if (Core.currentScene.name == "CoreGameScene")
            {
                if (!loadedScenes)
                {
                    amberStareScene1 = CreateNewPicScene("AmberStareScene01", Core.scenePath + "Amber\\AmberStareScene01.PNG");

                    anisBeachScene1 = CreateNewPicScene("AnisBeachScene01", Core.scenePath + "Anis\\AnisBeachScene01.PNG");
                    anisBeachScene2 = CreateNewPicScene("AnisBeachScene02", Core.scenePath + "Anis\\AnisBeachScene02.PNG");
                    anisBeachScene3 = CreateNewPicScene("AnisBeachScene03", Core.scenePath + "Anis\\AnisBeachScene03.PNG");
                    anisBeachScene4 = CreateNewPicScene("AnisBeachScene04", Core.scenePath + "Anis\\AnisBeachScene04.PNG");

                    frimaBeachScene1 = CreateNewPicScene("FrimaBeachScene01", Core.scenePath + "Frima\\FrimaBeachScene01.PNG");
                    frimaBeachScene2 = CreateNewPicScene("FrimaBeachScene02", Core.scenePath + "Frima\\FrimaBeachScene02.PNG");
                    frimaBeachScene3 = CreateNewPicScene("FrimaBeachScene03", Core.scenePath + "Frima\\FrimaBeachScene03.PNG");
                    frimaBeachScene4 = CreateNewPicScene("FrimaBeachScene04", Core.scenePath + "Frima\\FrimaBeachScene04.PNG");

                    guiltyBeachScene1 = CreateNewPicScene("GuiltyBeachScene01", Core.scenePath + "Guilty\\GuiltyBeachScene01.PNG");
                    guiltyBeachScene2 = CreateNewPicScene("GuiltyBeachScene02", Core.scenePath + "Guilty\\GuiltyBeachScene02.PNG");
                    guiltyBeachScene3 = CreateNewPicScene("GuiltyBeachScene03", Core.scenePath + "Guilty\\GuiltyBeachScene03.PNG");
                    guiltyBeachScene4 = CreateNewPicScene("GuiltyBeachScene04", Core.scenePath + "Guilty\\GuiltyBeachScene04.PNG");

                    helmBeachScene1 = CreateNewPicScene("HelmBeachScene01", Core.scenePath + "Helm\\HelmBeachScene01.PNG");
                    helmBeachScene2 = CreateNewPicScene("HelmBeachScene02", Core.scenePath + "Helm\\HelmBeachScene02.PNG");
                    helmBeachScene3 = CreateNewPicScene("HelmBeachScene03", Core.scenePath + "Helm\\HelmBeachScene03.PNG");
                    helmBeachScene4 = CreateNewPicScene("HelmBeachScene04", Core.scenePath + "Helm\\HelmBeachScene04.PNG");

                    maidenBeachScene1 = CreateNewPicScene("MaidenBeachScene01", Core.scenePath + "Maiden\\MaidenBeachScene01.PNG");
                    maidenBeachScene2 = CreateNewPicScene("MaidenBeachScene02", Core.scenePath + "Maiden\\MaidenBeachScene02.PNG");
                    maidenBeachScene3 = CreateNewPicScene("MaidenBeachScene03", Core.scenePath + "Maiden\\MaidenBeachScene03.PNG");
                    maidenBeachScene4 = CreateNewPicScene("MaidenBeachScene04", Core.scenePath + "Maiden\\MaidenBeachScene04.PNG");

                    maryBeachScene1 = CreateNewPicScene("MaryBeachScene01", Core.scenePath + "Mary\\MaryBeachScene01.PNG");
                    maryBeachScene2 = CreateNewPicScene("MaryBeachScene02", Core.scenePath + "Mary\\MaryBeachScene02.PNG");
                    maryBeachScene3 = CreateNewPicScene("MaryBeachScene03", Core.scenePath + "Mary\\MaryBeachScene03.PNG");
                    maryBeachScene4 = CreateNewPicScene("MaryBeachScene04", Core.scenePath + "Mary\\MaryBeachScene04.PNG");

                    mastBeachScene1 = CreateNewPicScene("MastBeachScene01", Core.scenePath + "Mast\\MastBeachScene01.PNG");
                    mastBeachScene2 = CreateNewPicScene("MastBeachScene02", Core.scenePath + "Mast\\MastBeachScene02.PNG");
                    mastBeachScene3 = CreateNewPicScene("MastBeachScene03", Core.scenePath + "Mast\\MastBeachScene03.PNG");
                    mastBeachScene4 = CreateNewPicScene("MastBeachScene04", Core.scenePath + "Mast\\MastBeachScene04.PNG");

                    neonBeachScene1 = CreateNewPicScene("NeonBeachScene01", Core.scenePath + "Neon\\NeonBeachScene01.PNG");
                    neonBeachScene2 = CreateNewPicScene("NeonBeachScene02", Core.scenePath + "Neon\\NeonBeachScene02.PNG");
                    neonBeachScene3 = CreateNewPicScene("NeonBeachScene03", Core.scenePath + "Neon\\NeonBeachScene03.PNG");
                    neonBeachScene4 = CreateNewPicScene("NeonBeachScene04", Core.scenePath + "Neon\\NeonBeachScene04.PNG");

                    pepperBeachScene1 = CreateNewPicScene("PepperBeachScene01", Core.scenePath + "Pepper\\PepperBeachScene01.PNG");
                    pepperBeachScene2 = CreateNewPicScene("PepperBeachScene02", Core.scenePath + "Pepper\\PepperBeachScene02.PNG");
                    pepperBeachScene3 = CreateNewPicScene("PepperBeachScene03", Core.scenePath + "Pepper\\PepperBeachScene03.PNG");
                    pepperBeachScene4 = CreateNewPicScene("PepperBeachScene04", Core.scenePath + "Pepper\\PepperBeachScene04.PNG");

                    rapiBeachScene1 = CreateNewPicScene("RapiBeachScene01", Core.scenePath + "Rapi\\RapiBeachScene01.PNG");
                    rapiBeachScene2 = CreateNewPicScene("RapiBeachScene02", Core.scenePath + "Rapi\\RapiBeachScene02.PNG");
                    rapiBeachScene3 = CreateNewPicScene("RapiBeachScene03", Core.scenePath + "Rapi\\RapiBeachScene03.PNG");
                    rapiBeachScene4 = CreateNewPicScene("RapiBeachScene04", Core.scenePath + "Rapi\\RapiBeachScene04.PNG");

                    rosannaBeachScene1 = CreateNewPicScene("RosannaBeachScene01", Core.scenePath + "Rosanna\\RosannaBeachScene01.PNG");
                    rosannaBeachScene2 = CreateNewPicScene("RosannaBeachScene02", Core.scenePath + "Rosanna\\RosannaBeachScene02.PNG");
                    rosannaBeachScene3 = CreateNewPicScene("RosannaBeachScene03", Core.scenePath + "Rosanna\\RosannaBeachScene03.PNG");
                    rosannaBeachScene4 = CreateNewPicScene("RosannaBeachScene04", Core.scenePath + "Rosanna\\RosannaBeachScene04.PNG");

                    sakuraBeachScene1 = CreateNewPicScene("SakuraBeachScene01", Core.scenePath + "Sakura\\SakuraBeachScene01.PNG");
                    sakuraBeachScene2 = CreateNewPicScene("SakuraBeachScene02", Core.scenePath + "Sakura\\SakuraBeachScene02.PNG");
                    sakuraBeachScene3 = CreateNewPicScene("SakuraBeachScene03", Core.scenePath + "Sakura\\SakuraBeachScene03.PNG");
                    sakuraBeachScene4 = CreateNewPicScene("SakuraBeachScene04", Core.scenePath + "Sakura\\SakuraBeachScene04.PNG");

                    viperBeachScene1 = CreateNewPicScene("ViperBeachScene01", Core.scenePath + "Viper\\ViperBeachScene01.PNG");
                    viperBeachScene2 = CreateNewPicScene("ViperBeachScene02", Core.scenePath + "Viper\\ViperBeachScene02.PNG");
                    viperBeachScene3 = CreateNewPicScene("ViperBeachScene03", Core.scenePath + "Viper\\ViperBeachScene03.PNG");
                    viperBeachScene4 = CreateNewPicScene("ViperBeachScene04", Core.scenePath + "Viper\\ViperBeachScene04.PNG");

                    yanBeachScene1 = CreateNewPicScene("YanBeachScene01", Core.scenePath + "Yan\\YanBeachScene01.PNG");
                    yanBeachScene2 = CreateNewPicScene("YanBeachScene02", Core.scenePath + "Yan\\YanBeachScene02.PNG");
                    yanBeachScene3 = CreateNewPicScene("YanBeachScene03", Core.scenePath + "Yan\\YanBeachScene03.PNG");
                    yanBeachScene4 = CreateNewPicScene("YanBeachScene04", Core.scenePath + "Yan\\YanBeachScene04.PNG");

                    Logger.LogInfo("----- SCENES LOADED -----");
                    loadedScenes = true;
                }
                if (Core.currentScene.name == "GameStart")
                {
                    if (loadedScenes)
                    {
                        Logger.LogInfo("----- SCENES UNLOADED -----");
                        loadedScenes = false;
                    }
                }
            }
        }

        public GameObject CreateNewPicScene(string name, string pathToCG)
        {
            GameObject newPicScene = GameObject.Instantiate(Core.cGManagerSexy.Find("Samanthabeach").gameObject, Core.cGManagerSexy);
            newPicScene.name = name;
            GameObject core = newPicScene.transform.Find("Core").gameObject;
            GameObject art = core.transform.Find("Art").gameObject;

            var rawData = System.IO.File.ReadAllBytes(pathToCG);
            Texture2D tex = new Texture2D(256, 256);
            tex.filterMode = FilterMode.Point;
            ImageConversion.LoadImage(tex, rawData);
            Sprite newSprite = Sprite.Create(tex, new Rect(0, 0, 256, 256), new Vector2(0.5f, 0.5f));
            art.GetComponent<SpriteRenderer>().sprite = newSprite;

            Core.cGManagerSexy.GetComponent<SpriteRendererLayoutManager>().targetObjects.Add(newPicScene);

            newPicScene.SetActive(false);
            return newPicScene;
        }

        public static void DialogueScenePlayer(Transform cGManager, string baseSceneName, GameObject baseDialogue)
        {
            if (!cGManager.Find(baseSceneName + "Scene01").gameObject.activeSelf && baseDialogue.transform.Find("Scene1").gameObject.activeSelf)
            {
                cGManager.Find(baseSceneName + "Scene01").gameObject.SetActive(true);
            }
            if (!cGManager.Find(baseSceneName + "Scene02").gameObject.activeSelf && baseDialogue.transform.Find("Scene2").gameObject.activeSelf)
            {
                cGManager.Find(baseSceneName + "Scene02").gameObject.SetActive(true);
            }
            if (!cGManager.Find(baseSceneName + "Scene03").gameObject.activeSelf && baseDialogue.transform.Find("Scene3").gameObject.activeSelf)
            {
                cGManager.Find(baseSceneName + "Scene03").gameObject.SetActive(true);
            }
            if (!cGManager.Find(baseSceneName + "Scene04").gameObject.activeSelf && baseDialogue.transform.Find("Scene4").gameObject.activeSelf)
            {
                if (baseDialogue.transform.Find("Scene1").gameObject.activeSelf)
                {
                    baseDialogue.transform.Find("Scene1").gameObject.SetActive(false);
                    cGManager.Find(baseSceneName + "Scene01").gameObject.SetActive(false);
                }
                cGManager.Find(baseSceneName + "Scene04").gameObject.SetActive(true);
            }
        }
    }
}
