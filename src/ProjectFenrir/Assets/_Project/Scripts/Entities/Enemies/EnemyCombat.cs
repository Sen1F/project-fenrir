using Fenrir.Combat;
using Fenrir.Config;
using Fenrir.Core;
using Fenrir.Entities.Player;
using Fenrir.Save;
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
                _base.Element          // attacker element = this enemy
            );

            // Defender element = player's current element, read from save
            Element defenderElement = Element.Fire; // fallback if save unavailable
            if (ServiceLocator.TryGet<ISaveManager>(out ISaveManager save))
                defenderElement = save.Current.Character.CurrentElement;

            target.GetComponent<PlayerHealth>()?.TakeHit(attack, defenderElement);
        }
    }
}
