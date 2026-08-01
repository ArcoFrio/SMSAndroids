using BepInEx;
using GameCreator.Runtime.Common;
using System.Collections.Generic;
using UnityEngine;

namespace SMSAndroidsCore
{
    /// <summary>
    /// Story-level coordination state for SMSAndroids. Previously this class
    /// also drove every scheduled / event / voyeur dialogue via a 2300-line
    /// switch on each character's <see cref="Schedule"/> location; that logic
    /// has been wholesale migrated into ModForge (each dialogue is now a pack
    /// def in <c>SMSAndroidsPack/modpack.json</c> with its own conditions +
    /// actions). The remaining surface here is one thing:
    /// <list type="bullet">
    ///   <item>Shared GC2 <see cref="SignalArgs"/> constants — referenced by
    ///   the surviving subsystems (<see cref="Minigames"/>, <see cref="Dialogues"/>)
    ///   that still emit fades directly rather than going through a pack
    ///   action.</item>
    /// </list>
    /// All the dialogue runners, scene activations, bust swaps and on-line
    /// SFX hooks live in the ModForge plugin now (<c>SMSModForge.PackPlugin</c>).
    /// </summary>
    [BepInPlugin(pluginGuid, Core.pluginName + " - Story", Core.pluginVersion)]
    internal class MainStory : BaseUnityPlugin
    {
        #region Plugin Info
        public const string pluginGuid = "treboy.starmakerstory.smsandroidscore.mainstory";
        #endregion

        public static bool loadedStory = false;

        // ── Shared GC2 signal constants ───────────────────────────────────
        // Referenced by Minigames/Dialogues/Places. Kept here (not in Core)
        // for backward compatibility with every existing emit site.
        public static SignalArgs blinkSignal                  = new SignalArgs(new PropertyName("Blink"), null);
        public static SignalArgs dialogueEndSignal            = new SignalArgs(new PropertyName("DialogueEnd"), null);
        public static SignalArgs dialogueStartSignal          = new SignalArgs(new PropertyName("DialogueStart"), null);
        public static SignalArgs drinkSignal                  = new SignalArgs(new PropertyName("drink"), null);
        public static SignalArgs fadeUISignal                 = new SignalArgs(new PropertyName("FadeUI"), null);
        public static SignalArgs fadeInSignal                 = new SignalArgs(new PropertyName("FadeIn2025"), null);
        public static SignalArgs fadeInBlackSignal            = new SignalArgs(new PropertyName("FadeInBlack"), null);
        public static SignalArgs fadeOutBlackSignal           = new SignalArgs(new PropertyName("FadeOutBlack"), null);
        public static SignalArgs fadeOutSignal                = new SignalArgs(new PropertyName("FadeOut2025"), null);
        public static SignalArgs flashSignal                  = new SignalArgs(new PropertyName("flash"), null);
        public static SignalArgs forceEnableUISignal          = new SignalArgs(new PropertyName("ForceEnableUI"), null);
        public static SignalArgs kissSignal                   = new SignalArgs(new PropertyName("kiss"), null);
        public static SignalArgs whiteFlashNoSoundBlackSignal = new SignalArgs(new PropertyName("whiteflashnosound"), null);

        // NOTE: the daily lottery numbers that used to be re-rolled here are
        // gone with the native day schedules — the pack's ScheduleDaily rules
        // roll their own DailyChance. The voyeur system (tier progression,
        // eligible-target list, lottery + random pick) also lives ENTIRELY in
        // the pack — a List variable maintained with AddToList/RemoveFromList,
        // a DailyRandom lottery variable, and the Variable action's "Random
        // from list" operation. Nothing of either remains on the plugin side.
        public void Update()
        {
            if (Core.currentScene.name == "CoreGameScene")
            {
                if (!loadedStory && Places.loadedPlaces && Characters.loadedBusts && Dialogues.loadedDialogues)
                {
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
