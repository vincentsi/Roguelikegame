using System.Collections.Generic;
using System.Linq;
using ProjectRoguelike.Levels;

namespace ProjectRoguelike.Procedural
{
    /// <summary>
    /// Représente le graphe complet du donjon avec nodes et edges.
    /// </summary>
    public sealed class DungeonGraph
    {
        private readonly List<RoomNode> _nodes = new();
        private readonly List<RoomEdge> _edges = new();
        private readonly Dictionary<RoomNode, List<RoomEdge>> _outgoingEdges = new();
        private readonly Dictionary<RoomNode, List<RoomEdge>> _incomingEdges = new();

        private static int _nextGraphId = 0;

        public int GraphId { get; }
        public LevelTheme Theme { get; set; }
        public LevelSeed Seed { get; set; }
        public RoomNode StartNode { get; set; }
        public RoomNode BossNode { get; set; }

        public IReadOnlyList<RoomNode> Nodes => _nodes;
        public IReadOnlyList<RoomEdge> Edges => _edges;

        public DungeonGraph()
        {
            GraphId = _nextGraphId++;
        }

        public static void ResetGraphIdCounter()
        {
            _nextGraphId = 0;
        }

        public void AddNode(RoomNode node)
        {
            if (node != null && !_nodes.Contains(node))
            {
                _nodes.Add(node);
                _outgoingEdges[node] = new List<RoomEdge>();
                _incomingEdges[node] = new List<RoomEdge>();
            }
        }

        public void AddEdge(RoomEdge edge)
        {
            if (edge == null) return;

            if (!_edges.Contains(edge))
            {
                _edges.Add(edge);
            }

            // Ajouter aux dictionnaires
            if (_outgoingEdges.TryGetValue(edge.FromNode, out var outgoing))
            {
                if (!outgoing.Contains(edge))
                {
                    outgoing.Add(edge);
                }
            }

            if (_incomingEdges.TryGetValue(edge.ToNode, out var incoming))
            {
                if (!incoming.Contains(edge))
                {
                    incoming.Add(edge);
                }
            }

            // Ajouter aux nodes
            edge.FromNode.AddOutgoingEdge(edge);
            edge.ToNode.AddIncomingEdge(edge);
        }

        public RoomNode GetNode(int nodeId)
        {
            return _nodes.FirstOrDefault(n => n.NodeId == nodeId);
        }

        public List<RoomEdge> GetAvailableExits(RoomNode node)
        {
            if (node == null || !_outgoingEdges.TryGetValue(node, out var edges))
            {
                return new List<RoomEdge>();
            }

            return edges.Where(e => e.CanTraverse()).ToList();
        }

        public RoomNode GetNextNode(RoomNode current, RoomEdge edge)
        {
            if (edge == null || edge.FromNode != current)
            {
                return null;
            }

            return edge.ToNode;
        }

        public bool IsBossReachable(RoomNode from)
        {
            if (BossNode == null || from == null)
            {
                return false;
            }

            // BFS pour vérifier si le boss est accessible
            var visited = new HashSet<RoomNode>();
            var queue = new Queue<RoomNode>();
            queue.Enqueue(from);
            visited.Add(from);

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();

                if (current == BossNode)
                {
                    return true;
                }

                if (_outgoingEdges.TryGetValue(current, out var edges))
                {
                    foreach (var edge in edges)
                    {
                        if (edge.CanTraverse() && !visited.Contains(edge.ToNode))
                        {
                            visited.Add(edge.ToNode);
                            queue.Enqueue(edge.ToNode);
                        }
                    }
                }
            }

            return false;
        }

        public int GetDepth(RoomNode node)
        {
            if (node == null || StartNode == null)
            {
                return 0;
            }

            // BFS pour calculer la profondeur
            var visited = new HashSet<RoomNode>();
            var queue = new Queue<(RoomNode node, int depth)>();
            queue.Enqueue((StartNode, 0));
            visited.Add(StartNode);

            while (queue.Count > 0)
            {
                var (current, depth) = queue.Dequeue();

                if (current == node)
                {
                    return depth;
                }

                if (_outgoingEdges.TryGetValue(current, out var edges))
                {
                    foreach (var edge in edges)
                    {
                        if (!visited.Contains(edge.ToNode))
                        {
                            visited.Add(edge.ToNode);
                            queue.Enqueue((edge.ToNode, depth + 1));
                        }
                    }
                }
            }

            return -1; // Node non accessible
        }

        public List<RoomNode> GetPathToBoss(RoomNode from)
        {
            if (BossNode == null || from == null)
            {
                return new List<RoomNode>();
            }

            // BFS pour trouver le chemin le plus court
            var visited = new HashSet<RoomNode>();
            var queue = new Queue<(RoomNode node, List<RoomNode> path)>();
            queue.Enqueue((from, new List<RoomNode> { from }));
            visited.Add(from);

            while (queue.Count > 0)
            {
                var (current, path) = queue.Dequeue();

                if (current == BossNode)
                {
                    return path;
                }

                if (_outgoingEdges.TryGetValue(current, out var edges))
                {
                    foreach (var edge in edges)
                    {
                        if (edge.CanTraverse() && !visited.Contains(edge.ToNode))
                        {
                            visited.Add(edge.ToNode);
                            var newPath = new List<RoomNode>(path) { edge.ToNode };
                            queue.Enqueue((edge.ToNode, newPath));
                        }
                    }
                }
            }

            return new List<RoomNode>(); // Pas de chemin trouvé
        }

        public void Clear()
        {
            _nodes.Clear();
            _edges.Clear();
            _outgoingEdges.Clear();
            _incomingEdges.Clear();
            StartNode = null;
            BossNode = null;
        }
    }
}

