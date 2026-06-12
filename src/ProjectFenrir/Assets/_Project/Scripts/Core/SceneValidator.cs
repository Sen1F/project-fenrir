using Fenrir.Audio;
using Fenrir.Combat;
using Fenrir.Entities.Player;
using Fenrir.Input;
using Fenrir.World;
using UnityEngine;

namespace Fenrir.Core
{
    /// <summary>
    /// Runs at scene start in development builds.
    /// Validates that all critical scene references exist and logs clear
    /// error messages instead of cryptic NullReferenceExceptions.
    /// Attach to any persistent GameObject (e.g. the WorldManager).
    /// </summary>
    public class SceneValidator : MonoBehaviour
    {
        private void Start()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            ValidateScene();
#endif
        }

        private static void ValidateScene()
        {
            int issues = 0;

            issues += Require<PlayerController>("PlayerController");
            issues += Require<PlayerHealth>    ("PlayerHealth");
            issues += Require<PlayerEnergy>    ("PlayerEnergy");
            issues += Require<PlayerCombat>    ("PlayerCombat");
            issues += Require<TouchMapper>     ("TouchMapper");
            issues += Require<InputHandler>    ("InputHandler");
            issues += Require<DayNightCycle>   ("DayNightCycle");
            issues += Require<WorldManager>    ("WorldManager");
            issues += Require<AudioManager>    ("AudioManager");
            issues += Require<CombatSystem>    ("CombatSystem");

            // Player tag
            var playerGo = GameObject.FindWithTag("Player");
            if (playerGo == null)
            {
                Debug.LogError("[SceneValidator] No GameObject tagged 'Player'. " +
                               "Select Player → Inspector → Tag → Player.");
                issues++;
            }

            if (issues == 0)
                Debug.Log("[SceneValidator] ✓ Scene validated — all critical references found.");
            else
                Debug.LogWarning($"[SceneValidator] {issues} issue(s) found. " +
                                 "Check errors above before playtesting.");
        }

        private static int Require<T>(string label) where T : Object
        {
            var obj = FindAnyObjectByType<T>();
            if (obj != null) return 0;

            Debug.LogError($"[SceneValidator] Missing: {label} — " +
                           $"no {typeof(T).Name} found in scene.");
            return 1;
        }
    }
}
