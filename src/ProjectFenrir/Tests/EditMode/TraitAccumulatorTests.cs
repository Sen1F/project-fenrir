using NUnit.Framework;
using Fenrir.Config;
using Fenrir.Evolution;
using Fenrir.Traits;

namespace Fenrir.Tests.EditMode
{
    public class TraitAccumulatorTests
    {
        private TraitProfile         _profile;
        private TraitWeightsConfig   _weights;
        private IEvolutionChecker    _checker;
        private TraitAccumulator     _accumulator;

        [SetUp]
        public void SetUp()
        {
            _profile     = TraitProfile.Neutral();
            _weights     = new TraitWeightsConfig();
            _checker     = new StubEvolutionChecker();
            _accumulator = new TraitAccumulator(_profile, _weights, _checker);
        }

        [Test]
        public void DodgeUsedEvent_IncreasesPatience()
        {
            float before = _profile.Get(TraitKey.Patience);
            _accumulator.Process(new DodgeUsedEvent());
            Assert.Greater(_profile.Get(TraitKey.Patience), before);
        }

        [Test]
        public void DodgeUsedEvent_DecreasesAggression()
        {
            float before = _profile.Get(TraitKey.Aggression);
            _accumulator.Process(new DodgeUsedEvent());
            Assert.Less(_profile.Get(TraitKey.Aggression), before);
        }

        [Test]
        public void TraitValues_NeverExceedMax()
        {
            for (int i = 0; i < 100; i++)
                _accumulator.Process(new CounterLandedEvent());

            Assert.LessOrEqual(_profile.Get(TraitKey.Dominance), GameConfig.TraitMax);
        }

        [Test]
        public void TraitValues_NeverGoBelowMin()
        {
            for (int i = 0; i < 100; i++)
                _accumulator.Process(new DialoguePunishmentEvent());

            Assert.GreaterOrEqual(_profile.Get(TraitKey.Mercy), GameConfig.TraitMin);
        }

        [Test]
        public void FrequencyDampening_ReducesDeltaAfterThreshold()
        {
            // First N events should accumulate normally
            for (int i = 0; i < GameConfig.FrequencyDampenAfter; i++)
                _accumulator.Process(new DodgeUsedEvent());

            float afterNormal = _profile.Get(TraitKey.Patience);
            float normalDeltaPerEvent = (afterNormal - GameConfig.TraitNeutral) / GameConfig.FrequencyDampenAfter;

            // Next event should be dampened (contribute less than the average delta)
            _accumulator.Process(new DodgeUsedEvent());
            float afterDampened = _profile.Get(TraitKey.Patience);
            float dampenedDelta = afterDampened - afterNormal;

            Assert.Less(dampenedDelta, normalDeltaPerEvent,
                "Dampened event should contribute less than the un-dampened average.");
        }

        [Test]
        public void UnknownEventKey_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => _accumulator.Process(new UnmappedTestEvent()));
        }

        [Test]
        public void ApplyEvolutionCarryover_ShiftsTraitsTowardNeutral()
        {
            _profile.Set(TraitKey.Aggression, 90f);
            _profile.ApplyEvolutionCarryover();
            float result = _profile.Get(TraitKey.Aggression);
            Assert.Less(result, 90f, "Aggression should shift toward neutral after evolution.");
            Assert.Greater(result, GameConfig.TraitNeutral, "Aggression should stay above neutral given high starting value.");
        }

        [Test]
        public void SessionEventCounts_ResetAfterEvolutionCarryover()
        {
            _accumulator.Process(new DodgeUsedEvent());
            Assert.Greater(_profile.GetSessionCount("DodgeUsedEvent"), 0);
            _profile.ApplyEvolutionCarryover();
            Assert.AreEqual(0, _profile.GetSessionCount("DodgeUsedEvent"));
        }

        // ── Helpers ──────────────────────────────────────────────────────────

        private class UnmappedTestEvent : BehaviorEvent { }

        private class StubEvolutionChecker : IEvolutionChecker
        {
            public EvolutionCandidate[] Check(TraitProfile profile, Element element)
                => System.Array.Empty<EvolutionCandidate>();
        }
    }
}
