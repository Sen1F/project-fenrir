using Fenrir.Config;
using Fenrir.Core;
using Fenrir.Traits;
using System.Text;
using UnityEngine;

namespace Fenrir.Analytics
{
    /// <summary>
    /// Listens to BehaviorEventBus and logs events to the Unity console.
    /// In development builds: also shows which traits changed and by how much,
    /// so "dodge → Patience +2.0" is visible in the Console during play testing.
    /// Post-MVP: route significant events to analytics backend.
    /// </summary>
    public class EventLogger
    {
        private readonly bool _verbose;

        public EventLogger(bool verbose = false)
        {
            _verbose = verbose;
            Resubscribe();
        }

        /// <summary>
        /// (Re)attaches all log handlers. Called from the constructor and again by
        /// Bootstrap after every full scene transition, because SceneRouter clears
        /// the BehaviorEventBus on each transition.
        /// </summary>
        public void Resubscribe()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            BehaviorEventBus.Subscribe<DodgeUsedEvent>(e             => Log(e));
            BehaviorEventBus.Subscribe<CounterLandedEvent>(e         => Log(e));
            BehaviorEventBus.Subscribe<PerfectBlockEvent>(e          => Log(e));
            BehaviorEventBus.Subscribe<BlockUsedEvent>(e             => Log(e));
            BehaviorEventBus.Subscribe<HeavyAttackLandedEvent>(e     => Log(e));
            BehaviorEventBus.Subscribe<LightAttackLandedEvent>(e     => Log(e));
            BehaviorEventBus.Subscribe<AbilityUsedFullEnergyEvent>(e => Log(e));
            BehaviorEventBus.Subscribe<AbilityUsedLowEnergyEvent>(e  => Log(e));
            BehaviorEventBus.Subscribe<HitTakenNoDodgeEvent>(e       => Log(e));
            BehaviorEventBus.Subscribe<CombatCompletedNoDodgeEvent>(e=> Log(e));

            BehaviorEventBus.Subscribe<DeathAmbushEvent>(e      => Log(e));
            BehaviorEventBus.Subscribe<DeathRecklessEvent>(e    => Log(e));
            BehaviorEventBus.Subscribe<DeathSacrificeEvent>(e   => Log(e));
            BehaviorEventBus.Subscribe<DeathOverwhelmedEvent>(e => Log(e));
            BehaviorEventBus.Subscribe<DeathPatternFailEvent>(e => Log(e));

            BehaviorEventBus.Subscribe<CreatureSparedEvent>(e       => Log(e));
            BehaviorEventBus.Subscribe<CreatureHuntedRepeatEvent>(e => Log(e));
            BehaviorEventBus.Subscribe<SecretAreaDiscoveredEvent>(e => Log(e));
            BehaviorEventBus.Subscribe<NightExplorationEvent>(e    => Log(e));
            BehaviorEventBus.Subscribe<NpcHelpedEvent>(e           => Log(e));
            BehaviorEventBus.Subscribe<NpcIgnoredEvent>(e          => Log(e));
            BehaviorEventBus.Subscribe<LoreObjectReadEvent>(e      => Log(e));
            BehaviorEventBus.Subscribe<QuestCompletedEvent>(e      => Log(e));

            BehaviorEventBus.Subscribe<EvolutionCompleteEvent>(e =>
                Debug.Log($"[EventLogger] ★ EVOLUTION: {e.EvolutionId}"));
            BehaviorEventBus.Subscribe<BossKilledEvent>(e =>
                Debug.Log($"[EventLogger] ★ BOSS DEFEATED: {e.BossId}"));
#endif
        }

        private void Log(BehaviorEvent evt)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (!_verbose) return;

            // Read the weight config to show which traits this event affects
            if (!ServiceLocator.TryGet<TraitWeightsConfig>(out TraitWeightsConfig weights))
            {
                Debug.Log($"[Traits] {evt.EventKey}");
                return;
            }

            if (!weights.Weights.TryGetValue(evt.EventKey, out var deltas))
            {
                Debug.Log($"[Traits] {evt.EventKey} (no weights mapped)");
                return;
            }

            // Read profile snapshot before accumulator processes for accurate before/after
            // (EventLogger fires from bus simultaneously with accumulator — order is not guaranteed,
            //  so we show intended deltas from config rather than live before/after diff)
            var sb = new StringBuilder($"[Traits] {evt.EventKey}");
            foreach (var pair in deltas)
            {
                float val = 0f;
                if (ServiceLocator.TryGet<ITraitAccumulator>(out ITraitAccumulator acc)
                    && acc is TraitAccumulator ta)
                {
                    System.Enum.TryParse(pair.Key, true, out TraitKey key);
                    val = ta.Profile.Get(key);
                }
                string sign  = pair.Value >= 0 ? "+" : "";
                sb.Append($"  {pair.Key}: {sign}{pair.Value:F1}  (now ≈{val:F0})");
            }
            Debug.Log(sb.ToString());
#endif
        }
    }
}
