using System;
using ProjectRoguelike.Gameplay.Combat;
using UnityEngine;

namespace ProjectRoguelike.Gameplay.Enemies
{
    /// <summary>
    /// Health component for enemies. Implements IDamageable for combat system.
    /// </summary>
    public sealed class EnemyHealthComponent : MonoBehaviour, IDamageable
    {
        [Header("Health")]
        [SerializeField] private float maxHealth = 50f;

        public float CurrentHealth { get; private set; }
        public float MaxHealth => maxHealth;
        public bool IsAlive => CurrentHealth > 0f;

        public event Action<float, float> OnHealthChanged;
        public event Action OnEnemyDied;

        private void Awake()
        {
            CurrentHealth = maxHealth;
        }

        public void ApplyDamage(float amount)
        {
            if (amount <= 0f || !IsAlive)
            {
                return;
            }

            CurrentHealth = Mathf.Max(0f, CurrentHealth - amount);
            OnHealthChanged?.Invoke(CurrentHealth, maxHealth);

            if (CurrentHealth <= 0f)
            {
                OnEnemyDied?.Invoke();
            }
        }

        public void Heal(float amount)
        {
            if (amount <= 0f || !IsAlive)
            {
                return;
            }

            CurrentHealth = Mathf.Min(maxHealth, CurrentHealth + amount);
            OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
        }

        public void ResetHealth()
        {
            CurrentHealth = maxHealth;
            OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
        }

        public void SetMaxHealth(float newMaxHealth)
        {
            maxHealth = newMaxHealth;
            CurrentHealth = Mathf.Min(CurrentHealth, maxHealth);
            OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
        }
    }
}

