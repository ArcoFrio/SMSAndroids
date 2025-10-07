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

        public static GameObject amber;
        public static GameObject amberCoatless;
        public static GameObject amberSwim;



        public static GameObject anis;
        public static GameObject anisSwim;
        public static GameObject anisSwimWet;
        public static GameObject anisSwimSlip;

        public static GameObject centi;
        public static GameObject centiSwim;
        public static GameObject centiSwimShirtless;
        public static GameObject centiSwimSlip;

        public static GameObject dorothy;
        public static GameObject dorothySwim;
        public static GameObject dorothySwimWet;
        public static GameObject dorothySwimSlip;

        public static GameObject elegg;
        public static GameObject eleggSwim;
        public static GameObject eleggSwimSlip;

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
        public static GameObject mastShirtless;
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
        public static GameObject sakuraShirtless;
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

        #region Vanilla Variables
        public static GameObject adrian;
        public static GameObject ameliaSwim;
        public static GameObject anna;
        public static GameObject doctorFrost;
        public static GameObject emmaSwim;
        public static GameObject gabriel;
        public static GameObject isabella;
        public static GameObject katarina;
        public static GameObject kate;
        public static GameObject masterZhen;
        public static GameObject mobsterBlonde;
        public static GameObject river;
        public static GameObject samSwim;
        public static GameObject sofia;
        public static GameObject toni;
        #endregion


        public static GameObject solidSnake;


        public static bool loadedBusts = false;

        public void Update()
        {
            if (Core.currentScene.name == "CoreGameScene")
            {
                if (!loadedBusts && Core.loadedCore)
                {
                    amber = CreateNewBust("Amber", Core.bustPath, "Amber\\Amber00.PNG", "Amber\\AmberBlink.PNG", "Amber\\Amber00Mask.PNG", "Amber\\Mouth", "Amber\\Expression", true, true);
                    amberCoatless = CreateNewBust("AmberCoatless", Core.bustPath, "Amber\\Amber01.PNG", "Amber\\AmberBlink.PNG", "Amber\\Amber01Mask.PNG", "Amber\\Mouth", "Amber\\Expression", true, true);
                    amberSwim = CreateNewBust("AmberSwim", Core.bustPath, "Amber\\AmberSwim00.PNG", "Amber\\AmberBlink.PNG", "Amber\\AmberSwim00Mask.PNG", "Amber\\Mouth", "Amber\\Expression", true, true);

                    anis = CreateNewBust("AnisBase", Core.bustNikkePath, "Anis\\Anis00.PNG", "Anis\\AnisBlink.PNG", "Anis\\Anis00Mask.PNG", "Anis\\Mouth", "Anis\\Expression", true, true);
                    anisSwim = CreateNewBust("AnisSwim", Core.bustNikkePath, "Anis\\AnisSwim00.PNG", "Anis\\AnisBlink.PNG", "Anis\\AnisSwim00Mask.PNG", "Anis\\Mouth", "Anis\\Expression", true, true);
                    anisSwimWet = CreateNewBust("AnisSwimWet", Core.bustNikkePath, "Anis\\AnisSwim01.PNG", "Anis\\AnisBlink.PNG", "Anis\\AnisSwim01Mask.PNG", "Anis\\Mouth", "Anis\\Expression", true, true);
                    anisSwimWet.transform.Find("MBase1").Find("Wet").gameObject.SetActive(true);
                    anisSwimSlip = CreateNewBust("AnisSwimSlip", Core.bustNikkePath, "Anis\\AnisSwim02.PNG", "Anis\\AnisBlink.PNG", "Anis\\AnisSwim02Mask.PNG", "Anis\\Mouth", "Anis\\Expression", true, true);
                    anisSwimSlip.transform.Find("MBase1").Find("Wet").gameObject.SetActive(true);

                    centi = CreateNewBust("CentiBase", Core.bustNikkePath, "Centi\\Centi00.PNG", "Centi\\CentiBlink.PNG", "Centi\\Centi00Mask.PNG", "Centi\\Mouth", "Centi\\Expression", true, true);
                    centiSwim = CreateNewBust("CentiSwim", Core.bustNikkePath, "Centi\\CentiSwim00.PNG", "Centi\\CentiBlink.PNG", "Centi\\CentiSwim00Mask.PNG", "Centi\\Mouth", "Centi\\Expression", true, true);
                    centiSwimShirtless = CreateNewBust("centiSwimShirtless", Core.bustNikkePath, "Centi\\CentiSwim01.PNG", "Centi\\CentiBlink.PNG", "Centi\\CentiSwim01Mask.PNG", "Centi\\Mouth", "Centi\\Expression", true, true);
                    centiSwimSlip = CreateNewBust("CentiSwimSlip", Core.bustNikkePath, "Centi\\CentiSwim02.PNG", "Centi\\CentiBlink.PNG", "Centi\\CentiSwim02Mask.PNG", "Centi\\Mouth", "Centi\\Expression", true, true);
                    centiSwimSlip.transform.Find("MBase1").Find("Wet").gameObject.SetActive(true);

                    dorothy = CreateNewBust("DorothyBase", Core.bustNikkePath, "Dorothy\\Dorothy00.PNG", "Dorothy\\DorothyBlink.PNG", "Dorothy\\Dorothy00Mask.PNG", "Dorothy\\Mouth", "Dorothy\\Expression", true, true);
                    dorothySwim = CreateNewBust("DorothySwim", Core.bustNikkePath, "Dorothy\\DorothySwim00.PNG", "Dorothy\\DorothyBlink.PNG", "Dorothy\\DorothySwim00Mask.PNG", "Dorothy\\Mouth", "Dorothy\\Expression", true, true);
                    dorothySwimWet = CreateNewBust("DorothySwimWet", Core.bustNikkePath, "Dorothy\\DorothySwim01.PNG", "Dorothy\\DorothyBlink.PNG", "Dorothy\\DorothySwim01Mask.PNG", "Dorothy\\Mouth", "Dorothy\\Expression", true, true);
                    dorothySwimWet.transform.Find("MBase1").Find("Wet").gameObject.SetActive(true);
                    dorothySwimSlip = CreateNewBust("DorothySwimSlip", Core.bustNikkePath, "Dorothy\\DorothySwim02.PNG", "Dorothy\\DorothyBlink.PNG", "Dorothy\\DorothySwim02Mask.PNG", "Dorothy\\Mouth", "Dorothy\\Expression", true, true);
                    dorothySwimSlip.transform.Find("MBase1").Find("Wet").gameObject.SetActive(true);

                    elegg = CreateNewBust("EleggBase", Core.bustNikkePath, "Elegg\\Elegg00.PNG", "Elegg\\EleggBlink.PNG", "Elegg\\Elegg00Mask.PNG", "Elegg\\Mouth", "Elegg\\Expression", true, true);
                    eleggSwim = CreateNewBust("EleggSwim", Core.bustNikkePath, "Elegg\\EleggSwim00.PNG", "Elegg\\EleggBlink.PNG", "Elegg\\EleggSwim00Mask.PNG", "Elegg\\Mouth", "Elegg\\Expression", true, true);
                    eleggSwimSlip = CreateNewBust("EleggSwimSlip", Core.bustNikkePath, "Elegg\\EleggSwim01.PNG", "Elegg\\EleggBlink.PNG", "Elegg\\EleggSwim01Mask.PNG", "Elegg\\Mouth", "Elegg\\Expression", true, true);

                    frima = CreateNewBust("FrimaBase", Core.bustNikkePath, "Frima\\Frima00.PNG", "Frima\\FrimaBlink.PNG", "Frima\\Frima00Mask.PNG", "Frima\\Mouth", "Frima\\Expression", true, true);
                    frimaSwim = CreateNewBust("FrimaSwim", Core.bustNikkePath, "Frima\\FrimaSwim00.PNG", "Frima\\FrimaBlink.PNG", "Frima\\FrimaSwim00Mask.PNG", "Frima\\Mouth", "Frima\\Expression", true, true);
                    frimaSwimShirtless = CreateNewBust("FrimaSwimShirtless", Core.bustNikkePath, "Frima\\FrimaSwim01.PNG", "Frima\\FrimaBlink.PNG", "Frima\\FrimaSwim01Mask.PNG", "Frima\\Mouth", "Frima\\Expression", true, true);
                    frimaSwimSlip = CreateNewBust("FrimaSwimSlip", Core.bustNikkePath, "Frima\\FrimaSwim02.PNG", "Frima\\FrimaBlink.PNG", "Frima\\FrimaSwim02Mask.PNG", "Frima\\Mouth", "Frima\\Expression", true, true);
                    frimaSwimSlip.transform.Find("MBase1").Find("Wet").gameObject.SetActive(true);

                    guilty = CreateNewBust("GuiltyBase", Core.bustNikkePath, "Guilty\\Guilty00.PNG", "Guilty\\GuiltyBlink.PNG", "Guilty\\Guilty00Mask.PNG", "Guilty\\Mouth", "Guilty\\Expression", true, true);
                    guiltySwim = CreateNewBust("GuiltySwim", Core.bustNikkePath, "Guilty\\GuiltySwim00.PNG", "Guilty\\GuiltyBlink.PNG", "Guilty\\GuiltySwim00Mask.PNG", "Guilty\\Mouth", "Guilty\\Expression", true, true);
                    guiltySwimSlip = CreateNewBust("GuiltySwimSlip", Core.bustNikkePath, "Guilty\\GuiltySwim01.PNG", "Guilty\\GuiltyBlink.PNG", "Guilty\\GuiltySwim01Mask.PNG", "Guilty\\Mouth", "Guilty\\Expression", true, true);
                    guiltySwimSlip.transform.Find("MBase1").Find("Wet").gameObject.SetActive(true);

                    helm = CreateNewBust("HelmBase", Core.bustNikkePath, "Helm\\Helm00.PNG", "Helm\\HelmBlink.PNG", "Helm\\Helm00Mask.PNG", "Helm\\Mouth", "Helm\\Expression", true, true);
                    helmSwim = CreateNewBust("HelmSwim", Core.bustNikkePath, "Helm\\HelmSwim00.PNG", "Helm\\HelmBlink.PNG", "Helm\\HelmSwim00Mask.PNG", "Helm\\Mouth", "Helm\\Expression", true, true);
                    helmSwimWet = CreateNewBust("HelmSwimWet", Core.bustNikkePath, "Helm\\HelmSwim01.PNG", "Helm\\HelmBlink.PNG", "Helm\\HelmSwim01Mask.PNG", "Helm\\Mouth", "Helm\\Expression", true, true);
                    helmSwimWet.transform.Find("MBase1").Find("Wet").gameObject.SetActive(true);
                    helmSwimShirtless = CreateNewBust("HelmSwimShirtless", Core.bustNikkePath, "Helm\\HelmSwim02.PNG", "Helm\\HelmBlink.PNG", "Helm\\HelmSwim02Mask.PNG", "Helm\\Mouth", "Helm\\Expression", true, true);
                    helmSwimShirtless.transform.Find("MBase1").Find("Wet").gameObject.SetActive(true);
                    helmSwimSlip = CreateNewBust("HelmSwimSlip", Core.bustNikkePath, "Helm\\HelmSwim03.PNG", "Helm\\HelmBlink.PNG", "Helm\\HelmSwim03Mask.PNG", "Helm\\Mouth", "Helm\\Expression", true, true);
                    helmSwimSlip.transform.Find("MBase1").Find("Wet").gameObject.SetActive(true);

                    maiden = CreateNewBust("MaidenBase", Core.bustNikkePath, "Maiden\\Maiden00.PNG", "Maiden\\MaidenBlink.PNG", "Maiden\\Maiden00Mask.PNG", "Maiden\\Mouth", "Maiden\\Expression", true, true);
                    maidenSwim = CreateNewBust("MaidenSwim", Core.bustNikkePath, "Maiden\\MaidenSwim00.PNG", "Maiden\\MaidenBlink.PNG", "Maiden\\MaidenSwim00Mask.PNG", "Maiden\\MouthB", "Maiden\\Expression", true, true);
                    maidenSwimSlip = CreateNewBust("MaidenSwimSlip", Core.bustNikkePath, "Maiden\\MaidenSwim01.PNG", "Maiden\\MaidenBlink.PNG", "Maiden\\MaidenSwim01Mask.PNG", "Maiden\\MouthB", "Maiden\\Expression", true, true);
                    maidenSwimSlip.transform.Find("MBase1").Find("Wet").gameObject.SetActive(true);

                    mary = CreateNewBust("MaryBase", Core.bustNikkePath, "Mary\\Mary00.PNG", "Mary\\MaryBlink.PNG", "Mary\\Mary00Mask.PNG", "Mary\\Mouth", "Mary\\Expression", true, true);
                    marySwim = CreateNewBust("MarySwim", Core.bustNikkePath, "Mary\\MarySwim00.PNG", "Mary\\MaryBlink.PNG", "Mary\\MarySwim00Mask.PNG", "Mary\\Mouth", "Mary\\Expression", true, true);
                    marySwimSlip = CreateNewBust("MarySwimSlip", Core.bustNikkePath, "Mary\\MarySwim01.PNG", "Mary\\MaryBlink.PNG", "Mary\\MarySwim01Mask.PNG", "Mary\\Mouth", "Mary\\Expression", true, true);

                    mast = CreateNewBust("MastBase", Core.bustNikkePath, "Mast\\Mast00.PNG", "Mast\\MastBlink.PNG", "Mast\\Mast00Mask.PNG", "Mast\\Mouth", "Mast\\Expression", true, true);
                    mastShirtless = CreateNewBust("MastShirtless", Core.bustNikkePath, "Mast\\Mast01.PNG", "Mast\\MastBlink.PNG", "Mast\\Mast01Mask.PNG", "Mast\\Mouth", "Mast\\Expression", true, true);
                    mastSwim = CreateNewBust("MastSwim", Core.bustNikkePath, "Mast\\MastSwim00.PNG", "Mast\\MastBlink.PNG", "Mast\\MastSwim00Mask.PNG", "Mast\\Mouth", "Mast\\Expression", true, true);
                    mastSwimSlip = CreateNewBust("MastSwimSlip", Core.bustNikkePath, "Mast\\MastSwim01.PNG", "Mast\\MastBlink.PNG", "Mast\\MastSwim01Mask.PNG", "Mast\\Mouth", "Mast\\Expression", true, true);

                    neon = CreateNewBust("NeonBase", Core.bustNikkePath, "Neon\\Neon00.PNG", "Neon\\NeonBlink.PNG", "Neon\\Neon00Mask.PNG", "Neon\\Mouth", "Neon\\Expression", true, true);
                    neonSwim = CreateNewBust("NeonSwim", Core.bustNikkePath, "Neon\\NeonSwim00.PNG", "Neon\\NeonBlink.PNG", "Neon\\NeonSwim00Mask.PNG", "Neon\\Mouth", "Neon\\Expression", true, true);
                    neonSwimWet = CreateNewBust("NeonSwimWet", Core.bustNikkePath, "Neon\\NeonSwim01.PNG", "Neon\\NeonBlink.PNG", "Neon\\NeonSwim01Mask.PNG", "Neon\\Mouth", "Neon\\Expression", true, true);
                    neonSwimWet.transform.Find("MBase1").Find("Wet").gameObject.SetActive(true);
                    neonSwimSlip = CreateNewBust("NeonSwimSlip", Core.bustNikkePath, "Neon\\NeonSwim02.PNG", "Neon\\NeonBlink.PNG", "Neon\\NeonSwim02Mask.PNG", "Neon\\Mouth", "Neon\\Expression", true, true);
                    neonSwimSlip.transform.Find("MBase1").Find("Wet").gameObject.SetActive(true);

                    pepper = CreateNewBust("PepperBase", Core.bustNikkePath, "Pepper\\Pepper00.PNG", "Pepper\\PepperBlink.PNG", "Pepper\\Pepper00Mask.PNG", "Pepper\\Mouth", "Pepper\\Expression", true, true);
                    pepperSwim = CreateNewBust("PepperSwim", Core.bustNikkePath, "Pepper\\PepperSwim00.PNG", "Pepper\\PepperBlink.PNG", "Pepper\\PepperSwim00Mask.PNG", "Pepper\\Mouth", "Pepper\\Expression", true, true);
                    pepperSwimSlip = CreateNewBust("PepperSwimSlip", Core.bustNikkePath, "Pepper\\PepperSwim01.PNG", "Pepper\\PepperBlink.PNG", "Pepper\\PepperSwim01Mask.PNG", "Pepper\\Mouth", "Pepper\\Expression", true, true);

                    rapi = CreateNewBust("RapiBase", Core.bustNikkePath, "Rapi\\Rapi00.PNG", "Rapi\\RapiBlink.PNG", "Rapi\\Rapi00Mask.PNG", "Rapi\\Mouth", "Rapi\\Expression", true, true);
                    rapiSwim = CreateNewBust("RapiSwim", Core.bustNikkePath, "Rapi\\RapiSwim00.PNG", "Rapi\\RapiBlink.PNG", "Rapi\\RapiSwim00Mask.PNG", "Rapi\\Mouth", "Rapi\\Expression", true, true);
                    rapiSwimSlip = CreateNewBust("RapiSwimSlip", Core.bustNikkePath, "Rapi\\RapiSwim01.PNG", "Rapi\\RapiBlink.PNG", "Rapi\\RapiSwim01Mask.PNG", "Rapi\\Mouth", "Rapi\\Expression", true, true);
                    rapiSwimSlip.transform.Find("MBase1").Find("Wet").gameObject.SetActive(true);

                    rosanna = CreateNewBust("RosannaBase", Core.bustNikkePath, "Rosanna\\Rosanna00.PNG", "Rosanna\\RosannaBlink.PNG", "Rosanna\\Rosanna00Mask.PNG", "Rosanna\\Mouth", "Rosanna\\Expression", true, true);
                    rosannaSwim = CreateNewBust("RosannaSwim", Core.bustNikkePath, "Rosanna\\RosannaSwim00.PNG", "Rosanna\\RosannaBlink.PNG", "Rosanna\\RosannaSwim00Mask.PNG", "Rosanna\\Mouth", "Rosanna\\Expression", true, true);
                    rosannaSwimSlip = CreateNewBust("RosannaSwimSlip", Core.bustNikkePath, "Rosanna\\RosannaSwim01.PNG", "Rosanna\\RosannaBlink.PNG", "Rosanna\\RosannaSwim01Mask.PNG", "Rosanna\\Mouth", "Rosanna\\Expression", true, true);

                    sakura = CreateNewBust("SakuraBase", Core.bustNikkePath, "Sakura\\Sakura00.PNG", "Sakura\\SakuraBlink.PNG", "Sakura\\Sakura00Mask.PNG", "Sakura\\Mouth", "Sakura\\Expression", true, true);
                    sakuraShirtless = CreateNewBust("SakuraShirtless", Core.bustNikkePath, "Sakura\\Sakura01.PNG", "Sakura\\SakuraBlink.PNG", "Sakura\\Sakura01Mask.PNG", "Sakura\\Mouth", "Sakura\\Expression", true, true);
                    sakuraSwim = CreateNewBust("SakuraSwim", Core.bustNikkePath, "Sakura\\SakuraSwim00.PNG", "Sakura\\SakuraBlink.PNG", "Sakura\\SakuraSwim00Mask.PNG", "Sakura\\Mouth", "Sakura\\Expression", true, true);
                    sakuraSwimSlip = CreateNewBust("SakuraSwimSlip", Core.bustNikkePath, "Sakura\\SakuraSwim01.PNG", "Sakura\\SakuraBlink.PNG", "Sakura\\SakuraSwim01Mask.PNG", "Sakura\\Mouth", "Sakura\\Expression", true, true);

                    viper = CreateNewBust("ViperBase", Core.bustNikkePath, "Viper\\Viper00.PNG", "Viper\\ViperBlink.PNG", "Viper\\Viper00Mask.PNG", "Viper\\Mouth", "Viper\\Expression", true, true);
                    viperSwim = CreateNewBust("ViperSwim", Core.bustNikkePath, "Viper\\ViperSwim00.PNG", "Viper\\ViperBlink.PNG", "Viper\\ViperSwim00Mask.PNG", "Viper\\Mouth", "Viper\\Expression", true, true);
                    viperSwimShirtless = CreateNewBust("ViperSwimShirtless", Core.bustNikkePath, "Viper\\ViperSwim01.PNG", "Viper\\ViperBlink.PNG", "Viper\\ViperSwim01Mask.PNG", "Viper\\Mouth", "Viper\\Expression", true, true);
                    viperSwimWet = CreateNewBust("ViperSwimWet", Core.bustNikkePath, "Viper\\ViperSwim02.PNG", "Viper\\ViperBlink.PNG", "Viper\\ViperSwim02Mask.PNG", "Viper\\Mouth", "Viper\\Expression", true, true);
                    viperSwimWet.transform.Find("MBase1").Find("Wet").gameObject.SetActive(true);
                    viperSwimSlip = CreateNewBust("ViperSwimSlip", Core.bustNikkePath, "Viper\\ViperSwim03.PNG", "Viper\\ViperBlink.PNG", "Viper\\ViperSwim03Mask.PNG", "Viper\\Mouth", "Viper\\Expression", true, true);
                    viperSwimSlip.transform.Find("MBase1").Find("Wet").gameObject.SetActive(true);

                    yan = CreateNewBust("YanBase", Core.bustNikkePath, "Yan\\Yan00.PNG", "Yan\\YanBlink.PNG", "Yan\\Yan00Mask.PNG", "Yan\\Mouth", "Yan\\Expression", true, true);
                    yanSwim = CreateNewBust("YanSwim", Core.bustNikkePath, "Yan\\YanSwim00.PNG", "Yan\\YanBlink.PNG", "Yan\\YanSwim00Mask.PNG", "Yan\\Mouth", "Yan\\Expression", true, true);
                    yanSwimSlip = CreateNewBust("YanSwimSlip", Core.bustNikkePath, "Yan\\YanSwim01.PNG", "Yan\\YanBlink.PNG", "Yan\\YanSwim01Mask.PNG", "Yan\\Mouth", "Yan\\Expression", true, true);



                    #region Vanilla
                    adrian = Core.bustManager.Find("Adrian_bust").gameObject;
                    ameliaSwim = Core.bustManager.Find("Amelia_Beach").gameObject;
                    anna = Core.bustManager.Find("Anna_Bust").gameObject;
                    doctorFrost = Core.bustManager.Find("doctorfrost_default").gameObject;
                    emmaSwim = Core.bustManager.Find("Emma_Swimwear").gameObject;
                    gabriel = Core.bustManager.Find("Gabriel_Bust").gameObject;
                    isabella = Core.bustManager.Find("Isabella").gameObject;
                    katarina = Core.bustManager.Find("Katarina_Normal").gameObject;
                    kate = Core.bustManager.Find("Kate").gameObject;
                    masterZhen = Core.bustManager.Find("Master_Default").gameObject;
                    mobsterBlonde = Core.bustManager.Find("S_Mobster1").gameObject;
                    river = Core.bustManager.Find("River_Base").gameObject;
                    samSwim = Core.bustManager.Find("Samantha_Swimsuit").gameObject;
                    sofia = Core.bustManager.Find("Sofia_Police").gameObject;
                    toni = Core.bustManager.Find("TomboyToni_BustDefault").gameObject;
                    #endregion


                    solidSnake = CreateNewBust("Snek", Core.bustPath, "Solid\\Snek00.PNG", "Solid\\SnekBlink.PNG", "Solid\\Snek00Mask.PNG", "Solid\\Mouth", "Solid\\Expression", false, false);


                    Core.bustManager.GetComponent<SpriteRendererLayoutManager>().RefreshCache();
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


        public GameObject CreateNewBust(string name, string pathToCG, string baseSprite, string blinkSprite, string maskSprite, string mouthSprite, string expressionSprite, bool hasMouth, bool hasExpression)
        {
            GameObject newBust = GameObject.Instantiate(Core.baseBust, Core.bustManager);
            newBust.name = name;
            GameObject mBase = newBust.transform.Find("MBase1").gameObject;
            GameObject blink = mBase.transform.Find("Blink").gameObject;
            GameObject mouth = mBase.transform.Find("Mouth").gameObject;
            GameObject expressions = mBase.transform.Find("Expressions").gameObject;
            Material mat = new Material(mBase.GetComponent<SpriteRenderer>().material);
            string[] expressionNames = { "Happy", "Angry", "Sad", "Flirty" };

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

            if (hasMouth)
            {
                for (int i = 1; i <= 4; i++)
                {
                    tex = new Texture2D(256, 256, TextureFormat.RGBA32, false);
                    rawData = System.IO.File.ReadAllBytes(pathToCG + mouthSprite + i + ".PNG");
                    tex.LoadImage(rawData);
                    tex.filterMode = FilterMode.Point;
                    newSprite = Sprite.Create(tex, new Rect(0, 0, 256, 256), new Vector2(0.5f, 0.5f));
                    mouth.transform.Find(i.ToString()).GetComponent<SpriteRenderer>().sprite = newSprite;
                }
            }
            else
            {
                for (int i = 1; i <= 4; i++)
                {
                    Destroy(mouth.transform.Find(i.ToString()).GetComponent<SpriteRenderer>());
                }
            }
            if (hasExpression)
            {
                foreach (string expressionName in expressionNames)
                {
                    tex = new Texture2D(256, 256, TextureFormat.RGBA32, false);
                    rawData = System.IO.File.ReadAllBytes(pathToCG + expressionSprite + expressionName + ".PNG");
                    tex.LoadImage(rawData);
                    tex.filterMode = FilterMode.Point;
                    newSprite = Sprite.Create(tex, new Rect(0, 0, 256, 256), new Vector2(0.5f, 0.5f));
                    expressions.transform.Find(expressionName).GetComponent<SpriteRenderer>().sprite = newSprite;
                }
            }
            else 
            {
                foreach (string expressionName in expressionNames)
                {
                    Destroy(expressions.transform.Find(expressionName).GetComponent<SpriteRenderer>());
                }
            }

            Destroy(expressions.GetComponent<Conditions>());
            Destroy(expressions.GetComponent<Trigger>());

            GameObject wetParticles = GameObject.Instantiate(Core.bustManager.Find("Anna_Towel").Find("MBase1").Find("Particle System").gameObject, mBase.transform);
            wetParticles.name = "Wet";
            wetParticles.SetActive(false);

            if (hasMouth)
            {
            }

            Core.bustManager.GetComponent<SpriteRendererLayoutManager>().targetObjects.Add(newBust);

            newBust.SetActive(false);
            return newBust;
        }


    }
}
