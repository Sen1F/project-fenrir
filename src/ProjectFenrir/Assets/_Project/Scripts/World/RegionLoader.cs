using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Fenrir.World
{
    /// <summary>
    /// Additive scene loading for Ember Forest sub-zones.
    /// Each sub-zone is a separate Unity scene loaded/unloaded as the player
    /// crosses region boundaries. Uses trigger volumes for seamless transitions.
    ///
    /// Sub-zones (Phase 1 MVP):
    ///   EmberForest_Core, EmberForest_Ashfields, EmberForest_Canopy,
    ///   EmberForest_Ruins, EmberForest_Shrine, EmberForest_DeepBurrow
    /// </summary>
    public class RegionLoader : MonoBehaviour
    {
        [SerializeField] private string _targetRegionScene;
        [SerializeField] private string _unloadOnEnter;      // scene to unload when this region loads

        private bool _loaded;

        // ── Trigger ───────────────────────────────────────────────────────────

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;
            if (_loaded) return;
            StartCoroutine(TransitionRegion());
        }

        private IEnumerator TransitionRegion()
        {
            _loaded = true;

            // Load new region additively
            if (!string.IsNullOrEmpty(_targetRegionScene) &&
                !SceneManager.GetSceneByName(_targetRegionScene).isLoaded)
            {
                var loadOp = SceneManager.LoadSceneAsync(_targetRegionScene, LoadSceneMode.Additive);
                loadOp.allowSceneActivation = true;
                yield return loadOp;
            }

            // Unload previous region after new one is ready
            if (!string.IsNullOrEmpty(_unloadOnEnter) &&
                SceneManager.GetSceneByName(_unloadOnEnter).isLoaded)
            {
                yield return SceneManager.UnloadSceneAsync(_unloadOnEnter);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            // Allow re-trigger if player leaves and re-enters
            if (other.CompareTag("Player")) _loaded = false;
        }
    }
}
