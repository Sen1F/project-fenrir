using Fenrir.Core;
using Fenrir.Save;
using Fenrir.UI;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Fenrir.Input
{
    /// <summary>
    /// Raw input layer using Unity Input System (new).
    /// On iOS/Android: forwards touch to GestureRecognizer.
    /// In Editor/PC: processes keyboard directly.
    ///
    /// Key bindings (Editor/PC):
    ///   WASD / Arrows — movement
    ///   Space         — dodge
    ///   J             — light attack
    ///   K             — heavy attack
    ///   L             — ability
    ///   B (hold)      — block
    ///   Tab           — toggle journal  (dev builds: handled by TraitDebugHUD for overlay toggle)
    ///   F5            — quick save      (DEVELOPMENT_BUILD / Editor only)
    /// </summary>
    public class InputHandler : MonoBehaviour
    {
        [SerializeField] private GestureRecognizer  _gesture;
        [SerializeField] private TouchMapper        _mapper;
        [SerializeField] private JournalController  _journal;

        private void Awake()
        {
            if (_gesture == null) _gesture = GetComponent<GestureRecognizer>();
            if (_mapper  == null) _mapper  = GetComponent<TouchMapper>();
            if (_journal == null) _journal = FindAnyObjectByType<JournalController>();
        }

        private void Update()
        {
            if (_mapper == null) return;

#if UNITY_IOS || UNITY_ANDROID
            // Touch path — Keyboard.current is null on device; guard must NOT block this
            ProcessTouches();
#else
            // Editor / PC — keyboard may still be null if Input System isn't configured
            if (Keyboard.current != null)
                ProcessKeyboard();
#endif
        }

        private void ProcessTouches() => _gesture?.Tick();

        private void ProcessKeyboard()
        {
            Keyboard kb = Keyboard.current;

            // ── Movement ─────────────────────────────────────────────────────
            float h = 0f, v = 0f;
            if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) h =  1f;
            if (kb.aKey.isPressed || kb.leftArrowKey.isPressed)  h = -1f;
            if (kb.wKey.isPressed || kb.upArrowKey.isPressed)    v =  1f;
            if (kb.sKey.isPressed || kb.downArrowKey.isPressed)  v = -1f;

            _mapper.OnMoveInput(new Vector2(h, v));

            // ── Combat ────────────────────────────────────────────────────────
            if (kb.spaceKey.wasPressedThisFrame) _mapper.OnDodge();
            if (kb.jKey.wasPressedThisFrame)     _mapper.OnLightAttack();
            if (kb.kKey.wasPressedThisFrame)     _mapper.OnHeavyAttack();
            if (kb.lKey.wasPressedThisFrame)     _mapper.OnAbility();

            if (kb.bKey.isPressed) _mapper.OnBlockHeld();
            else                   _mapper.OnBlockReleased();

            // ── UI ────────────────────────────────────────────────────────────
            // Tab is also consumed by TraitDebugHUD for overlay toggle in dev builds.
            // Both can coexist: Tab toggles the debug panel AND the journal.
            // If that's undesirable, move journal to a different key (e.g. G).
            if (kb.tabKey.wasPressedThisFrame) _journal?.Toggle();

            // ── Dev tools (stripped from release builds) ──────────────────────
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (kb.f5Key.wasPressedThisFrame)
                TriggerQuickSave();
#endif
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private static void TriggerQuickSave()
        {
            if (!ServiceLocator.TryGet<ISaveManager>(out ISaveManager save))
            {
                Debug.LogWarning("[InputHandler] Quick save failed — ISaveManager not registered.");
                return;
            }
            save.MarkDirty();
            // Fire-and-forget: awaiting in Update is not valid; SaveAsync internally
            // handles re-entrancy. The MarkDirty call above guarantees the write occurs
            // even if the coroutine hasn't yielded yet.
            _ = save.SaveAsync();
            Debug.Log("[Dev] Quick save triggered (F5). Writing to persistentDataPath.");
        }
#endif
    }
}
