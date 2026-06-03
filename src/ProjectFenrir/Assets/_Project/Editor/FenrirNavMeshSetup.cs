using Unity.AI.Navigation;
using UnityEditor;
using UnityEngine;

namespace Fenrir.Editor
{
    /// <summary>
    /// Fenrir → Setup → Bake NavMesh
    ///
    /// The project uses com.unity.ai.navigation (NavMeshSurface), not the legacy
    /// global bake. This script finds or creates a NavMeshSurface on the ground
    /// plane and bakes it so NavMeshAgents (enemies) can navigate.
    ///
    /// Run this in EmberForest.unity after spawning enemies.
    /// Re-run any time the ground geometry changes.
    /// </summary>
    public static class FenrirNavMeshSetup
    {
        [MenuItem("Fenrir/Setup/Bake NavMesh")]
        public static void BakeNavMesh()
        {
            // Find the ground plane. FenrirSceneFinalize names it "Ground".
            GameObject ground = GameObject.Find("Ground");
            if (ground == null)
            {
                Debug.LogError("[FenrirNavMeshSetup] 'Ground' GameObject not found in scene. " +
                               "Run Fenrir → Finalize EmberForest Scene first.");
                return;
            }

            // Get or add NavMeshSurface on the ground
            NavMeshSurface surface = ground.GetComponent<NavMeshSurface>();
            if (surface == null)
            {
                surface = Undo.AddComponent<NavMeshSurface>(ground);
                surface.collectObjects   = CollectObjects.All;   // bake entire scene geometry
                surface.useGeometry      = NavMeshCollectGeometry.PhysicsColliders;
                Debug.Log("[FenrirNavMeshSetup] NavMeshSurface added to Ground.");
            }

            // Bake synchronously in the Editor
            surface.BuildNavMesh();

            EditorUtility.SetDirty(ground);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(ground.scene);

            Debug.Log("[FenrirNavMeshSetup] NavMesh baked. Save the scene (Ctrl+S) before Play.");
        }
    }
}
