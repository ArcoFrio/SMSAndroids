﻿using BepInEx;
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
using static System.Net.Mime.MediaTypeNames;
using static UnityEngine.GraphicsBuffer;

namespace SMSAndroidsCore
{
    [BepInPlugin(pluginGuid, Core.pluginName + " - Story", Core.pluginVersion)]
    internal class MainStory : BaseUnityPlugin
    {
        #region Plugin Info
        public const string pluginGuid = "treboy.starmakerstory.smsandroidscore.mainstory";
        #endregion

        public static bool loadedStory = false;
        public static SignalArgs fadeUISignal = new SignalArgs(new PropertyName("FadeUI"), null);
        public static bool actionTodaySB = false;
        public static bool voyeurDialoguePlaying = false;
        public static bool relaxed = false;
        public static GameObject currentActiveBust;
        public static GameObject currentActiveBustMBase;
        public static GameObject currentActiveDialogue;
        public static GameObject currentActiveDialogueSpriteFocus;
        public static int generalLotteryNumber1 = 0;
        public static int generalLotteryNumber2 = 0;
        public static int generalLotteryNumber3 = 0;
        public static int voyeurLotteryNumber = 0;
        private string currentVoyeurTarget;
        public static GameObject lastEvaluatedLevel;
        public static bool evaluatingLevelDialogue = false;
        public static bool snekIsSolid = false;

        public static string[] starterVoyeurTargets = { "Anis", "Neon", "Rapi" };
        public static string[] fullVoyeurTargets = { "Anis", "Centi", "Dorothy", "Elegg", "Frima", "Guilty", "Helm", "Maiden", "Mary", "Mast", "Neon", "Pepper", "Rapi", "Rosanna", "Sakura", "Viper", "Yan" };
        public static List<GameObject> diagBusts = new List<GameObject>();
        public static List<string> voyeurTargetsLeft = new List<string>();

        // Helper to check if all starter targets have been found
        public static bool AllStarterVoyeurTargetsFound()
        {
            foreach (string character in starterVoyeurTargets)
            {
                if (!SaveManager.GetBool($"Voyeur_Seen{character}"))
                    return false;
            }
            return true;
        }

        private GameObject dialogueToActivate;
        private GameObject tempNewCurrentRT;
        private Vector2 refVelocity = Vector2.zero;

        public void Update()
        {
            if (Core.currentScene.name == "CoreGameScene")
            {
                if (!loadedStory && Places.loadedPlaces && Characters.loadedBusts && Dialogues.loadedDialogues)
                {
                    voyeurTargetsLeft.Clear();
                    string[] currentTargets;

                    if (AllStarterVoyeurTargetsFound() && SaveManager.GetBool("MountainLab_GKExplanation"))
                    {
                        currentTargets = fullVoyeurTargets;
                    }
                    else
                    {
                        currentTargets = starterVoyeurTargets;
                    }

                    foreach (string character in currentTargets)
                    {
                        if (!SaveManager.GetBool($"Voyeur_Seen{character}"))
                        {
                            voyeurTargetsLeft.Add(character);
                        }
                    }
                    Logger.LogInfo("----- STORY LOADED -----");
                    loadedStory = true;
                    //Debugging.PrintConditionsAndTriggers(Dialogues.amberDefaultDialogue);
                }

                if (loadedStory)
                {
                    if (currentActiveBust != null && currentActiveDialogueSpriteFocus.activeSelf && currentActiveBustMBase.GetComponent<SpriteRenderer>().sortingOrder != 17) { SpriteFocusChange(true); }
                    if (currentActiveBust != null && !currentActiveDialogueSpriteFocus.activeSelf && currentActiveBustMBase.GetComponent<SpriteRenderer>().sortingOrder != 0) { SpriteFocusChange(false); }
                    if (SaveManager.GetBool("SecretBeach_GKSeen") != true && SaveManager.GetInt("SecretBeach_RelaxedAmount") > 2)
                    {
                        SaveManager.SetBool("SecretBeach_GKSeen", true);
                    }

                    if(lastEvaluatedLevel != null)
                    {
                        if(!lastEvaluatedLevel.activeSelf)
                        {
                            diagBusts.Clear();
                            lastEvaluatedLevel = null;
                            evaluatingLevelDialogue = false;
                            if (Places.levelPark.GetComponent<Conditions>().enabled == false || Places.levelPark.GetComponent<Trigger>().enabled == false)
                            {
                                Places.levelPark.GetComponent<Conditions>().enabled = true;
                                Places.levelPark.GetComponent<Trigger>().enabled = true;
                            }
                            if (Places.levelParkingLot.GetComponent<Conditions>().enabled == false || Places.levelParkingLot.GetComponent<Trigger>().enabled == false)
                            {
                                Places.levelParkingLot.GetComponent<Conditions>().enabled = true;
                                Places.levelParkingLot.GetComponent<Trigger>().enabled = true;
                            }
                            if (Places.levelBeach.GetComponent<Trigger>().enabled == false)
                            {
                                //Places.levelBeach.GetComponent<Conditions>().enabled = true;
                                Places.levelBeach.GetComponent<Trigger>().enabled = true;
                            }
                        }
                    }

                    #region Schedule
                    switch (Schedule.amberLocation)
                    {
                        case "Hospitalhallway":
                            if (SaveManager.GetBool("Event_SeenAmberHospitalHallway01"))
                            {
                                Schedule.amberLocation = "MountainLab";
                            }
                            else
                            {
                                if (!Dialogues.dialoguePlaying && !evaluatingLevelDialogue && Places.levelHospitalHallway.activeSelf)
                                {
                                    evaluatingLevelDialogue = true;
                                    lastEvaluatedLevel = Places.levelHospitalHallway;
                                    this.dialogueToActivate = Dialogues.amberHospitalhallwayEvent01Dialogue;
                                    diagBusts.Add(Characters.amberCoatless);
                                    diagBusts.Add(Characters.doctorFrost);
                                    Invoke(nameof(CheckAndStartVanillaDialogue), 0.6f);
                                }
                                if (this.dialogueToActivate != null && this.dialogueToActivate.name == "AmberDialogueHospitalhallway01")
                                {
                                    if (this.dialogueToActivate.transform.Find("Scene1").gameObject.activeSelf)
                                    {
                                        Scenes.amberEventHospitalhallwayScene01.SetActive(true);
                                        this.dialogueToActivate.transform.Find("Scene1").gameObject.SetActive(false);
                                    }
                                    if (this.dialogueToActivate.transform.Find("Scene2").gameObject.activeSelf)
                                    {
                                        Scenes.amberEventHospitalhallwayScene02.SetActive(true);
                                        this.dialogueToActivate.transform.Find("Scene2").gameObject.SetActive(false);
                                    }
                                    if (this.dialogueToActivate.transform.Find("DialogueFinisher").gameObject.activeSelf)
                                    {
                                        Invoke(nameof(EndDialogueSequenceVanilla), 1.0f);
                                        this.dialogueToActivate.transform.Find("DialogueFinisher").gameObject.SetActive(false);
                                        Characters.amberCoatless.transform.Find("MBase1").Find("Leave").gameObject.SetActive(true);
                                        Characters.doctorFrost.transform.Find("MBase1").Find("Leave").gameObject.SetActive(true);
                                        Scenes.amberEventHospitalhallwayScene01.SetActive(false);
                                        Scenes.amberEventHospitalhallwayScene02.SetActive(false);
                                        Schedule.amberLocation = "MountainLab";
                                        SaveManager.SetBool("Event_SeenAmberHospitalHallway01", true);
                                    }
                                }
                            }
                            break;

                        default:
                            if (!Dialogues.dialoguePlaying && Places.mountainLabRoomtalk.activeSelf && SaveManager.GetBool("MountainLab_FirstVisited") 
                                && !(!SaveManager.GetBool("MountainLab_FirstVisitor") && SaveManager.AnyBoolVariableWithNameContains("Voyeur_Seen"))
                                && !(SaveManager.GetBool("MountainLab_FirstVisitor") && !SaveManager.GetBool("MountainLab_GKExplanation") && AllStarterVoyeurTargetsFound()))
                            {
                                StartDialogueSequence(Dialogues.amberDefaultDialogue);
                                Characters.amber.SetActive(true);
                            }
                            if (Dialogues.amberDefaultDialogueDialogueFinisher.activeSelf)
                            {
                                Invoke(nameof(EndDialogueSequence), 1.0f);
                                Dialogues.amberDefaultDialogueDialogueFinisher.SetActive(false);
                            }
                            break;
                    }

                    switch (Schedule.anisLocation)
                    {
                        case "Mall":
                            if (SaveManager.GetBool("Event_SeenAnisMall01"))
                            {
                                Schedule.anisLocation = "MountainLabRoomNikkeAnis";
                            }
                            else
                            {
                                if (!Dialogues.dialoguePlaying && !evaluatingLevelDialogue && Places.levelMall.activeSelf && SaveManager.GetBool("MountainLab_GKExplanation"))
                                {
                                    evaluatingLevelDialogue = true;
                                    lastEvaluatedLevel = Places.levelMall;
                                    this.dialogueToActivate = Dialogues.anisMallEvent01Dialogue;
                                    diagBusts.Add(Characters.anis);
                                    Invoke(nameof(CheckAndStartVanillaDialogue), 0.6f);
                                }
                                if (this.dialogueToActivate != null && this.dialogueToActivate.name == "AnisDialogueMall01")
                                {
                                    if (this.dialogueToActivate.transform.Find("Scene1").gameObject.activeSelf)
                                    {
                                        Scenes.anisEventMallScene01.SetActive(true);
                                        this.dialogueToActivate.transform.Find("Scene1").gameObject.SetActive(false);
                                    }
                                    if (this.dialogueToActivate.transform.Find("Scene2").gameObject.activeSelf)
                                    {
                                        Characters.kate.SetActive(true);
                                        this.dialogueToActivate.transform.Find("Scene2").gameObject.SetActive(false);
                                    }
                                    if (this.dialogueToActivate.transform.Find("DialogueFinisher").gameObject.activeSelf)
                                    {
                                        Invoke(nameof(EndDialogueSequenceVanilla), 1.0f);
                                        this.dialogueToActivate.transform.Find("DialogueFinisher").gameObject.SetActive(false);
                                        Characters.anis.transform.Find("MBase1").Find("Leave").gameObject.SetActive(true);
                                        Characters.kate.transform.Find("D1Base").Find("Leave").gameObject.SetActive(true);
                                        Scenes.anisEventMallScene01.SetActive(false);
                                        Schedule.anisLocation = "MountainLabRoomNikkeAnis";
                                        SaveManager.SetBool("Event_SeenAnisMall01", true);
                                    }
                                }
                            }
                            break;

                        default:
                            if (!Dialogues.dialoguePlaying && Places.mountainLabRoomNikkeAnisRoomtalk.activeSelf && SaveManager.GetBool("Voyeur_SeenAnis"))
                            {
                                StartDialogueSequence(Dialogues.anisDefaultDialogue);
                                Characters.anis.SetActive(true);
                            }
                            if (Dialogues.anisDefaultDialogueDialogueFinisher.activeSelf)
                            {
                                Invoke(nameof(EndDialogueSequence), 1.0f);
                                Dialogues.anisDefaultDialogueDialogueFinisher.SetActive(false);
                            }
                            break;
                    }

                    switch (Schedule.centiLocation)
                    {
                        case "Kenshome":
                            if (SaveManager.GetBool("Event_SeenCentiKensHome01"))
                            {
                                Schedule.centiLocation = "MountainLabRoomNikkeCenti";
                            }
                            else
                            {
                                if (!Dialogues.dialoguePlaying && !evaluatingLevelDialogue && Places.levelKensHome.activeSelf && SaveManager.GetBool("Voyeur_SeenCenti"))
                                {
                                    evaluatingLevelDialogue = true;
                                    lastEvaluatedLevel = Places.levelKensHome;
                                    this.dialogueToActivate = Dialogues.centiKenshomeEvent01Dialogue;
                                    diagBusts.Add(Characters.samSwim);
                                    Invoke(nameof(CheckAndStartVanillaDialogue), 0.6f);
                                }
                                if (this.dialogueToActivate != null && this.dialogueToActivate.name == "CentiDialogueKenshome01")
                                {
                                    if (this.dialogueToActivate.transform.Find("Scene1").gameObject.activeSelf)
                                    {
                                        Scenes.centiEventKenshomeScene01.SetActive(true);
                                        this.dialogueToActivate.transform.Find("Scene1").gameObject.SetActive(false);
                                    }
                                    if (this.dialogueToActivate.transform.Find("Scene2").gameObject.activeSelf)
                                    {
                                        Characters.centi.SetActive(true);
                                        this.dialogueToActivate.transform.Find("Scene2").gameObject.SetActive(false);
                                    }
                                    if (this.dialogueToActivate.transform.Find("Scene3").gameObject.activeSelf)
                                    {
                                        Characters.centi.transform.Find("MBase1").Find("Leave").gameObject.SetActive(true);
                                        this.dialogueToActivate.transform.Find("Scene3").gameObject.SetActive(false);
                                    }
                                    if (this.dialogueToActivate.transform.Find("DialogueFinisher").gameObject.activeSelf)
                                    {
                                        Invoke(nameof(EndDialogueSequenceVanilla), 1.0f);
                                        this.dialogueToActivate.transform.Find("DialogueFinisher").gameObject.SetActive(false);
                                        Characters.samSwim.transform.Find("MBase1").Find("Leave").gameObject.SetActive(true);
                                        Scenes.centiEventKenshomeScene01.SetActive(false);
                                        Schedule.centiLocation = "MountainLabRoomNikkeCenti";
                                        SaveManager.SetBool("Event_SeenCentiKensHome01", true);
                                    }
                                }
                            }
                            break;
                        default:
                            if (!Dialogues.dialoguePlaying && Places.mountainLabRoomNikkeCentiRoomtalk.activeSelf && SaveManager.GetBool("MountainLab_GKExplanation"))
                            {
                                StartDialogueSequence(Dialogues.centiDefaultDialogue);
                                Characters.centi.SetActive(true);
                            }
                            if (Dialogues.centiDefaultDialogueDialogueFinisher.activeSelf)
                            {
                                Invoke(nameof(EndDialogueSequence), 1.0f);
                                Dialogues.centiDefaultDialogueDialogueFinisher.SetActive(false);
                            }
                            break;
                    }

                    switch (Schedule.dorothyLocation)
                    {
                        case "Park":
                            if (SaveManager.GetBool("Event_SeenDorothyPark01"))
                            {
                                Schedule.dorothyLocation = "MountainLabRoomNikkeDorothy";
                            }
                            else
                            {
                                if (SaveManager.GetBool("Voyeur_SeenDorothy"))
                                {
                                    Places.levelPark.GetComponent<Conditions>().enabled = false;
                                    Places.levelPark.GetComponent<Trigger>().enabled = false;
                                    if (!Dialogues.dialoguePlaying && !evaluatingLevelDialogue && Places.levelPark.activeSelf)
                                    {
                                        evaluatingLevelDialogue = true;
                                        lastEvaluatedLevel = Places.levelPark;
                                        this.dialogueToActivate = Dialogues.dorothyParkEvent01Dialogue;
                                        diagBusts.Add(Characters.dorothy);
                                        Invoke(nameof(CheckAndStartVanillaDialogue), 0.6f);
                                    }
                                    if (this.dialogueToActivate != null && this.dialogueToActivate.name == "DorothyDialoguePark01")
                                    {
                                        if (this.dialogueToActivate.transform.Find("Scene1").gameObject.activeSelf)
                                        {
                                            Scenes.dorothyEventParkScene01.SetActive(true);
                                            this.dialogueToActivate.transform.Find("Scene1").gameObject.SetActive(false);
                                        }
                                        if (this.dialogueToActivate.transform.Find("Scene2").gameObject.activeSelf)
                                        {
                                            Characters.river.SetActive(true);
                                            this.dialogueToActivate.transform.Find("Scene2").gameObject.SetActive(false);
                                        }
                                        if (this.dialogueToActivate.transform.Find("Scene3").gameObject.activeSelf)
                                        {
                                            Characters.river.transform.Find("D1Base").Find("Leave").gameObject.SetActive(true);
                                            this.dialogueToActivate.transform.Find("Scene3").gameObject.SetActive(false);
                                        }
                                        if (this.dialogueToActivate.transform.Find("DialogueFinisher").gameObject.activeSelf)
                                        {
                                            Invoke(nameof(EndDialogueSequenceVanilla), 1.0f);
                                            this.dialogueToActivate.transform.Find("DialogueFinisher").gameObject.SetActive(false);
                                            Characters.dorothy.transform.Find("MBase1").Find("Leave").gameObject.SetActive(true);
                                            Scenes.dorothyEventParkScene01.SetActive(false);
                                            Schedule.dorothyLocation = "MountainLabRoomNikkeDorothy";
                                            SaveManager.SetBool("Event_SeenDorothyPark01", true);
                                        }
                                    }
                                }
                            }
                            break;
                        default:
                            if (!Dialogues.dialoguePlaying && Places.mountainLabRoomNikkeDorothyRoomtalk.activeSelf && SaveManager.GetBool("MountainLab_GKExplanation"))
                            {
                                StartDialogueSequence(Dialogues.dorothyDefaultDialogue);
                                Characters.dorothy.SetActive(true);
                            }
                            if (Dialogues.dorothyDefaultDialogueDialogueFinisher.activeSelf)
                            {
                                Invoke(nameof(EndDialogueSequence), 1.0f);
                                Dialogues.dorothyDefaultDialogueDialogueFinisher.SetActive(false);
                            }
                            break;
                    }

                    switch (Schedule.eleggLocation)
                    {
                        case "Downtown":
                            if (SaveManager.GetBool("Event_SeenEleggDowntown01"))
                            {
                                Schedule.eleggLocation = "MountainLabRoomNikkeElegg";
                            }
                            else
                            {
                                if (!Dialogues.dialoguePlaying && !evaluatingLevelDialogue && Places.levelDowntown.activeSelf && SaveManager.GetBool("Voyeur_SeenElegg"))
                                {
                                    evaluatingLevelDialogue = true;
                                    lastEvaluatedLevel = Places.levelDowntown;
                                    this.dialogueToActivate = Dialogues.eleggDowntownEvent01Dialogue;
                                    Invoke(nameof(CheckAndStartVanillaDialogue), 0.6f);
                                }
                                if (this.dialogueToActivate != null && this.dialogueToActivate.name == "EleggDialogueDowntown01")
                                {
                                    if (this.dialogueToActivate.transform.Find("Scene1").gameObject.activeSelf)
                                    {
                                        Scenes.eleggEventDowntownScene01.SetActive(true);
                                        this.dialogueToActivate.transform.Find("Scene1").gameObject.SetActive(false);
                                    }
                                    if (this.dialogueToActivate.transform.Find("Scene2").gameObject.activeSelf)
                                    {
                                        Characters.elegg.SetActive(true);
                                        this.dialogueToActivate.transform.Find("Scene2").gameObject.SetActive(false);
                                    }
                                    if (this.dialogueToActivate.transform.Find("Scene3").gameObject.activeSelf)
                                    {
                                        Characters.elegg.transform.Find("MBase1").Find("Leave").gameObject.SetActive(true);
                                        this.dialogueToActivate.transform.Find("Scene3").gameObject.SetActive(false);
                                    }
                                    if (this.dialogueToActivate.transform.Find("Scene4").gameObject.activeSelf)
                                    {
                                        Characters.adrian.SetActive(true);
                                        this.dialogueToActivate.transform.Find("Scene4").gameObject.SetActive(false);
                                    }
                                    if (this.dialogueToActivate.transform.Find("DialogueFinisher").gameObject.activeSelf)
                                    {
                                        Invoke(nameof(EndDialogueSequenceVanilla), 1.0f);
                                        this.dialogueToActivate.transform.Find("DialogueFinisher").gameObject.SetActive(false);
                                        Characters.adrian.transform.Find("D1Base").Find("Leave").gameObject.SetActive(true);
                                        Scenes.eleggEventDowntownScene01.SetActive(false);
                                        Schedule.eleggLocation = "MountainLabRoomNikkeElegg";
                                        SaveManager.SetBool("Event_SeenEleggDowntown01", true);
                                    }
                                }
                            }
                            break;
                        default:
                            if (!Dialogues.dialoguePlaying && Places.mountainLabRoomNikkeEleggRoomtalk.activeSelf && SaveManager.GetBool("MountainLab_GKExplanation"))
                            {
                                StartDialogueSequence(Dialogues.eleggDefaultDialogue);
                                Characters.elegg.SetActive(true);
                            }
                            if (Dialogues.eleggDefaultDialogueDialogueFinisher.activeSelf)
                            {
                                Invoke(nameof(EndDialogueSequence), 1.0f);
                                Dialogues.eleggDefaultDialogueDialogueFinisher.SetActive(false);
                            }
                            break;
                    }

                    switch (Schedule.frimaLocation)
                    {
                        case "Hotel":
                            if (SaveManager.GetBool("Event_SeenFrimaHotel01"))
                            {
                                Schedule.frimaLocation = "MountainLabRoomNikkeFrima";
                            }
                            else
                            {
                                if (!Dialogues.dialoguePlaying && !evaluatingLevelDialogue && Places.levelHotel.activeSelf && SaveManager.GetBool("Voyeur_SeenFrima"))
                                {
                                    evaluatingLevelDialogue = true;
                                    lastEvaluatedLevel = Places.levelHotel;
                                    this.dialogueToActivate = Dialogues.frimaHotelEvent01Dialogue;
                                    diagBusts.Add(Characters.frima);
                                    diagBusts.Add(Characters.isabella);
                                    Invoke(nameof(CheckAndStartVanillaDialogue), 0.6f);
                                }
                                if (this.dialogueToActivate != null && this.dialogueToActivate.name == "FrimaDialogueHotel01")
                                {
                                    if (this.dialogueToActivate.transform.Find("Scene1").gameObject.activeSelf)
                                    {
                                        Scenes.frimaEventHotelScene01.SetActive(true);
                                        this.dialogueToActivate.transform.Find("Scene1").gameObject.SetActive(false);
                                    }
                                    if (this.dialogueToActivate.transform.Find("Scene2").gameObject.activeSelf)
                                    {
                                        Characters.frima.transform.Find("MBase1").Find("Leave").gameObject.SetActive(true);
                                        this.dialogueToActivate.transform.Find("Scene2").gameObject.SetActive(false);
                                    }
                                    if (this.dialogueToActivate.transform.Find("DialogueFinisher").gameObject.activeSelf)
                                    {
                                        Invoke(nameof(EndDialogueSequenceVanilla), 1.0f);
                                        this.dialogueToActivate.transform.Find("DialogueFinisher").gameObject.SetActive(false);
                                        Characters.isabella.transform.Find("D1Base").Find("Leave").gameObject.SetActive(true);
                                        Scenes.frimaEventHotelScene01.SetActive(false);
                                        Schedule.frimaLocation = "MountainLabRoomNikkeFrima";
                                        SaveManager.SetBool("Event_SeenFrimaHotel01", true);
                                    }
                                }
                            }
                            break;
                        default:
                            if (!Dialogues.dialoguePlaying && Places.mountainLabRoomNikkeFrimaRoomtalk.activeSelf && SaveManager.GetBool("Voyeur_SeenFrima"))
                            {
                                StartDialogueSequence(Dialogues.frimaDefaultDialogue);
                                Characters.frima.SetActive(true);
                            }
                            if (Dialogues.frimaDefaultDialogueDialogueFinisher.activeSelf)
                            {
                                Invoke(nameof(EndDialogueSequence), 1.0f);
                                Dialogues.frimaDefaultDialogueDialogueFinisher.SetActive(false);
                            }
                            break;
                    }

                    switch (Schedule.guiltyLocation)
                    {
                        case "Parkinglot":
                            if (SaveManager.GetBool("Event_SeenGuiltyParkingLot01"))
                            {
                                Schedule.guiltyLocation = "MountainLabRoomNikkeGuilty";
                            }
                            else
                            {
                                if (SaveManager.GetBool("Voyeur_SeenGuilty"))
                                {
                                    Places.levelParkingLot.GetComponent<Conditions>().enabled = false;
                                    Places.levelParkingLot.GetComponent<Trigger>().enabled = false;
                                    if (!Dialogues.dialoguePlaying && !evaluatingLevelDialogue && Places.levelParkingLot.activeSelf)
                                    {
                                        evaluatingLevelDialogue = true;
                                        lastEvaluatedLevel = Places.levelParkingLot;
                                        this.dialogueToActivate = Dialogues.guiltyParkinglotEvent01Dialogue;
                                        diagBusts.Add(Characters.guilty);
                                        Invoke(nameof(CheckAndStartVanillaDialogue), 0.6f);
                                    }
                                    if (this.dialogueToActivate != null && this.dialogueToActivate.name == "GuiltyDialogueParkinglot01")
                                    {
                                        if (this.dialogueToActivate.transform.Find("Scene1").gameObject.activeSelf)
                                        {
                                            Scenes.guiltyEventParkinglotScene01.SetActive(true);
                                            Characters.mobsterBlonde.SetActive(false);
                                            this.dialogueToActivate.transform.Find("Scene1").gameObject.SetActive(false);
                                        }
                                        if (this.dialogueToActivate.transform.Find("Scene2").gameObject.activeSelf)
                                        {
                                            Characters.mobsterBlonde.SetActive(true);
                                            this.dialogueToActivate.transform.Find("Scene2").gameObject.SetActive(false);
                                        }
                                        if (this.dialogueToActivate.transform.Find("DialogueFinisher").gameObject.activeSelf)
                                        {
                                            Invoke(nameof(EndDialogueSequenceVanilla), 1.0f);
                                            this.dialogueToActivate.transform.Find("DialogueFinisher").gameObject.SetActive(false);
                                            Characters.guilty.transform.Find("MBase1").Find("Leave").gameObject.SetActive(true);
                                            Scenes.guiltyEventParkinglotScene01.SetActive(false);
                                            Schedule.guiltyLocation = "MountainLabRoomNikkeGuilty";
                                            SaveManager.SetBool("Event_SeenGuiltyParkingLot01", true);
                                        }
                                    }
                                }
                            }
                            break;
                        default:
                            if (!Dialogues.dialoguePlaying && Places.mountainLabRoomNikkeGuiltyRoomtalk.activeSelf && SaveManager.GetBool("Voyeur_SeenGuilty"))
                            {
                                StartDialogueSequence(Dialogues.guiltyDefaultDialogue);
                                Characters.guilty.SetActive(true);
                            }
                            if (Dialogues.guiltyDefaultDialogueDialogueFinisher.activeSelf)
                            {
                                Invoke(nameof(EndDialogueSequence), 1.0f);
                                Dialogues.guiltyDefaultDialogueDialogueFinisher.SetActive(false);
                            }
                            break;
                    }

                    switch (Schedule.helmLocation)
                    {
                        case "Beach":
                            if (SaveManager.GetBool("Event_SeenHelmBeach01"))
                            {
                                Schedule.helmLocation = "MountainLabRoomNikkeHelm";
                            }
                            else
                            {
                                if (SaveManager.GetBool("Voyeur_SeenHelm"))
                                {
                                    //Places.levelBeach.GetComponent<Conditions>().enabled = false;
                                    Places.levelBeach.GetComponent<Trigger>().enabled = false;
                                    if (!Dialogues.dialoguePlaying && !evaluatingLevelDialogue && Places.levelBeach.activeSelf)
                                    {
                                        evaluatingLevelDialogue = true;
                                        lastEvaluatedLevel = Places.levelBeach;
                                        this.dialogueToActivate = Dialogues.helmBeachEvent01Dialogue;
                                        diagBusts.Add(Characters.helm);
                                        Invoke(nameof(CheckAndStartVanillaDialogue), 0.6f);
                                    }
                                    if (this.dialogueToActivate != null && this.dialogueToActivate.name == "HelmDialogueBeach01")
                                    {
                                        if (this.dialogueToActivate.transform.Find("Scene1").gameObject.activeSelf)
                                        {
                                            Scenes.helmEventBeachScene01.SetActive(true);
                                            this.dialogueToActivate.transform.Find("Scene1").gameObject.SetActive(false);
                                        }
                                        if (this.dialogueToActivate.transform.Find("Scene2").gameObject.activeSelf)
                                        {
                                            Characters.emmaSwim.SetActive(true);
                                            this.dialogueToActivate.transform.Find("Scene2").gameObject.SetActive(false);
                                        }
                                        if (this.dialogueToActivate.transform.Find("DialogueFinisher").gameObject.activeSelf)
                                        {
                                            Invoke(nameof(EndDialogueSequenceVanilla), 1.0f);
                                            this.dialogueToActivate.transform.Find("DialogueFinisher").gameObject.SetActive(false);
                                            Characters.helm.transform.Find("MBase1").Find("Leave").gameObject.SetActive(true);
                                            Characters.emmaSwim.transform.Find("D1Base").Find("Leave").gameObject.SetActive(true);
                                            Scenes.helmEventBeachScene01.SetActive(false);
                                            Schedule.helmLocation = "MountainLabRoomNikkeHelm";
                                            SaveManager.SetBool("Event_SeenHelmBeach01", true);
                                        }
                                    }
                                }
                            }
                            break;
                        default:
                            if (!Dialogues.dialoguePlaying && Places.mountainLabRoomNikkeHelmRoomtalk.activeSelf && SaveManager.GetBool("Voyeur_SeenHelm"))
                            {
                                StartDialogueSequence(Dialogues.helmDefaultDialogue);
                                Characters.helm.SetActive(true);
                            }
                            if (Dialogues.helmDefaultDialogueDialogueFinisher.activeSelf)
                            {
                                Invoke(nameof(EndDialogueSequence), 1.0f);
                                Dialogues.helmDefaultDialogueDialogueFinisher.SetActive(false);
                            }
                            break;
                    }

                    switch (Schedule.maidenLocation)
                    {
                        case "Alley":
                            if (SaveManager.GetBool("Event_SeenMaidenAlley01"))
                            {
                                Schedule.maidenLocation = "MountainLabRoomNikkeMaiden";
                            }
                            else
                            {
                                if (!Dialogues.dialoguePlaying && !evaluatingLevelDialogue && Places.levelAlley.activeSelf && SaveManager.GetBool("Voyeur_SeenMaiden"))
                                {
                                    evaluatingLevelDialogue = true;
                                    lastEvaluatedLevel = Places.levelAlley;
                                    this.dialogueToActivate = Dialogues.maidenAlleyEvent01Dialogue;
                                    diagBusts.Add(Characters.maiden);
                                    Invoke(nameof(CheckAndStartVanillaDialogue), 0.6f);
                                }
                                if (this.dialogueToActivate != null && this.dialogueToActivate.name == "MaidenDialogueAlley01")
                                {
                                    if (this.dialogueToActivate.transform.Find("Scene1").gameObject.activeSelf)
                                    {
                                        Scenes.maidenEventAlleyScene01.SetActive(true);
                                        this.dialogueToActivate.transform.Find("Scene1").gameObject.SetActive(false);
                                    }
                                    if (this.dialogueToActivate.transform.Find("Scene2").gameObject.activeSelf)
                                    {
                                        Characters.toni.SetActive(true);
                                        this.dialogueToActivate.transform.Find("Scene2").gameObject.SetActive(false);
                                    }
                                    if (this.dialogueToActivate.transform.Find("DialogueFinisher").gameObject.activeSelf)
                                    {
                                        Invoke(nameof(EndDialogueSequenceVanilla), 1.0f);
                                        this.dialogueToActivate.transform.Find("DialogueFinisher").gameObject.SetActive(false);
                                        Characters.maiden.transform.Find("MBase1").Find("Leave").gameObject.SetActive(true);
                                        Characters.toni.transform.Find("MBase1").Find("Leave").gameObject.SetActive(true);
                                        Scenes.maidenEventAlleyScene01.SetActive(false);
                                        Schedule.maidenLocation = "MountainLabRoomNikkeMaiden";
                                        SaveManager.SetBool("Event_SeenMaidenAlley01", true);
                                    }
                                }
                            }
                            break;
                        default:
                            if (!Dialogues.dialoguePlaying && Places.mountainLabRoomNikkeMaidenRoomtalk.activeSelf && SaveManager.GetBool("Voyeur_SeenMaiden"))
                            {
                                StartDialogueSequence(Dialogues.maidenDefaultDialogue);
                                Characters.maiden.SetActive(true);
                            }
                            if (Dialogues.maidenDefaultDialogueDialogueFinisher.activeSelf)
                            {
                                Invoke(nameof(EndDialogueSequence), 1.0f);
                                Dialogues.maidenDefaultDialogueDialogueFinisher.SetActive(false);
                            }
                            break;
                    }

                    switch (Schedule.maryLocation)
                    {
                        case "Hospitalhallway":
                            if (SaveManager.GetBool("Event_SeenMaryHospitalHallway01"))
                            {
                                Schedule.maryLocation = "MountainLabRoomNikkeMary";
                            }
                            else
                            {
                                if (!Dialogues.dialoguePlaying && !evaluatingLevelDialogue && Places.levelHospitalHallway.activeSelf && SaveManager.GetBool("Voyeur_SeenMary"))
                                {
                                    evaluatingLevelDialogue = true;
                                    lastEvaluatedLevel = Places.levelHospitalHallway;
                                    this.dialogueToActivate = Dialogues.maryHospitalhallwayEvent01Dialogue;
                                    diagBusts.Add(Characters.anna);
                                    diagBusts.Add(Characters.mary);
                                    Invoke(nameof(CheckAndStartVanillaDialogue), 0.6f);
                                }
                                if (this.dialogueToActivate != null && this.dialogueToActivate.name == "MaryDialogueHospitalhallway01")
                                {
                                    if (this.dialogueToActivate.transform.Find("Scene1").gameObject.activeSelf)
                                    {
                                        Scenes.maryEventHospitalhallwayScene01.SetActive(true);
                                        this.dialogueToActivate.transform.Find("Scene1").gameObject.SetActive(false);
                                    }
                                    if (this.dialogueToActivate.transform.Find("DialogueFinisher").gameObject.activeSelf)
                                    {
                                        Invoke(nameof(EndDialogueSequenceVanilla), 1.0f);
                                        this.dialogueToActivate.transform.Find("DialogueFinisher").gameObject.SetActive(false);
                                        Characters.anna.transform.Find("MBase1").Find("Leave").gameObject.SetActive(true);
                                        Characters.mary.transform.Find("MBase1").Find("Leave").gameObject.SetActive(true);
                                        Scenes.maryEventHospitalhallwayScene01.SetActive(false);
                                        Schedule.maryLocation = "MountainLabRoomNikkeMary";
                                        SaveManager.SetBool("Event_SeenMaryHospitalHallway01", true);
                                    }
                                }
                            }
                            break;
                        default:
                            if (!Dialogues.dialoguePlaying && Places.mountainLabRoomNikkeMaryRoomtalk.activeSelf && SaveManager.GetBool("Voyeur_SeenMary"))
                            {
                                StartDialogueSequence(Dialogues.maryDefaultDialogue);
                                Characters.mary.SetActive(true);
                            }
                            if (Dialogues.maryDefaultDialogueDialogueFinisher.activeSelf)
                            {
                                Invoke(nameof(EndDialogueSequence), 1.0f);
                                Dialogues.maryDefaultDialogueDialogueFinisher.SetActive(false);
                            }
                            break;
                    }

                    switch (Schedule.mastLocation)
                    {
                        case "Beach":
                            if (SaveManager.GetBool("Event_SeenMastBeach01"))
                            {
                                Schedule.mastLocation = "MountainLabRoomNikkeMast";
                            }
                            else
                            {
                                if (SaveManager.GetBool("Voyeur_SeenMast"))
                                {
                                    //Places.levelBeach.GetComponent<Conditions>().enabled = false;
                                    Places.levelBeach.GetComponent<Trigger>().enabled = false;
                                    if (!Dialogues.dialoguePlaying && !evaluatingLevelDialogue && Places.levelBeach.activeSelf)
                                    {
                                        evaluatingLevelDialogue = true;
                                        lastEvaluatedLevel = Places.levelBeach;
                                        this.dialogueToActivate = Dialogues.mastBeachEvent01Dialogue;
                                        diagBusts.Add(Characters.mast);
                                        Invoke(nameof(CheckAndStartVanillaDialogue), 0.6f);
                                    }
                                    if (this.dialogueToActivate != null && this.dialogueToActivate.name == "MastDialogueBeach01")
                                    {
                                        if (this.dialogueToActivate.transform.Find("Scene1").gameObject.activeSelf)
                                        {
                                            Scenes.mastEventBeachScene01.SetActive(true);
                                            this.dialogueToActivate.transform.Find("Scene1").gameObject.SetActive(false);
                                        }
                                        if (this.dialogueToActivate.transform.Find("Scene2").gameObject.activeSelf)
                                        {
                                            Characters.ameliaSwim.SetActive(true);
                                            this.dialogueToActivate.transform.Find("Scene2").gameObject.SetActive(false);
                                        }
                                        if (this.dialogueToActivate.transform.Find("Scene3").gameObject.activeSelf)
                                        {
                                            ChangeActiveBust(Characters.mast, Characters.mastShirtless);
                                            this.dialogueToActivate.transform.Find("Scene3").gameObject.SetActive(false);
                                        }
                                        if (this.dialogueToActivate.transform.Find("DialogueFinisher").gameObject.activeSelf)
                                        {
                                            Invoke(nameof(EndDialogueSequenceVanilla), 1.0f);
                                            this.dialogueToActivate.transform.Find("DialogueFinisher").gameObject.SetActive(false);
                                            Characters.ameliaSwim.transform.Find("MBase1").Find("Leave").gameObject.SetActive(true);
                                            Characters.mastShirtless.transform.Find("MBase1").Find("Leave").gameObject.SetActive(true);
                                            Scenes.mastEventBeachScene01.SetActive(false);
                                            Schedule.mastLocation = "MountainLabRoomNikkeMast";
                                            SaveManager.SetBool("Event_SeenMastBeach01", true);
                                        }
                                    }
                                }
                            }
                            break;
                        default:
                            if (!Dialogues.dialoguePlaying && Places.mountainLabRoomNikkeMastRoomtalk.activeSelf && SaveManager.GetBool("Voyeur_SeenMast"))
                            {
                                StartDialogueSequence(Dialogues.mastDefaultDialogue);
                                Characters.mast.SetActive(true);
                            }
                            if (Dialogues.mastDefaultDialogueDialogueFinisher.activeSelf)
                            {
                                Invoke(nameof(EndDialogueSequence), 1.0f);
                                Dialogues.mastDefaultDialogueDialogueFinisher.SetActive(false);
                            }
                            break;
                    }

                    switch (Schedule.neonLocation)
                    {
                        case "Temple":
                            if (SaveManager.GetBool("Event_SeenNeonTemple01"))
                            {
                                Schedule.neonLocation = "MountainLabRoomNikkeNeon";
                            }
                            else
                            {
                                if (!Dialogues.dialoguePlaying && !evaluatingLevelDialogue && Places.levelTemple.activeSelf && SaveManager.GetBool("MountainLab_GKExplanation"))
                                {
                                    evaluatingLevelDialogue = true;
                                    lastEvaluatedLevel = Places.levelTemple;
                                    this.dialogueToActivate = Dialogues.neonTempleEvent01Dialogue;
                                    diagBusts.Add(Characters.neon);
                                    Invoke(nameof(CheckAndStartVanillaDialogue), 0.6f);
                                }
                                if (this.dialogueToActivate != null && this.dialogueToActivate.name == "NeonDialogueTemple01")
                                {
                                    if (this.dialogueToActivate.transform.Find("Scene1").gameObject.activeSelf)
                                    {
                                        Scenes.neonEventTempleScene01.SetActive(true);
                                        this.dialogueToActivate.transform.Find("Scene1").gameObject.SetActive(false);
                                    }
                                    if (this.dialogueToActivate.transform.Find("Scene2").gameObject.activeSelf)
                                    {
                                        Characters.masterZhen.SetActive(true);
                                        this.dialogueToActivate.transform.Find("Scene2").gameObject.SetActive(false);
                                    }
                                    if (this.dialogueToActivate.transform.Find("DialogueFinisher").gameObject.activeSelf)
                                    {
                                        Invoke(nameof(EndDialogueSequenceVanilla), 1.0f);
                                        this.dialogueToActivate.transform.Find("DialogueFinisher").gameObject.SetActive(false);
                                        Characters.masterZhen.transform.Find("D1Base").Find("Leave").gameObject.SetActive(true);
                                        Characters.neon.transform.Find("MBase1").Find("Leave").gameObject.SetActive(true);
                                        Scenes.neonEventTempleScene01.SetActive(false);
                                        Schedule.neonLocation = "MountainLabRoomNikkeNeon";
                                        SaveManager.SetBool("Event_SeenNeonTemple01", true);
                                    }
                                }
                            }
                            break;
                        default:
                            if (!Dialogues.dialoguePlaying && Places.mountainLabRoomNikkeNeonRoomtalk.activeSelf && SaveManager.GetBool("Voyeur_SeenNeon"))
                            {
                                StartDialogueSequence(Dialogues.neonDefaultDialogue);
                                Characters.neon.SetActive(true);
                            }
                            if (Dialogues.neonDefaultDialogueDialogueFinisher.activeSelf)
                            {
                                Invoke(nameof(EndDialogueSequence), 1.0f);
                                Dialogues.neonDefaultDialogueDialogueFinisher.SetActive(false);
                            }
                            break;
                    }

                    switch (Schedule.pepperLocation)
                    {
                        case "Hospital":
                            if (SaveManager.GetBool("Event_SeenPepperHospital01"))
                            {
                                Schedule.pepperLocation = "MountainLabRoomNikkePepper";
                            }
                            else
                            {
                                if (!Dialogues.dialoguePlaying && !evaluatingLevelDialogue && Places.levelHospital.activeSelf && SaveManager.GetBool("Voyeur_SeenPepper"))
                                {
                                    evaluatingLevelDialogue = true;
                                    lastEvaluatedLevel = Places.levelHospital;
                                    this.dialogueToActivate = Dialogues.pepperHospitalEvent01Dialogue;
                                    diagBusts.Add(Characters.pepper);
                                    Invoke(nameof(CheckAndStartVanillaDialogue), 0.6f);
                                }
                                if (this.dialogueToActivate != null && this.dialogueToActivate.name == "PepperDialogueHospital01")
                                {
                                    if (this.dialogueToActivate.transform.Find("Scene1").gameObject.activeSelf)
                                    {
                                        Scenes.pepperEventHospitalScene01.SetActive(true);
                                        this.dialogueToActivate.transform.Find("Scene1").gameObject.SetActive(false);
                                    }
                                    if (this.dialogueToActivate.transform.Find("DialogueFinisher").gameObject.activeSelf)
                                    {
                                        Invoke(nameof(EndDialogueSequenceVanilla), 1.0f);
                                        this.dialogueToActivate.transform.Find("DialogueFinisher").gameObject.SetActive(false);
                                        Characters.pepper.transform.Find("MBase1").Find("Leave").gameObject.SetActive(true);
                                        Scenes.pepperEventHospitalScene01.SetActive(false);
                                        Schedule.pepperLocation = "MountainLabRoomNikkePepper";
                                        SaveManager.SetBool("Event_SeenPepperHospital01", true);
                                    }
                                }
                            }
                            break;
                        default:
                            if (!Dialogues.dialoguePlaying && Places.mountainLabRoomNikkePepperRoomtalk.activeSelf && SaveManager.GetBool("Voyeur_SeenPepper"))
                            {
                                StartDialogueSequence(Dialogues.pepperDefaultDialogue);
                                Characters.pepper.SetActive(true);
                            }
                            if (Dialogues.pepperDefaultDialogueDialogueFinisher.activeSelf)
                            {
                                Invoke(nameof(EndDialogueSequence), 1.0f);
                                Dialogues.pepperDefaultDialogueDialogueFinisher.SetActive(false);
                            }
                            break;
                    }

                    switch (Schedule.rapiLocation)
                    {
                        case "Gasstation":
                            if (SaveManager.GetBool("Event_SeenRapiGasStation01"))
                            {
                                Schedule.rapiLocation = "MountainLabRoomNikkeRapi";
                            }
                            else
                            {
                                if (!Dialogues.dialoguePlaying && !evaluatingLevelDialogue && Places.levelGasStation.activeSelf && SaveManager.GetBool("MountainLab_GKExplanation"))
                                {
                                    evaluatingLevelDialogue = true;
                                    lastEvaluatedLevel = Places.levelGasStation;
                                    this.dialogueToActivate = Dialogues.rapiGasstationEvent01Dialogue;
                                    diagBusts.Add(Characters.rapi);
                                    diagBusts.Add(Characters.sofia);
                                    Invoke(nameof(CheckAndStartVanillaDialogue), 0.6f);
                                }
                                if (this.dialogueToActivate != null && this.dialogueToActivate.name == "RapiDialogueGasstation01")
                                {
                                    if (this.dialogueToActivate.transform.Find("Scene1").gameObject.activeSelf)
                                    {
                                        Scenes.rapiEventGasstationScene01.SetActive(true);
                                        this.dialogueToActivate.transform.Find("Scene1").gameObject.SetActive(false);
                                    }
                                    if (this.dialogueToActivate.transform.Find("DialogueFinisher").gameObject.activeSelf)
                                    {
                                        Invoke(nameof(EndDialogueSequenceVanilla), 1.0f);
                                        this.dialogueToActivate.transform.Find("DialogueFinisher").gameObject.SetActive(false);
                                        Characters.rapi.transform.Find("MBase1").Find("Leave").gameObject.SetActive(true);
                                        Characters.sofia.transform.Find("D1Base").Find("Leave").gameObject.SetActive(true);
                                        Scenes.rapiEventGasstationScene01.SetActive(false);
                                        Schedule.rapiLocation = "MountainLabRoomNikkeRapi";
                                        SaveManager.SetBool("Event_SeenRapiGasStation01", true);
                                    }
                                }
                            }
                            break;
                        default:
                            if (!Dialogues.dialoguePlaying && Places.mountainLabRoomNikkeRapiRoomtalk.activeSelf && SaveManager.GetBool("Voyeur_SeenRapi"))
                            {
                                StartDialogueSequence(Dialogues.rapiDefaultDialogue);
                                Characters.rapi.SetActive(true);
                            }
                            if (Dialogues.rapiDefaultDialogueDialogueFinisher.activeSelf)
                            {
                                Invoke(nameof(EndDialogueSequence), 1.0f);
                                Dialogues.rapiDefaultDialogueDialogueFinisher.SetActive(false);
                            }
                            break;
                    }

                    switch (Schedule.rosannaLocation)
                    {
                        case "Gabrielsmansion":
                            if (SaveManager.GetBool("Event_SeenRosannaGabrielsMansion01"))
                            {
                                Schedule.rosannaLocation = "MountainLabRoomNikkeRosanna";
                            }
                            else
                            {
                                if (!Dialogues.dialoguePlaying && !evaluatingLevelDialogue && Places.levelGabrielsMansion.activeSelf && SaveManager.GetBool("Voyeur_SeenRosanna"))
                                {
                                    evaluatingLevelDialogue = true;
                                    lastEvaluatedLevel = Places.levelGabrielsMansion;
                                    this.dialogueToActivate = Dialogues.rosannaGabrielsmansionEvent01Dialogue;
                                    diagBusts.Add(Characters.rosanna);
                                    Invoke(nameof(CheckAndStartVanillaDialogue), 0.6f);
                                }
                                if (this.dialogueToActivate != null && this.dialogueToActivate.name == "RosannaDialogueGabrielsmansion01")
                                {
                                    if (this.dialogueToActivate.transform.Find("Scene1").gameObject.activeSelf)
                                    {
                                        Scenes.rosannaEventGabrielsmansionScene01.SetActive(true);
                                        this.dialogueToActivate.transform.Find("Scene1").gameObject.SetActive(false);
                                    }
                                    if (this.dialogueToActivate.transform.Find("Scene2").gameObject.activeSelf)
                                    {
                                        Characters.gabriel.SetActive(true);
                                        this.dialogueToActivate.transform.Find("Scene2").gameObject.SetActive(false);
                                    }
                                    if (this.dialogueToActivate.transform.Find("DialogueFinisher").gameObject.activeSelf)
                                    {
                                        Invoke(nameof(EndDialogueSequenceVanilla), 1.0f);
                                        this.dialogueToActivate.transform.Find("DialogueFinisher").gameObject.SetActive(false);
                                        Characters.gabriel.transform.Find("D1Base").Find("Leave").gameObject.SetActive(true);
                                        Characters.rosanna.transform.Find("MBase1").Find("Leave").gameObject.SetActive(true);
                                        Scenes.rosannaEventGabrielsmansionScene01.SetActive(false);
                                        Schedule.rosannaLocation = "MountainLabRoomNikkeRosanna";
                                        SaveManager.SetBool("Event_SeenRosannaGabrielsMansion01", true);
                                    }
                                }
                            }
                            break;
                        default:
                            if (!Dialogues.dialoguePlaying && Places.mountainLabRoomNikkeRosannaRoomtalk.activeSelf && SaveManager.GetBool("Voyeur_SeenRosanna"))
                            {
                                StartDialogueSequence(Dialogues.rosannaDefaultDialogue);
                                Characters.rosanna.SetActive(true);
                            }
                            if (Dialogues.rosannaDefaultDialogueDialogueFinisher.activeSelf)
                            {
                                Invoke(nameof(EndDialogueSequence), 1.0f);
                                Dialogues.rosannaDefaultDialogueDialogueFinisher.SetActive(false);
                            }
                            break;
                    }

                    switch (Schedule.sakuraLocation)
                    {
                        case "Forest":
                            if (SaveManager.GetBool("Event_SeenSakuraForest01"))
                            {
                                Schedule.sakuraLocation = "MountainLabRoomNikkeSakura";
                            }
                            else
                            {
                                if (!Dialogues.dialoguePlaying && !evaluatingLevelDialogue && Places.levelForest.activeSelf && SaveManager.GetBool("Voyeur_SeenSakura"))
                                {
                                    evaluatingLevelDialogue = true;
                                    lastEvaluatedLevel = Places.levelForest;
                                    this.dialogueToActivate = Dialogues.sakuraForestEvent01Dialogue;
                                    diagBusts.Add(Characters.sakura);
                                    Invoke(nameof(CheckAndStartVanillaDialogue), 0.6f);
                                }
                                if (this.dialogueToActivate != null && this.dialogueToActivate.name == "SakuraDialogueForest01")
                                {
                                    if (this.dialogueToActivate.transform.Find("Scene1").gameObject.activeSelf)
                                    {
                                        Scenes.sakuraEventForestScene01.SetActive(true);
                                        ChangeActiveBust(Characters.sakura, Characters.sakuraShirtless);
                                        this.dialogueToActivate.transform.Find("Scene1").gameObject.SetActive(false);
                                    }
                                    if (this.dialogueToActivate.transform.Find("DialogueFinisher").gameObject.activeSelf)
                                    {
                                        Invoke(nameof(EndDialogueSequenceVanilla), 1.0f);
                                        this.dialogueToActivate.transform.Find("DialogueFinisher").gameObject.SetActive(false);
                                        Characters.sakuraShirtless.transform.Find("MBase1").Find("Leave").gameObject.SetActive(true);
                                        Scenes.sakuraEventForestScene01.SetActive(false);
                                        Schedule.sakuraLocation = "MountainLabRoomNikkeSakura";
                                        SaveManager.SetBool("Event_SeenSakuraForest01", true);
                                    }
                                }
                            }
                            break;
                        default:
                            if (!Dialogues.dialoguePlaying && Places.mountainLabRoomNikkeSakuraRoomtalk.activeSelf && SaveManager.GetBool("Voyeur_SeenSakura"))
                            {
                                StartDialogueSequence(Dialogues.sakuraDefaultDialogue);
                                Characters.sakura.SetActive(true);
                            }
                            if (Dialogues.sakuraDefaultDialogueDialogueFinisher.activeSelf)
                            {
                                Invoke(nameof(EndDialogueSequence), 1.0f);
                                Dialogues.sakuraDefaultDialogueDialogueFinisher.SetActive(false);
                            }
                            break;
                    }

                    switch (Schedule.viperLocation)
                    {
                        case "Villa":
                            if (SaveManager.GetBool("Event_SeenViperVilla01"))
                            {
                                Schedule.viperLocation = "MountainLabRoomNikkeViper";
                            }
                            else
                            {
                                if (!Dialogues.dialoguePlaying && !evaluatingLevelDialogue && Places.levelVilla.activeSelf && SaveManager.GetBool("Voyeur_SeenViper"))
                                {
                                    evaluatingLevelDialogue = true;
                                    lastEvaluatedLevel = Places.levelVilla;
                                    this.dialogueToActivate = Dialogues.viperVillaEvent01Dialogue;
                                    diagBusts.Add(Characters.viper);
                                    Invoke(nameof(CheckAndStartVanillaDialogue), 0.6f);
                                }
                                if (this.dialogueToActivate != null && this.dialogueToActivate.name == "ViperDialogueVilla01")
                                {
                                    if (this.dialogueToActivate.transform.Find("Scene1").gameObject.activeSelf)
                                    {
                                        Scenes.viperEventVillaScene01.SetActive(true);
                                        this.dialogueToActivate.transform.Find("Scene1").gameObject.SetActive(false);
                                    }
                                    if (this.dialogueToActivate.transform.Find("Scene2").gameObject.activeSelf)
                                    {
                                        Characters.katarina.SetActive(true);
                                        this.dialogueToActivate.transform.Find("Scene2").gameObject.SetActive(false);
                                    }
                                    if (this.dialogueToActivate.transform.Find("Scene3").gameObject.activeSelf)
                                    {
                                        Characters.viper.transform.Find("MBase1").Find("Leave").gameObject.SetActive(true);
                                        this.dialogueToActivate.transform.Find("Scene3").gameObject.SetActive(false);
                                    }
                                    if (this.dialogueToActivate.transform.Find("DialogueFinisher").gameObject.activeSelf)
                                    {
                                        Invoke(nameof(EndDialogueSequenceVanilla), 1.0f);
                                        this.dialogueToActivate.transform.Find("DialogueFinisher").gameObject.SetActive(false);
                                        Characters.katarina.transform.Find("D1Base").Find("Leave").gameObject.SetActive(true);
                                        Scenes.viperEventVillaScene01.SetActive(false);
                                        Schedule.viperLocation = "MountainLabRoomNikkeViper";
                                        SaveManager.SetBool("Event_SeenViperVilla01", true);
                                    }
                                }
                            }
                            break;
                        default:
                            if (!Dialogues.dialoguePlaying && Places.mountainLabRoomNikkeViperRoomtalk.activeSelf && SaveManager.GetBool("Voyeur_SeenViper"))
                            {
                                StartDialogueSequence(Dialogues.viperDefaultDialogue);
                                Characters.viper.SetActive(true);
                            }
                            if (Dialogues.viperDefaultDialogueDialogueFinisher.activeSelf)
                            {
                                Invoke(nameof(EndDialogueSequence), 1.0f);
                                Dialogues.viperDefaultDialogueDialogueFinisher.SetActive(false);
                            }
                            break;
                    }

                    switch (Schedule.yanLocation)
                    {
                        case "Mall":
                            if (SaveManager.GetBool("Event_SeenYanMall01"))
                            {
                                Schedule.yanLocation = "MountainLabRoomNikkeYan";
                            }
                            else
                            {
                                if (!Dialogues.dialoguePlaying && !evaluatingLevelDialogue && Places.levelMall.activeSelf && SaveManager.GetBool("Voyeur_SeenYan"))
                                {
                                    evaluatingLevelDialogue = true;
                                    lastEvaluatedLevel = Places.levelMall;
                                    this.dialogueToActivate = Dialogues.yanMallEvent01Dialogue;
                                    diagBusts.Add(Characters.yan);
                                    Invoke(nameof(CheckAndStartVanillaDialogue), 0.6f);
                                }
                                if (this.dialogueToActivate != null && this.dialogueToActivate.name == "YanDialogueMall01")
                                {
                                    if (this.dialogueToActivate.transform.Find("Scene1").gameObject.activeSelf)
                                    {
                                        Scenes.yanEventMallScene01.SetActive(true);
                                        this.dialogueToActivate.transform.Find("Scene1").gameObject.SetActive(false);
                                    }
                                    if (this.dialogueToActivate.transform.Find("DialogueFinisher").gameObject.activeSelf)
                                    {
                                        Invoke(nameof(EndDialogueSequenceVanilla), 1.0f);
                                        this.dialogueToActivate.transform.Find("DialogueFinisher").gameObject.SetActive(false);
                                        Characters.yan.transform.Find("MBase1").Find("Leave").gameObject.SetActive(true);
                                        Scenes.yanEventMallScene01.SetActive(false);
                                        Schedule.yanLocation = "MountainLabRoomNikkeYan";
                                        SaveManager.SetBool("Event_SeenYanMall01", true);
                                    }
                                }
                            }
                            break;
                        default:
                            if (!Dialogues.dialoguePlaying && Places.mountainLabRoomNikkeYanRoomtalk.activeSelf && SaveManager.GetBool("Voyeur_SeenYan"))
                            {
                                StartDialogueSequence(Dialogues.yanDefaultDialogue);
                                Characters.yan.SetActive(true);
                            }
                            if (Dialogues.yanDefaultDialogueDialogueFinisher.activeSelf)
                            {
                                Invoke(nameof(EndDialogueSequence), 1.0f);
                                Dialogues.yanDefaultDialogueDialogueFinisher.SetActive(false);
                            }
                            break;
                    }

                    switch (Schedule.snekLocation)
                    {
                        case "Forest":
                            if (SaveManager.GetBool("Event_SeenIt01"))
                            {
                                Schedule.amberLocation = "Unknown";
                            }
                            if (!Dialogues.dialoguePlaying && !evaluatingLevelDialogue && Places.levelForest.activeSelf)
                            {
                                evaluatingLevelDialogue = true;
                                lastEvaluatedLevel = Places.levelForest;
                                this.dialogueToActivate = Dialogues.snekForestEgg01Dialogue;
                                diagBusts.Add(Characters.solidSnake);
                                Invoke(nameof(CheckAndStartVanillaDialogue), 0.4f);
                            }
                            if (this.dialogueToActivate != null && this.dialogueToActivate.name == "SnekForest")
                            {
                                if (this.dialogueToActivate.transform.Find("Scene1").gameObject.activeSelf)
                                {
                                    Color c = Places.solid.GetComponent<SpriteRenderer>().color;
                                    c.a = Mathf.MoveTowards(c.a, 1f, 1f * Time.deltaTime);
                                    Places.solid.GetComponent<SpriteRenderer>().color = c;
                                    snekIsSolid = true;
                                }
                                if (this.dialogueToActivate.transform.Find("DialogueFinisher").gameObject.activeSelf)
                                {
                                    this.dialogueToActivate.transform.Find("Scene1").gameObject.SetActive(false);
                                    snekIsSolid = false;
                                    Invoke(nameof(EndDialogueSequenceVanilla), 1.0f);
                                    this.dialogueToActivate.transform.Find("DialogueFinisher").gameObject.SetActive(false);
                                    Characters.solidSnake.transform.Find("MBase1").Find("Leave").gameObject.SetActive(true);
                                    Schedule.snekLocation = "Unknown";
                                    SaveManager.SetBool("Event_SeenIt01", true);
                                }
                            }
                            break;
                        default:
                            break;
                    }
                    if (!snekIsSolid && Places.solid.GetComponent<SpriteRenderer>().color.a > 0)
                    {
                        Color c = Places.solid.GetComponent<SpriteRenderer>().color;
                        c.a = Mathf.MoveTowards(c.a, 0f, 1f * Time.deltaTime);
                        Places.solid.GetComponent<SpriteRenderer>().color = c;
                    }
                    #endregion
                    #region Mountain Lab
                    //------------------------------------------------------------------------------------------------ ML Story 2
                    if (!Dialogues.dialoguePlaying && Places.mountainLabRoomtalk.activeSelf && !SaveManager.GetBool("MountainLab_FirstVisited"))
                    {
                        StartDialogueSequence(Dialogues.mLDialogueMainFirst);
                        Characters.amberSwim.SetActive(true);
                    }
                    if (Dialogues.mLDialogueMainFirstScene1.activeSelf)
                    {
                        Dialogues.mLDialogueMainFirstScene1.SetActive(false);
                        Characters.amberSwim.transform.Find("MBase1").Find("Leave").gameObject.SetActive(true);
                    }
                    if (Dialogues.mLDialogueMainFirstScene5.activeSelf)
                    {
                        Dialogues.mLDialogueMainFirstScene5.SetActive(false);
                        Core.affectionIncrease.SetActive(true);
                        SaveManager.SetInt("Affection_Amber", SaveManager.GetInt("Affection_Amber") + 1);
                    }
                    if (Dialogues.mLDialogueMainFirstDialogueFinisher.activeSelf)
                    {
                        Invoke(nameof(EndDialogueSequence), 1.0f);
                        Dialogues.mLDialogueMainFirstDialogueFinisher.SetActive(false);
                        SaveManager.SetBool("MountainLab_FirstVisited", true);
                    }
//------------------------------------------------------------------------------------------------ ML Story 3
                    if (!Dialogues.dialoguePlaying && Places.mountainLabRoomtalk.activeSelf && SaveManager.GetBool("MountainLab_FirstVisited") && !SaveManager.GetBool("MountainLab_FirstVisitor") && SaveManager.AnyBoolVariableWithNameContains("Voyeur_Seen"))
                    {
                        StartDialogueSequence(Dialogues.mLDialogueMainStory03);
                        Characters.amber.SetActive(true);
                    }
                    if (Dialogues.mLDialogueMainStory03DialogueFinisher.activeSelf)
                    {
                        Invoke(nameof(EndDialogueSequence), 1.0f);
                        Dialogues.mLDialogueMainStory03DialogueFinisher.SetActive(false);
                        SaveManager.SetBool("MountainLab_FirstVisitor", true);
                    }
//------------------------------------------------------------------------------------------------ ML Story 4
                    if (!Dialogues.dialoguePlaying && Places.mountainLabRoomtalk.activeSelf && SaveManager.GetBool("MountainLab_FirstVisitor") && !SaveManager.GetBool("MountainLab_GKExplanation") && AllStarterVoyeurTargetsFound())
                    {
                        StartDialogueSequence(Dialogues.mLDialogueMainStory04);
                        Characters.amber.SetActive(true);
                        Characters.anis.SetActive(true);
                        Characters.rapi.SetActive(true);
                        Characters.neon.SetActive(true);
                    }
                    if (Dialogues.mLDialogueMainStory04Scene1.activeSelf)
                    {
                        Dialogues.mLDialogueMainFirstScene1.SetActive(false);
                        Characters.anis.transform.Find("MBase1").Find("Leave").gameObject.SetActive(true);
                    }
                    if (Dialogues.mLDialogueMainStory04Scene2.activeSelf)
                    {
                        Dialogues.mLDialogueMainFirstScene2.SetActive(false);
                        Characters.neon.transform.Find("MBase1").Find("Leave").gameObject.SetActive(true);
                    }
                    if (Dialogues.mLDialogueMainStory04Scene3.activeSelf)
                    {
                        Dialogues.mLDialogueMainFirstScene3.SetActive(false);
                        Characters.rapi.transform.Find("MBase1").Find("Leave").gameObject.SetActive(true);
                    }
                    if (Dialogues.mLDialogueMainStory04Scene5.activeSelf)
                    {
                        Dialogues.mLDialogueMainStory04Scene5.SetActive(false);
                        Core.affectionIncrease.SetActive(true);
                        SaveManager.SetInt("Affection_Rapi", SaveManager.GetInt("Affection_Rapi") + 1);
                    }
                    if (Dialogues.mLDialogueMainStory04DialogueFinisher.activeSelf)
                    {
                        Invoke(nameof(EndDialogueSequence), 1.0f);
                        Dialogues.mLDialogueMainStory04DialogueFinisher.SetActive(false);
                        SaveManager.SetBool("MountainLab_GKExplanation", true);
                    }
                    #endregion
                    #region Bad Weather
                    if (Places.secretBeachRoomtalk.activeSelf && Core.GetVariableBool("rainy-day"))
                    {
                        Places.weatherOutsideRain.SetActive(true);
                    }
                    if (Places.secretBeachRoomtalk.activeSelf && Core.GetVariableBool("snowy-day"))
                    {
                        Places.weatherOutsideSnow.SetActive(true);
                    }
                    if (!Dialogues.dialoguePlaying && Places.GetBadWeather() && Places.secretBeachRoomtalk.activeSelf)
                    {
                        StartDialogueSequence(Dialogues.badWeatherDialogue);
                    }
                    if (Dialogues.badWeatherDialogueFinisher.activeSelf)
                    {
                        Invoke(nameof(EndDialogueSequence), 1.0f);
                        Dialogues.badWeatherDialogueFinisher.SetActive(false);
                    }
                    #endregion
                    #region Secret Beach
                    if (!actionTodaySB && !Places.GetBadWeather())
                    {
//------------------------------------------------------------------------------------------------ SB First
                        if (!Dialogues.dialoguePlaying && Places.secretBeachRoomtalk.activeSelf && !SaveManager.GetBool("SecretBeach_FirstVisited"))
                        {
                            StartDialogueSequence(Dialogues.sBDialogueMainFirst);
                        }
                        if (Dialogues.sBDialogueMainFirstDialogueFinisher.activeSelf)
                        {
                            Invoke(nameof(EndDialogueSequence), 1.0f);
                            Dialogues.sBDialogueMainFirstDialogueFinisher.SetActive(false);
                            SaveManager.SetBool("SecretBeach_FirstVisited", true);
                            actionTodaySB = true;
                        }
//------------------------------------------------------------------------------------------------ SB Main
                        if (!Dialogues.dialoguePlaying && Places.secretBeachRoomtalk.activeSelf && SaveManager.GetBool("SecretBeach_FirstVisited") && SaveManager.GetInt("SecretBeach_RelaxedAmount") != 2 && 
                            !(SaveManager.GetBool("MountainLab_FirstVisited") != true && SaveManager.GetInt("SecretBeach_RelaxedAmount") > 2))
                        {
                            StartDialogueSequence(Dialogues.sBDialogueMain);
                        }
                        if (Dialogues.sBDialogueMainScene1.activeSelf)
                        {
                            Dialogues.sBDialogueMainScene1.SetActive(false);
                            SaveManager.SetInt("SecretBeach_RelaxedAmount", SaveManager.GetInt("SecretBeach_RelaxedAmount") + 1);
                            relaxed = true;
                        }
                        if (Dialogues.sBDialogueMainDialogueFinisher.activeSelf)
                        {
                            if (SaveManager.GetInt("SecretBeach_RelaxedAmount") > 2 && voyeurLotteryNumber <= 50 && relaxed && voyeurTargetsLeft.Count != 0)
                            {
                                currentVoyeurTarget = ChooseVoyeurTarget();
                                if (this.dialogueToActivate != null)
                                {
                                    foreach (Transform child in this.dialogueToActivate.transform)
                                    {
                                        child.gameObject.SetActive(false);
                                    }
                                }
                                relaxed = false;
                                Places.secretBeachRoomtalk.SetActive(true);
                                Core.bustManager.Find(currentVoyeurTarget + "Swim").gameObject.SetActive(true);
                                currentActiveBust = Core.bustManager.Find(currentVoyeurTarget + "Swim").gameObject;
                                currentActiveBustMBase = currentActiveBust.transform.Find("MBase1").gameObject;
                                this.dialogueToActivate = Places.secretBeachRoomtalk.transform.Find(currentVoyeurTarget + "DialogueSecretbeach01").gameObject;
                                currentActiveDialogue = Places.secretBeachRoomtalk.transform.Find(currentVoyeurTarget + "DialogueSecretbeach01").gameObject;
                                currentActiveDialogueSpriteFocus = currentActiveDialogue.transform.Find("SpriteFocus").gameObject;
                                Invoke(nameof(PlayDialogueStep), 1.0f);
                                voyeurDialoguePlaying = true;
                            }
                            else
                            {
                                Invoke(nameof(EndDialogueSequence), 1.0f);
                                actionTodaySB = true;
                            }
                            Dialogues.sBDialogueMainDialogueFinisher.SetActive(false);
                        }
//------------------------------------------------------------------------------------------------ SB GK
                        if (!Dialogues.dialoguePlaying && Places.secretBeachRoomtalk.activeSelf && SaveManager.GetInt("SecretBeach_RelaxedAmount") == 2)
                        {
                            StartDialogueSequence(Dialogues.sBDialogueMainGK);
                        }
                        if (Dialogues.sBDialogueMainGKScene1.activeSelf)
                        {
                            Dialogues.sBDialogueMainGKScene1.SetActive(false);
                            SaveManager.SetInt("SecretBeach_RelaxedAmount", SaveManager.GetInt("SecretBeach_RelaxedAmount") + 1);
                        }
                        if (Dialogues.sBDialogueMainGKScene2.activeSelf)
                        {
                            Places.secretBeachLevel.GetComponent<ParallaxMouseEffect>().enabled = false;
                            Places.secretBeachLevelBG.GetComponent<ParallaxMouseEffect>().enabled = false;
                            if (Places.secretBeachLevel.transform.position.y > -17)
                            {
                                Places.secretBeachLevel.transform.position = Vector2.SmoothDamp(Places.secretBeachLevel.transform.position, new Vector2(0, -17), ref refVelocity, 1.5f);
                            }
                        }
                        if (Dialogues.sBDialogueMainGKScene3.activeSelf && !Dialogues.sBDialogueMainGKScene4.activeSelf)
                        {
                            Places.secretBeachLevel.transform.position = new Vector2(Places.secretBeachLevel.transform.position.x, -17);
                            Places.secretBeachGatekeeperB.SetActive(true);
                        }
                        if (Dialogues.sBDialogueMainGKScene4.activeSelf)
                        {
                            Places.secretBeachFlash.SetActive(true);
                            Places.secretBeachGatekeeper.SetActive(false);
                            Places.secretBeachGatekeeperB.SetActive(false);
                        }
                        if (Dialogues.sBDialogueMainGKScene5.activeSelf)
                        {
                            if (Places.secretBeachLevel.transform.position.y < 0)
                            {
                                Places.secretBeachLevel.transform.position = Vector2.SmoothDamp(Places.secretBeachLevel.transform.position, new Vector2(0, 7), ref refVelocity, 1f);
                            }
                        }
                        if (Dialogues.sBDialogueMainGKDialogueFinisher.activeSelf)
                        {
                            Invoke(nameof(EndDialogueSequence), 1.0f);
                            Places.secretBeachLevel.transform.position = new Vector2(Places.secretBeachLevel.transform.position.x, 0);
                            Dialogues.sBDialogueMainGKDialogueFinisher.SetActive(false);
                            Places.secretBeachLevel.GetComponent<ParallaxMouseEffect>().enabled = true;
                            Places.secretBeachLevelBG.GetComponent<ParallaxMouseEffect>().enabled = true;
                            actionTodaySB = true;
                        }
//------------------------------------------------------------------------------------------------ SB Story 1
                        if (!Dialogues.dialoguePlaying && Places.secretBeachRoomtalk.activeSelf && SaveManager.GetBool("SecretBeach_UnlockedLab") != true && SaveManager.GetInt("SecretBeach_RelaxedAmount") > 2)
                        {
                            SaveManager.SetBool("SecretBeach_UnlockedLab", true);
                            StartDialogueSequence(Dialogues.sBDialogueStory01);
                        }
                        if (Dialogues.sBDialogueStory01Scene1.activeSelf)
                        {
                            Dialogues.sBDialogueStory01Scene1.SetActive(false);
                            Characters.amberSwim.SetActive(true);
                        }
                        if (Dialogues.sBDialogueStory01Scene2.activeSelf)
                        {
                            Dialogues.sBDialogueStory01Scene2.SetActive(false);
                            Scenes.amberStorySecretbeachScene01.SetActive(true);
                        }
                        if (Dialogues.sBDialogueStory01DialogueFinisher.activeSelf)
                        {
                            Invoke(nameof(EndDialogueSequence), 1.0f);
                            Dialogues.sBDialogueStory01DialogueFinisher.SetActive(false);
                            Characters.amberSwim.transform.Find("MBase1").Find("Leave").gameObject.SetActive(true);
                            Scenes.amberStorySecretbeachScene01.SetActive(false);
                            actionTodaySB = true;
                        }
                        #endregion
                        #region Voyeur
                        if (Dialogues.anisSecretbeachVoyeur01DialogueScene2.activeSelf && !Dialogues.anisSecretbeachVoyeur01DialogueScene3.activeSelf && !Characters.anisSwimWet.activeSelf ) { ChangeActiveBust(Characters.anisSwim, Characters.anisSwimWet); }
                        if (Dialogues.anisSecretbeachVoyeur01DialogueScene3.activeSelf && !Characters.anisSwimSlip.activeSelf) { ChangeActiveBust(Characters.anisSwimWet, Characters.anisSwimSlip); }
                        if (Dialogues.anisSecretbeachVoyeur01DialogueFinisher.activeSelf) { Characters.anisSwimSlip.transform.Find("MBase1").Find("Leave").gameObject.SetActive(true); }

                        if (Dialogues.centiSecretbeachVoyeur01DialogueScene2.activeSelf && !Dialogues.centiSecretbeachVoyeur01DialogueScene3.activeSelf && !Characters.centiSwimShirtless.activeSelf) { ChangeActiveBust(Characters.centiSwim, Characters.centiSwimShirtless); }
                        if (Dialogues.centiSecretbeachVoyeur01DialogueScene4.activeSelf && !Characters.centiSwimSlip.activeSelf) { ChangeActiveBust(Characters.centiSwimShirtless, Characters.centiSwimSlip); }
                        if (Dialogues.centiSecretbeachVoyeur01DialogueFinisher.activeSelf) { Characters.centiSwimSlip.transform.Find("MBase1").Find("Leave").gameObject.SetActive(true); }

                        if (Dialogues.dorothySecretbeachVoyeur01DialogueScene2.activeSelf && !Dialogues.dorothySecretbeachVoyeur01DialogueScene3.activeSelf && !Characters.dorothySwimWet.activeSelf) { ChangeActiveBust(Characters.dorothySwim, Characters.dorothySwimWet); }
                        if (Dialogues.dorothySecretbeachVoyeur01DialogueScene3.activeSelf && !Characters.dorothySwimSlip.activeSelf) { ChangeActiveBust(Characters.dorothySwimWet, Characters.dorothySwimSlip); }
                        if (Dialogues.dorothySecretbeachVoyeur01DialogueFinisher.activeSelf) { Characters.dorothySwimSlip.transform.Find("MBase1").Find("Leave").gameObject.SetActive(true); }

                        if (Dialogues.eleggSecretbeachVoyeur01DialogueScene3.activeSelf && !Characters.eleggSwimSlip.activeSelf) { ChangeActiveBust(Characters.eleggSwim, Characters.eleggSwimSlip); }
                        if (Dialogues.eleggSecretbeachVoyeur01DialogueFinisher.activeSelf) { Characters.eleggSwimSlip.transform.Find("MBase1").Find("Leave").gameObject.SetActive(true); }

                        if (Dialogues.frimaSecretbeachVoyeur01DialogueScene2.activeSelf && !Dialogues.frimaSecretbeachVoyeur01DialogueScene4.activeSelf && !Characters.frimaSwimShirtless.activeSelf) { ChangeActiveBust(Characters.frimaSwim, Characters.frimaSwimShirtless); }
                        if (Dialogues.frimaSecretbeachVoyeur01DialogueScene4.activeSelf && !Characters.frimaSwimSlip.activeSelf) { ChangeActiveBust(Characters.frimaSwimShirtless, Characters.frimaSwimSlip); }
                        if (Dialogues.frimaSecretbeachVoyeur01DialogueFinisher.activeSelf) { Characters.frimaSwimSlip.transform.Find("MBase1").Find("Leave").gameObject.SetActive(true); }

                        if (Dialogues.guiltySecretbeachVoyeur01DialogueScene4.activeSelf && !Characters.guiltySwimSlip.activeSelf) { ChangeActiveBust(Characters.guiltySwim, Characters.guiltySwimSlip); }
                        if (Dialogues.guiltySecretbeachVoyeur01DialogueFinisher.activeSelf) { Characters.guiltySwimSlip.transform.Find("MBase1").Find("Leave").gameObject.SetActive(true); }

                        if (Dialogues.helmSecretbeachVoyeur01DialogueScene2.activeSelf && !Dialogues.helmSecretbeachVoyeur01DialogueScene3.activeSelf && !Characters.helmSwimWet.activeSelf) { ChangeActiveBust(Characters.helmSwim, Characters.helmSwimWet); }
                        if (Dialogues.helmSecretbeachVoyeur01DialogueScene3.activeSelf && !Dialogues.helmSecretbeachVoyeur01DialogueScene4.activeSelf && !Characters.helmSwimShirtless.activeSelf) { ChangeActiveBust(Characters.helmSwimWet, Characters.helmSwimShirtless); }
                        if (Dialogues.helmSecretbeachVoyeur01DialogueScene4.activeSelf && !Characters.helmSwimSlip.activeSelf) { ChangeActiveBust(Characters.helmSwimShirtless, Characters.helmSwimSlip); }
                        if (Dialogues.helmSecretbeachVoyeur01DialogueFinisher.activeSelf) { Characters.helmSwimSlip.transform.Find("MBase1").Find("Leave").gameObject.SetActive(true); }

                        if (Dialogues.maidenSecretbeachVoyeur01DialogueScene4.activeSelf && !Characters.maidenSwimSlip.activeSelf) { ChangeActiveBust(Characters.maidenSwim, Characters.maidenSwimSlip); }
                        if (Dialogues.maidenSecretbeachVoyeur01DialogueFinisher.activeSelf) { Characters.maidenSwimSlip.transform.Find("MBase1").Find("Leave").gameObject.SetActive(true); }

                        if (Dialogues.marySecretbeachVoyeur01DialogueScene3.activeSelf && !Characters.marySwimSlip.activeSelf) { ChangeActiveBust(Characters.marySwim, Characters.marySwimSlip); }
                        if (Dialogues.marySecretbeachVoyeur01DialogueFinisher.activeSelf) { Characters.marySwimSlip.transform.Find("MBase1").Find("Leave").gameObject.SetActive(true); }

                        if (Dialogues.mastSecretbeachVoyeur01DialogueScene4.activeSelf && !Characters.mastSwimSlip.activeSelf) { ChangeActiveBust(Characters.mastSwim, Characters.mastSwimSlip); }
                        if (Dialogues.mastSecretbeachVoyeur01DialogueFinisher.activeSelf) { Characters.mastSwimSlip.transform.Find("MBase1").Find("Leave").gameObject.SetActive(true); }

                        if (Dialogues.neonSecretbeachVoyeur01DialogueScene3.activeSelf && !Dialogues.neonSecretbeachVoyeur01DialogueScene4.activeSelf && !Characters.neonSwimWet.activeSelf) { ChangeActiveBust(Characters.neonSwim, Characters.neonSwimWet); }
                        if (Dialogues.neonSecretbeachVoyeur01DialogueScene4.activeSelf && !Characters.neonSwimSlip.activeSelf) { ChangeActiveBust(Characters.neonSwimWet, Characters.neonSwimSlip); }
                        if (Dialogues.neonSecretbeachVoyeur01DialogueFinisher.activeSelf) { Characters.neonSwimSlip.transform.Find("MBase1").Find("Leave").gameObject.SetActive(true); }

                        if (Dialogues.pepperSecretbeachVoyeur01DialogueScene3.activeSelf && !Characters.pepperSwimSlip.activeSelf) { ChangeActiveBust(Characters.pepperSwim, Characters.pepperSwimSlip); }
                        if (Dialogues.pepperSecretbeachVoyeur01DialogueFinisher.activeSelf) { Characters.pepperSwimSlip.transform.Find("MBase1").Find("Leave").gameObject.SetActive(true); }

                        if (Dialogues.rapiSecretbeachVoyeur01DialogueScene4.activeSelf && !Characters.rapiSwimSlip.activeSelf) { ChangeActiveBust(Characters.rapiSwim, Characters.rapiSwimSlip); }
                        if (Dialogues.rapiSecretbeachVoyeur01DialogueFinisher.activeSelf) { Characters.rapiSwimSlip.transform.Find("MBase1").Find("Leave").gameObject.SetActive(true); }

                        if (Dialogues.rosannaSecretbeachVoyeur01DialogueScene4.activeSelf && !Characters.rosannaSwimSlip.activeSelf) { ChangeActiveBust(Characters.rosannaSwim, Characters.rosannaSwimSlip); }
                        if (Dialogues.rosannaSecretbeachVoyeur01DialogueFinisher.activeSelf) { Characters.rosannaSwimSlip.transform.Find("MBase1").Find("Leave").gameObject.SetActive(true); }

                        if (Dialogues.sakuraSecretbeachVoyeur01DialogueScene4.activeSelf && !Characters.sakuraSwimSlip.activeSelf) { ChangeActiveBust(Characters.sakuraSwim, Characters.sakuraSwimSlip); }
                        if (Dialogues.sakuraSecretbeachVoyeur01DialogueFinisher.activeSelf) { Characters.sakuraSwimSlip.transform.Find("MBase1").Find("Leave").gameObject.SetActive(true); }

                        if (Dialogues.viperSecretbeachVoyeur01DialogueScene1.activeSelf && !Dialogues.viperSecretbeachVoyeur01DialogueScene2.activeSelf && !Characters.viperSwimShirtless.activeSelf) { ChangeActiveBust(Characters.viperSwim, Characters.viperSwimShirtless); }
                        if (Dialogues.viperSecretbeachVoyeur01DialogueScene2.activeSelf && !Dialogues.viperSecretbeachVoyeur01DialogueScene4.activeSelf && !Characters.viperSwimWet.activeSelf) { ChangeActiveBust(Characters.viperSwimShirtless, Characters.viperSwimWet); }
                        if (Dialogues.viperSecretbeachVoyeur01DialogueScene4.activeSelf && !Characters.viperSwimSlip.activeSelf) { ChangeActiveBust(Characters.viperSwimWet, Characters.viperSwimSlip); }
                        if (Dialogues.viperSecretbeachVoyeur01DialogueFinisher.activeSelf) { Characters.viperSwimSlip.transform.Find("MBase1").Find("Leave").gameObject.SetActive(true); }

                        if (Dialogues.yanSecretbeachVoyeur01DialogueScene4.activeSelf && !Characters.yanSwimSlip.activeSelf) { ChangeActiveBust(Characters.yanSwim, Characters.yanSwimSlip); }
                        if (Dialogues.yanSecretbeachVoyeur01DialogueFinisher.activeSelf) { Characters.yanSwimSlip.transform.Find("MBase1").Find("Leave").gameObject.SetActive(true); }

                        if (currentActiveBust != null && voyeurDialoguePlaying) { Scenes.DialogueScenePlayer(Core.cGManagerSexy, currentVoyeurTarget + "VoyeurSecretbeach", currentActiveDialogue); }

                        if (voyeurDialoguePlaying)
                        {
                            if (currentActiveDialogue.transform.Find("DialogueFinisher").gameObject.activeSelf)
                            {
                                Invoke(nameof(EndDialogueSequence), 1.0f);
                                Core.cGManagerSexy.Find(currentVoyeurTarget + "VoyeurSecretbeachScene01").gameObject.SetActive(false);
                                Core.cGManagerSexy.Find(currentVoyeurTarget + "VoyeurSecretbeachScene02").gameObject.SetActive(false);
                                Core.cGManagerSexy.Find(currentVoyeurTarget + "VoyeurSecretbeachScene03").gameObject.SetActive(false);
                                Core.cGManagerSexy.Find(currentVoyeurTarget + "VoyeurSecretbeachScene04").gameObject.SetActive(false);
                                currentActiveDialogue.transform.Find("DialogueFinisher").gameObject.SetActive(false);
                                currentActiveBustMBase.transform.Find("Leave").gameObject.SetActive(true);
                                SaveManager.SetBool("Voyeur_Seen" + currentVoyeurTarget, true);
                                voyeurDialoguePlaying = false;
                                actionTodaySB = true;
                            }
                        }
                        #endregion
                    }
                }
            }
            if (Core.currentScene.name == "GameStart")
            {
                if (loadedStory)
                {
                    Logger.LogInfo("----- STORY UNLOADED -----");
                    loadedStory = false;
                }
            }
        }

        public void StartDialogueSequence(GameObject diag)
        {
            Logger.LogInfo(diag.name + " started.");
            Dialogues.dialoguePlaying = true;
            this.dialogueToActivate = diag;
            currentActiveDialogue = diag;
            currentActiveDialogueSpriteFocus = diag.transform.Find("SpriteFocus").gameObject;
            Signals.Emit(fadeUISignal);
            Invoke(nameof(PlayDialogueStep), 1.0f);
        }
        private void PlayDialogueStep()
        {
            Debug.Log("Playing " + dialogueToActivate.name);
            this.dialogueToActivate.GetComponent<Dialogue>().EventStartNext += OnDialogueLineStart;
            this.dialogueToActivate.transform.Find("DialogueActivator").gameObject.SetActive(true);
        }
        public void EndDialogueSequence()
        {
            Signals.Emit(fadeUISignal);
            this.dialogueToActivate.GetComponent<Dialogue>().EventStartNext -= OnDialogueLineStart;
            Invoke(nameof(FinishStep), 0.5f);
        }
        private void FinishStep()
        {
            if (this.dialogueToActivate != null)
            {
                foreach (Transform child in this.dialogueToActivate.transform)
                {
                    child.gameObject.SetActive(false);
                }
            }
            Dialogues.dialoguePlaying = false;
            Logger.LogInfo(this.dialogueToActivate.name + " finished.");
            this.dialogueToActivate = null;
            this.tempNewCurrentRT = null;
        }
        public void StartDialogueSequenceVanilla(GameObject diag)
        {
            Logger.LogInfo(diag.name + " started.");
            Dialogues.dialoguePlaying = true;
            this.tempNewCurrentRT = GameObject.Instantiate(Places.secretBeachRoomtalk, Core.roomTalk);
            this.dialogueToActivate = GameObject.Instantiate(diag, this.tempNewCurrentRT.transform);
            this.dialogueToActivate.name = diag.name;
            foreach (Transform child in this.tempNewCurrentRT.transform)
            {
                if (child != this.dialogueToActivate.transform)
                {
                    Destroy(child.gameObject);
                }
            }
            Destroy(this.tempNewCurrentRT.GetComponent<Conditions>());
            currentActiveDialogue = this.dialogueToActivate;
            currentActiveDialogueSpriteFocus = this.dialogueToActivate.transform.Find("SpriteFocus").gameObject;
            Signals.Emit(fadeUISignal);
            Invoke(nameof(PlayDialogueStepVanilla), 1.0f);
        }
        private void PlayDialogueStepVanilla()
        {
            Debug.Log("Playing " + dialogueToActivate.name);
            this.dialogueToActivate.GetComponent<Dialogue>().EventStartNext += OnDialogueLineStart;
            this.dialogueToActivate.transform.Find("DialogueActivator").gameObject.SetActive(true);
            this.tempNewCurrentRT.SetActive(true);
        }
        public void EndDialogueSequenceVanilla()
        {
            Signals.Emit(fadeUISignal);
            this.dialogueToActivate.GetComponent<Dialogue>().EventStartNext -= OnDialogueLineStart;
            Invoke(nameof(FinishStepVanilla), 0.5f);
        }
        private void FinishStepVanilla()
        {
            if (this.dialogueToActivate != null)
            {
                foreach (Transform child in this.dialogueToActivate.transform)
                {
                    child.gameObject.SetActive(false);
                }
            }
            Dialogues.dialoguePlaying = false;
            Logger.LogInfo(this.dialogueToActivate.name + " finished.");
            this.dialogueToActivate = null;
            this.tempNewCurrentRT = null;
            Destroy(this.tempNewCurrentRT);
            diagBusts.Clear();
        }
        public string ChooseVoyeurTarget()
        {
            int randomIndex = UnityEngine.Random.Range(0, voyeurTargetsLeft.Count);
            string chosenCharacter = voyeurTargetsLeft[randomIndex];
            Logger.LogInfo($"Voyeur target chosen: {chosenCharacter}");
            return chosenCharacter;
        }
        public void ChangeActiveBust(GameObject oldBust, GameObject newBust)
        {
            oldBust.SetActive(false);
            newBust.SetActive(true);
            currentActiveBust = newBust;
            currentActiveBustMBase = newBust.transform.GetChild(0).gameObject;
        }
        public static void ChangeBustSortingOrder(GameObject mBase, int order)
        {
            // Check if mBase is null
            if (mBase == null)
            {
                Debug.LogWarning("ChangeBustSortingOrder: mBase is null");
                return;
            }

            // Check if SpriteRenderer component exists
            SpriteRenderer spriteRenderer = mBase.GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
            {
                Debug.LogWarning($"ChangeBustSortingOrder: No SpriteRenderer found on {mBase.name}");
                return;
            }

            spriteRenderer.sortingOrder = order;
            
            // Check if the "Wet" child object exists and is active before trying to access it
            Transform wetChild = mBase.transform.Find("Wet");
            if (wetChild != null && wetChild.gameObject.activeSelf)
            {
                SpriteRenderer wetSpriteRenderer = wetChild.gameObject.GetComponent<SpriteRenderer>();
                if (wetSpriteRenderer != null)
                {
                    wetSpriteRenderer.sortingOrder = order + 1;
                }
            }
        }
        public static void SpriteFocusChange(bool activate)
        {
            if (activate)
            {
                ChangeBustSortingOrder(currentActiveBustMBase, 17);
                ChangeBustSortingOrder(currentActiveBustMBase.transform.Find("Blink").gameObject, 23);
                ChangeBustSortingOrder(currentActiveBustMBase.transform.Find("Expressions").Find("Angry").gameObject, 22);
                ChangeBustSortingOrder(currentActiveBustMBase.transform.Find("Expressions").Find("Happy").gameObject, 22);
                ChangeBustSortingOrder(currentActiveBustMBase.transform.Find("Expressions").Find("Flirty").gameObject, 22);
                ChangeBustSortingOrder(currentActiveBustMBase.transform.Find("Expressions").Find("Sad").gameObject, 22);
                ChangeBustSortingOrder(currentActiveBustMBase.transform.Find("Mouth").Find("1").gameObject, 23);
                ChangeBustSortingOrder(currentActiveBustMBase.transform.Find("Mouth").Find("2").gameObject, 23);
                ChangeBustSortingOrder(currentActiveBustMBase.transform.Find("Mouth").Find("3").gameObject, 23);
                ChangeBustSortingOrder(currentActiveBustMBase.transform.Find("Mouth").Find("4").gameObject, 23);
            }
            else
            {
                ChangeBustSortingOrder(currentActiveBustMBase, 0);
                ChangeBustSortingOrder(currentActiveBustMBase.transform.Find("Blink").gameObject, 6);
                ChangeBustSortingOrder(currentActiveBustMBase.transform.Find("Expressions").Find("Angry").gameObject, 5);
                ChangeBustSortingOrder(currentActiveBustMBase.transform.Find("Expressions").Find("Happy").gameObject, 5);
                ChangeBustSortingOrder(currentActiveBustMBase.transform.Find("Expressions").Find("Flirty").gameObject, 5);
                ChangeBustSortingOrder(currentActiveBustMBase.transform.Find("Expressions").Find("Sad").gameObject, 5);
                ChangeBustSortingOrder(currentActiveBustMBase.transform.Find("Mouth").Find("1").gameObject, 6);
                ChangeBustSortingOrder(currentActiveBustMBase.transform.Find("Mouth").Find("2").gameObject, 6);
                ChangeBustSortingOrder(currentActiveBustMBase.transform.Find("Mouth").Find("3").gameObject, 6);
                ChangeBustSortingOrder(currentActiveBustMBase.transform.Find("Mouth").Find("4").gameObject, 6);
            }
        }

        private void CheckAndStartVanillaDialogue()
        {
            if (!Dialogues.dialoguePlayingVanilla)
            {
                foreach (GameObject bust in diagBusts) 
                {
                    bust.SetActive(true);
                }
                StartDialogueSequenceVanilla(this.dialogueToActivate);
                Debug.Log("Starting Dialogue Sequence: " + this.dialogueToActivate);
            }
        }

        public static void OnDialogueLineStart(int nodeID)
        {
            string actor = GetCurrentSpeakingActor();
            string expression = GetCurrentActorExpression();

            if (!string.IsNullOrEmpty(actor) && !string.IsNullOrEmpty(expression))
            {
                Debug.Log($"[OnDialogueLineStart] Actor: {actor}, Expression: {expression}");
            }

            // Process variable replacement for the current dialogue line
            ProcessCurrentDialogueLine(nodeID);

            // Set the current active bust based on the speaking actor
            if (!string.IsNullOrEmpty(actor) && (actor != "PlayerActor" && actor != "S_Mobster1Actor"))
            {
                GameObject bustForActor = GetBustForActor(actor);
                if (bustForActor != null)
                {
                    currentActiveBust = bustForActor;
                    currentActiveBustMBase = bustForActor.transform.GetChild(0).gameObject;
                    Debug.Log($"[OnDialogueLineStart] Set currentActiveBust to: {bustForActor.name}");

                    currentActiveBustMBase.transform.Find("Mouth").gameObject.SetActive(true);
                    if (string.IsNullOrEmpty(expression) || expression == "my-expression" || expression == "neutral")
                    {
                        // Disable all expression child GameObjects
                        Transform expressionsTransform = currentActiveBustMBase.transform.Find("Expressions");
                        if (expressionsTransform != null)
                        {
                            foreach (Transform child in expressionsTransform)
                            {
                                child.gameObject.SetActive(false);
                            }
                        }
                    }
                    else
                    {
                        Transform expressionsTransform = currentActiveBustMBase.transform.Find("Expressions");
                        foreach (Transform child in expressionsTransform)
                        {
                            child.gameObject.SetActive(false);
                        }
                        currentActiveBustMBase.transform.Find("Expressions").Find(expression).gameObject.SetActive(true);
                    }
                }
            }
        }
        private static void ProcessCurrentDialogueLine(int nodeID)
        {
            try
            {
                if (currentActiveDialogue == null)
                {
                    Debug.LogWarning("[ProcessCurrentDialogueLine] currentActiveDialogue is null");
                    return;
                }

                var dialogue = currentActiveDialogue.GetComponent<Dialogue>();
                if (dialogue == null)
                {
                    Debug.LogWarning("[ProcessCurrentDialogueLine] No Dialogue component found");
                    return;
                }

                // Get the story
                var storyProp = dialogue.GetType().GetProperty("Story", BindingFlags.Public | BindingFlags.Instance);
                var story = storyProp?.GetValue(dialogue);
                if (story == null)
                {
                    Debug.LogWarning("[ProcessCurrentDialogueLine] Story is null");
                    return;
                }

                // Get the content
                var contentProp = story.GetType().GetProperty("Content", BindingFlags.Public | BindingFlags.Instance);
                var content = contentProp?.GetValue(story);
                if (content == null)
                {
                    Debug.LogWarning("[ProcessCurrentDialogueLine] Content is null");
                    return;
                }

                // Get the specific node by ID
                var getMethod = content.GetType().GetMethod("Get", BindingFlags.Public | BindingFlags.Instance);
                var node = getMethod?.Invoke(content, new object[] { nodeID });
                if (node == null)
                {
                    Debug.LogWarning($"[ProcessCurrentDialogueLine] Node {nodeID} not found");
                    return;
                }

                // Get the text field from the node
                var textField = node.GetType().GetField("m_Text", BindingFlags.NonPublic | BindingFlags.Instance);
                var nodeText = textField?.GetValue(node);
                if (nodeText == null)
                {
                    Debug.LogWarning("[ProcessCurrentDialogueLine] NodeText is null");
                    return;
                }

                // Get the PropertyGetString that contains the actual text
                var mTextField = nodeText.GetType().GetField("m_Text", BindingFlags.NonPublic | BindingFlags.Instance);
                var propertyGetString = mTextField?.GetValue(nodeText);
                if (propertyGetString == null)
                {
                    Debug.LogWarning("[ProcessCurrentDialogueLine] PropertyGetString is null");
                    return;
                }

                // Get the original text
                string originalText = propertyGetString.ToString();
                if (string.IsNullOrEmpty(originalText))
                {
                    Debug.Log("[ProcessCurrentDialogueLine] Original text is empty");
                    return;
                }

                Debug.Log($"[ProcessCurrentDialogueLine] Original text: {originalText}");

                // Process the text for variable replacement
                string processedText = Dialogues.ProcessTextWithVariables(originalText);

                // Only update if the text actually changed
                if (processedText != originalText)
                {
                    Debug.Log($"[ProcessCurrentDialogueLine] CHANGED: '{originalText}' -> '{processedText}'");

                    // Drill down to set the value in GetStringTextArea -> TextAreaField -> m_Text
                    var mPropertyField = propertyGetString.GetType().GetField("m_Property", BindingFlags.NonPublic | BindingFlags.Instance);
                    var propertyTypeGetString = mPropertyField?.GetValue(propertyGetString);
                    if (propertyTypeGetString != null)
                    {
                        var mTextField2 = propertyTypeGetString.GetType().GetField("m_Text", BindingFlags.NonPublic | BindingFlags.Instance);
                        var textAreaField = mTextField2?.GetValue(propertyTypeGetString);
                        if (textAreaField != null)
                        {
                            var mTextFieldInner = textAreaField.GetType().GetField("m_Text", BindingFlags.NonPublic | BindingFlags.Instance)
                                ?? textAreaField.GetType().BaseType.GetField("m_Text", BindingFlags.NonPublic | BindingFlags.Instance);
                            if (mTextFieldInner != null)
                            {
                                mTextFieldInner.SetValue(textAreaField, processedText);
                            }
                            else
                            {
                                Debug.LogWarning("[ProcessCurrentDialogueLine] Could not find m_Text field on TextAreaField or its base class");
                            }
                        }
                        else
                        {
                            Debug.LogWarning("[ProcessCurrentDialogueLine] Could not get m_Text (TextAreaField) from GetStringTextArea");
                        }
                    }
                    else
                    {
                        Debug.LogWarning("[ProcessCurrentDialogueLine] Could not get m_Property from PropertyGetString");
                    }
                }
                else
                {
                    //Debug.Log($"[ProcessCurrentDialogueLine] UNCHANGED: '{originalText}'");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[ProcessCurrentDialogueLine] Error processing dialogue line: {e.Message}");
            }
        }
        public static string GetCurrentSpeakingActor()
        {
            try
            {
                if (currentActiveDialogue == null)
                    return null;

                var dialogue = currentActiveDialogue.GetComponent<Dialogue>();
                if (dialogue == null)
                    return null;

                // Get the story
                var storyProp = dialogue.GetType().GetProperty("Story", BindingFlags.Public | BindingFlags.Instance);
                var story = storyProp?.GetValue(dialogue);
                if (story == null)
                    return null;

                // Get the current ID from the story
                var currentIdField = story.GetType().GetField("m_CurrentId", BindingFlags.NonPublic | BindingFlags.Instance);
                var currentId = currentIdField?.GetValue(story);
                if (currentId == null)
                    return null;

                int nodeId = (int)currentId;

                // Get the content
                var contentProp = story.GetType().GetProperty("Content", BindingFlags.Public | BindingFlags.Instance);
                var content = contentProp?.GetValue(story);
                if (content == null)
                    return null;

                // Get the specific node by ID
                var getMethod = content.GetType().GetMethod("Get", BindingFlags.Public | BindingFlags.Instance);
                var node = getMethod?.Invoke(content, new object[] { nodeId });
                if (node == null)
                    return null;

                // Get the actor from the node
                var actorProp = node.GetType().GetProperty("Actor", BindingFlags.Public | BindingFlags.Instance);
                var actor = actorProp?.GetValue(node);
                if (actor == null)
                    return null;

                var nameProp = actor.GetType().GetProperty("name", BindingFlags.Public | BindingFlags.Instance);
                return nameProp?.GetValue(actor) as string;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[GetCurrentSpeakingActor] Error getting current speaking actor: {e.Message}");
                return null;
            }
        }
        public static string GetCurrentActorExpression()
        {
            try
            {
                if (currentActiveDialogue == null)
                    return null;

                var dialogue = currentActiveDialogue.GetComponent<Dialogue>();
                if (dialogue == null)
                    return null;

                // Get the story
                var storyProp = dialogue.GetType().GetProperty("Story", BindingFlags.Public | BindingFlags.Instance);
                var story = storyProp?.GetValue(dialogue);
                if (story == null)
                    return null;

                // Get the current ID from the story
                var currentIdField = story.GetType().GetField("m_CurrentId", BindingFlags.NonPublic | BindingFlags.Instance);
                var currentId = currentIdField?.GetValue(story);
                if (currentId == null)
                    return null;

                int nodeId = (int)currentId;

                // Get the content
                var contentProp = story.GetType().GetProperty("Content", BindingFlags.Public | BindingFlags.Instance);
                var content = contentProp?.GetValue(story);
                if (content == null)
                    return null;

                // Get the specific node by ID
                var getMethod = content.GetType().GetMethod("Get", BindingFlags.Public | BindingFlags.Instance);
                var node = getMethod?.Invoke(content, new object[] { nodeId });
                if (node == null)
                    return null;

                // Get the actor from the node
                var actorProp = node.GetType().GetProperty("Actor", BindingFlags.Public | BindingFlags.Instance);
                var actor = actorProp?.GetValue(node);
                if (actor == null)
                    return null;

                // Get the expression index from the node
                var expressionProp = node.GetType().GetProperty("Expression", BindingFlags.Public | BindingFlags.Instance);
                var expressionIndex = expressionProp?.GetValue(node);
                if (expressionIndex == null)
                    return null;

                int index = (int)expressionIndex;

                // Get the expressions from the actor
                var expressionsField = actor.GetType().GetField("m_Expressions", BindingFlags.NonPublic | BindingFlags.Instance);
                var expressions = expressionsField?.GetValue(actor);
                if (expressions == null)
                    return null;

                // Get the expression by index
                var fromIndexMethod = expressions.GetType().GetMethod("FromIndex", BindingFlags.Public | BindingFlags.Instance);
                var expression = fromIndexMethod?.Invoke(expressions, new object[] { index });
                if (expression == null)
                    return null;

                // Get the ID from the expression
                var idProp = expression.GetType().GetProperty("Id", BindingFlags.Public | BindingFlags.Instance);
                var id = idProp?.GetValue(expression);
                return id?.ToString();
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[GetCurrentActorExpression] Error getting current actor expression: {e.Message}");
                return null;
            }
        }
        public static GameObject GetBustForActor(string actorName)
        {
            try
            {
                if (string.IsNullOrEmpty(actorName) || Core.bustManager == null)
                    return null;

                // Extract character name (remove "Actor" suffix)
                string characterName = actorName.Replace("Actor", "");
                
                // Find all busts that start with the character name
                List<GameObject> matchingBusts = new List<GameObject>();
                
                foreach (Transform child in Core.bustManager)
                {
                    if (child.name.StartsWith(characterName, StringComparison.OrdinalIgnoreCase))
                    {
                        matchingBusts.Add(child.gameObject);
                    }
                }

                if (matchingBusts.Count == 0)
                {
                    Debug.LogWarning($"[GetBustForActor] No busts found for character: {characterName}");
                    return null;
                }

                // Find the active bust among matching busts
                GameObject activeBust = matchingBusts.FirstOrDefault(bust => bust.activeSelf);
                
                if (activeBust != null)
                {
                    Debug.Log($"[GetBustForActor] Found active bust for {characterName}: {activeBust.name}");
                    return activeBust;
                }

                // If no active bust found, return the first matching bust (could be useful for fallback)
                Debug.LogWarning($"[GetBustForActor] No active bust found for {characterName}, but found {matchingBusts.Count} matching busts: {string.Join(", ", matchingBusts.Select(b => b.name))}");
                return matchingBusts.FirstOrDefault();
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[GetBustForActor] Error finding bust for actor {actorName}: {e.Message}");
                return null;
            }
        }
        private IEnumerator Fade(float startAlpha, float endAlpha, float duration, SpriteRenderer spriteRenderer)
        {
            float time = 0f;
            Color color = spriteRenderer.color;

            while (time < duration)
            {
                float t = time / duration;
                float alpha = Mathf.Lerp(startAlpha, endAlpha, t);
                spriteRenderer.color = new Color(color.r, color.g, color.b, alpha);

                time += Time.deltaTime;
                yield return null;
            }

            // Ensure it ends exactly at the target alpha
            spriteRenderer.color = new Color(color.r, color.g, color.b, endAlpha);
        }
    }
}
