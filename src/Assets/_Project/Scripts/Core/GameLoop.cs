using System;
using System.Threading.Tasks;
using Fenrir.Save;
using Fenrir.StateMachine;
using Fenrir.Traits;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Fenrir.Core
{
    /// <summary>
    /// Drives the top-level game loop:
    ///   - Pause / resume (Time.timeScale + SceneRouter state)
    ///   - Application focus / pause → debounced auto-save on background
    ///
    /// Attach to the Bootstrap GameObject (DontDestroyOnLoad).
    /// Registered with ServiceLocator in Bootstrap.
    /// </summary>
    public class GameLoop : MonoBehaviour
    {
        public bool IsPaused { get; private set; }

        // Debounce: OnApplicationPause + OnApplicationFocus both fire on iOS background
        private bool _savePending;

        // ── Lifecycle ─────────────────────────────────────────────────────────

        private void OnEnable()
        {
            SceneManager.sceneUnloaded += OnSceneUnloaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneUnloaded -= OnSceneUnloaded;
        }

        /// <summary>
        /// Clear event bus on every scene unload to prevent stale MonoBehaviour
        /// callbacks from destroyed scene objects firing during the next scene.
        /// Bootstrap (DontDestroyOnLoad) resubscribes its own handlers after load.
        /// </summary>
        private static void OnSceneUnloaded(Scene _)
        {
            BehaviorEventBus.Clear();
            Debug.Log("[GameLoop] BehaviorEventBus cleared on scene unload.");
        }

        // ── Pause / Resume ────────────────────────────────────────────────────

        public void Pause()
        {
            if (IsPaused) return;
            IsPaused       = true;
            Time.timeScale = 0f;
            // Use the router's internal setter so CurrentAppState stays accurate
            SceneRouter.SetPaused(true);
            Debug.Log("[GameLoop] Paused");
        }

        public void Resume()
        {
            if (!IsPaused) return;
            IsPaused       = false;
            Time.timeScale = 1f;
            SceneRouter.SetPaused(false);
            Debug.Log("[GameLoop] Resumed");
        }

        public void TogglePause()
        {
            if (IsPaused) Resume(); else Pause();
        }

        // ── Application lifecycle ─────────────────────────────────────────────

        private void OnApplicationPause(bool paused)
        {
            if (paused) RequestSave();
        }

        private void OnApplicationFocus(bool focused)
        {
            if (!focused) RequestSave();
        }

        private void OnApplicationQuit()
        {
            // Synchronous path on quit — fire-and-forget with error logging
            _ = TrySaveAsync();
        }

        // ── Debounced save ────────────────────────────────────────────────────

        private void RequestSave()
        {
            if (_savePending) return; // de-duplicate the double-fire on iOS background
            _savePending = true;
            _ = TrySaveAsync();
        }

        private async Task TrySaveAsync()
        {
            try
            {
                if (!ServiceLocator.TryGet<ISaveManager>(out ISaveManager save)) return;
                if (!save.IsDirty) return;
                await save.SaveAsync();
                Debug.Log("[GameLoop] Auto-saved.");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[GameLoop] Auto-save failed: {ex.Message}");
            }
            finally
            {
                _savePending = false;
            }
        }
    }
}
