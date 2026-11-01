"""SMSAndroids' full check in the game: the pack as ModForge builds it, and whether it does what it is meant to.

  python -I Tools/Game/ingame_check.py                 # every area
  python -I Tools/Game/ingame_check.py places dialogues  # some of them

Run export-pack.ps1 first (the pack under test), then this; it starts the test copy itself through launch.ps1
(its own desktop, muted, only the test save, never while the author's game runs) and closes it at the end.
Areas, each relaunching the game so none inherits another's state:

  startup    the load chain of both mods, ModForge's own warnings, errors logged while loading
  places     every place of the pack: travel there, what level is up, a screenshot, errors
  behaviour  what the pack is for (the port of the old SMSAndroids): schedules, Harbor Home, gifting, the gift
             shop, Secret Beach, the voyeur pick - each set up, run, and read back
  dialogues  every dialogue, played from its own place, every choice taken over repeated plays; each line read
             off the box (speaker, words, unfilled {tags}), the busts and scene pictures up, errors per play;
             coverage of the pack's lines counted by their words
  ui         the pack's UIs and SMSAndroids' own screens drawn and dumped for their look

Everything goes to %TEMP%\\SMSAndroids-ingame\\run-<time>\\: report.md (the findings), steps.jsonl (everything the
bridges answered), shots\\ and ui\\.
"""
import collections
import datetime
import json
import os
import re
import shutil
import subprocess
import sys
import time

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from bridge import ANSWER, Bridge, TEST  # noqa: E402

HERE = os.path.dirname(os.path.abspath(__file__))
PACK_ID = "SMSAndroidsPack"
PACK = json.load(open(os.path.join(HERE, "..", "..", PACK_ID, "modpack.json"), encoding="utf-8-sig"))
RUN = os.path.join(os.environ.get("TEMP", "."), "SMSAndroids-ingame", "run-" + datetime.datetime.now().strftime("%Y%m%d-%H%M"))
SHOTS_FROM = os.path.join(TEST, r"BepInEx\plugins\SMSModForge\DebugBridge\a0\shots")
UI_FROM = os.path.join(TEST, r"BepInEx\plugins\SMSAndroidsCore\DebugBridge\a0")
for d in ("shots", "ui"):
    os.makedirs(os.path.join(RUN, d), exist_ok=True)

mf, sa = Bridge("mf"), Bridge("sa")
findings = []
steps = open(os.path.join(RUN, "steps.jsonl"), "a", encoding="utf-8")


def log(area, what, answer):
    steps.write(json.dumps({"t": time.strftime("%H:%M:%S"), "area": area, "what": what, "answer": answer}, ensure_ascii=False) + "\n")
    steps.flush()


def find(severity, area, text):
    findings.append((severity, area, text))
    print("  [%s] %s: %s" % (severity, area, text), flush=True)


def ask(bridge, command, area="", timeout=120):
    answer = bridge.ask(command, timeout=timeout)
    log(area, command, answer)
    return answer


def body(answer):
    """An answer without its 'ok <verb> ' head."""
    return re.sub(r"^(ok|error|timeout) \S+ ?", "", answer)


def errors(area, where):
    answer = ask(sa, "errors clear", area)
    m = re.match(r"ok errors (\d+)(?: \|\| (.*))?", answer)
    if m and int(m.group(1)) > 0:
        find("error", area, "%s: %s error(s) logged - %s" % (where, m.group(1), (m.group(2) or "")[:600]))
    return int(m.group(1)) if m else -1


def shot(name):
    ask(mf, "shot " + name, "shot")
    src = os.path.join(SHOTS_FROM, name + ".png")
    for _ in range(40):
        if os.path.exists(src):
            break
        time.sleep(0.1)
    time.sleep(0.3)
    try:
        shutil.move(src, os.path.join(RUN, "shots", name + ".png"))
    except OSError:
        pass
    return os.path.join(RUN, "shots", name + ".png")


def ui_dump(name, path, area="ui"):
    answer = ask(sa, "ui %s %s" % (name, path), area)
    src = os.path.join(UI_FROM, name + ".txt")
    if answer.startswith("ok") and os.path.exists(src):
        shutil.move(src, os.path.join(RUN, "ui", name + ".txt"))
        return open(os.path.join(RUN, "ui", name + ".txt"), encoding="utf-8").read()
    return ""


def pvar(name):
    return body(ask(mf, "pvar %s %s" % (PACK_ID, name), "var")).split(" = ", 1)[-1]


def pset(name, value):
    return ask(mf, "pset %s %s %s" % (PACK_ID, name, value), "var")


def gget(name):
    return body(ask(mf, "get " + name, "var")).split(" = ", 1)[-1]


def gset(name, value):
    return ask(mf, "set %s %s" % (name, value), "var")


def playing():
    return body(ask(mf, "playing", "dialogue")).strip() == "True"


def place_token(level):
    """A manifest's place reference as the bridge's travel takes it."""
    if level.startswith("place:"):
        return "pack:%s.%s" % (PACK_ID, level[6:])
    return level


VANILLA_BOX = "Dialogue_Default_Dialogue(Clone)"


def click_vanilla(area):
    """One click on one of the game's own conversations, which the bridge's line/next cannot see: the box's
    words read off the drawn UI, a choice taken if offered, else a click on the box. Returns its words."""
    text = ui_dump("vanilla_box", VANILLA_BOX, area)
    speech = re.search(r'Speech\(Clone\) \[[^\]]*\] Text\("([^"]*)"', text)
    actor = re.search(r'Actor \[[^\]]*\] Text\("([^"]*)"', text)
    words = speech.group(1) if speech else ""
    if any("Choice" in l and "(hidden)" not in l for l in text.splitlines()):
        for t in re.findall(r'Text\("([^"]+)"', text):
            if t not in (words, actor.group(1) if actor else None) and ask(mf, "press-text " + t, area).startswith("ok"):
                return words
    ask(mf, "press %s/Speech/Dialogue_Default_Speech(Clone)/Trigger_Skip_MouseImage" % VANILLA_BOX, area)
    return words


def see_through(area, limit=250):
    """Clicks through whatever conversation is on (arriving somewhere can start one); returns what was said."""
    said = []
    for _ in range(limit):
        if not playing():
            break
        line = ask(mf, "line", area)
        if "(none)" in line:
            # Not a conversation the bridge reads: one of the game's own.
            said.append("vanilla: " + click_vanilla(area))
            time.sleep(0.6)
            continue
        said.append(line)
        choices = re.findall(r"choice: ([^|]+)", line)
        if choices:
            ask(mf, "press-text " + choices[-1].strip(), area)
        else:
            ask(mf, "next", area)
        time.sleep(0.4)
    return said


def travel(token, area, settle=3.0):
    answer = ask(mf, "travel " + token, area)
    if not answer.startswith("ok"):
        find("error", area, "travel %s: %s" % (token, answer))
        return False
    time.sleep(settle)
    # Arriving can start a conversation (queued ones start a moment later): see each through.
    for _ in range(3):
        if not playing():
            time.sleep(1.0)
            if not playing():
                break
        said = see_through(area)
        log(area, "arrival conversation at " + token, said)
        time.sleep(1.5)
    return True


def launch():
    stop()
    out = subprocess.run(["powershell", "-NoProfile", "-ExecutionPolicy", "Bypass", "-File", os.path.join(HERE, "launch.ps1")],
                         capture_output=True, text=True)
    if out.returncode != 0:
        raise SystemExit("launch failed: " + out.stdout + out.stderr)
    answer = ask(mf, "wait-ingame 180", "startup", timeout=240)
    if "ok" not in answer:
        raise SystemExit("the game did not get in: " + answer)
    # The pack's rules and SMSAndroids' load chain settle over the first seconds.
    deadline = time.time() + 60
    while time.time() < deadline:
        if "bases=True" in ask(sa, "state", "startup"):
            break
        time.sleep(1)
    time.sleep(3)


def stop():
    subprocess.run(["powershell", "-NoProfile", "-ExecutionPolicy", "Bypass", "-File", os.path.join(HERE, "stop.ps1")], capture_output=True)


# --------------------------------------------------------------------------------------------- startup
def area_startup():
    area = "startup"
    launch()
    state = ask(sa, "state", area)
    for flag in ("core", "busts", "places", "dialogues", "schedule", "story", "minigame", "visualizer", "bases"):
        if "%s=True" % flag not in state:
            find("error", area, "SMSAndroids' %s never finished loading (%s)" % (flag, state))
    warning = ask(mf, "warning", area)
    if "open" in warning:
        find("warn", area, "ModForge shows its save warning: " + warning)
    errors(area, "loading into the test save")
    shot("startup_room")
    # Every variable the pack declares is there to read.
    for v in PACK["variables"]:
        answer = ask(mf, "pvar %s %s" % (PACK_ID, v["name"]), area)
        if not answer.startswith("ok"):
            find("error", area, "pack variable %s cannot be read in the game: %s" % (v["name"], answer))
    stop()
    # What both plugins logged while the game ran.
    scan_logs(area)


def scan_logs(area):
    for name in ("BepInEx\\LogOutput.log", "Player-a0.log"):
        path = os.path.join(TEST, name)
        if not os.path.exists(path):
            continue
        text = open(path, encoding="utf-8", errors="replace").read()
        shutil.copy(path, os.path.join(RUN, os.path.basename(name).replace(".log", "-%s.log" % area)))
        hits = collections.Counter()
        for line in text.splitlines():
            if re.search(r"\[(Error|Warning|Fatal)\s*:|Exception|NullReference|\bERROR\b|failed", line, re.I) and "[Info" not in line:
                hits[re.sub(r"\d+", "#", line.strip())[:220]] += 1
        for line, count in hits.most_common(40):
            find("log", area, "%s x%d: %s" % (os.path.basename(name), count, line))


# --------------------------------------------------------------------------------------------- places
def area_places():
    area = "places"
    launch()
    for place in PACK["places"]:
        key = place["key"]
        if not travel("pack:%s.%s" % (PACK_ID, key), area):
            continue
        level = body(ask(mf, "level", area))
        if not any(l.endswith("_" + key) or l == key for l in level.split(", ")):
            find("error", area, "after travelling to %s the level up is %s" % (key, level))
        shot("place_" + key)
        errors(area, "visiting " + key)
    for ext in PACK.get("vanillaExtensions", []):
        target = ext.get("source") or ""
        if target.startswith("vanilla:"):
            if travel(target, area):
                shot("vanilla_" + target[8:])
                errors(area, "visiting " + target)
    stop()
    scan_logs(area)


# --------------------------------------------------------------------------------------------- dialogues
def norm(text):
    text = re.sub(r"<[^>]+>", "", text or "")
    return re.sub(r"\s+", " ", text).strip().lower()


def dialogue_setup(dl):
    """Travel to the dialogue's place and make its start conditions true, as far as they can be."""
    level = None
    for c in walk(dl.get("startConditions", [])):
        p = c.get("params", {})
        if c["type"] == "LevelActive" and not c.get("negate"):
            level = p.get("level")
        if c["type"] == "VariableCompare" and p.get("comparison", "equals") == "equals" and not c.get("negate") and p.get("source") != "vanilla":
            pset(p["name"], p.get("value", ""))
    return level


def walk(conds):
    for c in conds or []:
        if c.get("type") in ("Any", "All"):
            yield from walk(c.get("conditions"))
        else:
            yield c


def area_dialogues(only=None):
    area = "dialogues"
    texts = collections.defaultdict(set)   # dialogue -> normalised words of every line it has
    choices_of = collections.defaultdict(set)
    for dl in PACK["dialogues"]:
        for n in dl["nodes"]:
            if n.get("text"):
                (choices_of if n["kind"] == "Choice" else texts)[dl["key"]].add(norm(n["text"]))
    seen = collections.defaultdict(set)
    seen_choices = collections.defaultdict(set)
    taken = collections.defaultdict(lambda: collections.defaultdict(set))   # dialogue -> choice prompt -> choices taken
    launches = 0
    todo = [d for d in PACK["dialogues"] if not only or d["key"] in only]
    launch()
    for i, dl in enumerate(todo):
        key = dl["key"]
        if i and i % 12 == 0:
            # A fresh game every dozen dialogues: what one changes (money, flags, places) stays out of the rest.
            launch()
            launches += 1
        level = dialogue_setup(dl)
        if level:
            travel(place_token(level), area)
        if playing():
            see_through(area)
        errors(area, "before " + key)
        new_in_play = 1
        plays = 0
        while plays < 8 and (new_in_play > 0 or plays < 2):
            plays += 1
            before = len(seen[key]) + len(seen_choices[key])
            play_once(key, dl, seen, seen_choices, taken, plays)
            new_in_play = len(seen[key]) + len(seen_choices[key]) - before
            if not (texts[key] - seen[key]) and not (choices_of[key] - seen_choices[key]):
                break
        errors(area, "playing " + key)
        missed = texts[key] - seen[key]
        total = len(texts[key]) or 1
        print("%s: %d/%d lines, %d/%d choices in %d plays" % (key, len(texts[key] & seen[key]), len(texts[key]),
              len(choices_of[key] & seen_choices[key]), len(choices_of[key]), plays), flush=True)
        log(area, "coverage " + key, {"lines": len(texts[key] & seen[key]), "of": len(texts[key]), "missed": sorted(missed)[:50],
                                       "choices": len(choices_of[key] & seen_choices[key]), "choicesOf": len(choices_of[key])})
    stop()
    all_lines = sum(len(t) for t in texts.values())
    got = sum(len(texts[k] & seen[k]) for k in texts)
    find("info", area, "lines shown in play: %d of %d (%.0f%%); the rest sit behind random picks, conditions or choices "
                       "the plays did not reach" % (got, all_lines, 100.0 * got / max(all_lines, 1)))
    for k in texts:
        if texts[k] and not (texts[k] & seen[k]):
            find("warn", area, "%s: none of its %d lines showed when started through the bridge" % (k, len(texts[k])))
    scan_logs(area)


TOKEN = re.compile(r"\{[^}]*\}|\[PV:[^\]]*\]|\$[A-Za-z_]\w+|\bnull\b|\bNaN\b")


def play_once(key, dl, seen, seen_choices, taken, play):
    area = "dialogues"
    # Who stands in the room before (a lab room's character is drawn as her bust): only busts the
    # dialogue itself left behind count.
    before = {b.split(" [")[0] for b in body(ask(sa, "busts", area)).split(" | ") if b != "(none)"}
    answer = ask(mf, "dialogue %s %s" % (PACK_ID, key), area)
    if not answer.startswith("ok"):
        find("error", area, "%s would not start: %s" % (key, answer))
        return
    time.sleep(1.2)
    last, same, steps_taken, shots, cgs_seen = None, 0, 0, 0, set()
    click = None
    while steps_taken < 250:
        steps_taken += 1
        # One round trip per line: the click, the same pause the game gets either way (on its own clock),
        # then what the box shows and whether the conversation is still on. The bridge runs a batch in order.
        commands = ([click[0], "wait %.2f" % click[1]] if click else []) + ["line", "playing"]
        answers = [a[13:] for a in mf.send(*commands) if ANSWER.match(a)]
        for command, answer in zip(commands, answers):
            log(area, command, answer)
        if len(answers) < len(commands):
            find("error", area, "%s: the bridge stopped answering" % key)
            break
        line, still = answers[-2], body(answers[-1]).strip() == "True"
        click = None
        if "(none)" in line:
            if not still:
                break
            # Something is playing that the bridge cannot read: one of the game's own conversations.
            click_vanilla(area)
            click = ("wait 0.6", 0)
            continue
        speaker = (re.search(r"speaker: ([^|]*)", line) or [None, ""])[1].replace("<noparse></noparse>", "").strip()
        words = (re.search(r"\| line: ([^|]*)", line) or [None, ""])[1].strip()
        choices = [c.strip() for c in re.findall(r"choice: ([^|]+)", line)]
        if words:
            seen[key].add(norm(words))
            for t in TOKEN.findall(words):
                find("error", "dialogues", "%s shows an unfilled %r in %r" % (key, t, words[:120]))
        for c in choices:
            seen_choices[key].add(norm(c))
        if play == 1 and shots == 0 and words:
            # The first line of each dialogue, after its fade and typing.
            time.sleep(1.5)
            shot("dlg_%s" % key)
            shots += 1
        cg = body(ask(sa, "cg", area))
        if cg != "(none)":
            for scene in cg.split(" | "):
                if scene not in cgs_seen:
                    cgs_seen.add(scene)
                    if "DRAWS NOTHING" in scene:
                        find("error", "dialogues", "%s: scene %s is up but draws nothing" % (key, scene))
                    if play == 1:
                        time.sleep(1.0)
                        shot("cg_%s_%s" % (key, re.sub(r"\W", "", scene)[:40]))
        state = (line.split(" node ")[-1].split(" ||")[0], words, tuple(choices))
        # Stuck means no change for 20 s of clicking: a timed line (duration Timeout) ignores clicks on purpose.
        if state == last:
            if time.time() - same > 20:
                find("error", "dialogues", "%s is stuck on %r (no way on after 20 s of clicks)" % (key, words[:100]))
                break
        else:
            same = time.time()
        last = state
        if choices:
            prompt = norm(words) or "?"
            untaken = [c for c in choices if norm(c) not in taken[key][prompt]]
            pick = untaken[0] if untaken else choices[(play - 1) % len(choices)]
            taken[key][prompt].add(norm(pick))
            click = ("press-text " + pick, 0.6)
        else:
            click = ("next", 0.35)
    # A dialogue that hid the HUD must give it back when it ends, and leave no bust standing over the level.
    time.sleep(1.0)
    if playing():
        see_through(area)
    time.sleep(1.5)
    left = {b.split(" [")[0] for b in body(ask(sa, "busts", area)).split(" | ") if b != "(none)"} - before
    if left and not playing():
        find("warn", "dialogues", "%s (play %d) ended with a bust it showed still on screen: %s - last line %r" %
             (key, play, ", ".join(sorted(left)), (last or ("", "", ()))[1][:80]))
    hud = ask(sa, "at 60 680", area)
    if "$" not in hud and "Money" not in hud and "OtherHud" not in hud and "9_MainCanvas" not in hud:
        log(area, "hud after " + key, hud)


# --------------------------------------------------------------------------------------------- behaviour
def wait_for(name, want, seconds=8.0):
    deadline = time.time() + seconds
    value = pvar(name)
    while time.time() < deadline and value != want:
        time.sleep(0.5)
        value = pvar(name)
    return value


def area_behaviour():
    area = "behaviour"
    launch()
    # Schedules: each character met moves to her lab room (the old Schedule.cs defaults).
    lab = {"Anis": "MountainLabRoomNikkeAnis", "Centi": "MountainLabRoomNikkeCenti", "Dorothy": "MountainLabRoomNikkeDorothy",
           "Elegg": "MountainLabRoomNikkeElegg", "Frima": "MountainLabRoomNikkeFrima", "Guilty": "MountainLabRoomNikkeGuilty",
           "Helm": "MountainLabRoomNikkeHelm", "Maiden": "MountainLabRoomNikkeMaiden", "Mary": "MountainLabRoomNikkeMary",
           "Mast": "MountainLabRoomNikkeMast", "Neon": "MountainLabRoomNikkeNeon", "Pepper": "MountainLabRoomNikkePepper",
           "Rapi": "MountainLabRoomNikkeRapi", "Rosanna": "MountainLabRoomNikkeRosanna", "Sakura": "MountainLabRoomNikkeSakura",
           "Tove": "MountainLabRoomNikkeTove", "Viper": "MountainLabRoomNikkeViper", "Yan": "MountainLabRoomNikkeYan"}
    for who, room in lab.items():
        before = pvar("Location_" + who)
        pset("Voyeur_Seen" + who, "true")
        got = wait_for("Location_" + who, room, 6)
        # Day 1 may send some of them out instead (a daily outing); only their lab room or an outing is right.
        if got in ("Unknown", ""):
            find("error", area, "%s: met (Voyeur_Seen%s) but still nowhere after 6 s (was %s)" % (who, who, before))
        elif got != room:
            log(area, "schedule " + who, "went to %s instead of %s (an outing?)" % (got, room))
        if got == room:
            travel("pack:%s.%s" % (PACK_ID, room), area, 2.5)
            dump = ui_dump("npc_" + who, "5_Levels", area)
            shot("lab_" + who)
    errors(area, "schedules")
    # Amber: found once the lab is visited.
    pset("MountainLab_FirstVisited", "true")
    got = wait_for("Location_Amber", "MountainLab", 6)
    if got not in ("MountainLab", "HospitalHallway"):
        find("error", area, "Amber: lab visited but Location_Amber is %s" % got)

    # Harbor Home: Anis's NPC shows for every outfit value the pack writes.
    pset("HarborHome_Bought", "true")
    pset("Location_Anis", "HarborHomeLivingRoomCouchleft")
    travel("pack:%s.HarborHomeLivingRoom" % PACK_ID, area)
    for outfit in ("Default", "Swim", "AnisBase", "AnisSwim", "AnisTopless"):
        pset("HarborHome_Outfit_Anis", outfit)
        time.sleep(1.5)
        dump = ui_dump("hh_outfit_" + outfit, "5_Levels/170_HarborHomeLivingRoom/NPCs/Couchleft", area)
        shown = [l.strip() for l in dump.splitlines() if "(hidden)" not in l and l.strip() not in ("Couchleft", "Anis")]
        if not shown:
            find("error", area, "Harbor Home: with HarborHome_Outfit_Anis = %s, Anis is not drawn on the couch at all" % outfit)
    pset("HarborHome_Outfit_Anis", "Default")

    # Harbor Home roaming: on the visit list, Anis moves between spots and the occupancy list follows her.
    # Watched from another room: by design she never moves while the player looks at the room she is in.
    pset("HarborHome_VisitList", '["Anis"]')
    pset("Location_Anis", "HarborHomeLivingRoomCouchleft")
    travel("pack:%s.HarborHouseEntrance" % PACK_ID, area)
    places_seen = set()
    for _ in range(14):
        time.sleep(5)
        places_seen.add(pvar("Location_Anis"))
    occupied = pvar("HarborHome_Occupied")
    log(area, "roaming", {"locations": sorted(places_seen), "occupied": occupied})
    if len(places_seen) < 2:
        find("error", area, "Harbor Home roaming: Anis stayed at %s for 70 s while on the visit list" % sorted(places_seen))
    errors(area, "Harbor Home")

    # Gifting, the way a player gives one: Anis's dialogue, the gift option, the beer.
    pset("Location_Anis", "MountainLabRoomNikkeAnis")
    pset("DailyProc_Gifting_Anis", "false")
    travel("pack:%s.MountainLabRoomNikkeAnis" % PACK_ID, area)
    beer_before, affection_before = gget("Beer"), pvar("Affection_Anis")
    ask(mf, "dialogue %s AnisDialogueDefault" % PACK_ID, area)
    time.sleep(1.5)
    opened = False
    for _ in range(40):
        line = ask(mf, "line", area)
        if "I have something for you." in line:
            ask(mf, "press-text I have something for you.", area)
        elif "(none)" in line and not playing():
            break
        else:
            ask(mf, "next", area)
        time.sleep(0.6)
        if "active True" in ask(mf, "inspect PackUiCanvas_giftui/Pack_giftui/GiftWindow", area):
            opened = True
            break
    if not opened:
        find("error", area, "Gifting: the gift window never opened from Anis's 'I have something for you.'")
    else:
        shot("gift_window")
        ui_dump("gift_window", "PackUiCanvas_giftui", area)
        ask(mf, "press PackUiCanvas_giftui/Pack_giftui/GiftWindow/Panel/Gifts/Beer", area)
        time.sleep(4)
        reaction = playing()
        if not reaction:
            find("error", area, "Gifting: after giving Anis the beer her reaction (AnisDialogueGift) never starts - "
                                "Gifting_Gifted is %s; the HUD stays hidden until something starts it" % pvar("Gifting_Gifted"))
            # What the button should have done, so the reaction itself is still checked.
            pset("Gifting_Gifted", "true")
            time.sleep(3)
        see_through(area)
        time.sleep(1)
        after = {"Gifting_Gifted": pvar("Gifting_Gifted"), "Affection_Anis": pvar("Affection_Anis"), "Beer": gget("Beer"),
                 "DailyProc_Gifting_Anis": pvar("DailyProc_Gifting_Anis"), "reaction started by itself": reaction}
        log(area, "gift beer", after)
        # Affection rises in the after-gift dialogue, whose branches say what she likes (the author, 2026-11-01).
        if after["Affection_Anis"] == affection_before:
            find("error", area, "Gifting: Anis's reaction to the beer is a liked one ('liquid courage') but Affection_Anis "
                                "stayed %s after it" % affection_before)
        if after["Beer"] == beer_before == "True":
            find("error", area, "Gifting: the beer given is still in the player's inventory")
        if after["DailyProc_Gifting_Anis"] != "true":
            find("error", area, "Gifting: DailyProc_Gifting_Anis not set - she can be given gifts again the same day")
        hud = ask(sa, "at 140 680", area)
        shot("after_gift")
        log(area, "hud after gift", hud)
    errors(area, "gifting")

    # The gift shop: Claire's shop UI, buying one item.
    pset("GiftShop_BuildCounter", "2")
    pset("Location_Claire", "Unknown")
    wait_for("Location_Claire", "GiftShopInterior", 6)
    travel("pack:%s.GiftShopInterior" % PACK_ID, area)
    shot("giftshop_arrive")
    cash = gget("Cash")
    log(area, "gift shop", {"Location_Claire": pvar("Location_Claire"), "cash": cash, "Gift_Sunscreen": pvar("Gift_Sunscreen")})
    errors(area, "gift shop")
    stop()
    scan_logs(area)


# --------------------------------------------------------------------------------------------- hh
def run_dialogue_to_end(area, prefer=None, limit=200, only=None):
    """Clicks through the conversation on screen, taking the choice named in prefer when offered. With only,
    stops as soon as a line belongs to another dialogue (one that queued behind it)."""
    said, busts_seen = [], set()
    for _ in range(limit):
        line = ask(mf, "line", area)
        if only and "SMSAndroidsPack_" in line and ("SMSAndroidsPack_" + only + " ") not in line:
            break
        if "(none)" in line:
            if not playing():
                break
            time.sleep(0.4)
            continue
        said.append(line)
        b = body(ask(sa, "busts", area))
        if b != "(none)":
            busts_seen.add(b.split(" [")[0])
        choices = [c.strip() for c in re.findall(r"choice: ([^|]+)", line)]
        if choices:
            pick = next((c for c in choices if prefer and any(p in c for p in prefer)), choices[-1])
            ask(mf, "press-text " + pick, area)
        else:
            ask(mf, "next", area)
        time.sleep(0.45)
    return said, busts_seen


def area_hh():
    """Anis at Harbor Home, the way a player gets there: the invite, her roaming, talking to her in each room."""
    area = "hh"
    launch()
    for name, value in (("HarborHome_Bought", "true"), ("Voyeur_SeenAnis", "true"), ("MountainLab_FirstVisited", "true"),
                        ("Affection_Anis", "5"), ("Affection_Anis_Seen1", "true"), ("Affection_Anis_Seen2", "true"),
                        ("Affection_Anis_Seen3", "true")):
        pset(name, value)
    time.sleep(2)
    travel("pack:%s.MountainLabRoomNikkeAnis" % PACK_ID, area)
    if not playing():
        ask(mf, "dialogue %s AnisDialogueDefault" % PACK_ID, area)
        time.sleep(1.5)
    run_dialogue_to_end(area, prefer=["Wanna drop by my house?"])
    deadline = time.time() + 60
    while time.time() < deadline and not pvar("Location_Anis").startswith("HarborHome"):
        time.sleep(2)
    where = pvar("Location_Anis")
    if not where.startswith("HarborHome"):
        find("error", area, "invited to Harbor Home, Anis is still at %s a minute later" % where)
        stop()
        return
    # Pin her for the room checks (the dialogues and the talk panel do not read the roaming list).
    pset("HarborHome_VisitList", "[]")
    rooms = [("HarborHomeKitchen", "HarborHomeKitchenSink", "AnisDialogueHHDefault"),
             ("HarborHomeLivingRoom", "HarborHomeLivingRoomCouchleft", "AnisDialogueHHDefaultMovie"),
             ("HarborHomePool", "HarborHomePoolTanningleft", "AnisDialogueHHDefaultPool"),
             ("HarborHomeBedroom", "HarborHomeBedroomBedleft", "AnisDialogueHHDefault")]
    for room, spot, expected in rooms:
        pset("Location_Anis", spot)
        travel("pack:%s.%s" % (PACK_ID, room), area)
        drawn = [l.strip() for l in ui_dump("hh_%s" % room, "5_Levels", area).splitlines()
                 if "Anis" in l and "(hidden)" not in l and "NPC" in l]
        if not drawn:
            find("error", area, "%s: Anis (at %s, outfit %s) is not drawn in the room" % (room, spot, pvar("HarborHome_Outfit_Anis")))
        panel = ask(mf, "inspect 9_MainCanvas/HHTalkPanel/HHTalkButton1", area)
        if "active True" not in panel:
            find("error", area, "%s: no talk button for Anis" % room)
            continue
        ask(mf, "press 9_MainCanvas/HHTalkPanel/HHTalkButton1", area)
        time.sleep(2.5)
        first = ask(mf, "line", area)
        if expected not in first:
            find("error", area, "%s: talking to Anis started %s, not %s" % (room, first[8:90], expected))
        shot("hh_talk_" + room)
        said, busts_seen = run_dialogue_to_end(area, prefer=["Just passing by", "Nah"])
        if not busts_seen:
            find("error", area, "%s: Anis's bust never showed while she talked (outfit %s)" % (room, pvar("HarborHome_Outfit_Anis")))
        time.sleep(2)
        left = body(ask(sa, "busts", area))
        if left != "(none)":
            find("error", area, "%s: after the talk ends, a bust is left on screen: %s" % (room, left[:100]))
            shot("hh_left_" + room)
    # Changing her outfit keeps her drawn, and her bust follows the outfit.
    pset("Location_Anis", "HarborHomeKitchenSink")
    travel("pack:%s.HarborHomeKitchen" % PACK_ID, area)
    for outfit_choice, outfit in (("Sparkling Summer", "AnisSwim"), ("Default", "AnisBase")):
        ask(mf, "press 9_MainCanvas/HHTalkPanel/HHTalkButton1", area)
        time.sleep(2.5)
        run_dialogue_to_end(area, prefer=["(Change outfit.)", outfit_choice])
        time.sleep(2)
        value = pvar("HarborHome_Outfit_Anis")
        if value != outfit:
            find("error", area, "outfit change to %s left HarborHome_Outfit_Anis = %s" % (outfit_choice, value))
        drawn = [l for l in ui_dump("hh_kitchen_" + outfit, "5_Levels/174_HarborHomeKitchen/NPCs/Sink", area).splitlines()
                 if "NPC" in l and "(hidden)" not in l]
        if not drawn:
            find("error", area, "after changing to %s, Anis is no longer drawn at the sink" % outfit_choice)
        left = body(ask(sa, "busts", area))
        if left != "(none)":
            find("error", area, "after the outfit change ends, a bust is left on screen: %s" % left[:100])
        shot("hh_outfit_" + outfit)
    # The gift, from Harbor Home: window, reaction, HUD back.
    ask(mf, "press 9_MainCanvas/HHTalkPanel/HHTalkButton1", area)
    time.sleep(2.5)
    run_dialogue_to_end(area, prefer=["I have something for you."])
    time.sleep(1.5)
    if "active True" in ask(mf, "inspect PackUiCanvas_giftui/Pack_giftui/GiftWindow", area):
        shot("hh_gift_window")
        ask(mf, "press PackUiCanvas_giftui/Pack_giftui/GiftWindow/Panel/Gifts/Beer", area)
        time.sleep(3)
        if not playing():
            find("error", area, "after the gift, Anis's reaction does not start (Gifting_Gifted %s)" % pvar("Gifting_Gifted"))
        run_dialogue_to_end(area)
        time.sleep(3)
        hud = ask(sa, "at 140 680", area)
        if "OtherHud/Cash" not in hud:
            find("error", area, "after the gift and its reaction the HUD is not back: " + hud[:200])
        shot("hh_after_gift")
    else:
        find("error", area, "the gift window did not open from Harbor Home")
    errors(area, "Harbor Home")
    stop()
    scan_logs(area)


# --------------------------------------------------------------------------------------------- fixes
def hud_back(area):
    return "OtherHud/Cash" in ask(sa, "at 140 680", area)


def open_gift_window(area, room_token):
    travel(room_token, area)
    if not playing():
        ask(mf, "dialogue %s %s" % (PACK_ID, "AnisDialogueDefault" if "MountainLab" in room_token else "AnisDialogueHHDefault"), area)
        time.sleep(1.5)
    run_dialogue_to_end(area, prefer=["I have something for you."])
    time.sleep(1.5)
    return "active True" in ask(mf, "inspect PackUiCanvas_giftui/Pack_giftui/GiftWindow", area)


def area_fixes():
    """The gifting, gift shop and movie-night fixes of 2026-11-01, each read back."""
    area = "fixes"
    launch()
    for name, value in (("HarborHome_Bought", "true"), ("Voyeur_SeenAnis", "true"), ("MountainLab_FirstVisited", "true"),
                        ("Location_Anis", "MountainLabRoomNikkeAnis"), ("Affection_Anis", "1")):
        pset(name, value)
    time.sleep(2)
    # The window: only what the player has (the game's own items in this save, none of the shop's yet).
    if not open_gift_window(area, "pack:%s.MountainLabRoomNikkeAnis" % PACK_ID):
        find("error", area, "gift window did not open in the lab")
    else:
        time.sleep(1)
        shot("fix_gift_window")
        dump = ui_dump("fix_gift_window", "PackUiCanvas_giftui", area)
        shown = [l.strip().split(" [")[0] for l in dump.splitlines() if "Rounded #0A1A33FF" in l and "(hidden)" not in l]
        hidden = [l.strip().split(" (hidden)")[0] for l in dump.splitlines() if l.startswith("        ") and l.endswith("(hidden)") and "Gift" not in l]
        log(area, "gift window cards", {"shown": shown, "hidden": hidden})
        for flag in ("OVERFLOWS", "WIDER-THAN-BOX", "OFFSCREEN"):
            if flag in dump:
                find("error", area, "gift window: %s in %s" % (flag, [l.strip()[:80] for l in dump.splitlines() if flag in l][:4]))
        if any(s.startswith(("Bikini", "Ring", "Parasol")) for s in shown):
            find("error", area, "gift window shows a shop gift the player does not have: %s" % shown)
        # Give the beer: used up, reaction, affection, lock, HUD; in the lab her bust stays (it is her).
        beer, affection = gget("Beer"), pvar("Affection_Anis")
        ask(mf, "press PackUiCanvas_giftui/Pack_giftui/GiftWindow/Panel/Gifts/Beer", area)
        time.sleep(3)
        reaction = playing()
        run_dialogue_to_end(area)
        time.sleep(3)
        got = {"Beer": gget("Beer"), "Affection_Anis": pvar("Affection_Anis"), "DailyProc_Gifting_Anis": pvar("DailyProc_Gifting_Anis"),
               "Gifting_Gifted": pvar("Gifting_Gifted"), "reaction": reaction, "hud": hud_back(area), "busts": body(ask(sa, "busts", area))}
        log(area, "lab gift", got)
        if not reaction:
            find("error", area, "lab gift: the reaction did not start")
        if got["Beer"] != "False":
            find("error", area, "lab gift: the beer was not used up (%s)" % got["Beer"])
        if got["Affection_Anis"] != str(int(affection) + 1):
            find("error", area, "lab gift: Affection_Anis %s -> %s, expected +1" % (affection, got["Affection_Anis"]))
        if got["DailyProc_Gifting_Anis"] != "true":
            find("error", area, "lab gift: the daily lock was not set")
        if not got["hud"]:
            find("error", area, "lab gift: the HUD did not come back")
        if "AnisBase" not in got["busts"]:
            find("error", area, "lab gift: Anis's bust (her presence in the lab) left: %s" % got["busts"])
        shot("fix_after_lab_gift")
    # Closing the window without giving anything brings the HUD back.
    pset("DailyProc_Gifting_Anis", "false")
    if open_gift_window(area, "pack:%s.MountainLabRoomNikkeAnis" % PACK_ID):
        ask(mf, "press PackUiCanvas_giftui/Pack_giftui/GiftWindow/Panel/Close", area)
        time.sleep(3)
        if not hud_back(area):
            find("error", area, "closing the gift window without a gift leaves the HUD hidden")
        if pvar("Gifting_Target") != "":
            find("error", area, "closing the gift window leaves Gifting_Target set")
    # At Harbor Home the after-gift dialogue leaves her bust (her room sprite is her there).
    pset("DailyProc_Gifting_Anis", "false")
    pset("HarborHome_VisitList", "[]")
    pset("HarborHome_Visit_Anis", "true")
    pset("Location_Anis", "HarborHomeKitchenSink")
    travel("pack:%s.HarborHomeKitchen" % PACK_ID, area)
    ask(mf, "press 9_MainCanvas/HHTalkPanel/HHTalkButton1", area)
    time.sleep(2.5)
    run_dialogue_to_end(area, prefer=["I have something for you."])
    time.sleep(1.5)
    if "active True" in ask(mf, "inspect PackUiCanvas_giftui/Pack_giftui/GiftWindow", area):
        ask(mf, "press PackUiCanvas_giftui/Pack_giftui/GiftWindow/Panel/Gifts/Wine", area)
        time.sleep(3)
        run_dialogue_to_end(area)
        time.sleep(3)
        left = body(ask(sa, "busts", area))
        if left != "(none)":
            find("error", area, "Harbor Home gift: a bust is left after the reaction: %s" % left[:100])
        if not hud_back(area):
            find("error", area, "Harbor Home gift: the HUD did not come back")
        shot("fix_after_hh_gift")
    else:
        find("error", area, "gift window did not open at Harbor Home")
    errors(area, "gifting")
    # The shop: visible over the shop, buying, can't afford, closing.
    pset("GiftShop_BuildCounter", "2")
    pset("Location_Claire", "GiftShopInterior")
    travel("pack:%s.GiftShopInterior" % PACK_ID, area)
    if not playing():
        ask(mf, "dialogue %s ClaireDialogueDefault" % PACK_ID, area)
        time.sleep(1.5)
    run_dialogue_to_end(area, prefer=["Show me what you got."])
    time.sleep(2)
    shot("fix_shop")
    shop_dump = ui_dump("fix_shop", "PackUiCanvas_giftshop", area)
    if not shop_dump or shop_dump.startswith("PackUiCanvas_giftshop (hidden)"):
        find("error", area, "the gift shop is not on screen after 'Show me what you got.'")
    else:
        cash = int(float(gget("Cash")))
        ask(mf, "press PackUiCanvas_giftshop/Pack_giftshop/GiftShop/GiftStore/Core/Sunscreen/Button", area)
        time.sleep(1)
        if pvar("Gift_Sunscreen") != "true" or int(float(gget("Cash"))) != cash - 600:
            find("error", area, "buying sunscreen: Gift_Sunscreen %s, cash %s -> %s" % (pvar("Gift_Sunscreen"), cash, gget("Cash")))
        gset("Cash", "100")
        time.sleep(1)
        shot("fix_shop_poor")
        ask(mf, "press PackUiCanvas_giftshop/Pack_giftshop/GiftShop/GiftStore/Core/Ring/Button", area)
        time.sleep(1)
        if pvar("Gift_Ring") != "false" or gget("Cash") not in ("100", "100.0"):
            find("error", area, "buying a ring with $100 went through: Gift_Ring %s, cash %s" % (pvar("Gift_Ring"), gget("Cash")))
        poor = ui_dump("fix_shop_poor", "PackUiCanvas_giftshop", area)
        if "Can't afford" not in poor:
            find("error", area, "with $100 the shop does not say an item cannot be afforded")
        gset("Cash", str(cash))
        ask(mf, "press PackUiCanvas_giftshop/Pack_giftshop/GiftShop/CloseStore", area)
        time.sleep(3)
        if not hud_back(area):
            find("error", area, "closing the shop leaves the HUD hidden")
    errors(area, "gift shop")
    # Movie night: each show plays from Anis's choice and moves the game's own episode counter on as the game's
    # showing does (round to the first after the last); nothing else of the game's movie state changes.
    COUNTERS = {"Scifi": ("scifi-movie", 3), "Fantasy": ("fantasy-movie", 3), "News": ("news-movie", 9)}
    gset("news-movie", "8")   # the news at its last episode: the showing must take it back to the first
    time.sleep(1)
    before = {v: gget(v) for v in ("scifi-movie", "fantasy-movie", "news-movie", "incoming-movie")}
    pset("HarborHome_Outfit_Anis", "AnisBase")
    pset("Location_Anis", "HarborHomeLivingRoomCouchleft")
    for show, words in (("Scifi", "Let's watch something with Sci-fi."), ("Fantasy", "How about some fantasy stuff?"), ("News", "I wanna see the news.")):
        pset("DailyProc_Anis_TV", "false")
        travel("pack:%s.HarborHomeKitchen" % PACK_ID, area, 1.5)
        travel("pack:%s.HarborHomeLivingRoom" % PACK_ID, area)
        counter, count = COUNTERS[show]
        episode = int(float(gget(counter)))
        ask(mf, "press 9_MainCanvas/HHTalkPanel/HHTalkButton1", area)
        time.sleep(2.5)
        said, _ = run_dialogue_to_end(area, prefer=["Wanna watch something?", words], only="AnisDialogueHHDefaultMovie")
        time.sleep(1.0)
        line = ask(mf, "line", area)
        if "HHMovie" + show not in line:
            find("error", area, "%s: the show did not start after picking it (line: %s)" % (show, line[8:120]))
            continue
        time.sleep(5)
        shot("fix_movie_" + show)
        tv = ask(mf, "inspect X_Movies", area)
        if "active True" not in tv:
            find("error", area, "%s: the TV (X_Movies) is not on while the show plays: %s" % (show, tv[:120]))
        movie, _ = run_dialogue_to_end(area, only="HHMovie" + show)
        follow = ""
        for _ in range(10):
            follow = ask(mf, "line", area)
            if "(none)" not in follow:
                break
            time.sleep(0.5)
        log(area, "after " + show, {"follow": follow, "counter": gget(counter)})
        if "AnisRandomMovie01" not in follow:
            find("error", area, "%s: AnisRandomMovie01 did not follow the show (line: %s)" % (show, follow[8:120]))
        run_dialogue_to_end(area)
        time.sleep(2)
        want = (episode + 1) % count
        got = int(float(gget(counter)))
        if got != want:
            find("error", area, "%s: the game's %s went %d -> %d, expected %d as the game's own showing does" % (show, counter, episode, got, want))
        if "active True" in ask(mf, "inspect X_Movies", area):
            find("error", area, "%s: the TV stays on after the show" % show)
        if not hud_back(area):
            find("error", area, "%s: the HUD is not back after the show" % show)
    after = {v: gget(v) for v in before}
    if after["incoming-movie"] != before["incoming-movie"]:
        find("error", area, "the game's incoming-movie changed: %s -> %s" % (before["incoming-movie"], after["incoming-movie"]))
    if gget("claudia-newsseen") != "True":
        find("error", area, "after the news, the game's claudia-newsseen is %s" % gget("claudia-newsseen"))
    log(area, "game movie variables", {"before": before, "after": after})
    errors(area, "movie night")
    stop()
    scan_logs(area)


# --------------------------------------------------------------------------------------------- day
def area_day():
    """Sleeping into the next day: the pack's daily resets and the rules that run on a new day."""
    area = "day"
    launch()
    met = ["Anis", "Centi", "Dorothy", "Mary", "Rapi", "Yan"]
    for who in met:
        pset("Voyeur_Seen" + who, "true")
    pset("MountainLab_FirstVisited", "true")
    pset("GiftShop_BuildCounter", "2")
    pset("DailyProc_Gifting_Anis", "true")
    pset("SecretBeach_ActionToday", "true")
    time.sleep(3)
    before = {"Day": gget("Day"), **{"Location_" + w: pvar("Location_" + w) for w in met + ["Amber", "Claire"]}}
    log(area, "before sleeping", before)
    travel("vanilla:5_MyRoom", area)
    pressed = ask(mf, "press-text Sleep", area)
    if not pressed.startswith("ok"):
        find("error", area, "could not press Sleep in the bedroom: " + pressed)
        stop()
        return
    deadline = time.time() + 90
    day = before["Day"]
    while time.time() < deadline and day == before["Day"]:
        time.sleep(2)
        if playing():
            see_through(area)
        day = gget("Day")
    shot("next_morning")
    if day == before["Day"]:
        find("error", area, "after pressing Sleep the day stayed %s for 90 s" % day)
        stop()
        return
    # The morning: the game's after-sleep screens, then the sleep autosave - which is when ModForge runs its
    # daily refresh - and the HUD back. Read nothing before that.
    # The HUD shows behind the payout sheet, so "settled" is: nothing left to confirm three checks running.
    deadline, quiet = time.time() + 150, 0
    while time.time() < deadline and quiet < 3:
        if playing():
            see_through(area)
            quiet = 0
        # The payout sheet, then "What do I do until afternoon?" - the game saves (and ModForge turns the
        # day over) only once an activity is picked.
        pressed = [w for w in ("Confirm", "Continue", "OK", "Relax") if ask(mf, "press-text " + w, area).startswith("ok")]
        quiet = 0 if pressed else quiet + 1
        time.sleep(2)
    time.sleep(3)
    shot("morning_settled")
    after = {"Day": day, **{"Location_" + w: pvar("Location_" + w) for w in met + ["Amber", "Claire"]},
             "DailyProc_Gifting_Anis": pvar("DailyProc_Gifting_Anis"), "SecretBeach_ActionToday": pvar("SecretBeach_ActionToday"),
             "Voyeur_TargetsLeftCount": pvar("Voyeur_TargetsLeftCount"), "GiftShop_BuildCounter": pvar("GiftShop_BuildCounter")}
    log(area, "after sleeping", after)
    if after["DailyProc_Gifting_Anis"] != "false":
        find("error", area, "DailyProc_Gifting_Anis (refreshed daily) is still %s the next morning" % after["DailyProc_Gifting_Anis"])
    if after["SecretBeach_ActionToday"] != "false":
        find("error", area, "SecretBeach_ActionToday (refreshed daily) is still %s the next morning" % after["SecretBeach_ActionToday"])
    for who in met:
        if after["Location_" + who] in ("Unknown", ""):
            find("error", area, "%s is nowhere the morning after (Location_%s = %s); her schedule rules did not place her" %
                 (who, who, after["Location_" + who]))
    if day != "4" and after["Location_Claire"] != "GiftShopInterior":
        find("error", area, "Claire not at the gift shop on day %s (Location_Claire = %s)" % (day, after["Location_Claire"]))
    errors(area, "sleeping into the next day")
    stop()
    scan_logs(area)


# --------------------------------------------------------------------------------------------- ui
def area_ui():
    area = "ui"
    launch()
    ask(sa, "canvases", area)
    ui_dump("main_canvas", "9_MainCanvas", area)
    for x, y in ((640, 715), (1170, 70), (60, 680)):
        ask(sa, "at %d %d" % (x, y), area)
    # The massage minigame is out of use for now (the author, 2026-11-01): not checked.
    stop()
    scan_logs(area)


AREAS = {"startup": area_startup, "places": area_places, "behaviour": area_behaviour, "hh": area_hh, "fixes": area_fixes, "day": area_day,
         "dialogues": area_dialogues, "ui": area_ui}

if __name__ == "__main__":
    wanted = [a for a in sys.argv[1:] if a in AREAS] or list(AREAS)
    only = [a for a in sys.argv[1:] if a not in AREAS]
    print("run folder:", RUN, flush=True)
    try:
        for name in wanted:
            print("== " + name, flush=True)
            started = time.time()
            try:
                AREAS[name](only) if name == "dialogues" and only else AREAS[name]()
            except SystemExit as e:
                find("error", name, "area stopped: %s" % e)
            except Exception as e:  # one area failing must not lose the others
                find("error", name, "the check itself failed: %r" % e)
            print("   %s took %d s" % (name, time.time() - started), flush=True)
    finally:
        stop()
        with open(os.path.join(RUN, "report.md"), "w", encoding="utf-8") as f:
            f.write("# SMSAndroids in-game check %s\n\n" % os.path.basename(RUN))
            for sev in ("error", "warn", "log", "info"):
                rows = [x for x in findings if x[0] == sev]
                if rows:
                    f.write("## %s (%d)\n\n" % (sev, len(rows)))
                    for _, area, text in rows:
                        f.write("- **%s** %s\n" % (area, text))
                    f.write("\n")
        print("report:", os.path.join(RUN, "report.md"), flush=True)
