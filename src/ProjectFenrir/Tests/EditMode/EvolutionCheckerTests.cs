using NUnit.Framework;
using Fenrir.Config;
using Fenrir.Evolution;
using Fenrir.Traits;

namespace Fenrir.Tests.EditMode
{
    public class EvolutionCheckerTests
    {
        private EvolutionChecker _checker;

        [SetUp]
        public void SetUp()
        {
            // Minimal config with just the Fire → Inferno signature
            var config = new EvolutionSignaturesConfig();
            config.Signatures["inferno"] = new EvolutionSignature
            {
                Element = "Fire",
                Thresholds = new System.Collections.Generic.Dictionary<string, TraitThreshold>
                {
                    ["Aggression"]   = new TraitThreshold { Min = 70f },
                    ["Dominance"]    = new TraitThreshold { Min = 65f },
                    ["Mercy"]        = new TraitThreshold { Max = 35f },
                    ["Recklessness"] = new TraitThreshold { Min = 55f }
                }
            };

            _checker = new EvolutionChecker(config);
        }

        [Test]
        public void EligibleProfile_ReturnsInfernoCandidate()
        {
            TraitProfile profile = BuildInfernoProfile();
            EvolutionCandidate[] candidates = _checker.Check(profile, Element.Fire);

            Assert.AreEqual(1, candidates.Length);
            Assert.AreEqual("inferno", candidates[0].EvolutionId);
        }

        [Test]
        public void ProfileBelowAggression_ReturnsNoCandidates()
        {
            TraitProfile profile = BuildInfernoProfile();
            profile.Set(TraitKey.Aggression, 60f); // below required 70
            EvolutionCandidate[] candidates = _checker.Check(profile, Element.Fire);

            Assert.AreEqual(0, candidates.Length);
        }

        [Test]
        public void ProfileAboveMercyCap_ReturnsNoCandidates()
        {
            TraitProfile profile = BuildInfernoProfile();
            profile.Set(TraitKey.Mercy, 50f); // above max 35
            EvolutionCandidate[] candidates = _checker.Check(profile, Element.Fire);

            Assert.AreEqual(0, candidates.Length);
        }

        [Test]
        public void WrongElement_ReturnsNoCandidates()
        {
            TraitProfile profile = BuildInfernoProfile();
            EvolutionCandidate[] candidates = _checker.Check(profile, Element.Water);

            Assert.AreEqual(0, candidates.Length);
        }

        [Test]
        public void ExactThresholdValues_AreEligible()
        {
            TraitProfile profile = TraitProfile.Neutral();
            profile.Set(TraitKey.Aggression,   70f); // exactly at Min
            profile.Set(TraitKey.Dominance,    65f);
            profile.Set(TraitKey.Mercy,        35f); // exactly at Max
            profile.Set(TraitKey.Recklessness, 55f);

            EvolutionCandidate[] candidates = _checker.Check(profile, Element.Fire);
            Assert.AreEqual(1, candidates.Length);
        }

        [Test]
        public void FitScore_IsPositiveForEligibleProfile()
        {
            TraitProfile profile = BuildInfernoProfile();
            EvolutionCandidate[] candidates = _checker.Check(profile, Element.Fire);
            Assert.Greater(candidates[0].FitScore, 0f);
        }

        // ── Helpers ──────────────────────────────────────────────────────────

        private static TraitProfile BuildInfernoProfile()
        {
            TraitProfile profile = TraitProfile.Neutral();
            profile.Set(TraitKey.Aggression,   80f);
            profile.Set(TraitKey.Dominance,    75f);
            profile.Set(TraitKey.Mercy,        20f);
            profile.Set(TraitKey.Recklessness, 70f);
            return profile;
        }
    }
}
