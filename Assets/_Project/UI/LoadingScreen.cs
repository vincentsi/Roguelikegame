using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ProjectRoguelike.UI
{
    /// <summary>
    /// Manages the loading screen UI - displays during scene transitions and dungeon generation.
    /// </summary>
    public sealed class LoadingScreen : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject loadingPanel;
        [SerializeField] private TextMeshProUGUI loadingText;
        [SerializeField] private Image progressBarFill;
        [SerializeField] private TextMeshProUGUI progressText;

        [Header("Settings")]
        [SerializeField] private string[] loadingMessages = {
            "Loading...",
            "Preparing level...",
            "Generating dungeon...",
            "Almost there..."
        };

        private float _currentProgress = 0f;
        private string _currentMessage = "";

        private void Awake()
        {
            // Hide loading screen by default
            if (loadingPanel != null)
            {
                loadingPanel.SetActive(false);
            }
        }

        /// <summary>
        /// Shows the loading screen.
        /// </summary>
        public void Show()
        {
            if (loadingPanel != null)
            {
                loadingPanel.SetActive(true);
                // Make sure it's on top and can receive raycasts
                Canvas canvas = loadingPanel.GetComponentInParent<Canvas>();
                if (canvas != null)
                {
                    canvas.sortingOrder = 9999;
                    var raycaster = canvas.GetComponent<UnityEngine.UI.GraphicRaycaster>();
                    if (raycaster != null)
                    {
                        raycaster.enabled = true;
                    }
                }
            }
            SetProgress(0f);
            SetMessage(loadingMessages.Length > 0 ? loadingMessages[0] : "Loading...");
        }

        /// <summary>
        /// Hides the loading screen.
        /// </summary>
        public void Hide()
        {
            if (loadingPanel != null)
            {
                loadingPanel.SetActive(false);
                // Disable raycaster to prevent blocking interactions
                Canvas canvas = loadingPanel.GetComponentInParent<Canvas>();
                if (canvas != null)
                {
                    var raycaster = canvas.GetComponent<UnityEngine.UI.GraphicRaycaster>();
                    if (raycaster != null)
                    {
                        raycaster.enabled = false;
                    }
                }
            }
        }

        /// <summary>
        /// Sets the loading progress (0-1).
        /// </summary>
        public void SetProgress(float progress)
        {
            _currentProgress = Mathf.Clamp01(progress);
            
            if (progressBarFill != null)
            {
                progressBarFill.fillAmount = _currentProgress;
            }

            if (progressText != null)
            {
                progressText.text = $"{Mathf.RoundToInt(_currentProgress * 100)}%";
            }
        }

        /// <summary>
        /// Sets the loading message.
        /// </summary>
        public void SetMessage(string message)
        {
            _currentMessage = message;
            if (loadingText != null)
            {
                loadingText.text = message;
            }
        }

        /// <summary>
        /// Sets a random loading message from the list.
        /// </summary>
        public void SetRandomMessage()
        {
            if (loadingMessages.Length > 0)
            {
                var randomMessage = loadingMessages[Random.Range(0, loadingMessages.Length)];
                SetMessage(randomMessage);
            }
        }
    }
}

