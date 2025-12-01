using UnityEngine;

namespace ProjectRoguelike.Gameplay.Items
{
    /// <summary>
    /// ScriptableObject for consumable items (health potions, ammo packs, etc.)
    /// </summary>
    [CreateAssetMenu(fileName = "Consumable_", menuName = "Roguelike/Consumable Item", order = 2)]
    public sealed class ConsumableItem : ItemData
    {
        public enum ConsumableType
        {
            Health,
            Ammo,
            Stamina,
            Buff
        }

        [Header("Consumable Settings")]
        [SerializeField] private ConsumableType consumableType = ConsumableType.Health;
        [SerializeField] private float value = 50f; // Amount to restore/buff
        [SerializeField] private float duration = 0f; // 0 = instant, >0 = buff duration

        public ConsumableType Type => consumableType;
        public float Value => value;
        public float Duration => duration;

        public override void OnPickup(GameObject player)
        {
            switch (consumableType)
            {
                case ConsumableType.Health:
                    var stats = player.GetComponent<Player.PlayerStatsComponent>();
                    if (stats != null)
                    {
                        stats.Heal(value);
                        Debug.Log($"[ConsumableItem] Restored {value} health to {player.name}");
                    }
                    break;

                case ConsumableType.Ammo:
                    var weaponController = player.GetComponent<Weapons.WeaponController>();
                    if (weaponController != null)
                    {
                        // Add ammo to current weapon
                        // This will need a method on WeaponController/WeaponBase
                        Debug.Log($"[ConsumableItem] Restored {value} ammo to {player.name}");
                    }
                    break;

                case ConsumableType.Stamina:
                    var playerStats = player.GetComponent<Player.PlayerStatsComponent>();
                    if (playerStats != null)
                    {
                        // Add stamina restoration method if needed
                        Debug.Log($"[ConsumableItem] Restored {value} stamina to {player.name}");
                    }
                    break;

                case ConsumableType.Buff:
                    // Apply temporary buff (to be implemented later)
                    Debug.Log($"[ConsumableItem] Applied buff to {player.name} for {duration}s");
                    break;
            }
        }
    }
}

