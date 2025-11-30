using ProjectRoguelike.Core;
using UnityEngine;

namespace ProjectRoguelike.Gameplay.Enemies
{
    /// <summary>
    /// Basic AI for enemies: patrol, chase, attack states.
    /// </summary>
    [RequireComponent(typeof(EnemyController))]
    [RequireComponent(typeof(EnemyHealthComponent))]
    public sealed class EnemyAI : MonoBehaviour
    {
        public enum AIState
        {
            Patrol,
            Chase,
            Attack,
            Dead
        }

        [Header("Detection")]
        [SerializeField] private float detectionRange = 10f;
        [SerializeField] private float attackRange = 2f;
        [SerializeField] private float attackCooldown = 1.5f;
        [SerializeField] private float attackDamage = 10f;

        [Header("Patrol")]
        [SerializeField] private float patrolRadius = 5f;
        [SerializeField] private float patrolWaitTime = 2f;

        [Header("Death")]
        [SerializeField] private float despawnDelay = 3f;

        private EnemyController _controller;
        private EnemyHealthComponent _health;
        private Transform _player;
        private AIState _currentState = AIState.Patrol;
        private PlayerManager _playerManager;

        private Vector3 _patrolCenter;
        private Vector3 _patrolTarget;
        private float _attackCooldownTimer;
        private float _patrolWaitTimer;

        public AIState CurrentState => _currentState;

        private void Awake()
        {
            _controller = GetComponent<EnemyController>();
            _health = GetComponent<EnemyHealthComponent>();
            _patrolCenter = transform.position;

            _health.OnEnemyDied += OnEnemyDied;
        }

        private void Start()
        {
            // Get player via PlayerManager service
            var bootstrap = AppBootstrap.Instance;
            if (bootstrap != null && bootstrap.Services.TryResolve<PlayerManager>(out _playerManager))
            {
                _player = _playerManager.GetClosestPlayer(transform.position);
            }
            else
            {
                // Fallback to tag-based search if service not available
                var playerObj = GameObject.FindGameObjectWithTag("Player");
                if (playerObj != null)
                {
                    _player = playerObj.transform;
                }
            }

            SetPatrolTarget();
        }

        private void Update()
        {
            if (_currentState == AIState.Dead)
            {
                return;
            }

            // Update player reference (handles player respawn/multiplayer)
            if (_playerManager != null)
            {
                _player = _playerManager.GetClosestPlayer(transform.position);
            }

            if (_player == null)
            {
                return;
            }

            // Check if player is alive
            var playerStats = _player.GetComponent<ProjectRoguelike.Gameplay.Player.PlayerStatsComponent>();
            if (playerStats != null && playerStats.CurrentHealth <= 0f)
            {
                // Player is dead, return to patrol
                if (_currentState != AIState.Patrol)
                {
                    ChangeState(AIState.Patrol);
                }
                return;
            }

            var distanceToPlayer = Vector3.Distance(transform.position, _player.position);

            // State machine
            switch (_currentState)
            {
                case AIState.Patrol:
                    UpdatePatrol();
                    if (distanceToPlayer <= detectionRange)
                    {
                        ChangeState(AIState.Chase);
                    }
                    break;

                case AIState.Chase:
                    UpdateChase();
                    if (distanceToPlayer <= attackRange)
                    {
                        ChangeState(AIState.Attack);
                    }
                    else if (distanceToPlayer > detectionRange * 1.5f)
                    {
                        ChangeState(AIState.Patrol);
                    }
                    break;

                case AIState.Attack:
                    UpdateAttack();
                    if (distanceToPlayer > attackRange * 1.2f)
                    {
                        ChangeState(AIState.Chase);
                    }
                    break;
            }

            // Update attack cooldown
            if (_attackCooldownTimer > 0f)
            {
                _attackCooldownTimer -= Time.deltaTime;
            }
        }

        private void UpdatePatrol()
        {
            if (_controller.HasReachedDestination)
            {
                _patrolWaitTimer -= Time.deltaTime;
                if (_patrolWaitTimer <= 0f)
                {
                    SetPatrolTarget();
                }
            }
        }

        private void UpdateChase()
        {
            _controller.SetRunning(true);
            _controller.SetDestination(_player.position);
            _controller.LookAt(_player.position);
        }

        private void UpdateAttack()
        {
            _controller.Stop();
            _controller.LookAt(_player.position);

            if (_attackCooldownTimer <= 0f)
            {
                PerformAttack();
                _attackCooldownTimer = attackCooldown;
            }
        }

        private void PerformAttack()
        {
            // Simple melee attack - damage player if in range
            var distanceToPlayer = Vector3.Distance(transform.position, _player.position);
            if (distanceToPlayer <= attackRange)
            {
                var playerStats = _player.GetComponent<ProjectRoguelike.Gameplay.Player.PlayerStatsComponent>();
                if (playerStats != null && playerStats.CurrentHealth > 0f)
                {
                    playerStats.ApplyDamage(attackDamage);
                }
                else if (playerStats != null && playerStats.CurrentHealth <= 0f)
                {
                    // Player is already dead, stop attacking
                    ChangeState(AIState.Patrol);
                }
                else
                {
                    Debug.LogWarning("[EnemyAI] PlayerStatsComponent not found on player!");
                }
            }
        }

        private void SetPatrolTarget()
        {
            var randomDirection = Random.insideUnitSphere * patrolRadius;
            randomDirection.y = 0f;
            _patrolTarget = _patrolCenter + randomDirection;

            // Check if position is valid on NavMesh
            if (UnityEngine.AI.NavMesh.SamplePosition(_patrolTarget, out var hit, patrolRadius, UnityEngine.AI.NavMesh.AllAreas))
            {
                _patrolTarget = hit.position;
            }

            _controller.SetRunning(false);
            _controller.SetDestination(_patrolTarget);
            _patrolWaitTimer = patrolWaitTime;
        }

        private void ChangeState(AIState newState)
        {
            if (_currentState == newState)
            {
                return;
            }

            _currentState = newState;

            // State exit logic
            switch (newState)
            {
                case AIState.Patrol:
                    SetPatrolTarget();
                    break;
                case AIState.Chase:
                    _controller.Resume();
                    break;
                case AIState.Attack:
                    _controller.Stop();
                    break;
            }
        }

        private void OnEnemyDied()
        {
            _currentState = AIState.Dead;
            _controller.Stop();
            
            // Disable AI and movement
            enabled = false;
            if (_controller != null)
            {
                _controller.enabled = false;
            }
            
            // Disable NavMeshAgent
            var navAgent = GetComponent<UnityEngine.AI.NavMeshAgent>();
            if (navAgent != null)
            {
                navAgent.enabled = false;
            }
            
            // Disable collider (so it doesn't block shots)
            var collider = GetComponent<CapsuleCollider>();
            if (collider != null)
            {
                collider.enabled = false;
            }
            
            // Despawn after delay
            Destroy(gameObject, despawnDelay);
            
            // TODO: Play death animation, drop loot, etc.
        }

        private void OnDrawGizmosSelected()
        {
            // Draw detection range
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, detectionRange);

            // Draw attack range
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRange);

            // Draw patrol radius
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(_patrolCenter, patrolRadius);
        }
    }
}

