using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ProjectRoguelike.Gameplay.Procedural;

namespace ProjectRoguelike.Procedural
{
    /// <summary>
    /// Gère les portes et leurs interactions dans le donjon.
    /// </summary>
    public sealed class DoorManager : MonoBehaviour
    {
        private readonly Dictionary<RoomEdge, DoorComponent> _doorComponents = new();
        private RoomManager _roomManager;
        private DungeonGraph _currentGraph;

        public void Initialize(DungeonGraph graph, RoomManager roomManager)
        {
            _currentGraph = graph ?? throw new System.ArgumentNullException(nameof(graph));
            _roomManager = roomManager ?? throw new System.ArgumentNullException(nameof(roomManager));
        }

        public void SpawnDoorsForRoom(RoomNode room, GameObject roomInstance)
        {
            if (room == null || roomInstance == null)
            {
                return;
            }

            var roomModule = roomInstance.GetComponent<RoomModule>();
            if (roomModule == null)
            {
                Debug.LogWarning($"[DoorManager] Room {roomInstance.name} has no RoomModule!");
                return;
            }

            // Pour chaque edge sortant de cette salle
            var outgoingEdges = _currentGraph.GetAvailableExits(room);
            foreach (var edge in outgoingEdges)
            {
                // Trouver la porte correspondante dans le prefab
                var doorTransform = roomModule.GetDoor(edge.Direction);
                if (doorTransform == null)
                {
                    continue;
                }

                // Créer ou récupérer DoorComponent
                var doorComponent = doorTransform.GetComponent<DoorComponent>();
                if (doorComponent == null)
                {
                    doorComponent = doorTransform.gameObject.AddComponent<DoorComponent>();
                }

                // Initialiser la porte
                doorComponent.Initialize(edge);
                doorComponent.OnDoorInteracted += OnDoorInteracted;

                _doorComponents[edge] = doorComponent;
            }
        }

        public void UpdateDoorStates(RoomNode room)
        {
            if (room == null)
            {
                return;
            }

            // Déverrouiller les portes disponibles
            var availableExits = _currentGraph.GetAvailableExits(room);
            foreach (var edge in availableExits)
            {
                if (_doorComponents.TryGetValue(edge, out var doorComponent))
                {
                    doorComponent.SetLocked(false);
                    doorComponent.SetInteractable(true);
                }
            }
        }

        public void UnlockAvailableDoors(RoomNode room)
        {
            UpdateDoorStates(room);
        }

        public void LockAllDoors()
        {
            foreach (var doorComponent in _doorComponents.Values)
            {
                if (doorComponent != null)
                {
                    doorComponent.SetLocked(true);
                    doorComponent.SetInteractable(false);
                }
            }
        }

        private void OnDoorInteracted(DoorComponent door)
        {
            if (door == null || door.Edge == null)
            {
                return;
            }

            var targetRoom = door.Edge.ToNode;
            if (targetRoom == null)
            {
                Debug.LogError("[DoorManager] Door edge has no target room!");
                return;
            }

            // Demander au RoomManager de faire la transition
            _roomManager.TransitionToRoom(targetRoom, door.Edge);
        }

        public DoorComponent GetDoorForEdge(RoomEdge edge)
        {
            return _doorComponents.TryGetValue(edge, out var door) ? door : null;
        }

        public void Clear()
        {
            foreach (var doorComponent in _doorComponents.Values)
            {
                if (doorComponent != null)
                {
                    doorComponent.OnDoorInteracted -= OnDoorInteracted;
                }
            }

            _doorComponents.Clear();
        }
    }
}

