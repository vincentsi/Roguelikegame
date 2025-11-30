using ProjectRoguelike.Gameplay.Weapons;
using UnityEngine;

namespace ProjectRoguelike.Gameplay.Player
{
    /// <summary>
    /// Handles player death: disables controls, shows death state, etc.
    /// </summary>
    [RequireComponent(typeof(PlayerStatsComponent))]
    public sealed class PlayerDeathHandler : MonoBehaviour
    {
        [Header("Death Settings")]
        [SerializeField] private bool disableMovementOnDeath = true;
        [SerializeField] private bool disableWeaponsOnDeath = true;
        [SerializeField] private bool disableCameraOnDeath = true;

        private PlayerStatsComponent _stats;
        private FpsMovementController _movement;
        private WeaponController _weapons;
        private PlayerCameraController _camera;
        private bool _isDead;

        private void Awake()
        {
            _stats = GetComponent<PlayerStatsComponent>();
            _movement = GetComponent<FpsMovementController>();
            _weapons = GetComponent<WeaponController>();
            _camera = GetComponentInChildren<PlayerCameraController>();
        }

        private void OnEnable()
        {
            _stats.OnPlayerDowned += HandlePlayerDeath;
        }

        private void OnDisable()
        {
            _stats.OnPlayerDowned -= HandlePlayerDeath;
        }

        private void HandlePlayerDeath()
        {
            if (_isDead)
            {
                return;
            }

            _isDead = true;

            // Disable movement
            if (disableMovementOnDeath && _movement != null)
            {
                _movement.enabled = false;
            }

            // Disable weapons
            if (disableWeaponsOnDeath && _weapons != null)
            {
                _weapons.enabled = false;
            }

            // Disable camera (optional - you might want to keep it for spectator mode)
            if (disableCameraOnDeath && _camera != null)
            {
                _camera.enabled = false;
            }

            // Disable input
            var inputRouter = GetComponent<PlayerInputRouter>();
            if (inputRouter != null)
            {
                inputRouter.enabled = false;
            }

            // TODO: Show death UI, play death animation, enable spectator camera, etc.
            // TODO: In coop mode, allow teammates to revive
        }

        public void Revive()
        {
            if (!_isDead)
            {
                return;
            }

            _isDead = false;
            _stats.Revive();

            // Re-enable components
            if (_movement != null)
            {
                _movement.enabled = true;
            }

            if (_weapons != null)
            {
                _weapons.enabled = true;
            }

            if (_camera != null)
            {
                _camera.enabled = true;
            }

            var inputRouter = GetComponent<PlayerInputRouter>();
            if (inputRouter != null)
            {
                inputRouter.enabled = true;
            }

        }
    }
}

