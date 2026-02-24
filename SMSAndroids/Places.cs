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
        public static GameObject roomTalkTrail;
        public static GameObject roomTalkVilla;

        public static GameObject levelAlley;
        public static GameObject levelBeach;
        public static GameObject levelCinema;
        public static GameObject levelDowntown;
        public static GameObject levelForest;
        public static GameObject levelGabrielsMansion;
        public static GameObject levelGasStation;
        public static GameObject levelHospital;
        public static GameObject levelHospitalHallway;
        public static GameObject levelHotel;
        public static GameObject levelKensHome;
        public static GameObject levelMall;
        public static GameObject levelMyRoom;
        public static GameObject levelPark;
        public static GameObject levelParkingLot;
        public static GameObject levelTemple;
        public static GameObject levelTrail;
        public static GameObject levelVilla;

        public static int randomNumMall = -1;
        public static int randomNumMLRoomAnis = -1;

        public static GameObject buttonBeach;
        public static GameObject buttonCar;

        public static GameObject buttonGiftShop;
        public static GameObject buttonGiftShopInterior;
        public static GameObject buttonHarborHomeBathroom;
        public static GameObject buttonHarborHomeBedroom;
        public static GameObject buttonHarborHomeCloset;
        public static GameObject buttonHarborHouseEntrance;
        public static GameObject buttonHarborHomeKitchen;
        public static GameObject buttonHarborHomeLivingroom;
        public static GameObject buttonHarborHomePool;
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
        public static GameObject buttonMountainLabRoomNikkeTove;
        public static GameObject buttonMountainLabRoomNikkeViper;
        public static GameObject buttonMountainLabRoomNikkeYan;
        public static GameObject buttonSecretBeach;

        public static GameObject giftShopLevel;
        public static GameObject giftShopRoomtalk;
        public static GameObject giftShopInteriorLevel;
        public static GameObject giftShopInteriorRoomtalk;

        public static GameObject harborHomeBathroomLevel;
        public static GameObject harborHomeBathroomLevelB1;
        public static GameObject harborHomeBathroomRoomtalk;
        public static GameObject harborHomeBedroomLevel;
        public static GameObject harborHomeBedroomRoomtalk;
        public static GameObject harborHomeBedroomButtonCanvas;
        public static GameObject harborHomeClosetLevel;
        public static GameObject harborHomeClosetRoomtalk;
        public static GameObject harborHouseEntranceLevel;
        public static GameObject harborHouseEntranceRadialButton;
        public static GameObject harborHouseEntranceRadialButtonText;
        public static GameObject harborHouseEntranceRoomtalk;
        public static GameObject harborHomeKitchenLevel;
        public static GameObject harborHomeKitchenLevelB1;
        public static GameObject harborHomeKitchenRoomtalk;
        public static GameObject harborHomeLivingroomLevel;
        public static GameObject harborHomeLivingroomRoomtalk;
        public static GameObject harborHomePoolLevel;
        public static GameObject harborHomePoolRoomtalk;
        public static Transform harborHomeBathroomNPCShower;
        public static Transform harborHomeBedroomNPCBedleft;
        public static Transform harborHomeBedroomNPCBedright;
        public static Transform harborHomeClosetNPCChangingleft;
        public static Transform harborHomeClosetNPCChangingright;
        public static Transform harborHomeKitchenNPCFridge;
        public static Transform harborHomeKitchenNPCSink;
        public static Transform harborHomeLivingroomNPCCouchleft;
        public static Transform harborHomeLivingroomNPCCouchright;
        public static Transform harborHomePoolNPCTanningleft;
        public static Transform harborHomePoolNPCTanningright;

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
        public static GameObject mountainLabRoomNikkeToveLevel;
        public static GameObject mountainLabRoomNikkeToveRoomtalk;
        public static GameObject mountainLabRoomNikkeViperLevel;
        public static GameObject mountainLabRoomNikkeViperRoomtalk;
        public static GameObject mountainLabRoomNikkeYanLevel;
        public static GameObject mountainLabRoomNikkeYanRoomtalk;
        
        public static GameObject secretBeachLevel;
        public static GameObject secretBeachLevelBG;
        public static GameObject secretBeachRoomtalk;
        public static GameObject secretBeachGatekeeper;
        public static GameObject secretBeachGatekeeperB;
        public static GameObject secretBeachFlash;
        public static GameObject secretBeachSky;

        public static GameObject weatherInsideRain;
        public static GameObject weatherInsideSnow;
        public static GameObject weatherOutsideRain;
        public static GameObject weatherOutsideSnow;

        public static GameObject modShops;
        public static GameObject giftStore;
        private static float lastGiftStoreCheckTime = 0f;
        private static float giftStoreCheckInterval = 0.5f;
        private static bool wasGiftStoreActive = false;
        public static GameObject giftShopItemBikini;
        public static GameObject giftShopItemBodyLotion;
        public static GameObject giftShopItemBonsai;
        public static GameObject giftShopItemBouquet;
        public static GameObject giftShopItemFigure;
        public static GameObject giftShopItemNecklace;
        public static GameObject giftShopItemParasol;
        public static GameObject giftShopItemRing;
        public static GameObject giftShopItemSunglasses;
        public static GameObject giftShopItemSunscreen;

        public static GameObject extraNavRow;
        public Vector2 originLevelPos = Vector2.zero;
        public int vanillaLevelCount;

        public static bool loadedPlaces = false;
        private static bool harborHomeBedroomSwapApplied = false;
        public static bool insideHarborHome = false;
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

                    weatherInsideRain = GameObject.Find("Weather_System_Inside").transform.Find("Prefab_Rainy_Day").Find("Rain_Core").gameObject;
                    weatherInsideSnow = GameObject.Find("Weather_System_Inside").transform.Find("Prefab_Snowy_Day").Find("Snow_Core").gameObject;
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
                    roomTalkTrail = CreateNewRoomTalk("Trail");
                    roomTalkVilla = Core.roomTalk.Find("OutsideVilla").gameObject;

                    levelMyRoom = Core.level.Find("5_MyRoom").gameObject;
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
                    levelCinema = Core.level.Find("122_Cinema").gameObject;
                    levelTrail = Core.level.Find("138_HikingPath_Start").gameObject;

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
                    CreateNewPlace(919, "MountainLabRoomNikkeTove", "MountainLabRoom", "Tove's Room", 0.65f);
                    CreateNewPlace(920, "MountainLabRoomNikkeViper", "MountainLabRoom", "Viper's Room", 0.65f);
                    CreateNewPlace(921, "MountainLabRoomNikkeYan", "MountainLabRoom", "Yan's Room", 0.65f);
                    CreateNewPlace(922, "GiftShop", "GiftShop", "West Side", 0.75f);
                    CreateNewPlace(923, "GiftShopInterior", "GiftShopInterior", "Gift Shop", 0.65f);
                    CreateNewPlace(924, "HarborHomeLivingRoom", "HHomeLivingRoom", "Living Room", 0.05f);
                    CreateNewPlace(925, "HarborHomeBedroom", "HHomeBedroom", "Bedroom", 0.05f);
                    CreateNewPlace(926, "HarborHomeBathroom", "HHomeBathroom", "Bathroom", 0.15f);
                    CreateNewPlace(927, "HarborHomeCloset", "HHomeCloset", "Closet", 0.05f);
                    CreateNewPlace(928, "HarborHomeKitchen", "HHomeKitchen", "Kitchen", 0.05f);
                    CreateNewPlace(929, "HarborHomePool", "HHomePool", "Pool", 0.05f);
                    CreateNewPlace(930, "HarborHouseEntrance", "HHomeEntrance", "Entrance", 0.75f);

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
                    buttonMountainLabRoomNikkeTove = Core.mainCanvas.Find("Navigator").Find("MapButtons").Find("919_MountainLabRoomNikkeTove").gameObject;
                    buttonMountainLabRoomNikkeViper = Core.mainCanvas.Find("Navigator").Find("MapButtons").Find("920_MountainLabRoomNikkeViper").gameObject;
                    buttonMountainLabRoomNikkeYan = Core.mainCanvas.Find("Navigator").Find("MapButtons").Find("921_MountainLabRoomNikkeYan").gameObject;
                    buttonGiftShop = Core.mainCanvas.Find("Navigator").Find("MapButtons").Find("922_GiftShop").gameObject;
                    buttonGiftShopInterior = Core.mainCanvas.Find("Navigator").Find("MapButtons").Find("923_GiftShopInterior").gameObject;
                    buttonHarborHomeLivingroom = Core.mainCanvas.Find("Navigator").Find("MapButtons").Find("924_HarborHomeLivingRoom").gameObject;
                    buttonHarborHomeBedroom = Core.mainCanvas.Find("Navigator").Find("MapButtons").Find("925_HarborHomeBedroom").gameObject;
                    buttonHarborHomeBathroom = Core.mainCanvas.Find("Navigator").Find("MapButtons").Find("926_HarborHomeBathroom").gameObject;
                    buttonHarborHomeCloset = Core.mainCanvas.Find("Navigator").Find("MapButtons").Find("927_HarborHomeCloset").gameObject;
                    buttonHarborHomeKitchen = Core.mainCanvas.Find("Navigator").Find("MapButtons").Find("928_HarborHomeKitchen").gameObject;
                    buttonHarborHomePool = Core.mainCanvas.Find("Navigator").Find("MapButtons").Find("929_HarborHomePool").gameObject;
                    buttonHarborHouseEntrance = Core.mainCanvas.Find("Navigator").Find("MapButtons").Find("930_HarborHouseEntrance").gameObject;

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
                    mountainLabRoomNikkeToveLevel = Core.level.Find("919_MountainLabRoomNikkeTove").gameObject;
                    mountainLabRoomNikkeToveRoomtalk = Core.roomTalk.Find("MountainLabRoomNikkeTove").gameObject;
                    mountainLabRoomNikkeViperLevel = Core.level.Find("920_MountainLabRoomNikkeViper").gameObject;
                    mountainLabRoomNikkeViperRoomtalk = Core.roomTalk.Find("MountainLabRoomNikkeViper").gameObject;
                    mountainLabRoomNikkeYanLevel = Core.level.Find("921_MountainLabRoomNikkeYan").gameObject;
                    mountainLabRoomNikkeYanRoomtalk = Core.roomTalk.Find("MountainLabRoomNikkeYan").gameObject;

                    giftShopLevel = Core.level.Find("922_GiftShop").gameObject;
                    giftShopRoomtalk = Core.roomTalk.Find("GiftShop").gameObject;
                    giftShopInteriorLevel = Core.level.Find("923_GiftShopInterior").gameObject;
                    giftShopInteriorRoomtalk = Core.roomTalk.Find("GiftShopInterior").gameObject;

                    harborHomeLivingroomLevel = Core.level.Find("924_HarborHomeLivingRoom").gameObject;
                    harborHomeLivingroomRoomtalk = Core.roomTalk.Find("HarborHomeLivingRoom").gameObject;
                    harborHomeBedroomLevel = Core.level.Find("925_HarborHomeBedroom").gameObject;
                    harborHomeBedroomRoomtalk = Core.roomTalk.Find("HarborHomeBedroom").gameObject;
                    harborHomeBedroomButtonCanvas = GameObject.Instantiate(Core.level.Find("5_MyRoom").Find("PlayerRoom_ButtonCanvas").gameObject, harborHomeBedroomLevel.transform);
                    harborHomeBedroomButtonCanvas.transform.Find("Player_Room_Buttons").gameObject.GetComponent<ParallaxMouseEffect>().parallaxStrength = 0.05f;
                    harborHomeBedroomButtonCanvas.transform.Find("Player_Room_Buttons").Find("SleepButton").localPosition = new Vector2(0, -140);
                    harborHomeBedroomButtonCanvas.transform.Find("Player_Room_Buttons").Find("CalendarButton").localPosition = new Vector2(-20, 250);
                    harborHomeBedroomButtonCanvas.transform.Find("Player_Room_Buttons").Find("PCButton").localPosition = new Vector2(-720, -100);
                    harborHomeBathroomLevel = Core.level.Find("926_HarborHomeBathroom").gameObject;
                    harborHomeBathroomRoomtalk = Core.roomTalk.Find("HarborHomeBathroom").gameObject;
                    harborHomeClosetLevel = Core.level.Find("927_HarborHomeCloset").gameObject;
                    harborHomeClosetRoomtalk = Core.roomTalk.Find("HarborHomeCloset").gameObject;
                    harborHomeKitchenLevel = Core.level.Find("928_HarborHomeKitchen").gameObject;
                    harborHomeKitchenRoomtalk = Core.roomTalk.Find("HarborHomeKitchen").gameObject;
                    harborHomePoolLevel = Core.level.Find("929_HarborHomePool").gameObject;
                    harborHomePoolRoomtalk = Core.roomTalk.Find("HarborHomePool").gameObject;
                    harborHouseEntranceLevel = Core.level.Find("930_HarborHouseEntrance").gameObject;
                    harborHouseEntranceRoomtalk = Core.roomTalk.Find("HarborHouseEntrance").gameObject;

                    // Create Harbor Home Entrance radial button in Foundry
                    CreateHarborHouseEntranceRadialButton();

                    // Initialize ModShops and GiftStore
                    InitializeModShops();
                    giftShopItemBodyLotion = GameObject.Instantiate(Core.mainCanvas.Find("ShopCore").Find("GeneralStore").Find("Core").Find("Groceries (1)").gameObject, giftStore.transform.Find("Core"));
                    giftShopItemSunscreen = AddItemToGiftStore("Sunscreen", "$600", "Sunscreen.PNG");
                    giftShopItemRing = AddItemToGiftStore("Ring", "$700", "Ring.PNG");
                    giftShopItemBikini = AddItemToGiftStore("Bikini", "$900", "Bikini.PNG");
                    giftShopItemFigure = AddItemToGiftStore("Action Figure", "$950", "Figure.PNG");
                    giftShopItemBouquet = AddItemToGiftStore("Tropical Flower Bouquet", "$1100", "Bouquet.PNG");
                    giftShopItemSunglasses = AddItemToGiftStore("Sunglasses", "$1200", "Sunglasses.PNG");
                    giftShopItemParasol = AddItemToGiftStore("Parasol", "$1600", "Parasol.PNG");
                    giftShopItemNecklace = AddItemToGiftStore("Shark Tooth Necklace", "$1850", "Necklace.PNG");
                    giftShopItemBonsai = AddItemToGiftStore("Bonsai Tree", "$3500", "Bonsai.PNG");

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
                    secretBeachGatekeeperB.AddComponent<FadeInSprite>();
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

                    harborHomeBathroomLevelB1 = GameObject.Instantiate(harborHomeBathroomLevel.transform.Find("926_HarborHomeBathroom").gameObject, harborHomeBathroomLevel.transform);
                    harborHomeBathroomLevelB1.name = "ShowerGlassOverlay";
                    harborHomeBathroomLevelB1.GetComponent<SpriteRenderer>().sortingOrder = 0;
                    Destroy(harborHomeBathroomLevelB1.GetComponent<ParallaxMouseEffect>());
                    SetNewLevelSprite(harborHomeBathroomLevelB1, Core.locationPath, "HHomeBathroomB1.PNG", 2048, 1136);
                    harborHomeKitchenLevelB1 = GameObject.Instantiate(harborHomeKitchenLevel.transform.Find("928_HarborHomeKitchen").gameObject, harborHomeKitchenLevel.transform);
                    harborHomeKitchenLevelB1.name = "FridgeOpenOverlay";
                    harborHomeKitchenLevelB1.SetActive(false);
                    harborHomeKitchenLevelB1.GetComponent<SpriteRenderer>().sortingOrder = harborHomeKitchenLevel.GetComponent<SpriteRenderer>().sortingOrder + 1;
                    Destroy(harborHomeKitchenLevelB1.GetComponent<ParallaxMouseEffect>());
                    SetNewLevelSprite(harborHomeKitchenLevelB1, Core.locationPath, "HHomeKitchenB1.PNG", 2048, 1136);
                    harborHomeLivingroomLevel.GetComponent<SpriteRenderer>().sortingOrder = 2;
                    harborHomeBathroomNPCShower = GameObject.Instantiate(new GameObject(), harborHomeBathroomLevel.transform.Find("NPCs")).transform; harborHomeBathroomNPCShower.name = "Shower";
                    harborHomeBedroomNPCBedleft = GameObject.Instantiate(new GameObject(), harborHomeBedroomLevel.transform.Find("NPCs")).transform; harborHomeBedroomNPCBedleft.name = "Bedleft";
                    harborHomeBedroomNPCBedright = GameObject.Instantiate(new GameObject(), harborHomeBedroomLevel.transform.Find("NPCs")).transform; harborHomeBedroomNPCBedright.name = "Bedright";
                    harborHomeClosetNPCChangingleft = GameObject.Instantiate(new GameObject(), harborHomeClosetLevel.transform.Find("NPCs")).transform; harborHomeClosetNPCChangingleft.name = "Changingleft";
                    harborHomeClosetNPCChangingright = GameObject.Instantiate(new GameObject(), harborHomeClosetLevel.transform.Find("NPCs")).transform; harborHomeClosetNPCChangingright.name = "Changingright";
                    harborHomeKitchenNPCFridge = GameObject.Instantiate(new GameObject(), harborHomeKitchenLevel.transform.Find("NPCs")).transform; harborHomeKitchenNPCFridge.name = "Fridge";
                    harborHomeKitchenNPCSink = GameObject.Instantiate(new GameObject(), harborHomeKitchenLevel.transform.Find("NPCs")).transform; harborHomeKitchenNPCSink.name = "Sink";
                    harborHomeLivingroomNPCCouchleft = GameObject.Instantiate(new GameObject(), harborHomeLivingroomLevel.transform.Find("NPCs")).transform; harborHomeLivingroomNPCCouchleft.name = "Couchleft";
                    harborHomeLivingroomNPCCouchright = GameObject.Instantiate(new GameObject(), harborHomeLivingroomLevel.transform.Find("NPCs")).transform; harborHomeLivingroomNPCCouchright.name = "Couchright";
                    harborHomePoolNPCTanningleft = GameObject.Instantiate(new GameObject(), harborHomePoolLevel.transform.Find("NPCs")).transform; harborHomePoolNPCTanningleft.name = "Tanningleft";
                    harborHomePoolNPCTanningright = GameObject.Instantiate(new GameObject(), harborHomePoolLevel.transform.Find("NPCs")).transform; harborHomePoolNPCTanningright.name = "Tanningright";

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
                    harborHomeBedroomSwapApplied = false;
                }
            }

            if (Core.loadedBases)
            {
                // Check if we need to swap player room for Harbor Home bedroom (only once per scene load)
                if (!harborHomeBedroomSwapApplied && SaveManager.GetBool("HarborHome_Slept"))
                {
                    GameObject vanillaRoom = Core.level.Find("5_MyRoom")?.gameObject;
                    GameObject hallwayButton = Core.mainCanvas.Find("Navigator")?.Find("MapButtons")?.Find("09_Hallway")?.gameObject;
                    if (vanillaRoom != null && harborHomeBedroomLevel != null)
                    {
                        vanillaRoom.SetActive(false);
                        hallwayButton.SetActive(false);
                        harborHomeBedroomLevel.SetActive(true);
                        harborHomeBedroomSwapApplied = true;
                        Debug.Log("[Places] Swapped vanilla room for Harbor Home bedroom");
                    }
                }
                if (!insideHarborHome && harborHomeLivingroomLevel.activeSelf) { insideHarborHome = true; }
                if (insideHarborHome && (harborHouseEntranceLevel.activeSelf || levelMyRoom.activeSelf)) { insideHarborHome = false; }
                if (insideHarborHome && !Core.GetVariableBool("Disable-Specific-RNGEvents")) { Core.FindAndModifyVariableBool("Disable-Specific-RNGEvents", true); }
                if (!insideHarborHome && Core.GetVariableBool("Disable-Specific-RNGEvents") != Core.toggleRepeatableBedEvents.GetComponent<TogglePropertyBool>().isOn) 
                { Core.FindAndModifyVariableBool("Disable-Specific-RNGEvents", Core.toggleRepeatableBedEvents.GetComponent<TogglePropertyBool>().isOn); }

                if (levelMall.activeSelf && randomNumMall == -1) { randomNumMall = Core.GetRandomNumber(100); Debug.Log("randomNumMall: " + randomNumMall); } if (!levelMall.activeSelf) { randomNumMall = -1; }
                if (mountainLabRoomNikkeAnisLevel.activeSelf && randomNumMLRoomAnis == -1) { randomNumMLRoomAnis = Core.GetRandomNumber(100); Debug.Log("randomNumMLRoomAnis: " + randomNumMLRoomAnis); } if (!mountainLabRoomNikkeAnisLevel.activeSelf) { randomNumMLRoomAnis = -1; }

                if (secretBeachLevel.activeSelf || giftShopLevel.activeSelf) { buttonBeach.SetActive(true); }
                if (secretBeachLevel.activeSelf || harborHouseEntranceLevel.activeSelf) { buttonCar.SetActive(true); }

                harborHomeKitchenLevelB1.SetActive(Schedule.anisLocation == "HarborHomeKitchenFridge");

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
                    mountainLabRoomNikkeRosannaLevel.activeSelf || mountainLabRoomNikkeSakuraLevel.activeSelf || mountainLabRoomNikkeToveLevel.activeSelf || mountainLabRoomNikkeViperLevel.activeSelf || mountainLabRoomNikkeYanLevel.activeSelf);
                buttonMountainLabRoomNikkeNeon.SetActive(SaveManager.GetBool("Voyeur_SeenNeon") && mountainLabCorridorNikke2Level.activeSelf);
                buttonMountainLabRoomNikkePepper.SetActive(SaveManager.GetBool("Voyeur_SeenPepper") && mountainLabCorridorNikke2Level.activeSelf);
                buttonMountainLabRoomNikkeRapi.SetActive(SaveManager.GetBool("Voyeur_SeenRapi") && mountainLabCorridorNikke2Level.activeSelf);
                buttonMountainLabRoomNikkeRosanna.SetActive(SaveManager.GetBool("Voyeur_SeenRosanna") && mountainLabCorridorNikke2Level.activeSelf);
                buttonMountainLabRoomNikkeSakura.SetActive(SaveManager.GetBool("Voyeur_SeenSakura") && mountainLabCorridorNikke2Level.activeSelf);
                buttonMountainLabRoomNikkeTove.SetActive(SaveManager.GetBool("Voyeur_SeenTove") && mountainLabCorridorNikke2Level.activeSelf);
                buttonMountainLabRoomNikkeViper.SetActive(SaveManager.GetBool("Voyeur_SeenViper") && mountainLabCorridorNikke2Level.activeSelf);
                buttonMountainLabRoomNikkeYan.SetActive(SaveManager.GetBool("Voyeur_SeenYan") && mountainLabCorridorNikke2Level.activeSelf);
                buttonGiftShop.SetActive((SaveManager.GetBool("Voyeur_SeenCenti") && SaveManager.GetBool("Voyeur_SeenYan")) && (Core.levelBeach.activeSelf || giftShopInteriorLevel.activeSelf));
                buttonGiftShopInterior.SetActive(SaveManager.GetInt("GiftShop_BuildCounter") >= 2 && giftShopLevel.activeSelf);
                buttonHarborHouseEntrance.SetActive(harborHomeLivingroomLevel.activeSelf);
                buttonHarborHomeLivingroom.SetActive(SaveManager.GetBool("HarborHome_Bought") && (harborHouseEntranceLevel.activeSelf || harborHomeBedroomLevel.activeSelf || harborHomeKitchenLevel.activeSelf ||
                    harborHomePoolLevel.activeSelf));
                buttonHarborHomeBedroom.SetActive(harborHomeLivingroomLevel.activeSelf || harborHomeClosetLevel.activeSelf || harborHomeBathroomLevel.activeSelf);
                buttonHarborHomeBathroom.SetActive(harborHomeBedroomLevel.activeSelf || harborHomeClosetLevel.activeSelf);
                buttonHarborHomeCloset.SetActive(harborHomeBedroomLevel.activeSelf || harborHomeBathroomLevel.activeSelf);
                buttonHarborHomeKitchen.SetActive(harborHomeLivingroomLevel.activeSelf);
                buttonHarborHomePool.SetActive(harborHomeLivingroomLevel.activeSelf);
                buttonHarborHouseEntrance.SetActive(harborHomeLivingroomLevel.activeSelf);

                if (Core.GetVariableBool("rainy-day"))
                {
                    if (giftShopInteriorLevel.activeSelf || harborHomeLivingroomLevel.activeSelf || harborHomeBedroomLevel.activeSelf || harborHomeKitchenLevel.activeSelf) { weatherInsideRain.SetActive(true); }
                    if (giftShopLevel.activeSelf|| harborHouseEntranceLevel.activeSelf || harborHomePoolLevel.activeSelf || secretBeachLevel.activeSelf) { weatherOutsideRain.SetActive(true); }
                }
                if (Core.GetVariableBool("snowy-day"))
                {
                    if (giftShopInteriorLevel.activeSelf || harborHomeLivingroomLevel.activeSelf || harborHomeBedroomLevel.activeSelf || harborHomeKitchenLevel.activeSelf) { weatherInsideSnow.SetActive(true); }
                    if (giftShopLevel.activeSelf || harborHouseEntranceLevel.activeSelf || harborHomePoolLevel.activeSelf || secretBeachLevel.activeSelf) { weatherOutsideSnow.SetActive(true); }
                }

                if (buttonSecretBeach.transform.GetChild(0).gameObject.activeSelf) { ClickMapButton(secretBeachRoomtalk, 0); buttonSecretBeach.transform.GetChild(0).gameObject.SetActive(false); }
                if (buttonMountainLab.transform.GetChild(0).gameObject.activeSelf) { ClickMapButton(mountainLabRoomtalk, 1); buttonMountainLab.transform.GetChild(0).gameObject.SetActive(false); }
                if (buttonMountainLabCorridorNikke1.transform.GetChild(0).gameObject.activeSelf) { ClickMapButton(mountainLabCorridorNikke1Roomtalk, 2); buttonMountainLabCorridorNikke1.transform.GetChild(0).gameObject.SetActive(false); }
                if (buttonMountainLabRoomNikkeAnis.transform.GetChild(0).gameObject.activeSelf) { ClickMapButton(mountainLabRoomNikkeAnisRoomtalk, 3); buttonMountainLabRoomNikkeAnis.transform.GetChild(0).gameObject.SetActive(false); }
                if (buttonMountainLabRoomNikkeCenti.transform.GetChild(0).gameObject.activeSelf) { ClickMapButton(mountainLabRoomNikkeCentiRoomtalk, 4); buttonMountainLabRoomNikkeCenti.transform.GetChild(0).gameObject.SetActive(false); }
                if (buttonMountainLabRoomNikkeDorothy.transform.GetChild(0).gameObject.activeSelf) { ClickMapButton(mountainLabRoomNikkeDorothyRoomtalk, 5); buttonMountainLabRoomNikkeDorothy.transform.GetChild(0).gameObject.SetActive(false); }
                if (buttonMountainLabRoomNikkeElegg.transform.GetChild(0).gameObject.activeSelf) { ClickMapButton(mountainLabRoomNikkeEleggRoomtalk, 6); buttonMountainLabRoomNikkeElegg.transform.GetChild(0).gameObject.SetActive(false); }
                if (buttonMountainLabRoomNikkeFrima.transform.GetChild(0).gameObject.activeSelf) { ClickMapButton(mountainLabRoomNikkeFrimaRoomtalk, 7); buttonMountainLabRoomNikkeFrima.transform.GetChild(0).gameObject.SetActive(false); }
                if (buttonMountainLabRoomNikkeGuilty.transform.GetChild(0).gameObject.activeSelf) { ClickMapButton(mountainLabRoomNikkeGuiltyRoomtalk, 8); buttonMountainLabRoomNikkeGuilty.transform.GetChild(0).gameObject.SetActive(false); }
                if (buttonMountainLabRoomNikkeHelm.transform.GetChild(0).gameObject.activeSelf) { ClickMapButton(mountainLabRoomNikkeHelmRoomtalk, 9); buttonMountainLabRoomNikkeHelm.transform.GetChild(0).gameObject.SetActive(false); }
                if (buttonMountainLabRoomNikkeMaiden.transform.GetChild(0).gameObject.activeSelf) { ClickMapButton(mountainLabRoomNikkeMaidenRoomtalk, 10); buttonMountainLabRoomNikkeMaiden.transform.GetChild(0).gameObject.SetActive(false); }
                if (buttonMountainLabRoomNikkeMary.transform.GetChild(0).gameObject.activeSelf) { ClickMapButton(mountainLabRoomNikkeMaryRoomtalk, 11); buttonMountainLabRoomNikkeMary.transform.GetChild(0).gameObject.SetActive(false); }
                if (buttonMountainLabRoomNikkeMast.transform.GetChild(0).gameObject.activeSelf) { ClickMapButton(mountainLabRoomNikkeMastRoomtalk, 12); buttonMountainLabRoomNikkeMast.transform.GetChild(0).gameObject.SetActive(false); }
                if (buttonMountainLabCorridorNikke2.transform.GetChild(0).gameObject.activeSelf) { ClickMapButton(mountainLabCorridorNikke2Roomtalk, 13); buttonMountainLabCorridorNikke2.transform.GetChild(0).gameObject.SetActive(false); }
                if (buttonMountainLabRoomNikkeNeon.transform.GetChild(0).gameObject.activeSelf) { ClickMapButton(mountainLabRoomNikkeNeonRoomtalk, 14); buttonMountainLabRoomNikkeNeon.transform.GetChild(0).gameObject.SetActive(false); }
                if (buttonMountainLabRoomNikkePepper.transform.GetChild(0).gameObject.activeSelf) { ClickMapButton(mountainLabRoomNikkePepperRoomtalk, 15); buttonMountainLabRoomNikkePepper.transform.GetChild(0).gameObject.SetActive(false); }
                if (buttonMountainLabRoomNikkeRapi.transform.GetChild(0).gameObject.activeSelf) { ClickMapButton(mountainLabRoomNikkeRapiRoomtalk, 16); buttonMountainLabRoomNikkeRapi.transform.GetChild(0).gameObject.SetActive(false); }
                if (buttonMountainLabRoomNikkeRosanna.transform.GetChild(0).gameObject.activeSelf) { ClickMapButton(mountainLabRoomNikkeRosannaRoomtalk, 17); buttonMountainLabRoomNikkeRosanna.transform.GetChild(0).gameObject.SetActive(false); }
                if (buttonMountainLabRoomNikkeSakura.transform.GetChild(0).gameObject.activeSelf) { ClickMapButton(mountainLabRoomNikkeSakuraRoomtalk, 18); buttonMountainLabRoomNikkeSakura.transform.GetChild(0).gameObject.SetActive(false); }
                if (buttonMountainLabRoomNikkeTove.transform.GetChild(0).gameObject.activeSelf) { ClickMapButton(mountainLabRoomNikkeToveRoomtalk, 19); buttonMountainLabRoomNikkeTove.transform.GetChild(0).gameObject.SetActive(false); }
                if (buttonMountainLabRoomNikkeViper.transform.GetChild(0).gameObject.activeSelf) { ClickMapButton(mountainLabRoomNikkeViperRoomtalk, 20); buttonMountainLabRoomNikkeViper.transform.GetChild(0).gameObject.SetActive(false); }
                if (buttonMountainLabRoomNikkeYan.transform.GetChild(0).gameObject.activeSelf) { ClickMapButton(mountainLabRoomNikkeYanRoomtalk, 21); buttonMountainLabRoomNikkeYan.transform.GetChild(0).gameObject.SetActive(false); }
                if (buttonGiftShop.transform.GetChild(0).gameObject.activeSelf) { ClickMapButton(giftShopRoomtalk, 22); buttonGiftShop.transform.GetChild(0).gameObject.SetActive(false); }
                if (buttonGiftShopInterior.transform.GetChild(0).gameObject.activeSelf) { ClickMapButton(giftShopInteriorRoomtalk, 23); buttonGiftShopInterior.transform.GetChild(0).gameObject.SetActive(false); }
                if (buttonHarborHomeLivingroom.transform.GetChild(0).gameObject.activeSelf) { ClickMapButton(harborHomeLivingroomRoomtalk, 24); buttonHarborHomeLivingroom.transform.GetChild(0).gameObject.SetActive(false); }
                if (buttonHarborHomeBedroom.transform.GetChild(0).gameObject.activeSelf) { ClickMapButton(harborHomeBedroomRoomtalk, 25); buttonHarborHomeBedroom.transform.GetChild(0).gameObject.SetActive(false); harborHomeBedroomButtonCanvas.SetActive(true); }
                if (buttonHarborHomeBathroom.transform.GetChild(0).gameObject.activeSelf) { ClickMapButton(harborHomeBathroomRoomtalk, 26); buttonHarborHomeBathroom.transform.GetChild(0).gameObject.SetActive(false); }
                if (buttonHarborHomeCloset.transform.GetChild(0).gameObject.activeSelf) { ClickMapButton(harborHomeClosetRoomtalk, 27); buttonHarborHomeCloset.transform.GetChild(0).gameObject.SetActive(false); }
                if (buttonHarborHomeKitchen.transform.GetChild(0).gameObject.activeSelf) { ClickMapButton(harborHomeKitchenRoomtalk, 28); buttonHarborHomeKitchen.transform.GetChild(0).gameObject.SetActive(false); }
                if (buttonHarborHomePool.transform.GetChild(0).gameObject.activeSelf) { ClickMapButton(harborHomePoolRoomtalk, 29); buttonHarborHomePool.transform.GetChild(0).gameObject.SetActive(false); }
                if (buttonHarborHouseEntrance.transform.GetChild(0).gameObject.activeSelf) { ClickMapButton(harborHouseEntranceRoomtalk, 30); buttonHarborHouseEntrance.transform.GetChild(0).gameObject.SetActive(false); }

                if (SaveManager.GetBool("HarborHome_Bought") && harborHouseEntranceRadialButtonText.GetComponent<TextMeshProUGUI>().text == "House for Sale") { harborHouseEntranceRadialButtonText.GetComponent<TextMeshProUGUI>().text = "Home"; }

                // Update gift store item visibility based on proxy variables
                if (giftStore != null && giftStore.activeSelf)
                {
                    // Check if giftStore became active this frame
                    if (!wasGiftStoreActive && giftStore.activeInHierarchy)
                    {
                        // Rebuild layout when giftStore becomes active
                        Transform giftStoreCore = giftStore.transform.Find("Core");
                        if (giftStoreCore != null)
                        {
                            LayoutRebuilder.MarkLayoutForRebuild(giftStoreCore as RectTransform);
                            Debug.Log("[GiftStore] Marked layout for rebuild (became active)");
                        }
                    }
                    wasGiftStoreActive = giftStore.activeInHierarchy;

                    float currentTime = Time.time;
                    if (currentTime - lastGiftStoreCheckTime >= giftStoreCheckInterval)
                    {
                        UpdateGiftStoreItemVisibility();
                        lastGiftStoreCheckTime = currentTime;
                    }
                }


                if (Places.secretBeachLevel.activeSelf)
                {
                    secretBeachGatekeeper.transform.Rotate(0, 0, 1f * Time.deltaTime);
                }
            }
        }

        private void InitializeModShops()
        {
            // Create ModShops container if it doesn't exist
            if (modShops == null)
            {
                modShops = new GameObject("ModShops");
                modShops.transform.SetParent(Core.mainCanvas, false);
                modShops.SetActive(false); // Disable ModShops container initially
            }

            // Create GiftStore as a copy of GeneralStore
            if (giftStore == null)
            {
                // Find ShopCore
                Transform shopCore = Core.mainCanvas.Find("ShopCore");
                if (shopCore == null)
                {
                    Debug.LogError("[Places] Could not find ShopCore in mainCanvas");
                    return;
                }

                // Copy UI elements from ShopCore to ModShops (Image (3) first)
                string[] uiElementNames = { "Image (3)", "Image", "Image (2)", "shopname", "CloseStore" };
                foreach (string elementName in uiElementNames)
                {
                    Transform sourceElement = shopCore.Find(elementName);
                    if (sourceElement != null)
                    {
                        GameObject copiedElement = GameObject.Instantiate(sourceElement.gameObject, modShops.transform);
                        copiedElement.name = elementName;
                        copiedElement.SetActive(true); // Ensure UI elements are enabled
                        Debug.Log($"[Places] Copied {elementName} to ModShops");
                    }
                    else
                    {
                        Debug.LogWarning($"[Places] Could not find {elementName} in ShopCore");
                    }
                }

                // Copy GeneralStore last
                GameObject generalStore = shopCore.Find("GeneralStore")?.gameObject;
                if (generalStore == null)
                {
                    Debug.LogError("[Places] Could not find GeneralStore in ShopCore");
                    return;
                }

                // Instantiate GiftStore as a copy of GeneralStore
                giftStore = GameObject.Instantiate(generalStore, modShops.transform);
                giftStore.name = "GiftStore";
                giftStore.SetActive(false); // Start inactive

                // Delete all GameObjects inside GiftStore's Core child
                Transform coreTransform = giftStore.transform.Find("Core");
                if (coreTransform != null)
                {
                    // Destroy all children of Core
                    for (int i = coreTransform.childCount - 1; i >= 0; i--)
                    {
                        GameObject.Destroy(coreTransform.GetChild(i).gameObject);
                    }
                    Debug.Log("[Places] GiftStore created and emptied successfully");
                }
                else
                {
                    Debug.LogWarning("[Places] Could not find Core child in GiftStore");
                }

                // Move Image (3) to first position
                Transform image3 = modShops.transform.Find("Image (3)");
                if (image3 != null)
                {
                    image3.SetAsFirstSibling();
                    Debug.Log("[Places] Image (3) moved to first position");
                }

                // Replace ButtonInstructions on CloseStore with standard Unity Button
                Transform closeStoreTrans = modShops.transform.Find("CloseStore");
                if (closeStoreTrans != null)
                {
                    // Ensure CloseStore is active during configuration
                    bool wasActive = closeStoreTrans.gameObject.activeSelf;
                    closeStoreTrans.gameObject.SetActive(true);

                    // Remove ButtonInstructions component IMMEDIATELY (not at end of frame)
                    ButtonInstructions buttonInstructions = closeStoreTrans.GetComponent<ButtonInstructions>();
                    if (buttonInstructions != null)
                    {
                        GameObject.DestroyImmediate(buttonInstructions);
                        Debug.Log("[Places] Removed ButtonInstructions from CloseStore (copy)");
                    }

                    // Add a fresh Unity Button component
                    UnityEngine.UI.Button button = closeStoreTrans.gameObject.AddComponent<UnityEngine.UI.Button>();

                    // Set Target Graphic to child Image for visual feedback
                    Transform imageChild = closeStoreTrans.Find("Image");
                    if (imageChild != null)
                    {
                        UnityEngine.UI.Image targetImage = imageChild.GetComponent<UnityEngine.UI.Image>();
                        if (targetImage != null)
                        {
                            button.targetGraphic = targetImage;
                            Debug.Log("[Places] Set Button target graphic to child Image");
                        }
                    }

                    // Configure ColorBlock for highlighted color
                    var colors = button.colors;
                    colors.normalColor = new Color(0.7f, 0.7f, 0.7f, 1f); // Grey for default
                    colors.highlightedColor = new Color(1f, 0.75f, 0.8f, 1f); // Light pink for hover
                    colors.pressedColor = new Color(0.9f, 0.5f, 0.6f, 1f); // Darker pink for press
                    colors.selectedColor = new Color(0.7f, 0.7f, 0.7f, 1f); // Grey
                    colors.disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.5f); // Dark grey
                    colors.colorMultiplier = 1f;
                    colors.fadeDuration = 0.1f;
                    button.colors = colors;

                    // Set transition to ColorTint
                    button.transition = UnityEngine.UI.Selectable.Transition.ColorTint;

                    // Add onClick listener
                    button.onClick.RemoveAllListeners();
                    button.onClick.AddListener(DisableModShops);

                    // Restore original active state
                    closeStoreTrans.gameObject.SetActive(wasActive);

                    Debug.Log("[Places] Configured CloseStore with Unity Button");
                }
            }
        }

        private void CreateHarborHouseEntranceRadialButton()
        {
            // Find World_Map > Canvas > Core > Radial_Buttons
            Transform worldMap = Core.FindInActiveObjectByName("World_Map");
            if (worldMap == null)
            {
                Debug.LogError("[Places] Could not find World_Map for radial button creation");
                return;
            }

            Transform radialButtons = worldMap.Find("Canvas")?.Find("Core")?.Find("Radial_Buttons");
            if (radialButtons == null)
            {
                Debug.LogError("[Places] Could not find Radial_Buttons");
                return;
            }

            // Find the Seaside > Beach button to copy
            Transform beachButton = radialButtons.Find("Seaside")?.Find("Beach");
            if (beachButton == null)
            {
                Debug.LogError("[Places] Could not find Seaside > Beach button to copy");
                return;
            }

            // Find the Foundry section
            Transform foundry = radialButtons.Find("Foundry");
            if (foundry == null)
            {
                Debug.LogError("[Places] Could not find Foundry section");
                return;
            }

            // Create a copy of the Beach button in Foundry
            harborHouseEntranceRadialButton = GameObject.Instantiate(beachButton.gameObject, foundry);
            harborHouseEntranceRadialButton.name = "HarborHouseEntrance";
            harborHouseEntranceRadialButtonText = harborHouseEntranceRadialButton.transform.Find("Text (TMP)").gameObject;

            // Change the text to "House for Sale"
            Transform textTMP = harborHouseEntranceRadialButton.transform.Find("Text (TMP)");
            if (textTMP != null)
            {
                TextMeshProUGUI textComponent = textTMP.GetComponent<TextMeshProUGUI>();
                if (textComponent != null)
                {
                    textComponent.text = "House for Sale";
                }
            }

            // Remove ButtonInstructions and add Unity Button
            ButtonInstructions buttonInstructions = harborHouseEntranceRadialButton.GetComponent<ButtonInstructions>();
            if (buttonInstructions != null)
            {
                GameObject.DestroyImmediate(buttonInstructions);
            }

            // Add Unity Button component
            UnityEngine.UI.Button button = harborHouseEntranceRadialButton.AddComponent<UnityEngine.UI.Button>();

            // Set Target Graphic to the Image component for visual feedback
            UnityEngine.UI.Image targetImage = harborHouseEntranceRadialButton.GetComponent<UnityEngine.UI.Image>();
            if (targetImage != null)
            {
                button.targetGraphic = targetImage;
            }

            // Configure ColorBlock
            var colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(0.9f, 0.9f, 0.9f, 1f);
            colors.pressedColor = new Color(0.8f, 0.8f, 0.8f, 1f);
            colors.selectedColor = Color.white;
            colors.disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
            colors.colorMultiplier = 1f;
            colors.fadeDuration = 0.1f;
            button.colors = colors;
            button.transition = UnityEngine.UI.Selectable.Transition.ColorTint;

            // Add onClick listener
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnHarborHouseEntranceRadialButtonClick);

            Debug.Log("[Places] Created Harbor Home Entrance radial button in Foundry");
        }

        private void OnHarborHouseEntranceRadialButtonClick()
        {
            // Instruction 1: Set Active Click_Effect to True
            Transform worldMap = Core.FindInActiveObjectByName("World_Map");
            if (worldMap != null)
            {
                Transform clickEffect = worldMap.Find("Click_Effect");
                if (clickEffect != null)
                {
                    clickEffect.gameObject.SetActive(true);
                    ClickMapButton(harborHouseEntranceRoomtalk, 30, "HarborHomeMusic");
                }
            }

            // Instruction 2: Check If Core[Lock-Game] = False
            if (Core.GetVariableBool("Lock-Game"))
            {
                return; // Game is locked, don't proceed
            }

            // Instruction 3: Set Core[Upcoming-Level] = vanillaLevelCount + 30
            Core.FindAndModifyVariableDouble("Upcoming-Level", vanillaLevelCount + 30);

            // Instruction 4: District_Buttons CanvasGroup Interactable = True
            if (worldMap != null)
            {
                Transform districtButtons = worldMap.Find("Canvas")?.Find("Core")?.Find("District_Buttons");
                if (districtButtons != null)
                {
                    CanvasGroup canvasGroup = districtButtons.GetComponent<CanvasGroup>();
                    if (canvasGroup != null)
                    {
                        canvasGroup.interactable = true;
                    }
                }
            }

            // Instruction 5: Set Core[Start-Transfer] = True
            Core.FindAndModifyVariableBool("Start-Transfer", true);
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
            GameObject level = CreateNewLevel(index + "_" + name, Core.locationPath, pathName + ".PNG", pathName + "B.PNG", pathName + "Mask.PNG", parallaxStrength, name == "SecretBeach" || name == "GiftShop");
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
        public GameObject CreateNewLevel(string name, string pathToCG, string baseSprite, string secondarySprite, string maskSprite, float parallaxStrength, bool keepAudioAndParticles = false)
        {
            GameObject newLevel = GameObject.Instantiate(GameObject.Find("5_Levels").transform.Find("14_Beach").gameObject, GameObject.Find("5_Levels").transform);
            newLevel.name = name;
            newLevel.GetComponent<ParallaxMouseEffect>().parallaxStrength = parallaxStrength;
            GameObject secondaryTex = newLevel.transform.GetChild(1).gameObject;
            secondaryTex.name = name;
            secondaryTex.GetComponent<ParallaxMouseEffect>().parallaxStrength = parallaxStrength;
            GameObject NPCs = newLevel.transform.GetChild(2).gameObject;
            Destroy(NPCs.GetComponent<Actions>());
            Destroy(NPCs.GetComponent<Conditions>());
            Destroy(NPCs.GetComponent<DisableChildren>());
            foreach (Transform npc in NPCs.transform)
            {
                Destroy(npc.gameObject);
            }
            
            // Remove duplicate "14_Beach (1)" if it exists
            for (int i = newLevel.transform.childCount - 1; i >= 0; i--)
            {
                Transform child = newLevel.transform.GetChild(i);
                if (child.name == "14_Beach (1)")
                {
                    Destroy(child.gameObject);
                    break;
                }
            }
            
            // Disable Audio Source and Particle System (2) if not needed
            if (!keepAudioAndParticles)
            {
                for (int i = newLevel.transform.childCount - 1; i >= 0; i--)
                {
                    Transform child = newLevel.transform.GetChild(i);
                    if (child.name == "Audio Source" || child.name == "Particle System (2)")
                    {
                        child.gameObject.SetActive(false);
                    }
                }
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
        public static void SetNewLevelSprite(GameObject gO, string pathToCG, string baseSprite, int width, int height)
        {
            Material mat = new Material(gO.GetComponent<SpriteRenderer>().material);

            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            var rawData = System.IO.File.ReadAllBytes(pathToCG + baseSprite);
            tex.LoadImage(rawData);
            tex.filterMode = FilterMode.Point;
            Sprite newSprite = Sprite.Create(tex, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 70.32f);
            gO.GetComponent<SpriteRenderer>().sprite = newSprite;
        }
        public static void UpdateGiftShopTextureBasedOnBuildStatus()
        {
            if (giftShopLevel == null)
            {
                Debug.LogWarning("GiftShopLevel not initialized yet, cannot update texture");
                return;
            }

            int giftShopBuildCounter = SaveManager.GetInt("GiftShop_BuildCounter", 0);
            bool giftShopBuilt = giftShopBuildCounter >= 2;
            string baseSpriteName = giftShopBuilt ? "GiftShop.PNG" : "GiftShopAlt.PNG";
            string secondarySpriteName = giftShopBuilt ? "GiftShopB.PNG" : "GiftShopAltB.PNG";

            Debug.Log($"[UpdateGiftShopTexture] GiftShop_BuildCounter: {giftShopBuildCounter}, Built: {giftShopBuilt}, Using: {baseSpriteName}");
            
            // Update primary sprite
            SetNewLevelSprite(giftShopLevel, Core.locationPath, baseSpriteName, 1920, 1080);
            
            // Update secondary sprite (second child)
            if (giftShopLevel.transform.childCount > 1)
            {
                SetNewLevelSprite(giftShopLevel.transform.GetChild(1).gameObject, Core.locationPath, secondarySpriteName, 1920, 1080);
            }

            Debug.Log($"Gift Shop texture updated. Built: {giftShopBuilt}, Using: {baseSpriteName}");
        }
        public void ClickMapButton(GameObject roomTalk, Double index, string musicName = null)
        {
            if (!Core.GetVariableBool("Lock-Game"))
            {
                currentRoomTalk = roomTalk;
                Core.FindAndModifyVariableDouble("Upcoming-Level", vanillaLevelCount + index);
                Core.FindAndModifyVariableBool("Start-Transfer", true);

                // Handle music switching if a music name is provided
                if (!string.IsNullOrEmpty(musicName))
                {
                    GameObject audioPlayerParent = GameObject.Find("12_AudioPlayer");
                    if (audioPlayerParent != null)
                    {
                        // Disable all children of 12_AudioPlayer
                        foreach (Transform child in audioPlayerParent.transform)
                        {
                            child.gameObject.SetActive(false);
                        }

                        // Enable the specific child with the matching name
                        Transform targetMusic = audioPlayerParent.transform.Find(musicName);
                        if (targetMusic != null)
                        {
                            targetMusic.gameObject.SetActive(true);
                            Debug.Log($"[Places] Switched music to: {musicName}");
                        }
                        else
                        {
                            Debug.LogWarning($"[Places] Could not find music child '{musicName}' under 12_AudioPlayer");
                        }
                    }
                    else
                    {
                        Debug.LogWarning("[Places] Could not find 12_AudioPlayer in scene");
                    }
                }

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
                Invoke(nameof(EnableRoomTalk), 1.0f);
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

        public static void ActivateShop(GameObject shop)
        {
            shop.SetActive(true);
            modShops.SetActive(true);

            // CloseStore button is already configured in InitializeModShops()
            // No need to set up listener here anymore

            //Signals.Emit(MainStory.fadeUISignal);
        }

        private static void DisableModShops()
        {
            if (modShops != null)
            {
                modShops.SetActive(false);
            }
        }

        public static GameObject AddItemToGiftStore(string itemName, string itemPrice, string itemImageFileName)
        {
            // Find the Beer template from ShopCore
            Transform shopCore = Core.mainCanvas.Find("ShopCore");
            if (shopCore == null)
            {
                Debug.LogError("[AddItemToGiftStore] Could not find ShopCore");
                return null;
            }

            Transform generalStore = shopCore.Find("GeneralStore");
            if (generalStore == null)
            {
                Debug.LogError("[AddItemToGiftStore] Could not find GeneralStore");
                return null;
            }

            Transform coreTransform = generalStore.Find("Core");
            if (coreTransform == null)
            {
                Debug.LogError("[AddItemToGiftStore] Could not find GeneralStore > Core");
                return null;
            }

            Transform beerTemplate = coreTransform.Find("Beer");
            if (beerTemplate == null)
            {
                Debug.LogError("[AddItemToGiftStore] Could not find Beer template");
                return null;
            }

            // Find giftStore Core
            if (giftStore == null)
            {
                Debug.LogError("[AddItemToGiftStore] giftStore is not initialized");
                return null;
            }

            Transform giftStoreCore = giftStore.transform.Find("Core");
            if (giftStoreCore == null)
            {
                Debug.LogError("[AddItemToGiftStore] Could not find giftStore > Core");
                return null;
            }

            // Instantiate the item from Beer template
            GameObject newItem = GameObject.Instantiate(beerTemplate.gameObject, giftStoreCore);
            newItem.name = itemName;
            newItem.SetActive(true);

            // Change the item image (child position 0 named "Image")
            if (newItem.transform.childCount > 0)
            {
                Transform imageChild = newItem.transform.GetChild(0);
                if (imageChild != null && imageChild.name == "Image")
                {
                    UnityEngine.UI.Image imageComponent = imageChild.GetComponent<UnityEngine.UI.Image>();
                    if (imageComponent != null)
                    {
                        // Load new sprite from itemsPath
                        Texture2D tex = new Texture2D(256, 256, TextureFormat.RGBA32, false);
                        string fullPath = Core.exePath + Core.itemsPath + itemImageFileName;
                        var rawData = System.IO.File.ReadAllBytes(fullPath);
                        tex.LoadImage(rawData);
                        tex.filterMode = FilterMode.Point;
                        Sprite newSprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
                        imageComponent.sprite = newSprite;
                        Debug.Log($"[AddItemToGiftStore] Updated item image for {itemName}");
                    }
                }
            }

            // Change the item name text ("Text (TMP)" child)
            Transform textChild = newItem.transform.Find("Text (TMP)");
            if (textChild != null)
            {
                TextMeshProUGUI textComponent = textChild.GetComponent<TextMeshProUGUI>();
                if (textComponent != null)
                {
                    textComponent.text = itemName;
                    Debug.Log($"[AddItemToGiftStore] Updated item name text to {itemName}");
                }
            }

            // Change the price text (child position 2 named "Image" > "Text (TMP)")
            if (newItem.transform.childCount > 2)
            {
                Transform priceImageChild = newItem.transform.GetChild(2);
                if (priceImageChild != null && priceImageChild.name == "Image")
                {
                    Transform priceTextChild = priceImageChild.Find("Text (TMP)");
                    if (priceTextChild != null)
                    {
                        TextMeshProUGUI priceTextComponent = priceTextChild.GetComponent<TextMeshProUGUI>();
                        if (priceTextComponent != null)
                        {
                            priceTextComponent.text = itemPrice;
                            Debug.Log($"[AddItemToGiftStore] Updated price text to {itemPrice}");
                        }
                    }
                }
            }

            // Destroy ButtonInstructions component on Button child and add Unity Button
            Transform buttonChild = newItem.transform.Find("Button");
            if (buttonChild != null)
            {
                ButtonInstructions buttonInstructions = buttonChild.GetComponent<ButtonInstructions>();
                if (buttonInstructions != null)
                {
                    GameObject.DestroyImmediate(buttonInstructions);
                    Debug.Log($"[AddItemToGiftStore] Destroyed ButtonInstructions component on {itemName} > Button");
                }

                // Add Unity Button component
                UnityEngine.UI.Button button = buttonChild.gameObject.AddComponent<UnityEngine.UI.Button>();

                // Configure ColorBlock for highlighted color (white by default, light pink on hover)
                var colors = button.colors;
                colors.normalColor = new Color(1f, 1f, 1f, 1f); // White for default
                colors.highlightedColor = new Color(1f, 0.75f, 0.8f, 1f); // Light pink for hover
                colors.pressedColor = new Color(0.9f, 0.5f, 0.6f, 1f); // Darker pink for press
                colors.selectedColor = new Color(1f, 1f, 1f, 1f); // White
                colors.disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.5f); // Dark grey
                colors.colorMultiplier = 1f;
                colors.fadeDuration = 0.1f;
                button.colors = colors;

                // Set transition to ColorTint
                button.transition = UnityEngine.UI.Selectable.Transition.ColorTint;

                // Parse price (remove $ sign)
                string priceString = itemPrice.Replace("$", "").Trim();
                int price = 0;
                if (!int.TryParse(priceString, out price))
                {
                    Debug.LogWarning($"[AddItemToGiftStore] Failed to parse price '{itemPrice}' for item '{itemName}'");
                    price = 0;
                }

                // Create proxy variable name (replace spaces with dashes, add Gift_ prefix)
                string proxyVariableName = "Gift_" + itemName.Replace(" ", "-");

                // Load audio clips from otherbundle
                AudioClip buttonSound = Core.otherBundle.LoadAsset<AudioClip>("Button-1");
                AudioClip cashRegisterSound = Core.otherBundle.LoadAsset<AudioClip>("Cash Register");

                // Capture newItem reference for the lambda
                GameObject capturedItem = newItem;

                // Add onClick listener
                button.onClick.AddListener(() => {
                    Debug.Log("Click!");

                    // Play UI sound
                    if (buttonSound != null)
                    {
                        Singleton<AudioManager>.Instance.UserInterface.Play(buttonSound, AudioConfigSoundUI.Default, Args.EMPTY);
                    }

                    // Check if player has enough cash
                    double currentCash = Core.GetVariableNumber("Cash");
                    if (currentCash >= price)
                    {
                        // Decrement cash
                        Core.FindAndModifyVariableDouble("Cash", currentCash - price);
                        Debug.Log($"[GiftStore] Purchased {itemName} for ${price}. Remaining cash: {currentCash - price}");

                        // Play SFX
                        if (cashRegisterSound != null)
                        {
                            Singleton<AudioManager>.Instance.UserInterface.Play(cashRegisterSound, AudioConfigSoundUI.Default, Args.EMPTY);
                        }

                        // Scale parent to (0, 1, 1) - shrink horizontally
                        capturedItem.transform.localScale = new Vector3(0f, 1f, 1f);

                        // Set proxy variable to true
                        if (Core.proxyVariables != null && Core.proxyVariables.Exists(proxyVariableName))
                        {
                            Core.proxyVariables.Set(proxyVariableName, true);
                            Debug.Log($"[GiftStore] Set {proxyVariableName} to true");
                        }
                        else
                        {
                            Debug.LogWarning($"[GiftStore] Proxy variable '{proxyVariableName}' not found");
                        }

                        // Set parent inactive after scaling
                        capturedItem.SetActive(false);
                    }
                    else
                    {
                        Debug.Log($"[GiftStore] Not enough cash. Need ${price}, have ${currentCash}");
                    }
                });

                Debug.Log($"[AddItemToGiftStore] Configured button for {itemName} with price ${price}");
            }

            // Destroy Trigger component on the cloned item
            Trigger triggerComponent = newItem.GetComponent<Trigger>();
            if (triggerComponent != null)
            {
                GameObject.Destroy(triggerComponent);
                Debug.Log($"[AddItemToGiftStore] Destroyed Trigger component on {itemName}");
            }

            Debug.Log($"[AddItemToGiftStore] Successfully added item {itemName} to GiftStore");
            return newItem;
        }

        private static void UpdateGiftStoreItemVisibility()
        {
            if (Core.proxyVariables == null || giftStore == null) return;

            Transform giftStoreCore = giftStore.transform.Find("Core");
            if (giftStoreCore == null) return;

            bool layoutChanged = false;

            // Check each item in the gift store
            foreach (Transform item in giftStoreCore)
            {
                // Skip non-item objects (like CloseStore button)
                if (item.name == "CloseStore") continue;

                // Create proxy variable name from item name
                string proxyVariableName = "Gift_" + item.name.Replace(" ", "-");

                // Check if proxy variable exists and get its value
                if (Core.proxyVariables.Exists(proxyVariableName))
                {
                    bool isPurchased = (bool)Core.proxyVariables.Get(proxyVariableName);

                    // Set active to opposite of purchased state (if purchased, hide it)
                    if (item.gameObject.activeSelf == isPurchased)
                    {
                        item.gameObject.SetActive(!isPurchased);
                        layoutChanged = true;
                        Debug.Log($"[GiftStore] Set {item.name} active to {!isPurchased} (purchased: {isPurchased})");
                        
                        // Reset scale to (1,1,1) when item becomes visible
                        if (!isPurchased)
                        {
                            item.localScale = new Vector3(1f, 1f, 1f);
                            Debug.Log($"[GiftStore] Reset {item.name} scale to (1,1,1)");
                        }
                    }
                }
            }

            // Rebuild layout if any items changed visibility
            if (layoutChanged)
            {
                HorizontalLayoutGroup hlg = giftStoreCore.GetComponent<HorizontalLayoutGroup>();
                if (hlg != null)
                {
                    LayoutRebuilder.MarkLayoutForRebuild(giftStoreCore as RectTransform);
                }
                Debug.Log("[GiftStore] Marked layout for rebuild");
            }
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
