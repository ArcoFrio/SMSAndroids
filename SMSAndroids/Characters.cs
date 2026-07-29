using BepInEx;
using GameCreator.Runtime.Common;
using System.Collections.Generic;
using UnityEngine;

namespace SMSAndroidsCore
{
    /// <summary>
    /// Bust catalog + Anis Harbor Home NPC setup. Previously this class also
    /// ran the entire hardcoded bust-creation pipeline (~900 lines of
    /// per-character <c>CreateNewBust</c> calls). That pipeline is now owned
    /// by <c>SMSModForge.PackPlugin.BustFactory</c>, which reads
    /// <c>SMSAndroidsPack/modpack.json</c> + the <c>actors/&lt;Name&gt;/outfits</c>
    /// array and creates one bust GameObject per outfit under
    /// <see cref="Core.bustManager"/>. The names ModForge writes match exactly
    /// what this class used to write (<c>"Amber"</c>, <c>"Claire"</c>,
    /// <c>"AnisBase"</c>, <c>"CentiBase"</c>, …), so existing call sites that
    /// reference our static fields keep working.
    /// <para/>
    /// What this class still owns:
    /// <list type="bullet">
    ///   <item>The 21 character + 15 vanilla bust <see cref="GameObject"/>
    ///   fields. Populated by name-lookup against <see cref="Core.bustManager"/>
    ///   once ModForge has finished its bust pass. Nullable — if the player
    ///   strips a character from the pack, the matching field stays null.</item>
    ///   <item><see cref="characterGiftLikesMap"/> — read by Core's gift glue.</item>
    ///   <item><see cref="AddBlinkingSpriteToBlinkObjects"/> — utility the
    ///   massage minigame still calls on its own character variants.</item>
    /// </list>
    /// (The Anis Harbor Home NPC slots that used to be built here from the
    /// character bundle are pack content now: NPC placements inside the
    /// HarborHome places' GameObject trees, with pose variants picked by the
    /// pack's own RandomChildActivator and visibility driven by each
    /// container's activeConditions.)
    /// </summary>
    [BepInPlugin(pluginGuid, Core.pluginName + " - Characters", Core.pluginVersion)]
    internal class Characters : BaseUnityPlugin
    {
        #region Plugin Info
        public const string pluginGuid = "treboy.starmakerstory.smsandroidscore.characters";
        #endregion

        // ── 21 character base bust references ─────────────────────────────
        // Populated below from Core.bustManager by name. The vanilla + pack
        // bust GameObjects are children of 2_Bust_Manager; the pack ones are
        // created by ModForge's BustFactory using the same names we used to
        // assign manually here.
        public static GameObject amber, claire, sarah;
        public static GameObject anis, centi, dorothy, elegg, frima, guilty, helm, maiden,
                                 mary, mast, neon, pepper, rapi, rosanna, sakura, tove, viper, yan;

        // ── Vanilla side characters (always present in 2_Bust_Manager) ────
        public static GameObject adrian, ameliaSwim, anna, doctorFrost, emmaSwim, gabriel,
                                 isabella, katarina, kate, masterZhen, mobsterBlonde,
                                 river, samSwim, sofia, toni;

        // ── Cameo ─────────────────────────────────────────────────────────
        public static GameObject solidSnake;

        // ── Character → gifts they like mapping ───────────────────────────
        // Read by Core.IncrementAffectionForGiftIfLiked to decide whether
        // giving a particular gift bumps a character's affection. Pack-side
        // alternative: declare the same map via <c>characterGiftLikes</c>
        // entries in the modpack's actors[].outfits[].gifts list (the bust
        // pack loader folds those into this same dict at load time, so adds
        // here remain authoritative for SMSAndroids-only characters).
        public static Dictionary<string, List<string>> characterGiftLikesMap = new Dictionary<string, List<string>>
        {
            { "Anis", new List<string> { "Beer", "Body-Oil", "Chocolate", "Gift_Sunglasses", "Wine" } },
        };

        public static bool loadedBusts = false;

        public void Update()
        {
            if (Core.currentScene.name == "CoreGameScene")
            {
                if (!loadedBusts && Core.loadedCore && Places.loadedPlaces)
                {
                    // Defer one extra frame until ModForge has had a chance
                    // to populate the pack busts. We don't take a hard
                    // reference on ModForge; instead we wait for one of the
                    // expected pack-built bust GO names to appear under
                    // bustManager. If ModForge isn't installed, this gate
                    // never opens — leave it that way; SMSAndroids without
                    // the pack has no busts to use anyway.
                    if (Core.bustManager == null) return;
                    if (Core.bustManager.Find("AnisBase") == null) return;

                    ResolveCharacterBusts();
                    ResolveVanillaBusts();

                    Logger.LogInfo("----- BUSTS LOADED -----");
                    loadedBusts = true;
                }
            }
            if (Core.currentScene.name == "GameStart")
            {
                if (loadedBusts)
                {
                    Logger.LogInfo("----- BUSTS UNLOADED -----");
                    loadedBusts = false;
                }
            }
        }

        /// <summary>Look up each of the 21 character base busts by the
        /// GO name ModForge writes for them. Missing characters stay null.</summary>
        private static void ResolveCharacterBusts()
        {
            amber   = Core.bustManager.Find("Amber")?.gameObject;
            claire  = Core.bustManager.Find("Claire")?.gameObject;
            sarah   = Core.bustManager.Find("Sarah")?.gameObject;
            anis    = Core.bustManager.Find("AnisBase")?.gameObject;
            centi   = Core.bustManager.Find("CentiBase")?.gameObject;
            dorothy = Core.bustManager.Find("DorothyBase")?.gameObject;
            elegg   = Core.bustManager.Find("EleggBase")?.gameObject;
            frima   = Core.bustManager.Find("FrimaBase")?.gameObject;
            guilty  = Core.bustManager.Find("GuiltyBase")?.gameObject;
            helm    = Core.bustManager.Find("HelmBase")?.gameObject;
            maiden  = Core.bustManager.Find("MaidenBase")?.gameObject;
            mary    = Core.bustManager.Find("MaryBase")?.gameObject;
            mast    = Core.bustManager.Find("MastBase")?.gameObject;
            neon    = Core.bustManager.Find("NeonBase")?.gameObject;
            pepper  = Core.bustManager.Find("PepperBase")?.gameObject;
            rapi    = Core.bustManager.Find("RapiBase")?.gameObject;
            rosanna = Core.bustManager.Find("RosannaBase")?.gameObject;
            sakura  = Core.bustManager.Find("SakuraBase")?.gameObject;
            tove    = Core.bustManager.Find("ToveBase")?.gameObject;
            viper   = Core.bustManager.Find("ViperBase")?.gameObject;
            yan     = Core.bustManager.Find("YanBase")?.gameObject;

            solidSnake = Core.bustManager.Find("Snek")?.gameObject;
        }

        /// <summary>Snapshot the always-present vanilla bust GOs.</summary>
        private static void ResolveVanillaBusts()
        {
            adrian        = Core.bustManager.Find("Adrian_bust")?.gameObject;
            ameliaSwim    = Core.bustManager.Find("Amelia_Beach")?.gameObject;
            anna          = Core.bustManager.Find("Anna_Bust")?.gameObject;
            doctorFrost   = Core.bustManager.Find("doctorfrost_default")?.gameObject;
            emmaSwim      = Core.bustManager.Find("Emma_Swimwear")?.gameObject;
            gabriel       = Core.bustManager.Find("Gabriel_Bust")?.gameObject;
            isabella      = Core.bustManager.Find("Isabella")?.gameObject;
            katarina      = Core.bustManager.Find("Katarina_Normal")?.gameObject;
            kate          = Core.bustManager.Find("Kate")?.gameObject;
            masterZhen    = Core.bustManager.Find("Master_Default")?.gameObject;
            mobsterBlonde = Core.bustManager.Find("S_Mobster1")?.gameObject;
            river         = Core.bustManager.Find("River_Base")?.gameObject;
            samSwim       = Core.bustManager.Find("Samantha_Swimsuit")?.gameObject;
            sofia         = Core.bustManager.Find("Sofia_Police")?.gameObject;
            toni          = Core.bustManager.Find("TomboyToni_BustDefault")?.gameObject;
        }

        // ── Shared visual helpers ─────────────────────────────────

        /// <summary>
        /// Recursive walk that attaches a <c>BlinkingSprite</c> component to
        /// every descendant named "Blink". Public + static so the massage
        /// minigame can run it after instancing its own character variants.
        /// </summary>
        public static void AddBlinkingSpriteToBlinkObjects(GameObject parent)
        {
            if (parent == null)
            {
                Debug.LogWarning("AddBlinkingSpriteToBlinkObjects: parent GameObject is null");
                return;
            }
            foreach (Transform child in parent.transform)
            {
                if (child.gameObject.name == "Blink" &&
                    child.gameObject.GetComponent<BlinkingSprite>() == null)
                {
                    child.gameObject.AddComponent<BlinkingSprite>();
                }
                AddBlinkingSpriteToBlinkObjects(child.gameObject);
            }
        }

    }
}
