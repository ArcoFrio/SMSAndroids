"""What SMSAndroidsPack's logic says, and where it does not add up - read from modpack.json alone.

  python -I Tools/Game/pack_logic.py [pack folder] [out folder]

Writes logic.txt (every rule, dialogue start and NPC/place condition in readable form, negations and nested
Any/All included) and consistency.txt: values a variable is compared against that nothing ever sets it to, values
set that nothing reads, variables used but not declared, scenes and busts referenced that the pack does not have.
These are the places where the pack cannot do what it means to - a dialogue waiting for a location nothing
assigns never plays.
"""
import collections
import json
import os
import re
import sys

HERE = os.path.dirname(os.path.abspath(__file__))
PACK = sys.argv[1] if len(sys.argv) > 1 else os.path.join(HERE, "..", "..", "SMSAndroidsPack")
OUT = sys.argv[2] if len(sys.argv) > 2 else os.path.join(os.environ.get("TEMP", "."), "SMSAndroids-ingame")
os.makedirs(OUT, exist_ok=True)
d = json.load(open(os.path.join(PACK, "modpack.json"), encoding="utf-8-sig"))


def cond_text(c):
    if c.get("type") in ("Any", "All"):
        inner = (" OR " if c["type"] == "Any" else " AND ").join(cond_text(x) for x in c.get("conditions", []))
        s = "(" + inner + ")"
    else:
        p = c.get("params", {})
        s = c.get("type", "?") + "(" + ", ".join("%s=%s" % kv for kv in p.items()) + ")"
    return ("NOT " if c.get("negate") else "") + s


def act_text(a):
    return a.get("type", "?") + "(" + ", ".join("%s=%s" % kv for kv in a.get("params", {}).items()) + ")"


def walk_conditions(conds, where, negated=False):
    for c in conds or []:
        n = negated ^ bool(c.get("negate"))
        if c.get("type") in ("Any", "All"):
            yield from walk_conditions(c.get("conditions"), where, n)
        else:
            yield c, n, where


# Everything that holds conditions or actions, with where it is.
cond_sites, act_sites = [], []
for r in d.get("integrationRules", []):
    w = "rule " + r["key"]
    cond_sites.append((w, r.get("conditions", [])))
    act_sites.append((w, r.get("actions", [])))
    for i, b in enumerate(r.get("branches", []) or []):
        cond_sites.append((w + " branch %d" % (i + 1), b.get("conditions", [])))
        act_sites.append((w + " branch %d" % (i + 1), b.get("actions", [])))
for dl in d.get("dialogues", []):
    w = "dialogue " + dl["key"]
    cond_sites.append((w + " start", dl.get("startConditions", [])))
    for n in dl.get("nodes", []):
        cond_sites.append((w + " node %s" % n["id"], n.get("conditions", [])))
        act_sites.append((w + " node %s" % n["id"], (n.get("actionsOnStart") or []) + (n.get("actionsOnFinish") or [])))


def scan_tree(obj, where):
    """Conditions and actions held anywhere else (places' game objects, UIs, quests, map buttons)."""
    if isinstance(obj, dict):
        for k, v in obj.items():
            if isinstance(v, list) and v and isinstance(v[0], dict) and "type" in v[0]:
                if "condition" in k.lower():
                    cond_sites.append((where + " ." + k, v))
                elif "action" in k.lower():
                    act_sites.append((where + " ." + k, v))
            scan_tree(v, where + ("/" + str(obj.get("name") or obj.get("key") or "")) if k == "children" else where)
    elif isinstance(obj, list):
        for x in obj:
            scan_tree(x, where)


for section in ("places", "uis", "quests", "mapButtons", "npcs", "vanillaExtensions", "customRoomTalks"):
    for item in [i for i in d.get(section, []) if isinstance(i, dict)]:
        scan_tree(item, section[:-1] + " " + str(item.get("key") or item.get("id") or item.get("target") or "?"))

def flat_actions(acts, where):
    """Actions, with those inside an action's own branches (a dice roll's outcomes)."""
    for a in acts or []:
        yield a, where
        for i, b in enumerate(a.get("branches") or []):
            inner = where + " " + a["type"] + " branch %d" % (i + 1)
            cond_sites.append((inner, b.get("conditions", [])))
            yield from flat_actions((b.get("actions") or []) + ([b["action"]] if b.get("action") else []), inner)


act_sites = [(w, list(flat_actions(acts, w))) for w, acts in act_sites]
declared = {v["name"]: v for v in d.get("variables", [])}


def list_default(name):
    raw = declared.get(name, {}).get("defaultValue", "") or ""
    try:
        value = json.loads(raw)
        if isinstance(value, list):
            return [str(x) for x in value]
    except ValueError:
        pass
    return [x.strip() for x in raw.split(",") if x.strip()]


# What each list can hold: its default and whatever an action adds.
list_items = collections.defaultdict(set)
for name in declared:
    list_items[name].update(list_default(name))
for where, acts in act_sites:
    for a, _ in acts:
        p = a.get("params", {})
        if a["type"] == "AddToList" and p.get("list"):
            list_items[p["list"]].add(p.get("value", ""))

# A rule repeated over a list stands for one copy per name in it ({char} filled in).
rule_expansions = {}
for r in d.get("integrationRules", []):
    if r.get("forEach"):
        rule_expansions["rule " + r["key"]] = (r.get("forEachAs", "item"), sorted(list_items[r["forEach"].lstrip("$")]))


def expand(where, text):
    for prefix, (as_name, items) in rule_expansions.items():
        if where.startswith(prefix) and "{" + as_name + "}" in text:
            return [text.replace("{" + as_name + "}", item) for item in items]
    return [text]


# Variables SMSAndroids' own C# writes (SaveManager / gifting helpers), and the gift names its gift UI writes.
csharp_writes = collections.defaultdict(set)
src = os.path.join(HERE, "..", "..", "SMSAndroids")
for f in os.listdir(src):
    if f.endswith(".cs"):
        text = open(os.path.join(src, f), encoding="utf-8", errors="replace").read()
        for m in re.finditer(r'(?:SaveManager\.Set\w+|SetGifting\w+|ModForgeBridge\.Set\w*)\(\s*\$?"([A-Za-z0-9_\-{}]+)"', text):
            csharp_writes[m.group(1)].add("*csharp " + f)
        for m in re.finditer(r'AddGiftItem\("([^"]+)"', text):
            csharp_writes["Gifting_Gift"].add(m.group(1))

reads = collections.defaultdict(lambda: collections.defaultdict(set))   # name -> value -> {where}
reads_any = collections.defaultdict(set)
writes = collections.defaultdict(lambda: collections.defaultdict(set))
for name, values in csharp_writes.items():
    for v in values:
        writes[name][v].add("SMSAndroids C#")
for where, conds in cond_sites:
    for c, neg, w in walk_conditions(conds, where):
        p = c.get("params", {})
        raw_name = p.get("name") or p.get("list")
        if not raw_name or p.get("source") == "vanilla":
            continue
        for name, value in zip(expand(where, raw_name), expand(where, p.get("value", "")) if "{" in p.get("value", "") else [p.get("value", "")] * 999):
            reads_any[name].add(where)
            if c["type"] == "VariableCompare" and p.get("comparison", "equals") == "equals":
                reads[name][value].add(where)
            elif c["type"] == "VariableStartsWith":
                reads[name]["startswith:" + value].add(where)
            elif c["type"] == "ListContains":
                reads[name]["contains:" + value].add(where)
for where, acts in act_sites:
    for a, w in acts:
        p = a.get("params", {})
        if str(p.get("source", "")).lower() == "vanilla":
            continue
        for name in expand(where, p.get("name", "")) if p.get("name") else []:
            if a["type"] == "PickRandomFromList":
                for item in list_items[p.get("fromList", "").lstrip("$")] | ({p["fallback"]} if p.get("fallback") else set()):
                    writes[name][item].add(w)
                reads_any[p.get("fromList", "").lstrip("$")].add(w)
            elif a["type"] in ("SetVariable", "IncrementVariable", "CountList", "DiceRoll"):
                writes[name][p.get("value", "*" + a["type"]) if a["type"] == "SetVariable" else "*" + a["type"]].add(w)
                if a["type"] == "CountList":
                    reads_any[p.get("fromList", "")].add(w)
        if a["type"] in ("AddToList", "RemoveFromList") and p.get("list"):
            for name in expand(where, p["list"]):
                writes[name]["list:" + p.get("value", "")].add(w)
                reads_any[name].add(w)
        for k in ("excluding", "fromList"):
            if str(p.get(k, "")).startswith("$"):
                reads_any[p[k][1:]].add(w)
        for v in p.values():
            for m in re.finditer(r"\$([A-Za-z0-9_{}]+)", str(v)):
                for name in expand(where, m.group(1)):
                    reads_any[name].add(w)

lines = []
for r in d.get("integrationRules", []):
    lines.append("RULE %s [%s] %s" % (r["key"], r.get("triggerMode"), (r.get("description") or "").replace("\n", " ")))
    if r.get("forEach"):
        lines.append("   for each %s in %s" % (r.get("forEachAs"), r["forEach"]))
    for i, part in enumerate([r] + list(r.get("branches") or [])):
        tag = "   " if i == 0 else "   branch %d: " % i
        lines.append(tag + "IF " + " AND ".join(cond_text(c) for c in part.get("conditions", [])))
        lines.append(tag + "DO " + "; ".join(act_text(a) for a in part.get("actions", [])))
for dl in d.get("dialogues", []):
    lines.append("DIALOGUE %s starts when %s%s" % (dl["key"], " AND ".join(cond_text(c) for c in dl.get("startConditions", [])) or "(talked to only)",
                                                 "" if dl.get("queued", True) else " [not queued]"))
open(os.path.join(OUT, "logic.txt"), "w", encoding="utf-8").write("\n".join(lines) + "\n")

problems = []
for name in sorted(set(reads_any) | set(writes)):
    if name not in declared and not name.startswith("$") and "{" not in name:
        problems.append("UNDECLARED %s (read in %d places, written in %d)" % (name, len(reads_any[name]), sum(len(w) for w in writes[name].values())))
for name, values in sorted(reads.items()):
    if "{" in name:
        continue
    default = declared.get(name, {}).get("defaultValue", "")
    written = set(writes.get(name, {}))
    dynamic = any(v.startswith("*") or v.startswith("$") or "{" in v for v in written)
    for value, where in sorted(values.items()):
        if value.startswith(("startswith:", "contains:")) or "{" in value or value.startswith("$"):
            continue
        if value in written or value == default or dynamic:
            continue
        if declared.get(name, {}).get("type") == "Bool" and value in ("true", "false"):
            continue
        close = [w for w in written if w.lower() == value.lower()]
        problems.append("NEVER SET %s == %r is read by %s%s" % (name, value, "; ".join(sorted(where)[:4]),
                        "  <- written as %r (case differs)" % close[0] if close else ""))
for name, values in sorted(writes.items()):
    if "{" in name or name in ("",):
        continue
    read_values = set(reads.get(name, {}))
    prefixes = [v[len("startswith:"):] for v in read_values if v.startswith("startswith:")]
    for value, where in sorted(values.items()):
        if value.startswith(("*", "$", "list:")) or "{" in value or not name.startswith("Location_"):
            continue
        if value in read_values or any(value.startswith(p) for p in prefixes) or value == "Unknown":
            continue
        problems.append("NOBODY READS %s = %r (set by %s)" % (name, value, "; ".join(sorted(where)[:3])))
for name, v in sorted(declared.items()):
    if name not in reads_any and name not in writes:
        problems.append("UNUSED variable %s (%s) - no condition or action in the pack uses it%s" % (
            name, v.get("type"), "; " + v["description"][:90] if v.get("description") else ""))

scenes = {s["key"] for s in d.get("scenes", [])}
used_scenes = set()
for where, acts in act_sites:
    for a, where in acts:
        p = a.get("params", {})
        if a["type"] == "SetGameObjectActive" and p.get("kind") == "Scene":
            used_scenes.add(p.get("target"))
            if p.get("target") not in scenes:
                problems.append("MISSING SCENE %s used by %s" % (p.get("target"), where))
        if a["type"] == "CharacterFocus" and set(p) - {"actor", "focused"}:
            problems.append("LEFTOVER PARAMS on CharacterFocus in %s: %s" % (where, act_text(a)))
for s in sorted(scenes - used_scenes):
    problems.append("SCENE NEVER SHOWN %s - no action in the pack activates it" % s)

open(os.path.join(OUT, "consistency.txt"), "w", encoding="utf-8").write("\n".join(problems) + "\n")
kinds = collections.Counter(p.split(" ")[0] + (" " + p.split(" ")[1] if p.startswith(("NEVER", "NOBODY", "MISSING", "SCENE", "LEFTOVER")) else "") for p in problems)
print("logic: %d lines; consistency: %d findings %s" % (len(lines), len(problems), dict(kinds)))
print("written to", OUT)
