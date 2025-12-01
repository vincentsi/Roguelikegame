using UnityEngine;

namespace ProjectRoguelike.Gameplay.Items
{
    /// <summary>
    /// Base ScriptableObject for all items in the game.
    /// </summary>
    public abstract class ItemData : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string itemName = "Item";
        [SerializeField] private string description = "An item.";
        [SerializeField] private Sprite icon;
        [SerializeField] private ItemRarity rarity = ItemRarity.Common;

        [Header("Drop Settings")]
        [SerializeField] private float dropWeight = 1f; // Higher = more likely to drop
        [SerializeField] private int minDepth = 0; // Minimum floor depth to appear
        [SerializeField] private int maxDepth = 999; // Maximum floor depth to appear

        public string ItemName => itemName;
        public string Description => description;
        public Sprite Icon => icon;
        public ItemRarity Rarity => rarity;
        public float DropWeight => dropWeight;
        public int MinDepth => minDepth;
        public int MaxDepth => maxDepth;

        /// <summary>
        /// Called when the item is picked up by a player.
        /// </summary>
        public abstract void OnPickup(GameObject player);
    }

    public enum ItemRarity
    {
        Common,
        Uncommon,
        Rare,
        Epic,
        Legendary
    }
}

