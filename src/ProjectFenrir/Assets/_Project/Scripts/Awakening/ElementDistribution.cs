using System;
using Fenrir.Config;

namespace Fenrir.Awakening
{
    /// <summary>
    /// Derives an Element deterministically from a seed GUID.
    /// The same seed always produces the same element.
    ///
    /// In MVP, non-Common results fall back to Common.
    /// Full distribution activates when non-Common elements are implemented.
    /// </summary>
    public static class ElementDistribution
    {
        private const bool MvpCommonOnly = true; // flip to false when rare elements ship

        public static Element Derive(Guid seed)
        {
            // Convert first 8 bytes of GUID to a stable double in [0, 1)
            byte[] bytes = seed.ToByteArray();
            long raw = BitConverter.ToInt64(bytes, 0) & long.MaxValue;
            double roll = (double)raw / long.MaxValue;

            Element element = RollElement(roll);

            if (MvpCommonOnly && element.GetTier() != ElementTier.Common)
                element = RollCommon(roll);

            return element;
        }

        private static Element RollElement(double roll)
        {
            // Supreme: top 0.1%
            if (roll >= 0.999) return RollSupreme(roll);
            // Very Rare: next 0.9%
            if (roll >= 0.990) return RollVeryRare(roll);
            // Rare: next 6%
            if (roll >= 0.930) return RollRare(roll);
            // Common: bottom 93%
            return RollCommon(roll);
        }

        private static Element RollCommon(double roll)
        {
            double r = roll / 0.930; // normalise into [0,1) within Common band
            if (r < 0.25) return Element.Fire;
            if (r < 0.50) return Element.Water;
            if (r < 0.75) return Element.Earth;
            return Element.Air;
        }

        private static Element RollRare(double roll)
        {
            double r = (roll - 0.930) / 0.060;
            if (r < 0.25) return Element.Lightning;
            if (r < 0.50) return Element.Metal;
            if (r < 0.75) return Element.Ice;
            return Element.Nature;
        }

        private static Element RollVeryRare(double roll)
        {
            double r = (roll - 0.990) / 0.009;
            if (r < 0.333) return Element.Light;
            if (r < 0.666) return Element.Darkness;
            return Element.Shadow;
        }

        private static Element RollSupreme(double roll)
        {
            double r = (roll - 0.999) / 0.001;
            if (r < 0.25) return Element.Space;
            if (r < 0.50) return Element.Time;
            if (r < 0.75) return Element.Life;
            return Element.Death;
        }
    }
}
