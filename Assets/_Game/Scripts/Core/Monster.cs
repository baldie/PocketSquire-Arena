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

        public override string SpriteId {
            get
            {
                return Name.ToLower().Replace(" ", "_") + "_battle";
            }
        }

        public override string AttackSpriteId {
            get
            {
                return Name.ToLower().Replace(" ", "_") + "_attack";
            }
        }

        public override string SpecialAttackSpriteId {
            get
            {
                return Name.ToLower().Replace(" ", "_") + "_special_attack";
            }
        }

        public override string HitSpriteId {
            get
            {
                return Name.ToLower().Replace(" ", "_") + "_hit";
            }
        }

        public override string DefendSpriteId {
            get
            {
                return Name.ToLower().Replace(" ", "_") + "_defend";
            }
        }

        private static readonly Random _random = new Random();

        public override ActionType DetermineAction(Entity target)
        {
            // ~25% chance for special attack
            if (_random.Next(4) == 0)
                return ActionType.SpecialAttack;
            return ActionType.Attack;
        }

        public override string ToString()
        {
            return $"{Name} (Rank: {Rank}, Health: {Health}/{MaxHealth})";
        }
    }
}