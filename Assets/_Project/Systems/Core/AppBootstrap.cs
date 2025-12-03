using System.Threading.Tasks;
using UnityEngine;
using ProjectRoguelike.UI;

namespace ProjectRoguelike.Core
{
    /// <summary>
    /// Entry point MonoBehaviour living in Boot scene. Wires services and launches initial state.
    /// </summary>
    public sealed class AppBootstrap : MonoBehaviour
    {
        [Header("Flow")]
        [SerializeField] private bool autoStart = true;
        [SerializeField] private GameStateId entryState = GameStateId.Hub;

        public static AppBootstrap Instance { get; private set; }

        public ServiceRegistry Services { get; private set; }
        public GameEventBus EventBus { get; private set; }
        public GameStateMachine GameStateMachine { get; private set; }

        private async void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Check if we're already initialized (coming from another scene)
            bool alreadyInitialized = Services != null && GameStateMachine != null;

            if (!alreadyInitialized)
            {
                BootServices();
            }

            // Wait a frame before starting flow to ensure everything is initialized
            await Task.Yield();

            // Only auto-start if we're not already in a game state
            // This prevents reloading Hub when Boot is loaded during a run
            if (autoStart && !alreadyInitialized)
            {
                // Show loading screen early if we're going to Hub (from Boot)
                // Only do this if we're actually in Boot scene
                var activeScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
                if (activeScene.name == "Boot" && entryState == GameStateId.Hub)
                {
                    var loadingScreen = FindOrCreateLoadingScreen();
                    if (loadingScreen != null)
                    {
                        loadingScreen.Show();
                        loadingScreen.SetMessage("Loading...");
                        loadingScreen.SetProgress(0f);
                        
                        // Force multiple frames to render the loading screen before heavy operations
                        for (int i = 0; i < 10; i++)
                        {
                            await Task.Yield();
                        }
                    }
                }
                
                await StartFlowAsync();
            }
            // If already initialized, don't auto-start again (prevents reloading Hub when Boot loads during a run)
        }
        
        private LoadingScreen FindOrCreateLoadingScreen()
        {
            // Try to find existing loading screen
            var loadingScreen = UnityEngine.Object.FindObjectOfType<LoadingScreen>();
            if (loadingScreen != null)
            {
                return loadingScreen;
            }
            
            // Search in AppBootstrap children (where it's usually created)
            loadingScreen = GetComponentInChildren<LoadingScreen>();
            if (loadingScreen != null)
            {
                return loadingScreen;
            }
            
            // Search in scene
            var canvas = UnityEngine.Object.FindObjectOfType<Canvas>();
            if (canvas != null)
            {
                loadingScreen = canvas.GetComponentInChildren<LoadingScreen>();
                if (loadingScreen != null)
                {
                    return loadingScreen;
                }
            }
            
            // Create loading screen if it doesn't exist
            return CreateLoadingScreenRuntime();
        }
        
        private LoadingScreen CreateLoadingScreenRuntime()
        {
            // Create Canvas for loading screen
            GameObject canvasObj = new GameObject("LoadingCanvas");
            canvasObj.transform.SetParent(transform);
            UnityEngine.Object.DontDestroyOnLoad(canvasObj);
            
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 9999;
            canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
            var raycaster = canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            raycaster.enabled = false;
            
            // Create Loading Panel
            GameObject loadingPanel = new GameObject("LoadingPanel");
            loadingPanel.transform.SetParent(canvasObj.transform);
            var panelImage = loadingPanel.AddComponent<UnityEngine.UI.Image>();
            panelImage.color = new Color(0f, 0f, 0f, 0.95f);
            
            RectTransform panelRect = loadingPanel.GetComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.sizeDelta = Vector2.zero;
            panelRect.anchoredPosition = Vector2.zero;
            
            // Create Loading Text
            GameObject loadingTextObj = new GameObject("LoadingText");
            loadingTextObj.transform.SetParent(loadingPanel.transform);
            var loadingText = loadingTextObj.AddComponent<TMPro.TextMeshProUGUI>();
            loadingText.text = "Loading...";
            loadingText.fontSize = 36;
            loadingText.alignment = TMPro.TextAlignmentOptions.Center;
            loadingText.color = Color.white;
            
            RectTransform loadingTextRect = loadingTextObj.GetComponent<RectTransform>();
            loadingTextRect.anchorMin = new Vector2(0.5f, 0.5f);
            loadingTextRect.anchorMax = new Vector2(0.5f, 0.5f);
            loadingTextRect.sizeDelta = new Vector2(600, 50);
            loadingTextRect.anchoredPosition = new Vector2(0, 50);
            
            // Create Progress Bar Background
            GameObject progressBarBg = new GameObject("ProgressBarBackground");
            progressBarBg.transform.SetParent(loadingPanel.transform);
            var bgImage = progressBarBg.AddComponent<UnityEngine.UI.Image>();
            bgImage.color = new Color(0.3f, 0.3f, 0.3f, 1f);
            
            RectTransform bgRect = progressBarBg.GetComponent<RectTransform>();
            bgRect.anchorMin = new Vector2(0.5f, 0.5f);
            bgRect.anchorMax = new Vector2(0.5f, 0.5f);
            bgRect.sizeDelta = new Vector2(400, 20);
            bgRect.anchoredPosition = new Vector2(0, -20);
            
            // Create Progress Bar Fill
            GameObject progressBarFill = new GameObject("ProgressBarFill");
            progressBarFill.transform.SetParent(progressBarBg.transform);
            var fillImage = progressBarFill.AddComponent<UnityEngine.UI.Image>();
            fillImage.color = new Color(0.2f, 0.6f, 1f, 1f);
            fillImage.type = UnityEngine.UI.Image.Type.Filled;
            fillImage.fillMethod = UnityEngine.UI.Image.FillMethod.Horizontal;
            
            RectTransform fillRect = progressBarFill.GetComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.sizeDelta = Vector2.zero;
            fillRect.anchoredPosition = Vector2.zero;
            
            // Create Progress Text
            GameObject progressTextObj = new GameObject("ProgressText");
            progressTextObj.transform.SetParent(loadingPanel.transform);
            var progressText = progressTextObj.AddComponent<TMPro.TextMeshProUGUI>();
            progressText.text = "0%";
            progressText.fontSize = 24;
            progressText.alignment = TMPro.TextAlignmentOptions.Center;
            progressText.color = Color.white;
            
            RectTransform progressTextRect = progressTextObj.GetComponent<RectTransform>();
            progressTextRect.anchorMin = new Vector2(0.5f, 0.5f);
            progressTextRect.anchorMax = new Vector2(0.5f, 0.5f);
            progressTextRect.sizeDelta = new Vector2(100, 30);
            progressTextRect.anchoredPosition = new Vector2(0, -60);
            
            // Add LoadingScreen component
            LoadingScreen loadingScreenComponent = loadingPanel.AddComponent<LoadingScreen>();
            
            // Use reflection to set private fields (or make them public/protected)
            var loadingPanelField = typeof(LoadingScreen).GetField("loadingPanel", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var loadingTextField = typeof(LoadingScreen).GetField("loadingText", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var progressBarFillField = typeof(LoadingScreen).GetField("progressBarFill", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var progressTextField = typeof(LoadingScreen).GetField("progressText", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (loadingPanelField != null) loadingPanelField.SetValue(loadingScreenComponent, loadingPanel);
            if (loadingTextField != null) loadingTextField.SetValue(loadingScreenComponent, loadingText);
            if (progressBarFillField != null) progressBarFillField.SetValue(loadingScreenComponent, fillImage);
            if (progressTextField != null) progressTextField.SetValue(loadingScreenComponent, progressText);
            
            // Hide by default
            loadingPanel.SetActive(false);
            
            return loadingScreenComponent;
        }

        private void BootServices()
        {
            Services = new ServiceRegistry();
            EventBus = new GameEventBus();
            GameStateMachine = new GameStateMachine();
            var playerManager = new PlayerManager();
            var runConfigManager = new RunConfigManager();

            Services.Register(EventBus);
            Services.Register(GameStateMachine);
            Services.Register(playerManager);
            Services.Register(runConfigManager);

            var context = new GameStateContext(Services, EventBus);
            GameStateMachine.SetContext(context);

            GameStateMachine.RegisterState(new HubState());
            GameStateMachine.RegisterState(new LobbyState());
            GameStateMachine.RegisterState(new RunLoadingState());
            GameStateMachine.RegisterState(new RunActiveState());
            GameStateMachine.RegisterState(new ResultsState());
        }

        public Task StartFlowAsync()
        {
            // Don't start if entryState is None
            if (entryState == GameStateId.None)
            {
                return Task.CompletedTask;
            }
            return GameStateMachine.ChangeStateAsync(entryState);
        }

        public Task GoToHubAsync() => GameStateMachine.ChangeStateAsync(GameStateId.Hub);
        public Task GoToLobbyAsync() => GameStateMachine.ChangeStateAsync(GameStateId.Lobby);
        public Task BeginRunLoadingAsync() => GameStateMachine.ChangeStateAsync(GameStateId.RunLoading);
        public Task ShowResultsAsync() => GameStateMachine.ChangeStateAsync(GameStateId.Results);
    }
}

