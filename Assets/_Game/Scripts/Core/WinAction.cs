#nullable enable
using System;
using PocketSquire.Arena.Core.Perks;

namespace PocketSquire.Arena.Core
{
    /// <summary>
    /// Action that handles winning a battle.
    /// Applies gold gain multiplier from passive perks and fires BattleWon trigger event.
    /// </summary>
    public class WinAction : IGameAction
    {
        public ActionType Type => ActionType.Win;
        public Entity Actor { get; }
        public Entity Target { get; }

        /// <param name="actor">winner</param>
        /// <param name="target">loser</param>
        public WinAction(Entity actor, Entity target)
        {
            Actor = actor;
            Target = target;
        }

        public void ApplyEffect()
        {
            if (Actor is Player player)
            {
                player.GainExperience(Target.Experience);

                // Apply passive gold gain multiplier (e.g. Treasure Hunter perk)
                var passives = PerkProcessor.GetPassiveModifiers(player);
                int goldGained = (int)(Target.Gold * passives.GoldGainMultiplier);
                player.GainGold(goldGained);

                // Fire BattleWon event (e.g. Pious perk increases MaxHP)
                var context = new PerkContext { Player = player, Target = Target };
                PerkProcessor.ProcessEvent(PerkTriggerEvent.BattleWon, player, context);
            }

            GameState.CurrentRun?.NextRank();
        }
    }
}
