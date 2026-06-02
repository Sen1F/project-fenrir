using UnityEngine;
using UnityEngine.InputSystem;

namespace Fenrir.Input
{
    /// <summary>
    /// Raw input layer. Reads Unity's new Input System touch/gamepad events
    /// and forwards normalised values to GestureRecognizer and TouchMapper.
    /// This class knows nothing about game entities.
    /// </summary>
    public class InputHandler : MonoBehaviour
    {
        [SerializeField] private GestureRecognizer _gesture;
        [SerializeField] private TouchMapper       _mapper;

        private void Update()
        {
#if UNITY_IOS || UNITY_ANDROID
            ProcessTouches();
#else
            ProcessKeyboard();  // Editor / Mac fallback
#endif
        }

        // ── Mobile ────────────────────────────────────────────────────────────

        private void ProcessTouches()
        {
            // Unity's new input system touch processing is deferred to
            // GestureRecognizer, which owns the left/right split logic.
            _gesture.Tick();
        }

        // ── Editor / Keyboard ─────────────────────────────────────────────────

        private void ProcessKeyboard()
        {
            float h = 0f, v = 0f;
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) h =  1f;
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)  h = -1f;
            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed)    v =  1f;
            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed)  v = -1f;

            _mapper.OnMoveInput(new Vector2(h, v));

            if (Keyboard.current.spaceKey.wasPressedThisFrame) _mapper.OnDodge();
            if (Keyboard.current.jKey.wasPressedThisFrame)     _mapper.OnLightAttack();
            if (Keyboard.current.kKey.wasPressedThisFrame)     _mapper.OnHeavyAttack();
            if (Keyboard.current.lKey.wasPressedThisFrame)     _mapper.OnAbility();
            if (Keyboard.current.bKey.isPressed)               _mapper.OnBlockHeld();
            else                                               _mapper.OnBlockReleased();
        }
    }
}
