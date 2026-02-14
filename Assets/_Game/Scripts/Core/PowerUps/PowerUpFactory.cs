#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;

namespace PocketSquire.Arena.Core.PowerUps
{
    /// <summary>
    /// Factory for generating procedural power-up choices with context-aware weighting.
    /// </summary>
    public static class PowerUpFactory
    {
        public class PowerUpGenerationContext
        {
            public int ArenaLevel { get; set; }
            public int PlayerLuck { get; set; }
            public float PlayerHealthPercent { get; set; }
            public PowerUpCollection OwnedPowerUps { get; set; } = new();
        }

        private class ComponentTemplate
        {
            public Func<Rarity, PowerUpRank, PowerUpComponent> Factory { get; set; } = null!;
            public string UniqueKey { get; set; } = string.Empty;
            public float BaseWeight { get; set; }
        }

        private static readonly List<ComponentTemplate> _templates = new()
        {
            // Attribute Modifiers
            new() { UniqueKey = "ATTR_STRENGTH", BaseWeight = 10f, 
                Factory = (r, rk) => new AttributeModifierComponent(AttributeModifierComponent.AttributeType.Strength, 2f, r, rk) },
            new() { UniqueKey = "ATTR_CONSTITUTION", BaseWeight = 10f, 
                Factory = (r, rk) => new AttributeModifierComponent(AttributeModifierComponent.AttributeType.Constitution, 2f, r, rk) },
            new() { UniqueKey = "ATTR_INTELLIGENCE", BaseWeight = 10f, 
                Factory = (r, rk) => new AttributeModifierComponent(AttributeModifierComponent.AttributeType.Intelligence, 2f, r, rk) },
            new() { UniqueKey = "ATTR_AGILITY", BaseWeight = 10f, 
                Factory = (r, rk) => new AttributeModifierComponent(AttributeModifierComponent.AttributeType.Agility, 2f, r, rk) },
            new() { UniqueKey = "ATTR_LUCK", BaseWeight = 8f, 
                Factory = (r, rk) => new AttributeModifierComponent(AttributeModifierComponent.AttributeType.Luck, 1f, r, rk) },
            new() { UniqueKey = "ATTR_DEFENSE", BaseWeight = 10f, 
                Factory = (r, rk) => new AttributeModifierComponent(AttributeModifierComponent.AttributeType.Defense, 2f, r, rk) },
            
            // Loot Modifiers
            new() { UniqueKey = "LOOT_GOLD", BaseWeight = 12f, 
                Factory = (r, rk) => new LootModifierComponent(LootModifierComponent.LootType.Gold, 5f, r, rk) },
            new() { UniqueKey = "LOOT_EXPERIENCE", BaseWeight = 12f, 
                Factory = (r, rk) => new LootModifierComponent(LootModifierComponent.LootType.Experience, 5f, r, rk) },
            
            // Utility
            new() { UniqueKey = "UTIL_PARTIALHEAL", BaseWeight = 8f, 
                Factory = (r, rk) => new UtilityComponent(UtilityComponent.UtilityType.PartialHeal, 10f, r, rk) },
            
            // Monster Debuffs
            new() { UniqueKey = "DEBUFF_STRENGTH", BaseWeight = 7f, 
                Factory = (r, rk) => new MonsterDebuffComponent(MonsterDebuffComponent.DebuffType.Strength, 1f, r, rk) },
            new() { UniqueKey = "DEBUFF_DEFENSE", BaseWeight = 7f, 
                Factory = (r, rk) => new MonsterDebuffComponent(MonsterDebuffComponent.DebuffType.Defense, 1f, r, rk) },
        };

        /// <summary>
        /// Generates exactly 3 power-up choices based on context.
        /// </summary>
        public static List<PowerUp> Generate(PowerUpGenerationContext context, Random rng)
        {
            var choices = new List<PowerUp>();
            var usedKeys = new HashSet<string>();

            for (int i = 0; i < 3; i++)
            {
                var powerUp = GenerateSinglePowerUp(context, rng, usedKeys);
                choices.Add(powerUp);
                usedKeys.Add(powerUp.UniqueKey);
            }

            return choices;
        }

        private static PowerUp GenerateSinglePowerUp(
            PowerUpGenerationContext context, 
            Random rng, 
            HashSet<string> usedKeys)
        {
            const int maxRerollAttempts = 10;

            for (int attempt = 0; attempt < maxRerollAttempts; attempt++)
            {
                // 1. Select a template using weighted random
                var template = SelectWeightedTemplate(context, rng, usedKeys);
                
                // 2. Roll rarity based on luck
                var rarity = PowerUpScaling.RollRarity(context.PlayerLuck, rng);
                
                // 3. Determine rank (new or upgrade)
                PowerUpRank rank = PowerUpRank.I;
                if (context.OwnedPowerUps.Has(template.UniqueKey))
                {
                    var currentRank = context.OwnedPowerUps.GetRank(template.UniqueKey);
                    if (currentRank == PowerUpRank.III)
                    {
                        // Already maxed, try reroll
                        continue;
                    }
                    // Rank up
                    rank = (PowerUpRank)((int)currentRank! + 1);
                }
                
                // 4. Create the component
                var component = template.Factory(rarity, rank);
                return new PowerUp(component);
            }

            // Fallback: Single Coin power-up
            return CreateCoinFallback();
        }

        private static ComponentTemplate SelectWeightedTemplate(
            PowerUpGenerationContext context, 
            Random rng, 
            HashSet<string> usedKeys)
        {
            // Build weighted list with context-aware boosts
            var weights = new List<float>();
            var eligibleTemplates = new List<ComponentTemplate>();

            foreach (var template in _templates)
            {
                // Skip if already used in this generation batch
                if (usedKeys.Contains(template.UniqueKey))
                    continue;

                float weight = template.BaseWeight;

                // Context-aware boost: low health → boost heal
                if (template.UniqueKey == "UTIL_PARTIALHEAL" && context.PlayerHealthPercent < 0.25f)
                {
                    weight *= 3f;
                }

                weights.Add(weight);
                eligibleTemplates.Add(template);
            }

            // If no eligible templates (shouldn't happen), fallback to first
            if (eligibleTemplates.Count == 0)
                return _templates[0];

            // Weighted random selection
            float totalWeight = weights.Sum();
            float roll = (float)rng.NextDouble() * totalWeight;
            float cumulative = 0f;

            for (int i = 0; i < eligibleTemplates.Count; i++)
            {
                cumulative += weights[i];
                if (roll < cumulative)
                    return eligibleTemplates[i];
            }

            return eligibleTemplates[^1];
        }

        private static PowerUp CreateCoinFallback()
        {
            var component = new LootModifierComponent(
                LootModifierComponent.LootType.Gold,
                1f, // +1 flat gold
                Rarity.Common,
                PowerUpRank.I,
                isFlatBonus: true
            );
            return new PowerUp(component);
        }
    }
}
