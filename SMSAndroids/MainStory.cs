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
        public static bool voyeurDialoguePlaying = false;
        public static bool relaxed = false;
        public static GameObject currentActiveBust;
        public static GameObject currentActiveBustMBase;
        public static GameObject currentActiveDialogue;
        public static GameObject currentActiveDialogueSpriteFocus;
        public static int voyeurLotteryNumber = 0;
        private string currentVoyeurTarget;

        public static string[] voyeurTargets = { "Anis", "Frima", "Guilty", "Helm", "Maiden", "Mary", "Mast", "Neon", "Pepper", "Rapi", "Rosanna", "Sakura", "Viper", "Yan" };
        public static List<string> voyeurTargetsLeft = new List<string>();

        private GameObject dialogueToActivate;
        private Vector2 refVelocity = Vector2.zero;

        public void Update()
        {
            if (Core.currentScene.name == "CoreGameScene")
            {
                if (!loadedStory && Places.loadedPlaces && Characters.loadedBusts && Dialogues.loadedDialogues)
                {
                    voyeurTargetsLeft.Clear();
                    foreach (string character in voyeurTargets)
                    {
                        if (!SaveManager.GetBool($"Voyeur_Seen{character}"))
                        {
                            voyeurTargetsLeft.Add(character);
                        }
                    }
                    
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
                            relaxed = true;
                        }
                        if (Dialogues.sBDialogueMainDialogueFinisher.activeSelf)
                        {
                            if (SaveManager.GetInt("SecretBeach_RelaxedAmount") > 2 && voyeurLotteryNumber < 36 && relaxed && voyeurTargetsLeft.Count != 0)
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
                                this.dialogueToActivate = Places.secretBeachRoomtalk.transform.Find(currentVoyeurTarget + "DialogueBeach01").gameObject;
                                currentActiveDialogue = Places.secretBeachRoomtalk.transform.Find(currentVoyeurTarget + "DialogueBeach01").gameObject;
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
//------------------------------------------------------------------------------------------------ SB Event Voyeur
                        if (Dialogues.anisBeachDialogueScene2.activeSelf && !Dialogues.anisBeachDialogueScene3.activeSelf && !Characters.anisSwimWet.activeSelf ) { ChangeActiveBust(Characters.anisSwim, Characters.anisSwimWet); }
                        if (Dialogues.anisBeachDialogueScene3.activeSelf && !Characters.anisSwimSlip.activeSelf) { ChangeActiveBust(Characters.anisSwimWet, Characters.anisSwimSlip); }
                        if (Dialogues.anisBeachDialogueFinisher.activeSelf) { Characters.anisSwimSlip.transform.Find("MBase1").Find("Leave").gameObject.SetActive(true); }

                        if (Dialogues.frimaBeachDialogueScene2.activeSelf && !Dialogues.frimaBeachDialogueScene4.activeSelf && !Characters.frimaSwimShirtless.activeSelf) { ChangeActiveBust(Characters.frimaSwim, Characters.frimaSwimShirtless); }
                        if (Dialogues.frimaBeachDialogueScene4.activeSelf && !Characters.frimaSwimSlip.activeSelf) { ChangeActiveBust(Characters.frimaSwimShirtless, Characters.frimaSwimSlip); }
                        if (Dialogues.frimaBeachDialogueFinisher.activeSelf) { Characters.frimaSwimSlip.transform.Find("MBase1").Find("Leave").gameObject.SetActive(true); }

                        if (Dialogues.guiltyBeachDialogueScene4.activeSelf && !Characters.guiltySwimSlip.activeSelf) { ChangeActiveBust(Characters.guiltySwim, Characters.guiltySwimSlip); }
                        if (Dialogues.guiltyBeachDialogueFinisher.activeSelf) { Characters.guiltySwimSlip.transform.Find("MBase1").Find("Leave").gameObject.SetActive(true); }

                        if (Dialogues.helmBeachDialogueScene2.activeSelf && !Dialogues.helmBeachDialogueScene3.activeSelf && !Characters.helmSwimWet.activeSelf) { ChangeActiveBust(Characters.helmSwim, Characters.helmSwimWet); }
                        if (Dialogues.helmBeachDialogueScene3.activeSelf && !Dialogues.helmBeachDialogueScene4.activeSelf && !Characters.helmSwimShirtless.activeSelf) { ChangeActiveBust(Characters.helmSwimWet, Characters.helmSwimShirtless); }
                        if (Dialogues.helmBeachDialogueScene4.activeSelf && !Characters.helmSwimSlip.activeSelf) { ChangeActiveBust(Characters.helmSwimShirtless, Characters.helmSwimSlip); }
                        if (Dialogues.helmBeachDialogueFinisher.activeSelf) { Characters.helmSwimSlip.transform.Find("MBase1").Find("Leave").gameObject.SetActive(true); }

                        if (Dialogues.maidenBeachDialogueScene4.activeSelf && !Characters.maidenSwimSlip.activeSelf) { ChangeActiveBust(Characters.maidenSwim, Characters.maidenSwimSlip); }
                        if (Dialogues.maidenBeachDialogueFinisher.activeSelf) { Characters.maidenSwimSlip.transform.Find("MBase1").Find("Leave").gameObject.SetActive(true); }

                        if (Dialogues.maryBeachDialogueScene3.activeSelf && !Characters.marySwimSlip.activeSelf) { ChangeActiveBust(Characters.marySwim, Characters.marySwimSlip); }
                        if (Dialogues.maryBeachDialogueFinisher.activeSelf) { Characters.marySwimSlip.transform.Find("MBase1").Find("Leave").gameObject.SetActive(true); }

                        if (Dialogues.mastBeachDialogueScene4.activeSelf && !Characters.mastSwimSlip.activeSelf) { ChangeActiveBust(Characters.mastSwim, Characters.mastSwimSlip); }
                        if (Dialogues.mastBeachDialogueFinisher.activeSelf) { Characters.mastSwimSlip.transform.Find("MBase1").Find("Leave").gameObject.SetActive(true); }

                        if (Dialogues.neonBeachDialogueScene3.activeSelf && !Dialogues.neonBeachDialogueScene4.activeSelf && !Characters.neonSwimWet.activeSelf) { ChangeActiveBust(Characters.neonSwim, Characters.neonSwimWet); }
                        if (Dialogues.neonBeachDialogueScene4.activeSelf && !Characters.neonSwimSlip.activeSelf) { ChangeActiveBust(Characters.neonSwimWet, Characters.neonSwimSlip); }
                        if (Dialogues.neonBeachDialogueFinisher.activeSelf) { Characters.neonSwimSlip.transform.Find("MBase1").Find("Leave").gameObject.SetActive(true); }

                        if (Dialogues.pepperBeachDialogueScene3.activeSelf && !Characters.pepperSwimSlip.activeSelf) { ChangeActiveBust(Characters.pepperSwim, Characters.pepperSwimSlip); }
                        if (Dialogues.pepperBeachDialogueFinisher.activeSelf) { Characters.pepperSwimSlip.transform.Find("MBase1").Find("Leave").gameObject.SetActive(true); }

                        if (Dialogues.rapiBeachDialogueScene4.activeSelf && !Characters.rapiSwimSlip.activeSelf) { ChangeActiveBust(Characters.rapiSwim, Characters.rapiSwimSlip); }
                        if (Dialogues.rapiBeachDialogueFinisher.activeSelf) { Characters.rapiSwimSlip.transform.Find("MBase1").Find("Leave").gameObject.SetActive(true); }

                        if (Dialogues.rosannaBeachDialogueScene4.activeSelf && !Characters.rosannaSwimSlip.activeSelf) { ChangeActiveBust(Characters.rosannaSwim, Characters.rosannaSwimSlip); }
                        if (Dialogues.rosannaBeachDialogueFinisher.activeSelf) { Characters.rosannaSwimSlip.transform.Find("MBase1").Find("Leave").gameObject.SetActive(true); }

                        if (Dialogues.sakuraBeachDialogueScene4.activeSelf && !Characters.sakuraSwimSlip.activeSelf) { ChangeActiveBust(Characters.sakuraSwim, Characters.sakuraSwimSlip); }
                        if (Dialogues.sakuraBeachDialogueFinisher.activeSelf) { Characters.sakuraSwimSlip.transform.Find("MBase1").Find("Leave").gameObject.SetActive(true); }

                        if (Dialogues.viperBeachDialogueScene1.activeSelf && !Dialogues.viperBeachDialogueScene2.activeSelf && !Characters.viperSwimShirtless.activeSelf) { ChangeActiveBust(Characters.viperSwim, Characters.viperSwimShirtless); }
                        if (Dialogues.viperBeachDialogueScene2.activeSelf && !Dialogues.viperBeachDialogueScene4.activeSelf && !Characters.viperSwimWet.activeSelf) { ChangeActiveBust(Characters.viperSwimShirtless, Characters.viperSwimWet); }
                        if (Dialogues.viperBeachDialogueScene4.activeSelf && !Characters.viperSwimSlip.activeSelf) { ChangeActiveBust(Characters.viperSwimWet, Characters.viperSwimSlip); }
                        if (Dialogues.viperBeachDialogueFinisher.activeSelf) { Characters.viperSwimSlip.transform.Find("MBase1").Find("Leave").gameObject.SetActive(true); }

                        if (Dialogues.yanBeachDialogueScene4.activeSelf && !Characters.yanSwimSlip.activeSelf) { ChangeActiveBust(Characters.yanSwim, Characters.yanSwimSlip); }
                        if (Dialogues.yanBeachDialogueFinisher.activeSelf) { Characters.yanSwimSlip.transform.Find("MBase1").Find("Leave").gameObject.SetActive(true); }

                        if (currentActiveBust != null) { Scenes.DialogueScenePlayer(Core.cGManagerSexy, currentVoyeurTarget + "Beach", currentActiveDialogue); }
                        if (currentActiveBust != null && currentActiveDialogueSpriteFocus.activeSelf && currentActiveBustMBase.GetComponent<SpriteRenderer>().sortingOrder != 17) { ChangeBustSortingOrder(currentActiveBustMBase, 17); }
                        if (currentActiveBust != null && !currentActiveDialogueSpriteFocus.activeSelf && currentActiveBustMBase.GetComponent<SpriteRenderer>().sortingOrder != 0) { ChangeBustSortingOrder(currentActiveBustMBase, 0); }

                        if (voyeurDialoguePlaying)
                        {
                            if (currentActiveDialogue.transform.Find("DialogueFinisher").gameObject.activeSelf)
                            {
                                Invoke(nameof(EndDialogueSequence), 1.0f);
                                Core.cGManagerSexy.Find(currentVoyeurTarget + "BeachScene01").gameObject.SetActive(false);
                                Core.cGManagerSexy.Find(currentVoyeurTarget + "BeachScene02").gameObject.SetActive(false);
                                Core.cGManagerSexy.Find(currentVoyeurTarget + "BeachScene03").gameObject.SetActive(false);
                                Core.cGManagerSexy.Find(currentVoyeurTarget + "BeachScene04").gameObject.SetActive(false);
                                currentActiveDialogue.transform.Find("DialogueFinisher").gameObject.SetActive(false);
                                currentActiveBustMBase.transform.Find("Leave").gameObject.SetActive(true);
                                SaveManager.SetBool("Voyeur_Seen" + currentVoyeurTarget, true);
                                voyeurDialoguePlaying = false;
                                actionTodaySB = true;
                            }
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
            currentActiveDialogue = diag;
            currentActiveDialogueSpriteFocus = diag.transform.Find("SpriteFocus").gameObject;
            Signals.Emit(fadeUISignal);
            Invoke(nameof(PlayDialogueStep), 1.0f);
        }
        private void PlayDialogueStep()
        {
            this.dialogueToActivate.transform.Find("DialogueActivator").gameObject.SetActive(true);
        }
        public void EndDialogueSequence()
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
            currentActiveBustMBase = newBust.transform.Find("MBase1").gameObject;
        }
        public void ChangeBustSortingOrder(GameObject mBase, int order)
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
    }
}
