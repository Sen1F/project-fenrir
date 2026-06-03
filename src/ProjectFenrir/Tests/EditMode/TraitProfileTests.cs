using NUnit.Framework;
using Fenrir.Config;
using Fenrir.Traits;

namespace Fenrir.Tests.EditMode
{
    public class TraitProfileTests
    {
        [Test]
        public void Neutral_SetsAllTraitsToFifty()
        {
            TraitProfile profile = TraitProfile.Neutral();
            foreach (TraitKey key in System.Enum.GetValues(typeof(TraitKey)))
                Assert.AreEqual(GameConfig.TraitNeutral, profile.Get(key), 0.001f,
                    $"Expected neutral value for {key}");
        }

        [Test]
        public void Set_ClampsAboveMax()
        {
            TraitProfile profile = TraitProfile.Neutral();
            profile.Set(TraitKey.Aggression, 200f);
            Assert.AreEqual(GameConfig.TraitMax, profile.Get(TraitKey.Aggression), 0.001f);
        }

        [Test]
        public void Set_ClampsBelowMin()
        {
            TraitProfile profile = TraitProfile.Neutral();
            profile.Set(TraitKey.Mercy, -50f);
            Assert.AreEqual(GameConfig.TraitMin, profile.Get(TraitKey.Mercy), 0.001f);
        }

        [Test]
        public void Apply_AccumulatesDelta()
        {
            TraitProfile profile = TraitProfile.Neutral();
            profile.Apply(TraitKey.Curiosity, 10f);
            Assert.AreEqual(60f, profile.Get(TraitKey.Curiosity), 0.001f);
        }

        [Test]
        public void Get_ReturnsNeutralForUnsetKey()
        {
            TraitProfile profile = new TraitProfile();
            Assert.AreEqual(GameConfig.TraitNeutral, profile.Get(TraitKey.Wisdom), 0.001f,
                "Missing key should return neutral, not 0");
        }

        [Test]
        public void ApplyEvolutionCarryover_ShiftsAllTraits()
        {
            TraitProfile profile = TraitProfile.Neutral();
            profile.Set(TraitKey.Aggression, 100f);
            profile.Set(TraitKey.Patience,     0f);
            profile.ApplyEvolutionCarryover();

            Assert.Less(profile.Get(TraitKey.Aggression), 100f);
            Assert.Greater(profile.Get(TraitKey.Patience),   0f);
        }

        [Test]
        public void SessionEventCounts_IncrementCorrectly()
        {
            TraitProfile profile = TraitProfile.Neutral();
            Assert.AreEqual(0, profile.GetSessionCount("testEvent"));
            profile.IncrementSessionCount("testEvent");
            profile.IncrementSessionCount("testEvent");
            Assert.AreEqual(2, profile.GetSessionCount("testEvent"));
        }

        [Test]
        public void SessionEventCounts_ClearAfterCarryover()
        {
            TraitProfile profile = TraitProfile.Neutral();
            profile.IncrementSessionCount("someEvent");
            profile.ApplyEvolutionCarryover();
            Assert.AreEqual(0, profile.GetSessionCount("someEvent"));
        }
    }
}
