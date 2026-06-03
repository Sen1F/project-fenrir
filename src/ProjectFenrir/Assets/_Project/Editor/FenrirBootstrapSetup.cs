using Fenrir.Analytics;
using Fenrir.Core;
using Fenrir.StateMachine;
using UnityEditor;
using UnityEngine;

namespace Fenrir.Editor
{
    /// <summary>
    /// Fenrir → Setup → Setup Bootstrap Scene
    /// Idempotent — safe to run multiple times.
    /// Creates or repairs the Bootstrap scene's manager GameObject with all
    /// required components: Bootstrap, AppStateMachine, GameLoop, EventLogger wire.
    ///
    /// Run this once after creating Bootstrap.unity in the Unity Editor.
    /// </summary>
    public static class FenrirBootstrapSetup
    {
        private const string ManagerName = "FenrirManager";

        [MenuItem("Fenrir/Setup/Setup Bootstrap Scene")]
        public static void SetupBootstrapScene()
        {
            GameObject manager = GetOrCreate(ManagerName);

            EnsureComponent<Bootstrap>(manager);
            EnsureComponent<AppStateMachine>(manager);
            EnsureComponent<GameLoop>(manager);

            // Tag so SceneValidator can find it
            manager.tag = "GameController";

            // Build Settings: ensure Bootstrap is scene 0
            EnsureSceneInBuildSettings("Assets/_Project/Scenes/Bootstrap.unity", 0);
            EnsureSceneInBuildSettings("Assets/_Project/Scenes/Awakening.unity", 1);
            EnsureSceneInBuildSettings("Assets/_Project/Scenes/EmberForest.unity", 2);

            EditorUtility.SetDirty(manager);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                manager.scene);

            Debug.Log("[FenrirBootstrapSetup] Bootstrap scene configured. " +
                      "Save the scene (Ctrl+S) before entering Play mode.");

            Selection.activeGameObject = manager;
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private static GameObject GetOrCreate(string name)
        {
            GameObject go = GameObject.Find(name);
            if (go == null)
            {
                go = new GameObject(name);
                Undo.RegisterCreatedObjectUndo(go, $"Create {name}");
            }
            return go;
        }

        private static T EnsureComponent<T>(GameObject go) where T : Component
        {
            T existing = go.GetComponent<T>();
            if (existing != null) return existing;

            T added = Undo.AddComponent<T>(go);
            Debug.Log($"[FenrirBootstrapSetup] Added {typeof(T).Name} to {go.name}");
            return added;
        }

        private static void EnsureSceneInBuildSettings(string scenePath, int desiredIndex)
        {
            var scenes = new System.Collections.Generic.List<EditorBuildSettingsScene>(
                EditorBuildSettings.scenes);

            // Check if already present at any index
            foreach (var s in scenes)
                if (s.path == scenePath) return;

            scenes.Insert(desiredIndex, new EditorBuildSettingsScene(scenePath, true));
            EditorBuildSettings.scenes = scenes.ToArray();
            Debug.Log($"[FenrirBootstrapSetup] Added '{scenePath}' to Build Settings at index {desiredIndex}");
        }
    }
}
