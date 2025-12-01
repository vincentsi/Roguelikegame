using ProjectRoguelike.Gameplay.Combat;
using ProjectRoguelike.Gameplay.Player;
using UnityEngine;

namespace ProjectRoguelike.AI.AttackPatterns
{
    /// <summary>
    /// Simple melee attack pattern: damage player when in range.
    /// </summary>
    public sealed class MeleeAttackPattern : AttackPatternBase
    {
        public override bool CanAttack()
        {
            return cooldownTimer <= 0f && IsTargetInRange() && target != null;
        }

        public override void ExecuteAttack()
        {
            if (!CanAttack())
            {
                return;
            }

            var distance = Vector3.Distance(transform.position, target.position);
            if (distance > attackRange)
            {
                return;
            }

            // Try to damage player
            var damageable = target.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.ApplyDamage(attackDamage);
                Debug.Log($"[MeleeAttackPattern] Dealt {attackDamage} damage to {target.name}");
            }

            cooldownTimer = attackCooldown;
        }
    }
}

