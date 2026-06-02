using Fenrir.Traits;

namespace Fenrir.Evolution
{
    public interface IEvolutionChecker
    {
        EvolutionCandidate[] Check(TraitProfile profile, Config.Element element);
    }
}
