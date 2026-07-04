using BepInEx;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameCreator;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.Common.Audio;
using GameCreator.Runtime.Common.UnityUI;
using GameCreator.Runtime.Dialogue;
using GameCreator.Runtime.Dialogue.UnityUI;
using GameCreator.Runtime.Variables;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;

namespace SMSAndroidsCore
{
    [BepInPlugin(pluginGuid, Core.pluginName, Core.pluginVersion)]
    internal class Minigames : BaseUnityPlugin
    {
        #region Plugin Info
        public const string pluginGuid = "treboy.starmakerstory.smsandroidscore.minigames";
        #endregion
        public static Minigames Instance;
        public static bool loadedMinigame;
        public static bool minigameJustEnded;
        public static string minigameWordScore;

        // The unified massage minigame prefab (single asset bundle entry: "MinigameMassage").
        // Hierarchy is fixed in the prefab — see MassageMinigame_Hierarchy.txt.
        public static GameObject minigameMassage;

        public void Awake()
        {
            Instance = this;
        }

        public void Update()
        {
            // Wallpaper + Scenes gates dropped — both migrated to ModForge.
            if (Core.loadedCore && Characters.loadedBusts && Dialogues.loadedDialogues && MainStory.loadedStory && Places.loadedPlaces && Schedule.loadedSchedule)
            {
                if (!loadedMinigame)
                {
                    GameObject prefab = Core.minigameBundle != null
                        ? Core.minigameBundle.LoadAsset<GameObject>("MinigameMassage")
                        : null;

                    if (prefab == null)
                    {
                        Debug.LogError("[Minigames] Could not load 'MinigameMassage' from minigamebundle. " +
                                       "Make sure the unified prefab is named 'MinigameMassage' in the bundle.");
                        return;
                    }

                    minigameMassage = GameObject.Instantiate(prefab);
                    minigameMassage.name = "MinigameMassage";
                    minigameMassage.SetActive(false);

                    // World-space canvas needs the active camera reference.
                    Transform canvasT = minigameMassage.transform.Find("CanvasMassage");
                    if (canvasT != null)
                    {
                        Canvas canvas = canvasT.GetComponent<Canvas>();
                        if (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
                            canvas.worldCamera = Core.mainCamera.GetComponent<Camera>();
                    }

                    if (minigameMassage.GetComponent<MassageMinigame>() == null)
                        minigameMassage.AddComponent<MassageMinigame>();

                    loadedMinigame = true;
                    Logger.LogInfo("----- MINIGAMES LOADED -----");
                }
            }
        }

        /// <summary>
        /// Public entry used by dialogue/story to launch a minigame with a fade transition.
        /// The character variant shown is driven by the proxy variable
        /// <c>Minigame_Massage_Character</c> (set before calling).
        /// </summary>
        public void StartMinigame(GameObject minigameObject)
        {
            StartCoroutine(StartMinigameCoroutine(minigameObject));
        }

        private IEnumerator StartMinigameCoroutine(GameObject minigameObject)
        {
            Signals.Emit(MainStory.fadeInBlackSignal);
            Signals.Emit(MainStory.fadeUISignal);
            yield return new WaitForSeconds(1f);
            Core.level.gameObject.SetActive(false);
            minigameObject.SetActive(true);
            Signals.Emit(MainStory.fadeOutBlackSignal);
        }

        public void StopMinigame(GameObject minigameObject, string wordScore = null)
        {
            StartCoroutine(StopMinigameCoroutine(minigameObject, wordScore));
        }

        private IEnumerator StopMinigameCoroutine(GameObject minigameObject, string wordScore)
        {
            Signals.Emit(MainStory.fadeInBlackSignal);
            yield return new WaitForSeconds(1f);
            Core.level.gameObject.SetActive(true);
            Signals.Emit(MainStory.fadeUISignal);
            minigameObject.SetActive(false);
            minigameWordScore = wordScore;
            minigameJustEnded = true;
            Signals.Emit(MainStory.fadeOutBlackSignal);
            yield return new WaitForSeconds(1f);
            minigameJustEnded = false;
        }
    }
}
