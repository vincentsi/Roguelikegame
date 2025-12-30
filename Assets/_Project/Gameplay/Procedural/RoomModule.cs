using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectRoguelike.Procedural
{
    /// <summary>
    /// Component attached to room prefabs. Manages connections, spawn points, and room metadata.
    /// </summary>
    public sealed class RoomModule : MonoBehaviour
    {
        [Header("Connections")]
        [SerializeField] private Transform northDoor;
        [SerializeField] private Transform southDoor;
        [SerializeField] private Transform eastDoor;
        [SerializeField] private Transform westDoor;

        [Header("Spawn Points")]
        [SerializeField] private List<Transform> enemySpawnPoints = new();
        [SerializeField] private List<Transform> lootSpawnPoints = new();
        [SerializeField] private List<Transform> propSpawnPoints = new();

        [Header("Room Info")]
        [SerializeField] private RoomType roomType = RoomType.Combat;
        [SerializeField] private Vector2Int gridSize = new Vector2Int(1, 1);

        // Combat tracking
        private readonly HashSet<GameObject> _activeEnemies = new();
        private bool _combatCompleted = false;

        public event Action OnAllEnemiesDefeated;

        public RoomType RoomType => roomType;
        public Vector2Int GridSize => gridSize;
        public int ActiveEnemyCount => _activeEnemies.Count;
        public bool IsCombatCompleted => _combatCompleted;

        public Transform GetDoor(Direction direction)
        {
            return direction switch
            {
                Direction.North => northDoor,
                Direction.South => southDoor,
                Direction.East => eastDoor,
                Direction.West => westDoor,
                _ => null
            };
        }

        public IReadOnlyList<Transform> EnemySpawnPoints => enemySpawnPoints;
        public IReadOnlyList<Transform> LootSpawnPoints => lootSpawnPoints;
        public IReadOnlyList<Transform> PropSpawnPoints => propSpawnPoints;

        public void AddEnemySpawnPoint(Transform point)
        {
            if (point != null && !enemySpawnPoints.Contains(point))
            {
                enemySpawnPoints.Add(point);
            }
        }

        public void AddLootSpawnPoint(Transform point)
        {
            if (point != null && !lootSpawnPoints.Contains(point))
            {
                lootSpawnPoints.Add(point);
            }
        }

        public void AddPropSpawnPoint(Transform point)
        {
            if (point != null && !propSpawnPoints.Contains(point))
            {
                propSpawnPoints.Add(point);
            }
        }

        /// <summary>
        /// Enregistre un ennemi comme actif dans cette salle.
        /// À appeler lors du spawn de l'ennemi.
        /// </summary>
        public void RegisterEnemy(GameObject enemy)
        {
            if (enemy != null && _activeEnemies.Add(enemy))
            {
                Debug.Log($"[RoomModule] Enemy registered: {enemy.name} (Total: {_activeEnemies.Count})");
            }
        }

        /// <summary>
        /// Désenregistre un ennemi (appelé quand il meurt).
        /// Si tous les ennemis sont vaincus, déclenche l'événement.
        /// </summary>
        public void UnregisterEnemy(GameObject enemy)
        {
            if (enemy != null && _activeEnemies.Remove(enemy))
            {
                Debug.Log($"[RoomModule] Enemy defeated: {enemy.name} (Remaining: {_activeEnemies.Count})");

                if (_activeEnemies.Count == 0 && !_combatCompleted)
                {
                    _combatCompleted = true;
                    Debug.Log("[RoomModule] All enemies defeated!");
                    OnAllEnemiesDefeated?.Invoke();
                }
            }
        }

        /// <summary>
        /// Réinitialise l'état du combat (pour réutilisation de la salle).
        /// </summary>
        public void ResetCombatState()
        {
            _activeEnemies.Clear();
            _combatCompleted = false;
        }

        private void OnDrawGizmosSelected()
        {
            // Draw door positions
            Gizmos.color = Color.cyan;
            if (northDoor != null) Gizmos.DrawWireSphere(northDoor.position, 0.5f);
            if (southDoor != null) Gizmos.DrawWireSphere(southDoor.position, 0.5f);
            if (eastDoor != null) Gizmos.DrawWireSphere(eastDoor.position, 0.5f);
            if (westDoor != null) Gizmos.DrawWireSphere(westDoor.position, 0.5f);

            // Draw spawn points
            Gizmos.color = Color.red;
            foreach (var point in enemySpawnPoints)
            {
                if (point != null) Gizmos.DrawWireCube(point.position, Vector3.one * 0.5f);
            }

            Gizmos.color = Color.yellow;
            foreach (var point in lootSpawnPoints)
            {
                if (point != null) Gizmos.DrawWireCube(point.position, Vector3.one * 0.5f);
            }

            Gizmos.color = Color.green;
            foreach (var point in propSpawnPoints)
            {
                if (point != null) Gizmos.DrawWireCube(point.position, Vector3.one * 0.5f);
            }
        }
    }
}

