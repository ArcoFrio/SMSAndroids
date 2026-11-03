"""What SMSAndroidsCore still drew or held in C#, moved into SMSAndroidsPack (docs/PLUGIN-TO-PACK-2026-11-02.md).

  python -I Tools/Game/pack_plugin_to_pack_2026_11_02.py <modpack.json in> <modpack.json out> <pack folder>

Idempotent, and keeps the editor's formatting (2-space indent, CRLF) so the diff is only the change. The pack
folder receives the map art (UI/MapIcons): portrait tiles cut from each character's bust and the plate's pieces
(map_portraits.py, map_badges.py).

  P1  The Harbor Home talk panel: a pack UI where the game's talk bubble sits, one card per character in the
      room the player is in (her portrait in the outfit she wears there, her name); clicking one sets
      HarborHome_TalkSelected; dimmed while someone is selected or the game is locked. The game's talk bubble is
      off inside the house's rooms and back outside. HarborHome_PlayerRoom (the rooms' enter/exit hooks) says
      which room the player is in.
  P2  No random bedroom events at Harbor Home: entering the house saves the game's Disable-Specific-RNGEvents
      (the player's own setting) and holds it on; leaving puts the player's value back. A change the player makes
      in the settings while inside is kept for when they leave.
  P3  The world map's character icons: a rule turns each character's location into the map button it shows on
      (MapSpot_<name>), and a UI on the game's world map draws her face on that button and on its district's.
  P4  The rest of what the plugin did: the game bedroom's buttons and the living room's TV objects copied into the
      Harbor Home rooms, the pink affection effect copied beside the game's, and waking up at Harbor Home after a
      night there (a Travel rule).
"""
import json
import os
import subprocess
import sys
import tempfile

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import map_badges  # noqa: E402
import map_portraits  # noqa: E402

src, out, pack_root = sys.argv[1], sys.argv[2], sys.argv[3]
raw = open(src, encoding="utf-8-sig", newline="").read()
d = json.loads(raw)
done = []

HERE = os.path.dirname(os.path.abspath(__file__))
PACKTOOL = os.path.join(os.environ["TEMP"], "SMSAndroids-ingame", "packtool", "bin", "PackTool", "release", "PackTool.exe")


def var(name):
    return next((v for v in d["variables"] if v["name"] == name), None)


def add_var(name, type_, default, description, persisted=True):
    if var(name):
        return
    d["variables"].append({"name": name, "type": type_, "defaultValue": default, "persisted": persisted,
                           "description": description})
    done.append("variable " + name)


def cmp(name, value, comparison="equals", source=None, negate=False):
    c = {"type": "VariableCompare", "params": {"name": name, "value": value, "comparison": comparison}}
    if source:
        c["params"]["source"] = source
    if negate:
        c["negate"] = True
    return c


def starts(name, prefix, ignore_case=False):
    return {"type": "VariableStartsWith", "params": {"name": name, "value": prefix, "ignoreCase": "true" if ignore_case else "false"}}


def level(token, negate=False):
    c = {"type": "LevelActive", "params": {"level": token}}
    if negate:
        c["negate"] = True
    return c


def any_of(conditions):
    return {"type": "Any", "conditions": conditions}


def setv(name, value, source=None, value_source=None):
    a = {"type": "SetVariable", "params": {"name": name, "value": value}}
    if source:
        a["params"]["source"] = source
    if value_source:
        a["params"]["valueSource"] = value_source
    return a


def go_active(path, active):
    return {"type": "SetGameObjectActive", "params": {"kind": "Direct Path", "target": path, "active": "true" if active else "false"}}


def put_rule(rule):
    """Add the rule, or replace an older form of it with the same key."""
    rules = d["integrationRules"]
    at = next((i for i, r in enumerate(rules) if r["key"] == rule["key"]), None)
    if at is None:
        rules.append(rule)
        done.append("rule " + rule["key"])
    elif rules[at] != rule:
        rules[at] = rule
        done.append("rule " + rule["key"] + " (updated)")


def put_ui(ui):
    uis = d["uis"]
    at = next((i for i, u in enumerate(uis) if u["id"] == ui["id"]), None)
    if at is None:
        uis.append(ui)
        done.append("UI " + ui["id"])
    elif uis[at] != ui:
        uis[at] = ui
        done.append("UI " + ui["id"] + " (updated)")


def rect(pos=(0, 0), size=(100, 100), amin=(0.5, 0.5), amax=(0.5, 0.5), pivot=(0.5, 0.5)):
    return {"anchorMin": list(amin), "anchorMax": list(amax), "pivot": list(pivot), "position": list(pos), "size": list(size)}


def ui(name, r, image=None, text=None, children=None, **extra):
    n = {"IsBound": False, "name": name, "rect": r}
    if image:
        n["image"] = image
    if text:
        n["text"] = text
    n.update(extra)
    if children:
        n["children"] = children
    return n


def img(sprite, tint="#FFFFFFFF", type_="Simple", raycast=True, aspect=False):
    i = {"sprite": sprite, "type": type_, "tint": tint}
    if aspect:
        i["preserveAspect"] = True
    if not raycast:
        i["raycastTarget"] = False
    return i


def txt(value, size, color="#FFFFFFFF", font="Curse Casual SDF", align="Center", auto=None):
    t = {"value": value, "font": font, "size": size, "color": color, "alignment": align}
    if auto:
        t["autoSize"], t["autoSizeMin"], t["autoSizeMax"] = True, auto[0], auto[1]
    return t


NAVY, ACCENT = "#0A0F22E6", "#AF54E7"

# ---------------------------------------------------------------------------------------------- P1 talk panel
ROOMS = ["HarborHomeBathroom", "HarborHomeBedroom", "HarborHomeCloset", "HarborHomeKitchen",
         "HarborHomeLivingRoom", "HarborHomePool"]
add_var("HarborHome_PlayerRoom", "String", "",
        "The Harbor Home room the player is in (one of the room places' keys), empty anywhere else. Set by the "
        "rooms' enter/exit hooks; the talk panel shows whoever's Location_ starts with it.", persisted=False)
add_var("HarborHome_TalkBubbleHidden", "Bool", "false",
        "The game's talk bubble was switched off for a Harbor Home room, so it is put back on leaving.",
        persisted=False)

for place in d["places"]:
    if place["key"] not in ROOMS:
        continue
    room = place["key"]
    enter = {"conditions": [], "actions": [setv("HarborHome_PlayerRoom", room)]}
    # Only its own name: going from one room to the next can run the next one's enter before this exit.
    leave = {"conditions": [cmp("HarborHome_PlayerRoom", room)], "actions": [setv("HarborHome_PlayerRoom", "")]}
    for field, hook in (("onEnter", enter), ("onExit", leave)):
        hooks = place.setdefault(field, [])
        if hook not in hooks:
            hooks.append(hook)
            done.append(f"P1 {room} {field}")

TALK_BUTTON = "9_MainCanvas/TalkButton"
put_rule({
    "key": "HHTalkBubbleOff", "displayName": "Harbor Home - no talk bubble in the rooms",
    "description": "Inside the house's rooms the talk panel (UI hhtalk) is how you talk to someone, so the game's own talk bubble is kept off there.",
    "triggerMode": "WhilePassing",
    "conditions": [cmp("HarborHome_PlayerRoom", "", negate=True),
                   {"type": "GameObjectActive", "params": {"kind": "Direct Path", "target": TALK_BUTTON}}],
    "actions": [go_active(TALK_BUTTON, False), setv("HarborHome_TalkBubbleHidden", "true")]})
put_rule({
    "key": "HHTalkBubbleBack", "displayName": "Harbor Home - talk bubble back outside",
    "description": "Puts the game's talk bubble back once the player has left the house's rooms, if HHTalkBubbleOff took it away.",
    "triggerMode": "OnRisingEdge",
    "conditions": [cmp("HarborHome_PlayerRoom", ""), cmp("HarborHome_TalkBubbleHidden", "true")],
    "actions": [go_active(TALK_BUTTON, True), setv("HarborHome_TalkBubbleHidden", "false")]})

def values_set(name):
    """Every literal value the pack's actions write into a variable."""
    found = set()

    def walk(o):
        if isinstance(o, dict):
            p = o.get("params")
            if o.get("type") == "SetVariable" and isinstance(p, dict) and p.get("name") == name and not p.get("source"):
                found.add(p.get("value", ""))
            for v in o.values():
                walk(v)
        elif isinstance(o, list):
            for v in o:
                walk(v)
    walk(d)
    return found


chars = {c["name"]: c for c in d["characters"] if c.get("bustSource") == "Pack" and c.get("outfits")}
# A card for everyone the pack can invite (whose HarborHome_Visit_ something sets true); the next one invited gets
# hers by copying one in the editor, or by running this again.
TALKERS = [n for n in chars if var("Location_" + n) and "true" in values_set("HarborHome_Visit_" + n)]
# TALK_EXTRA=Rapi,... adds cards for characters not invited yet - for trying two cards in the game; never set when
# patching the author's pack.
TALKERS += [n for n in os.environ.get("TALK_EXTRA", "").split(",") if n in chars and n not in TALKERS]


TALK_ART = "UI/TalkPanel/"
done += ["P1 art " + f for f in map_badges.make_talk(os.path.join(pack_root, "UI", "TalkPanel"),
                                                     os.environ.get("MAP_ACCENT", map_badges.ACCENT))]
POP = 12                # her head breaks this far out of the top of the frame
POP_SIDE = 10           # ... and her hair and arms this far out of each side
BUST = 294              # a bust drawn this big shows her from the head to just above the navel
# A picture of the author's own replaces the bust crop for one outfit (UI/TalkPanel/Portraits/<outfit>.png) or for
# every outfit of a character (<name>.png): drawn for the portrait's window - WINDOW at 1080p, its top POP and its
# sides POP_SIDE outside the frame, transparent wherever the card should show through. Any size: the card gets a
# copy fitted to twice the window's size (UI/TalkPanel/Cards/), since a pack picture shown much smaller than it is
# drawn breaks up.
PORTRAITS = os.path.join(pack_root, "UI", "TalkPanel", "Portraits")
CARD_COPIES = os.path.join(pack_root, "UI", "TalkPanel", "Cards")
WINDOW = (map_badges.CARD[0] + 2 * POP_SIDE, map_badges.CARD[1] - map_badges.NAME_TAB + POP)
copies_made = set()

# Two guides beside the portraits, the portraits' exact size: _TemplateUnder (the card behind the portrait - put it
# under a drawing to see the result) and _TemplateOver (its outline - lay it over). Rewritten every run; their
# names match no outfit, so no card takes them for a portrait.
if os.path.isdir(PORTRAITS):
    from PIL import Image as _Image
    for _name, _img in zip(("_TemplateUnder.png", "_TemplateOver.png"),
                           map_badges.portrait_templates(WINDOW, POP, POP_SIDE, os.environ.get("MAP_ACCENT", map_badges.ACCENT))):
        _to = os.path.join(PORTRAITS, _name)
        if not os.path.exists(_to) or _Image.open(_to).convert("RGBA").tobytes() != _img.tobytes():
            _img.save(_to, optimize=True)
            done.append("P1 " + _name)


def bust_crop(o, c):
    """What the card shows when there is no picture of the author's: her bust, drawn BUST big, placed so her face is
    in the window's middle and the top of her hair just under its top. Returns the node's offset from the window's
    top-centre, in 1080p units."""
    from PIL import Image
    bust = Image.open(sprite_file(o["baseSprite"])).convert("RGBA")
    cx, top = map_portraits.head(bust, c["name"])
    k = BUST / bust.width
    return bust, (round((bust.width / 2 - cx) * k), round(top * k - 2))


def write_starter(o, c):
    """The card's current portrait as a picture of the author's own, to edit (e.g. take an arm out): exactly what
    the bust crop shows, at twice the window's size, transparent around her. Never over a file already there."""
    from PIL import Image
    to = os.path.join(PORTRAITS, o["gameObjectName"] + ".png")
    if os.path.exists(to) or os.path.exists(os.path.join(PORTRAITS, c["name"] + ".png")):
        return
    bust, (dx, dy) = bust_crop(o, c)
    w, h = WINDOW[0] * 2, WINDOW[1] * 2
    size = BUST * 2
    big = bust.resize((size, size), Image.NEAREST)          # pixel art: its own pixels, twice over
    out = Image.new("RGBA", (w, h), (0, 0, 0, 0))
    left, top = w // 2 + 2 * dx - size // 2, -2 * dy
    region = big.crop((-left, -top, -left + w, -top + h))
    out.alpha_composite(region)
    os.makedirs(PORTRAITS, exist_ok=True)
    out.save(to, optimize=True)
    done.append("P1 starter portrait " + o["gameObjectName"])


def own_portrait(o, c):
    """The card's copy of the author's picture for this outfit, or None to cut one from the bust."""
    from PIL import Image
    for key in (o["gameObjectName"], c["name"]):
        src = os.path.join(PORTRAITS, key + ".png")
        if os.path.exists(src):
            break
    else:
        return None
    w, h = WINDOW[0] * 2, WINDOW[1] * 2
    pic = Image.open(src).convert("RGBA")
    k = min(w / pic.width, h / pic.height)
    pic = pic.resize((max(1, round(pic.width * k)), max(1, round(pic.height * k))), Image.LANCZOS)
    card = Image.new("RGBA", (w, h), (0, 0, 0, 0))
    card.alpha_composite(pic, ((w - pic.width) // 2, h - pic.height))   # centred, standing on the name tab
    os.makedirs(CARD_COPIES, exist_ok=True)
    to = os.path.join(CARD_COPIES, key + ".png")
    if not os.path.exists(to) or Image.open(to).convert("RGBA").tobytes() != card.tobytes():
        card.save(to, optimize=True)
        done.append("P1 portrait " + key)
    copies_made.add(key + ".png")
    return "UI/TalkPanel/Cards/" + key + ".png"


def sprite_file(rel):
    """A pack picture on disk, whatever case the manifest spells it in."""
    path = os.path.join(pack_root, rel)
    if os.path.exists(path):
        return path
    folder = os.path.dirname(path)
    match = [f for f in os.listdir(folder) if f.lower() == os.path.basename(path).lower()] if os.path.isdir(folder) else []
    return os.path.join(folder, match[0]) if match else None


def portraits(c):
    """Her, from the head to about the waist, in the outfit she wears at Harbor Home when the pack says
    (HarborHome_Outfit_<name>), else her default: her bust drawn large behind the window, placed so her face is in
    its middle and the top of her hair just under its top (found from the picture, map_portraits.head)."""
    outfits = c["outfits"]
    default = next((o for o in outfits if o["gameObjectName"] == c.get("defaultOutfit")), outfits[0])
    hh = "HarborHome_Outfit_" + c["name"]

    def picture(o, conditions):
        if os.environ.get("WRITE_PORTRAITS"):
            write_starter(o, c)
        own = own_portrait(o, c)
        if own:
            return ui("Portrait_" + o["gameObjectName"], rect(size=(0, 0), amin=(0, 0), amax=(1, 1)),
                      image=img(own, raycast=False), activeConditions=conditions)
        _, pos = bust_crop(o, c)
        return ui("Portrait_" + o["gameObjectName"], rect(pos=pos, size=(BUST, BUST), amin=(0.5, 1), amax=(0.5, 1), pivot=(0.5, 1)),
                  image=img(o["baseSprite"], raycast=False, aspect=True), activeConditions=conditions)

    if not var(hh):
        return [picture(default, [])]
    worn = values_set(hh)
    others = [o for o in outfits if o is not default and o["gameObjectName"] in worn]
    made = [picture(default, [cmp(hh, o["gameObjectName"], negate=True) for o in others])]
    made += [picture(o, [cmp(hh, o["gameObjectName"])]) for o in others]
    return made


busy = any_of([cmp("HarborHome_TalkSelected", "", negate=True), cmp("Lock-Game", "true", source="vanilla")])
def here(n):
    """She is visiting and in the room the player is in."""
    return {"type": "All", "conditions": [cmp("HarborHome_Visit_" + n, "true"), starts("Location_" + n, "$HarborHome_PlayerRoom")]}


cards = []
for i, name in enumerate(TALKERS):
    c = chars[name]
    tab = map_badges.NAME_TAB
    # The cards line up in TALKERS order, so hers is the right-hand one exactly when someone before her is here too.
    right = any_of([here(n) for n in TALKERS[:i]]) if i else None
    whole = rect(size=(0, 0), amin=(0, 0), amax=(1, 1))
    frames = [ui("Frame", whole, image=img(TALK_ART + "TalkCard.png", raycast=False),
                 activeConditions=[dict(right, negate=True)] if right else [])]
    if right:
        frames.append(ui("FrameMirrored", whole, image=img(TALK_ART + "TalkCardMirrored.png", raycast=False),
                         activeConditions=[right]))
    # The portrait's window: down to the name tab, POP above the frame's top and POP_SIDE past its sides, so she
    # breaks out of the card (only her picture shows there; the rest of a bust is transparent).
    window = ui("Window", rect(pos=(0, (tab + POP) / 2), size=(2 * POP_SIDE, POP - tab), amin=(0, 0), amax=(1, 1)),
                clipChildren=True, children=portraits(c))
    label = ui("Name", rect(pos=(-9, 0), size=(-38, tab), amin=(0, 0), amax=(1, 0), pivot=(0.5, 0)),
               text=dict(txt((c.get("displayName") or name).upper(), 16, color="#0A0F22FF", font="Alata-Regular SDF",
                             auto=(10, 16)), characterSpacing=3.0))
    # The game's own talk-bubble icon on the tab, after her name: what the card replaces, and what it is for.
    talk = ui("TalkIcon", rect(pos=(-15, 4), size=(18, 18), amin=(1, 0), amax=(1, 0), pivot=(1, 0)),
              image=img("function_icon_talk_2", "#0A0F22E6", raycast=False, aspect=True))
    shade = ui("Busy", rect(size=(-4, -4), amin=(0, 0), amax=(1, 1)),
               image=img(TALK_ART + "Shade.png", raycast=False), activeConditions=[busy])
    cards.append(ui("Talk_" + name, rect(size=map_badges.CARD),
                    image=img(TALK_ART + "Hit.png"),
                    activeConditions=[here(name)],
                    clickConditions=[cmp("HarborHome_TalkSelected", ""), cmp("Lock-Game", "true", source="vanilla", negate=True)],
                    onClick=[setv("HarborHome_TalkSelected", name)],
                    # Hover: the game's own springy grow, as on its quest journal's tabs (attached by name).
                    components=[{"type": "UIHoverBounce", "bounceType": "Scale", "bounceAmount": 1.05, "speed": 12,
                                 "bounciness": 0.3}],
                    # Arriving: everything drawn sits in Body, which fades in while growing into place. On a child,
                    # so the hover's scaling of the card and this one never pull at the same transform.
                    children=[ui("Body", rect(size=(0, 0), amin=(0, 0), amax=(1, 1)),
                                 open={"fade": True, "scaleFrom": [0.85, 0.85, 1.0], "duration": 0.3, "easing": "QuadInOut"},
                                 children=frames + [window, talk, label, shade])]))

if os.path.isdir(CARD_COPIES):
    for stale in sorted(set(os.listdir(CARD_COPIES)) - copies_made):
        os.remove(os.path.join(CARD_COPIES, stale))
        done.append("P1 removed portrait " + stale)

put_ui({
    "id": "hhtalk", "name": "Harbor Home talk",
    "hidesWithGameplayUi": True, "startsOpen": True,
    # Where the game's talk bubble is: anchored to the bottom-right corner, 166 in and 131 up like the bubble,
    # less 20 so the card's foot sits as low as the bubble's.
    "nodes": [ui("HHTalk", rect(pos=(-166, 135), size=(300, 200), amin=(1, 0), amax=(1, 0)),
                 # Spaced so two cards both have room to break out sideways.
                 layout={"kind": "Horizontal", "spacing": [2.0 * POP_SIDE - 4, 0.0], "padding": [0.0, 0.0, 0.0, 0.0],
                         "alignment": "MiddleCenter", "controlWidth": False, "controlHeight": False,
                         "expandWidth": False, "expandHeight": False},
                 children=cards)]})

# ---------------------------------------------------------------------------------------------- P2 random events
RNG = "Disable-Specific-RNGEvents"
add_var("HarborHome_RandomEventsHeld", "Bool", "false",
        "The house is holding the game's " + RNG + " on (the player is inside); its value outside is in HarborHome_RandomEventsSetting.")
add_var("HarborHome_RandomEventsSetting", "Bool", "false",
        "The player's own " + RNG + " (the game's settings toggle), kept while the house holds it on, and put back on leaving.")
put_rule({
    "key": "HHRandomEventsHold", "displayName": "Harbor Home - no random bedroom events (hold)",
    "description": "Entering the house (its living room) keeps the player's own " + RNG + " setting and turns it on, so the game's random bedroom events don't fire at Harbor Home.",
    "triggerMode": "OnRisingEdge",
    "conditions": [level("place:HarborHomeLivingRoom"), cmp("HarborHome_RandomEventsHeld", "false")],
    "actions": [setv("HarborHome_RandomEventsSetting", "$" + RNG, value_source="vanilla"),
                setv(RNG, "true", source="vanilla"), setv("HarborHome_RandomEventsHeld", "true")]})
put_rule({
    "key": "HHRandomEventsKeep", "displayName": "Harbor Home - no random bedroom events (player's change)",
    "description": "Turning the setting off in the game's settings while inside the house is what the player wants once they leave; the house keeps it on until then.",
    "triggerMode": "WhilePassing",
    "conditions": [cmp("HarborHome_RandomEventsHeld", "true"), cmp(RNG, "false", source="vanilla")],
    "actions": [setv("HarborHome_RandomEventsSetting", "false"), setv(RNG, "true", source="vanilla")]})
put_rule({
    "key": "HHRandomEventsRelease", "displayName": "Harbor Home - no random bedroom events (release)",
    "description": "Leaving the house (its entrance, or the player's own room) puts the player's " + RNG + " setting back.",
    "triggerMode": "OnRisingEdge",
    "conditions": [cmp("HarborHome_RandomEventsHeld", "true"),
                   any_of([level("place:HarborHouseEntrance"), level("vanilla:5_MyRoom")])],
    "actions": [setv(RNG, "$HarborHome_RandomEventsSetting", source="vanilla"), setv("HarborHome_RandomEventsHeld", "false")]})

# ---------------------------------------------------------------------------------------------- P3 world map
# From ScheduleVisualizer.cs: a location value -> the radial button it shows on; a key ending in * is a prefix.
REMOTE = "Seaside/SecretBeach"
LOCATION_TO_BUTTON = [
    ("Beach", "Seaside/Beach"), ("Sofia", "Seaside/Sofia"), ("Parkinglot", "Badlands/Parkinglotbutton"),
    ("Tonihome", "Badlands/tonihomeworldmap"), ("AmusementPark", "Badlands/amusementparkworldmap"),
    ("Alley", "Badlands/Parkinglotbutton"), ("Kenshome", "Suburbs/Ken"), ("KenhouseOutside", "Suburbs/Ken"),
    ("Pool", "Suburbs/Pool"), ("Park", "Suburbs/Park"), ("TashaStore", "TheLine/tashastore"),
    ("FlowerStore", "TheLine/Flowerstoreworldmap"), ("Kate", "TheLine/Kate"), ("Gabrielsmansion", "TheHills/Gabriel"),
    ("Mario", "TheHills/Mario"), ("Villa", "TheHills/Mario"), ("Chloe", "TheHills/Chole"), ("Home", "Downtown/Home"),
    ("Downtown", "Downtown/Streets"), ("Streets", "Downtown/Streets"), ("Hospital", "Downtown/Hospital"),
    ("Hospitalhallway", "Downtown/Hospital"), ("Hotel", "Downtown/Streets"), ("Club", "NeonRow/Club"),
    ("Neonstreet", "NeonRow/Neonstreet"), ("Mall", "Shopside/Mall"), ("Gasstation", "Shopside/Gasstation"),
    ("Forest", "Evergreen/Forest"), ("Temple", "Evergreen/Forest"), ("Trail", "Evergreen/Trail"),
    ("Farm", "Plainview/Farm"), ("Harbor", "Foundry/Harbor"), ("hauhou", "Foundry/hauhou"),
    ("HarborHouseEntrance", "Foundry/HarborHouseEntrance"), ("HarborHome*", "Foundry/HarborHouseEntrance"),
    ("GiftShop", "Seaside/Beach"), ("GiftShopInterior", "Seaside/Beach"),
    ("SecretBeach*", REMOTE), ("MountainLab*", REMOTE), ("RemoteArea*", REMOTE), ("Facility*", REMOTE),
]
# Radial group -> its district button (some are named differently).
DISTRICT = {"Seaside": "Seaside", "Suburbs": "Suburbs", "Downtown": "Downtown", "TheHills": "TheHills",
            "TheLine": "The Line", "Evergreen": "Evergreen", "Badlands": "Badlands", "Plainview": "Plainview",
            "Shopside": "Shoppingdistrict", "NeonRow": "Nightlife", "Foundry": "Harbor"}
# Everyone with a bust in the pack and a location: the old map's 21, and whoever the pack adds later. Solid's
# location variable is named after Snek, the name he goes by in it.
LOCATION_NAME = {"Solid": "Snek"}
TILE_OF = {}
for name in sorted(map_portraits.all_portraits(pack_root)):
    if var("Location_" + LOCATION_NAME.get(name, name)):
        TILE_OF[LOCATION_NAME.get(name, name)] = name
MAP_CHARS = list(TILE_OF)

# Each portrait as a cut-corner tile, and the plate's pieces, in the accent (map_badges.py).
ACCENT = os.environ.get("MAP_ACCENT", map_badges.ACCENT)
icons = os.path.join(pack_root, "UI", "MapIcons")
done += ["P3 art " + c for c in map_badges.make(pack_root, icons, [TILE_OF[c] for c in MAP_CHARS], ACCENT)]

# The old map matched names ignoring case; a pack comparison does not, so every spelling the pack writes counts.
spellings = set().union(*(values_set("Location_" + c) for c in MAP_CHARS))


def matches(key):
    loc = "Location_{char}"
    if key.endswith("*"):
        return [starts(loc, key[:-1], ignore_case=True)]
    same = sorted({key} | {s for s in spellings if s.lower() == key.lower()})
    return [cmp(loc, same[0])] if len(same) == 1 else [any_of([cmp(loc, s) for s in same])]


steps = [{"conditions": matches(k), "actions": [setv("MapSpot_{char}", b)]} for k, b in LOCATION_TO_BUTTON]
steps.append({"conditions": [], "actions": [setv("MapSpot_{char}", "")]})
for c in MAP_CHARS:
    add_var("MapSpot_" + c, "String", "",
            f"The world-map button {c} shows on (radial group/button), from Location_{c} by rule MapSpots; empty when she shows nowhere.",
            persisted=False)
put_rule({
    "key": "MapSpots", "displayName": "World map - where each character shows",
    "description": "Turns each character's Location_ into the world-map button her icon is drawn on (MapSpot_<name>, read by UI mapicons). "
                   "One branch per location; a name ending in * in the old table is a prefix here. Unknown and anything unlisted shows nowhere.",
    "triggerMode": "OnRisingEdge", "forEach": ",".join(MAP_CHARS), "forEachAs": "char",
    "conditions": steps[0]["conditions"], "actions": steps[0]["actions"], "branches": steps[1:]})


ART = "UI/MapIcons/"
behind = [{"type": "LayoutElement", "ignoreLayout": True}]


def icon(c, size, condition):
    return ui("Icon_" + c, rect(size=(size, size)), image=img(ART + "Tile%s.png" % TILE_OF[c], raycast=False, aspect=True),
              activeConditions=[condition])


def piece(name, r, sprite, tint="#FFFFFFFF"):
    """A part of the strip drawn around the row, left out of its layout."""
    return ui(name, r, image=img(sprite, tint, raycast=False), components=behind)


def strip(conditions, beside, pos):
    """One row of portrait tiles on a navy plate, sized to whoever is in it.

    A ContentSizeFitter (one of the game's components, attached by name) makes the plate exactly as long as the
    tiles showing, so it grows and shrinks as people come and go; it is switched off when nobody is there.
    Everything drawn around the row is a child left out of the layout and stretched to the plate (on the plate
    itself a picture's own size would count as the plate's size). The cut corners are pictures at their own size;
    the middle and the top edge are flat colour stretched, so nothing shaped is ever resized. Under it, the game's
    own sparkle divider in the accent. A leader line ties it to a place's button; a wedge points up at a district's.
    beside: True for a place's button (the plate runs out to its right), False for a district's (centred under it)."""
    tile, cap = 30, 12
    sparkle = "#%02X%02X%02XFF" % tuple(int(v + (255 - v) * 0.35) for v in map_badges.rgb(ACCENT)[:3])
    fill = ART + "PlateFill.png"
    pieces = [piece("Fill", rect(size=(-2 * cap, 0), amin=(0, 0), amax=(1, 1)), fill),
              # Each end: its cut corner a picture at its own size, the rest of that end flat navy stretched.
              piece("EndL", rect(size=(cap, -cap), amin=(0, 0), amax=(0, 1), pivot=(0, 0)), fill),
              piece("CornerTL", rect(size=(cap, cap), amin=(0, 1), amax=(0, 1), pivot=(0, 1)), ART + "PlateCornerTL.png"),
              piece("EndR", rect(size=(cap, -cap), amin=(1, 0), amax=(1, 1), pivot=(1, 1)), fill),
              piece("CornerBR", rect(size=(cap, cap), amin=(1, 0), amax=(1, 0), pivot=(1, 0)), ART + "PlateCornerBR.png"),
              piece("Edge", rect(size=(-cap, map_badges.LINE), amin=(0, 1), amax=(1, 1), pivot=(1, 1)), ART + "PlateEdge.png"),
              piece("Sparkle", rect(size=(36, 12), amin=(0, 0), amax=(1, 0), pivot=(0.5, 0.5), pos=(0, -1)),
                    "LineTextFrame_06_White_Line", sparkle),
              piece("Leader", rect(size=(16, 6), amin=(0, 0.5), amax=(0, 0.5), pivot=(1, 0.5)), ART + "Leader.png")
              if beside else
              piece("Notch", rect(size=(12, 6), amin=(0.5, 1), amax=(0.5, 1), pivot=(0.5, 0)), ART + "Notch.png")]
    pad = (cap + 1, cap + 1, 4, 4)
    # Eight to a row, more rows past that; fitToContent (ModForge 1.8.3) keeps a short row's plate as short as
    # what it holds - without it a grid reports all eight columns as its width.
    # Tiles edge to edge: every frame shows whole.
    grid = {"kind": "Grid", "spacing": [0.0, 0.0], "padding": [float(v) for v in pad],
            "alignment": "UpperLeft" if beside else "UpperCenter", "cellSize": [float(tile), float(tile)],
            "constraint": "FixedColumnCount", "constraintCount": 8, "startAxis": "Horizontal", "startCorner": "UpperLeft",
            "fitToContent": True}
    if not beside:
        grid["centerLastLine"] = True
    return ui("AndroidsMapIcons", rect(pos=pos, size=(tile + pad[0] + pad[1], tile + pad[2] + pad[3]),
                                       pivot=(0, 0.5) if beside else (0.5, 1)),
              layout=grid,
              components=[{"type": "ContentSizeFitter", "horizontalFit": "PreferredSize", "verticalFit": "PreferredSize"}],
              activeConditions=[any_of(conditions)],
              # Fades in whenever it appears - the map opening, its district opening, someone arriving.
              open={"fade": True, "duration": 0.4, "easing": "QuadInOut"},
              upright=True,   # follows its button round the radial menu without turning with it (ModForge #17)
              children=pieces + [icon(c, tile, cond) for c, cond in zip(MAP_CHARS, conditions)])


# A place's button is 64 across, a district's 92; the leader and the wedge fill the gap.
grafts = []
for button in dict.fromkeys(b for _, b in LOCATION_TO_BUTTON):
    grafts.append({"bind": "Radial_Buttons/" + button,
                   "node": strip([cmp("MapSpot_" + c, button) for c in MAP_CHARS], True, (32 + 16, 0))})
for group in dict.fromkeys(b.split("/")[0] for _, b in LOCATION_TO_BUTTON):
    grafts.append({"bind": "District_Buttons/" + DISTRICT[group],
                   "node": strip([starts("MapSpot_" + c, group + "/") for c in MAP_CHARS], False, (0, -(46 + 8)))})

with tempfile.TemporaryDirectory() as tmp:
    spec, made = os.path.join(tmp, "spec.json"), os.path.join(tmp, "ui.json")
    json.dump({"id": "mapicons", "name": "World map - character icons", "source": "vanillaui:World_Map/Canvas/Core",
               "grafts": grafts}, open(spec, "w", encoding="utf-8"))
    subprocess.run([PACKTOOL, "ui-graft", spec, made], check=True, stdout=subprocess.DEVNULL)
    map_ui = json.load(open(made, encoding="utf-8"))


put_ui(map_ui)

# ---------------------------------------------------------------------------------------------- P4 Harbor Home
# What SMSAndroidsCore's Places and Core still did in C#, as pack data (ModForge 1.8.3: copies of the game's
# objects, the Travel action).


def place(key):
    return next(pl for pl in d["places"] if pl["key"] == key)


def put_object(key, node):
    """Add (or replace, by name) a top-level object of a place."""
    objects = place(key).setdefault("gameObjects", [])
    at = next((i for i, g in enumerate(objects) if g.get("name") == node["name"]), None)
    if at is None:
        objects.append(node)
        done.append("P4 %s: %s" % (key, node["name"]))
    elif objects[at] != node:
        objects[at] = node
        done.append("P4 %s: %s (updated)" % (key, node["name"]))


def moved(name, x, y):
    """One of a copy's own children, moved (local position)."""
    return {"name": name, "bind": True, "overrideTransform": True, "x": float(x), "y": float(y)}


# The game bedroom's sleep / calendar / PC buttons, copied into the Harbor Home bedroom with the buttons moved to
# its furniture and the drift with the mouse softened.
put_object("HarborHomeBedroom", {
    "name": "PlayerRoom_ButtonCanvas", "copyOf": "5_Levels/5_MyRoom/PlayerRoom_ButtonCanvas",
    "startActive": True, "overrideActive": True,
    "children": [{"name": "Player_Room_Buttons", "bind": True, "overrideParallax": True, "parallaxStrength": 0.05,
                  "children": [moved("SleepButton", 0, -140), moved("CalendarButton", -20, 250), moved("PCButton", -720, -100)]}]})
# The living room's TV objects, for the movie nights.
put_object("HarborHomeLivingRoom", {"name": "Movies", "copyOf": "5_Levels/3_LivingRoom/Movies"})
# The affection effect the pack's affection lines switch on by path: the game's "Loving" one, copied beside it,
# renamed and tinted pink (its hearts a deeper pink, set on their own particle system).
AFFECTION = "CFXR2 Expression Affectionate (Timed)"
put_object("HarborHomeLivingRoom", {
    "name": AFFECTION, "copyOf": "6_Effects/CFXR2 Expression Loving (Timed)", "copyBeside": True,
    "particleColor": "#FF99E6", "children": [{"name": "Hearts", "bind": True, "particleColor": "#FF59B3"}]})

# Waking up at Harbor Home: after a night slept there (HarborHome_Slept, the HarborHomeSleepFlag rule), the game's
# bedroom gives way to the house's - once per waking, so a later trip to the old room stays a trip there.
add_var("HarborHome_WokeThere", "Bool", "false",
        "Set once the player has woken up at Harbor Home after the last sleep (or has gone on into the house); re-armed "
        "by each sleep. Keeps HHWakeInHarborHome to waking up.", persisted=False)
sleep_rule = next((r for r in d["integrationRules"] if r["key"] == "HarborHomeSleepFlag"), None)
if sleep_rule:
    rearm = setv("HarborHome_WokeThere", "false")
    for group in [sleep_rule] + sleep_rule.get("branches", []):
        if rearm not in group.get("actions", []):
            group.setdefault("actions", []).append(rearm)
            done.append("P4 HarborHomeSleepFlag re-arms the waking")
put_rule({
    "key": "HHWakeInHarborHome", "displayName": "Harbor Home - wake up there",
    "description": "After a night slept at Harbor Home, the game's bedroom gives way to the house's: the same journey as its "
                   "navigator button, as soon as the game is free (ModForge's Travel).",
    "triggerMode": "OnRisingEdge",
    "conditions": [cmp("HarborHome_Slept", "true"), cmp("HarborHome_WokeThere", "false"), level("vanilla:5_MyRoom")],
    "actions": [{"type": "Travel", "params": {"target": "place:HarborHomeBedroom"}}, setv("HarborHome_WokeThere", "true")]})
put_rule({
    "key": "HHWakeSettled", "displayName": "Harbor Home - woken up and gone on",
    "description": "Woken up in the house itself and gone into another of its rooms: the morning is over, so a trip to the "
                   "game's bedroom later is a trip there.",
    "triggerMode": "OnRisingEdge",
    "conditions": [cmp("HarborHome_WokeThere", "false"),
                   any_of([level("place:" + r) for r in ROOMS + ["HarborHouseEntrance"] if r != "HarborHomeBedroom"])],
    "actions": [setv("HarborHome_WokeThere", "true")]})

text = json.dumps(d, indent=2, ensure_ascii=False).replace("\n", "\r\n")
if raw.startswith("\ufeff"):
    text = "\ufeff" + text
open(out, "w", encoding="utf-8", newline="").write(text)
print("\n".join(done) if done else "nothing to do")
