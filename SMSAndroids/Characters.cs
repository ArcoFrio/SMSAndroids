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
    ///   <item>Anis Harbor Home NPC slots — the empty <see cref="Transform"/>
    ///   parents + per-position <see cref="GameObject"/> NPC variants
    ///   (Default / Swim / Naked-for-shower). Loaded from the character bundle
    ///   in <see cref="Core.characterBundle"/>; not pack-driven content.</item>
    ///   <item><see cref="characterGiftLikesMap"/> — read by Core's gift glue.</item>
    ///   <item><see cref="AddBlinkingSpriteToBlinkObjects"/> + the particle copy
    ///   helpers — utilities the massage minigame still calls.</item>
    /// </list>
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

        // ── Anis Harbor Home NPC slots + variants ─────────────────────────
        // Each pair: an empty Transform slot anchored under a room's NPCs
        // sub-tree (created on init), and the NPC visual variants
        // (Default outfit, Swim outfit, Naked-for-shower) loaded as children
        // from Core.characterBundle. Schedule.cs sets pickNewOnEnable each
        // time Anis moves and per-frame gates each variant's SetActive based
        // on her current location + HarborHome_Outfit_Anis save state.
        public static Transform anisNPCHHBedleft, anisNPCHHBedright;
        public static Transform anisNPCHHChangingleft, anisNPCHHChangingright;
        public static Transform anisNPCHHCouchleft, anisNPCHHCouchright;
        public static Transform anisNPCHHFridge, anisNPCHHSink;
        public static Transform anisNPCHHShower;
        public static Transform anisNPCHHTanningleft, anisNPCHHTanningright;

        public static GameObject anisNPCHHBedleftDefault, anisNPCHHBedrightDefault;
        public static GameObject anisNPCHHChangingleftDefault, anisNPCHHChangingrightDefault;
        public static GameObject anisNPCHHCouchleftDefault, anisNPCHHCouchrightDefault;
        public static GameObject anisNPCHHFridgeDefault, anisNPCHHSinkDefault;

        public static GameObject anisNPCHHBedleftSwim, anisNPCHHBedrightSwim;
        public static GameObject anisNPCHHChangingleftSwim, anisNPCHHChangingrightSwim;
        public static GameObject anisNPCHHCouchleftSwim, anisNPCHHCouchrightSwim;
        public static GameObject anisNPCHHFridgeSwim, anisNPCHHSinkSwim;
        public static GameObject anisNPCHHTanningleftSwim, anisNPCHHTanningrightSwim;

        public static GameObject anisNPCHHShowerNaked;

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
                    SetupAnisHarborHomeNPCs();

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

        /// <summary>
        /// Build the Anis NPC slot tree under the Harbor Home rooms (created
        /// by <see cref="Places"/>). Each slot is an empty Transform named
        /// "Anis"; per-pose visual variants are loaded from the character
        /// bundle into each slot as Default / Swim / Naked children.
        /// Schedule.cs gates each variant's <c>SetActive</c> per frame from
        /// <c>anisLocation</c> + the <c>HarborHome_Outfit_Anis</c> save key.
        /// </summary>
        private void SetupAnisHarborHomeNPCs()
        {
            // Slot anchors — one empty Transform per Anis-eligible position.
            anisNPCHHBedleft       = GameObject.Instantiate(new GameObject(), Places.harborHomeBedroomNPCBedleft).transform;       anisNPCHHBedleft.name       = "Anis";
            anisNPCHHBedright      = GameObject.Instantiate(new GameObject(), Places.harborHomeBedroomNPCBedright).transform;      anisNPCHHBedright.name      = "Anis";
            anisNPCHHChangingleft  = GameObject.Instantiate(new GameObject(), Places.harborHomeClosetNPCChangingleft).transform;   anisNPCHHChangingleft.name  = "Anis";
            anisNPCHHChangingright = GameObject.Instantiate(new GameObject(), Places.harborHomeClosetNPCChangingright).transform;  anisNPCHHChangingright.name = "Anis";
            anisNPCHHCouchleft     = GameObject.Instantiate(new GameObject(), Places.harborHomeLivingroomNPCCouchleft).transform;  anisNPCHHCouchleft.name     = "Anis";
            anisNPCHHCouchright    = GameObject.Instantiate(new GameObject(), Places.harborHomeLivingroomNPCCouchright).transform; anisNPCHHCouchright.name    = "Anis";
            anisNPCHHFridge        = GameObject.Instantiate(new GameObject(), Places.harborHomeKitchenNPCFridge).transform;        anisNPCHHFridge.name        = "Anis";
            anisNPCHHShower        = GameObject.Instantiate(new GameObject(), Places.harborHomeBathroomNPCShower).transform;       anisNPCHHShower.name        = "Anis";
            anisNPCHHSink          = GameObject.Instantiate(new GameObject(), Places.harborHomeKitchenNPCSink).transform;          anisNPCHHSink.name          = "Anis";
            anisNPCHHTanningleft   = GameObject.Instantiate(new GameObject(), Places.harborHomePoolNPCTanningleft).transform;      anisNPCHHTanningleft.name   = "Anis";
            anisNPCHHTanningright  = GameObject.Instantiate(new GameObject(), Places.harborHomePoolNPCTanningright).transform;     anisNPCHHTanningright.name  = "Anis";

            // Default-outfit variants (clothed)
            anisNPCHHBedleftDefault       = CreateNPC("AnisNPCBedleftDefault",       anisNPCHHBedleft,       "Default");
            anisNPCHHBedrightDefault      = CreateNPC("AnisNPCBedrightDefault",      anisNPCHHBedright,      "Default");
            anisNPCHHChangingleftDefault  = CreateNPC("AnisNPCChangingleftDefault",  anisNPCHHChangingleft,  "Default");
            anisNPCHHChangingrightDefault = CreateNPC("AnisNPCChangingrightDefault", anisNPCHHChangingright, "Default");
            anisNPCHHCouchleftDefault     = CreateNPC("AnisNPCCouchleftDefault",     anisNPCHHCouchleft,     "Default");
            anisNPCHHCouchrightDefault    = CreateNPC("AnisNPCCouchrightDefault",    anisNPCHHCouchright,    "Default");
            anisNPCHHFridgeDefault        = CreateNPC("AnisNPCFridgeDefault",        anisNPCHHFridge,        "Default");
            anisNPCHHSinkDefault          = CreateNPC("AnisNPCSinkDefault",          anisNPCHHSink,          "Default");

            // Swimsuit variants
            anisNPCHHBedleftSwim       = CreateNPC("AnisNPCBedleftSwim",       anisNPCHHBedleft,       "Swim");
            anisNPCHHBedrightSwim      = CreateNPC("AnisNPCBedrightSwim",      anisNPCHHBedright,      "Swim");
            anisNPCHHChangingleftSwim  = CreateNPC("AnisNPCChangingleftSwim",  anisNPCHHChangingleft,  "Swim");
            anisNPCHHChangingrightSwim = CreateNPC("AnisNPCChangingrightSwim", anisNPCHHChangingright, "Swim");
            anisNPCHHCouchleftSwim     = CreateNPC("AnisNPCCouchleftSwim",     anisNPCHHCouchleft,     "Swim");
            anisNPCHHCouchrightSwim    = CreateNPC("AnisNPCCouchrightSwim",    anisNPCHHCouchright,    "Swim");
            anisNPCHHFridgeSwim        = CreateNPC("AnisNPCFridgeSwim",        anisNPCHHFridge,        "Swim");
            anisNPCHHSinkSwim          = CreateNPC("AnisNPCSinkSwim",          anisNPCHHSink,          "Swim");
            anisNPCHHTanningleftSwim   = CreateNPC("AnisNPCTanningleftSwim",   anisNPCHHTanningleft,   "Swim");
            anisNPCHHTanningrightSwim  = CreateNPC("AnisNPCTanningrightSwim",  anisNPCHHTanningright,  "Swim");

            // Shower naked variant (needs particle copy for the steam effect)
            anisNPCHHShowerNaked       = CreateNPC("AnisNPCShowerNaked", anisNPCHHShower, "Naked", copyParticles: true);
        }

        // ── NPC factory + helpers ─────────────────────────────────────────

        /// <summary>
        /// Instantiate an NPC variant from a character-bundle asset under
        /// the given parent slot. Adds the components Schedule relies on:
        /// <see cref="RandomChildActivator"/> (pose randomisation when the
        /// slot activates), the recursive <c>BlinkingSprite</c> binder, and
        /// the <c>FadeInAlpha</c> + particle copy as needed.
        /// </summary>
        public static GameObject CreateNPC(string assetName, Transform parent, string displayName, bool copyParticles = false)
        {
            GameObject npc = GameObject.Instantiate(Core.characterBundle.LoadAsset<GameObject>(assetName), parent);
            npc.name = displayName;
            npc.gameObject.AddComponent<RandomChildActivator>();
            AddBlinkingSpriteToBlinkObjects(npc);
            AddFadeInSpriteToBlinkParents(npc);
            if (copyParticles)
                CopyParticleSystemToParticleObjects(npc);
            return npc;
        }

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

        /// <summary>
        /// Recursive walk that attaches a <c>FadeInSprite</c> to the direct
        /// parent of every "Blink" GameObject (so the bust fades in cleanly
        /// rather than popping). Used by <see cref="CreateNPC"/>.
        /// </summary>
        public static void AddFadeInSpriteToBlinkParents(GameObject root)
        {
            if (root == null) return;
            foreach (Transform child in root.transform)
            {
                if (child.name == "Blink" && child.parent != null &&
                    child.parent.gameObject.GetComponent<FadeInSprite>() == null)
                {
                    child.parent.gameObject.AddComponent<FadeInSprite>();
                }
                AddFadeInSpriteToBlinkParents(child.gameObject);
            }
        }

        /// <summary>
        /// Recursive walk that copies every <c>ParticleSystem</c> module
        /// configuration from the vanilla AnnaInShower particle source onto
        /// every descendant named <c>"Particle System (1)"</c>. Only invoked
        /// for the shower-naked variant; the rest of the NPC variants don't
        /// need particles.
        /// </summary>
        public static void CopyParticleSystemToParticleObjects(GameObject parent)
        {
            if (parent == null)
            {
                Debug.LogWarning("CopyParticleSystemToParticleObjects: parent GameObject is null");
                return;
            }
            GameObject sourceGO = Core.level.Find("4_Bath").Find("AnnaInShower").Find("Particle System (1)").gameObject;
            ParticleSystem sourcePS = sourceGO.GetComponent<ParticleSystem>();
            if (sourcePS == null)
            {
                Debug.LogError("CopyParticleSystemToParticleObjects: Source ParticleSystem not found!");
                return;
            }
            foreach (Transform child in parent.transform)
            {
                if (child.gameObject.name == "Particle System (1)")
                {
                    ParticleSystem targetPS = child.gameObject.GetComponent<ParticleSystem>();
                    if (targetPS != null) CopyParticleSystemSettings(sourcePS, targetPS);
                }
                CopyParticleSystemToParticleObjects(child.gameObject);
            }
        }

        /// <summary>
        /// Field-by-field copy of every ParticleSystem module from
        /// <paramref name="source"/> onto <paramref name="target"/>. Long
        /// because Unity's particle modules are dozens of fields each and
        /// there's no built-in deep-copy. Kept verbatim from the pre-strip
        /// version — the particle behavior on Anis's shower NPC depends on
        /// matching the vanilla AnnaInShower source exactly.
        /// </summary>
        private static void CopyParticleSystemSettings(ParticleSystem source, ParticleSystem target)
        {
            // Main module
            var sourceMain = source.main; var targetMain = target.main;
            targetMain.duration = sourceMain.duration;
            targetMain.loop = sourceMain.loop;
            targetMain.prewarm = sourceMain.prewarm;
            targetMain.startDelay = sourceMain.startDelay;
            targetMain.startDelayMultiplier = sourceMain.startDelayMultiplier;
            targetMain.startLifetime = sourceMain.startLifetime;
            targetMain.startLifetimeMultiplier = sourceMain.startLifetimeMultiplier;
            targetMain.startSpeed = sourceMain.startSpeed;
            targetMain.startSpeedMultiplier = sourceMain.startSpeedMultiplier;
            targetMain.startSize3D = sourceMain.startSize3D;
            targetMain.startSize = sourceMain.startSize;
            targetMain.startSizeMultiplier = sourceMain.startSizeMultiplier;
            targetMain.startSizeX = sourceMain.startSizeX;
            targetMain.startSizeXMultiplier = sourceMain.startSizeXMultiplier;
            targetMain.startSizeY = sourceMain.startSizeY;
            targetMain.startSizeYMultiplier = sourceMain.startSizeYMultiplier;
            targetMain.startSizeZ = sourceMain.startSizeZ;
            targetMain.startSizeZMultiplier = sourceMain.startSizeZMultiplier;
            targetMain.startRotation3D = sourceMain.startRotation3D;
            targetMain.startRotation = sourceMain.startRotation;
            targetMain.startRotationMultiplier = sourceMain.startRotationMultiplier;
            targetMain.startRotationX = sourceMain.startRotationX;
            targetMain.startRotationXMultiplier = sourceMain.startRotationXMultiplier;
            targetMain.startRotationY = sourceMain.startRotationY;
            targetMain.startRotationYMultiplier = sourceMain.startRotationYMultiplier;
            targetMain.startRotationZ = sourceMain.startRotationZ;
            targetMain.startRotationZMultiplier = sourceMain.startRotationZMultiplier;
            targetMain.flipRotation = sourceMain.flipRotation;
            targetMain.startColor = sourceMain.startColor;
            targetMain.gravityModifier = sourceMain.gravityModifier;
            targetMain.gravityModifierMultiplier = sourceMain.gravityModifierMultiplier;
            targetMain.simulationSpace = sourceMain.simulationSpace;
            targetMain.simulationSpeed = sourceMain.simulationSpeed;
            targetMain.useUnscaledTime = sourceMain.useUnscaledTime;
            targetMain.scalingMode = sourceMain.scalingMode;
            targetMain.playOnAwake = sourceMain.playOnAwake;
            targetMain.emitterVelocityMode = sourceMain.emitterVelocityMode;
            targetMain.maxParticles = sourceMain.maxParticles;
            targetMain.stopAction = sourceMain.stopAction;
            targetMain.cullingMode = sourceMain.cullingMode;
            targetMain.ringBufferMode = sourceMain.ringBufferMode;
            targetMain.ringBufferLoopRange = sourceMain.ringBufferLoopRange;

            // Emission module
            var sourceEmission = source.emission; var targetEmission = target.emission;
            targetEmission.enabled = sourceEmission.enabled;
            targetEmission.rateOverTime = sourceEmission.rateOverTime;
            targetEmission.rateOverTimeMultiplier = sourceEmission.rateOverTimeMultiplier;
            targetEmission.rateOverDistance = sourceEmission.rateOverDistance;
            targetEmission.rateOverDistanceMultiplier = sourceEmission.rateOverDistanceMultiplier;

            // Shape module
            var sourceShape = source.shape; var targetShape = target.shape;
            targetShape.enabled = sourceShape.enabled;
            targetShape.shapeType = sourceShape.shapeType;
            targetShape.angle = sourceShape.angle;
            targetShape.radius = sourceShape.radius;
            targetShape.radiusThickness = sourceShape.radiusThickness;
            targetShape.arc = sourceShape.arc;
            targetShape.arcMode = sourceShape.arcMode;
            targetShape.arcSpread = sourceShape.arcSpread;
            targetShape.arcSpeed = sourceShape.arcSpeed;
            targetShape.arcSpeedMultiplier = sourceShape.arcSpeedMultiplier;
            targetShape.length = sourceShape.length;
            targetShape.boxThickness = sourceShape.boxThickness;
            targetShape.meshShapeType = sourceShape.meshShapeType;
            targetShape.mesh = sourceShape.mesh;
            targetShape.meshRenderer = sourceShape.meshRenderer;
            targetShape.skinnedMeshRenderer = sourceShape.skinnedMeshRenderer;
            targetShape.useMeshMaterialIndex = sourceShape.useMeshMaterialIndex;
            targetShape.meshMaterialIndex = sourceShape.meshMaterialIndex;
            targetShape.useMeshColors = sourceShape.useMeshColors;
            targetShape.normalOffset = sourceShape.normalOffset;
            targetShape.meshSpawnMode = sourceShape.meshSpawnMode;
            targetShape.meshSpawnSpread = sourceShape.meshSpawnSpread;
            targetShape.meshSpawnSpeed = sourceShape.meshSpawnSpeed;
            targetShape.meshSpawnSpeedMultiplier = sourceShape.meshSpawnSpeedMultiplier;
            targetShape.alignToDirection = sourceShape.alignToDirection;
            targetShape.randomDirectionAmount = sourceShape.randomDirectionAmount;
            targetShape.sphericalDirectionAmount = sourceShape.sphericalDirectionAmount;
            targetShape.randomPositionAmount = sourceShape.randomPositionAmount;
            targetShape.position = sourceShape.position;
            targetShape.rotation = sourceShape.rotation;
            targetShape.scale = sourceShape.scale;

            // Velocity-over-Lifetime
            var sourceVelocity = source.velocityOverLifetime; var targetVelocity = target.velocityOverLifetime;
            targetVelocity.enabled = sourceVelocity.enabled;
            targetVelocity.x = sourceVelocity.x; targetVelocity.y = sourceVelocity.y; targetVelocity.z = sourceVelocity.z;
            targetVelocity.xMultiplier = sourceVelocity.xMultiplier;
            targetVelocity.yMultiplier = sourceVelocity.yMultiplier;
            targetVelocity.zMultiplier = sourceVelocity.zMultiplier;
            targetVelocity.space = sourceVelocity.space;
            targetVelocity.orbitalX = sourceVelocity.orbitalX;
            targetVelocity.orbitalY = sourceVelocity.orbitalY;
            targetVelocity.orbitalZ = sourceVelocity.orbitalZ;
            targetVelocity.orbitalXMultiplier = sourceVelocity.orbitalXMultiplier;
            targetVelocity.orbitalYMultiplier = sourceVelocity.orbitalYMultiplier;
            targetVelocity.orbitalZMultiplier = sourceVelocity.orbitalZMultiplier;
            targetVelocity.orbitalOffsetX = sourceVelocity.orbitalOffsetX;
            targetVelocity.orbitalOffsetY = sourceVelocity.orbitalOffsetY;
            targetVelocity.orbitalOffsetZ = sourceVelocity.orbitalOffsetZ;
            targetVelocity.orbitalOffsetXMultiplier = sourceVelocity.orbitalOffsetXMultiplier;
            targetVelocity.orbitalOffsetYMultiplier = sourceVelocity.orbitalOffsetYMultiplier;
            targetVelocity.orbitalOffsetZMultiplier = sourceVelocity.orbitalOffsetZMultiplier;
            targetVelocity.radial = sourceVelocity.radial;
            targetVelocity.radialMultiplier = sourceVelocity.radialMultiplier;
            targetVelocity.speedModifier = sourceVelocity.speedModifier;
            targetVelocity.speedModifierMultiplier = sourceVelocity.speedModifierMultiplier;

            // Limit-Velocity-over-Lifetime
            var sourceLimitVelocity = source.limitVelocityOverLifetime; var targetLimitVelocity = target.limitVelocityOverLifetime;
            targetLimitVelocity.enabled = sourceLimitVelocity.enabled;
            targetLimitVelocity.limitX = sourceLimitVelocity.limitX;
            targetLimitVelocity.limitY = sourceLimitVelocity.limitY;
            targetLimitVelocity.limitZ = sourceLimitVelocity.limitZ;
            targetLimitVelocity.limitXMultiplier = sourceLimitVelocity.limitXMultiplier;
            targetLimitVelocity.limitYMultiplier = sourceLimitVelocity.limitYMultiplier;
            targetLimitVelocity.limitZMultiplier = sourceLimitVelocity.limitZMultiplier;
            targetLimitVelocity.limit = sourceLimitVelocity.limit;
            targetLimitVelocity.limitMultiplier = sourceLimitVelocity.limitMultiplier;
            targetLimitVelocity.dampen = sourceLimitVelocity.dampen;
            targetLimitVelocity.separateAxes = sourceLimitVelocity.separateAxes;
            targetLimitVelocity.space = sourceLimitVelocity.space;
            targetLimitVelocity.drag = sourceLimitVelocity.drag;
            targetLimitVelocity.dragMultiplier = sourceLimitVelocity.dragMultiplier;
            targetLimitVelocity.multiplyDragByParticleSize = sourceLimitVelocity.multiplyDragByParticleSize;
            targetLimitVelocity.multiplyDragByParticleVelocity = sourceLimitVelocity.multiplyDragByParticleVelocity;

            // Color-over-Lifetime / Color-by-Speed
            var sourceColor = source.colorOverLifetime; var targetColor = target.colorOverLifetime;
            targetColor.enabled = sourceColor.enabled;
            targetColor.color = sourceColor.color;
            var sourceColorBySpeed = source.colorBySpeed; var targetColorBySpeed = target.colorBySpeed;
            targetColorBySpeed.enabled = sourceColorBySpeed.enabled;
            targetColorBySpeed.color = sourceColorBySpeed.color;
            targetColorBySpeed.range = sourceColorBySpeed.range;

            // Size-over-Lifetime / Size-by-Speed
            var sourceSize = source.sizeOverLifetime; var targetSize = target.sizeOverLifetime;
            targetSize.enabled = sourceSize.enabled;
            targetSize.separateAxes = sourceSize.separateAxes;
            targetSize.size = sourceSize.size;
            targetSize.sizeMultiplier = sourceSize.sizeMultiplier;
            targetSize.x = sourceSize.x; targetSize.xMultiplier = sourceSize.xMultiplier;
            targetSize.y = sourceSize.y; targetSize.yMultiplier = sourceSize.yMultiplier;
            targetSize.z = sourceSize.z; targetSize.zMultiplier = sourceSize.zMultiplier;
            var sourceSizeBySpeed = source.sizeBySpeed; var targetSizeBySpeed = target.sizeBySpeed;
            targetSizeBySpeed.enabled = sourceSizeBySpeed.enabled;
            targetSizeBySpeed.separateAxes = sourceSizeBySpeed.separateAxes;
            targetSizeBySpeed.size = sourceSizeBySpeed.size;
            targetSizeBySpeed.sizeMultiplier = sourceSizeBySpeed.sizeMultiplier;
            targetSizeBySpeed.x = sourceSizeBySpeed.x; targetSizeBySpeed.xMultiplier = sourceSizeBySpeed.xMultiplier;
            targetSizeBySpeed.y = sourceSizeBySpeed.y; targetSizeBySpeed.yMultiplier = sourceSizeBySpeed.yMultiplier;
            targetSizeBySpeed.z = sourceSizeBySpeed.z; targetSizeBySpeed.zMultiplier = sourceSizeBySpeed.zMultiplier;
            targetSizeBySpeed.range = sourceSizeBySpeed.range;

            // Rotation-over-Lifetime / Rotation-by-Speed
            var sourceRotation = source.rotationOverLifetime; var targetRotation = target.rotationOverLifetime;
            targetRotation.enabled = sourceRotation.enabled;
            targetRotation.x = sourceRotation.x; targetRotation.xMultiplier = sourceRotation.xMultiplier;
            targetRotation.y = sourceRotation.y; targetRotation.yMultiplier = sourceRotation.yMultiplier;
            targetRotation.z = sourceRotation.z; targetRotation.zMultiplier = sourceRotation.zMultiplier;
            targetRotation.separateAxes = sourceRotation.separateAxes;
            var sourceRotationBySpeed = source.rotationBySpeed; var targetRotationBySpeed = target.rotationBySpeed;
            targetRotationBySpeed.enabled = sourceRotationBySpeed.enabled;
            targetRotationBySpeed.x = sourceRotationBySpeed.x; targetRotationBySpeed.xMultiplier = sourceRotationBySpeed.xMultiplier;
            targetRotationBySpeed.y = sourceRotationBySpeed.y; targetRotationBySpeed.yMultiplier = sourceRotationBySpeed.yMultiplier;
            targetRotationBySpeed.z = sourceRotationBySpeed.z; targetRotationBySpeed.zMultiplier = sourceRotationBySpeed.zMultiplier;
            targetRotationBySpeed.separateAxes = sourceRotationBySpeed.separateAxes;
            targetRotationBySpeed.range = sourceRotationBySpeed.range;

            // Noise module
            var sourceNoise = source.noise; var targetNoise = target.noise;
            targetNoise.enabled = sourceNoise.enabled;
            targetNoise.separateAxes = sourceNoise.separateAxes;
            targetNoise.strength = sourceNoise.strength;
            targetNoise.strengthMultiplier = sourceNoise.strengthMultiplier;
            targetNoise.strengthX = sourceNoise.strengthX; targetNoise.strengthXMultiplier = sourceNoise.strengthXMultiplier;
            targetNoise.strengthY = sourceNoise.strengthY; targetNoise.strengthYMultiplier = sourceNoise.strengthYMultiplier;
            targetNoise.strengthZ = sourceNoise.strengthZ; targetNoise.strengthZMultiplier = sourceNoise.strengthZMultiplier;
            targetNoise.frequency = sourceNoise.frequency;
            targetNoise.scrollSpeed = sourceNoise.scrollSpeed;
            targetNoise.scrollSpeedMultiplier = sourceNoise.scrollSpeedMultiplier;
            targetNoise.damping = sourceNoise.damping;
            targetNoise.octaveCount = sourceNoise.octaveCount;
            targetNoise.octaveMultiplier = sourceNoise.octaveMultiplier;
            targetNoise.octaveScale = sourceNoise.octaveScale;
            targetNoise.quality = sourceNoise.quality;
            targetNoise.remapEnabled = sourceNoise.remapEnabled;
            targetNoise.remap = sourceNoise.remap; targetNoise.remapMultiplier = sourceNoise.remapMultiplier;
            targetNoise.remapX = sourceNoise.remapX; targetNoise.remapXMultiplier = sourceNoise.remapXMultiplier;
            targetNoise.remapY = sourceNoise.remapY; targetNoise.remapYMultiplier = sourceNoise.remapYMultiplier;
            targetNoise.remapZ = sourceNoise.remapZ; targetNoise.remapZMultiplier = sourceNoise.remapZMultiplier;
            targetNoise.positionAmount = sourceNoise.positionAmount;
            targetNoise.rotationAmount = sourceNoise.rotationAmount;
            targetNoise.sizeAmount = sourceNoise.sizeAmount;

            // Renderer module
            var sourceRenderer = source.GetComponent<ParticleSystemRenderer>();
            var targetRenderer = target.GetComponent<ParticleSystemRenderer>();
            if (sourceRenderer != null && targetRenderer != null)
            {
                targetRenderer.renderMode = sourceRenderer.renderMode;
                targetRenderer.sortMode = sourceRenderer.sortMode;
                targetRenderer.sortingFudge = sourceRenderer.sortingFudge;
                targetRenderer.normalDirection = sourceRenderer.normalDirection;
                targetRenderer.material = sourceRenderer.material;
                targetRenderer.trailMaterial = sourceRenderer.trailMaterial;
                targetRenderer.sortingLayerID = sourceRenderer.sortingLayerID;
                targetRenderer.sortingOrder = sourceRenderer.sortingOrder;
                targetRenderer.minParticleSize = sourceRenderer.minParticleSize;
                targetRenderer.maxParticleSize = sourceRenderer.maxParticleSize;
                targetRenderer.alignment = sourceRenderer.alignment;
                targetRenderer.flip = sourceRenderer.flip;
                targetRenderer.allowRoll = sourceRenderer.allowRoll;
                targetRenderer.pivot = sourceRenderer.pivot;
                targetRenderer.maskInteraction = sourceRenderer.maskInteraction;
                targetRenderer.enableGPUInstancing = sourceRenderer.enableGPUInstancing;
                targetRenderer.shadowCastingMode = sourceRenderer.shadowCastingMode;
                targetRenderer.receiveShadows = sourceRenderer.receiveShadows;
                targetRenderer.shadowBias = sourceRenderer.shadowBias;
                targetRenderer.motionVectorGenerationMode = sourceRenderer.motionVectorGenerationMode;
                targetRenderer.forceRenderingOff = sourceRenderer.forceRenderingOff;
                targetRenderer.lightProbeUsage = sourceRenderer.lightProbeUsage;
                targetRenderer.reflectionProbeUsage = sourceRenderer.reflectionProbeUsage;
                targetRenderer.probeAnchor = sourceRenderer.probeAnchor;
            }
        }
    }
}
