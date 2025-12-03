using ProjectRoguelike.Gameplay.Player;
using ProjectRoguelike.Gameplay.Weapons;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_TEXTMESHPRO
using TMPro;
#endif

namespace ProjectRoguelike.UI
{
    public sealed class HudController : MonoBehaviour
    {
        [SerializeField] private PlayerStatsComponent stats;
        [SerializeField] private WeaponController weaponController;
        [SerializeField] private Slider healthSlider;
        [SerializeField] private Slider staminaSlider;
        
        [Header("Ammo Display")]
        [Tooltip("Drag the AmmoLabel GameObject from Hierarchy here")]
        [SerializeField] private GameObject ammoLabelGameObject;
        
        [Header("Crosshair")]
        [SerializeField] private UnityEngine.UI.Image crosshair;
        [SerializeField] private Color crosshairReadyColor = Color.white;
        [SerializeField] private Color crosshairFireColor = Color.cyan;
        [SerializeField] private float crosshairFlashDuration = 0.12f;

        private float _crosshairTimer;

        private void OnEnable()
        {
            if (stats != null)
            {
                stats.OnHealthChanged += HandleHealthChanged;
                stats.OnStaminaChanged += HandleStaminaChanged;
                HandleHealthChanged(stats.CurrentHealth, stats.MaxHealth);
                HandleStaminaChanged(stats.CurrentStamina, stats.MaxStamina);
            }

            if (weaponController != null)
            {
                weaponController.OnAmmoChanged += HandleAmmoChanged;
                if (weaponController.TryGetAmmoSnapshot(out var current, out var mag, out var reserve))
                {
                    HandleAmmoChanged(current, mag, reserve);
                }
            }
            // WeaponController is optional - will be assigned when player spawns
        }

        private void OnDisable()
        {
            if (stats != null)
            {
                stats.OnHealthChanged -= HandleHealthChanged;
                stats.OnStaminaChanged -= HandleStaminaChanged;
            }

            if (weaponController != null)
            {
                weaponController.OnAmmoChanged -= HandleAmmoChanged;
            }
        }

        private void Update()
        {
            if (crosshair == null)
            {
                return;
            }

            if (_crosshairTimer > 0f)
            {
                _crosshairTimer -= Time.deltaTime;
                float t = Mathf.Clamp01(_crosshairTimer / crosshairFlashDuration);
                if (crosshair != null)
                {
                    crosshair.color = Color.Lerp(crosshairReadyColor, crosshairFireColor, t);
                }
            }
            else
            {
                if (crosshair != null)
                {
                    crosshair.color = crosshairReadyColor;
                }
            }
        }

        private void HandleHealthChanged(float current, float max)
        {
            if (healthSlider != null)
            {
                healthSlider.maxValue = max;
                healthSlider.value = current;
            }
        }

        private void HandleStaminaChanged(float current, float max)
        {
            if (staminaSlider != null)
            {
                staminaSlider.maxValue = max;
                staminaSlider.value = current;
            }
        }

        private void HandleAmmoChanged(int current, int mag, int reserve)
        {
            Debug.Log($"[HudController] HandleAmmoChanged called: {current} / {reserve}");
            
            if (ammoLabelGameObject == null)
            {
                Debug.LogError("[HudController] AmmoLabel GameObject is not assigned in Inspector!");
                return;
            }

            // Format: "30 / 90" (current in magazine / reserve)
            string ammoText = $"{current} / {reserve}";
            Debug.Log($"[HudController] Setting ammo text to: {ammoText}");

#if UNITY_TEXTMESHPRO
            var tmpText = ammoLabelGameObject.GetComponent<TMPro.TMP_Text>();
            if (tmpText != null)
            {
                tmpText.text = ammoText;
                _crosshairTimer = crosshairFlashDuration;
                Debug.Log($"[HudController] Updated TMP_Text successfully");
                return;
            }
            Debug.LogWarning($"[HudController] TMP_Text component not found on {ammoLabelGameObject.name}");
#endif
            var text = ammoLabelGameObject.GetComponent<UnityEngine.UI.Text>();
            if (text != null)
            {
                text.text = ammoText;
                _crosshairTimer = crosshairFlashDuration;
                Debug.Log($"[HudController] Updated Text successfully");
            }
            else
            {
                Debug.LogError($"[HudController] No Text or TMP_Text component found on {ammoLabelGameObject.name}! Make sure the GameObject has a Text or TextMeshPro component.");
            }
        }
    }
}
