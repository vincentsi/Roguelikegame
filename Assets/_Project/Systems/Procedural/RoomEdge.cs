namespace ProjectRoguelike.Procedural
{
    /// <summary>
    /// Représente une connexion entre deux salles avec type de porte.
    /// </summary>
    public sealed class RoomEdge
    {
        public int EdgeId { get; }
        public RoomNode FromNode { get; }
        public RoomNode ToNode { get; }
        public DoorType DoorType { get; }
        public Direction Direction { get; }
        public bool IsLocked { get; private set; }
        public RoomRewardData RewardPreview { get; set; }
        public float TraverseCost { get; set; } = 1f;

        public RoomEdge(int edgeId, RoomNode fromNode, RoomNode toNode, DoorType doorType, Direction direction)
        {
            EdgeId = edgeId;
            FromNode = fromNode ?? throw new System.ArgumentNullException(nameof(fromNode));
            ToNode = toNode ?? throw new System.ArgumentNullException(nameof(toNode));
            DoorType = doorType;
            Direction = direction;
            IsLocked = true; // Par défaut verrouillée
        }

        public bool CanTraverse()
        {
            return !IsLocked && ToNode != null;
        }

        public void Unlock()
        {
            IsLocked = false;
        }

        public void Lock()
        {
            IsLocked = true;
        }
    }
}

