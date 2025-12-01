using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectRoguelike.Systems.Hub
{
    /// <summary>
    /// Manages character selection in the hub.
    /// </summary>
    public sealed class CharacterSelector : MonoBehaviour
    {
        [System.Serializable]
        public class CharacterData
        {
            public string characterName;
            public Sprite characterIcon;
            public GameObject characterPrefab; // Prefab to spawn for this character
            public string description;
        }

        [Header("Characters")]
        [SerializeField] private List<CharacterData> availableCharacters = new List<CharacterData>();
        [SerializeField] private int selectedCharacterIndex = 0;

        public event Action<int> OnCharacterSelected;

        public IReadOnlyList<CharacterData> AvailableCharacters => availableCharacters;
        public int SelectedCharacterIndex => selectedCharacterIndex;
        public CharacterData SelectedCharacter => availableCharacters.Count > 0 && selectedCharacterIndex >= 0 && selectedCharacterIndex < availableCharacters.Count 
            ? availableCharacters[selectedCharacterIndex] 
            : null;

        /// <summary>
        /// Selects a character by index.
        /// </summary>
        public void SelectCharacter(int index)
        {
            if (index < 0 || index >= availableCharacters.Count)
            {
                Debug.LogWarning($"[CharacterSelector] Invalid character index: {index}");
                return;
            }

            selectedCharacterIndex = index;
            OnCharacterSelected?.Invoke(index);
            Debug.Log($"[CharacterSelector] Selected character: {availableCharacters[index].characterName}");
        }

        /// <summary>
        /// Selects the next character.
        /// </summary>
        public void SelectNext()
        {
            if (availableCharacters.Count == 0)
            {
                return;
            }

            int nextIndex = (selectedCharacterIndex + 1) % availableCharacters.Count;
            SelectCharacter(nextIndex);
        }

        /// <summary>
        /// Selects the previous character.
        /// </summary>
        public void SelectPrevious()
        {
            if (availableCharacters.Count == 0)
            {
                return;
            }

            int prevIndex = (selectedCharacterIndex - 1 + availableCharacters.Count) % availableCharacters.Count;
            SelectCharacter(prevIndex);
        }

        /// <summary>
        /// Confirms the character selection and starts the run.
        /// </summary>
        public void ConfirmSelection()
        {
            if (SelectedCharacter == null)
            {
                Debug.LogWarning("[CharacterSelector] No character selected!");
                return;
            }

            // TODO: Store selected character for the run
            // This will be used when spawning the player in the run scene
            Debug.Log($"[CharacterSelector] Confirmed selection: {SelectedCharacter.characterName}");
        }
    }
}

