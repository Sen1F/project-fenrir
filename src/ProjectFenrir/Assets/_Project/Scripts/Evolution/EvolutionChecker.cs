using System;
using System.Collections.Generic;
using Fenrir.Traits;
using UnityEngine;

namespace Fenrir.Evolution
{
    public class EvolutionChecker : IEvolutionChecker
    {
        private readonly EvolutionSignaturesConfig _config;

        public EvolutionChecker(EvolutionSignaturesConfig config)
        {
            _config = config;
        }

        public EvolutionCandidate[] Check(TraitProfile profile, Config.Element element)
        {
            string elementName = element.ToString().ToLower();
            var candidates = new List<EvolutionCandidate>();

            foreach (KeyValuePair<string, EvolutionSignature> pair in _config.Signatures)
            {
                string id = pair.Key;
                EvolutionSignature sig = pair.Value;

                if (!sig.Element.Equals(elementName, StringComparison.OrdinalIgnoreCase))
                    continue;

                float fitScore = 0f;
                bool allMet = true;

                foreach (KeyValuePair<string, TraitThreshold> threshold in sig.Thresholds)
                {
                    if (!Enum.TryParse(threshold.Key, true, out TraitKey traitKey))
                    {
                        Debug.LogWarning($"[EvolutionChecker] Unknown trait key '{threshold.Key}' in signature '{id}'");
                        continue;
                    }

                    float value = profile.Get(traitKey);
                    if (!threshold.Value.IsMet(value))
                    {
                        allMet = false;
                        break;
                    }
                    fitScore += threshold.Value.FitScore(value);
                }

                if (allMet)
                    candidates.Add(new EvolutionCandidate(id, fitScore, element));
            }

            return candidates.ToArray();
        }
    }
}
