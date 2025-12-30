using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace ProjectRoguelike.Procedural
{
    /// <summary>
    /// Gère le chargement/déchargement asynchrone des salles.
    /// </summary>
    public sealed class RoomLoader
    {
        private readonly Transform _dungeonRoot;
        private readonly Dictionary<RoomNode, GameObject> _loadedRooms = new();
        private RoomNode _currentActiveRoom;
        private readonly Queue<RoomNode> _preloadQueue = new();
        private const float RoomSpacing = 20f;

        public RoomLoader(Transform dungeonRoot)
        {
            _dungeonRoot = dungeonRoot != null ? dungeonRoot : new GameObject("DungeonRoot").transform;
        }

        public async Task LoadRoomAsync(RoomNode node)
        {
            if (node == null)
            {
                Debug.LogError("[RoomLoader] Cannot load null room node!");
                return;
            }

            if (_loadedRooms.ContainsKey(node))
            {
                return; // Déjà chargé
            }

            var roomPrefab = node.RoomData.RoomPrefab;
            if (roomPrefab == null)
            {
                Debug.LogError($"[RoomLoader] Room {node.RoomData.RoomName} has no prefab!");
                return;
            }

            // Calculer position 3D
            var worldPosition = new Vector3(
                node.GridPosition.x * RoomSpacing,
                0f,
                node.GridPosition.y * RoomSpacing
            );

            // Instancier (pour l'instant synchrone, peut être amélioré avec Addressables)
            var roomInstance = Object.Instantiate(roomPrefab, worldPosition, Quaternion.identity, _dungeonRoot);
            roomInstance.name = $"{node.RoomData.RoomName}_{node.NodeId}";
            roomInstance.SetActive(false); // Désactivé par défaut

            _loadedRooms[node] = roomInstance;

            // Attendre une frame pour permettre l'initialisation
            await Task.Yield();
        }

        public async Task UnloadRoomAsync(RoomNode node)
        {
            if (node == null || !_loadedRooms.TryGetValue(node, out var roomInstance))
            {
                return;
            }

            if (roomInstance != null)
            {
                Object.Destroy(roomInstance);
            }

            _loadedRooms.Remove(node);
            await Task.Yield();
        }

        public void PreloadAdjacentRooms(RoomNode currentRoom)
        {
            if (currentRoom == null)
            {
                return;
            }

            _currentActiveRoom = currentRoom;

            // Trouver les salles adjacentes (1-2 de distance)
            var adjacentRooms = FindAdjacentRooms(currentRoom, maxDistance: 2);

            foreach (var room in adjacentRooms)
            {
                if (!_loadedRooms.ContainsKey(room))
                {
                    // Charger en arrière-plan (ne pas attendre)
                    _ = LoadRoomAsync(room);
                }
            }
        }

        public void UnloadDistantRooms(RoomNode currentRoom, int maxDistance)
        {
            if (currentRoom == null)
            {
                return;
            }

            var roomsToUnload = new List<RoomNode>();

            foreach (var kvp in _loadedRooms)
            {
                var node = kvp.Key;
                if (node == currentRoom)
                {
                    continue; // Ne pas décharger la salle actuelle
                }

                // Calculer la distance
                int distance = CalculateDistance(currentRoom, node);
                if (distance > maxDistance)
                {
                    roomsToUnload.Add(node);
                }
            }

            // Décharger les salles distantes
            foreach (var node in roomsToUnload)
            {
                _ = UnloadRoomAsync(node);
            }
        }

        public GameObject GetLoadedRoom(RoomNode node)
        {
            return _loadedRooms.TryGetValue(node, out var room) ? room : null;
        }

        public bool IsRoomLoaded(RoomNode node)
        {
            return _loadedRooms.ContainsKey(node);
        }

        public void ClearAll()
        {
            foreach (var room in _loadedRooms.Values)
            {
                if (room != null)
                {
                    Object.Destroy(room);
                }
            }

            _loadedRooms.Clear();
            _preloadQueue.Clear();
            _currentActiveRoom = null;
        }

        private List<RoomNode> FindAdjacentRooms(RoomNode from, int maxDistance)
        {
            var adjacent = new List<RoomNode>();
            var visited = new HashSet<RoomNode> { from };
            var queue = new Queue<(RoomNode node, int distance)>();
            queue.Enqueue((from, 0));

            while (queue.Count > 0)
            {
                var (current, distance) = queue.Dequeue();

                if (distance > 0 && distance <= maxDistance)
                {
                    adjacent.Add(current);
                }

                if (distance >= maxDistance)
                {
                    continue;
                }

                // Parcourir les connexions
                var directions = new[] { Direction.North, Direction.East, Direction.South, Direction.West };
                foreach (var dir in directions)
                {
                    var next = current.GetConnection(dir);
                    if (next != null && !visited.Contains(next))
                    {
                        visited.Add(next);
                        queue.Enqueue((next, distance + 1));
                    }
                }
            }

            return adjacent;
        }

        private int CalculateDistance(RoomNode a, RoomNode b)
        {
            if (a == null || b == null)
            {
                return int.MaxValue;
            }

            // Distance Manhattan
            return Mathf.Abs(a.GridPosition.x - b.GridPosition.x) + 
                   Mathf.Abs(a.GridPosition.y - b.GridPosition.y);
        }
    }
}

