using System;
using System.Threading.Tasks;
using Fenrir.Save;
using UnityEngine;

namespace Fenrir.Core
{
    /// <summary>
    /// Drives the top-level game loop:
    ///   - Pause / resume (Time.timeScale + SceneRouter state)
    ///   - Application focus / pause → debounced auto-save on background
    ///
    /// Bus clearing on scene transitions lives in SceneRouter.TransitionAsync
    /// (before activation) so new-scene Awake/OnEnable subscriptions survive.
    /// </summary>
    public class GameLoop : MonoBehaviour
    {
        public bool IsPaused { get; private set; }

        private bool _savePending;

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
