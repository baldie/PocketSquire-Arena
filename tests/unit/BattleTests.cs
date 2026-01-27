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
            _player = new Player("Squire", 10, 10, new Attributes(), Player.CharGender.m);
            _monster = new Monster("Training Dummy", 10, 10, new Attributes());
        }

        [Test]
        public void Battle_Initialization_PlayerGoesFirst()
        {
            // Act
            var battle = new Battle(_player, _monster);

            // Assert
            Assert.That(battle.CurrentTurn?.IsPlayerTurn, Is.True);
            Assert.That(battle.IsOver(), Is.False);
        }

        [Test]
        public void Battle_ChangeTurns_SwitchesActor()
        {
            // Arrange
            var battle = new Battle(_player, _monster);

            // Act - End turn
            battle.AdvanceTurn();

            // Assert - Now monster turn
            Assert.That(battle.CurrentTurn?.IsPlayerTurn, Is.False);

            // Act - End turn again
            battle.AdvanceTurn();

            // Assert - Now player turn again
            Assert.That(battle.CurrentTurn?.IsPlayerTurn, Is.True);
        }

        [Test]
        public void Battle_IsOver_WhenPlayerDies()
        {
            // Arrange
            var battle = new Battle(_player, _monster);
            
            // Act
            _player.TakeDamage(10);

            // Assert
            Assert.That(_player.IsDefeated, Is.True);
            Assert.That(battle.IsOver(), Is.True);
        }

        [Test]
        public void Battle_IsOver_WhenMonsterDies()
        {
            // Arrange
            var battle = new Battle(_player, _monster);
            
            // Act
            _monster.TakeDamage(10);

            // Assert
            Assert.That(_monster.IsDefeated, Is.True);
            Assert.That(battle.IsOver(), Is.True);
        }

        [Test]
        public void Battle_HandleActionComplete_ReturnsChangeTurnsAction()
        {
            // Arrange
            var battle = new Battle(_player, _monster);
            var dummyAction = new AttackAction(_player, _monster);

            // Act
            var result = battle.DetermineNextAction(dummyAction);

            // Assert
            Assert.That(result, Is.TypeOf<ChangeTurnsAction>());
        }

        [Test]
        public void Battle_HandleActionComplete_ReturnsWinAction_WhenMonsterDies()
        {
            // Arrange
            var battle = new Battle(_player, _monster);
            _monster.TakeDamage(10);
            var attackAction = new AttackAction(_player, _monster);

            // Act
            var result = battle.DetermineNextAction(attackAction);

            // Assert
            Assert.That(result, Is.TypeOf<WinAction>());
            Assert.That(battle.IsOver(), Is.True);
        }
        [Test]
        public void Battle_ChangeTurns_ResetsDefendingState()
        {
            // Arrange
            var battle = new Battle(_player, _monster);
            _player.IsDefending = true;

            // Act 1 - End Turn. It is now Monster's turn.
            // Player should STILL be defending against Monster.
            battle.AdvanceTurn();
            Assert.That(_player.IsDefending, Is.True, "Defending should persist during opponent's turn");

            // Act 2 - End Turn again. It is now Player's turn again.
            // Player's defend should now reset.
            battle.AdvanceTurn();
            Assert.That(_player.IsDefending, Is.False, "Defending state should be reset when turn starts again");
        }
    }
}
