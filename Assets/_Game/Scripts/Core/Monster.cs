using System;
namespace PocketSquire.Arena.Core
{
    [Serializable]
    public class Monster : Entity
    {
        public string SpriteId;
        public string AttackSoundId;
        public string BlockSoundId;
        public string HitSoundId;

        public Monster() : base() { }
        
        public Monster(string name, int health, int maxHealth, Attributes attributes) : base(name, health, maxHealth, attributes)
        {
        }
    }
}