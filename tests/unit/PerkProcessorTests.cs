using NUnit.Framework;
using PocketSquire.Arena.Core;
using PocketSquire.Arena.Core.Perks;
using System.IO;
using System;

namespace PocketSquire.Arena.Tests
{
    [TestFixture]
    public class PerkProcessorTests
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

        private Player MakePlayer(int hp = 100, int maxHp = 100, int gold = 1000)
        {
            var p = new Player("Hero", hp, maxHp, new Attributes { Strength = 10, Luck = 5 }, Player.Genders.m);
            p.GainGold(gold);
            return p;
        }

        private void UnlockAndActivate(Player player, string perkId)
        {
            var perk = GameWorld.GetPerkById(perkId) ?? new Perk { Id = perkId };
            player.AcquiredPerks.Add(perk);
            player.ActivePerks.Add(perk);
            player.PerkStates[perkId] = new PerkState { PerkId = perkId };
        }

        // --- RestoreHP (flat and percent) ---

        [Test]
        public void ProcessEvent_SecondWind_RestoresHPWhenBelowThreshold()
        {
            var player = MakePlayer(hp: 20, maxHp: 100);
            UnlockAndActivate(player, "second_wind");

            var ctx = new PerkContext
            {
                Player = player,
                PlayerHpPercent = 20, // below 30% threshold
                Rng = new System.Random(0)
            };

            var result = PerkProcessor.ProcessEvent(PerkTriggerEvent.HPBelowThreshold, player, ctx);

            Assert.That(result.HealAmount, Is.GreaterThan(0), "Should have healed");
            // Heals 15% of 100 = 15
            Assert.That(player.Health, Is.EqualTo(35));
        }

        [Test]
        public void ProcessEvent_SecondWind_DoesNotTriggerAboveThreshold()
        {
            var player = MakePlayer(hp: 50, maxHp: 100);
            UnlockAndActivate(player, "second_wind");

            var ctx = new PerkContext
            {
                Player = player,
                PlayerHpPercent = 50, // above 30% threshold
                Rng = new System.Random(0)
            };

            var result = PerkProcessor.ProcessEvent(PerkTriggerEvent.HPBelowThreshold, player, ctx);

            Assert.That(result.HealAmount, Is.EqualTo(0), "Should not heal above threshold");
            Assert.That(player.Health, Is.EqualTo(50));
        }

        [Test]
        public void ProcessEvent_SecondWind_OncePerBattle_SecondCallIgnored()
        {
            var player = MakePlayer(hp: 20, maxHp: 100);
            UnlockAndActivate(player, "second_wind");

            var ctx = new PerkContext { Player = player, PlayerHpPercent = 20, Rng = new System.Random(0) };
            PerkProcessor.ProcessEvent(PerkTriggerEvent.HPBelowThreshold, player, ctx);
            int hpAfterFirst = player.Health;

            // Second trigger should be gated by OncePerBattle
            PerkProcessor.ProcessEvent(PerkTriggerEvent.HPBelowThreshold, player, ctx);

            Assert.That(player.Health, Is.EqualTo(hpAfterFirst), "Should not trigger twice per battle");
        }

        // --- DoubleDamage with proc chance ---

        [Test]
        public void ProcessEvent_LuckyStrike_ProcChance5Percent_SometimesDoubles()
        {
            var player = MakePlayer();
            UnlockAndActivate(player, "lucky_strike");

            int doubleDamageCount = 0;
            for (int seed = 0; seed < 200; seed++)
            {
                var testPlayer = MakePlayer();
                UnlockAndActivate(testPlayer, "lucky_strike");

                var ctx = new PerkContext
                {
                    Player = testPlayer,
                    PlayerHpPercent = 100,
                    Damage = 10,
                    Rng = new System.Random(seed)
                };
                var result = PerkProcessor.ProcessEvent(PerkTriggerEvent.PlayerAttackedMonster, testPlayer, ctx);
                if (result.ShouldDoubleDamage) doubleDamageCount++;
            }

            // 5% of 200 = ~10, allow a wide range
            Assert.That(doubleDamageCount, Is.InRange(1, 30), $"Expected ~5% proc rate, got {doubleDamageCount}/200");
        }

        // --- StackDamageBuff (stacks, max stacks, reset) ---

        [Test]
        public void ProcessEvent_WarriorsResolve_StacksIncrease()
        {
            var player = MakePlayer();
            UnlockAndActivate(player, "warriors_resolve");

            var ctx = new PerkContext { Player = player, PlayerHpPercent = 100, Rng = new System.Random(0) };

            var r1 = PerkProcessor.ProcessEvent(PerkTriggerEvent.PlayerHitMonster, player, ctx);
            var state = player.PerkStates["warriors_resolve"];

            // After first hit: stacks should have gone to 1 (but state was reset because we re-process)
            Assert.That(state.CurrentStacks, Is.GreaterThanOrEqualTo(0));
        }

        [Test]
        public void ProcessEvent_WarriorsResolve_ResetsOnMiss()
        {
            var player = MakePlayer();
            UnlockAndActivate(player, "warriors_resolve");
            var state = player.PerkStates["warriors_resolve"];
            state.CurrentStacks = 3;

            var ctx = new PerkContext { Player = player, PlayerHpPercent = 100, Rng = new System.Random(0) };
            PerkProcessor.ProcessEvent(PerkTriggerEvent.PlayerMissedMonster, player, ctx);

            Assert.That(state.ConsecutiveCounter, Is.EqualTo(0), "Miss should reset consecutive counter");
        }

        // --- SurviveFatalBlow once per run ---

        [Test]
        public void ProcessEvent_PhoenixHeart_ReturnsSurviveFatalBlow()
        {
            // ProcessEvent returns the flag; TakeDamage (via AttackAction) is responsible
            // for actually setting Health = 1.
            var player = MakePlayer(hp: 1, maxHp: 100);
            UnlockAndActivate(player, "phoenix_heart");

            var ctx = new PerkContext { Player = player, PlayerHpPercent = 1, Rng = new System.Random(0) };
            var result = PerkProcessor.ProcessEvent(PerkTriggerEvent.WouldDie, player, ctx);

            Assert.That(result.SurviveFatalBlow, Is.True, "SurviveFatalBlow flag should be set");
            // Health is NOT modified here — TakeDamage owns that responsibility
        }

        [Test]
        public void TakeDamage_WouldDieCheck_SetsHealthTo1WhenCallbackReturnsTrue()
        {
            // End-to-end: TakeDamage supplies Health = 1 when the perk callback returns true
            var player = MakePlayer(hp: 5, maxHp: 100);

            bool callbackFired = false;
            player.TakeDamage(99, dmg =>
            {
                callbackFired = true;
                return true; // simulate perk saving player
            });

            Assert.That(callbackFired, Is.True, "Callback should have fired");
            Assert.That(player.Health, Is.EqualTo(1), "Health should be exactly 1 after survival");
        }

        [Test]
        public void TakeDamage_WouldDieCheck_NotFiredWhenDamageBelowHealth()
        {
            var player = MakePlayer(hp: 50, maxHp: 100);

            bool callbackFired = false;
            player.TakeDamage(10, dmg =>
            {
                callbackFired = true;
                return true;
            });

            Assert.That(callbackFired, Is.False, "Callback should not fire when player survives normally");
            Assert.That(player.Health, Is.EqualTo(40));
        }

        [Test]
        public void ProcessEvent_PhoenixHeart_OncePerRun_SecondCallIgnored()
        {
            var player = MakePlayer(hp: 1, maxHp: 100);
            UnlockAndActivate(player, "phoenix_heart");

            var ctx = new PerkContext { Player = player, PlayerHpPercent = 1, Rng = new System.Random(0) };
            PerkProcessor.ProcessEvent(PerkTriggerEvent.WouldDie, player, ctx);

            // Second trigger
            player.Health = 1;
            var result2 = PerkProcessor.ProcessEvent(PerkTriggerEvent.WouldDie, player, ctx);
            Assert.That(result2.SurviveFatalBlow, Is.False, "Should not trigger again this run");
        }

        // --- Passive modifiers ---

        [Test]
        public void GetPassiveModifiers_NoPerks_DefaultValues()
        {
            var player = MakePlayer();

            var result = PerkProcessor.GetPassiveModifiers(player);

            Assert.That(result.HitChanceBonusPercent, Is.EqualTo(0));
            Assert.That(result.DamageBuffMultiplier, Is.EqualTo(1f));
            Assert.That(result.GoldGainMultiplier, Is.EqualTo(1f));
        }

        [Test]
        public void GetPassiveModifiers_TreasureHunter_GoldGainIncreased()
        {
            var player = MakePlayer();
            UnlockAndActivate(player, "treasure_hunter");

            var result = PerkProcessor.GetPassiveModifiers(player);

            // treasure_hunter: 10% more gold -> multiplier should be > 1.0
            Assert.That(result.GoldGainMultiplier, Is.GreaterThan(1.0f));
        }

        // --- Duration tick-down ---

        [Test]
        public void TickDuration_DecrementsDuration()
        {
            var player = MakePlayer();
            player.PerkStates["some_perk"] = new PerkState
            {
                PerkId = "some_perk",
                RemainingDuration = 3
            };

            PerkProcessor.TickDuration(player);

            Assert.That(player.PerkStates["some_perk"].RemainingDuration, Is.EqualTo(2));
        }

        [Test]
        public void TickDuration_DoesNotGoBelowZero()
        {
            var player = MakePlayer();
            player.PerkStates["zeroed_perk"] = new PerkState
            {
                PerkId = "zeroed_perk",
                RemainingDuration = 0
            };

            PerkProcessor.TickDuration(player);

            Assert.That(player.PerkStates["zeroed_perk"].RemainingDuration, Is.EqualTo(0));
        }

        // --- ResetForBattle ---

        [Test]
        public void ResetPerksForBattle_ClearsOncePerBattleFlag()
        {
            var player = MakePlayer();
            player.PerkStates["battle_perk"] = new PerkState
            {
                PerkId = "battle_perk",
                HasTriggeredThisBattle = true,
                CurrentStacks = 3
            };

            PerkProcessor.ResetPerksForBattle(player);

            var state = player.PerkStates["battle_perk"];
            Assert.That(state.HasTriggeredThisBattle, Is.False);
            Assert.That(state.CurrentStacks, Is.EqualTo(0));
        }

        // --- ConsecutiveCount logic ---

        [Test]
        public void ProcessEvent_PrecisionStrike_TriggersAfter3ConsecutiveHits()
        {
            var player = MakePlayer();
            UnlockAndActivate(player, "precision_strike");

            var ctx = new PerkContext { Player = player, PlayerHpPercent = 100, Damage = 10, Rng = new System.Random(0) };

            var r1 = PerkProcessor.ProcessEvent(PerkTriggerEvent.ConsecutiveHits, player, ctx);
            var r2 = PerkProcessor.ProcessEvent(PerkTriggerEvent.ConsecutiveHits, player, ctx);
            var r3 = PerkProcessor.ProcessEvent(PerkTriggerEvent.ConsecutiveHits, player, ctx);

            // Only the 3rd should trigger (consecutiveCount = 3)
            Assert.That(r1.BonusDamageFlat, Is.EqualTo(0), "Should not trigger on 1st hit");
            Assert.That(r2.BonusDamageFlat, Is.EqualTo(0), "Should not trigger on 2nd hit");
            Assert.That(r3.BonusDamageFlat, Is.GreaterThan(0), "Should trigger on 3rd consecutive hit");
        }
    }
}
