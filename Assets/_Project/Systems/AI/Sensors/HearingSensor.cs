using UnityEngine;

namespace ProjectRoguelike.AI.Sensors
{
    /// <summary>
    /// Hearing sensor for AI: detects sounds (gunshots, footsteps, etc.)
    /// </summary>
    public sealed class HearingSensor : MonoBehaviour
    {
        [Header("Hearing Settings")]
        [SerializeField] private float hearingRange = 20f;
        [SerializeField] private float soundDecayRate = 5f; // How fast sounds fade

        private Vector3? _lastHeardPosition;
        private float _soundIntensity;

        public bool HasHeardSomething => _lastHeardPosition.HasValue && _soundIntensity > 0.1f;
        public Vector3? LastHeardPosition => _lastHeardPosition;
        public float SoundIntensity => _soundIntensity;

        public void SetHearingRange(float range) => hearingRange = range;

        private void Update()
        {
            // Decay sound over time
            if (_soundIntensity > 0f)
            {
                _soundIntensity -= soundDecayRate * Time.deltaTime;
                if (_soundIntensity <= 0f)
                {
                    _lastHeardPosition = null;
                    _soundIntensity = 0f;
                }
            }
        }

        /// <summary>
        /// Called by other systems (weapons, player movement) when a sound is made.
        /// </summary>
        public void OnSoundDetected(Vector3 position, float intensity = 1f)
        {
            var distance = Vector3.Distance(transform.position, position);
            if (distance > hearingRange)
            {
                return;
            }

            // Intensity decreases with distance
            var distanceFactor = 1f - (distance / hearingRange);
            var effectiveIntensity = intensity * distanceFactor;

            // Only update if this sound is louder than current
            if (effectiveIntensity > _soundIntensity)
            {
                _lastHeardPosition = position;
                _soundIntensity = effectiveIntensity;
            }
        }

        public void ClearSound()
        {
            _lastHeardPosition = null;
            _soundIntensity = 0f;
        }

        private void OnDrawGizmosSelected()
        {
            if (!enabled) return;

            // Draw hearing range (only when selected)
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(transform.position, hearingRange);

            // Draw last heard position (only when selected)
            if (_lastHeardPosition.HasValue)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(_lastHeardPosition.Value, 0.5f);
                Gizmos.DrawLine(transform.position, _lastHeardPosition.Value);
            }
        }
    }
}

