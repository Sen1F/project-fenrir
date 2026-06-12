using System;
using System.Threading.Tasks;
using Fenrir.Save;
using Fenrir.Traits;
using UnityEngine;

namespace Fenrir.Core
{
    /// <summary>
    /// Drives the top-level game loop:
    ///   - Pause / resume (Time.timeScale + SceneRouter state)
    ///   - Application focus / pause → debounced auto-save on background
    ///   - Full-scene transitions → clears BehaviorEventBus stale handlers
    ///
    /// Additive sub-zone loads (RegionLoader) do NOT clear the bus — only
    /// full SceneRouter transitions do, preventing trait subscriptions from
    /// being wiped mid-gameplay.
    /// </summary>
    public class GameLoop : MonoBehaviour
    {
        public bool IsPaused { get; private set; }

        private bool _savePending;

        // ── Lifecycle ─────────────────────────────────────────────────────────

        private void OnEnable()
        {
            // Subscribe to full-scene completions only — NOT SceneManager.sceneUnloaded,
            // which also fires for additive sub-zone unloads from RegionLoader.
            SceneRouter.OnSceneLoadComplete += OnFullSceneTransitionComplete;
        }

        private void OnDisable()
        {
            SceneRouter.OnSceneLoadComplete -= OnFullSceneTransitionComplete;
        }

        /// <summary>
        /// Called after every full SceneRouter navigation.
        /// Clears stale scene-object handlers from the destroyed scene.
        /// Bootstrap (DontDestroyOnLoad) re-subscribes the TraitAccumulator immediately after.
        /// </summary>
        private static void OnFullSceneTransitionComplete()
        {
            BehaviorEventBus.Clear();
            Debug.Log("[GameLoop] BehaviorEventBus cleared after full scene transition.");
        }

        // ── Pause / Resume ────────────────────────────────────────────────────

        public void Pause()
        {
            if (IsPaused) return;
            IsPaused       = true;
            Time.timeScale = 0f;
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

        public void TogglePause() { if (IsPaused) Resume(); else Pause(); }

        // ── Application lifecycle ─────────────────────────────────────────────

        private void OnApplicationPause(bool paused)  { if (paused)   RequestSave(); }
        private void OnApplicationFocus(bool focused) { if (!focused) RequestSave(); }
        private void OnApplicationQuit()              { _ = TrySaveAsync(); }

        // ── Debounced save ────────────────────────────────────────────────────

        private void RequestSave()
        {
            if (_savePending) return;
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
