using System;
using UnityEngine;
using ProjectRoguelike.Procedural;

namespace ProjectRoguelike.Procedural
{
    /// <summary>
    /// ScriptableObject contenant les paramètres de génération du donjon.
    /// </summary>
    [CreateAssetMenu(fileName = "DungeonGenSettings_", menuName = "Roguelike/Dungeon Generator Settings", order = 3)]
    public sealed class DungeonGeneratorSettings : ScriptableObject
    {
        [Header("Room Count")]
        [SerializeField] private int minRoomsBeforeBoss = 4;
        [SerializeField] private int maxRoomsBeforeBoss = 6;

        [Header("Exits Per Room")]
        [SerializeField] private int minExitsPerRoom = 2;
        [SerializeField] private int maxExitsPerRoom = 3;

        [Header("Room Type Probabilities")]
        [Range(0f, 1f)]
        [SerializeField] private float eliteRoomChance = 0.2f;
        [Range(0f, 1f)]
        [SerializeField] private float shopRoomChance = 0.15f;
        [Range(0f, 1f)]
        [SerializeField] private float eventRoomChance = 0.1f;

        [Header("Door Type Distribution")]
        [SerializeField] private DoorTypeDistribution doorDistribution = new();

        [Header("Validation")]
        [SerializeField] private bool ensureBossReachable = true;
        [SerializeField] private int maxGenerationAttempts = 100;

        public int MinRoomsBeforeBoss => minRoomsBeforeBoss;
        public int MaxRoomsBeforeBoss => maxRoomsBeforeBoss;
        public int MinExitsPerRoom => minExitsPerRoom;
        public int MaxExitsPerRoom => maxExitsPerRoom;
        public float EliteRoomChance => eliteRoomChance;
        public float ShopRoomChance => shopRoomChance;
        public float EventRoomChance => eventRoomChance;
        public DoorTypeDistribution DoorDistribution => doorDistribution;
        public bool EnsureBossReachable => ensureBossReachable;
        public int MaxGenerationAttempts => maxGenerationAttempts;
    }

    /// <summary>
    /// Distribution des types de portes pour la génération.
    /// </summary>
    [Serializable]
    public sealed class DoorTypeDistribution
    {
        [Range(0f, 1f)]
        [SerializeField] private float currencyWeight = 0.3f;
        [Range(0f, 1f)]
        [SerializeField] private float upgradeWeight = 0.25f;
        [Range(0f, 1f)]
        [SerializeField] private float weaponWeight = 0.1f;
        [Range(0f, 1f)]
        [SerializeField] private float consumableWeight = 0.1f;
        [Range(0f, 1f)]
        [SerializeField] private float eliteWeight = 0.15f;
        [Range(0f, 1f)]
        [SerializeField] private float shopWeight = 0.05f;
        [Range(0f, 1f)]
        [SerializeField] private float eventWeight = 0.05f;

        public float CurrencyWeight => currencyWeight;
        public float UpgradeWeight => upgradeWeight;
        public float WeaponWeight => weaponWeight;
        public float ConsumableWeight => consumableWeight;
        public float EliteWeight => eliteWeight;
        public float ShopWeight => shopWeight;
        public float EventWeight => eventWeight;

        public float GetTotalWeight()
        {
            return currencyWeight + upgradeWeight + weaponWeight + consumableWeight + 
                   eliteWeight + shopWeight + eventWeight;
        }

        public DoorType SelectRandomType(System.Random random)
        {
            float total = GetTotalWeight();
            float value = (float)random.NextDouble() * total;

            value -= currencyWeight;
            if (value <= 0f) return DoorType.Currency;

            value -= upgradeWeight;
            if (value <= 0f) return DoorType.Upgrade;

            value -= weaponWeight;
            if (value <= 0f) return DoorType.Weapon;

            value -= consumableWeight;
            if (value <= 0f) return DoorType.Consumable;

            value -= eliteWeight;
            if (value <= 0f) return DoorType.Elite;

            value -= shopWeight;
            if (value <= 0f) return DoorType.Shop;

            return DoorType.Event;
        }
    }
}

