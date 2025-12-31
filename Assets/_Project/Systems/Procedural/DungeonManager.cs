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
        [SerializeField] private bool autoSpawnPlayer = true;

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

            // Wait a frame for NavMesh to be fully ready, then spawn player and enemies
            if (Application.isPlaying)
            {
                StartCoroutine(SpawnEntitiesAfterNavMeshReady(enemyPrefab));
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
                    Debug.Log("[DungeonManager] Building NavMesh using NavMeshSurface...");
                    var buildMethod = navMeshSurfaceType.GetMethod("BuildNavMesh");
                    buildMethod?.Invoke(navMeshSurface, null);
                    Debug.Log("[DungeonManager] NavMesh built successfully!");
                    return;
                }
                else
                {
                    Debug.LogWarning("[DungeonManager] NavMeshSurface component not found. Add NavMeshSurface to scene or DungeonManager GameObject.");
                }
            }

            // Fallback: Manual rebake using NavMeshBuilder
            Debug.Log("[DungeonManager] Building NavMesh using NavMeshBuilder (fallback)...");
            var buildSettings = NavMesh.GetSettingsByID(0);

            // Calculate bounds based on actual dungeon size
            Bounds bounds = new Bounds(dungeonRoot.position, Vector3.one);
            foreach (Transform child in dungeonRoot)
            {
                var renderers = child.GetComponentsInChildren<Renderer>();
                foreach (var renderer in renderers)
                {
                    bounds.Encapsulate(renderer.bounds);
                }
            }
            bounds.Expand(10f); // Add some padding

            // Collect all geometry from dungeon root
            var sources = new List<NavMeshBuildSource>();
            var markups = new List<NavMeshBuildMarkup>();
            NavMeshBuilder.CollectSources(dungeonRoot, ~0, NavMeshCollectGeometry.PhysicsColliders, 0, markups, sources);

            if (sources.Count == 0)
            {
                Debug.LogError("[DungeonManager] No NavMesh sources found! Make sure room prefabs have colliders and are children of DungeonRoot.");
                return;
            }

            Debug.Log($"[DungeonManager] Found {sources.Count} NavMesh sources in bounds {bounds}");

            var data = NavMeshBuilder.BuildNavMeshData(buildSettings, sources, bounds, Vector3.zero, Quaternion.identity);
            if (data != null)
            {
                NavMesh.RemoveAllNavMeshData();
                NavMesh.AddNavMeshData(data);
                Debug.Log("[DungeonManager] NavMesh built successfully!");
            }
            else
            {
                Debug.LogError("[DungeonManager] Failed to build NavMesh data. Check that rooms have colliders.");
            }
        }

        private void SpawnPlayerIfNeeded()
        {
            // Get PlayerManager first
            var bootstrap = ProjectRoguelike.Core.AppBootstrap.Instance;
            if (bootstrap == null || !bootstrap.Services.TryResolve<ProjectRoguelike.Core.PlayerManager>(out var playerManager))
            {
                Debug.LogError("[DungeonManager] PlayerManager not available! Cannot spawn player.");
                return;
            }

            // Check if player is already registered in PlayerManager (more reliable than tag search)
            if (playerManager.PlayerCount > 0)
            {
                Debug.Log("[DungeonManager] Player already registered in PlayerManager, skipping spawn.");
                return;
            }

            // Check if unregistered player exists in scene and register it
            var existingPlayer = GameObject.FindGameObjectWithTag("Player");
            if (existingPlayer != null)
            {
                Debug.Log("[DungeonManager] Found existing unregistered player, registering it now.");
                playerManager.RegisterPlayer(existingPlayer.transform);
                return;
            }

            // Load player prefab from Resources
            var playerPrefab = Resources.Load<GameObject>("Prefabs/player/Player");
            if (playerPrefab == null)
            {
                Debug.LogError("[DungeonManager] Player prefab not found at Resources/Prefabs/player/Player!");
                return;
            }

            // Find spawn position (first room center)
            // Convert GridPosition to world position using RoomSpacing constant (20f)
            Vector3 spawnPosition = Vector3.zero;
            if (_generator != null && _generator.Nodes.Count > 0)
            {
                var firstRoom = _generator.Nodes[0];
                const float RoomSpacing = 20f; // Must match RoomAssembler.RoomSpacing
                spawnPosition = new Vector3(
                    firstRoom.GridPosition.x * RoomSpacing,
                    1f, // Spawn 1m above ground
                    firstRoom.GridPosition.y * RoomSpacing
                );
            }

            // Instantiate player
            var player = Instantiate(playerPrefab, spawnPosition, Quaternion.identity);
            player.name = "Player";

            // Ensure player has the Player tag
            if (!player.CompareTag("Player"))
            {
                player.tag = "Player";
            }

            // Register with PlayerManager IMMEDIATELY (don't wait for PlayerRegistration component)
            playerManager.RegisterPlayer(player.transform);
            Debug.Log($"[DungeonManager] Player spawned and registered at {spawnPosition}! PlayerCount: {playerManager.PlayerCount}");
        }

        private System.Collections.IEnumerator SpawnEntitiesAfterNavMeshReady(GameObject enemyPrefab)
        {
            // Wait a frame for NavMesh to be fully registered
            yield return null;

            // Spawn player FIRST (so PlayerManager has player registered before enemies spawn)
            if (autoSpawnPlayer)
            {
                SpawnPlayerIfNeeded();
            }

            // Wait another frame to ensure player is fully registered in PlayerManager
            yield return null;

            // Spawn enemies (after NavMesh and player are ready)
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

