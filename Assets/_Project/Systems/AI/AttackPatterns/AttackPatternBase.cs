using UnityEngine;

namespace ProjectRoguelike.AI.AttackPatterns
{
    /// <summary>
    /// Base class for enemy attack patterns.
    /// </summary>
    public abstract class AttackPatternBase : MonoBehaviour
    {
        protected Transform target;
        protected float attackDamage;
        protected float attackRange;
        protected float attackCooldown;

        protected float cooldownTimer;

        public virtual void Initialize(Transform targetTransform, float damage, float range, float cooldown)
        {
            target = targetTransform;
            attackDamage = damage;
            attackRange = range;
            attackCooldown = cooldown;
            cooldownTimer = 0f;
        }

        public virtual void Tick(float deltaTime)
        {
            if (cooldownTimer > 0f)
            {
                cooldownTimer -= deltaTime;
            }
        }

        public abstract bool CanAttack();
        public abstract void ExecuteAttack();

        protected bool IsTargetInRange()
        {
            if (target == null)
            {
                return false;
            }

            var distance = Vector3.Distance(transform.position, target.position);
            return distance <= attackRange;
        }
    }
}

