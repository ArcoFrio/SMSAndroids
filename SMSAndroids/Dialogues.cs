using BepInEx;
using GameCreator.Runtime.Dialogue;
using GameCreator.Runtime.VisualScripting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SMSAndroidsCore
{
    [BepInPlugin(pluginGuid, Core.pluginName, Core.pluginVersion)]
    internal class Dialogues : BaseUnityPlugin
    {
        #region Plugin Info
        public const string pluginGuid = "treboy.starmakerstory.smsandroidscore.dialogues";
        #endregion
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

        public static bool loadedDialogues = false;
        public void Update()
        {
            if (Core.currentScene.name == "CoreGameScene")
            {
                if (!loadedDialogues && Places.loadedPlaces)
                {
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
            GameObject dialogue = Core.dialogueBundle.LoadAsset<GameObject>(bundleAsset);
            GameObject dialogueInstance = GameObject.Instantiate(dialogue, roomTalk);
            dialogueInstance.GetComponent<Dialogue>().Story.Content.DialogueSkin = Core.coreEvents.Find("SmallTalks").Find("FailedGroceries").Find("GameObject").GetComponent<Dialogue>().Story.Content.DialogueSkin;
            return dialogueInstance;
        }
    }
}
