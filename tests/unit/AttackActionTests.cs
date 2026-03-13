using NUnit.Framework;
using PocketSquire.Arena.Core;
using PocketSquire.Arena.Core.Perks;
using System;
using System.IO;

namespace PocketSquire.Arena.Tests
{
    [TestFixture]
    public class AttackActionTests
    {
        private static string GetProjectRoot()
        {
            string current = Environment.CurrentDirectory;
            while (!Directory.Exists(Path.Combine(current, "Assets")) && Directory.GetParent(current) != null)
            {
                var parent = Directory.GetParent(current);
                if (parent == null) break;
                current = parent.FullName;
            }

            return current;
        }

        [SetUp]
        public void Setup()
        {
            GameWorld.Load(GetProjectRoot());
        }

        private static Monster MakeMonster(int str, int dex, int luck, int mag = 0, int def = 0)
        {
            return new Monster("TestEntity", 100, 100, new Attributes
            {
                Strength = str,
                Dexterity = dex,
                Luck = luck,
                Magic = mag,
                Defense = def
            });
        }

        private static Player MakePlayer(int hp = 100, int maxHp = 100)
        {
            return new Player("Hero", hp, maxHp, new Attributes
            {
                Strength = 5,
                Constitution = 5,
                Magic = 5,
                Dexterity = 5,
                Luck = 5,
                Defense = 5
            }, Player.Genders.m);
        }

        private static void UnlockAndActivate(Player player, string perkId)
        {
            var perk = GameWorld.GetPerkById(perkId) ?? new Perk { Id = perkId };
            player.AcquiredPerks.Add(perk);
            player.ActivePerks.Add(perk);
            player.PerkStates[perkId] = new PerkState { PerkId = perkId };
        }

        [Test]
        public void AttackAction_HitAppliesFinalDamageAfterDefenseReduction()
        {
            var attacker = MakeMonster(str: 10, dex: 50, luck: 0);
            var target = MakeMonster(str: 5, dex: 0, luck: 0, def: 20);

            AttackAction? action = null;
            for (int seed = 0; seed < 200; seed++)
            {
                var candidate = new AttackAction(attacker, target, new Random(seed));
                if (candidate.DidHit && !candidate.IsCrit)
                {
                    action = candidate;
                    break;
                }
            }

            Assert.That(action, Is.Not.Null, "Expected to find a deterministic hit without a crit.");
            int hpBefore = target.Health;

            action!.ApplyEffect();

            Assert.That(hpBefore - target.Health, Is.EqualTo(action.FinalDamage));
            Assert.That(action.FinalDamage, Is.EqualTo(CombatCalculator.ApplyDefenseReduction(action.Damage, target)));
        }

        [Test]
        public void AttackAction_MissedAttack_DoesNotChangeTargetHealth()
        {
            var attacker = MakeMonster(str: 10, dex: 0, luck: 1);
            var target = MakeMonster(str: 5, dex: 100, luck: 100);

            for (int seed = 0; seed < 1000; seed++)
            {
                var rng = new Random(seed);
                var action = new AttackAction(attacker, target, rng);
                if (!action.DidHit)
                {
                    int hpBefore = target.Health;
                    action.ApplyEffect();
                    Assert.That(action.Damage, Is.EqualTo(0));
                    Assert.That(action.FinalDamage, Is.EqualTo(0));
                    Assert.That(target.Health, Is.EqualTo(hpBefore), $"Missed attack with seed {seed} should not change HP");
                    return;
                }
            }

            Assert.Fail("Expected at least one miss across 1000 seeds.");
        }

        [Test]
        public void AttackAction_MonsterHitPlayer_CanBeNullifiedByPerk()
        {
            var attacker = MakeMonster(str: 12, dex: 80, luck: 0);
            var player = MakePlayer();
            var perk = new Perk
            {
                Id = "test_nullify",
                DisplayName = "Test Nullify",
                PerkType = PerkType.Triggered,
                TriggerEvent = PerkTriggerEvent.MonsterAttackHitPlayer,
                Effect = PerkEffectType.NullifyDamage,
                ProcPercent = 100
            };
            player.AcquiredPerks.Add(perk);
            player.ActivePerks.Add(perk);
            player.PerkStates[perk.Id] = new PerkState { PerkId = perk.Id };

            var action = new AttackAction(attacker, player, new Random(0));

            Assert.That(action.DidHit, Is.True, "Seed should produce a hit for the nullify-perk test.");
            int hpBefore = player.Health;

            action.ApplyEffect();

            Assert.That(player.Health, Is.EqualTo(hpBefore), "Arcane Shield should nullify the incoming hit.");
            Assert.That(action.FinalDamage, Is.EqualTo(0));
        }
    }
}
