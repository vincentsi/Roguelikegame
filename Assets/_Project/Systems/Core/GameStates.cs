using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ProjectRoguelike.Core
{
    public sealed class HubState : GameStateBase
    {
        public override GameStateId Id => GameStateId.Hub;

        protected override async Task OnEnter(GameStateContext context)
        {
            Debug.Log("[GameState] Enter Hub");
            
            // Load hub scene if not already loaded
            var hubScene = SceneManager.GetSceneByName("Hub");
            if (!hubScene.isLoaded)
            {
                var loadOperation = SceneManager.LoadSceneAsync("Hub", LoadSceneMode.Single);
                while (!loadOperation.isDone)
                {
                    await Task.Yield();
                }
            }
        }

        protected override Task OnExit(GameStateContext context)
        {
            Debug.Log("[GameState] Exit Hub");
            return Task.CompletedTask;
        }
    }

    public sealed class LobbyState : GameStateBase
    {
        public override GameStateId Id => GameStateId.Lobby;

        protected override Task OnEnter(GameStateContext context)
        {
            Debug.Log("[GameState] Enter Lobby");
            return Task.CompletedTask;
        }
    }

    public sealed class RunLoadingState : GameStateBase
    {
        public override GameStateId Id => GameStateId.RunLoading;

        protected override async Task OnEnter(GameStateContext context)
        {
            Debug.Log("[GameState] Preparing procedural run...");
            
            // TODO: Trigger procedural builder + scene streaming
            // For now, load the Boot scene (which has the dungeon generator)
            var runScene = SceneManager.GetSceneByName("Boot");
            if (!runScene.isLoaded)
            {
                var loadOperation = SceneManager.LoadSceneAsync("Boot", LoadSceneMode.Single);
                while (!loadOperation.isDone)
                {
                    await Task.Yield();
                }
            }

            // Transition to RunActive after loading
            await Task.Delay(500); // Small delay for scene to initialize
            if (context.Services.TryResolve<GameStateMachine>(out var stateMachine))
            {
                await stateMachine.ChangeStateAsync(GameStateId.RunActive);
            }
        }
    }

    public sealed class RunActiveState : GameStateBase
    {
        public override GameStateId Id => GameStateId.RunActive;

        protected override Task OnEnter(GameStateContext context)
        {
            Debug.Log("[GameState] Run active — gameplay enabled.");
            return Task.CompletedTask;
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

