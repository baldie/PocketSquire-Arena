
using NUnit.Framework;
using PocketSquire.Arena.Core.LevelUp;
using System.Collections.Generic;

namespace PocketSquire.Arena.Core.Tests.LevelUp
{
    [TestFixture]
    public class XpCurveGeneratorTests
    {
        [Test]
        public void Generate_ShouldStartAtZero()
        {
            var config = new XpCurveConfig { BaseXp = 100, Exponent = 1 };
            config.MaxLevel = 3;

            var schedule = XpCurveGenerator.Generate(config);

            Assert.That(schedule[0], Is.EqualTo(0));
            Assert.That(schedule[1], Is.GreaterThan(0));
        }

        [Test]
        public void Generate_LinearCurve_ShouldAccumulateLinearly()
        {
            // Base = 100, Exp = 1
            // Formula: Delta = Base * (L-1)^1
            // L2 Delta = 100 * 1 = 100. Total = 100.
            // L3 Delta = 100 * 2 = 200. Total = 300.
            // L4 Delta = 100 * 3 = 300. Total = 600.
            
            var config = new XpCurveConfig { BaseXp = 100, Exponent = 1 };
            config.MaxLevel = 4;

            var schedule = XpCurveGenerator.Generate(config);

            Assert.That(schedule[0], Is.EqualTo(0));
            Assert.That(schedule[1], Is.EqualTo(100));
            Assert.That(schedule[2], Is.EqualTo(300));
            Assert.That(schedule[3], Is.EqualTo(600));
        }

        [Test]
        public void Generate_QuadraticCurve_ShouldAccumulateQuadratically()
        {
            // Base = 100, Exp = 2
            // Formula: Delta = Base * (L-1)^2
            // L2 Delta = 100 * 1^2 = 100. Total = 100.
            // L3 Delta = 100 * 2^2 = 400. Total = 500.
            // L4 Delta = 100 * 3^2 = 900. Total = 1400.

            var config = new XpCurveConfig { BaseXp = 100, Exponent = 2 };
            config.MaxLevel = 4;

            var schedule = XpCurveGenerator.Generate(config);

            Assert.That(schedule[0], Is.EqualTo(0));
            Assert.That(schedule[1], Is.EqualTo(100));
            Assert.That(schedule[2], Is.EqualTo(500));
            Assert.That(schedule[3], Is.EqualTo(1400));
        }
    }
}
