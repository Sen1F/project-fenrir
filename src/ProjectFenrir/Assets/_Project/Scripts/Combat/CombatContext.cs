using System.Collections.Generic;
using Fenrir.Audio;
using Fenrir.Core;
using Fenrir.Entities.Enemies;
using Fenrir.Save;
using Fenrir.StateMachine;
using Fenrir.Traits;
using UnityEngine;

namespace Fenrir.Combat
{
    /// <summary>
    /// Tracks the current combat encounter: active enemies, engagement type,
    /// player behaviour flags. Emits trait events when combat ends.
    ///
    /// Lives in the EmberForest scene. Registered with ServiceLocator on Awake.
    /// </summary>
    public class CombatContext : MonoBehaviour
    {
        private readonly HashSet<EnemyBase> _activeEnemies = new();
        private float _combatStartTime;
        private bool  _playerDodgedThisFight;
        private int   _hitsWithoutDodge;

        // Sacrifice death tracking
        private int _peakEnemyCount;

        // Key: EnemyBase instance, Value: total damage dealt by player to that enemy this fight
        private readonly Dictionary<EnemyBase, float> _damageDealtPerEnemy = new();

        // ── Public read-only queries (used by PlayerHealth for death classification) ──

        /// <summary>Highest number of simultaneous enemies in this encounter.</summary>
        public int PeakEnemyCount => _peakEnemyCount;

        /// <summary>
        /// Highest fraction of any single enemy's max HP that the player dealt in this fight.
        /// Returns 0 if no damage was recorded.
        /// </summary>
        public float MaxDamageFractionDealt
        {
            get
            {
                float max = 0f;
                foreach (KeyValuePair<EnemyBase, float> pair in _damageDealtPerEnemy)
                {
                    var health = pair.Key.GetComponent<EnemyHealth>();
                    if (health == null) continue;
                    float fraction = pair.Value / health.MaxHp;
                    if (fraction > max) max = fraction;
                }
                return max;
            }
        }

        // ── Lifecycle ─────────────────────────────────────────────────────────

        private void Awake()
        {
            ServiceLocator.Register<CombatContext>(this);
        }

        private void OnDestroy()
        {
            ServiceLocator.Unregister<CombatContext>();
        }

        // ── Enemy registration ────────────────────────────────────────────────

        public void RegisterEnemy(EnemyBase enemy)
        {
            bool wasClear = _activeEnemies.Count == 0;
            _activeEnemies.Add(enemy);
            if (wasClear) BeginCombat();   // zone count snapshotted inside BeginCombat
        }

        public void UnregisterEnemy(EnemyBase enemy)
        {
            _activeEnemies.Remove(enemy);
            if (_activeEnemies.Count == 0) EndCombat();
        }

        // ── Player action tracking ────────────────────────────────────────────

        public void RecordDodge() => _playerDodgedThisFight = true;

        /// <summary>
        /// Called by PlayerHealth after emitting HitTakenNoDodgeEvent.
        /// Only increments internal counter — does NOT re-emit.
        /// </summary>
        public void NotifyHitTaken() => _hitsWithoutDodge++;

        /// <summary>
        /// Called by PlayerCombat after a confirmed hit lands on an enemy.
        /// Accumulates damage per enemy for Sacrifice death classification.
        /// </summary>
        public void RecordDamageDealt(EnemyBase enemy, float damage)
        {
            if (enemy == null) return;
            _damageDealtPerEnemy.TryGetValue(enemy, out float prev);
            _damageDealtPerEnemy[enemy] = prev + damage;
        }

        // ── Internal ──────────────────────────────────────────────────────────

        private void BeginCombat()
        {
            _combatStartTime       = Time.time;
            _playerDodgedThisFight = false;
            _hitsWithoutDodge      = 0;
            _damageDealtPerEnemy.Clear();

            // Group = all living enemies present in the zone, not just aggroed ones.
            // A player fighting one wolf while two more patrol nearby is still a group fight.
            _peakEnemyCount = CountLivingEnemiesInZone();

            SceneRouter.SetGameState(GameState.Combat);

            if (ServiceLocator.TryGet<AudioManager>(out AudioManager audio))
                audio.StartCombatMusic();

            Debug.Log($"[CombatContext] Combat started. Zone enemies: {_peakEnemyCount}");
        }

        /// <summary>
        /// Counts all EnemyHealth components in the scene that are alive.
        /// Intentionally includes non-aggroed enemies — the zone defines the threat context.
        /// </summary>
        private static int CountLivingEnemiesInZone()
        {
            int count = 0;
            foreach (Entities.Enemies.EnemyHealth h in
                     FindObjectsByType<Entities.Enemies.EnemyHealth>(FindObjectsSortMode.None))
            {
                if (!h.IsDead) count++;
            }
            return count;
        }

        private void EndCombat()
        {
            float duration = Time.time - _combatStartTime;

            if (!_playerDodgedThisFight)
                BehaviorEventBus.Emit(new CombatCompletedNoDodgeEvent());

            SceneRouter.SetGameState(GameState.Exploration);

            if (ServiceLocator.TryGet<AudioManager>(out AudioManager audio))
                audio.StopCombatMusic();

            if (ServiceLocator.TryGet<ISaveManager>(out ISaveManager save))
                save.MarkDirty();

            Debug.Log($"[CombatContext] Combat ended after {duration:F1}s.");
        }
    }
}
