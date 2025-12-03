using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using ProjectRoguelike.Procedural;
using UnityEngine.AI;
using ProjectRoguelike.UI;

namespace ProjectRoguelike.Core
{
    public sealed class HubState : GameStateBase
    {
        public override GameStateId Id => GameStateId.Hub;

        protected override async Task OnEnter(GameStateContext context)
        {
            // Check if Hub is already loaded (we're coming from Hub, not Boot)
            var hubScene = SceneManager.GetSceneByName("Hub");
            bool isHubAlreadyLoaded = hubScene.isLoaded;
            
            // Show loading screen when loading Hub (if not already shown by AppBootstrap)
            LoadingScreen loadingScreen = FindOrCreateLoadingScreen();
            
            // If Hub is already loaded and we're already in Hub state, just hide loading screen and return
            // This prevents reloading Hub when Boot loads and AppBootstrap tries to reload Hub
            if (isHubAlreadyLoaded)
            {
                // Make sure camera is enabled if we're already in Hub
                var mainCam = Camera.main;
                if (mainCam != null && !mainCam.enabled)
                {
                    mainCam.enabled = true;
                }
                
                // CRITICAL: Hide loading screen if Hub is already loaded
                if (loadingScreen != null)
                {
                    loadingScreen.Hide();
                }
                
                return;
            }
            if (loadingScreen != null)
            {
                // Update message and progress (AppBootstrap might have already shown it)
                loadingScreen.SetMessage("Loading Hub...");
                loadingScreen.SetProgress(0f);
                
                // If not already shown, show it now
                var loadingPanelField = typeof(LoadingScreen).GetField("loadingPanel", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (loadingPanelField != null)
                {
                    var panel = loadingPanelField.GetValue(loadingScreen) as GameObject;
                    if (panel == null || !panel.activeSelf)
                    {
                        loadingScreen.Show();
                    }
                }
                else
                {
                    // Fallback: just show it
                    loadingScreen.Show();
                }
                
                // Force multiple frame updates to ensure loading screen is rendered before heavy operations
                for (int i = 0; i < 5; i++)
                {
                    await Task.Yield();
                }
            }
            
            // Load hub scene if not already loaded
            if (!isHubAlreadyLoaded)
            {
                // Only load scene in play mode
                if (!Application.isPlaying)
                {
                    return;
                }
                
                var loadOperation = SceneManager.LoadSceneAsync("Hub", LoadSceneMode.Single);
                loadOperation.allowSceneActivation = true;
                
                while (!loadOperation.isDone)
                {
                    if (loadingScreen != null)
                    {
                        // Unity's progress goes from 0 to 0.9, then jumps to 1.0 when scene is activated
                        float progress = loadOperation.progress / 0.9f;
                        loadingScreen.SetProgress(Mathf.Clamp01(progress));
                    }
                    await Task.Yield();
                }
            }
            
            // Wait a bit for scene to fully initialize
            await Task.Delay(100);
            
            if (loadingScreen != null)
            {
                loadingScreen.SetProgress(1f);
            }
            
            // Wait a frame to ensure scene is fully loaded
            await Task.Yield();
            await Task.Delay(100);
            
            // CRITICAL: Re-enable camera before hiding loading screen
            // This ensures camera is never stuck disabled
            var mainCamera = Camera.main;
            if (mainCamera != null && !mainCamera.enabled)
            {
                mainCamera.enabled = true;
            }
            
            // Also check all cameras to be safe
            var allCameras = UnityEngine.Object.FindObjectsOfType<Camera>();
            foreach (var cam in allCameras)
            {
                if (cam.gameObject.activeInHierarchy && !cam.enabled && cam.tag == "MainCamera")
                {
                    cam.enabled = true;
                }
            }
            
            // Hide loading screen last
            if (loadingScreen != null)
            {
                loadingScreen.Hide();
            }
        }

        protected override Task OnExit(GameStateContext context)
        {
            return Task.CompletedTask;
        }
    }

    public sealed class LobbyState : GameStateBase
    {
        public override GameStateId Id => GameStateId.Lobby;

        protected override Task OnEnter(GameStateContext context)
        {
            return Task.CompletedTask;
        }
    }

    public sealed class RunLoadingState : GameStateBase
    {
        public override GameStateId Id => GameStateId.RunLoading;

        protected override async Task OnEnter(GameStateContext context)
        {
            // Show loading screen BEFORE doing anything else
            // This must happen before scene loading to be visible
            var loadingScreen = FindOrCreateLoadingScreen();
            if (loadingScreen != null)
            {
                loadingScreen.Show();
                loadingScreen.SetMessage("Preparing level...");
                loadingScreen.SetProgress(0.1f);
                
                // Force multiple frame updates to ensure loading screen is rendered
                // This is critical - we need to give Unity time to render the UI
                for (int i = 0; i < 10; i++)
                {
                    await Task.Yield();
                }
            }
            
            // Get run config from RunConfigManager
            RunConfig runConfig = null;
            if (context.Services.TryResolve<RunConfigManager>(out var runConfigManager))
            {
                runConfig = runConfigManager.CurrentRunConfig;
            }

            if (runConfig == null)
            {
                // Create a default config
                runConfig = new RunConfig("Default", "Level 1", 1, UnityEngine.Random.Range(0, int.MaxValue), 8, new List<RoomData>());
            }

            if (loadingScreen != null)
            {
                loadingScreen.SetMessage($"Loading {runConfig.LevelName}...");
                loadingScreen.SetProgress(0.2f);
            }
            
            // Load the Run scene (which has the dungeon generator)
            var runScene = SceneManager.GetSceneByName("Run");
            if (!runScene.isLoaded)
            {
                // Only load scene in play mode
                if (!Application.isPlaying)
                {
                    if (loadingScreen != null)
                    {
                        loadingScreen.Hide();
                    }
                    return;
                }
                
                if (loadingScreen != null)
                {
                    loadingScreen.SetMessage("Loading scene...");
                }
                
                var loadOperation = SceneManager.LoadSceneAsync("Run", LoadSceneMode.Single);
                
                // Check if scene exists (LoadSceneAsync returns null if scene doesn't exist)
                if (loadOperation == null)
                {
                    Debug.LogError("[RunLoadingState] Run scene not found! Create it with Tools > Create Run Scene.");
                    if (loadingScreen != null)
                    {
                        loadingScreen.Hide();
                    }
                    return; // Exit early if scene doesn't exist
                }
                
                loadOperation.allowSceneActivation = true;
                
                while (!loadOperation.isDone)
                {
                    if (loadingScreen != null)
                    {
                        // Unity's progress goes from 0 to 0.9, then jumps to 1.0
                        float progress = Mathf.Clamp01(loadOperation.progress / 0.9f);
                        loadingScreen.SetProgress(0.2f + (progress * 0.2f));
                    }
                    await Task.Yield();
                }
            }
            
            // Verify scene loaded correctly
            runScene = SceneManager.GetSceneByName("Run");
            if (!runScene.isLoaded)
            {
                Debug.LogError("[RunLoadingState] Failed to load Run scene! Make sure it's in Build Settings.");
                if (loadingScreen != null)
                {
                    loadingScreen.Hide();
                }
                return;
            }

            // Wait for scene to fully initialize
            await Task.Delay(100);
            
            if (loadingScreen != null)
            {
                loadingScreen.SetProgress(0.4f);
            }

            // Ensure player exists in Run scene (might have been destroyed during scene load)
            EnsurePlayerInRunScene();

            // Find DungeonManager in the scene and generate the dungeon
            var dungeonManager = UnityEngine.Object.FindObjectOfType<DungeonManager>();
            if (dungeonManager != null)
            {
                // Clear any existing dungeon
                dungeonManager.ClearDungeon();

                // Configure DungeonManager with run config
                dungeonManager.SetSeed(runConfig.Seed);
                dungeonManager.SetTargetRoomCount(runConfig.TargetRoomCount);
                
                if (runConfig.AvailableRooms.Count > 0)
                {
                    dungeonManager.SetAvailableRooms(runConfig.AvailableRooms);
                }
                else
                {
                    Debug.LogError("[RunLoadingState] No available rooms in RunConfig! Make sure RoomData are assigned in HubManager.");
                }

                if (loadingScreen != null)
                {
                    loadingScreen.SetMessage("Generating dungeon...");
                    loadingScreen.SetProgress(0.5f);
                }

                // Generate the dungeon
                dungeonManager.GenerateDungeon();
                
                if (loadingScreen != null)
                {
                    loadingScreen.SetProgress(0.9f);
                    loadingScreen.SetMessage("Almost there...");
                }
            }
            else
            {
                Debug.LogError("[RunLoadingState] DungeonManager not found in Run scene! Make sure you've created the Run scene with Tools > Create Run Scene.");
                
                // Hide loading screen even if there's an error
                if (loadingScreen != null)
                {
                    loadingScreen.Hide();
                }
                return; // Don't transition to RunActive if dungeon generation failed
            }

            // Transition to RunActive after generation
            await Task.Delay(500); // Small delay for dungeon to fully assemble
            
            if (loadingScreen != null)
            {
                loadingScreen.SetProgress(1f);
                loadingScreen.SetMessage("Ready!");
            }
            
            // Small delay to show "Ready!" message
            await Task.Delay(200);
            
            // Hide loading screen before transitioning
            if (loadingScreen != null)
            {
                loadingScreen.Hide();
            }
            
            // Ensure loading screen is hidden
            await Task.Yield();
            
            if (context.Services.TryResolve<GameStateMachine>(out var stateMachine))
            {
                await stateMachine.ChangeStateAsync(GameStateId.RunActive);
            }
            else
            {
                Debug.LogError("[RunLoadingState] GameStateMachine not found! Cannot transition to RunActive.");
            }
        }

        private void EnsurePlayerInRunScene()
        {
            // Check if player exists
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player == null)
            {
                // Player will be spawned/teleported in RunActiveState
            }
        }
    }

    public sealed class RunActiveState : GameStateBase
    {
        public override GameStateId Id => GameStateId.RunActive;

        protected override async Task OnEnter(GameStateContext context)
        {
            // CRITICAL: Ensure camera is enabled when entering RunActive
            var mainCamera = Camera.main;
            if (mainCamera != null && !mainCamera.enabled)
            {
                mainCamera.enabled = true;
            }
            
            // Ensure loading screen is hidden when entering RunActive
            var loadingScreen = FindOrCreateLoadingScreen();
            if (loadingScreen != null)
            {
                loadingScreen.Hide();
            }
            
            // Wait a frame to ensure loading screen is hidden
            await Task.Yield();
            
            // Teleport player to the first room of the generated dungeon
            TeleportPlayerToDungeon();
        }

        private void TeleportPlayerToDungeon()
        {
            // Find player (should exist from Hub, might be DontDestroyOnLoad)
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player == null)
            {
                Debug.LogWarning("[RunActiveState] Player not found! Trying to spawn player in dungeon.");
                // Try to spawn player if not found (might have been destroyed during scene load)
                SpawnPlayerInDungeon();
                return;
            }

            // Find DungeonManager to get the first room
            var dungeonManager = UnityEngine.Object.FindObjectOfType<DungeonManager>();
            if (dungeonManager == null || dungeonManager.Generator == null)
            {
                Debug.LogWarning("[RunActiveState] DungeonManager or Generator not found! Player will stay at current position.");
                return;
            }

            // Get the first room (always at Vector2Int.zero)
            var nodes = dungeonManager.Generator.Nodes;
            if (nodes == null || nodes.Count == 0)
            {
                Debug.LogError("[RunActiveState] No rooms generated! Player will stay at current position. Check if RoomData are assigned in HubManager.");
                return;
            }
            
            // Find the first room (start room at Vector2Int.zero)
            var firstRoomNode = nodes[0]; // First node is always the start room
            if (firstRoomNode == null)
            {
                return;
            }

            // Calculate spawn position (center of first room)
            // Rooms are spaced 20 units apart, first room is at (0, 0, 0)
            Vector3 spawnPosition = new Vector3(0f, 1f, 0f); // 1 unit above ground

            // Try to find the instantiated room GameObject
            // Rooms are named like "RoomName_(x, y)" where first room is at (0, 0)
            // Vector2Int.ToString() gives "(x, y)" format
            var expectedRoomName = $"{firstRoomNode.RoomData.RoomName}_{firstRoomNode.GridPosition}";
            var roomGameObject = GameObject.Find(expectedRoomName);
            
            if (roomGameObject == null)
            {
                // Try alternative naming or find by DungeonRoot
                var dungeonRoot = GameObject.Find("DungeonRoot");
                if (dungeonRoot != null && dungeonRoot.transform.childCount > 0)
                {
                    // Try to find the room by checking all children
                    for (int i = 0; i < dungeonRoot.transform.childCount; i++)
                    {
                        var child = dungeonRoot.transform.GetChild(i).gameObject;
                        
                        // Check if this is the start room (at grid position 0,0)
                        if (child.name.Contains(firstRoomNode.RoomData.RoomName) && 
                            (firstRoomNode.GridPosition == Vector2Int.zero || child.name.Contains("(0, 0)")))
                        {
                            roomGameObject = child;
                            break;
                        }
                    }
                    
                    // If still not found, use first child as fallback
                    if (roomGameObject == null)
                    {
                        roomGameObject = dungeonRoot.transform.GetChild(0).gameObject;
                    }
                }
            }

            if (roomGameObject != null)
            {
                var roomModule = roomGameObject.GetComponent<RoomModule>();
                if (roomModule != null)
                {
                    // Use first enemy spawn point if available, or center of room
                    if (roomModule.EnemySpawnPoints.Count > 0)
                    {
                        spawnPosition = roomModule.EnemySpawnPoints[0].position;
                    }
                    else
                    {
                        spawnPosition = roomGameObject.transform.position + new Vector3(0f, 1f, 0f);
                    }
                }
                else
                {
                    spawnPosition = roomGameObject.transform.position + new Vector3(0f, 1f, 0f);
                }
            }
            else
            {
                // Fallback: use default position (first room should be at 0,0,0)
                spawnPosition = new Vector3(0f, 1f, 0f);
            }

            // Teleport player
            var characterController = player.GetComponent<CharacterController>();
            if (characterController != null)
            {
                characterController.enabled = false;
                player.transform.position = spawnPosition;
                characterController.enabled = true;
            }
            else
            {
                player.transform.position = spawnPosition;
            }

        }

        private void SpawnPlayerInDungeon()
        {
            // Find player prefab (try to get from PlayerManager or find in scene)
            var bootstrap = AppBootstrap.Instance;
            if (bootstrap != null && bootstrap.Services.TryResolve<PlayerManager>(out var playerManager))
            {
                var primaryPlayer = playerManager.PrimaryPlayer;
                if (primaryPlayer != null)
                {
                    // Player exists but might be in wrong scene, just teleport it
                    TeleportPlayerToDungeon();
                    return;
                }
            }
        }
    }

    public sealed class ResultsState : GameStateBase
    {
        public override GameStateId Id => GameStateId.Results;

        protected override async Task OnEnter(GameStateContext context)
        {
            Debug.Log("[GameState] Showing results and payouts.");
            
            // TODO: Show results UI, calculate rewards, etc.
            // After showing results, transition back to Hub
            await Task.Delay(3000); // Show results for 3 seconds (placeholder)
            
            if (context.Services.TryResolve<GameStateMachine>(out var stateMachine))
            {
                await stateMachine.ChangeStateAsync(GameStateId.Hub);
            }
        }
    }
}

