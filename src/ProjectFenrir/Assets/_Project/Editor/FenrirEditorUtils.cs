using UnityEditor;
using UnityEngine;

namespace Fenrir.Editor
{
    /// <summary>
    /// Shared utilities used by all Fenrir editor setup scripts.
    /// </summary>
    internal static class FenrirEditorUtils
    {
        /// <summary>
        /// Adds <paramref name="tag"/> to TagManager.asset if not already registered.
        /// Unity throws UnityException on go.tag = "X" when "X" is not in TagManager.
        /// </summary>
        internal static void EnsureTag(string tag)
        {
            SerializedObject tagManager = new SerializedObject(
                AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);

            SerializedProperty tagsProp = tagManager.FindProperty("tags");

            for (int i = 0; i < tagsProp.arraySize; i++)
            {
                if (tagsProp.GetArrayElementAtIndex(i).stringValue == tag)
                    return;
            }

            tagsProp.InsertArrayElementAtIndex(tagsProp.arraySize);
            tagsProp.GetArrayElementAtIndex(tagsProp.arraySize - 1).stringValue = tag;
            tagManager.ApplyModifiedProperties();

            Debug.Log($"[FenrirEditor] Registered tag '{tag}' in TagManager.");
        }

        /// <summary>
        /// Adds a user layer to TagManager.asset if not already present.
        /// Unity has 32 layers; user layers start at index 8.
        /// </summary>
        internal static void EnsureLayer(string layerName)
        {
            SerializedObject tagManager = new SerializedObject(
                AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);

            // User layers are stored as "User Layer 8" … "User Layer 31"
            for (int i = 8; i <= 31; i++)
            {
                SerializedProperty sp = tagManager.FindProperty($"layers.Array.data[{i}]");
                if (sp == null) continue;
                if (sp.stringValue == layerName) return;           // already exists
            }

            // Find first empty slot
            for (int i = 8; i <= 31; i++)
            {
                SerializedProperty sp = tagManager.FindProperty($"layers.Array.data[{i}]");
                if (sp == null) continue;
                if (string.IsNullOrEmpty(sp.stringValue))
                {
                    sp.stringValue = layerName;
                    tagManager.ApplyModifiedProperties();
                    Debug.Log($"[FenrirEditor] Registered layer '{layerName}' at index {i}.");
                    return;
                }
            }

            Debug.LogWarning($"[FenrirEditor] No free layer slot for '{layerName}' — all 32 layers in use.");
        }

        /// <summary>
        /// Sets whether two named layers ignore each other in the Physics collision matrix.
        /// Call after EnsureLayer so both layers are registered before reading their indices.
        /// </summary>
        internal static void SetLayerCollision(string layerA, string layerB, bool ignore)
        {
            int a = LayerMask.NameToLayer(layerA);
            int b = LayerMask.NameToLayer(layerB);

            if (a < 0 || b < 0)
            {
                Debug.LogWarning($"[FenrirEditor] SetLayerCollision: layer '{layerA}' or '{layerB}' not found. Run EnsureLayer first.");
                return;
            }

            Physics.IgnoreLayerCollision(a, b, ignore);
            Debug.Log($"[FenrirEditor] Layer collision {layerA}↔{layerB} ignore={ignore}.");
        }
    }
}
