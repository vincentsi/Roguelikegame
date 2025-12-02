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
        [SerializeField] private bool thirdPersonMode = false;
        [SerializeField] private Vector3 thirdPersonOffset = new Vector3(0f, 2f, -5f);
        [SerializeField] private KeyCode toggleCameraKey = KeyCode.V;

        private float _yaw;
        private float _pitch;
        private float _headBobTimer;
        private Vector3 _defaultPivotLocalPos;
        private Vector3 _firstPersonPos;
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
                _firstPersonPos = cameraPivot.localPosition;
                var euler = cameraPivot.localEulerAngles;
                _pitch = euler.x;
                
                // Si on démarre en 3ème personne, ajuster le pitch pour une vue de côté
                if (thirdPersonMode)
                {
                    _pitch = 15f; // Angle par défaut pour voir le personnage de côté
                }
            }

            _yaw = transform.eulerAngles.y;
            UpdateCameraPosition();
        }

        private void Update()
        {
            if (Input.GetKeyDown(toggleCameraKey))
            {
                thirdPersonMode = !thirdPersonMode;
                
                // Réinitialiser le pitch si on passe en 3ème personne
                if (thirdPersonMode && _pitch < -10f || _pitch > 60f)
                {
                    _pitch = 15f; // Angle par défaut pour voir le personnage de côté
                }
                
                UpdateCameraPosition();
            }
        }

        private void LateUpdate()
        {
            // S'assurer que la caméra 3ème personne est toujours à jour
            if (thirdPersonMode && cameraPivot != null)
            {
                UpdateThirdPersonCamera();
            }
        }

        private void UpdateCameraPosition()
        {
            if (cameraPivot == null) return;

            if (!thirdPersonMode)
            {
                cameraPivot.localPosition = _firstPersonPos;
                cameraPivot.localRotation = Quaternion.Euler(_pitch, 0f, 0f);
            }
            else
            {
                UpdateThirdPersonCamera();
            }
        }

        private void UpdateThirdPersonCamera()
        {
            if (cameraPivot == null)
            {
                return;
            }

            // En 3ème personne, calculer la position de la caméra derrière le personnage
            float distance = Mathf.Abs(thirdPersonOffset.z);
            float height = thirdPersonOffset.y;
            
            // Convertir pitch en radians pour le calcul vertical
            float pitchRad = _pitch * Mathf.Deg2Rad;
            
            // Calculer la distance horizontale et verticale
            float horizontalDistance = distance * Mathf.Cos(pitchRad);
            float verticalOffset = distance * Mathf.Sin(pitchRad);
            
            // Position derrière le personnage dans l'espace local
            // Z négatif = derrière (car le personnage regarde vers -Z dans l'espace local)
            Vector3 cameraLocalPos = new Vector3(
                0f,  // Pas de décalage latéral
                height + verticalOffset,  // Hauteur + offset vertical
                -horizontalDistance  // Distance derrière (négatif)
            );
            
            cameraPivot.localPosition = cameraLocalPos;
            
            // Faire regarder la caméra vers le personnage (position du Player dans l'espace monde)
            cameraPivot.LookAt(transform.position);
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

            if (thirdPersonMode)
            {
                // En 3ème personne, le personnage et la caméra tournent ensemble
                transform.rotation = Quaternion.Euler(0f, _yaw, 0f);
                // Mettre à jour la caméra immédiatement
                UpdateThirdPersonCamera();
            }
            else
            {
                // En 1ère personne, rotation normale
                transform.rotation = Quaternion.Euler(0f, _yaw, 0f);
                
                if (cameraPivot != null)
                {
                    cameraPivot.localRotation = Quaternion.Euler(_pitch, 0f, 0f);
                }
            }
        }

        public void ApplyHeadBob(float amplitude, float frequency, float deltaTime)
        {
            if (cameraPivot == null || thirdPersonMode)
            {
                return; // Pas de head bob en 3ème personne
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
