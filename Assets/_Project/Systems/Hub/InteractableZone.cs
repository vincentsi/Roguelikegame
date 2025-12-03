using UnityEngine;
using System;

namespace ProjectRoguelike.Systems.Hub
{
    /// <summary>
    /// Zone interactive dans le Hub - ouvre des UI ou déclenche des actions quand le player s'approche.
    /// </summary>
    public sealed class InteractableZone : MonoBehaviour
    {
        [Header("Interaction Settings")]
        [SerializeField] private float interactionRange = 3f;
        [SerializeField] private KeyCode interactKey = KeyCode.E;
        [SerializeField] private bool autoInteract = false; // Si true, interagit automatiquement sans appuyer sur E

        [Header("Visual Feedback")]
        [SerializeField] private GameObject interactionPrompt; // UI pour afficher "Appuyez sur E"
        [SerializeField] private bool showGizmo = true;

        [Header("Actions")]
        [SerializeField] private InteractableType interactableType = InteractableType.Shop;
        [SerializeField] private HubManager hubManager;

        public event Action OnPlayerEntered;
        public event Action OnPlayerExited;
        public event Action OnInteracted;

        private bool _playerInRange;
        private Transform _player;

        public enum InteractableType
        {
            Shop,
            CharacterSelection,
            NarrativeLab,
            Collection,
            StartRun,
            StartRunLeft,
            StartRunRight,
            StartRunFront,
            Custom
        }

        private void Awake()
        {
            if (hubManager == null)
            {
                hubManager = FindObjectOfType<HubManager>();
            }

            if (interactionPrompt != null)
            {
                interactionPrompt.SetActive(false);
            }
        }

        private void Update()
        {
            // Find player if not found
            if (_player == null)
            {
                var playerObj = GameObject.FindGameObjectWithTag("Player");
                if (playerObj != null)
                {
                    _player = playerObj.transform;
                }
            }

            if (_player == null)
            {
                return;
            }

            // Check distance to player
            float distance = Vector3.Distance(transform.position, _player.position);
            bool wasInRange = _playerInRange;
            _playerInRange = distance <= interactionRange;

            // Handle enter/exit
            if (_playerInRange && !wasInRange)
            {
                Debug.Log($"[InteractableZone] Player entered zone: {gameObject.name} (Type: {interactableType})");
                OnPlayerEntered?.Invoke();
                if (interactionPrompt != null)
                {
                    interactionPrompt.SetActive(true);
                }
            }
            else if (!_playerInRange && wasInRange)
            {
                Debug.Log($"[InteractableZone] Player exited zone: {gameObject.name}");
                OnPlayerExited?.Invoke();
                if (interactionPrompt != null)
                {
                    interactionPrompt.SetActive(false);
                }
            }

            // Handle interaction
            if (_playerInRange)
            {
                if (autoInteract || Input.GetKeyDown(interactKey))
                {
                    Interact();
                }
            }
        }

        public void Interact()
        {
            Debug.Log($"[InteractableZone] Interacted with {gameObject.name} (Type: {interactableType})");
            OnInteracted?.Invoke();

            if (hubManager == null)
            {
                hubManager = FindObjectOfType<HubManager>();
            }

            if (hubManager == null)
            {
                Debug.LogError("[InteractableZone] HubManager not found! Cannot interact.");
                return;
            }

            switch (interactableType)
            {
                case InteractableType.Shop:
                    hubManager.OpenShop();
                    break;

                case InteractableType.CharacterSelection:
                    hubManager.OpenCharacterSelection();
                    break;

                case InteractableType.NarrativeLab:
                    hubManager.OpenNarrativeLab();
                    break;

                case InteractableType.Collection:
                    hubManager.OpenCollection();
                    break;

                case InteractableType.StartRun:
                    Debug.Log("[InteractableZone] Starting run...");
                    hubManager.StartRun();
                    break;

                case InteractableType.StartRunLeft:
                    Debug.Log("[InteractableZone] Starting run from left door...");
                    hubManager.StartRunLeft();
                    break;

                case InteractableType.StartRunRight:
                    Debug.Log("[InteractableZone] Starting run from right door...");
                    hubManager.StartRunRight();
                    break;

                case InteractableType.StartRunFront:
                    Debug.Log("[InteractableZone] Starting run from front door...");
                    hubManager.StartRunFront();
                    break;

                case InteractableType.Custom:
                    // Custom action handled by event
                    break;
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (!showGizmo)
            {
                return;
            }

            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, interactionRange);
        }
    }
}

