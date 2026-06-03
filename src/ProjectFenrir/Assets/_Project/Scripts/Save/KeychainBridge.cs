using System;
using UnityEngine;

namespace Fenrir.Save
{
    /// <summary>
    /// Bridges Unity C# to native iOS Keychain for tamper-resistant slot seed storage.
    /// In Editor and non-iOS builds, falls back to PlayerPrefs (dev only).
    /// </summary>
    public static class KeychainBridge
    {
#if UNITY_IOS && !UNITY_EDITOR
        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern string Keychain_GetSeed(string key);

        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern bool Keychain_SetSeed(string key, string value);
#endif

        public static Guid GetOrCreateSeed(int slotIndex)
        {
            string key = string.Format(Config.GameConfig.KeychainSlotSeedKey, slotIndex);
            string existing = Read(key);

            if (!string.IsNullOrEmpty(existing) && Guid.TryParse(existing, out Guid parsed))
                return parsed;

            Guid newSeed = Guid.NewGuid();
            Write(key, newSeed.ToString());
            Debug.Log($"[KeychainBridge] Generated new seed for slot {slotIndex}");
            return newSeed;
        }

        private static string Read(string key)
        {
#if UNITY_IOS && !UNITY_EDITOR
            return Keychain_GetSeed(key);
#else
            return PlayerPrefs.GetString(key, null);
#endif
        }

        private static void Write(string key, string value)
        {
#if UNITY_IOS && !UNITY_EDITOR
            Keychain_SetSeed(key, value);
#else
            PlayerPrefs.SetString(key, value);
            PlayerPrefs.Save();
#endif
        }
    }
}
