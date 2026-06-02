using System;
using Fenrir.Combat;
using Fenrir.Config;
using Fenrir.Traits;
using UnityEngine;

namespace Fenrir.Entities.Player
{
    /// <summary>
    /// Manages player HP, applies incoming attacks via HitStateManager, emits trait events on death.
    /// </summary>
    [RequireComponent(typeof(HitStateManager))]
    public class PlayerHealth : MonoBehaviour
    {
        [SerializeField] private float _maxHp = 100f;

        public float MaxHp           => _maxHp;
        public float Current         { get; private set; }
        public float CurrentNormalized => Current / _maxHp;
        public bool  IsDead          { get; private set; }

        public event Action OnDied;
        public event Action<float> OnDamaged;   // passes normalised HP after hit

        private HitStateManager _hitState;

        // Combat context for death classification
        private int   _hitsTakenThisCombat;
        private float _combatStartTime;
        private bool  _inCombat;
        private int   _consecutiveDeathsVsSameEnemy; // tracked by PlayerCombat, injected here

        public void SetConsecutiveDeaths(int v) => _consecutiveDeathsVsSameEnemy = v;

        private void Awake()
        {
            Current   = _maxHp;
            _hitState = GetComponent<HitStateManager>();
        }

        public void BeginCombat()
        {
            _inCombat         = true;
            _combatStartTime  = Time.time;
            _hitsTakenThisCombat = 0;
        }

        public void EndCombat() => _inCombat = false;

        /// <summary>Returns whether the hit landed (false if dodging).</summary>
        public bool TakeHit(AttackData attack, Element defenderElement)
        {
            if (IsDead) return false;
            if (_hitState.IsInvincible) return false;

            float damage = attack.BaseDamage;

            if (_hitState.IsPerfectBlockWindow)
            {
                // Perfect block absorbs all damage
                BehaviorEventBus.Emit(new PerfectBlockEvent());
                return true;
            }

            if (_hitState.IsBlocking)
            {
                damage *= (1f - GameConfig.BaseBlockDamageReduction);
                BehaviorEventBus.Emit(new BlockUsedEvent());
            }
            else
            {
                _hitsTakenThisCombat++;
                BehaviorEventBus.Emit(new HitTakenNoDodgeEvent());
            }

            damage = AttackResolver.Resolve(new AttackData(
                attack.Type, damage, attack.AttackerElement, attack.IsCounter, attack.IsAbility
            ), defenderElement);

            Current = Mathf.Max(0f, Current - damage);
            OnDamaged?.Invoke(CurrentNormalized);

            if (Current <= 0f) Die();
            return true;
        }

        public void Heal(float amount)
        {
            Current = Mathf.Min(_maxHp, Current + amount);
        }

        // ── Death ─────────────────────────────────────────────────────────────

        private void Die()
        {
            if (IsDead) return;
            IsDead = true;

            EmitDeathEvent();
            OnDied?.Invoke();
        }

        private void EmitDeathEvent()
        {
            // Ambush: died within 5s of entering combat
            if (_inCombat && (Time.time - _combatStartTime) < 5f)
            {
                // DeathAmbushEvent → no trait shift per design
                BehaviorEventBus.Emit(new DeathAmbushEvent());
                return;
            }

            // Pattern failure: 3rd+ death vs same enemy type
            if (_consecutiveDeathsVsSameEnemy >= 2)
            {
                BehaviorEventBus.Emit(new DeathPatternFailEvent());
                return;
            }

            // Reckless: took 3+ unhpblocked hits with no dodge or block used
            if (_hitsTakenThisCombat >= 3)
            {
                BehaviorEventBus.Emit(new DeathRecklessEvent());
                return;
            }

            // Overwhelmed: classified externally (set before death) — fallback
            BehaviorEventBus.Emit(new DeathOverwhelmedEvent());
        }

        public void Revive(float hpFraction = 1f)
        {
            IsDead  = false;
            Current = _maxHp * Mathf.Clamp01(hpFraction);
        }
    }
}
