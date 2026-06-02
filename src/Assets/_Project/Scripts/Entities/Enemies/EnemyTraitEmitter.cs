using Fenrir.Traits;
using UnityEngine;

namespace Fenrir.Entities.Enemies
{
    /// <summary>
    /// Emits trait events related to enemy outcomes (kills, boss defeats, creature sparing).
    /// Called by game systems (CombatSystem, world scripting) — not by EnemyAI directly.
    /// </summary>
    [RequireComponent(typeof(EnemyBase))]
    public class EnemyTraitEmitter : MonoBehaviour
    {
        private EnemyBase _base;

        private void Awake() => _base = GetComponent<EnemyBase>();

        /// <summary>Player killed this enemy — emit boss signal if applicable.</summary>
        public void OnKilledByPlayer(bool isBoss, string bossId = null)
        {
            if (isBoss)
                BehaviorEventBus.Emit(new BossKilledEvent { BossId = bossId ?? _base.EnemyId });
        }

        /// <summary>Called when the player has killed this creature type 3+ times in sequence.</summary>
        public void OnHuntedRepeat()
            => BehaviorEventBus.Emit(new CreatureHuntedRepeatEvent());
    }
}
