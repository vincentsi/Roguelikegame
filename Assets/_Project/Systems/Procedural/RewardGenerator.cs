using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using ProjectRoguelike.Levels;

namespace ProjectRoguelike.Procedural
{
    /// <summary>
    /// Génère et assigne des récompenses aux portes/edges du donjon.
    /// Style Hades: chaque porte montre clairement ce qu'elle offre.
    /// </summary>
    public sealed class RewardGenerator
    {
        private readonly LevelSeed _seed;
        private readonly LevelTheme _theme;

        // Probabilités de base pour chaque type de récompense (ajustées par profondeur)
        private readonly Dictionary<DoorType, float> _baseWeights = new()
        {
            { DoorType.Currency, 0.35f },      // 35% - Monnaie (le plus commun)
            { DoorType.Upgrade, 0.25f },       // 25% - Amélioration
            { DoorType.Weapon, 0.15f },        // 15% - Nouvelle arme
            { DoorType.Consumable, 0.10f },    // 10% - Consommables
            { DoorType.Elite, 0.08f },         // 8% - Combat élite
            { DoorType.Shop, 0.05f },          // 5% - Boutique
            { DoorType.Event, 0.02f },         // 2% - Événement spécial
        };

        public RewardGenerator(LevelSeed seed, LevelTheme theme)
        {
            _seed = seed ?? throw new System.ArgumentNullException(nameof(seed));
            _theme = theme ?? throw new System.ArgumentNullException(nameof(theme));
        }

        /// <summary>
        /// Assigne des récompenses à toutes les edges d'une salle.
        /// S'assure que les choix sont variés et intéressants.
        /// </summary>
        public void AssignRewardsToRoomExits(RoomNode room, List<RoomEdge> edges, int currentDepth, int totalDepth)
        {
            if (edges == null || edges.Count == 0) return;

            // Créer un pool de types disponibles (sans doublon si possible)
            var availableTypes = new List<DoorType>();
            var usedTypes = new HashSet<DoorType>();

            foreach (var edge in edges)
            {
                // Ne pas modifier les portes spéciales (Boss, déjà assignées)
                if (edge.DoorType == DoorType.Boss)
                {
                    AssignBossReward(edge);
                    continue;
                }

                // Sélectionner un type de porte qui n'a pas encore été utilisé
                DoorType selectedType = SelectDoorTypeWeighted(currentDepth, totalDepth, usedTypes);
                edge.DoorType = selectedType;
                usedTypes.Add(selectedType);

                // Générer les données de récompense
                GeneratedReward reward = GenerateRewardData(selectedType, currentDepth, totalDepth);
                edge.RewardPreview = reward;
            }
        }

        /// <summary>
        /// Sélectionne un type de porte en utilisant des poids pondérés.
        /// Les poids changent selon la profondeur (plus profond = meilleures récompenses).
        /// </summary>
        private DoorType SelectDoorTypeWeighted(int currentDepth, int totalDepth, HashSet<DoorType> usedTypes)
        {
            // Calculer le pourcentage de progression dans le donjon
            float progressRatio = totalDepth > 0 ? (float)currentDepth / totalDepth : 0f;

            // Ajuster les poids selon la progression
            var adjustedWeights = new Dictionary<DoorType, float>();

            foreach (var kvp in _baseWeights)
            {
                DoorType doorType = kvp.Key;
                float baseWeight = kvp.Value;

                // Si déjà utilisé, réduire drastiquement le poids (mais pas 0 pour éviter les blocages)
                if (usedTypes.Contains(doorType))
                {
                    adjustedWeights[doorType] = baseWeight * 0.1f;
                    continue;
                }

                // Ajustement basé sur la profondeur
                float depthMultiplier = 1f;

                switch (doorType)
                {
                    case DoorType.Currency:
                        // Moins commun en profondeur
                        depthMultiplier = 1f - (progressRatio * 0.3f);
                        break;

                    case DoorType.Upgrade:
                        // Plus commun en profondeur
                        depthMultiplier = 1f + (progressRatio * 0.5f);
                        break;

                    case DoorType.Weapon:
                        // Plus commun au milieu
                        depthMultiplier = 1f + (progressRatio * 0.3f);
                        break;

                    case DoorType.Elite:
                        // Seulement après 30% de progression
                        depthMultiplier = progressRatio < 0.3f ? 0.1f : 1f + progressRatio;
                        break;

                    case DoorType.Shop:
                        // Plus rare au début
                        depthMultiplier = progressRatio < 0.2f ? 0.5f : 1.2f;
                        break;

                    case DoorType.Event:
                        // Rare mais possible partout
                        depthMultiplier = 1f;
                        break;

                    case DoorType.Consumable:
                        // Constant
                        depthMultiplier = 1f;
                        break;
                }

                adjustedWeights[doorType] = baseWeight * depthMultiplier;
            }

            // Sélection pondérée
            float totalWeight = adjustedWeights.Values.Sum();
            float randomValue = _seed.NextFloat() * totalWeight;

            float cumulativeWeight = 0f;
            foreach (var kvp in adjustedWeights)
            {
                cumulativeWeight += kvp.Value;
                if (randomValue <= cumulativeWeight)
                {
                    return kvp.Key;
                }
            }

            // Fallback (ne devrait jamais arriver)
            return DoorType.Currency;
        }

        /// <summary>
        /// Génère les données de récompense pour un type de porte donné.
        /// </summary>
        private GeneratedReward GenerateRewardData(DoorType doorType, int currentDepth, int totalDepth)
        {
            float progressRatio = totalDepth > 0 ? (float)currentDepth / totalDepth : 0f;

            var reward = new GeneratedReward();

            switch (doorType)
            {
                case DoorType.Currency:
                    reward.RewardType = RoomRewardType.Currency;
                    reward.RewardName = "Gold";
                    reward.RewardIcon = null; // TODO: Assigner icône
                    reward.RewardColor = new Color(1f, 0.84f, 0f); // Or
                    // Montant augmente avec la profondeur: 50-150 base
                    reward.CurrencyAmount = Mathf.RoundToInt(50 + (progressRatio * 100) + _seed.NextInt(-10, 10));
                    break;

                case DoorType.Upgrade:
                    reward.RewardType = RoomRewardType.Upgrade;
                    reward.RewardName = "Power Upgrade";
                    reward.RewardColor = new Color(0.5f, 0.5f, 1f); // Bleu
                    break;

                case DoorType.Weapon:
                    reward.RewardType = RoomRewardType.Weapon;
                    reward.RewardName = "New Weapon";
                    reward.RewardColor = new Color(1f, 0.3f, 0.3f); // Rouge
                    break;

                case DoorType.Consumable:
                    reward.RewardType = RoomRewardType.Consumable;
                    reward.RewardName = "Health Kit";
                    reward.RewardColor = new Color(0.3f, 1f, 0.3f); // Vert
                    break;

                case DoorType.Elite:
                    reward.RewardType = RoomRewardType.EliteReward;
                    reward.RewardName = "Elite Challenge";
                    reward.RewardColor = new Color(1f, 0.5f, 0f); // Orange
                    // Elite donne plus de monnaie
                    reward.CurrencyAmount = Mathf.RoundToInt(100 + (progressRatio * 150));
                    break;

                case DoorType.Shop:
                    reward.RewardType = RoomRewardType.Shop;
                    reward.RewardName = "Shop";
                    reward.RewardColor = new Color(1f, 1f, 1f); // Blanc
                    break;

                case DoorType.Event:
                    reward.RewardType = RoomRewardType.EventReward;
                    reward.RewardName = "Mystery Event";
                    reward.RewardColor = new Color(0.8f, 0.3f, 1f); // Violet
                    break;

                case DoorType.Random:
                    reward.RewardType = RoomRewardType.Random;
                    reward.RewardName = "???";
                    reward.RewardColor = Color.gray;
                    break;

                default:
                    reward.RewardType = RoomRewardType.Currency;
                    reward.RewardName = "Gold";
                    reward.RewardColor = new Color(1f, 0.84f, 0f);
                    reward.CurrencyAmount = 50;
                    break;
            }

            return reward;
        }

        /// <summary>
        /// Assigne une récompense de boss (toujours spéciale).
        /// </summary>
        private void AssignBossReward(RoomEdge edge)
        {
            var reward = new GeneratedReward
            {
                RewardType = RoomRewardType.BossReward,
                RewardName = "Boss Fight",
                RewardColor = new Color(1f, 0f, 0f), // Rouge intense
                CurrencyAmount = 500 // Grosse récompense
            };

            edge.RewardPreview = reward;
        }

        /// <summary>
        /// Valide qu'une edge a bien une récompense assignée.
        /// </summary>
        public static bool ValidateEdgeReward(RoomEdge edge)
        {
            if (edge == null) return false;
            if (edge.RewardPreview == null) return false;
            if (string.IsNullOrEmpty(edge.RewardPreview.RewardName)) return false;

            return true;
        }
    }
}
