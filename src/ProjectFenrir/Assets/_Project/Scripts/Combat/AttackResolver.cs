using Fenrir.Config;
using UnityEngine;

namespace Fenrir.Combat
{
    /// <summary>
    /// Stateless helper that applies elemental resistances and returns final damage.
    /// Resistances apply to ability hits only (IsAbility == true).
    /// Same element: −25%.  Opposite element: +25%.
    /// </summary>
    public static class AttackResolver
    {
        // Values from GameConfig — single source of truth
        // GameConfig.ElementalResistanceMod = -0.25f → multiplier = 1 + (-0.25) = 0.75
        // GameConfig.ElementalWeaknessMod   = +0.25f → multiplier = 1 + (+0.25) = 1.25
        private static float SameElementMult     => 1f + GameConfig.ElementalResistanceMod;
        private static float OppositeElementMult => 1f + GameConfig.ElementalWeaknessMod;

        public static float Resolve(AttackData attack, Element defenderElement)
        {
            float damage = attack.BaseDamage;

            if (attack.IsAbility)
            {
                Element atk = attack.AttackerElement;
                Element def = defenderElement;

                if (atk == def)
                    damage *= SameElementMult;
                else if (ElementExtensions.AreOpposites(atk, def))
                    damage *= OppositeElementMult;
            }

            return Mathf.Max(0f, damage);
        }
    }
}
