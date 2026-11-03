"""Screenshots of the world map's character icons as a player sees them, at 1080p, with a crowd.

  python -I Tools/Game/look_map.py <name>     # shots to %TEMP%\\SMSAndroids-ingame\\look-<name>\\

Export the pack first (export-pack.ps1). Starts the test copy through launch.ps1 (its own desktop, muted, the test
save only) and closes it.
"""
import os
import shutil
import subprocess
import sys
import time

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from bridge import Bridge, TEST  # noqa: E402

HERE = os.path.dirname(os.path.abspath(__file__))
PACK = "SMSAndroidsPack"
MAP = "World_Map/Canvas/Core"
OUT = os.path.join(os.environ["TEMP"], "SMSAndroids-ingame", "look-" + sys.argv[1])
SHOTS = os.path.join(TEST, r"BepInEx\plugins\SMSModForge\DebugBridge\a0\shots")
CROWD = {"Elegg": "MountainLabRoomNikkeElegg", "Frima": "MountainLabRoomNikkeFrima", "Guilty": "MountainLabRoomNikkeGuilty",
         "Helm": "MountainLabRoomNikkeHelm", "Maiden": "MountainLabRoomNikkeMaiden", "Mary": "MountainLabRoomNikkeMary",
         "Mast": "MountainLabRoomNikkeMast", "Neon": "MountainLabRoomNikkeNeon", "Pepper": "MountainLabRoomNikkePepper",
         "Anis": "Mall", "Centi": "Mall", "Dorothy": "Mall", "Rapi": "Mall", "Rosanna": "Mall", "Sakura": "Mall",
         "Tove": "Gasstation", "Viper": "Gasstation", "Yan": "Downtown", "Naga": "Downtown", "Amber": "Beach", "Marciana": "Beach",
         "Claire": "GiftShop"}

mf = Bridge("mf")


def ps(script, *args):
    return subprocess.run(["powershell", "-NoProfile", "-ExecutionPolicy", "Bypass", "-File", os.path.join(HERE, script)] + list(args),
                          capture_output=True, text=True)


def shot(name):
    mf.ask("shot " + name)
    src = os.path.join(SHOTS, name + ".png")
    for _ in range(50):
        if os.path.exists(src):
            break
        time.sleep(0.1)
    time.sleep(0.3)
    shutil.move(src, os.path.join(OUT, name + ".png"))


os.makedirs(OUT, exist_ok=True)
ps("stop.ps1")
out = ps("launch.ps1", "-Width", "1920", "-Height", "1080")
if out.returncode != 0:
    raise SystemExit(out.stdout + out.stderr)
try:
    print(mf.ask("wait-ingame 180", timeout=240))
    time.sleep(8)
    for who, where in CROWD.items():
        mf.ask("pset %s Location_%s %s" % (PACK, who, where))
    mf.ask("travel pack:%s.HarborHouseEntrance" % PACK)
    time.sleep(4)
    mf.ask("set-active World_Map true")
    time.sleep(2.5)
    shot("districts")
    for district, group in (("Shoppingdistrict", "Shopside"), ("Seaside", "Seaside")):
        mf.ask("press %s/District_Buttons/%s" % (MAP, district))
        if group == "Shopside":
            time.sleep(0.15)
            shot("radial_opening")     # mid-spin and mid-fade
        time.sleep(2.5)
        shot("radial_" + group)
        mf.ask("press %s/Radial_Buttons/%s/Return" % (MAP, group))
        time.sleep(2.5)
    # The Harbor Home talk card, Anis in the kitchen.
    mf.ask("set-active World_Map false")
    for name, value in (("HarborHome_Bought", "true"), ("HarborHome_FirstVisited", "true"), ("HarborHome_Visit_Anis", "true"),
                        ("HarborHome_VisitList", "[]"), ("Location_Anis", "HarborHomeKitchenSink")):
        mf.ask("pset %s %s %s" % (PACK, name, value))
    time.sleep(2)
    for _ in range(40):                  # anything that started meanwhile, clicked through
        if "True" not in mf.ask("playing"):
            break
        line = mf.ask("line")
        choices = [c.split("|")[0].strip() for c in line.split("choice: ")[1:]]
        mf.ask("press-text " + choices[-1] if choices else "next")
        time.sleep(0.5)
    print(mf.ask("travel pack:%s.HarborHomeKitchen" % PACK))
    time.sleep(4)
    shot("talk")
    # Two people in the room (a card for Rapi only exists in a pack built with TALK_EXTRA=Rapi).
    mf.ask("pset %s HarborHome_Visit_Rapi true" % PACK)
    mf.ask("pset %s Location_Rapi HarborHomeKitchenFridge" % PACK)
    time.sleep(1)
    shot("talk_two")
    # Hover on the left card (the game's own hover grow), then back.
    sa = Bridge("sa")
    sa.ask("hover 9_MainCanvas/Pack_hhtalk/HHTalk/Talk_Anis")
    time.sleep(0.6)
    shot("talk_hover")
    sa.ask("hover 9_MainCanvas/Pack_hhtalk/HHTalk/Talk_Anis off")
    time.sleep(0.6)
    # Arriving: Rapi leaves and comes back, caught part-way through her card's entry.
    mf.ask("pset %s Location_Rapi HarborHomePoolTanningleft" % PACK)
    time.sleep(1)
    mf.ask("pset %s Location_Rapi HarborHomeKitchenFridge" % PACK)
    time.sleep(0.12)
    shot("talk_arriving")
    time.sleep(1)
    mf.ask("set Lock-Game true")
    time.sleep(0.5)
    shot("talk_busy")
    mf.ask("set Lock-Game false")
finally:
    ps("stop.ps1")
print(OUT)
