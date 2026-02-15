#nullable enable
using System;

namespace PocketSquire.Arena.Core.PowerUps
{
    /// <summary>
    /// Power-up component that modifies a single player attribute (Str, Def, etc.).
    /// </summary>
    [Serializable]
    public class AttributeModifierComponent : PowerUpComponent
    {
        public enum AttributeType
        {
            Strength,
            Constitution,
            Magic,
            Dexterity,
            Luck,
            Defense
        }

        public AttributeType TargetAttribute { get; private set; }

        public override string UniqueKey => $"ATTR_{TargetAttribute.ToString().ToUpper()}";

        public override string DisplayName => $"{TargetAttribute} Boost {RomanNumeral(Rank)}";

        public override string Description => 
            $"Increases {TargetAttribute} by {ComputeValue(1):F0} (scales with arena level).";

        public AttributeModifierComponent(
            AttributeType targetAttribute, 
            float baseValue, 
            Rarity rarity, 
            PowerUpRank rank)
            : base(PowerUpComponentType.AttributeModifier, baseValue, rarity, rank)
        {
            TargetAttribute = targetAttribute;
        }

        /// <summary>
        /// Applies the attribute bonus to the given Attributes instance.
        /// </summary>
        public void ApplyToAttributes(Attributes attributes, int arenaLevel)
        {
            int bonus = (int)Math.Round(ComputeValue(arenaLevel));
            
            switch (TargetAttribute)
            {
                case AttributeType.Strength:
                    attributes.Strength += bonus;
                    break;
                case AttributeType.Constitution:
                    attributes.Constitution += bonus;
                    break;
                case AttributeType.Magic:
                    attributes.Magic += bonus;
                    break;
                case AttributeType.Dexterity:
                    attributes.Dexterity += bonus;
                    break;
                case AttributeType.Luck:
                    attributes.Luck += bonus;
                    break;
                case AttributeType.Defense:
                    attributes.Defense += bonus;
                    break;
            }
        }

        private static string RomanNumeral(PowerUpRank rank)
        {
            return rank switch
            {
                PowerUpRank.I => "I",
                PowerUpRank.II => "II",
                PowerUpRank.III => "III",
                _ => ""
            };
        }
    }
}
