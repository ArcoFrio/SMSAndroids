using BepInEx;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SMSAndroidsCore
{
    /// <summary>
    /// Thin façade over the ModForge pack store plus the SMSAndroids-side
    /// after-sleep gameplay turnover.
    /// <para/>
    /// Persistence is owned entirely by SMSModForge.PackPlugin now: the pack
    /// declares every variable this mod uses and writes the single per-slot
    /// file (<c>SMSModForge_SMSAndroidsPack.json</c>) at the sleep autosave and
    /// on manual saves. This class no longer keeps its own cache or
    /// <c>SMSAndroidsCore_Save.txt</c> — the static <c>Get*/Set*</c> API simply
    /// reads/writes the pack through <see cref="ModForgeBridge"/>, so the many
    /// existing call sites (Schedule, MassageMinigame, Dialogues, Places, …)
    /// keep working unchanged. What remains here is the gameplay that fires at
    /// the post-sleep "Saved" moment (lottery re-rolls, gift-shop counter,
    /// schedule refresh) and a one-time importer that folds any legacy
    /// <c>.txt</c> values into the pack. The voyeur system is pack-owned now.
    /// </summary>
    [BepInPlugin(pluginGuid, Core.pluginName + " - SaveManager", Core.pluginVersion)]
    internal class SaveManager : BaseUnityPlugin
    {
        #region Plugin Info
        public const string pluginGuid = "treboy.starmakerstory.smsandroidscore.savemanager";
        #endregion

        /// <summary>The pack all SMSAndroids state now lives in.</summary>
        private const string PackId = "SMSAndroidsPack";

        // Latch for the post-sleep turnover gate (afterSleepEvents → savedUI).
        private bool afterSleepEventsProc = false;

        public void Awake()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        public void OnSceneLoaded(Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
        {
            if (scene.name == "CoreGameScene")
            {
                Core.RefreshDailyProxyVariables();
            }
            StartCoroutine(MigrateLegacySaveWhenReady());
        }

        public void Update()
        {
            if (Core.currentScene.name != "CoreGameScene") return;
            if (!Schedule.loadedSchedule) return;

            // Post-sleep turnover gate, same shape as before: latch when the
            // after-sleep sequence starts, run once the "Saved" UI shows. This
            // is gameplay only — the pack plugin performs the actual disk
            // commit (it flushes on the Saved-UI falling edge, after this block
            // has written its values into the shared pack store via the façade).
            if (Core.afterSleepEvents.activeSelf && !afterSleepEventsProc)
            {
                afterSleepEventsProc = true;
            }
            if (Core.savedUI.activeSelf && afterSleepEventsProc)
            {
                // What is left here is what the pack genuinely cannot do: it
                // reads GC2 proxy variables and plugin-side level state.
                //
                // Gone, and deliberately: the GiftShop_BuildCounter increment
                // (the pack's GS_Counter rule owns the count, and this was a
                // second, unlatched writer racing it), the HarborHome_SleepCD
                // reset (the variable is refreshMode Daily now), and two
                // MainStory flags nothing ever read.
                //
                // HarborHome_Slept is gone too, and is worth spelling out. The
                // original mod set it from harborHomeBedroomLevel.activeSelf —
                // "was the player in the Harbor Home bedroom when they slept" —
                // and that got rewritten to read harborHomeBedroomSwapApplied,
                // which Places only ever sets BECAUSE this variable is already
                // true. Each was written from the other, so from a fresh save
                // neither could ever become true and the bedroom swap could
                // never start. The pack's HarborHomeSleepFlag rule makes the
                // original observation again, on the same after-sleep gate;
                // re-adding a write here would race it and lose the first night.
                Core.RefreshDailyProxyVariables();
                Debug.Log("Day: " + Core.GetVariableNumber("Day"));
                // Gift-shop art is the pack's GiftShopArt integration rule now —
                // it re-evaluates every tick, so it also corrects itself on load
                // instead of only when something remembers to call it.
                afterSleepEventsProc = false;
            }
        }

        #region Public API (façade over the pack store)
        // Every getter/setter routes to the pack via ModForgeBridge. When
        // ModForge isn't loaded these degrade to the supplied default / no-op,
        // exactly like the bridge's other consumers. Numeric bounds (e.g.
        // Affection 0–5) are enforced by the pack's manifest declaration.

        public static void SetString(string variableName, string value)
            => ModForgeBridge.SetString(PackId, variableName, value ?? "");
        public static string GetString(string variableName, string defaultValue = "")
            => ModForgeBridge.GetString(PackId, variableName, defaultValue);

        public static void SetInt(string variableName, int value)
            => ModForgeBridge.SetInt(PackId, variableName, value);
        public static int GetInt(string variableName, int defaultValue = 0)
            => ModForgeBridge.GetInt(PackId, variableName, defaultValue);

        public static void SetFloat(string variableName, float value)
            => ModForgeBridge.SetFloat(PackId, variableName, value);
        public static float GetFloat(string variableName, float defaultValue = 0f)
            => ModForgeBridge.GetFloat(PackId, variableName, defaultValue);

        public static void SetBool(string variableName, bool value)
            => ModForgeBridge.SetBool(PackId, variableName, value);
        public static bool GetBool(string variableName, bool defaultValue = false)
            => ModForgeBridge.GetBool(PackId, variableName, defaultValue);

        public static bool HasVariable(string variableName)
            => ModForgeBridge.HasVariable(PackId, variableName);
        #endregion

        #region Legacy save migration
        private const string PackSaveFileName = "SMSModForge_SMSAndroidsPack.json";

        // Variables whose authoritative value historically lived ONLY in
        // SMSAndroidsCore_Save.txt (their writers called SaveManager, never the
        // pack). For a slot that already has a pack file, only these need
        // importing — the shared keys are already pack-authored there. For a
        // slot with NO pack file yet (the common case for existing saves, where
        // the pack json was never written), the .txt is the sole source of
        // truth and we import every declared key so shared progression
        // (affection / events / voyeur / places) survives the switch.
        private static readonly HashSet<string> LegacyOnlyKeys = new HashSet<string>(StringComparer.Ordinal)
        {
            "Minigame_Massage_Played",
            "Minigame_Massage_Highscore",
            "Minigame_Massage_Anis_Level",
            "Minigame_Massage_Anis_Highscore",
            "Wallpaper_Current",
        };

        private IEnumerator MigrateLegacySaveWhenReady()
        {
            // Wait until the slot is known AND the pack has actually BOUND that
            // slot (ActiveSlot == SlotLoaded). Binding does reset-then-load, so
            // importing before it would be wiped; importing after means our
            // writes sit in the live store and ride the next autosave to disk.
            while (Core.saveLoadManager == null || Core.saveLoadManager.SlotLoaded <= 0
                   || !ModForgeBridge.IsAvailable || !ModForgeBridge.HasPack(PackId)
                   || ModForgeBridge.GetPackActiveSlot(PackId) != Core.saveLoadManager.SlotLoaded)
            {
                yield return null;
            }
            TryMigrateLegacySlot(Core.saveLoadManager.SlotLoaded);
        }

        private void TryMigrateLegacySlot(int slot)
        {
            try
            {
                string path = LegacySaveFilePath(slot);
                if (string.IsNullOrEmpty(path) || !File.Exists(path)) return;

                // Whether the pack already owns this slot decides scope: if a
                // pack file exists the shared keys are authoritative there, so
                // we import only the SMSAndroids-only keys; otherwise the .txt
                // is the only record and we import everything declared.
                string dir = Path.GetDirectoryName(path);
                bool packExisted = File.Exists(Path.Combine(dir, PackSaveFileName));

                // Map declared pack vars by name, case-insensitively, so a
                // legacy key whose casing drifted from the manifest still lands
                // on the right (canonical) variable instead of being dropped.
                var declared = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                foreach (var n in ModForgeBridge.EnumerateVariables(PackId)) declared[n] = n;

                int imported = 0;
                foreach (var line in File.ReadAllLines(path))
                {
                    if (string.IsNullOrWhiteSpace(line) || !line.Contains("=")) continue;
                    var parts = line.Split(new[] { '=' }, 2);
                    string key = parts[0];
                    string raw = parts[1];
                    if (key == "saveSlot" || key == "lastSaved") continue;
                    if (!declared.TryGetValue(key, out string canonical)) continue; // not a declared pack var
                    if (packExisted && !LegacyOnlyKeys.Contains(canonical)) continue; // pack owns shared keys

                    // Write the raw string under the canonical name; the pack
                    // stores values as strings and parses/clamps per the
                    // variable's declared type on read.
                    ModForgeBridge.SetString(PackId, canonical, raw);
                    imported++;
                }

                // Persist the imported values to the pack file NOW, so a
                // load-then-quit (no sleep) can't lose them once we rename the
                // legacy file. Only rename after a confirmed write.
                if (!ModForgeBridge.FlushPackToDisk(PackId))
                {
                    Debug.LogWarning($"[SaveManager] Could not flush slot {slot} after import; leaving legacy file for retry.");
                    return;
                }

                // Rename so the import only ever runs once for this slot.
                string migrated = Path.Combine(dir, "SMSAndroidsCore_Save.migrated.txt");
                if (File.Exists(migrated)) File.Delete(migrated);
                File.Move(path, migrated);
                Debug.Log($"[SaveManager] Migrated {imported} value(s) from slot {slot} into the pack " +
                          $"({(packExisted ? "pack file existed — SMSAndroids-only keys" : "full import")}); renamed the old save.");
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[SaveManager] Legacy migration failed for slot {slot}: {e.Message}");
            }
        }

        private static string LegacySaveFilePath(int slot)
        {
            if (slot < 1) return null;
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string localLow = Path.Combine(Path.GetDirectoryName(appData), "LocalLow");
            string saves = Path.Combine(localLow, "Arvus Games", "Starmaker Story", "Saves");
            return Path.Combine(saves, $"NANOSAVE_{slot:D4}", "SMSAndroidsCore_Save.txt");
        }
        #endregion

    }
}
