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
    [BepInPlugin(pluginGuid, Core.pluginName + " - Characters", Core.pluginVersion)]
    internal class Characters : BaseUnityPlugin
    {
        #region Plugin Info
        public const string pluginGuid = "treboy.starmakerstory.smsandroidscore.characters";
        #endregion

        public static GameObject amberSwim;



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
                    amberSwim = CreateNewBust("AmberSwim", Core.bustPath, "Amber\\AmberSwim00.PNG", "Amber\\AmberBlink.PNG", "Amber\\AmberSwim00Mask.PNG", "Amber\\AmberMouth");

                    anis = CreateNewBust("AnisBase", Core.bustNikkePath, "Anis\\Anis00.PNG", "Anis\\AnisBlink.PNG", "Anis\\Anis00Mask.PNG", "Anis\\AnisMouth");
                    anisSwim = CreateNewBust("AnisSwim", Core.bustNikkePath, "Anis\\AnisSwim00.PNG", "Anis\\AnisBlink.PNG", "Anis\\AnisSwim00Mask.PNG", "Anis\\AnisMouth");
                    anisSwimWet = CreateNewBust("AnisSwimWet", Core.bustNikkePath, "Anis\\AnisSwim01.PNG", "Anis\\AnisBlink.PNG", "Anis\\AnisSwim01Mask.PNG", "Anis\\AnisMouth");
                    anisSwimWet.transform.Find("MBase1").Find("Wet").gameObject.SetActive(true);
                    anisSwimSlip = CreateNewBust("AnisSwimSlip", Core.bustNikkePath, "Anis\\AnisSwim02.PNG", "Anis\\AnisBlink.PNG", "Anis\\AnisSwim02Mask.PNG", "Anis\\AnisMouth");
                    anisSwimSlip.transform.Find("MBase1").Find("Wet").gameObject.SetActive(true);

                    frima = CreateNewBust("FrimaBase", Core.bustNikkePath, "Frima\\Frima00.PNG", "Frima\\FrimaBlink.PNG", "Frima\\Frima00Mask.PNG", "Frima\\FrimaMouth");
                    frimaSwim = CreateNewBust("FrimaSwim", Core.bustNikkePath, "Frima\\FrimaSwim00.PNG", "Frima\\FrimaBlink.PNG", "Frima\\FrimaSwim00Mask.PNG", "Frima\\FrimaMouth");
                    frimaSwimShirtless = CreateNewBust("FrimaSwimShirtless", Core.bustNikkePath, "Frima\\FrimaSwim01.PNG", "Frima\\FrimaBlink.PNG", "Frima\\FrimaSwim01Mask.PNG", "Frima\\FrimaMouth");
                    frimaSwimSlip = CreateNewBust("FrimaSwimSlip", Core.bustNikkePath, "Frima\\FrimaSwim02.PNG", "Frima\\FrimaBlink.PNG", "Frima\\FrimaSwim02Mask.PNG", "Frima\\FrimaMouth");
                    frimaSwimSlip.transform.Find("MBase1").Find("Wet").gameObject.SetActive(true);

                    guilty = CreateNewBust("GuiltyBase", Core.bustNikkePath, "Guilty\\Guilty00.PNG", "Guilty\\GuiltyBlink.PNG", "Guilty\\Guilty00Mask.PNG", "Guilty\\GuiltyMouth");
                    guiltySwim = CreateNewBust("GuiltySwim", Core.bustNikkePath, "Guilty\\GuiltySwim00.PNG", "Guilty\\GuiltyBlink.PNG", "Guilty\\GuiltySwim00Mask.PNG", "Guilty\\GuiltyMouth");
                    guiltySwimSlip = CreateNewBust("GuiltySwimSlip", Core.bustNikkePath, "Guilty\\GuiltySwim01.PNG", "Guilty\\GuiltyBlink.PNG", "Guilty\\GuiltySwim01Mask.PNG", "Guilty\\GuiltyMouth");
                    guiltySwimSlip.transform.Find("MBase1").Find("Wet").gameObject.SetActive(true);

                    helm = CreateNewBust("HelmBase", Core.bustNikkePath, "Helm\\Helm00.PNG", "Helm\\HelmBlink.PNG", "Helm\\Helm00Mask.PNG", "Helm\\HelmMouth");
                    helmSwim = CreateNewBust("HelmSwim", Core.bustNikkePath, "Helm\\HelmSwim00.PNG", "Helm\\HelmBlink.PNG", "Helm\\HelmSwim00Mask.PNG", "Helm\\HelmMouth");
                    helmSwimWet = CreateNewBust("HelmSwimWet", Core.bustNikkePath, "Helm\\HelmSwim01.PNG", "Helm\\HelmBlink.PNG", "Helm\\HelmSwim01Mask.PNG", "Helm\\HelmMouth");
                    helmSwimWet.transform.Find("MBase1").Find("Wet").gameObject.SetActive(true);
                    helmSwimShirtless = CreateNewBust("HelmSwimShirtless", Core.bustNikkePath, "Helm\\HelmSwim02.PNG", "Helm\\HelmBlink.PNG", "Helm\\HelmSwim02Mask.PNG", "Helm\\HelmMouth");
                    helmSwimShirtless.transform.Find("MBase1").Find("Wet").gameObject.SetActive(true);
                    helmSwimSlip = CreateNewBust("HelmSwimSlip", Core.bustNikkePath, "Helm\\HelmSwim03.PNG", "Helm\\HelmBlink.PNG", "Helm\\HelmSwim03Mask.PNG", "Helm\\HelmMouth");
                    helmSwimSlip.transform.Find("MBase1").Find("Wet").gameObject.SetActive(true);

                    maiden = CreateNewBust("MaidenBase", Core.bustNikkePath, "Maiden\\Maiden00.PNG", "Maiden\\MaidenBlink.PNG", "Maiden\\Maiden00Mask.PNG", "Maiden\\MaidenMouth");
                    maidenSwim = CreateNewBust("MaidenSwim", Core.bustNikkePath, "Maiden\\MaidenSwim00.PNG", "Maiden\\MaidenBlink.PNG", "Maiden\\MaidenSwim00Mask.PNG", "Maiden\\MaidenMouth");
                    maidenSwimSlip = CreateNewBust("MaidenSwimSlip", Core.bustNikkePath, "Maiden\\MaidenSwim01.PNG", "Maiden\\MaidenBlink.PNG", "Maiden\\MaidenSwim01Mask.PNG", "Maiden\\MaidenMouth");
                    maidenSwimSlip.transform.Find("MBase1").Find("Wet").gameObject.SetActive(true);

                    mary = CreateNewBust("MaryBase", Core.bustNikkePath, "Mary\\Mary00.PNG", "Mary\\MaryBlink.PNG", "Mary\\Mary00Mask.PNG", "Mary\\MaryMouth");
                    marySwim = CreateNewBust("MarySwim", Core.bustNikkePath, "Mary\\MarySwim00.PNG", "Mary\\MaryBlink.PNG", "Mary\\MarySwim00Mask.PNG", "Mary\\MaryMouth");
                    marySwimSlip = CreateNewBust("MarySwimSlip", Core.bustNikkePath, "Mary\\MarySwim01.PNG", "Mary\\MaryBlink.PNG", "Mary\\MarySwim01Mask.PNG", "Mary\\MaryMouth");

                    mast = CreateNewBust("MastBase", Core.bustNikkePath, "Mast\\Mast00.PNG", "Mast\\MastBlink.PNG", "Mast\\Mast00Mask.PNG", "Mast\\MastMouth");
                    mastSwim = CreateNewBust("MastSwim", Core.bustNikkePath, "Mast\\MastSwim00.PNG", "Mast\\MastBlink.PNG", "Mast\\MastSwim00Mask.PNG", "Mast\\MastMouth");
                    mastSwimSlip = CreateNewBust("MastSwimSlip", Core.bustNikkePath, "Mast\\MastSwim01.PNG", "Mast\\MastBlink.PNG", "Mast\\MastSwim01Mask.PNG", "Mast\\MastMouth");

                    neon = CreateNewBust("NeonBase", Core.bustNikkePath, "Neon\\Neon00.PNG", "Neon\\NeonBlink.PNG", "Neon\\Neon00Mask.PNG", "Neon\\NeonMouth");
                    neonSwim = CreateNewBust("NeonSwim", Core.bustNikkePath, "Neon\\NeonSwim00.PNG", "Neon\\NeonBlink.PNG", "Neon\\NeonSwim00Mask.PNG", "Neon\\NeonMouth");
                    neonSwimWet = CreateNewBust("NeonSwimWet", Core.bustNikkePath, "Neon\\NeonSwim01.PNG", "Neon\\NeonBlink.PNG", "Neon\\NeonSwim01Mask.PNG", "Neon\\NeonMouth");
                    neonSwimWet.transform.Find("MBase1").Find("Wet").gameObject.SetActive(true);
                    neonSwimSlip = CreateNewBust("NeonSwimSlip", Core.bustNikkePath, "Neon\\NeonSwim02.PNG", "Neon\\NeonBlink.PNG", "Neon\\NeonSwim02Mask.PNG", "Neon\\NeonMouth");
                    neonSwimSlip.transform.Find("MBase1").Find("Wet").gameObject.SetActive(true);

                    pepper = CreateNewBust("PepperBase", Core.bustNikkePath, "Pepper\\Pepper00.PNG", "Pepper\\PepperBlink.PNG", "Pepper\\Pepper00Mask.PNG", "Pepper\\PepperMouth");
                    pepperSwim = CreateNewBust("PepperSwim", Core.bustNikkePath, "Pepper\\PepperSwim00.PNG", "Pepper\\PepperBlink.PNG", "Pepper\\PepperSwim00Mask.PNG", "Pepper\\PepperMouth");
                    pepperSwimSlip = CreateNewBust("PepperSwimSlip", Core.bustNikkePath, "Pepper\\PepperSwim01.PNG", "Pepper\\PepperBlink.PNG", "Pepper\\PepperSwim01Mask.PNG", "Pepper\\PepperMouth");

                    rapi = CreateNewBust("RapiBase", Core.bustNikkePath, "Rapi\\Rapi00.PNG", "Rapi\\RapiBlink.PNG", "Rapi\\Rapi00Mask.PNG", "Rapi\\RapiMouth");
                    rapiSwim = CreateNewBust("RapiSwim", Core.bustNikkePath, "Rapi\\RapiSwim00.PNG", "Rapi\\RapiBlink.PNG", "Rapi\\RapiSwim00Mask.PNG", "Rapi\\RapiMouth");
                    rapiSwimSlip = CreateNewBust("RapiSwimSlip", Core.bustNikkePath, "Rapi\\RapiSwim01.PNG", "Rapi\\RapiBlink.PNG", "Rapi\\RapiSwim01Mask.PNG", "Rapi\\RapiMouth");
                    rapiSwimSlip.transform.Find("MBase1").Find("Wet").gameObject.SetActive(true);

                    rosanna = CreateNewBust("RosannaBase", Core.bustNikkePath, "Rosanna\\Rosanna00.PNG", "Rosanna\\RosannaBlink.PNG", "Rosanna\\Rosanna00Mask.PNG", "Rosanna\\RosannaMouth");
                    rosannaSwim = CreateNewBust("RosannaSwim", Core.bustNikkePath, "Rosanna\\RosannaSwim00.PNG", "Rosanna\\RosannaBlink.PNG", "Rosanna\\RosannaSwim00Mask.PNG", "Rosanna\\RosannaMouth");
                    rosannaSwimSlip = CreateNewBust("RosannaSwimSlip", Core.bustNikkePath, "Rosanna\\RosannaSwim01.PNG", "Rosanna\\RosannaBlink.PNG", "Rosanna\\RosannaSwim01Mask.PNG", "Rosanna\\RosannaMouth");

                    sakura = CreateNewBust("SakuraBase", Core.bustNikkePath, "Sakura\\Sakura00.PNG", "Sakura\\SakuraBlink.PNG", "Sakura\\Sakura00Mask.PNG", "Sakura\\SakuraMouth");
                    sakuraSwim = CreateNewBust("SakuraSwim", Core.bustNikkePath, "Sakura\\SakuraSwim00.PNG", "Sakura\\SakuraBlink.PNG", "Sakura\\SakuraSwim00Mask.PNG", "Sakura\\SakuraMouth");
                    sakuraSwimSlip = CreateNewBust("SakuraSwimSlip", Core.bustNikkePath, "Sakura\\SakuraSwim01.PNG", "Sakura\\SakuraBlink.PNG", "Sakura\\SakuraSwim01Mask.PNG", "Sakura\\SakuraMouth");

                    viper = CreateNewBust("ViperBase", Core.bustNikkePath, "Viper\\Viper00.PNG", "Viper\\ViperBlink.PNG", "Viper\\Viper00Mask.PNG", "Viper\\ViperMouth");
                    viperSwim = CreateNewBust("ViperSwim", Core.bustNikkePath, "Viper\\ViperSwim00.PNG", "Viper\\ViperBlink.PNG", "Viper\\ViperSwim00Mask.PNG", "Viper\\ViperMouth");
                    viperSwimShirtless = CreateNewBust("ViperSwimShirtless", Core.bustNikkePath, "Viper\\ViperSwim01.PNG", "Viper\\ViperBlink.PNG", "Viper\\ViperSwim01Mask.PNG", "Viper\\ViperMouth");
                    viperSwimWet = CreateNewBust("ViperSwimWet", Core.bustNikkePath, "Viper\\ViperSwim02.PNG", "Viper\\ViperBlink.PNG", "Viper\\ViperSwim02Mask.PNG", "Viper\\ViperMouth");
                    viperSwimWet.transform.Find("MBase1").Find("Wet").gameObject.SetActive(true);
                    viperSwimSlip = CreateNewBust("ViperSwimSlip", Core.bustNikkePath, "Viper\\ViperSwim03.PNG", "Viper\\ViperBlink.PNG", "Viper\\ViperSwim03Mask.PNG", "Viper\\ViperMouth");
                    viperSwimSlip.transform.Find("MBase1").Find("Wet").gameObject.SetActive(true);

                    yan = CreateNewBust("YanBase", Core.bustNikkePath, "Yan\\Yan00.PNG", "Yan\\YanBlink.PNG", "Yan\\Yan00Mask.PNG", "Yan\\YanMouth");
                    yanSwim = CreateNewBust("YanSwim", Core.bustNikkePath, "Yan\\YanSwim00.PNG", "Yan\\YanBlink.PNG", "Yan\\YanSwim00Mask.PNG", "Yan\\YanMouth");
                    yanSwimSlip = CreateNewBust("YanSwimSlip", Core.bustNikkePath, "Yan\\YanSwim01.PNG", "Yan\\YanBlink.PNG", "Yan\\YanSwim01Mask.PNG", "Yan\\YanMouth");

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

            GameObject wetParticles = GameObject.Instantiate(Core.bustManager.Find("Anna_Towel").Find("MBase1").Find("Particle System").gameObject, mBase.transform);
            wetParticles.name = "Wet";
            wetParticles.SetActive(false);

            Core.bustManager.GetComponent<SpriteRendererLayoutManager>().targetObjects.Add(newBust);

            newBust.SetActive(false);
            return newBust;
        }
    }
}
