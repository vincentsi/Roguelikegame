using UnityEngine;

namespace ProjectRoguelike.Procedural
{
    /// <summary>
    /// ScriptableObject définissant la récompense d'une salle/porte.
    /// </summary>
    [CreateAssetMenu(fileName = "RoomReward_", menuName = "Roguelike/Room Reward Data", order = 2)]
    public sealed class RoomRewardData : ScriptableObject
    {
        [Header("Reward Type")]
        [SerializeField] private DoorType rewardType = DoorType.Currency;
        [SerializeField] private string rewardName = "Reward";
        [SerializeField] private Sprite rewardIcon;
        [SerializeField] private Color rewardColor = Color.white;

        [Header("Reward Values")]
        [SerializeField] private int currencyAmount = 0;
        [SerializeField] private bool isRandom = false;

        [Header("Item Rewards (Optional)")]
        [SerializeField] private ScriptableObject itemReward; // ItemData, WeaponData, etc.

        public DoorType RewardType => rewardType;
        public string RewardName => rewardName;
        public Sprite RewardIcon => rewardIcon;
        public Color RewardColor => rewardColor;
        public int CurrencyAmount => currencyAmount;
        public bool IsRandom => isRandom;
        public ScriptableObject ItemReward => itemReward;
    }
}

