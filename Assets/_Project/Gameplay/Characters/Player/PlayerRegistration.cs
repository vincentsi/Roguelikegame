using ProjectRoguelike.Core;
using UnityEngine;

namespace ProjectRoguelike.Gameplay.Player
{
    /// <summary>
    /// Registers the player with PlayerManager on enable, unregisters on disable.
    /// Attach to the root player GameObject.
    /// </summary>
    public sealed class PlayerRegistration : MonoBehaviour
    {
        private void OnEnable()
        {
            // Try to register immediately
            TryRegister();
        }

        private void Start()
        {
            // Retry in Start() in case AppBootstrap wasn't ready in OnEnable()
            TryRegister();
        }

        private void TryRegister()
        {
            var bootstrap = AppBootstrap.Instance;
            if (bootstrap != null && bootstrap.Services.TryResolve<PlayerManager>(out var manager))
            {
                manager.RegisterPlayer(transform);
            }
            else
            {
                Debug.LogWarning("[PlayerRegistration] PlayerManager not found. Make sure AppBootstrap is initialized.");
            }
        }

        private void OnDisable()
        {
            var bootstrap = AppBootstrap.Instance;
            if (bootstrap != null && bootstrap.Services.TryResolve<PlayerManager>(out var manager))
            {
                manager.UnregisterPlayer(transform);
            }
        }
    }
}

