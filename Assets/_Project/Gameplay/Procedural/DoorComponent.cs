using System;
using UnityEngine;
using ProjectRoguelike.Procedural;

namespace ProjectRoguelike.Gameplay.Procedural
{
    /// <summary>
    /// Composant Unity attaché aux portes dans les prefabs de salles.
    /// </summary>
    public sealed class DoorComponent : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private DoorVisual visual;
        [SerializeField] private Collider interactionCollider;

        private RoomEdge _edge;
        private bool _isInteractable = false;

        public RoomEdge Edge => _edge;
        public DoorType DoorType => _edge?.DoorType ?? DoorType.Random;
        public RoomRewardData RewardPreview => _edge?.RewardPreview;
        public bool IsLocked { get; private set; } = true;
        public bool IsInteractable => _isInteractable && !IsLocked;

        public event Action<DoorComponent> OnDoorInteracted;

        private void Awake()
        {
            if (visual == null)
            {
                visual = GetComponent<DoorVisual>();
            }

            if (interactionCollider == null)
            {
                interactionCollider = GetComponent<Collider>();
            }
        }

        public void Initialize(RoomEdge edge)
        {
            _edge = edge ?? throw new System.ArgumentNullException(nameof(edge));

            if (visual != null)
            {
                visual.UpdateVisual(edge.DoorType, edge.RewardPreview);
            }

            SetLocked(true);
            SetInteractable(false);
        }

        public void SetLocked(bool locked)
        {
            IsLocked = locked;

            if (visual != null)
            {
                visual.SetGlow(!locked);
            }

            if (interactionCollider != null)
            {
                interactionCollider.enabled = !locked && _isInteractable;
            }
        }

        public void SetInteractable(bool interactable)
        {
            _isInteractable = interactable;

            if (interactionCollider != null)
            {
                interactionCollider.enabled = interactable && !IsLocked;
            }
        }

        public void OnPlayerInteract()
        {
            if (!IsInteractable)
            {
                return;
            }

            OnDoorInteracted?.Invoke(this);
        }

        public void ShowRewardPreview()
        {
            if (visual != null)
            {
                visual.SetGlow(true);
            }
        }

        public void HideRewardPreview()
        {
            if (visual != null && IsLocked)
            {
                visual.SetGlow(false);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (IsInteractable && other.CompareTag("Player"))
            {
                ShowRewardPreview();
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                HideRewardPreview();
            }
        }
    }
}

