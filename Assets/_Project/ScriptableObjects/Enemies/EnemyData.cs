using UnityEngine;

namespace ProjectRoguelike.Enemies
{
    /// <summary>
    /// ScriptableObject containing data for enemy types (melee, ranged, fast, tank).
    /// </summary>
    [CreateAssetMenu(fileName = "EnemyData_", menuName = "Roguelike/Enemy Data", order = 2)]
    public sealed class EnemyData : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string enemyName = "Enemy";
        [SerializeField] private EnemyType enemyType = EnemyType.Melee;

        [Header("Stats")]
        [SerializeField] private float maxHealth = 50f;
        [SerializeField] private float moveSpeed = 3.5f;
        [SerializeField] private float runSpeed = 5f;
        [SerializeField] private float rotationSpeed = 10f;

        [Header("Combat")]
        [SerializeField] private float attackDamage = 10f;
        [SerializeField] private float attackRange = 2f;
        [SerializeField] private float attackCooldown = 1.5f;
        [SerializeField] private AttackPattern attackPattern = AttackPattern.Melee;

        [Header("Detection")]
        [SerializeField] private float detectionRange = 10f;
        [SerializeField] private float viewAngle = 90f;
        [SerializeField] private float hearingRange = 20f;

        [Header("Patrol")]
        [SerializeField] private float patrolRadius = 5f;
        [SerializeField] private float patrolWaitTime = 2f;

        [Header("Prefab")]
        [SerializeField] private GameObject enemyPrefab;

        public string EnemyName => enemyName;
        public EnemyType EnemyType => enemyType;
        public float MaxHealth => maxHealth;
        public float MoveSpeed => moveSpeed;
        public float RunSpeed => runSpeed;
        public float RotationSpeed => rotationSpeed;
        public float AttackDamage => attackDamage;
        public float AttackRange => attackRange;
        public float AttackCooldown => attackCooldown;
        public AttackPattern AttackPattern => attackPattern;
        public float DetectionRange => detectionRange;
        public float ViewAngle => viewAngle;
        public float HearingRange => hearingRange;
        public float PatrolRadius => patrolRadius;
        public float PatrolWaitTime => patrolWaitTime;
        public GameObject EnemyPrefab => enemyPrefab;
    }

    public enum EnemyType
    {
        Melee,
        Ranged,
        Fast,
        Tank,
        Boss
    }

    public enum AttackPattern
    {
        Melee,      // Simple melee attack
        Ranged,     // Projectile attack
        Burst,      // Multiple quick attacks
        Charge,     // Charge then attack
        Area        // Area of effect attack
    }
}

