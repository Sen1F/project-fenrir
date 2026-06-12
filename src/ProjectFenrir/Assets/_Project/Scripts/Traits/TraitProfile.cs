using System;
using System.Collections.Generic;
using Fenrir.Config;

namespace Fenrir.Traits
{
    [Serializable]
    public class TraitProfile
    {
        public Dictionary<TraitKey, float> Traits = new();
        public Dictionary<string, int> SessionEventCounts = new();
        /// <summary>Unix timestamp. JsonUtility cannot serialize System.DateTime.</summary>
        public long LastUpdatedUtc = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        public static TraitProfile Neutral()
        {
            var profile = new TraitProfile();
            foreach (TraitKey key in Enum.GetValues(typeof(TraitKey)))
                profile.Traits[key] = GameConfig.TraitNeutral;
            return profile;
        }

        public float Get(TraitKey key) =>
            Traits.TryGetValue(key, out float value) ? value : GameConfig.TraitNeutral;

        public void Set(TraitKey key, float value) =>
            Traits[key] = Math.Clamp(value, GameConfig.TraitMin, GameConfig.TraitMax);

        public void Apply(TraitKey key, float delta) =>
            Set(key, Get(key) + delta);

        /// <summary>
        /// Shifts all trait values 30% toward neutral on evolution.
        /// </summary>
        public void ApplyEvolutionCarryover()
        {
            foreach (TraitKey key in Enum.GetValues(typeof(TraitKey)))
            {
                float current = Get(key);
                float shifted = current + (GameConfig.TraitNeutral - current) * GameConfig.TraitEvolutionCarryover;
                Set(key, shifted);
            }
            SessionEventCounts.Clear();
        }

        public int GetSessionCount(string eventKey)
        {
            SessionEventCounts.TryGetValue(eventKey, out int count);
            return count;
        }

        public void IncrementSessionCount(string eventKey) =>
            SessionEventCounts[eventKey] = GetSessionCount(eventKey) + 1;
    }
}
