using BepInEx;
using GameCreator;
using GameCreator.Runtime.Characters;
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
    [BepInPlugin(pluginGuid, Core.pluginName + " - Places", Core.pluginVersion)]
    internal class Places : BaseUnityPlugin
    {
        #region Plugin Info
        public const string pluginGuid = "treboy.starmakerstory.smsandroidscore.places";
        #endregion

        public static GameObject roomTalkAlley;
        public static GameObject roomTalkBeach;
        public static GameObject roomTalkDowntown;
        public static GameObject roomTalkForest;
        public static GameObject roomTalkGabrielsMansion;
        public static GameObject roomTalkGasStation;
        public static GameObject roomTalkHospital;
        public static GameObject roomTalkHospitalHallway;
        public static GameObject roomTalkHotel;
        public static GameObject roomTalkKensHome;
        public static GameObject roomTalkMall;
        public static GameObject roomTalkPark;
        public static GameObject roomTalkParkingLot;
        public static GameObject roomTalkTemple;
        public static GameObject roomTalkVilla;

        public static GameObject levelAlley;
        public static GameObject levelBeach;
        public static GameObject levelDowntown;
        public static GameObject levelForest;
        public static GameObject levelGabrielsMansion;
        public static GameObject levelGasStation;
        public static GameObject levelHospital;
        public static GameObject levelHospitalHallway;
        public static GameObject levelHotel;
        public static GameObject levelKensHome;
        public static GameObject levelMall;
        public static GameObject levelPark;
        public static GameObject levelParkingLot;
        public static GameObject levelTemple;
        public static GameObject levelVilla;

        public static GameObject buttonBeach;
        public static GameObject buttonCar;

        public static GameObject buttonSecretBeach;
        public static GameObject buttonMountainLab;
        public static GameObject buttonMountainLabCorridorNikke1;
        public static GameObject buttonMountainLabCorridorNikke2;
        public static GameObject buttonMountainLabRoomNikkeAnis;
        public static GameObject buttonMountainLabRoomNikkeCenti;
        public static GameObject buttonMountainLabRoomNikkeDorothy;
        public static GameObject buttonMountainLabRoomNikkeElegg;
        public static GameObject buttonMountainLabRoomNikkeFrima;
        public static GameObject buttonMountainLabRoomNikkeGuilty;
        public static GameObject buttonMountainLabRoomNikkeHelm;
        public static GameObject buttonMountainLabRoomNikkeMaiden;
        public static GameObject buttonMountainLabRoomNikkeMary;
        public static GameObject buttonMountainLabRoomNikkeMast;
        public static GameObject buttonMountainLabRoomNikkeNeon;
        public static GameObject buttonMountainLabRoomNikkePepper;
        public static GameObject buttonMountainLabRoomNikkeRapi;
        public static GameObject buttonMountainLabRoomNikkeRosanna;
        public static GameObject buttonMountainLabRoomNikkeSakura;
        public static GameObject buttonMountainLabRoomNikkeViper;
        public static GameObject buttonMountainLabRoomNikkeYan;

        public static GameObject secretBeachLevel;
        public static GameObject secretBeachLevelBG;
        public static GameObject secretBeachRoomtalk;
        public static GameObject secretBeachGatekeeper;
        public static GameObject secretBeachGatekeeperB;
        public static GameObject secretBeachFlash;
        public static GameObject secretBeachSky;

        public static GameObject mountainLabLevel;
        public static GameObject mountainLabRoomtalk;

        public static GameObject mountainLabCorridorNikke1Level;
        public static GameObject mountainLabCorridorNikke1Roomtalk;
        public static GameObject mountainLabCorridorNikke2Level;
        public static GameObject mountainLabCorridorNikke2Roomtalk;

        public static GameObject mountainLabRoomNikkeAnisLevel;
        public static GameObject mountainLabRoomNikkeAnisRoomtalk;
        public static GameObject mountainLabRoomNikkeCentiLevel;
        public static GameObject mountainLabRoomNikkeCentiRoomtalk;
        public static GameObject mountainLabRoomNikkeDorothyLevel;
        public static GameObject mountainLabRoomNikkeDorothyRoomtalk;
        public static GameObject mountainLabRoomNikkeEleggLevel;
        public static GameObject mountainLabRoomNikkeEleggRoomtalk;
        public static GameObject mountainLabRoomNikkeFrimaLevel;
        public static GameObject mountainLabRoomNikkeFrimaRoomtalk;
        public static GameObject mountainLabRoomNikkeGuiltyLevel;
        public static GameObject mountainLabRoomNikkeGuiltyRoomtalk;
        public static GameObject mountainLabRoomNikkeHelmLevel;
        public static GameObject mountainLabRoomNikkeHelmRoomtalk;
        public static GameObject mountainLabRoomNikkeMaidenLevel;
        public static GameObject mountainLabRoomNikkeMaidenRoomtalk;
        public static GameObject mountainLabRoomNikkeMaryLevel;
        public static GameObject mountainLabRoomNikkeMaryRoomtalk;
        public static GameObject mountainLabRoomNikkeMastLevel;
        public static GameObject mountainLabRoomNikkeMastRoomtalk;
        public static GameObject mountainLabRoomNikkeNeonLevel;
        public static GameObject mountainLabRoomNikkeNeonRoomtalk;
        public static GameObject mountainLabRoomNikkePepperLevel;
        public static GameObject mountainLabRoomNikkePepperRoomtalk;
        public static GameObject mountainLabRoomNikkeRapiLevel;
        public static GameObject mountainLabRoomNikkeRapiRoomtalk;
        public static GameObject mountainLabRoomNikkeRosannaLevel;
        public static GameObject mountainLabRoomNikkeRosannaRoomtalk;
        public static GameObject mountainLabRoomNikkeSakuraLevel;
        public static GameObject mountainLabRoomNikkeSakuraRoomtalk;
        public static GameObject mountainLabRoomNikkeViperLevel;
        public static GameObject mountainLabRoomNikkeViperRoomtalk;
        public static GameObject mountainLabRoomNikkeYanLevel;
        public static GameObject mountainLabRoomNikkeYanRoomtalk;

        public static GameObject weatherOutsideRain;
        public static GameObject weatherOutsideSnow;

        public static GameObject extraNavRow;
        public Vector2 originLevelPos = Vector2.zero;
        public int vanillaLevelCount;

        public static bool loadedPlaces = false;
        public static GameObject currentRoomTalk;
        public static GameObject solid;
        public void Update()
        {
            if (Core.currentScene.name == "CoreGameScene")
            {
                if (!loadedPlaces && Core.loadedCore)
                {
                    vanillaLevelCount = Core.level.childCount;
                    extraNavRow = GameObject.Instantiate(Core.mainCanvas.Find("Navigator").Find("Image").gameObject, Core.mainCanvas.Find("Navigator"));
                    Vector2 extraNavRowSize = extraNavRow.GetComponent<RectTransform>().sizeDelta;
                    Vector2 extraNavRowPosition = extraNavRow.GetComponent<RectTransform>().anchoredPosition;
                    extraNavRowSize.y *= 2;
                    extraNavRowPosition.y *= 1.5f;
                    extraNavRow.GetComponent<RectTransform>().sizeDelta = extraNavRowSize;
                    extraNavRow.GetComponent<RectTransform>().anchoredPosition = extraNavRowPosition;
                    extraNavRow.transform.SetSiblingIndex(Core.mainCanvas.Find("Navigator").Find("Image").GetSiblingIndex() + 1);

                    weatherOutsideRain = GameObject.Find("Weather_System_Outside").transform.Find("Prefab_Rainy_Day").Find("Rain_Core").gameObject;
                    weatherOutsideSnow = GameObject.Find("Weather_System_Outside").transform.Find("Prefab_Snowy_Day").Find("Snow_Core").gameObject;

                    roomTalkAlley = Core.roomTalk.Find("ParkingLotBackyard_Events").gameObject;
                    roomTalkBeach = Core.roomTalk.Find("Beach").gameObject;
                    roomTalkDowntown = Core.roomTalk.Find("Downtown").gameObject;
                    roomTalkForest = Core.roomTalk.Find("EvergreenForest_Entrance").gameObject;
                    roomTalkGabrielsMansion = Core.roomTalk.Find("Mansion").gameObject;
                    roomTalkGasStation = Core.roomTalk.Find("Gasstation").gameObject;
                    roomTalkHospital = CreateNewRoomTalk("Hospital");
                    roomTalkHospitalHallway = Core.roomTalk.Find("Hospitalhallway").gameObject;
                    roomTalkHotel = CreateNewRoomTalk("Hotel");
                    roomTalkKensHome = Core.roomTalk.Find("KenhouseOutside").gameObject;
                    roomTalkMall = Core.roomTalk.Find("Mall").gameObject;
                    roomTalkPark = Core.roomTalk.Find("publicparksuburbs").gameObject;
                    roomTalkParkingLot = Core.roomTalk.Find("Parkinglot_events").gameObject;
                    roomTalkTemple = Core.roomTalk.Find("Temple_Entrance").gameObject;
                    roomTalkVilla = Core.roomTalk.Find("OutsideVilla").gameObject;

                    levelBeach = Core.level.Find("14_Beach").gameObject;
                    levelKensHome = Core.level.Find("21_Suburban Exterior House").gameObject;
                    levelMall = Core.level.Find("25_Mall").gameObject;
                    levelDowntown = Core.level.Find("26_Downtown").gameObject;
                    levelGabrielsMansion = Core.level.Find("35_MansionOutside").gameObject;
                    levelHotel = Core.level.Find("39_HotelLobby").gameObject;
                    levelPark = Core.level.Find("58_Subpark").gameObject;
                    levelGasStation = Core.level.Find("59_Gasstation").gameObject;
                    levelForest = Core.level.Find("67_Jap_ForestEntrance").gameObject;
                    levelTemple = Core.level.Find("68_Jap_Temple").gameObject;
                    levelVilla = Core.level.Find("70_Villa_Outside").gameObject;
                    levelHospital = Core.level.Find("84_HospitalEntrance").gameObject;
                    levelHospitalHallway = Core.level.Find("85_HospitalHallway").gameObject;
                    levelParkingLot = Core.level.Find("110_BadlandsParkingLot").gameObject;
                    levelAlley = Core.level.Find("111_BadlandsParkingLotBackside").gameObject;

                    CreateNewPlace(900, "SecretBeach", "SecretBeach", "Remote Area", 0.75f);
                    CreateNewPlace(901, "MountainLab", "MountainLab", "Facility", 0.75f);
                    CreateNewPlace(902, "MountainLabCorridorNikke1", "MountainLabCorridor", "Sector N", 0.75f);
                    CreateNewPlace(903, "MountainLabRoomNikkeAnis", "MountainLabRoom", "Anis' Room", 0.65f);
                    CreateNewPlace(904, "MountainLabRoomNikkeCenti", "MountainLabRoom", "Centi's Room", 0.65f);
                    CreateNewPlace(905, "MountainLabRoomNikkeDorothy", "MountainLabRoom", "Dorothy's Room", 0.65f);
                    CreateNewPlace(906, "MountainLabRoomNikkeElegg", "MountainLabRoom", "Elegg's Room", 0.65f);
                    CreateNewPlace(907, "MountainLabRoomNikkeFrima", "MountainLabRoom", "Frima's Room", 0.65f);
                    CreateNewPlace(908, "MountainLabRoomNikkeGuilty", "MountainLabRoom", "Guilty's Room", 0.65f);
                    CreateNewPlace(909, "MountainLabRoomNikkeHelm", "MountainLabRoom", "Helm's Room", 0.65f);
                    CreateNewPlace(910, "MountainLabRoomNikkeMaiden", "MountainLabRoom", "Maiden's Room", 0.65f);
                    CreateNewPlace(911, "MountainLabRoomNikkeMary", "MountainLabRoom", "Mary's Room", 0.65f);
                    CreateNewPlace(912, "MountainLabRoomNikkeMast", "MountainLabRoom", "Mast's Room", 0.65f);
                    CreateNewPlace(913, "MountainLabCorridorNikke2", "MountainLabCorridor", "Next Section", 0.75f);
                    CreateNewPlace(914, "MountainLabRoomNikkeNeon", "MountainLabRoom", "Neon's Room", 0.65f);
                    CreateNewPlace(915, "MountainLabRoomNikkePepper", "MountainLabRoom", "Pepper's Room", 0.65f);
                    CreateNewPlace(916, "MountainLabRoomNikkeRapi", "MountainLabRoom", "Rapi's Room", 0.65f);
                    CreateNewPlace(917, "MountainLabRoomNikkeRosanna", "MountainLabRoom", "Rosanna's Room", 0.65f);
                    CreateNewPlace(918, "MountainLabRoomNikkeSakura", "MountainLabRoom", "Sakura's Room", 0.65f);
                    CreateNewPlace(919, "MountainLabRoomNikkeViper", "MountainLabRoom", "Viper's Room", 0.65f);
                    CreateNewPlace(920, "MountainLabRoomNikkeYan", "MountainLabRoom", "Yan's Room", 0.65f);

                    buttonBeach = Core.mainCanvas.Find("Navigator").Find("MapButtons").Find("14_beach").gameObject;
                    buttonCar = Core.mainCanvas.Find("Navigator").Find("MapButtons").Find("22_car").gameObject;

                    buttonSecretBeach = Core.mainCanvas.Find("Navigator").Find("MapButtons").Find("900_SecretBeach").gameObject;
                    buttonMountainLab = Core.mainCanvas.Find("Navigator").Find("MapButtons").Find("901_MountainLab").gameObject;
                    buttonMountainLabCorridorNikke1 = Core.mainCanvas.Find("Navigator").Find("MapButtons").Find("902_MountainLabCorridorNikke1").gameObject;
                    buttonMountainLabRoomNikkeAnis = Core.mainCanvas.Find("Navigator").Find("MapButtons").Find("903_MountainLabRoomNikkeAnis").gameObject;
                    buttonMountainLabRoomNikkeCenti = Core.mainCanvas.Find("Navigator").Find("MapButtons").Find("904_MountainLabRoomNikkeCenti").gameObject;
                    buttonMountainLabRoomNikkeDorothy = Core.mainCanvas.Find("Navigator").Find("MapButtons").Find("905_MountainLabRoomNikkeDorothy").gameObject;
                    buttonMountainLabRoomNikkeElegg = Core.mainCanvas.Find("Navigator").Find("MapButtons").Find("906_MountainLabRoomNikkeElegg").gameObject;
                    buttonMountainLabRoomNikkeFrima = Core.mainCanvas.Find("Navigator").Find("MapButtons").Find("907_MountainLabRoomNikkeFrima").gameObject;
                    buttonMountainLabRoomNikkeGuilty = Core.mainCanvas.Find("Navigator").Find("MapButtons").Find("908_MountainLabRoomNikkeGuilty").gameObject;
                    buttonMountainLabRoomNikkeHelm = Core.mainCanvas.Find("Navigator").Find("MapButtons").Find("909_MountainLabRoomNikkeHelm").gameObject;
                    buttonMountainLabRoomNikkeMaiden = Core.mainCanvas.Find("Navigator").Find("MapButtons").Find("910_MountainLabRoomNikkeMaiden").gameObject;
                    buttonMountainLabRoomNikkeMary = Core.mainCanvas.Find("Navigator").Find("MapButtons").Find("911_MountainLabRoomNikkeMary").gameObject;
                    buttonMountainLabRoomNikkeMast = Core.mainCanvas.Find("Navigator").Find("MapButtons").Find("912_MountainLabRoomNikkeMast").gameObject;
                    buttonMountainLabCorridorNikke2 = Core.mainCanvas.Find("Navigator").Find("MapButtons").Find("913_MountainLabCorridorNikke2").gameObject;
                    buttonMountainLabRoomNikkeNeon = Core.mainCanvas.Find("Navigator").Find("MapButtons").Find("914_MountainLabRoomNikkeNeon").gameObject;
                    buttonMountainLabRoomNikkePepper = Core.mainCanvas.Find("Navigator").Find("MapButtons").Find("915_MountainLabRoomNikkePepper").gameObject;
                    buttonMountainLabRoomNikkeRapi = Core.mainCanvas.Find("Navigator").Find("MapButtons").Find("916_MountainLabRoomNikkeRapi").gameObject;
                    buttonMountainLabRoomNikkeRosanna = Core.mainCanvas.Find("Navigator").Find("MapButtons").Find("917_MountainLabRoomNikkeRosanna").gameObject;
                    buttonMountainLabRoomNikkeSakura = Core.mainCanvas.Find("Navigator").Find("MapButtons").Find("918_MountainLabRoomNikkeSakura").gameObject;
                    buttonMountainLabRoomNikkeViper = Core.mainCanvas.Find("Navigator").Find("MapButtons").Find("919_MountainLabRoomNikkeViper").gameObject;
                    buttonMountainLabRoomNikkeYan = Core.mainCanvas.Find("Navigator").Find("MapButtons").Find("920_MountainLabRoomNikkeYan").gameObject;

                    secretBeachLevel = Core.level.Find("900_SecretBeach").gameObject;
                    secretBeachLevelBG = secretBeachLevel.transform.GetChild(1).gameObject;
                    secretBeachRoomtalk = Core.roomTalk.Find("SecretBeach").gameObject;

                    mountainLabLevel = Core.level.Find("901_MountainLab").gameObject;
                    mountainLabRoomtalk = Core.roomTalk.Find("MountainLab").gameObject;

                    mountainLabCorridorNikke1Level = Core.level.Find("902_MountainLabCorridorNikke1").gameObject;
                    mountainLabCorridorNikke1Roomtalk = Core.roomTalk.Find("MountainLabCorridorNikke1").gameObject;
                    mountainLabCorridorNikke2Level = Core.level.Find("913_MountainLabCorridorNikke2").gameObject;
                    mountainLabCorridorNikke2Roomtalk = Core.roomTalk.Find("MountainLabCorridorNikke2").gameObject;

                    mountainLabRoomNikkeAnisLevel = Core.level.Find("903_MountainLabRoomNikkeAnis").gameObject;
                    mountainLabRoomNikkeAnisRoomtalk = Core.roomTalk.Find("MountainLabRoomNikkeAnis").gameObject;
                    mountainLabRoomNikkeCentiLevel = Core.level.Find("904_MountainLabRoomNikkeCenti").gameObject;
                    mountainLabRoomNikkeCentiRoomtalk = Core.roomTalk.Find("MountainLabRoomNikkeCenti").gameObject;
                    mountainLabRoomNikkeDorothyLevel = Core.level.Find("905_MountainLabRoomNikkeDorothy").gameObject;
                    mountainLabRoomNikkeDorothyRoomtalk = Core.roomTalk.Find("MountainLabRoomNikkeDorothy").gameObject;
                    mountainLabRoomNikkeEleggLevel = Core.level.Find("906_MountainLabRoomNikkeElegg").gameObject;
                    mountainLabRoomNikkeEleggRoomtalk = Core.roomTalk.Find("MountainLabRoomNikkeElegg").gameObject;
                    mountainLabRoomNikkeFrimaLevel = Core.level.Find("907_MountainLabRoomNikkeFrima").gameObject;
                    mountainLabRoomNikkeFrimaRoomtalk = Core.roomTalk.Find("MountainLabRoomNikkeFrima").gameObject;
                    mountainLabRoomNikkeGuiltyLevel = Core.level.Find("908_MountainLabRoomNikkeGuilty").gameObject;
                    mountainLabRoomNikkeGuiltyRoomtalk = Core.roomTalk.Find("MountainLabRoomNikkeGuilty").gameObject;
                    mountainLabRoomNikkeHelmLevel = Core.level.Find("909_MountainLabRoomNikkeHelm").gameObject;
                    mountainLabRoomNikkeHelmRoomtalk = Core.roomTalk.Find("MountainLabRoomNikkeHelm").gameObject;
                    mountainLabRoomNikkeMaidenLevel = Core.level.Find("910_MountainLabRoomNikkeMaiden").gameObject;
                    mountainLabRoomNikkeMaidenRoomtalk = Core.roomTalk.Find("MountainLabRoomNikkeMaiden").gameObject;
                    mountainLabRoomNikkeMaryLevel = Core.level.Find("911_MountainLabRoomNikkeMary").gameObject;
                    mountainLabRoomNikkeMaryRoomtalk = Core.roomTalk.Find("MountainLabRoomNikkeMary").gameObject;
                    mountainLabRoomNikkeMastLevel = Core.level.Find("912_MountainLabRoomNikkeMast").gameObject;
                    mountainLabRoomNikkeMastRoomtalk = Core.roomTalk.Find("MountainLabRoomNikkeMast").gameObject;
                    mountainLabRoomNikkeNeonLevel = Core.level.Find("914_MountainLabRoomNikkeNeon").gameObject;
                    mountainLabRoomNikkeNeonRoomtalk = Core.roomTalk.Find("MountainLabRoomNikkeNeon").gameObject;
                    mountainLabRoomNikkePepperLevel = Core.level.Find("915_MountainLabRoomNikkePepper").gameObject;
                    mountainLabRoomNikkePepperRoomtalk = Core.roomTalk.Find("MountainLabRoomNikkePepper").gameObject;
                    mountainLabRoomNikkeRapiLevel = Core.level.Find("916_MountainLabRoomNikkeRapi").gameObject;
                    mountainLabRoomNikkeRapiRoomtalk = Core.roomTalk.Find("MountainLabRoomNikkeRapi").gameObject;
                    mountainLabRoomNikkeRosannaLevel = Core.level.Find("917_MountainLabRoomNikkeRosanna").gameObject;
                    mountainLabRoomNikkeRosannaRoomtalk = Core.roomTalk.Find("MountainLabRoomNikkeRosanna").gameObject;
                    mountainLabRoomNikkeSakuraLevel = Core.level.Find("918_MountainLabRoomNikkeSakura").gameObject;
                    mountainLabRoomNikkeSakuraRoomtalk = Core.roomTalk.Find("MountainLabRoomNikkeSakura").gameObject;
                    mountainLabRoomNikkeViperLevel = Core.level.Find("919_MountainLabRoomNikkeViper").gameObject;
                    mountainLabRoomNikkeViperRoomtalk = Core.roomTalk.Find("MountainLabRoomNikkeViper").gameObject;
                    mountainLabRoomNikkeYanLevel = Core.level.Find("920_MountainLabRoomNikkeYan").gameObject;
                    mountainLabRoomNikkeYanRoomtalk = Core.roomTalk.Find("MountainLabRoomNikkeYan").gameObject;

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
                    secretBeachSky.GetComponent<ParallaxMouseEffect>().enabled = false;
                    secretBeachFlash.GetComponent<ParallaxMouseEffect>().enabled = false;
                    secretBeachFlash.GetComponent<SpriteRenderer>().sortingOrder = -9;
                    secretBeachGatekeeper.GetComponent<ParallaxMouseEffect>().enabled = false;
                    secretBeachGatekeeper.GetComponent<SpriteRenderer>().sortingOrder = -10;
                    secretBeachGatekeeperB.GetComponent<ParallaxMouseEffect>().enabled = false;
                    secretBeachGatekeeperB.GetComponent<SpriteRenderer>().sortingOrder = -11;
                    secretBeachFlash.SetActive(false);
                    secretBeachGatekeeperB.SetActive(false);
                    secretBeachGatekeeperB.GetComponent<SpriteRenderer>().color = new Color(secretBeachGatekeeperB.GetComponent<SpriteRenderer>().color.r,
                        secretBeachGatekeeperB.GetComponent<SpriteRenderer>().color.g, secretBeachGatekeeperB.GetComponent<SpriteRenderer>().color.b, 0);
                    secretBeachGatekeeperB.AddComponent<FadeInSprite2>();
                    secretBeachGatekeeperB.GetComponent<FadeInSprite2>().fadeInDuration = 1f;
                    secretBeachFlash.AddComponent<FadeOutSprite>();
                    originLevelPos = Places.secretBeachLevel.transform.position;

                    SetupMapButtonsGrid(Core.mainCanvas.Find("Navigator").Find("MapButtons").gameObject, new Vector2(125, 75), 6, 15f, 20f, this);


                    solid = GameObject.Instantiate(Places.secretBeachLevel.transform.GetChild(1).gameObject, Places.levelForest.transform);
                    SetNewLevelSprite(solid, Core.bustPath, "Solid\\Solid.PNG", 2048, 1136);
                    solid.GetComponent<SpriteRenderer>().color = new Color(1,1,1,0);
                    solid.name = "Solid";

                    Material mat = new Material(Core.bustManager.Find("Anna_Bust").gameObject.GetComponent<SpriteRenderer>().material);
                    Texture2D tex = new Texture2D(2048, 1136, TextureFormat.RGBA32, false);
                    var rawData = System.IO.File.ReadAllBytes(Core.bustPath + "Solid\\SolidMask.PNG");
                    tex.LoadImage(rawData);
                    tex.filterMode = FilterMode.Point;
                    mat.SetTexture("_MaskTex", tex);
                    solid.GetComponent<SpriteRenderer>().material = mat;
                    solid.GetComponent<SpriteRenderer>().material.SetTexture("_MaskTex", tex);
                    solid.GetComponent<SpriteRenderer>().sortingOrder = -5;

                    solid.SetActive(true);

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
                if (secretBeachLevel.activeSelf) { buttonBeach.SetActive(true); }
                if (secretBeachLevel.activeSelf) { buttonCar.SetActive(true); }

                buttonSecretBeach.SetActive(Core.levelBeach.activeSelf || mountainLabLevel.activeSelf);
                buttonMountainLab.SetActive(SaveManager.GetBool("SecretBeach_UnlockedLab") && (secretBeachLevel.activeSelf || mountainLabCorridorNikke1Level.activeSelf || mountainLabCorridorNikke2Level.activeSelf));
                buttonMountainLabCorridorNikke1.SetActive(mountainLabLevel.activeSelf || mountainLabRoomNikkeAnisLevel.activeSelf || mountainLabRoomNikkeCentiLevel.activeSelf || mountainLabRoomNikkeDorothyLevel.activeSelf ||
                    mountainLabRoomNikkeEleggLevel.activeSelf || mountainLabRoomNikkeFrimaLevel.activeSelf || mountainLabRoomNikkeGuiltyLevel.activeSelf || mountainLabRoomNikkeHelmLevel.activeSelf || mountainLabRoomNikkeMaidenLevel.activeSelf || 
                    mountainLabRoomNikkeMaryLevel.activeSelf || mountainLabRoomNikkeMastLevel.activeSelf || mountainLabCorridorNikke2Level.activeSelf);
                buttonMountainLabRoomNikkeAnis.SetActive(SaveManager.GetBool("Voyeur_SeenAnis") && mountainLabCorridorNikke1Level.activeSelf);
                buttonMountainLabRoomNikkeCenti.SetActive(SaveManager.GetBool("Voyeur_SeenCenti") && mountainLabCorridorNikke1Level.activeSelf);
                buttonMountainLabRoomNikkeDorothy.SetActive(SaveManager.GetBool("Voyeur_SeenDorothy") && mountainLabCorridorNikke1Level.activeSelf);
                buttonMountainLabRoomNikkeElegg.SetActive(SaveManager.GetBool("Voyeur_SeenElegg") && mountainLabCorridorNikke1Level.activeSelf);
                buttonMountainLabRoomNikkeFrima.SetActive(SaveManager.GetBool("Voyeur_SeenFrima") && mountainLabCorridorNikke1Level.activeSelf);
                buttonMountainLabRoomNikkeGuilty.SetActive(SaveManager.GetBool("Voyeur_SeenGuilty") && mountainLabCorridorNikke1Level.activeSelf);
                buttonMountainLabRoomNikkeHelm.SetActive(SaveManager.GetBool("Voyeur_SeenHelm") && mountainLabCorridorNikke1Level.activeSelf);
                buttonMountainLabRoomNikkeMaiden.SetActive(SaveManager.GetBool("Voyeur_SeenMaiden") && mountainLabCorridorNikke1Level.activeSelf);
                buttonMountainLabRoomNikkeMary.SetActive(SaveManager.GetBool("Voyeur_SeenMary") && mountainLabCorridorNikke1Level.activeSelf);
                buttonMountainLabRoomNikkeMast.SetActive(SaveManager.GetBool("Voyeur_SeenMast") && mountainLabCorridorNikke1Level.activeSelf);
                buttonMountainLabCorridorNikke2.SetActive(mountainLabRoomNikkeNeonLevel.activeSelf || mountainLabRoomNikkePepperLevel.activeSelf || mountainLabRoomNikkeRapiLevel.activeSelf || mountainLabCorridorNikke1Level.activeSelf || 
                    mountainLabRoomNikkeRosannaLevel.activeSelf || mountainLabRoomNikkeSakuraLevel.activeSelf || mountainLabRoomNikkeViperLevel.activeSelf || mountainLabRoomNikkeYanLevel.activeSelf);
                buttonMountainLabRoomNikkeNeon.SetActive(SaveManager.GetBool("Voyeur_SeenNeon") && mountainLabCorridorNikke2Level.activeSelf);
                buttonMountainLabRoomNikkePepper.SetActive(SaveManager.GetBool("Voyeur_SeenPepper") && mountainLabCorridorNikke2Level.activeSelf);
                buttonMountainLabRoomNikkeRapi.SetActive(SaveManager.GetBool("Voyeur_SeenRapi") && mountainLabCorridorNikke2Level.activeSelf);
                buttonMountainLabRoomNikkeRosanna.SetActive(SaveManager.GetBool("Voyeur_SeenRosanna") && mountainLabCorridorNikke2Level.activeSelf);
                buttonMountainLabRoomNikkeSakura.SetActive(SaveManager.GetBool("Voyeur_SeenSakura") && mountainLabCorridorNikke2Level.activeSelf);
                buttonMountainLabRoomNikkeViper.SetActive(SaveManager.GetBool("Voyeur_SeenViper") && mountainLabCorridorNikke2Level.activeSelf);
                buttonMountainLabRoomNikkeYan.SetActive(SaveManager.GetBool("Voyeur_SeenYan") && mountainLabCorridorNikke2Level.activeSelf);

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
                if (buttonMountainLabCorridorNikke1.transform.GetChild(0).gameObject.activeSelf)
                {
                    ClickMapButton(mountainLabCorridorNikke1Roomtalk, 2);
                    buttonMountainLabCorridorNikke1.transform.GetChild(0).gameObject.SetActive(false);
                }
                if (buttonMountainLabRoomNikkeAnis.transform.GetChild(0).gameObject.activeSelf)
                {
                    ClickMapButton(mountainLabRoomNikkeAnisRoomtalk, 3);
                    buttonMountainLabRoomNikkeAnis.transform.GetChild(0).gameObject.SetActive(false);
                }
                if (buttonMountainLabRoomNikkeCenti.transform.GetChild(0).gameObject.activeSelf)
                {
                    ClickMapButton(mountainLabRoomNikkeCentiRoomtalk, 4);
                    buttonMountainLabRoomNikkeCenti.transform.GetChild(0).gameObject.SetActive(false);
                }
                if (buttonMountainLabRoomNikkeDorothy.transform.GetChild(0).gameObject.activeSelf)
                {
                    ClickMapButton(mountainLabRoomNikkeDorothyRoomtalk, 5);
                    buttonMountainLabRoomNikkeDorothy.transform.GetChild(0).gameObject.SetActive(false);
                }
                if (buttonMountainLabRoomNikkeElegg.transform.GetChild(0).gameObject.activeSelf)
                {
                    ClickMapButton(mountainLabRoomNikkeEleggRoomtalk, 6);
                    buttonMountainLabRoomNikkeElegg.transform.GetChild(0).gameObject.SetActive(false);
                }
                if (buttonMountainLabRoomNikkeFrima.transform.GetChild(0).gameObject.activeSelf)
                {
                    ClickMapButton(mountainLabRoomNikkeFrimaRoomtalk, 7);
                    buttonMountainLabRoomNikkeFrima.transform.GetChild(0).gameObject.SetActive(false);
                }
                if (buttonMountainLabRoomNikkeGuilty.transform.GetChild(0).gameObject.activeSelf)
                {
                    ClickMapButton(mountainLabRoomNikkeGuiltyRoomtalk, 8);
                    buttonMountainLabRoomNikkeGuilty.transform.GetChild(0).gameObject.SetActive(false);
                }
                if (buttonMountainLabRoomNikkeHelm.transform.GetChild(0).gameObject.activeSelf)
                {
                    ClickMapButton(mountainLabRoomNikkeHelmRoomtalk, 9);
                    buttonMountainLabRoomNikkeHelm.transform.GetChild(0).gameObject.SetActive(false);
                }
                if (buttonMountainLabRoomNikkeMaiden.transform.GetChild(0).gameObject.activeSelf)
                {
                    ClickMapButton(mountainLabRoomNikkeMaidenRoomtalk, 10);
                    buttonMountainLabRoomNikkeMaiden.transform.GetChild(0).gameObject.SetActive(false);
                }
                if (buttonMountainLabRoomNikkeMary.transform.GetChild(0).gameObject.activeSelf)
                {
                    ClickMapButton(mountainLabRoomNikkeMaryRoomtalk, 11);
                    buttonMountainLabRoomNikkeMary.transform.GetChild(0).gameObject.SetActive(false);
                }
                if (buttonMountainLabRoomNikkeMast.transform.GetChild(0).gameObject.activeSelf)
                {
                    ClickMapButton(mountainLabRoomNikkeMastRoomtalk, 12);
                    buttonMountainLabRoomNikkeMast.transform.GetChild(0).gameObject.SetActive(false);
                }
                
                if (buttonMountainLabCorridorNikke2.transform.GetChild(0).gameObject.activeSelf)
                {
                    ClickMapButton(mountainLabCorridorNikke2Roomtalk, 13);
                    buttonMountainLabCorridorNikke2.transform.GetChild(0).gameObject.SetActive(false);
                }
                if (buttonMountainLabRoomNikkeNeon.transform.GetChild(0).gameObject.activeSelf)
                {
                    ClickMapButton(mountainLabRoomNikkeNeonRoomtalk, 14);
                    buttonMountainLabRoomNikkeNeon.transform.GetChild(0).gameObject.SetActive(false);
                }
                if (buttonMountainLabRoomNikkePepper.transform.GetChild(0).gameObject.activeSelf)
                {
                    ClickMapButton(mountainLabRoomNikkePepperRoomtalk, 15);
                    buttonMountainLabRoomNikkePepper.transform.GetChild(0).gameObject.SetActive(false);
                }
                if (buttonMountainLabRoomNikkeRapi.transform.GetChild(0).gameObject.activeSelf)
                {
                    ClickMapButton(mountainLabRoomNikkeRapiRoomtalk, 16);
                    buttonMountainLabRoomNikkeRapi.transform.GetChild(0).gameObject.SetActive(false);
                }
                if (buttonMountainLabRoomNikkeRosanna.transform.GetChild(0).gameObject.activeSelf)
                {
                    ClickMapButton(mountainLabRoomNikkeRosannaRoomtalk, 17);
                    buttonMountainLabRoomNikkeRosanna.transform.GetChild(0).gameObject.SetActive(false);
                }
                if (buttonMountainLabRoomNikkeSakura.transform.GetChild(0).gameObject.activeSelf)
                {
                    ClickMapButton(mountainLabRoomNikkeSakuraRoomtalk, 18);
                    buttonMountainLabRoomNikkeSakura.transform.GetChild(0).gameObject.SetActive(false);
                }
                if (buttonMountainLabRoomNikkeViper.transform.GetChild(0).gameObject.activeSelf)
                {
                    ClickMapButton(mountainLabRoomNikkeViperRoomtalk, 19);
                    buttonMountainLabRoomNikkeViper.transform.GetChild(0).gameObject.SetActive(false);
                }
                if (buttonMountainLabRoomNikkeYan.transform.GetChild(0).gameObject.activeSelf)
                {
                    ClickMapButton(mountainLabRoomNikkeYanRoomtalk, 20);
                    buttonMountainLabRoomNikkeYan.transform.GetChild(0).gameObject.SetActive(false);
                }



                if (Places.secretBeachLevel.activeSelf)
                {
                    secretBeachGatekeeper.transform.Rotate(0, 0, 1f * Time.deltaTime);
                }
            }
        }

        public void CreateNewPlace(int index, string name, string pathName, string buttonText, float parallaxStrength)
        {
            // Variables
            GameObject baseMapButton = GameObject.Find("9_MainCanvas").transform.Find("Navigator").Find("MapButtons").Find("14_beach").gameObject;

            // Button
            GameObject mapButton = GameObject.Instantiate(Core.otherBundle.LoadAsset<GameObject>("ButtonTemplate"), baseMapButton.transform.parent);
            GameObject mapButtonText = GameObject.Instantiate(baseMapButton.transform.GetChild(0).gameObject, mapButton.transform);
            GameObject mapButtonImage = GameObject.Instantiate(baseMapButton.transform.GetChild(1).gameObject, mapButton.transform);
            GameObject mapButtonImage1 = GameObject.Instantiate(baseMapButton.transform.GetChild(2).gameObject, mapButton.transform);
            GameObject mapButtonKBNumber = GameObject.Instantiate(baseMapButton.transform.GetChild(3).gameObject, mapButton.transform);
            mapButton.SetActive(false);
            mapButton.AddComponent<ButtonHover>();
            mapButton.name = index + "_" + name;
            mapButtonText.name = "Text (TMP)";
            mapButtonImage.GetComponent<UnityEngine.UI.Image>().color = new Color32(156, 111, 22, 0);
            mapButtonImage.name = "Image";
            mapButtonImage1.name = "Image (1)";
            mapButtonKBNumber.name = "keyboardnumber";
            mapButton.GetComponent<UnityEngine.UI.Image>().sprite = baseMapButton.GetComponent<UnityEngine.UI.Image>().sprite;
            mapButtonText.GetComponent<TextMeshProUGUI>().text = buttonText;
            mapButtonImage.GetComponent<UnityEngine.UI.Image>().sprite = baseMapButton.transform.GetChild(1).gameObject.GetComponent<UnityEngine.UI.Image>().sprite;
            mapButtonKBNumber.GetComponent<TextMeshProUGUI>().text = "2";

            // Level
            GameObject level = CreateNewLevel(index + "_" + name, Core.locationPath, pathName + ".PNG", pathName + "B.PNG", pathName + "Mask.PNG", parallaxStrength);
            Destroy(level.GetComponent<Trigger>());

            // RoomTalk
            CreateNewRoomTalk(name);
        }
        public GameObject CreateNewRoomTalk(string name)
        {
            GameObject roomTalk = GameObject.Instantiate(Core.roomTalk.Find("Beach").gameObject, Core.roomTalk);
            roomTalk.name = name;
            for (int i = roomTalk.transform.childCount - 1; i > 0; i--)
            {
                Destroy(roomTalk.transform.GetChild(i).gameObject);
            }
            Destroy(roomTalk.GetComponent<Conditions>());
            return roomTalk;
        }
        public GameObject CreateNewLevel(string name, string pathToCG, string baseSprite, string secondarySprite, string maskSprite, float parallaxStrength)
        {
            GameObject newLevel = GameObject.Instantiate(GameObject.Find("5_Levels").transform.Find("14_Beach").gameObject, GameObject.Find("5_Levels").transform);
            newLevel.name = name;
            newLevel.GetComponent<ParallaxMouseEffect>().parallaxStrength = parallaxStrength;
            GameObject secondaryTex = newLevel.transform.GetChild(1).gameObject;
            secondaryTex.name = name;
            secondaryTex.GetComponent<ParallaxMouseEffect>().parallaxStrength = parallaxStrength;
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

        public static void SetupMapButtonsGrid(GameObject mapButtons, Vector2 cellSize, int columns, float horizontalSpacing, float rowSpacing, MonoBehaviour context)
        {
            // Remove other layout groups
            var hlg = mapButtons.GetComponent<HorizontalLayoutGroup>();
            if (hlg != null) UnityEngine.Object.Destroy(hlg);
            var vlg = mapButtons.GetComponent<VerticalLayoutGroup>();
            if (vlg != null) UnityEngine.Object.Destroy(vlg);

            // Remove Unity's GridLayoutGroup if present
            var unityGrid = mapButtons.GetComponent<UnityEngine.UI.GridLayoutGroup>();
            if (unityGrid != null) UnityEngine.Object.Destroy(unityGrid);

            // Start coroutine to add custom GridLayoutGroup after one frame
            context.StartCoroutine(AddGridLayoutGroupNextFrame(mapButtons, cellSize, columns, horizontalSpacing, rowSpacing));
        }

        private static IEnumerator AddGridLayoutGroupNextFrame(GameObject mapButtons, Vector2 cellSize, int columns, float horizontalSpacing, float rowSpacing)
        {
            yield return null; // Wait one frame so old LayoutGroup is destroyed

            var grid = mapButtons.GetComponent<SMSAndroidsCore.GridLayoutGroup>();
            if (grid == null) grid = mapButtons.AddComponent<SMSAndroidsCore.GridLayoutGroup>();

            grid.constraint = UnityEngine.UI.GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = columns;
            grid.cellSize = cellSize;
            grid.spacing = new Vector2(horizontalSpacing, rowSpacing);
            grid.childAlignment = TextAnchor.MiddleCenter;
            grid.startAxis = UnityEngine.UI.GridLayoutGroup.Axis.Horizontal;
            grid.startCorner = UnityEngine.UI.GridLayoutGroup.Corner.UpperLeft;
        }

        public static bool CheckAllConditions(GameObject targetGameObject, Args args)
        {
            if (targetGameObject == null)
            {
                Debug.LogError("Target GameObject is null");
                return false; // Or throw an exception, depending on desired behavior
            }

            // Find all "Conditions" components. Note: this relies on the *name* of the component, not the type.
            var conditionsComponents = targetGameObject.GetComponents<MonoBehaviour>()
                .Where(c => c != null && c.GetType().Name == "Conditions");

            // If there are no conditions components, return true, as no conditions means all conditions are met
            if (!conditionsComponents.Any())
            {
                return true;
            }

            bool anyConditionMet = false; // Flag to track if at least one condition is met

            foreach (var cond in conditionsComponents)
            {
                // Access the m_Branches field using reflection
                var branchesField = cond.GetType().GetField("m_Branches", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                if (branchesField != null)
                {
                    var branches = branchesField.GetValue(cond);

                    // Check if branches is not null
                    if (branches != null)
                    {
                        // Access the m_Branches array within the BranchList
                        var branchesArrayField = branches.GetType().GetField("m_Branches", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                        if (branchesArrayField != null)
                        {
                            var branchesArray = branchesArrayField.GetValue(branches) as System.Collections.IEnumerable;

                            if (branchesArray != null)
                            {
                                // Iterate through each branch
                                foreach (var branch in branchesArray)
                                {
                                    if (branch != null)
                                    {
                                        // Access the m_Condition field within the branch
                                        var conditionField = branch.GetType().GetField("m_Condition", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                                        var conditionListField = branch.GetType().GetField("m_ConditionList", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                                        if (conditionListField != null)
                                        {
                                            var conditionList = conditionListField.GetValue(branch) as ConditionList;
                                            if (conditionList != null)
                                            {
                                                if (conditionList.Check(args, CheckMode.And))
                                                {
                                                    Debug.Log("ConditionList Met!");
                                                    anyConditionMet = true;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
            }

            return anyConditionMet;
        }

        return false;
    }
}
}
