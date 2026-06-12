using System;
using System.Collections.Generic;
using Fenrir.Traits;

namespace Fenrir.Evolution
{
    [Serializable]
    public class TraitThreshold
    {
        public float? Min;
        public float? Max;

        public bool IsMet(float value)
        {
            if (Min.HasValue && value < Min.Value) return false;
            if (Max.HasValue && value > Max.Value) return false;
            return true;
        }

        /// <summary>How far this value exceeds (or undercuts) the threshold — used for fit scoring.</summary>
        public float FitScore(float value)
        {
            if (Min.HasValue) return value - Min.Value;
            if (Max.HasValue) return Max.Value - value;
            return 0f;
        }
    }

    [Serializable]
    public class EvolutionSignature
    {
        public string Element;
        public Dictionary<string, TraitThreshold> Thresholds = new();
    }

    [Serializable]
    public class EvolutionSignaturesConfig
    {
        /// <summary>Key = evolutionId (e.g. "inferno")</summary>
        public Dictionary<string, EvolutionSignature> Signatures = new();
    }
}
