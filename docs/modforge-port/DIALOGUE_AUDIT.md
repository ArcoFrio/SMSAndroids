# Dialogue Subsystem — pre-prune audit of SMSAndroids

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


Companion to [INTEGRATION.md](INTEGRATION.md) and
[PORTING_ROADMAP.md](PORTING_ROADMAP.md). Written specifically as a
**safety check before removing dialogue-related code from SMSAndroids** —
this catalogs every dialogue-touching surface in SMSAndroids and marks
each one as **safe to remove**, **partially removable**, or **must
stay**, with the new findings highlighted.

The bare numbers, for scale:

| Surface (in SMSAndroids) | Count |
|---|---|
| `public static GameObject *Dialogue*` field declarations | 826 |
| `public static GameObject *Scene<N>` declarations | 424 |
| `DialogueActivator` / `DialogueFinisher` GO declarations | 160 |
| `MouthActivator` / `SpriteFocus` GO declarations | 156 |
| `CreateNewDialogue(…)` calls | 81 |
| `StartDialogueSequence*` / `EndDialogueSequence*` / `currentActive*` / `diagBusts` references in MainStory.cs | 214 |
| `evaluatingLevelDialogue` / `dialoguePlaying*` / dispatcher-state references in MainStory.cs | 384 |
| Pre-built `SignalArgs` (kissSignal et al.) usages in MainStory.cs | 40 |
| Per-actor speech-skin / colour helper calls | 65 |

Each section below tells you which of those are owned by the pack now.

---

## 1. Safe to remove (pack owns it end-to-end)

### 1.1 Dialogue prefab GameObject inventory (`Dialogues.cs` top section)

- **What:** the 826 + 424 + 160 + 156 = **~1,566 static fields** that hold
  references to dialogue prefabs and their Scene<N> / DialogueActivator /
  DialogueFinisher / MouthActivator / SpriteFocus children.
- **Pack equivalent:** `DialogueDef` array in modpack.json
  (80 dialogues, 2,395 nodes) — built at runtime by `DialogueBuilder`.
- **Verdict:** ✅ safe to remove **all 1,566** fields. Pack dialogues
  are GO-built from JSON; nothing else in SMSAndroids needs the
  references.

### 1.2 `CreateNewDialogue(bundleAsset, roomTalk)` (Dialogues.cs)

- **What:** instantiates a dialogue prefab from `dialoguebundle` under
  a roomtalk parent. 81 call sites in Dialogues.cs's init.
- **Pack equivalent:** `DialogueBuilder.Build` constructs the GC2
  Dialogue from JSON node-by-node — no asset bundle prefab needed.
- **Verdict:** ✅ safe to remove `CreateNewDialogue` and all 81 calls.
- **Asset bundle consequence:** the `dialoguebundle` itself still
  needs to load — it carries the `Proxy Variables` GNV asset that
  `Core.proxyVariables` registers. **Don't delete the bundle**; just
  stop calling `CreateNewDialogue` on its dialogue prefabs.

### 1.3 Per-dialogue Update() switch blocks (`MainStory.cs`)

- **What:** ~80 large switch-case-or-if blocks of the shape:
  ```
  if (!Dialogues.dialoguePlaying && <gates>) {
      StartDialogueSequence(Dialogues.<diag>);
      diagBusts.Add(...);
  }
  if (Dialogues.<diag>Scene1.activeSelf) { Scenes.X.SetActive(true); ... }
  if (Dialogues.<diag>DialogueFinisher.activeSelf) { ... SaveManager.SetBool(...); }
  ```
- **Pack equivalent:** `DialogueDispatcher` polls `startConditions`
  per dialogue; `actionsOnStart` / `actionsOnFinish` on each node
  drive the Scene / SetVariable / LeaveBust side effects.
- **Verdict:** ✅ safe to remove **every** dialogue-specific block in
  the Update() body. The pack dispatcher already evaluates the
  equivalent gates and runs the equivalent actions for all 80
  dialogues.
- **Be careful:** the Update() body also contains schedule-driven HH
  roaming, voyeur lottery roll, per-character location switching that
  isn't dialogue. Don't remove those by accident — only the
  `Dialogues.X` / `Scenes.X` / `Characters.X` blocks that mirror
  what's in the per-dialogue scene markers.

### 1.4 `StartDialogueSequence*` family + helpers (`MainStory.cs`)

- **What:** `StartDialogueSequence`, `StartDialogueSequenceDelayed`,
  `StartDialogueSequenceQueue`, `PlayDialogueStep`,
  `EndDialogueSequence`, `FinishStep`. Plus the `*Vanilla` parallel
  family (`CheckAndStartVanillaDialogue`,
  `StartDialogueSequenceVanilla`, `EndDialogueSequenceVanilla`).
  Also `dialogueToActivate`, `tempNewCurrentRT`,
  `evaluatingLevelDialogue`, `amberDefaultDiagQueued`,
  `lastEvaluatedLevel`, `currentActiveBust`, `currentActiveBustMBase`,
  `currentActiveDialogue`, `currentActiveDialogueSpriteFocus`,
  `diagBusts`.
- **Pack equivalent:** `DialogueDispatcher` is a parallel, self-contained
  dispatcher with its own per-built `currentActive*`-like state in
  `BuiltDialogue` and `_byDialogue`.
- **Verdict:** ✅ safe to remove all of these **once the per-dialogue
  Update blocks (§1.3) are gone**. They have no callers outside the
  dialogue dispatcher.
- **Order-of-operations:** remove §1.3 FIRST, then these. The reverse
  order leaves dangling references.

### 1.5 Bust helpers (`MainStory.cs`)

- **What:** `ChangeActiveBust(GameObject, GameObject)`,
  `ChangeBustSortingOrder(GameObject, int)`,
  `SpriteFocusChange(...)`, `GetBustForActor(string)`,
  `Fade(SpriteRenderer, float, float)` (alpha tween coroutine).
- **Pack equivalent:** `ActorRegistry.SetActorBust`,
  `ActorRegistry.SetSpriteFocus`, `ActorRegistry.LeaveBust`.
  Per-node `Outfit` field on dialogue nodes drives outfit swaps.
- **Verdict:** ✅ safe to remove. Pack actions cover every call site
  the per-dialogue blocks in §1.3 used these for.
- **Edge case — Fade:** if any non-dialogue code uses `Fade` (e.g. a
  generic alpha tween), keep it. Quick grep before removing.

### 1.6 Pre-built `SignalArgs` (`MainStory.cs`)

- **What:** `kissSignal`, `flashSignal`, `fadeInSignal`,
  `fadeOutSignal`, `fadeInBlackSignal`, `fadeOutBlackSignal`,
  `fadeUISignal`, `blinkSignal`, `drinkSignal`,
  `dialogueStartSignal`, `dialogueEndSignal`,
  `forceEnableUISignal`, `whiteFlashNoSoundBlackSignal`. Cached
  signal-args structs for reuse.
- **Pack equivalent:** the `EmitSignal` action emits by name via
  reflection — no cached structs needed. `SceneDef.Sound = Kiss/Flash`
  emits `kiss` / `flash` signals automatically on scene activation.
- **Verdict:** ✅ safe to remove. Pack rebuilds the args per emit; the
  performance difference is unmeasurable in this context.
- **Be careful:** if anything outside dialogue (e.g. a one-shot bit
  of UI code) uses these vars, that caller needs converting to
  `Signals.Emit(new SignalArgs(...))` first.

### 1.7 Per-line dialogue text variable substitution (`Dialogues.cs`)

- **What:** `ProcessTextWithVariables`, `ProcessGlobalVariables`,
  `ProcessSaveManagerVariables`, `GetGlobalVariableValue`,
  `GetSaveManagerVariableValue`, `ConvertValueToString`. Resolves
  `[GV:VarName]` (GC2 globals) and `[SM:VarName]` (SaveManager)
  placeholders inline in dialogue text.
- **Stays in SMSAndroids per author preference** — predates the
  realisation that GC2's native `{X}` syntax already resolves GNVs
  at render time. SMSAndroids' Process functions still cover
  authored `[GV:]` / `[SM:]` tokens in any non-pack code path.
- **Pack equivalent:** `DialogueBuilder.ResolvePlaceholders` —
  reduced to **`[PV:name]` only** (pack variables). `{X}` for
  GC2 globals goes through GC2's own runtime, not this code.
- **Verdict:** ✅ safe to remove for **pack-owned dialogues** — they
  go through DialogueBuilder, not Dialogues.ProcessTextWithVariables.
- **Placeholder cleanup already done:**
  - 22 dialogue nodes previously contained `[GV:PCName]`. **All
    rewritten to `{PC}`** — the canonical vanilla GC2 GNV for the
    player's name is `PC`, so GC2 resolves it at render time.
  - 0 nodes contain `[SM:]` — extracted text doesn't carry
    SaveManager placeholders, so no conversion needed.
  - All 5 `_convert_*.py` extractor scripts updated to emit `{X}`
    for `GetStringGlobalName`, with a `GNV_ALIASES` remap table
    that translates SMSAndroids-side variable names to their
    vanilla equivalents on the way out.

### 1.8 Per-actor speech-skin / colour helpers (`Dialogues.cs`)

- **What:** `overrideSpeechSkinBlue`, `overrideSpeechSkinGreen`,
  `overrideSpeechSkinPink`, `overrideSpeechSkinYellow`, plus
  `GetActorOverrideSpeechSkinValue`, `SetActorOverrideSpeechSkinValue`,
  `AddActorColorToSpeechUI` (overloads),
  `AddActorColorsToSpeechUI` (overloads), `ColorFromBytes`,
  `GetAllActorExpressions`, `AddExpressionSetInstructionToOnStart`.
- **Pack equivalent:** `ActorDef.NameColor` + `SpeechColorApplier`
  registers per-actor name colours; expressions are owned per-actor
  via `ActorDef.Expressions`.
- **Verdict:** ✅ safe to remove for pack-owned dialogues. Keep only
  if any non-pack dialogue still needs them (unlikely once dialogue
  is 100% pack-owned).

### 1.9 SFX text-pattern system (`Dialogues.cs`)

- **What:** `CreateSFX(textPattern, clipName, volume)`,
  `GetRandomAudioClipForSFX`, `SFXMapping`, `textToSFX` dictionary,
  and the `Dialogues.cs:2021-2042` block of 24 hardcoded
  `CreateSFX(...)` calls.
- **Pack equivalent:** `SfxDef.TextPatterns` + `SfxRegistry`
  registers patterns; `SfxRegistry.FireMatchingPatterns` on node
  start does the matching. Variants auto-detected by
  `SfxFactory` (`<key>_<N>.<ext>`).
- **Verdict:** ✅ safe to remove for pack-owned dialogues. SMSAndroids'
  `MainStory.OnDialogueLineStart` → `ProcessSFXTriggersForText`
  becomes dead.
- **⚠️ Hook scope subtlety:** the pack's pattern-firing is
  per-dialogue (hooked in `DialogueDispatcher.OnStartNext`). Vanilla
  GC2 dialogues that the pack DIDN'T port lose SFX detection. For
  the SMSAndroidsPack (which ports all 80 SMSAndroids dialogues),
  this is a non-issue — vanilla GC2 dialogues outside the pack never
  had `*pattern*` text in them anyway.

### 1.10 SFX playback helpers (`MainStory.cs`)

- **What:** `PlaySFXWithDelay`, `GetSizeMultiplierAtPosition`,
  `lastProcessedNodeID`, `lastPlayedIndicesPerNode`,
  `sfxDelayCoroutine`, `sfxPlaybackInstance`.
- **Pack equivalent:** `ActionRuntime.PlayOneShotAfter` coroutine +
  `SfxRegistry.PlayAfter` for the delayed pattern matches.
- **Verdict:** ✅ safe to remove with §1.9.
- **Minor regression to accept:** the pack doesn't currently apply
  `<size=X%>` tag-based volume multipliers (SMSAndroids' polish
  detail). Easy to port if you ever want parity.

### 1.11 Music construction (`Dialogues.cs`)

- **What:** `CreateMusicPlayer(assetName)` + the
  `audioHarborHomeMusic = CreateMusicPlayer("HarborHomeMusic")` call.
- **Pack equivalent:** `MusicDef` + `MusicFactory`. The
  `SMSAndroidsPack/modpack.json` already declares
  `HarborHomeMusic` pointing at `Audio/HarborHomeMusic.ogg`.
- **Verdict:** ✅ safe to remove. Pack creates the same-named GO
  under `12_AudioPlayer`, so the existing `SwitchMusic` action
  calls (and the `MapButton.Music` field) find it the same way.

### 1.12 Wallpaper subsystem (`Wallpaper.cs`)

- **What:** entire `Wallpaper` plugin (~250 lines): `CreateWallpaper`,
  `UpdateWallpaperDisplay`, the 4 wallpaper field declarations,
  per-frame visibility loop.
- **Pack equivalent:** `WallpaperDef` + `WallpaperFactory` +
  `WallpaperRegistry` (per-frame visibility re-evaluation). 4
  entries declared in SMSAndroidsPack pointing at the
  `Wallpaper/` folder.
- **Verdict:** ✅ safe to delete the entire `Wallpaper.cs` file.
- **Asset consequence:** the
  `BepInEx/plugins/SMSAndroidsCore/Wallpaper/` directory is no
  longer needed by SMSAndroids. The pack folder
  `SMSAndroidsPack/Wallpaper/` carries the PNGs now.

### 1.13 Scene CG construction (`Scenes.cs`)

- **What:** 135 `CreateNewPicScene(name, pngPath)` calls + the
  matching 135 static GO declarations.
- **Pack equivalent:** `SceneDef` × 135 in `modpack.json` with
  `externalSpritePath` pointing at the existing SMSAndroids
  PNGs. `SceneFactory` builds equivalent GO under
  `4_CG_Manager-Sexy/pack:SMSAndroidsPack.<key>`.
- **Verdict:** ✅ safe to delete the entire `Scenes.cs` file once
  the pack scenes are verified visually equivalent.
- **Asset consequence:** the `Scenes/` folder full of PNGs is still
  referenced by `SceneDef.externalSpritePath`. **Don't delete the
  folder** — or, copy the PNGs into `SMSAndroidsPack/Scenes/` and
  switch every `SceneDef.externalSpritePath` to `sceneSprite` for
  full pack self-containment.

---

## 2. Partially removable — keep the shared pieces

### 2.1 `Dialogues.dialoguePlaying` / `dialoguePlayingVanilla`

- **What:** booleans mirrored to `Checks_Dialogue-is-playing` proxy
  variable. Used widely by per-frame `if (!Dialogues.dialoguePlaying)`
  gates in MainStory.cs.
- **Status:** **don't remove these YET.** INTEGRATION.md #2 — pack
  dialogues currently bypass the flag, so removing it breaks any
  remaining vanilla / SMSAndroids code that gates on it. First
  resolve #2 (pack writes to a shared flag), then the gates can be
  removed alongside §1.3.

### 2.2 SaveManager dialogue-driven flag writes

- **What:** `MainStory.cs` writes ~50 SaveManager flags from inside
  per-dialogue blocks (`Event_Seen*`, `Voyeur_Seen*`,
  `Affection_Anis_Seen{1,2,3}`, `HarborHome_Visit_Anis`,
  `MountainLab_*`, `SecretBeach_*`, `DailyProc_*`).
- **Pack equivalent:** pack-side `SetVariable` actions in dialogue
  finishers write to `PackVariableStore` instead.
- **Verdict:** ✅ writes can be removed from MainStory.cs as part of
  §1.3 cleanup.
- **⚠️ Read-side gap (INTEGRATION.md #3):** other SMSAndroids systems
  (Schedule, Wallpaper, Places button-gating, MainStory's tier
  promotion) STILL `SaveManager.GetBool("Voyeur_Seen<Char>")`. The
  pack writes its own copy and never touches SaveManager. **Fix #3
  before removing these writes**, or SMSAndroids' readers see stale
  values forever after.

### 2.3 `OnDialogueLineStart` / `ProcessCurrentDialogueLine` hooks

- **What:** Subscribes to per-dialogue line callbacks for SFX
  triggering. Currently fires on pack-built dialogues too (the
  hook is on the GC2 Dialogue runtime which is shared).
- **Pack equivalent:** `DialogueDispatcher.OnStartNext` calls
  `_ctx.Sfx.FireMatchingPatterns(text, ...)`.
- **Verdict:** safe to remove the SFX-related parts of
  `OnDialogueLineStart` AFTER `Dialogues.textToSFX` is empty
  (§1.9 done). The hook itself might do other per-line work
  (look for non-SFX code inside it).

### 2.4 `audioShower` / `audioShowerQuiet` (`Dialogues.cs` + `Schedule.cs`)

- **What:** Two AudioSource GOs on `Core.audioPlayer` that toggle
  based on whether Anis is in the bathroom and which HH room the
  player is in. Not dialogue, but built in Dialogues.cs.
- **Pack equivalent:** ❌ not ported. These are part of the HH
  roaming UX, not dialogue.
- **Verdict:** **must stay**. Roaming logic in Schedule.cs (Tier 3
  in PORTING_ROADMAP) drives these.

### 2.5 `Core` helper coroutines

- **What:** `EmitSignalDelayed(string, float)`,
  `EmitSignalGameObjectDelayed(string, GameObject, GameObject, float)`,
  `ChangeOutfitDelayed(...)`. Used by dialogue scene markers for
  staged transitions.
- **Pack equivalent:** `EmitSignalDelayed` action covers the simple
  delayed-signal case. The GameObject variant
  (`EmitSignalGameObjectDelayed`) does cross-fade between two GOs
  (deactivate one, activate other, then emit) — used for Affection02's
  Mall↔Cinema transition (INTEGRATION.md #20, currently un-ported).
- **Verdict:** keep `EmitSignalGameObjectDelayed` and `ChangeOutfitDelayed`
  until #20 lands or the corresponding dialogues are accepted as
  visually degraded.

### 2.6 `IncrementAffectionForGiftIfLiked` / `SetAndSyncGiftVariable` (`Core.cs`)

- **What:** Gift handshake glue invoked from
  `Dialogues.AnisDialogueGift` finisher in vanilla.
- **Pack equivalent:** the pack dialogue's finisher emits
  `OpenGiftUI` + `SetVariable Gifting_Gifted = false`. The actual
  affection bump is INTEGRATION.md #7.
- **Verdict:** **keep both helpers** — they're still the entry point
  for the gift-give flow. Whichever side eventually owns the gift
  UI will continue to call these.

---

## 3. Must stay — pack doesn't (and shouldn't) own them

| Subsystem | Location | Reason |
|---|---|---|
| HHTalk panel UI | `Dialogues.cs:2071+` | Tier 3 — custom Unity UI overlay |
| Gift UI overlay | `Dialogues.cs:2118+` (`UpdateGiftUIVisibility`, `AddGiftItem`) | Tier 3 — INTEGRATION.md #7 keeps it in SMSAndroids |
| HH roaming (`Schedule.cs`) | Schedule.cs's HH Roaming System region | Tier 3 — per-char NPC slot picking |
| Massage minigame | `MassageMinigame.cs` + shaders | Tier 3 |
| BreastPhysics / SqueezeSprite / LotionTrail / DragSprite | own files + shaders | Tier 3 |
| ScheduleVisualizer | `ScheduleVisualizer.cs` | Tier 3 |
| `Wallpaper_Current` index drift correction | `Wallpaper.cs:96-124` | merges into the pack wallpaper system if you delete `Wallpaper.cs`; pack registry already handles the active-display sync via its click handler |
| Proxy Variables GNV asset | `Core.proxyVariables` + the GNV in `dialoguebundle` | needed by ANY vanilla dialogue / condition that references `Affection_*`, `Voyeur_Seen*`, `Gift_*`, etc. |
| `SyncVanillaToProxyVariables` | `Core.cs` | the proxy variable bridge — needed as long as the proxy asset exists |
| GC2 variable bridge | `Core.cs` | needed by pack's `GameVariableBridge` (independent reflection path, but the proxy var assets must be registered for the bridge to find them) |
| NanoSave integration | `SaveManager.cs` | vanilla save system, nothing to do with dialogue |

---

## 4. 🆕 New gaps surfaced by this audit

### ~~4.1 `{PC}` placeholder~~ ✅ NOT A GAP

GC2's dialogue runtime handles `{X}` placeholders natively at line
display time — it walks every registered `GlobalNameVariables`
asset and substitutes the value. `{PC}` (and `{PCName}` etc.) are
resolved by GC2 with no pack-side code needed.

Followup work landed alongside this clarification:
- Pack's `DialogueBuilder.ResolvePlaceholders` lost the `[GV:]`
  branch (build-time substitution of GC2 globals is strictly worse
  than `{X}` render-time resolution — the build-time path bakes the
  load-time value so any mid-session change is missed).
- The 22 existing `[GV:PCName]` references in modpack.json were
  rewritten to `{PC}` — the vanilla GC2 GNV for the player's name is
  `PC`, not `PCName`. `PCName` was a SMSAndroids-side alias from
  before the user discovered `{PC}` existed natively.
- All five `_convert_*.py` extractor scripts now emit `{X}` for
  `GetStringGlobalName` instead of `[GV:X]`, plus carry a
  `GNV_ALIASES` table that remaps SMSAndroids-side names to the
  canonical vanilla ones (`PCName` → `PC` so far). Extend the
  table when more aliases surface.
- `[PV:name]` substitution stays — pack variables aren't registered
  as GC2 GNVs so `{X}` can't see them. (A future option: programmatically
  register pack variables as a GNV asset so `{X}` works for them
  too; see PORTING_ROADMAP for that path.)

### 4.2 Dialogue prefab asset bundle entries

- **What:** `dialoguebundle` ships 81+ dialogue prefabs that
  `CreateNewDialogue` loads. Once §1.2 lands, nothing loads them.
- **Verdict:** the prefabs stay in the bundle dead weight, but the
  bundle file itself is still needed (`Proxy Variables` GNV asset
  is also in there). No action required — just understand the
  prefabs are vestigial.

### 4.3 `<size=X%>` SFX volume modulation

- **What:** SMSAndroids' `GetSizeMultiplierAtPosition` reads the
  `<size=140%>` TMP tag wrapping a `*plap*` match and scales the
  SFX volume up by that ratio. Used so emphasised SFX (loud
  `<size=140%><b>*THWACK*`) sound louder than quiet ones
  (`<size=60%>*plap*`).
- **Pack status:** not ported. Pack plays at `defaultVolume` flat
  regardless of surrounding text size.
- **Verdict:** minor polish gap. Easy to port — scan backwards
  from the match position for the nearest `<size=N%>` tag and
  multiply by `N/100`. Add to `SfxRegistry.FireMatchingPatterns`.

### 4.4 The OnDialogueLineStart subscription registration point

- **What:** SMSAndroids subscribes its `OnDialogueLineStart` to every
  pack-built dialogue too, because the subscription is on the
  shared GC2 `Dialogue.EventStartNext` event. Once SMSAndroids'
  subscription is removed (§1.10), only the pack's per-dialogue
  hook fires.
- **Why this matters:** pack's hook is sufficient for SFX (§1.9
  resolved). But if SMSAndroids' `OnDialogueLineStart` does
  ANYTHING ELSE besides SFX (e.g. updates SaveManager, tracks
  analytics, drives non-SFX side effects), that work disappears
  when §1.10 lands.
- **Action item:** read `MainStory.OnDialogueLineStart` carefully
  before deleting. Anything non-SFX needs porting first.

### 4.5 Removing `MainStory` Update() blocks safely

- **The trap:** the Update() body interleaves dialogue dispatch with
  schedule mutation, voyeur lottery, button gating, weather toggles,
  HH roaming, etc. Removing dialogue blocks naively could remove
  schedule logic too.
- **Recommended workflow:**
  1. Tag each `case "<Location>":` block with a comment listing
     which dialogue handlers it owns.
  2. For each `if (!Dialogues.dialoguePlaying && ...) { StartDialogueSequence ... }`
     block: delete it (pack handles).
  3. For each `if (Dialogues.<diag>SceneN.activeSelf) { ... }`
     block: delete it (pack handles).
  4. For each `Schedule.<char>Location = ...` write near a dialogue
     finisher: **keep** (pack doesn't write schedule yet —
     INTEGRATION.md #21).
  5. For each `SaveManager.SetBool(...)` write near a dialogue
     finisher: delete (pack handles via `SetVariable`).
  6. For each `Characters.<bust>.SetActive(true)` near a
     `StartDialogueSequence`: delete (pack handles via
     `ActorRegistry`).

---

## 5. Cleanup checklist (suggested order)

This minimises broken-intermediate-state windows.

- [ ] **Phase 0 — fix the live read-side divergences first.**
  - [ ] INTEGRATION.md #1: hook SMSAndroids' voyeur picker to write
    `Voyeur_NextTarget` into the pack variable store, so pack
    voyeurs actually fire.
  - [ ] INTEGRATION.md #2: mirror `dialoguePlaying` between pack
    dispatcher and SMSAndroids.
  - [ ] INTEGRATION.md #3: pack mirrors `Voyeur_Seen*` /
    `Event_Seen*` / `Affection_*` writes back to SaveManager, OR
    SMSAndroids reads through the pack store. Whichever, pick one
    authoritative source per flag.
  - [ ] §4.1: add `{PC}` placeholder support in `DialogueBuilder.ResolvePlaceholders`.
  - [ ] §4.4: audit `OnDialogueLineStart` for non-SFX work and
    port anything found.

- [ ] **Phase 1 — drop the per-dialogue dispatch (§1.3 + §1.4).**
  - [ ] Tag every Update() block in MainStory.cs as
    dialogue / schedule / other.
  - [ ] Remove every dialogue block per §4.5 workflow.
  - [ ] Remove `StartDialogueSequence*` / `EndDialogueSequence*` /
    `currentActive*` / `diagBusts` definitions (now dead).
  - [ ] Build, launch the game, sanity-check a few dialogues fire
    correctly through the pack dispatcher.

- [ ] **Phase 2 — drop the dialogue inventory (§1.1 + §1.2).**
  - [ ] Delete all 1,566 dialogue-related static fields from
    Dialogues.cs.
  - [ ] Delete `CreateNewDialogue` and its 81 call sites.
  - [ ] Build, launch — dialogues should still play (pack-built).

- [ ] **Phase 3 — drop the dialogue-only helpers
  (§1.5 / §1.6 / §1.7 / §1.8 / §1.9 / §1.10).**
  - [ ] Bust helpers, signal-args, variable substitution, speech-skin,
    SFX system.
  - [ ] Keep §2.4 `audioShower` and §2.5 coroutine helpers if
    Schedule / non-dialogue code still uses them.

- [ ] **Phase 4 — drop Wallpaper.cs (§1.12) and Scenes.cs (§1.13).**
  - [ ] Verify pack wallpapers + scenes look identical first.
  - [ ] Optional: copy `Scenes/` PNGs into `SMSAndroidsPack/Scenes/`
    and switch `externalSpritePath` → `sceneSprite` for full pack
    self-containment, then delete the SMSAndroids `Scenes/` folder.

- [ ] **Phase 5 — tighten the integration polish.**
  - [ ] §4.3 `<size=N%>` SFX volume modulation if you want the
    parity. Optional.
  - [ ] Tier 3 items stay — they're intentionally SMSAndroids-owned.

---

## How to update this file

Mark items off as you complete them. When new dialogue-touching code
gets discovered (likely during cleanup itself), add it under the
appropriate section. The point of this file is to be the **single
authoritative list** so nothing gets removed accidentally and nothing
gets left behind that should have gone.
