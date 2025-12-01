using System.Collections.Generic;
using UnityEngine;

namespace ProjectRoguelike.Procedural
{
    /// <summary>
    /// Component attached to room prefabs. Manages connections, spawn points, and room metadata.
    /// </summary>
    public sealed class RoomModule : MonoBehaviour
    {
        [Header("Connections")]
        [SerializeField] private Transform northDoor;
        [SerializeField] private Transform southDoor;
        [SerializeField] private Transform eastDoor;
        [SerializeField] private Transform westDoor;

        [Header("Spawn Points")]
        [SerializeField] private List<Transform> enemySpawnPoints = new();
        [SerializeField] private List<Transform> lootSpawnPoints = new();
        [SerializeField] private List<Transform> propSpawnPoints = new();

        [Header("Room Info")]
        [SerializeField] private RoomType roomType = RoomType.Combat;
        [SerializeField] private Vector2Int gridSize = new Vector2Int(1, 1);

        public RoomType RoomType => roomType;
        public Vector2Int GridSize => gridSize;

        public Transform GetDoor(Direction direction)
        {
            return direction switch
            {
                Direction.North => northDoor,
                Direction.South => southDoor,
                Direction.East => eastDoor,
                Direction.West => westDoor,
                _ => null
            };
        }

        public IReadOnlyList<Transform> EnemySpawnPoints => enemySpawnPoints;
        public IReadOnlyList<Transform> LootSpawnPoints => lootSpawnPoints;
        public IReadOnlyList<Transform> PropSpawnPoints => propSpawnPoints;

        public void AddEnemySpawnPoint(Transform point)
        {
            if (point != null && !enemySpawnPoints.Contains(point))
            {
                enemySpawnPoints.Add(point);
            }
        }

        public void AddLootSpawnPoint(Transform point)
        {
            if (point != null && !lootSpawnPoints.Contains(point))
            {
                lootSpawnPoints.Add(point);
            }
        }

        public void AddPropSpawnPoint(Transform point)
        {
            if (point != null && !propSpawnPoints.Contains(point))
            {
                propSpawnPoints.Add(point);
            }
        }

        private void OnDrawGizmosSelected()
        {
            // Draw door positions
            Gizmos.color = Color.cyan;
            if (northDoor != null) Gizmos.DrawWireSphere(northDoor.position, 0.5f);
            if (southDoor != null) Gizmos.DrawWireSphere(southDoor.position, 0.5f);
            if (eastDoor != null) Gizmos.DrawWireSphere(eastDoor.position, 0.5f);
            if (westDoor != null) Gizmos.DrawWireSphere(westDoor.position, 0.5f);

            // Draw spawn points
            Gizmos.color = Color.red;
            foreach (var point in enemySpawnPoints)
            {
                if (point != null) Gizmos.DrawWireCube(point.position, Vector3.one * 0.5f);
            }

            Gizmos.color = Color.yellow;
            foreach (var point in lootSpawnPoints)
            {
                if (point != null) Gizmos.DrawWireCube(point.position, Vector3.one * 0.5f);
            }

            Gizmos.color = Color.green;
            foreach (var point in propSpawnPoints)
            {
                if (point != null) Gizmos.DrawWireCube(point.position, Vector3.one * 0.5f);
            }
        }
    }
}

