using System;
using System.Collections.Generic;
using UnityEngine;

namespace Fenrir.Traits
{
    /// <summary>
    /// Typed event bus for behavioral signals.
    /// Supports Subscribe / Unsubscribe / Emit / Clear.
    /// Stores wrapper delegates so Unsubscribe can match by original handler identity.
    /// </summary>
    public static class BehaviorEventBus
    {
        // Outer key: event Type. Inner key: original delegate (boxed). Value: wrapper.
        private static readonly Dictionary<Type, Dictionary<object, Action<BehaviorEvent>>> _handlers = new();

        public static void Subscribe<T>(Action<T> handler) where T : BehaviorEvent
        {
            Type type = typeof(T);
            if (!_handlers.ContainsKey(type))
                _handlers[type] = new Dictionary<object, Action<BehaviorEvent>>();

            if (_handlers[type].ContainsKey(handler)) return;   // already subscribed

            Action<BehaviorEvent> wrapper = evt => handler((T)evt);
            _handlers[type][handler] = wrapper;
        }

        public static void Unsubscribe<T>(Action<T> handler) where T : BehaviorEvent
        {
            Type type = typeof(T);
            if (_handlers.TryGetValue(type, out var map))
                map.Remove(handler);
        }

        public static void Emit<T>(T evt) where T : BehaviorEvent
        {
            Type type = typeof(T);
            if (!_handlers.TryGetValue(type, out var map)) return;

            foreach (var wrapper in new List<Action<BehaviorEvent>>(map.Values))
            {
                try { wrapper(evt); }
                catch (Exception ex)
                {
                    Debug.LogError($"[BehaviorEventBus] Handler error for {type.Name}: {ex}");
                }
            }
        }

        /// <summary>Call on scene unload to prevent stale references.</summary>
        public static void Clear() => _handlers.Clear();
    }
}
