using UnityEngine;
using UnityEngine.AI;

namespace Fenrir.Entities.Enemies
{
    /// <summary>
    /// Simple state-driven AI using Unity NavMesh.
    /// States: Idle → Chase → Attack → Stagger → Dead.
    /// </summary>
    [RequireComponent(typeof(EnemyBase), typeof(EnemyHealth), typeof(NavMeshAgent))]
    public class EnemyAI : MonoBehaviour
    {
        private enum AIState { Idle, Chase, Attack, Stagger, Dead }

        // ── Tunables ──────────────────────────────────────────────────────────
        [SerializeField] private float _detectionRange  = 10f;
        [SerializeField] private float _attackRange     = 2f;
        [SerializeField] private float _attackInterval  = 1.5f;
        [SerializeField] private float _staggerDuration = 0.5f;

        // ── Components ────────────────────────────────────────────────────────
        private EnemyBase   _base;
        private EnemyHealth _health;
        private NavMeshAgent _agent;

        // ── State ─────────────────────────────────────────────────────────────
        private AIState   _state        = AIState.Idle;
        private Transform _target;
        private Transform _cachedPlayer;   // cached once; never call FindWithTag in Update
        private float     _attackCooldown;
        private float     _staggerTimer;

        private void Awake()
        {
            _base   = GetComponent<EnemyBase>();
            _health = GetComponent<EnemyHealth>();
            _agent  = GetComponent<NavMeshAgent>();

            _health.OnDied += HandleDeath;
        }

        private void Start()
        {
            // Cache player once at scene start — FindWithTag must NOT run in Update
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null) _cachedPlayer = player.transform;
        }

        private void Update()
        {
            switch (_state)
            {
                case AIState.Idle:    TickIdle();    break;
                case AIState.Chase:   TickChase();   break;
                case AIState.Attack:  TickAttack();  break;
                case AIState.Stagger: TickStagger(); break;
            }
        }

        // ── State ticks ───────────────────────────────────────────────────────

        private void TickIdle()
        {
            _target = _cachedPlayer;
            if (_target == null) return;

            float dist = Vector3.Distance(transform.position, _target.position);
            if (dist <= _detectionRange) TransitionTo(AIState.Chase);
        }

        private void TickChase()
        {
            if (_target == null) { TransitionTo(AIState.Idle); return; }

            float dist = Vector3.Distance(transform.position, _target.position);

            if (dist > _detectionRange * 1.5f)
            {
                TransitionTo(AIState.Idle);
                return;
            }

            if (dist <= _attackRange)
            {
                TransitionTo(AIState.Attack);
                return;
            }

            _agent.SetDestination(_target.position);
        }

        private void TickAttack()
        {
            if (_target == null) { TransitionTo(AIState.Idle); return; }

            float dist = Vector3.Distance(transform.position, _target.position);

            if (dist > _attackRange)
            {
                TransitionTo(AIState.Chase);
                return;
            }

            _agent.ResetPath();
            _attackCooldown -= Time.deltaTime;
            if (_attackCooldown <= 0f)
            {
                _attackCooldown = _attackInterval;
                PerformAttack();
            }
        }

        private void TickStagger()
        {
            _staggerTimer -= Time.deltaTime;
            if (_staggerTimer <= 0f)
                TransitionTo(_target != null ? AIState.Chase : AIState.Idle);
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private void PerformAttack()
        {
            // Delegate to EnemyCombat on the same GameObject
            GetComponent<EnemyCombat>()?.Attack(_target.gameObject);
        }

        // FindWithTag removed — player is cached in Start() via _cachedPlayer.

        private void TransitionTo(AIState next)
        {
            _state = next;
            if (next == AIState.Idle || next == AIState.Dead)
                _agent.ResetPath();
        }

        private void HandleDeath()
        {
            TransitionTo(AIState.Dead);
            _agent.enabled = false;
        }

        public void Stagger()
        {
            _staggerTimer = _staggerDuration;
            TransitionTo(AIState.Stagger);
        }
    }
}
