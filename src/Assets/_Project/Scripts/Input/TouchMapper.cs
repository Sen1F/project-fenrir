using Fenrir.Entities.Player;
using UnityEngine;

namespace Fenrir.Input
{
    /// <summary>
    /// Bridges gesture intents to the player entity.
    /// GestureRecognizer calls this; it forwards commands to PlayerController / PlayerCombat.
    /// </summary>
    public class TouchMapper : MonoBehaviour
    {
        [SerializeField] private PlayerController _controller;
        [SerializeField] private PlayerCombat     _combat;

        // Optional: closest enemy lock-on target (set by combat system)
        public GameObject CurrentTarget { get; set; }

        // ── Movement ──────────────────────────────────────────────────────────

        public void OnMoveInput(Vector2 input) => _controller.SetMoveInput(input);

        // ── Combat ────────────────────────────────────────────────────────────

        public void OnLightAttack()  => _combat.DoLightAttack(CurrentTarget);
        public void OnHeavyAttack()  => _combat.DoHeavyAttack(CurrentTarget);
        public void OnAbility()      => _combat.DoAbility(CurrentTarget);
        public void OnDodge()
        {
            _controller.RequestDodge();
            _combat.DoDodge();
        }
        public void OnBlockHeld()    => _combat.DoBlock();
        public void OnBlockReleased()=> _combat.EndBlock();
    }
}
