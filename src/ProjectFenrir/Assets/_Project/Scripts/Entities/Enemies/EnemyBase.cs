using Fenrir.Config;
using UnityEngine;

namespace Fenrir.Entities.Enemies
{
    public enum EnemyArchetype
    {
        Aggressive,   // high damage, low HP
        Defensive,    // shields, blocks
        Ranged,       // projectile attacks
        Pack,         // weak alone, dangerous in numbers
        Elite,        // mini-boss tier, telegraphed heavy attacks
        Boss          // true boss — unique per zone, triggers BossKilledEvent
    }

    /// <summary>
    /// Base component shared by all enemy types.
    /// Holds shared data (ID, archetype, element).
    /// Specific AI behaviour lives in EnemyAI.
    /// </summary>
    public class EnemyBase : MonoBehaviour
    {
        [SerializeField] private string        _enemyId   = "enemy_unknown";
        [SerializeField] private EnemyArchetype _archetype = EnemyArchetype.Aggressive;
        [SerializeField] private Element        _element   = Element.Fire;

        public string         EnemyId   => _enemyId;
        public EnemyArchetype Archetype => _archetype;
        public Element        Element   => _element;
    }
}
