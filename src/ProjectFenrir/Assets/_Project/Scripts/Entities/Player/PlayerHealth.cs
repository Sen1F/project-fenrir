using System;
using Fenrir.Combat;
using Fenrir.Config;
using Fenrir.Core;
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

        public float MaxHp             => _maxHp;
        public float Current           { get; private set; }
        public float CurrentNormalized => Current / _maxHp;
        public bool  IsDead            { get; private set; }

        public event Action        OnDied;
        public event Action<float> OnDamaged;      // normalised HP after hit

        /// <summary>Set immediately before OnDied fires. Read by DeathMessageProvider.</summary>
        public PlayerDeathType LastDeathType { get; private set; } = PlayerDeathType.None;

        private HitStateManager _hitState;

        // Death classification data
        private int   _hitsTakenThisCombat;
        private float _combatStartTime;
        private bool  _inCombat;
        private int   _consecutiveDeathsVsSameEnemy;  // injected by PlayerCombat
        private bool  _attackedWhileLowHp;            // set by NotifyAttackAttempt

        public void SetConsecutiveDeaths(int v) => _consecutiveDeathsVsSameEnemy = v;

        /// <summary>
        /// Called by PlayerCombat before each attack attempt.
        /// Records whether the player was at low HP — needed for Sacrifice death classification.
        /// </summary>
        public void NotifyAttackAttempt()
        {
            if (CurrentNormalized < GameConfig.SacrificeHpThreshold)
                _attackedWhileLowHp = true;
        }

        private void Awake()
        {
            Current   = _maxHp;
            _hitState = GetComponent<HitStateManager>();
        }

        public void BeginCombat()
        {
            _inCombat              = true;
            _combatStartTime       = Time.time;
            _hitsTakenThisCombat   = 0;
            _attackedWhileLowHp    = false;
        }

        public void EndCombat() => _inCombat = false;

        /// <summary>Returns whether the hit landed (false if currently invincible).</summary>
        public bool TakeHit(AttackData attack, Element defenderElement)
        {
            if (IsDead)               return false;
            if (_hitState.IsInvincible) return false;

            float damage = attack.BaseDamage;

            if (_hitState.IsPerfectBlockWindow)
            {
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
                // CombatContext increments its own counter — no re-emit
                if (ServiceLocator.TryGet<CombatContext>(out CombatContext ctx))
                    ctx.NotifyHitTaken();
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
            // 1. Ambush — died within 5s of combat start → no trait shift
            if (_inCombat && (Time.time - _combatStartTime) < 5f)
            {
                LastDeathType = PlayerDeathType.Ambush;
                BehaviorEventBus.Emit(new DeathAmbushEvent());
                return;
            }

            // 2. Pattern failure — 3rd+ death vs the same enemy type
            if (_consecutiveDeathsVsSameEnemy >= 2)
            {
                LastDeathType = PlayerDeathType.PatternFail;
                BehaviorEventBus.Emit(new DeathPatternFailEvent());
                return;
            }

            // 3. Sacrifice — attacked at low HP AND (group fight OR minor damage dealt)
            if (_attackedWhileLowHp && IsSacrificeDeath())
            {
                LastDeathType = PlayerDeathType.Sacrifice;
                BehaviorEventBus.Emit(new DeathSacrificeEvent());
                return;
            }

            // 4. Reckless — 3+ unblocked hits, no dodge or block
            if (_hitsTakenThisCombat >= 3)
            {
                LastDeathType = PlayerDeathType.Reckless;
                BehaviorEventBus.Emit(new DeathRecklessEvent());
                return;
            }

            // 5. Overwhelmed — fallback
            LastDeathType = PlayerDeathType.Overwhelmed;
            BehaviorEventBus.Emit(new DeathOverwhelmedEvent());
        }

        /// <summary>
        /// Sacrifice conditions: minor damage dealt (< SacrificeMinorDamageMax of any
        /// enemy's max HP) OR the encounter had 2+ enemies simultaneously.
        /// </summary>
        private static bool IsSacrificeDeath()
        {
            if (!ServiceLocator.TryGet<CombatContext>(out CombatContext ctx)) return false;

            // Group fight = enemies that are aggroed or alerted at the moment of death
            // (live query via EnemyAI.IsAwareOfPlayer — not a stale snapshot)
            bool groupFight  = ctx.GetAwareEnemyCount() >= GameConfig.SacrificeGroupEnemyMin;
            bool minorDamage = ctx.MaxDamageFractionDealt < GameConfig.SacrificeMinorDamageMax;

            return groupFight || minorDamage;
        }

        public void Revive(float hpFraction = 1f)
        {
            IsDead              = false;
            _attackedWhileLowHp = false;
            LastDeathType       = PlayerDeathType.None;
            Current             = _maxHp * Mathf.Clamp01(hpFraction);
        }
    }
}
