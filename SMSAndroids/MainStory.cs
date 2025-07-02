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
                    if (SaveManager.GetBool("SecretBeach_GKSeen") != true && SaveManager.GetInt("SecretBeach_RelaxedAmount") > 2)
                    {
                        SaveManager.SetBool("SecretBeach_GKSeen", true);
                    }

                    if (!Dialogues.dialoguePlaying && Places.GetBadWeather())
                    {
                        StartDialogueSequence(Dialogues.badWeatherDialogue);
                    }
                    if (Dialogues.badWeatherDialogueFinisher.activeSelf)
                    {
                        Invoke(nameof(EndDialogueSequence), 1.0f);
                        Dialogues.badWeatherDialogueFinisher.SetActive(false);
                    }



//------------------------------------------------------------------------------------------------ MOUNTAIN LAB
                    if (!Dialogues.dialoguePlaying && Places.mountainLabRoomtalk.activeSelf && !SaveManager.GetBool("MountainLab_FirstVisited"))
                    {
                        StartDialogueSequence(Dialogues.mLDialogueMainFirst);
                        Characters.amberSwim.SetActive(true);
                    }
                    if (Dialogues.mLDialogueMainFirstScene1.activeSelf)
                    {
                        Characters.amberSwim.transform.Find("MBase1").Find("Leave").gameObject.SetActive(true);
                    }
                    if (Dialogues.mLDialogueMainFirstDialogueFinisher.activeSelf)
                    {
                        Invoke(nameof(EndDialogueSequence), 1.0f);
                        Dialogues.mLDialogueMainFirstDialogueFinisher.SetActive(false);
                        SaveManager.SetBool("MountainLab_FirstVisited", true);
                    }



//------------------------------------------------------------------------------------------------ SECRET BEACH
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
                            Places.secretBeachLevelBG.GetComponent<MoveRelative2Mouse>().enabled = false;
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
                            Places.secretBeachLevelBG.GetComponent<MoveRelative2Mouse>().enabled = true;
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
                            Scenes.amberStareScene1.SetActive(true);
                        }
                        if (Dialogues.sBDialogueStory01DialogueFinisher.activeSelf)
                        {
                            Invoke(nameof(EndDialogueSequence), 1.0f);
                            Dialogues.sBDialogueStory01DialogueFinisher.SetActive(false);
                            Characters.amberSwim.transform.Find("MBase1").Find("Leave").gameObject.SetActive(true);
                            Scenes.amberStareScene1.SetActive(false);
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

                        if (currentActiveBust != null && voyeurDialoguePlaying) { Scenes.DialogueScenePlayer(Core.cGManagerSexy, currentVoyeurTarget + "Beach", currentActiveDialogue); }
                        if (currentActiveBust != null && currentActiveDialogueSpriteFocus.activeSelf && currentActiveBustMBase.GetComponent<SpriteRenderer>().sortingOrder != 17) { SpriteFocusChange(true); }
                        if (currentActiveBust != null && !currentActiveDialogueSpriteFocus.activeSelf && currentActiveBustMBase.GetComponent<SpriteRenderer>().sortingOrder != 0) { SpriteFocusChange(false); }

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
        public static void OnDialogueLineStart(int nodeID)
        {
            string actor = GetCurrentSpeakingActor();
            string expression = GetCurrentActorExpression();

            if (!string.IsNullOrEmpty(actor) && !string.IsNullOrEmpty(expression))
            {
                Debug.Log($"[OnDialogueLineStart] Actor: {actor}, Expression: {expression}");
            }

            // Set the current active bust based on the speaking actor
            if (!string.IsNullOrEmpty(actor) && actor != "PlayerActor")
            {
                GameObject bustForActor = GetBustForActor(actor);
                if (bustForActor != null)
                {
                    currentActiveBust = bustForActor;
                    currentActiveBustMBase = bustForActor.transform.Find("MBase1").gameObject;
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
                        currentActiveBustMBase.transform.Find("Expressions").Find(expression).gameObject.SetActive(true);
                    }
                }
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
    }
}
