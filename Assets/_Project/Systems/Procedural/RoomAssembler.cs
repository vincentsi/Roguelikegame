using System.Collections.Generic;
using UnityEngine;

namespace ProjectRoguelike.Procedural
{
    /// <summary>
    /// Assembles the dungeon by instantiating room prefabs and connecting them.
    /// </summary>
    public sealed class RoomAssembler
    {
        private readonly Transform _root;
        private readonly Dictionary<RoomNode, GameObject> _instantiatedRooms = new();
        private const float RoomSpacing = 20f; // Distance between room centers

        public RoomAssembler(Transform root)
        {
            _root = root != null ? root : new GameObject("DungeonRoot").transform;
        }

        public void AssembleDungeon(DungeonGenerator generator)
        {
            if (generator == null || generator.Nodes.Count == 0)
            {
                Debug.LogError("[RoomAssembler] Invalid generator or no rooms to assemble!");
                return;
            }

            // Clear existing rooms
            foreach (var room in _instantiatedRooms.Values)
            {
                if (room != null)
                {
                    Object.Destroy(room);
                }
            }
            _instantiatedRooms.Clear();

            // Instantiate all rooms
            foreach (var node in generator.Nodes)
            {
                var roomPrefab = node.RoomData.RoomPrefab;
                if (roomPrefab == null)
                {
                    Debug.LogWarning($"[RoomAssembler] Room {node.RoomData.RoomName} has no prefab assigned!");
                    continue;
                }

                var worldPosition = new Vector3(
                    node.GridPosition.x * RoomSpacing,
                    0f,
                    node.GridPosition.y * RoomSpacing
                );

                var roomInstance = Object.Instantiate(roomPrefab, worldPosition, Quaternion.identity, _root);
                roomInstance.name = $"{node.RoomData.RoomName}_{node.GridPosition}";
                _instantiatedRooms[node] = roomInstance;

                // Configure room module if present
                var roomModule = roomInstance.GetComponent<RoomModule>();
                if (roomModule == null)
                {
                    Debug.LogWarning($"[RoomAssembler] Room {roomInstance.name} has no RoomModule component!");
                }
            }

            // Connect rooms (align doors)
            foreach (var node in generator.Nodes)
            {
                if (!_instantiatedRooms.TryGetValue(node, out var roomInstance))
                {
                    continue;
                }

                var roomModule = roomInstance.GetComponent<RoomModule>();
                if (roomModule == null)
                {
                    continue;
                }

                // Connect to neighbors
                foreach (var direction in node.GetConnectedDirections())
                {
                    var neighbor = node.GetConnection(direction);
                    if (neighbor == null || !_instantiatedRooms.TryGetValue(neighbor, out var neighborInstance))
                    {
                        continue;
                    }

                    var neighborModule = neighborInstance.GetComponent<RoomModule>();
                    if (neighborModule == null)
                    {
                        continue;
                    }

                    // Align doors (optional - can be done via positioning or door prefabs)
                    AlignDoors(roomModule, direction, neighborModule, GetOppositeDirection(direction));
                }
            }

            Debug.Log($"[RoomAssembler] Assembled {_instantiatedRooms.Count} rooms");
        }

        private void AlignDoors(RoomModule room1, Direction dir1, RoomModule room2, Direction dir2)
        {
            var door1 = room1.GetDoor(dir1);
            var door2 = room2.GetDoor(dir2);

            if (door1 != null && door2 != null)
            {
                // Doors are already positioned correctly by room spacing
                // This can be extended to spawn corridor prefabs if needed
            }
        }

        private Direction GetOppositeDirection(Direction direction)
        {
            return (Direction)(((int)direction + 2) % 4);
        }

        public void Clear()
        {
            foreach (var room in _instantiatedRooms.Values)
            {
                if (room != null)
                {
                    Object.Destroy(room);
                }
            }
            _instantiatedRooms.Clear();
        }
    }
}

