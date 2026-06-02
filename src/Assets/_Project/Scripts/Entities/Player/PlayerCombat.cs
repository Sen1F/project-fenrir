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
    /// Produces AttackData structs for AttackResolver.
    /// Tracks consecutive deaths vs. the same enemy ID for pattern-failure detection.
    /// </summary>
    [RequireComponent(typeof(PlayerHealth), typeof(PlayerEnergy),
                      typeof(PlayerTraitEmitter), typeof(HitStateManager))]
    public class PlayerCombat : MonoBehaviour
    {
        // ── Tunables ──────────────────────────────────────────────────────────
        [SerializeField] private float _lightAttackDamage  = 15f;
        [SerializeField] private float _heavyAttackDamage  = 35f;
        [SerializeField] private float _abilityDamage      = 50f;
        [SerializeField] private float _abilityEnergyCost  = 40f;

        // ── Components ────────────────────────────────────────────────────────
        private PlayerHealth       _health;
        private PlayerEnergy       _energy;
        private PlayerTraitEmitter _emitter;
        private HitStateManager    _hitState;

        // ── Combat state ──────────────────────────────────────────────────────
        private bool   _usedDodgeThisCombat;
        private string _currentEnemyId;
        private int    _consecutiveDeathsVsSameEnemy;

        private Element PlayerElement => ServiceLocator.TryGet<ISaveManager>(out var save)
            ? save.Current.Character.CurrentElement
            : Element.Fire;

        private void Awake()
        {
            _health   = GetComponent<PlayerHealth>();
            _energy   = GetComponent<PlayerEnergy>();
            _emitter  = GetComponent<PlayerTraitEmitter>();
            _hitState = GetComponent<HitStateManager>();

            _health.OnDied += HandleDeath;
        }

        // ── Input forwarding (called by InputHandler/TouchMapper) ─────────────

        public void DoLightAttack(GameObject target)
        {
            var attack = new AttackData(AttackType.Light, _lightAttackDamage, PlayerElement);
            _emitter.OnLightAttackLanded();  // optimistic — real hit detection in AttackResolver
            TryHitTarget(target, attack);
        }

        public void DoHeavyAttack(GameObject target)
        {
            var attack = new AttackData(AttackType.Heavy, _heavyAttackDamage, PlayerElement);
            _emitter.OnHeavyAttackLanded();
            TryHitTarget(target, attack);
        }

        public void DoAbility(GameObject target)
        {
            if (!_energy.TrySpend(_abilityEnergyCost)) return;
            var attack = new AttackData(AttackType.Ability, _abilityDamage, PlayerElement, isAbility: true);
            _emitter.OnAbilityUsed();
            TryHitTarget(target, attack);
        }

        public void DoBlock()  => _hitState.BeginBlock();
        public void EndBlock() => _hitState.EndBlock();

        public void DoDodge()
        {
            _hitState.BeginDodge();
            _usedDodgeThisCombat = true;
            _emitter.OnDodgeUsed();
        }

        // ── Combat lifecycle ──────────────────────────────────────────────────

        public void EnterCombat(string enemyId)
        {
            _currentEnemyId    = enemyId;
            _usedDodgeThisCombat = false;
            _health.BeginCombat();
        }

        public void ExitCombat(bool playerWon)
        {
            _health.EndCombat();

            if (playerWon)
            {
                _consecutiveDeathsVsSameEnemy = 0;

                if (!_usedDodgeThisCombat)
                    _emitter.OnCombatCompletedNoDodge();
            }
        }

        // ── Internal ──────────────────────────────────────────────────────────

        private void TryHitTarget(GameObject target, AttackData attack)
        {
            if (target == null) return;
            var enemyHealth = target.GetComponent<Entities.Enemies.EnemyHealth>();
            if (enemyHealth != null)
                enemyHealth.TakeHit(attack, PlayerElement);
        }

        private void HandleDeath()
        {
            if (_currentEnemyId != null)
                _consecutiveDeathsVsSameEnemy++;

            _health.SetConsecutiveDeaths(_consecutiveDeathsVsSameEnemy);

            // Apply currency loss
            if (ServiceLocator.TryGet<ISaveManager>(out var save))
            {
                float loss = Random.Range(GameConfig.DeathCurrencyLossMin, GameConfig.DeathCurrencyLossMax);
                int toRemove = Mathf.FloorToInt(save.Current.Character.Currency * loss);
                save.Current.Character.Currency = Mathf.Max(0, save.Current.Character.Currency - toRemove);
                save.MarkDirty();
            }
        }
    }
}
