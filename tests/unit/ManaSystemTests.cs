using NUnit.Framework;
using PocketSquire.Arena.Core;
using System.Reflection;

namespace PocketSquire.Arena.Tests
{
    [TestFixture]
    public class ManaSystemTests
    {
        [SetUp]
        public void Setup()
        {
            GameState.Player = null;
            GameState.Battle = null;
        }

        [Test]
        public void GetManaProfile_ReturnsExpectedValues_ForPhysicalAndCasterClasses()
        {
            var fighterProfile = PlayerClass.GetManaProfile(PlayerClass.ClassName.Fighter);
            var wizardProfile = PlayerClass.GetManaProfile(PlayerClass.ClassName.Wizard);

            Assert.That(fighterProfile.UsesMana, Is.False);
            Assert.That(fighterProfile.BaseManaCost, Is.EqualTo(0));
            Assert.That(fighterProfile.RegenPerTurn, Is.EqualTo(0));

            Assert.That(wizardProfile.UsesMana, Is.True);
            Assert.That(wizardProfile.BaseManaCost, Is.EqualTo(16));
            Assert.That(wizardProfile.RegenPerTurn, Is.EqualTo(6));
        }

        [Test]
        public void PlayerManaHelpers_ReportAffordabilityAndSpendCorrectly()
        {
            var caster = new Player("Caster", 20, 20, new Attributes(), Player.Genders.m);
            caster.ChangeClass(PlayerClass.ClassName.SpellCaster);
            caster.Mana = 10;

            Assert.That(caster.UsesMana, Is.True);
            Assert.That(caster.SpecialAttackManaCost, Is.EqualTo(8));
            Assert.That(caster.ManaRegenPerTurn, Is.EqualTo(4));
            Assert.That(caster.CanAffordSpecialAttack(), Is.True);
            Assert.That(caster.TrySpendManaForSpecialAttack(), Is.True);
            Assert.That(caster.Mana, Is.EqualTo(2));
            Assert.That(caster.CanAffordSpecialAttack(), Is.False);
        }

        [Test]
        public void PhysicalClasses_DoNotSpendManaForSpecialAttacks()
        {
            var fighter = new Player("Fighter", 20, 20, new Attributes(), Player.Genders.m);
            fighter.Mana = 0;

            Assert.That(fighter.UsesMana, Is.False);
            Assert.That(fighter.CanAffordSpecialAttack(), Is.True);
            Assert.That(fighter.TrySpendManaForSpecialAttack(), Is.True);
            Assert.That(fighter.Mana, Is.EqualTo(0));
        }

        [Test]
        public void SpecialAttackAction_Constructor_ThrowsWhenPlayerCannotAffordMana()
        {
            var caster = new Player("Caster", 20, 20, new Attributes(), Player.Genders.m);
            var target = new Monster("Dummy", 20, 20, new Attributes());
            SetClassAndMana(caster, PlayerClass.ClassName.SpellCaster, maxMana: 20, mana: 0);

            Assert.Throws<System.InvalidOperationException>(() => new SpecialAttackAction(caster, target));
        }

        [Test]
        public void SpecialAttackAction_ApplyEffect_SpendsManaForCaster()
        {
            var caster = new Player("Caster", 100, 100, new Attributes
            {
                Magic = 12,
                Dexterity = 50,
                Luck = 0
            }, Player.Genders.m);
            var target = new Monster("Dummy", 100, 100, new Attributes());
            SetClassAndMana(caster, PlayerClass.ClassName.SpellCaster, maxMana: 20, mana: 20);

            SpecialAttackAction? action = null;
            for (int seed = 0; seed < 200; seed++)
            {
                var candidate = new SpecialAttackAction(caster, target, new System.Random(seed));
                if (candidate.DidHit)
                {
                    action = candidate;
                    break;
                }
            }

            Assert.That(action, Is.Not.Null);
            action!.ApplyEffect();

            Assert.That(caster.Mana, Is.EqualTo(12));
        }

        [Test]
        public void ChangeTurnsAction_RegeneratesManaForManaUsers()
        {
            var player = new Player("Caster", 100, 100, new Attributes(), Player.Genders.m);
            SetClassAndMana(player, PlayerClass.ClassName.SpellCaster, maxMana: 20, mana: 5);
            var monster = new Monster("Dummy", 100, 100, new Attributes());
            var battle = new Battle(player, monster);
            GameState.Battle = battle;

            var action = new ChangeTurnsAction(battle);
            action.ApplyEffect();

            Assert.That(player.Mana, Is.EqualTo(9));
        }

        [Test]
        public void LoadFromSaveData_ClampsManaToMaxMana()
        {
            var player = new Player("Caster", 20, 20, new Attributes(), Player.Genders.m)
            {
                Mana = 50,
                MaxMana = 20
            };

            GameState.LoadFromSaveData(new SaveData { Player = player });

            Assert.That(GameState.Player, Is.Not.Null);
            Assert.That(GameState.Player!.Mana, Is.EqualTo(20));
        }

        private static void SetClassAndMana(Player player, PlayerClass.ClassName className, int maxMana, int mana)
        {
            var classField = typeof(Player).GetField("<Class>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic);
            classField?.SetValue(player, className);
            player.MaxMana = maxMana;
            player.Mana = mana;
        }
    }
}
