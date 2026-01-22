using NUnit.Framework;
using PocketSquire.Arena.Core;
using System;

namespace PocketSquire.Arena.Tests
{
    [TestFixture]
    public class BattleTests
    {
        private Player _player;
        private Monster _monster;

        [SetUp]
        public void Setup()
        {
            _player = new Player("Squire", 10, 10, new Attributes(), Player.Gender.Male);
            _monster = new Monster("Test Monster", 10, 10, new Attributes());
        }

        [Test]
        public void Battle_Initialization_PlayerGoesFirst()
        {
            // Act
            var battle = new Battle(_player, _monster);

            // Assert
            Assert.That(battle.CurrentTurn.IsPlayerTurn, Is.True);
            Assert.That(battle.IsOver, Is.False);
        }

        [Test]
        public void Battle_ChangeTurns_SwitchesActor()
        {
            // Arrange
            var battle = new Battle(_player, _monster);

            // Act - Player ends turn
            battle.CurrentTurn.End();

            // Assert - Now monster turn
            Assert.That(battle.CurrentTurn.IsPlayerTurn, Is.False);

            // Act - Monster executes turn
            battle.CurrentTurn.Execute();

            // Assert - Now player turn again
            Assert.That(battle.CurrentTurn.IsPlayerTurn, Is.True);
        }

        [Test]
        public void Battle_IsOver_WhenPlayerDies()
        {
            // Arrange
            var battle = new Battle(_player, _monster);
            
            // Act
            _player.TakeDamage(10);

            // Assert
            Assert.That(_player.IsDead, Is.True);
            Assert.That(battle.IsOver, Is.True);
        }

        [Test]
        public void Battle_IsOver_WhenMonsterDies()
        {
            // Arrange
            var battle = new Battle(_player, _monster);
            
            // Act
            _monster.TakeDamage(10);

            // Assert
            Assert.That(_monster.IsDead, Is.True);
            Assert.That(battle.IsOver, Is.True);
        }

        [Test]
        public void Turn_Execute_DoesNotAllowPlayerAction()
        {
            // Arrange
            var battle = new Battle(_player, _monster);
            // CurrentTurn is Player's turn

            // Act & Assert
            // Execute is for non-player entities. It should log and return.
            // We can't easily assert the Console log here without redirecting, 
            // but we can check that it doesn't change the turn.
            battle.CurrentTurn.Execute();
            
            Assert.That(battle.CurrentTurn.IsPlayerTurn, Is.True, "Execute should do nothing on player turn");
        }
    }
}
