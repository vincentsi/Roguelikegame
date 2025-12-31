using UnityEngine;

namespace ProjectRoguelike.Procedural
{
    /// <summary>
    /// Types de récompenses disponibles dans les salles.
    /// </summary>
    public enum RoomRewardType
    {
        Currency,
        Upgrade,
        Weapon,
        Consumable,
        EliteReward,
        Shop,
        EventReward,
        BossReward,
        Random
    }

    /// <summary>
    /// Classe pour les récompenses générées dynamiquement (pas ScriptableObject).
    /// Utilisée par RewardGenerator pour créer des récompenses à la volée.
    /// </summary>
    [System.Serializable]
    public class GeneratedReward
    {
        public RoomRewardType RewardType;
        public string RewardName;
        public Sprite RewardIcon;
        public Color RewardColor;
        public int CurrencyAmount;
        public bool IsRandom;

        public GeneratedReward()
        {
            RewardType = RoomRewardType.Currency;
            RewardName = "Reward";
            RewardIcon = null;
            RewardColor = Color.white;
            CurrencyAmount = 0;
            IsRandom = false;
        }
    }
}
