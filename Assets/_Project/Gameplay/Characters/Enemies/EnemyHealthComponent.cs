using System;
using ProjectRoguelike.Gameplay.Combat;
using ProjectRoguelike.Procedural;
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

        private RoomModule _parentRoom;

        private void Awake()
        {
            CurrentHealth = maxHealth;

            // Trouver le RoomModule parent et s'enregistrer
            _parentRoom = GetComponentInParent<RoomModule>();
            if (_parentRoom == null)
            {
                // Chercher dans la scène (l'ennemi peut être spawné sans être enfant)
                var rooms = FindObjectsOfType<RoomModule>();
                foreach (var room in rooms)
                {
                    if (room.gameObject.activeInHierarchy && Vector3.Distance(transform.position, room.transform.position) < 50f)
                    {
                        _parentRoom = room;
                        break;
                    }
                }
            }

            if (_parentRoom != null)
            {
                _parentRoom.RegisterEnemy(gameObject);
            }
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

                // Notifier le RoomModule
                if (_parentRoom != null)
                {
                    _parentRoom.UnregisterEnemy(gameObject);
                }

                // Détruire l'ennemi après un délai (pour animations)
                Destroy(gameObject, 0.5f);
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

        private void OnDestroy()
        {
            // S'assurer de se désenregistrer si détruit autrement
            if (_parentRoom != null && IsAlive)
            {
                _parentRoom.UnregisterEnemy(gameObject);
            }
        }
    }
}

