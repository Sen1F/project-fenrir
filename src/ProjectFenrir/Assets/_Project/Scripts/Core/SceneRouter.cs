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
    /// Owns CurrentAppState / CurrentGameState so every system reads a single
    /// source of truth. GameLoop and AppStateMachine mutate state via the public
    /// setters here rather than firing events themselves.
    /// </summary>
    public static class SceneRouter
    {
        // ── Scene name constants ──────────────────────────────────────────────
        public const string SceneBootstrap   = "Bootstrap";
        public const string SceneMainMenu    = "MainMenu";
        public const string SceneAwakening   = "Awakening";
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
            => await TransitionAsync(SceneMainMenu, AppState.MainMenu);

        public static async Task LoadAwakeningAsync()
            => await TransitionAsync(SceneAwakening, AppState.InGame, GameState.Awakening);

        public static async Task LoadGameAsync()
            => await TransitionAsync(SceneEmberForest, AppState.InGame, GameState.Exploration);

        // ── State setters (called by GameLoop, CombatContext, etc.) ───────────

        public static void SetGameState(GameState state)
        {
            if (CurrentGameState == state) return;
            CurrentGameState = state;
            OnGameStateChanged?.Invoke(state);
        }

        /// <summary>Called by GameLoop.Pause/Resume — updates state without loading a scene.</summary>
        public static void SetPaused(bool paused)
        {
            AppState target = paused ? AppState.Paused : AppState.InGame;
            if (CurrentAppState == target) return;
            CurrentAppState = target;
            OnAppStateChanged?.Invoke(target);
        }

        /// <summary>
        /// Resets static state. Call from Bootstrap.Awake in Editor builds to handle
        /// domain reloads cleanly.
        /// </summary>
        public static void ResetForEditorReload()
        {
            CurrentAppState  = AppState.Booting;
            CurrentGameState = GameState.Exploration;
        }

        // ── Internal ──────────────────────────────────────────────────────────

        private static async Task TransitionAsync(
            string sceneName,
            AppState nextAppState,
            GameState nextGameState = GameState.Exploration)
        {
            if (CurrentAppState == AppState.Loading)
            {
                Debug.LogWarning($"[SceneRouter] Navigation to '{sceneName}' ignored — already loading.");
                return;
            }

            SetAppState(AppState.Loading);

            // Clear the bus BEFORE the new scene activates. New-scene objects
            // subscribe in Awake/OnEnable during activation; clearing after
            // completion (the old GameLoop hook) wiped those fresh subscriptions.
            // Old-scene handlers are about to be destroyed with their scene anyway.
            // Additive sub-zone loads (RegionLoader) never pass through here.
            Traits.BehaviorEventBus.Clear();
            OnSceneLoadStarted?.Invoke();

            AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
            op.allowSceneActivation = false;

            // Phase 1: wait until the scene data is ready (progress stalls at 0.9 until activated)
            while (op.progress < 0.9f)
                await Task.Yield();

            op.allowSceneActivation = true;

            // Phase 2: wait until the scene is fully active (isDone → Awake + Start have fired)
            while (!op.isDone)
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
