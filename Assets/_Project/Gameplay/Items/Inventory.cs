using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectRoguelike.Gameplay.Items
{
    /// <summary>
    /// Basic inventory system for managing collected items.
    /// </summary>
    public sealed class Inventory : MonoBehaviour
    {
        [Header("Inventory Settings")]
        [SerializeField] private int maxSlots = 10;
        [SerializeField] private bool allowStacking = true;

        private List<ItemData> _items = new List<ItemData>();
        private Dictionary<ItemData, int> _itemCounts = new Dictionary<ItemData, int>();

        public event Action<ItemData> OnItemAdded;
        public event Action<ItemData> OnItemRemoved;
        public event Action OnInventoryChanged;

        public int ItemCount => _items.Count;
        public int MaxSlots => maxSlots;
        public bool IsFull => _items.Count >= maxSlots;

        /// <summary>
        /// Adds an item to the inventory.
        /// </summary>
        public bool AddItem(ItemData item)
        {
            if (item == null)
            {
                return false;
            }

            // Check if item can stack
            if (allowStacking && _itemCounts.ContainsKey(item))
            {
                _itemCounts[item]++;
                OnItemAdded?.Invoke(item);
                OnInventoryChanged?.Invoke();
                return true;
            }

            // Check if inventory is full
            if (IsFull)
            {
                Debug.LogWarning($"[Inventory] Inventory is full! Cannot add {item.ItemName}");
                return false;
            }

            _items.Add(item);
            _itemCounts[item] = 1;
            OnItemAdded?.Invoke(item);
            OnInventoryChanged?.Invoke();
            return true;
        }

        /// <summary>
        /// Removes an item from the inventory.
        /// </summary>
        public bool RemoveItem(ItemData item)
        {
            if (item == null || !_items.Contains(item))
            {
                return false;
            }

            if (_itemCounts.ContainsKey(item))
            {
                _itemCounts[item]--;
                if (_itemCounts[item] <= 0)
                {
                    _itemCounts.Remove(item);
                    _items.Remove(item);
                }
            }
            else
            {
                _items.Remove(item);
            }

            OnItemRemoved?.Invoke(item);
            OnInventoryChanged?.Invoke();
            return true;
        }

        /// <summary>
        /// Checks if the inventory contains an item.
        /// </summary>
        public bool HasItem(ItemData item)
        {
            return _items.Contains(item);
        }

        /// <summary>
        /// Gets the count of a specific item.
        /// </summary>
        public int GetItemCount(ItemData item)
        {
            if (item == null || !_itemCounts.ContainsKey(item))
            {
                return 0;
            }
            return _itemCounts[item];
        }

        /// <summary>
        /// Gets all items in the inventory.
        /// </summary>
        public List<ItemData> GetAllItems()
        {
            return new List<ItemData>(_items);
        }

        /// <summary>
        /// Clears the inventory.
        /// </summary>
        public void Clear()
        {
            _items.Clear();
            _itemCounts.Clear();
            OnInventoryChanged?.Invoke();
        }

        // Test methods (Context Menu - right-click in Inspector during Play mode)
        [ContextMenu("Test: Show Inventory Status")]
        private void TestShowInventory()
        {
            Debug.Log($"[Inventory] Status:");
            Debug.Log($"  - Max Slots: {maxSlots}");
            Debug.Log($"  - Current Items: {ItemCount}");
            Debug.Log($"  - Is Full: {IsFull}");
            Debug.Log($"  - Allow Stacking: {allowStacking}");
            
            if (_items.Count > 0)
            {
                Debug.Log($"  - Items:");
                foreach (var item in _items)
                {
                    int count = GetItemCount(item);
                    Debug.Log($"    - {item.ItemName} x{count}");
                }
            }
            else
            {
                Debug.Log("  - Inventory is empty");
            }
        }
    }
}

