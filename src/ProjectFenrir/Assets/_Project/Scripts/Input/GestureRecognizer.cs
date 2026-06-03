using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

namespace Fenrir.Input
{
    /// <summary>
    /// Splits the screen into left (movement joystick) and right (swipe actions) halves.
    /// Classifies right-side swipes into attack/dodge/block intents and delegates to TouchMapper.
    /// </summary>
    public class GestureRecognizer : MonoBehaviour
    {
        [SerializeField] private TouchMapper _mapper;

        // ── Swipe thresholds ──────────────────────────────────────────────────
        [SerializeField] private float _swipeDistanceThreshold = 60f;   // pixels
        [SerializeField] private float _holdThreshold          = 0.3f;  // seconds → block

        private Vector2 _leftAnchor;
        private bool    _leftActive;
        private int     _leftFingerId = -1;

        private Vector2 _rightStartPos;
        private float   _rightStartTime;
        private int     _rightFingerId = -1;
        private bool    _rightActive;

        private void Awake()
        {
            if (_mapper == null) _mapper = GetComponent<TouchMapper>();
        }

        private void OnEnable()  => EnhancedTouchSupport.Enable();
        private void OnDisable() => EnhancedTouchSupport.Disable();

        public void Tick()
        {
            foreach (var touch in Touch.activeTouches)
            {
                float screenHalf = Screen.width * 0.5f;
                bool  isLeft     = touch.startScreenPosition.x < screenHalf;

                switch (touch.phase)
                {
                    case UnityEngine.InputSystem.TouchPhase.Began:
                        if (isLeft  && _leftFingerId  < 0) BeginLeft(touch);
                        if (!isLeft && _rightFingerId < 0) BeginRight(touch);
                        break;

                    case UnityEngine.InputSystem.TouchPhase.Moved:
                    case UnityEngine.InputSystem.TouchPhase.Stationary:
                        if (touch.touchId == _leftFingerId)  UpdateLeft(touch);
                        if (touch.touchId == _rightFingerId) UpdateRight(touch);
                        break;

                    case UnityEngine.InputSystem.TouchPhase.Ended:
                    case UnityEngine.InputSystem.TouchPhase.Canceled:
                        if (touch.touchId == _leftFingerId)  EndLeft(touch);
                        if (touch.touchId == _rightFingerId) EndRight(touch);
                        break;
                }
            }
        }

        // ── Left (movement joystick) ──────────────────────────────────────────

        private void BeginLeft(Touch t)
        {
            _leftFingerId = t.touchId;
            _leftAnchor   = t.startScreenPosition;
            _leftActive   = true;
        }

        private void UpdateLeft(Touch t)
        {
            if (!_leftActive) return;
            Vector2 delta = t.screenPosition - _leftAnchor;
            // Normalise by half-screen height for consistent feel across devices
            Vector2 norm  = delta / (Screen.height * 0.3f);
            norm          = Vector2.ClampMagnitude(norm, 1f);
            _mapper.OnMoveInput(norm);
        }

        private void EndLeft(Touch t)
        {
            _leftFingerId = -1;
            _leftActive   = false;
            _mapper.OnMoveInput(Vector2.zero);
        }

        // ── Right (swipe actions) ─────────────────────────────────────────────

        private void BeginRight(Touch t)
        {
            _rightFingerId  = t.touchId;
            _rightStartPos  = t.startScreenPosition;
            _rightStartTime = Time.time;
            _rightActive    = true;
        }

        private void UpdateRight(Touch t)
        {
            if (!_rightActive) return;

            // Block: finger held without moving
            float heldDuration = Time.time - _rightStartTime;
            float moved        = Vector2.Distance(t.screenPosition, _rightStartPos);

            if (moved < 20f && heldDuration > _holdThreshold)
                _mapper.OnBlockHeld();
        }

        private void EndRight(Touch t)
        {
            _rightFingerId = -1;
            _rightActive   = false;
            _mapper.OnBlockReleased();

            float heldDuration = Time.time - _rightStartTime;
            float moved        = Vector2.Distance(t.screenPosition, _rightStartPos);

            if (moved < 20f && heldDuration < _holdThreshold)
            {
                // Tap → light attack
                _mapper.OnLightAttack();
                return;
            }

            if (moved < _swipeDistanceThreshold) return;

            Vector2 dir     = (t.screenPosition - _rightStartPos).normalized;
            float   angle   = Vector2.SignedAngle(Vector2.right, dir);

            if (angle > 45f && angle < 135f)
                _mapper.OnDodge();          // swipe up → dodge
            else if (Mathf.Abs(angle) < 45f)
                _mapper.OnHeavyAttack();    // swipe right → heavy
            else if (Mathf.Abs(angle) > 135f)
                _mapper.OnAbility();        // swipe left → ability
            // swipe down → no action (reserved)
        }
    }
}
