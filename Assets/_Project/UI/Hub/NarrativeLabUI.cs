using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ProjectRoguelike.Systems.Hub;

namespace ProjectRoguelike.UI.Hub
{
    /// <summary>
    /// UI pour le Narrative Lab - affiche les entrées d'histoire découvertes.
    /// </summary>
    public sealed class NarrativeLabUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private NarrativeLab narrativeLab;
        [SerializeField] private Button closeButton;

        [Header("UI Elements")]
        [SerializeField] private Transform storyListParent; // Parent pour la liste des entrées
        [SerializeField] private GameObject storyEntryPrefab; // Prefab pour une entrée d'histoire
        [SerializeField] private TextMeshProUGUI selectedTitleText;
        [SerializeField] private TextMeshProUGUI selectedContentText;
        [SerializeField] private Image selectedImage; // Image optionnelle pour l'entrée sélectionnée

        private List<GameObject> _storyEntryInstances = new List<GameObject>();
        private NarrativeLab.StoryEntry? _selectedEntry;

        private void Awake()
        {
            if (narrativeLab == null)
            {
                narrativeLab = FindObjectOfType<NarrativeLab>();
            }

            if (closeButton != null)
            {
                closeButton.onClick.AddListener(CloseNarrativeLab);
            }
        }

        private void OnEnable()
        {
            RefreshStoryList();
        }

        private void RefreshStoryList()
        {
            // Clear existing items
            foreach (var item in _storyEntryInstances)
            {
                if (item != null)
                {
                    Destroy(item);
                }
            }
            _storyEntryInstances.Clear();

            if (narrativeLab == null || storyListParent == null)
            {
                return;
            }

            // Create items for each story entry
            foreach (var entry in narrativeLab.StoryEntries)
            {
                CreateStoryEntryItem(entry);
            }
        }

        private void CreateStoryEntryItem(NarrativeLab.StoryEntry entry)
        {
            if (storyEntryPrefab == null || storyListParent == null)
            {
                Debug.LogWarning("[NarrativeLabUI] Story Entry Prefab or Parent not assigned!");
                return;
            }

            var itemObj = Instantiate(storyEntryPrefab, storyListParent);
            _storyEntryInstances.Add(itemObj);

            // Configure the item
            var titleText = itemObj.transform.Find("TitleText")?.GetComponent<TextMeshProUGUI>();
            var statusText = itemObj.transform.Find("StatusText")?.GetComponent<TextMeshProUGUI>();
            var button = itemObj.GetComponent<Button>();

            if (titleText != null)
            {
                titleText.text = entry.title;
            }

            if (statusText != null)
            {
                if (entry.isUnlocked)
                {
                    statusText.text = "UNLOCKED";
                    statusText.color = Color.green;
                }
                else
                {
                    statusText.text = "LOCKED";
                    statusText.color = Color.gray;
                }
            }

            if (button != null)
            {
                button.interactable = entry.isUnlocked;
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => SelectStoryEntry(entry));
            }
        }

        private void SelectStoryEntry(NarrativeLab.StoryEntry entry)
        {
            if (!entry.isUnlocked)
            {
                return;
            }

            _selectedEntry = entry;

            if (selectedTitleText != null)
            {
                selectedTitleText.text = entry.title;
            }

            if (selectedContentText != null)
            {
                selectedContentText.text = entry.content;
            }

            // TODO: Display image if available
            // if (selectedImage != null && entry.image != null)
            // {
            //     selectedImage.sprite = entry.image;
            //     selectedImage.gameObject.SetActive(true);
            // }
        }

        public void CloseNarrativeLab()
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

