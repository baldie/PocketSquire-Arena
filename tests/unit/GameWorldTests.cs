using NUnit.Framework;
using PocketSquire.Arena.Core;
using System.IO;
using System;

namespace PocketSquire.Arena.Tests
{
    [TestFixture]
    public class GameWorldTests
    {
        private string GetProjectRoot()
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

        [Test]
        public void Load_ShouldLoadMonstersFromFile()
        {
            // Arrange
            string root = GetProjectRoot();
            string path = Path.Combine(root, "Assets/_Game/Data/monsters.json");
            Assert.That(File.Exists(path), Is.True, $"Monster file not found at: {path}");

            // Act
            GameWorld.Load(root);

            // Assert - test logic, not specific values
            Assert.That(GameWorld.AllMonsters.Count, Is.GreaterThan(0), "Monsters list should not be empty");
            
            // Verify monsters have required properties populated
            var firstMonster = GameWorld.AllMonsters[0];
            Assert.That(firstMonster.Name, Is.Not.Null.And.Not.Empty, "Monster should have a name");
            Assert.That(firstMonster.MaxHealth, Is.GreaterThan(0), "Monster should have positive MaxHealth");
            Assert.That(firstMonster.Attributes, Is.Not.Null, "Monster should have Attributes");
        }

        [Test]
        public void Load_ShouldLoadClassTemplatesFromFile()
        {
            // Arrange
            string root = GetProjectRoot();

            // Act
            GameWorld.Load(root);

            // Assert - test logic, not specific values
            Assert.That(GameWorld.ClassTemplates.Count, Is.GreaterThan(0), "ClassTemplates list should not be empty");
            
            // Verify players have required properties populated
            var firstPlayer = GameWorld.ClassTemplates[0];
            Assert.That(firstPlayer.Name, Is.Not.Null.And.Not.Empty, "Player class should have a name");
        }

        [Test]
        public void GetMonsterByName_ReturnsCorrectMonster()
        {
            // Arrange
            string root = GetProjectRoot();
            GameWorld.Load(root);
            var expectedName = GameWorld.AllMonsters[0].Name;

            // Act
            var result = GameWorld.GetMonsterByName(expectedName);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Name, Is.EqualTo(expectedName));
        }

        [Test]
        public void GetMonsterByName_ReturnsNullForUnknownName()
        {
            // Arrange
            string root = GetProjectRoot();
            GameWorld.Load(root);

            // Act
            var result = GameWorld.GetMonsterByName("NonExistentMonster12345");

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public void GetClassTemplate_ReturnsCorrectPlayer()
        {
            // Arrange
            string root = GetProjectRoot();
            GameWorld.Load(root);
            // Assuming the first player name follows the {gender}_{className} pattern
            var fullName = GameWorld.ClassTemplates[0].Name;
            var parts = fullName.Split('_');
            var gender = parts[0];
            var className = parts[1];

            // Act
            var genderEnum = (Player.Genders)Enum.Parse(typeof(Player.Genders), gender);
            var classEnum = (PlayerClass.ClassName)Enum.Parse(typeof(PlayerClass.ClassName), className, true);
            var result = GameWorld.GetClassTemplate(genderEnum, classEnum);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Name, Is.EqualTo(fullName));
        }

        [Test]
        public void ResetAllMonsters_ShouldRestoreHealthToMax()
        {
            // Arrange - use synthetic data, not JSON
            GameWorld.AllMonsters.Clear();
            var monster = new Monster("Test Monster", 5, 10, new Attributes());
            GameWorld.AllMonsters.Add(monster);

            // Act
            GameWorld.ResetAllMonsters();

            // Assert
            Assert.That(monster.Health, Is.EqualTo(10));
        }
    }
}
