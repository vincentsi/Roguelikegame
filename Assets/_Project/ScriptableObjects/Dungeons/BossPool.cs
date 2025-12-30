using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ProjectRoguelike.Levels;
using ProjectRoguelike.Procedural;

namespace ProjectRoguelike.Procedural
{
    /// <summary>
    /// ScriptableObject contenant la pool de bosses disponibles pour un niveau.
    /// </summary>
    [CreateAssetMenu(fileName = "BossPool_", menuName = "Roguelike/Boss Pool", order = 4)]
    public sealed class BossPool : ScriptableObject
    {
        [Header("Theme")]
        [SerializeField] private LevelTheme associatedTheme;

        [Header("Bosses")]
        [SerializeField] private List<BossData> availableBosses = new();

        public LevelTheme AssociatedTheme => associatedTheme;
        public IReadOnlyList<BossData> AvailableBosses => availableBosses;

        public BossData SelectRandom(LevelSeed seed)
        {
            if (availableBosses == null || availableBosses.Count == 0)
            {
                return null;
            }

            if (availableBosses.Count == 1)
            {
                return availableBosses[0];
            }

            int index = seed.NextInt(availableBosses.Count);
            return availableBosses[index];
        }
    }

    /// <summary>
    /// Données d'un boss (peut être étendu plus tard).
    /// </summary>
    [System.Serializable]
    public sealed class BossData
    {
        [SerializeField] private string bossName = "Boss";
        [SerializeField] private GameObject bossPrefab;
        [SerializeField] private float healthMultiplier = 5f;
        [SerializeField] private float damageMultiplier = 2f;

        public string BossName => bossName;
        public GameObject BossPrefab => bossPrefab;
        public float HealthMultiplier => healthMultiplier;
        public float DamageMultiplier => damageMultiplier;
    }
}

