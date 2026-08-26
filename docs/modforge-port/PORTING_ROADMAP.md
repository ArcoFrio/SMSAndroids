# Porting Roadmap — what's still in SMSAndroids that could move to ModForge

A systematic walk through every SMSAndroids subsystem, categorising
each as **already covered**, **portable / ready**, **portable / complex**,
or **stays in SMSAndroids**. Companion to [INTEGRATION.md](INTEGRATION.md)
— that file tracks gaps in work already shipped; this one tracks what's
still untouched.

Use this when deciding the next batch of work after dialogues. The
items are ordered roughly by ROI per line of effort.

> **What this file is.** An engineering tracker for moving functionality out of
> SMSAndroids and into ModForge. It lives in the SMSAndroids repository because
> its subject is SMSAndroids: ModForge is a generic toolkit and carries nothing
> about this mod, in code or in prose. The reference that ships to pack authors
> is the in-app Documentation section on the ModForge tab, written to be read by
> people who have never heard of SMSAndroids.
>
> **How much of it is verified.** Claims about ModForge — what has shipped, what
> an action is called, what a model holds — were checked against the ModForge
> sources on 2026-08-26. Claims about SMSAndroids internals (file names, line
> numbers, variable names, what its own code does today) were **not**, so
> anything here about this repository's own code may have moved on.

---

## Tier 0 — already covered by ModForge

For reference, these don't need any more porting work (modulo
INTEGRATION.md gaps):

| SMSAndroids subsystem | ModForge equivalent |
|---|---|
| Bust factory (`Characters.cs`) | `OutfitDef` + `BustFactory` (via SMSBustForge BustPacks) |
| Variable store (`SaveManager.cs`) | `PackVariableDef` + `PackVariableStore` |
| Places + roomtalks (`Places.CreateNewPlace`) | `PlaceDef` + `PlaceFactory` |
| Navigator buttons (`Places.cs` map-button gating) | `NavigatorButtonDef` + `NavigatorRuntime` + `ButtonHover/Click` |
| World map radial buttons | `MapButtonDef` + `RadialButtonRuntime` |
| Dialogue prefabs (`Dialogues.cs`) | `DialogueDef` + `DialogueBuilder` + `DialogueDispatcher` |
| GC2 variable bridge (`Core.cs` reflection) | `GameVariableBridge` |
| Per-dialogue scene CGs (pack-authored) | `SceneDef` + `SceneFactory` |
| Speech-bubble name colours | `ActorDef.NameColor` + `SpeechColorApplier` |

---

## Tier 1 — portable / ready (clean small wins)

These have a tight, hardcoded surface in SMSAndroids and a natural
model shape on the pack side. Each is a few-hour port: model + VM +
XAML tab + plugin factory + manifest entries.

### ~~1A. Wallpapers~~ ✅ RESOLVED
- **SMSAndroids surface:** `Wallpaper.cs` (~150 lines). 4 wallpapers:
  `AnisSwimsuit`, `DorothySwimsuit`, `HelmSwimsuit`, `SolidGearOfMetal`.
  Each has a PNG (`Core.wallpaperPath + WallpaperX.PNG`), an
  `Event_Seen<Char><Place>01` unlock gate (so wallpapers appear in
  the PC selector once the matching event has played), and a clone of
  the base wallpaper + selector button.
- **Pack shape proposal:**
  ```jsonc
  "wallpapers": [
    { "key": "AnisSwim", "displayName": "Anis Swimsuit",
      "spritePath": "Wallpapers/AnisSwim.png",
      "unlockCondition": { "type": "VariableEquals",
                           "params": { "name": "Event_SeenAnisMall01", "value": "true" } } }
  ]
  ```
- **Why it's worth doing:** lets pack authors ship wallpapers without
  asking SMSAndroids to add them. Tiny scope. Used UI elements
  (`Wallpaperselection` panel, `Desktop/Wallpaper` parent) are
  vanilla — no Unity-side work.
- **Resolution:** `WallpaperDef` model + `WallpaperFactory` runtime +
  `WallpaperRegistry` per-frame visibility tick. Pack supports both
  `spritePath` (pack-relative) and `externalSpritePath` (absolute,
  transitional). 4 entries added to SMSAndroidsPack/modpack.json
  pointing at the existing SMSAndroids wallpaper PNGs.
- **Status:** done, including the Wallpapers tab this entry once listed
  as future polish.

### 1B. Gift shop items
- **SMSAndroids surface:** `Places.cs:440-448` hardcodes 9 items via
  `AddItemToGiftStore(name, price, image)`. Each is gated by
  `Gift_<Name>` proxy variable for visibility (set as the player
  unlocks gifts through dialogue beats).
- **Pack shape proposal:**
  ```jsonc
  "giftItems": [
    { "key": "Sunscreen", "displayName": "Sunscreen", "price": 600,
      "spritePath": "Items/Sunscreen.png",
      "visibleCondition": { "type": "VariableEquals",
                            "params": { "name": "Gift_Sunscreen", "value": "true" } } }
  ]
  ```
- **Why it's worth doing:** clean way to add new giftable items per
  pack. Mirrors the existing `Places.AddItemToGiftStore` pattern with
  pack-controlled visibility conditions.
- **Estimated effort:** small. Same shape as wallpapers.

### ~~1C. Music tracks~~ ✅ RESOLVED
- **SMSAndroids surface:** `Dialogues.CreateMusicPlayer(assetName)`
  (3727). One vanilla call site today: `HarborHomeMusic`. The pack
  already has `SwitchMusic` action but no way to declare *new* tracks
  — the action's `music` param has to name an existing GO under
  `12_AudioPlayer`.
- **Pack shape proposal:**
  ```jsonc
  "music": [
    { "key": "MyTrack", "audioPath": "Audio/MyTrack.ogg",
      "loop": true, "volume": 0.7 }
  ]
  ```
- **Why it's worth doing:** the existing `SwitchMusic` action becomes
  fully self-serve. Without this, pack dialogues can only switch
  *between* vanilla / SMSAndroids tracks.
- **Estimated effort:** small. Requires `AudioClip.Create` from a
  WAV/OGG on disk (or wraps `UnityWebRequestMultimedia` for OGG).
- **Resolution:** `MusicDef` model (key, displayName, audioPath,
  optional loop / volume overrides) + `MusicFactory` runtime. The
  factory clones the vanilla <c>12_AudioPlayer/Beach</c> template,
  renames the clone to the pack's key, and streams the audio file
  into the AudioSource via `UnityWebRequestMultimedia.GetAudioClip`
  on a coroutine. Format inferred from extension (.ogg / .wav / .mp3).
  Name collisions with existing audio children are refused (no
  stomping a vanilla / SMSAndroids track).
- **Status:** runtime + manifest support shipped. No entries added
  to SMSAndroidsPack — SMSAndroids' own `HarborHomeMusic` still
  loads via its asset-bundle path, and the existing `SwitchMusic`
  action finds it the same way. Future pack authors can add new
  tracks by dropping audio files into the pack folder and
  declaring them under the new `"music"` array.

---

## Tier 2 — portable / complex

Worth doing eventually but each is a multi-day port with edge cases.
Listed in priority order.

### ~~2A. Vanilla / SMSAndroids scene CGs (135 entries)~~ ✅ RESOLVED
- **SMSAndroids surface:** `Scenes.cs` is 419 lines of
  `CreateNewPicScene("<Name>", "<PngPath>")` calls. 135 scenes total:
  47 Anis (Affection × 13, HHBedroom × 5, HHBathroom × 10,
  ChillTopless × 6, etc.), 5 per other Nikke (4 voyeur + 1 event),
  3 Amber (event + story).
- **Why it's a big deal:** every pack dialogue that activates one of
  these CGs currently uses `SetGameObjectActive` with the absolute
  path `4_CG_Manager-Sexy/<Name>` (Batch 2 voyeurs, Batch 3a Anis
  Random, Batch 3c Affection arc — basically every dialogue with a
  scene). The scenes are owned by SMSAndroids' `Scenes.cs`. If
  SMSAndroids ever phases out, those scene GOs disappear and every
  pack dialogue that references them breaks.
- **Pack shape proposal:** the existing `SceneDef` model is already
  the right shape — extend the SMSAndroidsPack to declare all 135
  scenes pointing at PNGs shipped in the pack. Then convert every
  pack dialogue's `SetGameObjectActive 4_CG_Manager-Sexy/X` →
  `ActivateScene <key>`.
- **Estimated effort:** medium. The model already exists; the work
  is (a) shipping 135 PNGs in the pack folder, (b) declaring them in
  modpack.json, (c) rewriting the Batch 2/3 dialogue actions to use
  `ActivateScene` keys instead of literal paths. A converter pass on
  the existing modpack.json can do the last step automatically.
- **Friction:** the PNGs currently live in
  `BepInEx/plugins/SMSAndroidsCore/Scenes/...` — pack would need its
  own copies (or a way to reference them). If duplicating the PNGs
  is OK, the work is mechanical.
- **Resolution:** `SceneDef` extended with `externalSpritePath` so
  packs can reference the SMSAndroids PNGs without copying.
  `SceneFactory.cs` now honours the new field. `_port_scenes.py`
  parsed all 135 `CreateNewPicScene` declarations from `Scenes.cs`,
  generated matching `SceneDef` entries in SMSAndroidsPack/modpack.json
  with `externalSpritePath`, and rewrote 115 existing dialogue
  `SetGameObjectActive(true)` actions on `4_CG_Manager-Sexy/<key>`
  paths into `ActivateScene(scene=<key>)` calls (plus 159
  `SetGameObjectActive(false)` calls repathed to the
  `pack:<packId>.<key>`-prefixed scene GOs the factory creates).
- **Status:** Batches 2/3 dialogues now activate scenes through the
  pack's own registry — when SMSAndroids' `Scenes.cs` is eventually
  retired, the pack still owns scene activation. 15 references in
  Batch 3 conversion guesses point at scene names that don't exist
  in SMSAndroids' `Scenes.cs` (e.g. `AnisHHLivingRoom01Scene*`,
  `CentiStoryGiftShopScene*`); those were broken before the port
  and remain broken — they fail silently.

### ~~2B. Character schedules~~ ❌ DECLINED
- **SMSAndroids surface:** `Schedule.cs`. Per-character
  `<char>DefaultLocation`, `<char>Location`, `<char>HHLocation`,
  `<char>HHOutfit` for 21 characters. `SetDay{1..5}Schedule` switch
  statements pick alternate locations from a complex rule set
  (`Voyeur_Seen<Char>`, `Affection_Anis_Seen{1,2}` thresholds,
  lottery numbers, weather, gift-shop unlock state).
- **Why it matters:** ~half the dialogue gates we wrote in
  Batches 1-3 read `Location_<Char>` (Schedule's mirror proxy) to
  check where a character is. Today the schedule is fixed by
  SMSAndroids; pack-authored dialogues can only *react* to it, never
  influence it.
- **Pack shape proposal:**
  ```jsonc
  "schedules": {
    "anis": {
      "defaultLocation": "MountainLabRoomNikkeAnis",
      "rules": [
        { "day": 3, "location": "Mall",
          "conditions": [
            { "type": "VariableEquals", "params": { "name": "Event_SeenAnisMall01", "value": "false" } }
          ] },
        { "day": 5, "location": "SecretBeach",
          "conditions": [ ... ] }
      ]
    }
  }
  ```
- **Estimated effort:** medium-large. The data model is
  straightforward but each character has lottery-dependent
  alternates that don't reduce to clean rules. Some rules also do
  *side effects* (set HHOutfit), which needs more vocabulary.
- **Friction:** SMSAndroids' HH roaming logic (`Schedule.cs` HH
  Roaming System region) is tightly coupled to per-character HH NPC
  slot data. That part stays in SMSAndroids — pack would only own
  the "where do I default to today" decision.
- **Decision:** stays in SMSAndroids — schedule rules are too
  game-specific (per-character lottery thresholds, day-of-week
  alternates, HH roaming) to generalise into pack-author-friendly
  primitives. Pack dialogues continue to *read* schedule state
  via the existing `Location_<Char>` proxy variables.

### ~~2C. Per-line SFX text patterns~~ ✅ RESOLVED (variant)
- **SMSAndroids surface:** `Dialogues.textToSFX` table populated by
  ~24 `CreateSFX("*pattern*", "ClipName", volume)` calls in
  `Dialogues.cs:2021-2042`. Hooked from
  `MainStory.OnDialogueLineStart` — when a dialogue line containing
  a registered pattern starts, the matching audio plays.
- **Why it matters:** pack dialogues that have SFX patterns in their
  text (we saw `*plap*`, `*thwack*`, `*hooooonk*`, etc. in extracted
  dialogues) silently produce no sound when triggered through the
  pack dispatcher, unless SMSAndroids' OnDialogueLineStart hook
  happens to still fire. Today that hook DOES fire (GC2 dialogue
  runs through the same pipeline either way), so this is
  *latently* working — but documented behaviour, not guaranteed
  behaviour. INTEGRATION.md #8.
- **Pack shape proposal:**
  ```jsonc
  "sfx": [
    { "pattern": "*plap*", "audioPath": "Audio/SFX/Plap.ogg", "volume": 0.75 }
  ]
  ```
  Plus a pack-side hook into the GC2 dialogue's per-line callback —
  identical mechanism to SMSAndroids.
- **Estimated effort:** small-medium once Music (1C) lands (same
  AudioClip loader). The dialogue hook is the moderately tricky bit.
- **Resolution (variant — explicit `PlaySFX` action, not text-pattern):**
  `SfxDef` model (key, displayName, audioPath, optional defaultVolume)
  + `SfxRegistry` + `SfxFactory` (one shared `SfxPlayer` AudioSource
  per pack under `12_AudioPlayer`, async-load each clip via the same
  `UnityWebRequestMultimedia` path Music uses). New `PlaySFX` node
  action with `clip` + `volume` + `delay` params — multiple plays =
  multiple actions stacked on the same node, each with independent
  delay. Overlapping audio handled natively by
  `AudioSource.PlayOneShot`. SMSAndroids' text-pattern → SFX hook
  continues to fire alongside (latently working — same GC2 dialogue
  runtime); the new action is for explicit author-driven SFX
  triggers rather than text-pattern triggers.

### 2D. Schedule visualiser (World Map character icons)
- **SMSAndroids surface:** `ScheduleVisualizer.cs`. Renders per-char
  icons on World Map district / radial buttons. Uses procedurally
  generated `Texture2D` overlays with rounded corners + circular
  cutouts, per-character `Icon{Char}.png` from disk, hover
  promotion of district buttons, sibling-index tweaking.
- **Why it might matter:** pack-defined characters today don't show
  on the World Map. If a pack adds a brand-new Nikke with her own
  schedule entries, she's invisible on the map.
- **Pack shape proposal:** declare an `iconPath` on `ActorDef` plus
  `worldMapLocation` rule entries that map `Location_<Char>` strings
  to district / radial button names. The procedural overlay
  rendering can be ported wholesale — it's self-contained code.
- **Estimated effort:** medium. The procedural texture work is
  ~300 lines and depends on Unity drawing primitives.
- **Why deferred:** only valuable if you actually have new
  pack-defined Nikkes. None today.

### ~~2E. Weather overlay GOs~~ ✅ SHIPPED

- **Resolution:** `PlaceDef.WeatherType`, exactly the proposed shape, spelled
  `None` / `Inside` / `Outside`. It sits under Behaviour on the Places tab.
  The "why deferred" note below is kept for the reasoning, not the decision.
- **SMSAndroids surface:** `Places.cs` toggles `weatherInside{Rain,Snow}`
  / `weatherOutside{Rain,Snow}` overlay GOs based on the GC2
  `rainy-day` / `snowy-day` global variables. Per-level: indoor
  levels get the indoor weather, outdoor levels get the outdoor.
- **Pack shape proposal:** `PlaceDef.weather` field with values
  `Indoor` / `Outdoor` / `None`. The runtime instantiates the
  appropriate overlay child under each place's level GO.
- **Estimated effort:** small. Self-contained.
- **Why deferred:** doesn't block anything today. The vanilla weather
  system covers vanilla places; pack places get no weather but that's
  a polish issue not a functional one.

---

## Tier 3 — stays in SMSAndroids

Game-side intrinsic functionality that doesn't map cleanly to pack
authoring. These are documented here so we don't waste time looking
at them.

| Subsystem | Why it stays |
|---|---|
| Harbor Home NPC roaming (`Schedule.cs` HH region) | Per-char position slot picking, audio gating, RandomChildActivator wiring — too tightly coupled to specific Anis HH NPC GOs |
| HHTalk panel (`Dialogues.cs`) | Custom UI overlay (rounded sprites, list buttons, selection state) — Unity UI work |
| Gift UI overlay (`Dialogues.cs`) | Same as HHTalk — UI work; INTEGRATION.md #7 |
| NanoSave integration (`SaveManager.cs`) | Vanilla save system hooks — fundamentally game-side |
| Massage minigame (`MassageMinigame.cs` + shaders) | Custom MonoBehaviour + shader system; pattern JSON is the only piece a pack could meaningfully own |
| BreastPhysics / SqueezeSprite / LotionTrail / DragSprite | Custom MonoBehaviours + shaders for the minigame |
| Voyeur tier promotion logic | Stays as INTEGRATION.md #1A's near-term path — pack-native version is INTEGRATION.md #1B |
| `OnDialogueLineStart` per-line hook | GC2 dialogue runtime detail; SMSAndroids subscribes and runs SFX logic against the result |
| Affection clamping (`SaveManager.SetInt`) | Behaviour parity needs the pack to set `Min/Max` on each Affection_* var (already noted in INTEGRATION.md #3b) — doesn't need a port, just a manifest update |
| Proxy Variables asset (`Core.cs`) | A GC2 asset that lives in the dialogue bundle. Pack uses its own variable store; the bridge to vanilla GC2 vars stays in SMSAndroids |

---

## Recommended sequencing

If the goal is "phase out SMSAndroids":

1. **First**: Tier 1A (Wallpapers) + Tier 1B (Gift items) +
   Tier 1C (Music) — three quick wins that establish the pattern
   for pack-declared content beyond busts/dialogues.
2. **Second**: Tier 2A (Scenes) — biggest impact, makes Batches 2/3
   self-contained instead of dependent on SMSAndroids' `Scenes.cs`.
3. **Third**: Tier 2B (Schedules) — once content is pack-owned,
   *driving* it (deciding where characters go each day) is the next
   piece SMSAndroids still controls.
4. **Fourth**: Tier 2C (SFX) + Tier 2D (Visualiser) + Tier 2E
   (Weather) — polish.
5. **Last**: INTEGRATION.md #1B (pack-native voyeur picker) and
   leaving Tier 3 behind.

If the goal is "ship more content per pack without changing
SMSAndroids":

1. Wallpapers + Gift items (1A + 1B) — small, useful, isolated.
2. Scenes (2A) — unlocks per-pack story arcs without SMSAndroids work.
3. Music + SFX (1C + 2C) — atmosphere parity.

---

## How to update this file

When an item lands, strike it through (`~~`) and add a "Resolved by
commit XYZ" note. When new opportunities are discovered (porting
work usually surfaces more), add them under the appropriate tier.
