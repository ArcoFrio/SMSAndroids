# SMSAndroids ↔ SMSModForge — Pending Integration Work

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


A running ledger of every cross-mod handshake, bridge, or compatibility
patch that is still **open** between the legacy `SMSAndroids` plugin and
the new `SMSModForge` editor/`SMSModForge.PackPlugin` runtime.

The goal of the SMSModForge project is to eventually replace SMSAndroids
entirely with pack-authored content. Until then both mods run side by
side, and the items below are the touch points where they have to agree.
Anything checked off has either landed or has a tracking PR; anything
unchecked is real work that's been deferred.

> **Read order:** the **Critical** section is what blocks day-to-day
> behaviour today. **Shared state divergence** is the systemic risk —
> the pack writes its own copies of variables SMSAndroids tracks. The
> rest documents specific features that punted on integration.

---

## Critical (blocking pack functionality today)

### 1. Voyeur target picker handshake *(introduced in Batch 2)*

- **What's broken:** the 18 per-character voyeur dialogues are
  authored at `SMSAndroidsPack/modpack.json` (`*DialogueSecretbeach01`),
  each gated on `Voyeur_NextTarget == <Char>` *and*
  `Voyeur_Seen<Char> == false`. **Nothing currently writes
  `Voyeur_NextTarget`**, so the dispatcher never picks any of them.
- **Why pack-side picking is hard:** the legacy `MainStory.cs:2604-2610`
  picks via `Random.Range(0, voyeurTargetsLeft.Count)` over an
  eligibility list that is rebuilt by `MainStory.cs:114-142` on every
  `CoreGameScene` load and re-rebuilt by `SaveManager.cs` after the
  player sleeps. The tier ladder (`starterVoyeurTargets` →
  `gSVoyeurTargets` → `fullVoyeurTargets`) and the per-character
  `Voyeur_Seen` filter together don't reduce to a static pack
  expression.
- **Resolution paths:**
  - **(A) Hybrid (recommended near-term):** SMSAndroids retains the
    tier/eligibility/picker. When `sBDialogueMain`'s finisher rolls a
    voyeur win, instead of starting `Dialogues.<char>SecretbeachVoyeur01Dialogue`
    it writes the chosen target into our pack variable. Needs ~10
    lines in SMSAndroids: a `SMSModForge.PackPlugin` reflection helper
    (`Plugin.Instance.SetPackVariable(packId, name, value)`) plus a
    call site replacing the current `dialogueToActivate = ...`
    assignment.
  - **(B) Pack-native:** add a `PickRandomFromList` action
    (params: `source` = comma-separated, `target` = var name). Then
    Batch 3's `sBDialogueMain` build computes the eligible list with
    a chain of conditional `SetVariable`s and calls
    `PickRandomFromList target=Voyeur_NextTarget`.
- **Owner / status:** deferred. Voyeur dialogues are authored but
  inert until either (A) or (B) lands.

### 2. `Dialogues.dialoguePlaying` coordination

- **What's broken:** SMSAndroids' `Dialogues.cs` exposes
  `dialoguePlaying` (mod dialogues) and `dialoguePlayingVanilla` (any
  vanilla dialogue under `8_Room_Talk`) and mirrors both to the GC2
  proxy variable `Checks_Dialogue-is-playing`. Most of
  `MainStory.cs`'s per-frame triggers gate on
  `!Dialogues.dialoguePlaying`. **Pack-built dialogues bypass this
  flag entirely** — when a pack voyeur or event plays, SMSAndroids
  thinks no dialogue is running and may queue a competing one.
- **Resolution:** the pack dispatcher needs to either
  - set `Dialogues.dialoguePlaying = true` via reflection when a pack
    dialogue starts, and clear it on stop, **or**
  - mirror its own "any pack dialogue active" state into
    `Checks_Dialogue-is-playing` directly (so vanilla / SMSAndroids
    see one canonical flag).
- **Status:** deferred. Hasn't caused a visible bug yet because Batch
  1 events ran while the player was in roomtalks that don't host
  competing SMSAndroids logic, but any Batch 3 Default-dialogue
  conversion at Harbor Home or Mall will hit this fast.

### 3. `Voyeur_Seen<Char>` write store divergence

- **What's broken:** the pack writes `Voyeur_Seen<Char> = true` to its
  own per-pack save file (`Saves/<packId>.json`) at the voyeur
  dialogue's finisher. SMSAndroids reads `SaveManager.GetBool("Voyeur_Seen{character}")`
  from `SMSAndroidsCore_Save.txt` in `MainStory.cs:114-142`,
  `Schedule.cs`, the HHTalk panel, and elsewhere. The two stores
  drift the moment a voyeur dialogue runs pack-side.
- **Resolution paths:**
  - **(A) Pack mirrors writes back to SaveManager** for the overlap
    set (see "Shared state divergence" table below). The plugin would
    need a SMSAndroids reflection write helper invoked from every
    `SetVariable` whose name is in the mirror list.
  - **(B) SMSAndroids reads from the pack store** via a reverse
    helper (`Plugin.Instance.GetPackVariableBool(packId, name)`)
    wherever it currently calls `SaveManager.GetBool` on a shared
    name. More invasive change in SMSAndroids.
  - **(C) Authority moves to one side per name** — e.g. `Voyeur_Seen*`
    becomes pack-canonical, SMSAndroids reads through a bridge for
    those specifically. Easier on SMSAndroids' side, harder to
    enumerate.
- **Status:** deferred. Same blast radius as (#2) — currently latent
  because no pack dialogue has fired the write path in actual play.

---

## Shared state divergence

Every variable in the table below exists in **both** SaveManager
(SMSAndroids' `defaultValues`) **and** `SMSAndroidsPack/modpack.json`.
They will diverge as soon as either side writes. The mirror-or-defer
decision in critical item #3 has to cover every row here.

### 3a. Boolean flags (overlap with `Core.saveManagerBoolMappings` + ad-hoc)

| Variable                              | SMSAndroids writer            | Pack writer (planned)            | Pack reader |
| ------------------------------------- | ----------------------------- | -------------------------------- | ----------- |
| `Voyeur_Seen<Char>` × 18              | `MainStory.cs:2404`           | Batch 2 voyeur finishers ✅      | voyeur gates, future Default/Random gates |
| `Event_Seen<Char><Place>01` × 19      | `MainStory.cs` per-event      | Batch 1 event finishers ✅       | event gates |
| `Event_SeenIt01`                      | (none yet)                    | reserved for Batch 3              | — |
| `Affection_Anis_Seen{1,2,3}`          | `MainStory.cs` affection arcs | Batch 3 affection dialogues       | Schedule, HHTalk |
| `MountainLab_FirstVisited`            | `Places.cs`                   | (read only, planned)              | tier eligibility, dialogue gates |
| `MountainLab_FirstVisitor`            | `MainStory.cs`                | (read only, planned)              | dialogue gates |
| `MountainLab_GKExplanation`           | `MainStory.cs`                | (read only, planned)              | voyeur tier promotion |
| `SecretBeach_FirstVisited`            | `MainStory.cs:2206`           | Batch 3 SBMain first-visit finish | SBMain gating |
| `SecretBeach_GKSeen`                  | `MainStory.cs` SBGK finisher  | Batch 3                           | dialogue gates |
| `SecretBeach_UnlockedLab`             | `MainStory.cs`                | Batch 3                           | navigator unlocks |
| `HarborHome_Bought`                   | `Places.cs` ModShops          | (stays in SMSAndroids?)           | dialogue gates |
| `HarborHome_FirstVisited`             | `Places.cs`                   | (stays in SMSAndroids?)           | — |
| `HarborHome_Visit_<Char>` × 21        | `Schedule.cs` HH roaming      | (stays in SMSAndroids?)           | HH dialogue gates |
| `HarborHome_Slept`                    | `SaveManager.cs` post-sleep   | (stays in SMSAndroids?)           | next-day gating |
| `HarborHome_SleepCD`                  | `MainStory.cs` HH bedroom     | Batch 3                           | bedroom dialogue gate |
| `DailyProc_Anis_Massage`              | `Minigames.cs`                | (stays in SMSAndroids?)           | — |
| `DailyProc_Anis_Shower`               | `MainStory.cs:611`            | Batch 3 random HH dialogues       | random dialogue gate |
| `DailyProc_Anis_TV`                   | `MainStory.cs:779`            | Batch 3                           | TV dialogue gate |
| `Gift_*` × 9                          | `Places.cs` gift shop         | (stays in SMSAndroids?)           | gift UI gates |
| `GiftShop_FirstVisited`               | `Places.cs`                   | (stays in SMSAndroids?)           | — |
| `GiftShop_BuildCounter` (Int)         | `SaveManager.cs` daily bump   | (stays in SMSAndroids?)           | voyeur tier promotion |

### 3b. Numeric clamps

| Variable             | Type | Range  | SaveManager clamps? | Pack clamps?    |
| -------------------- | ---- | ------ | ------------------- | --------------- |
| `Affection_<Char>` × 21 | Int  | 0..5   | yes (`SaveManager.SetInt`) | not yet — needs `minValue=0, maxValue=5` set on each entry |
| `SecretBeach_RelaxedAmount` | Int | 0..N | no                  | no clamp needed |

The pack's `Affection_*` variables need their `min/max` fields
populated to keep parity with SaveManager's clamp. Otherwise any
pack-driven `IncrementVariable Affection_Anis +1` could push past 5
when SaveManager would have refused.

### 3c. GC2 proxy variables (the `SyncVanillaToProxyVariables` path)

SMSAndroids maintains a "Proxy Variables" GNV asset that mirrors:

- `Beer` → `Gift_Beer` and ~8 other gift booleans (read by GC2
  vanilla dialogues).
- `Cash` (GC2 numeric) → `Gameplay_Cash`.
- `Affection_<Char>` SaveManager int → `Affection_<Char>` GNV.
- `Voyeur_Seen*` / `Affection_Anis_Seen*` / `Gift_*` / `HarborHome_*`
  → matching GNV booleans.

Pack writes only go to the pack store today, so the GNV layer stays
stale. **Any vanilla dialogue branching on these proxies will read
old values** once the pack starts driving them. Resolution falls out
of critical #3.

### 3d. `Location_<Char>` schedule mirror (verified ✅)

Per `SMSAndroids/CLAUDE.md`: "Mirrors all `<char>Location` strings
into the proxy variable `Location_<Char>` (string)." Batch 1 event
dialogues gate on these via `GameVariableEquals` and it works —
no bridge needed, this row is informational only.

---

## Asset / GameObject ownership

### 4. Scene CG GOs under `4_CG_Manager-Sexy`

- **What's owned by SMSAndroids:** `Scenes.cs` constructs every
  vanilla event / story / voyeur scene as a child of
  `4_CG_Manager-Sexy`. Including 72 voyeur CGs
  (`<Char>VoyeurSecretbeachScene{01..04}` × 18 characters).
- **Pack reference:** Batch 2 voyeur dialogues hit these via
  `SetGameObjectActive` with absolute paths. The Batch 1 event
  dialogues that referenced these scenes do so through pack-side
  `ActivateScene` actions whose registry entries point at the
  SMSAndroids-built GOs.
- **What blocks phase-out:** if SMSAndroids stops creating these,
  the pack needs to ship them itself — either via Unity-side scene
  factory work or by extending `SceneDef` to optionally point at an
  existing GO path.

### 5. Bust `Leave` child activation race

- **What it is:** SMSAndroids' MainStory.cs literally writes
  `Characters.<bust>.transform.Find("MBase1").Find("Leave").gameObject.SetActive(true)`
  in ~40 places (every "graceful exit" beat). The pack's `LeaveBust`
  action does the same lookup against the actor's *currently* active
  bust.
- **Where it bites:** Elegg's Voyeur Scene 5/8 (`MainStory.cs:2336-2339`)
  calls Leave on a bust GO that may not match what the actor registry
  considers current (Scene 5 leaves `EleggSwim` after `ChangeActiveBust`
  already swapped to `EleggSwimSlip`). Batch 2 ports these as
  literal `SetGameObjectActive` calls on the explicit GO path, which
  matches vanilla behaviour but **bypasses the pack's actor registry
  tracking**. If both mods drive the same bust the registry will get
  stale.
- **Resolution (when phasing out SMSAndroids):** review every
  `SetGameObjectActive 2_Bust_Manager/...` action in the pack and
  convert to `LeaveBust` / `SetActorBust` once the actor registry is
  the only owner.

### 6. NPC GOs under each Harbor Home room

- `Schedule.cs` swaps Anis HH variants in/out per frame via
  `RandomChildActivator`. None of this is in the pack. Any Batch 3
  HH dialogue conversion has to **not** stomp Anis's position GO
  while playing — either avoid touching HH NPC GOs from pack actions,
  or coordinate with Schedule.

---

## Signal & flow bridges

### 7. GiftUI open/close (deferred since session start)

- **User direction (verbatim):** *"we can make a sort of crossover by
  signaling to SMSAndroids when to open the GiftUI and signaling back
  to SMSAndroidsPack when it's closed."*
- **Concretely needed:**
  - **Pack → SMSAndroids:** action `OpenGiftUI` (params: `target` actor
    key). Emits a GC2 signal `OpenGiftUI` that SMSAndroids subscribes
    to in `Dialogues.cs` (which already owns `giftUI`).
  - **SMSAndroids → Pack:** when the player picks an item or dismisses,
    SMSAndroids writes the result to pack variables:
    - `Gifting_Target` (string, already a proxy variable today)
    - `Gifting_Gifted` (bool, already proxy)
    - `Gifting_Item` (string)
    pack-side gift dialogues gate on these.
- **Status:** no action type added, no SMSAndroids change made.
  Affects affection-up gift sequences (Batch 3).

### 8. SFX text-pattern playback

- **What SMSAndroids does:** `Dialogues.cs` `textToSFX` table maps
  regex-ish text patterns to audio clips; `MainStory.cs.OnDialogueLineStart`
  fires the right clip on each line.
- **Pack-side:** no equivalent. Batch 1's converter dropped
  `InstructionCommonAudioSFXPlay` instructions with a warning ("SFX
  instruction deferred").
- **Resolution paths:**
  - Add a `PlaySFX` pack action (params: `clip` or `pattern`).
  - Or let SMSAndroids continue to hook GC2's per-line callback and
    drive it from text — works as long as the pack uses GC2 dialogue
    runtime (which it does).
- **Status:** option B is the current default by accident — the pack
  uses GC2's Dialogue, SMSAndroids' `OnDialogueLineStart` is still
  bound, so SFX *should* fire on pack-built dialogues too. Needs
  verification.

### 9. `evaluatingLevelDialogue` reentrancy guard

- **What it is:** `MainStory.cs` sets `evaluatingLevelDialogue = true`
  before `Invoke(CheckAndStartVanillaDialogue, 0.65f)` so the next
  frame doesn't re-enter the same conditional and queue the dialogue
  twice.
- **Pack side:** the dispatcher uses its own internal "dialogue
  active" tracking, so it's not affected. But if SMSAndroids' loop
  evaluates a pack dialogue's roomtalk while the pack dispatcher is
  about to fire that dialogue, both mods could race.
- **Resolution:** subsumed by critical #2 (one shared "dialogue
  playing" flag).

### 10. `Schedule.<char>Location` writes from pack

- **What's missing:** Batch 1 event finishers update Schedule by
  setting per-character location through SMSAndroids' Schedule API
  (e.g. Tove returns to her default after `ToveDialogueTrail01`
  finishes). The pack currently has no Schedule write path — it only
  *reads* `Location_<Char>` via the proxy.
- **Resolution:** add a `SetSchedule` pack action that writes through
  to SMSAndroids' Schedule via reflection (`Schedule.SetCharacterLocation(name, loc)`).
- **Status:** none of the converted Batch 1 dialogues actually need
  this — they all rely on the schedule pinning that already happens
  through SMSAndroids. Confirmation pending the first live test.

---

## Random-gate pre-wires (no SMSAndroids change needed, just tracking)

### 11. `RandomNumMall` / `RandomNumMLRoomAnis` *(pre-declared in Batch 2)*

- **What:** two `LevelRandom` pack variables in `modpack.json` that
  mirror SMSAndroids' `Places.randomNumMall` / `randomNumMLRoomAnis`
  (`Places.cs:561-562`).
- **Used by:** Batch 3 will reference these from
  `anisRandomDialogue67` and `anisRandomDialogueLabRoomChill01`'s
  start conditions (`<= 30` and `<= 10` respectively).
- **Open question:** SMSAndroids' static fields still exist and still
  re-roll. After Batch 3 the pack variables become canonical and the
  static fields are dead weight. **Don't delete them in SMSAndroids
  until Batch 3 has been verified live** — they're still referenced
  by `MainStory.cs:415` and `:937`.

### 12. `Voyeur_NextTarget`, `SecretBeach_ActionToday` *(introduced in Batch 2)*

- Both are non-persisted pack variables. `SecretBeach_ActionToday`
  mirrors `MainStory.actionTodaySB` (a session-only `bool` in
  SMSAndroids). The pack reset path is `refreshMode: Daily`, which
  is redundant for a non-persisted var but documents intent.
- The handshake to populate `Voyeur_NextTarget` is critical #1.

---

## Editor-side TODOs (no SMSAndroids interaction)

### ~~13. `PickRandomFromList` action~~ ✅ RESOLVED

Landed in Batch 3a (task #27). `NodeActionTypes.PickRandomFromList`
takes `source` (literal CSV or `$varName`) and `target` (name of the
variable to write the pick into). `SBDialogueMain`'s `[Relax]` exit
uses it to pick a voyeur target from `$Voyeur_EligibleList`. Critical
item #1's path (B) is now implementable — the missing piece is the
external population of `Voyeur_EligibleList`.

### 14. Confirm `LeaveBust` actor lookup vs `SetGameObjectActive` literal
calls coexist cleanly

Already noted under #5. Live test on Batch 2 voyeur dialogues is the
verification step.

---

## New gaps surfaced by Batch 3

### 15. `OpenGiftUI` / `OpenGiftStore` signals are emitted but unbound

The Default-dialogue Scene 5 markers emit:
- `OpenGiftStore` from `ClaireDialogueDefault` (vanilla: `Places.ActivateShop(giftStore)`)
- `OpenGiftUI` from `AnisDialogueDefault` (vanilla: `Dialogues.giftUI.SetActive(true)` + `Gifting_Target = "Anis"`)

No code listens for them today. SMSAndroids needs a small subscribe in
`Dialogues.cs` / `Places.cs` to wire these signals to their existing
gift UI / gift store activations. This is the concrete realisation of
critical-tier item #7's bidirectional bridge.

### 16. `Voyeur_EligibleList` is read but never written

`SBDialogueMain`'s `[Relax]` exit calls `PickRandomFromList source=$Voyeur_EligibleList`,
but nothing writes that variable today. Required write logic (already
exists in SMSAndroids' `MainStory.cs:114-142` and `SaveManager.cs` after-sleep):
1. Compute tier (starter / GS / full) from `MountainLab_GKExplanation` +
   `GiftShop_BuildCounter >= 2`.
2. Filter by `Voyeur_Seen<Char> == false`.
3. Join with commas, write to pack variable `Voyeur_EligibleList`.

Simplest near-term resolution: ~10-line addition to SMSAndroids that
rebuilds the list on the same events it already rebuilds `voyeurTargetsLeft`,
writing via a reflection helper into `Plugin.Instance._contexts[i].Vars.Set`.
This is the concrete realisation of critical-tier item #1's path (A).

### 17. Voyeur tier promotion gates on `MountainLab_GKExplanation` /
       `GiftShop_BuildCounter` — both stay in SMSAndroids store

Pack dialogues read these via `VariableEquals` / `VariableGreaterOrEqual`,
which goes through the pack store. SMSAndroids writes them through
SaveManager. Same divergence as critical item #3 — needs the mirror
or the bridge.

### 18. Random `DailyProc_Anis_Shower` Athletic trait gate

`AnisRandomShower01` vanilla also requires `newtrait-Athletic >= 5`
(a GC2 number variable). Pack-side condition vocabulary lacks
`GameVariableNumberGreaterOrEqual` — only `GameVariableEquals` (string
equality) exists. The dialogue ports without the trait gate, so it will
fire whenever the other conditions are true regardless of Athletic.

To fix: add `GameVariableNumberGreaterOrEqual` / `LessOrEqual`
condition types to `NodeConditionDef` + `ConditionEvaluator`,
reading the GC2 number via `GameVariableBridge.GetNumber`.

### 19. `AnisRandomMovie01` "nobody else in living room" gate

Vanilla requires `!Schedule.IsAnyoneAtLocationBesides("HarborHomeLivingRoom", "Anis")`.
No pack equivalent. Dialogue ports without this gate, so it fires
whenever the other conditions are true regardless of who else is in
the room.

To fix: either expose this check via a new condition type that scans
`Location_<Char>` proxy variables, or have SMSAndroids maintain a
`HHLivingRoom_AnyoneElse` bool pack variable.

### 20. Mall ↔ Cinema level transition in Affection02

Vanilla `AnisDialogueAffection02` Scene7 / Scene8 cross-activate
`Places.levelCinema` with the camera-tween form of `EmitSignalGameObjectDelayed`.
The pack port only emits the fade-in / fade-out pair; the level swap
is dropped. Affection02 will run but the player won't be visually
transported to the cinema scene — they'll stay at Mall.

To fix: add a `SetLevelActive` action that resolves a level token and
activates it (matching `LevelActive`'s resolution path). Pair it
with the existing fade signals.

### 21. `Schedule.<char>Location` writes from finishers

Many dialogue finishers in vanilla write `Schedule.anisLocation = "..."`
to relocate the character after the dialogue. Pack finishers can't.
Dialogues that need this (AnisRandom67 → MountainLabRoomNikkeAnis,
Affection01/02 → MountainLabRoomNikkeAnis) just don't reposition Anis.

To fix: implement INTEGRATION.md #10's `SetSchedule` action — calls
`Schedule.SetCharacterLocation(actor, location)` via reflection.

---

## Cross-plugin pack-variable API (canonical handshake)

`Plugin.Instance` exposes the public surface other BepInEx plugins
use to read / write / subscribe to pack variables. This is the
replacement for SMSAndroids' "Proxy Variables" GNV mirror layer —
no asset bundle dependency, no AssetBundle-managed GNV asset, just
direct access to the in-memory `PackVariableStore`.

### Reads (all return a default when the var doesn't exist)

```csharp
var p = SMSModForge.PackPlugin.Plugin.Instance;
string  s = p.GetPackVariableString("SMSAndroidsPack", "Gifting_Target", "");
bool    b = p.GetPackVariableBool  ("SMSAndroidsPack", "Voyeur_SeenAnis", false);
int     i = p.GetPackVariableInt   ("SMSAndroidsPack", "Affection_Anis", 0);
float   f = p.GetPackVariableFloat ("SMSAndroidsPack", "SomeFloat", 0f);
var lst   = p.GetPackVariableList  ("SMSAndroidsPack", "Voyeur_EligibleList");
```

### Writes (return true if the pack was found)

```csharp
p.SetPackVariable      ("SMSAndroidsPack", "Gifting_Target", "Anis");
p.SetPackVariableBool  ("SMSAndroidsPack", "Voyeur_SeenAnis", true);
p.SetPackVariableInt   ("SMSAndroidsPack", "Affection_Anis", 3);
p.SetPackVariableFloat ("SMSAndroidsPack", "SomeFloat", 0.5f);
p.AddToPackList        ("SMSAndroidsPack", "Voyeur_EligibleList", "Yan");
p.RemoveFromPackList   ("SMSAndroidsPack", "Voyeur_EligibleList", "Anis");
p.ClearPackList        ("SMSAndroidsPack", "Voyeur_EligibleList");
```

Writes that target a variable not declared in the manifest still
land in the in-memory dict but are not clamped or persisted — they're
an escape hatch, not the recommended path.

### Discovery

```csharp
foreach (var packId in p.GetLoadedPackIds()) { ... }
bool has = p.HasPack("SMSAndroidsPack");
bool hasV = p.HasPackVariable("SMSAndroidsPack", "Voyeur_SeenAnis");
foreach (var name in p.EnumeratePackVariables("SMSAndroidsPack")) { ... }
```

### Change subscription

```csharp
SMSModForge.PackPlugin.Plugin.OnPackVariableChanged += (packId, name, oldVal, newVal) =>
{
    if (packId == "SMSAndroidsPack" && name == "Gifting_Gifted" && newVal == "true")
    {
        // gift-UI bridge: trigger the in-game gift sequence
    }
};
```

The event fires only on actual value changes (idempotent writes
don't re-notify). The new value is already committed before the
event fires, so handlers that re-query through `GetPackVariable*`
see the post-change state.

### Pre-condition

`Plugin.Instance` is non-null after this plugin's `Awake` has run,
and `_contexts` populates after `LoadAllPacks` finishes (early in
`CoreGameScene`). Subscribers in another plugin should add their
handlers in `Awake` — they'll fire from the moment the first
pack-driven write happens.

### Resolves INTEGRATION items

- **#1 (voyeur picker handshake):** SMSAndroids can write
  `Voyeur_NextTarget` directly via `SetPackVariable` instead of
  starting the legacy dialogue. Combined with the SBMain `[Relax]`
  exit's `PickRandomFromList` action, the voyeur flow becomes pack-driven.
- **#3 (write-store divergence):** SMSAndroids reads `Voyeur_Seen*`
  / `Event_Seen*` / `Affection_*` directly from the pack store
  rather than `SaveManager.GetBool`. One source of truth, no mirror.
- **#7 / #15 (gift UI handshake):** SMSAndroids subscribes to
  `OnPackVariableChanged` for `Gifting_*` writes and the
  `OpenGiftUI` / `OpenGiftStore` signals; writes the result back via
  `SetPackVariable` when the UI closes.
- **#17 (voyeur tier promotion):** SMSAndroids reads
  `MountainLab_GKExplanation` / `GiftShop_BuildCounter` from the
  pack store when computing the eligible list.
- **#10 / #21 (Schedule writes):** SMSAndroids reads pack variables
  for schedule rule inputs; pack `SetSchedule` action calls
  `Schedule.SetCharacterLocation` directly via reflection.

These six items shift from "needs design" to "needs ~10 lines of
SMSAndroids edits each."

---

## Resolved

- **#13 PickRandomFromList action** (Batch 3a). New action type for
  pack-side random selection. Used by SBMain.

- **Per-character SaveManager mirror for `Voyeur_Seen*` writes**
  (Batch 2). Pack writes to its own store; SaveManager still
  authoritative. Critical item #3 remains *partially* open — pack
  writes work but reads from SMSAndroids will see stale values.

- **EmitSignalDelayed action** (Batch 2). Fire-and-forget delayed
  signal emission, matching `Core.EmitSignalDelayed`.

- **Voyeur tier system observability** — by enumerating `Voyeur_Seen<Char>`
  in Batch 2 we documented the full tier set for future pack-native
  reimplementation.

---

## How to update this file

When something lands:

1. Move the item from its section to a new **Resolved** section at
   the bottom with the resolving commit / PR / build noted.
2. If a new integration point is discovered while editing the pack
   or SMSAndroids, add it under the most relevant section above.
3. Items that turn out to be non-issues after verification get a
   `~~strike-through~~` rather than deletion — preserves the
   investigation trail.

The point of this file is to keep handshakes from getting forgotten
when work pauses. If you find yourself thinking "wait, did we ever
wire X?", this file should answer.
