using System;
using System.Collections.Generic;
using UnityEngine;

namespace Fenrir.Traits
{
    /// <summary>
    /// Typed event bus for behavioral signals.
    /// Game systems emit events here; TraitAccumulator listens.
    /// No direct coupling between emitter and receiver.
    /// </summary>
    public static class BehaviorEventBus
    {
        private static readonly Dictionary<Type, List<Action<BehaviorEvent>>> _handlers = new();

        public static void Subscribe<T>(Action<T> handler) where T : BehaviorEvent
        {
            Type type = typeof(T);
            if (!_handlers.ContainsKey(type))
                _handlers[type] = new List<Action<BehaviorEvent>>();

            _handlers[type].Add(evt => handler((T)evt));
        }

        public static void Emit<T>(T evt) where T : BehaviorEvent
        {
            Type type = typeof(T);
            if (!_handlers.TryGetValue(type, out List<Action<BehaviorEvent>> handlers))
                return;

            // Iterate over a copy in case handlers modify the list
            foreach (Action<BehaviorEvent> handler in handlers.ToArray())
            {
                try { handler(evt); }
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
