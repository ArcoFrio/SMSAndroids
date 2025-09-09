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
    [BepInPlugin(pluginGuid, Core.pluginName + " - Schedule", Core.pluginVersion)]
    internal class Schedule : BaseUnityPlugin
    {
        #region Plugin Info
        public const string pluginGuid = "treboy.starmakerstory.smsandroidscore.schedule";
        #endregion
        
        // Character location variables - default to their rooms
        public static string amberLocation;

        public static string anisLocation;
        public static string centiLocation;
        public static string dorothyLocation;
        public static string eleggLocation;
        public static string frimaLocation;
        public static string guiltyLocation;
        public static string helmLocation;
        public static string maidenLocation;
        public static string maryLocation;
        public static string mastLocation;
        public static string neonLocation;
        public static string pepperLocation;
        public static string rapiLocation;
        public static string rosannaLocation;
        public static string sakuraLocation;
        public static string viperLocation;
        public static string yanLocation;

        public static string snekLocation;

        public static double day;
        public static bool loadedSchedule = false;

        public void Update()
        {
            if (Core.currentScene.name == "CoreGameScene")
            {
                if (!loadedSchedule && Core.loadedCore)
                {
                    InitializeCharacterLocations();
                    Logger.LogInfo("----- SCHEDULE LOADED -----");
                    loadedSchedule = true;
                }
            }
            if (Core.currentScene.name == "GameStart")
            {
                if (loadedSchedule)
                {
                    Logger.LogInfo("----- SCHEDULE UNLOADED -----");
                    loadedSchedule = false;
                }
            }
        }

        private void InitializeCharacterLocations()
        {
            // Set default locations to their rooms
            amberLocation = "MountainLab"; // Amber's default room

            anisLocation = "MountainLabRoomNikkeAnis";
            centiLocation = "MountainLabRoomNikkeCenti";
            dorothyLocation = "MountainLabRoomNikkeDorothy";
            eleggLocation = "MountainLabRoomNikkeElegg";
            frimaLocation = "MountainLabRoomNikkeFrima";
            guiltyLocation = "MountainLabRoomNikkeGuilty";
            helmLocation = "MountainLabRoomNikkeHelm";
            maidenLocation = "MountainLabRoomNikkeMaiden";
            maryLocation = "MountainLabRoomNikkeMary";
            mastLocation = "MountainLabRoomNikkeMast";
            neonLocation = "MountainLabRoomNikkeNeon";
            pepperLocation = "MountainLabRoomNikkePepper";
            rapiLocation = "MountainLabRoomNikkeRapi";
            rosannaLocation = "MountainLabRoomNikkeRosanna";
            sakuraLocation = "MountainLabRoomNikkeSakura";
            viperLocation = "MountainLabRoomNikkeViper";
            yanLocation = "MountainLabRoomNikkeYan";

            snekLocation = "Unknown";
        }

        public static void UpdateScheduleForDay()
        {
            day = Core.GetVariableNumber("Day");
            Debug.Log("Day: " + day);
            switch (day)
            {
                case 1:
                    SetDay1Schedule();
                    break;
                case 2:
                    SetDay2Schedule();
                    break;
                case 3:
                    SetDay3Schedule();
                    break;
                case 4:
                    SetDay4Schedule();
                    break;
                case 5:
                    SetDay5Schedule();
                    break;
                default:
                    // Default to day 1 schedule for any other day value
                    SetDay1Schedule();
                    break;
            }
            
            Debug.Log($"Schedule updated for Day {day}");
        }

        private static void SetDay1Schedule()
        {
            // All characters in their rooms
            amberLocation = (MainStory.generalLotteryNumber1 <= 50) ? "Hospitalhallway" : "MountainLab";
            anisLocation = "MountainLabRoomNikkeAnis";
            centiLocation = "MountainLabRoomNikkeCenti";
            dorothyLocation = "MountainLabRoomNikkeDorothy";
            eleggLocation = "MountainLabRoomNikkeElegg";
            frimaLocation = (MainStory.generalLotteryNumber2 <= 50) ? "Hotel" : "MountainLabRoomNikkeFrima";
            guiltyLocation = "MountainLabRoomNikkeGuilty";
            helmLocation = "MountainLabRoomNikkeHelm";
            maidenLocation = "MountainLabRoomNikkeMaiden";
            maryLocation = "MountainLabRoomNikkeMary";
            mastLocation = "MountainLabRoomNikkeMast";
            neonLocation = (MainStory.generalLotteryNumber3 <= 50 && !Places.GetBadWeather()) ? "Temple" : "MountainLabRoomNikkeNeon";
            pepperLocation = "MountainLabRoomNikkePepper";
            rapiLocation = "MountainLabRoomNikkeRapi";
            rosannaLocation = "MountainLabRoomNikkeRosanna";
            sakuraLocation = "MountainLabRoomNikkeSakura";
            viperLocation = (MainStory.generalLotteryNumber1 <= 50 && !Places.GetBadWeather()) ? "Villa" : "MountainLabRoomNikkeViper";
            yanLocation = "MountainLabRoomNikkeYan";

            snekLocation = "Unknown";
        }

        private static void SetDay2Schedule()
        {
            // All characters in their rooms
            amberLocation = "MountainLab";
            anisLocation = (MainStory.generalLotteryNumber3 <= 50) ? "Mall" : "MountainLabRoomNikkeAnis";
            centiLocation = "MountainLabRoomNikkeCenti";
            dorothyLocation = "MountainLabRoomNikkeDorothy";
            eleggLocation = "MountainLabRoomNikkeElegg";
            frimaLocation = "MountainLabRoomNikkeFrima";
            guiltyLocation = (MainStory.generalLotteryNumber1 <= 50 && !Places.GetBadWeather()) ? "Parkinglot" : "MountainLabRoomNikkeGuilty";
            helmLocation = "MountainLabRoomNikkeHelm";
            maidenLocation = "MountainLabRoomNikkeMaiden";
            maryLocation = "MountainLabRoomNikkeMary";
            mastLocation = "MountainLabRoomNikkeMast";
            neonLocation = "MountainLabRoomNikkeNeon";
            pepperLocation = (MainStory.generalLotteryNumber2 <= 50) ? "Hospital" : "MountainLabRoomNikkePepper";
            rapiLocation = "MountainLabRoomNikkeRapi";
            rosannaLocation = "MountainLabRoomNikkeRosanna";
            sakuraLocation = "MountainLabRoomNikkeSakura";
            viperLocation = "MountainLabRoomNikkeViper";
            yanLocation = "MountainLabRoomNikkeYan";

            snekLocation = "Unknown";
        }

        private static void SetDay3Schedule()
        {
            // All characters in their rooms
            amberLocation = "MountainLab";
            anisLocation = "MountainLabRoomNikkeAnis";
            centiLocation = (MainStory.generalLotteryNumber1 <= 50 && !Places.GetBadWeather() && Core.GetVariableBool("samantha-met")) ? "Kenshome" : "MountainLabRoomNikkeCenti";
            dorothyLocation = "MountainLabRoomNikkeDorothy";
            eleggLocation = "MountainLabRoomNikkeElegg";
            frimaLocation = "MountainLabRoomNikkeFrima";
            guiltyLocation = "MountainLabRoomNikkeGuilty";
            helmLocation = "MountainLabRoomNikkeHelm";
            maidenLocation = "MountainLabRoomNikkeMaiden";
            maryLocation = (MainStory.generalLotteryNumber1 <= 50) ? "Hospitalhallway" : "MountainLabRoomNikkeMary";
            mastLocation = (MainStory.generalLotteryNumber2 <= 50 && !Places.GetBadWeather()) ? "Beach" : "MountainLabRoomNikkeMast";
            neonLocation = "MountainLabRoomNikkeNeon";
            pepperLocation = "MountainLabRoomNikkePepper";
            rapiLocation = (MainStory.generalLotteryNumber3 <= 50 && !Places.GetBadWeather()) ? "Gasstation" : "MountainLabRoomNikkeRapi";
            rosannaLocation = "MountainLabRoomNikkeRosanna";
            sakuraLocation = "MountainLabRoomNikkeSakura";
            viperLocation = "MountainLabRoomNikkeViper";
            yanLocation = "MountainLabRoomNikkeYan";

            snekLocation = (MainStory.generalLotteryNumber3 == 69) ? "Forest" : "Unknown";
        }

        private static void SetDay4Schedule()
        {
            // All characters in their rooms
            amberLocation = "MountainLab";
            anisLocation = "MountainLabRoomNikkeAnis";
            centiLocation = "MountainLabRoomNikkeCenti";
            dorothyLocation = (MainStory.generalLotteryNumber2 <= 50 && !Places.GetBadWeather()) ? "Park" : "MountainLabRoomNikkeDorothy";
            eleggLocation = "MountainLabRoomNikkeElegg";
            frimaLocation = "MountainLabRoomNikkeFrima";
            guiltyLocation = "MountainLabRoomNikkeGuilty";
            helmLocation = (MainStory.generalLotteryNumber1 <= 50 && !Places.GetBadWeather()) ? "Beach" : "MountainLabRoomNikkeHelm";
            maidenLocation = "MountainLabRoomNikkeMaiden";
            maryLocation = "MountainLabRoomNikkeMary";
            mastLocation = "MountainLabRoomNikkeMast";
            neonLocation = "MountainLabRoomNikkeNeon";
            pepperLocation = "MountainLabRoomNikkePepper";
            rapiLocation = "MountainLabRoomNikkeRapi";
            rosannaLocation = (MainStory.generalLotteryNumber3 <= 50 && !Places.GetBadWeather()) ? "Gabrielsmansion" : "MountainLabRoomNikkeRosanna";
            sakuraLocation = "MountainLabRoomNikkeSakura";
            viperLocation = "MountainLabRoomNikkeViper";
            yanLocation = (MainStory.generalLotteryNumber1 <= 50) ? "Mall" : "MountainLabRoomNikkeYan";

            snekLocation = "Unknown";
        }

        private static void SetDay5Schedule()
        {
            // All characters in their rooms
            amberLocation = "MountainLab";
            anisLocation = "MountainLabRoomNikkeAnis";
            centiLocation = "MountainLabRoomNikkeCenti";
            dorothyLocation = "MountainLabRoomNikkeDorothy";
            eleggLocation = (MainStory.generalLotteryNumber2 <= 50 && !Places.GetBadWeather()) ? "Downtown" : "MountainLabRoomNikkeElegg";
            frimaLocation = "MountainLabRoomNikkeFrima";
            guiltyLocation = "MountainLabRoomNikkeGuilty";
            helmLocation = "MountainLabRoomNikkeHelm";
            maidenLocation = (MainStory.generalLotteryNumber1 <= 50 && !Places.GetBadWeather()) ? "Alley" : "MountainLabRoomNikkeMaiden";
            maryLocation = "MountainLabRoomNikkeMary";
            mastLocation = "MountainLabRoomNikkeMast";
            neonLocation = "MountainLabRoomNikkeNeon";
            pepperLocation = "MountainLabRoomNikkePepper";
            rapiLocation = "MountainLabRoomNikkeRapi";
            rosannaLocation = "MountainLabRoomNikkeRosanna";
            sakuraLocation = (MainStory.generalLotteryNumber3 <= 50 && !Places.GetBadWeather()) ? "Forest" : "MountainLabRoomNikkeSakura";
            viperLocation = "MountainLabRoomNikkeViper";
            yanLocation = "MountainLabRoomNikkeYan";

            snekLocation = "Unknown";
        }

        // Public method to get a character's current location
        public static string GetCharacterLocation(string characterName)
        {
            switch (characterName.ToLower())
            {
                case "amber": return amberLocation;
                case "anis": return anisLocation;
                case "centi": return anisLocation;
                case "dorothy": return anisLocation;
                case "elegg": return anisLocation;
                case "frima": return frimaLocation;
                case "guilty": return guiltyLocation;
                case "helm": return helmLocation;
                case "maiden": return maidenLocation;
                case "mary": return maryLocation;
                case "mast": return mastLocation;
                case "neon": return neonLocation;
                case "pepper": return pepperLocation;
                case "rapi": return rapiLocation;
                case "rosanna": return rosannaLocation;
                case "sakura": return sakuraLocation;
                case "viper": return viperLocation;
                case "yan": return yanLocation;
                default: return null;
            }
        }

        // Public method to manually set a character's location
        public static void SetCharacterLocation(string characterName, string location)
        {
            switch (characterName.ToLower())
            {
                case "amber": amberLocation = location; break;
                case "anis": anisLocation = location; break;
                case "centi": anisLocation = location; break;
                case "dorothy": anisLocation = location; break;
                case "elegg": anisLocation = location; break;
                case "frima": frimaLocation = location; break;
                case "guilty": guiltyLocation = location; break;
                case "helm": helmLocation = location; break;
                case "maiden": maidenLocation = location; break;
                case "mary": maryLocation = location; break;
                case "mast": mastLocation = location; break;
                case "neon": neonLocation = location; break;
                case "pepper": pepperLocation = location; break;
                case "rapi": rapiLocation = location; break;
                case "rosanna": rosannaLocation = location; break;
                case "sakura": sakuraLocation = location; break;
                case "viper": viperLocation = location; break;
                case "yan": yanLocation = location; break;
            }
        }

        private IEnumerator UpdateScheduleWithDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            UpdateScheduleForDay();
        }
    }
}
