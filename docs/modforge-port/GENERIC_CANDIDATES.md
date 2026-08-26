# Generic SMSAndroids functions worth considering for ModForge

Companion to [PORTING_ROADMAP.md](PORTING_ROADMAP.md) — that doc
catalogues SMSAndroids subsystems wholesale (Wallpapers, Music, Scenes
etc., mostly **shipped now**). This one catalogues smaller **helper-level
functions** that aren't whole subsystems but could meaningfully reduce
what packs need SMSAndroids for.

Each entry has a short "what / why / fit" so priorities can be argued
before anything is built.

**Four of these have since shipped** — A1, A2, A3 and B3 — and are marked
below rather than deleted, since what was decided is worth keeping. The rest
are still proposals.

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

## Tier A — strong candidates (well-defined, immediately useful)

### ~~A1. `EmitSignalGameObjectDelayed` → cross-fade-and-signal action~~ ✅ SHIPPED

- **Shipped as:** the `TransitionLevels` action, with the parameter shape
  proposed below. Documented for authors as "cross-fade from one place to
  another, moving the player mid-conversation".

- **Where it lives:** `Core.cs:1200` + private coroutine at `:1213`.
- **What it does:** the full level-transition primitive. Given two GOs
  and a delay:
  1. Enable any `Trigger` + `Conditions` components on GO1.
  2. Wait 2/3 of the delay.
  3. Deactivate GO1.
  4. Disable any `Trigger` + `Conditions` on GO2.
  5. Activate GO2.
  6. Wait the remaining 1/3.
  7. Emit a named signal.
- **Why it matters:** this is precisely what
  [INTEGRATION.md item #20](INTEGRATION.md) flagged as the missing
  piece for `AnisDialogueAffection02`'s Mall ↔ Cinema transition (Scene7 /
  Scene8). Right now the pack emits the fade signals but skips the
  level swap, so the visual lands wrong.
- **Fit in ModForge:** new node action, parallel to `EmitSignalDelayed`:
  ```jsonc
  { "type": "TransitionLevels",
    "params": {
      "fromLevel":  "vanilla:54_Mall",
      "toLevel":    "vanilla:64_Cinema",
      "signal":     "FadeOut2025",
      "seconds":    "1.5"
    } }
  ```
- **Effort:** small. ~50 lines in `ActionRuntime.cs` + LevelOptions
  resolution (we already have `ResolveLevelTarget` from
  `LevelRandom`).
- **Recommendation:** **yes, port.** Closes a known open gap.

### ~~A2. `Fade` alpha tween → `FadeSprite` action~~ ✅ SHIPPED

- **Shipped as:** the `FadeSprite` action, which takes a target through the
  shared target picker and fades it to a chosen opacity over a duration.

- **Where it lives:** `MainStory.Fade(SpriteRenderer, float duration, float to)`.
- **What it does:** smooths a `SpriteRenderer.color.a` from current to
  target over `duration` seconds. A standard alpha tween.
- **Why it matters:** any pack-authored visual transition (e.g.
  fading a CG overlay or bust without a Leave child) currently has
  to use `SetGameObjectActive` (instant) or rely on the pre-existing
  GC2 fade signals (`fadeInSignal` etc., screen-wide).
- **Fit in ModForge:** new node action:
  ```jsonc
  { "type": "FadeSprite",
    "params": {
      "path":     "4_CG_Manager-Sexy/pack:Foo.MyScene/Core/Art",
      "to":       "0",
      "seconds":  "0.5"
    } }
  ```
- **Effort:** tiny. Fade is ~10 lines.
- **Recommendation:** **yes, port** if you anticipate using it.
  Otherwise defer — it's small enough to add when needed.

### ~~A3. `FindInActiveObjectByName` / `Transform.FindInActiveObjectByName`~~ ✅ SHIPPED

- **Shipped as:** the plugin's own `TransformExtensions` (an `includeInactive`
  descent) plus `Resources.FindObjectsOfTypeAll` where a scene-wide sweep is
  needed. Authors see it as "objects that start switched off are still
  found", which is what lets an action turn on something never yet visible.

- **Where it lives:** `Core.cs` + `TransformExtensions.cs`.
- **What it does:** recursive transform search that includes
  **inactive** children. Standard `GameObject.Find` only sees active
  ones.
- **Why it matters:** every factory in the pack (`SceneFactory`,
  `WallpaperFactory`, `MusicFactory`, `SfxFactory`) currently looks up
  prototype GOs by hardcoded path strings like
  `"Desktop/Wallpaper/Wallpaper (0)"` — works only if every ancestor
  is active. The wallpaper builder hit a subtle bug here once
  (the wallpaper panel isn't normally active at scene load).
- **Fit in ModForge:** internal utility — add a static
  `TransformExtensions.FindRecursiveIncludingInactive(this Transform, string)`
  in the plugin assembly. Factories switch to it where path-string
  lookup is fragile.
- **Effort:** trivial. ~15 lines.
- **Recommendation:** **yes, port** as an internal utility. Cheap
  reliability win; no manifest changes.

### A4. `CreateModHeader` → ModForge main-menu badge

- **Where it lives:** `Core.cs` + `Core.menuModHeader` static GO.
- **What it does:** injects a text line on the GameStart main menu
  banner saying "Androids Mod {pluginVersion} for 1.8E". Turns red
  if the trailing game-version segment doesn't match the original
  menu text — a sanity check that you're running the targeted game
  build.
- **Why it matters for ModForge:** a generalised version
  ("SMSModForge — N pack(s) loaded · v1.8E") on the GameStart scene
  would be hugely useful for diagnosing pack-loading issues at a
  glance. Today, if a pack doesn't load (manifest typo, missing
  bundle, etc.) the only signal is BepInEx console output, which is
  off by default.
- **Fit in ModForge:** plugin-side, no pack authoring. The plugin
  watches `SceneManager.sceneLoaded` for `GameStart`, walks to the
  menu TMP text, appends our banner. Could also include click-to-open
  the BepInEx log if a pack failed to load.
- **Effort:** small. ~30 lines + version-check heuristic.
- **Recommendation:** **yes** — it's the single best low-effort
  reliability improvement.

---

## Tier B — useful but bigger or more debatable

### B1. Mod-shops subsystem

- **Where it lives:** `Places.cs` `ModShops` region —
  `InitializeModShops`, `AddItemToGiftStore`, `ActivateShop`,
  `DisableModShops`, `UpdateGiftStoreItemVisibility`.
- **What it does:** clones the vanilla `GeneralStore` as `GiftStore`
  under a `ModShops` container. Items are added with name, price,
  PNG. Each item's visibility is gated by a `Gift_<Name>` proxy var.
- **Why it could be portable:** the **mechanism** (clone store,
  populate items, gate by variable, hook close button) is generic.
  A future pack might want to add e.g. a music-disc store, a poster
  store, an outfit-purchase store. None of those are gift-specific.
- **Caveat / open question:** you previously said gifts stay in
  SMSAndroids ("Gifts seem like a more complex task that should
  remain in the plugin side"). Was that about the gift→affection
  coupling specifically, or the shop UI too?
  - If shop UI = portable, gift→affection coupling = SMSAndroids,
    then a pack-side `ShopDef` (key, items, gating var per item)
    works fine and you keep the SMSAndroids gift-give flow
    untouched.
  - If shops = SMSAndroids, then no port. Easier.
- **Effort:** medium. Model + ShopFactory + per-item config + UI
  layout. ~200 lines.
- **Recommendation:** **discuss.** Depends on your answer to the
  open question above.

### B2. `ChangeOutfitDelayed` → `SetActorBustDelayed` action

- **Where it lives:** `Core.cs:1175` + private coroutine.
- **What it does:** waits `delay` seconds, then deactivates one GO,
  activates another, and writes a SaveManager string.
- **Why partial fit:** the SaveManager write is bust-system-specific
  (`HarborHome_Outfit_Anis = "Swim"` etc.). The bust swap part is
  already covered by `SetActorBust` action + the existing
  `Wait` action — you can do
  ```
  Wait 1.5
  SetActorBust actor=anis bustKey=AnisSwim
  ```
  to get the same thing.
- **Recommendation:** **skip.** Already expressible with existing
  actions composed.

### ~~B3. `RandomChildActivator` MonoBehaviour~~ ✅ SHIPPED

- **Shipped as:** a generic reimplementation in the plugin's `PackComponents`,
  alongside `FadeInSprite`, `FadeOutSprite`, `BlinkingSprite` and
  `DisappearAfterDelay`. The names match the vanilla ones so authors can
  recognise them; none of the code is shared with SMSAndroids.

- **Where it lives:** `RandomChildActivator.cs`.
- **What it does:** on `OnEnable`, activates one random child;
  on `OnDisable`, deactivates all. `pickNewOnEnable` flag controls
  re-rolling.
- **Why portable:** generic — works for any GO with a list of
  variant children that should random-select on activation.
- **Fit:** would need a way for a pack to attach the component to
  a specific GO. Options:
  - New `PlaceDef.NPCSlots` field that takes a list of slot definitions,
    each with a `randomVariants` flag.
  - Or a generic `AttachComponent` mechanism in the pack manifest.
  - Or just expose the MonoBehaviour and let the pack action-system
    attach it at runtime: a new action `AttachRandomChildActivator
    path="<scene-path>"`.
- **Recommendation:** **defer.** Useful but the attachment mechanism
  is the design question, and we don't have a concrete pack use case
  yet. Re-evaluate when someone wants it.

### B4. New-game / sleep / after-sleep hooks

- **Where it lives:** `SaveManager.cs` Update body — detects sleep
  (`Core.afterSleepEvents` + `Core.savedUI` active) and runs
  the daily turnover.
- **Why it might matter:** packs might want to trigger something
  specifically when the player sleeps (different from "day changed"
  — the player can sleep multiple times per session, only the first
  per day advances the day). Today we have `Daily` refresh mode on
  variables; no per-sleep hook.
- **Fit:** could add a new `LevelActive: vanilla:<bedroom>` +
  `GameObjectActive` condition combo on dialogues to detect sleep
  via the afterSleepEvents UI — actually, we have that already
  (`AnisRandomSleep01` uses it). So sleep events are already
  expressible by dialogue start conditions, no new mechanism needed.
- **Recommendation:** **skip.** Existing condition vocabulary covers
  it.

### B5. `<size=X%>` SFX volume modulation

- **Where it lives:** `MainStory.GetSizeMultiplierAtPosition`.
- **What it does:** scans backwards from a SFX text-pattern match
  for the nearest `<size=N%>` TMP tag, multiplies the SFX volume by
  N/100. So `<size=140%><b>*THWACK*` plays louder than `<size=60%>*plap*`.
- **Why portable:** trivial polish but a quality touch.
- **Already noted:** DIALOGUE_AUDIT.md §4.3 flagged this.
- **Fit:** ~15 lines in `SfxRegistry.FireMatchingPatterns` — peek
  back for `<size=`, parse the number, apply.
- **Recommendation:** **yes, easy port** if you care about parity.

---

## Tier C — game-specific, won't port

For completeness so we don't revisit:

| Function | Reason |
|---|---|
| `IncrementAffectionForGiftIfLiked` | Gift-system-coupled; stays per earlier call |
| `SetAndSyncGiftVariable` | Same as above |
| `DisableAllActiveBustChildren` | Bust-internal helper |
| `ChangeBustSortingOrder` | Already covered by `SetSpriteFocus` |
| Voyeur tier helpers (`AllStarter*Found` etc.) | Game-specific; INTEGRATION.md #1 path A keeps in SMSAndroids |
| `GetBadWeather` | Reads GC2 `rainy-day`/`snowy-day`; pack can read those directly via `GameVariableEquals` if needed |
| `ChooseVoyeurTarget` | Game-specific; INTEGRATION.md #1 |
| HH roaming timer logic | Game-specific |
| BreastPhysics / Squeeze / Drag / LotionTrail shaders | Minigame; Tier 3 |
| Lottery numbers | Covered by `DailyRandom` variable mode |
| Day-of-week alternates | Declined as PORTING_ROADMAP 2B |

---

## My recommendation in one paragraph

**Land A1, A3, A4, and B5 in one sweep** — they're all small (<100
lines each), they each close a real gap (transition fidelity for
Affection02, factory-lookup reliability, pack-loading
diagnosability, SFX size-tag parity), and none of them touches
manifest schema in a way that requires pack-side authoring. A2
(`FadeSprite` action) is also tiny but worth deferring until we have
a concrete use case so we don't over-build. B1 (mod shops) needs
your call on whether the shop UI is in or out of pack scope before
we touch it. Everything else either stays in SMSAndroids or is
already covered.

What do you think?
