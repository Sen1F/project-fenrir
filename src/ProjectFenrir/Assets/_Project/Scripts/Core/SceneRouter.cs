using System;
using System.Threading.Tasks;
using Fenrir.StateMachine;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Fenrir.Core
{
    /// <summary>
    /// Typed async scene router. All navigation goes through here — never call
    /// SceneManager.LoadScene directly from game code.
    ///
    /// Fires <see cref="OnAppStateChanged"/> before every transition so systems
    /// (audio, HUD, AI) can react without coupling to scene names.
    /// </summary>
    public static class SceneRouter
    {
        // ── Scene name constants ──────────────────────────────────────────────
        public const string SceneBootstrap  = "Bootstrap";
        public const string SceneMainMenu   = "MainMenu";
        public const string SceneAwakening  = "Awakening";
        public const string SceneEmberForest = "EmberForest";

        // ── State ─────────────────────────────────────────────────────────────
        public static AppState  CurrentAppState  { get; private set; } = AppState.Booting;
        public static GameState CurrentGameState { get; private set; } = GameState.Exploration;

        // ── Events ────────────────────────────────────────────────────────────
        public static event Action<AppState>  OnAppStateChanged;
        public static event Action<GameState> OnGameStateChanged;
        public static event Action            OnSceneLoadStarted;
        public static event Action            OnSceneLoadComplete;

        // ── Navigation ────────────────────────────────────────────────────────

        public static async Task LoadMainMenuAsync()
        {
            await TransitionAsync(SceneMainMenu, AppState.MainMenu);
        }

        public static async Task LoadAwakeningAsync()
        {
            await TransitionAsync(SceneAwakening, AppState.InGame, GameState.Awakening);
        }

        public static async Task LoadGameAsync()
        {
            await TransitionAsync(SceneEmberForest, AppState.InGame, GameState.Exploration);
        }

        public static void SetGameState(GameState state)
        {
            if (CurrentGameState == state) return;
            CurrentGameState = state;
            OnGameStateChanged?.Invoke(state);
        }

        // ── Internal ──────────────────────────────────────────────────────────

        private static async Task TransitionAsync(
            string sceneName,
            AppState nextAppState,
            GameState nextGameState = GameState.Exploration)
        {
            if (CurrentAppState == AppState.Loading) return; // Guard re-entrant calls

            SetAppState(AppState.Loading);
            OnSceneLoadStarted?.Invoke();

            AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
            op.allowSceneActivation = false;

            // Wait until Unity has loaded >90% (scene data ready, not yet activated)
            while (op.progress < 0.9f)
                await Task.Yield();

            op.allowSceneActivation = true;

            // Wait one more frame for Awake/Start to fire
            await Task.Yield();

            SetAppState(nextAppState);
            SetGameState(nextGameState);
            OnSceneLoadComplete?.Invoke();

            Debug.Log($"[SceneRouter] → {sceneName} | App:{nextAppState} Game:{nextGameState}");
        }

        private static void SetAppState(AppState state)
        {
            if (CurrentAppState == state) return;
            CurrentAppState = state;
            OnAppStateChanged?.Invoke(state);
        }
    }
}
