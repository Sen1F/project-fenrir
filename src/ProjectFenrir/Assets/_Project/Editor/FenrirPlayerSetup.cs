#if UNITY_EDITOR
// Editor/FenrirPlayerSetup.cs
// Run via: Fenrir → Setup Player & Camera
// Adds a visible capsule mesh to the Player and a follow target for Cinemachine.
// Cinemachine camera is added manually via Component menu after this runs.

using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Fenrir.Editor
{
    public static class FenrirPlayerSetup
    {
        [MenuItem("Fenrir/Setup Player & Camera")]
        public static void Setup()
        {
            const string scenePath = "Assets/_Project/Scenes/EmberForest.unity";
            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

            SetupPlayerMesh();
            SetupFollowTarget();
            AddGroundPlane();

            EditorSceneManager.SaveScene(scene);
            Debug.Log("[FenrirPlayerSetup] ✓ Player mesh and follow target set up. " +
                      "Add a CinemachineCamera via Component menu and set Follow/LookAt to CameraFollowTarget.");
        }

        // ── Player capsule ────────────────────────────────────────────────────

        private static void SetupPlayerMesh()
        {
            var player = GameObject.Find("Player");
            if (player == null) { Debug.LogError("[FenrirPlayerSetup] Player not found."); return; }

            if (player.GetComponentInChildren<MeshRenderer>() != null)
            {
                Debug.Log("[FenrirPlayerSetup] Player mesh already exists — skipping.");
                return;
            }

            // Capsule body
            var body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.name = "Mesh";
            body.transform.SetParent(player.transform);
            body.transform.localPosition = new Vector3(0f, 1f, 0f);
            body.transform.localScale    = Vector3.one;
            Object.DestroyImmediate(body.GetComponent<CapsuleCollider>());

            // Forward indicator
            var arrow = GameObject.CreatePrimitive(PrimitiveType.Cube);
            arrow.name = "ForwardArrow";
            arrow.transform.SetParent(player.transform);
            arrow.transform.localPosition = new Vector3(0f, 1f, 0.65f);
            arrow.transform.localScale    = new Vector3(0.15f, 0.15f, 0.4f);
            Object.DestroyImmediate(arrow.GetComponent<BoxCollider>());

            player.transform.position = Vector3.zero;
            Debug.Log("[FenrirPlayerSetup] ✓ Player capsule mesh added.");
        }

        // ── Camera follow target ──────────────────────────────────────────────

        private static void SetupFollowTarget()
        {
            var player = GameObject.Find("Player");
            if (player == null) return;

            if (player.transform.Find("CameraFollowTarget") != null) return;

            var target = new GameObject("CameraFollowTarget");
            target.transform.SetParent(player.transform);
            target.transform.localPosition = new Vector3(0f, 1.6f, 0f); // eye height

            Debug.Log("[FenrirPlayerSetup] ✓ CameraFollowTarget created on Player.");
        }

        // ── Ground plane ──────────────────────────────────────────────────────

        private static void AddGroundPlane()
        {
            if (GameObject.Find("Ground") != null) return;

            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
            ground.transform.position   = Vector3.zero;
            ground.transform.localScale = new Vector3(10f, 1f, 10f);

            Debug.Log("[FenrirPlayerSetup] ✓ Ground plane added.");
        }
    }
}
#endif
