using System;

namespace PocketSquire.Arena.Core
{
    [Serializable]
    public class Player : Entity
    {
        public enum CharGender {
            Male,
            Female
        }

        public CharGender Gender;

        public int Experience;
        public int Gold;
        public int Level {
            get
            {
                // TODO: fill in the actual values
                return (int)Math.Ceiling((double)(Experience+1) / 100);
            }
        }

        public Player() : base() { }

        public Player(string name, int health, int maxHealth, Attributes attributes, CharGender gender) : base(name, health, maxHealth, attributes)
        {
            this.Gender = gender;
        }

        public void GainExperience(int amount)
        {
            Experience += amount;
        }

        public void GainGold(int amount)
        {
            Gold += amount;
        }

        public string GetSpriteId(GameContext context)
        {
            // TODO: eventually you will need to add class into this mix
            var sprite = "player_";
            sprite += this.Gender == CharGender.Male ? "m_" : "f_";
            sprite += "l" + this.Level.ToString();
            switch(context)
            {
                case GameContext.Battle: 
                    sprite += "_battle";
                    break;
                case GameContext.Town: 
                    sprite += "_town";
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(context), context, null);
            };
            return sprite;
        }

        public override string GetActionSoundId(ActionType actionType)
        {
            switch(actionType)
            {
                case ActionType.Attack:
                    return !string.IsNullOrEmpty(AttackSoundId) ? AttackSoundId : "player_attack";
                case ActionType.Block:
                    return !string.IsNullOrEmpty(BlockSoundId) ? BlockSoundId : "player_block";
                case ActionType.UseItem:
                    return "player_item";
                case ActionType.Yield:
                    return string.Empty;
                default:
                    return string.Empty;
            };
        }

        public override string GetActionAnimationId(ActionType actionType)
        {
            return actionType switch
            {
                ActionType.Attack => "Attack",
                ActionType.Block => "Block",
                ActionType.UseItem => "Item",
                ActionType.Yield => "Yield",
                _ => "Idle"
            };
        }

        public override string GetHitSoundId() => "player_hit";
        public override string GetHitAnimationId() => "Hit";
    }
}