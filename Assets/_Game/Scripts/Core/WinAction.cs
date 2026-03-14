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
                var run = GameState.CurrentRun;
                var passives = PerkProcessor.GetPassiveModifiers(player);

                float xpMultiplier = 1f;
                if (run != null)
                {
                    xpMultiplier += run.PowerUps.GetXpBonusPercent(run.ArenaRank) / 100f;
                }
                player.GainExperience((int)(Target.Experience * xpMultiplier));

                float goldMultiplier = passives.GoldGainMultiplier;
                if (run != null)
                {
                    goldMultiplier *= 1f + (run.PowerUps.GetGoldBonusPercent(run.ArenaRank) / 100f);
                }
                int goldGained = (int)(Target.Gold * goldMultiplier);
                player.GainGold(goldGained);

                if (run != null)
                {
                    run.PowerUps.ApplyUtilityEffects(player, run.ArenaRank);
                }

                var context = new PerkContext { Player = player, Target = Target };
                PerkProcessor.ProcessEvent(PerkTriggerEvent.BattleWon, player, context);
            }

            GameState.CurrentRun?.NextRank();
        }
    }
}
