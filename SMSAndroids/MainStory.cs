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
    internal class MainStory : BaseUnityPlugin
    {
        #region Plugin Info
        public const string pluginGuid = "treboy.starmakerstory.smsandroidscore.mainstory";
        #endregion

        public Places placeInstance;

        public GameObject secretBeachGatekeeper;
        public GameObject secretBeachGatekeeperB;
        public GameObject secretBeachFlash;
        public GameObject secretBeachSky;

        public Vector2 originLevelPos = Vector2.zero;

        public static bool loadedStory = false;
        public void Update()
        {
            if (Core.currentScene.name == "CoreGameScene")
            {
                if (!loadedStory && Places.loadedPlaces && Characters.loadedBusts)
                {
                    secretBeachSky = GameObject.Instantiate(Core.secretBeachLevel.transform.GetChild(1).gameObject, Core.secretBeachLevel.transform);
                    secretBeachFlash = GameObject.Instantiate(Core.secretBeachLevel.transform.GetChild(1).gameObject, Core.secretBeachLevel.transform);
                    secretBeachGatekeeper = GameObject.Instantiate(Core.secretBeachLevel.transform.GetChild(1).gameObject, Core.secretBeachLevel.transform);
                    secretBeachGatekeeperB = GameObject.Instantiate(Core.secretBeachLevel.transform.GetChild(1).gameObject, secretBeachGatekeeper.transform);
                    placeInstance.SetNewLevelSprite(secretBeachSky, Core.locationPath, "SecretBeachUp.PNG", 2048, 1729);
                    placeInstance.SetNewLevelSprite(secretBeachFlash, Core.locationPath, "Flash.PNG", 2048, 1729);
                    placeInstance.SetNewLevelSprite(secretBeachGatekeeper, Core.locationPath, "Gatekeeper.PNG", 1024, 783);
                    placeInstance.SetNewLevelSprite(secretBeachGatekeeperB, Core.locationPath, "GatekeeperB.PNG", 1024, 783);
                    secretBeachSky.name = "Sky";
                    secretBeachFlash.name = "Flash";
                    secretBeachGatekeeper.name = "Gatekeeper";
                    secretBeachGatekeeperB.name = "Portal";
                    secretBeachSky.transform.position = new Vector2(secretBeachSky.transform.position.x, 15);
                    secretBeachFlash.transform.position = new Vector2(secretBeachFlash.transform.position.x, 15);
                    secretBeachGatekeeper.transform.position = new Vector2(secretBeachSky.transform.position.x, 18);
                    Core.secretBeachLevel.transform.GetChild(1).GetComponent<MoveRelative2Mouse>().enabled = false;
                    secretBeachSky.GetComponent<MoveRelative2Mouse>().enabled = false;
                    secretBeachFlash.GetComponent<MoveRelative2Mouse>().enabled = false;
                    secretBeachFlash.GetComponent<SpriteRenderer>().sortingOrder = -9;
                    secretBeachGatekeeper.GetComponent<MoveRelative2Mouse>().enabled = false;
                    secretBeachGatekeeper.GetComponent<SpriteRenderer>().sortingOrder = -10;
                    secretBeachGatekeeperB.GetComponent<MoveRelative2Mouse>().enabled = false;
                    secretBeachGatekeeperB.GetComponent<SpriteRenderer>().sortingOrder = -11;
                    secretBeachFlash.SetActive(false);
                    secretBeachGatekeeperB.SetActive(false);
                    secretBeachGatekeeperB.GetComponent<SpriteRenderer>().color = new Color(secretBeachGatekeeperB.GetComponent<SpriteRenderer>().color.r,
                        secretBeachGatekeeperB.GetComponent<SpriteRenderer>().color.g, secretBeachGatekeeperB.GetComponent<SpriteRenderer>().color.b, 0);
                    secretBeachGatekeeperB.AddComponent<FadeInSprite2>();
                    secretBeachGatekeeperB.GetComponent<FadeInSprite2>().fadeInDuration = 1f;
                    secretBeachFlash.AddComponent<FadeOutSprite>();
                    originLevelPos = Core.secretBeachLevel.transform.position;

                    Core.dialogueBeachMainInstance = GameObject.Instantiate(Core.dialogueBundle.LoadAsset<GameObject>("SBDialogueMain"), beach.transform);
                    dialogueBeachMainInstance.GetComponent<Dialogue>().Story.Content.DialogueSkin = GameObject.Find("8_Core_Events").transform.Find("SmallTalks").Find("FailedGroceries").GetComponent<Dialogue>().Story.Content.DialogueSkin;
                    dialogueBeachMainInstance = GameObject.Instantiate(dialogueBeachMain, beach.transform);
                    beachMainDialogueActivator = dialogueBeachMainInstance.transform.Find("DialogueActivator").gameObject;
                    beachMainDialogueFinisher = dialogueBeachMainInstance.transform.Find("DialogueFinisher").gameObject;
                    beachMainDialogueSpriteFocus = dialogueBeachMainInstance.transform.Find("SpriteFocus").gameObject;
                    dialogueBeachMainGK = dialogueBundle.LoadAsset<GameObject>("SBDialogueMainGatekeeper");
                    dialogueBeachMainDialogueGK = dialogueBeachMainGK.GetComponent<Dialogue>();
                    dialogueBeachMainDialogueGK.Story.Content.DialogueSkin = baseDialogueObject.GetComponent<Dialogue>().Story.Content.DialogueSkin;
                    dialogueBeachMainGKInstance = GameObject.Instantiate(dialogueBeachMainGK, beach.transform);
                    beachMainDialogueGKScene1 = dialogueBeachMainGKInstance.transform.Find("Scene1").gameObject;
                    beachMainDialogueGKScene2 = dialogueBeachMainGKInstance.transform.Find("Scene2").gameObject;
                    beachMainDialogueGKScene3 = dialogueBeachMainGKInstance.transform.Find("Scene3").gameObject;
                    beachMainDialogueGKScene4 = dialogueBeachMainGKInstance.transform.Find("Scene4").gameObject;
                    beachMainDialogueGKScene5 = dialogueBeachMainGKInstance.transform.Find("Scene5").gameObject;
                    beachMainDialogueGKBlinkActivator = dialogueBeachMainGKInstance.transform.Find("MouthActivator").gameObject;
                    beachMainDialogueGKActivator = dialogueBeachMainGKInstance.transform.Find("DialogueActivator").gameObject;
                    beachMainDialogueGKFinisher = dialogueBeachMainGKInstance.transform.Find("DialogueFinisher").gameObject;
                    beachMainDialogueGKSpriteFocus = dialogueBeachMainGKInstance.transform.Find("SpriteFocus").gameObject;

                    Logger.LogInfo("----- STORY LOADED -----");
                    loadedStory = true;
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
    }
}
