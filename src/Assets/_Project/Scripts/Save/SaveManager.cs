using System;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

namespace Fenrir.Save
{
    public class SaveManager : ISaveManager
    {
        public SaveData Current { get; private set; } = new SaveData();

        private bool _isDirty;
        private readonly string _savePath;

        public SaveManager(string overridePath = null)
        {
            _savePath = overridePath
                ?? Path.Combine(Application.persistentDataPath, Config.GameConfig.SaveFileName);
        }

        public async Task SaveAsync()
        {
            try
            {
                Current.SavedAt = DateTime.UtcNow;
                string json = JsonUtility.ToJson(Current, prettyPrint: true);
                await File.WriteAllTextAsync(_savePath, json);
                _isDirty = false;
                Debug.Log($"[SaveManager] Saved to {_savePath}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SaveManager] Save failed: {ex}");
            }
        }

        public async Task<bool> LoadAsync()
        {
            if (!File.Exists(_savePath))
            {
                Debug.Log("[SaveManager] No save file found — starting fresh.");
                return false;
            }

            try
            {
                string json = await File.ReadAllTextAsync(_savePath);
                Current = JsonUtility.FromJson<SaveData>(json);
                Debug.Log($"[SaveManager] Loaded save v{Current.Version}");
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SaveManager] Load failed: {ex}");
                return false;
            }
        }

        public void MarkDirty() => _isDirty = true;

        public bool IsDirty => _isDirty;
    }
}
