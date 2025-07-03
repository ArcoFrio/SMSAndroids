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
    [BepInPlugin(pluginGuid, Core.pluginName + " - Places", Core.pluginVersion)]
    internal class Places : BaseUnityPlugin
    {
        #region Plugin Info
        public const string pluginGuid = "treboy.starmakerstory.smsandroidscore.places";
        #endregion
        
        public static GameObject buttonBeach;
        public static GameObject buttonSecretBeach;
        public static GameObject buttonMountainLab;
        public static GameObject buttonMountainLabRooms1;

        public static GameObject secretBeachLevel;
        public static GameObject secretBeachLevelBG;
        public static GameObject secretBeachRoomtalk;
        public static GameObject secretBeachGatekeeper;
        public static GameObject secretBeachGatekeeperB;
        public static GameObject secretBeachFlash;
        public static GameObject secretBeachSky;

        public static GameObject mountainLabLevel;
        public static GameObject mountainLabRoomtalk;
        public static GameObject mountainLabRooms1Level;
        public static GameObject mountainLabRooms1Roomtalk;

        public Vector2 originLevelPos = Vector2.zero;
        public int vanillaLevelCount;

        public static bool loadedPlaces = false;
        public static GameObject currentRoomTalk;
        public void Update()
        {
            if (Core.currentScene.name == "CoreGameScene")
            {
                if (!loadedPlaces && Core.loadedCore)
                {
                    vanillaLevelCount = Core.level.childCount;

                    CreateNewPlace(900, "SecretBeach", "Remote Area", 0.4f, 0.3f);
                    CreateNewPlace(901, "MountainLab", "Underground Lab", 0.4f, 0.3f);
                    CreateNewPlace(902, "MountainLabRooms1", "Sector A", 0.4f, 0.3f);

                    buttonBeach = Core.mainCanvas.Find("Navigator").Find("MapButtons").Find("14_beach").gameObject;
                    buttonSecretBeach = Core.mainCanvas.Find("Navigator").Find("MapButtons").Find("900_SecretBeach").gameObject;
                    buttonMountainLab = Core.mainCanvas.Find("Navigator").Find("MapButtons").Find("901_MountainLab").gameObject;
                    buttonMountainLabRooms1 = Core.mainCanvas.Find("Navigator").Find("MapButtons").Find("902_MountainLabRooms1").gameObject;

                    secretBeachLevel = Core.level.Find("900_SecretBeach").gameObject;
                    secretBeachLevelBG = secretBeachLevel.transform.GetChild(1).gameObject;
                    secretBeachRoomtalk = Core.roomTalk.Find("SecretBeach").gameObject;
                    mountainLabLevel = Core.level.Find("901_MountainLab").gameObject;
                    mountainLabRoomtalk = Core.roomTalk.Find("MountainLab").gameObject;
                    mountainLabRooms1Level = Core.level.Find("902_MountainLabRooms1").gameObject;
                    mountainLabRooms1Roomtalk = Core.roomTalk.Find("MountainLabRooms1").gameObject;

                    secretBeachSky = GameObject.Instantiate(Places.secretBeachLevel.transform.GetChild(1).gameObject, Places.secretBeachLevel.transform);
                    secretBeachFlash = GameObject.Instantiate(Places.secretBeachLevel.transform.GetChild(1).gameObject, Places.secretBeachLevel.transform);
                    secretBeachGatekeeper = GameObject.Instantiate(Places.secretBeachLevel.transform.GetChild(1).gameObject, Places.secretBeachLevel.transform);
                    secretBeachGatekeeperB = GameObject.Instantiate(Places.secretBeachLevel.transform.GetChild(1).gameObject, secretBeachGatekeeper.transform);
                    SetNewLevelSprite(secretBeachSky, Core.locationPath, "SecretBeachUp.PNG", 2048, 1729);
                    SetNewLevelSprite(secretBeachFlash, Core.locationPath, "Flash.PNG", 2048, 1729);
                    SetNewLevelSprite(secretBeachGatekeeper, Core.locationPath, "Gatekeeper.PNG", 1024, 783);
                    SetNewLevelSprite(secretBeachGatekeeperB, Core.locationPath, "GatekeeperB.PNG", 1024, 783);
                    secretBeachSky.name = "Sky";
                    secretBeachFlash.name = "Flash";
                    secretBeachGatekeeper.name = "Gatekeeper";
                    secretBeachGatekeeperB.name = "Portal";
                    secretBeachSky.transform.position = new Vector2(secretBeachSky.transform.position.x, 15);
                    secretBeachFlash.transform.position = new Vector2(secretBeachFlash.transform.position.x, 15);
                    secretBeachGatekeeper.transform.position = new Vector2(secretBeachSky.transform.position.x, 18);
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
                    originLevelPos = Places.secretBeachLevel.transform.position;

                    Logger.LogInfo("----- PLACES LOADED -----");
                    loadedPlaces = true;
                }
            }
            if (Core.currentScene.name == "GameStart")
            {
                if (loadedPlaces)
                {
                    Logger.LogInfo("----- PLACES UNLOADED -----");
                    loadedPlaces = false;
                }
            }

            if (Core.loadedBases)
            {
                if (!buttonSecretBeach.activeSelf && (Core.levelBeach.activeSelf || mountainLabLevel.activeSelf))
                {
                    buttonSecretBeach.SetActive(true);
                }
                if (buttonSecretBeach.activeSelf && (!Core.levelBeach.activeSelf && !mountainLabLevel.activeSelf))
                {
                    buttonSecretBeach.SetActive(false);
                }
                if (!buttonMountainLab.activeSelf && (secretBeachLevel.activeSelf || mountainLabRooms1Level.activeSelf) && SaveManager.GetBool("SecretBeach_UnlockedLab"))
                {
                    buttonMountainLab.SetActive(true);
                }
                if (buttonMountainLab.activeSelf && (!secretBeachLevel.activeSelf && !mountainLabRooms1Level.activeSelf))
                {
                    buttonMountainLab.SetActive(false);
                }
                if (!buttonMountainLabRooms1.activeSelf && mountainLabLevel.activeSelf)
                {
                    buttonMountainLabRooms1.SetActive(true);
                }
                if (buttonMountainLabRooms1.activeSelf && !mountainLabLevel.activeSelf)
                {
                    buttonMountainLabRooms1.SetActive(false);
                }

                if (secretBeachLevel.activeSelf && !buttonBeach.activeSelf)
                {
                    buttonBeach.SetActive(true);
                }

                if (buttonSecretBeach.transform.GetChild(0).gameObject.activeSelf)
                {
                    ClickMapButton(secretBeachRoomtalk, 0);
                    buttonSecretBeach.transform.GetChild(0).gameObject.SetActive(false);
                }
                if (buttonMountainLab.transform.GetChild(0).gameObject.activeSelf)
                {
                    ClickMapButton(mountainLabRoomtalk, 1);
                    buttonMountainLab.transform.GetChild(0).gameObject.SetActive(false);
                }
                if (buttonMountainLabRooms1.transform.GetChild(0).gameObject.activeSelf)
                {
                    ClickMapButton(mountainLabRooms1Roomtalk, 2);
                    buttonMountainLabRooms1.transform.GetChild(0).gameObject.SetActive(false);
                }

                if (Places.secretBeachLevel.activeSelf)
                {
                    secretBeachGatekeeper.transform.Rotate(0, 0, 1f * Time.deltaTime);
                }
            }
        }

        public void CreateNewPlace(int index, string name, string buttonText, float moveRelativeForeground, float moveRelativeBackground)
        {
            // Variables
            GameObject baseMapButton = GameObject.Find("9_MainCanvas").transform.Find("Navigator").Find("MapButtons").Find("14_beach").gameObject;

            // Button
            GameObject mapButton = GameObject.Instantiate(Core.otherBundle.LoadAsset<GameObject>(index + "_" + name), baseMapButton.transform.parent);
            GameObject mapButtonText = GameObject.Instantiate(baseMapButton.transform.GetChild(0).gameObject, mapButton.transform);
            GameObject mapButtonImage = GameObject.Instantiate(baseMapButton.transform.GetChild(1).gameObject, mapButton.transform);
            GameObject mapButtonImage1 = GameObject.Instantiate(baseMapButton.transform.GetChild(2).gameObject, mapButton.transform);
            GameObject mapButtonKBNumber = GameObject.Instantiate(baseMapButton.transform.GetChild(3).gameObject, mapButton.transform);
            mapButton.name = index + "_" + name;
            mapButtonText.name = "Text (TMP)";
            mapButtonImage.name = "Image";
            mapButtonImage1.name = "Image (1)";
            mapButtonKBNumber.name = "keyboardnumber";
            mapButton.GetComponent<UnityEngine.UI.Image>().sprite = baseMapButton.GetComponent<UnityEngine.UI.Image>().sprite;
            mapButtonText.GetComponent<TextMeshProUGUI>().text = buttonText;
            mapButtonImage.GetComponent<UnityEngine.UI.Image>().sprite = baseMapButton.transform.GetChild(1).gameObject.GetComponent<UnityEngine.UI.Image>().sprite;
            mapButtonKBNumber.GetComponent<TextMeshProUGUI>().text = "2";

            // Level
            GameObject level = CreateNewLevel(index + "_" + name, Core.locationPath, name + ".PNG", name + "B.PNG", name + "Mask.PNG", moveRelativeForeground, moveRelativeBackground);
            Destroy(level.GetComponent<Trigger>());

            // RoomTalk
            GameObject roomTalk = GameObject.Instantiate(Core.roomTalk.Find("Beach").gameObject, Core.roomTalk);
            roomTalk.name = name;
            for (int i = roomTalk.transform.childCount - 1; i > 0; i--)
            {
                Destroy(roomTalk.transform.GetChild(i).gameObject);
            }
            Destroy(roomTalk.GetComponent<Conditions>());
        }
        public GameObject CreateNewLevel(string name, string pathToCG, string baseSprite, string secondarySprite, string maskSprite, float moveRelativeForeground, float moveRelativeBackground)
        {
            GameObject newLevel = GameObject.Instantiate(GameObject.Find("5_Levels").transform.Find("14_Beach").gameObject, GameObject.Find("5_Levels").transform);
            newLevel.name = name;
            newLevel.GetComponent<MoveRelative2Mouse>().moveModifier = moveRelativeForeground;
            GameObject secondaryTex = newLevel.transform.GetChild(1).gameObject;
            secondaryTex.name = name;
            secondaryTex.GetComponent<MoveRelative2Mouse>().moveModifier = moveRelativeBackground;
            GameObject NPCs = newLevel.transform.GetChild(2).gameObject;
            foreach (Transform npc in NPCs.transform)
            {
                Destroy(npc.gameObject);
            }
            Material mat = new Material(newLevel.GetComponent<SpriteRenderer>().material);

            var rawData = System.IO.File.ReadAllBytes(pathToCG + baseSprite);
            Texture2D tex = new Texture2D(2048, 1136);
            tex.filterMode = FilterMode.Bilinear;
            ImageConversion.LoadImage(tex, rawData);
            Sprite newSprite = Sprite.Create(tex, new Rect(0, 0, 2048, 1136), new Vector2(0.5f, 0.5f), 70.32f);
            newLevel.GetComponent<SpriteRenderer>().sprite = newSprite;

            rawData = System.IO.File.ReadAllBytes(pathToCG + secondarySprite);
            tex = new Texture2D(2048, 1136);
            tex.filterMode = FilterMode.Point;
            ImageConversion.LoadImage(tex, rawData);
            newSprite = Sprite.Create(tex, new Rect(0, 0, 2048, 1136), new Vector2(0.5f, 0.5f), 70.32f);
            secondaryTex.GetComponent<SpriteRenderer>().sprite = newSprite;

            rawData = System.IO.File.ReadAllBytes(pathToCG + maskSprite);
            tex = new Texture2D(256, 143, TextureFormat.RGBA32, false);
            tex.LoadImage(rawData);
            tex.filterMode = FilterMode.Point;
            mat.SetTexture("_MaskTex", tex);
            newLevel.GetComponent<SpriteRenderer>().material = mat;
            newLevel.GetComponent<SpriteRenderer>().material.SetTexture("_MaskTex", tex);

            newLevel.SetActive(false);
            return newLevel;
        }
        public void SetNewLevelSprite(GameObject gO, string pathToCG, string baseSprite, int width, int height)
        {
            Material mat = new Material(gO.GetComponent<SpriteRenderer>().material);

            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            var rawData = System.IO.File.ReadAllBytes(pathToCG + baseSprite);
            tex.LoadImage(rawData);
            tex.filterMode = FilterMode.Point;
            Sprite newSprite = Sprite.Create(tex, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 70.32f);
            gO.GetComponent<SpriteRenderer>().sprite = newSprite;
        }
        public void ClickMapButton(GameObject roomTalk, Double index)
        {
            if (!Core.GetVariableBool("Lock-Game"))
            {
                currentRoomTalk = roomTalk;
                Core.FindAndModifyVariableDouble("Upcoming-Level", vanillaLevelCount + index);
                Core.FindAndModifyVariableBool("Start-Transfer", true);
                
                var triggerChangeLevel = Core.gameplay?.Find("TransferScene")?.gameObject;
                if (triggerChangeLevel != null)
                {
                    var triggerComponent = triggerChangeLevel.GetComponent<Trigger>();
                    if (triggerComponent != null)
                    {
                        Debug.Log("[Places] Executing TransferScene trigger");
                        triggerComponent.Execute();
                    }
                }

                Invoke(nameof(EnableRoomTalk), 1.5f);
            }
        }
        private void EnableRoomTalk()
        {
            currentRoomTalk.SetActive(true);
        }
        public static bool GetBadWeather()
        {
            if (Core.GetVariableBool("rainy-day") || Core.GetVariableBool("snowy-day"))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

    }
}
