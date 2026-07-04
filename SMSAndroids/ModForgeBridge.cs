using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace SMSAndroidsCore
{
    /// <summary>
    /// Reflection-only bridge to <c>SMSModForge.PackPlugin.Plugin</c>. Lets the
    /// rest of SMSAndroids read + react to pack variables, place schedule writes
    /// against pack lists, and subscribe to per-variable change events — all
    /// without taking a hard reference on the ModForge assembly (which lives as
    /// a sibling BepInEx plugin and can be missing entirely).
    /// <para/>
    /// Resolution is lazy: nothing reaches across plugins until the first call.
    /// If ModForge isn't loaded, every method is a graceful no-op (returns the
    /// supplied default for reads, <c>false</c> for writes). This is the SAFE
    /// substitute for the old <see cref="SaveManager"/>-backed reads — call
    /// sites switching to it can never hard-crash if ModForge is removed.
    /// <para/>
    /// Cached <see cref="MethodInfo"/> handles are shared across calls; the
    /// resolution cost is paid exactly once per game session.
    /// </summary>
    public static class ModForgeBridge
    {
        // The ModForge plugin singleton is exposed as a static auto-
        // property named "Instance" on the Plugin type. Most public-API
        // methods are *instance* methods (not static) — see
        // SMSModForge.PackPlugin/Plugin.cs.
        private const string PluginTypeName = "SMSModForge.PackPlugin.Plugin";
        private const string InstancePropertyName = "Instance";

        // Two-stage resolution:
        //   • _typeResolved is set once the ModForge type is found (or
        //     conclusively not found in the loaded assemblies).
        //   • _pluginInstance is re-fetched from the Instance property
        //     on every IsAvailable check until it's non-null, so we
        //     gracefully handle the case where SMSAndroids loaded
        //     before ModForge reached Awake.
        private static bool _typeResolved;
        private static Type _pluginType;
        private static PropertyInfo _instanceProperty;
        private static object _pluginInstance;
        private static bool _eventAttached;

        // Instance methods on Plugin.
        private static MethodInfo _hasPack;
        private static MethodInfo _getPackActiveSlot;
        private static MethodInfo _flushPackToDisk;
        private static MethodInfo _hasPackVariable;
        private static MethodInfo _enumeratePackVariables;
        private static MethodInfo _getString, _getBool, _getInt, _getFloat, _getList;
        private static MethodInfo _setString, _setBool, _setInt, _setFloat;
        private static MethodInfo _addToList, _removeFromList, _clearList;
        private static MethodInfo _isAnyDialoguePlaying;

        // The global change event is static (signature: packId, name, oldValue, newValue).
        // We resolve it once and attach a single forwarding handler; per-key
        // subscriptions then fan out from _subscribers.
        private static EventInfo _onPackVariableChangedEvent;
        private static Delegate _attachedForwarder;

        // Per-(packId|varName) subscriber lists. Key format: "<packId>|<name>"
        // (the same delimiter ModForge uses internally, so it's easy to grep).
        // Multiple subscribers per key are allowed; the dispatch is O(subscribers).
        private static readonly Dictionary<string, List<Action<string, string>>> _subscribers
            = new Dictionary<string, List<Action<string, string>>>(StringComparer.Ordinal);

        /// <summary>True when ModForge was found in the loaded assemblies AND
        /// its <c>Plugin.Instance</c> static field has been populated (i.e.
        /// ModForge has reached <c>Awake</c>). Both checks are pre-conditions
        /// for every other method here.</summary>
        public static bool IsAvailable
        {
            get
            {
                EnsureResolved();
                return _pluginType != null && _pluginInstance != null;
            }
        }

        // ── Read API ──────────────────────────────────────────────────────

        /// <summary>Returns true when the named pack is loaded.</summary>
        public static bool HasPack(string packId)
        {
            EnsureResolved();
            if (_pluginInstance == null || _hasPack == null || string.IsNullOrEmpty(packId)) return false;
            try { return (bool)_hasPack.Invoke(_pluginInstance, new object[] { packId }); }
            catch (Exception ex) { LogWarn("HasPack failed: " + ex.Message); return false; }
        }

        /// <summary>The NanoSave slot the named pack's store is bound to, or
        /// <c>-1</c> when unloaded / not yet bound. Used to wait for the pack to
        /// finish loading a slot before writing into it.</summary>
        public static int GetPackActiveSlot(string packId)
        {
            EnsureResolved();
            if (_pluginInstance == null || _getPackActiveSlot == null) return -1;
            try { return (int)_getPackActiveSlot.Invoke(_pluginInstance, new object[] { packId }); }
            catch (Exception ex) { LogWarn("GetPackActiveSlot failed: " + ex.Message); return -1; }
        }

        /// <summary>Force an immediate write of the named pack's in-memory state
        /// to its bound slot file. Returns true when a write happened.</summary>
        public static bool FlushPackToDisk(string packId)
        {
            EnsureResolved();
            if (_pluginInstance == null || _flushPackToDisk == null) return false;
            try { return (bool)_flushPackToDisk.Invoke(_pluginInstance, new object[] { packId }); }
            catch (Exception ex) { LogWarn("FlushPackToDisk failed: " + ex.Message); return false; }
        }

        /// <summary>Returns true when the named variable is declared in the named pack.</summary>
        public static bool HasVariable(string packId, string varName)
        {
            EnsureResolved();
            if (_pluginInstance == null || _hasPackVariable == null) return false;
            try { return (bool)_hasPackVariable.Invoke(_pluginInstance, new object[] { packId, varName }); }
            catch (Exception ex) { LogWarn("HasVariable failed: " + ex.Message); return false; }
        }

        /// <summary>Enumerates every variable name declared on <paramref name="packId"/>.
        /// Empty enumeration when the pack isn't loaded.</summary>
        public static IEnumerable<string> EnumerateVariables(string packId)
        {
            EnsureResolved();
            if (_pluginInstance == null || _enumeratePackVariables == null) return Array.Empty<string>();
            try
            {
                var en = _enumeratePackVariables.Invoke(_pluginInstance, new object[] { packId })
                         as System.Collections.IEnumerable;
                if (en == null) return Array.Empty<string>();
                var list = new List<string>();
                foreach (var n in en) list.Add(n as string ?? "");
                return list;
            }
            catch (Exception ex) { LogWarn("EnumerateVariables failed: " + ex.Message); return Array.Empty<string>(); }
        }

        public static string GetString(string packId, string varName, string fallback = "")
        {
            EnsureResolved();
            if (_pluginInstance == null || _getString == null) return fallback;
            try { return (string)_getString.Invoke(_pluginInstance, new object[] { packId, varName, fallback }); }
            catch (Exception ex) { LogWarn("GetString failed: " + ex.Message); return fallback; }
        }

        /// <summary>
        /// True when ModForge has any dialogue playing (pack or vanilla, including
        /// a pack dialogue still in its fade-in window). Read each frame to gate
        /// our own dialogue/event logic while a pack dialogue runs. Pack-agnostic;
        /// returns false when ModForge isn't loaded.
        /// </summary>
        public static bool IsAnyDialoguePlaying()
        {
            EnsureResolved();
            if (_pluginInstance == null || _isAnyDialoguePlaying == null) return false;
            try { return (bool)_isAnyDialoguePlaying.Invoke(_pluginInstance, null); }
            catch (Exception ex) { LogWarn("IsAnyDialoguePlaying failed: " + ex.Message); return false; }
        }

        public static bool GetBool(string packId, string varName, bool fallback = false)
        {
            EnsureResolved();
            if (_pluginInstance == null || _getBool == null) return fallback;
            try { return (bool)_getBool.Invoke(_pluginInstance, new object[] { packId, varName, fallback }); }
            catch (Exception ex) { LogWarn("GetBool failed: " + ex.Message); return fallback; }
        }

        public static int GetInt(string packId, string varName, int fallback = 0)
        {
            EnsureResolved();
            if (_pluginInstance == null || _getInt == null) return fallback;
            try { return (int)_getInt.Invoke(_pluginInstance, new object[] { packId, varName, fallback }); }
            catch (Exception ex) { LogWarn("GetInt failed: " + ex.Message); return fallback; }
        }

        public static float GetFloat(string packId, string varName, float fallback = 0f)
        {
            EnsureResolved();
            if (_pluginInstance == null || _getFloat == null) return fallback;
            try { return (float)_getFloat.Invoke(_pluginInstance, new object[] { packId, varName, fallback }); }
            catch (Exception ex) { LogWarn("GetFloat failed: " + ex.Message); return fallback; }
        }

        /// <summary>Returns a snapshot of the named List variable as plain strings.
        /// Returns an empty list (never null) when the variable doesn't exist.</summary>
        public static IReadOnlyList<string> GetList(string packId, string varName)
        {
            EnsureResolved();
            if (_pluginInstance == null || _getList == null) return Array.Empty<string>();
            try
            {
                var raw = _getList.Invoke(_pluginInstance, new object[] { packId, varName });
                if (raw is System.Collections.IEnumerable en)
                {
                    var list = new List<string>();
                    foreach (var v in en) list.Add(v as string ?? "");
                    return list;
                }
                return Array.Empty<string>();
            }
            catch (Exception ex) { LogWarn("GetList failed: " + ex.Message); return Array.Empty<string>(); }
        }

        // ── Write API ─────────────────────────────────────────────────────
        // All writers return true when ModForge accepted the write and false
        // when no matching pack was found. They do NOT throw on missing
        // variables — ModForge's store falls back to an in-memory write so
        // the variable becomes visible to subsequent reads.

        public static bool SetString(string packId, string varName, string value)
            => InvokeBool(_setString, packId, varName, value);

        public static bool SetBool(string packId, string varName, bool value)
            => InvokeBool(_setBool, packId, varName, value);

        public static bool SetInt(string packId, string varName, int value)
            => InvokeBool(_setInt, packId, varName, value);

        public static bool SetFloat(string packId, string varName, float value)
            => InvokeBool(_setFloat, packId, varName, value);

        public static bool AddToList(string packId, string varName, string value)
            => InvokeBool(_addToList, packId, varName, value);

        public static bool RemoveFromList(string packId, string varName, string value)
            => InvokeBool(_removeFromList, packId, varName, value);

        public static bool ClearList(string packId, string varName)
        {
            EnsureResolved();
            if (_pluginInstance == null || _clearList == null) return false;
            try { return (bool)_clearList.Invoke(_pluginInstance, new object[] { packId, varName }); }
            catch (Exception ex) { LogWarn("ClearList failed: " + ex.Message); return false; }
        }

        // ── Change subscription ───────────────────────────────────────────

        /// <summary>
        /// Subscribe to changes on a specific pack variable. The handler
        /// receives <c>(oldValue, newValue)</c> — strings, exactly as
        /// stored. Safe to call before ModForge has loaded; the
        /// subscription is registered locally and fires whenever the
        /// upstream global event resolves and starts forwarding.
        /// </summary>
        public static void Subscribe(string packId, string varName, Action<string, string> handler)
        {
            if (handler == null || string.IsNullOrEmpty(packId) || string.IsNullOrEmpty(varName)) return;
            EnsureResolved();
            string key = packId + "|" + varName;
            if (!_subscribers.TryGetValue(key, out var list))
            {
                list = new List<Action<string, string>>(1);
                _subscribers[key] = list;
            }
            list.Add(handler);
        }

        /// <summary>Remove a previously-registered subscription. No-op if not found.</summary>
        public static void Unsubscribe(string packId, string varName, Action<string, string> handler)
        {
            if (handler == null) return;
            string key = packId + "|" + varName;
            if (_subscribers.TryGetValue(key, out var list))
                list.Remove(handler);
        }

        // ── Internals ─────────────────────────────────────────────────────

        /// <summary>
        /// Lazy one-shot resolution. Scans every loaded assembly for the
        /// ModForge Plugin type. Once found, caches the type, the
        /// singleton instance, every public-API <see cref="MethodInfo"/>,
        /// and attaches a single static handler to
        /// <c>OnPackVariableChanged</c> that fans out to per-key
        /// subscribers. Subsequent calls are O(1).
        /// </summary>
        private static void EnsureResolved()
        {
            // Stage 1: one-shot type + method resolution.
            if (!_typeResolved)
            {
                _typeResolved = true;
                try
                {
                    foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
                    {
                        _pluginType = asm.GetType(PluginTypeName, false, false);
                        if (_pluginType != null) break;
                    }
                    if (_pluginType == null) return;

                    _instanceProperty = _pluginType.GetProperty(InstancePropertyName,
                        BindingFlags.Public | BindingFlags.Static);

                    ResolveMethods();
                    TryAttachChangeEvent();
                }
                catch (Exception ex)
                {
                    Debug.LogWarning("[ModForgeBridge] Stage 1 resolution failed: " + ex.Message);
                }
            }

            // Stage 2: re-fetch instance until it shows up. Cheap
            // (single property read) — and gracefully handles the case
            // where SMSAndroids tried to use the bridge before ModForge
            // reached its own Awake.
            if (_pluginInstance == null && _instanceProperty != null)
            {
                try { _pluginInstance = _instanceProperty.GetValue(null, null); }
                catch { /* swallow — try again next call */ }
            }
        }

        private static void ResolveMethods()
        {
            _hasPack                = _pluginType.GetMethod("HasPack", new[] { typeof(string) });
                _getPackActiveSlot      = _pluginType.GetMethod("GetPackActiveSlot", new[] { typeof(string) });
                _flushPackToDisk        = _pluginType.GetMethod("FlushPackToDisk", new[] { typeof(string) });
                _hasPackVariable        = _pluginType.GetMethod("HasPackVariable", new[] { typeof(string), typeof(string) });
                _enumeratePackVariables = _pluginType.GetMethod("EnumeratePackVariables", new[] { typeof(string) });

                _getString = _pluginType.GetMethod("GetPackVariableString", new[] { typeof(string), typeof(string), typeof(string) });
                _getBool   = _pluginType.GetMethod("GetPackVariableBool",   new[] { typeof(string), typeof(string), typeof(bool) });
                _getInt    = _pluginType.GetMethod("GetPackVariableInt",    new[] { typeof(string), typeof(string), typeof(int) });
                _getFloat  = _pluginType.GetMethod("GetPackVariableFloat",  new[] { typeof(string), typeof(string), typeof(float) });
                _getList   = _pluginType.GetMethod("GetPackVariableList",   new[] { typeof(string), typeof(string) });

                _setString = _pluginType.GetMethod("SetPackVariable",       new[] { typeof(string), typeof(string), typeof(string) });
                _setBool   = _pluginType.GetMethod("SetPackVariableBool",   new[] { typeof(string), typeof(string), typeof(bool) });
                _setInt    = _pluginType.GetMethod("SetPackVariableInt",    new[] { typeof(string), typeof(string), typeof(int) });
                _setFloat  = _pluginType.GetMethod("SetPackVariableFloat",  new[] { typeof(string), typeof(string), typeof(float) });

            _addToList      = _pluginType.GetMethod("AddToPackList",      new[] { typeof(string), typeof(string), typeof(string) });
            _removeFromList = _pluginType.GetMethod("RemoveFromPackList", new[] { typeof(string), typeof(string), typeof(string) });
            _clearList      = _pluginType.GetMethod("ClearPackList",      new[] { typeof(string), typeof(string) });
            _isAnyDialoguePlaying = _pluginType.GetMethod("IsAnyDialoguePlaying", Type.EmptyTypes);
            Debug.Log("[ModForgeBridge] Methods resolved on SMSModForge.PackPlugin.Plugin.");
        }

        /// <summary>
        /// Attach our static forwarder to ModForge's
        /// <c>OnPackVariableChanged</c> event. The event handler type
        /// is <c>System.Action&lt;string,string,string,string&gt;</c>;
        /// <see cref="Delegate.CreateDelegate(Type, MethodInfo)"/> gives
        /// us a delegate of the exact event type around the static
        /// <see cref="OnAnyPackVariableChanged"/> method. Safe to call
        /// multiple times — guarded by <see cref="_eventAttached"/>.
        /// </summary>
        private static void TryAttachChangeEvent()
        {
            if (_eventAttached) return;
            _onPackVariableChangedEvent = _pluginType.GetEvent("OnPackVariableChanged",
                BindingFlags.Public | BindingFlags.Static);
            if (_onPackVariableChangedEvent == null) return;

            var fwd = typeof(ModForgeBridge).GetMethod(
                nameof(OnAnyPackVariableChanged),
                BindingFlags.NonPublic | BindingFlags.Static);
            _attachedForwarder = Delegate.CreateDelegate(
                _onPackVariableChangedEvent.EventHandlerType, fwd);
            _onPackVariableChangedEvent.AddEventHandler(null, _attachedForwarder);
            _eventAttached = true;
        }

        /// <summary>Static forwarder attached to ModForge's <c>OnPackVariableChanged</c>
        /// event. Looks up subscribers for <c>(packId, varName)</c> and dispatches
        /// the <c>(oldValue, newValue)</c> tuple to each.</summary>
        private static void OnAnyPackVariableChanged(string packId, string varName, string oldValue, string newValue)
        {
            string key = packId + "|" + varName;
            if (!_subscribers.TryGetValue(key, out var list) || list.Count == 0) return;
            // Snapshot so handlers can unsubscribe during dispatch safely.
            var snap = list.ToArray();
            for (int i = 0; i < snap.Length; i++)
            {
                try { snap[i](oldValue, newValue); }
                catch (Exception ex) { LogWarn("subscriber threw: " + ex.Message); }
            }
        }

        private static bool InvokeBool(MethodInfo m, params object[] args)
        {
            EnsureResolved();
            if (_pluginInstance == null || m == null) return false;
            try { return (bool)m.Invoke(_pluginInstance, args); }
            catch (Exception ex) { LogWarn(m.Name + " failed: " + ex.Message); return false; }
        }

        private static void LogWarn(string msg) => Debug.LogWarning("[ModForgeBridge] " + msg);
    }
}
