# In-game test of SMSAndroids + ModForge, 2026-11-01

ModForge 1.8.1 (repo HEAD, Debug build with its test bridge), SMSAndroidsCore (this repo, Debug with
`TestBridge.cs`), SMSAndroidsPack 0.6.1 as synced from the working folder on 2026-11-01 and exported with
ModForge's own exporter. Test copy `Starmaker Story 1.8E - Androids test`, test save = the author's slots 0 + 1
(Monday, early game: lab not found, Anis not met, Harbor Home not bought); later states set through the bridge.
How to run it again: [`Tools/Game/README.md`](../Tools/Game/README.md).

Statuses: **fixed** (done here), **proposed** (a pack JSON fix ready and tested, waiting for the author),
**ModForge** (in `SMSModForge/FROM-PACK-TESTING.md`), **author** (a design decision), **info**.

## What works

- **Loading.** Both mods load into the test save with no errors; SMSAndroids' whole load chain completes
  (core, busts, places, dialogues, schedule, story, minigame, visualizer); every one of the pack's 183
  variables is readable in the game.
- **Places.** All 31 pack places and the 4 vanilla extensions travel to the right level with no errors.
- **Schedules.** Each of the 18 lab characters, once met, is placed in her own lab room by her "found" rule
  (Monday outings such as Frima/Hotel, Neon/Temple, Viper/Villa happen instead at their 70%, as in the old
  `Schedule.cs`); Amber is placed once the lab is visited; Claire is placed in the shop once it is built; the
  world-map icon lookup ignores case, so the mixed `HospitalHallway` / `Hospitalhallway` spellings are harmless.
- **Harbor Home.** The invite ("Wanna drop by my house?", after Anis's affection route) moves Anis in within
  seconds; she roams between spots and the occupancy list follows; the talk panel lists her in her room and
  starts the right dialogue for the room (`AnisDialogueHHDefault` in kitchen/bedroom, the movie one in the
  living room, the pool one at the pool).
- **Dialogues.** All 86 dialogues played from their own place (several plays each, every choice in turn):
  no errors, no stuck line, no unfilled `{tag}` or `[PV:]`, no scene picture that draws nothing, and outside
  Harbor Home none leaves a bust behind (the Secret Beach and story ones re-checked). About half of all lines
  were reached (the rest sit behind random picks and conditions a test save cannot hold at once).
- **The day change.** Sleeping (payout, then a morning activity, which is when the game saves) runs ModForge's
  turnover: 36 daily variables reset, the rules resume, the pack is committed to the save; Tuesday's schedule
  rules then place the characters (Anis at the Mall).
- **The proposed fixes F1-F3** were run in the test copy as a patched export: the Harbor Home area passes
  (bust in every room, sprites kept after both outfit changes, no bust left after a talk, the gift reaction
  starts, the HUD comes back), and ModForge's validator reports nothing new for it.

## Pack (SMSAndroidsPack)

All fixed by `Tools/Game/pack_fixes_2026_11_01.py` (F1-F6), applied to the working folder on 2026-11-01 after
the `fixes`, `hh` and `behaviour` areas passed on the patched pack. The script is idempotent.

1. **Gifts never reached the after-gift dialogue - soft-lock.** *fixed (F1)* The gifting window's buttons never
   set `Gifting_Gifted`, so `AnisDialogueGift` never started and the HUD stayed hidden. Giving a gift now uses it
   up (the game's items through their own variables, the shop's through `Gift_*`), sets the flag, and the
   reaction plays.
2. **The after-gift dialogue raised no affection and set no daily lock.** *fixed (F1)* Its liked branches (beer,
   body oil, chocolate, sunglasses, wine) raise `Affection_Anis` by 1; its silent first beat sets
   `DailyProc_Gifting_Anis`, which already hides the gift option for the rest of the day.
3. **After a gift at Harbor Home her bust stayed beside her room sprite.** *fixed (F1)* The branches no longer
   Exit; they run on to a silent last root that leaves her bust only when she is at Harbor Home (in her lab room
   the bust is how she stands there). The disliked-gift fallback now says so in its conditions.
4. **`HarborHome_Outfit_Anis` had two vocabularies.** *fixed (F2)* Outfit names everywhere (default `AnisBase`;
   placements show the base pose for `AnisBase`/`AnisTopless`, the swim pose for `AnisSwim`); a rule converts an
   old save's `Default`/`Swim`.
5. **Harbor Home exits left Anis's bust standing.** *fixed (F3)*
6. **The gift shop soft-locked the game.** *fixed (F5)* It was built inside the gameplay HUD's canvas, which the
   opening dialogue keeps hidden: invisible shop, invisible close button, no way on. It has its own canvas now,
   closing it brings the HUD back, the leftover "Milky Coins / Tasha Coins" counters are gone, its close button
   shows its X (it was white on white), all nine items fit the panel, and an item you cannot afford says
   "Can't afford" (buying it already did nothing - the earlier "cash can go negative" was wrong).
7. **The movie had no movie.** *fixed (F6)* The game's three TV shows are rebuilt as pack dialogues
   (`HHMovieScifi`, `HHMovieFantasy`, `HHMovieNews`) from ModForge's extract of the game's own: their lines, their
   slides, the speakers' mouths, the show music, the game's own episode counters - advanced after each showing
   exactly as the game's after-show scripts do (Sci-fi and Fantasy 2 -> 0, News 8 -> 0), so the next watch, in
   either place, shows the next episode - and the news's `claudia-newsseen`. Nothing else of the game's state is
   carried over: no method calls, nothing that could move a story on (the author, 2026-11-01). Picking a show in
   `AnisDialogueHHDefaultMovie` plays it, uses up the day's movie (changing your mind no longer does), and
   `AnisRandomMovie01` follows it straight away (only after a show now, not on every visit). Checked: the
   counters went 0 -> 1, 0 -> 1 and 8 -> 0, `incoming-movie` was untouched, `claudia-newsseen` was set. One gap: the
   game zooms its Sci-fi second slide per episode, which a pack cannot do yet (ModForge #9) - the rebuilt show
   shows the whole slide.
8. **The topless rule may flip-flop.** *author* `HarborHomeAnisTopless`'s else-branch has only "outfit is
   AnisTopless" as its condition (not "she left the right side of the bed"); not seen misbehaving in the game.
9. **Smaller.** *info* 20 scenes no action shows yet (the newest Rapi affection set, Cinderella's clothing store,
   Marciana's library, Naga's public pool, Siren's beach event, Anis's sunbathing set) - fine if work in
   progress. `BeachsideVisitorsAndroids2` can never be completed (its `Task` has no text and nothing finishes
   it). Leftover parameters on three `CharacterFocus` actions in Marciana's and Naga's voyeur dialogues. 22
   manifest backups ship in the export (ModForge #1) - moving them out of the pack folder saves players 36 MB.

## SMSAndroids (this repo)

- **The Harbor Home talk panel was an empty dark box** in rooms with nobody to talk to (where the game's talk
  bubble usually is). *fixed* It shows only with someone in the room.
- **Its name label used TextMeshPro's fallback font** (LiberationSans, 16 px) beside the game's HUD. *fixed* It
  uses the game's own Curse Casual at 18 px.
- **The massage minigame cannot be reached** - nothing launches it since the migration (the old
  `MainStory.cs` did, from the pool dialogue). *info* Out of use for now, by the author's choice.
- The old C# gift UI (`GiftUIBridge`, `Dialogues.giftUI`, `AddGiftItem`) is no longer opened by the pack, which
  uses its own gifting window. *info* Dead code, can go once the pack's window does everything (see Pack 2).

## ModForge (handed over in `SMSModForge/FROM-PACK-TESTING.md`)

1. Export ships the author's manifest backups (36 MB of a 96 MB pack).
2. Actions keep the parameters of the type they used to be.
3. Validator: a value compared against that nothing ever sets (found Pack 3).
4. A dialogue that leaves the UI faded into a panel nothing closes properly (Pack 1's soft-lock).
5. A pack action to play one of the game's own conversations (Pack 5).
6. Validator: a dialogue that can end with a bust still on screen (Pack 4).
7. Validator: an outfit bound to a variable that can hold a non-outfit (Pack 3).
8. A pack's arrival dialogue colliding with one of the game's own: at the gas station on a car-wash day the
   game's conversation ran on top of `RapiDialogueGasstation01` (flags all unchecked - it should have waited),
   Rapi's bust stood through it, and her dialogue only played on the next visit.

## Look (UIs made in the mod)

- **Gifting window** (pack UI). *fixed (F4)* Redrawn in the game's look with SMSAndroids' purple: a dimmed
  background, a navy panel with a purple top line, a "Give a gift" title (the game's title font), a close button
  that brings the HUD back, and a 7-column grid of cards that fit their cells (the old cards overlapped by
  33 px), names that size themselves to fit, only the gifts the player has, and a line saying so when there are
  none.
- **Gift shop** (pack UI): *fixed (F5)* as in Pack 6.
- **Harbor Home talk panel** (SMSAndroids): fixed as above; its portrait always shows Anis's base outfit, not the
  one she is wearing (cosmetic).
- **Mountain Lab navigation labels**: the second corridor is labelled "Next Section" even from its own rooms
  going back to it - confusing beside the first corridor's "Sector N".
- Everything else drawn by the pack (places, busts, room sprites, scene pictures, the navigator) matched the
  game's look in every screenshot taken.
