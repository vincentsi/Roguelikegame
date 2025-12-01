using ProjectRoguelike.Gameplay.Combat;
using UnityEngine;

namespace ProjectRoguelike.AI.AttackPatterns
{
    /// <summary>
    /// Burst attack pattern: multiple quick attacks in succession.
    /// </summary>
    public sealed class BurstAttackPattern : AttackPatternBase
    {
        [Header("Burst Settings")]
        [SerializeField] private int burstCount = 3;
        [SerializeField] private float burstInterval = 0.3f;
        [SerializeField] private float burstCooldown = 3f;

        private int _currentBurstCount;
        private float _burstTimer;
        private bool _isBursting;

        public override void Initialize(Transform targetTransform, float damage, float range, float cooldown)
        {
            base.Initialize(targetTransform, damage, range, cooldown);
            _currentBurstCount = 0;
            _isBursting = false;
        }

        public override void Tick(float deltaTime)
        {
            base.Tick(deltaTime);

            if (_isBursting)
            {
                _burstTimer -= deltaTime;
                if (_burstTimer <= 0f)
                {
                    ExecuteSingleAttack();
                    _currentBurstCount++;

                    if (_currentBurstCount >= burstCount)
                    {
                        _isBursting = false;
                        cooldownTimer = burstCooldown;
                        _currentBurstCount = 0;
                    }
                    else
                    {
                        _burstTimer = burstInterval;
                    }
                }
            }
        }

        public override bool CanAttack()
        {
            return !_isBursting && cooldownTimer <= 0f && IsTargetInRange();
        }

        public override void ExecuteAttack()
        {
            if (!CanAttack())
            {
                return;
            }

            _isBursting = true;
            _currentBurstCount = 0;
            _burstTimer = 0f; // Start immediately
            ExecuteSingleAttack();
        }

        private void ExecuteSingleAttack()
        {
            if (target == null || !IsTargetInRange())
            {
                _isBursting = false;
                return;
            }

            var damageable = target.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.ApplyDamage(attackDamage);
            }
        }
    }
}

