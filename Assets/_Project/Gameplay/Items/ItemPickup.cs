using UnityEngine;

namespace ProjectRoguelike.Gameplay.Items
{
    /// <summary>
    /// Component attached to item pickups in the world.
    /// Handles player interaction and item collection.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public sealed class ItemPickup : MonoBehaviour
    {
        [Header("Item Data")]
        [SerializeField] private ItemData itemData;

        [Header("Pickup Settings")]
        [SerializeField] private float pickupRadius = 2f;
        [SerializeField] private bool autoPickup = false; // If true, pickup on trigger enter
        [SerializeField] private float rotationSpeed = 90f; // Visual rotation
        [SerializeField] private float bobSpeed = 2f; // Vertical bobbing
        [SerializeField] private float bobAmount = 0.2f;

        [Header("Visual")]
        [SerializeField] private GameObject visualModel;
        [SerializeField] private ParticleSystem pickupEffect;

        private Vector3 _startPosition;
        private float _bobTimer;
        private bool _isPickedUp;

        private void Awake()
        {
            _startPosition = transform.position;
            
            // Ensure collider is a trigger
            var collider = GetComponent<Collider>();
            if (collider != null)
            {
                collider.isTrigger = true;
            }

            // If no visual model assigned, use the first child or this object
            if (visualModel == null && transform.childCount > 0)
            {
                visualModel = transform.GetChild(0).gameObject;
            }
        }

        private void Update()
        {
            if (_isPickedUp)
            {
                return;
            }

            // Visual effects: rotation and bobbing
            if (visualModel != null)
            {
                visualModel.transform.Rotate(0f, rotationSpeed * Time.deltaTime, 0f);
                
                _bobTimer += Time.deltaTime * bobSpeed;
                var offset = Mathf.Sin(_bobTimer) * bobAmount;
                visualModel.transform.localPosition = new Vector3(0f, offset, 0f);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_isPickedUp || !autoPickup)
            {
                return;
            }

            if (other.CompareTag("Player"))
            {
                TryPickup(other.gameObject);
            }
        }

        /// <summary>
        /// Called by player interaction system or auto-pickup.
        /// </summary>
        public bool TryPickup(GameObject player)
        {
            if (_isPickedUp || itemData == null)
            {
                return false;
            }

            // Add to inventory first
            var inventory = player.GetComponent<Inventory>();
            if (inventory != null)
            {
                bool added = inventory.AddItem(itemData);
                if (added)
                {
                    Debug.Log($"[ItemPickup] Added {itemData.ItemName} to inventory");
                }
                else
                {
                    Debug.LogWarning($"[ItemPickup] Failed to add {itemData.ItemName} to inventory (inventory full?)");
                }
            }

            // Apply item effect (equip weapon, restore health, etc.)
            itemData.OnPickup(player);

            // Visual feedback
            if (pickupEffect != null)
            {
                var effect = Instantiate(pickupEffect, transform.position, Quaternion.identity);
                effect.Play();
                Destroy(effect.gameObject, 2f);
            }

            _isPickedUp = true;
            
            // Destroy or hide the pickup
            Destroy(gameObject);

            return true;
        }

        public void SetItemData(ItemData data)
        {
            itemData = data;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, pickupRadius);
        }
    }
}

