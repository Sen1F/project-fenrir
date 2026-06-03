// Editor/FenrirSceneSetup.cs
// Run once via: Fenrir → Setup Scenes
// Creates Bootstrap, Awakening, and EmberForest scenes with correct GameObjects,
// adds them to Build Settings, and saves everything.

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using System.Collections.Generic;

namespace Fenrir.Editor
{
    public static class FenrirSceneSetup
    {
        private const string ScenePath = "Assets/_Project/Scenes/";

        [MenuItem("Fenrir/Setup Scenes")]
        public static void SetupScenes()
        {
            Directory.CreateDirectory(Application.dataPath + "/_Project/Scenes");
            AssetDatabase.Refresh();

            CreateBootstrapScene();
            CreateAwakeningScene();
            CreateEmberForestScene();
            AddScenesToBuildSettings();

            Debug.Log("[FenrirSceneSetup] ✓ All scenes created and added to Build Settings.");
        }

        // ── Bootstrap ─────────────────────────────────────────────────────────

        private static void CreateBootstrapScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // Bootstrap GameObject
            var go = new GameObject("Bootstrap");
            go.AddComponent<Fenrir.Core.Bootstrap>();

            // EventSystem (needed for UI)
            CreateEventSystem();

            EditorSceneManager.SaveScene(scene, ScenePath + "Bootstrap.unity");
            Debug.Log("[FenrirSceneSetup] ✓ Bootstrap scene created.");
        }

        // ── Awakening ─────────────────────────────────────────────────────────

        private static void CreateAwakeningScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            // AwakeningSequencer
            var sequencer = new GameObject("AwakeningSequencer");
            sequencer.AddComponent<Fenrir.Awakening.AwakeningSequencer>();

            // Placeholder UI panels (empty GameObjects — designer fills in)
            CreateUIPanel("CharacterCreationPanel", sequencer.transform);
            CreateUIPanel("ElementRevealPanel",     sequencer.transform);
            CreateUIPanel("LoadingPanel",           sequencer.transform);

            EditorSceneManager.SaveScene(scene, ScenePath + "Awakening.unity");
            Debug.Log("[FenrirSceneSetup] ✓ Awakening scene created.");
        }

        // ── EmberForest ───────────────────────────────────────────────────────

        private static void CreateEmberForestScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            // ── Player ────────────────────────────────────────────────────────
            var player = new GameObject("Player");
            player.tag = "Player";
            player.AddComponent<CharacterController>();
            player.AddComponent<Fenrir.Entities.Player.PlayerController>();
            player.AddComponent<Fenrir.Entities.Player.PlayerHealth>();
            player.AddComponent<Fenrir.Entities.Player.PlayerEnergy>();
            player.AddComponent<Fenrir.Combat.HitStateManager>();
            var combat  = player.AddComponent<Fenrir.Entities.Player.PlayerCombat>();
            var emitter = player.AddComponent<Fenrir.Entities.Player.PlayerTraitEmitter>();

            // ── Input ─────────────────────────────────────────────────────────
            var inputGo     = new GameObject("InputSystem");
            var mapper      = inputGo.AddComponent<Fenrir.Input.TouchMapper>();
            var gesture     = inputGo.AddComponent<Fenrir.Input.GestureRecognizer>();
            var inputHandler = inputGo.AddComponent<Fenrir.Input.InputHandler>();

            // ── World ─────────────────────────────────────────────────────────
            var worldGo = new GameObject("WorldManager");
            worldGo.AddComponent<Fenrir.World.WorldManager>();

            // DayNightCycle — wire directional light in Inspector
            var dnGo = new GameObject("DayNightCycle");
            dnGo.AddComponent<Fenrir.World.DayNightCycle>();

            // ── Combat System ─────────────────────────────────────────────────
            var csGo = new GameObject("CombatSystem");
            csGo.AddComponent<Fenrir.Combat.CombatSystem>();

            // ── Audio ─────────────────────────────────────────────────────────
            var audioGo = new GameObject("AudioManager");
            audioGo.AddComponent<Fenrir.Audio.AudioManager>();
            audioGo.AddComponent<Fenrir.Audio.MusicLayer>();
            audioGo.AddComponent<Fenrir.Audio.SFXPool>();

            // ── HUD (Canvas) ──────────────────────────────────────────────────
            var hudCanvas = new GameObject("HUD");
            var canvas    = hudCanvas.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            hudCanvas.AddComponent<UnityEngine.UI.CanvasScaler>();
            hudCanvas.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            hudCanvas.AddComponent<Fenrir.UI.HUD>();
            hudCanvas.AddComponent<Fenrir.UI.EnergyBar>();
            hudCanvas.AddComponent<Fenrir.UI.JournalController>();

            CreateEventSystem();

            EditorSceneManager.SaveScene(scene, ScenePath + "EmberForest.unity");
            Debug.Log("[FenrirSceneSetup] ✓ EmberForest scene created.");
        }

        // ── Build Settings ────────────────────────────────────────────────────

        private static void AddScenesToBuildSettings()
        {
            var scenes = new List<EditorBuildSettingsScene>
            {
                new EditorBuildSettingsScene(ScenePath + "Bootstrap.unity",   true),
                new EditorBuildSettingsScene(ScenePath + "Awakening.unity",   true),
                new EditorBuildSettingsScene(ScenePath + "EmberForest.unity", true),
            };
            EditorBuildSettings.scenes = scenes.ToArray();
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private static void CreateEventSystem()
        {
            var es = new GameObject("EventSystem");
            es.AddComponent<UnityEngine.EventSystems.EventSystem>();
            // Use new Input System module — StandaloneInputModule causes errors when
            // Active Input Handling is set to "Input System Package (New)"
            es.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
        }

        private static GameObject CreateUIPanel(string name, Transform parent)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent);
            go.SetActive(false);
            return go;
        }
    }
}
#endif
