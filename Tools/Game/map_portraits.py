"""Square head-and-shoulders portraits for the world map, cut from each character's own bust in the pack.

  python -I Tools/Game/map_portraits.py <pack folder> <out folder> [sheet.png]

Every character with a bust in the pack gets one, so a new character has hers as soon as she has a bust. The head
is found from the picture: the top of the figure, then the middle of what is drawn just below it. Where that
lands badly on a particular bust (a hat, a raised arm, hair to one side), FRAMING moves or resizes that one crop.
"""
import json
import os
import sys

from PIL import Image

SIDE = 80            # the crop, in the bust's own pixels (busts are 256 x 256 full figures)
FRAMING = {          # name: (dx, dy, side) in bust pixels - corrections after looking at the sheet
    "Amber": (14, 0, SIDE),     # ponytail to one side pulls the hair's middle left of her face
    "Anis": (-6, 0, SIDE),      # cap tilted right
    "Helm": (-1, 4, SIDE),     # tall cap: start lower so her chin is in
    "Mast": (-4, 2, SIDE),      # tall cap and side tail
    "Rapi": (-5, 0, SIDE),      # beret to the right
}


def bust_path(pack, character):
    outfits = character.get("outfits") or []
    outfit = next((o for o in outfits if o["gameObjectName"] == character.get("defaultOutfit")), outfits[0] if outfits else None)
    if not outfit:
        return None
    path = os.path.join(pack, outfit["baseSprite"])
    folder = os.path.dirname(path)
    if not os.path.exists(path) and os.path.isdir(folder):
        # Manifest paths and file names do not always agree on case.
        match = [f for f in os.listdir(folder) if f.lower() == os.path.basename(path).lower()]
        path = os.path.join(folder, match[0]) if match else path
    return path if os.path.exists(path) else None


def head(img, name=""):
    """Where her head is on a bust: its middle across (the FRAMING correction included) and the top of her hair."""
    alpha = img.getchannel("A").point(lambda v: 255 if v > 96 else 0)
    w, h = img.size
    top = next(y for y in range(h) if sum(1 for x in range(w) if alpha.getpixel((x, y))) > 2)
    xs = [x for y in range(top + 6, min(h, top + 30)) for x in range(w) if alpha.getpixel((x, y))]
    return sorted(xs)[len(xs) // 2] + FRAMING.get(name, (0, 0, SIDE))[0], top


def crop_box(img, name):
    cx, top = head(img, name)
    _, dy, side = FRAMING.get(name, (0, 0, SIDE))
    left = int(cx - side / 2)
    # Down from the top of the hair, so the chin clears the tile's frame and its cut bottom corner.
    upper = int(top + 8 + dy)
    return left, upper, left + side, upper + side


def portrait(path, name):
    img = Image.open(path).convert("RGBA")
    return img.crop(crop_box(img, name))


def all_portraits(pack):
    manifest = json.load(open(os.path.join(pack, "modpack.json"), encoding="utf-8-sig"))
    made = {}
    for character in manifest["characters"]:
        if character.get("bustSource") != "Pack":
            continue
        path = bust_path(pack, character)
        if path:
            made[character["name"]] = portrait(path, character["name"])
    return made


if __name__ == "__main__":
    pack, out = sys.argv[1], sys.argv[2]
    os.makedirs(out, exist_ok=True)
    found = all_portraits(pack)
    for name, img in found.items():
        img.save(os.path.join(out, name + ".png"))
    if len(sys.argv) > 3:
        cell = SIDE * 3 + 10
        sheet = Image.new("RGBA", (9 * cell, ((len(found) + 8) // 9) * cell), (60, 40, 60, 255))
        for i, (name, img) in enumerate(found.items()):
            sheet.alpha_composite(img.resize((SIDE * 3, SIDE * 3), Image.NEAREST), ((i % 9) * cell + 5, (i // 9) * cell + 5))
        sheet.save(sys.argv[3])
    print(len(found), sorted(found))
