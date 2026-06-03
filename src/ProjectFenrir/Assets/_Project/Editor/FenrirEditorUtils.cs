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
    }
}
