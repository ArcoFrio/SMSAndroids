using System.IO;
using SMSModForge.Model;
using SMSModForge.Validation;

// PackTool export <pack folder> <out .smspack> [issues report]
// Reads the pack the way the editor opens it (migrations reported, nothing written to the folder),
// lists the editor's issues, then writes the archive with the editor's own exporter.
if (args.Length < 3 || args[0] != "export")
{
    Console.Error.WriteLine("usage: PackTool export <pack folder> <out .smspack> [issues report]");
    return 2;
}
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
