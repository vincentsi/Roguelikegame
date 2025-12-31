using UnityEngine;

namespace ProjectRoguelike.Procedural
{
    /// <summary>
    /// Types de portes avec leurs récompenses associées.
    /// </summary>
    public enum DoorType
    {
        Currency,       // Monnaie (meta-progression)
        Upgrade,        // Amélioration temporaire
        Weapon,         // Arme
        Consumable,     // Item consommable
        Elite,          // Salle Elite (combat difficile)
        Shop,           // Salle Shop
        Event,          // Événement aléatoire
        Random,         // Récompense aléatoire
        Boss            // Porte vers le boss
    }

    /// <summary>
    /// État d'une salle dans le graphe.
    /// </summary>
    public enum RoomState
    {
        Unvisited,      // Non visitée
        Available,      // Disponible (portes déverrouillées)
        Visited,        // Visitée
        Locked          // Verrouillée
    }

    /// <summary>
    /// Données d'une porte pour l'affichage et l'interaction.
    /// </summary>
    public struct DoorData
    {
        public RoomEdge Edge;
        public DoorType Type;
        public GeneratedReward Reward;
        public bool IsAvailable;
        public Vector3 WorldPosition;
        public Quaternion WorldRotation;

        public DoorData(RoomEdge edge, DoorType type, GeneratedReward reward, bool isAvailable, Vector3 position, Quaternion rotation)
        {
            Edge = edge;
            Type = type;
            Reward = reward;
            IsAvailable = isAvailable;
            WorldPosition = position;
            WorldRotation = rotation;
        }
    }
}

