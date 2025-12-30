using UnityEngine;
using TMPro;
using ProjectRoguelike.Procedural;

namespace ProjectRoguelike.Gameplay.Procedural
{
    /// <summary>
    /// Composant visuel pour afficher le type de porte et la récompense.
    /// </summary>
    public sealed class DoorVisual : MonoBehaviour
    {
        [Header("Visual Components")]
        [SerializeField] private SpriteRenderer iconRenderer;
        [SerializeField] private TextMeshPro rewardText;
        [SerializeField] private ParticleSystem rewardParticles;
        [SerializeField] private Light doorLight;

        [Header("Visual Settings")]
        [SerializeField] private Color currencyColor = Color.yellow;
        [SerializeField] private Color upgradeColor = Color.blue;
        [SerializeField] private Color weaponColor = Color.red;
        [SerializeField] private Color eliteColor = new Color(1f, 0.84f, 0f); // Or
        [SerializeField] private Color shopColor = Color.green;
        [SerializeField] private Color eventColor = new Color(0.5f, 0f, 0.5f); // Violet
        [SerializeField] private Color randomColor = Color.white;
        [SerializeField] private Color bossColor = Color.black;

        private DoorType _currentType;
        private bool _isGlowing = false;

        public void UpdateVisual(DoorType type, RoomRewardData reward)
        {
            _currentType = type;

            Color color = GetColorForType(type);
            string text = GetTextForType(type, reward);

            if (iconRenderer != null && reward != null && reward.RewardIcon != null)
            {
                iconRenderer.sprite = reward.RewardIcon;
                iconRenderer.color = reward.RewardColor;
            }

            if (rewardText != null)
            {
                rewardText.text = text;
                rewardText.color = color;
            }

            if (doorLight != null)
            {
                doorLight.color = color;
                doorLight.enabled = _isGlowing;
            }

            if (rewardParticles != null)
            {
                var main = rewardParticles.main;
                main.startColor = color;
            }
        }

        public void SetGlow(bool active)
        {
            _isGlowing = active;

            if (doorLight != null)
            {
                doorLight.enabled = active;
            }

            if (rewardParticles != null)
            {
                if (active && !rewardParticles.isPlaying)
                {
                    rewardParticles.Play();
                }
                else if (!active && rewardParticles.isPlaying)
                {
                    rewardParticles.Stop();
                }
            }
        }

        public void PlayOpenAnimation()
        {
            // À implémenter avec animation si nécessaire
            SetGlow(false);
        }

        public void PlayCloseAnimation()
        {
            // À implémenter avec animation si nécessaire
        }

        private Color GetColorForType(DoorType type)
        {
            return type switch
            {
                DoorType.Currency => currencyColor,
                DoorType.Upgrade => upgradeColor,
                DoorType.Weapon => weaponColor,
                DoorType.Elite => eliteColor,
                DoorType.Shop => shopColor,
                DoorType.Event => eventColor,
                DoorType.Random => randomColor,
                DoorType.Boss => bossColor,
                _ => Color.white
            };
        }

        private string GetTextForType(DoorType type, RoomRewardData reward)
        {
            if (reward != null && !string.IsNullOrEmpty(reward.RewardName))
            {
                return reward.RewardName;
            }

            return type switch
            {
                DoorType.Currency => "Currency",
                DoorType.Upgrade => "Upgrade",
                DoorType.Weapon => "Weapon",
                DoorType.Elite => "Elite",
                DoorType.Shop => "Shop",
                DoorType.Event => "Event",
                DoorType.Random => "Random",
                DoorType.Boss => "Boss",
                _ => "Door"
            };
        }
    }
}

