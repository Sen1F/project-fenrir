using System.Collections.Generic;
using System.IO;
using Unity.AI.Navigation;
using Unity.Cinemachine;
using UnityEditor;
using UnityEngine.AI;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem.UI;

using Fenrir.Audio;
using Fenrir.Awakening;
using Fenrir.Combat;
using Fenrir.Core;
using Fenrir.Entities.Player;
using Fenrir.Input;
using Fenrir.UI;
using Fenrir.World;

namespace Fenrir.Editor
{
    /// <summary>
    /// Fenrir → Setup Project
    ///
    /// Single-click full project setup. Idempotent — safe to run multiple times.
    ///
    /// Order of operations:
    ///   1. Register tags (Player, Enemy, GameController)
    ///   2. Create / configure Bootstrap scene
    ///   3. Create / configure Awakening scene
    ///   4. Create / configure EmberForest scene
    ///   5. Finalize EmberForest (ground, mesh, Cinemachine, EventSystem)
    ///   6. Wire all serialized references
    ///   7. Spawn Ash Wolf enemy
    ///   8. Bake NavMesh
    ///   9. Set Build Settings (Bootstrap=0, Awakening=1, EmberForest=2)
    ///  10. Save all scenes
    /// </summary>
    public static class FenrirSetup
    {
        private const string ScenePath       = "Assets/_Project/Scenes/";
        private const string BootstrapScene  = ScenePath + "Bootstrap.unity";
        private const string AwakeningScene  = ScenePath + "Awakening.unity";
        private const string GameScene       = ScenePath + "EmberForest.unity";

        [MenuItem("Fenrir/Setup Project")]
        public static void SetupProject()
        {
            // ── 1. Tags ───────────────────────────────────────────────────────
            FenrirEditorUtils.EnsureTag("Player");
            FenrirEditorUtils.EnsureTag("Enemy");
            FenrirEditorUtils.EnsureTag("GameController");

            // ── 2. Bootstrap ──────────────────────────────────────────────────
            SetupBootstrap();

            // ── 3. Awakening ──────────────────────────────────────────────────
            SetupAwakening();

            // ── 4–8. EmberForest (all steps in one scene open) ────────────────
            SetupEmberForest();

            // ── 9. Build Settings ─────────────────────────────────────────────
            SetBuildSettings();

            Debug.Log("[FenrirSetup] ✓ Project setup complete. Press Play in EmberForest.");
        }

        // ═════════════════════════════════════════════════════════════════════
        // Bootstrap
        // ═════════════════════════════════════════════════════════════════════

        private static void SetupBootstrap()
        {
            Directory.CreateDirectory(Application.dataPath + "/_Project/Scenes");
            AssetDatabase.Refresh();

            var scene = EditorSceneManager.OpenScene(BootstrapScene, OpenSceneMode.Single);

            // If no scene existed yet, NewScene creates it; otherwise OpenScene loaded it
            if (!File.Exists(Path.GetFullPath(BootstrapScene)))
                scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // FenrirManager — Bootstrap + AppStateMachine + GameLoop
            GameObject mgr = GetOrCreate("FenrirManager");
            mgr.tag = "GameController";
            EnsureComponent<Bootstrap>(mgr);
            EnsureComponent<Fenrir.StateMachine.AppStateMachine>(mgr);
            EnsureComponent<GameLoop>(mgr);

            // EventSystem
            CreateEventSystem();

            EditorSceneManager.SaveScene(scene, BootstrapScene);
            Debug.Log("[FenrirSetup] ✓ Bootstrap scene ready.");
        }

        // ═════════════════════════════════════════════════════════════════════
        // Awakening
        // ═════════════════════════════════════════════════════════════════════

        private static void SetupAwakening()
        {
            var scene = File.Exists(Path.GetFullPath(AwakeningScene))
                ? EditorSceneManager.OpenScene(AwakeningScene, OpenSceneMode.Single)
                : EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            GameObject seq = GetOrCreate("AwakeningSequencer");
            EnsureComponent<AwakeningSequencer>(seq);

            EnsureChild(seq, "CharacterCreationPanel");
            EnsureChild(seq, "ElementRevealPanel");
            EnsureChild(seq, "LoadingPanel");

            EditorSceneManager.SaveScene(scene, AwakeningScene);
            Debug.Log("[FenrirSetup] ✓ Awakening scene ready.");
        }

        // ═════════════════════════════════════════════════════════════════════
        // EmberForest — all steps while scene is open
        // ═════════════════════════════════════════════════════════════════════

        private static void SetupEmberForest()
        {
            var scene = File.Exists(Path.GetFullPath(GameScene))
                ? EditorSceneManager.OpenScene(GameScene, OpenSceneMode.Single)
                : EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            // ── Step 4: Create GameObjects ────────────────────────────────────
            CreatePlayer();
            CreateInputSystem();
            CreateWorldManager();
            CreateDayNightCycle();
            CreateCombatSystem();
            CreateAudio();
            CreateHUD();
            CreateEventSystem();

            // ── Step 5: Finalize ──────────────────────────────────────────────
            FixEventSystem();
            EnsureGround();
            EnsurePlayerMesh();
            EnsureCinemachineCamera();
            PositionPlayer();

            // ── Step 6: Wire references ───────────────────────────────────────
            WireReferences();

            // ── Step 7: Enemy ─────────────────────────────────────────────────
            SpawnAshWolf();

            // ── Step 8: NavMesh ───────────────────────────────────────────────
            BakeNavMesh();

            EditorSceneManager.SaveScene(scene, GameScene);
            Debug.Log("[FenrirSetup] ✓ EmberForest scene ready.");
        }

        // ── Create GameObjects ────────────────────────────────────────────────

        private static void CreatePlayer()
        {
            GameObject p = GetOrCreate("Player");
            p.tag = "Player";
            EnsureComponent<CharacterController>(p);
            EnsureComponent<PlayerController>(p);
            EnsureComponent<PlayerHealth>(p);
            EnsureComponent<PlayerEnergy>(p);
            EnsureComponent<HitStateManager>(p);
            EnsureComponent<PlayerCombat>(p);
            EnsureComponent<PlayerTraitEmitter>(p);
        }

        private static void CreateInputSystem()
        {
            GameObject go = GetOrCreate("InputSystem");
            EnsureComponent<TouchMapper>(go);
            EnsureComponent<GestureRecognizer>(go);
            EnsureComponent<InputHandler>(go);
        }

        private static void CreateWorldManager()
        {
            GameObject go = GetOrCreate("WorldManager");
            EnsureComponent<WorldManager>(go);
            EnsureComponent<SceneValidator>(go);
        }

        private static void CreateDayNightCycle()
        {
            GameObject go = GetOrCreate("DayNightCycle");
            EnsureComponent<DayNightCycle>(go);
        }

        private static void CreateCombatSystem()
        {
            GameObject go = GetOrCreate("CombatSystem");
            EnsureComponent<CombatSystem>(go);
            EnsureComponent<CombatContext>(go);
        }

        private static void CreateAudio()
        {
            GameObject go = GetOrCreate("AudioManager");
            EnsureComponent<AudioManager>(go);
            EnsureComponent<MusicLayer>(go);
            EnsureComponent<SFXPool>(go);
        }

        private static void CreateHUD()
        {
            GameObject go = GetOrCreate("HUD");
            if (go.GetComponent<Canvas>() == null)
            {
                Canvas c = go.AddComponent<Canvas>();
                c.renderMode = RenderMode.ScreenSpaceOverlay;
                go.AddComponent<UnityEngine.UI.CanvasScaler>();
                go.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            }
            EnsureComponent<HUD>(go);
            EnsureComponent<EnergyBar>(go);
            EnsureComponent<JournalController>(go);
        }

        // ── Finalize ──────────────────────────────────────────────────────────

        private static void FixEventSystem()
        {
            GameObject es = GameObject.Find("EventSystem");
            if (es == null) return;
            var old = es.GetComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            if (old != null) Object.DestroyImmediate(old);
            EnsureComponent<InputSystemUIInputModule>(es);
        }

        private static void EnsureGround()
        {
            if (GameObject.Find("Ground") != null) return;
            GameObject g = GameObject.CreatePrimitive(PrimitiveType.Plane);
            g.name = "Ground";
            g.transform.localScale = new Vector3(20f, 1f, 20f);
        }

        private static void EnsurePlayerMesh()
        {
            GameObject player = GameObject.Find("Player");
            if (player == null || player.GetComponentInChildren<MeshRenderer>() != null) return;

            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.name = "Mesh";
            body.transform.SetParent(player.transform);
            body.transform.localPosition = new Vector3(0f, 1f, 0f);
            Object.DestroyImmediate(body.GetComponent<CapsuleCollider>());

            GameObject arrow = GameObject.CreatePrimitive(PrimitiveType.Cube);
            arrow.name = "ForwardArrow";
            arrow.transform.SetParent(player.transform);
            arrow.transform.localPosition = new Vector3(0f, 1f, 0.65f);
            arrow.transform.localScale    = new Vector3(0.15f, 0.15f, 0.4f);
            Object.DestroyImmediate(arrow.GetComponent<BoxCollider>());

            if (player.transform.Find("CameraFollowTarget") == null)
            {
                GameObject t = new("CameraFollowTarget");
                t.transform.SetParent(player.transform);
                t.transform.localPosition = new Vector3(0f, 1.6f, 0f);
            }
        }

        private static void EnsureCinemachineCamera()
        {
            if (GameObject.Find("CM FreeLook") != null) return;

            GameObject player = GameObject.Find("Player");
            if (player == null) return;
            Transform follow = player.transform.Find("CameraFollowTarget");
            if (follow == null) return;

            Camera mainCam = Camera.main;
            if (mainCam != null) EnsureComponent<CinemachineBrain>(mainCam.gameObject);

            GameObject camGo = new("CM FreeLook");
            camGo.transform.position = new Vector3(0f, 3f, -6f);

            CinemachineCamera vcam = camGo.AddComponent<CinemachineCamera>();
            vcam.Follow   = follow;
            vcam.LookAt   = follow;
            vcam.Priority = new PrioritySettings { Value = 10 };

            CinemachineOrbitalFollow orb = camGo.AddComponent<CinemachineOrbitalFollow>();
            orb.OrbitStyle         = CinemachineOrbitalFollow.OrbitStyles.Sphere;
            orb.Radius             = 5f;
            orb.VerticalAxis.Value = 15f;

            camGo.AddComponent<CinemachineRotationComposer>();
        }

        private static void PositionPlayer()
        {
            GameObject player = GameObject.Find("Player");
            if (player != null && player.transform.position.y <= 0f)
                player.transform.position = new Vector3(0f, 0.1f, 0f);
        }

        // ── Wire references ───────────────────────────────────────────────────

        private static void WireReferences()
        {
            GameObject playerGo  = GameObject.Find("Player");
            GameObject inputGo   = GameObject.Find("InputSystem");
            GameObject worldGo   = GameObject.Find("WorldManager");
            GameObject dnGo      = GameObject.Find("DayNightCycle");
            GameObject audioGo   = GameObject.Find("AudioManager");
            GameObject hudGo     = GameObject.Find("HUD");
            GameObject lightGo   = GameObject.Find("Directional Light");

            if (playerGo == null || inputGo == null) return;

            TouchMapper      mapper   = inputGo.GetComponent<TouchMapper>();
            GestureRecognizer gesture = inputGo.GetComponent<GestureRecognizer>();
            InputHandler      handler = inputGo.GetComponent<InputHandler>();
            DayNightCycle     dn      = dnGo?.GetComponent<DayNightCycle>();
            WorldManager      wm      = worldGo?.GetComponent<WorldManager>();
            AudioManager      audio   = audioGo?.GetComponent<AudioManager>();
            MusicLayer        music   = audioGo?.GetComponent<MusicLayer>();
            SFXPool           sfx     = audioGo?.GetComponent<SFXPool>();
            HUD               hud     = hudGo?.GetComponent<HUD>();
            EnergyBar         bar     = hudGo?.GetComponent<EnergyBar>();
            JournalController journal = hudGo?.GetComponent<JournalController>();
            Light             light   = lightGo?.GetComponent<Light>();

            SetRef(mapper,   "_controller", playerGo.GetComponent<PlayerController>());
            SetRef(mapper,   "_combat",     playerGo.GetComponent<PlayerCombat>());
            SetRef(gesture,  "_mapper",     mapper);
            SetRef(handler,  "_gesture",    gesture);
            SetRef(handler,  "_mapper",     mapper);
            SetRef(dn,       "_directionalLight", light);
            SetRef(wm,       "_dayNight",   dn);
            SetRef(audio,    "_musicLayer", music);
            SetRef(audio,    "_sfxPool",    sfx);
            SetRef(hud,      "_health",     playerGo.GetComponent<PlayerHealth>());
            SetRef(bar,      "_energy",     playerGo.GetComponent<PlayerEnergy>());
            SetRef(journal,  "_hud",        hud);
        }

        // ── Enemy spawn ───────────────────────────────────────────────────────

        private static void SpawnAshWolf()
        {
            // Skip if already spawned
            if (GameObject.Find("AshWolf") != null) return;

            FenrirEditorUtils.EnsureTag("Enemy");

            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            go.name = "AshWolf";
            go.tag  = "Enemy";
            go.transform.position = new Vector3(0f, 0.1f, 14f);  // 14 units away — outside 8f detection range

            Shader urpLit = Shader.Find("Universal Render Pipeline/Lit");
            if (urpLit != null)
            {
                var mat = new Material(urpLit) { color = new Color(0.8f, 0.2f, 0.1f) };
                go.GetComponent<Renderer>().sharedMaterial = mat;
            }

            UnityEngine.AI.NavMeshAgent agent = go.AddComponent<UnityEngine.AI.NavMeshAgent>();
            agent.speed            = 4.5f;
            agent.stoppingDistance = 1.4f;
            agent.radius           = 0.4f;
            agent.height           = 2f;
            agent.avoidancePriority = 50;
            agent.obstacleAvoidanceType = UnityEngine.AI.ObstacleAvoidanceType.LowQualityObstacleAvoidance;
            // Prevent the NavMeshAgent from physically pushing the player's CharacterController
            agent.updatePosition   = true;
            agent.updateRotation   = true;

            Fenrir.Entities.Enemies.EnemyHealth  health  = go.AddComponent<Fenrir.Entities.Enemies.EnemyHealth>();
            Fenrir.Entities.Enemies.EnemyCombat  combat  = go.AddComponent<Fenrir.Entities.Enemies.EnemyCombat>();
            Fenrir.Entities.Enemies.EnemyAI      ai      = go.AddComponent<Fenrir.Entities.Enemies.EnemyAI>();
            go.AddComponent<Fenrir.Entities.Enemies.EnemyBase>();
            go.AddComponent<Fenrir.Entities.Enemies.EnemyTraitEmitter>();

            WriteFloat(health, "_maxHp",          80f);
            WriteFloat(combat, "_attackDamage",   12f);
            WriteFloat(ai,     "_detectionRange", 8f);
            WriteFloat(ai,     "_attackRange",    1.8f);

            Debug.Log("[FenrirSetup] ✓ Ash Wolf spawned.");
        }

        // ── NavMesh ───────────────────────────────────────────────────────────

        private static void BakeNavMesh()
        {
            GameObject ground = GameObject.Find("Ground");
            if (ground == null) { Debug.LogWarning("[FenrirSetup] Ground not found — NavMesh skipped."); return; }

            NavMeshSurface surface = ground.GetComponent<NavMeshSurface>()
                                  ?? ground.AddComponent<NavMeshSurface>();

            surface.collectObjects = CollectObjects.All;
            surface.useGeometry    = NavMeshCollectGeometry.PhysicsColliders;
            surface.BuildNavMesh();

            Debug.Log("[FenrirSetup] ✓ NavMesh baked.");
        }

        // ── Build Settings ────────────────────────────────────────────────────

        private static void SetBuildSettings()
        {
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(BootstrapScene, true),
                new EditorBuildSettingsScene(AwakeningScene, true),
                new EditorBuildSettingsScene(GameScene,      true),
            };
        }

        // ═════════════════════════════════════════════════════════════════════
        // Shared helpers
        // ═════════════════════════════════════════════════════════════════════

        private static GameObject GetOrCreate(string name)
        {
            return GameObject.Find(name) ?? new GameObject(name);
        }

        private static T EnsureComponent<T>(GameObject go) where T : Component
        {
            return go.GetComponent<T>() ?? go.AddComponent<T>();
        }

        private static void EnsureChild(GameObject parent, string childName)
        {
            if (parent.transform.Find(childName) != null) return;
            GameObject child = new(childName);
            child.transform.SetParent(parent.transform);
            child.SetActive(false);
        }

        private static void CreateEventSystem()
        {
            if (GameObject.Find("EventSystem") != null) return;
            GameObject es = new("EventSystem");
            es.AddComponent<UnityEngine.EventSystems.EventSystem>();
            es.AddComponent<InputSystemUIInputModule>();
        }

        private static void SetRef(Object target, string field, Object value)
        {
            if (target == null || value == null) return;
            SerializedObject   so = new(target);
            SerializedProperty sp = so.FindProperty(field);
            if (sp == null) { Debug.LogWarning($"[FenrirSetup] Field '{field}' not found on {target.GetType().Name}"); return; }
            sp.objectReferenceValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void WriteFloat(Component target, string field, float value)
        {
            SerializedObject   so = new(target);
            SerializedProperty sp = so.FindProperty(field);
            if (sp == null) { Debug.LogWarning($"[FenrirSetup] Field '{field}' not found on {target.GetType().Name}"); return; }
            sp.floatValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
