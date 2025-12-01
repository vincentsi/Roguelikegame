using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace ProjectRoguelike.Procedural
{
    /// <summary>
    /// Manages spawning of enemies, loot, and props in generated rooms.
    /// </summary>
    public sealed class SpawnPointManager
    {
        private readonly LevelSeed _seed;
        private readonly List<GameObject> _spawnedObjects = new();

        public SpawnPointManager(LevelSeed seed)
        {
            _seed = seed ?? throw new System.ArgumentNullException(nameof(seed));
        }

        public void SpawnInRoom(RoomModule roomModule, GameObject enemyPrefab = null)
        {
            if (roomModule == null)
            {
                return;
            }

            // Spawn enemies
            if (enemyPrefab != null && roomModule.EnemySpawnPoints.Count > 0)
            {
                var enemyCount = UnityEngine.Random.Range(
                    roomModule.RoomType == RoomType.Boss ? 1 : 0,
                    roomModule.EnemySpawnPoints.Count + 1
                );

                var availableSpawns = new List<Transform>(roomModule.EnemySpawnPoints);
                for (int i = 0; i < enemyCount && availableSpawns.Count > 0; i++)
                {
                    var spawnIndex = _seed.NextInt(availableSpawns.Count);
                    var spawnPoint = availableSpawns[spawnIndex];
                    availableSpawns.RemoveAt(spawnIndex);

                    // Calculate spawn position on NavMesh (above ground)
                    var spawnPosition = spawnPoint.position;
                    
                    // Try to find valid position on NavMesh
                    if (NavMesh.SamplePosition(spawnPosition, out var hit, 5f, NavMesh.AllAreas))
                    {
                        spawnPosition = hit.position;
                    }
                    else
                    {
                        // Fallback: raise Y position (assuming enemy is ~2 units tall, pivot at center)
                        spawnPosition.y += 1f; // Adjust based on enemy height
                    }

                    var enemy = Object.Instantiate(enemyPrefab, spawnPosition, spawnPoint.rotation, roomModule.transform);
                    enemy.name = $"Enemy_{roomModule.name}_{i}";
                    _spawnedObjects.Add(enemy);
                }
            }

            // TODO: Spawn loot and props when systems are ready
        }

        public void SpawnInAllRooms(RoomAssembler assembler, DungeonGenerator generator, GameObject enemyPrefab)
        {
            if (generator == null || assembler == null)
            {
                return;
            }

            // Get all RoomModules in the scene (from assembled rooms)
            var roomModules = Object.FindObjectsOfType<RoomModule>();
            foreach (var roomModule in roomModules)
            {
                SpawnInRoom(roomModule, enemyPrefab);
            }
        }

        public void Clear()
        {
            foreach (var obj in _spawnedObjects)
            {
                if (obj != null)
                {
                    Object.Destroy(obj);
                }
            }
            _spawnedObjects.Clear();
        }
    }
}

