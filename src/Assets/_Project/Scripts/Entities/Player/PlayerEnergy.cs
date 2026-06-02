using Fenrir.Config;
using System;
using UnityEngine;

namespace Fenrir.Entities.Player
{
    /// <summary>
    /// Manages the player energy bar. Energy recharges over time.
    /// UI reads CurrentNormalized (0–1) and ColorThreshold to colour the bar.
    /// </summary>
    public class PlayerEnergy : MonoBehaviour
    {
        // ── Tunables ──────────────────────────────────────────────────────────
        [SerializeField] private float _rechargePerSecond = 8f;   // tuned per design doc

        // ── State ─────────────────────────────────────────────────────────────
        public float Current           { get; private set; } = GameConfig.EnergyMax;
        public float Max               => GameConfig.EnergyMax;
        public float CurrentNormalized => Current / Max;

        /// <summary>Raised whenever energy changes (e.g. to drive UI colour).</summary>
        public event Action<float> OnEnergyChanged;

        // ── Public API ────────────────────────────────────────────────────────

        /// <returns>True if enough energy was available and was consumed.</returns>
        public bool TrySpend(float amount)
        {
            if (Current < amount) return false;
            Current = Mathf.Max(0f, Current - amount);
            OnEnergyChanged?.Invoke(Current);
            return true;
        }

        public bool HasEnergy(float amount) => Current >= amount;

        public bool IsLow => CurrentNormalized <= GameConfig.EnergyLowThreshold;

        // ── Lifecycle ─────────────────────────────────────────────────────────

        private void Update()
        {
            if (Current >= Max) return;
            Current = Mathf.Min(Max, Current + _rechargePerSecond * Time.deltaTime);
            OnEnergyChanged?.Invoke(Current);
        }
    }
}
