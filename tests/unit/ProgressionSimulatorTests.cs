
using NUnit.Framework;
using PocketSquire.Arena.Core.LevelUp;
using System.Collections.Generic;

namespace PocketSquire.Arena.Core.Tests.LevelUp
{
    [TestFixture]
    public class ProgressionSimulatorTests
    {
        [Test]
        public void SimulateRun_ShouldLevelUpCorrectly()
        {
            // Setup simple linear curve
            var thresholds = new int[] { 0, 100, 200, 300, 400 }; // XP required for L1, L2, L3, L4, L5
            // Note: The logic expects threshold[0] = 0 (Level 1).
            // Wait, ProgressionLogic implementation:
            // GetLevelForExperience(xp): index of first threshold > xp.
            // If thresholds = {0, 100}, creating L1 requires 0 xp.
            // xp < 100 -> L1. xp >= 100 -> L2.
            
            var rewards = new List<LevelReward>();
            var logic = new ProgressionLogic(thresholds, rewards);
            var simulator = new ProgressionSimulator(logic);

            // Simulate gaining 50 XP per step. target L3 (200 XP).
            // Step 1: 50 -> L1
            // Step 2: 100 -> L2 (Reached!)
            // Step 3: 150 -> L2
            // Step 4: 200 -> L3 (Reached!)
            
            var result = simulator.SimulateRun(3, 50, 10);

            Assert.That(result.LevelReached, Is.EqualTo(3));
            Assert.That(result.TotalXpGained, Is.EqualTo(200)); 
        }

        [Test]
        public void GetXpToReachLevel_ShouldMatchLogic()
        {
            var thresholds = new int[] { 0, 100, 300 };
            var rewards = new List<LevelReward>();
            var logic = new ProgressionLogic(thresholds, rewards);
            var simulator = new ProgressionSimulator(logic);

            Assert.That(simulator.GetXpToReachLevel(2), Is.EqualTo(100));
            Assert.That(simulator.GetXpToReachLevel(3), Is.EqualTo(300));
        }
    }
}
