#if UNITY_EDITOR
// Editor/FenrirWireReferences.cs
// Run once via: Fenrir → Wire Scene References
// Opens EmberForest, wires all serialized cross-component references, saves.

using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

using Fenrir.Audio;
using Fenrir.Combat;
using Fenrir.Entities.Player;
using Fenrir.Input;
using Fenrir.UI;
using Fenrir.World;

namespace Fenrir.Editor
{
    public static class FenrirWireReferences
    {
        [MenuItem("Fenrir/Wire Scene References")]
        public static void WireAll()
        {
            WireEmberForest();
            Debug.Log("[FenrirWireReferences] ✓ All references wired.");
        }

        private static void WireEmberForest()
        {
            const string scenePath = "Assets/_Project/Scenes/EmberForest.unity";
            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

            // ── Find GameObjects ──────────────────────────────────────────────
            var playerGo      = GameObject.Find("Player");
            var inputGo       = GameObject.Find("InputSystem");
            var worldGo       = GameObject.Find("WorldManager");
            var dnGo          = GameObject.Find("DayNightCycle");
            var audioGo       = GameObject.Find("AudioManager");
            var hudGo         = GameObject.Find("HUD");
            var dirLight      = GameObject.Find("Directional Light");

            if (playerGo == null || inputGo == null || dnGo == null)
            {
                Debug.LogError("[FenrirWireReferences] Could not find required GameObjects. Run Setup Scenes first.");
                return;
            }

            // ── Get Components ────────────────────────────────────────────────
            var playerController = playerGo.GetComponent<PlayerController>();
            var playerCombat     = playerGo.GetComponent<PlayerCombat>();
            var playerHealth     = playerGo.GetComponent<PlayerHealth>();
            var playerEnergy     = playerGo.GetComponent<PlayerEnergy>();

            var touchMapper      = inputGo.GetComponent<TouchMapper>();
            var gestureRecog     = inputGo.GetComponent<GestureRecognizer>();
            var inputHandler     = inputGo.GetComponent<InputHandler>();

            var worldManager     = worldGo?.GetComponent<WorldManager>();
            var dayNight         = dnGo.GetComponent<DayNightCycle>();

            var audioManager     = audioGo?.GetComponent<AudioManager>();
            var musicLayer       = audioGo?.GetComponent<MusicLayer>();
            var sfxPool          = audioGo?.GetComponent<SFXPool>();

            var hud              = hudGo?.GetComponent<HUD>();
            var energyBar        = hudGo?.GetComponent<EnergyBar>();
            var journal          = hudGo?.GetComponent<JournalController>();

            var light            = dirLight?.GetComponent<Light>();

            // ── Wire References ───────────────────────────────────────────────

            // TouchMapper → Player
            if (touchMapper != null)
            {
                SetField(touchMapper, "_controller", playerController);
                SetField(touchMapper, "_combat",     playerCombat);
            }

            // GestureRecognizer → TouchMapper
            if (gestureRecog != null)
                SetField(gestureRecog, "_mapper", touchMapper);

            // InputHandler → GestureRecognizer + TouchMapper
            if (inputHandler != null)
            {
                SetField(inputHandler, "_gesture", gestureRecog);
                SetField(inputHandler, "_mapper",  touchMapper);
            }

            // DayNightCycle → Directional Light
            if (dayNight != null && light != null)
                SetField(dayNight, "_directionalLight", light);

            // WorldManager → DayNightCycle
            if (worldManager != null)
                SetField(worldManager, "_dayNight", dayNight);

            // AudioManager → MusicLayer + SFXPool
            if (audioManager != null)
            {
                SetField(audioManager, "_musicLayer", musicLayer);
                SetField(audioManager, "_sfxPool",    sfxPool);
            }

            // HUD → PlayerHealth
            if (hud != null)
                SetField(hud, "_health", playerHealth);

            // EnergyBar → PlayerEnergy
            if (energyBar != null)
                SetField(energyBar, "_energy", playerEnergy);

            // JournalController → HUD
            if (journal != null)
                SetField(journal, "_hud", hud);

            EditorSceneManager.SaveScene(scene);
            Debug.Log("[FenrirWireReferences] ✓ EmberForest references wired and saved.");
        }

        // ── Helper ────────────────────────────────────────────────────────────

        private static void SetField(Object target, string fieldName, Object value)
        {
            var so   = new SerializedObject(target);
            var prop = so.FindProperty(fieldName);
            if (prop != null)
            {
                prop.objectReferenceValue = value;
                so.ApplyModifiedPropertiesWithoutUndo();
            }
            else
            {
                Debug.LogWarning($"[FenrirWireReferences] Field '{fieldName}' not found on {target.GetType().Name}");
            }
        }
    }
}
#endif
