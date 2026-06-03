namespace Fenrir.Evolution
{
    public class EvolutionCandidate
    {
        public string EvolutionId { get; }
        public float FitScore { get; }        // higher = better match
        public Config.Element RequiredElement { get; }

        public EvolutionCandidate(string id, float fitScore, Config.Element element)
        {
            EvolutionId      = id;
            FitScore         = fitScore;
            RequiredElement  = element;
        }
    }
}
