using System;
using Fenrir.Combat;
using Fenrir.Config;
using UnityEngine;

namespace Fenrir.Entities.Enemies
{
    /// <summary>
    /// HP management for an enemy. Applies incoming player attacks.
    /// </summary>
    public class EnemyHealth : MonoBehaviour
    {
        [SerializeField] private float   _maxHp         = 60f;
        [SerializeField] private Element _element       = Element.Fire;

        public float   MaxHp             => _maxHp;
        public float   Current           { get; private set; }
        public float   CurrentNormalized => Current / _maxHp;
        public bool    IsDead            { get; private set; }
        public Element Element           => _element;

        public event Action OnDied;
        public event Action<float> OnDamaged;

        private void Awake() => Current = _maxHp;

        public void TakeHit(AttackData attack, Element attackerElement)
        {
            if (IsDead) return;

            float damage = AttackResolver.Resolve(attack, _element);
            Current = Mathf.Max(0f, Current - damage);
            OnDamaged?.Invoke(CurrentNormalized);

            if (Current <= 0f) Die();
        }

        private void Die()
        {
            if (IsDead) return;
            IsDead = true;
            OnDied?.Invoke();
        }

        public void ResetHealth()
        {
            IsDead  = false;
            Current = _maxHp;
        }
    }
}
