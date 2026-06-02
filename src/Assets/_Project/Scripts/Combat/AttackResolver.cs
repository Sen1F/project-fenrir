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
        private const float SameElementMod     = 0.75f;   // −25%
        private const float OppositeElementMod = 1.25f;   // +25%

        public static float Resolve(AttackData attack, Element defenderElement)
        {
            float damage = attack.BaseDamage;

            if (attack.IsAbility)
            {
                Element atk = attack.AttackerElement;
                Element def = defenderElement;

                if (atk == def)
                {
                    damage *= SameElementMod;
                }
                else if (ElementExtensions.AreOpposites(atk, def))
                {
                    damage *= OppositeElementMod;
                }
            }

            return Mathf.Max(0f, damage);
        }
    }
}
