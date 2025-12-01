using UnityEngine;
using UnityEngine.SceneManagement;
using ProjectRoguelike.Core;
using ProjectRoguelike.Systems.Meta;

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

        private CurrencyManager _currencyManager;

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
            var bootstrap = AppBootstrap.Instance;
            if (bootstrap != null && bootstrap.GameStateMachine != null)
            {
                bootstrap.BeginRunLoadingAsync();
            }
            else
            {
                // Fallback: direct scene load
                SceneManager.LoadScene(runSceneName);
            }
        }

        /// <summary>
        /// Opens the character selection screen.
        /// </summary>
        public void OpenCharacterSelection()
        {
            Debug.Log("[HubManager] Opening Character Selection...");
            if (characterSelectionUI != null)
            {
                characterSelectionUI.SetActive(true);
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
        }
    }
}

