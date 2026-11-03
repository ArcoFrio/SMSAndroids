"""The world map's character art: each character's portrait (map_portraits.py) as a cut-corner tile, and the pieces
of the plate the tiles sit on.

  python -I Tools/Game/map_badges.py <pack folder> <out folder> [accent #RRGGBB] [preview.png]

Everything is drawn at twice the size it shows on a 1080p screen. Pack pictures load without mipmaps or nine-slice
borders, so a picture shown much smaller than it is drawn breaks up, and a shape stretched to fit goes soft;
straight edges and solid fills stretch cleanly, so the plate's middle and lines are stretched and only its corners
are pictures. One object per face keeps the pack's manifest small.
"""
import os
import sys

from PIL import Image, ImageDraw

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import map_portraits  # noqa: E402

X = 2                         # drawn at twice the 1080p size
TILE = 30                     # a face's size on a 1080p screen
CAP = 12                      # the plate's corner pieces' width on a 1080p screen
NAVY = (10, 15, 34, 230)      # SMSAndroids navy, as on the gift window and talk panel
LIGHT = (226, 230, 240, 255)
EDGE = (8, 10, 18, 240)
ACCENT = "#4DA3FF"             # electric blue (the author, 2026-11-02)


def rgb(hex_):
    hex_ = hex_.lstrip("#")
    return tuple(int(hex_[i:i + 2], 16) for i in (0, 2, 4)) + (255,)


def _cut(size, inset, cut):
    """A square with its top-left and bottom-right corners cut at 45 degrees."""
    m = Image.new("L", (size, size), 0)
    a, b, c = inset, size - 1 - inset, cut
    ImageDraw.Draw(m).polygon([(a + c, a), (b, a), (b, b - c), (b - c, b), (a, b), (a, a + c)], fill=255)
    return m


def tile(face, accent):
    """The portrait in a thin light frame, two corners cut, the top-left cut marked in the accent."""
    size = TILE * X
    big = size * 8
    cut = int(big * 0.20)
    out = Image.new("RGBA", (big, big), (0, 0, 0, 0))
    out.paste(Image.new("RGBA", (big, big), EDGE), (0, 0), _cut(big, 0, cut))
    out.paste(Image.new("RGBA", (big, big), LIGHT), (0, 0), _cut(big, int(big * 0.035), cut))
    inner = _cut(big, int(big * 0.075), cut)
    pic = face.resize((big, big), Image.NEAREST)          # pixel art: keep its pixels; the box reduction averages them
    layer = Image.new("RGBA", (big, big), (24, 22, 36, 255))
    layer.alpha_composite(pic)
    out.paste(layer, (0, 0), inner)
    mark = Image.new("L", (big, big), 0)
    ImageDraw.Draw(mark).polygon([(int(big * 0.035), int(big * 0.24)), (int(big * 0.24), int(big * 0.035)),
                                  (int(big * 0.33), int(big * 0.035)), (int(big * 0.035), int(big * 0.33))], fill=255)
    out.paste(Image.new("RGBA", (big, big), rgb(accent)), (0, 0), mark)
    return out.resize((size, size), Image.BOX)


LINE = 2                      # an accent line's thickness on a 1080p screen: whole pixels, so it lands on them
CUT = 9                       # a cut corner's size on a 1080p screen
ROOT2 = 2 ** 0.5


def inset(points, t):
    """A convex outline moved t inwards, edge by edge, corners mitred - so a band between the two is an exact,
    even line inside the shape rather than a stroke half outside it."""
    n = len(points)
    cx, cy = sum(p[0] for p in points) / n, sum(p[1] for p in points) / n
    lines = []
    for i in range(n):
        (x1, y1), (x2, y2) = points[i], points[(i + 1) % n]
        dx, dy = x2 - x1, y2 - y1
        length = (dx * dx + dy * dy) ** 0.5
        nx, ny = -dy / length, dx / length
        if (cx - x1) * nx + (cy - y1) * ny < 0:
            nx, ny = -nx, -ny
        lines.append(((x1 + nx * t, y1 + ny * t), (dx, dy)))
    out = []
    for i in range(n):
        (px, py), (ax, ay) = lines[i - 1]
        (qx, qy), (bx, by) = lines[i]
        den = ax * by - ay * bx
        u = ((qx - px) * by - (qy - py) * bx) / den
        out.append((px + ax * u, py + ay * u))
    return out


def cut_accent(c, t, top_to=None):
    """The accent along a top-left cut (from (0, c) to (c, 0)), mitred at both ends; with top_to, carried on along
    the top edge to x = top_to."""
    inner_left = (t, c + t * ROOT2 - t)          # the cut's inner line meets the left edge's
    inner_top = (c + t * ROOT2 - t, t)           # ... and the top edge's
    if top_to is None:
        return [(0, c), (c, 0), inner_top, inner_left]
    return [(0, c), (c, 0), (top_to, 0), (top_to, t), inner_top, inner_left]


def plate_corner(bottom_right, accent):
    """A plate's cut corner: navy, the cut lined in the accent - and, top-left, the top edge too, continuing the
    plate's stretched top line at exactly its thickness. Only the corner is a picture, so the plate can be any
    height (one row or more) without stretching a shape."""
    w, k = CAP * X, 8
    W, c, t = w * k, CUT * X * k, LINE * X * k
    img = Image.new("RGBA", (W, W), (0, 0, 0, 0))
    d = ImageDraw.Draw(img)
    d.polygon([(c, 0), (W, 0), (W, W), (0, W), (0, c)], fill=NAVY)
    d.polygon(cut_accent(c, t, None if bottom_right else W), fill=rgb(accent))
    img = img.resize((w, w), Image.BOX)
    return img.transpose(Image.ROTATE_180) if bottom_right else img


def leader(accent):
    """From the button's edge to the plate: a thin line with a dot where it leaves the button."""
    w, h, k = 16 * X, 6 * X, 8
    img = Image.new("RGBA", (w * k, h * k), (0, 0, 0, 0))
    d = ImageDraw.Draw(img)
    r = h * k // 2
    d.ellipse((0, 0, 2 * r - 1, 2 * r - 1), fill=rgb(accent))
    d.rectangle((r, r - LINE * X * k // 2, w * k, r + LINE * X * k // 2), fill=rgb(accent))
    return img.resize((w, h), Image.BOX)


def notch(accent):
    """Under a district's button: a small wedge pointing up at it."""
    w, h, k = 12 * X, 6 * X, 8
    img = Image.new("RGBA", (w * k, h * k), (0, 0, 0, 0))
    ImageDraw.Draw(img).polygon([(0, h * k), (w * k / 2, 0), (w * k, h * k)], fill=rgb(accent))
    return img.resize((w, h), Image.BOX)


CARD = (128, 196)            # the Harbor Home talk card on a 1080p screen
CARD_CUT = 12
NAME_TAB = 26                # the accent tab across its foot, where her name is


def talk_card(accent):
    """The Harbor Home talk card's frame, the same family as the map's plate: navy, two corners cut, the top edge and
    the top-left cut in the accent, and an accent tab across the foot for her name (its end follows the cut). The
    card never changes size, so the whole frame is one picture at exactly twice its size."""
    k = 4
    W, H = CARD[0] * X * k, CARD[1] * X * k
    c, tab, t = CARD_CUT * X * k, NAME_TAB * X * k, LINE * X * k
    shape = [(c, 0), (W, 0), (W, H - c), (W - c, H), (0, H), (0, c)]
    img = Image.new("RGBA", (W, H), (0, 0, 0, 0))
    d = ImageDraw.Draw(img)
    # A faint light outline all round, as a band just inside the edge; inside it navy; the tab in the accent.
    inner = inset(shape, X * k)
    d.polygon(shape, fill=(220, 232, 255, 46))
    d.polygon(inner, fill=NAVY)
    tab_layer = Image.new("RGBA", (W, H), (0, 0, 0, 0))
    ImageDraw.Draw(tab_layer).rectangle((0, H - tab, W, H), fill=rgb(accent))
    inner_mask = Image.new("L", (W, H), 0)
    ImageDraw.Draw(inner_mask).polygon(inner, fill=255)
    img.paste(tab_layer, (0, 0), Image.composite(tab_layer.getchannel("A"), Image.new("L", (W, H), 0), inner_mask))
    d = ImageDraw.Draw(img)
    d.polygon(cut_accent(c, t, W), fill=rgb(accent))
    return img.resize((CARD[0] * X, CARD[1] * X), Image.BOX)


def portrait_templates(window, pop, pop_side, accent=ACCENT):
    """Guides for drawing a talk-card portrait, each exactly the portrait's size (twice the window): the card as it
    sits behind the portrait (put it under the drawing to see the result), and its outline alone (lay it over).
    The window starts pop_side left of the card and pop above it; its foot is the top of the name tab."""
    w, h = window[0] * X, window[1] * X
    left, top = pop_side * X, pop * X
    card = talk_card(accent)                       # the whole card, tab included; the window stops above the tab
    under = Image.new("RGBA", (w, h), (0, 0, 0, 0))
    under.alpha_composite(card.crop((0, 0, card.width, h - top)), (left, top))
    k = 4
    W, H = CARD[0] * X * k, CARD[1] * X * k
    c, t = CARD_CUT * X * k, LINE * X * k
    shape = [(c, 0), (W, 0), (W, H - c), (W - c, H), (0, H), (0, c)]
    lines = Image.new("RGBA", (W, H), (0, 0, 0, 0))
    d = ImageDraw.Draw(lines)
    d.polygon(shape, fill=(255, 255, 255, 230))
    d.polygon(inset(shape, X * k), fill=(0, 0, 0, 0))
    d.polygon(cut_accent(c, t, W), fill=rgb(accent))
    tab = NAME_TAB * X * k
    d.rectangle((0, H - tab - X * k, W, H - tab), fill=rgb(accent))       # where the name tab starts
    lines = lines.resize((CARD[0] * X, CARD[1] * X), Image.BOX)
    over = Image.new("RGBA", (w, h), (0, 0, 0, 0))
    over.alpha_composite(lines.crop((0, 0, lines.width, h - top)), (left, top))
    return under, over


def make_talk(out_dir, accent=ACCENT):
    """The talk card's pictures into out_dir; returns what changed."""
    os.makedirs(out_dir, exist_ok=True)
    changed = []
    card = talk_card(accent)
    # The right-hand card of two wears the frame mirrored. The card itself is an invisible picture that takes the
    # clicks; the frames are drawn on it.
    for name, img in (("TalkCard.png", card), ("TalkCardMirrored.png", card.transpose(Image.FLIP_LEFT_RIGHT)),
                      ("Shade.png", Image.new("RGBA", (4, 4), (0, 0, 0, 150))), ("Hit.png", Image.new("RGBA", (4, 4), (0, 0, 0, 0)))):
        to = os.path.join(out_dir, name)
        if not os.path.exists(to) or Image.open(to).convert("RGBA").tobytes() != img.tobytes():
            img.save(to, optimize=True)
            changed.append(name)
    return changed


def make(pack, out_dir, names, accent=ACCENT):
    """Draw every piece into out_dir and remove anything else there; returns what changed."""
    os.makedirs(out_dir, exist_ok=True)
    faces = map_portraits.all_portraits(pack)
    wanted = {"Tile%s.png" % n: tile(faces[n], accent) for n in names}
    wanted.update({"PlateCornerTL.png": plate_corner(False, accent), "PlateCornerBR.png": plate_corner(True, accent),
                   "PlateFill.png": Image.new("RGBA", (4, 4), NAVY), "PlateEdge.png": Image.new("RGBA", (4, 4), rgb(accent)),
                   "Leader.png": leader(accent), "Notch.png": notch(accent)})
    changed = []
    for name, img in wanted.items():
        to = os.path.join(out_dir, name)
        if not os.path.exists(to) or Image.open(to).convert("RGBA").tobytes() != img.tobytes():
            img.save(to, optimize=True)
            changed.append(name)
    for old in sorted(set(os.listdir(out_dir)) - set(wanted)):
        os.remove(os.path.join(out_dir, old))
        changed.append("removed " + old)
    return changed


if __name__ == "__main__":
    pack, out_dir = sys.argv[1], sys.argv[2]
    accent = sys.argv[3] if len(sys.argv) > 3 else ACCENT
    names = sorted(map_portraits.all_portraits(pack))
    print(make(pack, out_dir, names, accent))
    if len(sys.argv) > 4:
        sheet = Image.new("RGBA", (9 * 70 + 10, 3 * 70 + 10), (60, 30, 50, 255))
        for i, n in enumerate(names):
            sheet.alpha_composite(Image.open(os.path.join(out_dir, "Tile%s.png" % n)), (10 + (i % 9) * 70, 10 + (i // 9) * 70))
        sheet.save(sys.argv[4])
