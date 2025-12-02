using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ProjectRoguelike.Systems.Hub;
using ProjectRoguelike.Systems.Meta;

namespace ProjectRoguelike.UI.Hub
{
    /// <summary>
    /// UI pour le Shop - affiche les unlocks disponibles et permet l'achat.
    /// </summary>
    public sealed class ShopUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private ShopSystem shopSystem;
        [SerializeField] private CurrencyManager currencyManager;
        [SerializeField] private Button closeButton;

        [Header("UI Elements")]
        [SerializeField] private TextMeshProUGUI currencyText;
        [SerializeField] private Transform unlockListParent; // Parent pour les items de la liste
        [SerializeField] private GameObject unlockItemPrefab; // Prefab pour un item d'unlock

        private List<GameObject> _unlockItemInstances = new List<GameObject>();

        private void Awake()
        {
            if (shopSystem == null)
            {
                shopSystem = FindObjectOfType<ShopSystem>();
            }

            if (currencyManager == null)
            {
                currencyManager = FindObjectOfType<CurrencyManager>();
            }

            if (closeButton != null)
            {
                closeButton.onClick.AddListener(CloseShop);
            }
        }

        private void OnEnable()
        {
            RefreshUI();
            
            if (currencyManager != null)
            {
                currencyManager.OnCurrencyChanged += UpdateCurrencyDisplay;
            }
        }

        private void OnDisable()
        {
            if (currencyManager != null)
            {
                currencyManager.OnCurrencyChanged -= UpdateCurrencyDisplay;
            }
        }

        private void RefreshUI()
        {
            UpdateCurrencyDisplay(currencyManager != null ? currencyManager.CurrentCurrency : 0);
            RefreshUnlockList();
        }

        private void UpdateCurrencyDisplay(int currency)
        {
            if (currencyText != null)
            {
                currencyText.text = $"Currency: {currency}";
            }
        }

        private void RefreshUnlockList()
        {
            // Clear existing items
            foreach (var item in _unlockItemInstances)
            {
                if (item != null)
                {
                    Destroy(item);
                }
            }
            _unlockItemInstances.Clear();

            if (shopSystem == null || unlockListParent == null)
            {
                return;
            }

            // Create items for each unlock
            foreach (var unlock in shopSystem.AvailableUnlocks)
            {
                if (unlock == null)
                {
                    continue;
                }

                CreateUnlockItem(unlock);
            }
        }

        private void CreateUnlockItem(ProgressionData unlock)
        {
            if (unlockItemPrefab == null || unlockListParent == null)
            {
                Debug.LogWarning("[ShopUI] Unlock Item Prefab or Parent not assigned!");
                return;
            }

            var itemObj = Instantiate(unlockItemPrefab, unlockListParent);
            _unlockItemInstances.Add(itemObj);

            // Configure the item (assuming it has TextMeshProUGUI components)
            var nameText = itemObj.transform.Find("NameText")?.GetComponent<TextMeshProUGUI>();
            var descriptionText = itemObj.transform.Find("DescriptionText")?.GetComponent<TextMeshProUGUI>();
            var costText = itemObj.transform.Find("CostText")?.GetComponent<TextMeshProUGUI>();
            var buyButton = itemObj.transform.Find("BuyButton")?.GetComponent<Button>();
            var statusText = itemObj.transform.Find("StatusText")?.GetComponent<TextMeshProUGUI>();

            if (nameText != null)
            {
                nameText.text = unlock.UnlockName;
            }

            if (descriptionText != null)
            {
                descriptionText.text = unlock.Description;
            }

            if (costText != null)
            {
                costText.text = $"Cost: {unlock.Cost}";
            }

            bool isUnlocked = shopSystem.IsUnlocked(unlock);
            var status = shopSystem.GetPurchaseStatus(unlock);

            if (statusText != null)
            {
                if (isUnlocked)
                {
                    statusText.text = "UNLOCKED";
                    statusText.color = Color.green;
                }
                else
                {
                    statusText.text = GetStatusText(status);
                    statusText.color = GetStatusColor(status);
                }
            }

            if (buyButton != null)
            {
                buyButton.interactable = !isUnlocked && status == ShopSystem.PurchaseStatus.CanPurchase;
                buyButton.onClick.RemoveAllListeners();
                buyButton.onClick.AddListener(() => TryPurchaseUnlock(unlock));
            }
        }

        private string GetStatusText(ShopSystem.PurchaseStatus status)
        {
            switch (status)
            {
                case ShopSystem.PurchaseStatus.InsufficientCurrency:
                    return "INSUFFICIENT FUNDS";
                case ShopSystem.PurchaseStatus.PrerequisitesNotMet:
                    return "PREREQUISITES NOT MET";
                case ShopSystem.PurchaseStatus.CanPurchase:
                    return "AVAILABLE";
                case ShopSystem.PurchaseStatus.AlreadyUnlocked:
                    return "UNLOCKED";
                default:
                    return "UNAVAILABLE";
            }
        }

        private Color GetStatusColor(ShopSystem.PurchaseStatus status)
        {
            switch (status)
            {
                case ShopSystem.PurchaseStatus.CanPurchase:
                    return Color.white;
                case ShopSystem.PurchaseStatus.InsufficientCurrency:
                    return Color.red;
                case ShopSystem.PurchaseStatus.PrerequisitesNotMet:
                    return Color.yellow;
                case ShopSystem.PurchaseStatus.AlreadyUnlocked:
                    return Color.green;
                default:
                    return Color.gray;
            }
        }

        private void TryPurchaseUnlock(ProgressionData unlock)
        {
            if (shopSystem == null)
            {
                return;
            }

            bool success = shopSystem.TryPurchase(unlock);
            if (success)
            {
                Debug.Log($"[ShopUI] Successfully purchased {unlock.UnlockName}!");
                RefreshUI();
            }
            else
            {
                var status = shopSystem.GetPurchaseStatus(unlock);
                Debug.LogWarning($"[ShopUI] Failed to purchase {unlock.UnlockName}. Status: {status}");
            }
        }

        public void CloseShop()
        {
            gameObject.SetActive(false);
            // Notify HubManager that UI is closed
            var hubManager = FindObjectOfType<HubManager>();
            if (hubManager != null)
            {
                hubManager.CloseAllUI();
            }
        }
    }
}

