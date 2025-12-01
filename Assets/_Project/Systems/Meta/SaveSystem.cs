using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using ProjectRoguelike.Systems.Hub;

namespace ProjectRoguelike.Systems.Meta
{
    /// <summary>
    /// Handles saving and loading meta-progression data (currency, unlocks, etc.).
    /// </summary>
    public sealed class SaveSystem : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private string saveFileName = "save.json";

        private string SaveFilePath => Path.Combine(Application.persistentDataPath, saveFileName);

        [System.Serializable]
        private class SaveData
        {
            public int currency;
            public List<string> unlockedItemIds = new List<string>();
            public int selectedCharacterIndex;
        }

        /// <summary>
        /// Saves the current game state.
        /// </summary>
        public void SaveGame(CurrencyManager currencyManager, ShopSystem shopSystem, CharacterSelector characterSelector = null)
        {
            var saveData = new SaveData();

            // Save currency
            if (currencyManager != null)
            {
                saveData.currency = currencyManager.CurrentCurrency;
            }

            // Save unlocked items (using asset GUIDs as IDs)
            if (shopSystem != null)
            {
                // TODO: Get unlocked items from ShopSystem and save their GUIDs
                // For now, this is a placeholder
            }

            // Save selected character
            if (characterSelector != null)
            {
                saveData.selectedCharacterIndex = characterSelector.SelectedCharacterIndex;
            }

            // Serialize and save
            try
            {
                string json = JsonUtility.ToJson(saveData, true);
                File.WriteAllText(SaveFilePath, json);
                Debug.Log($"[SaveSystem] Game saved to {SaveFilePath}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveSystem] Failed to save game: {e.Message}");
            }
        }

        /// <summary>
        /// Loads the saved game state.
        /// </summary>
        public void LoadGame(CurrencyManager currencyManager, ShopSystem shopSystem, CharacterSelector characterSelector = null)
        {
            if (!File.Exists(SaveFilePath))
            {
                Debug.Log("[SaveSystem] No save file found. Starting fresh.");
                return;
            }

            try
            {
                string json = File.ReadAllText(SaveFilePath);
                SaveData saveData = JsonUtility.FromJson<SaveData>(json);

                // Load currency
                if (currencyManager != null)
                {
                    currencyManager.SetCurrency(saveData.currency);
                }

                // Load unlocked items
                if (shopSystem != null && saveData.unlockedItemIds != null)
                {
                    // TODO: Restore unlocked items from GUIDs
                    // For now, this is a placeholder
                }

                // Load selected character
                if (characterSelector != null && saveData.selectedCharacterIndex >= 0)
                {
                    characterSelector.SelectCharacter(saveData.selectedCharacterIndex);
                }

                Debug.Log($"[SaveSystem] Game loaded from {SaveFilePath}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveSystem] Failed to load game: {e.Message}");
            }
        }

        /// <summary>
        /// Deletes the save file.
        /// </summary>
        public void DeleteSave()
        {
            if (File.Exists(SaveFilePath))
            {
                File.Delete(SaveFilePath);
                Debug.Log("[SaveSystem] Save file deleted.");
            }
        }

        /// <summary>
        /// Checks if a save file exists.
        /// </summary>
        public bool HasSaveFile()
        {
            return File.Exists(SaveFilePath);
        }
    }
}

