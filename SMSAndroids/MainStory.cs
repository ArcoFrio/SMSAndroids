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
    [BepInPlugin(pluginGuid, Core.pluginName + " - Story", Core.pluginVersion)]
    internal class MainStory : BaseUnityPlugin
    {
        #region Plugin Info
        public const string pluginGuid = "treboy.starmakerstory.smsandroidscore.mainstory";
        #endregion

        public static bool loadedStory = false;
        public static SignalArgs fadeUISignal = new SignalArgs(new PropertyName("FadeUI"), null);
        public static bool actionTodaySB = false;

        private GameObject dialogueToActivate;
        private Vector2 refVelocity = Vector2.zero;
        public void Update()
        {
            if (Core.currentScene.name == "CoreGameScene")
            {
                if (!loadedStory && Places.loadedPlaces && Characters.loadedBusts && Dialogues.loadedDialogues)
                {
                    Logger.LogInfo("----- STORY LOADED -----");
                    loadedStory = true;
                }

                if (loadedStory)
                {
//------------------------------------------------------------------------------------------------ SECRET BEACH
                    if (!actionTodaySB)
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
                        if (!Dialogues.dialoguePlaying && Places.secretBeachRoomtalk.activeSelf && SaveManager.GetBool("SecretBeach_FirstVisited") && SaveManager.GetInt("SecretBeach_RelaxedAmount") != 2)
                        {
                            StartDialogueSequence(Dialogues.sBDialogueMain);
                        }
                        if (Dialogues.sBDialogueMainScene1.activeSelf)
                        {
                            Dialogues.sBDialogueMainScene1.SetActive(false);
                            SaveManager.SetInt("SecretBeach_RelaxedAmount", SaveManager.GetInt("SecretBeach_RelaxedAmount") + 1);
                        }
                        if (Dialogues.sBDialogueMainDialogueFinisher.activeSelf)
                        {
                            Invoke(nameof(EndDialogueSequence), 1.0f);
                            Dialogues.sBDialogueMainDialogueFinisher.SetActive(false);
                            actionTodaySB = true;
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
                            Places.secretBeachLevel.GetComponent<MoveRelative2Mouse>().enabled = false;
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
                            Places.secretBeachLevel.GetComponent<MoveRelative2Mouse>().enabled = true;
                            actionTodaySB = true;
                        }
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
            Signals.Emit(fadeUISignal);
            Invoke(nameof(PlayDialogueStep), 1.0f);
        }
        private void PlayDialogueStep()
        {
            this.dialogueToActivate.transform.Find("DialogueActivator").gameObject.SetActive(true);
        }
        private void EndDialogueSequence()
        {
            Signals.Emit(fadeUISignal);
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
        }
    }
}
