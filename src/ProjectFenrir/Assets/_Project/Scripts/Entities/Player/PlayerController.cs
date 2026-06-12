using Fenrir.Core;
using Fenrir.StateMachine;
using UnityEngine;

namespace Fenrir.Entities.Player
{
    /// <summary>
    /// Owns movement for the player entity.
    /// State transitions route through AppStateMachine (single source of truth).
    /// Receives movement vectors from InputHandler (via TouchMapper).
    /// Camera is handled separately by a CinemachineFreeLook.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private float _moveSpeed   = 5f;
        [SerializeField] private float _dodgeSpeed  = 12f;
        [SerializeField] private float _rotateSpeed = 720f;
        [SerializeField] private float _gravity     = -20f;

        // Read-only mirror — AppStateMachine is the authoritative source
        public PlayerState State =>
            ServiceLocator.TryGet<AppStateMachine>(out AppStateMachine sm)
                ? sm.PlayerState
                : PlayerState.Idle;

        private CharacterController _cc;
        private Vector3  _moveInput;
        private Vector3  _velocity;
        private bool     _dodgeRequested;

        private void Awake() => _cc = GetComponent<CharacterController>();

        // ── Input injection (called by TouchMapper) ───────────────────────────

        public void SetMoveInput(Vector2 input)
            => _moveInput = new Vector3(input.x, 0f, input.y);

        public void RequestDodge() => _dodgeRequested = true;

        // ── Lifecycle ─────────────────────────────────────────────────────────

        private void Update()
        {
            if (State == PlayerState.Dead) return;

            ApplyGravity();
            HandleMovement();
        }

        private void ApplyGravity()
        {
            if (_cc.isGrounded && _velocity.y < 0f)
                _velocity.y = -2f;   // stick to ground

            _velocity.y += _gravity * Time.deltaTime;
        }

        private void HandleMovement()
        {
            Vector3 worldMove = TransformMoveInput(_moveInput);
            float   speed     = _moveSpeed;

            if (_dodgeRequested)
            {
                _dodgeRequested = false;
                worldMove       = worldMove == Vector3.zero ? transform.forward : worldMove.normalized;
                speed           = _dodgeSpeed;
                TransitionTo(PlayerState.Dodging);
            }
            else if (worldMove.sqrMagnitude > 0.01f)
            {
                RotateToward(worldMove);
                if (State == PlayerState.Idle) TransitionTo(PlayerState.Moving);
            }
            else
            {
                if (State == PlayerState.Moving) TransitionTo(PlayerState.Idle);
            }

            _cc.Move((worldMove * speed + _velocity) * Time.deltaTime);
        }

        /// <summary>Rotates move input relative to the main camera's horizontal orientation.</summary>
        private Vector3 TransformMoveInput(Vector3 input)
        {
            if (input.sqrMagnitude < 0.01f) return Vector3.zero;

            Transform cam = Camera.main != null ? Camera.main.transform : null;
            if (cam == null) return input.normalized;

            Vector3 camForward = Vector3.ProjectOnPlane(cam.forward, Vector3.up).normalized;
            Vector3 camRight   = Vector3.ProjectOnPlane(cam.right,   Vector3.up).normalized;

            return (camForward * input.z + camRight * input.x).normalized;
        }

        private void RotateToward(Vector3 direction)
        {
            if (direction == Vector3.zero) return;
            Quaternion target = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, target,
                _rotateSpeed * Time.deltaTime);
        }

        // ── State transitions (delegate to AppStateMachine) ───────────────────

        public void TransitionTo(PlayerState next)
        {
            if (ServiceLocator.TryGet<AppStateMachine>(out AppStateMachine sm))
                sm.TrySetPlayerState(next);
        }

        public void SetDead() => TransitionTo(PlayerState.Dead);
        public void SetIdle() => TransitionTo(PlayerState.Idle);
    }
}
