using UnityEngine;
using UnityEngine.InputSystem;

namespace Fenrir.Input
{
    /// <summary>
    /// Raw input layer using Unity Input System (new).
    /// Forwards to GestureRecognizer (touch) or processes keyboard directly (Editor).
    /// </summary>
    public class InputHandler : MonoBehaviour
    {
        [SerializeField] private GestureRecognizer _gesture;
        [SerializeField] private TouchMapper       _mapper;

        private void Awake()
        {
            // Auto-find on same GameObject if not wired via Inspector
            if (_gesture == null) _gesture = GetComponent<GestureRecognizer>();
            if (_mapper  == null) _mapper  = GetComponent<TouchMapper>();
        }

        private void Update()
        {
            if (_mapper == null) return;
            if (Keyboard.current == null) return;

#if UNITY_IOS || UNITY_ANDROID
            ProcessTouches();
#else
            ProcessKeyboard();
#endif
        }

        private void ProcessTouches()
        {
            _gesture?.Tick();
        }

        private void ProcessKeyboard()
        {
            var kb = Keyboard.current;

            float h = 0f, v = 0f;
            if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) h =  1f;
            if (kb.aKey.isPressed || kb.leftArrowKey.isPressed)  h = -1f;
            if (kb.wKey.isPressed || kb.upArrowKey.isPressed)    v =  1f;
            if (kb.sKey.isPressed || kb.downArrowKey.isPressed)  v = -1f;

            _mapper.OnMoveInput(new Vector2(h, v));

            if (kb.spaceKey.wasPressedThisFrame) _mapper.OnDodge();
            if (kb.jKey.wasPressedThisFrame)     _mapper.OnLightAttack();
            if (kb.kKey.wasPressedThisFrame)     _mapper.OnHeavyAttack();
            if (kb.lKey.wasPressedThisFrame)     _mapper.OnAbility();

            if (kb.bKey.isPressed) _mapper.OnBlockHeld();
            else                   _mapper.OnBlockReleased();
        }
    }
}
