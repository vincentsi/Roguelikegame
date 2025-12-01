using UnityEngine;

namespace ProjectRoguelike.Systems.Meta
{
    /// <summary>
    /// ScriptableObject for unlockable items/upgrades in meta-progression.
    /// </summary>
    [CreateAssetMenu(fileName = "Progression_", menuName = "Roguelike/Progression Data", order = 1)]
    public sealed class ProgressionData : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string unlockName = "Unlock";
        [SerializeField] private string description = "An unlockable item.";
        [SerializeField] private Sprite icon;

        [Header("Unlock Settings")]
        [SerializeField] private int cost = 100; // Currency cost to unlock
        [SerializeField] private bool isUnlockedByDefault = false;
        [SerializeField] private ProgressionData[] prerequisites; // Must unlock these first

        [Header("Unlock Type")]
        [SerializeField] private UnlockType unlockType = UnlockType.Weapon;
        [SerializeField] private GameObject unlockPrefab; // For weapon/character unlocks
        [SerializeField] private Gameplay.Items.ItemData unlockItem; // For item unlocks

        public string UnlockName => unlockName;
        public string Description => description;
        public Sprite Icon => icon;
        public int Cost => cost;
        public bool IsUnlockedByDefault => isUnlockedByDefault;
        public ProgressionData[] Prerequisites => prerequisites;
        public UnlockType Type => unlockType;
        public GameObject UnlockPrefab => unlockPrefab;
        public Gameplay.Items.ItemData UnlockItem => unlockItem;

        public enum UnlockType
        {
            Weapon,
            Character,
            Item,
            Upgrade,
            Cosmetic
        }
    }
}

