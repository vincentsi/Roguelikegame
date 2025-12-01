using System.Collections.Generic;
using UnityEngine;

namespace ProjectRoguelike.Systems.Hub
{
    /// <summary>
    /// Manages narrative/story elements in the hub.
    /// Basic system for displaying story content.
    /// </summary>
    public sealed class NarrativeLab : MonoBehaviour
    {
        [System.Serializable]
        public class StoryEntry
        {
            public string title;
            [TextArea(3, 10)]
            public string content;
            public Sprite image; // Optional image for the story entry
            public bool isUnlocked = true; // Whether this story entry is unlocked
        }

        [Header("Story Content")]
        [SerializeField] private List<StoryEntry> storyEntries = new List<StoryEntry>();

        [Header("Settings")]
        [SerializeField] private int currentStoryIndex = 0;

        public IReadOnlyList<StoryEntry> StoryEntries => storyEntries;
        public int CurrentStoryIndex => currentStoryIndex;
        public StoryEntry CurrentStory => storyEntries.Count > 0 && currentStoryIndex >= 0 && currentStoryIndex < storyEntries.Count
            ? storyEntries[currentStoryIndex]
            : null;

        /// <summary>
        /// Shows the next story entry.
        /// </summary>
        public void ShowNext()
        {
            if (storyEntries.Count == 0)
            {
                return;
            }

            int nextIndex = (currentStoryIndex + 1) % storyEntries.Count;
            ShowStory(nextIndex);
        }

        /// <summary>
        /// Shows the previous story entry.
        /// </summary>
        public void ShowPrevious()
        {
            if (storyEntries.Count == 0)
            {
                return;
            }

            int prevIndex = (currentStoryIndex - 1 + storyEntries.Count) % storyEntries.Count;
            ShowStory(prevIndex);
        }

        /// <summary>
        /// Shows a specific story entry by index.
        /// </summary>
        public void ShowStory(int index)
        {
            if (index < 0 || index >= storyEntries.Count)
            {
                return;
            }

            if (!storyEntries[index].isUnlocked)
            {
                Debug.LogWarning($"[NarrativeLab] Story entry {index} is locked!");
                return;
            }

            currentStoryIndex = index;
            Debug.Log($"[NarrativeLab] Showing story: {storyEntries[index].title}");
        }

        /// <summary>
        /// Unlocks a story entry.
        /// </summary>
        public void UnlockStory(int index)
        {
            if (index >= 0 && index < storyEntries.Count)
            {
                storyEntries[index].isUnlocked = true;
            }
        }
    }
}

