using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ProjectRoguelike.Procedural
{
    /// <summary>
    /// Generates a dungeon layout using a graph-based approach.
    /// Creates a network of connected rooms based on RoomData configurations.
    /// </summary>
    public sealed class DungeonGenerator
    {
        private readonly LevelSeed _seed;
        private readonly List<RoomNode> _nodes = new();
        private readonly Dictionary<Vector2Int, RoomNode> _grid = new();

        public IReadOnlyList<RoomNode> Nodes => _nodes;
        public int RoomCount => _nodes.Count;

        public DungeonGenerator(LevelSeed seed)
        {
            _seed = seed ?? throw new System.ArgumentNullException(nameof(seed));
        }

        public void Generate(int targetRoomCount, List<RoomData> availableRooms)
        {
            if (availableRooms == null || availableRooms.Count == 0)
            {
                Debug.LogError("[DungeonGenerator] No available rooms provided!");
                return;
            }

            _nodes.Clear();
            _grid.Clear();

            // Start with a spawn room (usually combat or special)
            var startRoom = SelectRoom(availableRooms, RoomType.Combat, 0);
            if (startRoom == null)
            {
                startRoom = availableRooms[0]; // Fallback
            }

            var startNode = new RoomNode(Vector2Int.zero, startRoom);
            _nodes.Add(startNode);
            _grid[Vector2Int.zero] = startNode;

            // Generate additional rooms
            int attempts = 0;
            int maxAttempts = targetRoomCount * 10;

            while (_nodes.Count < targetRoomCount && attempts < maxAttempts)
            {
                attempts++;

                // Pick a random existing node to branch from
                var parentNode = _nodes[_seed.NextInt(_nodes.Count)];

                // Pick a random direction
                var direction = (Direction)_seed.NextInt(4);
                var offset = GetDirectionOffset(direction);
                var newPosition = parentNode.GridPosition + offset;

                // Check if position is free
                if (_grid.ContainsKey(newPosition))
                {
                    continue;
                }

                // Check if parent has door in this direction
                if (!parentNode.RoomData.HasDoorInDirection(direction))
                {
                    continue;
                }

                // Select appropriate room type based on depth and requirements
                var depth = Mathf.Max(Mathf.Abs(newPosition.x), Mathf.Abs(newPosition.y));
                RoomType preferredType = depth >= targetRoomCount - 2 ? RoomType.Boss : RoomType.Combat;

                var newRoom = SelectRoom(availableRooms, preferredType, depth);
                if (newRoom == null)
                {
                    continue;
                }

                // Check if new room has door in opposite direction
                var oppositeDir = GetOppositeDirection(direction);
                if (!newRoom.HasDoorInDirection(oppositeDir))
                {
                    continue;
                }

                // Create new node
                var newNode = new RoomNode(newPosition, newRoom);
                newNode.SetConnection(parentNode, direction);
                parentNode.SetConnection(newNode, oppositeDir);

                _nodes.Add(newNode);
                _grid[newPosition] = newNode;
            }

            Debug.Log($"[DungeonGenerator] Generated {_nodes.Count} rooms with seed {_seed.Seed}");
        }

        private RoomData SelectRoom(List<RoomData> availableRooms, RoomType preferredType, int depth)
        {
            // Filter rooms by depth and type
            var validRooms = availableRooms
                .Where(r => r.CanSpawnAtDepth(depth))
                .Where(r => r.RoomType == preferredType || preferredType == RoomType.Combat)
                .ToList();

            if (validRooms.Count == 0)
            {
                // Fallback: any room at this depth
                validRooms = availableRooms.Where(r => r.CanSpawnAtDepth(depth)).ToList();
            }

            if (validRooms.Count == 0)
            {
                return null;
            }

            // Weighted random selection
            float totalWeight = validRooms.Sum(r => r.SpawnWeight);
            float randomValue = _seed.NextFloat() * totalWeight;

            foreach (var room in validRooms)
            {
                randomValue -= room.SpawnWeight;
                if (randomValue <= 0f)
                {
                    return room;
                }
            }

            return validRooms[0]; // Fallback
        }

        private Vector2Int GetDirectionOffset(Direction direction)
        {
            return direction switch
            {
                Direction.North => Vector2Int.up,
                Direction.South => Vector2Int.down,
                Direction.East => Vector2Int.right,
                Direction.West => Vector2Int.left,
                _ => Vector2Int.zero
            };
        }

        private Direction GetOppositeDirection(Direction direction)
        {
            return (Direction)(((int)direction + 2) % 4);
        }
    }

    public sealed class RoomNode
    {
        public Vector2Int GridPosition { get; }
        public RoomData RoomData { get; }
        private readonly Dictionary<Direction, RoomNode> _connections = new();

        public RoomNode(Vector2Int gridPosition, RoomData roomData)
        {
            GridPosition = gridPosition;
            RoomData = roomData;
        }

        public void SetConnection(RoomNode other, Direction direction)
        {
            _connections[direction] = other;
        }

        public RoomNode GetConnection(Direction direction)
        {
            return _connections.TryGetValue(direction, out var node) ? node : null;
        }

        public bool HasConnection(Direction direction)
        {
            return _connections.ContainsKey(direction);
        }

        public IEnumerable<Direction> GetConnectedDirections()
        {
            return _connections.Keys;
        }
    }
}

