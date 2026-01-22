using System;

namespace PocketSquire.Arena.Core
{
    [Serializable]
    public class Player : Entity
    {
        public enum Gender {
            Male,
            Female
        }

        private Gender gender;

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

        public Player(string name, int health, int maxHealth, Attributes attributes, Gender gender) : base(name, health, maxHealth, attributes)
        {
            this.gender = gender;
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
            sprite += this.gender == Gender.Male ? "m_" : "f_";
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
    }
}