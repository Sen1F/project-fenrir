using Fenrir.Save;
using Fenrir.StateMachine;
using Fenrir.Traits;
using UnityEngine;

namespace Fenrir.Core
{
    /// <summary>
    /// Drives the top-level game loop:
    ///   - Pause / resume (sets Time.timeScale)
    ///   - Application focus / pause → auto-save
    ///   - Scene unload → BehaviorEventBus.Clear()
    ///
    /// Attach to the Bootstrap GameObject (DontDestroyOnLoad).
    /// Registered with ServiceLocator in Bootstrap.
    /// </summary>
    public class GameLoop : MonoBehaviour
    {
        public bool IsPaused { get; private set; }

        // ── Pause / Resume ────────────────────────────────────────────────────

        public void Pause()
        {
            if (IsPaused) return;
            IsPaused       = true;
            Time.timeScale = 0f;
            SceneRouter.OnAppStateChanged?.Invoke(AppState.Paused);
            Debug.Log("[GameLoop] Paused");
        }

        public void Resume()
        {
            if (!IsPaused) return;
            IsPaused       = false;
            Time.timeScale = 1f;
            SceneRouter.OnAppStateChanged?.Invoke(AppState.InGame);
            Debug.Log("[GameLoop] Resumed");
        }

        public void TogglePause()
        {
            if (IsPaused) Resume(); else Pause();
        }

        // ── Application lifecycle ─────────────────────────────────────────────

        private void OnApplicationPause(bool paused)
        {
            if (paused) SaveAndClearBus();
        }

        private void OnApplicationFocus(bool focused)
        {
            if (!focused) SaveAndClearBus();
        }

        private void OnApplicationQuit()
        {
            SaveAndClearBus();
        }

        // ── Scene unload ──────────────────────────────────────────────────────

        private void OnDestroy()
        {
            // Only clear bus when the Bootstrap object itself is destroyed
            // (i.e. the full session is ending — not mid-game scene transitions).
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private static async void SaveAndClearBus()
        {
            if (!ServiceLocator.TryGet<ISaveManager>(out ISaveManager save)) return;
            if (save.IsDirty)
            {
                await save.SaveAsync();
                Debug.Log("[GameLoop] Auto-saved on pause/quit.");
            }
        }
    }
}
