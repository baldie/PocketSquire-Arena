using NUnit.Framework;
using PocketSquire.Arena.Core;
using PocketSquire.Arena.Core.PowerUps;
using System;

namespace PocketSquire.Arena.Tests
{
    [TestFixture]
    public class CombatCalculatorTests
    {
        [SetUp]
        public void Setup()
        {
            GameState.CurrentRun = null;
        }

        private static Entity MakeEntity(int str, int dex, int mag, int luck, int def = 0)
        {
            return new Monster("Test", 100, 100, new Attributes
            {
                Strength = str,
                Dexterity = dex,
                Magic = mag,
                Luck = luck,
                Defense = def
            });
        }

        [Test]
        public void GetAttackStyle_ReturnsExpectedMappings()
        {
            Assert.That(PlayerClass.GetAttackStyle(PlayerClass.ClassName.Fighter), Is.EqualTo(PlayerClass.AttackStyle.Physical));
            Assert.That(PlayerClass.GetAttackStyle(PlayerClass.ClassName.Archer), Is.EqualTo(PlayerClass.AttackStyle.Ranged));
            Assert.That(PlayerClass.GetAttackStyle(PlayerClass.ClassName.Wizard), Is.EqualTo(PlayerClass.AttackStyle.Magic));
            Assert.That(PlayerClass.GetAttackStyle(PlayerClass.ClassName.Ranger), Is.EqualTo(PlayerClass.AttackStyle.HybridPhysRanged));
            Assert.That(PlayerClass.GetAttackStyle(PlayerClass.ClassName.Warden), Is.EqualTo(PlayerClass.AttackStyle.HybridMagicRanged));
            Assert.That(PlayerClass.GetAttackStyle(PlayerClass.ClassName.Paladin), Is.EqualTo(PlayerClass.AttackStyle.HybridPhysMagic));
        }

        [Test]
        public void CalculateBaseDamage_UsesExpectedStatWeighting()
        {
            var actor = MakeEntity(str: 10, dex: 9, mag: 8, luck: 7);

            Assert.That(CombatCalculator.CalculateBaseDamage(actor, PlayerClass.AttackStyle.Physical), Is.EqualTo(11));
            Assert.That(CombatCalculator.CalculateBaseDamage(actor, PlayerClass.AttackStyle.Ranged), Is.EqualTo(10));
            Assert.That(CombatCalculator.CalculateBaseDamage(actor, PlayerClass.AttackStyle.Magic), Is.EqualTo(8));
            Assert.That(CombatCalculator.CalculateBaseDamage(actor, PlayerClass.AttackStyle.HybridPhysRanged), Is.EqualTo(9));
            Assert.That(CombatCalculator.CalculateBaseDamage(actor, PlayerClass.AttackStyle.HybridMagicRanged), Is.EqualTo(8));
            Assert.That(CombatCalculator.CalculateBaseDamage(actor, PlayerClass.AttackStyle.HybridPhysMagic), Is.EqualTo(9));
        }

        [Test]
        public void ResolveAttack_SpecialAttackGetsBonusDamageAndHitPenalty()
        {
            var actor = MakeEntity(str: 10, dex: 12, mag: 0, luck: 0);
            var target = MakeEntity(str: 0, dex: 5, mag: 0, luck: 0, def: 10);

            var normal = CombatCalculator.ResolveAttack(actor, target, false, new Random(0), guaranteedHit: true);
            var special = CombatCalculator.ResolveAttack(actor, target, true, new Random(0), guaranteedHit: true);

            Assert.That(special.RawDamage, Is.GreaterThan(normal.RawDamage));
            Assert.That(special.Damage, Is.GreaterThan(normal.Damage));
            Assert.That(normal.HitChance - special.HitChance, Is.EqualTo(CombatCalculator.SpecialAttackHitPenalty));
        }

        [Test]
        public void CalculateDefendDamageReduction_ScalesWithDefense()
        {
            var defender = MakeEntity(str: 0, dex: 0, mag: 0, luck: 0, def: 5);
            Assert.That(CombatCalculator.CalculateDefendDamageReduction(defender), Is.EqualTo(0.54f).Within(0.001f));

            defender.Attributes.Defense = 20;
            Assert.That(CombatCalculator.CalculateDefendDamageReduction(defender), Is.EqualTo(0.66f).Within(0.001f));

            defender.Attributes.Defense = 50;
            Assert.That(CombatCalculator.CalculateDefendDamageReduction(defender), Is.EqualTo(0.80f).Within(0.001f));
        }

        [Test]
        public void CalculateMaxHealth_UsesClassBaseHpAndCon()
        {
            int squireHp = CombatCalculator.CalculateMaxHealth(
                CombatCalculator.GetClassBaseHP(PlayerClass.ClassName.Squire),
                constitution: 5);
            int wizardHp = CombatCalculator.CalculateMaxHealth(
                CombatCalculator.GetClassBaseHP(PlayerClass.ClassName.Wizard),
                constitution: 10);

            Assert.That(squireHp, Is.EqualTo(38));
            Assert.That(wizardHp, Is.EqualTo(58));
        }

        [Test]
        public void CalculateBaseDamage_UsesPowerUpAdjustedPlayerAttributes()
        {
            var player = new Player("Hero", 100, 100, new Attributes
            {
                Strength = 10,
                Dexterity = 5,
                Magic = 5,
                Luck = 5,
                Defense = 5
            }, Player.Genders.m);

            int baseDamage = CombatCalculator.CalculateBaseDamage(player, CombatCalculator.GetAttackStyle(player));

            GameWorld.AllMonsters.Clear();
            GameState.CurrentRun = Run.StartNewRun();
            GameState.CurrentRun.PowerUps.Add(new PowerUp(
                new AttributeModifierComponent(
                    AttributeModifierComponent.AttributeType.Strength,
                    5f,
                    Rarity.Common,
                    PowerUpRank.I)));

            int boostedDamage = CombatCalculator.CalculateBaseDamage(player, CombatCalculator.GetAttackStyle(player));

            Assert.That(boostedDamage, Is.GreaterThan(baseDamage));
        }
    }
}
