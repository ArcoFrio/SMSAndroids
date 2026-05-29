# SMSAndroids — Project Reference

This file is auto-loaded by Claude Code into every conversation in this repo. It is a structural reference for the **Androids mod** for the Unity visual novel **Starmaker Story** (target version **1.8E**). Keep it accurate — update it whenever a class's responsibility, public surface, or load gating changes.

The mod is hybrid:
- **Hardcoded plugin half** (this repo, C# → `SMSAndroidsCore.dll`) injected via BepInEx.
- **Asset half** (separate Unity project, not in this repo) shipped as AssetBundles loaded from `BepInEx/plugins/SMSAndroidsCore/Assets/`.

The base game is built on **Game Creator 2 (GC2)**. The mod heavily uses GC2 runtime types — `GlobalNameVariables`, `Conditions`, `Trigger`, `Signals`, `ButtonInstructions`, `Dialogue`, `Actor`, etc. Manually decompiled GC2 references live under [`SMSAndroids/ReferenceClasses/`](SMSAndroids/ReferenceClasses) and are **read-only** — never edit them, and don't treat them as part of the build (they're not compiled). They exist only to make GC2 internals legible.

---

## Build & runtime

- **Target framework:** .NET Framework 4.8, `OutputType=Library` → `SMSAndroidsCore.dll`.
- **Debug `OutputPath`:** `D:\Downloads\SkyrimM\spec\uTorrent\Starmaker Story 1.8E\BepInEx\plugins\` — i.e. Debug builds drop directly into the user's local game install.
- **Key references** (from [SMSAndroidsCore.csproj](SMSAndroids/SMSAndroidsCore.csproj), all under `..\Libraries\`):
  - `BepInEx.dll`, `0Harmony.dll` — modding harness.
  - `Assembly-CSharp.dll` / `Assembly-CSharp-firstpass.dll` — base game scripts.
  - `GameCreator.Runtime.{Core,Dialogue,Inventory,Quests,Stats}.dll` — GC2.
  - `Newtonsoft.Json.dll` — used by [MassageMinigame](SMSAndroids/MassageMinigame.cs) for pattern JSON.
  - `Fullscreen.NanoSave.dll` — game's save system; mod hooks its `SaveLoadManager` and the NanoSave UI.
  - Full `UnityEngine.*Module.dll` family.
- The repo has **no automated tests**. Verification means launching Starmaker Story 1.8E with the built DLL.

---

## Plugin architecture

Each major file is a separate **`[BepInPlugin]` `BaseUnityPlugin`** with its own GUID under `treboy.starmakerstory.smsandroidscore.*`. Unity instantiates each one independently at startup; coordination happens through static fields on `Core` and per-class `loaded*` boolean flags.

### Scene gating

The mod cares about two scenes:

- **`GameStart`** — main menu. Mod sets `loadedMenu`, injects a header text. Resets all `loaded*` flags on entry.
- **`CoreGameScene`** — actual gameplay. All initialisation runs here.

[Core.OnSceneLoaded](SMSAndroids/Core.cs) clears every `loaded*` flag whenever any scene loads, so reload safely.

### Load order (dependency chain)

Each plugin's `Update()` is a `if (!loaded* && deps satisfied) { init... loaded*=true; }` pattern. The chain is:

1. **`Core.loadedCore`** — needs `CoreGameScene` to be active. Caches root transforms (`bustManager`, `mainCanvas`, `level`, `roomTalk`, `coreEvents`, etc.), the `SaveLoadManager`, the current save slot, and initialises **Proxy Variables**.
2. **`Characters.loadedBusts`** — needs `loadedCore`. Builds every bust outfit and NPC. The legacy 9-arg `CreateNewBust(name, pathToCG, base, blink, mask, mouth, expression, hasMouth, hasExpression)` is now a thin wrapper that forwards to a new `CreateNewBust(BustOutfitDescriptor)` overload (single source of truth for the bust hierarchy mutation; the legacy path produces a byte-identical result).
2b. **`BustPacks.loadedPacks`** — needs `Characters.loadedBusts`. Scans `BepInEx\plugins\SMSAndroidsCore\BustPacks\*\bustpack.json` and builds each authored outfit through the descriptor overload of `CreateNewBust`. Pack-built busts are stored in `BustPacks.bustsByKey` (keyed by `outfit.key` from the manifest) and folded into `Characters.characterGiftLikesMap`. The manifest format is authored by the sibling **SMSBustForge** WPF editor (see `C:\Users\gabri\source\repos\SMSBustForge`).
3. **`Dialogues.loadedDialogues`** — needs `loadedCore` (and operates alongside Characters). Builds dialogue GameObjects from `dialoguebundle`, the gift UI, the HHTalk panel, sets up SFX text mappings.
4. **`Places.loadedPlaces`** — needs `loadedCore`. Creates new map levels (Mountain Lab, Secret Beach, Gift Shop, Harbor Home rooms), map buttons, mod shops.
5. **`Scenes.loadedScenes`** — needs `loadedCore`. Instantiates scene art GameObjects from PNGs in `Scenes/<Char>/...`.
6. **`Schedule.loadedSchedule`** — needs `loadedCore`. Initialises per-character location strings + Harbor Home roaming.
7. **`MainStory.loadedStory`** — needs Places + Characters + Dialogues. Picks the active voyeur tier and runs all the per-character dialogue logic via switch-on-location. Integrates most mod functions into the game.
8. **`Wallpaper.loadedWallpaper`** — needs `Core.loadedBases`. Adds wallpapers + buttons to the in-game PC.
9. **`Minigames.loadedMinigame`** — needs Core/Characters/Dialogues/MainStory/Places/Scenes/Schedule/Wallpaper. Instantiates the massage minigame from `minigamebundle`.
10. **`ScheduleVisualizer.loadedVisualizer`** — needs Core + Schedule. Adds character-icon overlays on the World Map.

`Core.loadedBases` is true whenever `loadedCore && loadedBusts && loadedDialogues && loadedStory && loadedPlaces && loadedScenes` — many systems gate their per-frame work on this.

### Sprawling `Update()` bodies

Most files do almost everything inside `Update()`:
- One-shot initialisation guarded by the `loaded*` flag.
- Per-frame UI gating (`buttonX.SetActive(condition)`).
- Per-frame proxy-variable sync (`Schedule` keeps `Location_<Char>` proxy strings in sync with mod state).
- Per-frame dialogue/scene state machines (`MainStory`).

---

## Asset bundles & disk paths

[Core.cs](SMSAndroids/Core.cs) loads four bundles from `BepInEx\plugins\SMSAndroidsCore\Assets\`:

| Field | File | Notes |
| --- | --- | --- |
| `characterBundle` | `characterbundle` | NPC prefabs. |
| `dialogueBundle` | `dialoguebundle` | GC2 dialogue prefabs + the `Proxy Variables` `GlobalNameVariables` asset. |
| `otherBundle` | `otherbundle` | UI / shared prefabs (`ButtonTemplate`, `UI_Select` clip, etc.). |
| `minigameBundle` | `minigamebundle` | Optional. Massage minigame visuals. The mod tolerates its absence. |

Other on-disk content paths (all relative to the game `exePath`, derived in `Core.Awake`):

- `Core.assetPath` = `BepInEx\plugins\SMSAndroidsCore\Assets\`
- `Core.audioPath` = `...\Audio\`
- `Core.minigamePath` = `...\Minigame\` (massage `Patterns/*.json` lives under `Minigame\Patterns`)
- `Core.bustPath` = `...\Busts\` (with `Busts\Nikke\` and `Busts\Solid\` for special art)
- `BustPacks.PacksRoot` = `BepInEx\plugins\SMSAndroidsCore\BustPacks\` (one subfolder per pack, each containing `bustpack.json` + `Sprites/<Character>/*.PNG` + optional `Particles/*.json`)
- `Core.itemsPath` = `...\Items\` (gift shop item PNGs)
- `Core.locationPath` = `...\Locations\` (level art + masks)
- `Core.scenePath` = `...\Scenes\` (story scene art organised per-character)
- `Core.uiPath` = `...\UI\` (e.g. `Icon{Char}.png` for the World Map visualizer)
- `Core.wallpaperPath` = `...\Wallpaper\`

Save files are stored **outside** the plugin folder, at `%AppData%\..\LocalLow\Arvus Games\Starmaker Story\Saves\NANOSAVE_{slot:D4}\SMSAndroidsCore_Save.txt` — see [SaveManager.GetSaveFilePath](SMSAndroids/SaveManager.cs).

---

## File-by-file reference

> ⚠️ Several files are too large for `Read` in one shot. Use `offset`/`limit` or `Grep` for targeted lookups: [Characters.cs](SMSAndroids/Characters.cs) (~47k tokens), [Dialogues.cs](SMSAndroids/Dialogues.cs) (~64k), [MainStory.cs](SMSAndroids/MainStory.cs) (~266 KB), [Places.cs](SMSAndroids/Places.cs) (~40k), [Debugging.cs](SMSAndroids/Debugging.cs) (~32k).

### Plugin classes (each is a `BaseUnityPlugin`)

**[Core.cs](SMSAndroids/Core.cs)** — `treboy.starmakerstory.smsandroidscore.core` — version source of truth (`pluginVersion`).
The mod's foundation:
- Loads the four asset bundles at `Awake`.
- Resolves all `CoreGameScene` root transforms once and exposes them as static fields (`bustManager`, `mainCanvas`, `level`, `roomTalk`, `coreEvents`, `effects`, `audioPlayer`, etc.).
- Caches frequently-needed GameObjects: `baseBust`, `affectionIncrease`, `levelBeach`, `mainCamera`, `savedUI`, `saveLoadSystem`, `introMomentNewGame`, `toggleRepeatableBedEvents`, etc.
- **GC2 variable bridge** via reflection: `FindAndModifyVariable{Bool,Double,GameObject}`, `GetVariable{Bool,Number}`, `AddGameObjectToLocalListVariables`. These reach into `GlobalNameVariablesManager` to read/write game-defined GC2 variables.
- **Proxy Variables system**: a custom `GlobalNameVariables` asset (`"Proxy Variables"`, loaded from `dialogueBundle`) is registered with `GlobalNameVariablesManager` so dialogue/conditions can reference mod state. `Core.proxyVariables` exposes the asset; `GetProxyVariable{Bool,Number,String}` and `FindAndModifyProxyVariable{Bool,String}` are the API. `SyncVanillaToProxyVariables` polls (every 1.5s) and mirrors:
  - Vanilla GC2 booleans (gift inventory, e.g. `Beer` → `Gift_Beer`) — see `booleanVariableMappings`.
  - Vanilla numerics (`Cash` → `Gameplay_Cash`) — see `numericVariableMappings`.
  - SaveManager ints (`Affection_<Char>`) — see `saveManagerIntMappings`.
  - SaveManager bools (`Affection_Anis_Seen2/3`, `Gift_*`, `HarborHome_Bought/FirstVisited`) — see `saveManagerBoolMappings`.
- Daily reset: `RefreshDailyProxyVariables` sets every proxy var beginning with `DailyProc_` back to `false`.
- Affection/gift glue: `IncrementAffectionForGiftIfLiked(giftName, charName)` consults `Characters.characterGiftLikesMap` and bumps `Affection_<Char>` via `SaveManager`. `SetAndSyncGiftVariable` sets a `Gift_*` proxy + persists it to `SaveManager`.
- Coroutine helpers run via a found `Debugging` instance: `EmitSignalDelayed`, `EmitSignalGameObjectDelayed` (full enable→disable→enable→signal sequence used to swap busts mid-scene), `ChangeOutfitDelayed`.
- `CreateModHeader` adds a "Androids Mod {version} for 1.8E" string on the main menu — turns red if the trailing version segment doesn't match the original menu text (sanity check that you're targeting the expected base game version).
- Utility: `FindInActiveObjectByName(name)` searches every `Transform` (including inactive). `GetRandomNumber`, `GetRandomFloat`. `DisableAllActiveBustChildren`.

**[Debugging.cs](SMSAndroids/Debugging.cs)** — `treboy.starmakerstory.smsandroidscore.debugging` — research / dev-only inspection.
Exists to let me reverse-engineer the base game's GC2 nodes at runtime. **Calls are commented out in production**; uncomment in `Core.Update` when investigating. Methods:
- `PrintConditionsAndTriggers`, `PrintButtonInstructions`, `PrintToggleOnValueChanged` — print all the GC2 conditions/instructions/listeners attached to a GameObject's components.
- `PrintDialogueComponentInfo` / `PrintDialogueComponentDeep` — dump full GC2 `Dialogue` graphs (nodes, choices, conditions, instructions). Recursive walk via reflection.
- `PrintActionsComponentInfo`, `PrintSingleInstruction`, `GetFieldValueDisplayString` — deep instruction logging.
- `PrintAllActorExpressions(Actor)` / `PrintAllActorExpressionsFromDialogue` — list all expressions defined for a GC2 actor.
- `PrintAllGlobalNameVariables`, `PrintProxyVariables`, `PrintAllGlobalNameVariablesDelayed` — runtime dumps of GC2 variable sets.
- `PrintAllSignals` — enumerates registered `ISignalReceiver` instances.
- `FindAllSetActiveInstructionsInDialogues` — bulk searches every dialogue for `SetActive` instructions (used to map scene activations).
- `MonitorGlobalVariableChanges` / `Start/StopMonitoringGlobalVariables` — coroutine that watches a variable for changes.
- `GetLocalListVariablesValues` / `PrintLocalListVariablesValues` — inspect a `LocalListVariables` component.
- `ReloadAmberBustTextures` — texture hot-reload helper.
- `LogToDialogueFile` — writes the dump to a file instead of console (large outputs).

**[SaveManager.cs](SMSAndroids/SaveManager.cs)** — `treboy.starmakerstory.smsandroidscore.savemanager`. A single-instance plugin.
Persistent mod state, separate from the vanilla save:
- `defaultValues` is the schema + defaults for everything mod-tracked: `DailyProc_*`, `GiftShop_*`, `HarborHome_*`, `MountainLab_*`, `SecretBeach_*`, `Affection_*` (clamped 0–5 in `SetInt`), `Affection_Anis_Seen{1,2,3}`, `Event_Seen*`, `Voyeur_Seen*`, `Gift_*`, `Minigame_Massage_*` (legacy `Played`/`Highscore` plus per-character `<Char>_Level`/`<Char>_Highscore`), `Wallpaper_Current`, `Mod_Version`. **Add new persistent variables here.**
- Static API: `GetString/SetString/GetInt/SetInt/GetFloat/SetFloat/GetBool/SetBool`, `DeleteVariable`, `HasVariable`, `GetAllVariables`, `ClearAllVariables`, `ResetToDefaults`, `RefreshDailyVariables`, `AnyBoolVariableWithNameContains`, `CountBoolVariablesWithNameContains`.
- Per-frame: when the player sleeps (`Core.afterSleepEvents` + `Core.savedUI` active), runs the daily turnover — bumps `GiftShop_BuildCounter`, sets `HarborHome_Slept` from current room state, autosaves to slot 1 (and slot 2 on Monday/Day 1), re-rolls all four lottery numbers, resets `MainStory.relaxed`/`actionTodaySB`, rebuilds `MainStory.voyeurTargetsLeft`, calls `Places.UpdateGiftShopTextureBasedOnBuildStatus`, schedules `Schedule.UpdateScheduleForDay` via `Invoke`.
- New-game detection (`Core.introMomentNewGame.activeSelf`) → `ResetToDefaults`.
- **NanoSave integration:** finds the `NanoSave` GameObject, walks `Content > Content > List > Viewport > Content`, and attaches click listeners to:
  - `EmptySaveSlot(Clone)` → `SaveToNextAvailableSlot` → after 0.2s `SaveToLatestSlot` (finds the most-recently-modified `NANOSAVE_*` folder and copies the source slot's mod save into it).
  - Each `SaveSlots(Clone)` → parses "Save XXXX" label, attaches overwrite listener.
  - Listener stacking is prevented by `SaveMenuEmptySaveSlotMarker` / `SaveMenuOverwriteSlotMarker` components.
  - Source slot for copy: slot 1 if new game OR autosave already triggered this session, otherwise the currently loaded slot.
- File format: plain text `key=value` per line, with header `saveSlot=` and `lastSaved=`. Loading parses against the type in `defaultValues`.
- Saves are gated to `5_MyRoom` being active (you can only save from the player's room).

**[Schedule.cs](SMSAndroids/Schedule.cs)** — `treboy.starmakerstory.smsandroidscore.schedule`.
Where each character is supposed to be on each in-game day:
- Per-character state triplets: `<char>DefaultLocation`, `<char>Location`, `<char>HHLocation`, `<char>HHOutfit` for each of Amber, Claire, Sarah, Anis, Centi, Dorothy, Elegg, Frima, Guilty, Helm, Maiden, Mary, Mast, Neon, Pepper, Rapi, Rosanna, Sakura, Tove, Viper, Yan, plus `snek*`.
- `SetDay{1..5}Schedule` switch each character to a possible alternate location keyed off `Voyeur_Seen<Char>`, lottery numbers (`MainStory.generalLotteryNumber{1,2,3}`), special unlock flags, and `Places.GetBadWeather()`. Some Anis routes also gate on `Affection_Anis_Seen{1,2}` + `Affection_Anis` thresholds. `UpdateScheduleForDay()` dispatches based on `Schedule.day` and bumps `scheduleVersion` (watched by `ScheduleVisualizer`).
- `GetCharacterLocation(name)` / `SetCharacterLocation(name, loc)` / `AnyCharacterLocationStartsWith(prefix)` are the public lookup API.
- **Harbor Home roaming** (`#region Harbor Home Roaming System`):
  - 21 HH-eligible characters (Amber, Claire, Sarah + 18 Nikkes).
  - 6 HH rooms (Bathroom, Bedroom, Closet, Kitchen, LivingRoom, Pool); each has an `NPCs` child whose direct children are the position slots.
  - Per-character timers (`hhNextRoamTime`) reroll every 15–45s. First pass runs 3s after `Core.loadedBases`, only for characters with `HarborHome_Visit_<Char> == true` and currently at default location or already in HH.
  - Position picking respects: bathroom → must go to closet next, pool → must go to bathroom next, no two characters share a slot. Falls back to `HarborHomeLivingRoom` when no valid slot exists. Skips reassignment while the player has the matching room open (`currentRoomLevel.activeSelf`).
- Per-frame Anis NPC bedroom/closet/kitchen/pool/bathroom variant gating: a separate `RandomChildActivator`-driven slot exists for each combination of `(position, outfit)` — the mod toggles them based on `anisLocation` and `SaveManager.GetString("HarborHome_Outfit_Anis")` (`"Default"` vs `"Swim"`); `anisNPCHHShowerNaked` is the bathroom variant.
- Audio gating for `Dialogues.audioShower` / `audioShowerQuiet` based on which HH room is active vs. where Anis is.
- Mirrors all `<char>Location` strings into the proxy variable `Location_<Char>` (string).

**[ScheduleVisualizer.cs](SMSAndroids/ScheduleVisualizer.cs)** — `treboy.starmakerstory.smsandroidscore.schedulevisualizer`.
Renders character-icon overlays on the World Map:
- `LocationToButtonMapping` — schedule location string → `District/RadialButtonName` (with wildcard support: `HarborHome*` → all HH locations map to a single button). Locations that shouldn't display map to `null` or to `Seaside/Beach` as a stub.
- `RadialToDistrictMapping` — radial button parent → district button name (some have spaces or different names: `TheLine` ↔ `The Line`, `Shopside` ↔ `Shoppingdistrict`, `NeonRow` ↔ `Nightlife`, `Foundry` ↔ `Harbor`).
- Character icons loaded from `Core.uiPath` / `Icon{Char}.png` into `characterIconCache`.
- Two overlay flavours: **radial** (right of button, rounded right + circular cutout on left) and **district** (below button, rounded bottom + circular cutout on top). Both are procedurally drawn into a `Texture2D`. District overlays pre-cache 4 sprite/height pairs (1–4 rows) for dynamic resizing.
- `UpdateAllCharacterIcons` is invoked each time `World_Map` becomes active. Watched via `Schedule.scheduleVersion`.
- Hover handler `SetAsLastSibling`-promotes a district button so its overlay renders on top.

**[Places.cs](SMSAndroids/Places.cs)** — `treboy.starmakerstory.smsandroidscore.places`.
The world. Caches every vanilla level (5_MyRoom through 138_HikingPath_Start) and every vanilla room-talk GameObject, then constructs new ones via `CreateNewPlace(index, name, pathName, buttonText, parallaxStrength)`:
- New levels (numeric IDs 900–930): `SecretBeach`, `MountainLab`, `MountainLabCorridorNikke{1,2}`, `MountainLabRoomNikke<Char>` × 18 Nikkes, `GiftShop`, `GiftShopInterior`, `HarborHome{LivingRoom,Bedroom,Bathroom,Closet,Kitchen,Pool}`, `HarborHouseEntrance`. Each gets a map button (`Navigator/MapButtons/{id}_{name}`), a level GameObject (`5_Levels/{id}_{name}`), and a roomtalk node (`Core.roomTalk/{name}`).
- `CreateNewLevel` clones `14_Beach`, swaps the base/secondary/mask sprites from `Core.locationPath`, strips its `NPCs` children + `Trigger`, and configures parallax strength. `keepAudioAndParticles` decides whether the audio source survives (only for SecretBeach + GiftShop).
- `SetupMapButtonsGrid` replaces the navigator's layout group with the custom [GridLayoutGroup](SMSAndroids/GridLayoutGroup.cs) (6 columns) so map buttons flow into a second row when needed; `extraNavRow` is a duplicated Navigator strip that activates when row 2 is in use.
- Per-frame map-button gating (lots of `buttonX.SetActive(condition)`), with conditions covering `SecretBeach_UnlockedLab`, `Voyeur_Seen<Char>`, `GiftShop_BuildCounter >= 2`, `HarborHome_Bought`, current active level. New buttons whose hidden first child becomes active call `ClickMapButton(roomTalk, index, musicName=null)` to drive the vanilla `TransferScene` trigger and (optionally) swap music under `12_AudioPlayer`.
- Harbor Home plumbing: per-room `<room>NPC<Position>` empty Transforms appended to the room's `NPCs` child; bedroom gets a copy of `5_MyRoom`'s `Player_Room_Buttons`; kitchen/bathroom have B1 overlays (`HHomeKitchenB1.PNG`, `HHomeBathroomB1.PNG`) toggled with Anis position. After sleep, if `HarborHome_Slept` is true, swaps `5_MyRoom` for the HH bedroom on next scene load (`harborHomeBedroomSwapApplied`).
- Secret Beach: extra layered children — `Sky`, `Flash`, `Gatekeeper`, `Portal` — instantiated from a child + retextured. `Gatekeeper` rotates every frame while the level is active. Disables `Disable-Specific-RNGEvents` while inside HH and restores from `Core.toggleRepeatableBedEvents` toggle when leaving.
- `solid` — a special hidden sprite added to `levelForest` ("Solid Snake" cameo via `solidSnake`) with a custom mask material.
- Weather (`weatherInside{Rain,Snow}`, `weatherOutside{Rain,Snow}`) toggled per-room based on GC2 vars `rainy-day` / `snowy-day`.
- **Mod shops:** `InitializeModShops()` creates `ModShops` container + `GiftStore` (cloned from vanilla `GeneralStore`, emptied, `CloseStore` button replaced with a stock Unity Button → `DisableModShops`). `AddItemToGiftStore(name, price, image)` clones the `Beer` template and re-skins. `UpdateGiftStoreItemVisibility` (called every 0.5s while open) gates each item by its `Gift_<Item>` proxy variable. `UpdateGiftShopTextureBasedOnBuildStatus` swaps the level art between `GiftShop.PNG` and `GiftShopAlt.PNG` based on `GiftShop_BuildCounter`.
- Harbor Home Entrance radial button is added to `World_Map > Canvas > Core > Radial_Buttons > Foundry`. Click handler emits the `TransferScene` flow with `HarborHomeMusic`. Label changes from `"House for Sale"` to `"Home"` once `HarborHome_Bought` is true.
- Static helpers: `GetBadWeather()`, `ClickMapButton`, `CreateNewLevel`, `CreateNewRoomTalk`, `SetNewLevelSprite`, `ActivateShop`, `AddItemToGiftStore`, `CreateNewPlace`.

**[BustOutfitDescriptor.cs](SMSAndroids/BustOutfitDescriptor.cs)** — pure data POCOs (`BustOutfitDescriptor`, `JiggleParamsValues`, `ParticleSpec`) consumed by `Characters.CreateNewBust(BustOutfitDescriptor)`. Mirrors the JSON shape produced by SMSBustForge. Paths in the descriptor are absolute (or relative-to-exe) — the same form `File.ReadAllBytes` accepts. `Jiggle == null` is the sentinel meaning "inherit the material defaults from `Core.baseBust`", which is the behaviour every hardcoded `CreateNewBust(...)` call site relies on.

**[BustPacks.cs](SMSAndroids/BustPacks.cs)** — `treboy.starmakerstory.smsandroidscore.bustpacks`. Pack loader plugin. Scans `BepInEx\plugins\SMSAndroidsCore\BustPacks\*\bustpack.json`, deserialises each manifest with `Newtonsoft.Json` (`JObject` walk, no contract types needed), and calls `Characters.CreateNewBust(desc)` per outfit. Output goes into `BustPacks.bustsByKey` (keyed by `outfit.key`). Gift likes from the manifest are folded into `Characters.characterGiftLikesMap`. Runs once per `CoreGameScene`, gated on `Characters.loadedBusts`; resets on `GameStart`. Tolerates a missing `BustPacks/` folder and continues on bad manifests (logged, not thrown).

**[Characters.cs](SMSAndroids/Characters.cs)** — `treboy.starmakerstory.smsandroidscore.characters`.
Bust/NPC factory. The whole top of the file is `public static GameObject` declarations, one per outfit/state per character. Pattern per character:
- Base bust + outfit variants: e.g. Anis has `anis`, `anisCoatless`, `anisTopless`, `anisSwim`, `anisSwimWet`, `anisSwimSlip`, `anisNaked`. Other Nikkes follow `<char>{,Underwear,Coatless,Topless,Swim,SwimSlip,SwimShirtless,SwimWet,Naked}` patterns (not every variant exists for every character).
- Anis additionally has a full Harbor Home NPC matrix: `anisNPCHH{Bedleft,Bedright,Changingleft,Changingright,Couchleft,Couchright,Fridge,Sink,Tanningleft,Tanningright,Shower}` — each has `Default` and `Swim` variants (Shower has `Naked`). These are the slots Schedule swaps in/out per-frame.
- Side characters: `adrian`, `ameliaSwim`, `anna`, `doctorFrost`, `emmaSwim`, `gabriel`, `isabella`, `katarina`, `kate`, `masterZhen`, `mobsterBlonde`, `river`, `samSwim`, `sofia`, `toni`. Cameo: `solidSnake`.
- **`characterGiftLikesMap`** maps a character name to the list of gift names that grant affection when given. Currently only `"Anis"` is populated.
- `CreateNewBust(name, pathToCG, baseSprite, blinkSprite, maskSprite, mouthSprite, expressionSprite, hasMouth, hasExpression)` — constructs a full bust prefab from on-disk PNGs.
- `CreateNPC(assetName, parent, displayName, copyParticles=false)` and `CreateNewNPC(name, pathToCG, spritePath, maskSpritePath, localPos, localScale, particlePos, parent)` — sister factories for NPC characters.
- Helpers: `AddBlinkingSpriteToBlinkObjects`, `CopyParticleSystemToParticleObjects`, `AddFadeInSpriteToBlinkParents`.

**[Dialogues.cs](SMSAndroids/Dialogues.cs)** — `treboy.starmakerstory.smsandroidscore.dialogues`.
The most sprawling file. Holds:
- **Speech skin overrides** (`overrideSpeechSkin{Blue,Green,Pink,Yellow}`).
- All the dialogue GameObjects + their per-scene children + activator/finisher/mouthActivator/spriteFocus markers — e.g. `amberDefaultDialogue`, `amberDefaultDialogueScene{1..5}`, `amberDefaultDialogueDialogue{Activator,Finisher}`, etc. Same pattern for every other scheduled or event dialogue (`claireDefaultDialogue`, `anisAffection0{1,2,3}Dialogue`, `anisChillToplessDialogue`, `<char>EventXyzDialogue`, the SecretBeach `sBDialogueMain*` story dialogues, etc.).
- **Audio**: `audioShower`, `audioShowerQuiet` — on `Core.audioPlayer`. Toggled by `Schedule.cs` based on Anis's HH location vs. visible HH room.
- **Bad-weather dialogue** (`badWeatherDialogue{,Activator,Finisher}`).
- **Gift UI** (`giftUI`, `giftItemTemplate`, `giftVanillaMap`) — clones the vanilla gift-give popup; `AddGiftItem(name, displayName, isVanillaGNV, textureFileName)` registers a new item button. `UpdateGiftUIVisibility` runs per-frame.
- `dialoguePlaying` (mod dialogues), `dialoguePlayingVanilla` (any vanilla dialogue active under `roomTalk`) — both mirror to proxy variable `Checks_Dialogue-is-playing`.
- **HHTalk panel** (`InitializeHHTalkPanel`, `UpdateHHTalkPanel`, `PopulateHHTalkButtons`, `ConfigureHHTalkButton`, `GetCharacterBustSprite`, `GetDefaultBustForCharacter`, `GetHHTalkRoomNameFromLocation`, `CreateRoundedRectSprite`) — overlay shown when in a Harbor Home room, listing characters present and letting the player select who to talk to.
- **SFX system** (`SFXMapping`, `textToSFX`, `CreateSFX(textPattern, clipName, volume)`, `GetRandomAudioClipForSFX`) — text-pattern → audio clips lookup used during dialogue line playback (mostly driven from `MainStory.OnDialogueLineStart`).
- **Music** (`CreateMusicPlayer(assetName)`).
- **Variable substitution** in dialogue text (`ProcessTextWithVariables` / `ProcessGlobalVariables` / `ProcessSaveManagerVariables` / `GetGlobalVariableValue` / `GetSaveManagerVariableValue` / `ConvertValueToString`) — supports `{var}` placeholders that resolve from GC2 globals or the mod SaveManager.
- **Actor/expression utilities** (`GetActorOverrideSpeechSkinValue` / `SetActorOverrideSpeechSkinValue`, `GetAllActorExpressions`, `AddExpressionSetInstructionToOnStart`, `ColorFromBytes`, `AddActorColorToSpeechUI` overloads, `AddActorColorsToSpeechUI` overloads).
- `CreateNewDialogue(bundleAsset, roomTalk)` — instantiates a dialogue prefab from `dialogueBundle` under a roomtalk parent.

**[Scenes.cs](SMSAndroids/Scenes.cs)** — `treboy.starmakerstory.smsandroidscore.scenes`.
Pure GameObject inventory of "scene art" full-screen images shown during dialogues:
- Per-character event scenes: `<char>Event<Place>Scene<NN>` (e.g. `anisEventMallScene01`).
- Per-character story scenes: `amberStorySecretbeachScene01`, etc.
- Anis-specific Affection arcs (`anisAffection0{1,2,3}Scene{01..05}`), HH bedroom/bathroom routes (`anisHHBedroom01Scene{01..05}`, `anisHHBathroom01Scene{01..10}`), and the chill-topless route (`anisChillToplessScene{01..06}`).
- Voyeur scenes for every Nikke (`<char>VoyeurSecretbeachScene{01..04}`).
- Built via `CreateNewPicScene(name, pngPath)` — no dialogue logic, just the visual GO.

**[MainStory.cs](SMSAndroids/MainStory.cs)** — `treboy.starmakerstory.smsandroidscore.mainstory`.
The story's brain. Drives every scheduled / event / voyeur dialogue:
- Pre-built `SignalArgs` for everything: `blinkSignal`, `dialogueStartSignal` / `dialogueEndSignal`, `drinkSignal`, `fadeUISignal`, `fadeIn{,Black}Signal`, `fadeOut{,Black}Signal`, `flashSignal`, `forceEnableUISignal`, `kissSignal`, `whiteFlashNoSoundBlackSignal`.
- Voyeur tier system: `starterVoyeurTargets` (Anis, Neon, Rapi) → `gSVoyeurTargets` (Yan, Centi) → `fullVoyeurTargets` (all 18 Nikkes). Tier promotion gated on `MountainLab_GKExplanation` and `GiftShop_BuildCounter >= 2`. `voyeurTargetsLeft` rebuilt each day. `AllStarterVoyeurTargetsFound` / `AllGSVoyeurTargetsFound` are the gates.
- Lottery numbers: `generalLotteryNumber1/2/3` and `voyeurLotteryNumber` — re-rolled by `SaveManager` on scene load and after sleep. Used by `Schedule` and `MainStory` to decide alternate locations and event triggers.
- State: `currentActiveBust`, `currentActiveBustMBase`, `currentActiveDialogue`, `currentActiveDialogueSpriteFocus`, `actionTodaySB` (Secret Beach lock), `voyeurDialoguePlaying`, `relaxed`, `snekIsSolid`, `lastEvaluatedLevel`, `evaluatingLevelDialogue`, `amberDefaultDiagQueued`, `diagBusts`.
- The bulk of the file is the `Update()` body — a giant series of `switch (Schedule.<char>Location)` blocks that, per character: detect when the matching level is active and the right preconditions hold, queue the right dialogue (`StartDialogueSequence` / `StartDialogueSequenceQueue` / `StartDialogueSequenceVanilla` and their delayed/finisher counterparts), activate the right `Scenes.*` sprite when each `Scene{N}` child of the dialogue GO is hit, set affection / `Event_*` / `Voyeur_*` save flags at the right beat, and update Schedule for the day after dispatching.
- Dialogue runner methods: `StartDialogueSequence(diag)`, `StartDialogueSequenceDelayed`, `StartDialogueSequenceQueue`, `PlayDialogueStep`, `EndDialogueSequence`, `FinishStep` — and a parallel `*Vanilla` family for dialogues that fire through the vanilla pipeline rather than through the mod's framework.
- Bust helpers: `ChangeActiveBust`, `ChangeBustSortingOrder`, `SpriteFocusChange`, `GetBustForActor(actorName)`, `Fade` (alpha tween coroutine).
- Voyeur logic: `ChooseVoyeurTarget()`, `ProcessCurrentDialogueLine`.
- **Per-line dialogue hook**: `OnDialogueLineStart(nodeID)` is the central per-line callback; calls `ProcessSFXTriggersForText(text, nodeID)` (text → audio via `Dialogues.textToSFX`), `GetSizeMultiplierAtPosition(text, position)`, `PlaySFXWithDelay`, etc. `GetCurrentSpeakingActor` / `GetCurrentActorExpression` query GC2's current dialogue state.

**[Wallpaper.cs](SMSAndroids/Wallpaper.cs)** — `treboy.starmakerstory.smsandroidscore.wallpaper` (note: `pluginName` is set to `... - SaveManager` — that's a copy-paste typo, the GUID is what BepInEx actually uses).
Adds extra wallpapers to the in-game PC:
- Hardcoded set: `AnisSwimsuit` (`WallpaperAnis1.PNG`), `DorothySwimsuit` (`WallpaperDorothy1.PNG`), `HelmSwimsuit` (`WallpaperHelm1.PNG`), `SolidGearOfMetal` (`Solid.PNG`).
- Each is gated by an `Event_Seen<Char><Place>01` SaveManager flag, so wallpapers unlock when the corresponding scene plays.
- `CreateWallpaper(name, texturePath)` clones the base wallpaper + button, hooks a click listener that sets the `Wallpaper` GC2 variable + `SaveManager.Wallpaper_Current`, plays a `UI_Select` SFX from `otherBundle`. Auto-deactivates the new button.
- Per-frame: keeps the active wallpaper consistent — if multiple are active, deactivates all except `Wallpaper_Current`; if exactly one is active and differs from the saved value, updates the save.

**[Minigames.cs](SMSAndroids/Minigames.cs)** — `treboy.starmakerstory.smsandroidscore.minigames`.
Massage minigame instantiation. Loads a single unified prefab `MinigameMassage` from `minigameBundle` (the prefab carries Canvas, Characters/<Char>/<Variants>, Hands with their `SqueezeContourFollower`+`LotionTrail` already attached, and `MassageBackground` — see [MassageMinigame_Hierarchy.txt](SMSAndroids/MassageMinigame_Hierarchy.txt)). Wires the world-space canvas's camera, ensures a `MassageMinigame` component is on the root, and starts disabled. All character / variant / hand-target wiring is now handled by `MassageMinigame.OnEnable` (see below).
Public entry: `StartMinigame(GameObject)` runs the standard fade-in-black → enable → fade-out coroutine. Set the proxy variable `Minigame_Massage_Character` (string) before calling so the minigame knows which character to display.

### Custom MonoBehaviours / runtime utilities

**[MassageMinigame.cs](SMSAndroids/MassageMinigame.cs)** — Zone-based, mouse-button-gated up-and-down stroke rhythm minigame.
- States: `Idle`, `StartMenu`, `Countdown`, `Playing`, `Results`. Stroke ratings: `Perfect/Good/OK/Miss`. Final ranks: `S/A/B/C/D`.
- Patterns loaded from `Core.minigamePath/Patterns/*.json` via `Newtonsoft.Json` (`MassagePatternCollection`). See [SMSAndroids/SamplePatterns/massage_patterns.json](SMSAndroids/SamplePatterns/massage_patterns.json) for an authored example.
- Optional UI from `minigameBundle` (sprites `ZoneHighlight`, `Cursor`, `MinigameBackground`, `StartMenuBackground`, `CloseButton`); falls back to a procedurally-built UI if absent (`_proceduralUI` flag).
- Public API: `LoadPatterns()`, `LoadVisualAssets()`, `StartMinigame()`, `StopMinigame()`.
- **Character & variant selection (OnEnable):** reads proxy variable `Minigame_Massage_Character` (string, fallback `"Anis"`) to pick the active GO under `Characters/`. Within that character, picks a variant by sibling index using SaveManager int `Minigame_Massage_<Char>_Level` (0-based). All other characters and other variants are deactivated. Level >= variant count → **sandbox mode**: a random variant is picked and progression doesn't advance. After each completed round, `ApplyProgression` increments the level only if the *average* stroke rating (Perfect=3 / Good=2 / OK=1 / Miss=0, then banded) is `Good` or higher AND the saved level still equals the variant played. New variants added to the prefab are picked up automatically because the variant count is read from sibling count at runtime.
- **Hand re-targetting (OnEnable):** `WireHandsToActiveVariant` re-points `HandLeft`/`HandRight`'s `SqueezeContourFollower.targetSprite` at the active variant's `SpriteRenderer`, calls `RefreshMask()`, and lazily wires bundle assets (`LotionTrailMAT`, `Hand squeeze`/`Hand float` sprites) if the prefab didn't carry them.
- **Results display:** `ShowResults` shows the average word rating in the breakdown alongside the per-rating counts and best combo, in addition to the existing S/A/B/C/D rank.
- **Persistence:** legacy keys `Minigame_Massage_Played` / `Minigame_Massage_Highscore` still updated. Per-character keys `Minigame_Massage_<Char>_Level` and `Minigame_Massage_<Char>_Highscore` track progression and per-character bests. Both are exposed to dialogue conditions via `Core.saveManagerIntMappings`.

**[MassageMovementPattern.cs](SMSAndroids/MassageMovementPattern.cs)** — Pure data classes for the minigame.
- `MassagePatternSegment` (zone index, `min/maxStrokes`, `direction` "up"/"down" → `DirectionInt`, legacy `targetSpeed`/`speedTolerance`).
- `MassageMovementPattern` (`patternName` + `MassagePatternSegment[] segments`).
- `ZoneDefinition` (normalized `yMin`/`yMax` per zone — zones may overlap).
- `MassagePatternCollection` (top-level JSON wrapper: `{ "patterns": [...] }`).

**[BreastPhysics.cs](SMSAndroids/BreastPhysics.cs)** — Soft-body deformation MonoBehaviour. Triangulates a `PolygonCollider2D` into a deformable mesh, pinned along an editable polyline (`pinPoints`), with per-vertex springs + propagation smoothing. Drives the `Sprites/SqueezeSprite` shader's `_MouseX`, `_MouseY`, `_Clicked` properties via `MaterialPropertyBlock`. Replaces the `SpriteRenderer`'s rendering with a child `MeshRenderer`. The companion editor lives at [SMSAndroids/Editor/BreastPhysicsEditor.cs](SMSAndroids/Editor/BreastPhysicsEditor.cs) — Editor-only.

**[DragSpriteController.cs](SMSAndroids/DragSpriteController.cs)** — Dough-like grab/drag effect for the `Sprites/DragSprite` shader. Tracks click + drag in UV space with `SmoothDamp`, simulates a 2D damped harmonic spring on release. Uses `IsPixelOpaque` (requires Read/Write enabled on the texture) to gate grabs to opaque pixels only.

**[LotionTrail.cs](SMSAndroids/LotionTrail.cs)** — Paints a varnish/lotion ribbon along a path (typically driven by a `SqueezeContourFollower`). Quad-strip mesh with optional rounded end caps; on Y-direction reversal, splits into a new strip with a unique stencil ref so layers stack via alpha but don't self-overlap. On release, the strip uniformly fades over `fadeOutTime`. Uses the `Sprites/LotionTrail` shader.

**[SqueezeContourFollower.cs](SMSAndroids/SqueezeContourFollower.cs)** — Locks a follower GO to the left or right edge of a sprite contour, sampled either from a mask texture's red channel (`MaskRedChannel`) or the sprite's alpha edge (`SpriteAlphaEdge`). Vertical follows the mouse, horizontal snaps to the edge. Optional click-sprite swap (`defaultSprite` / `clickSprite`) and `clickActiveObject` toggle. Computes "full sprite world bounds" manually (`GetFullSpriteWorldBounds`) so it works with Tight sprite mesh trimming.

**[SqueezeSpriteController.cs](SMSAndroids/SqueezeSpriteController.cs)** — Drives the `Sprites/SqueezeSprite` shader (`_MouseX`, `_MouseY`, `_Clicked`). Smooths position + click strength. Optional `onlyWhenHovered` gating.

**[RandomChildActivator.cs](SMSAndroids/RandomChildActivator.cs)** — `OnEnable` activates one random child (or the previously selected one if `pickNewOnEnable` is false). `OnDisable` deactivates all children. Used by Schedule for Anis's HH NPC variants — Schedule sets `pickNewOnEnable = true` whenever Anis's location changes so the next enable picks a fresh pose.

**[ButtonHover.cs](SMSAndroids/ButtonHover.cs)** — Fades child(2)'s `Image` alpha on pointer enter/exit. Used by the new map buttons created in `Places.CreateNewPlace`.

**[GridLayoutGroup.cs](SMSAndroids/GridLayoutGroup.cs)** — Custom subclass of Unity's `GridLayoutGroup` (based on MrBeardy's BeardyGridLayout). Anchors top-left, applies a vertical shift so the **last row sits at the top** of its container, and toggles `Places.extraNavRow.SetActive` based on whether more than one row is in use (deferred via coroutine to avoid layout-rebuild conflicts).

**[TransformExtensions.cs](SMSAndroids/TransformExtensions.cs)** — `Transform.FindInActiveObjectByName(name)` — recursive find that includes inactive children.

### Shaders

Hand-edited shader files referenced by the runtime classes:
- [SMSAndroids/Sprites_DragSprite.shader](SMSAndroids/Sprites_DragSprite.shader) — `Sprites/DragSprite`
- [SMSAndroids/Sprites_LotionTrail.shader](SMSAndroids/Sprites_LotionTrail.shader) — `Sprites/LotionTrail`
- [SMSAndroids/Sprites_SqueezeSprite.shader](SMSAndroids/Sprites_SqueezeSprite.shader) — `Sprites/SqueezeSprite`
- [SMSAndroids/Sprites_JiggleSprite_Reconstructed.shader](SMSAndroids/Sprites_JiggleSprite_Reconstructed.shader) — reconstruction of the base game's `Sprites/JiggleSprite` for reference (the active version lives in `ReferenceClasses`).

### Scene captures (read-only)

`SMSAndroids/InitialHierarchy_*` text files are scene-hierarchy dumps captured for navigation reference (`CoreGameScene` and `GameStart` for both 1.8 and the 1.8C build). Useful when figuring out a `transform.Find(...)` path; treat as historical snapshots.

---

## Conventions to follow when editing

- **Plugin pattern.** New persistent subsystems get their own `BaseUnityPlugin` + GUID under `treboy.starmakerstory.smsandroidscore.<name>`, with a `loaded<Name>` flag, a `Logger.LogInfo("----- <NAME> LOADED -----")` confirmation, and reset of the flag in the `GameStart` branch.
- **Don't pre-empt the load chain.** Init code goes inside `if (!loadedX && <deps>) { ... loadedX = true; }`. Don't do work in `Awake()` beyond loading bundles or wiring `SceneManager.sceneLoaded`.
- **Persist via `SaveManager`, not `PlayerPrefs`.** New persistent state means: add an entry to `SaveManager.defaultValues` with the right type, then use `SetX/GetX`. If it should also be visible to dialogue conditions, add it to one of the `saveManager*Mappings` lists in `Core.cs`.
- **Surfacing state to dialogue.** Mod state visible to GC2 conditions/instructions goes through Proxy Variables. The asset itself is authored in the Unity bundle project; the matching keys must already exist in `Proxy Variables` for `Core.proxyVariables.Set/Get` to work — `SyncVanillaToProxyVariables` will warn if a mapping points at a missing key.
- **Affection clamping.** `SaveManager.SetInt` clamps any `Affection_*` key to `[0, 5]`. Don't bypass this.
- **Daily reset semantics.** Variables that should reset every in-game day: prefix with `DailyProc_` (proxy) or suffix with `_Daily` (SaveManager). Both reset paths are wired automatically.
- **Coroutine helpers** that return `IEnumerator` are usually started via `Debugging.FindObjectOfType<Debugging>()` in `Core` because `Core` itself doesn't always have a started instance handy — keep this idiom unless you have a real reason to change it.
- **Don't touch `ReferenceClasses/`.** It's not in the `Compile` list of the csproj anyway, but the contents are documentation about how GC2 internals are shaped. Pretend it's read-only.
- **Comments policy:** existing style is "comment only when the why isn't obvious" — keep that.
