using Fenrir.Evolution;

namespace Fenrir.Traits
{
    public interface ITraitAccumulator
    {
        void Process(BehaviorEvent evt);
        void ApplyDecay(float daysSinceLastPlay);
        EvolutionCandidate[] CheckEligibility(Config.Element element);
        TraitProfile Profile { get; }
    }
}
