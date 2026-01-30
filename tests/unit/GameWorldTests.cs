using NUnit.Framework;
using PocketSquire.Arena.Core;
using System.IO;
using System;

namespace PocketSquire.Arena.Tests
{
    [TestFixture]
    public class GameWorldTests
    {
        [Test]
        public void Load_ShouldLoadMonstersFromFile()
        {
            // Find project root by looking for "Assets" folder up the tree
            string current = Environment.CurrentDirectory;
            string root = current;
            while (!Directory.Exists(Path.Combine(root, "Assets")) && Directory.GetParent(root) != null)
            {
                var parent = Directory.GetParent(root);
                if (parent == null) break; 
                root = parent.FullName;
            }

            string path = Path.Combine(root, "Assets/_Game/Data/monsters.json");
            Assert.That(File.Exists(path), Is.True, $"Monster file not found at local resolved path: {path} (Started at {current})");

            // Act
            GameWorld.Load(root);

            // Assert
            Assert.That(GameWorld.AllMonsters.Count, Is.GreaterThan(0), "Monsters list should not be empty");
            
            var dummy = GameWorld.GetMonsterByName("Training Dummy");
            Assert.That(dummy, Is.Not.Null);
            Assert.That(dummy!.MaxHealth, Is.EqualTo(10));
            Assert.That(dummy!.Attributes.Constitution, Is.EqualTo(10));
            Assert.That(dummy!.PosX, Is.EqualTo(-330));
            Assert.That(dummy!.PosY, Is.EqualTo(450));
            Assert.That(dummy!.Width, Is.EqualTo(1920));
            Assert.That(dummy!.Height, Is.EqualTo(2240));
            Assert.That(dummy!.ScaleX, Is.EqualTo(0.4f).Within(0.001f));
            Assert.That(dummy!.ScaleY, Is.EqualTo(0.35f).Within(0.001f));
            Assert.That(dummy!.SpriteId, Is.EqualTo("training_dummy_battle"));
            Assert.That(dummy!.AttackSoundId, Is.EqualTo("TrainingDummyAttack"));
        }
        [Test]
        public void Load_ShouldLoadPlayersFromFile()
        {
            // Find project root
            string root = "";
            string current = Environment.CurrentDirectory;
            while (!Directory.Exists(Path.Combine(current, "Assets")) && Directory.GetParent(current) != null)
            {
                current = Directory.GetParent(current)!.FullName;
            }
            root = current;

            // Act
            GameWorld.Load(root);

            // Assert
            Assert.That(GameWorld.Players.Count, Is.GreaterThan(0), "Players list should not be empty");
            
            var player = GameWorld.GetPlayerByName("player_m_l1");
            Assert.That(player, Is.Not.Null);
            Assert.That(player!.Health, Is.EqualTo(3));
            Assert.That(player!.Attributes.Strength, Is.EqualTo(1));
            Assert.That(player!.SpriteId, Is.EqualTo("player_m_l1_battle"));
            Assert.That(player!.AttackSoundId, Is.EqualTo("m_physical_attack"));
            Assert.That(player!.Gender, Is.EqualTo(Player.CharGender.m));
        }
        [Test]
        public void ResetMonsters_ShouldRestoreHealthToMax()
        {
            // Arrange
            GameWorld.AllMonsters.Clear();
            var monster = new Monster("Test Monster", 5, 10, new Attributes());
            GameWorld.AllMonsters.Add(monster);

            // Act
            GameWorld.ResetMonsters();

            // Assert
            Assert.That(monster.Health, Is.EqualTo(10));
        }
    }
}
