# What SMSAndroidsCore still does, and where each part can go (2026-11-02)

**Outcome (end of 2026-11-02):** everything the plugin did in play is now the pack's, and the plugin is no longer
shipped. What is left of SMSAndroidsCore is the massage minigame (out of use) and its supports; the pack needs
nothing from it - checked in the game without it (the test copy has no SMSAndroidsCore since).

A review of every component of the plugin against what the pack and ModForge can do today (ModForge as of
2026-11-02, unreleased work included). Four outcomes:

- **Remove**: dead - nothing the game, the pack or another part of the plugin reads any more.
- **Pack now**: ModForge can already do it; move it into the pack and delete the C#.
- **Needs ModForge**: a general feature is missing; written up in `SMSModForge/FROM-PACK-TESTING.md`
  (entries 10 and 13; 11 and 12 were declined - the pack does them), and the C# stays until it lands.
- **Stays**: genuinely plugin work.

Out of scope: `Debugging.cs` (developer tools), `TestBridge.cs` (the in-game checks), and the massage minigame
with its physics (`MassageMinigame`, `Minigames`, `BreastPhysics`, `LotionTrail`, `Squeeze*`, `DragSprite*`,
`MassageMovementPattern`) - out of use for now, by the author's choice.

## Remove (dead)

| Part | Where | Why it is dead |
|---|---|---|
| The C# gift UI | `Dialogues.InitializeGiftUICanvas`, `AddGiftItem`, `UpdateGiftUIVisibility`, `giftUI` | The pack has its own gifting window (`giftui`); no dialogue opens the C# one any more. |
| The C# gift store | `Places.InitializeModShops`, `AddItemToGiftStore`, `ActivateShop`, `UpdateGiftStoreItemVisibility`, `CheckAllConditions`, the `giftShopItem*` fields | The pack has its own shop (`giftshop`), opened by Claire. |
| `GiftUIBridge` | `GiftUIBridge.cs` | Its two signals are emitted by nothing (only an old leftover parameter on the pack's UI actions names one, and ModForge's migration strips it). |
| Gift helpers | `Core.IncrementAffectionForGiftIfLiked`, `SetGifting*`, `GetGiftingString`, `SetAndSyncGiftVariable`, `Characters.characterGiftLikesMap` | Only the C# gift UI called them; the likes and the affection raise are in `AnisDialogueGift` now. |
| Proxy Variables | `Core.InitializeProxyVariables`, `SetupVariableSync`, `SyncVanillaToProxyVariables`, `RefreshDailyProxyVariables`, `Get/FindAndModifyProxyVariable*`, the mapping tables; `SaveManager`'s after-sleep turnover (it only refreshes these) | Nothing in the pack reads a proxy (its `Gift_*` checks read its own variables of the same names), no game content can know the mod's asset, and the only reader left is the out-of-use minigame (`Minigame_Massage_Character`). Today it costs a throttled per-frame poll and a log line per change. |
| "A dialogue is playing" mirror | `Dialogues.MonitorRoomTalkChildren`, `dialoguePlaying*` and their proxy | Only the proxy above read it. |
| Level randoms | `Places.randomNumMall`, `randomNumMLRoomAnis` | The pack's `RandomNumMall` / `RandomNumMLRoomAnis` (LevelRandom) replaced them; the C# values are only logged. |
| Delayed helpers | `Core.EmitSignalDelayed`, `ChangeOutfitDelayed`, `EmitSignalGameObjectDelayed`, `DisableAllActiveBustChildren`, `AddGameObjectToLocalListVariables`, `Find/GetVariableGameObject` | Called from nowhere outside their own definitions. |
| Room talk and level fields | `Places.roomTalk*`, `level*` (Alley ... Trail, Cinema) | Resolved at load, read by nothing. |

## Pack now - done 2026-11-02

Built by `Tools/Game/pack_plugin_to_pack_2026_11_02.py` (idempotent; the map UI is seeded from the game's screen
with ModForge's own code through `Tools/PackTool ui-graft`) and checked by the in-game `migrated` area, each part
with a control that must come out the other way.

| Part | Was | In the pack now |
|---|---|---|
| Harbor Home talk panel | `Dialogues.InitializeHHTalkPanel` and friends (~500 lines). | UI `hhtalk`, anchored where the game's talk bubble is: a card per character the pack can invite (Anis today), shown while her `Location_` starts with `HarborHome_PlayerRoom` (set by the six rooms' enter/exit hooks). In the map's style: a navy cut-corner frame with an electric-blue top edge and an electric-blue name tab carrying her name and the game's talk icon; her portrait from the head to just above the navel, in the outfit `HarborHome_Outfit_<name>` names, breaking out of the frame at the top and sides - from `UI/TalkPanel/Portraits/<outfit>.png` (or `<name>.png`) when the author has one there, which they edit freely (the script writes a starter of the bust crop with `WRITE_PORTRAITS=1`, never over an existing file, and fits a copy for the card into `UI/TalkPanel/Cards/`), else cut from her bust; the right-hand card of two wears the frame mirrored. It grows into place as it appears, and grows on hover with the game's own `UIHoverBounce`; press feedback waits on ModForge #19. A click sets `HarborHome_TalkSelected` unless someone is selected or `Lock-Game` is on, and the card dims then. Rules `HHTalkBubbleOff` / `HHTalkBubbleBack` keep the game's talk bubble off in the rooms and put it back outside. The next character invited gets a card by copying Anis's in the editor (or by running the script again). |
| No random bedroom events at home | `Places` (insideHarborHome). | Rules `HHRandomEventsHold` (entering the living room saves the player's `Disable-Specific-RNGEvents` into `HarborHome_RandomEventsSetting` and holds it on), `HHRandomEventsKeep` (the player turning it off inside is kept for later) and `HHRandomEventsRelease` (the entrance or the player's own room puts their value back). Unlike the C#, it survives a save and reload inside the house. |
| World-map character icons | `ScheduleVisualizer` (1,350 lines). | Rule `MapSpots` (one per character, `forEach`) turns `Location_<name>` into `MapSpot_<name>` - the old table, `*` keys as prefixes, every spelling the pack writes; UI `mapicons` on `vanillaui:World_Map/Canvas/Core` shows her portrait beside that place's button and under its district's: square tiles with cut corners on a navy plate with cut corners, an electric-blue top edge, the game's sparkle divider beneath, a leader line to a place's button and a wedge under a district's. Eight to a row edge to edge, more rows past that, fading in as they appear; the plate is only as big as who is there (ContentSizeFitter + the grid's `fitToContent`, ModForge #16) and hidden when nobody is. Portraits are cut from each character's own bust (`Tools/Game/map_portraits.py`, so a new character gets hers with her bust) and drawn with the plate's pieces at twice their 1080p size (`map_badges.py`); only corners are pictures, the rest flat colour stretched, so nothing goes soft. 27 characters. |

Three ModForge gaps found on the way, handed over and fixed the same day: UI-extension objects two levels down (#14), a pack screen in a 100x100 box (#15), and a grid that wraps but stays as small as what it holds (#16). Then #17 (a node held level while its parent turns - the map strips stay level as a district's radial menu spins open) and #18 (pack pictures clamped, which cleared the stray lines at the plate's corners). #19, press feedback for the talk cards, is open. The map UI is saved nested as the editor writes it, and the talk panel is anchored to the corner like the game's talk bubble.

## Moved once ModForge shipped #10 and #13 (2026-11-02)

| Part | Was | In the pack now |
|---|---|---|
| Harbor Home bedroom buttons | `Places`: cloned the game bedroom's `PlayerRoom_ButtonCanvas` into the pack bedroom, moved three buttons, softened the parallax. | `HarborHomeBedroom` object `PlayerRoom_ButtonCanvas`: `copyOf` the game's, its `Player_Room_Buttons` bound with `overrideParallax` 0.05 and the Sleep / Calendar / PC buttons bound with `overrideTransform`. |
| Harbor Home TV objects | `Places`: cloned the game living room's `Movies`. | `HarborHomeLivingRoom` object `Movies`: `copyOf` the game's. |
| The affection effect | `Core`: cloned the game's "Loving" effect, tinted it pink, named it `CFXR2 Expression Affectionate (Timed)`. | `copyOf` the game's with `copyBeside` (so it sits under `6_Effects`, where the pack's lines switch it on by path), `particleColor` #FF99E6 and its `Hearts` #FF59B3. |
| Waking up at Harbor Home | `Places`: once `HarborHome_Slept`, hid the game bedroom and showed the Harbor Home one, once per load. | Rule `HHWakeInHarborHome`: in the game's bedroom with `HarborHome_Slept` and not yet woken this time (`HarborHome_WokeThere`, session-only), the `Travel` action to `place:HarborHomeBedroom`. `HarborHomeSleepFlag` re-arms it at each sleep; `HHWakeSettled` ends the morning once the player goes on into the house, so a later trip to the old room stays a trip there. |

Checked in the game by the `moved` area, each against the game's original as the control.

## Removed with them

| Part | Why |
|---|---|
| `SaveManager` (façade and legacy importer) | The author dropped saves from before ModForge (2026-11-02): `SMSAndroidsCore_Save.txt` is no longer imported. The minigame reads its pack variables through `ModForgeBridge` directly. |
| `TransformExtensions` | Used by nothing. |
| `TestBridge.cs` | Moved to `Tools/Game/TestBridge`, a test-only plugin that reads the game's scene, so the checks run without SMSAndroidsCore. |

## What is left in SMSAndroidsCore

All of it is the massage minigame or developer tooling:

| Part | Role |
|---|---|
| `MassageMinigame`, `Minigames`, `BreastPhysics`, `LotionTrail`, `Squeeze*`, `DragSprite*`, `MassageMovementPattern` | The minigame. |
| `Core` | Root cache, load chain, `minigamebundle`. |
| `Characters` | Bust lookups for the minigame. |
| `MainStory`, `Schedule` | Load-order latches and signal names the minigame uses. |
| `ModForgeBridge` | The minigame's progress in pack variables (`Minigame_Massage_*`). |
| `Debugging` | Developer inspection; every call commented out. |

So the plugin does not need to ship. If the minigame comes back, it ships with it.
