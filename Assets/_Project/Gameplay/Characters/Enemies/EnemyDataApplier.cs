using ProjectRoguelike.Enemies;
using ProjectRoguelike.AI.AttackPatterns;
using ProjectRoguelike.AI.Sensors;
using UnityEngine;
using UnityEngine.AI;

namespace ProjectRoguelike.Gameplay.Enemies
{
    /// <summary>
    /// Applies EnemyData to an enemy GameObject, configuring all components.
    /// </summary>
    [RequireComponent(typeof(EnemyHealthComponent))]
    [RequireComponent(typeof(EnemyController))]
    [RequireComponent(typeof(EnemyAI))]
    public sealed class EnemyDataApplier : MonoBehaviour
    {
        [Header("Data")]
        [SerializeField] private EnemyData enemyData;

        private void Awake()
        {
            if (enemyData == null)
            {
                Debug.LogWarning($"[EnemyDataApplier] No EnemyData assigned to {gameObject.name}");
                return;
            }

            ApplyData();
        }

        private void ApplyData()
        {
            // Apply health
            var health = GetComponent<EnemyHealthComponent>();
            if (health != null)
            {
                health.SetMaxHealth(enemyData.MaxHealth);
            }

            // Apply movement speeds
            var controller = GetComponent<EnemyController>();
            if (controller != null)
            {
                controller.SetWalkSpeed(enemyData.MoveSpeed);
                controller.SetRunSpeed(enemyData.RunSpeed);
                controller.SetRotationSpeed(enemyData.RotationSpeed);
            }

            // Apply NavMeshAgent settings
            var agent = GetComponent<NavMeshAgent>();
            if (agent != null)
            {
                agent.speed = enemyData.MoveSpeed;
                agent.angularSpeed = enemyData.RotationSpeed * 60f;
            }

            // Add sensors if not present and configure them
            var visionSensor = GetComponent<VisionSensor>();
            if (visionSensor == null)
            {
                visionSensor = gameObject.AddComponent<VisionSensor>();
            }
            visionSensor.SetViewRange(enemyData.DetectionRange);
            visionSensor.SetViewAngle(enemyData.ViewAngle);

            var hearingSensor = GetComponent<HearingSensor>();
            if (hearingSensor == null)
            {
                hearingSensor = gameObject.AddComponent<HearingSensor>();
            }
            hearingSensor.SetHearingRange(enemyData.HearingRange);

            // Add attack pattern based on data
            var existingPattern = GetComponent<AttackPatternBase>();
            if (existingPattern != null)
            {
                Destroy(existingPattern);
            }

            AttackPatternBase pattern = enemyData.AttackPattern switch
            {
                AttackPattern.Melee => gameObject.AddComponent<MeleeAttackPattern>(),
                AttackPattern.Burst => gameObject.AddComponent<BurstAttackPattern>(),
                _ => gameObject.AddComponent<MeleeAttackPattern>()
            };

            // Initialize pattern (will be done when target is found)
        }

        public void InitializeAttackPattern(Transform target)
        {
            var pattern = GetComponent<AttackPatternBase>();
            if (pattern != null && enemyData != null)
            {
                pattern.Initialize(target, enemyData.AttackDamage, enemyData.AttackRange, enemyData.AttackCooldown);
            }
        }
    }
}

