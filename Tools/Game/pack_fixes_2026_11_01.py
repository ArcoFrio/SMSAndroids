"""The SMSAndroidsPack fixes from the 2026-11-01 in-game test (docs/INGAME-TEST-2026-11-01.md), as one pass.

  python -I Tools/Game/pack_fixes_2026_11_01.py <modpack.json in> <modpack.json out>

Idempotent: running it on a pack that already has a fix leaves that part alone. Keeps the editor's formatting
(2-space indent, CRLF) so the diff is only the change.

  F1  Gifting: the window offers only gifts the player has; giving one uses it up and starts the after-gift
      dialogue (Gifting_Gifted); the after-gift dialogue raises affection on its liked branches, sets the daily
      gift lock, and leaves Anis's bust when she is at Harbor Home (in her lab room the bust is how she stands
      there, so it stays).
  F2  HarborHome_Outfit_Anis holds outfit names everywhere (placements, default, old saves converted).
  F3  Harbor Home exits leave Anis's bust.
  F4  The gifting window, redrawn: no overlaps, names that fit, a panel, a title and a way out.
  F5  The gift shop: its own canvas (it was drawn inside the hidden HUD - invisible, and the game stuck), closing
      it brings the HUD back, no leftover coin counters, and an item you cannot afford says so.
  F6  Movie night: the three TV shows rebuilt as pack dialogues from the game's own (lines, slides, mouths,
      the show's music), the game's own episode counters (advanced as the game does) and the news's seen flag -
      nothing else of the game's state (no method calls, nothing that could move a story on); picking one plays it,
      and AnisRandomMovie01 follows it once it is over.
"""
import copy
import json
import os
import sys

src, out = sys.argv[1], sys.argv[2]
raw = open(src, encoding="utf-8-sig", newline="").read()
d = json.loads(raw)
done = []

VANILLA_MOVIES = os.path.join(os.path.dirname(os.path.abspath(__file__)), "..", "..", "..", "SMSModForge", "SMSModForge",
                              "Resources", "VanillaDialogues", "Dialogues")


def var(name):
    return next((v for v in d["variables"] if v["name"] == name), None)


def add_var(name, type_, default, description, persisted=True, refresh=None):
    if var(name):
        return
    v = {"name": name, "type": type_, "defaultValue": default, "persisted": persisted}
    if refresh:
        v["refreshMode"] = refresh
    v["description"] = description
    d["variables"].append(v)
    done.append("variable " + name)


def dialogue(key):
    return next(x for x in d["dialogues"] if x["key"] == key)


def node(dl, nid):
    return next(n for n in dl["nodes"] if n["id"] == nid)


def acts(n, which="actionsOnFinish"):
    n.setdefault(which, [])
    return n[which]


def has(actions, type_, **params):
    return any(a["type"] == type_ and all(a.get("params", {}).get(k) == v for k, v in params.items()) for a in actions)


def cmp(name, value, comparison="equals", source=None, negate=False):
    c = {"type": "VariableCompare", "params": {"name": name, "value": value, "comparison": comparison}}
    if source:
        c["params"]["source"] = source
    if negate:
        c["negate"] = True
    return c


def setv(name, value, source=None):
    a = {"type": "SetVariable", "params": {"name": name, "value": value}}
    if source:
        a["params"]["source"] = source
    return a


def go_active(path, active):
    return {"type": "SetGameObjectActive", "params": {"kind": "Direct Path", "target": path, "active": "true" if active else "false"}}


def text_node(nid, actor, text, conditions=None, start=None, finish=None, children=None, timeout=None, outfit=""):
    n = {"id": nid, "kind": "Text", "actor": actor, "expression": "neutral", "outfit": outfit, "text": text,
         "conditions": conditions or [], "actionsOnStart": start or [], "actionsOnFinish": finish or [],
         "children": children or []}
    if timeout is not None:
        n["duration"] = "Timeout"
        n["timeout"] = timeout
    return n


# ---------------------------------------------------------------------------------------------- gifts
# The gift list, from the window as the author built it: label, the value written to Gifting_Gift, art, and
# where the game keeps whether the player has one (the game's own items are its variables, the shop's are ours).
GIFT_UI = next(u for u in d["uis"] if u["id"] == "giftui")
GIFTS = []
if GIFT_UI["nodes"][0]["name"] == "GiftWindow" and GIFT_UI["nodes"][0]["children"][0]["name"] == "Gifts":
    for card in GIFT_UI["nodes"][0]["children"][0]["children"]:
        button = next(c for c in card["children"] if c["name"] == "Button")
        gift = next(a["params"]["value"] for a in button["onClick"] if a["params"].get("name") == "Gifting_Gift")
        art = next(c["image"]["sprite"] for c in card["children"] if c.get("image", {}).get("sprite", "").startswith("Items/"))
        GIFTS.append((card["name"], gift, art))
else:
    # Already redrawn (a second run): the same list, read off the new cards.
    panel = next(c for c in GIFT_UI["nodes"][0]["children"] if c["name"] == "Panel")
    for card in next(c for c in panel["children"] if c["name"] == "Gifts")["children"]:
        gift = next(a["params"]["value"] for a in card["onClick"] if a["params"].get("name") == "Gifting_Gift")
        art = next(c["image"]["sprite"] for c in card["children"] if c["name"] == "Icon")
        GIFTS.append((card["name"], gift, art))


def owned(gift, negate=False):
    """The gift is the player's: the shop's items are pack variables (Gift_*), the rest the game's own."""
    if gift.startswith("Gift_"):
        return cmp(gift, "true", negate=negate)
    return cmp(gift, "true", source="vanilla", negate=negate)


def use_up(gift):
    return setv(gift, "false") if gift.startswith("Gift_") else setv(gift, "false", source="vanilla")


# F1 - the after-gift dialogue
LIKED = {1101037876: "Beer", -187467026: "Body-Oil", 1604517939: "Chocolate", 1177834438: "Gift_Sunglasses", 1594721460: "Wine"}
gift_dl = dialogue("AnisDialogueGift")
first = node(gift_dl, 2041717051)
if not has(first["actionsOnStart"], "SetVariable", name="DailyProc_Gifting_Anis"):
    first["actionsOnStart"].append(setv("DailyProc_Gifting_Anis", "true"))
    done.append("F1 gift lock for the day")
for nid in LIKED:
    n = node(gift_dl, nid)
    if not has(n["actionsOnStart"], "IncrementVariable", name="Affection_Anis"):
        n["actionsOnStart"].append({"type": "IncrementVariable", "params": {"name": "Affection_Anis", "delta": "1"}})
        done.append("F1 affection on " + LIKED[nid])
LEAVE_ID = 2041717052
if LEAVE_ID not in gift_dl["rootNodeIds"]:
    # Every branch used to Exit; now each runs on through the remaining roots (their gift conditions skip
    # them) to a silent last root that leaves her bust at Harbor Home only.
    for n in gift_dl["nodes"]:
        if (n.get("jump") or {}).get("mode") == "Exit":
            del n["jump"]
    fallback = node(gift_dl, 2032850330)
    fallback["conditions"] = [cmp("Gifting_Gift", g, negate=True) for g in LIKED.values()]
    gift_dl["nodes"].append(text_node(LEAVE_ID, "", "", timeout=0.1,
                                      conditions=[{"type": "VariableStartsWith", "params": {"name": "Location_Anis", "value": "HarborHome", "ignoreCase": "false"}}],
                                      start=[{"type": "LeaveBust", "params": {"actor": "anis"}}]))
    gift_dl["rootNodeIds"].append(LEAVE_ID)
    done.append("F1 after-gift bust leaves at Harbor Home")

# F2 - outfit names
ov = var("HarborHome_Outfit_Anis")
if ov["defaultValue"] != "AnisBase":
    ov["defaultValue"] = "AnisBase"
    ov["description"] = ("Anis's outfit at Harbor Home, as one of her outfits (AnisBase, AnisSwim, AnisTopless): her lines' "
                         "$HarborHome_Outfit_Anis binding and the room placements both read it.")
    done.append("F2 outfit default AnisBase")


def any_of(values):
    return {"type": "Any", "conditions": [cmp("HarborHome_Outfit_Anis", v) for v in values]}


def walk_places(n):
    for i, c in enumerate(n.get("activeConditions") or []):
        p = c.get("params", {})
        if c.get("type") == "VariableCompare" and p.get("name") == "HarborHome_Outfit_Anis":
            if p.get("value") == "Default":
                n["activeConditions"][i] = any_of(["AnisBase", "AnisTopless"])
                done.append("F2 placement Default")
            elif p.get("value") == "Swim":
                n["activeConditions"][i] = any_of(["AnisSwim"])
                done.append("F2 placement Swim")
    for c in n.get("children") or []:
        walk_places(c)


for place in d["places"]:
    for g in place.get("gameObjects") or []:
        walk_places(g)
if not any(r["key"] == "HarborHomeOutfitNames" for r in d["integrationRules"]):
    def set_to(old, new):
        return {"conditions": [cmp("HarborHome_Outfit_Anis", old)], "actions": [setv("HarborHome_Outfit_Anis", new)]}
    first_branch = set_to("Default", "AnisBase")
    d["integrationRules"].append({
        "key": "HarborHomeOutfitNames", "displayName": "Harbor Home - Anis outfit names",
        "description": "Saves from before the outfit variable held outfit names say Default or Swim; turns them into AnisBase / AnisSwim, the outfits her lines and placements read.",
        "triggerMode": "OnRisingEdge", "conditions": first_branch["conditions"], "actions": first_branch["actions"],
        "branches": [set_to("Swim", "AnisSwim")]})
    done.append("F2 rule HarborHomeOutfitNames")

# F3 - Harbor Home exits leave the bust (the gift exit keeps her up for the reaction)
for key in ("AnisDialogueHHDefault", "AnisDialogueHHDefaultMovie", "AnisDialogueHHDefaultPool",
            "AnisRandomMovie01", "AnisRandomShower01", "AnisRandomSleep01"):
    for n in dialogue(key)["nodes"]:
        fin = n.get("actionsOnFinish") or []
        if (n.get("jump") or {}).get("mode") != "Exit" or has(fin + (n.get("actionsOnStart") or []), "LeaveBust"):
            continue
        if any(a["type"] == "SetGameObjectActive" and a["params"].get("kind") == "UI" for a in fin):
            continue
        fin.append({"type": "LeaveBust", "params": {"actor": "anis"}})
        n["actionsOnFinish"] = fin
        done.append("F3 LeaveBust " + key)

# ---------------------------------------------------------------------------------------------- F4 gift window
NAVY, RAISED, ACCENT = "#000E20F7", "#0A1A33FF", "#AF54E7"


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


def img(sprite, tint="#FFFFFFFF", type_="Simple", ppu=None, raycast=True, aspect=False):
    i = {"sprite": sprite, "type": type_, "tint": tint}
    if ppu:
        i["pixelsPerUnitMultiplier"] = ppu
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


RESTORE_HUD = {"type": "EmitSignal", "params": {"signal": "FadeUI"}}
if GIFT_UI["nodes"][0]["name"] == "GiftWindow" and GIFT_UI["nodes"][0]["children"][0]["name"] == "Gifts":
    cards = []
    for label, gift, art in GIFTS:
        cards.append(ui(label, rect(size=(150, 176)), image=img("Rounded", RAISED, "Sliced", 1.6),
                        activeConditions=[owned(gift)],
                        onClick=[setv("Gifting_Gift", gift), use_up(gift), setv("Gifting_Gifted", "true"),
                                 {"type": "SetGameObjectActive", "params": {"kind": "UI", "target": "giftui", "active": "false"}}],
                        hoverTint="#3A2A5CFF",
                        children=[
                            ui("Stroke", rect(amin=(0, 0), amax=(1, 1), size=(0, 0)), image=img("Semi Rounded - Stroke 2px", ACCENT + "59", "Sliced", 1.6, raycast=False)),
                            ui("Glow", rect((0, 22), (120, 120)), image=img("Gift (Bold)", ACCENT + "1A", raycast=False)),
                            ui("Icon", rect((0, 22), (104, 104)), image=img(art, raycast=False, aspect=True)),
                            ui("Name", rect((0, -58), (138, 44)), text=txt(label, 26, auto=(15, 26))),
                        ]))
    nothing = ui("Nothing", rect((0, -40), (900, 120)), text=txt("You have nothing to give right now.", 34, "#AFC3E6FF"),
                 activeConditions=[owned(g, negate=True) for _, g, _ in GIFTS])
    window = ui("GiftWindow", rect(amin=(0, 0), amax=(1, 1), size=(0, 0)), children=[
        ui("Dim", rect(amin=(0, 0), amax=(1, 1), size=(0, 0)), image=img("Rounded", "#00000080", "Sliced")),
        ui("Panel", rect((0, 0), (1280, 780)), image=img("Rounded", NAVY, "Sliced", 1.2),
           shadow={"color": "#000000A0", "distance": [8.0, -8.0]},
           open={"fade": True, "scaleFrom": [0.95, 0.95], "duration": 0.25, "easing": "QuadInOut"},
           children=[
               ui("Accent", rect((0, 0), (-120, 6), (0, 1), (1, 1), (0.5, 1)), image=img("Semi Rounded", ACCENT + "FF", "Sliced", raycast=False)),
               ui("Title", rect((0, -78), (900, 84), (0.5, 1), (0.5, 1)), text=txt("Give a gift", 66, font="Barton SDF")),
               ui("Subtitle", rect((0, -132), (900, 40), (0.5, 1), (0.5, 1)), text=txt("Pick something from your bag.", 30, "#AFC3E6FF")),
               ui("Close", rect((-62, -62), (76, 76), (1, 1), (1, 1)), image=img("Circle - 256", RAISED), hoverTint=ACCENT + "FF",
                  onClick=[{"type": "SetGameObjectActive", "params": {"kind": "UI", "target": "giftui", "active": "false"}},
                           setv("Gifting_Target", ""), RESTORE_HUD],
                  children=[ui("X", rect((0, 0), (34, 34)), image=img("Close", raycast=False))]),
               ui("Gifts", rect((0, -62), (1160, 560)), layout={
                   "kind": "Grid", "spacing": [16.0, 16.0], "padding": [0.0, 0.0, 0.0, 0.0], "alignment": "UpperCenter",
                   "controlWidth": False, "controlHeight": False, "expandWidth": False, "expandHeight": False,
                   "cellSize": [150.0, 176.0], "constraint": "FixedColumnCount", "constraintCount": 7,
                   "startAxis": "Horizontal", "centerLastLine": True, "startCorner": "UpperLeft", "reverse": False},
                  children=cards),
               nothing,
           ]),
    ])
    GIFT_UI["nodes"] = [window]
    done.append("F4 gift window redrawn (%d gifts)" % len(cards))

# ---------------------------------------------------------------------------------------------- F5 gift shop
shop = next(u for u in d["uis"] if u["id"] == "giftshop")
if shop.get("hidesWithGameplayUi", True):
    shop["hidesWithGameplayUi"] = False
    shop["sortingOrder"] = 100
    done.append("F5 shop on its own canvas")
root = shop["nodes"][0]
if any(c["name"] == "Image (2)" for c in root["children"]):
    root["children"] = [c for c in root["children"] if c["name"] != "Image (2)"]
    done.append("F5 coin counters removed")
close = next(c for c in root["children"] if c["name"] == "CloseStore")
if not has(close["onClick"], "EmitSignal", signal="FadeUI"):
    close["onClick"].append(RESTORE_HUD)
    done.append("F5 closing the shop brings the HUD back")
if close["image"]["tint"] == "#FFFFFFFF":
    # Its X is white: on a white button it was not there at all.
    close["image"] = img("Circle - 256", RAISED)
    close["hoverTint"] = ACCENT + "FF"
    done.append("F5 close button visible")
row = next(c for c in next(c for c in root["children"] if c["name"] == "GiftStore")["children"] if c["name"] == "Core")
if row["layout"]["spacing"][0] > 20:
    # Nine items in stock ran past the panel's right edge.
    row["layout"]["spacing"][0] = 20.0
    done.append("F5 the full row fits the panel")


def shop_cards(n):
    for c in n.get("children") or []:
        if any(g["name"] == "Button" for g in c.get("children") or []):
            yield c
        yield from shop_cards(c)


for card in shop_cards(root):
    button = next(g for g in card["children"] if g["name"] == "Button")
    price = next(c["params"]["value"] for c in button["clickConditions"] if c["params"].get("name") == "Cash")
    label = button["children"][0]
    if not label.get("activeConditions"):
        label["activeConditions"] = [cmp("Cash", price, "greater or equal", "vanilla")]
        short = copy.deepcopy(label)
        short["name"] = "Short"
        short["text"].update({"value": "Can't afford", "size": 24.0, "color": "#A33A3AFF"})
        short["activeConditions"] = [cmp("Cash", price, "greater or equal", "vanilla", negate=True)]
        button["children"].append(short)
        done.append("F5 can't-afford label")

# ---------------------------------------------------------------------------------------------- F6 movie night
# The shows' speakers. Sci-fi's and Fantasy's are drawn on the slides, so they are names only; the news
# anchor is the game's own Claudia, bust and all, as the game shows her.
VANILLA_ACTORS = {"Claudia": "claudia"}
TV = {"M_SpaceAlien": ("tvVarrus", "Varrus", "male"), "M_SpaceCaptain": ("tvSpectre", "James Spectre", "male"),
      "M_DemonLord": ("tvDemonLord", "The Demon Lord", "male"),
      "M_Schwitzen": ("tvSchwitzen", "Schwitzen", "male")}
for role, (key, name, pronouns) in TV.items():
    if not any(c["key"] == key for c in d["characters"]):
        d["characters"].append({"key": key, "name": name, "displayName": name, "nameColor": "#FFFFFF", "bustSource": "None",
                                "defaultOutfit": "", "expressions": [{"key": "neutral", "expressionGoName": ""}],
                                "pronouns": pronouns})
        done.append("F6 TV character " + name)

SHOWS = [("Scifi", "X_Movies_Scifi_Dialogue.json", "scifi-movie", -341363289, 1829101018, 910000001),
         ("Fantasy", "X_Movies_Fantasy_Dialogue.json", "fantasy-movie", -279621954, -861631083, 910000002),
         ("News", "X_Movies_News_Dialogue.json", "news-movie", 2110337104, -749642050, 910000003)]
add_var("HarborHome_Movie", "String", "", "The TV show picked with Anis in the living room, waiting to play (Scifi, Fantasy, News); cleared when it starts.", persisted=False)
add_var("HarborHome_MovieOver", "Bool", "false", "A TV show has just finished in the living room; AnisRandomMovie01 plays on it and clears it.", persisted=False)


# The game's variables a show may write, as the game's own show does: the news marks itself seen. The episode
# counters are advanced at the end, the way the game's after-show scripts do. Nothing else of the game's state is
# touched - in particular nothing that could move a story on (the author, 2026-11-01).
GAME_FLAGS_KEPT = {"claudia-newsseen"}


def translate(instr):
    """A game instruction as pack actions, or None for one left out: what is drawn or heard, and the few game
    variables in GAME_FLAGS_KEPT; never a method call."""
    t = instr.get("type")
    f = instr.get("fields", {})
    if t == "InstructionGameObjectSetActive":
        path = (f.get("m_GameObject") or {}).get("path")
        if not path or not (path.startswith("X_Movies") or path.startswith("12_AudioPlayer")):
            return None
        return go_active(path, bool((f.get("m_Active") or {}).get("value")))
    if t == "InstructionCommonTimeWait":
        return {"type": "Wait", "params": {"seconds": str((f.get("m_Seconds") or {}).get("value", 1))}}
    if t == "InstructionBooleanSetBool":
        variable = (f.get("m_Set") or {}).get("variable") or {}
        if variable.get("scope") == "global" and variable.get("name") in GAME_FLAGS_KEPT:
            return setv(variable["name"], "true" if (f.get("m_From") or {}).get("value") else "false", source="vanilla")
    return None


for show, file, counter, choice_id, exit_id, end_id in SHOWS:
    key = "HHMovie" + show
    # Built afresh every run from the game's extract, so a change here reaches a pack that has an older build;
    # the pack-only episode counters of the first build (2026-11-01, never released) go with it.
    d["dialogues"] = [x for x in d["dialogues"] if x["key"] != key]
    d["integrationRules"] = [r for r in d["integrationRules"] if r["key"] != "HHMovieEpisodes" + show]
    d["variables"] = [v for v in d["variables"] if v["name"] != "HarborHome_MovieEp_" + show]
    g = json.load(open(os.path.join(VANILLA_MOVIES, file), encoding="utf-8"))
    nodes, roots, episodes = [], [], set()

    def build(gid):
        gn = g["nodes"][str(gid)]
        start = [a for a in (translate(i) for i in gn.get("onStart") or []) if a]
        finish = [a for a in (translate(i) for i in gn.get("onFinish") or []) if a]
        conditions = []
        for c in gn.get("conditions") or []:
            title = c.get("title", "")
            if "[" + counter + "]" in title:
                n_ep = title.rsplit("=", 1)[1].strip()
                episodes.add(int(n_ep))
                conditions.append(cmp(counter, n_ep, source="vanilla"))
        actor = VANILLA_ACTORS.get(gn.get("actor")) or TV.get(gn.get("actor"), ("", "", ""))[0]
        text = gn.get("text", "")
        timeout = gn.get("timeout") if gn.get("duration") == "Timeout" else None
        if not text and timeout is not None:
            # A silent beat that runs a list with waits: long enough for its waits to play out.
            waits = sum(float(a["params"]["seconds"]) for a in start + finish if a["type"] == "Wait")
            timeout = max(float(timeout), waits + 0.3)
        children = [build(c) for c in gn.get("children") or []]
        nid = int(gid)
        made = text_node(nid, actor, text, conditions, start, finish, children, timeout)
        if gn.get("expression") == 1:
            made["expression"] = "Happy"
        nodes.append(made)
        return nid

    for r in g["roots"]:
        roots.append(build(r))
    count = max(episodes) + 1 if episodes else 1
    # Before the show: it is no longer waiting, and the TV is the screen.
    opening = node({"nodes": nodes}, roots[0])
    opening["actionsOnStart"] = [setv("HarborHome_Movie", ""), go_active("X_Movies", True)] + opening["actionsOnStart"]
    # After it: the next episode next time, round to the first; the room's follow-up may play.
    nodes.append(text_node(end_id, "", "", timeout=0.1,
                           start=[{"type": "LeaveBust", "params": {"actor": a}} for a in sorted({n["actor"] for n in nodes} & set(VANILLA_ACTORS.values()))] +
                                 [go_active("X_Movies", False),
                                  {"type": "IncrementVariable", "params": {"name": counter, "delta": "1", "source": "vanilla"}},
                                  setv("HarborHome_MovieOver", "true")]))
    roots.append(end_id)
    d["dialogues"].append({"key": key, "displayName": key,
                           "startConditions": [{"type": "LevelActive", "params": {"level": "place:HarborHomeLivingRoom"}},
                                               cmp("HarborHome_Movie", show)],
                           "rootNodeIds": roots, "queueBehind": True, "nodes": nodes})
    d["integrationRules"].append({
        "key": "HHMovieEpisodes" + show, "displayName": "Harbor Home - %s episodes" % show,
        "description": ("The game's own %s counter after a Harbor Home showing: past its last episode (%d) the next "
                        "one is the first again, as the game's %s_Condition does after its own showing. Only the "
                        "pack's +1 ever takes it to %d." % (counter, count - 1, show, count)),
        "triggerMode": "OnRisingEdge", "conditions": [cmp(counter, str(count), "greater or equal", "vanilla")],
        "actions": [setv(counter, "0", source="vanilla")]})
    # Picking the show: it plays once Anis's dialogue ends, and that is the day's movie.
    movie_dl = dialogue("AnisDialogueHHDefaultMovie")
    exit_node = node(movie_dl, exit_id)
    if not has(exit_node.get("actionsOnFinish") or [], "SetVariable", name="HarborHome_Movie"):
        exit_node["actionsOnFinish"] = [setv("HarborHome_Movie", show), setv("DailyProc_Anis_TV", "true")] + (exit_node.get("actionsOnFinish") or [])
    done.append("F6 %s show (%d nodes, %d episodes, the game's %s)" % (show, len(nodes), count, counter))

movie_dl = dialogue("AnisDialogueHHDefaultMovie")
ask = node(movie_dl, 6027698)
for which in ("actionsOnStart", "actionsOnFinish"):
    if has(ask.get(which) or [], "SetVariable", name="DailyProc_Anis_TV"):
        # Changing your mind no longer uses up the day's movie; picking a show does (above).
        ask[which] = [a for a in ask[which] if not (a["type"] == "SetVariable" and a["params"].get("name") == "DailyProc_Anis_TV")]
        done.append("F6 the day's movie is used by picking a show")
after = dialogue("AnisRandomMovie01")
if not after.get("queueBehind"):
    # Straight after the show: it queues behind it rather than starting seconds later from the room.
    after["queueBehind"] = True
    done.append("F6 AnisRandomMovie01 queues behind the show")
if not any(c.get("params", {}).get("name") == "HarborHome_MovieOver" for c in after["startConditions"]):
    after["startConditions"].append(cmp("HarborHome_MovieOver", "true"))
    for rid in after["rootNodeIds"]:
        r = node(after, rid)
        r["actionsOnStart"] = [setv("HarborHome_MovieOver", "false")] + (r.get("actionsOnStart") or [])
    done.append("F6 AnisRandomMovie01 follows the show")

crlf = "\r\n" in raw
text = json.dumps(d, ensure_ascii=False, indent=2)
if crlf:
    text = text.replace("\n", "\r\n")
open(out, "w", encoding="utf-8", newline="").write(text + raw[len(raw.rstrip()):])
import collections
for c, k in collections.Counter(done).items():
    print("%3d  %s" % (k, c))
