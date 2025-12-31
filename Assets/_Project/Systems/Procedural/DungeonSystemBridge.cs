using UnityEngine;
using ProjectRoguelike.Levels;

namespace ProjectRoguelike.Procedural
{
    /// <summary>
    /// Pont entre l'ancien système (DungeonGenerator) et le nouveau (DungeonGraphGenerator + LevelManager).
    /// Permet une migration progressive sans casser le code existant.
    /// </summary>
    public sealed class DungeonSystemBridge : MonoBehaviour
    {
        [Header("System Selection")]
        [SerializeField] private bool useLegacySystem = true;
        [Tooltip("Si false, utilise DungeonGraphGenerator + LevelManager")]

        [Header("Legacy System (DungeonGenerator)")]
        [SerializeField] private DungeonManager legacyDungeonManager;

        [Header("New System (DungeonGraphGenerator)")]
        [SerializeField] private LevelManager levelManager;
        [SerializeField] private RoomManager roomManager;
        [SerializeField] private DoorManager doorManager;
        [SerializeField] private DungeonGeneratorSettings generatorSettings;

        public bool IsUsingLegacySystem => useLegacySystem;
        public DungeonManager LegacyDungeonManager => legacyDungeonManager;
        public LevelManager LevelManager => levelManager;
        public RoomManager RoomManager => roomManager;

        private void Awake()
        {
            // Validation
            if (useLegacySystem && legacyDungeonManager == null)
            {
                Debug.LogError("[DungeonSystemBridge] Legacy system selected but DungeonManager is null!");
            }

            if (!useLegacySystem && (levelManager == null || roomManager == null || doorManager == null))
            {
                Debug.LogError("[DungeonSystemBridge] New system selected but components are missing!");
            }

            // Créer les composants manquants si nécessaire
            if (!useLegacySystem)
            {
                if (levelManager == null)
                {
                    var levelGO = new GameObject("[LevelManager]");
                    levelGO.transform.SetParent(transform);
                    levelManager = levelGO.AddComponent<LevelManager>();
                }

                if (roomManager == null)
                {
                    var roomGO = new GameObject("[RoomManager]");
                    roomGO.transform.SetParent(transform);
                    roomManager = roomGO.AddComponent<RoomManager>();
                }

                if (doorManager == null)
                {
                    var doorGO = new GameObject("[DoorManager]");
                    doorGO.transform.SetParent(transform);
                    doorManager = doorGO.AddComponent<DoorManager>();
                }
            }
        }

        /// <summary>
        /// Génère un donjon en utilisant le système sélectionné.
        /// </summary>
        public async System.Threading.Tasks.Task GenerateDungeonAsync(LevelTheme theme, int difficulty)
        {
            if (useLegacySystem)
            {
                // Utiliser l'ancien système (non async)
                if (legacyDungeonManager != null)
                {
                    legacyDungeonManager.GenerateDungeon();
                    await System.Threading.Tasks.Task.Yield();
                }
                else
                {
                    Debug.LogError("[DungeonSystemBridge] Cannot generate dungeon - LegacyDungeonManager is null!");
                }
            }
            else
            {
                // Utiliser le nouveau système
                if (levelManager != null && theme != null)
                {
                    await levelManager.InitializeLevel(theme, difficulty);
                }
                else
                {
                    Debug.LogError("[DungeonSystemBridge] Cannot generate dungeon - LevelManager or theme is null!");
                }
            }
        }

        /// <summary>
        /// Récupère le générateur actuel (selon le système utilisé).
        /// </summary>
        public object GetCurrentGenerator()
        {
            return useLegacySystem
                ? (object)legacyDungeonManager?.Generator
                : levelManager?.GetCurrentDungeon();
        }

        /// <summary>
        /// Nettoie le donjon actuel.
        /// </summary>
        public void CleanupDungeon()
        {
            if (useLegacySystem)
            {
                legacyDungeonManager?.ClearDungeon();
            }
            else
            {
                // Le nouveau système gère le cleanup automatiquement via RoomManager
                Debug.Log("[DungeonSystemBridge] New system cleanup handled by RoomManager");
            }
        }

        /// <summary>
        /// Migre du système legacy vers le nouveau système.
        /// ATTENTION: À appeler uniquement en dehors d'un run actif!
        /// </summary>
        [ContextMenu("Migrate to New System")]
        public void MigrateToNewSystem()
        {
            if (!useLegacySystem)
            {
                Debug.LogWarning("[DungeonSystemBridge] Already using new system!");
                return;
            }

            Debug.Log("[DungeonSystemBridge] Migrating to new system...");
            useLegacySystem = false;

            // Créer les composants du nouveau système si nécessaire
            Awake();

            Debug.Log("[DungeonSystemBridge] Migration complete. Remember to assign LevelTheme to initialize!");
        }

        /// <summary>
        /// Rollback vers le système legacy.
        /// </summary>
        [ContextMenu("Rollback to Legacy System")]
        public void RollbackToLegacySystem()
        {
            if (useLegacySystem)
            {
                Debug.LogWarning("[DungeonSystemBridge] Already using legacy system!");
                return;
            }

            Debug.Log("[DungeonSystemBridge] Rolling back to legacy system...");
            useLegacySystem = true;

            Debug.Log("[DungeonSystemBridge] Rollback complete.");
        }
    }
}
