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
    }
}