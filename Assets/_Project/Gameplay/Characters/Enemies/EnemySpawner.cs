using UnityEngine;

namespace ProjectRoguelike.Gameplay.Enemies
{
    /// <summary>
    /// Utility script to spawn enemies for testing.
    /// </summary>
    public sealed class EnemySpawner : MonoBehaviour
    {
        [Header("Spawn Settings")]
        [SerializeField] private GameObject enemyPrefab;
        [SerializeField] private int spawnCount = 3;
        [SerializeField] private float spawnRadius = 5f;
        [SerializeField] private bool spawnOnStart = true;

        private void Start()
        {
            if (spawnOnStart)
            {
                SpawnEnemies();
            }
        }

        [ContextMenu("Spawn Enemies")]
        public void SpawnEnemies()
        {
            if (enemyPrefab == null)
            {
                Debug.LogError("[EnemySpawner] Enemy prefab is not assigned!");
                return;
            }

            for (int i = 0; i < spawnCount; i++)
            {
                var angle = (360f / spawnCount) * i;
                var direction = Quaternion.Euler(0, angle, 0) * Vector3.forward;
                var spawnPosition = transform.position + direction * spawnRadius;

                // Check if position is valid on NavMesh
                if (UnityEngine.AI.NavMesh.SamplePosition(spawnPosition, out var hit, spawnRadius, UnityEngine.AI.NavMesh.AllAreas))
                {
                    spawnPosition = hit.position;
                }
                else
                {
                    // Fallback: spawn at transform position with offset
                    spawnPosition = transform.position + direction * 2f;
                }

                var enemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
                Debug.Log($"[EnemySpawner] Spawned enemy {i + 1} at {spawnPosition}");
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, spawnRadius);
            Gizmos.DrawWireSphere(transform.position, 0.5f);
        }
    }
}

