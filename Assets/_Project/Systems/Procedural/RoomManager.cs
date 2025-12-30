using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ProjectRoguelike.Core;

namespace ProjectRoguelike.Procedural
{
    /// <summary>
    /// Gère la salle actuellement active et les transitions entre salles.
    /// </summary>
    public sealed class RoomManager : MonoBehaviour
    {
        private DungeonGraph _currentGraph;
        private RoomNode _currentRoom;
        private RoomLoader _roomLoader;
        private DoorManager _doorManager;
        private readonly List<RoomNode> _visitedRooms = new();

        public event Action<RoomNode> OnRoomEntered;
        public event Action<RoomNode> OnRoomCompleted;
        public event Action<RoomNode, RoomNode> OnRoomTransition;
        public event Action OnBossDefeated;

        public RoomNode GetCurrentRoom() => _currentRoom;
        public bool IsBossRoom() => _currentRoom != null && _currentRoom.IsBossRoom;

        public void Initialize(DungeonGraph graph, RoomLoader roomLoader, DoorManager doorManager)
        {
            _currentGraph = graph ?? throw new System.ArgumentNullException(nameof(graph));
            _roomLoader = roomLoader ?? throw new System.ArgumentNullException(nameof(roomLoader));
            _doorManager = doorManager ?? throw new System.ArgumentNullException(nameof(doorManager));

            if (_currentGraph.StartNode == null)
            {
                Debug.LogError("[RoomManager] Graph has no start node!");
                return;
            }

            // Entrer dans la salle de départ
            EnterRoom(_currentGraph.StartNode);
        }

        public async void EnterRoom(RoomNode room)
        {
            if (room == null)
            {
                Debug.LogError("[RoomManager] Cannot enter null room!");
                return;
            }

            // Charger la salle si nécessaire
            if (!_roomLoader.IsRoomLoaded(room))
            {
                await _roomLoader.LoadRoomAsync(room);
            }

            // Activer la salle
            var roomInstance = _roomLoader.GetLoadedRoom(room);
            if (roomInstance != null)
            {
                roomInstance.SetActive(true);

                // S'abonner à l'événement de victoire du combat
                var roomModule = roomInstance.GetComponent<RoomModule>();
                if (roomModule != null)
                {
                    roomModule.OnAllEnemiesDefeated -= OnRoomCombatCompleted;
                    roomModule.OnAllEnemiesDefeated += OnRoomCombatCompleted;
                }

                // Créer les portes pour cette salle
                _doorManager.SpawnDoorsForRoom(room, roomInstance);
            }

            // Mettre à jour l'état
            _currentRoom = room;
            room.MarkAsVisited();
            if (!_visitedRooms.Contains(room))
            {
                _visitedRooms.Add(room);
            }

            // Précharger les salles adjacentes
            _roomLoader.PreloadAdjacentRooms(room);

            // Déverrouiller les portes de sortie
            _doorManager.UpdateDoorStates(room);

            OnRoomEntered?.Invoke(room);
        }

        public void CompleteCurrentRoom()
        {
            if (_currentRoom == null)
            {
                return;
            }

            _currentRoom.MarkAsAvailable();
            _doorManager.UnlockAvailableDoors(_currentRoom);

            OnRoomCompleted?.Invoke(_currentRoom);
        }

        public async void TransitionToRoom(RoomNode targetRoom, RoomEdge edge)
        {
            if (targetRoom == null || edge == null)
            {
                Debug.LogError("[RoomManager] Invalid transition parameters!");
                return;
            }

            if (_currentRoom == null)
            {
                Debug.LogError("[RoomManager] No current room to transition from!");
                return;
            }

            // Verrouiller toutes les portes
            _doorManager.LockAllDoors();

            // Décharger les salles distantes
            _roomLoader.UnloadDistantRooms(targetRoom, 2);

            // Charger la salle cible
            if (!_roomLoader.IsRoomLoaded(targetRoom))
            {
                await _roomLoader.LoadRoomAsync(targetRoom);
            }

            // Précharger les salles adjacentes
            _roomLoader.PreloadAdjacentRooms(targetRoom);

            // Téléporter les joueurs (à implémenter selon le système de joueur)
            TeleportPlayersToRoom(targetRoom);

            // Ancienne salle devient inactive
            if (_currentRoom != null)
            {
                var oldRoomInstance = _roomLoader.GetLoadedRoom(_currentRoom);
                if (oldRoomInstance != null)
                {
                    oldRoomInstance.SetActive(false);
                }
            }

            // Entrer dans la nouvelle salle
            var previousRoom = _currentRoom;
            EnterRoom(targetRoom);

            OnRoomTransition?.Invoke(previousRoom, targetRoom);
        }

        public bool CanExitCurrentRoom()
        {
            return _currentRoom != null && _currentRoom.CanExit();
        }

        public List<DoorData> GetAvailableExits()
        {
            if (_currentRoom == null)
            {
                return new List<DoorData>();
            }

            return _currentRoom.GetAvailableExits();
        }

        public void NotifyBossDefeated()
        {
            if (_currentRoom != null && _currentRoom.IsBossRoom)
            {
                Debug.Log("[RoomManager] Boss defeated!");
                OnBossDefeated?.Invoke();
            }
        }

        private void TeleportPlayersToRoom(RoomNode room)
        {
            // Trouver le point d'entrée de la salle
            var roomInstance = _roomLoader.GetLoadedRoom(room);
            if (roomInstance == null)
            {
                return;
            }

            var roomModule = roomInstance.GetComponent<RoomModule>();
            if (roomModule == null)
            {
                return;
            }

            // Utiliser PlayerManager au lieu de FindGameObjectsWithTag
            var playerManager = ServiceRegistry.Get<PlayerManager>();
            if (playerManager == null || playerManager.PlayerCount == 0)
            {
                Debug.LogWarning("[RoomManager] No PlayerManager or no players registered!");
                return;
            }

            // Téléporter au point d'entrée (première porte disponible)
            var directions = new[] { Direction.North, Direction.East, Direction.South, Direction.West };
            Transform spawnPoint = null;

            foreach (var dir in directions)
            {
                var door = roomModule.GetDoor(dir);
                if (door != null)
                {
                    spawnPoint = door;
                    break;
                }
            }

            if (spawnPoint == null)
            {
                spawnPoint = roomInstance.transform; // Fallback
            }

            // Téléporter tous les joueurs
            playerManager.TeleportAllPlayersTo(spawnPoint);
        }

        private void OnRoomCombatCompleted()
        {
            Debug.Log("[RoomManager] Room combat completed, unlocking doors...");
            CompleteCurrentRoom();
        }

        private void OnDestroy()
        {
            // Nettoyer les événements
            if (_currentRoom != null)
            {
                var roomInstance = _roomLoader?.GetLoadedRoom(_currentRoom);
                if (roomInstance != null)
                {
                    var roomModule = roomInstance.GetComponent<RoomModule>();
                    if (roomModule != null)
                    {
                        roomModule.OnAllEnemiesDefeated -= OnRoomCombatCompleted;
                    }
                }
            }
        }
    }
}

