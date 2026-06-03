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
            if (wasClear) BeginCombat();
        }

        public void UnregisterEnemy(EnemyBase enemy)
        {
            _activeEnemies.Remove(enemy);
            if (_activeEnemies.Count == 0) EndCombat();
        }

        // ── Player action tracking (called by PlayerCombat / PlayerTraitEmitter) ──

        public void RecordDodge() => _playerDodgedThisFight = true;

        public void RecordHitTakenNoDodge()
        {
            _hitsWithoutDodge++;
            BehaviorEventBus.Emit(new HitTakenNoDodgeEvent());
        }

        // ── Internal ──────────────────────────────────────────────────────────

        private void BeginCombat()
        {
            _combatStartTime       = Time.time;
            _playerDodgedThisFight = false;
            _hitsWithoutDodge      = 0;

            // Do NOT force player state here — CombatSystem and PlayerCombat own that.
            // Only update game state and audio.
            SceneRouter.SetGameState(GameState.Combat);

            if (ServiceLocator.TryGet<AudioManager>(out AudioManager audio))
                audio.StartCombatMusic();

            Debug.Log("[CombatContext] Combat started.");
        }

        private void EndCombat()
        {
            float duration = Time.time - _combatStartTime;

            // "Dodge never used (combat completed)" — fires whenever the player finished
            // a fight without using a single dodge, regardless of hits taken.
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
