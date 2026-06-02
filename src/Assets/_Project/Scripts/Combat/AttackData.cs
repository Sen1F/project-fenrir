using Fenrir.Config;

namespace Fenrir.Combat
{
    public enum AttackType { Light, Heavy, Ability }

    /// <summary>
    /// Immutable descriptor produced by PlayerCombat or EnemyCombat and consumed by AttackResolver.
    /// </summary>
    public readonly struct AttackData
    {
        public readonly AttackType  Type;
        public readonly float       BaseDamage;
        public readonly Element     AttackerElement;
        public readonly bool        IsCounter;       // landed via perfect-block window
        public readonly bool        IsAbility;

        public AttackData(AttackType type, float baseDamage, Element element,
                          bool isCounter = false, bool isAbility = false)
        {
            Type            = type;
            BaseDamage      = baseDamage;
            AttackerElement = element;
            IsCounter       = isCounter;
            IsAbility       = isAbility;
        }
    }
}
