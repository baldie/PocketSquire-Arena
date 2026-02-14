using NUnit.Framework;
using PocketSquire.Arena.Core.PowerUps;

namespace PocketSquire.Arena.Core.Tests.PowerUps
{
    [TestFixture]
    public class PowerUpComponentTests
    {
        [Test]
        public void AttributeModifier_Apply_IncreasesPlayerStat()
        {
            var component = new AttributeModifierComponent(
                AttributeModifierComponent.AttributeType.Strength,
                5f,
                Rarity.Common,
                PowerUpRank.I
            );

            var attributes = new Attributes { Strength = 10 };
            component.ApplyToAttributes(attributes, 1);

            // At level 1: 5 × 1.0 × 1.0 × (1 + ln(2)) ≈ 8.46, rounds to 8
            Assert.That(attributes.Strength, Is.GreaterThan(10));
        }

        [Test]
        public void MonsterDebuff_Apply_ReducesMonsterStat_ClampedAtOne()
        {
            var component = new MonsterDebuffComponent(
                MonsterDebuffComponent.DebuffType.Defense,
                100f, // Large debuff
                Rarity.Legendary,
                PowerUpRank.III
            );

            var monster = new Monster("Test", 100, 100, new Attributes { Defense = 5 });
            component.ApplyToMonster(monster, 1);

            // Should clamp at 1, not go below
            Assert.That(monster.Attributes.Defense, Is.EqualTo(1));
        }

        [Test]
        public void UtilityHeal_Apply_RestoresPercentHealth()
        {
            var component = new UtilityComponent(
                UtilityComponent.UtilityType.PartialHeal,
                10f, // 10% base
                Rarity.Common,
                PowerUpRank.I
            );

            var player = new Player("Test", 50, 100, new Attributes(), Player.CharGender.m);
            int healthBefore = player.Health;
            component.ApplyToPlayer(player, 1);

            // Should heal some amount
            Assert.That(player.Health, Is.GreaterThan(healthBefore));
            Assert.That(player.Health, Is.LessThanOrEqualTo(player.MaxHealth));
        }

        [Test]
        public void LootModifier_GetBonusValue_ReturnsPercentage()
        {
            var component = new LootModifierComponent(
                LootModifierComponent.LootType.Gold,
                5f,
                Rarity.Rare,
                PowerUpRank.II
            );

            float bonus = component.GetBonusValue(1);

            // Should return scaled percentage
            Assert.That(bonus, Is.GreaterThan(0f));
        }

        [Test]
        public void LootModifier_FlatBonus_ReturnsBaseValue()
        {
            var component = new LootModifierComponent(
                LootModifierComponent.LootType.Gold,
                1f,
                Rarity.Common,
                PowerUpRank.I,
                isFlatBonus: true
            );

            float bonus = component.GetBonusValue(1);

            // Flat bonus should return base value regardless of level
            Assert.That(bonus, Is.EqualTo(1f));
        }

        [Test]
        public void PowerUp_IncrementRank_UpgradesRank()
        {
            var component = new AttributeModifierComponent(
                AttributeModifierComponent.AttributeType.Luck,
                2f,
                Rarity.Common,
                PowerUpRank.I
            );
            var powerUp = new PowerUp(component);

            Assert.That(powerUp.Rank, Is.EqualTo(PowerUpRank.I));
            
            powerUp.IncrementRank();
            Assert.That(powerUp.Rank, Is.EqualTo(PowerUpRank.II));
            
            powerUp.IncrementRank();
            Assert.That(powerUp.Rank, Is.EqualTo(PowerUpRank.III));
            
            // Should cap at III
            powerUp.IncrementRank();
            Assert.That(powerUp.Rank, Is.EqualTo(PowerUpRank.III));
        }

        [Test]
        public void PowerUpCollection_Add_RanksUpExisting()
        {
            var collection = new PowerUpCollection();
            var component = new AttributeModifierComponent(
                AttributeModifierComponent.AttributeType.Strength,
                2f,
                Rarity.Common,
                PowerUpRank.I
            );
            var powerUp1 = new PowerUp(component);
            var powerUp2 = new PowerUp(new AttributeModifierComponent(
                AttributeModifierComponent.AttributeType.Strength,
                2f,
                Rarity.Rare,
                PowerUpRank.I
            ));

            collection.Add(powerUp1);
            Assert.That(collection.Count, Is.EqualTo(1));
            Assert.That(collection.GetRank("ATTR_STRENGTH"), Is.EqualTo(PowerUpRank.I));

            collection.Add(powerUp2);
            Assert.That(collection.Count, Is.EqualTo(1)); // Still 1, ranked up
            Assert.That(collection.GetRank("ATTR_STRENGTH"), Is.EqualTo(PowerUpRank.II));
        }
    }
}
