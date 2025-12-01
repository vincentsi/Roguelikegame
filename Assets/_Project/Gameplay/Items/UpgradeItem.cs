using UnityEngine;

namespace ProjectRoguelike.Gameplay.Items
{
    /// <summary>
    /// ScriptableObject for upgrade items (permanent stat boosts, weapon mods, etc.)
    /// </summary>
    [CreateAssetMenu(fileName = "Upgrade_", menuName = "Roguelike/Upgrade Item", order = 3)]
    public sealed class UpgradeItem : ItemData
    {
        public enum UpgradeType
        {
            Damage,
            FireRate,
            ReloadSpeed,
            MaxHealth,
            MaxStamina,
            MovementSpeed,
            WeaponMod
        }

        [Header("Upgrade Settings")]
        [SerializeField] private UpgradeType upgradeType = UpgradeType.Damage;
        [SerializeField] private float value = 10f; // Flat or percentage increase
        [SerializeField] private bool isPercentage = false; // true = %, false = flat

        public UpgradeType Type => upgradeType;
        public float Value => value;
        public bool IsPercentage => isPercentage;

        public override void OnPickup(GameObject player)
        {
            // Apply upgrade to player stats
            // This will need integration with a player upgrade system
            Debug.Log($"[UpgradeItem] Applied {upgradeType} upgrade ({value}{(isPercentage ? "%" : "")}) to {player.name}");
            
            // TODO: Implement actual upgrade application
            // - Store upgrades in a component on the player
            // - Apply modifiers to stats/weapons
        }
    }
}

