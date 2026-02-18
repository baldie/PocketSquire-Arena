using System;
using NUnit.Framework;
using PocketSquire.Arena.Core;
using System.Collections.Generic;

namespace PocketSquire.Arena.Tests.Unit
{
    [TestFixture]
    public class ItemActionTests
    {
        private Player _player;
        private Monster _monster;
        private Battle _battle;

        [SetUp]
        public void Setup()
        {
            // Reset GameState and GameWorld
            _player = new Player { Name = "TestPlayer", Health = 100, MaxHealth = 100 };
            _monster = new Monster { Name = "TestMonster", Health = 50, MaxHealth = 50 };
            
            // Mock items in GameWorld
            GameWorld.Items = new List<Item>
            {
                new Item { Id = 1, Name = "Small Health Potion", Description = "Heals 25% HP", Target = ItemTarget.Self, SoundEffect = "gulp" },
                new Item { Id = 2, Name = "Bomb", Description = "Deals 20 DMG", Target = ItemTarget.Enemy }
            };

            GameState.Player = _player;
            _battle = new Battle(_player, _monster);
            GameState.Battle = _battle;
        }

        [Test]
        public void Constructor_ShouldSetItemData_UsingId()
        {
            var action = new ItemAction(1);
            
            Assert.That(action.ItemId, Is.EqualTo(1));
            Assert.That(action.ItemData, Is.Not.Null);
            Assert.That(action.ItemData.Name, Is.EqualTo("Small Health Potion"));
        }

        [Test]
        public void Constructor_ShouldThrow_IfItemNotFound()
        {
            Assert.Throws<InvalidOperationException>(() => new ItemAction(999));
        }

        [Test]
        public void ApplyEffect_ShouldRemoveItemFromInventory()
        {
            // Arrange — add 2 potions (fills the stack limit of 2)
            _player.Inventory.AddItem(1, 2);
            var action = new ItemAction(1);

            // Act
            action.ApplyEffect();

            // Assert
            Assert.That(_player.Inventory.GetItemCount(1), Is.EqualTo(1));
        }

        [Test]
        public void ApplyEffect_ShouldLogWarning_IfItemNotInInventory()
        {
            // Arrange - inventory empty
            var action = new ItemAction(1);

            // Act
            // Capture console output if possible, but for now just ensure no exception
            Assert.That(() => action.ApplyEffect(), Throws.Nothing);
            Assert.That(_player.Inventory.GetItemCount(1), Is.EqualTo(0));
        }
        
        [Test]
        public void ApplyEffect_ShouldUseCorrectActor()
        {
            // By default uses current turn actor
            _battle.CurrentTurn = new Turn(_player, _monster);
            var action = new ItemAction(1);
            Assert.That(action.Actor, Is.EqualTo(_player));
            
            _player.Inventory.AddItem(1, 1);
            action.ApplyEffect();
            Assert.That(_player.Inventory.GetItemCount(1), Is.EqualTo(0));
        }


        [Test]
        public void ApplyEffect_ShouldHealPlayer_WhenUsingHealthPotion()
        {
            // Arrange
            _player.Health = 50;
            _player.MaxHealth = 100;
            _player.Inventory.AddItem(1, 1);
            
            // Item 1 is "Small Health Potion" with "Heals 25% HP" (contains %)
            // So it should heal 25% of 100 = 25 HP.
            // Result 50 + 25 = 75.
            var action = new ItemAction(1);

            // Act
            action.ApplyEffect();

            // Assert
            Assert.That(_player.Health, Is.EqualTo(75));
        }
    }
}
