using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SMSModForge.Model;
using SMSModForge.Rendering;
using SMSModForge.Validation;

// PackTool export <pack folder> <out .smspack> [issues report]
//   Reads the pack the way the editor opens it (migrations reported, nothing written to the folder),
//   lists the editor's issues, then writes the archive with the editor's own exporter.
// PackTool ui-tree <vanillaui token> [depth]
//   Prints one of the game's screens as the editor seeds it: bind path, active, rect.
// PackTool ui-graft <spec.json> <out.json>
//   Seeds the game screen the spec names with the editor's own code, hangs the spec's new objects
//   under the bound objects it names, and writes that UI exactly as the editor would save it.
//   spec: { "id", "name", "source": "vanillaui:...", "grafts": [ { "bind": "A/B", "node": {UiNodeDef} } ] }
switch (args.FirstOrDefault())
{
    case "export" when args.Length >= 3: return Export(args);
    case "ui-tree" when args.Length >= 2: return UiTree(args[1], args.Length > 2 ? int.Parse(args[2]) : 99);
    case "ui-graft" when args.Length >= 3: return UiGraft(args[1], args[2]);
    case "resave-ui" when args.Length >= 4: return ResaveUi(args[1], args[2], args[3]);
}
Console.Error.WriteLine("usage: PackTool export <pack folder> <out .smspack> [issues report]\n"
                      + "       PackTool ui-tree <vanillaui token> [depth]\n"
                      + "       PackTool ui-graft <spec.json> <out.json>");
return 2;

static int Export(string[] args)
{
    string root = Path.GetFullPath(args[1]), output = Path.GetFullPath(args[2]);
    string? report = args.Length > 3 ? Path.GetFullPath(args[3]) : null;

    var pack = PackRepository.Load(root);
    var migration = PackRepository.LastMigration;
    Console.WriteLine($"pack {pack.PackId} {pack.Version}");
    if (migration is { Migrated: true })
        foreach (var change in migration.Changes) Console.WriteLine("migration: " + change);
    else Console.WriteLine("migration: none (the folder is in this ModForge's form)");

    var issues = PackValidator.Validate(pack, root);
    var lines = issues.OrderByDescending(i => i.Severity).ThenBy(i => i.Where)
                      .Select(i => $"{i.Severity}\t{i.Code}\t{i.Where}\t{i.Message.Replace('\n', ' ')}").ToList();
    foreach (var group in issues.GroupBy(i => i.Severity).OrderByDescending(g => g.Key))
        Console.WriteLine($"issues {group.Key}: {group.Count()}");
    if (report != null) File.WriteAllLines(report, lines);
    else foreach (var line in lines.Where(l => l.StartsWith("Error"))) Console.WriteLine(line);

    var result = PackExporter.Export(root, output);
    Console.WriteLine($"exported {result.FileCount} files, {result.SourceBytes / 1048576.0:N1} MB -> {result.CompressedBytes / 1048576.0:N1} MB: {result.OutputPath}");
    return 0;
}

static UiNodeDef Seed(string token)
{
    var screen = VanillaUiCatalog.Find(token) ?? throw new InvalidOperationException("not in the catalogue: " + token);
    var vanilla = VanillaUiLibrary.Node(screen) ?? throw new InvalidOperationException("no surface data for " + token);
    return VanillaUiSeed.FromBase(vanilla);
}

static IEnumerable<(UiNodeDef Node, int Depth)> Walk(UiNodeDef node, int depth = 0)
{
    yield return (node, depth);
    foreach (var child in node.Children)
        foreach (var x in Walk(child, depth + 1)) yield return x;
}

static int UiTree(string token, int maxDepth)
{
    foreach (var (n, d) in Walk(Seed(token)).Where(x => x.Depth <= maxDepth))
    {
        var r = n.Rect;
        Console.WriteLine($"{new string(' ', d * 2)}{n.Bind}  active={n.StartActive}  anchor=({r.AnchorMin[0]},{r.AnchorMin[1]})-({r.AnchorMax[0]},{r.AnchorMax[1]})"
                        + $" pivot=({r.Pivot[0]},{r.Pivot[1]}) pos=({r.Position[0]},{r.Position[1]}) size=({r.Size[0]},{r.Size[1]})"
                        + (n.Image != null ? $" image={n.Image.Sprite}" : "") + (n.Text != null ? $" text=\"{n.Text.Value}\"" : ""));
    }
    return 0;
}

// What one UI becomes after the editor opens the pack (seeding its game screen back in, as the UI tab does) and saves it.
static int ResaveUi(string packRoot, string uiId, string outPath)
{
    var pack = PackRepository.Load(Path.GetFullPath(packRoot));
    var ui = pack.Uis.First(u => u.Id == uiId);
    var vanilla = VanillaUiLibrary.Node(VanillaUiCatalog.Find(ui.Source));
    ui.Nodes = new List<UiNodeDef> { VanillaUiSeed.Merge(vanilla!, ui.Nodes, out int stranded) };
    var saved = JObject.Parse(PackRepository.SerializeAsSaved(pack));
    File.WriteAllText(outPath, ((JArray)saved["uis"]!).First(u => (string)u["id"]! == uiId).ToString(Formatting.Indented));
    Console.WriteLine($"{uiId}: {stranded} stranded -> {outPath}");
    return 0;
}

static int UiGraft(string specPath, string outPath)
{
    var spec = JObject.Parse(File.ReadAllText(specPath));
    string source = (string)spec["source"]!;
    var root = Seed(source);
    var byBind = Walk(root).ToDictionary(x => x.Node.Bind, x => x.Node);
    var serializer = JsonSerializer.Create(PackRepository.ManifestSettings);

    // An object the catalogue does not list - one a pack builds into the screen at runtime, such as its own
    // map button - is bound by name under the nearest object it does list; the runtime finds it by the same path.
    UiNodeDef BoundNode(string bind)
    {
        if (byBind.TryGetValue(bind, out var known)) return known;
        int cut = bind.LastIndexOf('/');
        if (cut <= 0) throw new InvalidOperationException("no object '" + bind + "' on " + source);
        var above = BoundNode(bind.Substring(0, cut));
        var made = new UiNodeDef { Name = bind.Substring(cut + 1), Bind = bind };
        above.Children.Add(made);
        byBind[bind] = made;
        return made;
    }

    UiNodeDef parent;
    foreach (var graft in (JArray)spec["grafts"]!)
    {
        string bind = (string)graft["bind"]!;
        parent = BoundNode(bind);
        parent.Children.Add(graft["node"]!.ToObject<UiNodeDef>(serializer)!);
    }

    var pack = PackRepository.CreateEmpty("PackToolUiGraft");
    pack.Uis.Add(new UiDef { Id = (string)spec["id"]!, Name = (string)spec["name"]!, Source = source, Nodes = { root } });
    var saved = JObject.Parse(PackRepository.SerializeAsSaved(pack));
    File.WriteAllText(outPath, saved["uis"]![0]!.ToString(Formatting.Indented));
    Console.WriteLine($"{spec["grafts"]!.Count()} graft(s) on {source} -> {outPath}");
    return 0;
}
