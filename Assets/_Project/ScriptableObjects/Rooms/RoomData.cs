using UnityEngine;

namespace ProjectRoguelike.Procedural
{
    /// <summary>
    /// ScriptableObject containing metadata for a room module.
    /// Used by the procedural generator to select and place rooms.
    /// </summary>
    [CreateAssetMenu(fileName = "RoomData_", menuName = "Roguelike/Room Data", order = 1)]
    public sealed class RoomData : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string roomName = "Room";
        [SerializeField] private RoomType roomType = RoomType.Combat;

        [Header("Dimensions")]
        [SerializeField] private Vector2Int size = new Vector2Int(1, 1); // In grid units
        [SerializeField] private Vector3 prefabScale = Vector3.one;

        [Header("Connections")]
        [SerializeField] private bool hasNorthDoor = true;
        [SerializeField] private bool hasSouthDoor = true;
        [SerializeField] private bool hasEastDoor = true;
        [SerializeField] private bool hasWestDoor = true;

        [Header("Spawn Settings")]
        [SerializeField] private int minEnemySpawns = 0;
        [SerializeField] private int maxEnemySpawns = 3;
        [SerializeField] private int minLootSpawns = 0;
        [SerializeField] private int maxLootSpawns = 2;

        [Header("Weight & Rarity")]
        [SerializeField] private float spawnWeight = 1f; // Higher = more likely to spawn
        [SerializeField] private int minDepth = 0; // Minimum floor depth to appear
        [SerializeField] private int maxDepth = 999; // Maximum floor depth to appear

        [Header("Prefab Reference")]
        [SerializeField] private GameObject roomPrefab;

        public string RoomName => roomName;
        public RoomType RoomType => roomType;
        public Vector2Int Size => size;
        public Vector3 PrefabScale => prefabScale;
        public bool HasNorthDoor => hasNorthDoor;
        public bool HasSouthDoor => hasSouthDoor;
        public bool HasEastDoor => hasEastDoor;
        public bool HasWestDoor => hasWestDoor;
        public int MinEnemySpawns => minEnemySpawns;
        public int MaxEnemySpawns => maxEnemySpawns;
        public int MinLootSpawns => minLootSpawns;
        public int MaxLootSpawns => maxLootSpawns;
        public float SpawnWeight => spawnWeight;
        public int MinDepth => minDepth;
        public int MaxDepth => maxDepth;
        public GameObject RoomPrefab => roomPrefab;

        public bool CanSpawnAtDepth(int depth)
        {
            return depth >= minDepth && depth <= maxDepth;
        }

        public bool HasDoorInDirection(Direction direction)
        {
            return direction switch
            {
                Direction.North => hasNorthDoor,
                Direction.South => hasSouthDoor,
                Direction.East => hasEastDoor,
                Direction.West => hasWestDoor,
                _ => false
            };
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            // Validation du prefab
            if (roomPrefab != null)
            {
                var roomModule = roomPrefab.GetComponent<RoomModule>();
                if (roomModule == null)
                {
                    Debug.LogError($"[RoomData] Le prefab '{roomPrefab.name}' n'a pas de composant RoomModule! Ajouter un RoomModule au prefab.", this);
                }
                else
                {
                    // Vérifier que les portes configurées correspondent
                    ValidateDoor(Direction.North, hasNorthDoor, roomModule);
                    ValidateDoor(Direction.South, hasSouthDoor, roomModule);
                    ValidateDoor(Direction.East, hasEastDoor, roomModule);
                    ValidateDoor(Direction.West, hasWestDoor, roomModule);
                }
            }

            // Validation des valeurs
            if (spawnWeight < 0f)
            {
                Debug.LogWarning($"[RoomData] '{roomName}' a un spawnWeight négatif ({spawnWeight}). Sera mis à 0.", this);
                spawnWeight = 0f;
            }

            if (minEnemySpawns < 0) minEnemySpawns = 0;
            if (maxEnemySpawns < minEnemySpawns) maxEnemySpawns = minEnemySpawns;
            if (minLootSpawns < 0) minLootSpawns = 0;
            if (maxLootSpawns < minLootSpawns) maxLootSpawns = minLootSpawns;
        }

        private void ValidateDoor(Direction direction, bool shouldHaveDoor, RoomModule roomModule)
        {
            var door = roomModule.GetDoor(direction);
            if (shouldHaveDoor && door == null)
            {
                Debug.LogWarning($"[RoomData] '{roomName}' est configuré pour avoir une porte {direction}, mais le prefab n'en a pas!", this);
            }
            else if (!shouldHaveDoor && door != null)
            {
                Debug.LogWarning($"[RoomData] '{roomName}' n'est pas configuré pour avoir une porte {direction}, mais le prefab en a une!", this);
            }
        }
#endif
    }

    public enum RoomType
    {
        Combat,
        Elite,          // Combat difficile
        Shop,           // Magasin
        Event,          // Événement spécial
        Loot,
        Boss,
        Hub,
        Corridor,
        Special
    }

    public enum Direction
    {
        North = 0,
        East = 1,
        South = 2,
        West = 3
    }
}

