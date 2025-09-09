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
using System.Text.RegularExpressions;
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

        #region Secret Beach Variables
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
        #endregion
        #region Mountain Lab Variables
        public static GameObject mLDialogueMainFirst;
        public static GameObject mLDialogueMainFirstScene1;
        public static GameObject mLDialogueMainFirstScene2;
        public static GameObject mLDialogueMainFirstScene3;
        public static GameObject mLDialogueMainFirstScene4;
        public static GameObject mLDialogueMainFirstScene5;
        public static GameObject mLDialogueMainFirstDialogueActivator;
        public static GameObject mLDialogueMainFirstDialogueFinisher;
        public static GameObject mLDialogueMainFirstMouthActivator;
        public static GameObject mLDialogueMainFirstSpriteFocus;

        public static GameObject amberDefaultDialogue;
        public static GameObject amberDefaultDialogueScene1;
        public static GameObject amberDefaultDialogueScene2;
        public static GameObject amberDefaultDialogueScene3;
        public static GameObject amberDefaultDialogueScene4;
        public static GameObject amberDefaultDialogueScene5;
        public static GameObject amberDefaultDialogueDialogueActivator;
        public static GameObject amberDefaultDialogueDialogueFinisher;
        public static GameObject amberDefaultDialogueMouthActivator;
        public static GameObject amberDefaultDialogueSpriteFocus;

        public static GameObject mLDialogueMainStory03;
        public static GameObject mLDialogueMainStory03Scene1;
        public static GameObject mLDialogueMainStory03Scene2;
        public static GameObject mLDialogueMainStory03Scene3;
        public static GameObject mLDialogueMainStory03Scene4;
        public static GameObject mLDialogueMainStory03Scene5;
        public static GameObject mLDialogueMainStory03DialogueActivator;
        public static GameObject mLDialogueMainStory03DialogueFinisher;
        public static GameObject mLDialogueMainStory03MouthActivator;
        public static GameObject mLDialogueMainStory03SpriteFocus;

        public static GameObject mLDialogueMainStory04;
        public static GameObject mLDialogueMainStory04Scene1;
        public static GameObject mLDialogueMainStory04Scene2;
        public static GameObject mLDialogueMainStory04Scene3;
        public static GameObject mLDialogueMainStory04Scene4;
        public static GameObject mLDialogueMainStory04Scene5;
        public static GameObject mLDialogueMainStory04DialogueActivator;
        public static GameObject mLDialogueMainStory04DialogueFinisher;
        public static GameObject mLDialogueMainStory04MouthActivator;
        public static GameObject mLDialogueMainStory04SpriteFocus;
        #endregion
        #region Character Variables
        public static GameObject anisDefaultDialogue;
        public static GameObject anisDefaultDialogueScene1;
        public static GameObject anisDefaultDialogueScene2;
        public static GameObject anisDefaultDialogueScene3;
        public static GameObject anisDefaultDialogueScene4;
        public static GameObject anisDefaultDialogueScene5;
        public static GameObject anisDefaultDialogueDialogueActivator;
        public static GameObject anisDefaultDialogueDialogueFinisher;
        public static GameObject anisDefaultDialogueMouthActivator;
        public static GameObject anisDefaultDialogueSpriteFocus;

        public static GameObject centiDefaultDialogue;
        public static GameObject centiDefaultDialogueScene1;
        public static GameObject centiDefaultDialogueScene2;
        public static GameObject centiDefaultDialogueScene3;
        public static GameObject centiDefaultDialogueScene4;
        public static GameObject centiDefaultDialogueScene5;
        public static GameObject centiDefaultDialogueDialogueActivator;
        public static GameObject centiDefaultDialogueDialogueFinisher;
        public static GameObject centiDefaultDialogueMouthActivator;
        public static GameObject centiDefaultDialogueSpriteFocus;

        public static GameObject dorothyDefaultDialogue;
        public static GameObject dorothyDefaultDialogueScene1;
        public static GameObject dorothyDefaultDialogueScene2;
        public static GameObject dorothyDefaultDialogueScene3;
        public static GameObject dorothyDefaultDialogueScene4;
        public static GameObject dorothyDefaultDialogueScene5;
        public static GameObject dorothyDefaultDialogueDialogueActivator;
        public static GameObject dorothyDefaultDialogueDialogueFinisher;
        public static GameObject dorothyDefaultDialogueMouthActivator;
        public static GameObject dorothyDefaultDialogueSpriteFocus;

        public static GameObject eleggDefaultDialogue;
        public static GameObject eleggDefaultDialogueScene1;
        public static GameObject eleggDefaultDialogueScene2;
        public static GameObject eleggDefaultDialogueScene3;
        public static GameObject eleggDefaultDialogueScene4;
        public static GameObject eleggDefaultDialogueScene5;
        public static GameObject eleggDefaultDialogueDialogueActivator;
        public static GameObject eleggDefaultDialogueDialogueFinisher;
        public static GameObject eleggDefaultDialogueMouthActivator;
        public static GameObject eleggDefaultDialogueSpriteFocus;

        public static GameObject frimaDefaultDialogue;
        public static GameObject frimaDefaultDialogueScene1;
        public static GameObject frimaDefaultDialogueScene2;
        public static GameObject frimaDefaultDialogueScene3;
        public static GameObject frimaDefaultDialogueScene4;
        public static GameObject frimaDefaultDialogueScene5;
        public static GameObject frimaDefaultDialogueDialogueActivator;
        public static GameObject frimaDefaultDialogueDialogueFinisher;
        public static GameObject frimaDefaultDialogueMouthActivator;
        public static GameObject frimaDefaultDialogueSpriteFocus;

        public static GameObject guiltyDefaultDialogue;
        public static GameObject guiltyDefaultDialogueScene1;
        public static GameObject guiltyDefaultDialogueScene2;
        public static GameObject guiltyDefaultDialogueScene3;
        public static GameObject guiltyDefaultDialogueScene4;
        public static GameObject guiltyDefaultDialogueScene5;
        public static GameObject guiltyDefaultDialogueDialogueActivator;
        public static GameObject guiltyDefaultDialogueDialogueFinisher;
        public static GameObject guiltyDefaultDialogueMouthActivator;
        public static GameObject guiltyDefaultDialogueSpriteFocus;

        public static GameObject helmDefaultDialogue;
        public static GameObject helmDefaultDialogueScene1;
        public static GameObject helmDefaultDialogueScene2;
        public static GameObject helmDefaultDialogueScene3;
        public static GameObject helmDefaultDialogueScene4;
        public static GameObject helmDefaultDialogueScene5;
        public static GameObject helmDefaultDialogueDialogueActivator;
        public static GameObject helmDefaultDialogueDialogueFinisher;
        public static GameObject helmDefaultDialogueMouthActivator;
        public static GameObject helmDefaultDialogueSpriteFocus;

        public static GameObject maidenDefaultDialogue;
        public static GameObject maidenDefaultDialogueScene1;
        public static GameObject maidenDefaultDialogueScene2;
        public static GameObject maidenDefaultDialogueScene3;
        public static GameObject maidenDefaultDialogueScene4;
        public static GameObject maidenDefaultDialogueScene5;
        public static GameObject maidenDefaultDialogueDialogueActivator;
        public static GameObject maidenDefaultDialogueDialogueFinisher;
        public static GameObject maidenDefaultDialogueMouthActivator;
        public static GameObject maidenDefaultDialogueSpriteFocus;

        public static GameObject maryDefaultDialogue;
        public static GameObject maryDefaultDialogueScene1;
        public static GameObject maryDefaultDialogueScene2;
        public static GameObject maryDefaultDialogueScene3;
        public static GameObject maryDefaultDialogueScene4;
        public static GameObject maryDefaultDialogueScene5;
        public static GameObject maryDefaultDialogueDialogueActivator;
        public static GameObject maryDefaultDialogueDialogueFinisher;
        public static GameObject maryDefaultDialogueMouthActivator;
        public static GameObject maryDefaultDialogueSpriteFocus;

        public static GameObject mastDefaultDialogue;
        public static GameObject mastDefaultDialogueScene1;
        public static GameObject mastDefaultDialogueScene2;
        public static GameObject mastDefaultDialogueScene3;
        public static GameObject mastDefaultDialogueScene4;
        public static GameObject mastDefaultDialogueScene5;
        public static GameObject mastDefaultDialogueDialogueActivator;
        public static GameObject mastDefaultDialogueDialogueFinisher;
        public static GameObject mastDefaultDialogueMouthActivator;
        public static GameObject mastDefaultDialogueSpriteFocus;

        public static GameObject neonDefaultDialogue;
        public static GameObject neonDefaultDialogueScene1;
        public static GameObject neonDefaultDialogueScene2;
        public static GameObject neonDefaultDialogueScene3;
        public static GameObject neonDefaultDialogueScene4;
        public static GameObject neonDefaultDialogueScene5;
        public static GameObject neonDefaultDialogueDialogueActivator;
        public static GameObject neonDefaultDialogueDialogueFinisher;
        public static GameObject neonDefaultDialogueMouthActivator;
        public static GameObject neonDefaultDialogueSpriteFocus;

        public static GameObject pepperDefaultDialogue;
        public static GameObject pepperDefaultDialogueScene1;
        public static GameObject pepperDefaultDialogueScene2;
        public static GameObject pepperDefaultDialogueScene3;
        public static GameObject pepperDefaultDialogueScene4;
        public static GameObject pepperDefaultDialogueScene5;
        public static GameObject pepperDefaultDialogueDialogueActivator;
        public static GameObject pepperDefaultDialogueDialogueFinisher;
        public static GameObject pepperDefaultDialogueMouthActivator;
        public static GameObject pepperDefaultDialogueSpriteFocus;

        public static GameObject rapiDefaultDialogue;
        public static GameObject rapiDefaultDialogueScene1;
        public static GameObject rapiDefaultDialogueScene2;
        public static GameObject rapiDefaultDialogueScene3;
        public static GameObject rapiDefaultDialogueScene4;
        public static GameObject rapiDefaultDialogueScene5;
        public static GameObject rapiDefaultDialogueDialogueActivator;
        public static GameObject rapiDefaultDialogueDialogueFinisher;
        public static GameObject rapiDefaultDialogueMouthActivator;
        public static GameObject rapiDefaultDialogueSpriteFocus;

        public static GameObject rosannaDefaultDialogue;
        public static GameObject rosannaDefaultDialogueScene1;
        public static GameObject rosannaDefaultDialogueScene2;
        public static GameObject rosannaDefaultDialogueScene3;
        public static GameObject rosannaDefaultDialogueScene4;
        public static GameObject rosannaDefaultDialogueScene5;
        public static GameObject rosannaDefaultDialogueDialogueActivator;
        public static GameObject rosannaDefaultDialogueDialogueFinisher;
        public static GameObject rosannaDefaultDialogueMouthActivator;
        public static GameObject rosannaDefaultDialogueSpriteFocus;

        public static GameObject sakuraDefaultDialogue;
        public static GameObject sakuraDefaultDialogueScene1;
        public static GameObject sakuraDefaultDialogueScene2;
        public static GameObject sakuraDefaultDialogueScene3;
        public static GameObject sakuraDefaultDialogueScene4;
        public static GameObject sakuraDefaultDialogueScene5;
        public static GameObject sakuraDefaultDialogueDialogueActivator;
        public static GameObject sakuraDefaultDialogueDialogueFinisher;
        public static GameObject sakuraDefaultDialogueMouthActivator;
        public static GameObject sakuraDefaultDialogueSpriteFocus;

        public static GameObject viperDefaultDialogue;
        public static GameObject viperDefaultDialogueScene1;
        public static GameObject viperDefaultDialogueScene2;
        public static GameObject viperDefaultDialogueScene3;
        public static GameObject viperDefaultDialogueScene4;
        public static GameObject viperDefaultDialogueScene5;
        public static GameObject viperDefaultDialogueDialogueActivator;
        public static GameObject viperDefaultDialogueDialogueFinisher;
        public static GameObject viperDefaultDialogueMouthActivator;
        public static GameObject viperDefaultDialogueSpriteFocus;

        public static GameObject yanDefaultDialogue;
        public static GameObject yanDefaultDialogueScene1;
        public static GameObject yanDefaultDialogueScene2;
        public static GameObject yanDefaultDialogueScene3;
        public static GameObject yanDefaultDialogueScene4;
        public static GameObject yanDefaultDialogueScene5;
        public static GameObject yanDefaultDialogueDialogueActivator;
        public static GameObject yanDefaultDialogueDialogueFinisher;
        public static GameObject yanDefaultDialogueMouthActivator;
        public static GameObject yanDefaultDialogueSpriteFocus;
        #endregion
        #region Event Variables
        public static GameObject amberHospitalhallwayEvent01Dialogue;
        public static GameObject amberHospitalhallwayEvent01DialogueScene1;
        public static GameObject amberHospitalhallwayEvent01DialogueScene2;
        public static GameObject amberHospitalhallwayEvent01DialogueScene3;
        public static GameObject amberHospitalhallwayEvent01DialogueScene4;
        public static GameObject amberHospitalhallwayEvent01DialogueScene5;
        public static GameObject amberHospitalhallwayEvent01DialogueDialogueActivator;
        public static GameObject amberHospitalhallwayEvent01DialogueDialogueFinisher;
        public static GameObject amberHospitalhallwayEvent01DialogueMouthActivator;
        public static GameObject amberHospitalhallwayEvent01DialogueSpriteFocus;

        public static GameObject anisMallEvent01Dialogue;
        public static GameObject anisMallEvent01DialogueScene1;
        public static GameObject anisMallEvent01DialogueScene2;
        public static GameObject anisMallEvent01DialogueScene3;
        public static GameObject anisMallEvent01DialogueScene4;
        public static GameObject anisMallEvent01DialogueScene5;
        public static GameObject anisMallEvent01DialogueDialogueActivator;
        public static GameObject anisMallEvent01DialogueDialogueFinisher;
        public static GameObject anisMallEvent01DialogueMouthActivator;
        public static GameObject anisMallEvent01DialogueSpriteFocus;

        public static GameObject centiKenshomeEvent01Dialogue;
        public static GameObject centiKenshomeEvent01DialogueScene1;
        public static GameObject centiKenshomeEvent01DialogueScene2;
        public static GameObject centiKenshomeEvent01DialogueScene3;
        public static GameObject centiKenshomeEvent01DialogueScene4;
        public static GameObject centiKenshomeEvent01DialogueScene5;
        public static GameObject centiKenshomeEvent01DialogueDialogueActivator;
        public static GameObject centiKenshomeEvent01DialogueDialogueFinisher;
        public static GameObject centiKenshomeEvent01DialogueMouthActivator;
        public static GameObject centiKenshomeEvent01DialogueSpriteFocus;

        public static GameObject dorothyParkEvent01Dialogue;
        public static GameObject dorothyParkEvent01DialogueScene1;
        public static GameObject dorothyParkEvent01DialogueScene2;
        public static GameObject dorothyParkEvent01DialogueScene3;
        public static GameObject dorothyParkEvent01DialogueScene4;
        public static GameObject dorothyParkEvent01DialogueScene5;
        public static GameObject dorothyParkEvent01DialogueDialogueActivator;
        public static GameObject dorothyParkEvent01DialogueDialogueFinisher;
        public static GameObject dorothyParkEvent01DialogueMouthActivator;
        public static GameObject dorothyParkEvent01DialogueSpriteFocus;

        public static GameObject eleggDowntownEvent01Dialogue;
        public static GameObject eleggDowntownEvent01DialogueScene1;
        public static GameObject eleggDowntownEvent01DialogueScene2;
        public static GameObject eleggDowntownEvent01DialogueScene3;
        public static GameObject eleggDowntownEvent01DialogueScene4;
        public static GameObject eleggDowntownEvent01DialogueScene5;
        public static GameObject eleggDowntownEvent01DialogueDialogueActivator;
        public static GameObject eleggDowntownEvent01DialogueDialogueFinisher;
        public static GameObject eleggDowntownEvent01DialogueMouthActivator;
        public static GameObject eleggDowntownEvent01DialogueSpriteFocus;

        public static GameObject frimaHotelEvent01Dialogue;
        public static GameObject frimaHotelEvent01DialogueScene1;
        public static GameObject frimaHotelEvent01DialogueScene2;
        public static GameObject frimaHotelEvent01DialogueScene3;
        public static GameObject frimaHotelEvent01DialogueScene4;
        public static GameObject frimaHotelEvent01DialogueScene5;
        public static GameObject frimaHotelEvent01DialogueDialogueActivator;
        public static GameObject frimaHotelEvent01DialogueDialogueFinisher;
        public static GameObject frimaHotelEvent01DialogueMouthActivator;
        public static GameObject frimaHotelEvent01DialogueSpriteFocus;

        public static GameObject guiltyParkinglotEvent01Dialogue;
        public static GameObject guiltyParkinglotEvent01DialogueScene1;
        public static GameObject guiltyParkinglotEvent01DialogueScene2;
        public static GameObject guiltyParkinglotEvent01DialogueScene3;
        public static GameObject guiltyParkinglotEvent01DialogueScene4;
        public static GameObject guiltyParkinglotEvent01DialogueScene5;
        public static GameObject guiltyParkinglotEvent01DialogueDialogueActivator;
        public static GameObject guiltyParkinglotEvent01DialogueDialogueFinisher;
        public static GameObject guiltyParkinglotEvent01DialogueMouthActivator;
        public static GameObject guiltyParkinglotEvent01DialogueSpriteFocus;

        public static GameObject helmBeachEvent01Dialogue;
        public static GameObject helmBeachEvent01DialogueScene1;
        public static GameObject helmBeachEvent01DialogueScene2;
        public static GameObject helmBeachEvent01DialogueScene3;
        public static GameObject helmBeachEvent01DialogueScene4;
        public static GameObject helmBeachEvent01DialogueScene5;
        public static GameObject helmBeachEvent01DialogueDialogueActivator;
        public static GameObject helmBeachEvent01DialogueDialogueFinisher;
        public static GameObject helmBeachEvent01DialogueMouthActivator;
        public static GameObject helmBeachEvent01DialogueSpriteFocus;

        public static GameObject maidenAlleyEvent01Dialogue;
        public static GameObject maidenAlleyEvent01DialogueScene1;
        public static GameObject maidenAlleyEvent01DialogueScene2;
        public static GameObject maidenAlleyEvent01DialogueScene3;
        public static GameObject maidenAlleyEvent01DialogueScene4;
        public static GameObject maidenAlleyEvent01DialogueScene5;
        public static GameObject maidenAlleyEvent01DialogueDialogueActivator;
        public static GameObject maidenAlleyEvent01DialogueDialogueFinisher;
        public static GameObject maidenAlleyEvent01DialogueMouthActivator;
        public static GameObject maidenAlleyEvent01DialogueSpriteFocus;

        public static GameObject maryHospitalhallwayEvent01Dialogue;
        public static GameObject maryHospitalhallwayEvent01DialogueScene1;
        public static GameObject maryHospitalhallwayEvent01DialogueScene2;
        public static GameObject maryHospitalhallwayEvent01DialogueScene3;
        public static GameObject maryHospitalhallwayEvent01DialogueScene4;
        public static GameObject maryHospitalhallwayEvent01DialogueScene5;
        public static GameObject maryHospitalhallwayEvent01DialogueDialogueActivator;
        public static GameObject maryHospitalhallwayEvent01DialogueDialogueFinisher;
        public static GameObject maryHospitalhallwayEvent01DialogueMouthActivator;
        public static GameObject maryHospitalhallwayEvent01DialogueSpriteFocus;

        public static GameObject mastBeachEvent01Dialogue;
        public static GameObject mastBeachEvent01DialogueScene1;
        public static GameObject mastBeachEvent01DialogueScene2;
        public static GameObject mastBeachEvent01DialogueScene3;
        public static GameObject mastBeachEvent01DialogueScene4;
        public static GameObject mastBeachEvent01DialogueScene5;
        public static GameObject mastBeachEvent01DialogueDialogueActivator;
        public static GameObject mastBeachEvent01DialogueDialogueFinisher;
        public static GameObject mastBeachEvent01DialogueMouthActivator;
        public static GameObject mastBeachEvent01DialogueSpriteFocus;

        public static GameObject neonTempleEvent01Dialogue;
        public static GameObject neonTempleEvent01DialogueScene1;
        public static GameObject neonTempleEvent01DialogueScene2;
        public static GameObject neonTempleEvent01DialogueScene3;
        public static GameObject neonTempleEvent01DialogueScene4;
        public static GameObject neonTempleEvent01DialogueScene5;
        public static GameObject neonTempleEvent01DialogueDialogueActivator;
        public static GameObject neonTempleEvent01DialogueDialogueFinisher;
        public static GameObject neonTempleEvent01DialogueMouthActivator;
        public static GameObject neonTempleEvent01DialogueSpriteFocus;

        public static GameObject pepperHospitalEvent01Dialogue;
        public static GameObject pepperHospitalEvent01DialogueScene1;
        public static GameObject pepperHospitalEvent01DialogueScene2;
        public static GameObject pepperHospitalEvent01DialogueScene3;
        public static GameObject pepperHospitalEvent01DialogueScene4;
        public static GameObject pepperHospitalEvent01DialogueScene5;
        public static GameObject pepperHospitalEvent01DialogueDialogueActivator;
        public static GameObject pepperHospitalEvent01DialogueDialogueFinisher;
        public static GameObject pepperHospitalEvent01DialogueMouthActivator;
        public static GameObject pepperHospitalEvent01DialogueSpriteFocus;

        public static GameObject rapiGasstationEvent01Dialogue;
        public static GameObject rapiGasstationEvent01DialogueScene1;
        public static GameObject rapiGasstationEvent01DialogueScene2;
        public static GameObject rapiGasstationEvent01DialogueScene3;
        public static GameObject rapiGasstationEvent01DialogueScene4;
        public static GameObject rapiGasstationEvent01DialogueScene5;
        public static GameObject rapiGasstationEvent01DialogueDialogueActivator;
        public static GameObject rapiGasstationEvent01DialogueDialogueFinisher;
        public static GameObject rapiGasstationEvent01DialogueMouthActivator;
        public static GameObject rapiGasstationEvent01DialogueSpriteFocus;

        public static GameObject rosannaGabrielsmansionEvent01Dialogue;
        public static GameObject rosannaGabrielsmansionEvent01DialogueScene1;
        public static GameObject rosannaGabrielsmansionEvent01DialogueScene2;
        public static GameObject rosannaGabrielsmansionEvent01DialogueScene3;
        public static GameObject rosannaGabrielsmansionEvent01DialogueScene4;
        public static GameObject rosannaGabrielsmansionEvent01DialogueScene5;
        public static GameObject rosannaGabrielsmansionEvent01DialogueDialogueActivator;
        public static GameObject rosannaGabrielsmansionEvent01DialogueDialogueFinisher;
        public static GameObject rosannaGabrielsmansionEvent01DialogueMouthActivator;
        public static GameObject rosannaGabrielsmansionEvent01DialogueSpriteFocus;

        public static GameObject sakuraForestEvent01Dialogue;
        public static GameObject sakuraForestEvent01DialogueScene1;
        public static GameObject sakuraForestEvent01DialogueScene2;
        public static GameObject sakuraForestEvent01DialogueScene3;
        public static GameObject sakuraForestEvent01DialogueScene4;
        public static GameObject sakuraForestEvent01DialogueScene5;
        public static GameObject sakuraForestEvent01DialogueDialogueActivator;
        public static GameObject sakuraForestEvent01DialogueDialogueFinisher;
        public static GameObject sakuraForestEvent01DialogueMouthActivator;
        public static GameObject sakuraForestEvent01DialogueSpriteFocus;

        public static GameObject viperVillaEvent01Dialogue;
        public static GameObject viperVillaEvent01DialogueScene1;
        public static GameObject viperVillaEvent01DialogueScene2;
        public static GameObject viperVillaEvent01DialogueScene3;
        public static GameObject viperVillaEvent01DialogueScene4;
        public static GameObject viperVillaEvent01DialogueScene5;
        public static GameObject viperVillaEvent01DialogueDialogueActivator;
        public static GameObject viperVillaEvent01DialogueDialogueFinisher;
        public static GameObject viperVillaEvent01DialogueMouthActivator;
        public static GameObject viperVillaEvent01DialogueSpriteFocus;

        public static GameObject yanMallEvent01Dialogue;
        public static GameObject yanMallEvent01DialogueScene1;
        public static GameObject yanMallEvent01DialogueScene2;
        public static GameObject yanMallEvent01DialogueScene3;
        public static GameObject yanMallEvent01DialogueScene4;
        public static GameObject yanMallEvent01DialogueScene5;
        public static GameObject yanMallEvent01DialogueDialogueActivator;
        public static GameObject yanMallEvent01DialogueDialogueFinisher;
        public static GameObject yanMallEvent01DialogueMouthActivator;
        public static GameObject yanMallEvent01DialogueSpriteFocus;
        #endregion
        #region Voyeur Variables
        public static GameObject anisSecretbeachVoyeur01Dialogue;
        public static GameObject anisSecretbeachVoyeur01DialogueScene1;
        public static GameObject anisSecretbeachVoyeur01DialogueScene2;
        public static GameObject anisSecretbeachVoyeur01DialogueScene3;
        public static GameObject anisSecretbeachVoyeur01DialogueScene4;
        public static GameObject anisSecretbeachVoyeur01DialogueScene5;
        public static GameObject anisSecretbeachVoyeur01DialogueActivator;
        public static GameObject anisSecretbeachVoyeur01DialogueFinisher;
        public static GameObject anisSecretbeachVoyeur01DialogueMouthActivator;
        public static GameObject anisSecretbeachVoyeur01DialogueSpriteFocus;

        public static GameObject centiSecretbeachVoyeur01Dialogue;
        public static GameObject centiSecretbeachVoyeur01DialogueScene1;
        public static GameObject centiSecretbeachVoyeur01DialogueScene2;
        public static GameObject centiSecretbeachVoyeur01DialogueScene3;
        public static GameObject centiSecretbeachVoyeur01DialogueScene4;
        public static GameObject centiSecretbeachVoyeur01DialogueScene5;
        public static GameObject centiSecretbeachVoyeur01DialogueActivator;
        public static GameObject centiSecretbeachVoyeur01DialogueFinisher;
        public static GameObject centiSecretbeachVoyeur01DialogueMouthActivator;
        public static GameObject centiSecretbeachVoyeur01DialogueSpriteFocus;

        public static GameObject dorothySecretbeachVoyeur01Dialogue;
        public static GameObject dorothySecretbeachVoyeur01DialogueScene1;
        public static GameObject dorothySecretbeachVoyeur01DialogueScene2;
        public static GameObject dorothySecretbeachVoyeur01DialogueScene3;
        public static GameObject dorothySecretbeachVoyeur01DialogueScene4;
        public static GameObject dorothySecretbeachVoyeur01DialogueScene5;
        public static GameObject dorothySecretbeachVoyeur01DialogueActivator;
        public static GameObject dorothySecretbeachVoyeur01DialogueFinisher;
        public static GameObject dorothySecretbeachVoyeur01DialogueMouthActivator;
        public static GameObject dorothySecretbeachVoyeur01DialogueSpriteFocus;

        public static GameObject eleggSecretbeachVoyeur01Dialogue;
        public static GameObject eleggSecretbeachVoyeur01DialogueScene1;
        public static GameObject eleggSecretbeachVoyeur01DialogueScene2;
        public static GameObject eleggSecretbeachVoyeur01DialogueScene3;
        public static GameObject eleggSecretbeachVoyeur01DialogueScene4;
        public static GameObject eleggSecretbeachVoyeur01DialogueScene5;
        public static GameObject eleggSecretbeachVoyeur01DialogueActivator;
        public static GameObject eleggSecretbeachVoyeur01DialogueFinisher;
        public static GameObject eleggSecretbeachVoyeur01DialogueMouthActivator;
        public static GameObject eleggSecretbeachVoyeur01DialogueSpriteFocus;

        public static GameObject frimaSecretbeachVoyeur01Dialogue;
        public static GameObject frimaSecretbeachVoyeur01DialogueScene1;
        public static GameObject frimaSecretbeachVoyeur01DialogueScene2;
        public static GameObject frimaSecretbeachVoyeur01DialogueScene3;
        public static GameObject frimaSecretbeachVoyeur01DialogueScene4;
        public static GameObject frimaSecretbeachVoyeur01DialogueScene5;
        public static GameObject frimaSecretbeachVoyeur01DialogueActivator;
        public static GameObject frimaSecretbeachVoyeur01DialogueFinisher;
        public static GameObject frimaSecretbeachVoyeur01DialogueMouthActivator;
        public static GameObject frimaSecretbeachVoyeur01DialogueSpriteFocus;

        public static GameObject guiltySecretbeachVoyeur01Dialogue;
        public static GameObject guiltySecretbeachVoyeur01DialogueScene1;
        public static GameObject guiltySecretbeachVoyeur01DialogueScene2;
        public static GameObject guiltySecretbeachVoyeur01DialogueScene3;
        public static GameObject guiltySecretbeachVoyeur01DialogueScene4;
        public static GameObject guiltySecretbeachVoyeur01DialogueScene5;
        public static GameObject guiltySecretbeachVoyeur01DialogueActivator;
        public static GameObject guiltySecretbeachVoyeur01DialogueFinisher;
        public static GameObject guiltySecretbeachVoyeur01DialogueMouthActivator;
        public static GameObject guiltySecretbeachVoyeur01DialogueSpriteFocus;

        public static GameObject helmSecretbeachVoyeur01Dialogue;
        public static GameObject helmSecretbeachVoyeur01DialogueScene1;
        public static GameObject helmSecretbeachVoyeur01DialogueScene2;
        public static GameObject helmSecretbeachVoyeur01DialogueScene3;
        public static GameObject helmSecretbeachVoyeur01DialogueScene4;
        public static GameObject helmSecretbeachVoyeur01DialogueScene5;
        public static GameObject helmSecretbeachVoyeur01DialogueActivator;
        public static GameObject helmSecretbeachVoyeur01DialogueFinisher;
        public static GameObject helmSecretbeachVoyeur01DialogueMouthActivator;
        public static GameObject helmSecretbeachVoyeur01DialogueSpriteFocus;

        public static GameObject maidenSecretbeachVoyeur01Dialogue;
        public static GameObject maidenSecretbeachVoyeur01DialogueScene1;
        public static GameObject maidenSecretbeachVoyeur01DialogueScene2;
        public static GameObject maidenSecretbeachVoyeur01DialogueScene3;
        public static GameObject maidenSecretbeachVoyeur01DialogueScene4;
        public static GameObject maidenSecretbeachVoyeur01DialogueScene5;
        public static GameObject maidenSecretbeachVoyeur01DialogueActivator;
        public static GameObject maidenSecretbeachVoyeur01DialogueFinisher;
        public static GameObject maidenSecretbeachVoyeur01DialogueMouthActivator;
        public static GameObject maidenSecretbeachVoyeur01DialogueSpriteFocus;

        public static GameObject marySecretbeachVoyeur01Dialogue;
        public static GameObject marySecretbeachVoyeur01DialogueScene1;
        public static GameObject marySecretbeachVoyeur01DialogueScene2;
        public static GameObject marySecretbeachVoyeur01DialogueScene3;
        public static GameObject marySecretbeachVoyeur01DialogueScene4;
        public static GameObject marySecretbeachVoyeur01DialogueScene5;
        public static GameObject marySecretbeachVoyeur01DialogueActivator;
        public static GameObject marySecretbeachVoyeur01DialogueFinisher;
        public static GameObject marySecretbeachVoyeur01DialogueMouthActivator;
        public static GameObject marySecretbeachVoyeur01DialogueSpriteFocus;

        public static GameObject mastSecretbeachVoyeur01Dialogue;
        public static GameObject mastSecretbeachVoyeur01DialogueScene1;
        public static GameObject mastSecretbeachVoyeur01DialogueScene2;
        public static GameObject mastSecretbeachVoyeur01DialogueScene3;
        public static GameObject mastSecretbeachVoyeur01DialogueScene4;
        public static GameObject mastSecretbeachVoyeur01DialogueScene5;
        public static GameObject mastSecretbeachVoyeur01DialogueActivator;
        public static GameObject mastSecretbeachVoyeur01DialogueFinisher;
        public static GameObject mastSecretbeachVoyeur01DialogueMouthActivator;
        public static GameObject mastSecretbeachVoyeur01DialogueSpriteFocus;

        public static GameObject neonSecretbeachVoyeur01Dialogue;
        public static GameObject neonSecretbeachVoyeur01DialogueScene1;
        public static GameObject neonSecretbeachVoyeur01DialogueScene2;
        public static GameObject neonSecretbeachVoyeur01DialogueScene3;
        public static GameObject neonSecretbeachVoyeur01DialogueScene4;
        public static GameObject neonSecretbeachVoyeur01DialogueScene5;
        public static GameObject neonSecretbeachVoyeur01DialogueActivator;
        public static GameObject neonSecretbeachVoyeur01DialogueFinisher;
        public static GameObject neonSecretbeachVoyeur01DialogueMouthActivator;
        public static GameObject neonSecretbeachVoyeur01DialogueSpriteFocus;

        public static GameObject pepperSecretbeachVoyeur01Dialogue;
        public static GameObject pepperSecretbeachVoyeur01DialogueScene1;
        public static GameObject pepperSecretbeachVoyeur01DialogueScene2;
        public static GameObject pepperSecretbeachVoyeur01DialogueScene3;
        public static GameObject pepperSecretbeachVoyeur01DialogueScene4;
        public static GameObject pepperSecretbeachVoyeur01DialogueScene5;
        public static GameObject pepperSecretbeachVoyeur01DialogueActivator;
        public static GameObject pepperSecretbeachVoyeur01DialogueFinisher;
        public static GameObject pepperSecretbeachVoyeur01DialogueMouthActivator;
        public static GameObject pepperSecretbeachVoyeur01DialogueSpriteFocus;

        public static GameObject rapiSecretbeachVoyeur01Dialogue;
        public static GameObject rapiSecretbeachVoyeur01DialogueScene1;
        public static GameObject rapiSecretbeachVoyeur01DialogueScene2;
        public static GameObject rapiSecretbeachVoyeur01DialogueScene3;
        public static GameObject rapiSecretbeachVoyeur01DialogueScene4;
        public static GameObject rapiSecretbeachVoyeur01DialogueScene5;
        public static GameObject rapiSecretbeachVoyeur01DialogueActivator;
        public static GameObject rapiSecretbeachVoyeur01DialogueFinisher;
        public static GameObject rapiSecretbeachVoyeur01DialogueMouthActivator;
        public static GameObject rapiSecretbeachVoyeur01DialogueSpriteFocus;

        public static GameObject rosannaSecretbeachVoyeur01Dialogue;
        public static GameObject rosannaSecretbeachVoyeur01DialogueScene1;
        public static GameObject rosannaSecretbeachVoyeur01DialogueScene2;
        public static GameObject rosannaSecretbeachVoyeur01DialogueScene3;
        public static GameObject rosannaSecretbeachVoyeur01DialogueScene4;
        public static GameObject rosannaSecretbeachVoyeur01DialogueScene5;
        public static GameObject rosannaSecretbeachVoyeur01DialogueActivator;
        public static GameObject rosannaSecretbeachVoyeur01DialogueFinisher;
        public static GameObject rosannaSecretbeachVoyeur01DialogueMouthActivator;
        public static GameObject rosannaSecretbeachVoyeur01DialogueSpriteFocus;

        public static GameObject sakuraSecretbeachVoyeur01Dialogue;
        public static GameObject sakuraSecretbeachVoyeur01DialogueScene1;
        public static GameObject sakuraSecretbeachVoyeur01DialogueScene2;
        public static GameObject sakuraSecretbeachVoyeur01DialogueScene3;
        public static GameObject sakuraSecretbeachVoyeur01DialogueScene4;
        public static GameObject sakuraSecretbeachVoyeur01DialogueScene5;
        public static GameObject sakuraSecretbeachVoyeur01DialogueActivator;
        public static GameObject sakuraSecretbeachVoyeur01DialogueFinisher;
        public static GameObject sakuraSecretbeachVoyeur01DialogueMouthActivator;
        public static GameObject sakuraSecretbeachVoyeur01DialogueSpriteFocus;

        public static GameObject viperSecretbeachVoyeur01Dialogue;
        public static GameObject viperSecretbeachVoyeur01DialogueScene1;
        public static GameObject viperSecretbeachVoyeur01DialogueScene2;
        public static GameObject viperSecretbeachVoyeur01DialogueScene3;
        public static GameObject viperSecretbeachVoyeur01DialogueScene4;
        public static GameObject viperSecretbeachVoyeur01DialogueScene5;
        public static GameObject viperSecretbeachVoyeur01DialogueActivator;
        public static GameObject viperSecretbeachVoyeur01DialogueFinisher;
        public static GameObject viperSecretbeachVoyeur01DialogueMouthActivator;
        public static GameObject viperSecretbeachVoyeur01DialogueSpriteFocus;

        public static GameObject yanSecretbeachVoyeur01Dialogue;
        public static GameObject yanSecretbeachVoyeur01DialogueScene1;
        public static GameObject yanSecretbeachVoyeur01DialogueScene2;
        public static GameObject yanSecretbeachVoyeur01DialogueScene3;
        public static GameObject yanSecretbeachVoyeur01DialogueScene4;
        public static GameObject yanSecretbeachVoyeur01DialogueScene5;
        public static GameObject yanSecretbeachVoyeur01DialogueActivator;
        public static GameObject yanSecretbeachVoyeur01DialogueFinisher;
        public static GameObject yanSecretbeachVoyeur01DialogueMouthActivator;
        public static GameObject yanSecretbeachVoyeur01DialogueSpriteFocus;
        #endregion

        public static GameObject backupHaus;

        public static GameObject vanillaDiagNoOneThere;
        public static GameObject vanillaDiagRiverRepeat;

        public static GameObject snekForestEgg01Dialogue;
        public static GameObject snekForestEgg01DialogueScene1;
        public static GameObject snekForestEgg01DialogueActivator;
        public static GameObject snekForestEgg01DialogueFinisher;

        public static bool loadedDialogues = false;
        public static bool dialoguePlaying = false;
        public static bool dialoguePlayingVanilla = false;

        private static bool dialoguePlayingVanillaInvokeRepeatingRunning = false;

        private void MonitorRoomTalkChildren()
        {
            if (Core.roomTalk == null)
            {
                Debug.LogWarning("Core.roomTalk is null. Cannot monitor children.");
                return;
            }

            bool anyChildActive = false;
            foreach (Transform child in Core.roomTalk)
            {
                if (child.name != "Always_Active" && child.gameObject.activeSelf)
                {
                    anyChildActive = true;
                    break;
                }
            }

            //Debug.Log("Monitoring.");
            dialoguePlayingVanilla = anyChildActive;
            //Debug.Log("dialoguePlayingVanilla = " + dialoguePlayingVanilla);
        }

        
        public void Update()
        {
            if (Core.currentScene.name == "CoreGameScene")
            {
                if (!loadedDialogues && Places.loadedPlaces)
                {
                    backupHaus = GameObject.Instantiate(new GameObject());
                    backupHaus.name = "BackupHaus";

                    vanillaDiagNoOneThere = Places.roomTalkParkingLot.transform.Find("NoOneThere").gameObject;
                    vanillaDiagRiverRepeat = Places.roomTalkPark.transform.Find("RiverRepeat").gameObject;

                    overrideSpeechSkinBlue = GetActorOverrideSpeechSkinValue(Core.roomTalk.Find("Bath").Find("AnnaAndAdrianFirsttime").gameObject, "Adrian");
                    overrideSpeechSkinGreen = GetActorOverrideSpeechSkinValue(Core.roomTalk.Find("Bath").Find("NoOneInShower").gameObject, "You");
                    overrideSpeechSkinPink = GetActorOverrideSpeechSkinValue(Core.roomTalk.Find("Beach").Find("AmeliaBeach").gameObject, "Amelia");
                    overrideSpeechSkinYellow = GetActorOverrideSpeechSkinValue(Core.roomTalk.Find("Bath").Find("AnnaInShower").gameObject, "Anna");

                    badWeatherDialogue = CreateNewDialogue("BadWeather", Places.secretBeachRoomtalk.transform);
                    badWeatherDialogueActivator = badWeatherDialogue.transform.Find("DialogueActivator").gameObject;
                    badWeatherDialogueFinisher = badWeatherDialogue.transform.Find("DialogueFinisher").gameObject;

                    #region Secret Beach Initialization
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
                    #endregion
                    #region Mountain Lab Initialization
                    mLDialogueMainFirst = CreateNewDialogue("MLDialogueStory02", Places.mountainLabRoomtalk.transform);
                    mLDialogueMainFirstScene1 = mLDialogueMainFirst.transform.Find("Scene1").gameObject;
                    mLDialogueMainFirstScene2 = mLDialogueMainFirst.transform.Find("Scene2").gameObject;
                    mLDialogueMainFirstScene3 = mLDialogueMainFirst.transform.Find("Scene3").gameObject;
                    mLDialogueMainFirstScene4 = mLDialogueMainFirst.transform.Find("Scene4").gameObject;
                    mLDialogueMainFirstScene5 = mLDialogueMainFirst.transform.Find("Scene5").gameObject;
                    mLDialogueMainFirstDialogueActivator = mLDialogueMainFirst.transform.Find("DialogueActivator").gameObject;
                    mLDialogueMainFirstDialogueFinisher = mLDialogueMainFirst.transform.Find("DialogueFinisher").gameObject;
                    mLDialogueMainFirstMouthActivator = mLDialogueMainFirst.transform.Find("MouthActivator").gameObject;
                    mLDialogueMainFirstSpriteFocus = mLDialogueMainFirst.transform.Find("SpriteFocus").gameObject;

                    mLDialogueMainStory03 = CreateNewDialogue("MLDialogueStory03", Places.mountainLabRoomtalk.transform);
                    mLDialogueMainStory03Scene1 = mLDialogueMainStory03.transform.Find("Scene1").gameObject;
                    mLDialogueMainStory03Scene2 = mLDialogueMainStory03.transform.Find("Scene2").gameObject;
                    mLDialogueMainStory03Scene3 = mLDialogueMainStory03.transform.Find("Scene3").gameObject;
                    mLDialogueMainStory03Scene4 = mLDialogueMainStory03.transform.Find("Scene4").gameObject;
                    mLDialogueMainStory03Scene5 = mLDialogueMainStory03.transform.Find("Scene5").gameObject;
                    mLDialogueMainStory03DialogueActivator = mLDialogueMainStory03.transform.Find("DialogueActivator").gameObject;
                    mLDialogueMainStory03DialogueFinisher = mLDialogueMainStory03.transform.Find("DialogueFinisher").gameObject;
                    mLDialogueMainStory03MouthActivator = mLDialogueMainStory03.transform.Find("MouthActivator").gameObject;
                    mLDialogueMainStory03SpriteFocus = mLDialogueMainStory03.transform.Find("SpriteFocus").gameObject;

                    mLDialogueMainStory04 = CreateNewDialogue("MLDialogueStory04", Places.mountainLabRoomtalk.transform);
                    mLDialogueMainStory04Scene1 = mLDialogueMainStory04.transform.Find("Scene1").gameObject;
                    mLDialogueMainStory04Scene2 = mLDialogueMainStory04.transform.Find("Scene2").gameObject;
                    mLDialogueMainStory04Scene3 = mLDialogueMainStory04.transform.Find("Scene3").gameObject;
                    mLDialogueMainStory04Scene4 = mLDialogueMainStory04.transform.Find("Scene4").gameObject;
                    mLDialogueMainStory04Scene5 = mLDialogueMainStory04.transform.Find("Scene5").gameObject;
                    mLDialogueMainStory04DialogueActivator = mLDialogueMainStory04.transform.Find("DialogueActivator").gameObject;
                    mLDialogueMainStory04DialogueFinisher = mLDialogueMainStory04.transform.Find("DialogueFinisher").gameObject;
                    mLDialogueMainStory04MouthActivator = mLDialogueMainStory04.transform.Find("MouthActivator").gameObject;
                    mLDialogueMainStory04SpriteFocus = mLDialogueMainStory04.transform.Find("SpriteFocus").gameObject;
                    #endregion
                    #region Character Initialization

                    amberDefaultDialogue = CreateNewDialogue("AmberDialogueDefault", Places.mountainLabRoomtalk.transform);
                    amberDefaultDialogueScene1 = amberDefaultDialogue.transform.Find("Scene1").gameObject;
                    amberDefaultDialogueScene2 = amberDefaultDialogue.transform.Find("Scene2").gameObject;
                    amberDefaultDialogueScene3 = amberDefaultDialogue.transform.Find("Scene3").gameObject;
                    amberDefaultDialogueScene4 = amberDefaultDialogue.transform.Find("Scene4").gameObject;
                    amberDefaultDialogueScene5 = amberDefaultDialogue.transform.Find("Scene5").gameObject;
                    amberDefaultDialogueDialogueActivator = amberDefaultDialogue.transform.Find("DialogueActivator").gameObject;
                    amberDefaultDialogueDialogueFinisher = amberDefaultDialogue.transform.Find("DialogueFinisher").gameObject;
                    amberDefaultDialogueMouthActivator = amberDefaultDialogue.transform.Find("MouthActivator").gameObject;
                    amberDefaultDialogueSpriteFocus = amberDefaultDialogue.transform.Find("SpriteFocus").gameObject;

                    anisDefaultDialogue = CreateNewDialogue("AnisDialogueDefault", Places.mountainLabRoomNikkeAnisRoomtalk.transform);
                    anisDefaultDialogueScene1 = anisDefaultDialogue.transform.Find("Scene1").gameObject;
                    anisDefaultDialogueScene2 = anisDefaultDialogue.transform.Find("Scene2").gameObject;
                    anisDefaultDialogueScene3 = anisDefaultDialogue.transform.Find("Scene3").gameObject;
                    anisDefaultDialogueScene4 = anisDefaultDialogue.transform.Find("Scene4").gameObject;
                    anisDefaultDialogueScene5 = anisDefaultDialogue.transform.Find("Scene5").gameObject;
                    anisDefaultDialogueDialogueActivator = anisDefaultDialogue.transform.Find("DialogueActivator").gameObject;
                    anisDefaultDialogueDialogueFinisher = anisDefaultDialogue.transform.Find("DialogueFinisher").gameObject;
                    anisDefaultDialogueMouthActivator = anisDefaultDialogue.transform.Find("MouthActivator").gameObject;
                    anisDefaultDialogueSpriteFocus = anisDefaultDialogue.transform.Find("SpriteFocus").gameObject;

                    centiDefaultDialogue = CreateNewDialogue("CentiDialogueDefault", Places.mountainLabRoomNikkeCentiRoomtalk.transform);
                    centiDefaultDialogueScene1 = centiDefaultDialogue.transform.Find("Scene1").gameObject;
                    centiDefaultDialogueScene2 = centiDefaultDialogue.transform.Find("Scene2").gameObject;
                    centiDefaultDialogueScene3 = centiDefaultDialogue.transform.Find("Scene3").gameObject;
                    centiDefaultDialogueScene4 = centiDefaultDialogue.transform.Find("Scene4").gameObject;
                    centiDefaultDialogueScene5 = centiDefaultDialogue.transform.Find("Scene5").gameObject;
                    centiDefaultDialogueDialogueActivator = centiDefaultDialogue.transform.Find("DialogueActivator").gameObject;
                    centiDefaultDialogueDialogueFinisher = centiDefaultDialogue.transform.Find("DialogueFinisher").gameObject;
                    centiDefaultDialogueMouthActivator = centiDefaultDialogue.transform.Find("MouthActivator").gameObject;
                    centiDefaultDialogueSpriteFocus = centiDefaultDialogue.transform.Find("SpriteFocus").gameObject;

                    dorothyDefaultDialogue = CreateNewDialogue("DorothyDialogueDefault", Places.mountainLabRoomNikkeDorothyRoomtalk.transform);
                    dorothyDefaultDialogueScene1 = dorothyDefaultDialogue.transform.Find("Scene1").gameObject;
                    dorothyDefaultDialogueScene2 = dorothyDefaultDialogue.transform.Find("Scene2").gameObject;
                    dorothyDefaultDialogueScene3 = dorothyDefaultDialogue.transform.Find("Scene3").gameObject;
                    dorothyDefaultDialogueScene4 = dorothyDefaultDialogue.transform.Find("Scene4").gameObject;
                    dorothyDefaultDialogueScene5 = dorothyDefaultDialogue.transform.Find("Scene5").gameObject;
                    dorothyDefaultDialogueDialogueActivator = dorothyDefaultDialogue.transform.Find("DialogueActivator").gameObject;
                    dorothyDefaultDialogueDialogueFinisher = dorothyDefaultDialogue.transform.Find("DialogueFinisher").gameObject;
                    dorothyDefaultDialogueMouthActivator = dorothyDefaultDialogue.transform.Find("MouthActivator").gameObject;
                    dorothyDefaultDialogueSpriteFocus = dorothyDefaultDialogue.transform.Find("SpriteFocus").gameObject;

                    eleggDefaultDialogue = CreateNewDialogue("EleggDialogueDefault", Places.mountainLabRoomNikkeEleggRoomtalk.transform);
                    eleggDefaultDialogueScene1 = eleggDefaultDialogue.transform.Find("Scene1").gameObject;
                    eleggDefaultDialogueScene2 = eleggDefaultDialogue.transform.Find("Scene2").gameObject;
                    eleggDefaultDialogueScene3 = eleggDefaultDialogue.transform.Find("Scene3").gameObject;
                    eleggDefaultDialogueScene4 = eleggDefaultDialogue.transform.Find("Scene4").gameObject;
                    eleggDefaultDialogueScene5 = eleggDefaultDialogue.transform.Find("Scene5").gameObject;
                    eleggDefaultDialogueDialogueActivator = eleggDefaultDialogue.transform.Find("DialogueActivator").gameObject;
                    eleggDefaultDialogueDialogueFinisher = eleggDefaultDialogue.transform.Find("DialogueFinisher").gameObject;
                    eleggDefaultDialogueMouthActivator = eleggDefaultDialogue.transform.Find("MouthActivator").gameObject;
                    eleggDefaultDialogueSpriteFocus = eleggDefaultDialogue.transform.Find("SpriteFocus").gameObject;

                    frimaDefaultDialogue = CreateNewDialogue("FrimaDialogueDefault", Places.mountainLabRoomNikkeFrimaRoomtalk.transform);
                    frimaDefaultDialogueScene1 = frimaDefaultDialogue.transform.Find("Scene1").gameObject;
                    frimaDefaultDialogueScene2 = frimaDefaultDialogue.transform.Find("Scene2").gameObject;
                    frimaDefaultDialogueScene3 = frimaDefaultDialogue.transform.Find("Scene3").gameObject;
                    frimaDefaultDialogueScene4 = frimaDefaultDialogue.transform.Find("Scene4").gameObject;
                    frimaDefaultDialogueScene5 = frimaDefaultDialogue.transform.Find("Scene5").gameObject;
                    frimaDefaultDialogueDialogueActivator = frimaDefaultDialogue.transform.Find("DialogueActivator").gameObject;
                    frimaDefaultDialogueDialogueFinisher = frimaDefaultDialogue.transform.Find("DialogueFinisher").gameObject;
                    frimaDefaultDialogueMouthActivator = frimaDefaultDialogue.transform.Find("MouthActivator").gameObject;
                    frimaDefaultDialogueSpriteFocus = frimaDefaultDialogue.transform.Find("SpriteFocus").gameObject;

                    guiltyDefaultDialogue = CreateNewDialogue("GuiltyDialogueDefault", Places.mountainLabRoomNikkeGuiltyRoomtalk.transform);
                    guiltyDefaultDialogueScene1 = guiltyDefaultDialogue.transform.Find("Scene1").gameObject;
                    guiltyDefaultDialogueScene2 = guiltyDefaultDialogue.transform.Find("Scene2").gameObject;
                    guiltyDefaultDialogueScene3 = guiltyDefaultDialogue.transform.Find("Scene3").gameObject;
                    guiltyDefaultDialogueScene4 = guiltyDefaultDialogue.transform.Find("Scene4").gameObject;
                    guiltyDefaultDialogueScene5 = guiltyDefaultDialogue.transform.Find("Scene5").gameObject;
                    guiltyDefaultDialogueDialogueActivator = guiltyDefaultDialogue.transform.Find("DialogueActivator").gameObject;
                    guiltyDefaultDialogueDialogueFinisher = guiltyDefaultDialogue.transform.Find("DialogueFinisher").gameObject;
                    guiltyDefaultDialogueMouthActivator = guiltyDefaultDialogue.transform.Find("MouthActivator").gameObject;
                    guiltyDefaultDialogueSpriteFocus = guiltyDefaultDialogue.transform.Find("SpriteFocus").gameObject;

                    helmDefaultDialogue = CreateNewDialogue("HelmDialogueDefault", Places.mountainLabRoomNikkeHelmRoomtalk.transform);
                    helmDefaultDialogueScene1 = helmDefaultDialogue.transform.Find("Scene1").gameObject;
                    helmDefaultDialogueScene2 = helmDefaultDialogue.transform.Find("Scene2").gameObject;
                    helmDefaultDialogueScene3 = helmDefaultDialogue.transform.Find("Scene3").gameObject;
                    helmDefaultDialogueScene4 = helmDefaultDialogue.transform.Find("Scene4").gameObject;
                    helmDefaultDialogueScene5 = helmDefaultDialogue.transform.Find("Scene5").gameObject;
                    helmDefaultDialogueDialogueActivator = helmDefaultDialogue.transform.Find("DialogueActivator").gameObject;
                    helmDefaultDialogueDialogueFinisher = helmDefaultDialogue.transform.Find("DialogueFinisher").gameObject;
                    helmDefaultDialogueMouthActivator = helmDefaultDialogue.transform.Find("MouthActivator").gameObject;
                    helmDefaultDialogueSpriteFocus = helmDefaultDialogue.transform.Find("SpriteFocus").gameObject;

                    maidenDefaultDialogue = CreateNewDialogue("MaidenDialogueDefault", Places.mountainLabRoomNikkeMaidenRoomtalk.transform);
                    maidenDefaultDialogueScene1 = maidenDefaultDialogue.transform.Find("Scene1").gameObject;
                    maidenDefaultDialogueScene2 = maidenDefaultDialogue.transform.Find("Scene2").gameObject;
                    maidenDefaultDialogueScene3 = maidenDefaultDialogue.transform.Find("Scene3").gameObject;
                    maidenDefaultDialogueScene4 = maidenDefaultDialogue.transform.Find("Scene4").gameObject;
                    maidenDefaultDialogueScene5 = maidenDefaultDialogue.transform.Find("Scene5").gameObject;
                    maidenDefaultDialogueDialogueActivator = maidenDefaultDialogue.transform.Find("DialogueActivator").gameObject;
                    maidenDefaultDialogueDialogueFinisher = maidenDefaultDialogue.transform.Find("DialogueFinisher").gameObject;
                    maidenDefaultDialogueMouthActivator = maidenDefaultDialogue.transform.Find("MouthActivator").gameObject;
                    maidenDefaultDialogueSpriteFocus = maidenDefaultDialogue.transform.Find("SpriteFocus").gameObject;

                    maryDefaultDialogue = CreateNewDialogue("MaryDialogueDefault", Places.mountainLabRoomNikkeMaryRoomtalk.transform);
                    maryDefaultDialogueScene1 = maryDefaultDialogue.transform.Find("Scene1").gameObject;
                    maryDefaultDialogueScene2 = maryDefaultDialogue.transform.Find("Scene2").gameObject;
                    maryDefaultDialogueScene3 = maryDefaultDialogue.transform.Find("Scene3").gameObject;
                    maryDefaultDialogueScene4 = maryDefaultDialogue.transform.Find("Scene4").gameObject;
                    maryDefaultDialogueScene5 = maryDefaultDialogue.transform.Find("Scene5").gameObject;
                    maryDefaultDialogueDialogueActivator = maryDefaultDialogue.transform.Find("DialogueActivator").gameObject;
                    maryDefaultDialogueDialogueFinisher = maryDefaultDialogue.transform.Find("DialogueFinisher").gameObject;
                    maryDefaultDialogueMouthActivator = maryDefaultDialogue.transform.Find("MouthActivator").gameObject;
                    maryDefaultDialogueSpriteFocus = maryDefaultDialogue.transform.Find("SpriteFocus").gameObject;

                    mastDefaultDialogue = CreateNewDialogue("MastDialogueDefault", Places.mountainLabRoomNikkeMastRoomtalk.transform);
                    mastDefaultDialogueScene1 = mastDefaultDialogue.transform.Find("Scene1").gameObject;
                    mastDefaultDialogueScene2 = mastDefaultDialogue.transform.Find("Scene2").gameObject;
                    mastDefaultDialogueScene3 = mastDefaultDialogue.transform.Find("Scene3").gameObject;
                    mastDefaultDialogueScene4 = mastDefaultDialogue.transform.Find("Scene4").gameObject;
                    mastDefaultDialogueScene5 = mastDefaultDialogue.transform.Find("Scene5").gameObject;
                    mastDefaultDialogueDialogueActivator = mastDefaultDialogue.transform.Find("DialogueActivator").gameObject;
                    mastDefaultDialogueDialogueFinisher = mastDefaultDialogue.transform.Find("DialogueFinisher").gameObject;
                    mastDefaultDialogueMouthActivator = mastDefaultDialogue.transform.Find("MouthActivator").gameObject;
                    mastDefaultDialogueSpriteFocus = mastDefaultDialogue.transform.Find("SpriteFocus").gameObject;

                    neonDefaultDialogue = CreateNewDialogue("NeonDialogueDefault", Places.mountainLabRoomNikkeNeonRoomtalk.transform);
                    neonDefaultDialogueScene1 = neonDefaultDialogue.transform.Find("Scene1").gameObject;
                    neonDefaultDialogueScene2 = neonDefaultDialogue.transform.Find("Scene2").gameObject;
                    neonDefaultDialogueScene3 = neonDefaultDialogue.transform.Find("Scene3").gameObject;
                    neonDefaultDialogueScene4 = neonDefaultDialogue.transform.Find("Scene4").gameObject;
                    neonDefaultDialogueScene5 = neonDefaultDialogue.transform.Find("Scene5").gameObject;
                    neonDefaultDialogueDialogueActivator = neonDefaultDialogue.transform.Find("DialogueActivator").gameObject;
                    neonDefaultDialogueDialogueFinisher = neonDefaultDialogue.transform.Find("DialogueFinisher").gameObject;
                    neonDefaultDialogueMouthActivator = neonDefaultDialogue.transform.Find("MouthActivator").gameObject;
                    neonDefaultDialogueSpriteFocus = neonDefaultDialogue.transform.Find("SpriteFocus").gameObject;

                    pepperDefaultDialogue = CreateNewDialogue("PepperDialogueDefault", Places.mountainLabRoomNikkePepperRoomtalk.transform);
                    pepperDefaultDialogueScene1 = pepperDefaultDialogue.transform.Find("Scene1").gameObject;
                    pepperDefaultDialogueScene2 = pepperDefaultDialogue.transform.Find("Scene2").gameObject;
                    pepperDefaultDialogueScene3 = pepperDefaultDialogue.transform.Find("Scene3").gameObject;
                    pepperDefaultDialogueScene4 = pepperDefaultDialogue.transform.Find("Scene4").gameObject;
                    pepperDefaultDialogueScene5 = pepperDefaultDialogue.transform.Find("Scene5").gameObject;
                    pepperDefaultDialogueDialogueActivator = pepperDefaultDialogue.transform.Find("DialogueActivator").gameObject;
                    pepperDefaultDialogueDialogueFinisher = pepperDefaultDialogue.transform.Find("DialogueFinisher").gameObject;
                    pepperDefaultDialogueMouthActivator = pepperDefaultDialogue.transform.Find("MouthActivator").gameObject;
                    pepperDefaultDialogueSpriteFocus = pepperDefaultDialogue.transform.Find("SpriteFocus").gameObject;

                    rapiDefaultDialogue = CreateNewDialogue("RapiDialogueDefault", Places.mountainLabRoomNikkeRapiRoomtalk.transform);
                    rapiDefaultDialogueScene1 = rapiDefaultDialogue.transform.Find("Scene1").gameObject;
                    rapiDefaultDialogueScene2 = rapiDefaultDialogue.transform.Find("Scene2").gameObject;
                    rapiDefaultDialogueScene3 = rapiDefaultDialogue.transform.Find("Scene3").gameObject;
                    rapiDefaultDialogueScene4 = rapiDefaultDialogue.transform.Find("Scene4").gameObject;
                    rapiDefaultDialogueScene5 = rapiDefaultDialogue.transform.Find("Scene5").gameObject;
                    rapiDefaultDialogueDialogueActivator = rapiDefaultDialogue.transform.Find("DialogueActivator").gameObject;
                    rapiDefaultDialogueDialogueFinisher = rapiDefaultDialogue.transform.Find("DialogueFinisher").gameObject;
                    rapiDefaultDialogueMouthActivator = rapiDefaultDialogue.transform.Find("MouthActivator").gameObject;
                    rapiDefaultDialogueSpriteFocus = rapiDefaultDialogue.transform.Find("SpriteFocus").gameObject;

                    rosannaDefaultDialogue = CreateNewDialogue("RosannaDialogueDefault", Places.mountainLabRoomNikkeRosannaRoomtalk.transform);
                    rosannaDefaultDialogueScene1 = rosannaDefaultDialogue.transform.Find("Scene1").gameObject;
                    rosannaDefaultDialogueScene2 = rosannaDefaultDialogue.transform.Find("Scene2").gameObject;
                    rosannaDefaultDialogueScene3 = rosannaDefaultDialogue.transform.Find("Scene3").gameObject;
                    rosannaDefaultDialogueScene4 = rosannaDefaultDialogue.transform.Find("Scene4").gameObject;
                    rosannaDefaultDialogueScene5 = rosannaDefaultDialogue.transform.Find("Scene5").gameObject;
                    rosannaDefaultDialogueDialogueActivator = rosannaDefaultDialogue.transform.Find("DialogueActivator").gameObject;
                    rosannaDefaultDialogueDialogueFinisher = rosannaDefaultDialogue.transform.Find("DialogueFinisher").gameObject;
                    rosannaDefaultDialogueMouthActivator = rosannaDefaultDialogue.transform.Find("MouthActivator").gameObject;
                    rosannaDefaultDialogueSpriteFocus = rosannaDefaultDialogue.transform.Find("SpriteFocus").gameObject;

                    sakuraDefaultDialogue = CreateNewDialogue("SakuraDialogueDefault", Places.mountainLabRoomNikkeSakuraRoomtalk.transform);
                    sakuraDefaultDialogueScene1 = sakuraDefaultDialogue.transform.Find("Scene1").gameObject;
                    sakuraDefaultDialogueScene2 = sakuraDefaultDialogue.transform.Find("Scene2").gameObject;
                    sakuraDefaultDialogueScene3 = sakuraDefaultDialogue.transform.Find("Scene3").gameObject;
                    sakuraDefaultDialogueScene4 = sakuraDefaultDialogue.transform.Find("Scene4").gameObject;
                    sakuraDefaultDialogueScene5 = sakuraDefaultDialogue.transform.Find("Scene5").gameObject;
                    sakuraDefaultDialogueDialogueActivator = sakuraDefaultDialogue.transform.Find("DialogueActivator").gameObject;
                    sakuraDefaultDialogueDialogueFinisher = sakuraDefaultDialogue.transform.Find("DialogueFinisher").gameObject;
                    sakuraDefaultDialogueMouthActivator = sakuraDefaultDialogue.transform.Find("MouthActivator").gameObject;
                    sakuraDefaultDialogueSpriteFocus = sakuraDefaultDialogue.transform.Find("SpriteFocus").gameObject;

                    viperDefaultDialogue = CreateNewDialogue("ViperDialogueDefault", Places.mountainLabRoomNikkeViperRoomtalk.transform);
                    viperDefaultDialogueScene1 = viperDefaultDialogue.transform.Find("Scene1").gameObject;
                    viperDefaultDialogueScene2 = viperDefaultDialogue.transform.Find("Scene2").gameObject;
                    viperDefaultDialogueScene3 = viperDefaultDialogue.transform.Find("Scene3").gameObject;
                    viperDefaultDialogueScene4 = viperDefaultDialogue.transform.Find("Scene4").gameObject;
                    viperDefaultDialogueScene5 = viperDefaultDialogue.transform.Find("Scene5").gameObject;
                    viperDefaultDialogueDialogueActivator = viperDefaultDialogue.transform.Find("DialogueActivator").gameObject;
                    viperDefaultDialogueDialogueFinisher = viperDefaultDialogue.transform.Find("DialogueFinisher").gameObject;
                    viperDefaultDialogueMouthActivator = viperDefaultDialogue.transform.Find("MouthActivator").gameObject;
                    viperDefaultDialogueSpriteFocus = viperDefaultDialogue.transform.Find("SpriteFocus").gameObject;

                    yanDefaultDialogue = CreateNewDialogue("YanDialogueDefault", Places.mountainLabRoomNikkeYanRoomtalk.transform);
                    yanDefaultDialogueScene1 = yanDefaultDialogue.transform.Find("Scene1").gameObject;
                    yanDefaultDialogueScene2 = yanDefaultDialogue.transform.Find("Scene2").gameObject;
                    yanDefaultDialogueScene3 = yanDefaultDialogue.transform.Find("Scene3").gameObject;
                    yanDefaultDialogueScene4 = yanDefaultDialogue.transform.Find("Scene4").gameObject;
                    yanDefaultDialogueScene5 = yanDefaultDialogue.transform.Find("Scene5").gameObject;
                    yanDefaultDialogueDialogueActivator = yanDefaultDialogue.transform.Find("DialogueActivator").gameObject;
                    yanDefaultDialogueDialogueFinisher = yanDefaultDialogue.transform.Find("DialogueFinisher").gameObject;
                    yanDefaultDialogueMouthActivator = yanDefaultDialogue.transform.Find("MouthActivator").gameObject;
                    yanDefaultDialogueSpriteFocus = yanDefaultDialogue.transform.Find("SpriteFocus").gameObject;
                    #endregion
                    #region Event Initialization
                    amberHospitalhallwayEvent01Dialogue = CreateNewDialogue("AmberDialogueHospitalhallway01", Places.roomTalkHospitalHallway.transform);
                    amberHospitalhallwayEvent01DialogueScene1 = amberHospitalhallwayEvent01Dialogue.transform.Find("Scene1").gameObject;
                    amberHospitalhallwayEvent01DialogueScene2 = amberHospitalhallwayEvent01Dialogue.transform.Find("Scene2").gameObject;
                    amberHospitalhallwayEvent01DialogueScene3 = amberHospitalhallwayEvent01Dialogue.transform.Find("Scene3").gameObject;
                    amberHospitalhallwayEvent01DialogueScene4 = amberHospitalhallwayEvent01Dialogue.transform.Find("Scene4").gameObject;
                    amberHospitalhallwayEvent01DialogueScene5 = amberHospitalhallwayEvent01Dialogue.transform.Find("Scene5").gameObject;
                    amberHospitalhallwayEvent01DialogueDialogueActivator = amberHospitalhallwayEvent01Dialogue.transform.Find("DialogueActivator").gameObject;
                    amberHospitalhallwayEvent01DialogueDialogueFinisher = amberHospitalhallwayEvent01Dialogue.transform.Find("DialogueFinisher").gameObject;
                    amberHospitalhallwayEvent01DialogueMouthActivator = amberHospitalhallwayEvent01Dialogue.transform.Find("MouthActivator").gameObject;
                    amberHospitalhallwayEvent01DialogueSpriteFocus = amberHospitalhallwayEvent01Dialogue.transform.Find("SpriteFocus").gameObject;

                    anisMallEvent01Dialogue = CreateNewDialogue("AnisDialogueMall01", Places.roomTalkMall.transform);
                    anisMallEvent01DialogueScene1 = anisMallEvent01Dialogue.transform.Find("Scene1").gameObject;
                    anisMallEvent01DialogueScene2 = anisMallEvent01Dialogue.transform.Find("Scene2").gameObject;
                    anisMallEvent01DialogueScene3 = anisMallEvent01Dialogue.transform.Find("Scene3").gameObject;
                    anisMallEvent01DialogueScene4 = anisMallEvent01Dialogue.transform.Find("Scene4").gameObject;
                    anisMallEvent01DialogueScene5 = anisMallEvent01Dialogue.transform.Find("Scene5").gameObject;
                    anisMallEvent01DialogueDialogueActivator = anisMallEvent01Dialogue.transform.Find("DialogueActivator").gameObject;
                    anisMallEvent01DialogueDialogueFinisher = anisMallEvent01Dialogue.transform.Find("DialogueFinisher").gameObject;
                    anisMallEvent01DialogueMouthActivator = anisMallEvent01Dialogue.transform.Find("MouthActivator").gameObject;
                    anisMallEvent01DialogueSpriteFocus = anisMallEvent01Dialogue.transform.Find("SpriteFocus").gameObject;

                    centiKenshomeEvent01Dialogue = CreateNewDialogue("CentiDialogueKenshome01", Places.roomTalkKensHome.transform);
                    centiKenshomeEvent01DialogueScene1 = centiKenshomeEvent01Dialogue.transform.Find("Scene1").gameObject;
                    centiKenshomeEvent01DialogueScene2 = centiKenshomeEvent01Dialogue.transform.Find("Scene2").gameObject;
                    centiKenshomeEvent01DialogueScene3 = centiKenshomeEvent01Dialogue.transform.Find("Scene3").gameObject;
                    centiKenshomeEvent01DialogueScene4 = centiKenshomeEvent01Dialogue.transform.Find("Scene4").gameObject;
                    centiKenshomeEvent01DialogueScene5 = centiKenshomeEvent01Dialogue.transform.Find("Scene5").gameObject;
                    centiKenshomeEvent01DialogueDialogueActivator = centiKenshomeEvent01Dialogue.transform.Find("DialogueActivator").gameObject;
                    centiKenshomeEvent01DialogueDialogueFinisher = centiKenshomeEvent01Dialogue.transform.Find("DialogueFinisher").gameObject;
                    centiKenshomeEvent01DialogueMouthActivator = centiKenshomeEvent01Dialogue.transform.Find("MouthActivator").gameObject;
                    centiKenshomeEvent01DialogueSpriteFocus = centiKenshomeEvent01Dialogue.transform.Find("SpriteFocus").gameObject;

                    dorothyParkEvent01Dialogue = CreateNewDialogue("DorothyDialoguePark01", Places.roomTalkPark.transform);
                    dorothyParkEvent01DialogueScene1 = dorothyParkEvent01Dialogue.transform.Find("Scene1").gameObject;
                    dorothyParkEvent01DialogueScene2 = dorothyParkEvent01Dialogue.transform.Find("Scene2").gameObject;
                    dorothyParkEvent01DialogueScene3 = dorothyParkEvent01Dialogue.transform.Find("Scene3").gameObject;
                    dorothyParkEvent01DialogueScene4 = dorothyParkEvent01Dialogue.transform.Find("Scene4").gameObject;
                    dorothyParkEvent01DialogueScene5 = dorothyParkEvent01Dialogue.transform.Find("Scene5").gameObject;
                    dorothyParkEvent01DialogueDialogueActivator = dorothyParkEvent01Dialogue.transform.Find("DialogueActivator").gameObject;
                    dorothyParkEvent01DialogueDialogueFinisher = dorothyParkEvent01Dialogue.transform.Find("DialogueFinisher").gameObject;
                    dorothyParkEvent01DialogueMouthActivator = dorothyParkEvent01Dialogue.transform.Find("MouthActivator").gameObject;
                    dorothyParkEvent01DialogueSpriteFocus = dorothyParkEvent01Dialogue.transform.Find("SpriteFocus").gameObject;

                    eleggDowntownEvent01Dialogue = CreateNewDialogue("EleggDialogueDowntown01", Places.roomTalkDowntown.transform);
                    eleggDowntownEvent01DialogueScene1 = eleggDowntownEvent01Dialogue.transform.Find("Scene1").gameObject;
                    eleggDowntownEvent01DialogueScene2 = eleggDowntownEvent01Dialogue.transform.Find("Scene2").gameObject;
                    eleggDowntownEvent01DialogueScene3 = eleggDowntownEvent01Dialogue.transform.Find("Scene3").gameObject;
                    eleggDowntownEvent01DialogueScene4 = eleggDowntownEvent01Dialogue.transform.Find("Scene4").gameObject;
                    eleggDowntownEvent01DialogueScene5 = eleggDowntownEvent01Dialogue.transform.Find("Scene5").gameObject;
                    eleggDowntownEvent01DialogueDialogueActivator = eleggDowntownEvent01Dialogue.transform.Find("DialogueActivator").gameObject;
                    eleggDowntownEvent01DialogueDialogueFinisher = eleggDowntownEvent01Dialogue.transform.Find("DialogueFinisher").gameObject;
                    eleggDowntownEvent01DialogueMouthActivator = eleggDowntownEvent01Dialogue.transform.Find("MouthActivator").gameObject;
                    eleggDowntownEvent01DialogueSpriteFocus = eleggDowntownEvent01Dialogue.transform.Find("SpriteFocus").gameObject;

                    frimaHotelEvent01Dialogue = CreateNewDialogue("FrimaDialogueHotel01", Places.roomTalkHotel.transform);
                    frimaHotelEvent01DialogueScene1 = frimaHotelEvent01Dialogue.transform.Find("Scene1").gameObject;
                    frimaHotelEvent01DialogueScene2 = frimaHotelEvent01Dialogue.transform.Find("Scene2").gameObject;
                    frimaHotelEvent01DialogueScene3 = frimaHotelEvent01Dialogue.transform.Find("Scene3").gameObject;
                    frimaHotelEvent01DialogueScene4 = frimaHotelEvent01Dialogue.transform.Find("Scene4").gameObject;
                    frimaHotelEvent01DialogueScene5 = frimaHotelEvent01Dialogue.transform.Find("Scene5").gameObject;
                    frimaHotelEvent01DialogueDialogueActivator = frimaHotelEvent01Dialogue.transform.Find("DialogueActivator").gameObject;
                    frimaHotelEvent01DialogueDialogueFinisher = frimaHotelEvent01Dialogue.transform.Find("DialogueFinisher").gameObject;
                    frimaHotelEvent01DialogueMouthActivator = frimaHotelEvent01Dialogue.transform.Find("MouthActivator").gameObject;
                    frimaHotelEvent01DialogueSpriteFocus = frimaHotelEvent01Dialogue.transform.Find("SpriteFocus").gameObject;

                    guiltyParkinglotEvent01Dialogue = CreateNewDialogue("GuiltyDialogueParkinglot01", Places.roomTalkParkingLot.transform);
                    guiltyParkinglotEvent01DialogueScene1 = guiltyParkinglotEvent01Dialogue.transform.Find("Scene1").gameObject;
                    guiltyParkinglotEvent01DialogueScene2 = guiltyParkinglotEvent01Dialogue.transform.Find("Scene2").gameObject;
                    guiltyParkinglotEvent01DialogueScene3 = guiltyParkinglotEvent01Dialogue.transform.Find("Scene3").gameObject;
                    guiltyParkinglotEvent01DialogueScene4 = guiltyParkinglotEvent01Dialogue.transform.Find("Scene4").gameObject;
                    guiltyParkinglotEvent01DialogueScene5 = guiltyParkinglotEvent01Dialogue.transform.Find("Scene5").gameObject;
                    guiltyParkinglotEvent01DialogueDialogueActivator = guiltyParkinglotEvent01Dialogue.transform.Find("DialogueActivator").gameObject;
                    guiltyParkinglotEvent01DialogueDialogueFinisher = guiltyParkinglotEvent01Dialogue.transform.Find("DialogueFinisher").gameObject;
                    guiltyParkinglotEvent01DialogueMouthActivator = guiltyParkinglotEvent01Dialogue.transform.Find("MouthActivator").gameObject;
                    guiltyParkinglotEvent01DialogueSpriteFocus = guiltyParkinglotEvent01Dialogue.transform.Find("SpriteFocus").gameObject;

                    helmBeachEvent01Dialogue = CreateNewDialogue("HelmDialogueBeach01", Places.roomTalkBeach.transform);
                    helmBeachEvent01DialogueScene1 = helmBeachEvent01Dialogue.transform.Find("Scene1").gameObject;
                    helmBeachEvent01DialogueScene2 = helmBeachEvent01Dialogue.transform.Find("Scene2").gameObject;
                    helmBeachEvent01DialogueScene3 = helmBeachEvent01Dialogue.transform.Find("Scene3").gameObject;
                    helmBeachEvent01DialogueScene4 = helmBeachEvent01Dialogue.transform.Find("Scene4").gameObject;
                    helmBeachEvent01DialogueScene5 = helmBeachEvent01Dialogue.transform.Find("Scene5").gameObject;
                    helmBeachEvent01DialogueDialogueActivator = helmBeachEvent01Dialogue.transform.Find("DialogueActivator").gameObject;
                    helmBeachEvent01DialogueDialogueFinisher = helmBeachEvent01Dialogue.transform.Find("DialogueFinisher").gameObject;
                    helmBeachEvent01DialogueMouthActivator = helmBeachEvent01Dialogue.transform.Find("MouthActivator").gameObject;
                    helmBeachEvent01DialogueSpriteFocus = helmBeachEvent01Dialogue.transform.Find("SpriteFocus").gameObject;

                    maidenAlleyEvent01Dialogue = CreateNewDialogue("MaidenDialogueAlley01", Places.roomTalkAlley.transform);
                    maidenAlleyEvent01DialogueScene1 = maidenAlleyEvent01Dialogue.transform.Find("Scene1").gameObject;
                    maidenAlleyEvent01DialogueScene2 = maidenAlleyEvent01Dialogue.transform.Find("Scene2").gameObject;
                    maidenAlleyEvent01DialogueScene3 = maidenAlleyEvent01Dialogue.transform.Find("Scene3").gameObject;
                    maidenAlleyEvent01DialogueScene4 = maidenAlleyEvent01Dialogue.transform.Find("Scene4").gameObject;
                    maidenAlleyEvent01DialogueScene5 = maidenAlleyEvent01Dialogue.transform.Find("Scene5").gameObject;
                    maidenAlleyEvent01DialogueDialogueActivator = maidenAlleyEvent01Dialogue.transform.Find("DialogueActivator").gameObject;
                    maidenAlleyEvent01DialogueDialogueFinisher = maidenAlleyEvent01Dialogue.transform.Find("DialogueFinisher").gameObject;
                    maidenAlleyEvent01DialogueMouthActivator = maidenAlleyEvent01Dialogue.transform.Find("MouthActivator").gameObject;
                    maidenAlleyEvent01DialogueSpriteFocus = maidenAlleyEvent01Dialogue.transform.Find("SpriteFocus").gameObject;

                    maryHospitalhallwayEvent01Dialogue = CreateNewDialogue("MaryDialogueHospitalhallway01", Places.roomTalkHospitalHallway.transform);
                    maryHospitalhallwayEvent01DialogueScene1 = maryHospitalhallwayEvent01Dialogue.transform.Find("Scene1").gameObject;
                    maryHospitalhallwayEvent01DialogueScene2 = maryHospitalhallwayEvent01Dialogue.transform.Find("Scene2").gameObject;
                    maryHospitalhallwayEvent01DialogueScene3 = maryHospitalhallwayEvent01Dialogue.transform.Find("Scene3").gameObject;
                    maryHospitalhallwayEvent01DialogueScene4 = maryHospitalhallwayEvent01Dialogue.transform.Find("Scene4").gameObject;
                    maryHospitalhallwayEvent01DialogueScene5 = maryHospitalhallwayEvent01Dialogue.transform.Find("Scene5").gameObject;
                    maryHospitalhallwayEvent01DialogueDialogueActivator = maryHospitalhallwayEvent01Dialogue.transform.Find("DialogueActivator").gameObject;
                    maryHospitalhallwayEvent01DialogueDialogueFinisher = maryHospitalhallwayEvent01Dialogue.transform.Find("DialogueFinisher").gameObject;
                    maryHospitalhallwayEvent01DialogueMouthActivator = maryHospitalhallwayEvent01Dialogue.transform.Find("MouthActivator").gameObject;
                    maryHospitalhallwayEvent01DialogueSpriteFocus = maryHospitalhallwayEvent01Dialogue.transform.Find("SpriteFocus").gameObject;

                    mastBeachEvent01Dialogue = CreateNewDialogue("MastDialogueBeach01", Places.roomTalkBeach.transform);
                    mastBeachEvent01DialogueScene1 = mastBeachEvent01Dialogue.transform.Find("Scene1").gameObject;
                    mastBeachEvent01DialogueScene2 = mastBeachEvent01Dialogue.transform.Find("Scene2").gameObject;
                    mastBeachEvent01DialogueScene3 = mastBeachEvent01Dialogue.transform.Find("Scene3").gameObject;
                    mastBeachEvent01DialogueScene4 = mastBeachEvent01Dialogue.transform.Find("Scene4").gameObject;
                    mastBeachEvent01DialogueScene5 = mastBeachEvent01Dialogue.transform.Find("Scene5").gameObject;
                    mastBeachEvent01DialogueDialogueActivator = mastBeachEvent01Dialogue.transform.Find("DialogueActivator").gameObject;
                    mastBeachEvent01DialogueDialogueFinisher = mastBeachEvent01Dialogue.transform.Find("DialogueFinisher").gameObject;
                    mastBeachEvent01DialogueMouthActivator = mastBeachEvent01Dialogue.transform.Find("MouthActivator").gameObject;
                    mastBeachEvent01DialogueSpriteFocus = mastBeachEvent01Dialogue.transform.Find("SpriteFocus").gameObject;

                    neonTempleEvent01Dialogue = CreateNewDialogue("NeonDialogueTemple01", Places.roomTalkTemple.transform);
                    neonTempleEvent01DialogueScene1 = neonTempleEvent01Dialogue.transform.Find("Scene1").gameObject;
                    neonTempleEvent01DialogueScene2 = neonTempleEvent01Dialogue.transform.Find("Scene2").gameObject;
                    neonTempleEvent01DialogueScene3 = neonTempleEvent01Dialogue.transform.Find("Scene3").gameObject;
                    neonTempleEvent01DialogueScene4 = neonTempleEvent01Dialogue.transform.Find("Scene4").gameObject;
                    neonTempleEvent01DialogueScene5 = neonTempleEvent01Dialogue.transform.Find("Scene5").gameObject;
                    neonTempleEvent01DialogueDialogueActivator = neonTempleEvent01Dialogue.transform.Find("DialogueActivator").gameObject;
                    neonTempleEvent01DialogueDialogueFinisher = neonTempleEvent01Dialogue.transform.Find("DialogueFinisher").gameObject;
                    neonTempleEvent01DialogueMouthActivator = neonTempleEvent01Dialogue.transform.Find("MouthActivator").gameObject;
                    neonTempleEvent01DialogueSpriteFocus = neonTempleEvent01Dialogue.transform.Find("SpriteFocus").gameObject;

                    pepperHospitalEvent01Dialogue = CreateNewDialogue("PepperDialogueHospital01", Places.roomTalkHospital.transform);
                    pepperHospitalEvent01DialogueScene1 = pepperHospitalEvent01Dialogue.transform.Find("Scene1").gameObject;
                    pepperHospitalEvent01DialogueScene2 = pepperHospitalEvent01Dialogue.transform.Find("Scene2").gameObject;
                    pepperHospitalEvent01DialogueScene3 = pepperHospitalEvent01Dialogue.transform.Find("Scene3").gameObject;
                    pepperHospitalEvent01DialogueScene4 = pepperHospitalEvent01Dialogue.transform.Find("Scene4").gameObject;
                    pepperHospitalEvent01DialogueScene5 = pepperHospitalEvent01Dialogue.transform.Find("Scene5").gameObject;
                    pepperHospitalEvent01DialogueDialogueActivator = pepperHospitalEvent01Dialogue.transform.Find("DialogueActivator").gameObject;
                    pepperHospitalEvent01DialogueDialogueFinisher = pepperHospitalEvent01Dialogue.transform.Find("DialogueFinisher").gameObject;
                    pepperHospitalEvent01DialogueMouthActivator = pepperHospitalEvent01Dialogue.transform.Find("MouthActivator").gameObject;
                    pepperHospitalEvent01DialogueSpriteFocus = pepperHospitalEvent01Dialogue.transform.Find("SpriteFocus").gameObject;

                    rapiGasstationEvent01Dialogue = CreateNewDialogue("RapiDialogueGasstation01", Places.roomTalkGasStation.transform);
                    rapiGasstationEvent01DialogueScene1 = rapiGasstationEvent01Dialogue.transform.Find("Scene1").gameObject;
                    rapiGasstationEvent01DialogueScene2 = rapiGasstationEvent01Dialogue.transform.Find("Scene2").gameObject;
                    rapiGasstationEvent01DialogueScene3 = rapiGasstationEvent01Dialogue.transform.Find("Scene3").gameObject;
                    rapiGasstationEvent01DialogueScene4 = rapiGasstationEvent01Dialogue.transform.Find("Scene4").gameObject;
                    rapiGasstationEvent01DialogueScene5 = rapiGasstationEvent01Dialogue.transform.Find("Scene5").gameObject;
                    rapiGasstationEvent01DialogueDialogueActivator = rapiGasstationEvent01Dialogue.transform.Find("DialogueActivator").gameObject;
                    rapiGasstationEvent01DialogueDialogueFinisher = rapiGasstationEvent01Dialogue.transform.Find("DialogueFinisher").gameObject;
                    rapiGasstationEvent01DialogueMouthActivator = rapiGasstationEvent01Dialogue.transform.Find("MouthActivator").gameObject;
                    rapiGasstationEvent01DialogueSpriteFocus = rapiGasstationEvent01Dialogue.transform.Find("SpriteFocus").gameObject;

                    rosannaGabrielsmansionEvent01Dialogue = CreateNewDialogue("RosannaDialogueGabrielsmansion01", Places.roomTalkGabrielsMansion.transform);
                    rosannaGabrielsmansionEvent01DialogueScene1 = rosannaGabrielsmansionEvent01Dialogue.transform.Find("Scene1").gameObject;
                    rosannaGabrielsmansionEvent01DialogueScene2 = rosannaGabrielsmansionEvent01Dialogue.transform.Find("Scene2").gameObject;
                    rosannaGabrielsmansionEvent01DialogueScene3 = rosannaGabrielsmansionEvent01Dialogue.transform.Find("Scene3").gameObject;
                    rosannaGabrielsmansionEvent01DialogueScene4 = rosannaGabrielsmansionEvent01Dialogue.transform.Find("Scene4").gameObject;
                    rosannaGabrielsmansionEvent01DialogueScene5 = rosannaGabrielsmansionEvent01Dialogue.transform.Find("Scene5").gameObject;
                    rosannaGabrielsmansionEvent01DialogueDialogueActivator = rosannaGabrielsmansionEvent01Dialogue.transform.Find("DialogueActivator").gameObject;
                    rosannaGabrielsmansionEvent01DialogueDialogueFinisher = rosannaGabrielsmansionEvent01Dialogue.transform.Find("DialogueFinisher").gameObject;
                    rosannaGabrielsmansionEvent01DialogueMouthActivator = rosannaGabrielsmansionEvent01Dialogue.transform.Find("MouthActivator").gameObject;
                    rosannaGabrielsmansionEvent01DialogueSpriteFocus = rosannaGabrielsmansionEvent01Dialogue.transform.Find("SpriteFocus").gameObject;

                    sakuraForestEvent01Dialogue = CreateNewDialogue("SakuraDialogueForest01", Places.roomTalkForest.transform);
                    sakuraForestEvent01DialogueScene1 = sakuraForestEvent01Dialogue.transform.Find("Scene1").gameObject;
                    sakuraForestEvent01DialogueScene2 = sakuraForestEvent01Dialogue.transform.Find("Scene2").gameObject;
                    sakuraForestEvent01DialogueScene3 = sakuraForestEvent01Dialogue.transform.Find("Scene3").gameObject;
                    sakuraForestEvent01DialogueScene4 = sakuraForestEvent01Dialogue.transform.Find("Scene4").gameObject;
                    sakuraForestEvent01DialogueScene5 = sakuraForestEvent01Dialogue.transform.Find("Scene5").gameObject;
                    sakuraForestEvent01DialogueDialogueActivator = sakuraForestEvent01Dialogue.transform.Find("DialogueActivator").gameObject;
                    sakuraForestEvent01DialogueDialogueFinisher = sakuraForestEvent01Dialogue.transform.Find("DialogueFinisher").gameObject;
                    sakuraForestEvent01DialogueMouthActivator = sakuraForestEvent01Dialogue.transform.Find("MouthActivator").gameObject;
                    sakuraForestEvent01DialogueSpriteFocus = sakuraForestEvent01Dialogue.transform.Find("SpriteFocus").gameObject;

                    viperVillaEvent01Dialogue = CreateNewDialogue("ViperDialogueVilla01", Places.roomTalkVilla.transform);
                    viperVillaEvent01DialogueScene1 = viperVillaEvent01Dialogue.transform.Find("Scene1").gameObject;
                    viperVillaEvent01DialogueScene2 = viperVillaEvent01Dialogue.transform.Find("Scene2").gameObject;
                    viperVillaEvent01DialogueScene3 = viperVillaEvent01Dialogue.transform.Find("Scene3").gameObject;
                    viperVillaEvent01DialogueScene4 = viperVillaEvent01Dialogue.transform.Find("Scene4").gameObject;
                    viperVillaEvent01DialogueScene5 = viperVillaEvent01Dialogue.transform.Find("Scene5").gameObject;
                    viperVillaEvent01DialogueDialogueActivator = viperVillaEvent01Dialogue.transform.Find("DialogueActivator").gameObject;
                    viperVillaEvent01DialogueDialogueFinisher = viperVillaEvent01Dialogue.transform.Find("DialogueFinisher").gameObject;
                    viperVillaEvent01DialogueMouthActivator = viperVillaEvent01Dialogue.transform.Find("MouthActivator").gameObject;
                    viperVillaEvent01DialogueSpriteFocus = viperVillaEvent01Dialogue.transform.Find("SpriteFocus").gameObject;

                    yanMallEvent01Dialogue = CreateNewDialogue("YanDialogueMall01", Places.roomTalkMall.transform);
                    yanMallEvent01DialogueScene1 = yanMallEvent01Dialogue.transform.Find("Scene1").gameObject;
                    yanMallEvent01DialogueScene2 = yanMallEvent01Dialogue.transform.Find("Scene2").gameObject;
                    yanMallEvent01DialogueScene3 = yanMallEvent01Dialogue.transform.Find("Scene3").gameObject;
                    yanMallEvent01DialogueScene4 = yanMallEvent01Dialogue.transform.Find("Scene4").gameObject;
                    yanMallEvent01DialogueScene5 = yanMallEvent01Dialogue.transform.Find("Scene5").gameObject;
                    yanMallEvent01DialogueDialogueActivator = yanMallEvent01Dialogue.transform.Find("DialogueActivator").gameObject;
                    yanMallEvent01DialogueDialogueFinisher = yanMallEvent01Dialogue.transform.Find("DialogueFinisher").gameObject;
                    yanMallEvent01DialogueMouthActivator = yanMallEvent01Dialogue.transform.Find("MouthActivator").gameObject;
                    yanMallEvent01DialogueSpriteFocus = yanMallEvent01Dialogue.transform.Find("SpriteFocus").gameObject;
                    #endregion
                    #region Voyeur Initialization
                    anisSecretbeachVoyeur01Dialogue = CreateNewDialogue("AnisDialogueSecretbeach01", Places.secretBeachRoomtalk.transform);
                    anisSecretbeachVoyeur01DialogueScene1 = anisSecretbeachVoyeur01Dialogue.transform.Find("Scene1").gameObject;
                    anisSecretbeachVoyeur01DialogueScene2 = anisSecretbeachVoyeur01Dialogue.transform.Find("Scene2").gameObject;
                    anisSecretbeachVoyeur01DialogueScene3 = anisSecretbeachVoyeur01Dialogue.transform.Find("Scene3").gameObject;
                    anisSecretbeachVoyeur01DialogueScene4 = anisSecretbeachVoyeur01Dialogue.transform.Find("Scene4").gameObject;
                    anisSecretbeachVoyeur01DialogueScene5 = anisSecretbeachVoyeur01Dialogue.transform.Find("Scene5").gameObject;
                    anisSecretbeachVoyeur01DialogueActivator = anisSecretbeachVoyeur01Dialogue.transform.Find("DialogueActivator").gameObject;
                    anisSecretbeachVoyeur01DialogueFinisher = anisSecretbeachVoyeur01Dialogue.transform.Find("DialogueFinisher").gameObject;
                    anisSecretbeachVoyeur01DialogueMouthActivator = anisSecretbeachVoyeur01Dialogue.transform.Find("MouthActivator").gameObject;
                    anisSecretbeachVoyeur01DialogueSpriteFocus = anisSecretbeachVoyeur01Dialogue.transform.Find("SpriteFocus").gameObject;

                    centiSecretbeachVoyeur01Dialogue = CreateNewDialogue("CentiDialogueSecretbeach01", Places.secretBeachRoomtalk.transform);
                    centiSecretbeachVoyeur01DialogueScene1 = centiSecretbeachVoyeur01Dialogue.transform.Find("Scene1").gameObject;
                    centiSecretbeachVoyeur01DialogueScene2 = centiSecretbeachVoyeur01Dialogue.transform.Find("Scene2").gameObject;
                    centiSecretbeachVoyeur01DialogueScene3 = centiSecretbeachVoyeur01Dialogue.transform.Find("Scene3").gameObject;
                    centiSecretbeachVoyeur01DialogueScene4 = centiSecretbeachVoyeur01Dialogue.transform.Find("Scene4").gameObject;
                    centiSecretbeachVoyeur01DialogueScene5 = centiSecretbeachVoyeur01Dialogue.transform.Find("Scene5").gameObject;
                    centiSecretbeachVoyeur01DialogueActivator = centiSecretbeachVoyeur01Dialogue.transform.Find("DialogueActivator").gameObject;
                    centiSecretbeachVoyeur01DialogueFinisher = centiSecretbeachVoyeur01Dialogue.transform.Find("DialogueFinisher").gameObject;
                    centiSecretbeachVoyeur01DialogueMouthActivator = centiSecretbeachVoyeur01Dialogue.transform.Find("MouthActivator").gameObject;
                    centiSecretbeachVoyeur01DialogueSpriteFocus = centiSecretbeachVoyeur01Dialogue.transform.Find("SpriteFocus").gameObject;

                    dorothySecretbeachVoyeur01Dialogue = CreateNewDialogue("DorothyDialogueSecretbeach01", Places.secretBeachRoomtalk.transform);
                    dorothySecretbeachVoyeur01DialogueScene1 = dorothySecretbeachVoyeur01Dialogue.transform.Find("Scene1").gameObject;
                    dorothySecretbeachVoyeur01DialogueScene2 = dorothySecretbeachVoyeur01Dialogue.transform.Find("Scene2").gameObject;
                    dorothySecretbeachVoyeur01DialogueScene3 = dorothySecretbeachVoyeur01Dialogue.transform.Find("Scene3").gameObject;
                    dorothySecretbeachVoyeur01DialogueScene4 = dorothySecretbeachVoyeur01Dialogue.transform.Find("Scene4").gameObject;
                    dorothySecretbeachVoyeur01DialogueScene5 = dorothySecretbeachVoyeur01Dialogue.transform.Find("Scene5").gameObject;
                    dorothySecretbeachVoyeur01DialogueActivator = dorothySecretbeachVoyeur01Dialogue.transform.Find("DialogueActivator").gameObject;
                    dorothySecretbeachVoyeur01DialogueFinisher = dorothySecretbeachVoyeur01Dialogue.transform.Find("DialogueFinisher").gameObject;
                    dorothySecretbeachVoyeur01DialogueMouthActivator = dorothySecretbeachVoyeur01Dialogue.transform.Find("MouthActivator").gameObject;
                    dorothySecretbeachVoyeur01DialogueSpriteFocus = dorothySecretbeachVoyeur01Dialogue.transform.Find("SpriteFocus").gameObject;

                    eleggSecretbeachVoyeur01Dialogue = CreateNewDialogue("EleggDialogueSecretbeach01", Places.secretBeachRoomtalk.transform);
                    eleggSecretbeachVoyeur01DialogueScene1 = eleggSecretbeachVoyeur01Dialogue.transform.Find("Scene1").gameObject;
                    eleggSecretbeachVoyeur01DialogueScene2 = eleggSecretbeachVoyeur01Dialogue.transform.Find("Scene2").gameObject;
                    eleggSecretbeachVoyeur01DialogueScene3 = eleggSecretbeachVoyeur01Dialogue.transform.Find("Scene3").gameObject;
                    eleggSecretbeachVoyeur01DialogueScene4 = eleggSecretbeachVoyeur01Dialogue.transform.Find("Scene4").gameObject;
                    eleggSecretbeachVoyeur01DialogueScene5 = eleggSecretbeachVoyeur01Dialogue.transform.Find("Scene5").gameObject;
                    eleggSecretbeachVoyeur01DialogueActivator = eleggSecretbeachVoyeur01Dialogue.transform.Find("DialogueActivator").gameObject;
                    eleggSecretbeachVoyeur01DialogueFinisher = eleggSecretbeachVoyeur01Dialogue.transform.Find("DialogueFinisher").gameObject;
                    eleggSecretbeachVoyeur01DialogueMouthActivator = eleggSecretbeachVoyeur01Dialogue.transform.Find("MouthActivator").gameObject;
                    eleggSecretbeachVoyeur01DialogueSpriteFocus = eleggSecretbeachVoyeur01Dialogue.transform.Find("SpriteFocus").gameObject;

                    frimaSecretbeachVoyeur01Dialogue = CreateNewDialogue("FrimaDialogueSecretbeach01", Places.secretBeachRoomtalk.transform);
                    frimaSecretbeachVoyeur01DialogueScene1 = frimaSecretbeachVoyeur01Dialogue.transform.Find("Scene1").gameObject;
                    frimaSecretbeachVoyeur01DialogueScene2 = frimaSecretbeachVoyeur01Dialogue.transform.Find("Scene2").gameObject;
                    frimaSecretbeachVoyeur01DialogueScene3 = frimaSecretbeachVoyeur01Dialogue.transform.Find("Scene3").gameObject;
                    frimaSecretbeachVoyeur01DialogueScene4 = frimaSecretbeachVoyeur01Dialogue.transform.Find("Scene4").gameObject;
                    frimaSecretbeachVoyeur01DialogueScene5 = frimaSecretbeachVoyeur01Dialogue.transform.Find("Scene5").gameObject;
                    frimaSecretbeachVoyeur01DialogueActivator = frimaSecretbeachVoyeur01Dialogue.transform.Find("DialogueActivator").gameObject;
                    frimaSecretbeachVoyeur01DialogueFinisher = frimaSecretbeachVoyeur01Dialogue.transform.Find("DialogueFinisher").gameObject;
                    frimaSecretbeachVoyeur01DialogueMouthActivator = frimaSecretbeachVoyeur01Dialogue.transform.Find("MouthActivator").gameObject;
                    frimaSecretbeachVoyeur01DialogueSpriteFocus = frimaSecretbeachVoyeur01Dialogue.transform.Find("SpriteFocus").gameObject;

                    guiltySecretbeachVoyeur01Dialogue = CreateNewDialogue("GuiltyDialogueSecretbeach01", Places.secretBeachRoomtalk.transform);
                    guiltySecretbeachVoyeur01DialogueScene1 = guiltySecretbeachVoyeur01Dialogue.transform.Find("Scene1").gameObject;
                    guiltySecretbeachVoyeur01DialogueScene2 = guiltySecretbeachVoyeur01Dialogue.transform.Find("Scene2").gameObject;
                    guiltySecretbeachVoyeur01DialogueScene3 = guiltySecretbeachVoyeur01Dialogue.transform.Find("Scene3").gameObject;
                    guiltySecretbeachVoyeur01DialogueScene4 = guiltySecretbeachVoyeur01Dialogue.transform.Find("Scene4").gameObject;
                    guiltySecretbeachVoyeur01DialogueScene5 = guiltySecretbeachVoyeur01Dialogue.transform.Find("Scene5").gameObject;
                    guiltySecretbeachVoyeur01DialogueActivator = guiltySecretbeachVoyeur01Dialogue.transform.Find("DialogueActivator").gameObject;
                    guiltySecretbeachVoyeur01DialogueFinisher = guiltySecretbeachVoyeur01Dialogue.transform.Find("DialogueFinisher").gameObject;
                    guiltySecretbeachVoyeur01DialogueMouthActivator = guiltySecretbeachVoyeur01Dialogue.transform.Find("MouthActivator").gameObject;
                    guiltySecretbeachVoyeur01DialogueSpriteFocus = guiltySecretbeachVoyeur01Dialogue.transform.Find("SpriteFocus").gameObject;

                    helmSecretbeachVoyeur01Dialogue = CreateNewDialogue("HelmDialogueSecretbeach01", Places.secretBeachRoomtalk.transform);
                    helmSecretbeachVoyeur01DialogueScene1 = helmSecretbeachVoyeur01Dialogue.transform.Find("Scene1").gameObject;
                    helmSecretbeachVoyeur01DialogueScene2 = helmSecretbeachVoyeur01Dialogue.transform.Find("Scene2").gameObject;
                    helmSecretbeachVoyeur01DialogueScene3 = helmSecretbeachVoyeur01Dialogue.transform.Find("Scene3").gameObject;
                    helmSecretbeachVoyeur01DialogueScene4 = helmSecretbeachVoyeur01Dialogue.transform.Find("Scene4").gameObject;
                    helmSecretbeachVoyeur01DialogueScene5 = helmSecretbeachVoyeur01Dialogue.transform.Find("Scene5").gameObject;
                    helmSecretbeachVoyeur01DialogueActivator = helmSecretbeachVoyeur01Dialogue.transform.Find("DialogueActivator").gameObject;
                    helmSecretbeachVoyeur01DialogueFinisher = helmSecretbeachVoyeur01Dialogue.transform.Find("DialogueFinisher").gameObject;
                    helmSecretbeachVoyeur01DialogueMouthActivator = helmSecretbeachVoyeur01Dialogue.transform.Find("MouthActivator").gameObject;
                    helmSecretbeachVoyeur01DialogueSpriteFocus = helmSecretbeachVoyeur01Dialogue.transform.Find("SpriteFocus").gameObject;

                    maidenSecretbeachVoyeur01Dialogue = CreateNewDialogue("MaidenDialogueSecretbeach01", Places.secretBeachRoomtalk.transform);
                    maidenSecretbeachVoyeur01DialogueScene1 = maidenSecretbeachVoyeur01Dialogue.transform.Find("Scene1").gameObject;
                    maidenSecretbeachVoyeur01DialogueScene2 = maidenSecretbeachVoyeur01Dialogue.transform.Find("Scene2").gameObject;
                    maidenSecretbeachVoyeur01DialogueScene3 = maidenSecretbeachVoyeur01Dialogue.transform.Find("Scene3").gameObject;
                    maidenSecretbeachVoyeur01DialogueScene4 = maidenSecretbeachVoyeur01Dialogue.transform.Find("Scene4").gameObject;
                    maidenSecretbeachVoyeur01DialogueScene5 = maidenSecretbeachVoyeur01Dialogue.transform.Find("Scene5").gameObject;
                    maidenSecretbeachVoyeur01DialogueActivator = maidenSecretbeachVoyeur01Dialogue.transform.Find("DialogueActivator").gameObject;
                    maidenSecretbeachVoyeur01DialogueFinisher = maidenSecretbeachVoyeur01Dialogue.transform.Find("DialogueFinisher").gameObject;
                    maidenSecretbeachVoyeur01DialogueMouthActivator = maidenSecretbeachVoyeur01Dialogue.transform.Find("MouthActivator").gameObject;
                    maidenSecretbeachVoyeur01DialogueSpriteFocus = maidenSecretbeachVoyeur01Dialogue.transform.Find("SpriteFocus").gameObject;

                    marySecretbeachVoyeur01Dialogue = CreateNewDialogue("MaryDialogueSecretbeach01", Places.secretBeachRoomtalk.transform);
                    marySecretbeachVoyeur01DialogueScene1 = marySecretbeachVoyeur01Dialogue.transform.Find("Scene1").gameObject;
                    marySecretbeachVoyeur01DialogueScene2 = marySecretbeachVoyeur01Dialogue.transform.Find("Scene2").gameObject;
                    marySecretbeachVoyeur01DialogueScene3 = marySecretbeachVoyeur01Dialogue.transform.Find("Scene3").gameObject;
                    marySecretbeachVoyeur01DialogueScene4 = marySecretbeachVoyeur01Dialogue.transform.Find("Scene4").gameObject;
                    marySecretbeachVoyeur01DialogueScene5 = marySecretbeachVoyeur01Dialogue.transform.Find("Scene5").gameObject;
                    marySecretbeachVoyeur01DialogueActivator = marySecretbeachVoyeur01Dialogue.transform.Find("DialogueActivator").gameObject;
                    marySecretbeachVoyeur01DialogueFinisher = marySecretbeachVoyeur01Dialogue.transform.Find("DialogueFinisher").gameObject;
                    marySecretbeachVoyeur01DialogueMouthActivator = marySecretbeachVoyeur01Dialogue.transform.Find("MouthActivator").gameObject;
                    marySecretbeachVoyeur01DialogueSpriteFocus = marySecretbeachVoyeur01Dialogue.transform.Find("SpriteFocus").gameObject;

                    mastSecretbeachVoyeur01Dialogue = CreateNewDialogue("MastDialogueSecretbeach01", Places.secretBeachRoomtalk.transform);
                    mastSecretbeachVoyeur01DialogueScene1 = mastSecretbeachVoyeur01Dialogue.transform.Find("Scene1").gameObject;
                    mastSecretbeachVoyeur01DialogueScene2 = mastSecretbeachVoyeur01Dialogue.transform.Find("Scene2").gameObject;
                    mastSecretbeachVoyeur01DialogueScene3 = mastSecretbeachVoyeur01Dialogue.transform.Find("Scene3").gameObject;
                    mastSecretbeachVoyeur01DialogueScene4 = mastSecretbeachVoyeur01Dialogue.transform.Find("Scene4").gameObject;
                    mastSecretbeachVoyeur01DialogueScene5 = mastSecretbeachVoyeur01Dialogue.transform.Find("Scene5").gameObject;
                    mastSecretbeachVoyeur01DialogueActivator = mastSecretbeachVoyeur01Dialogue.transform.Find("DialogueActivator").gameObject;
                    mastSecretbeachVoyeur01DialogueFinisher = mastSecretbeachVoyeur01Dialogue.transform.Find("DialogueFinisher").gameObject;
                    mastSecretbeachVoyeur01DialogueMouthActivator = mastSecretbeachVoyeur01Dialogue.transform.Find("MouthActivator").gameObject;
                    mastSecretbeachVoyeur01DialogueSpriteFocus = mastSecretbeachVoyeur01Dialogue.transform.Find("SpriteFocus").gameObject;

                    neonSecretbeachVoyeur01Dialogue = CreateNewDialogue("NeonDialogueSecretbeach01", Places.secretBeachRoomtalk.transform);
                    neonSecretbeachVoyeur01DialogueScene1 = neonSecretbeachVoyeur01Dialogue.transform.Find("Scene1").gameObject;
                    neonSecretbeachVoyeur01DialogueScene2 = neonSecretbeachVoyeur01Dialogue.transform.Find("Scene2").gameObject;
                    neonSecretbeachVoyeur01DialogueScene3 = neonSecretbeachVoyeur01Dialogue.transform.Find("Scene3").gameObject;
                    neonSecretbeachVoyeur01DialogueScene4 = neonSecretbeachVoyeur01Dialogue.transform.Find("Scene4").gameObject;
                    neonSecretbeachVoyeur01DialogueScene5 = neonSecretbeachVoyeur01Dialogue.transform.Find("Scene5").gameObject;
                    neonSecretbeachVoyeur01DialogueActivator = neonSecretbeachVoyeur01Dialogue.transform.Find("DialogueActivator").gameObject;
                    neonSecretbeachVoyeur01DialogueFinisher = neonSecretbeachVoyeur01Dialogue.transform.Find("DialogueFinisher").gameObject;
                    neonSecretbeachVoyeur01DialogueMouthActivator = neonSecretbeachVoyeur01Dialogue.transform.Find("MouthActivator").gameObject;
                    neonSecretbeachVoyeur01DialogueSpriteFocus = neonSecretbeachVoyeur01Dialogue.transform.Find("SpriteFocus").gameObject;

                    pepperSecretbeachVoyeur01Dialogue = CreateNewDialogue("PepperDialogueSecretbeach01", Places.secretBeachRoomtalk.transform);
                    pepperSecretbeachVoyeur01DialogueScene1 = pepperSecretbeachVoyeur01Dialogue.transform.Find("Scene1").gameObject;
                    pepperSecretbeachVoyeur01DialogueScene2 = pepperSecretbeachVoyeur01Dialogue.transform.Find("Scene2").gameObject;
                    pepperSecretbeachVoyeur01DialogueScene3 = pepperSecretbeachVoyeur01Dialogue.transform.Find("Scene3").gameObject;
                    pepperSecretbeachVoyeur01DialogueScene4 = pepperSecretbeachVoyeur01Dialogue.transform.Find("Scene4").gameObject;
                    pepperSecretbeachVoyeur01DialogueScene5 = pepperSecretbeachVoyeur01Dialogue.transform.Find("Scene5").gameObject;
                    pepperSecretbeachVoyeur01DialogueActivator = pepperSecretbeachVoyeur01Dialogue.transform.Find("DialogueActivator").gameObject;
                    pepperSecretbeachVoyeur01DialogueFinisher = pepperSecretbeachVoyeur01Dialogue.transform.Find("DialogueFinisher").gameObject;
                    pepperSecretbeachVoyeur01DialogueMouthActivator = pepperSecretbeachVoyeur01Dialogue.transform.Find("MouthActivator").gameObject;
                    pepperSecretbeachVoyeur01DialogueSpriteFocus = pepperSecretbeachVoyeur01Dialogue.transform.Find("SpriteFocus").gameObject;

                    rapiSecretbeachVoyeur01Dialogue = CreateNewDialogue("RapiDialogueSecretbeach01", Places.secretBeachRoomtalk.transform);
                    rapiSecretbeachVoyeur01DialogueScene1 = rapiSecretbeachVoyeur01Dialogue.transform.Find("Scene1").gameObject;
                    rapiSecretbeachVoyeur01DialogueScene2 = rapiSecretbeachVoyeur01Dialogue.transform.Find("Scene2").gameObject;
                    rapiSecretbeachVoyeur01DialogueScene3 = rapiSecretbeachVoyeur01Dialogue.transform.Find("Scene3").gameObject;
                    rapiSecretbeachVoyeur01DialogueScene4 = rapiSecretbeachVoyeur01Dialogue.transform.Find("Scene4").gameObject;
                    rapiSecretbeachVoyeur01DialogueScene5 = rapiSecretbeachVoyeur01Dialogue.transform.Find("Scene5").gameObject;
                    rapiSecretbeachVoyeur01DialogueActivator = rapiSecretbeachVoyeur01Dialogue.transform.Find("DialogueActivator").gameObject;
                    rapiSecretbeachVoyeur01DialogueFinisher = rapiSecretbeachVoyeur01Dialogue.transform.Find("DialogueFinisher").gameObject;
                    rapiSecretbeachVoyeur01DialogueMouthActivator = rapiSecretbeachVoyeur01Dialogue.transform.Find("MouthActivator").gameObject;
                    rapiSecretbeachVoyeur01DialogueSpriteFocus = rapiSecretbeachVoyeur01Dialogue.transform.Find("SpriteFocus").gameObject;

                    rosannaSecretbeachVoyeur01Dialogue = CreateNewDialogue("RosannaDialogueSecretbeach01", Places.secretBeachRoomtalk.transform);
                    rosannaSecretbeachVoyeur01DialogueScene1 = rosannaSecretbeachVoyeur01Dialogue.transform.Find("Scene1").gameObject;
                    rosannaSecretbeachVoyeur01DialogueScene2 = rosannaSecretbeachVoyeur01Dialogue.transform.Find("Scene2").gameObject;
                    rosannaSecretbeachVoyeur01DialogueScene3 = rosannaSecretbeachVoyeur01Dialogue.transform.Find("Scene3").gameObject;
                    rosannaSecretbeachVoyeur01DialogueScene4 = rosannaSecretbeachVoyeur01Dialogue.transform.Find("Scene4").gameObject;
                    rosannaSecretbeachVoyeur01DialogueScene5 = rosannaSecretbeachVoyeur01Dialogue.transform.Find("Scene5").gameObject;
                    rosannaSecretbeachVoyeur01DialogueActivator = rosannaSecretbeachVoyeur01Dialogue.transform.Find("DialogueActivator").gameObject;
                    rosannaSecretbeachVoyeur01DialogueFinisher = rosannaSecretbeachVoyeur01Dialogue.transform.Find("DialogueFinisher").gameObject;
                    rosannaSecretbeachVoyeur01DialogueMouthActivator = rosannaSecretbeachVoyeur01Dialogue.transform.Find("MouthActivator").gameObject;
                    rosannaSecretbeachVoyeur01DialogueSpriteFocus = rosannaSecretbeachVoyeur01Dialogue.transform.Find("SpriteFocus").gameObject;

                    sakuraSecretbeachVoyeur01Dialogue = CreateNewDialogue("SakuraDialogueSecretbeach01", Places.secretBeachRoomtalk.transform);
                    sakuraSecretbeachVoyeur01DialogueScene1 = sakuraSecretbeachVoyeur01Dialogue.transform.Find("Scene1").gameObject;
                    sakuraSecretbeachVoyeur01DialogueScene2 = sakuraSecretbeachVoyeur01Dialogue.transform.Find("Scene2").gameObject;
                    sakuraSecretbeachVoyeur01DialogueScene3 = sakuraSecretbeachVoyeur01Dialogue.transform.Find("Scene3").gameObject;
                    sakuraSecretbeachVoyeur01DialogueScene4 = sakuraSecretbeachVoyeur01Dialogue.transform.Find("Scene4").gameObject;
                    sakuraSecretbeachVoyeur01DialogueScene5 = sakuraSecretbeachVoyeur01Dialogue.transform.Find("Scene5").gameObject;
                    sakuraSecretbeachVoyeur01DialogueActivator = sakuraSecretbeachVoyeur01Dialogue.transform.Find("DialogueActivator").gameObject;
                    sakuraSecretbeachVoyeur01DialogueFinisher = sakuraSecretbeachVoyeur01Dialogue.transform.Find("DialogueFinisher").gameObject;
                    sakuraSecretbeachVoyeur01DialogueMouthActivator = sakuraSecretbeachVoyeur01Dialogue.transform.Find("MouthActivator").gameObject;
                    sakuraSecretbeachVoyeur01DialogueSpriteFocus = sakuraSecretbeachVoyeur01Dialogue.transform.Find("SpriteFocus").gameObject;

                    viperSecretbeachVoyeur01Dialogue = CreateNewDialogue("ViperDialogueSecretbeach01", Places.secretBeachRoomtalk.transform);
                    viperSecretbeachVoyeur01DialogueScene1 = viperSecretbeachVoyeur01Dialogue.transform.Find("Scene1").gameObject;
                    viperSecretbeachVoyeur01DialogueScene2 = viperSecretbeachVoyeur01Dialogue.transform.Find("Scene2").gameObject;
                    viperSecretbeachVoyeur01DialogueScene3 = viperSecretbeachVoyeur01Dialogue.transform.Find("Scene3").gameObject;
                    viperSecretbeachVoyeur01DialogueScene4 = viperSecretbeachVoyeur01Dialogue.transform.Find("Scene4").gameObject;
                    viperSecretbeachVoyeur01DialogueScene5 = viperSecretbeachVoyeur01Dialogue.transform.Find("Scene5").gameObject;
                    viperSecretbeachVoyeur01DialogueActivator = viperSecretbeachVoyeur01Dialogue.transform.Find("DialogueActivator").gameObject;
                    viperSecretbeachVoyeur01DialogueFinisher = viperSecretbeachVoyeur01Dialogue.transform.Find("DialogueFinisher").gameObject;
                    viperSecretbeachVoyeur01DialogueMouthActivator = viperSecretbeachVoyeur01Dialogue.transform.Find("MouthActivator").gameObject;
                    viperSecretbeachVoyeur01DialogueSpriteFocus = viperSecretbeachVoyeur01Dialogue.transform.Find("SpriteFocus").gameObject;

                    yanSecretbeachVoyeur01Dialogue = CreateNewDialogue("YanDialogueSecretbeach01", Places.secretBeachRoomtalk.transform);
                    yanSecretbeachVoyeur01DialogueScene1 = yanSecretbeachVoyeur01Dialogue.transform.Find("Scene1").gameObject;
                    yanSecretbeachVoyeur01DialogueScene2 = yanSecretbeachVoyeur01Dialogue.transform.Find("Scene2").gameObject;
                    yanSecretbeachVoyeur01DialogueScene3 = yanSecretbeachVoyeur01Dialogue.transform.Find("Scene3").gameObject;
                    yanSecretbeachVoyeur01DialogueScene4 = yanSecretbeachVoyeur01Dialogue.transform.Find("Scene4").gameObject;
                    yanSecretbeachVoyeur01DialogueScene5 = yanSecretbeachVoyeur01Dialogue.transform.Find("Scene5").gameObject;
                    yanSecretbeachVoyeur01DialogueActivator = yanSecretbeachVoyeur01Dialogue.transform.Find("DialogueActivator").gameObject;
                    yanSecretbeachVoyeur01DialogueFinisher = yanSecretbeachVoyeur01Dialogue.transform.Find("DialogueFinisher").gameObject;
                    yanSecretbeachVoyeur01DialogueMouthActivator = yanSecretbeachVoyeur01Dialogue.transform.Find("MouthActivator").gameObject;
                    yanSecretbeachVoyeur01DialogueSpriteFocus = yanSecretbeachVoyeur01Dialogue.transform.Find("SpriteFocus").gameObject;
                    #endregion

                    snekForestEgg01Dialogue = CreateNewDialogue("SnekForest", Places.roomTalkForest.transform);
                    snekForestEgg01DialogueScene1 = snekForestEgg01Dialogue.transform.Find("Scene1").gameObject;
                    snekForestEgg01DialogueActivator = snekForestEgg01Dialogue.transform.Find("DialogueActivator").gameObject;
                    snekForestEgg01DialogueFinisher = snekForestEgg01Dialogue.transform.Find("DialogueFinisher").gameObject;

                    SetActorOverrideSpeechSkinValue(eleggDowntownEvent01Dialogue, "AdrianActor", overrideSpeechSkinBlue);
                    SetActorOverrideSpeechSkinValue(sBDialogueStory01, "AmberActor", overrideSpeechSkinYellow);
                    SetActorOverrideSpeechSkinValue(sBDialogueMainFirst, "PlayerActor", overrideSpeechSkinGreen);

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

            // ROOMTALK MONITORING
            if (loadedDialogues)
            {
                if (!dialoguePlayingVanillaInvokeRepeatingRunning)
                {
                    InvokeRepeating("MonitorRoomTalkChildren", 0f, 0.5f);
                    dialoguePlayingVanillaInvokeRepeatingRunning = true;
                }
            }
            else
            {
                if (dialoguePlayingVanillaInvokeRepeatingRunning)
                {
                    CancelInvoke("MonitorRoomTalkChildren");
                    dialoguePlayingVanillaInvokeRepeatingRunning = false;
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
            
            // Add to the tracking list
            // allDialogues.Add(dialogueInstance); // This line is removed as per the edit hint
            
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
                Debug.LogError("[AddExpressionSetInstructionToOnStart] Dialogue.Story.Content is null");
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

        #region Dialogue Text Processing

        /// <summary>
        /// Processes text by replacing variable placeholders with actual values.
        /// Supports [GV:VariableName] for Global Variables and [SM:VariableName] for Save Manager variables.
        /// </summary>
        /// <param name="text">The text to process</param>
        /// <returns>The processed text with variable values substituted</returns>
        public static string ProcessTextWithVariables(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;

            string result = text;

            // Process Global Variables [GV:VariableName]
            result = ProcessGlobalVariables(result);

            // Process Save Manager Variables [SM:VariableName]
            result = ProcessSaveManagerVariables(result);

            return result;
        }

        /// <summary>
        /// Processes Global Variables in the format [GV:VariableName]
        /// </summary>
        /// <param name="text">The text to process</param>
        /// <returns>The text with Global Variables replaced</returns>
        private static string ProcessGlobalVariables(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;

            string result = text;
            var regex = new System.Text.RegularExpressions.Regex(@"\[GV:([^\]]+)\]");
            var matches = regex.Matches(result);

            foreach (System.Text.RegularExpressions.Match match in matches)
            {
                string variableName = match.Groups[1].Value;
                string replacement = GetGlobalVariableValue(variableName);
                result = result.Replace(match.Value, replacement);
            }

            return result;
        }

        /// <summary>
        /// Processes Save Manager Variables in the format [SM:VariableName]
        /// </summary>
        /// <param name="text">The text to process</param>
        /// <returns>The text with Save Manager Variables replaced</returns>
        private static string ProcessSaveManagerVariables(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;

            string result = text;
            var regex = new System.Text.RegularExpressions.Regex(@"\[SM:([^\]]+)\]");
            var matches = regex.Matches(result);

            foreach (System.Text.RegularExpressions.Match match in matches)
            {
                string variableName = match.Groups[1].Value;
                string replacement = GetSaveManagerVariableValue(variableName);
                result = result.Replace(match.Value, replacement);
            }

            return result;
        }

        /// <summary>
        /// Gets the value of a Global Variable as a string
        /// </summary>
        /// <param name="variableName">The name of the Global Variable</param>
        /// <returns>The variable value as a string, or the variable name if not found</returns>
        private static string GetGlobalVariableValue(string variableName)
        {
            try
            {
                var manager = GlobalNameVariablesManager.Instance;
                if (manager == null)
                {
                    Debug.LogWarning($"[GetGlobalVariableValue] GlobalNameVariablesManager not initialized for variable: {variableName}");
                    return variableName;
                }

                // Access private Values dictionary
                PropertyInfo valuesProp = typeof(GlobalNameVariablesManager).GetProperty("Values", BindingFlags.NonPublic | BindingFlags.Instance);
                var values = valuesProp.GetValue(manager) as Dictionary<IdString, NameVariableRuntime>;
                if (values == null) return variableName;

                foreach (var pair in values)
                {
                    // Access the runtime's Variables dictionary
                    PropertyInfo varsProp = typeof(NameVariableRuntime).GetProperty("Variables", BindingFlags.NonPublic | BindingFlags.Instance);
                    var variables = varsProp.GetValue(pair.Value) as Dictionary<string, NameVariable>;
                    if (variables == null) continue;

                    if (variables.TryGetValue(variableName, out var nameVar))
                    {
                        object value = nameVar.Value;
                        return ConvertValueToString(value);
                    }
                }

                Debug.LogWarning($"[GetGlobalVariableValue] Variable '{variableName}' not found in any global variable set");
                return variableName;
            }
            catch (Exception e)
            {
                Debug.LogError($"[GetGlobalVariableValue] Error getting global variable '{variableName}': {e.Message}");
                return variableName;
            }
        }

        /// <summary>
        /// Gets the value of a Save Manager Variable as a string
        /// </summary>
        /// <param name="variableName">The name of the Save Manager Variable</param>
        /// <returns>The variable value as a string, or the variable name if not found</returns>
        private static string GetSaveManagerVariableValue(string variableName)
        {
            try
            {
                if (SaveManager.HasVariable(variableName))
                {
                    // Try to get the value based on its type
                    if (SaveManager.GetBool(variableName) != SaveManager.GetBool(variableName, !SaveManager.GetBool(variableName)))
                    {
                        // It's a bool variable
                        return SaveManager.GetBool(variableName).ToString();
                    }
                    else if (SaveManager.GetInt(variableName) != 0 || SaveManager.GetInt(variableName, 1) != 1)
                    {
                        // It's an int variable
                        return SaveManager.GetInt(variableName).ToString();
                    }
                    else if (SaveManager.GetFloat(variableName) != 0f || SaveManager.GetFloat(variableName, 1f) != 1f)
                    {
                        // It's a float variable
                        return SaveManager.GetFloat(variableName).ToString("F1");
                    }
                    else
                    {
                        // It's a string variable
                        return SaveManager.GetString(variableName);
                    }
                }

                Debug.LogWarning($"[GetSaveManagerVariableValue] Variable '{variableName}' not found in Save Manager");
                return variableName;
            }
            catch (Exception e)
            {
                Debug.LogError($"[GetSaveManagerVariableValue] Error getting Save Manager variable '{variableName}': {e.Message}");
                return variableName;
            }
        }

        /// <summary>
        /// Converts any value to a string representation
        /// </summary>
        /// <param name="value">The value to convert</param>
        /// <returns>The string representation of the value</returns>
        private static string ConvertValueToString(object value)
        {
            if (value == null) return "null";

            try
            {
                if (value is bool boolValue)
                {
                    return boolValue.ToString();
                }
                else if (value is int intValue)
                {
                    return intValue.ToString();
                }
                else if (value is float floatValue)
                {
                    return floatValue.ToString("F1");
                }
                else if (value is double doubleValue)
                {
                    return doubleValue.ToString("F1");
                }
                else if (value is string stringValue)
                {
                    return stringValue;
                }
                else
                {
                    return value.ToString();
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[ConvertValueToString] Error converting value to string: {e.Message}");
                return value.ToString();
            }
        }



        #endregion
    }
}
