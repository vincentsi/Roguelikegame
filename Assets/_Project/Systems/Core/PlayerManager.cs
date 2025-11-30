using System.Collections.Generic;
using UnityEngine;

namespace ProjectRoguelike.Core
{
    /// <summary>
    /// Manages player references for AI and other systems.
    /// Replaces GameObject.FindGameObjectWithTag for better performance and multiplayer support.
    /// </summary>
    public sealed class PlayerManager
    {
        private readonly List<Transform> _players = new();

        public IReadOnlyList<Transform> Players => _players;
        public Transform PrimaryPlayer => _players.Count > 0 ? _players[0] : null;
        public int PlayerCount => _players.Count;

        public void RegisterPlayer(Transform player)
        {
            if (player == null)
            {
                return;
            }

            if (!_players.Contains(player))
            {
                _players.Add(player);
                Debug.Log($"[PlayerManager] Registered player: {player.name} (Total: {_players.Count})");
            }
        }

        public void UnregisterPlayer(Transform player)
        {
            if (player == null)
            {
                return;
            }

            if (_players.Remove(player))
            {
                Debug.Log($"[PlayerManager] Unregistered player: {player.name} (Total: {_players.Count})");
            }
        }

        public Transform GetClosestPlayer(Vector3 position)
        {
            if (_players.Count == 0)
            {
                return null;
            }

            Transform closest = null;
            float closestDistance = float.MaxValue;

            foreach (var player in _players)
            {
                if (player == null)
                {
                    continue;
                }

                float distance = Vector3.SqrMagnitude(player.position - position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closest = player;
                }
            }

            return closest;
        }

        public void Clear()
        {
            _players.Clear();
        }
    }
}

