using System.Collections.Generic;
using Fenrir.Core;
using Fenrir.Entities.Enemies;
using Fenrir.Save;
using Fenrir.StateMachine;
using Fenrir.Traits;
using UnityEngine;

namespace Fenrir.Combat
{
    /// <summary>
    /// Tracks the current combat encounter: which enemies are active, when combat
    /// started, and how the player engaged. Emits trait events on combat end.
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

        // ── Player action tracking ────────────────────────────────────────────

        public void RecordDodge()  => _playerDodgedThisFight = true;

        public void RecordHitTakenNoDodge()
        {
            _hitsWithoutDodge++;
            BehaviorEventBus.Emit(new HitTakenNoDodgeEvent());
        }

        // ── Internal ──────────────────────────────────────────────────────────

        private void BeginCombat()
        {
            _combatStartTime        = Time.time;
            _playerDodgedThisFight  = false;
            _hitsWithoutDodge       = 0;

            if (ServiceLocator.TryGet<AppStateMachine>(out var sm))
                sm.TrySetPlayerState(PlayerState.Idle); // let CombatSystem own state from here

            SceneRouter.SetGameState(GameState.Combat);

            if (ServiceLocator.TryGet<Audio.AudioManager>(out var audio))
                audio.StartCombatMusic();

            Debug.Log("[CombatContext] Combat started.");
        }

        private void EndCombat()
        {
            float duration = Time.time - _combatStartTime;

            // Ambush: died within 5s of combat start (handled in PlayerHealth.OnDeath)
            // Here we handle the survived-combat events
            if (!_playerDodgedThisFight && _hitsWithoutDodge == 0)
                BehaviorEventBus.Emit(new CombatCompletedNoDodgeEvent());

            SceneRouter.SetGameState(GameState.Exploration);

            if (ServiceLocator.TryGet<Audio.AudioManager>(out var audio))
                audio.StopCombatMusic();

            if (ServiceLocator.TryGet<ISaveManager>(out var save))
                save.MarkDirty();

            Debug.Log($"[CombatContext] Combat ended after {duration:F1}s.");
        }
    }
}
