using UnityEngine;

namespace ProjectRoguelike.Gameplay.Player
{
    /// <summary>
    /// Fait tourner le modèle visuel vers la direction du mouvement pour que l'animation soit orientée correctement.
    /// </summary>
    public sealed class ModelRotationController : MonoBehaviour
    {
        [SerializeField] private Transform modelTransform; // Le transform du modèle enfant
        [SerializeField] private float rotationSpeed = 10f; // Vitesse de rotation
        [SerializeField] private float minMovementSpeed = 0.1f; // Vitesse minimale pour tourner
        
        private CharacterController _controller;
        private Vector3 _lastMovementDirection;

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();
            
            // Si modelTransform n'est pas assigné, cherche dans les enfants
            if (modelTransform == null)
            {
                modelTransform = GetComponentInChildren<SkinnedMeshRenderer>()?.transform;
                if (modelTransform == null)
                {
                    modelTransform = GetComponentInChildren<MeshRenderer>()?.transform;
                }
            }
        }

        private void Update()
        {
            if (modelTransform == null || _controller == null) return;

            // Obtenir la direction du mouvement
            Vector3 velocity = _controller.velocity;
            velocity.y = 0f; // Ignorer le mouvement vertical
            
            // Si on bouge assez vite, faire tourner le modèle
            if (velocity.magnitude > minMovementSpeed)
            {
                // Calculer la rotation cible
                Quaternion targetRotation = Quaternion.LookRotation(velocity.normalized);
                
                // Interpoler vers la rotation cible
                modelTransform.rotation = Quaternion.Slerp(
                    modelTransform.rotation, 
                    targetRotation, 
                    rotationSpeed * Time.deltaTime
                );
            }
        }
    }
}

