using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ProjectRoguelike.Systems.Hub;
using ProjectRoguelike.Systems.Meta;

namespace ProjectRoguelike.UI.Hub
{
    /// <summary>
    /// UI pour la Collection - affiche tous les items débloquables (armes, personnages, etc.).
    /// </summary>
    public sealed class CollectionUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private CollectionSystem collectionSystem;
        [SerializeField] private Button closeButton;

        [Header("UI Elements")]
        [SerializeField] private Transform categoryTabsParent; // Pour les onglets de catégories
        [SerializeField] private Transform itemListParent; // Parent pour les items de la liste
        [SerializeField] private GameObject categoryTabPrefab; // Prefab pour un onglet de catégorie
        [SerializeField] private GameObject collectionItemPrefab; // Prefab pour un item de collection

        [Header("Item Display")]
        [SerializeField] private TextMeshProUGUI itemNameText;
        [SerializeField] private TextMeshProUGUI itemDescriptionText;
        [SerializeField] private Image itemIconImage;
        [SerializeField] private TextMeshProUGUI itemStatusText;

        private List<GameObject> _categoryTabInstances = new List<GameObject>();
        private List<GameObject> _itemInstances = new List<GameObject>();
        private ProgressionData.UnlockType _currentCategory = ProgressionData.UnlockType.Weapon;
        private ProgressionData _selectedItem;

        private void Awake()
        {
            if (collectionSystem == null)
            {
                collectionSystem = FindObjectOfType<CollectionSystem>();
            }

            if (closeButton != null)
            {
                closeButton.onClick.AddListener(CloseCollection);
            }
        }

        private void OnEnable()
        {
            RefreshUI();
            
            if (collectionSystem != null)
            {
                collectionSystem.OnCollectionUpdated += RefreshUI;
            }
        }

        private void OnDisable()
        {
            if (collectionSystem != null)
            {
                collectionSystem.OnCollectionUpdated -= RefreshUI;
            }
        }

        private void RefreshUI()
        {
            RefreshCategoryTabs();
            RefreshItemList();
            UpdateSelectedItemDisplay();
        }

        private void RefreshCategoryTabs()
        {
            // Clear existing tabs
            foreach (var tab in _categoryTabInstances)
            {
                if (tab != null) Destroy(tab);
            }
            _categoryTabInstances.Clear();

            if (categoryTabsParent == null || categoryTabPrefab == null) return;

            // Create tabs for each unlock type
            var unlockTypes = System.Enum.GetValues(typeof(ProgressionData.UnlockType));
            foreach (ProgressionData.UnlockType type in unlockTypes)
            {
                GameObject tabObj = Instantiate(categoryTabPrefab, categoryTabsParent);
                Button tabButton = tabObj.GetComponent<Button>();
                TextMeshProUGUI tabText = tabObj.GetComponentInChildren<TextMeshProUGUI>();

                if (tabText != null)
                {
                    tabText.text = GetCategoryName(type);
                }

                if (tabButton != null)
                {
                    ProgressionData.UnlockType categoryType = type; // Capture for closure
                    tabButton.onClick.AddListener(() => OnCategorySelected(categoryType));
                    
                    // Highlight current category
                    var colors = tabButton.colors;
                    colors.normalColor = _currentCategory == type ? Color.yellow : Color.white;
                    tabButton.colors = colors;
                }

                _categoryTabInstances.Add(tabObj);
            }
        }

        private void RefreshItemList()
        {
            // Clear existing items
            foreach (var item in _itemInstances)
            {
                if (item != null) Destroy(item);
            }
            _itemInstances.Clear();

            if (collectionSystem == null || itemListParent == null || collectionItemPrefab == null) return;

            // Get items for current category
            var items = collectionSystem.GetItemsByType(_currentCategory);

            foreach (var item in items)
            {
                if (item == null) continue;

                GameObject itemObj = Instantiate(collectionItemPrefab, itemListParent);
                Button itemButton = itemObj.GetComponent<Button>();
                
                // Set item name
                TextMeshProUGUI nameText = itemObj.GetComponentInChildren<TextMeshProUGUI>();
                if (nameText != null)
                {
                    nameText.text = item.UnlockName;
                }

                // Set item icon if available
                Image iconImage = itemObj.GetComponentInChildren<Image>();
                if (iconImage != null && item.Icon != null)
                {
                    iconImage.sprite = item.Icon;
                }

                // Set visual state based on unlock status
                var status = collectionSystem.GetItemStatus(item);
                SetItemVisualState(itemObj, status);

                if (itemButton != null)
                {
                    ProgressionData selectedItem = item; // Capture for closure
                    itemButton.onClick.AddListener(() => OnItemSelected(selectedItem));
                }

                _itemInstances.Add(itemObj);
            }
        }

        private void SetItemVisualState(GameObject itemObj, CollectionSystem.CollectionItemStatus status)
        {
            // Adjust alpha/color based on status
            CanvasGroup canvasGroup = itemObj.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = itemObj.AddComponent<CanvasGroup>();
            }

            switch (status)
            {
                case CollectionSystem.CollectionItemStatus.Unlocked:
                    canvasGroup.alpha = 1f;
                    break;
                case CollectionSystem.CollectionItemStatus.Locked:
                case CollectionSystem.CollectionItemStatus.LockedPrerequisites:
                case CollectionSystem.CollectionItemStatus.LockedInsufficientCurrency:
                    canvasGroup.alpha = 0.5f;
                    break;
            }
        }

        private void OnCategorySelected(ProgressionData.UnlockType category)
        {
            _currentCategory = category;
            RefreshUI();
        }

        private void OnItemSelected(ProgressionData item)
        {
            _selectedItem = item;
            UpdateSelectedItemDisplay();
        }

        private void UpdateSelectedItemDisplay()
        {
            if (_selectedItem == null)
            {
                if (itemNameText != null) itemNameText.text = "";
                if (itemDescriptionText != null) itemDescriptionText.text = "";
                if (itemIconImage != null) itemIconImage.sprite = null;
                if (itemStatusText != null) itemStatusText.text = "";
                return;
            }

            if (itemNameText != null)
            {
                itemNameText.text = _selectedItem.UnlockName;
            }

            if (itemDescriptionText != null)
            {
                itemDescriptionText.text = _selectedItem.Description;
            }

            if (itemIconImage != null)
            {
                itemIconImage.sprite = _selectedItem.Icon;
            }

            if (itemStatusText != null && collectionSystem != null)
            {
                var status = collectionSystem.GetItemStatus(_selectedItem);
                itemStatusText.text = GetStatusText(status);
            }
        }

        private string GetStatusText(CollectionSystem.CollectionItemStatus status)
        {
            switch (status)
            {
                case CollectionSystem.CollectionItemStatus.Unlocked:
                    return "✓ Débloqué";
                case CollectionSystem.CollectionItemStatus.Locked:
                    return "🔒 Verrouillé";
                case CollectionSystem.CollectionItemStatus.LockedPrerequisites:
                    return "🔒 Prérequis manquants";
                case CollectionSystem.CollectionItemStatus.LockedInsufficientCurrency:
                    return "🔒 Monnaie insuffisante";
                default:
                    return "";
            }
        }

        private string GetCategoryName(ProgressionData.UnlockType type)
        {
            switch (type)
            {
                case ProgressionData.UnlockType.Weapon:
                    return "Armes";
                case ProgressionData.UnlockType.Character:
                    return "Personnages";
                case ProgressionData.UnlockType.Item:
                    return "Items";
                case ProgressionData.UnlockType.Upgrade:
                    return "Améliorations";
                case ProgressionData.UnlockType.Cosmetic:
                    return "Cosmétiques";
                default:
                    return type.ToString();
            }
        }

        public void CloseCollection()
        {
            var hubManager = FindObjectOfType<HubManager>();
            if (hubManager != null)
            {
                hubManager.CloseAllUI();
            }
            else
            {
                gameObject.SetActive(false);
            }
        }
    }
}

