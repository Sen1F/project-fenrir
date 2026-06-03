using System;
using Fenrir.Core;
using UnityEngine;

namespace Fenrir.StateMachine
{
    /// <summary>
    /// Singleton MonoBehaviour service. Owns PlayerState and listens to SceneRouter
    /// for App/GameState. Fires typed events so any system can react without polling.
    ///
    /// Registered with ServiceLocator in Bootstrap — access via
    ///   ServiceLocator.Get&lt;AppStateMachine&gt;()
    /// </summary>
    public class AppStateMachine : MonoBehaviour
    {
        // ── Player state ──────────────────────────────────────────────────────
        public PlayerState PlayerState { get; private set; } = PlayerState.Idle;

        public event Action<PlayerState> OnPlayerStateChanged;

        // ── Convenience pass-throughs ─────────────────────────────────────────
        public AppState  AppState  => SceneRouter.CurrentAppState;
        public GameState GameState => SceneRouter.CurrentGameState;

        // ── Lifecycle ─────────────────────────────────────────────────────────

        private void OnEnable()
        {
            SceneRouter.OnAppStateChanged  += HandleAppStateChanged;
            SceneRouter.OnGameStateChanged += HandleGameStateChanged;
        }

        private void OnDisable()
        {
            SceneRouter.OnAppStateChanged  -= HandleAppStateChanged;
            SceneRouter.OnGameStateChanged -= HandleGameStateChanged;
        }

        // ── Player state transitions ──────────────────────────────────────────

        /// <summary>
        /// Request a player state transition. Ignored if the transition is illegal.
        /// Returns true if the transition was applied.
        /// </summary>
        public bool TrySetPlayerState(PlayerState next)
        {
            if (!IsValidTransition(PlayerState, next))
            {
                Debug.LogWarning($"[AppStateMachine] Invalid player transition: {PlayerState} → {next}");
                return false;
            }

            PlayerState prev = PlayerState;
            PlayerState = next;
            OnPlayerStateChanged?.Invoke(next);
            Debug.Log($"[AppStateMachine] Player: {prev} → {next}");
            return true;
        }

        public bool IsPlayerBusy()
        {
            return PlayerState is PlayerState.Attacking
                                or PlayerState.Dodging
                                or PlayerState.Staggered
                                or PlayerState.Dead
                                or PlayerState.InEvolution;
        }

        public bool CanPlayerAct()
        {
            return (GameState == GameState.Exploration || GameState == GameState.Combat)
                   && PlayerState is PlayerState.Idle or PlayerState.Moving;
        }

        // ── Transition table ──────────────────────────────────────────────────

        private static bool IsValidTransition(PlayerState from, PlayerState to)
        {
            // Dead is terminal until respawn logic explicitly resets to Idle
            if (from == PlayerState.Dead && to != PlayerState.Idle)
                return false;

            // Evolution is terminal until the sequencer completes it
            if (from == PlayerState.InEvolution && to != PlayerState.Idle)
                return false;

            // Can always go to Dead from any fighting state
            if (to == PlayerState.Dead)
                return from != PlayerState.Dead;

            return true; // All other transitions permitted
        }

        // ── App / Game state listeners ────────────────────────────────────────

        private void HandleAppStateChanged(AppState state)
        {
            if (state == AppState.Loading)
                TrySetPlayerState(PlayerState.Idle);
        }

        private void HandleGameStateChanged(GameState state)
        {
            // When a cutscene or dialogue starts, lock the player
            if (state is GameState.Cutscene or GameState.Dialogue or GameState.Evolution)
                TrySetPlayerState(PlayerState.InCutscene);
            else if (state == GameState.Exploration)
                TrySetPlayerState(PlayerState.Idle);
        }
    }
}
