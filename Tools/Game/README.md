# Testing SMSAndroids in the game without touching the author's saves

The same arrangement as ModForge's checks (`SMSModForge/IN_GAME_LOOK.md`) and the Gallery's
(`SMSGallery/Tools/Game/README.md`), with a test copy, a name and a colour of SMSAndroids' own. The author's
rule (2026-10-24): a check run in the game must not change any of their saves, the game's own files or any mod's.

## The pieces

- **A test copy of the game**, `Starmaker Story 1.8E - Androids test` beside the real one, made by
  `setup-test-game.ps1 -Build`: the game, BepInEx's core and configuration, ModForge (Debug, with its test
  bridge) and what it loads, and SMSAndroids' test bridge (`TestBridge/`, a test-only plugin that reads the game's
  scene) - what a player of the pack has, since the pack needs no plugin of its own (2026-11-02). Not
  SMSAndroidsCore (`-WithPlugin` adds it, for the massage minigame), nor the Gallery, UnityExplorer,
  RuntimeUnityEditor or SMSDiagDebug. The builds go into the copy only; the author's game keeps whatever they put
  there.
- **Renamed** `SMSAndroidTests` in its own game data (the same length as `Starmaker Story`), so Unity gives it
  its own saves folder, settings and window state. The game keeps its list of save slots in slot 0, shared by
  every slot, so a copy saving beside the author's would write their slot 0.
- **Its own test save**, `make-test-save.ps1`: a copy of the author's slots 0 and 1 (whole folders, so
  ModForge's pack files come too), titled "Androids test save" in teal, kept clean in `TestSave\Saves` and put
  back before every start. The author's files are hashed before and after the copy.
- **Started safely** (`launch.ps1`): on a desktop of its own (never on the author's screen, never taking their
  focus), muted, never while another copy of the game runs that is not another check's, never one in front.
- **The pack under test** (`export-pack.ps1`): the repo's `SMSAndroidsPack` is first brought up to date from the
  folder the author edits it in (`- AI Gen\...\- STARMAKER MOD-\SMSAndroidsPack`; added and replaced files only,
  minus `sync-exclude.txt`), then exported with ModForge's own exporter and validator (`Tools/PackTool`) into the
  copy's `Mods` folder. **Always do this before testing** - a check of an old pack proves nothing.

## The checks

```
powershell -File Tools\Game\export-pack.ps1          # sync + export (and the editor's issues list)
python -I Tools\Game\pack_logic.py                   # what the pack's logic says, and where it does not add up
python -I Tools\Game\ingame_check.py                 # everything in the game (the dialogue pass alone is ~2 h)
python -I Tools\Game\ingame_check.py places day      # some areas
python -I Tools\Game\ingame_check.py dialogues AnisDialogueGift SBDialogueMain   # some dialogues
```

`ingame_check.py` areas, each in a fresh game: `startup` (load chain, errors, every pack variable readable),
`places` (every place and vanilla extension), `behaviour` (what the pack is for: schedules, Harbor Home outfits
and roaming, gifting, the gift shop), `hh` (Anis at Harbor Home: the invite, talking in each room, outfit
changes, gifts), `fixes` (the 2026-11-01 fixes: the gift window, gifting in the lab and at Harbor Home, the
shop, the three TV shows and the game's own movie variables left alone), `day` (sleeping into the next day,
through the payout and the morning activity: daily resets, schedule rules), `migrated` (what moved from C# into
the pack on 2026-11-02: the Harbor Home talk panel, no random bedroom events in the house, the world map's
character icons - each with a control), `moved` (the rest of the plugin, moved later that day: the bedroom buttons
and TV copied into Harbor Home, the affection effect, waking up there after a night slept there - each against the
game's original),
`dialogues` (every dialogue from its own place, every choice over repeated plays, each line read off the box,
scene pictures, unfilled `{tags}`, coverage), `ui` (canvases and SMSAndroids' own screens). Results go to
`%TEMP%\SMSAndroids-ingame\run-<time>\`: `report.md`, `steps.jsonl`, `shots\`, `ui\`.

`Tools/PackTool` also reads the game's screens as the editor seeds them: `PackTool ui-tree vanillaui:<screen>`
prints one, and `ui-graft <spec> <out>` builds a UI extension of it with ModForge's own code (how
`pack_plugin_to_pack_2026_11_02.py` makes the world-map icons).

`pack_logic.py` writes `logic.txt` (every rule and dialogue start, readably) and `consistency.txt` (values a
variable is compared to that nothing sets, variables nothing uses, scenes nothing shows). Compare with the mod
before ModForge (`git show d42d17d:SMSAndroids/<file>`) when judging what the pack is meant to do.

Drive a running copy by hand with `bridge.py`: `mf` is ModForge's bridge (its commands are listed in
`SMSModForge/Tools/Game/bridge.ps1`), `sa` is SMSAndroids' (`TestBridge/TestBridge.cs`: `state`, `errors`,
`signal`, `busts`, `cg`, `canvases`, `ui`, `at`, `xf`, `particles`, `hover`, `track`). `look_map.py <name>` takes
1080p shots of the world map's character icons and the Harbor Home talk cards, with a crowd.

```
python -I Tools\Game\bridge.py mf "travel pack:SMSAndroidsPack.SecretBeach" "wait 3" "shot beach"
python -I Tools\Game\bridge.py sa "errors" "at 640 360"
```

Close it with `stop.ps1`.
