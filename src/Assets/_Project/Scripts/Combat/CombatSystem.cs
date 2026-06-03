using Fenrir.Entities.Enemies;
using Fenrir.Entities.Player;
using UnityEngine;

namespace Fenrir.Combat
{
    /// <summary>
    /// Scene-level coordinator. Hooks enemies into PlayerCombat on registration,
    /// emits trait signals on kill, and tracks per-type kill counts.
    /// </summary>
    public class CombatSystem : MonoBehaviour
    {
        private const string BossEnemyId = "emberlord";   // must match EnemyBase._enemyId

        // Lazy: player may not be in the scene when CombatSystem.Awake fires
        private PlayerCombat PlayerCombat =>
            _playerCombat != null ? _playerCombat
            : (_playerCombat = FindAnyObjectByType<PlayerCombat>());

        private PlayerCombat _playerCombat;

        // Key: enemyId, Value: consecutive kill count this session
        private readonly System.Collections.Generic.Dictionary<string, int> _killCounts = new();

        public void RegisterEnemy(EnemyBase enemy)
        {
            string id      = enemy.EnemyId;
            var health     = enemy.GetComponent<EnemyHealth>();
            var emitter    = enemy.GetComponent<EnemyTraitEmitter>();

            PlayerCombat?.EnterCombat(id);

            if (health != null)
                health.OnDied += () => HandleEnemyDeath(enemy, emitter);
        }

        private void HandleEnemyDeath(EnemyBase enemy, EnemyTraitEmitter emitter)
        {
            string id = enemy.EnemyId;

            _killCounts.TryGetValue(id, out int count);
            _killCounts[id] = count + 1;

            // Boss = specific enemy ID, not archetype (Elite = miniboss tier)
            bool isBoss = id == BossEnemyId;
            emitter?.OnKilledByPlayer(isBoss, id);

            if (_killCounts[id] >= 3)
                emitter?.OnHuntedRepeat();

            PlayerCombat?.ExitCombat(playerWon: true);
        }
    }
}
