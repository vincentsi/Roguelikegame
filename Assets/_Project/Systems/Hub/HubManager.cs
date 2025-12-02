using UnityEngine;
using UnityEngine.SceneManagement;
using ProjectRoguelike.Core;
using ProjectRoguelike.Systems.Meta;
using ProjectRoguelike.Gameplay.Player;

namespace ProjectRoguelike.Systems.Hub
{
    /// <summary>
    /// Manages the Hub scene - entry point, UI, navigation to other systems.
    /// </summary>
    public sealed class HubManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameObject characterSelectionUI;
        [SerializeField] private GameObject shopUI;
        [SerializeField] private GameObject narrativeLabUI;
        [SerializeField] private GameObject playerPrefab; // Player prefab to spawn in hub

        [Header("Spawn Settings")]
        [SerializeField] private Transform playerSpawnPoint; // Where to spawn the player in hub
        [SerializeField] private Vector3 defaultSpawnPosition = new Vector3(0f, 1f, 0f);

        [Header("Settings")]
        [SerializeField] private string runSceneName = "Boot"; // Scene to load for runs (temporary, will be procedural later)

        [Header("Run Doors")]
        [SerializeField] private RunDoorConfig leftDoor;
        [SerializeField] private RunDoorConfig rightDoor;
        [SerializeField] private RunDoorConfig frontDoor;

        private CurrencyManager _currencyManager;
        private bool _isAnyUIOpen = false;
        private PlayerInputRouter _playerInputRouter;

        [System.Serializable]
        public class RunDoorConfig
        {
            public string doorName = "Level";
            public string levelName = "Level 1";
            public int difficulty = 1;
            public string sceneName = "Boot"; // Scene to load for this door
            public bool isUnlocked = true;
        }

        private void Awake()
        {
            // Find or create CurrencyManager
            _currencyManager = FindObjectOfType<CurrencyManager>();
            if (_currencyManager == null)
            {
                Debug.LogWarning("[HubManager] CurrencyManager not found. Creating one...");
                var currencyObj = new GameObject("CurrencyManager");
                _currencyManager = currencyObj.AddComponent<CurrencyManager>();
                DontDestroyOnLoad(currencyObj);
            }
        }

        private void Start()
        {
            // Spawn player in hub
            SpawnPlayer();

            // Find player input router
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                _playerInputRouter = player.GetComponent<PlayerInputRouter>();
            }

            // Initialize hub UI
            if (characterSelectionUI != null)
            {
                characterSelectionUI.SetActive(false);
            }
            if (shopUI != null)
            {
                shopUI.SetActive(false);
            }
            if (narrativeLabUI != null)
            {
                narrativeLabUI.SetActive(false);
            }

            Debug.Log("[HubManager] Hub initialized");
        }

        private void Update()
        {
            // Close UI with Escape key
            if (_isAnyUIOpen && Input.GetKeyDown(KeyCode.Escape))
            {
                CloseAllUI();
            }
        }

        private void SpawnPlayer()
        {
            // Check if player already exists
            var existingPlayer = GameObject.FindGameObjectWithTag("Player");
            if (existingPlayer != null)
            {
                Debug.Log("[HubManager] Player already exists in scene");
                return;
            }

            // Spawn player
            if (playerPrefab != null)
            {
                Vector3 spawnPos = playerSpawnPoint != null ? playerSpawnPoint.position : defaultSpawnPosition;
                var player = Instantiate(playerPrefab, spawnPos, Quaternion.identity);
                player.name = "Player";
                Debug.Log($"[HubManager] Spawned player at {spawnPos}");
            }
            else
            {
                Debug.LogWarning("[HubManager] Player Prefab not assigned! Player will not spawn in hub.");
            }
        }

        /// <summary>
        /// Starts a new run (transitions to RunLoading state).
        /// </summary>
        public void StartRun()
        {
            StartRunWithDoor(frontDoor); // Default to front door
        }

        /// <summary>
        /// Starts a run from a specific door.
        /// </summary>
        public void StartRunWithDoor(RunDoorConfig door)
        {
            if (door == null)
            {
                Debug.LogError("[HubManager] Door config is null!");
                return;
            }

            if (!door.isUnlocked)
            {
                Debug.LogWarning($"[HubManager] Door '{door.doorName}' is locked!");
                return;
            }

            Debug.Log($"[HubManager] Starting run from door: {door.doorName} ({door.levelName})");

            var bootstrap = AppBootstrap.Instance;
            if (bootstrap != null && bootstrap.GameStateMachine != null)
            {
                // TODO: Pass door config to run (difficulty, level type, etc.)
                bootstrap.BeginRunLoadingAsync();
            }
            else
            {
                // Fallback: direct scene load
                SceneManager.LoadScene(string.IsNullOrEmpty(door.sceneName) ? runSceneName : door.sceneName);
            }
        }

        /// <summary>
        /// Starts a run from the left door.
        /// </summary>
        public void StartRunLeft() => StartRunWithDoor(leftDoor);

        /// <summary>
        /// Starts a run from the right door.
        /// </summary>
        public void StartRunRight() => StartRunWithDoor(rightDoor);

        /// <summary>
        /// Starts a run from the front door.
        /// </summary>
        public void StartRunFront() => StartRunWithDoor(frontDoor);

        /// <summary>
        /// Opens the character selection screen.
        /// </summary>
        public void OpenCharacterSelection()
        {
            Debug.Log("[HubManager] Opening Character Selection...");
            if (characterSelectionUI != null)
            {
                characterSelectionUI.SetActive(true);
                OnUIOpened();
                Debug.Log("[HubManager] Character Selection UI opened.");
            }
            else
            {
                Debug.LogWarning("[HubManager] Character Selection UI not assigned. Please create the UI in the Hub scene.");
            }
        }

        /// <summary>
        /// Opens the shop.
        /// </summary>
        public void OpenShop()
        {
            Debug.Log("[HubManager] Opening Shop...");
            if (shopUI != null)
            {
                shopUI.SetActive(true);
                OnUIOpened();
                Debug.Log("[HubManager] Shop UI opened.");
            }
            else
            {
                Debug.LogWarning("[HubManager] Shop UI not assigned. Please create the UI in the Hub scene.");
            }
        }

        /// <summary>
        /// Opens the narrative lab.
        /// </summary>
        public void OpenNarrativeLab()
        {
            Debug.Log("[HubManager] Opening Narrative Lab...");
            if (narrativeLabUI != null)
            {
                narrativeLabUI.SetActive(true);
                OnUIOpened();
                Debug.Log("[HubManager] Narrative Lab UI opened.");
            }
            else
            {
                Debug.LogWarning("[HubManager] Narrative Lab UI not assigned. Please create the UI in the Hub scene.");
            }
        }

        /// <summary>
        /// Closes all UI panels.
        /// </summary>
        public void CloseAllUI()
        {
            if (characterSelectionUI != null) characterSelectionUI.SetActive(false);
            if (shopUI != null) shopUI.SetActive(false);
            if (narrativeLabUI != null) narrativeLabUI.SetActive(false);
            OnUIClosed();
        }

        private void OnUIOpened()
        {
            _isAnyUIOpen = true;
            // Unlock cursor and make it visible
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            // Disable player input (movement and camera)
            if (_playerInputRouter != null)
            {
                _playerInputRouter.enabled = false;
            }

            // Also disable movement controller directly
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                var movement = player.GetComponent<FpsMovementController>();
                if (movement != null)
                {
                    movement.enabled = false;
                }
            }
        }

        private void OnUIClosed()
        {
            _isAnyUIOpen = false;
            // Lock cursor and hide it
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            // Re-enable player input
            if (_playerInputRouter != null)
            {
                _playerInputRouter.enabled = true;
            }

            // Re-enable movement controller
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                var movement = player.GetComponent<FpsMovementController>();
                if (movement != null)
                {
                    movement.enabled = true;
                }
            }
        }
    }
}

