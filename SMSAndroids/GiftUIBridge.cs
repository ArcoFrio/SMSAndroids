using GameCreator.Runtime.Common;
using UnityEngine;

namespace SMSAndroidsCore
{
    /// <summary>
    /// Routes the GC2 signals <c>SMSAndroidsCore.OpenGiftUI</c> and
    /// <c>SMSAndroidsCore.OpenGiftStore</c> into the existing SMSAndroids
    /// gift-giving + gift-shop UIs. Lets ModForge packs open these UIs via a
    /// plain <c>EmitSignal</c> action — no direct cross-plugin call required.
    /// The names are namespaced with the plugin's assembly so they can't collide
    /// with a custom signal another pack might use; the pack's <c>EmitSignal</c>
    /// actions must emit these exact strings.
    /// <para/>
    /// The bridge is a tiny singleton implementing <see cref="ISignalReceiver"/>.
    /// One instance is created on first <see cref="EnsureSubscribed"/> call and
    /// stays subscribed for the lifetime of the process. Re-subscription is
    /// idempotent (<see cref="Signals.Subscribe"/> rejects duplicates), so it's
    /// safe to call from the per-frame loaders that fire on scene change.
    /// <para/>
    /// We deliberately do nothing else here — the actual UI activation paths
    /// live in <see cref="Dialogues.giftUI"/> / <see cref="Places.giftStore"/>
    /// and stay unchanged. This file is the routing layer only.
    /// </summary>
    internal sealed class GiftUIBridge : ISignalReceiver
    {
        // PropertyNames are interned by content, so these are cheap and
        // round-trip equal to anything constructed with the same string.
        private static readonly PropertyName OpenGiftUISignal    = new PropertyName("SMSAndroidsCore.OpenGiftUI");
        private static readonly PropertyName OpenGiftStoreSignal = new PropertyName("SMSAndroidsCore.OpenGiftStore");

        private static GiftUIBridge _instance;
        private static bool _subscribed;

        /// <summary>Idempotent. First call constructs the singleton + subscribes;
        /// later calls are O(1) no-ops. Safe to call from any per-frame load
        /// path; the cost after the first hit is one bool read.</summary>
        public static void EnsureSubscribed()
        {
            if (_subscribed) return;
            if (_instance == null) _instance = new GiftUIBridge();
            Signals.Subscribe(_instance, OpenGiftUISignal);
            Signals.Subscribe(_instance, OpenGiftStoreSignal);
            _subscribed = true;
            Debug.Log("[GiftUIBridge] Subscribed to SMSAndroidsCore.OpenGiftUI + SMSAndroidsCore.OpenGiftStore signals.");
        }

        /// <summary>GC2 callback. Dispatch on signal name to the matching
        /// activation path. Unknown signals are ignored (we only subscribed
        /// to two, so this branch can't actually fire — but cheap insurance).</summary>
        void ISignalReceiver.OnReceiveSignal(SignalArgs args)
        {
            if (args.signal == OpenGiftUISignal)
            {
                ShowGiftUI();
            }
            else if (args.signal == OpenGiftStoreSignal)
            {
                ShowGiftStore();
            }
        }

        private static void ShowGiftUI()
        {
            if (Dialogues.giftUI == null)
            {
                Debug.LogWarning("[GiftUIBridge] OpenGiftUI received but Dialogues.giftUI is null " +
                    "(probably fired before the load chain reached CoreGameScene).");
                return;
            }
            Dialogues.giftUI.SetActive(true);
        }

        private static void ShowGiftStore()
        {
            if (Places.giftStore == null)
            {
                Debug.LogWarning("[GiftUIBridge] OpenGiftStore received but Places.giftStore is null " +
                    "(probably fired before Places.loadedPlaces).");
                return;
            }
            Places.ActivateShop(Places.giftStore);
        }
    }
}
