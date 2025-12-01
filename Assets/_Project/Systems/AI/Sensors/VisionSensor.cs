using UnityEngine;

namespace ProjectRoguelike.AI.Sensors
{
    /// <summary>
    /// Vision sensor for AI: detects players within field of view and range.
    /// </summary>
    public sealed class VisionSensor : MonoBehaviour
    {
        [Header("Vision Settings")]
        [SerializeField] private float viewRange = 15f;
        [SerializeField] private float viewAngle = 90f; // Degrees
        [SerializeField] private LayerMask obstacleLayer = ~0;
        [SerializeField] private float checkInterval = 0.2f; // How often to check (performance)

        private Transform _target;
        private float _lastCheckTime;
        private bool _hasLineOfSight;

        public bool HasTarget => _target != null;
        public Transform Target => _target;
        public bool HasLineOfSight => _hasLineOfSight;
        public float DistanceToTarget => _target != null ? Vector3.Distance(transform.position, _target.position) : float.MaxValue;

        public void SetViewRange(float range) => viewRange = range;
        public void SetViewAngle(float angle) => viewAngle = angle;

        private void Update()
        {
            if (Time.time - _lastCheckTime < checkInterval)
            {
                return;
            }

            _lastCheckTime = Time.time;
            ScanForTargets();
        }

        private void ScanForTargets()
        {
            _target = null;
            _hasLineOfSight = false;

            // Get closest player via PlayerManager
            var bootstrap = Core.AppBootstrap.Instance;
            if (bootstrap == null || !bootstrap.Services.TryResolve<Core.PlayerManager>(out var playerManager))
            {
                return;
            }

            var player = playerManager.GetClosestPlayer(transform.position);
            if (player == null)
            {
                return;
            }

            // Check if player is alive
            if (player.TryGetComponent<Gameplay.Player.PlayerStatsComponent>(out var stats) && stats.CurrentHealth <= 0f)
            {
                return;
            }

            var directionToPlayer = (player.position - transform.position);
            var distance = directionToPlayer.magnitude;

            // Check range
            if (distance > viewRange)
            {
                return;
            }

            // Check angle (field of view)
            directionToPlayer.Normalize();
            var angle = Vector3.Angle(transform.forward, directionToPlayer);
            if (angle > viewAngle * 0.5f)
            {
                return;
            }

            // Check line of sight (raycast)
            if (Physics.Raycast(transform.position, directionToPlayer, out var hit, distance, obstacleLayer))
            {
                // Check if we hit the player (or their collider)
                if (hit.collider.transform == player || hit.collider.transform.IsChildOf(player))
                {
                    _target = player;
                    _hasLineOfSight = true;
                }
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (!enabled) return;

            // Draw view range (only when selected)
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, viewRange);

            // Draw view angle (only when selected)
            Gizmos.color = Color.cyan;
            var halfAngle = viewAngle * 0.5f;
            var leftRay = Quaternion.Euler(0, -halfAngle, 0) * transform.forward;
            var rightRay = Quaternion.Euler(0, halfAngle, 0) * transform.forward;
            Gizmos.DrawRay(transform.position, leftRay * viewRange);
            Gizmos.DrawRay(transform.position, rightRay * viewRange);
            
            // Draw a line connecting the two rays to form a cone
            var leftEnd = transform.position + leftRay * viewRange;
            var rightEnd = transform.position + rightRay * viewRange;
            Gizmos.DrawLine(leftEnd, rightEnd);

            // Draw line to target (only when selected)
            if (_target != null)
            {
                Gizmos.color = _hasLineOfSight ? Color.green : Color.red;
                Gizmos.DrawLine(transform.position, _target.position);
            }
        }
    }
}

