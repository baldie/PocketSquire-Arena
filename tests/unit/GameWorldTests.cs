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

            string path = Path.Combine(root, "Assets/_Game/Scripts/Data/monsters.json");
            Assert.That(File.Exists(path), Is.True, $"Monster file not found at local resolved path: {path} (Started at {current})");

            // Act
            GameWorld.Load(root);

            // Assert
            Assert.That(GameWorld.Monsters.Count, Is.GreaterThan(0), "Monsters list should not be empty");
            
            var dummy = GameWorld.GetMonsterByName("Training Dummy");
            Assert.That(dummy, Is.Not.Null);
            Assert.That(dummy!.MaxHealth, Is.EqualTo(50));
            Assert.That(dummy!.Attributes.Constitution, Is.EqualTo(10));
        }
    }
}
