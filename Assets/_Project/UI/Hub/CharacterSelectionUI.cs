using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ProjectRoguelike.Systems.Hub;

namespace ProjectRoguelike.UI.Hub
{
    /// <summary>
    /// UI pour la sélection de personnage.
    /// </summary>
    public sealed class CharacterSelectionUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private CharacterSelector characterSelector;
        [SerializeField] private Button closeButton;

        [Header("UI Elements")]
        [SerializeField] private TextMeshProUGUI characterNameText;
        [SerializeField] private TextMeshProUGUI characterDescriptionText;
        [SerializeField] private Image characterIconImage;
        [SerializeField] private Button previousButton;
        [SerializeField] private Button nextButton;
        [SerializeField] private Button selectButton;

        private int _currentCharacterIndex = 0;

        private void Awake()
        {
            if (characterSelector == null)
            {
                characterSelector = FindObjectOfType<CharacterSelector>();
            }

            if (closeButton != null)
            {
                closeButton.onClick.AddListener(CloseCharacterSelection);
            }

            if (previousButton != null)
            {
                previousButton.onClick.AddListener(SelectPreviousCharacter);
            }

            if (nextButton != null)
            {
                nextButton.onClick.AddListener(SelectNextCharacter);
            }

            if (selectButton != null)
            {
                selectButton.onClick.AddListener(SelectCurrentCharacter);
            }
        }

        private void OnEnable()
        {
            if (characterSelector != null)
            {
                _currentCharacterIndex = 0;
                RefreshUI();
            }
        }

        private void RefreshUI()
        {
            if (characterSelector == null)
            {
                return;
            }

            var characters = characterSelector.AvailableCharacters;
            if (characters == null || characters.Count == 0)
            {
                if (characterNameText != null)
                {
                    characterNameText.text = "No Characters Available";
                }
                if (characterDescriptionText != null)
                {
                    characterDescriptionText.text = "";
                }
                return;
            }

            // Clamp index
            _currentCharacterIndex = Mathf.Clamp(_currentCharacterIndex, 0, characters.Count - 1);
            var currentCharacter = characters[_currentCharacterIndex];

            if (characterNameText != null)
            {
                characterNameText.text = currentCharacter.characterName;
            }

            if (characterDescriptionText != null)
            {
                characterDescriptionText.text = currentCharacter.description;
            }

            if (characterIconImage != null && currentCharacter.characterIcon != null)
            {
                characterIconImage.sprite = currentCharacter.characterIcon;
            }

            // Update button states
            if (previousButton != null)
            {
                previousButton.interactable = characters.Count > 1;
            }

            if (nextButton != null)
            {
                nextButton.interactable = characters.Count > 1;
            }
        }

        private void SelectNextCharacter()
        {
            if (characterSelector == null)
            {
                return;
            }

            var characters = characterSelector.AvailableCharacters;
            if (characters == null || characters.Count <= 1)
            {
                return;
            }

            _currentCharacterIndex = (_currentCharacterIndex + 1) % characters.Count;
            RefreshUI();
        }

        private void SelectPreviousCharacter()
        {
            if (characterSelector == null)
            {
                return;
            }

            var characters = characterSelector.AvailableCharacters;
            if (characters == null || characters.Count <= 1)
            {
                return;
            }

            _currentCharacterIndex = (_currentCharacterIndex - 1 + characters.Count) % characters.Count;
            RefreshUI();
        }

        private void SelectCurrentCharacter()
        {
            if (characterSelector == null)
            {
                return;
            }

            var characters = characterSelector.AvailableCharacters;
            if (characters == null || _currentCharacterIndex < 0 || _currentCharacterIndex >= characters.Count)
            {
                return;
            }

            characterSelector.SelectCharacter(_currentCharacterIndex);
            Debug.Log($"[CharacterSelectionUI] Selected character: {characters[_currentCharacterIndex].characterName}");
            CloseCharacterSelection();
        }

        public void CloseCharacterSelection()
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

