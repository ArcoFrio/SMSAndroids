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
    internal class Characters : BaseUnityPlugin
    {
        #region Plugin Info
        public const string pluginGuid = "treboy.starmakerstory.smsandroidscore.characters";
        #endregion

        public static GameObject anis;
        public static GameObject anisSwim;
        public static GameObject anisSwimWet;
        public static GameObject anisSwimSlip;

        public static GameObject frima;
        public static GameObject frimaSwim;
        public static GameObject frimaSwimShirtless;
        public static GameObject frimaSwimSlip;

        public static GameObject guilty;
        public static GameObject guiltySwim;
        public static GameObject guiltySwimSlip;

        public static GameObject helm;
        public static GameObject helmSwim;
        public static GameObject helmSwimWet;
        public static GameObject helmSwimShirtless;
        public static GameObject helmSwimSlip;

        public static GameObject maiden;
        public static GameObject maidenSwim;
        public static GameObject maidenSwimSlip;

        public static GameObject mary;
        public static GameObject marySwim;
        public static GameObject marySwimSlip;

        public static GameObject mast;
        public static GameObject mastSwim;
        public static GameObject mastSwimSlip;

        public static GameObject neon;
        public static GameObject neonSwim;
        public static GameObject neonSwimWet;
        public static GameObject neonSwimSlip;

        public static GameObject pepper;
        public static GameObject pepperSwim;
        public static GameObject pepperSwimSlip;

        public static GameObject rapi;
        public static GameObject rapiSwim;
        public static GameObject rapiSwimSlip;

        public static GameObject rosanna;
        public static GameObject rosannaSwim;
        public static GameObject rosannaSwimSlip;

        public static GameObject sakura;
        public static GameObject sakuraSwim;
        public static GameObject sakuraSwimSlip;

        public static GameObject viper;
        public static GameObject viperSwim;
        public static GameObject viperSwimShirtless;
        public static GameObject viperSwimWet;
        public static GameObject viperSwimSlip;

        public static GameObject yan;
        public static GameObject yanSwim;
        public static GameObject yanSwimSlip;

        public static bool loadedBusts = false;

        public void Update()
        {
            if (Core.currentScene.name == "CoreGameScene")
            {
                if (!loadedBusts && Core.loadedCore)
                {
                    // anis = CreateNewBust("AnisBase", Core.bustPath, "Anis\\Anis00.PNG", "Anis\\AnisBlink.PNG", "Anis\\Anis00Mask.PNG", "Anis\\AnisMouth");
                    anisSwim = CreateNewBust("AnisSwim", Core.bustPath, "Anis\\AnisSwim00.PNG", "Anis\\AnisBlink.PNG", "Anis\\AnisSwim00Mask.PNG", "Anis\\AnisMouth");
                    anisSwimWet = CreateNewBust("AnisSwimWet", Core.bustPath, "Anis\\AnisSwim01.PNG", "Anis\\AnisBlink.PNG", "Anis\\AnisSwim01Mask.PNG", "Anis\\AnisMouth");
                    anisSwimSlip = CreateNewBust("AnisSwimSlip", Core.bustPath, "Anis\\AnisSwim02.PNG", "Anis\\AnisBlink.PNG", "Anis\\AnisSwim02Mask.PNG", "Anis\\AnisMouth");

                    // frima = CreateNewBust("FrimaBase", Core.bustPath, "Frima\\Frima00.PNG", "Frima\\FrimaBlink.PNG", "Frima\\Frima00Mask.PNG", "Frima\\FrimaMouth");
                    frimaSwim = CreateNewBust("FrimaSwim", Core.bustPath, "Frima\\FrimaSwim00.PNG", "Frima\\FrimaBlink.PNG", "Frima\\FrimaSwim00Mask.PNG", "Frima\\FrimaMouth");
                    frimaSwimShirtless = CreateNewBust("FrimaSwimShirtless", Core.bustPath, "Frima\\FrimaSwim01.PNG", "Frima\\FrimaBlink.PNG", "Frima\\FrimaSwim01Mask.PNG", "Frima\\FrimaMouth");
                    frimaSwimSlip = CreateNewBust("FrimaSwimSlip", Core.bustPath, "Frima\\FrimaSwim02.PNG", "Frima\\FrimaBlink.PNG", "Frima\\FrimaSwim02Mask.PNG", "Frima\\FrimaMouth");

                    // guilty = CreateNewBust("GuiltyBase", Core.bustPath, "Guilty\\Guilty00.PNG", "Guilty\\GuiltyBlink.PNG", "Guilty\\Guilty00Mask.PNG", "Guilty\\GuiltyMouth");
                    guiltySwim = CreateNewBust("GuiltySwim", Core.bustPath, "Guilty\\GuiltySwim00.PNG", "Guilty\\GuiltyBlink.PNG", "Guilty\\GuiltySwim00Mask.PNG", "Guilty\\GuiltyMouth");
                    guiltySwimSlip = CreateNewBust("GuiltySwimSlip", Core.bustPath, "Guilty\\GuiltySwim01.PNG", "Guilty\\GuiltyBlink.PNG", "Guilty\\GuiltySwim01Mask.PNG", "Guilty\\GuiltyMouth");

                    // helm = CreateNewBust("HelmBase", Core.bustPath, "Helm\\Helm00.PNG", "Helm\\HelmBlink.PNG", "Helm\\Helm00Mask.PNG", "Helm\\HelmMouth");
                    helmSwim = CreateNewBust("HelmSwim", Core.bustPath, "Helm\\HelmSwim00.PNG", "Helm\\HelmBlink.PNG", "Helm\\HelmSwim00Mask.PNG", "Helm\\HelmMouth");
                    helmSwimWet = CreateNewBust("HelmSwimWet", Core.bustPath, "Helm\\HelmSwim01.PNG", "Helm\\HelmBlink.PNG", "Helm\\HelmSwim01Mask.PNG", "Helm\\HelmMouth");
                    helmSwimShirtless = CreateNewBust("HelmSwimShirtless", Core.bustPath, "Helm\\HelmSwim02.PNG", "Helm\\HelmBlink.PNG", "Helm\\HelmSwim02Mask.PNG", "Helm\\HelmMouth");
                    helmSwimSlip = CreateNewBust("HelmSwimSlip", Core.bustPath, "Helm\\HelmSwim03.PNG", "Helm\\HelmBlink.PNG", "Helm\\HelmSwim03Mask.PNG", "Helm\\HelmMouth");

                    // maiden = CreateNewBust("MaidenBase", Core.bustPath, "Maiden\\Maiden00.PNG", "Maiden\\MaidenBlink.PNG", "Maiden\\Maiden00Mask.PNG", "Maiden\\MaidenMouth");
                    maidenSwim = CreateNewBust("MaidenSwim", Core.bustPath, "Maiden\\MaidenSwim00.PNG", "Maiden\\MaidenBlink.PNG", "Maiden\\MaidenSwim00Mask.PNG", "Maiden\\MaidenMouth");
                    maidenSwimSlip = CreateNewBust("MaidenSwimSlip", Core.bustPath, "Maiden\\MaidenSwim01.PNG", "Maiden\\MaidenBlink.PNG", "Maiden\\MaidenSwim01Mask.PNG", "Maiden\\MaidenMouth");

                    // mary = CreateNewBust("MaryBase", Core.bustPath, "Mary\\Mary00.PNG", "Mary\\MaryBlink.PNG", "Mary\\Mary00Mask.PNG", "Mary\\MaryMouth");
                    marySwim = CreateNewBust("MarySwim", Core.bustPath, "Mary\\MarySwim00.PNG", "Mary\\MaryBlink.PNG", "Mary\\MarySwim00Mask.PNG", "Mary\\MaryMouth");
                    marySwimSlip = CreateNewBust("MarySwimSlip", Core.bustPath, "Mary\\MarySwim01.PNG", "Mary\\MaryBlink.PNG", "Mary\\MarySwim01Mask.PNG", "Mary\\MaryMouth");

                    // mast = CreateNewBust("MastBase", Core.bustPath, "Mast\\Mast00.PNG", "Mast\\MastBlink.PNG", "Mast\\Mast00Mask.PNG", "Mast\\MastMouth");
                    mastSwim = CreateNewBust("MastSwim", Core.bustPath, "Mast\\MastSwim00.PNG", "Mast\\MastBlink.PNG", "Mast\\MastSwim00Mask.PNG", "Mast\\MastMouth");
                    mastSwimSlip = CreateNewBust("MastSwimSlip", Core.bustPath, "Mast\\MastSwim01.PNG", "Mast\\MastBlink.PNG", "Mast\\MastSwim01Mask.PNG", "Mast\\MastMouth");

                    // neon = CreateNewBust("NeonBase", Core.bustPath, "Neon\\Neon00.PNG", "Neon\\NeonBlink.PNG", "Neon\\Neon00Mask.PNG", "Neon\\NeonMouth");
                    neonSwim = CreateNewBust("NeonSwim", Core.bustPath, "Neon\\NeonSwim00.PNG", "Neon\\NeonBlink.PNG", "Neon\\NeonSwim00Mask.PNG", "Neon\\NeonMouth");
                    neonSwimWet = CreateNewBust("NeonSwimWet", Core.bustPath, "Neon\\NeonSwim01.PNG", "Neon\\NeonBlink.PNG", "Neon\\NeonSwim01Mask.PNG", "Neon\\NeonMouth");
                    neonSwimSlip = CreateNewBust("NeonSwimSlip", Core.bustPath, "Neon\\NeonSwim02.PNG", "Neon\\NeonBlink.PNG", "Neon\\NeonSwim02Mask.PNG", "Neon\\NeonMouth");

                    // pepper = CreateNewBust("PepperBase", Core.bustPath, "Pepper\\Pepper00.PNG", "Pepper\\PepperBlink.PNG", "Pepper\\Pepper00Mask.PNG", "Pepper\\PepperMouth");
                    pepperSwim = CreateNewBust("PepperSwim", Core.bustPath, "Pepper\\PepperSwim00.PNG", "Pepper\\PepperBlink.PNG", "Pepper\\PepperSwim00Mask.PNG", "Pepper\\PepperMouth");
                    pepperSwimSlip = CreateNewBust("PepperSwimSlip", Core.bustPath, "Pepper\\PepperSwim01.PNG", "Pepper\\PepperBlink.PNG", "Pepper\\PepperSwim01Mask.PNG", "Pepper\\PepperMouth");

                    // rapi = CreateNewBust("RapiBase", Core.bustPath, "Rapi\\Rapi00.PNG", "Rapi\\RapiBlink.PNG", "Rapi\\Rapi00Mask.PNG", "Rapi\\RapiMouth");
                    rapiSwim = CreateNewBust("RapiSwim", Core.bustPath, "Rapi\\RapiSwim00.PNG", "Rapi\\RapiBlink.PNG", "Rapi\\RapiSwim00Mask.PNG", "Rapi\\RapiMouth");
                    rapiSwimSlip = CreateNewBust("RapiSwimSlip", Core.bustPath, "Rapi\\RapiSwim01.PNG", "Rapi\\RapiBlink.PNG", "Rapi\\RapiSwim01Mask.PNG", "Rapi\\RapiMouth");

                    // rosanna = CreateNewBust("RosannaBase", Core.bustPath, "Rosanna\\Rosanna00.PNG", "Rosanna\\RosannaBlink.PNG", "Rosanna\\Rosanna00Mask.PNG", "Rosanna\\RosannaMouth");
                    rosannaSwim = CreateNewBust("RosannaSwim", Core.bustPath, "Rosanna\\RosannaSwim00.PNG", "Rosanna\\RosannaBlink.PNG", "Rosanna\\RosannaSwim00Mask.PNG", "Rosanna\\RosannaMouth");
                    rosannaSwimSlip = CreateNewBust("RosannaSwimSlip", Core.bustPath, "Rosanna\\RosannaSwim01.PNG", "Rosanna\\RosannaBlink.PNG", "Rosanna\\RosannaSwim01Mask.PNG", "Rosanna\\RosannaMouth");

                    // sakura = CreateNewBust("SakuraBase", Core.bustPath, "Sakura\\Sakura00.PNG", "Sakura\\SakuraBlink.PNG", "Sakura\\Sakura00Mask.PNG", "Sakura\\SakuraMouth");
                    sakuraSwim = CreateNewBust("SakuraSwim", Core.bustPath, "Sakura\\SakuraSwim00.PNG", "Sakura\\SakuraBlink.PNG", "Sakura\\SakuraSwim00Mask.PNG", "Sakura\\SakuraMouth");
                    sakuraSwimSlip = CreateNewBust("SakuraSwimSlip", Core.bustPath, "Sakura\\SakuraSwim01.PNG", "Sakura\\SakuraBlink.PNG", "Sakura\\SakuraSwim01Mask.PNG", "Sakura\\SakuraMouth");

                    // viper = CreateNewBust("ViperBase", Core.bustPath, "Viper\\Viper00.PNG", "Viper\\ViperBlink.PNG", "Viper\\Viper00Mask.PNG", "Viper\\ViperMouth");
                    viperSwim = CreateNewBust("ViperSwim", Core.bustPath, "Viper\\ViperSwim00.PNG", "Viper\\ViperBlink.PNG", "Viper\\ViperSwim00Mask.PNG", "Viper\\ViperMouth");
                    viperSwimShirtless = CreateNewBust("ViperSwimShirtless", Core.bustPath, "Viper\\ViperSwim01.PNG", "Viper\\ViperBlink.PNG", "Viper\\ViperSwim01Mask.PNG", "Viper\\ViperMouth");
                    viperSwimWet = CreateNewBust("ViperSwimWet", Core.bustPath, "Viper\\ViperSwim02.PNG", "Viper\\ViperBlink.PNG", "Viper\\ViperSwim02Mask.PNG", "Viper\\ViperMouth");
                    viperSwimSlip = CreateNewBust("ViperSwimSlip", Core.bustPath, "Viper\\ViperSwim03.PNG", "Viper\\ViperBlink.PNG", "Viper\\ViperSwim03Mask.PNG", "Viper\\ViperMouth");

                    // yan = CreateNewBust("YanBase", Core.bustPath, "Yan\\Yan00.PNG", "Yan\\YanBlink.PNG", "Yan\\Yan00Mask.PNG", "Yan\\YanMouth");
                    yanSwim = CreateNewBust("YanSwim", Core.bustPath, "Yan\\YanSwim00.PNG", "Yan\\YanBlink.PNG", "Yan\\YanSwim00Mask.PNG", "Yan\\YanMouth");
                    yanSwimSlip = CreateNewBust("YanSwimSlip", Core.bustPath, "Yan\\YanSwim01.PNG", "Yan\\YanBlink.PNG", "Yan\\YanSwim01Mask.PNG", "Yan\\YanMouth");

                    Logger.LogInfo("----- BUSTS LOADED -----");
                    loadedBusts = true;
                }
            }
            if (Core.currentScene.name == "GameStart")
            {
                if (loadedBusts)
                {
                    Logger.LogInfo("----- BUSTS UNLOADED -----");
                    loadedBusts = false;
                }
            }
        }


        public GameObject CreateNewBust(string name, string pathToCG, string baseSprite, string blinkSprite, string maskSprite, string mouthSprite)
        {
            GameObject newBust = GameObject.Instantiate(Core.baseBust, Core.bustManager);
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

            Core.bustManager.GetComponent<SpriteRendererLayoutManager>().targetObjects.Add(newBust);

            newBust.SetActive(false);
            return newBust;
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
    }
}
