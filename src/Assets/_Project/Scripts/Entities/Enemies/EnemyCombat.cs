using Fenrir.Combat;
using Fenrir.Entities.Player;
using UnityEngine;

namespace Fenrir.Entities.Enemies
{
    /// <summary>
    /// Produces and delivers attacks from the enemy to the player.
    /// </summary>
    [RequireComponent(typeof(EnemyBase))]
    public class EnemyCombat : MonoBehaviour
    {
        [SerializeField] private float _attackDamage = 12f;

        private EnemyBase _base;

        private void Awake() => _base = GetComponent<EnemyBase>();

        public void Attack(GameObject target)
        {
            if (target == null) return;

            var attack = new AttackData(
                AttackType.Light,
                _attackDamage,
                _base.Element
            );

            var playerElement = _base.Element; // defender element fetched from player save in PlayerHealth
            target.GetComponent<PlayerHealth>()
                  ?.TakeHit(attack, playerElement);
        }
    }
}
