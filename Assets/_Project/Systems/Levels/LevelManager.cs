using System;
using System.Threading.Tasks;
using UnityEngine;
using ProjectRoguelike.Procedural;

namespace ProjectRoguelike.Levels
{
    /// <summary>
    /// Gère le niveau actuel (thème, progression, génération).
    /// </summary>
    public sealed class LevelManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private DungeonManager dungeonManager;
        [SerializeField] private RoomManager roomManager;
        [SerializeField] private DoorManager doorManager;
        [SerializeField] private DungeonGeneratorSettings generatorSettings;

        private LevelTheme _currentTheme;
        private int _currentLevelIndex;
        private DungeonGraph _currentDungeon;
        private RoomLoader _roomLoader;
        private bool _isLevelComplete = false;

        public event Action OnLevelComplete;
        public event Action OnLevelFailed;

        public LevelTheme GetCurrentTheme() => _currentTheme;
        public int GetCurrentLevelIndex() => _currentLevelIndex;
        public DungeonGraph GetCurrentDungeon() => _currentDungeon;

        private void Awake()
        {
            if (dungeonManager == null)
            {
                dungeonManager = FindObjectOfType<DungeonManager>();
            }

            if (roomManager == null)
            {
                roomManager = GetComponent<RoomManager>();
                if (roomManager == null)
                {
                    roomManager = gameObject.AddComponent<RoomManager>();
                }
            }

            if (doorManager == null)
            {
                doorManager = GetComponent<DoorManager>();
                if (doorManager == null)
                {
                    doorManager = gameObject.AddComponent<DoorManager>();
                }
            }
        }

        public async Task InitializeLevel(LevelTheme theme, int levelIndex)
        {
            if (theme == null)
            {
                Debug.LogError("[LevelManager] Cannot initialize with null theme!");
                return;
            }

            _currentTheme = theme;
            _currentLevelIndex = levelIndex;
            _isLevelComplete = false;

            // Générer le donjon
            await GenerateDungeonAsync();

            // Initialiser RoomManager et DoorManager
            if (_currentDungeon != null)
            {
                if (_roomLoader == null)
                {
                    var dungeonRoot = dungeonManager != null ? dungeonManager.transform : transform;
                    _roomLoader = new RoomLoader(dungeonRoot);
                }

                roomManager.Initialize(_currentDungeon, _roomLoader, doorManager);
                doorManager.Initialize(_currentDungeon, roomManager);

                // Écouter les événements
                roomManager.OnRoomEntered += OnRoomEntered;
                roomManager.OnBossDefeated += OnBossDefeated;
            }
        }

        private async Task GenerateDungeonAsync()
        {
            if (_currentTheme == null)
            {
                Debug.LogError("[LevelManager] No theme set for generation!");
                return;
            }

            if (generatorSettings == null)
            {
                Debug.LogError("[LevelManager] No DungeonGeneratorSettings assigned!");
                return;
            }

            // Réinitialiser les compteurs statiques avant de générer
            RoomNode.ResetNodeIdCounter();
            DungeonGraph.ResetGraphIdCounter();

            // Créer seed
            var seed = new LevelSeed();

            // Créer générateur
            var generator = new DungeonGraphGenerator(seed, generatorSettings);

            // Générer le graphe
            int minRooms = generatorSettings.MinRoomsBeforeBoss;
            int maxRooms = generatorSettings.MaxRoomsBeforeBoss;
            _currentDungeon = generator.Generate(minRooms, maxRooms, _currentTheme);

            if (_currentDungeon == null)
            {
                Debug.LogError("[LevelManager] Failed to generate dungeon!");
                return;
            }

            // Assembler les salles (utiliser RoomAssembler existant si disponible)
            if (dungeonManager != null)
            {
                // Le DungeonManager existant peut être utilisé pour l'assemblage
                // ou on peut créer un nouvel assembler
                await AssembleDungeonAsync();
            }

            await Task.Yield();
        }

        private async Task AssembleDungeonAsync()
        {
            if (_currentDungeon == null || _roomLoader == null)
            {
                return;
            }

            // Charger toutes les salles (ou seulement le start pour commencer)
            // Pour l'instant, on charge juste le start
            if (_currentDungeon.StartNode != null)
            {
                await _roomLoader.LoadRoomAsync(_currentDungeon.StartNode);
            }

            await Task.Yield();
        }

        private void OnRoomEntered(RoomNode room)
        {
            // Spawner les portes pour cette salle
            var roomInstance = _roomLoader.GetLoadedRoom(room);
            if (roomInstance != null)
            {
                doorManager.SpawnDoorsForRoom(room, roomInstance);
            }
        }

        private void OnBossDefeated()
        {
            if (_isLevelComplete)
            {
                return;
            }

            _isLevelComplete = true;
            OnLevelComplete?.Invoke();
        }

        public void OnLevelComplete()
        {
            // Sauvegarder progression, donner récompenses, etc.
            Debug.Log($"[LevelManager] Level {_currentLevelIndex} completed!");
        }

        public void OnLevelFailed()
        {
            OnLevelFailed?.Invoke();
            Debug.Log($"[LevelManager] Level {_currentLevelIndex} failed!");
        }

        private void OnDestroy()
        {
            if (roomManager != null)
            {
                roomManager.OnRoomEntered -= OnRoomEntered;
                roomManager.OnBossDefeated -= OnBossDefeated;
            }

            _roomLoader?.ClearAll();
            doorManager?.Clear();
        }
    }
}

