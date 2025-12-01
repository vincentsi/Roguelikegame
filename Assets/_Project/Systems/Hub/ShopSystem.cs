using System;
using System.Collections.Generic;
using UnityEngine;
using ProjectRoguelike.Systems.Meta;
using ProjectRoguelike.Gameplay.Items;

namespace ProjectRoguelike.Systems.Hub
{
    /// <summary>
    /// Manages the shop system - purchasing unlocks with currency.
    /// </summary>
    public sealed class ShopSystem : MonoBehaviour
    {
        [Header("Shop Configuration")]
        [SerializeField] private List<ProgressionData> availableUnlocks = new List<ProgressionData>();

        [Header("References")]
        [SerializeField] private CurrencyManager currencyManager;

        private HashSet<ProgressionData> _unlockedItems = new HashSet<ProgressionData>();

        public event Action<ProgressionData> OnItemUnlocked;
        public event Action<ProgressionData> OnPurchaseAttempted;

        public IReadOnlyList<ProgressionData> AvailableUnlocks => availableUnlocks;
        public bool IsUnlocked(ProgressionData item) => _unlockedItems.Contains(item);

        private void Awake()
        {
            // Load unlocked items from save system (TODO: implement save/load)
            LoadUnlockedItems();
        }

        private void Start()
        {
            // Find CurrencyManager in Start (after HubManager has created it if needed)
            if (currencyManager == null)
            {
                currencyManager = FindObjectOfType<CurrencyManager>();
                if (currencyManager == null)
                {
                    Debug.LogWarning("[ShopSystem] CurrencyManager not found! Shop purchases will not work.");
                }
            }
        }

        /// <summary>
        /// Attempts to purchase an unlock.
        /// </summary>
        public bool TryPurchase(ProgressionData unlock)
        {
            if (unlock == null)
            {
                return false;
            }

            if (_unlockedItems.Contains(unlock))
            {
                Debug.LogWarning($"[ShopSystem] {unlock.UnlockName} is already unlocked!");
                return false;
            }

            // Check prerequisites
            if (!ArePrerequisitesMet(unlock))
            {
                Debug.LogWarning($"[ShopSystem] Prerequisites not met for {unlock.UnlockName}");
                return false;
            }

            // Check if player has enough currency
            if (currencyManager == null)
            {
                // Try to find it again (in case it was created after Start)
                currencyManager = FindObjectOfType<CurrencyManager>();
                if (currencyManager == null)
                {
                    Debug.LogError("[ShopSystem] CurrencyManager not found!");
                    return false;
                }
            }

            if (!currencyManager.TrySpendCurrency(unlock.Cost))
            {
                Debug.LogWarning($"[ShopSystem] Not enough currency to purchase {unlock.UnlockName}. Need {unlock.Cost}, have {currencyManager.CurrentCurrency}");
                OnPurchaseAttempted?.Invoke(unlock);
                return false;
            }

            // Unlock the item
            _unlockedItems.Add(unlock);
            OnItemUnlocked?.Invoke(unlock);
            Debug.Log($"[ShopSystem] Purchased {unlock.UnlockName} for {unlock.Cost} currency");

            // Save unlocked items (TODO: implement save system)
            SaveUnlockedItems();

            return true;
        }

        /// <summary>
        /// Checks if all prerequisites for an unlock are met.
        /// </summary>
        private bool ArePrerequisitesMet(ProgressionData unlock)
        {
            if (unlock.Prerequisites == null || unlock.Prerequisites.Length == 0)
            {
                return true;
            }

            foreach (var prerequisite in unlock.Prerequisites)
            {
                if (prerequisite == null)
                {
                    continue;
                }

                if (!_unlockedItems.Contains(prerequisite))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Gets the purchase status of an unlock (can purchase, already unlocked, etc.).
        /// </summary>
        public PurchaseStatus GetPurchaseStatus(ProgressionData unlock)
        {
            if (unlock == null)
            {
                return PurchaseStatus.Invalid;
            }

            if (_unlockedItems.Contains(unlock))
            {
                return PurchaseStatus.AlreadyUnlocked;
            }

            if (!ArePrerequisitesMet(unlock))
            {
                return PurchaseStatus.PrerequisitesNotMet;
            }

            if (currencyManager == null || currencyManager.CurrentCurrency < unlock.Cost)
            {
                return PurchaseStatus.InsufficientCurrency;
            }

            return PurchaseStatus.CanPurchase;
        }

        private void LoadUnlockedItems()
        {
            // TODO: Load from save system
            // For now, unlock items that are marked as unlocked by default
            foreach (var unlock in availableUnlocks)
            {
                if (unlock != null && unlock.IsUnlockedByDefault)
                {
                    _unlockedItems.Add(unlock);
                }
            }
        }

        private void SaveUnlockedItems()
        {
            // TODO: Save to persistent storage
            Debug.Log($"[ShopSystem] Saving {_unlockedItems.Count} unlocked items...");
        }

        public enum PurchaseStatus
        {
            CanPurchase,
            AlreadyUnlocked,
            InsufficientCurrency,
            PrerequisitesNotMet,
            Invalid
        }

        // Test methods (Context Menu - right-click in Inspector during Play mode)
        [ContextMenu("Test: Purchase First Unlock")]
        private void TestPurchaseFirst()
        {
            if (availableUnlocks.Count == 0)
            {
                Debug.LogWarning("[ShopSystem] No unlocks available to purchase!");
                return;
            }

            var firstUnlock = availableUnlocks[0];
            if (firstUnlock == null)
            {
                Debug.LogWarning("[ShopSystem] First unlock is null!");
                return;
            }

            bool success = TryPurchase(firstUnlock);
            if (success)
            {
                Debug.Log($"[ShopSystem] Successfully purchased {firstUnlock.UnlockName}!");
            }
            else
            {
                var status = GetPurchaseStatus(firstUnlock);
                Debug.LogWarning($"[ShopSystem] Failed to purchase {firstUnlock.UnlockName}. Status: {status}");
            }
        }

        [ContextMenu("Test: Show Shop Status")]
        private void TestShowStatus()
        {
            Debug.Log($"[ShopSystem] Shop Status:");
            Debug.Log($"  - Available Unlocks: {availableUnlocks.Count}");
            Debug.Log($"  - Unlocked Items: {_unlockedItems.Count}");
            
            if (currencyManager != null)
            {
                Debug.Log($"  - Current Currency: {currencyManager.CurrentCurrency}");
            }

            foreach (var unlock in availableUnlocks)
            {
                if (unlock == null) continue;
                var status = GetPurchaseStatus(unlock);
                Debug.Log($"  - {unlock.UnlockName}: {status} (Cost: {unlock.Cost})");
            }
        }
    }
}

