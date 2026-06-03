#if UNITY_EDITOR
// Editor/FenrirSceneFinalize.cs
// Run via: Fenrir → Finalize EmberForest Scene
// Adds ground plane, SceneValidator, fixes EventSystem, positions Player correctly.
// Safe to run multiple times — all ops are idempotent.

using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem.UI;
using Unity.Cinemachine;

namespace Fenrir.Editor
{
    public static class FenrirSceneFinalize
    {
        [MenuItem("Fenrir/Finalize EmberForest Scene")]
        public static void FinalizeScene()
        {
            const string path = "Assets/_Project/Scenes/EmberForest.unity";
            var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);

            FixEventSystem();
            EnsureGroundPlane();
            EnsurePlayerMesh();
            PositionPlayer();
            EnsureCinemachineCamera();
            AddSceneValidator();
            EnsurePlayerTag();

            EditorSceneManager.SaveScene(scene);
            Debug.Log("[FenrirSceneFinalize] ✓ EmberForest finalized.");
        }

        // ── EventSystem: replace StandaloneInputModule with InputSystemUIInputModule ──

        private static void FixEventSystem()
        {
            var es = GameObject.Find("EventSystem");
            if (es == null) return;

            var old = es.GetComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            if (old != null)
            {
                Object.DestroyImmediate(old);
                Debug.Log("[FenrirSceneFinalize] ✓ Removed StandaloneInputModule.");
            }

            if (es.GetComponent<InputSystemUIInputModule>() == null)
            {
                es.AddComponent<InputSystemUIInputModule>();
                Debug.Log("[FenrirSceneFinalize] ✓ Added InputSystemUIInputModule.");
            }
        }

        // ── Ground plane ──────────────────────────────────────────────────────

        private static void EnsureGroundPlane()
        {
            if (GameObject.Find("Ground") != null)
            {
                Debug.Log("[FenrirSceneFinalize] Ground plane already exists.");
                return;
            }

            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
            ground.transform.position   = Vector3.zero;
            ground.transform.localScale = new Vector3(20f, 1f, 20f);
            Debug.Log("[FenrirSceneFinalize] ✓ Ground plane added (200x200 units).");
        }

        // ── Player positioning ────────────────────────────────────────────────

        private static void PositionPlayer()
        {
            var player = GameObject.Find("Player");
            if (player == null) return;

            // Only reposition if at origin or underground
            if (player.transform.position.y < 0f || player.transform.position == Vector3.zero)
            {
                player.transform.position = new Vector3(0f, 0.1f, 0f);
                Debug.Log("[FenrirSceneFinalize] ✓ Player positioned above ground.");
            }
        }

        // ── SceneValidator ────────────────────────────────────────────────────

        private static void AddSceneValidator()
        {
            var wm = GameObject.Find("WorldManager");
            if (wm == null) return;

            if (wm.GetComponent<Fenrir.Core.SceneValidator>() == null)
            {
                wm.AddComponent<Fenrir.Core.SceneValidator>();
                Debug.Log("[FenrirSceneFinalize] ✓ SceneValidator added to WorldManager.");
            }
        }

        // ── Player capsule mesh ───────────────────────────────────────────────

        private static void EnsurePlayerMesh()
        {
            var player = GameObject.Find("Player");
            if (player == null) return;
            if (player.GetComponentInChildren<MeshRenderer>() != null) return;

            var body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.name = "Mesh";
            body.transform.SetParent(player.transform);
            body.transform.localPosition = new Vector3(0f, 1f, 0f);
            Object.DestroyImmediate(body.GetComponent<CapsuleCollider>());

            var arrow = GameObject.CreatePrimitive(PrimitiveType.Cube);
            arrow.name = "ForwardArrow";
            arrow.transform.SetParent(player.transform);
            arrow.transform.localPosition = new Vector3(0f, 1f, 0.65f);
            arrow.transform.localScale    = new Vector3(0.15f, 0.15f, 0.4f);
            Object.DestroyImmediate(arrow.GetComponent<BoxCollider>());

            // Camera follow target at eye height
            if (player.transform.Find("CameraFollowTarget") == null)
            {
                var t = new GameObject("CameraFollowTarget");
                t.transform.SetParent(player.transform);
                t.transform.localPosition = new Vector3(0f, 1.6f, 0f);
            }

            Debug.Log("[FenrirSceneFinalize] ✓ Player mesh added.");
        }

        // ── Cinemachine camera ────────────────────────────────────────────────

        private static void EnsureCinemachineCamera()
        {
            if (GameObject.Find("CM FreeLook") != null) return;

            var player = GameObject.Find("Player");
            if (player == null) return;

            var followTarget = player.transform.Find("CameraFollowTarget");
            if (followTarget == null) return;

            // Ensure Main Camera has CinemachineBrain
            var mainCam = Camera.main;
            if (mainCam != null && mainCam.GetComponent<CinemachineBrain>() == null)
                mainCam.gameObject.AddComponent<CinemachineBrain>();

            // Create Cinemachine camera
            var camGo = new GameObject("CM FreeLook");
            camGo.transform.position = new Vector3(0f, 3f, -6f);

            var vcam = camGo.AddComponent<CinemachineCamera>();
            vcam.Follow = followTarget;
            vcam.LookAt = followTarget;
            vcam.Priority = new PrioritySettings { Value = 10 };

            var follow = camGo.AddComponent<CinemachineOrbitalFollow>();
            follow.OrbitStyle        = CinemachineOrbitalFollow.OrbitStyles.Sphere;
            follow.Radius            = 5f;
            follow.VerticalAxis.Value = 15f;

            camGo.AddComponent<CinemachineRotationComposer>();

            Debug.Log("[FenrirSceneFinalize] ✓ CinemachineCamera created.");
        }

        // ── Player tag ────────────────────────────────────────────────────────

        private static void EnsurePlayerTag()
        {
            var player = GameObject.Find("Player");
            if (player == null) return;

            if (player.tag != "Player")
            {
                player.tag = "Player";
                Debug.Log("[FenrirSceneFinalize] ✓ Player tag set.");
            }
        }
    }
}
#endif
