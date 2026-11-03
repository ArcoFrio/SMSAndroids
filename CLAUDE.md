# SMSAndroids — Project Reference

This file is auto-loaded by Claude Code into every conversation in this repo. It is a structural reference for the **Androids mod** for the Unity visual novel **Starmaker Story** (target version **1.8E**). Keep it accurate — update it whenever a class's responsibility, public surface, or load gating changes.

## The mod is the pack (read this first)

Since 2026-11-02 the Androids mod ships **entirely as a ModForge pack**: `SMSAndroidsPack.smspack`, authored with the ModForge editor (sibling repo `C:\Users\gabri\source\repos\SMSModForge`) and run by ModForge's generic plugin (**SMSModForge.PackPlugin**). The pack builds dialogues, scene CGs, wallpapers, music, SFX, places/levels, navigator and map buttons, busts/outfits, UIs, integration rules and its own variables — and, since ModForge 1.8.3, copies of the game's own objects and journeys between places. ModForge's own author is the author of this mod too: pack work does not wait on a ModForge release.

- **SMSAndroidsCore** (this repo, .NET 4.8 → `SMSAndroidsCore.dll`) is now **only the massage minigame**, which is out of use for now (the author's call). It is **not shipped**: the pack needs nothing from it. It stays C# because a minigame is the kind of thing a pack can't express. How everything else left it: [`docs/PLUGIN-TO-PACK-2026-11-02.md`](docs/PLUGIN-TO-PACK-2026-11-02.md).
- **The goal** (the author, 2026-11-02): no C# beside the pack, except something complicated like a minigame. Before adding C#, look for the pack way — a UI on one of the game's screens (`vanillaui:`), integration rules (with `forEach` / branches), place enter/exit hooks, the game's own components attached by name (ContentSizeFitter, UIHoverBounce), copies of game objects (`copyOf`), the Travel action. A general gap goes to `SMSModForge/FROM-PACK-TESTING.md`, written generically.

[`ModForgeBridge.cs`](SMSAndroids/ModForgeBridge.cs) is how the minigame reads and writes its pack variables (`Minigame_Massage_*`): typed `Get/Set` by reflection, so neither DLL references the other. **ModForge stays generic — the dependency only ever points SMSAndroids → ModForge.**

---

## Build & runtime

- **Target framework:** .NET Framework 4.8, `OutputType=Library` → `SMSAndroidsCore.dll`.
- **Debug `OutputPath`:** `D:\Downloads\SkyrimM\spec\uTorrent\Starmaker Story 1.8E\BepInEx\plugins\` — Debug builds drop directly into the local game install.
- **Key references** (all under `..\Libraries\`): `BepInEx.dll`, `0Harmony.dll`, `Assembly-CSharp*.dll`, `GameCreator.Runtime.*.dll` (GC2), `Newtonsoft.Json.dll`, `Fullscreen.NanoSave.dll`, full `UnityEngine.*Module.dll` family.
- Manually decompiled GC2 references live under [`SMSAndroids/ReferenceClasses/`](SMSAndroids/ReferenceClasses) — **read-only documentation**, not compiled.
- **Testing is in the game, in a test copy** — [`Tools/Game/README.md`](Tools/Game/README.md). Never the author's game or saves. `setup-test-game.ps1 -Build` puts Debug ModForge and SMSAndroids' **test bridge** ([`Tools/Game/TestBridge`](Tools/Game/TestBridge), a test-only plugin that reads the game's scene) into `Starmaker Story 1.8E - Androids test` — **without SMSAndroidsCore**, as players have it (`-WithPlugin` adds it, for the minigame). `launch.ps1` (own desktop, muted, test save only), `ingame_check.py` (startup, places, behaviour, hh, fixes, day, migrated, moved, dialogues, ui), `look_map.py` (1080p shots of the map and talk cards), `pack_logic.py` (static logic consistency), `bridge.py` for hand-driving.
- **The pack under test is the author's working folder, not this repo's copy.** They edit `C:\Users\gabri\source\repos\- AI Gen\sd-webui-forge-neo\output\- STARMAKER MOD-\SMSAndroidsPack`. Before any work or test, run `Tools/Game/export-pack.ps1`: it copies anything newer from there into `SMSAndroidsPack/` (add/replace only, minus `sync-exclude.txt`) and exports the `.smspack` with ModForge's own exporter. A pack fix made in JSON must go into the working folder too, or the next sync undoes it — and only while the ModForge editor is not open on it.
- **Generated pack parts:** `Tools/Game/pack_plugin_to_pack_2026_11_02.py` builds what moved out of the plugin (the Harbor Home talk cards, the world map's character icons, the random-events and wake-up rules, the Harbor Home copies). It is idempotent; its art comes from `map_portraits.py` / `map_badges.py`. Talk-card portraits the author edits live in the pack's `UI/TalkPanel/Portraits/` (starters with `WRITE_PORTRAITS=1`, never over an existing file).
- **What the pack is meant to do** is the mod before ModForge (`git show d42d17d:SMSAndroids/<file>`), minus the author's deliberate changes.

## Scene gating & load chain (the minigame)

Each file is a `[BepInPlugin]` `BaseUnityPlugin` (GUIDs under `treboy.starmakerstory.smsandroidscore.*`) with a `loaded*` flag set in `Update()` and reset on `GameStart`:

1. **`Core.loadedCore`** — caches the roots the minigame uses, loads `minigamebundle`.
2. **`Characters.loadedBusts`** — needs `loadedCore` + **waits for ModForge's bust pass** (probes `bustManager.Find("AnisBase")`). Resolves the character and vanilla bust fields by name.
3. **`Schedule.loadedSchedule`**, **`MainStory.loadedStory`**, then **`Minigames`**.

**Init-block convention:** these blocks re-run every frame until they complete. Any "wait for dependency" guard must be the **first** statement — side effects (Instantiate/clone) placed before a retry-return leak one copy per frame.

## Disk paths

- `BepInEx\plugins\SMSAndroidsCore\Assets\minigamebundle` and the `Minigame\` / `Busts\` content paths are the minigame's. Nothing else in that folder is read.
- **Saves** (per NanoSave slot): `%AppData%\..\LocalLow\Arvus Games\Starmaker Story\Saves\NANOSAVE_{slot:D4}\SMSModForge_<packId>.json` — the mod's single save file, ModForge's from start to end. Saves from before ModForge (`SMSAndroidsCore_Save.txt`) are no longer imported (the author, 2026-11-02).

---

## File-by-file reference

**[Core.cs](SMSAndroids/Core.cs)** — root-transform cache, scene load chain, the minigame bundle.

**[Characters.cs](SMSAndroids/Characters.cs)** — bust field resolution (by GO name, from ModForge-built busts) + `AddBlinkingSpriteToBlinkObjects`, both for the minigame.

**[Schedule.cs](SMSAndroids/Schedule.cs)** / **[MainStory.cs](SMSAndroids/MainStory.cs)** — load-order latches and shared `SignalArgs` constants for the minigame. Schedules, roaming and the story are the pack's.

**[Minigames.cs](SMSAndroids/Minigames.cs)** / **[MassageMinigame.cs](SMSAndroids/MassageMinigame.cs)** — massage minigame host + logic (out of use; progress in pack variables `Minigame_Massage_*` through `ModForgeBridge`, character from `Minigame_Massage_Character`, default Anis). `BreastPhysics`, `DragSpriteController`, `LotionTrail`, `SqueezeContourFollower`, `SqueezeSpriteController` (shader-driven effects; shaders in repo root `Sprites_*.shader`), `MassageMovementPattern` (data POCOs).

**[ModForgeBridge.cs](SMSAndroids/ModForgeBridge.cs)** — see above.

**[Debugging.cs](SMSAndroids/Debugging.cs)** — dev-only GC2 inspection (dialogue graph dumps, variable monitors). Calls commented out in production.

### Removed in the migration (don't recreate)

`Places.cs` (Harbor Home copies and wake-up → pack `copyOf` / Travel), `SaveManager.cs` (pack façade, legacy `.txt` importer), `TransformExtensions.cs`, `Dialogues.cs` (gift UI, Harbor Home talk panel, dialogue-playing mirror), `GiftUIBridge.cs`, `ScheduleVisualizer.cs` (world-map icons), `TestBridge.cs` (now `Tools/Game/TestBridge`, test-only), the affection-effect clone in `Core`, the Proxy Variables system, the C# gift store, `Scenes.cs`, `Wallpaper.cs`, `BustOutfitDescriptor.cs`, `BustPacks.cs`, `GridLayoutGroup.cs`, `ButtonHover.cs`, `RandomChildActivator.cs`, MainStory's dialogue runner + lottery numbers, Dialogues' prefab inventory + SFX-text system + `{var}` substitution + audio handles, Places' place/level/map-button building, Characters' `CreateNPC` factory, Schedule's location simulation. Their replacements live in SMSModForge.PackPlugin + the pack manifest.

---

## Conventions to follow when editing

- **Plugin pattern.** New persistent subsystems get their own `BaseUnityPlugin` + GUID, a `loaded<Name>` flag, a `Logger.LogInfo("----- <NAME> LOADED -----")` confirmation, and flag reset in the `GameStart` branch.
- **Guard first.** Inside `if (!loadedX && deps) { ... }` blocks, dependency-wait early-returns go before any side effects (the block re-runs per frame; clones placed earlier leak per retry).
- **Content goes in the pack.** New dialogues, scenes, places, wallpapers, music, SFX, busts, variables, UIs → author in the ModForge editor and export the `.smspack`.
- **Persistence is pack-owned.** All state is a pack variable, declared in the manifest; the minigame reaches its own through `ModForgeBridge`.
- **Surfacing state to vanilla GC2**: a pack action with `source: vanilla` writes the game's own variables.
- **Numeric bounds are pack-declared.** `Affection_*` carry `minValue`/`maxValue` in the manifest and `PackVariableStore` clamps on write.
- **Daily reset semantics.** A pack variable with `refreshMode: Daily` resets each in-game day.
- **Don't touch `ReferenceClasses/`.**
- **Comments policy:** comment only when the *why* isn't obvious.
