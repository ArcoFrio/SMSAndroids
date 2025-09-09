using BepInEx;
using GameCreator;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.Common.Audio;
using GameCreator.Runtime.Common.SaveSystem;
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
using System.ComponentModel;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Policy;
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
using System.Xml.Serialization;
using static System.Net.Mime.MediaTypeNames;

namespace SMSAndroidsCore
{
    [BepInPlugin(pluginGuid, Core.pluginName + " - SaveManager", Core.pluginVersion)]
    internal class Wallpaper : BaseUnityPlugin
    {
        #region Plugin Info
        public const string pluginGuid = "treboy.starmakerstory.smsandroidscore.wallpaper";

        public static bool loadedWallpaper = false;
        public static GameObject baseWallpaper;
        public static GameObject baseWallpaperButton;
        public static GameObject wallpaperSnek;
        public static GameObject wallpaperSnekButton;
        public static GameObject wallpaperSnekButtonActivator;
        public static int vanillaWallpaperCount = 0;
        #endregion
        public void Update()
        {
            if (Core.currentScene.name == "CoreGameScene")
            {
                if (!loadedWallpaper && Core.loadedBases)
                {
                //    baseWallpaper = Core.mainCanvas.Find("Desktop").Find("Wallpaper").Find("Wallpaper (0)").gameObject;
                //    baseWallpaperButton = Core.mainCanvas.Find("Wallpaperselection").Find("UI_Core").Find("List").Find("WallpaperButton (0)").gameObject;
                //    vanillaWallpaperCount = baseWallpaper.transform.parent.childCount;

                //    wallpaperSnek = Instantiate(baseWallpaper, baseWallpaper.transform.parent);
                //    wallpaperSnek.name = "SolidGearOfMetal";
                //    Texture2D tex = new Texture2D(1920, 1080);
                //    tex.LoadImage(File.ReadAllBytes(Core.wallpaperPath + "Solid.PNG"));
                //    Sprite newSprite = Sprite.Create(tex, new Rect(0, 0, 1920, 1080), new Vector2(0.5f, 0.5f));
                //    wallpaperSnek.GetComponent<UnityEngine.UI.Image>().sprite = newSprite;

                //    wallpaperSnekButton = Instantiate(baseWallpaperButton, baseWallpaperButton.transform.parent);
                //    GameObject wpButton = GameObject.Instantiate(Core.otherBundle.LoadAsset<GameObject>("ButtonTemplate"), wallpaperSnekButton.transform);
                //    wallpaperSnekButtonActivator = wpButton.transform.GetChild(0).gameObject;
                //    wallpaperSnekButton.name = "WallpaperButton (SolidGearOfMetal)";
                //    wallpaperSnekButton.GetComponent<UnityEngine.UI.Image>().sprite = newSprite;
                //    Destroy(wallpaperSnekButton.GetComponent<ButtonInstructions>());

                //    loadedWallpaper = true;
                //}
                //if (loadedWallpaper)
                //{
                //    if (wallpaperSnekButtonActivator.activeSelf)
                //    {
                //        Core.FindAndModifyVariableDouble("Wallpaper", 0);
                //        wallpaperSnekButtonActivator.SetActive(false);
                //    }
                }
            }
            if (Core.currentScene.name == "GameStart")
            {
                if (loadedWallpaper)
                {
                    loadedWallpaper = false;
                }
            }
        }
        
    }
}
