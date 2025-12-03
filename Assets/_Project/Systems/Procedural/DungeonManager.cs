using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace ProjectRoguelike.Procedural
{
    /// <summary>
    /// MonoBehaviour that manages procedural dungeon generation and assembly.
    /// </summary>
    public sealed class DungeonManager : MonoBehaviour
    {
        [Header("Generation Settings")]
        [SerializeField] private int targetRoomCount = 8;
        [SerializeField] private int seed = -1; // -1 = random
        [SerializeField] private List<RoomData> availableRooms = new();

        [Header("Assembly")]
        [SerializeField] private Transform dungeonRoot;

        [Header("Spawning")]
        [SerializeField] private GameObject enemyPrefab;
        [SerializeField] private bool autoSpawnEnemies = true;

        private LevelSeed _levelSeed;
        private DungeonGenerator _generator;
        private RoomAssembler _assembler;
        private SpawnPointManager _spawnManager;

        public DungeonGenerator Generator => _generator;
        public RoomAssembler Assembler => _assembler;

        private void Awake()
        {
            if (dungeonRoot == null)
            {
                dungeonRoot = new GameObject("DungeonRoot").transform;
            }

            _assembler = new RoomAssembler(dungeonRoot);
        }

        [ContextMenu("Generate Dungeon")]
        public void GenerateDungeon()
        {
            if (availableRooms == null || availableRooms.Count == 0)
            {
                Debug.LogError("[DungeonManager] No available rooms assigned!");
                return;
            }

            // Initialize assembler if not already done (for Context Menu calls in Edit mode)
            if (_assembler == null)
            {
                if (dungeonRoot == null)
                {
                    dungeonRoot = new GameObject("DungeonRoot").transform;
                }
                _assembler = new RoomAssembler(dungeonRoot);
            }

            // Create seed
            _levelSeed = seed == -1 ? new LevelSeed() : new LevelSeed(seed);

            // Generate layout
            _generator = new DungeonGenerator(_levelSeed);
            _generator.Generate(targetRoomCount, availableRooms);

            // Assemble rooms
            _assembler.AssembleDungeon(_generator);

            // Rebuild NavMesh after rooms are placed
            RebuildNavMesh();

            // Wait a frame for NavMesh to be fully ready
            // (NavMesh rebuild is synchronous but agents need a frame to register)
            if (Application.isPlaying)
            {
                StartCoroutine(SpawnEnemiesAfterNavMeshReady(enemyPrefab));
            }
            else
            {
                // In editor mode, spawn immediately
                if (autoSpawnEnemies && enemyPrefab != null)
                {
                    _spawnManager = new SpawnPointManager(_levelSeed);
                    _spawnManager.SpawnInAllRooms(_assembler, _generator, enemyPrefab);
                }
            }

        }

        [ContextMenu("Clear Dungeon")]
        public void ClearDungeon()
        {
            _spawnManager?.Clear();
            _assembler?.Clear();
            
            // Destroy DungeonRoot and all its children
            if (dungeonRoot != null)
            {
                if (Application.isPlaying)
                {
                    Object.Destroy(dungeonRoot.gameObject);
                }
                else
                {
                    Object.DestroyImmediate(dungeonRoot.gameObject);
                }
                dungeonRoot = null;
            }
            
            _generator = null;
            _spawnManager = null;
            _assembler = null; // Reset assembler so it gets recreated on next generation
        }

        public void SetSeed(int newSeed)
        {
            seed = newSeed;
        }

        public void SetTargetRoomCount(int roomCount)
        {
            targetRoomCount = roomCount;
        }

        public void SetAvailableRooms(List<RoomData> rooms)
        {
            if (rooms != null)
            {
                availableRooms = new List<RoomData>(rooms); // Create a copy to avoid reference issues
            }
        }

        public int GetCurrentSeed()
        {
            return _levelSeed?.Seed ?? seed;
        }

        private void RebuildNavMesh()
        {
            // Try to find NavMeshSurface component (AI Navigation package)
            var navMeshSurfaceType = System.Type.GetType("Unity.AI.Navigation.NavMeshSurface, Unity.AI.Navigation");
            if (navMeshSurfaceType != null)
            {
                var navMeshSurface = FindObjectOfType(navMeshSurfaceType);
                if (navMeshSurface != null)
                {
                    var buildMethod = navMeshSurfaceType.GetMethod("BuildNavMesh");
                    buildMethod?.Invoke(navMeshSurface, null);
                    return;
                }
            }

            // Fallback: Manual rebake using NavMeshBuilder
            var buildSettings = NavMesh.GetSettingsByID(0);
            var bounds = new Bounds(Vector3.zero, Vector3.one * 1000f); // Large bounds to cover all rooms
            
            // Collect all static geometry
            var sources = new List<NavMeshBuildSource>();
            NavMeshBuilder.CollectSources(bounds, 0, NavMeshCollectGeometry.RenderMeshes, 0, new List<NavMeshBuildMarkup>(), sources);
            
            var data = NavMeshBuilder.BuildNavMeshData(buildSettings, sources, bounds, Vector3.zero, Quaternion.identity);
            if (data != null)
            {
                NavMesh.RemoveAllNavMeshData();
                NavMesh.AddNavMeshData(data);
            }
            else
            {
                Debug.LogWarning("[DungeonManager] Failed to rebuild NavMesh. Make sure rooms are marked as Navigation Static.");
            }
        }

        private System.Collections.IEnumerator SpawnEnemiesAfterNavMeshReady(GameObject enemyPrefab)
        {
            // Wait a frame for NavMesh to be fully registered
            yield return null;

            // Spawn enemies (after NavMesh is ready)
            if (autoSpawnEnemies && enemyPrefab != null)
            {
                _spawnManager = new SpawnPointManager(_levelSeed);
                _spawnManager.SpawnInAllRooms(_assembler, _generator, enemyPrefab);
                
                // Disable NavMeshAgents temporarily and re-enable them after a frame
                // This ensures they're properly placed on the NavMesh
                var allEnemies = new List<NavMeshAgent>();
                foreach (var spawned in _spawnManager.SpawnedObjects)
                {
                    if (spawned != null)
                    {
                        var agent = spawned.GetComponent<NavMeshAgent>();
                        if (agent != null)
                        {
                            agent.enabled = false;
                            allEnemies.Add(agent);
                        }
                    }
                }

                // Wait another frame
                yield return null;

                // Re-enable NavMeshAgents now that NavMesh is ready
                foreach (var agent in allEnemies)
                {
                    if (agent != null)
                    {
                        agent.enabled = true;
                        // Warp agent to its current position to ensure it's on NavMesh
                        agent.Warp(agent.transform.position);
                    }
                }
            }
        }
    }
}

