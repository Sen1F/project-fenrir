using System;
using System.Collections.Generic;
using NUnit.Framework;
using Fenrir.Awakening;
using Fenrir.Config;

namespace Fenrir.Tests.EditMode
{
    public class ElementDistributionTests
    {
        [Test]
        public void SameSeed_AlwaysProducesSameElement()
        {
            Guid seed = Guid.NewGuid();
            Element first  = ElementDistribution.Derive(seed);
            Element second = ElementDistribution.Derive(seed);
            Assert.AreEqual(first, second);
        }

        [Test]
        public void DifferentSeeds_ProduceDiverseElements()
        {
            var seen = new HashSet<Element>();
            for (int i = 0; i < 1000; i++)
                seen.Add(ElementDistribution.Derive(Guid.NewGuid()));

            // In MVP, only Common elements (Fire/Water/Earth/Air) are active.
            // We should see all four given enough samples.
            Assert.GreaterOrEqual(seen.Count, 4,
                "At least 4 distinct elements expected from 1000 seeds.");
        }

        [Test]
        public void CommonBand_ContainsAllFourElements()
        {
            // Force seeds into the Common band by constructing GUIDs
            // whose first 8 bytes yield a value below 0.930 * long.MaxValue.
            // Simpler: just use enough random seeds — statistically certain at N=5000.
            var seen = new HashSet<Element>();
            for (int i = 0; i < 5000; i++)
                seen.Add(ElementDistribution.Derive(Guid.NewGuid()));

            Assert.IsTrue(seen.Contains(Element.Fire),  "Fire missing from distribution");
            Assert.IsTrue(seen.Contains(Element.Water), "Water missing from distribution");
            Assert.IsTrue(seen.Contains(Element.Earth), "Earth missing from distribution");
            Assert.IsTrue(seen.Contains(Element.Air),   "Air missing from distribution");
        }

        [Test]
        public void GuidEmpty_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => ElementDistribution.Derive(Guid.Empty));
        }

        [Test]
        public void Result_IsDefinedElement()
        {
            for (int i = 0; i < 100; i++)
            {
                Element result = ElementDistribution.Derive(Guid.NewGuid());
                Assert.IsTrue(Enum.IsDefined(typeof(Element), result),
                    $"Got undefined Element value: {result}");
                Assert.AreNotEqual(Element.None, result,
                    "Distribution must never return Element.None");
            }
        }
    }
}
