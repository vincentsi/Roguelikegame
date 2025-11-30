using System.Threading.Tasks;
using UnityEngine;

namespace ProjectRoguelike.Core
{
    public sealed class HubState : GameStateBase
    {
        public override GameStateId Id => GameStateId.Hub;

        protected override Task OnEnter(GameStateContext context)
        {
            Debug.Log("[GameState] Enter Hub");
            // TODO: Load hub scene, enable hub-specific services, show UI.
            return Task.CompletedTask;
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

        protected override Task OnEnter(GameStateContext context)
        {
            Debug.Log("[GameState] Preparing procedural run...");
            // TODO: Trigger procedural builder + scene streaming, then inform flow controller to advance.
            return Task.CompletedTask;
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

        protected override Task OnEnter(GameStateContext context)
        {
            Debug.Log("[GameState] Showing results and payouts.");
            return Task.CompletedTask;
        }
    }
}

