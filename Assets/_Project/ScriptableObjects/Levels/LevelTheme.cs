using System.Collections.Generic;
using UnityEngine;
using ProjectRoguelike.Procedural;

namespace ProjectRoguelike.Levels
{
    /// <summary>
    /// ScriptableObject définissant le thème visuel et les paramètres d'un niveau.
    /// </summary>
    [CreateAssetMenu(fileName = "LevelTheme_", menuName = "Roguelike/Level Theme", order = 1)]
    public sealed class LevelTheme : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string themeName = "Level";
        [SerializeField] private int levelIndex = 1;

        [Header("Room Pools")]
        [SerializeField] private List<RoomData> combatRooms = new();
        [SerializeField] private List<RoomData> eliteRooms = new();
        [SerializeField] private List<RoomData> shopRooms = new();
        [SerializeField] private List<RoomData> eventRooms = new();
        [SerializeField] private List<RoomData> bossRooms = new();

        [Header("Visual Settings")]
        [SerializeField] private Color ambientColor = new Color(0.2f, 0.2f, 0.3f);
        [SerializeField] private Material[] themeMaterials;

        [Header("Audio")]
        [SerializeField] private AudioClip backgroundMusic;

        [Header("Enemy Settings")]
        [SerializeField] private int baseEnemyCount = 5;
        [SerializeField] private float enemyDifficultyMultiplier = 1f;

        public string ThemeName => themeName;
        public int LevelIndex => levelIndex;
        public IReadOnlyList<RoomData> CombatRooms => combatRooms;
        public IReadOnlyList<RoomData> EliteRooms => eliteRooms;
        public IReadOnlyList<RoomData> ShopRooms => shopRooms;
        public IReadOnlyList<RoomData> EventRooms => eventRooms;
        public IReadOnlyList<RoomData> BossRooms => bossRooms;
        public Color AmbientColor => ambientColor;
        public Material[] ThemeMaterials => themeMaterials;
        public AudioClip BackgroundMusic => backgroundMusic;
        public int BaseEnemyCount => baseEnemyCount;
        public float EnemyDifficultyMultiplier => enemyDifficultyMultiplier;

        public List<RoomData> GetRoomsByType(RoomType type)
        {
            return type switch
            {
                RoomType.Combat => new List<RoomData>(combatRooms),
                RoomType.Elite => new List<RoomData>(eliteRooms),
                RoomType.Shop => new List<RoomData>(shopRooms),
                RoomType.Event => new List<RoomData>(eventRooms),
                RoomType.Boss => new List<RoomData>(bossRooms),
                _ => new List<RoomData>()
            };
        }
    }
}

