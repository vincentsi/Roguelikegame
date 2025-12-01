using System.Collections.Generic;
using UnityEngine;
using ProjectRoguelike.Procedural;

namespace ProjectRoguelike.Gameplay.Items
{
    /// <summary>
    /// Component that can spawn loot items when triggered (enemy death, container opened, etc.)
    /// </summary>
    public sealed class LootDrop : MonoBehaviour
    {
        [System.Serializable]
        public class LootEntry
        {
            public ItemData itemData;
            public float weight = 1f;
        }

        [Header("Loot Configuration")]
        [SerializeField] private List<LootEntry> lootTable = new List<LootEntry>();
        [SerializeField] private int minDrops = 0;
        [SerializeField] private int maxDrops = 1;
        [SerializeField] private float dropRadius = 1f;
        [SerializeField] private GameObject itemPickupPrefab; // Prefab with ItemPickup component

        [Header("Spawn Settings")]
        [SerializeField] private bool spawnOnStart = false;
        [SerializeField] private bool spawnOnDeath = true;

        private LevelSeed _levelSeed;

        private void Start()
        {
            if (spawnOnStart)
            {
                SpawnLoot();
            }
        }

        public void SetLevelSeed(LevelSeed seed)
        {
            _levelSeed = seed;
        }

        /// <summary>
        /// Spawns loot based on the loot table.
        /// </summary>
        public void SpawnLoot()
        {
            if (lootTable == null || lootTable.Count == 0)
            {
                return;
            }

            // Calculate number of drops
            int dropCount = Random.Range(minDrops, maxDrops + 1);
            if (_levelSeed != null)
            {
                dropCount = Mathf.Clamp(_levelSeed.NextInt(maxDrops - minDrops + 1) + minDrops, minDrops, maxDrops);
            }

            // Filter loot table by depth (if levelSeed provides depth info)
            var availableLoot = FilterLootByDepth(lootTable);

            if (availableLoot.Count == 0)
            {
                return;
            }

            // Spawn items
            for (int i = 0; i < dropCount; i++)
            {
                var item = SelectRandomLoot(availableLoot);
                if (item != null)
                {
                    SpawnItemPickup(item);
                }
            }
        }

        private List<LootEntry> FilterLootByDepth(List<LootEntry> loot)
        {
            // TODO: Filter by current depth when depth system is implemented
            // For now, return all loot
            return new List<LootEntry>(loot);
        }

        private ItemData SelectRandomLoot(List<LootEntry> loot)
        {
            if (loot.Count == 0)
            {
                return null;
            }

            // Weighted random selection
            float totalWeight = 0f;
            foreach (var entry in loot)
            {
                if (entry.itemData != null)
                {
                    totalWeight += entry.weight * entry.itemData.DropWeight;
                }
            }

            if (totalWeight <= 0f)
            {
                return loot[0].itemData;
            }

            float randomValue = Random.Range(0f, totalWeight);
            if (_levelSeed != null)
            {
                randomValue = _levelSeed.NextFloat() * totalWeight;
            }

            float currentWeight = 0f;
            foreach (var entry in loot)
            {
                if (entry.itemData == null)
                {
                    continue;
                }

                currentWeight += entry.weight * entry.itemData.DropWeight;
                if (randomValue <= currentWeight)
                {
                    return entry.itemData;
                }
            }

            return loot[loot.Count - 1].itemData;
        }

        private void SpawnItemPickup(ItemData itemData)
        {
            // Calculate spawn position (random offset within drop radius)
            Vector2 randomOffset = Random.insideUnitCircle * dropRadius;
            if (_levelSeed != null)
            {
                randomOffset = new Vector2(
                    (_levelSeed.NextFloat() * 2f - 1f) * dropRadius,
                    (_levelSeed.NextFloat() * 2f - 1f) * dropRadius
                );
            }

            Vector3 spawnPosition = transform.position + new Vector3(randomOffset.x, 0f, randomOffset.y);

            // Ensure spawn is on ground
            if (Physics.Raycast(spawnPosition + Vector3.up, Vector3.down, out var hit, 2f))
            {
                spawnPosition = hit.point + Vector3.up * 0.1f;
            }

            // Spawn pickup
            GameObject pickupObj;
            if (itemPickupPrefab != null)
            {
                pickupObj = Instantiate(itemPickupPrefab, spawnPosition, Quaternion.identity);
            }
            else
            {
                // Create default pickup if no prefab assigned
                pickupObj = new GameObject($"ItemPickup_{itemData.ItemName}");
                pickupObj.transform.position = spawnPosition;
                var pickup = pickupObj.AddComponent<ItemPickup>();
                pickup.SetItemData(itemData);
                
                // Add a simple sphere collider
                var collider = pickupObj.AddComponent<SphereCollider>();
                collider.isTrigger = true;
                collider.radius = 0.5f;
            }

            // Set item data if component exists
            var itemPickup = pickupObj.GetComponent<ItemPickup>();
            if (itemPickup != null)
            {
                itemPickup.SetItemData(itemData);
            }
        }

        /// <summary>
        /// Called when the entity dies (if spawnOnDeath is true).
        /// </summary>
        public void OnDeath()
        {
            if (spawnOnDeath)
            {
                SpawnLoot();
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, dropRadius);
        }
    }
}

