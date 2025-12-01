using System;
using UnityEngine;

namespace ProjectRoguelike.Systems.Meta
{
    /// <summary>
    /// Manages currency (meta-progression currency earned during runs).
    /// </summary>
    public sealed class CurrencyManager : MonoBehaviour
    {
        [Header("Currency Settings")]
        [SerializeField] private int startingCurrency = 0;

        private int _currentCurrency;

        public event Action<int> OnCurrencyChanged;

        public int CurrentCurrency => _currentCurrency;

        private void Awake()
        {
            _currentCurrency = startingCurrency;
            Debug.Log($"[CurrencyManager] Initialized with {_currentCurrency} starting currency");
        }

        /// <summary>
        /// Adds currency (e.g., from killing enemies, completing rooms).
        /// </summary>
        public void AddCurrency(int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            _currentCurrency += amount;
            OnCurrencyChanged?.Invoke(_currentCurrency);
            Debug.Log($"[CurrencyManager] Added {amount} currency. Total: {_currentCurrency}");
        }

        /// <summary>
        /// Spends currency (e.g., in shop).
        /// </summary>
        public bool TrySpendCurrency(int amount)
        {
            if (amount <= 0 || _currentCurrency < amount)
            {
                return false;
            }

            _currentCurrency -= amount;
            OnCurrencyChanged?.Invoke(_currentCurrency);
            Debug.Log($"[CurrencyManager] Spent {amount} currency. Remaining: {_currentCurrency}");
            return true;
        }

        /// <summary>
        /// Sets currency directly (for save/load).
        /// </summary>
        public void SetCurrency(int amount)
        {
            _currentCurrency = Mathf.Max(0, amount);
            OnCurrencyChanged?.Invoke(_currentCurrency);
        }

        // Test methods (Context Menu - right-click in Inspector during Play mode)
        [ContextMenu("Test: Add 50 Currency")]
        private void TestAddCurrency()
        {
            AddCurrency(50);
        }

        [ContextMenu("Test: Spend 25 Currency")]
        private void TestSpendCurrency()
        {
            TrySpendCurrency(25);
        }

        [ContextMenu("Test: Show Current Currency")]
        private void TestShowCurrency()
        {
            Debug.Log($"[CurrencyManager] Current Currency: {_currentCurrency}");
        }
    }
}

