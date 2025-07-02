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
    [BepInPlugin(pluginGuid, Core.pluginName + " - Dialogues", Core.pluginVersion)]
    internal class Dialogues : BaseUnityPlugin
    {
        #region Plugin Info
        public const string pluginGuid = "treboy.starmakerstory.smsandroidscore.dialogues";
        #endregion
        public static GameObject overrideSpeechSkinBlue;
        public static GameObject overrideSpeechSkinGreen;
        public static GameObject overrideSpeechSkinPink;
        public static GameObject overrideSpeechSkinYellow;

        public static GameObject badWeatherDialogue;
        public static GameObject badWeatherDialogueActivator;
        public static GameObject badWeatherDialogueFinisher;

        public static GameObject sBDialogueMainFirst;
        public static GameObject sBDialogueMainFirstScene1;
        public static GameObject sBDialogueMainFirstScene2;
        public static GameObject sBDialogueMainFirstScene3;
        public static GameObject sBDialogueMainFirstScene4;
        public static GameObject sBDialogueMainFirstScene5;
        public static GameObject sBDialogueMainFirstDialogueActivator;
        public static GameObject sBDialogueMainFirstDialogueFinisher;
        public static GameObject sBDialogueMainFirstMouthActivator;
        public static GameObject sBDialogueMainFirstSpriteFocus;

        public static GameObject sBDialogueMain;
        public static GameObject sBDialogueMainScene1;
        public static GameObject sBDialogueMainScene2;
        public static GameObject sBDialogueMainScene3;
        public static GameObject sBDialogueMainScene4;
        public static GameObject sBDialogueMainScene5;
        public static GameObject sBDialogueMainDialogueActivator;
        public static GameObject sBDialogueMainDialogueFinisher;
        public static GameObject sBDialogueMainMouthActivator;
        public static GameObject sBDialogueMainSpriteFocus;

        public static GameObject sBDialogueMainGK;
        public static GameObject sBDialogueMainGKScene1;
        public static GameObject sBDialogueMainGKScene2;
        public static GameObject sBDialogueMainGKScene3;
        public static GameObject sBDialogueMainGKScene4;
        public static GameObject sBDialogueMainGKScene5;
        public static GameObject sBDialogueMainGKDialogueActivator;
        public static GameObject sBDialogueMainGKDialogueFinisher;
        public static GameObject sBDialogueMainGKMouthActivator;
        public static GameObject sBDialogueMainGKSpriteFocus;

        public static GameObject sBDialogueStory01;
        public static GameObject sBDialogueStory01Scene1;
        public static GameObject sBDialogueStory01Scene2;
        public static GameObject sBDialogueStory01Scene3;
        public static GameObject sBDialogueStory01Scene4;
        public static GameObject sBDialogueStory01Scene5;
        public static GameObject sBDialogueStory01DialogueActivator;
        public static GameObject sBDialogueStory01DialogueFinisher;
        public static GameObject sBDialogueStory01MouthActivator;
        public static GameObject sBDialogueStory01SpriteFocus;

        #region Voyeur Variables
        public static GameObject anisBeachDialogue;
        public static GameObject anisBeachDialogueScene1;
        public static GameObject anisBeachDialogueScene2;
        public static GameObject anisBeachDialogueScene3;
        public static GameObject anisBeachDialogueScene4;
        public static GameObject anisBeachDialogueScene5;
        public static GameObject anisBeachDialogueActivator;
        public static GameObject anisBeachDialogueFinisher;
        public static GameObject anisBeachDialogueMouthActivator;
        public static GameObject anisBeachDialogueSpriteFocus;

        public static GameObject frimaBeachDialogue;
        public static GameObject frimaBeachDialogueScene1;
        public static GameObject frimaBeachDialogueScene2;
        public static GameObject frimaBeachDialogueScene3;
        public static GameObject frimaBeachDialogueScene4;
        public static GameObject frimaBeachDialogueScene5;
        public static GameObject frimaBeachDialogueActivator;
        public static GameObject frimaBeachDialogueFinisher;
        public static GameObject frimaBeachDialogueMouthActivator;
        public static GameObject frimaBeachDialogueSpriteFocus;

        public static GameObject guiltyBeachDialogue;
        public static GameObject guiltyBeachDialogueScene1;
        public static GameObject guiltyBeachDialogueScene2;
        public static GameObject guiltyBeachDialogueScene3;
        public static GameObject guiltyBeachDialogueScene4;
        public static GameObject guiltyBeachDialogueScene5;
        public static GameObject guiltyBeachDialogueActivator;
        public static GameObject guiltyBeachDialogueFinisher;
        public static GameObject guiltyBeachDialogueMouthActivator;
        public static GameObject guiltyBeachDialogueSpriteFocus;

        public static GameObject helmBeachDialogue;
        public static GameObject helmBeachDialogueScene1;
        public static GameObject helmBeachDialogueScene2;
        public static GameObject helmBeachDialogueScene3;
        public static GameObject helmBeachDialogueScene4;
        public static GameObject helmBeachDialogueScene5;
        public static GameObject helmBeachDialogueActivator;
        public static GameObject helmBeachDialogueFinisher;
        public static GameObject helmBeachDialogueMouthActivator;
        public static GameObject helmBeachDialogueSpriteFocus;

        public static GameObject maidenBeachDialogue;
        public static GameObject maidenBeachDialogueScene1;
        public static GameObject maidenBeachDialogueScene2;
        public static GameObject maidenBeachDialogueScene3;
        public static GameObject maidenBeachDialogueScene4;
        public static GameObject maidenBeachDialogueScene5;
        public static GameObject maidenBeachDialogueActivator;
        public static GameObject maidenBeachDialogueFinisher;
        public static GameObject maidenBeachDialogueMouthActivator;
        public static GameObject maidenBeachDialogueSpriteFocus;

        public static GameObject maryBeachDialogue;
        public static GameObject maryBeachDialogueScene1;
        public static GameObject maryBeachDialogueScene2;
        public static GameObject maryBeachDialogueScene3;
        public static GameObject maryBeachDialogueScene4;
        public static GameObject maryBeachDialogueScene5;
        public static GameObject maryBeachDialogueActivator;
        public static GameObject maryBeachDialogueFinisher;
        public static GameObject maryBeachDialogueMouthActivator;
        public static GameObject maryBeachDialogueSpriteFocus;

        public static GameObject mastBeachDialogue;
        public static GameObject mastBeachDialogueScene1;
        public static GameObject mastBeachDialogueScene2;
        public static GameObject mastBeachDialogueScene3;
        public static GameObject mastBeachDialogueScene4;
        public static GameObject mastBeachDialogueScene5;
        public static GameObject mastBeachDialogueActivator;
        public static GameObject mastBeachDialogueFinisher;
        public static GameObject mastBeachDialogueMouthActivator;
        public static GameObject mastBeachDialogueSpriteFocus;

        public static GameObject neonBeachDialogue;
        public static GameObject neonBeachDialogueScene1;
        public static GameObject neonBeachDialogueScene2;
        public static GameObject neonBeachDialogueScene3;
        public static GameObject neonBeachDialogueScene4;
        public static GameObject neonBeachDialogueScene5;
        public static GameObject neonBeachDialogueActivator;
        public static GameObject neonBeachDialogueFinisher;
        public static GameObject neonBeachDialogueMouthActivator;
        public static GameObject neonBeachDialogueSpriteFocus;

        public static GameObject pepperBeachDialogue;
        public static GameObject pepperBeachDialogueScene1;
        public static GameObject pepperBeachDialogueScene2;
        public static GameObject pepperBeachDialogueScene3;
        public static GameObject pepperBeachDialogueScene4;
        public static GameObject pepperBeachDialogueScene5;
        public static GameObject pepperBeachDialogueActivator;
        public static GameObject pepperBeachDialogueFinisher;
        public static GameObject pepperBeachDialogueMouthActivator;
        public static GameObject pepperBeachDialogueSpriteFocus;

        public static GameObject rapiBeachDialogue;
        public static GameObject rapiBeachDialogueScene1;
        public static GameObject rapiBeachDialogueScene2;
        public static GameObject rapiBeachDialogueScene3;
        public static GameObject rapiBeachDialogueScene4;
        public static GameObject rapiBeachDialogueScene5;
        public static GameObject rapiBeachDialogueActivator;
        public static GameObject rapiBeachDialogueFinisher;
        public static GameObject rapiBeachDialogueMouthActivator;
        public static GameObject rapiBeachDialogueSpriteFocus;

        public static GameObject rosannaBeachDialogue;
        public static GameObject rosannaBeachDialogueScene1;
        public static GameObject rosannaBeachDialogueScene2;
        public static GameObject rosannaBeachDialogueScene3;
        public static GameObject rosannaBeachDialogueScene4;
        public static GameObject rosannaBeachDialogueScene5;
        public static GameObject rosannaBeachDialogueActivator;
        public static GameObject rosannaBeachDialogueFinisher;
        public static GameObject rosannaBeachDialogueMouthActivator;
        public static GameObject rosannaBeachDialogueSpriteFocus;

        public static GameObject sakuraBeachDialogue;
        public static GameObject sakuraBeachDialogueScene1;
        public static GameObject sakuraBeachDialogueScene2;
        public static GameObject sakuraBeachDialogueScene3;
        public static GameObject sakuraBeachDialogueScene4;
        public static GameObject sakuraBeachDialogueScene5;
        public static GameObject sakuraBeachDialogueActivator;
        public static GameObject sakuraBeachDialogueFinisher;
        public static GameObject sakuraBeachDialogueMouthActivator;
        public static GameObject sakuraBeachDialogueSpriteFocus;

        public static GameObject viperBeachDialogue;
        public static GameObject viperBeachDialogueScene1;
        public static GameObject viperBeachDialogueScene2;
        public static GameObject viperBeachDialogueScene3;
        public static GameObject viperBeachDialogueScene4;
        public static GameObject viperBeachDialogueScene5;
        public static GameObject viperBeachDialogueActivator;
        public static GameObject viperBeachDialogueFinisher;
        public static GameObject viperBeachDialogueMouthActivator;
        public static GameObject viperBeachDialogueSpriteFocus;

        public static GameObject yanBeachDialogue;
        public static GameObject yanBeachDialogueScene1;
        public static GameObject yanBeachDialogueScene2;
        public static GameObject yanBeachDialogueScene3;
        public static GameObject yanBeachDialogueScene4;
        public static GameObject yanBeachDialogueScene5;
        public static GameObject yanBeachDialogueActivator;
        public static GameObject yanBeachDialogueFinisher;
        public static GameObject yanBeachDialogueMouthActivator;
        public static GameObject yanBeachDialogueSpriteFocus;
#endregion
        public static bool loadedDialogues = false;
        public static bool dialoguePlaying = false;
        public void Update()
        {
            if (Core.currentScene.name == "CoreGameScene")
            {
                if (!loadedDialogues && Places.loadedPlaces) 
                {
                    overrideSpeechSkinBlue = GetActorOverrideSpeechSkinValue(Core.roomTalk.Find("Bath").Find("AnnaAndAdrianFirsttime").gameObject, "Adrian");
                    overrideSpeechSkinGreen = GetActorOverrideSpeechSkinValue(Core.roomTalk.Find("Bath").Find("NoOneInShower").gameObject, "You");
                    overrideSpeechSkinPink = GetActorOverrideSpeechSkinValue(Core.roomTalk.Find("Beach").Find("AmeliaBeach").gameObject, "Amelia");
                    overrideSpeechSkinYellow = GetActorOverrideSpeechSkinValue(Core.roomTalk.Find("Bath").Find("AnnaInShower").gameObject, "Anna");

                    Debugging.PrintAllActorExpressionsFromDialogue(Core.roomTalk.Find("Bath").Find("AnnaInShower").gameObject, "Anna");

                    badWeatherDialogue = CreateNewDialogue("BadWeather", Places.secretBeachRoomtalk.transform);
                    badWeatherDialogueActivator = badWeatherDialogue.transform.Find("DialogueActivator").gameObject;
                    badWeatherDialogueFinisher = badWeatherDialogue.transform.Find("DialogueFinisher").gameObject;

                    sBDialogueMainFirst = CreateNewDialogue("SBDialogueMainFirst", Places.secretBeachRoomtalk.transform);
                    sBDialogueMainFirstScene1 = sBDialogueMainFirst.transform.Find("Scene1").gameObject;
                    sBDialogueMainFirstScene2 = sBDialogueMainFirst.transform.Find("Scene2").gameObject;
                    sBDialogueMainFirstScene3 = sBDialogueMainFirst.transform.Find("Scene3").gameObject;
                    sBDialogueMainFirstScene4 = sBDialogueMainFirst.transform.Find("Scene4").gameObject;
                    sBDialogueMainFirstScene5 = sBDialogueMainFirst.transform.Find("Scene5").gameObject;
                    sBDialogueMainFirstDialogueActivator = sBDialogueMainFirst.transform.Find("DialogueActivator").gameObject;
                    sBDialogueMainFirstDialogueFinisher = sBDialogueMainFirst.transform.Find("DialogueFinisher").gameObject;
                    sBDialogueMainFirstMouthActivator = sBDialogueMainFirst.transform.Find("MouthActivator").gameObject;
                    sBDialogueMainFirstSpriteFocus = sBDialogueMainFirst.transform.Find("SpriteFocus").gameObject;

                    sBDialogueMain = CreateNewDialogue("SBDialogueMain", Places.secretBeachRoomtalk.transform);
                    sBDialogueMainScene1 = sBDialogueMain.transform.Find("Scene1").gameObject;
                    sBDialogueMainScene2 = sBDialogueMain.transform.Find("Scene2").gameObject;
                    sBDialogueMainScene3 = sBDialogueMain.transform.Find("Scene3").gameObject;
                    sBDialogueMainScene4 = sBDialogueMain.transform.Find("Scene4").gameObject;
                    sBDialogueMainScene5 = sBDialogueMain.transform.Find("Scene5").gameObject;
                    sBDialogueMainDialogueActivator = sBDialogueMain.transform.Find("DialogueActivator").gameObject;
                    sBDialogueMainDialogueFinisher = sBDialogueMain.transform.Find("DialogueFinisher").gameObject;
                    sBDialogueMainMouthActivator = sBDialogueMain.transform.Find("MouthActivator").gameObject;
                    sBDialogueMainSpriteFocus = sBDialogueMain.transform.Find("SpriteFocus").gameObject;

                    sBDialogueMainGK = CreateNewDialogue("SBDialogueMainGatekeeper", Places.secretBeachRoomtalk.transform);
                    sBDialogueMainGKScene1 = sBDialogueMainGK.transform.Find("Scene1").gameObject;
                    sBDialogueMainGKScene2 = sBDialogueMainGK.transform.Find("Scene2").gameObject;
                    sBDialogueMainGKScene3 = sBDialogueMainGK.transform.Find("Scene3").gameObject;
                    sBDialogueMainGKScene4 = sBDialogueMainGK.transform.Find("Scene4").gameObject;
                    sBDialogueMainGKScene5 = sBDialogueMainGK.transform.Find("Scene5").gameObject;
                    sBDialogueMainGKDialogueActivator = sBDialogueMainGK.transform.Find("DialogueActivator").gameObject;
                    sBDialogueMainGKDialogueFinisher = sBDialogueMainGK.transform.Find("DialogueFinisher").gameObject;
                    sBDialogueMainGKMouthActivator = sBDialogueMainGK.transform.Find("MouthActivator").gameObject;
                    sBDialogueMainGKSpriteFocus = sBDialogueMainGK.transform.Find("SpriteFocus").gameObject;

                    sBDialogueStory01 = CreateNewDialogue("SBDialogueStory01", Places.secretBeachRoomtalk.transform);
                    sBDialogueStory01Scene1 = sBDialogueStory01.transform.Find("Scene1").gameObject;
                    sBDialogueStory01Scene2 = sBDialogueStory01.transform.Find("Scene2").gameObject;
                    sBDialogueStory01Scene3 = sBDialogueStory01.transform.Find("Scene3").gameObject;
                    sBDialogueStory01Scene4 = sBDialogueStory01.transform.Find("Scene4").gameObject;
                    sBDialogueStory01Scene5 = sBDialogueStory01.transform.Find("Scene5").gameObject;
                    sBDialogueStory01DialogueActivator = sBDialogueStory01.transform.Find("DialogueActivator").gameObject;
                    sBDialogueStory01DialogueFinisher = sBDialogueStory01.transform.Find("DialogueFinisher").gameObject;
                    sBDialogueStory01MouthActivator = sBDialogueStory01.transform.Find("MouthActivator").gameObject;
                    sBDialogueStory01SpriteFocus = sBDialogueStory01.transform.Find("SpriteFocus").gameObject;

                    #region Voyeur Initialization
                    anisBeachDialogue = CreateNewDialogue("AnisDialogueBeach01", Places.secretBeachRoomtalk.transform);
                    anisBeachDialogueScene1 = anisBeachDialogue.transform.Find("Scene1").gameObject;
                    anisBeachDialogueScene2 = anisBeachDialogue.transform.Find("Scene2").gameObject;
                    anisBeachDialogueScene3 = anisBeachDialogue.transform.Find("Scene3").gameObject;
                    anisBeachDialogueScene4 = anisBeachDialogue.transform.Find("Scene4").gameObject;
                    anisBeachDialogueScene5 = anisBeachDialogue.transform.Find("Scene5").gameObject;
                    anisBeachDialogueActivator = anisBeachDialogue.transform.Find("DialogueActivator").gameObject;
                    anisBeachDialogueFinisher = anisBeachDialogue.transform.Find("DialogueFinisher").gameObject;
                    anisBeachDialogueMouthActivator = anisBeachDialogue.transform.Find("MouthActivator").gameObject;
                    anisBeachDialogueSpriteFocus = anisBeachDialogue.transform.Find("SpriteFocus").gameObject;

                    frimaBeachDialogue = CreateNewDialogue("FrimaDialogueBeach01", Places.secretBeachRoomtalk.transform);
                    frimaBeachDialogueScene1 = frimaBeachDialogue.transform.Find("Scene1").gameObject;
                    frimaBeachDialogueScene2 = frimaBeachDialogue.transform.Find("Scene2").gameObject;
                    frimaBeachDialogueScene3 = frimaBeachDialogue.transform.Find("Scene3").gameObject;
                    frimaBeachDialogueScene4 = frimaBeachDialogue.transform.Find("Scene4").gameObject;
                    frimaBeachDialogueScene5 = frimaBeachDialogue.transform.Find("Scene5").gameObject;
                    frimaBeachDialogueActivator = frimaBeachDialogue.transform.Find("DialogueActivator").gameObject;
                    frimaBeachDialogueFinisher = frimaBeachDialogue.transform.Find("DialogueFinisher").gameObject;
                    frimaBeachDialogueMouthActivator = frimaBeachDialogue.transform.Find("MouthActivator").gameObject;
                    frimaBeachDialogueSpriteFocus = frimaBeachDialogue.transform.Find("SpriteFocus").gameObject;

                    guiltyBeachDialogue = CreateNewDialogue("GuiltyDialogueBeach01", Places.secretBeachRoomtalk.transform);
                    guiltyBeachDialogueScene1 = guiltyBeachDialogue.transform.Find("Scene1").gameObject;
                    guiltyBeachDialogueScene2 = guiltyBeachDialogue.transform.Find("Scene2").gameObject;
                    guiltyBeachDialogueScene3 = guiltyBeachDialogue.transform.Find("Scene3").gameObject;
                    guiltyBeachDialogueScene4 = guiltyBeachDialogue.transform.Find("Scene4").gameObject;
                    guiltyBeachDialogueScene5 = guiltyBeachDialogue.transform.Find("Scene5").gameObject;
                    guiltyBeachDialogueActivator = guiltyBeachDialogue.transform.Find("DialogueActivator").gameObject;
                    guiltyBeachDialogueFinisher = guiltyBeachDialogue.transform.Find("DialogueFinisher").gameObject;
                    guiltyBeachDialogueMouthActivator = guiltyBeachDialogue.transform.Find("MouthActivator").gameObject;
                    guiltyBeachDialogueSpriteFocus = guiltyBeachDialogue.transform.Find("SpriteFocus").gameObject;

                    helmBeachDialogue = CreateNewDialogue("HelmDialogueBeach01", Places.secretBeachRoomtalk.transform);
                    helmBeachDialogueScene1 = helmBeachDialogue.transform.Find("Scene1").gameObject;
                    helmBeachDialogueScene2 = helmBeachDialogue.transform.Find("Scene2").gameObject;
                    helmBeachDialogueScene3 = helmBeachDialogue.transform.Find("Scene3").gameObject;
                    helmBeachDialogueScene4 = helmBeachDialogue.transform.Find("Scene4").gameObject;
                    helmBeachDialogueScene5 = helmBeachDialogue.transform.Find("Scene5").gameObject;
                    helmBeachDialogueActivator = helmBeachDialogue.transform.Find("DialogueActivator").gameObject;
                    helmBeachDialogueFinisher = helmBeachDialogue.transform.Find("DialogueFinisher").gameObject;
                    helmBeachDialogueMouthActivator = helmBeachDialogue.transform.Find("MouthActivator").gameObject;
                    helmBeachDialogueSpriteFocus = helmBeachDialogue.transform.Find("SpriteFocus").gameObject;

                    maidenBeachDialogue = CreateNewDialogue("MaidenDialogueBeach01", Places.secretBeachRoomtalk.transform);
                    maidenBeachDialogueScene1 = maidenBeachDialogue.transform.Find("Scene1").gameObject;
                    maidenBeachDialogueScene2 = maidenBeachDialogue.transform.Find("Scene2").gameObject;
                    maidenBeachDialogueScene3 = maidenBeachDialogue.transform.Find("Scene3").gameObject;
                    maidenBeachDialogueScene4 = maidenBeachDialogue.transform.Find("Scene4").gameObject;
                    maidenBeachDialogueScene5 = maidenBeachDialogue.transform.Find("Scene5").gameObject;
                    maidenBeachDialogueActivator = maidenBeachDialogue.transform.Find("DialogueActivator").gameObject;
                    maidenBeachDialogueFinisher = maidenBeachDialogue.transform.Find("DialogueFinisher").gameObject;
                    maidenBeachDialogueMouthActivator = maidenBeachDialogue.transform.Find("MouthActivator").gameObject;
                    maidenBeachDialogueSpriteFocus = maidenBeachDialogue.transform.Find("SpriteFocus").gameObject;

                    maryBeachDialogue = CreateNewDialogue("MaryDialogueBeach01", Places.secretBeachRoomtalk.transform);
                    maryBeachDialogueScene1 = maryBeachDialogue.transform.Find("Scene1").gameObject;
                    maryBeachDialogueScene2 = maryBeachDialogue.transform.Find("Scene2").gameObject;
                    maryBeachDialogueScene3 = maryBeachDialogue.transform.Find("Scene3").gameObject;
                    maryBeachDialogueScene4 = maryBeachDialogue.transform.Find("Scene4").gameObject;
                    maryBeachDialogueScene5 = maryBeachDialogue.transform.Find("Scene5").gameObject;
                    maryBeachDialogueActivator = maryBeachDialogue.transform.Find("DialogueActivator").gameObject;
                    maryBeachDialogueFinisher = maryBeachDialogue.transform.Find("DialogueFinisher").gameObject;
                    maryBeachDialogueMouthActivator = maryBeachDialogue.transform.Find("MouthActivator").gameObject;
                    maryBeachDialogueSpriteFocus = maryBeachDialogue.transform.Find("SpriteFocus").gameObject;

                    mastBeachDialogue = CreateNewDialogue("MastDialogueBeach01", Places.secretBeachRoomtalk.transform);
                    mastBeachDialogueScene1 = mastBeachDialogue.transform.Find("Scene1").gameObject;
                    mastBeachDialogueScene2 = mastBeachDialogue.transform.Find("Scene2").gameObject;
                    mastBeachDialogueScene3 = mastBeachDialogue.transform.Find("Scene3").gameObject;
                    mastBeachDialogueScene4 = mastBeachDialogue.transform.Find("Scene4").gameObject;
                    mastBeachDialogueScene5 = mastBeachDialogue.transform.Find("Scene5").gameObject;
                    mastBeachDialogueActivator = mastBeachDialogue.transform.Find("DialogueActivator").gameObject;
                    mastBeachDialogueFinisher = mastBeachDialogue.transform.Find("DialogueFinisher").gameObject;
                    mastBeachDialogueMouthActivator = mastBeachDialogue.transform.Find("MouthActivator").gameObject;
                    mastBeachDialogueSpriteFocus = mastBeachDialogue.transform.Find("SpriteFocus").gameObject;

                    neonBeachDialogue = CreateNewDialogue("NeonDialogueBeach01", Places.secretBeachRoomtalk.transform);
                    neonBeachDialogueScene1 = neonBeachDialogue.transform.Find("Scene1").gameObject;
                    neonBeachDialogueScene2 = neonBeachDialogue.transform.Find("Scene2").gameObject;
                    neonBeachDialogueScene3 = neonBeachDialogue.transform.Find("Scene3").gameObject;
                    neonBeachDialogueScene4 = neonBeachDialogue.transform.Find("Scene4").gameObject;
                    neonBeachDialogueScene5 = neonBeachDialogue.transform.Find("Scene5").gameObject;
                    neonBeachDialogueActivator = neonBeachDialogue.transform.Find("DialogueActivator").gameObject;
                    neonBeachDialogueFinisher = neonBeachDialogue.transform.Find("DialogueFinisher").gameObject;
                    neonBeachDialogueMouthActivator = neonBeachDialogue.transform.Find("MouthActivator").gameObject;
                    neonBeachDialogueSpriteFocus = neonBeachDialogue.transform.Find("SpriteFocus").gameObject;

                    pepperBeachDialogue = CreateNewDialogue("PepperDialogueBeach01", Places.secretBeachRoomtalk.transform);
                    pepperBeachDialogueScene1 = pepperBeachDialogue.transform.Find("Scene1").gameObject;
                    pepperBeachDialogueScene2 = pepperBeachDialogue.transform.Find("Scene2").gameObject;
                    pepperBeachDialogueScene3 = pepperBeachDialogue.transform.Find("Scene3").gameObject;
                    pepperBeachDialogueScene4 = pepperBeachDialogue.transform.Find("Scene4").gameObject;
                    pepperBeachDialogueScene5 = pepperBeachDialogue.transform.Find("Scene5").gameObject;
                    pepperBeachDialogueActivator = pepperBeachDialogue.transform.Find("DialogueActivator").gameObject;
                    pepperBeachDialogueFinisher = pepperBeachDialogue.transform.Find("DialogueFinisher").gameObject;
                    pepperBeachDialogueMouthActivator = pepperBeachDialogue.transform.Find("MouthActivator").gameObject;
                    pepperBeachDialogueSpriteFocus = pepperBeachDialogue.transform.Find("SpriteFocus").gameObject;

                    rapiBeachDialogue = CreateNewDialogue("RapiDialogueBeach01", Places.secretBeachRoomtalk.transform);
                    rapiBeachDialogueScene1 = rapiBeachDialogue.transform.Find("Scene1").gameObject;
                    rapiBeachDialogueScene2 = rapiBeachDialogue.transform.Find("Scene2").gameObject;
                    rapiBeachDialogueScene3 = rapiBeachDialogue.transform.Find("Scene3").gameObject;
                    rapiBeachDialogueScene4 = rapiBeachDialogue.transform.Find("Scene4").gameObject;
                    rapiBeachDialogueScene5 = rapiBeachDialogue.transform.Find("Scene5").gameObject;
                    rapiBeachDialogueActivator = rapiBeachDialogue.transform.Find("DialogueActivator").gameObject;
                    rapiBeachDialogueFinisher = rapiBeachDialogue.transform.Find("DialogueFinisher").gameObject;
                    rapiBeachDialogueMouthActivator = rapiBeachDialogue.transform.Find("MouthActivator").gameObject;
                    rapiBeachDialogueSpriteFocus = rapiBeachDialogue.transform.Find("SpriteFocus").gameObject;

                    rosannaBeachDialogue = CreateNewDialogue("RosannaDialogueBeach01", Places.secretBeachRoomtalk.transform);
                    rosannaBeachDialogueScene1 = rosannaBeachDialogue.transform.Find("Scene1").gameObject;
                    rosannaBeachDialogueScene2 = rosannaBeachDialogue.transform.Find("Scene2").gameObject;
                    rosannaBeachDialogueScene3 = rosannaBeachDialogue.transform.Find("Scene3").gameObject;
                    rosannaBeachDialogueScene4 = rosannaBeachDialogue.transform.Find("Scene4").gameObject;
                    rosannaBeachDialogueScene5 = rosannaBeachDialogue.transform.Find("Scene5").gameObject;
                    rosannaBeachDialogueActivator = rosannaBeachDialogue.transform.Find("DialogueActivator").gameObject;
                    rosannaBeachDialogueFinisher = rosannaBeachDialogue.transform.Find("DialogueFinisher").gameObject;
                    rosannaBeachDialogueMouthActivator = rosannaBeachDialogue.transform.Find("MouthActivator").gameObject;
                    rosannaBeachDialogueSpriteFocus = rosannaBeachDialogue.transform.Find("SpriteFocus").gameObject;

                    sakuraBeachDialogue = CreateNewDialogue("SakuraDialogueBeach01", Places.secretBeachRoomtalk.transform);
                    sakuraBeachDialogueScene1 = sakuraBeachDialogue.transform.Find("Scene1").gameObject;
                    sakuraBeachDialogueScene2 = sakuraBeachDialogue.transform.Find("Scene2").gameObject;
                    sakuraBeachDialogueScene3 = sakuraBeachDialogue.transform.Find("Scene3").gameObject;
                    sakuraBeachDialogueScene4 = sakuraBeachDialogue.transform.Find("Scene4").gameObject;
                    sakuraBeachDialogueScene5 = sakuraBeachDialogue.transform.Find("Scene5").gameObject;
                    sakuraBeachDialogueActivator = sakuraBeachDialogue.transform.Find("DialogueActivator").gameObject;
                    sakuraBeachDialogueFinisher = sakuraBeachDialogue.transform.Find("DialogueFinisher").gameObject;
                    sakuraBeachDialogueMouthActivator = sakuraBeachDialogue.transform.Find("MouthActivator").gameObject;
                    sakuraBeachDialogueSpriteFocus = sakuraBeachDialogue.transform.Find("SpriteFocus").gameObject;

                    viperBeachDialogue = CreateNewDialogue("ViperDialogueBeach01", Places.secretBeachRoomtalk.transform);
                    viperBeachDialogueScene1 = viperBeachDialogue.transform.Find("Scene1").gameObject;
                    viperBeachDialogueScene2 = viperBeachDialogue.transform.Find("Scene2").gameObject;
                    viperBeachDialogueScene3 = viperBeachDialogue.transform.Find("Scene3").gameObject;
                    viperBeachDialogueScene4 = viperBeachDialogue.transform.Find("Scene4").gameObject;
                    viperBeachDialogueScene5 = viperBeachDialogue.transform.Find("Scene5").gameObject;
                    viperBeachDialogueActivator = viperBeachDialogue.transform.Find("DialogueActivator").gameObject;
                    viperBeachDialogueFinisher = viperBeachDialogue.transform.Find("DialogueFinisher").gameObject;
                    viperBeachDialogueMouthActivator = viperBeachDialogue.transform.Find("MouthActivator").gameObject;
                    viperBeachDialogueSpriteFocus = viperBeachDialogue.transform.Find("SpriteFocus").gameObject;

                    yanBeachDialogue = CreateNewDialogue("YanDialogueBeach01", Places.secretBeachRoomtalk.transform);
                    yanBeachDialogueScene1 = yanBeachDialogue.transform.Find("Scene1").gameObject;
                    yanBeachDialogueScene2 = yanBeachDialogue.transform.Find("Scene2").gameObject;
                    yanBeachDialogueScene3 = yanBeachDialogue.transform.Find("Scene3").gameObject;
                    yanBeachDialogueScene4 = yanBeachDialogue.transform.Find("Scene4").gameObject;
                    yanBeachDialogueScene5 = yanBeachDialogue.transform.Find("Scene5").gameObject;
                    yanBeachDialogueActivator = yanBeachDialogue.transform.Find("DialogueActivator").gameObject;
                    yanBeachDialogueFinisher = yanBeachDialogue.transform.Find("DialogueFinisher").gameObject;
                    yanBeachDialogueMouthActivator = yanBeachDialogue.transform.Find("MouthActivator").gameObject;
                    yanBeachDialogueSpriteFocus = yanBeachDialogue.transform.Find("SpriteFocus").gameObject;
                    #endregion

                    SetActorOverrideSpeechSkinValue(sBDialogueMainFirst, "PlayerActor", overrideSpeechSkinGreen);
                    SetActorOverrideSpeechSkinValue(sBDialogueStory01, "AmberActor", overrideSpeechSkinYellow);

                    Logger.LogInfo("----- DIALOGUES LOADED -----");
                    loadedDialogues = true;
                }
            }
            if (Core.currentScene.name == "GameStart")
            {
                if (loadedDialogues)
                {
                    Logger.LogInfo("----- DIALOGUES UNLOADED -----");
                    loadedDialogues = false;
                }
            }
        }

        public GameObject CreateNewDialogue(string bundleAsset, Transform roomTalk)
        {
            // Check if dialogue bundle is loaded
            if (Core.dialogueBundle == null)
            {
                Debug.LogError($"Core.dialogueBundle is null! Cannot create dialogue '{bundleAsset}'");
                return null;
            }

            // Try to load the asset from the bundle
            GameObject dialogue = Core.dialogueBundle.LoadAsset<GameObject>(bundleAsset);
            
            // Check if the asset was found
            if (dialogue == null)
            {
                Debug.LogError($"Failed to load asset '{bundleAsset}' from dialogue bundle. Bundle may be missing this asset.");
                
                // List all available assets in the bundle for debugging
                Debug.Log($"Available assets in dialogue bundle:");
                foreach (var assetName in Core.dialogueBundle.GetAllAssetNames())
                {
                    Debug.Log($"- {assetName}");
                }
                return null;
            }

            // Check if roomTalk transform is valid
            if (roomTalk == null)
            {
                Debug.LogError($"roomTalk transform is null! Cannot instantiate dialogue '{bundleAsset}'");
                return null;
            }

            // Instantiate the dialogue
            GameObject dialogueInstance = GameObject.Instantiate(dialogue, roomTalk);
            dialogueInstance.name = bundleAsset;
            
            // Set the dialogue skin
            try
            {
                var referenceDialogue = Core.coreEvents.Find("SmallTalks")?.Find("FailedGroceries")?.Find("GameObject")?.GetComponent<Dialogue>();
                if (referenceDialogue != null)
                {
                    dialogueInstance.GetComponent<Dialogue>().Story.Content.DialogueSkin = referenceDialogue.Story.Content.DialogueSkin;
                }
                else
                {
                    Debug.LogWarning($"Could not find reference dialogue for skin. Dialogue '{bundleAsset}' may not have proper skin.");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Error setting dialogue skin for '{bundleAsset}': {e.Message}");
            }
            
            Debug.Log($"Successfully created dialogue: {bundleAsset}");
            return dialogueInstance;
        }
        public static GameObject GetActorOverrideSpeechSkinValue(GameObject dialogueGO, string actorName)
        {
            var dialogue = dialogueGO.GetComponent(typeof(GameCreator.Runtime.Dialogue.Dialogue));
            if (dialogue == null) return null;

            var storyProp = dialogue.GetType().GetProperty("Story", BindingFlags.Public | BindingFlags.Instance);
            var story = storyProp.GetValue(dialogue);
            var contentProp = story.GetType().GetProperty("Content", BindingFlags.Public | BindingFlags.Instance);
            var content = contentProp.GetValue(story);

            var rolesField = content.GetType().GetField("m_Roles", BindingFlags.NonPublic | BindingFlags.Instance);
            var roles = rolesField.GetValue(content) as Array;
            if (roles == null) return null;

            foreach (var roleObj in roles)
            {
                var actorField = roleObj.GetType().GetField("m_Actor", BindingFlags.NonPublic | BindingFlags.Instance);
                var actor = actorField.GetValue(roleObj);
                if (actor != null)
                {
                    var nameProp = actor.GetType().GetProperty("name", BindingFlags.Public | BindingFlags.Instance);
                    string thisActorName = nameProp?.GetValue(actor) as string;
                    if (thisActorName == actorName)
                    {
                        var overrideSpeechSkinField = actor.GetType().GetField("m_OverrideSpeechSkin", BindingFlags.NonPublic | BindingFlags.Instance);
                        var speechSkin = overrideSpeechSkinField.GetValue(actor);
                        if (speechSkin != null)
                        {
                            var mValueField = speechSkin.GetType().GetField("m_Value", BindingFlags.NonPublic | BindingFlags.Instance);
                            var prefab = mValueField.GetValue(speechSkin) as GameObject;
                            return prefab;
                        }
                    }
                }
            }
            return null;
        }

        public static void SetActorOverrideSpeechSkinValue(GameObject dialogueGO, string actorName, GameObject newSkinPrefab)
        {
            var dialogue = dialogueGO.GetComponent(typeof(GameCreator.Runtime.Dialogue.Dialogue));
            if (dialogue == null)
            {
                Debug.LogError($"No Dialogue component found on {dialogueGO.name}");
                return;
            }

            var storyProp = dialogue.GetType().GetProperty("Story", BindingFlags.Public | BindingFlags.Instance);
            var story = storyProp.GetValue(dialogue);
            if (story == null)
            {
                Debug.LogError("Dialogue.Story is null.");
                return;
            }

            var contentProp = story.GetType().GetProperty("Content", BindingFlags.Public | BindingFlags.Instance);
            var content = contentProp.GetValue(story);
            if (content == null)
            {
                Debug.LogError("Dialogue.Story.Content is null.");
                return;
            }

            var rolesField = content.GetType().GetField("m_Roles", BindingFlags.NonPublic | BindingFlags.Instance);
            var roles = rolesField.GetValue(content) as Array;
            if (roles == null)
            {
                Debug.LogError("Could not find Roles in Dialogue content.");
                return;
            }

            bool actorFound = false;
            foreach (var roleObj in roles)
            {
                if (roleObj == null) continue;

                var actorField = roleObj.GetType().GetField("m_Actor", BindingFlags.NonPublic | BindingFlags.Instance);
                var actor = actorField.GetValue(roleObj);
                if (actor != null)
                {
                    var nameProp = actor.GetType().GetProperty("name", BindingFlags.Public | BindingFlags.Instance);
                    string thisActorName = nameProp?.GetValue(actor) as string;
                    if (thisActorName == actorName)
                    {
                        actorFound = true;
                        var overrideSpeechSkinField = actor.GetType().GetField("m_OverrideSpeechSkin", BindingFlags.NonPublic | BindingFlags.Instance);
                        if (overrideSpeechSkinField == null)
                        {
                            Debug.LogError($"Could not find m_OverrideSpeechSkin field on Actor object for '{actorName}'.");
                            break; 
                        }

                        var speechSkin = overrideSpeechSkinField.GetValue(actor);

                        if (speechSkin == null)
                        {
                            try
                            {
                                Type speechSkinType = overrideSpeechSkinField.FieldType;
                                if (typeof(ScriptableObject).IsAssignableFrom(speechSkinType))
                                {
                                    speechSkin = ScriptableObject.CreateInstance(speechSkinType);
                                }
                                else
                                {
                                    speechSkin = Activator.CreateInstance(speechSkinType);
                                    Debug.Log($"Created regular instance of {speechSkinType.Name} for actor '{actorName}'.");
                                }
                                overrideSpeechSkinField.SetValue(actor, speechSkin);
                            }
                            catch (Exception e)
                            {
                                Debug.LogError($"Failed to create an instance of the OverrideSpeechSkin for actor '{actorName}'. Exception: {e}");
                                break;
                            }
                        }
                        
                        var mValueField = speechSkin.GetType().GetField("m_Value", BindingFlags.NonPublic | BindingFlags.Instance);
                        if (mValueField != null)
                        {
                            mValueField.SetValue(speechSkin, newSkinPrefab);
                            Debug.Log($"Set override speech skin for actor '{actorName}' to '{(newSkinPrefab ? newSkinPrefab.name : "null")}'.");
                        }
                        else
                        {
                            Debug.LogError($"Could not find m_Value field on SpeechSkin object for actor '{actorName}'.");
                        }

                        break; 
                    }
                }
            }

            if (!actorFound)
            {
                Debug.LogError($"Actor with name '{actorName}' not found in dialogue '{dialogueGO.name}'.");
            }
        }

        public class ExpressionInfo
        {
            public string Id;
            public string Name;
            public Sprite Sprite; // or Texture2D, if that's what is used
        }

        public static List<ExpressionInfo> GetAllActorExpressions(Actor actor)
        {
            var result = new List<ExpressionInfo>();
            if (actor == null) return result;

            var expressions = actor.GetType().GetField("m_Expressions", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(actor);
            if (expressions == null) return result;

            var lengthProp = expressions.GetType().GetProperty("Length", BindingFlags.Public | BindingFlags.Instance);
            int length = (int)(lengthProp?.GetValue(expressions) ?? 0);

            var fromIndexMethod = expressions.GetType().GetMethod("FromIndex", BindingFlags.Public | BindingFlags.Instance);

            for (int i = 0; i < length; i++)
            {
                var expression = fromIndexMethod.Invoke(expressions, new object[] { i });
                if (expression != null)
                {
                    var idProp = expression.GetType().GetProperty("Id", BindingFlags.Public | BindingFlags.Instance);
                    var nameProp = expression.GetType().GetProperty("Name", BindingFlags.Public | BindingFlags.Instance);
                    var spriteProp = expression.GetType().GetProperty("Sprite", BindingFlags.Public | BindingFlags.Instance); // or Texture

                    result.Add(new ExpressionInfo
                    {
                        Id = idProp?.GetValue(expression)?.ToString(),
                        Name = nameProp?.GetValue(expression)?.ToString(),
                        Sprite = spriteProp?.GetValue(expression) as Sprite
                    });
                }
            }
            return result;
        }

        public static void AddExpressionSetInstructionToOnStart(GameObject dialogueGO, string actorName, int expressionIndex, int value)
        {
            if (dialogueGO == null)
            {
                Debug.LogError("[AddExpressionSetInstructionToOnStart] dialogueGO is null.");
                return;
            }

            // 1. Extract character name from actor name (remove "Actor" suffix)
            string characterName = actorName;
            if (actorName.EndsWith("Actor"))
            {
                characterName = actorName.Substring(0, actorName.Length - 5); // Remove "Actor" suffix
            }

            // 2. Compose the variable name
            string variableName = characterName + "-expression";

            // 3. Check if the global name variable exists and create it if it doesn't
            var manager = GlobalNameVariablesManager.Instance;
            if (manager == null)
            {
                Debug.LogError("[AddExpressionSetInstructionToOnStart] GlobalNameVariablesManager not initialized");
                return;
            }

            PropertyInfo valuesProp = typeof(GlobalNameVariablesManager).GetProperty("Values", BindingFlags.NonPublic | BindingFlags.Instance);
            var values = valuesProp.GetValue(manager) as Dictionary<IdString, NameVariableRuntime>;
            bool exists = false;
            NameVariableRuntime targetRuntime = null;
            
            if (values != null)
            {
                foreach (var pair in values)
                {
                    PropertyInfo varsProp = typeof(NameVariableRuntime).GetProperty("Variables", BindingFlags.NonPublic | BindingFlags.Instance);
                    var variables = varsProp.GetValue(pair.Value) as Dictionary<string, NameVariable>;
                    if (variables != null && variables.ContainsKey(variableName))
                    {
                        exists = true;
                        targetRuntime = pair.Value;
                        break;
                    }
                }
            }

            // 4. Create the variable if it doesn't exist
            if (!exists)
            {
                Debug.Log($"[AddExpressionSetInstructionToOnStart] Creating global variable '{variableName}'");
                
                // Get the first available runtime to add the variable to
                if (values != null && values.Count > 0)
                {
                    targetRuntime = values.First().Value;
                    PropertyInfo varsProp = typeof(NameVariableRuntime).GetProperty("Variables", BindingFlags.NonPublic | BindingFlags.Instance);
                    var variables = varsProp.GetValue(targetRuntime) as Dictionary<string, NameVariable>;
                    
                    if (variables != null)
                    {
                        // Create a new NameVariable for numbers
                        var nameVarType = Type.GetType("GameCreator.Runtime.Variables.NameVariable, GameCreator.Runtime.Variables");
                        if (nameVarType != null)
                        {
                            var newVariable = Activator.CreateInstance(nameVarType);
                            
                            // Create a proper TValue for numbers
                            var tValueType = Type.GetType("GameCreator.Runtime.Variables.TValue, GameCreator.Runtime.Variables");
                            var createValueMethod = tValueType?.GetMethod("CreateValue", BindingFlags.Public | BindingFlags.Static);
                            if (createValueMethod != null)
                            {
                                // Find the type ID for numbers (usually "number" or similar)
                                var typeIDType = Type.GetType("GameCreator.Runtime.Common.IdString, GameCreator.Runtime.Common");
                                var typeID = Activator.CreateInstance(typeIDType, "number");
                                var tValue = createValueMethod.Invoke(null, new object[] { typeID, 0.0 });
                                
                                // Set the m_Value field
                                var mValueField = newVariable.GetType().GetField("m_Value", BindingFlags.NonPublic | BindingFlags.Instance);
                                mValueField?.SetValue(newVariable, tValue);
                                
                                // Set the m_Name field
                                var mNameField = newVariable.GetType().GetField("m_Name", BindingFlags.NonPublic | BindingFlags.Instance);
                                var nameIdString = Activator.CreateInstance(typeIDType, variableName);
                                mNameField?.SetValue(newVariable, nameIdString);
                                
                                variables[variableName] = newVariable as NameVariable;
                                exists = true;
                                Debug.Log($"[AddExpressionSetInstructionToOnStart] Successfully created global variable '{variableName}'");
                            }
                        }
                    }
                }
                
                if (!exists)
                {
                    Debug.LogError($"[AddExpressionSetInstructionToOnStart] Failed to create global variable '{variableName}'");
                    return;
                }
            }

            // 3. Get the Actor from the dialogue (similar to SetActorOverrideSpeechSkinValue)
            var dialogue = dialogueGO.GetComponent(typeof(GameCreator.Runtime.Dialogue.Dialogue));
            if (dialogue == null)
            {
                Debug.LogError($"[AddExpressionSetInstructionToOnStart] Dialogue component not found on {dialogueGO.name}");
                return;
            }

            var storyProp = dialogue.GetType().GetProperty("Story", BindingFlags.Public | BindingFlags.Instance);
            var story = storyProp?.GetValue(dialogue);
            if (story == null)
            {
                Debug.LogError("[AddExpressionSetInstructionToOnStart] Dialogue.Story is null.");
                return;
            }

            var contentProp = story.GetType().GetProperty("Content", BindingFlags.Public | BindingFlags.Instance);
            var content = contentProp?.GetValue(story);
            if (content == null)
            {
                Debug.LogError("[AddExpressionSetInstructionToOnStart] Dialogue.Story.Content is null.");
                return;
            }

            var rolesField = content.GetType().GetField("m_Roles", BindingFlags.NonPublic | BindingFlags.Instance);
            var roles = rolesField?.GetValue(content) as Array;
            if (roles == null)
            {
                Debug.LogError("[AddExpressionSetInstructionToOnStart] Could not find Roles in Dialogue content.");
                return;
            }

            Actor targetActor = null;
            foreach (var roleObj in roles)
            {
                if (roleObj == null) continue;

                var actorField = roleObj.GetType().GetField("m_Actor", BindingFlags.NonPublic | BindingFlags.Instance);
                var actor = actorField?.GetValue(roleObj) as Actor;
                if (actor != null)
                {
                    var nameProp = actor.GetType().GetProperty("name", BindingFlags.Public | BindingFlags.Instance);
                    string thisActorName = nameProp?.GetValue(actor) as string;
                    if (thisActorName == actorName)
                    {
                        targetActor = actor;
                        break;
                    }
                }
            }

            if (targetActor == null)
            {
                Debug.LogError($"[AddExpressionSetInstructionToOnStart] Actor '{actorName}' not found in dialogue '{dialogueGO.name}'.");
                return;
            }

            // 4. Get the expressions from the actor
            var expressionsField = targetActor.GetType().GetField("m_Expressions", BindingFlags.NonPublic | BindingFlags.Instance);
            var expressions = expressionsField?.GetValue(targetActor);
            if (expressions == null)
            {
                Debug.LogError($"[AddExpressionSetInstructionToOnStart] No expressions found for actor: {actorName}");
                return;
            }

            // 5. Get the specific expression by index
            var fromIndexMethod = expressions.GetType().GetMethod("FromIndex", BindingFlags.Public | BindingFlags.Instance);
            var expression = fromIndexMethod?.Invoke(expressions, new object[] { expressionIndex });
            if (expression == null)
            {
                Debug.LogError($"[AddExpressionSetInstructionToOnStart] Expression at index {expressionIndex} not found for actor: {actorName}");
                return;
            }

            try
            {
                // 6. Create a new InstructionArithmeticSetNumber
                var instrType = Type.GetType("GameCreator.Runtime.VisualScripting.InstructionArithmeticSetNumber, GameCreator.Runtime.VisualScripting, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
                if (instrType == null)
                {
                    // Try alternative type names
                    instrType = Type.GetType("GameCreator.Runtime.VisualScripting.InstructionArithmeticSetNumber");
                    if (instrType == null)
                    {
                        // Try to find it in loaded assemblies
                        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                        {
                            if (assembly.FullName.Contains("GameCreator"))
                            {
                                instrType = assembly.GetType("GameCreator.Runtime.VisualScripting.InstructionArithmeticSetNumber");
                                if (instrType != null) break;
                            }
                        }
                    }
                }
                
                if (instrType == null)
                {
                    Debug.LogError("[AddExpressionSetInstructionToOnStart] Could not find InstructionArithmeticSetNumber type");
                    return;
                }
                var newInstr = Activator.CreateInstance(instrType);

                // 7. Set up m_Set (SetNumberGlobalName)
                var setType = Type.GetType("GameCreator.Runtime.Variables.SetNumberGlobalName, GameCreator.Runtime.Variables");
                if (setType == null)
                {
                    // Try alternative type names
                    setType = Type.GetType("GameCreator.Runtime.Variables.SetNumberGlobalName");
                    if (setType == null)
                    {
                        // Try to find it in loaded assemblies
                        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                        {
                            if (assembly.FullName.Contains("GameCreator"))
                            {
                                setType = assembly.GetType("GameCreator.Runtime.Variables.SetNumberGlobalName");
                                if (setType != null) break;
                            }
                        }
                    }
                }
                
                if (setType == null)
                {
                    Debug.LogError("[AddExpressionSetInstructionToOnStart] Could not find SetNumberGlobalName type");
                    return;
                }
                var setInstance = Activator.CreateInstance(setType);
                var mVarField = setType.GetField("m_Variable", BindingFlags.NonPublic | BindingFlags.Instance);
                if (mVarField != null)
                {
                    var fieldSetGlobalNameType = mVarField.FieldType;
                    var fieldSetGlobalName = Activator.CreateInstance(fieldSetGlobalNameType, 2); // 2 = TYPE_ID for numbers
                    var mNameField = fieldSetGlobalNameType.GetField("m_Name", BindingFlags.NonPublic | BindingFlags.Instance);
                    if (mNameField != null)
                    {
                        mNameField.SetValue(fieldSetGlobalName, variableName);
                    }
                    mVarField.SetValue(setInstance, fieldSetGlobalName);
                }

                var propertySetNumberType = Type.GetType("GameCreator.Runtime.Common.PropertySetNumber, GameCreator.Runtime.Common");
                if (propertySetNumberType == null)
                {
                    // Try alternative type names
                    propertySetNumberType = Type.GetType("GameCreator.Runtime.Common.PropertySetNumber");
                    if (propertySetNumberType == null)
                    {
                        // Try to find it in loaded assemblies
                        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                        {
                            if (assembly.FullName.Contains("GameCreator"))
                            {
                                propertySetNumberType = assembly.GetType("GameCreator.Runtime.Common.PropertySetNumber");
                                if (propertySetNumberType != null) break;
                            }
                        }
                    }
                }
                
                if (propertySetNumberType == null)
                {
                    Debug.LogError("[AddExpressionSetInstructionToOnStart] Could not find PropertySetNumber type");
                    return;
                }
                var propertySetNumber = Activator.CreateInstance(propertySetNumberType, setInstance);

                var setField = instrType.GetField("m_Set", BindingFlags.NonPublic | BindingFlags.Instance);
                if (setField != null)
                {
                    setField.SetValue(newInstr, propertySetNumber);
                }

                // 8. Set up m_From (GetDecimalDecimal)
                var getDecimalType = Type.GetType("GameCreator.Runtime.Common.GetDecimalDecimal, GameCreator.Runtime.Common");
                if (getDecimalType == null)
                {
                    // Try alternative type names
                    getDecimalType = Type.GetType("GameCreator.Runtime.Common.GetDecimalDecimal");
                    if (getDecimalType == null)
                    {
                        // Try to find it in loaded assemblies
                        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                        {
                            if (assembly.FullName.Contains("GameCreator"))
                            {
                                getDecimalType = assembly.GetType("GameCreator.Runtime.Common.GetDecimalDecimal");
                                if (getDecimalType != null) break;
                            }
                        }
                    }
                }
                
                if (getDecimalType == null)
                {
                    Debug.LogError("[AddExpressionSetInstructionToOnStart] Could not find GetDecimalDecimal type");
                    return;
                }
                
                var getDecimal = Activator.CreateInstance(getDecimalType, (double)value);
                var propertyGetDecimalType = Type.GetType("GameCreator.Runtime.Common.PropertyGetDecimal, GameCreator.Runtime.Common");
                if (propertyGetDecimalType == null)
                {
                    // Try alternative type names
                    propertyGetDecimalType = Type.GetType("GameCreator.Runtime.Common.PropertyGetDecimal");
                    if (propertyGetDecimalType == null)
                    {
                        // Try to find it in loaded assemblies
                        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                        {
                            if (assembly.FullName.Contains("GameCreator"))
                            {
                                propertyGetDecimalType = assembly.GetType("GameCreator.Runtime.Common.PropertyGetDecimal");
                                if (propertyGetDecimalType != null) break;
                            }
                        }
                    }
                }
                
                if (propertyGetDecimalType == null)
                {
                    Debug.LogError("[AddExpressionSetInstructionToOnStart] Could not find PropertyGetDecimal type");
                    return;
                }
                var propertyGetDecimal = Activator.CreateInstance(propertyGetDecimalType, getDecimal);

                var fromField = instrType.GetField("m_From", BindingFlags.NonPublic | BindingFlags.Instance);
                if (fromField != null)
                {
                    fromField.SetValue(newInstr, propertyGetDecimal);
                }

                // 9. Add to the On Start InstructionList
                var onStartField = expression.GetType().GetField("m_InstructionsOnStart", BindingFlags.NonPublic | BindingFlags.Instance);
                var onStart = onStartField?.GetValue(expression);
                if (onStart == null)
                {
                    Debug.LogError("[AddExpressionSetInstructionToOnStart] Could not access m_InstructionsOnStart field");
                    return;
                }

                var instrListField = onStart.GetType().GetField("m_Instructions", BindingFlags.NonPublic | BindingFlags.Instance);
                var instrList = instrListField?.GetValue(onStart);
                if (instrList == null)
                {
                    Debug.LogError("[AddExpressionSetInstructionToOnStart] Could not access m_Instructions field");
                    return;
                }

                var addMethod = instrList.GetType().GetMethod("Add", BindingFlags.Public | BindingFlags.Instance);
                if (addMethod != null)
                {
                    addMethod.Invoke(instrList, new object[] { newInstr });
                    Debug.Log($"[AddExpressionSetInstructionToOnStart] Successfully added InstructionArithmeticSetNumber to OnStart for variable '{variableName}' with value {value} on {actorName} expression {expressionIndex} in dialogue '{dialogueGO.name}'.");
                }
                else
                {
                    Debug.LogError("[AddExpressionSetInstructionToOnStart] Could not find Add method on InstructionList");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[AddExpressionSetInstructionToOnStart] Error creating instruction: {e.Message}");
            }
        }
    }
}
