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

        // Key: EnemyBase instance, Value: total damage dealt by player to that enemy this fight
        private readonly Dictionary<EnemyBase, float> _damageDealtPerEnemy = new();

        // ── Public queries (used by PlayerHealth for death classification) ─────

        /// <summary>
        /// Count of enemies currently aggroed or alerted — queried live at death time
        /// so enemies that join mid-fight are included. Uses EnemyAI.IsAwareOfPlayer.
        /// </summary>
        public int GetAwareEnemyCount()
        {
            int count = 0;
            foreach (Entities.Enemies.EnemyAI ai in
                     FindObjectsByType<Entities.Enemies.EnemyAI>(FindObjectsSortMode.None))
            {
                if (ai.IsAwareOfPlayer) count++;
            }
            return count;
        }

        /// <summary>
        /// Highest fraction of any single enemy's max HP dealt by the player this fight.
        /// Returns 0 if no damage recorded.
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

            SceneRouter.SetGameState(GameState.Combat);

            if (ServiceLocator.TryGet<AudioManager>(out AudioManager audio))
                audio.StartCombatMusic();

            Debug.Log("[CombatContext] Combat started.");
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
