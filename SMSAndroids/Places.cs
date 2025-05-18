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
    internal class Places : BaseUnityPlugin
    {
        #region Plugin Info
        public const string pluginGuid = "treboy.starmakerstory.smsandroidscore.places";
        #endregion
        
        public static GameObject secretBeachButton;
        public static GameObject secretBeachLevel;
        public static GameObject secretBeachRoomtalk;

        public static bool loadedPlaces = false;
        public void Update()
        {
            if (Core.currentScene.name == "CoreGameScene")
            {
                if (!loadedPlaces)
                {
                    CreateNewPlace(900, "SecretBeach", "Remote Area");

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
        }

        public void CreateNewPlace(int index, string name, string buttonText)
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
            GameObject level = CreateNewLevel(index + "_" + name, Core.locationPath, name + ".PNG", name + "B.PNG", name + "Mask.PNG");

            // RoomTalk
            GameObject roomTalk = GameObject.Instantiate(GameObject.Find("8_Room_Talk").transform.Find("Beach").gameObject, GameObject.Find("8_Room_Talk").transform);
            roomTalk.name = name;
            for (int i = roomTalk.transform.childCount - 1; i > 0; i--)
            {
                Destroy(roomTalk.transform.GetChild(i).gameObject);
            }
        }
        public GameObject CreateNewLevel(string name, string pathToCG, string baseSprite, string secondarySprite, string maskSprite)
        {
            GameObject newLevel = GameObject.Instantiate(GameObject.Find("5_Levels").transform.Find("14_Beach").gameObject, GameObject.Find("5_Levels").transform);
            newLevel.name = name;
            GameObject secondaryTex = newLevel.transform.GetChild(1).gameObject;
            secondaryTex.name = name;
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
    }
}
