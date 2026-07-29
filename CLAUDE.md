# SMSAndroids — Project Reference

This file is auto-loaded by Claude Code into every conversation in this repo. It is a structural reference for the **Androids mod** for the Unity visual novel **Starmaker Story** (target version **1.8E**). Keep it accurate — update it whenever a class's responsibility, public surface, or load gating changes.

## The two-plugin architecture (read this first)

Content for this mod has been migrated out of hardcoded C# into a **data-driven mod pack** loaded by a sibling plugin:

- **SMSAndroidsCore** (this repo, .NET 4.8 → `SMSAndroidsCore.dll`) — native features that can't be expressed as pack data: schedule/roaming simulation, minigames, gift UI, world-map overlays, save management, and the bridges below.
- **SMSModForge.PackPlugin** (sibling repo `C:\Users\gabri\source\repos\SMSBustForge`) — generic pack runtime. Reads `.smspack` archives (ZIP, `modpack.json` at root) from `BepInEx/plugins/SMSModForge/ModPacks/` and builds **dialogues, scene CGs, wallpapers, music, SFX, places/levels, navigator + radial map buttons, the extended navigator grid, busts/outfits, and pack variables** at runtime. The migrated content ships as `SMSAndroidsPack.smspack`, authored with the SMSBustForge WPF editor.

Neither DLL hard-references the other. Coordination is reflection + GC2 signals:

| Bridge (this repo) | Direction | Purpose |
| --- | --- | --- |
| [ModForgeBridge.cs](SMSAndroids/ModForgeBridge.cs) | SMSAndroids → ModForge | Typed `Get/Set/Subscribe` on pack variables + generic queries like `IsAnyDialoguePlaying()`; lazy two-stage type resolution, tolerant of either plugin loading first. `SaveManager` is now a thin façade over this, so all SMSAndroids state reads/writes go straight to the pack. **ModForge stays generic — the dependency only ever points SMSAndroids → ModForge.** |
| [GiftUIBridge.cs](SMSAndroids/GiftUIBridge.cs) | ModForge → SMSAndroids (via GC2 signal) | Subscribes to GC2 signals `OpenGiftUI` / `OpenGiftStore`; packs open these UIs via a plain `EmitSignal` action. SMSAndroids owns the listener — ModForge just emits whatever signal string the pack names. |

**Ownership rule:** if it can be authored as pack data, it belongs in the pack. SMSAndroids-side code that duplicates pack-built GameObjects (levels, map buttons, dialogues, scenes, wallpapers, the navigator grid) has been deleted; don't reintroduce it.

---

## Build & runtime

- **Target framework:** .NET Framework 4.8, `OutputType=Library` → `SMSAndroidsCore.dll`.
- **Debug `OutputPath`:** `D:\Downloads\SkyrimM\spec\uTorrent\Starmaker Story 1.8E\BepInEx\plugins\` — Debug builds drop directly into the local game install.
- **Key references** (all under `..\Libraries\`): `BepInEx.dll`, `0Harmony.dll`, `Assembly-CSharp*.dll`, `GameCreator.Runtime.*.dll` (GC2), `Newtonsoft.Json.dll`, `Fullscreen.NanoSave.dll`, full `UnityEngine.*Module.dll` family.
- Manually decompiled GC2 references live under [`SMSAndroids/ReferenceClasses/`](SMSAndroids/ReferenceClasses) — **read-only documentation**, not compiled.
- No automated tests. Verification = launching Starmaker Story 1.8E with both DLLs + the `.smspack` installed.

## Scene gating & load chain

Each major file is a `[BepInPlugin]` `BaseUnityPlugin` (GUIDs under `treboy.starmakerstory.smsandroidscore.*`) with a `loaded*` flag set in `Update()` and reset on `GameStart`. Current chain:

1. **`Core.loadedCore`** — caches root transforms, loads bundles, registers Proxy Variables.
2. **`Characters.loadedBusts`** — needs `loadedCore` + **waits for ModForge's bust pass** (probes `bustManager.Find("AnisBase")`). Resolves the 21 character + 15 vanilla bust fields by name.
3. **`Places.loadedPlaces`** — needs `loadedCore` + **waits for ModForge's place pass** (`TryResolvePackBuiltPlaces` probes for a `_SecretBeach` level, retries each frame). Binds level/roomtalk fields by name-suffix; grafts SMSAndroids-side extras onto pack-built levels.
4. **`Dialogues.loadedDialogues`** — needs `Places.loadedPlaces`. Gift UI + HHTalk panel only.
5. **`Schedule.loadedSchedule`**, **`MainStory.loadedStory`**, then **`Minigames`**, **`ScheduleVisualizer`**.

`Core.loadedBases` = `loadedCore && loadedBusts && loadedDialogues && loadedStory && loadedPlaces`.

**Init-block convention:** these blocks re-run every frame until they complete. Any "wait for dependency" guard must be the **first** statement — side effects (Instantiate/clone) placed before a retry-return leak one copy per frame.

## Disk paths

- Bundles: `BepInEx\plugins\SMSAndroidsCore\Assets\{dialoguebundle,otherbundle,minigamebundle}`. `dialoguebundle` survives mainly for the **Proxy Variables** GNV asset; `otherbundle` for UI sounds + `ButtonTemplate`; `minigamebundle` for the massage minigame. (`characterbundle` is retired — the Anis HH NPC prefabs it carried are pack NPC placements now; the file can be dropped from the distribution.)
- Content paths under `Assets\` (`Core.itemsPath`, `Core.locationPath` for B1 overlays/SecretBeach extras, `Core.uiPath`, `Core.minigamePath`, `Core.bustPath` for the Solid cameo).
- **Saves** (per NanoSave slot): `%AppData%\..\LocalLow\Arvus Games\Starmaker Story\Saves\NANOSAVE_{slot:D4}\`
  - `SMSModForge_<packId>.json` — the single mod save file. ModForge owns the full lifecycle: sleep autosave to slot 1 (+ Monday backup to slot 2), manual-save copy to the chosen slot, plus slot-switch flushes. All SMSAndroids state lives here too.
  - `SMSAndroidsCore_Save.txt` — **retired.** `SaveManager` no longer reads or writes it; on first load of a slot it imports any legacy values into the pack (full import when no pack file exists yet, else just the SMSAndroids-only keys) and renames the file `SMSAndroidsCore_Save.migrated.txt`.

---

## File-by-file reference

### Plugin classes

**[Core.cs](SMSAndroids/Core.cs)** — foundation. Bundle loading, root-transform cache, GC2 variable bridge via reflection (`FindAndModifyVariable*`, `GetVariable*`), **Proxy Variables** system (`GetProxyVariable*` / `FindAndModifyProxyVariable*`, `SyncVanillaToProxyVariables` polling mirror), daily proxy reset, affection/gift glue (`IncrementAffectionForGiftIfLiked`, `SetAndSyncGiftVariable`), `FindInActiveObjectByName`. (The old `CreateModHeader` menu banner was removed — ModForge's "Mods" banner lists the pack, which is this mod's user-facing identity.)

**[SaveManager.cs](SMSAndroids/SaveManager.cs)** — thin façade over the pack store plus the after-sleep gameplay turnover. Static `Get/Set{String,Int,Float,Bool}` + `HasVariable` route through `ModForgeBridge` (packId `SMSAndroidsPack`), so existing call sites keep working while persistence lives entirely in the pack (the pack manifest declares every variable; numeric bounds like Affection 0–5 are enforced there). Still owns: the post-sleep turnover at the `afterSleepEvents`→`savedUI` gate (gift-shop counter + per-day flag resets — **no** disk writes; ModForge commits), and the one-time legacy `.txt` importer. (Voyeur-tier rebuild removed — pack-owned.) No more `defaultValues`, cache, file I/O, slot management, or NanoSave save-slot listeners (those moved to ModForge's `PackManualSaveSync`).

**[Schedule.cs](SMSAndroids/Schedule.cs)** — load-order latch ONLY (`loadedSchedule`, gating Minigames / ScheduleVisualizer / SaveManager's turnover). The entire location simulation it used to run is pack-owned: baselines via `Schedule_<Char>_Found` rules, per-day outings via `ScheduleDaily_<Char>_<Day>` rules, Harbor-Home roaming via `HHRoam`/`HHRelease` (parameterized over `HarborHome_VisitList`), Anis NPC visibility via `activeConditions` on the pose containers, shower audio via `HHShowerAudio`, fridge overlay via `HHFridgeOverlay`. Everything reads/writes the pack's `Location_<Char>` variables.

**[Places.cs](SMSAndroids/Places.cs)** — vanilla-level cache + SMSAndroids-side extras grafted onto **pack-built** levels. `TryResolvePackBuiltPlaces()` binds `secretBeach*`/`mountainLab*`/`giftShop*`/`harborHome*` level + roomtalk fields by name-suffix (retry-gated until ModForge builds them). Extras it still owns: bedroom `PlayerRoom_ButtonCanvas` graft, the HH living-room base-sprite sorting tweak + vanilla `Movies` subtree graft (cross-level clones aren't pack-expressible), weather toggles, bedroom swap after sleeping at HH, gift shop texture by build status, **mod shops/GiftStore** (`ActivateShop`, `AddItemToGiftStore`, visibility by `Gift_*` proxies), `CreateNewRoomTalk` (Hospital/Hotel/Trail extras), "House for Sale"→"Home" label swap on the **pack-built** Foundry radial button. (HH NPC slot Transforms, the ShowerGlass B1 overlay and the Solid cameo are pack content now — slot containers + `ShowerGlassOverlay` live on the HarborHome places, Solid on the `vanilla:67_Jap_ForestEntrance` extension.) **No map buttons, no navigation, no navigator grid** — all ModForge (`NavigatorRuntime` evaluates the pack's per-button conditions; `NavigatorGridSetup`/`NavigatorGridLayout` own the 6-column grid + extra nav row).

**[Characters.cs](SMSAndroids/Characters.cs)** — bust field resolution (by GO name, from ModForge-built busts) + `characterGiftLikesMap` + `AddBlinkingSpriteToBlinkObjects` (used by the massage minigame). The Anis HH NPC factory is gone — the NPCs are pack placements with the pack's own RandomChildActivator/FadeInSprite components.

**[Dialogues.cs](SMSAndroids/Dialogues.cs)** — Gift UI (`InitializeGiftUICanvas`, `AddGiftItem`, per-frame visibility), HHTalk panel (per-room character buttons), `dialoguePlaying`/`dialoguePlayingVanilla` flags (mirrored to `Checks_Dialogue-is-playing`). Each frame, `dialoguePlaying = ModForgeBridge.IsAnyDialoguePlaying() || dialoguePlayingVanilla` — **we read ModForge's generic query; ModForge no longer writes our fields.** All dialogue *content* lives in the pack.

**[MainStory.cs](SMSAndroids/MainStory.cs)** — slim story state: shared `SignalArgs` constants, per-day flags (`relaxed`, `actionTodaySB`). The old per-character dialogue dispatcher is gone — pack dialogues with conditions/actions replaced it. **The voyeur system is fully pack-owned** (eligible-target List variable + AddToList/RemoveFromList maintenance, DailyRandom lottery variable, "Random from list" pick); `VoyeurBridge` and all tier/pick code were removed. The three general lottery numbers are gone too — the pack's ScheduleDaily rules roll their own DailyChance.

**[Minigames.cs](SMSAndroids/Minigames.cs)** / **[MassageMinigame.cs](SMSAndroids/MassageMinigame.cs)** — massage minigame host + logic (unchanged by the migration; see code comments for character/variant selection, progression, persistence keys `Minigame_Massage_*`).

**[ScheduleVisualizer.cs](SMSAndroids/ScheduleVisualizer.cs)** — character-icon overlays on the World Map. `LocationToButtonMapping` (wildcards; `HarborHome*` → `Foundry/HarborHouseEntrance` — resolves the **pack-built** radial button, which ModForge names by bare place key). Icons from `Core.uiPath`.

**[Debugging.cs](SMSAndroids/Debugging.cs)** — dev-only GC2 inspection (dialogue graph dumps, variable monitors). Calls commented out in production.

### Bridges

**[ModForgeBridge.cs](SMSAndroids/ModForgeBridge.cs)**, **[GiftUIBridge.cs](SMSAndroids/GiftUIBridge.cs)** — see the table at the top. (`ModForgeSaveSyncer` was removed — `SaveManager` now reads the pack directly, so the pack→SaveManager mirror is obsolete.)

### MonoBehaviours / utilities

`BreastPhysics`, `DragSpriteController`, `LotionTrail`, `SqueezeContourFollower`, `SqueezeSpriteController` (shader-driven interaction effects; shaders in repo root `Sprites_*.shader`), `MassageMovementPattern` (minigame data POCOs), `TransformExtensions`.

### Removed in the migration (don't recreate)

`Scenes.cs`, `Wallpaper.cs`, `BustOutfitDescriptor.cs`, `BustPacks.cs`, `GridLayoutGroup.cs`, `ButtonHover.cs`, `RandomChildActivator.cs`, MainStory's dialogue runner + lottery numbers, Dialogues' prefab inventory + SFX-text system + `{var}` substitution + `CreateMusicPlayer`/audio handles, Places' `CreateNewPlace`/`CreateNewLevel`/map-button gating/`ClickMapButton`/HH slot Transforms/B1 overlays/Solid cameo, Characters' `CreateNPC` factory + particle-copy helpers, Schedule's entire location simulation (day schedules, HH roaming, NPC gating, shower audio). Their replacements live in SMSModForge.PackPlugin + the pack manifest.

---

## Conventions to follow when editing

- **Plugin pattern.** New persistent subsystems get their own `BaseUnityPlugin` + GUID, a `loaded<Name>` flag, a `Logger.LogInfo("----- <NAME> LOADED -----")` confirmation, and flag reset in the `GameStart` branch.
- **Guard first.** Inside `if (!loadedX && deps) { ... }` blocks, dependency-wait early-returns go before any side effects (the block re-runs per frame; clones placed earlier leak per retry).
- **Content goes in the pack.** New dialogues, scenes, places, wallpapers, music, SFX, busts, variables → author in SMSBustForge and export the `.smspack`. Only add C# here for behavior the pack format can't express — and prefer extending the pack format.
- **Persistence is pack-owned.** All state is a pack variable; `SaveManager.Get/Set` is just a façade over `ModForgeBridge`. To add persistent state, declare the variable in the pack manifest — there's no SMSAndroids-side save file or sync table to update anymore.
- **Surfacing state to vanilla GC2** goes through Proxy Variables (`Core.proxyVariables`); the asset's keys are authored in the Unity bundle project.
- **Affection clamping.** `SaveManager.SetInt` clamps `Affection_*` to `[0, 5]`. Don't bypass.
- **Daily reset semantics.** `DailyProc_` prefix (proxy) / `_Daily` suffix (SaveManager) reset automatically each in-game day.
- **Don't touch `ReferenceClasses/`.**
- **Comments policy:** comment only when the *why* isn't obvious.
