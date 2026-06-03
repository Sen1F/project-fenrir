using System.Collections.Generic;
using Fenrir.Combat;
using Fenrir.Config;
using Fenrir.Core;
using Fenrir.Save;
using Fenrir.Traits;
using UnityEngine;

namespace Fenrir.Entities.Player
{
    /// <summary>
    /// Owns the player's attack/block/dodge decision logic.
    /// Produces AttackData structs, passes to target health, then emits trait events
    /// ONLY after confirming the hit landed.
    /// Tracks per-enemy-type consecutive deaths for pattern-failure detection.
    /// </summary>
    [RequireComponent(typeof(PlayerHealth))]
    [RequireComponent(typeof(PlayerEnergy))]
    [RequireComponent(typeof(PlayerTraitEmitter))]
    [RequireComponent(typeof(HitStateManager))]
    public class PlayerCombat : MonoBehaviour
    {
        [SerializeField] private float _lightAttackDamage = 15f;
        [SerializeField] private float _heavyAttackDamage = 35f;
        [SerializeField] private float _abilityDamage     = 50f;
        [SerializeField] private float _abilityEnergyCost = 40f;

        private PlayerHealth       _health;
        private PlayerEnergy       _energy;
        private PlayerTraitEmitter _emitter;
        private HitStateManager    _hitState;

        // Per-enemy-ID death counts (keyed by EnemyBase.EnemyId)
        private readonly Dictionary<string, int> _deathsByEnemyId = new();
        private string _currentEnemyId;
        private bool   _usedDodgeThisCombat;

        private Element PlayerElement => ServiceLocator.TryGet<ISaveManager>(out ISaveManager s)
            ? s.Current.Character.CurrentElement
            : Element.Fire;

        private void Awake()
        {
            _health   = GetComponent<PlayerHealth>();
            _energy   = GetComponent<PlayerEnergy>();
            _emitter  = GetComponent<PlayerTraitEmitter>();
            _hitState = GetComponent<HitStateManager>();

            _health.OnDied += HandleDeath;
        }

        // ── Input forwarding ──────────────────────────────────────────────────

        public void DoLightAttack(GameObject target)
        {
            var attack = new AttackData(AttackType.Light, _lightAttackDamage, PlayerElement);
            if (TryHitTarget(target, attack))
                _emitter.OnLightAttackLanded();   // emit ONLY after confirmed hit
        }

        public void DoHeavyAttack(GameObject target)
        {
            var attack = new AttackData(AttackType.Heavy, _heavyAttackDamage, PlayerElement);
            if (TryHitTarget(target, attack))
                _emitter.OnHeavyAttackLanded();
        }

        public void DoAbility(GameObject target)
        {
            if (!_energy.TrySpend(_abilityEnergyCost)) return;
            var attack = new AttackData(AttackType.Ability, _abilityDamage, PlayerElement, isAbility: true);
            if (TryHitTarget(target, attack))
                _emitter.OnAbilityUsed();
        }

        public void DoBlock()  => _hitState.BeginBlock();
        public void EndBlock() => _hitState.EndBlock();

        public void DoDodge()
        {
            _hitState.BeginDodge();
            _usedDodgeThisCombat = true;
            _emitter.OnDodgeUsed();

            // Notify CombatContext so end-of-combat dodge flag is set
            if (ServiceLocator.TryGet<CombatContext>(out CombatContext ctx))
                ctx.RecordDodge();
        }

        // ── Combat lifecycle ──────────────────────────────────────────────────

        public void EnterCombat(string enemyId)
        {
            _currentEnemyId      = enemyId;
            _usedDodgeThisCombat = false;
            _health.BeginCombat();
        }

        public void ExitCombat(bool playerWon)
        {
            _health.EndCombat();

            if (playerWon && !_usedDodgeThisCombat)
                _emitter.OnCombatCompletedNoDodge();

            // Winning any fight resets the pattern-failure counter for that enemy type
            if (playerWon && _currentEnemyId != null)
                _deathsByEnemyId[_currentEnemyId] = 0;
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private bool TryHitTarget(GameObject target, AttackData attack)
        {
            if (target == null) return false;
            var enemyHealth = target.GetComponent<Entities.Enemies.EnemyHealth>();
            if (enemyHealth == null) return false;

            enemyHealth.TakeHit(attack, PlayerElement);
            return true;
        }

        private void HandleDeath()
        {
            if (_currentEnemyId != null)
            {
                _deathsByEnemyId.TryGetValue(_currentEnemyId, out int prev);
                _deathsByEnemyId[_currentEnemyId] = prev + 1;
                // Inject correct consecutive count so PlayerHealth can classify
                _health.SetConsecutiveDeaths(_deathsByEnemyId[_currentEnemyId]);
            }

            // Currency loss on death
            if (ServiceLocator.TryGet<ISaveManager>(out ISaveManager save))
            {
                float pct  = Random.Range(GameConfig.DeathCurrencyLossMin, GameConfig.DeathCurrencyLossMax);
                float loss = save.Current.Character.Currency * pct;
                save.Current.Character.Currency = Mathf.Max(0f, save.Current.Character.Currency - loss);
                save.MarkDirty();
            }
        }
    }
}
