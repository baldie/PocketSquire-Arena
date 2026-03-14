using System;
using PocketSquire.Arena.Core.Perks;

namespace PocketSquire.Arena.Core
{
    /// <summary>
    /// Action that handles losing a battle
    /// </summary>
    public class LoseAction : IGameAction
    {
        public ActionType Type => ActionType.Lose;
        public Entity Actor { get; }
        public Entity Target { get; } 
        public int GoldLost { get; private set; }

        /// <param name="actor">loser</param>
        /// <param name="target">winner</param>
        public LoseAction(Entity actor, Entity target)
        {
            Actor = actor;
            Target = target;
        }

        public void ApplyEffect()
        {
            if (Actor is Player player)
            {
                var context = new PerkContext { Player = player, Target = Target };
                ApplyArenaGoldLoss(player);
                PerkProcessor.ProcessEvent(PerkTriggerEvent.BattleLost, player, context);
            }
        }

        private void ApplyArenaGoldLoss(Player player)
        {
            var passives = PerkProcessor.GetPassiveModifiers(player);
            float goldLossMultiplier = Math.Max(0f, 0.5f - (passives.KeepMoneyPercent / 100f));
            GoldLost = (int)(player.Gold * goldLossMultiplier);

            player.Gold -= GoldLost;
        }
    }
}
