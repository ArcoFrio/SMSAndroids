using BepInEx;
using UnityEngine;

namespace SMSAndroidsCore
{
    /// <summary>
    /// Load-order latch only. The character schedule this plugin used to
    /// simulate lives entirely in the pack now:
    /// <list type="bullet">
    ///   <item>Baseline "found → in their room" placement: the pack's
    ///   <c>Schedule_&lt;Char&gt;_Found</c> integration rules.</item>
    ///   <item>Per-day outings (lottery + weather + one-time event guards):
    ///   the <c>ScheduleDaily_&lt;Char&gt;_&lt;Day&gt;</c> rules.</item>
    ///   <item>Harbor Home roaming: the <c>HHRoam</c> / <c>HHRelease</c>
    ///   rules, parameterized over <c>HarborHome_VisitList</c>.</item>
    ///   <item>Anis's per-slot NPC visibility: <c>activeConditions</c> on the
    ///   pose-variant containers of the HarborHome places.</item>
    ///   <item>Shower audio + fridge overlay: the <c>HHShowerAudio</c> /
    ///   <c>HHFridgeOverlay</c> rules.</item>
    /// </list>
    /// All of it reads/writes the pack's <c>Location_&lt;Char&gt;</c>
    /// variables — the single source of truth <see cref="ScheduleVisualizer"/>
    /// and the HHTalk panel already consume.
    /// <para/>
    /// The plugin survives because <see cref="loadedSchedule"/> is a load gate
    /// for <see cref="Minigames"/>, <see cref="ScheduleVisualizer"/> and
    /// <see cref="SaveManager"/>'s post-sleep turnover; it latches on the same
    /// condition the old initializer used, so downstream timing is unchanged.
    /// </summary>
    [BepInPlugin(pluginGuid, Core.pluginName + " - Schedule", Core.pluginVersion)]
    internal class Schedule : BaseUnityPlugin
    {
        #region Plugin Info
        public const string pluginGuid = "treboy.starmakerstory.smsandroidscore.schedule";
        #endregion

        public static bool loadedSchedule = false;

        public void Update()
        {
            if (Core.currentScene.name == "CoreGameScene")
            {
                if (!loadedSchedule && Core.loadedCore)
                {
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
    }
}
