#nullable enable
using System;

namespace PocketSquire.Arena.Core.PowerUps
{
    /// <summary>
    /// Power-up component that debuffs monsters at the start of each fight.
    /// </summary>
    [Serializable]
    public class MonsterDebuffComponent : PowerUpComponent
    {
        public enum DebuffType
        {
            Strength,
            Constitution,
            Intelligence,
            Agility,
            Luck,
            Defense
        }

        public DebuffType TargetAttribute { get; private set; }

        public override string UniqueKey => $"DEBUFF_{TargetAttribute.ToString().ToUpper()}";

        public override string DisplayName => $"Monster {TargetAttribute} Curse {RomanNumeral(Rank)}";

        public override string Description => 
            $"Reduces monster {TargetAttribute} by {ComputeValue(1):F0} at fight start (scales with arena level).";

        public MonsterDebuffComponent(
            DebuffType targetAttribute, 
            float baseValue, 
            Rarity rarity, 
            PowerUpRank rank)
            : base(PowerUpComponentType.MonsterDebuff, baseValue, rarity, rank)
        {
            TargetAttribute = targetAttribute;
        }

        public override void ApplyToMonster(Monster monster, int arenaLevel)
        {
            int debuff = (int)Math.Round(ComputeValue(arenaLevel));
            
            switch (TargetAttribute)
            {
                case DebuffType.Strength:
                    monster.Attributes.Strength = Math.Max(1, monster.Attributes.Strength - debuff);
                    break;
                case DebuffType.Constitution:
                    monster.Attributes.Constitution = Math.Max(1, monster.Attributes.Constitution - debuff);
                    break;
                case DebuffType.Intelligence:
                    monster.Attributes.Intelligence = Math.Max(1, monster.Attributes.Intelligence - debuff);
                    break;
                case DebuffType.Agility:
                    monster.Attributes.Agility = Math.Max(1, monster.Attributes.Agility - debuff);
                    break;
                case DebuffType.Luck:
                    monster.Attributes.Luck = Math.Max(1, monster.Attributes.Luck - debuff);
                    break;
                case DebuffType.Defense:
                    monster.Attributes.Defense = Math.Max(1, monster.Attributes.Defense - debuff);
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
