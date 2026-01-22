using System;
namespace PocketSquire.Arena.Core
{
    [Serializable]
    public class Monster : Entity
    {
        public string SpriteId = string.Empty;
        public string AttackSoundId = string.Empty;
        public string BlockSoundId = string.Empty;
        public string HitSoundId = string.Empty;

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

        public override string GetHitSoundId() => HitSoundId;
        public override string GetHitAnimationId() => "Hit";
    }
}