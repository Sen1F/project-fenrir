using Fenrir.Traits;
using UnityEngine;

namespace Fenrir.Analytics
{
    /// <summary>
    /// Lightweight event logger. Listens to BehaviorEventBus and writes to
    /// Unity console in development. Post-MVP: route to analytics backend.
    /// </summary>
    public class EventLogger
    {
        private readonly bool _verbose;

        public EventLogger(bool verbose = false)
        {
            _verbose = verbose;
            SubscribeAll();
        }

        private void SubscribeAll()
        {
            // Log every BehaviorEvent type in development
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            BehaviorEventBus.Subscribe<DodgeUsedEvent>(LogEvent);
            BehaviorEventBus.Subscribe<CounterLandedEvent>(LogEvent);
            BehaviorEventBus.Subscribe<DeathRecklessEvent>(LogEvent);
            BehaviorEventBus.Subscribe<DeathSacrificeEvent>(LogEvent);
            BehaviorEventBus.Subscribe<DeathOverwhelmedEvent>(LogEvent);
            BehaviorEventBus.Subscribe<DeathPatternFailEvent>(LogEvent);
            BehaviorEventBus.Subscribe<CreatureSparedEvent>(LogEvent);
            BehaviorEventBus.Subscribe<SecretAreaDiscoveredEvent>(LogEvent);
            BehaviorEventBus.Subscribe<EvolutionCompleteEvent>(e =>
                Debug.Log($"[EventLogger] ★ EVOLUTION: {e.EvolutionId}"));
            BehaviorEventBus.Subscribe<BossKilledEvent>(e =>
                Debug.Log($"[EventLogger] ★ BOSS DEFEATED: {e.BossId}"));
#endif
        }

        private void LogEvent(BehaviorEvent evt)
        {
            if (_verbose)
                Debug.Log($"[EventLogger] {evt.EventKey}");
        }
    }
}
