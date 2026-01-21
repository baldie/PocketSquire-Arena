using System;

namespace PocketSquire.Arena.Core
{
    [Serializable]
    public class Player : Entity
    {
        public static Player GetDefaultPlayer()
        {
            var attributes = new Attributes();
            attributes.Strength = 1;
            attributes.Constitution = 1;
            attributes.Intelligence = 1;
            attributes.Wisdom = 1;
            attributes.Luck = 1;
            return new Player("Squire", 10, 10, attributes);
        }

        public int Experience;
        public int Gold;

        public Player() : base() { }

        public Player(string name, int health, int maxHealth, Attributes attributes) : base(name, health, maxHealth, attributes)
        {
        }

        public void GainExperience(int amount)
        {
            Experience += amount;
        }

        public void GainGold(int amount)
        {
            Gold += amount;
        }
    }
}