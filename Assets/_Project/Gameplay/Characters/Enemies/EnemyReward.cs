using UnityEngine;
using ProjectRoguelike.Systems.Meta;

namespace ProjectRoguelike.Gameplay.Enemies
{
    /// <summary>
    /// Component that gives currency reward when the enemy dies.
    /// </summary>
    public sealed class EnemyReward : MonoBehaviour
    {
        [Header("Currency Reward")]
        [SerializeField] private int currencyReward = 10;

        /// <summary>
        /// Called when the enemy dies. Gives currency reward to the player.
        /// </summary>
        public void OnEnemyDied()
        {
            if (currencyReward <= 0)
            {
                return;
            }

            var currencyManager = FindObjectOfType<CurrencyManager>();
            if (currencyManager != null)
            {
                currencyManager.AddCurrency(currencyReward);
            }
            else
            {
                Debug.LogWarning($"[EnemyReward] CurrencyManager not found! Cannot give {currencyReward} currency.");
            }
        }
    }
}

