using System;
namespace PocketSquire.Arena.Core
{
    [Serializable]
    public class Player : Entity
    {
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