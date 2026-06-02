using UnityEngine;

namespace Fenrir.Combat
{
    /// <summary>
    /// Tracks per-entity block / perfect-block / dodge windows.
    /// Attach one instance to any entity that can receive hits (player, enemy).
    /// </summary>
    public class HitStateManager : MonoBehaviour
    {
        // ── Tunables ──────────────────────────────────────────────────────────
        [SerializeField] private float _blockWindowDuration        = 0.25f;
        [SerializeField] private float _perfectBlockWindowDuration = 0.12f;
        [SerializeField] private float _dodgeInvincibilityDuration = 0.4f;

        // ── State ─────────────────────────────────────────────────────────────
        public bool IsBlocking        { get; private set; }
        public bool IsDodging         { get; private set; }
        public bool IsInvincible      { get; private set; }

        private float _blockTimer;
        private float _perfectBlockTimer;
        private float _dodgeTimer;

        // ── Public API ────────────────────────────────────────────────────────

        public void BeginBlock()
        {
            IsBlocking          = true;
            _blockTimer         = _blockWindowDuration;
            _perfectBlockTimer  = _perfectBlockWindowDuration;
        }

        public void EndBlock() => IsBlocking = false;

        public bool IsPerfectBlockWindow => IsBlocking && _perfectBlockTimer > 0f;

        public void BeginDodge()
        {
            IsDodging    = true;
            IsInvincible = true;
            _dodgeTimer  = _dodgeInvincibilityDuration;
        }

        // ── Lifecycle ─────────────────────────────────────────────────────────

        private void Update()
        {
            if (_perfectBlockTimer > 0f)
                _perfectBlockTimer -= Time.deltaTime;

            if (_blockTimer > 0f)
            {
                _blockTimer -= Time.deltaTime;
                if (_blockTimer <= 0f) IsBlocking = false;
            }

            if (_dodgeTimer > 0f)
            {
                _dodgeTimer -= Time.deltaTime;
                if (_dodgeTimer <= 0f)
                {
                    IsDodging    = false;
                    IsInvincible = false;
                }
            }
        }
    }
}
