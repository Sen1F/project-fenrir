using Fenrir.Entities.Enemies;
using Fenrir.Entities.Player;
using Fenrir.Traits;
using UnityEngine;

namespace Fenrir.Combat
{
    /// <summary>
    /// Scene-level coordinator. Tracks active combats, enemy-kill counts per type,
    /// and emits high-level signals (combat started, enemy defeated, area cleared).
    /// Attach to the scene root or a persistent manager object.
    /// </summary>
    public class CombatSystem : MonoBehaviour
    {
        private PlayerCombat _playerCombat;

        // Key: enemyId, Value: consecutive kill count this session
        private readonly System.Collections.Generic.Dictionary<string, int> _killCounts = new();

        private void Awake()
        {
            _playerCombat = FindAnyObjectByType<PlayerCombat>();
        }

        /// <summary>
        /// Called when an enemy spawns into an encounter.
        /// Hooks up death listener and notifies PlayerCombat.
        /// </summary>
        public void RegisterEnemy(EnemyBase enemy)
        {
            string id = enemy.EnemyId;
            var health  = enemy.GetComponent<EnemyHealth>();
            var emitter = enemy.GetComponent<EnemyTraitEmitter>();

            _playerCombat?.EnterCombat(id);

            if (health != null)
            {
                health.OnDied += () => HandleEnemyDeath(enemy, emitter);
            }
        }

        private void HandleEnemyDeath(EnemyBase enemy, EnemyTraitEmitter emitter)
        {
            string id = enemy.EnemyId;

            _killCounts.TryGetValue(id, out int count);
            _killCounts[id] = count + 1;

            bool isBoss = enemy.Archetype == EnemyArchetype.Elite;
            emitter?.OnKilledByPlayer(isBoss);

            if (_killCounts[id] >= 3)
                emitter?.OnHuntedRepeat();

            _playerCombat?.ExitCombat(playerWon: true);
        }
    }
}
