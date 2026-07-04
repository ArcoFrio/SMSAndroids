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
        public static GameObject harborHouseEntranceRoomtalk;
        public static GameObject harborHomeKitchenLevel;
        public static GameObject harborHomeKitchenLevelB1;
        public static GameObject harborHomeKitchenRoomtalk;
        public static GameObject harborHomeLivingroomLevel;
        public static GameObject harborHomeLivingroomLevelMovies;
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
        // Secret Beach Sky / Flash / Gatekeeper / Portal overlays migrated to the
        // pack (SMSAndroidsPack place "overlays" + Move/Spin dialogue actions).

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

        public static bool loadedPlaces = false;
        public static bool harborHomeBedroomSwapApplied = false;
        public static bool insideHarborHome = false;
        public static GameObject solid;
        public void Update()
        {
            if (Core.currentScene.name == "CoreGameScene")
            {
                if (!loadedPlaces && Core.loadedCore)
                {
                    // GATE FIRST, side effects after. This block re-runs
                    // every frame until it completes, so anything that
                    // Instantiates/clones before this check leaks one copy
                    // per retry frame.
                    if (!TryResolvePackBuiltPlaces())
                    {
                        return;
                    }


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

                    // The 31 CreateNewPlace calls that used to live here are
                    // gone. The matching levels / map buttons / roomtalks
                    // are owned by SMSModForge.PackPlugin — same key set,
                    // same parallax / audio / sprite layout. References to
                    // the pack-built GameObjects were already resolved by
                    // TryResolvePackBuiltPlaces at the top of this block.

                    // The pack creates the roomtalk children of the bedroom
                    // level; SMSAndroids grafts the PlayerRoom_ButtonCanvas
                    // (sleep / calendar / PC buttons) onto it so the player
                    // can still interact with their bedroom from the HH
                    // bedroom view.
                    harborHomeBedroomButtonCanvas = GameObject.Instantiate(Core.level.Find("5_MyRoom").Find("PlayerRoom_ButtonCanvas").gameObject, harborHomeBedroomLevel.transform);
                    harborHomeBedroomButtonCanvas.transform.Find("Player_Room_Buttons").gameObject.GetComponent<ParallaxMouseEffect>().parallaxStrength = 0.05f;
                    harborHomeBedroomButtonCanvas.transform.Find("Player_Room_Buttons").Find("SleepButton").localPosition = new Vector2(0, -140);
                    harborHomeBedroomButtonCanvas.transform.Find("Player_Room_Buttons").Find("CalendarButton").localPosition = new Vector2(-20, 250);
                    harborHomeBedroomButtonCanvas.transform.Find("Player_Room_Buttons").Find("PCButton").localPosition = new Vector2(-720, -100);
                    // Active from creation — visibility follows the bedroom
                    // level via the hierarchy. (The old activation lived in
                    // the bedroom map-button click handler, which is gone
                    // now that ModForge owns navigation.)
                    harborHomeBedroomButtonCanvas.SetActive(true);

                    // The Harbor Home Entrance radial button — including its
                    // "House for Sale" -> "Home" rename once the house is
                    // bought — is fully pack-side now: the label is
                    // [PV:HarborHome_ButtonLabel] (live-resolved by
                    // RadialButtonRuntime) and an Integration rule writes
                    // the variable on the rising edge of HarborHome_Bought.

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

                    // Secret Beach Sky / Flash / Gatekeeper / Portal overlays are
                    // now authored as pack data (SMSAndroidsPack place "overlays")
                    // and animated by the GK dialogue's MoveGameObject /
                    // SpinGameObject / FadeSprite actions — no longer built here.

                    // Navigator grid + extended second-row background are
                    // owned by ModForge (NavigatorGridSetup / NavigatorGridLayout)
                    // along with the map buttons themselves.

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

                    // The B1 overlays (shower glass + fridge-open) are
                    // SMSAndroids-side art layered on top of the pack-built
                    // HH bathroom + kitchen levels. We clone the secondary-
                    // sprite child (child index 1 — the same secondary the
                    // PlaceFactory cloned from the Beach prototype) so the
                    // overlay carries the right shader / material.
                    harborHomeBathroomLevelB1 = GameObject.Instantiate(harborHomeBathroomLevel.transform.GetChild(1).gameObject, harborHomeBathroomLevel.transform);
                    harborHomeBathroomLevelB1.name = "ShowerGlassOverlay";
                    harborHomeBathroomLevelB1.GetComponent<SpriteRenderer>().sortingOrder = 0;
                    Destroy(harborHomeBathroomLevelB1.GetComponent<ParallaxMouseEffect>());
                    SetNewLevelSprite(harborHomeBathroomLevelB1, Core.locationPath, "HHomeBathroomB1.PNG", 2048, 1136);
                    harborHomeKitchenLevelB1 = GameObject.Instantiate(harborHomeKitchenLevel.transform.GetChild(1).gameObject, harborHomeKitchenLevel.transform);
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
                    harborHomeLivingroomLevelMovies = GameObject.Instantiate(Core.level.Find("3_LivingRoom").Find("Movies").gameObject, harborHomeLivingroomLevel.transform);
                    harborHomePoolNPCTanningleft = GameObject.Instantiate(new GameObject(), harborHomePoolLevel.transform.Find("NPCs")).transform; harborHomePoolNPCTanningleft.name = "Tanningleft";
                    harborHomePoolNPCTanningright = GameObject.Instantiate(new GameObject(), harborHomePoolLevel.transform.Find("NPCs")).transform; harborHomePoolNPCTanningright.name = "Tanningright";

                    // Subscribe to gift-UI signals here too, in case Places
                    // finishes before Dialogues (load order isn't strict
                    // between them). EnsureSubscribed is idempotent so both
                    // call sites can stay.
                    GiftUIBridge.EnsureSubscribed();

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

                harborHomeKitchenLevelB1.SetActive(Schedule.anisLocation == "HarborHomeKitchenFridge");

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


                // (Gatekeeper spin moved to the pack's SpinGameObject action.)
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

        /// <summary>
        /// Bind every <c>secretBeach*</c> / <c>mountainLab*</c> /
        /// <c>giftShop*</c> / <c>harborHome*</c> / <c>harborHouse*</c>
        /// static field to the matching GameObject built by
        /// <c>SMSModForge.PackPlugin.PlaceFactory</c>. Pack-built levels are named <c>&lt;absoluteIndex&gt;_&lt;key&gt;</c>
        /// where the index is auto-assigned at build time (so we can't
        /// hard-code it the way the original SMSAndroids code did); we
        /// suffix-match on <c>_&lt;key&gt;</c> instead. Pack roomtalks are
        /// named bare (just <c>&lt;key&gt;</c>) so we keep the original
        /// exact-name <see cref="Transform.Find"/>.
        /// <para/>
        /// Returns <c>false</c> when one or more of the expected pack GOs
        /// isn't present yet — typically because ModForge hasn't reached
        /// its <c>LoadAllPacks</c> step yet. The caller treats this as
        /// "retry next frame" by returning from the init guard.
        /// </summary>
        private bool TryResolvePackBuiltPlaces()
        {
            var levelRoot = Core.level;
            var roomTalkRoot = Core.roomTalk;
            if (levelRoot == null || roomTalkRoot == null) return false;

            // Probe: if SecretBeach isn't built yet, the pack hasn't run.
            // (Picked as a probe because it's the first place SMSAndroids
            // ever created, and the SMSAndroidsPack ships it as places[0].)
            if (FindChildBySuffix(levelRoot, "_SecretBeach") == null) return false;

            // SecretBeach
            secretBeachLevel        = FindChildBySuffix(levelRoot, "_SecretBeach");
            secretBeachLevelBG      = secretBeachLevel?.transform.childCount > 1
                                          ? secretBeachLevel.transform.GetChild(1).gameObject : null;
            secretBeachRoomtalk     = roomTalkRoot.Find("SecretBeach")?.gameObject;

            // MountainLab + 2 corridors
            mountainLabLevel        = FindChildBySuffix(levelRoot, "_MountainLab");
            mountainLabRoomtalk     = roomTalkRoot.Find("MountainLab")?.gameObject;
            mountainLabCorridorNikke1Level    = FindChildBySuffix(levelRoot, "_MountainLabCorridorNikke1");
            mountainLabCorridorNikke1Roomtalk = roomTalkRoot.Find("MountainLabCorridorNikke1")?.gameObject;
            mountainLabCorridorNikke2Level    = FindChildBySuffix(levelRoot, "_MountainLabCorridorNikke2");
            mountainLabCorridorNikke2Roomtalk = roomTalkRoot.Find("MountainLabCorridorNikke2")?.gameObject;

            // 18 Nikke rooms. Same pattern; one helper line per girl.
            BindMountainLabRoom("Anis",    out mountainLabRoomNikkeAnisLevel,    out mountainLabRoomNikkeAnisRoomtalk,    levelRoot, roomTalkRoot);
            BindMountainLabRoom("Centi",   out mountainLabRoomNikkeCentiLevel,   out mountainLabRoomNikkeCentiRoomtalk,   levelRoot, roomTalkRoot);
            BindMountainLabRoom("Dorothy", out mountainLabRoomNikkeDorothyLevel, out mountainLabRoomNikkeDorothyRoomtalk, levelRoot, roomTalkRoot);
            BindMountainLabRoom("Elegg",   out mountainLabRoomNikkeEleggLevel,   out mountainLabRoomNikkeEleggRoomtalk,   levelRoot, roomTalkRoot);
            BindMountainLabRoom("Frima",   out mountainLabRoomNikkeFrimaLevel,   out mountainLabRoomNikkeFrimaRoomtalk,   levelRoot, roomTalkRoot);
            BindMountainLabRoom("Guilty",  out mountainLabRoomNikkeGuiltyLevel,  out mountainLabRoomNikkeGuiltyRoomtalk,  levelRoot, roomTalkRoot);
            BindMountainLabRoom("Helm",    out mountainLabRoomNikkeHelmLevel,    out mountainLabRoomNikkeHelmRoomtalk,    levelRoot, roomTalkRoot);
            BindMountainLabRoom("Maiden",  out mountainLabRoomNikkeMaidenLevel,  out mountainLabRoomNikkeMaidenRoomtalk,  levelRoot, roomTalkRoot);
            BindMountainLabRoom("Mary",    out mountainLabRoomNikkeMaryLevel,    out mountainLabRoomNikkeMaryRoomtalk,    levelRoot, roomTalkRoot);
            BindMountainLabRoom("Mast",    out mountainLabRoomNikkeMastLevel,    out mountainLabRoomNikkeMastRoomtalk,    levelRoot, roomTalkRoot);
            BindMountainLabRoom("Neon",    out mountainLabRoomNikkeNeonLevel,    out mountainLabRoomNikkeNeonRoomtalk,    levelRoot, roomTalkRoot);
            BindMountainLabRoom("Pepper",  out mountainLabRoomNikkePepperLevel,  out mountainLabRoomNikkePepperRoomtalk,  levelRoot, roomTalkRoot);
            BindMountainLabRoom("Rapi",    out mountainLabRoomNikkeRapiLevel,    out mountainLabRoomNikkeRapiRoomtalk,    levelRoot, roomTalkRoot);
            BindMountainLabRoom("Rosanna", out mountainLabRoomNikkeRosannaLevel, out mountainLabRoomNikkeRosannaRoomtalk, levelRoot, roomTalkRoot);
            BindMountainLabRoom("Sakura",  out mountainLabRoomNikkeSakuraLevel,  out mountainLabRoomNikkeSakuraRoomtalk,  levelRoot, roomTalkRoot);
            BindMountainLabRoom("Tove",    out mountainLabRoomNikkeToveLevel,    out mountainLabRoomNikkeToveRoomtalk,    levelRoot, roomTalkRoot);
            BindMountainLabRoom("Viper",   out mountainLabRoomNikkeViperLevel,   out mountainLabRoomNikkeViperRoomtalk,   levelRoot, roomTalkRoot);
            BindMountainLabRoom("Yan",     out mountainLabRoomNikkeYanLevel,     out mountainLabRoomNikkeYanRoomtalk,     levelRoot, roomTalkRoot);

            // GiftShop pair
            giftShopLevel           = FindChildBySuffix(levelRoot, "_GiftShop");
            giftShopRoomtalk        = roomTalkRoot.Find("GiftShop")?.gameObject;
            giftShopInteriorLevel   = FindChildBySuffix(levelRoot, "_GiftShopInterior");
            giftShopInteriorRoomtalk = roomTalkRoot.Find("GiftShopInterior")?.gameObject;

            // Harbor Home (six rooms + entrance)
            harborHomeLivingroomLevel  = FindChildBySuffix(levelRoot, "_HarborHomeLivingRoom");
            harborHomeLivingroomRoomtalk = roomTalkRoot.Find("HarborHomeLivingRoom")?.gameObject;
            harborHomeBedroomLevel     = FindChildBySuffix(levelRoot, "_HarborHomeBedroom");
            harborHomeBedroomRoomtalk  = roomTalkRoot.Find("HarborHomeBedroom")?.gameObject;
            harborHomeBathroomLevel    = FindChildBySuffix(levelRoot, "_HarborHomeBathroom");
            harborHomeBathroomRoomtalk = roomTalkRoot.Find("HarborHomeBathroom")?.gameObject;
            harborHomeClosetLevel      = FindChildBySuffix(levelRoot, "_HarborHomeCloset");
            harborHomeClosetRoomtalk   = roomTalkRoot.Find("HarborHomeCloset")?.gameObject;
            harborHomeKitchenLevel     = FindChildBySuffix(levelRoot, "_HarborHomeKitchen");
            harborHomeKitchenRoomtalk  = roomTalkRoot.Find("HarborHomeKitchen")?.gameObject;
            harborHomePoolLevel        = FindChildBySuffix(levelRoot, "_HarborHomePool");
            harborHomePoolRoomtalk     = roomTalkRoot.Find("HarborHomePool")?.gameObject;
            harborHouseEntranceLevel   = FindChildBySuffix(levelRoot, "_HarborHouseEntrance");
            harborHouseEntranceRoomtalk = roomTalkRoot.Find("HarborHouseEntrance")?.gameObject;

            return true;
        }

        /// <summary>
        /// Per-Nikke binder for the 18 MountainLabRoomNikke<i>X</i> entries.
        /// Folded out so <see cref="TryResolvePackBuiltPlaces"/> stays
        /// readable — same body for every girl, only the key changes.
        /// </summary>
        private static void BindMountainLabRoom(string charName,
            out GameObject level, out GameObject roomtalk,
            Transform levelRoot, Transform roomTalkRoot)
        {
            string suffix = "_MountainLabRoomNikke" + charName;
            level    = FindChildBySuffix(levelRoot, suffix);
            roomtalk = roomTalkRoot.Find("MountainLabRoomNikke" + charName)?.gameObject;
        }

        /// <summary>Find the first direct child of <paramref name="parent"/>
        /// whose name ends with <paramref name="suffix"/>; returns null when
        /// no such child exists. Used by the pack-place resolver since the
        /// pack-built GO names carry an unpredictable index prefix and we
        /// can only match the trailing key portion.</summary>
        private static GameObject FindChildBySuffix(Transform parent, string suffix)
        {
            if (parent == null || string.IsNullOrEmpty(suffix)) return null;
            foreach (Transform t in parent)
            {
                if (t.name.EndsWith(suffix, System.StringComparison.Ordinal))
                    return t.gameObject;
            }
            return null;
        }

        // CreateNewPlace + CreateNewLevel are gone — ModForge's
        // SMSModForge.PackPlugin.PlaceFactory owns the loose-PNG-to-level
        // pipeline now. CreateNewRoomTalk stays because Hospital / Hotel /
        // Trail still get a SMSAndroids-side extra roomtalk node grafted
        // under Core.roomTalk for legacy event dialogues that target them.
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
