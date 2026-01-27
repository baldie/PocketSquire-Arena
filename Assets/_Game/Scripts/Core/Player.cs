using System;

namespace PocketSquire.Arena.Core
{
    [Serializable]
    public class Player : Entity
    {
        public enum CharGender {
            m,
            f
        }

        public CharGender Gender;
        public int Gold;
        public int Level { get; private set; } = 1;

        public override string SpriteId {
            get
            {
                return "player_" + Gender.ToString() + "_l" + Level.ToString() + "_battle";
            }
        }

        public override string HitSpriteId
        {
            get
            {
                return "player_" + Gender.ToString() + "_l" + Level.ToString() + "_hit";
            }
        }

        public override string DefeatSpriteId {
            get
            {
                return "player_" + Gender.ToString() + "_l" + Level.ToString() + "_defeat";
            }
        }

        public override string YieldSpriteId {
            get
            {
                return "player_" + Gender.ToString() + "_l" + Level.ToString() + "_yield";
            }
        }

        public override string AttackSpriteId {
            get
            {
                return "player_" + Gender.ToString() + "_l" + Level.ToString() + "_attack";
            }
        }

        public override string DefendSpriteId {
            get
            {
                return "player_" + Gender.ToString() + "_l" + Level.ToString() + "_defend";
            }
        }

        public string ItemSpriteId {
            get
            {
                return "player_" + Gender.ToString() + "_l" + Level.ToString() + "_item";
            }
        }

        public override string WinSpriteId {
            get
            {
                return "player_" + Gender.ToString() + "_l" + Level.ToString() + "_win";
            }
        }

        public bool CanLevelUp() {
            if (GameWorld.Progression == null) return false;
            var nextLevel = GameWorld.Progression.GetLevelForExperience(this.Experience);

            return this.Level < nextLevel;
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

        public void AcceptNewLevel() {
            if (GameWorld.Progression != null) {
                this.Level = GameWorld.Progression.GetLevelForExperience(this.Experience);
            }
        }

        public string GetSpriteId(GameContext context)
        {
            if (context == GameContext.Battle)
            {
                return this.SpriteId;
            }

            // TODO: eventually we will need to add class into this mix
            var sprite = "player_";
            sprite += this.Gender.ToString() + "_";
            sprite += "l" + this.Level.ToString();
            switch(context)
            {
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
                case ActionType.Item:
                    return "player_item";
                case ActionType.Yield:
                    return string.Empty;
                default:
                    return string.Empty;
            };
        }
    }
}