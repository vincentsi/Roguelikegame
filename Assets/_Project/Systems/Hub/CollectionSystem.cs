using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ProjectRoguelike.Systems.Meta;

namespace ProjectRoguelike.Systems.Hub
{
    /// <summary>
    /// Manages the collection system - displays all unlockable items (weapons, characters, etc.).
    /// </summary>
    public sealed class CollectionSystem : MonoBehaviour
    {
        [Header("Collection Configuration")]
        [SerializeField] private List<ProgressionData> allCollectionItems = new List<ProgressionData>();

        [Header("References")]
        [SerializeField] private ShopSystem shopSystem; // To check unlock status

        public event Action OnCollectionUpdated;

        public IReadOnlyList<ProgressionData> AllCollectionItems => allCollectionItems;
        
        /// <summary>
        /// Gets all collection items of a specific type.
        /// </summary>
        public List<ProgressionData> GetItemsByType(ProgressionData.UnlockType type)
        {
            return allCollectionItems.Where(item => item != null && item.Type == type).ToList();
        }

        /// <summary>
        /// Checks if an item is unlocked.
        /// </summary>
        public bool IsUnlocked(ProgressionData item)
        {
            if (item == null) return false;
            if (shopSystem == null)
            {
                shopSystem = FindObjectOfType<ShopSystem>();
            }
            return shopSystem != null && shopSystem.IsUnlocked(item);
        }

        /// <summary>
        /// Gets the unlock status of an item.
        /// </summary>
        public CollectionItemStatus GetItemStatus(ProgressionData item)
        {
            if (item == null) return CollectionItemStatus.Unknown;

            if (IsUnlocked(item))
            {
                return CollectionItemStatus.Unlocked;
            }

            if (shopSystem != null)
            {
                var purchaseStatus = shopSystem.GetPurchaseStatus(item);
                switch (purchaseStatus)
                {
                    case ShopSystem.PurchaseStatus.CanPurchase:
                        return CollectionItemStatus.Locked;
                    case ShopSystem.PurchaseStatus.PrerequisitesNotMet:
                        return CollectionItemStatus.LockedPrerequisites;
                    case ShopSystem.PurchaseStatus.InsufficientCurrency:
                        return CollectionItemStatus.LockedInsufficientCurrency;
                    case ShopSystem.PurchaseStatus.AlreadyUnlocked:
                        return CollectionItemStatus.Unlocked;
                    default:
                        return CollectionItemStatus.Locked;
                }
            }

            return CollectionItemStatus.Locked;
        }

        private void Start()
        {
            // Find ShopSystem if not assigned
            if (shopSystem == null)
            {
                shopSystem = FindObjectOfType<ShopSystem>();
            }

            // Subscribe to shop events to update collection
            if (shopSystem != null)
            {
                shopSystem.OnItemUnlocked += OnItemUnlocked;
            }
        }

        private void OnDestroy()
        {
            if (shopSystem != null)
            {
                shopSystem.OnItemUnlocked -= OnItemUnlocked;
            }
        }

        private void OnItemUnlocked(ProgressionData item)
        {
            OnCollectionUpdated?.Invoke();
        }

        public enum CollectionItemStatus
        {
            Unknown,
            Unlocked,
            Locked,
            LockedPrerequisites,
            LockedInsufficientCurrency
        }
    }
}

