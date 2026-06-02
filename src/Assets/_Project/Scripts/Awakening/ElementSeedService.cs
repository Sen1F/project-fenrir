using Fenrir.Config;
using Fenrir.Save;

namespace Fenrir.Awakening
{
    public class ElementSeedService
    {
        public Element GetSlotElement(int slotIndex)
        {
            var seed = KeychainBridge.GetOrCreateSeed(slotIndex);
            return ElementDistribution.Derive(seed);
        }

        /// <summary>
        /// Rolls a new element for a reroll, excluding the current one.
        /// Reroll still uses the full distribution — the player may land on
        /// the same tier (just not the identical element).
        /// </summary>
        public Element Reroll(Element current)
        {
            Element result;
            int attempts = 0;
            do
            {
                result = ElementDistribution.Derive(System.Guid.NewGuid());
                attempts++;
            }
            while (result == current && attempts < 20);

            return result;
        }
    }
}
