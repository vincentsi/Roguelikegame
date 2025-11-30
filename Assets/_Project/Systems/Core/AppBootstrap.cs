using System.Threading.Tasks;
using UnityEngine;

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

            BootServices();

            if (autoStart)
            {
                await StartFlowAsync();
            }
        }

        private void BootServices()
        {
            Services = new ServiceRegistry();
            EventBus = new GameEventBus();
            GameStateMachine = new GameStateMachine();
            var playerManager = new PlayerManager();

            Services.Register(EventBus);
            Services.Register(GameStateMachine);
            Services.Register(playerManager);

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
            return GameStateMachine.ChangeStateAsync(entryState);
        }

        public Task GoToHubAsync() => GameStateMachine.ChangeStateAsync(GameStateId.Hub);
        public Task GoToLobbyAsync() => GameStateMachine.ChangeStateAsync(GameStateId.Lobby);
        public Task BeginRunLoadingAsync() => GameStateMachine.ChangeStateAsync(GameStateId.RunLoading);
        public Task ShowResultsAsync() => GameStateMachine.ChangeStateAsync(GameStateId.Results);
    }
}

