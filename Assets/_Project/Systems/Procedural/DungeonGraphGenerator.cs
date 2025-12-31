using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ProjectRoguelike.Levels;

namespace ProjectRoguelike.Procedural
{
    /// <summary>
    /// Générateur de graphe de donjon style Hades.
    /// Crée un graphe avec 4-6 salles avant le boss, 2-3 portes par salle.
    /// </summary>
    public sealed class DungeonGraphGenerator
    {
        private readonly LevelSeed _seed;
        private readonly DungeonGeneratorSettings _settings;
        private int _nextEdgeId = 0;
        private LevelTheme _currentTheme;
        private RewardGenerator _rewardGenerator;

        public DungeonGraphGenerator(LevelSeed seed, DungeonGeneratorSettings settings)
        {
            _seed = seed ?? throw new System.ArgumentNullException(nameof(seed));
            _settings = settings ?? throw new System.ArgumentNullException(nameof(settings));
        }

        public DungeonGraph Generate(int minRooms, int maxRooms, LevelTheme theme)
        {
            if (theme == null)
            {
                Debug.LogError("[DungeonGraphGenerator] LevelTheme is null!");
                return null;
            }

            // Initialiser le thème et le générateur de récompenses
            _currentTheme = theme;
            _rewardGenerator = new RewardGenerator(_seed, theme);

            var graph = new DungeonGraph
            {
                Theme = theme,
                Seed = _seed
            };

            int attempts = 0;
            while (attempts < _settings.MaxGenerationAttempts)
            {
                attempts++;
                graph.Clear();
                _nextEdgeId = 0;

                // 1. Créer StartRoom
                var startRoom = CreateStartRoom(theme);
                if (startRoom == null)
                {
                    continue;
                }

                var startNode = new RoomNode(Vector2Int.zero, startRoom)
                {
                    IsStartRoom = true,
                    Depth = 0
                };
                graph.AddNode(startNode);
                graph.StartNode = startNode;

                // 2. Déterminer nombre de salles
                int roomCount = _seed.NextInt(minRooms, maxRooms + 1);
                int totalRooms = roomCount + 1; // +1 pour le boss

                // 3. Créer les salles intermédiaires
                var pathRooms = CreatePathRooms(roomCount, totalRooms, theme, graph);
                if (pathRooms == null || pathRooms.Count < roomCount)
                {
                    continue; // Réessayer
                }

                // 4. Créer BossRoom
                var bossNode = CreateBossRoom(theme, graph, totalRooms);
                if (bossNode == null)
                {
                    continue;
                }
                graph.BossNode = bossNode;

                // 5. Connecter toutes les salles au boss
                EnsureBossReachable(graph, pathRooms, bossNode);

                // 6. Assigner des récompenses à toutes les edges
                AssignRewardsToGraph(graph, totalRooms);

                // 7. Valider le graphe
                if (ValidateGraph(graph))
                {
                    Debug.Log($"[DungeonGraphGenerator] Generated graph with {graph.Nodes.Count} nodes and {graph.Edges.Count} edges");
                    return graph;
                }
            }

            Debug.LogError($"[DungeonGraphGenerator] Failed to generate valid graph after {attempts} attempts");
            return null;
        }

        private RoomData CreateStartRoom(LevelTheme theme)
        {
            var combatRooms = theme.GetRoomsByType(RoomType.Combat);
            if (combatRooms.Count == 0)
            {
                Debug.LogError("[DungeonGraphGenerator] No combat rooms available for start room!");
                return null;
            }

            // Sélectionner une salle de combat aléatoire
            int index = _seed.NextInt(combatRooms.Count);
            return combatRooms[index];
        }

        private List<RoomNode> CreatePathRooms(int count, int totalRooms, LevelTheme theme, DungeonGraph graph)
        {
            var pathRooms = new List<RoomNode>();
            var availableNodes = new List<RoomNode> { graph.StartNode };
            var grid = new Dictionary<Vector2Int, RoomNode> { { Vector2Int.zero, graph.StartNode } };

            for (int i = 0; i < count; i++)
            {
                int depth = i + 1;
                RoomType roomType = SelectRoomType(depth, totalRooms);

                // Sélectionner un node parent aléatoire
                if (availableNodes.Count == 0)
                {
                    Debug.LogWarning($"[DungeonGraphGenerator] No available nodes to branch from at depth {depth}");
                    break;
                }

                var parentNode = availableNodes[_seed.NextInt(availableNodes.Count)];

                // Créer la nouvelle salle
                var roomData = SelectRoomForType(theme, roomType, depth);
                if (roomData == null)
                {
                    continue;
                }

                // Trouver une position libre
                var position = FindFreePosition(parentNode.GridPosition, grid, _seed);
                if (position == null)
                {
                    continue;
                }

                var newNode = new RoomNode(position.Value, roomData)
                {
                    Depth = depth
                };
                graph.AddNode(newNode);
                pathRooms.Add(newNode);
                grid[position.Value] = newNode;
                availableNodes.Add(newNode);

                // Créer 2-3 connexions sortantes
                int exitCount = _seed.NextInt(_settings.MinExitsPerRoom, _settings.MaxExitsPerRoom + 1);
                CreateExitsForRoom(newNode, exitCount, graph, grid, availableNodes, depth, totalRooms);
            }

            return pathRooms;
        }

        private void CreateExitsForRoom(RoomNode node, int exitCount, DungeonGraph graph,
            Dictionary<Vector2Int, RoomNode> grid, List<RoomNode> availableNodes, int depth, int totalRooms)
        {
            // Créer 2-3 sorties par salle pour offrir des choix au joueur
            var availableDirections = new List<Direction>();

            // Trouver toutes les directions disponibles sur le RoomData
            foreach (Direction dir in System.Enum.GetValues(typeof(Direction)))
            {
                // Vérifier que la salle a une porte dans cette direction
                if (node.RoomData.HasDoorInDirection(dir) && !node.HasConnection(dir))
                {
                    availableDirections.Add(dir);
                }
            }

            // Shuffle pour randomiser
            for (int i = availableDirections.Count - 1; i > 0; i--)
            {
                int j = _seed.NextInt(i + 1);
                (availableDirections[i], availableDirections[j]) = (availableDirections[j], availableDirections[i]);
            }

            // Créer jusqu'à exitCount sorties
            int createdExits = 0;
            foreach (var direction in availableDirections)
            {
                if (createdExits >= exitCount) break;

                // Calculer la position de la salle suivante
                Vector2Int offset = GetDirectionOffset(direction);
                Vector2Int nextPosition = node.GridPosition + offset;

                // Éviter de créer une salle trop proche du départ ou déjà occupée
                if (grid.ContainsKey(nextPosition))
                    continue;

                // Créer une nouvelle salle ou connecter à une salle existante dans availableNodes
                RoomNode targetNode = null;

                // Chercher d'abord une salle disponible proche
                var nearbyRoom = availableNodes.FirstOrDefault(r =>
                    !grid.ContainsKey(nextPosition) &&
                    r.Depth <= depth + 1 &&
                    r != node);

                if (nearbyRoom != null && _seed.NextFloat() > 0.7f) // 30% de chance de connecter à une salle existante
                {
                    targetNode = nearbyRoom;
                }
                else
                {
                    // Créer une nouvelle salle
                    var roomType = SelectRoomType(depth + 1, totalRooms);
                    var roomPool = _currentTheme.GetRoomsByType(roomType);

                    if (roomPool.Count > 0)
                    {
                        int roomIndex = _seed.NextInt(roomPool.Count);
                        var roomData = roomPool[roomIndex];

                        // Vérifier que la nouvelle salle a une porte dans la direction opposée
                        Direction oppositeDir = GetOppositeDirection(direction);
                        if (!roomData.HasDoorInDirection(oppositeDir))
                            continue;

                        targetNode = new RoomNode(nextPosition, roomData)
                        {
                            Depth = depth + 1,
                            IsStartRoom = false
                        };

                        graph.AddNode(targetNode);
                        grid[nextPosition] = targetNode;
                        availableNodes.Add(targetNode);
                    }
                }

                // Créer la connexion si on a une salle cible
                if (targetNode != null)
                {
                    // Sélectionner un type de porte (sera rempli avec des récompenses plus tard)
                    DoorType doorType = SelectDoorType(node, depth, totalRooms);
                    ConnectRooms(graph, node, targetNode, direction, doorType);
                    createdExits++;
                }
            }

            // S'assurer qu'on a créé au moins 1 sortie (fallback)
            if (createdExits == 0 && availableDirections.Count > 0)
            {
                Debug.LogWarning($"[DungeonGraphGenerator] Failed to create exits for room at {node.GridPosition}, forcing one exit");
                // Forcer au moins une connexion
                var forcedDirection = availableDirections[0];
                Vector2Int offset = GetDirectionOffset(forcedDirection);
                Vector2Int nextPosition = node.GridPosition + offset;

                if (!grid.ContainsKey(nextPosition))
                {
                    var roomType = SelectRoomType(depth + 1, totalRooms);
                    var roomPool = _currentTheme.GetRoomsByType(roomType);

                    if (roomPool.Count > 0)
                    {
                        int roomIndex = _seed.NextInt(roomPool.Count);
                        var roomData = roomPool[roomIndex];

                        var targetNode = new RoomNode(nextPosition, roomData)
                        {
                            Depth = depth + 1
                        };

                        graph.AddNode(targetNode);
                        grid[nextPosition] = targetNode;
                        availableNodes.Add(targetNode);

                        DoorType doorType = SelectDoorType(node, depth, totalRooms);
                        ConnectRooms(graph, node, targetNode, forcedDirection, doorType);
                    }
                }
            }
        }

        private RoomNode CreateBossRoom(LevelTheme theme, DungeonGraph graph, int totalRooms)
        {
            var bossRooms = theme.GetRoomsByType(RoomType.Boss);
            if (bossRooms.Count == 0)
            {
                Debug.LogError("[DungeonGraphGenerator] No boss rooms available!");
                return null;
            }

            int index = _seed.NextInt(bossRooms.Count);
            var bossRoomData = bossRooms[index];

            // Trouver une position pour le boss (loin du start)
            var bossPosition = new Vector2Int(0, totalRooms); // Au nord du start
            var bossNode = new RoomNode(bossPosition, bossRoomData)
            {
                IsBossRoom = true,
                Depth = totalRooms
            };

            graph.AddNode(bossNode);
            return bossNode;
        }

        private void EnsureBossReachable(DungeonGraph graph, List<RoomNode> pathRooms, RoomNode bossNode)
        {
            // Connecter toutes les salles de profondeur maximale au boss
            if (pathRooms.Count == 0)
            {
                // Si pas de salles intermédiaires, connecter start au boss
                ConnectRooms(graph, graph.StartNode, bossNode, Direction.North, DoorType.Boss);
                return;
            }

            // Connecter les salles entre elles selon leur position
            ConnectPathRooms(graph, pathRooms);

            // Connecter les salles de profondeur maximale au boss
            int maxDepth = pathRooms.Max(r => r.Depth);
            var deepestRooms = pathRooms.Where(r => r.Depth == maxDepth).ToList();

            // Connecter au moins une salle profonde au boss
            foreach (var room in deepestRooms)
            {
                // Trouver une direction disponible
                var directions = new[] { Direction.North, Direction.East, Direction.South, Direction.West };
                foreach (var dir in directions)
                {
                    if (room.RoomData.HasDoorInDirection(dir) && !room.HasConnection(dir))
                    {
                        ConnectRooms(graph, room, bossNode, dir, DoorType.Boss);
                        break;
                    }
                }
            }

            // S'assurer que toutes les salles peuvent atteindre le boss
            // En créant des connexions supplémentaires si nécessaire
            EnsureAllRoomsConnected(graph, pathRooms);
        }

        private void ConnectPathRooms(DungeonGraph graph, List<RoomNode> pathRooms)
        {
            // Connecter les salles selon leur position dans le graphe
            for (int i = 0; i < pathRooms.Count; i++)
            {
                var current = pathRooms[i];
                var directions = new[] { Direction.North, Direction.East, Direction.South, Direction.West };

                // Chercher des salles adjacentes à connecter
                foreach (var other in pathRooms)
                {
                    if (other == current) continue;

                    var offset = other.GridPosition - current.GridPosition;
                    Direction? dir = GetDirectionFromOffset(offset);

                    if (dir.HasValue && !current.HasConnection(dir.Value))
                    {
                        if (current.RoomData.HasDoorInDirection(dir.Value) &&
                            other.RoomData.HasDoorInDirection(GetOppositeDirection(dir.Value)))
                        {
                            DoorType doorType = SelectDoorType(current, current.Depth, pathRooms.Count + 1);
                            ConnectRooms(graph, current, other, dir.Value, doorType);
                        }
                    }
                }

                // Connecter au start si proche
                if (graph.StartNode != null && !AreConnected(current, graph.StartNode, graph))
                {
                    var startOffset = graph.StartNode.GridPosition - current.GridPosition;
                    Direction? startDir = GetDirectionFromOffset(startOffset);
                    if (startDir.HasValue && current.RoomData.HasDoorInDirection(startDir.Value))
                    {
                        DoorType doorType = SelectDoorType(graph.StartNode, 0, pathRooms.Count + 1);
                        ConnectRooms(graph, graph.StartNode, current, GetOppositeDirection(startDir.Value), doorType);
                    }
                }
            }
        }

        private void EnsureAllRoomsConnected(DungeonGraph graph, List<RoomNode> pathRooms)
        {
            // Vérifier que toutes les salles sont connectées au graphe
            var allNodes = new List<RoomNode> { graph.StartNode };
            allNodes.AddRange(pathRooms);
            allNodes.Add(graph.BossNode);

            foreach (var node in allNodes)
            {
                if (node == graph.BossNode) continue; // Boss n'a pas besoin de sorties

                int connectionCount = node.GetConnectedDirections().Count();
                if (connectionCount == 0)
                {
                    // Trouver le node le plus proche et le connecter
                    RoomNode closest = null;
                    float minDist = float.MaxValue;

                    foreach (var other in allNodes)
                    {
                        if (other == node) continue;
                        float dist = Vector2Int.Distance(node.GridPosition, other.GridPosition);
                        if (dist < minDist)
                        {
                            minDist = dist;
                            closest = other;
                        }
                    }

                    if (closest != null)
                    {
                        var offset = closest.GridPosition - node.GridPosition;
                        Direction? dir = GetDirectionFromOffset(offset);
                        if (dir.HasValue && node.RoomData.HasDoorInDirection(dir.Value))
                        {
                            DoorType doorType = node.IsBossRoom ? DoorType.Boss : SelectDoorType(node, node.Depth, pathRooms.Count + 1);
                            ConnectRooms(graph, node, closest, dir.Value, doorType);
                        }
                    }
                }
            }
        }

        private Direction? GetDirectionFromOffset(Vector2Int offset)
        {
            if (offset == Vector2Int.up) return Direction.North;
            if (offset == Vector2Int.down) return Direction.South;
            if (offset == Vector2Int.right) return Direction.East;
            if (offset == Vector2Int.left) return Direction.West;
            return null;
        }

        private void ConnectRooms(DungeonGraph graph, RoomNode from, RoomNode to, Direction direction, DoorType doorType)
        {
            var edge = new RoomEdge(_nextEdgeId++, from, to, doorType, direction);
            from.SetConnection(to, direction);
            to.SetConnection(from, GetOppositeDirection(direction));
            graph.AddEdge(edge);
        }

        private bool AreConnected(RoomNode a, RoomNode b, DungeonGraph graph)
        {
            var directions = new[] { Direction.North, Direction.East, Direction.South, Direction.West };
            foreach (var dir in directions)
            {
                if (a.GetConnection(dir) == b)
                {
                    return true;
                }
            }
            return false;
        }

        private RoomType SelectRoomType(int depth, int totalRooms)
        {
            float progress = (float)depth / totalRooms;

            if (progress < 0.3f) // Première partie
            {
                float roll = _seed.NextFloat();
                if (roll < _settings.EliteRoomChance) return RoomType.Elite;
                return RoomType.Combat;
            }
            else if (progress < 0.7f) // Milieu
            {
                float roll = _seed.NextFloat();
                if (roll < _settings.ShopRoomChance) return RoomType.Shop;
                if (roll < _settings.ShopRoomChance + _settings.EliteRoomChance) return RoomType.Elite;
                if (roll < _settings.ShopRoomChance + _settings.EliteRoomChance + _settings.EventRoomChance) return RoomType.Event;
                return RoomType.Combat;
            }
            else // Fin
            {
                float roll = _seed.NextFloat();
                if (roll < _settings.EventRoomChance * 2f) return RoomType.Event;
                if (roll < _settings.EventRoomChance * 2f + _settings.EliteRoomChance) return RoomType.Elite;
                return RoomType.Combat;
            }
        }

        private DoorType SelectDoorType(RoomNode node, int depth, int totalRooms)
        {
            // Utiliser la distribution des portes
            var random = new System.Random(_seed.Seed + depth);
            return _settings.DoorDistribution.SelectRandomType(random);
        }

        private RoomData SelectRoomForType(LevelTheme theme, RoomType type, int depth)
        {
            var rooms = theme.GetRoomsByType(type);
            if (rooms.Count == 0)
            {
                // Fallback sur combat
                rooms = theme.GetRoomsByType(RoomType.Combat);
                if (rooms.Count == 0)
                {
                    return null;
                }
            }

            // Filtrer par depth
            var validRooms = rooms.Where(r => r.CanSpawnAtDepth(depth)).ToList();
            if (validRooms.Count == 0)
            {
                validRooms = rooms; // Fallback
            }

            // Sélection pondérée
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

            return validRooms[0];
        }

        private Vector2Int? FindFreePosition(Vector2Int from, Dictionary<Vector2Int, RoomNode> grid, LevelSeed seed)
        {
            var directions = new[] { Direction.North, Direction.East, Direction.South, Direction.West };
            var shuffled = directions.OrderBy(x => seed.NextInt(1000)).ToList();

            foreach (var dir in shuffled)
            {
                var offset = GetDirectionOffset(dir);
                var pos = from + offset;
                if (!grid.ContainsKey(pos))
                {
                    return pos;
                }
            }

            return null;
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

        /// <summary>
        /// Assigne des récompenses à toutes les edges du graphe.
        /// </summary>
        private void AssignRewardsToGraph(DungeonGraph graph, int totalRooms)
        {
            if (_rewardGenerator == null)
            {
                Debug.LogError("[DungeonGraphGenerator] RewardGenerator is null!");
                return;
            }

            // Pour chaque salle (sauf le boss), assigner des récompenses à ses sorties
            foreach (var node in graph.Nodes)
            {
                if (node.IsBossRoom) continue; // Le boss n'a pas de sorties

                // Récupérer toutes les edges sortantes de cette salle
                var outgoingEdges = node.GetOutgoingEdges().ToList();

                if (outgoingEdges.Count > 0)
                {
                    _rewardGenerator.AssignRewardsToRoomExits(node, outgoingEdges, node.Depth, totalRooms);
                }
            }

            // Valider que toutes les edges ont des récompenses
            int validEdges = 0;
            int totalEdges = graph.Edges.Count;

            foreach (var edge in graph.Edges)
            {
                if (RewardGenerator.ValidateEdgeReward(edge))
                {
                    validEdges++;
                }
                else
                {
                    Debug.LogWarning($"[DungeonGraphGenerator] Edge {edge.EdgeId} missing reward data");
                }
            }

            Debug.Log($"[DungeonGraphGenerator] Assigned rewards to {validEdges}/{totalEdges} edges");
        }

        private bool ValidateGraph(DungeonGraph graph)
        {
            if (graph.StartNode == null || graph.BossNode == null)
            {
                return false;
            }

            if (!graph.IsBossReachable(graph.StartNode))
            {
                return false;
            }

            // Vérifier que toutes les salles ont au moins une connexion
            foreach (var node in graph.Nodes)
            {
                if (node != graph.StartNode && node != graph.BossNode)
                {
                    if (node.GetConnectedDirections().Count() == 0)
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}

