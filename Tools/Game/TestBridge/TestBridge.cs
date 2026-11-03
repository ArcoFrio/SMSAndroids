#if DEBUG
using BepInEx;
using GameCreator.Runtime.Common;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace SMSAndroidsTestBridge
{
    /// <summary>
    /// SMSAndroids' half of the in-game checks (Tools/Game): what ModForge's
    /// own test bridge cannot see - this mod's load chain, the errors the game
    /// logs while a check runs, and the drawn layout of a UI for judging its look.
    /// <para/>
    /// Debug builds only, and asleep unless the game was started with
    /// <c>-mfBridge &lt;name&gt;</c>, which only Tools/Game/launch.ps1 passes.
    /// Commands are read from <c>BepInEx/plugins/SMSAndroidsCore/DebugBridge/&lt;name&gt;/commands.txt</c>
    /// and answered in <c>results.txt</c> beside it, a time first on every line,
    /// the same form as ModForge's bridge.
    /// <list type="bullet">
    /// <item><c>state</c>: every load flag of this mod, and whether a dialogue plays.</item>
    /// <item><c>errors</c> / <c>errors clear</c>: errors and exceptions the game logged since the last clear.</item>
    /// <item><c>signal &lt;name&gt;</c>: a Game Creator signal, as a pack's EmitSignal sends it.</item>
    /// <item><c>massage</c>: the massage minigame started, what the Debug P key does.</item>
    /// <item><c>busts</c> / <c>cg</c>: the busts on screen with their parts, the scene pictures up.</item>
    /// <item><c>canvases</c>: every canvas on screen, its sort order and size.</item>
    /// <item><c>at &lt;x&gt; &lt;y&gt;</c>: every UI graphic over a screen point, topmost first.</item>
    /// <item><c>ui &lt;file&gt; &lt;root&gt;[/path]</c>: everything drawn under an object written to
    /// <c>&lt;file&gt;.txt</c> - rects on screen, sprites, colours, fonts, text that overflows.</item>
    /// </list>
    /// Nothing here saves.
    /// </summary>
    [BepInPlugin(pluginGuid, "SMSAndroids test bridge", "1.0.0")]
    internal sealed class TestBridge : BaseUnityPlugin
    {
        public const string pluginGuid = "treboy.starmakerstory.smsandroids.testbridge";

        private string _dir;
        private StreamWriter _results;
        private long _readUpTo;
        private readonly List<string> _errors = new List<string>();
        private readonly object _errorLock = new object();

        private void Awake()
        {
            string name = Arg("-mfBridge");
            if (name == null) { enabled = false; return; }
            _dir = Path.Combine(Paths.PluginPath, "SMSAndroidsTestBridge", "DebugBridge", name);
            Directory.CreateDirectory(_dir);
            File.Delete(Path.Combine(_dir, "commands.txt"));
            _results = new StreamWriter(Path.Combine(_dir, "results.txt"), false, new UTF8Encoding(false)) { AutoFlush = true };
            Application.logMessageReceivedThreaded += OnLog;
            Say("ok bridge up");
        }

        private void OnDestroy()
        {
            Application.logMessageReceivedThreaded -= OnLog;
            if (_results != null) _results.Dispose();
        }

        private void OnLog(string message, string stack, LogType type)
        {
            if (type != LogType.Error && type != LogType.Exception && type != LogType.Assert) return;
            string first = (stack ?? "").Split('\n').FirstOrDefault(l => l.Trim().Length > 0) ?? "";
            lock (_errorLock) _errors.Add(type + ": " + message.Replace("\n", " ") + (first.Length > 0 ? " @ " + first.Trim() : ""));
        }

        private static string Arg(string name)
        {
            var args = Environment.GetCommandLineArgs();
            int at = Array.IndexOf(args, name);
            return at >= 0 && at + 1 < args.Length ? args[at + 1] : null;
        }

        private void Say(string line)
        {
            _results.WriteLine(DateTime.Now.ToString("HH:mm:ss.fff", CultureInfo.InvariantCulture) + " " + line);
        }

        private void Update()
        {
            // A command file is written whole by the driver and replaced for the next batch.
            string path = Path.Combine(_dir, "commands.txt");
            if (!File.Exists(path)) return;
            string[] lines;
            try { lines = File.ReadAllLines(path); File.Delete(path); }
            catch (IOException) { return; }
            foreach (var line in lines.Select(l => l.Trim()).Where(l => l.Length > 0))
            {
                try { Run(line); }
                catch (Exception e) { Say("error " + line + ": " + e.GetType().Name + " " + e.Message); }
            }
        }

        private void Run(string command)
        {
            var parts = command.Split(new[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries);
            string verb = parts[0].ToLowerInvariant();
            string rest = parts.Length > 1 ? parts[1].Trim() : "";
            switch (verb)
            {
                case "ping":
                    Say("ok ping scene=" + SceneManager.GetActiveScene().name);
                    break;
                case "state":
                    // The game in play: its scene up, and its levels and bust manager there to be read.
                    Say("ok state scene=" + SceneManager.GetActiveScene().name + " levels=" + (GameObject.Find("5_Levels") != null)
                        + " busts=" + (GameObject.Find("2_Bust_Manager") != null));
                    break;
                case "errors":
                {
                    string[] now;
                    lock (_errorLock)
                    {
                        now = _errors.ToArray();
                        if (rest == "clear") _errors.Clear();
                    }
                    Say("ok errors " + now.Length + (now.Length == 0 ? "" : " || " + string.Join(" || ", now.Distinct().Take(40).ToArray())));
                    break;
                }
                case "signal":
                    Signals.Emit(new SignalArgs(new PropertyName(rest), null));
                    Say("ok signal " + rest);
                    break;
                case "busts":
                {
                    // Every bust up, with the outfit and face parts it shows.
                    var bustManager = GameObject.Find("2_Bust_Manager");
                    if (bustManager == null) { Say("error busts: no bust manager"); break; }
                    var up = new List<string>();
                    foreach (Transform bust in bustManager.transform)
                    {
                        if (!bust.gameObject.activeInHierarchy) continue;
                        var drawn = bust.GetComponentsInChildren<SpriteRenderer>(false).Where(r => r.enabled)
                            .Select(r => r.name + (r.sprite == null ? "(no sprite)" : "") + (r.color.a < 0.99f ? " a" + N(r.color.a) : ""));
                        up.Add(bust.name + " [" + string.Join(", ", drawn.ToArray()) + "]");
                    }
                    Say("ok busts " + (up.Count == 0 ? "(none)" : string.Join(" | ", up.ToArray())));
                    break;
                }
                case "cg":
                {
                    // The scene pictures up (the game's CG manager), and whether each draws anything.
                    var cgManager = GameObject.Find("4_CG_Manager-Sexy");
                    if (cgManager == null) { Say("error cg: no CG manager"); break; }
                    var up = new List<string>();
                    foreach (Transform scene in cgManager.transform)
                    {
                        if (!scene.gameObject.activeInHierarchy) continue;
                        var images = scene.GetComponentsInChildren<Image>(false);
                        var sprites = scene.GetComponentsInChildren<SpriteRenderer>(false);
                        bool draws = images.Any(i => i.enabled && i.sprite != null) || sprites.Any(s => s.enabled && s.sprite != null);
                        up.Add(scene.name + (draws ? "" : " (DRAWS NOTHING)"));
                    }
                    Say("ok cg " + (up.Count == 0 ? "(none)" : string.Join(" | ", up.ToArray())));
                    break;
                }
                case "canvases":
                {
                    var found = FindObjectsOfType<Canvas>().Where(c => c.isRootCanvas && c.gameObject.activeInHierarchy && c.enabled)
                        .OrderBy(c => c.sortingOrder)
                        .Select(c => PathOf(c.transform) + " order " + c.sortingOrder + " " + c.renderMode
                                     + " " + Size(((RectTransform)c.transform).rect) + " scale " + N(c.transform.lossyScale.x));
                    Say("ok canvases " + string.Join(" | ", found.ToArray()));
                    break;
                }
                case "ui":
                {
                    var words = rest.Split(new[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries);
                    var target = words.Length > 1 ? Find(words[1]) : null;
                    if (target == null) { Say("error ui: no " + rest); break; }
                    var lines = new List<string>();
                    Dump(target, 0, lines);
                    File.WriteAllLines(Path.Combine(_dir, words[0] + ".txt"), lines.ToArray());
                    Say("ok ui " + words[0] + " " + lines.Count + " lines, screen " + Screen.width + "x" + Screen.height);
                    break;
                }
                case "at":
                {
                    // at <x> <y>: every UI graphic drawn over that screen point (y from the bottom),
                    // the canvas drawn last first - what a player sees there and what covers what.
                    var xy = rest.Split(' ');
                    var point = new Vector2(float.Parse(xy[0], CultureInfo.InvariantCulture), float.Parse(xy[1], CultureInfo.InvariantCulture));
                    var hits = new List<KeyValuePair<int, string>>();
                    foreach (var g in FindObjectsOfType<Graphic>())
                    {
                        if (!g.isActiveAndEnabled || g.canvas == null || ShownAlpha(g) < 0.02f) continue;
                        var root = g.canvas.rootCanvas;
                        Camera cam = root.renderMode == RenderMode.ScreenSpaceOverlay ? null : root.worldCamera;
                        if (!RectTransformUtility.RectangleContainsScreenPoint(g.rectTransform, point, cam)) continue;
                        hits.Add(new KeyValuePair<int, string>(root.sortingOrder * 100000 + g.depth,
                            PathOf(g.transform) + " (" + g.GetType().Name + " " + Hex(g.color) + " shown " + N(ShownAlpha(g)) + ", canvas " + root.name + " " + root.sortingOrder + ")"));
                    }
                    Say("ok at " + rest + ": " + (hits.Count == 0 ? "(nothing)" : string.Join(" | ", hits.OrderByDescending(h => h.Key).Select(h => h.Value).Take(12).ToArray())));
                    break;
                }
                case "xf":
                {
                    // xf <path>: where an object is, locally, and whether it is up.
                    var target = Find(rest);
                    if (target == null) { Say("error xf: no " + rest); break; }
                    var p = target.localPosition;
                    Say("ok xf " + rest + ": active " + target.gameObject.activeInHierarchy + " local " + N(p.x) + "," + N(p.y)
                        + " scale " + N(target.localScale.x) + " children " + target.childCount);
                    break;
                }
                case "particles":
                {
                    // particles <path>: the colour a particle system's particles start in.
                    var target = Find(rest);
                    var system = target != null ? target.GetComponent<ParticleSystem>() : null;
                    if (system == null) { Say("error particles: no particle system at " + rest); break; }
                    var c = system.main.startColor.color;
                    Say("ok particles " + rest + ": start #" + ColorUtility.ToHtmlStringRGBA(c));
                    break;
                }
                case "hover":
                {
                    // hover <path> [off]: the pointer arriving on (or leaving) an object, as a mouse would - for
                    // seeing a hover state without clicking.
                    bool off = rest.EndsWith(" off");
                    string path = off ? rest.Substring(0, rest.Length - 4) : rest;
                    var target = Find(path);
                    if (target == null) { Say("error hover: no " + path); break; }
                    var data = new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current);
                    if (off) UnityEngine.EventSystems.ExecuteEvents.ExecuteHierarchy(target.gameObject, data, UnityEngine.EventSystems.ExecuteEvents.pointerExitHandler);
                    else UnityEngine.EventSystems.ExecuteEvents.ExecuteHierarchy(target.gameObject, data, UnityEngine.EventSystems.ExecuteEvents.pointerEnterHandler);
                    Say("ok hover " + rest);
                    break;
                }
                case "track":
                {
                    // track <seconds> <name> <path>|<path>...: every frame for that long, each object's local and
                    // world rotation, scale and position, into <name>.txt - for seeing what an animation moves.
                    var words = rest.Split(new[] { ' ' }, 3, StringSplitOptions.RemoveEmptyEntries);
                    var targets = words[2].Split('|').Select(p => new KeyValuePair<string, Transform>(p, Find(p))).ToList();
                    var missing = targets.Where(t => t.Value == null).Select(t => t.Key).ToArray();
                    if (missing.Length > 0) { Say("error track: no " + string.Join(", ", missing)); break; }
                    StartCoroutine(Track(float.Parse(words[0], CultureInfo.InvariantCulture), Path.Combine(_dir, words[1] + ".txt"), targets));
                    Say("ok track " + targets.Count + " for " + words[0] + "s");
                    break;
                }
                default:
                    Say("error unknown command: " + command);
                    break;
            }
        }

        private static System.Collections.IEnumerator Track(float seconds, string file, List<KeyValuePair<string, Transform>> targets)
        {
            var lines = new List<string>();
            float start = Time.realtimeSinceStartup;
            while (Time.realtimeSinceStartup - start < seconds)
            {
                string t = N(Time.realtimeSinceStartup - start);
                foreach (var target in targets)
                {
                    var x = target.Value;
                    if (x == null) continue;
                    var rt = x as RectTransform;
                    lines.Add(t + "\t" + target.Key + "\tactive " + x.gameObject.activeInHierarchy + "\tlocalRotZ " + N(x.localEulerAngles.z)
                              + "\tworldRotZ " + N(x.eulerAngles.z) + "\tlocalScale " + N(x.localScale.x) + "," + N(x.localScale.y)
                              + "\tlossyScale " + N(x.lossyScale.x) + "\tanchored " + (rt != null ? N(rt.anchoredPosition.x) + "," + N(rt.anchoredPosition.y) : "-")
                              + "\tworld " + N(x.position.x) + "," + N(x.position.y));
                }
                // At the end of the frame: what is drawn, after every LateUpdate has had its say.
                yield return new WaitForEndOfFrame();
            }
            File.WriteAllLines(file, lines.ToArray());
        }

        /// <summary>A root object by name, then a path below it; inactive roots included.</summary>
        private static Transform Find(string path)
        {
            int slash = path.IndexOf('/');
            string rootName = slash < 0 ? path : path.Substring(0, slash);
            var roots = new List<GameObject>();
            for (int i = 0; i < SceneManager.sceneCount; i++) roots.AddRange(SceneManager.GetSceneAt(i).GetRootGameObjects());
            var probe = new GameObject("TestBridgeProbe");
            DontDestroyOnLoad(probe);
            roots.AddRange(probe.scene.GetRootGameObjects());
            Destroy(probe);
            var root = roots.FirstOrDefault(r => r != null && r.name == rootName);
            if (root == null) return null;
            return slash < 0 ? root.transform : root.transform.Find(path.Substring(slash + 1));
        }

        /// <summary>One line per object drawn: where it is on screen and what it draws with.</summary>
        private static void Dump(Transform t, int depth, List<string> lines)
        {
            if (!t.gameObject.activeInHierarchy) { lines.Add(new string(' ', depth * 2) + t.name + " (hidden)"); return; }
            var sb = new StringBuilder(new string(' ', depth * 2)).Append(t.name);
            var rt = t as RectTransform;
            if (rt != null)
            {
                var corners = new Vector3[4];
                rt.GetWorldCorners(corners);
                var canvas = t.GetComponentInParent<Canvas>();
                Camera cam = canvas != null && canvas.rootCanvas.renderMode != RenderMode.ScreenSpaceOverlay ? canvas.rootCanvas.worldCamera : null;
                Vector2 a = RectTransformUtility.WorldToScreenPoint(cam, corners[0]);
                Vector2 b = RectTransformUtility.WorldToScreenPoint(cam, corners[2]);
                sb.Append(" [screen ").Append(N(a.x)).Append(',').Append(N(a.y)).Append(" - ").Append(N(b.x)).Append(',').Append(N(b.y)).Append(']');
                if (b.x < 0 || b.y < 0 || a.x > Screen.width || a.y > Screen.height) sb.Append(" OFFSCREEN");
                else if (a.x < -1 || a.y < -1 || b.x > Screen.width + 1 || b.y > Screen.height + 1) sb.Append(" CLIPPED-BY-SCREEN");
            }
            var image = t.GetComponent<Image>();
            if (image != null && image.enabled)
                sb.Append(" Image(").Append(image.sprite != null ? image.sprite.name : "no sprite").Append(' ').Append(Hex(image.color))
                  .Append(' ').Append(image.type).Append(image.preserveAspect ? " aspect" : "").Append(')');
            var raw = t.GetComponent<RawImage>();
            if (raw != null && raw.enabled)
                sb.Append(" RawImage(").Append(raw.texture != null ? raw.texture.name : "none").Append(' ').Append(Hex(raw.color)).Append(')');
            var text = t.GetComponent<TMP_Text>();
            if (text != null && text.enabled)
            {
                sb.Append(" Text(\"").Append((text.text ?? "").Replace("\n", "\\n")).Append("\" ")
                  .Append(text.font != null ? text.font.name : "no font").Append(' ').Append(N(text.fontSize))
                  .Append(text.enableAutoSizing ? " auto " + N(text.fontSizeMin) + "-" + N(text.fontSizeMax) : "")
                  .Append(' ').Append(Hex(text.color)).Append(' ').Append(text.alignment).Append(')');
                if (text.isTextOverflowing) sb.Append(" OVERFLOWS");
                if (rt != null && text.enableWordWrapping == false && text.preferredWidth > rt.rect.width + 1) sb.Append(" WIDER-THAN-BOX");
            }
            var legacy = t.GetComponent<Text>();
            if (legacy != null && legacy.enabled)
                sb.Append(" LegacyText(\"").Append(legacy.text.Replace("\n", "\\n")).Append("\" ").Append(legacy.font != null ? legacy.font.name : "-")
                  .Append(' ').Append(legacy.fontSize).Append(' ').Append(Hex(legacy.color)).Append(')');
            var button = t.GetComponent<Selectable>();
            if (button != null) sb.Append(' ').Append(button.GetType().Name).Append(button.interactable ? "" : "(off)");
            var group = t.GetComponent<CanvasGroup>();
            if (group != null) sb.Append(" alpha ").Append(N(group.alpha));
            var layout = t.GetComponent<LayoutGroup>();
            if (layout != null) sb.Append(' ').Append(layout.GetType().Name);
            lines.Add(sb.ToString());
            foreach (Transform child in t) Dump(child, depth + 1, lines);
        }

        /// <summary>How opaque a graphic is drawn: its colour, its renderer's alpha and every canvas group above it.</summary>
        private static float ShownAlpha(Graphic g)
        {
            float alpha = g.color.a * g.canvasRenderer.GetAlpha();
            for (var t = g.transform; t != null; t = t.parent)
            {
                var group = t.GetComponent<CanvasGroup>();
                if (group != null) { alpha *= group.alpha; if (group.ignoreParentGroups) break; }
            }
            return alpha;
        }

        private static string PathOf(Transform t) => t.parent == null ? t.name : PathOf(t.parent) + "/" + t.name;
        private static string N(float v) => v.ToString("0.##", CultureInfo.InvariantCulture);
        private static string Size(Rect r) => N(r.width) + "x" + N(r.height);
        private static string Hex(Color c) => "#" + ColorUtility.ToHtmlStringRGBA(c);
    }
}
#endif
