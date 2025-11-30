using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProjectRoguelike.Core
{
    public enum GameStateId
    {
        None = 0,
        Hub = 1,
        Lobby = 2,
        RunLoading = 3,
        RunActive = 4,
        Results = 5
    }

    public readonly struct GameStateChangedEvent
    {
        public GameStateId Previous { get; }
        public GameStateId Current { get; }

        public GameStateChangedEvent(GameStateId previous, GameStateId current)
        {
            Previous = previous;
            Current = current;
        }
    }

    public interface IGameState
    {
        GameStateId Id { get; }
        Task Enter(GameStateContext context);
        Task Exit(GameStateContext context);
    }

    public sealed class GameStateContext
    {
        public ServiceRegistry Services { get; }
        public GameEventBus EventBus { get; }

        public GameStateContext(ServiceRegistry services, GameEventBus eventBus)
        {
            Services = services ?? throw new ArgumentNullException(nameof(services));
            EventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        }

        public T Resolve<T>() where T : class => Services.Resolve<T>();
        public bool TryResolve<T>(out T service) where T : class => Services.TryResolve(out service);
    }

    public sealed class GameStateMachine
    {
        private readonly Dictionary<GameStateId, IGameState> _states = new();
        private GameStateContext _context;
        private IGameState _currentState;
        private bool _isTransitioning;

        public GameStateId CurrentStateId => _currentState?.Id ?? GameStateId.None;

        public void SetContext(GameStateContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public void RegisterState(IGameState state)
        {
            if (state == null)
            {
                throw new ArgumentNullException(nameof(state));
            }

            _states[state.Id] = state;
        }

        public async Task ChangeStateAsync(GameStateId targetId)
        {
            if (_context == null)
            {
                throw new InvalidOperationException("GameStateMachine context not set.");
            }

            if (_isTransitioning || (_currentState != null && _currentState.Id == targetId))
            {
                return;
            }

            _isTransitioning = true;
            var previous = _currentState?.Id ?? GameStateId.None;

            if (_currentState != null)
            {
                await _currentState.Exit(_context);
            }

            if (!_states.TryGetValue(targetId, out var nextState))
            {
                _isTransitioning = false;
                throw new InvalidOperationException($"Game state not registered: {targetId}");
            }

            _currentState = nextState;
            await nextState.Enter(_context);

            _context.EventBus.Publish(new GameStateChangedEvent(previous, targetId));
            _isTransitioning = false;
        }
    }

    public abstract class GameStateBase : IGameState
    {
        public abstract GameStateId Id { get; }

        public Task Enter(GameStateContext context)
        {
            return OnEnter(context);
        }

        public Task Exit(GameStateContext context)
        {
            return OnExit(context);
        }

        protected virtual Task OnEnter(GameStateContext context) => Task.CompletedTask;
        protected virtual Task OnExit(GameStateContext context) => Task.CompletedTask;
    }
}

