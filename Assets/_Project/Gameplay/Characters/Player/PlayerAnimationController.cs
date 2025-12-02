using UnityEngine;

namespace ProjectRoguelike.Gameplay.Player
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class PlayerAnimationController : MonoBehaviour
    {
        [SerializeField] private Animator animatorOnModel; // Assigner manuellement le modèle enfant
        private Animator _animator;
        private CharacterController _controller;
        private Vector3 _lastPosition;
        private float _speed;

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();
            _lastPosition = transform.position;
            
            // Chercher l'Animator sur ce GameObject ou sur les enfants
            if (animatorOnModel != null)
            {
                _animator = animatorOnModel;
            }
            else
            {
                _animator = GetComponent<Animator>();
                if (_animator == null)
                {
                    // Chercher dans les enfants (le modèle visuel)
                    _animator = GetComponentInChildren<Animator>();
                }
            }
            
            if (_animator == null)
            {
                Debug.LogWarning("[PlayerAnimationController] No Animator found! Please assign one or add it to the model child.");
            }
        }

        private void Update()
        {
            if (_animator == null || _controller == null) return;

            // Calculer la vitesse de déplacement (utiliser la vitesse du CharacterController)
            Vector3 velocity = _controller.velocity;
            velocity.y = 0f; // Ignorer le mouvement vertical
            _speed = velocity.magnitude;

            // Mettre à jour l'Animator
            if (HasParameter("Speed"))
            {
                _animator.SetFloat("Speed", _speed);
                // Debug: afficher la vitesse toutes les secondes
                if (Time.frameCount % 60 == 0)
                {
                    Debug.Log($"[PlayerAnimation] Speed: {_speed:F2}, Current State: {GetCurrentStateName()}");
                }
            }
            else if (HasParameter("IsMoving"))
            {
                _animator.SetBool("IsMoving", _speed > 0.1f);
            }
            else
            {
                // Aucun paramètre trouvé - debug
                if (Time.frameCount % 120 == 0)
                {
                    Debug.LogWarning("[PlayerAnimation] No 'Speed' or 'IsMoving' parameter found in Animator!");
                }
            }
        }

        private string GetCurrentStateName()
        {
            if (_animator == null) return "None";
            AnimatorStateInfo stateInfo = _animator.GetCurrentAnimatorStateInfo(0);
            return _animator.GetCurrentAnimatorClipInfo(0).Length > 0 
                ? _animator.GetCurrentAnimatorClipInfo(0)[0].clip.name 
                : "No Clip";
        }

        private bool HasParameter(string paramName)
        {
            if (_animator == null) return false;
            
            foreach (AnimatorControllerParameter param in _animator.parameters)
            {
                if (param.name == paramName)
                {
                    return true;
                }
            }
            return false;
        }
    }
}

