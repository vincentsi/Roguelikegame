using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor;
using ProjectRoguelike.Core;
using ProjectRoguelike.UI;

namespace ProjectRoguelike.Editor.Tools
{
    /// <summary>
    /// Editor tool to create a loading screen UI in the Boot scene.
    /// </summary>
    public static class CreateLoadingScreen
    {
        [MenuItem("Tools/Create Loading Screen", false, 50)]
        public static void CreateLoadingScreenUI()
        {
            // Find or create AppBootstrap (should be in Boot scene)
            var bootstrap = Object.FindObjectOfType<AppBootstrap>();
            if (bootstrap == null)
            {
                Debug.LogError("[CreateLoadingScreen] AppBootstrap not found! Make sure you're in the Boot scene.");
                return;
            }

            // Check if loading screen already exists
            var existingLoadingScreen = bootstrap.GetComponentInChildren<LoadingScreen>();
            if (existingLoadingScreen != null)
            {
                Debug.LogWarning("[CreateLoadingScreen] LoadingScreen already exists! Skipping creation.");
                return;
            }

            // Create Canvas for loading screen (if it doesn't exist)
            Canvas canvas = bootstrap.GetComponentInChildren<Canvas>();
            if (canvas == null)
            {
                GameObject canvasObj = new GameObject("LoadingCanvas");
                canvasObj.transform.SetParent(bootstrap.transform);
                
                // Make sure it's DontDestroyOnLoad (since bootstrap is already DontDestroyOnLoad, children inherit it)
                // But we'll also ensure it explicitly
                Object.DontDestroyOnLoad(canvasObj);
                
                canvas = canvasObj.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = 9999; // Always on top
                canvasObj.AddComponent<CanvasScaler>();
                // Don't add GraphicRaycaster - we don't want to block interactions when hidden
                var raycaster = canvasObj.AddComponent<GraphicRaycaster>();
                raycaster.enabled = false; // Disable by default, will be enabled when showing
            }

            // Create Loading Panel
            GameObject loadingPanel = new GameObject("LoadingPanel");
            loadingPanel.transform.SetParent(canvas.transform);
            Image panelImage = loadingPanel.AddComponent<Image>();
            panelImage.color = new Color(0f, 0f, 0f, 0.95f); // Dark background

            RectTransform panelRect = loadingPanel.GetComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.sizeDelta = Vector2.zero;
            panelRect.anchoredPosition = Vector2.zero;

            // Create Loading Text
            GameObject loadingTextObj = new GameObject("LoadingText");
            loadingTextObj.transform.SetParent(loadingPanel.transform);
            TextMeshProUGUI loadingText = loadingTextObj.AddComponent<TextMeshProUGUI>();
            loadingText.text = "Loading...";
            loadingText.fontSize = 36;
            loadingText.alignment = TextAlignmentOptions.Center;
            loadingText.color = Color.white;

            RectTransform loadingTextRect = loadingTextObj.GetComponent<RectTransform>();
            loadingTextRect.anchorMin = new Vector2(0.5f, 0.5f);
            loadingTextRect.anchorMax = new Vector2(0.5f, 0.5f);
            loadingTextRect.sizeDelta = new Vector2(600, 50);
            loadingTextRect.anchoredPosition = new Vector2(0, 50);

            // Create Progress Bar Background
            GameObject progressBarBg = new GameObject("ProgressBarBackground");
            progressBarBg.transform.SetParent(loadingPanel.transform);
            Image bgImage = progressBarBg.AddComponent<Image>();
            bgImage.color = new Color(0.3f, 0.3f, 0.3f, 1f);

            RectTransform bgRect = progressBarBg.GetComponent<RectTransform>();
            bgRect.anchorMin = new Vector2(0.5f, 0.5f);
            bgRect.anchorMax = new Vector2(0.5f, 0.5f);
            bgRect.sizeDelta = new Vector2(400, 20);
            bgRect.anchoredPosition = new Vector2(0, -20);

            // Create Progress Bar Fill
            GameObject progressBarFill = new GameObject("ProgressBarFill");
            progressBarFill.transform.SetParent(progressBarBg.transform);
            Image fillImage = progressBarFill.AddComponent<Image>();
            fillImage.color = new Color(0.2f, 0.6f, 1f, 1f); // Blue
            fillImage.type = Image.Type.Filled;
            fillImage.fillMethod = Image.FillMethod.Horizontal;

            RectTransform fillRect = progressBarFill.GetComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.sizeDelta = Vector2.zero;
            fillRect.anchoredPosition = Vector2.zero;

            // Create Progress Text
            GameObject progressTextObj = new GameObject("ProgressText");
            progressTextObj.transform.SetParent(loadingPanel.transform);
            TextMeshProUGUI progressText = progressTextObj.AddComponent<TextMeshProUGUI>();
            progressText.text = "0%";
            progressText.fontSize = 24;
            progressText.alignment = TextAlignmentOptions.Center;
            progressText.color = Color.white;

            RectTransform progressTextRect = progressTextObj.GetComponent<RectTransform>();
            progressTextRect.anchorMin = new Vector2(0.5f, 0.5f);
            progressTextRect.anchorMax = new Vector2(0.5f, 0.5f);
            progressTextRect.sizeDelta = new Vector2(100, 30);
            progressTextRect.anchoredPosition = new Vector2(0, -60);

            // Add LoadingScreen component
            LoadingScreen loadingScreen = loadingPanel.AddComponent<LoadingScreen>();
            var serializedLoadingScreen = new SerializedObject(loadingScreen);
            serializedLoadingScreen.FindProperty("loadingPanel").objectReferenceValue = loadingPanel;
            serializedLoadingScreen.FindProperty("loadingText").objectReferenceValue = loadingText;
            serializedLoadingScreen.FindProperty("progressBarFill").objectReferenceValue = fillImage;
            serializedLoadingScreen.FindProperty("progressText").objectReferenceValue = progressText;
            serializedLoadingScreen.ApplyModifiedProperties();

            // Hide by default
            loadingPanel.SetActive(false);

            Debug.Log("[CreateLoadingScreen] Loading screen created successfully in Boot scene!");
        }
    }
}

