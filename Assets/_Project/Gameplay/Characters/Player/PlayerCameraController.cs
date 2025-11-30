using UnityEngine;

namespace ProjectRoguelike.Gameplay.Player
{
    public sealed class PlayerCameraController : MonoBehaviour
    {
        [SerializeField] private Camera playerCamera;
        [SerializeField] private Transform cameraPivot;
        [SerializeField] private float minPitch = -85f;
        [SerializeField] private float maxPitch = 85f;
        [SerializeField] private float recoilReturnSpeed = 6f;
        [SerializeField] private float maxRecoilOffset = 5f; // Clamp recoil to prevent excessive accumulation

        private float _yaw;
        private float _pitch;
        private float _headBobTimer;
        private Vector3 _defaultPivotLocalPos;
        private Vector2 _recoilOffset;

        private void Awake()
        {
            if (cameraPivot == null && playerCamera != null)
            {
                cameraPivot = playerCamera.transform;
            }

            if (cameraPivot != null)
            {
                _defaultPivotLocalPos = cameraPivot.localPosition;
                var euler = cameraPivot.localEulerAngles;
                _pitch = euler.x;
            }

            _yaw = transform.eulerAngles.y;
        }

        public void ApplyLook(Vector2 lookDelta, float deltaTime)
        {
            // Recover from recoil over time FIRST (before applying new input)
            _recoilOffset = Vector2.Lerp(_recoilOffset, Vector2.zero, recoilReturnSpeed * deltaTime);
            
            // lookDelta from Input System is already a per-frame delta, apply directly
            _yaw += lookDelta.x;
            _pitch -= lookDelta.y;
            
            // Apply recoil offset (recoil pushes camera up = negative pitch, and to the side)
            _pitch -= _recoilOffset.y; // Subtract to push up (negative pitch = looking up)
            _yaw += _recoilOffset.x;
            
            _pitch = Mathf.Clamp(_pitch, minPitch, maxPitch);

            transform.rotation = Quaternion.Euler(0f, _yaw, 0f);
            if (cameraPivot != null)
            {
                cameraPivot.localRotation = Quaternion.Euler(_pitch, 0f, 0f);
            }
        }

        public void ApplyHeadBob(float amplitude, float frequency, float deltaTime)
        {
            if (cameraPivot == null)
            {
                return;
            }

            if (frequency <= 0f || amplitude <= 0f)
            {
                cameraPivot.localPosition = Vector3.Lerp(cameraPivot.localPosition, _defaultPivotLocalPos, 10f * deltaTime);
                return;
            }

            _headBobTimer += frequency * deltaTime;
            float offset = Mathf.Sin(_headBobTimer * Mathf.PI * 2f) * amplitude;
            var targetPos = _defaultPivotLocalPos + new Vector3(0f, offset, 0f);
            cameraPivot.localPosition = Vector3.Lerp(cameraPivot.localPosition, targetPos, 15f * deltaTime);
        }

        public void AddRecoil(Vector2 recoil)
        {
            _recoilOffset += recoil;
            // Clamp to prevent excessive accumulation
            _recoilOffset = Vector2.ClampMagnitude(_recoilOffset, maxRecoilOffset);
        }
    }
}
