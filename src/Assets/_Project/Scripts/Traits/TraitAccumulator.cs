using System;
using System.Collections.Generic;
using Fenrir.Config;
using Fenrir.Evolution;
using UnityEngine;

namespace Fenrir.Traits
{
    public class TraitAccumulator : ITraitAccumulator
    {
        public TraitProfile Profile { get; private set; }

        private TraitWeightsConfig _weights;
        private IEvolutionChecker _evolutionChecker;

        public TraitAccumulator(TraitProfile profile, TraitWeightsConfig weights, IEvolutionChecker checker)
        {
            Profile = profile;
            _weights = weights;
            _evolutionChecker = checker;
        }

        public void Process(BehaviorEvent evt)
        {
            if (!_weights.Weights.TryGetValue(evt.EventKey, out Dictionary<string, float> deltas))
                return;

            float dampen = ComputeDampeningFactor(evt.EventKey);
            Profile.IncrementSessionCount(evt.EventKey);

            foreach (KeyValuePair<string, float> entry in deltas)
            {
                if (!Enum.TryParse(entry.Key, true, out TraitKey key)) continue;
                Profile.Apply(key, entry.Value * dampen);
            }

            Profile.LastUpdated = DateTime.UtcNow;
        }

        public void ApplyDecay(float daysSinceLastPlay)
        {
            // Disabled in MVP — see GameConfig comment
            // foreach (TraitKey key in Enum.GetValues(typeof(TraitKey)))
            // {
            //     float current = Profile.Get(key);
            //     float decayed = current + (GameConfig.TraitNeutral - current)
            //                              * GameConfig.TraitDecayRatePerDay * daysSinceLastPlay;
            //     Profile.Set(key, decayed);
            // }
        }

        public EvolutionCandidate[] CheckEligibility(Config.Element element) =>
            _evolutionChecker.Check(Profile, element);

        // ── Private ──────────────────────────────────────────────────────────

        private float ComputeDampeningFactor(string eventKey)
        {
            int count = Profile.GetSessionCount(eventKey);
            if (count < GameConfig.FrequencyDampenAfter) return 1f;

            // Halves every FrequencyDampenAfter occurrences
            int steps = (count - GameConfig.FrequencyDampenAfter) / GameConfig.FrequencyDampenAfter;
            return Mathf.Pow(0.5f, steps + 1);
        }
    }
}
