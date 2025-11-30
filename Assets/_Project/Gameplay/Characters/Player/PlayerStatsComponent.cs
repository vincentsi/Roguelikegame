using System;
using ProjectRoguelike.Gameplay.Combat;
using UnityEngine;

namespace ProjectRoguelike.Gameplay.Player
{
    /// <summary>
    /// Handles health/stamina pools shared by other systems (HUD, movement, weapons).
    /// </summary>
    public sealed class PlayerStatsComponent : MonoBehaviour, IDamageable
    {
        [Header("Health")]
        [SerializeField] private float maxHealth = 120f;
        [SerializeField] private float reviveHealthPercent = 0.35f;

        [Header("Stamina")]
        [SerializeField] private float maxStamina = 100f;
        [SerializeField] private float sprintCostPerSecond = 25f;
        [SerializeField] private float slideCost = 15f;
        [SerializeField] private float regenDelay = 0.75f;
        [SerializeField] private float regenPerSecond = 30f;

        public float CurrentHealth { get; private set; }
        public float CurrentStamina { get; private set; }
        public float MaxHealth => maxHealth;
        public float MaxStamina => maxStamina;

        public event Action<float, float> OnHealthChanged;
        public event Action<float, float> OnStaminaChanged;
        public event Action OnPlayerDowned;

        private float _regenCooldown;

        private void Awake()
        {
            CurrentHealth = maxHealth;
            CurrentStamina = maxStamina;
        }

        private void Update()
        {
            if (_regenCooldown > 0f)
            {
                _regenCooldown -= Time.deltaTime;
                return;
            }

            if (CurrentStamina < maxStamina)
            {
                var newValue = Mathf.Min(maxStamina, CurrentStamina + regenPerSecond * Time.deltaTime);
                if (!Mathf.Approximately(newValue, CurrentStamina))
                {
                    CurrentStamina = newValue;
                    OnStaminaChanged?.Invoke(CurrentStamina, maxStamina);
                }
            }
        }

        public void ApplyDamage(float value)
        {
            if (value <= 0f)
            {
                return;
            }

            CurrentHealth = Mathf.Max(0f, CurrentHealth - value);
            OnHealthChanged?.Invoke(CurrentHealth, maxHealth);

            if (CurrentHealth <= 0f)
            {
                OnPlayerDowned?.Invoke();
            }
        }

        public void Heal(float amount)
        {
            if (amount <= 0f)
            {
                return;
            }

            CurrentHealth = Mathf.Min(maxHealth, CurrentHealth + amount);
            OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
        }

        public void Revive()
        {
            CurrentHealth = Mathf.Max(1f, maxHealth * reviveHealthPercent);
            OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
        }

        public bool TrySpendSprintStamina(float deltaTime)
        {
            var cost = sprintCostPerSecond * deltaTime;
            return TrySpendStamina(cost);
        }

        public bool TrySpendSlideCost()
        {
            return TrySpendStamina(slideCost);
        }

        public bool TrySpendStamina(float amount)
        {
            if (CurrentStamina < amount)
            {
                return false;
            }

            CurrentStamina -= amount;
            _regenCooldown = regenDelay;
            OnStaminaChanged?.Invoke(CurrentStamina, maxStamina);
            return true;
        }

        public void ForceStamina(float value)
        {
            CurrentStamina = Mathf.Clamp(value, 0f, maxStamina);
            OnStaminaChanged?.Invoke(CurrentStamina, maxStamina);
        }
    }
}

