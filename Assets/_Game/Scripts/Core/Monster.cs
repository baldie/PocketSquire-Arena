using System;
namespace PocketSquire.Arena.Core
{
    [Serializable]
    public class Monster : Entity
    {
        public Monster() : base() 
        { 
        }
        
        public Monster(string name, int health, int maxHealth, Attributes attributes) : base(name, health, maxHealth, attributes)
        {
        }

        public override string GetActionSoundId(ActionType actionType)
        {
            return actionType switch
            {
                ActionType.Attack => AttackSoundId,
                ActionType.Block => BlockSoundId,
                ActionType.Yield => string.Empty,
                _ => string.Empty
            };
        }

        public override string GetActionAnimationId(ActionType actionType)
        {
            return actionType switch
            {
                ActionType.Attack => "Attack",
                ActionType.Block => "Block",
                ActionType.Yield => "Yield",
                _ => "Idle"
            };
        }

        public override IGameAction DetermineAction(Entity target)
        {
            // Basic AI: Always attack
            return new AttackAction(this, target, 1);
        }

        public override string GetHitSoundId() => HitSoundId;
        public override string GetHitAnimationId() => "Hit";
    }
}