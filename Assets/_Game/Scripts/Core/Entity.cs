#nullable enable
using System;

namespace PocketSquire.Arena.Core
{
    [Serializable]
    public class Entity
    {
        public enum GameContext
        {
            Battle,
            Town
        }
        public string Name = string.Empty;
        public int Health;
        public int MaxHealth;
        public int Experience;
        public Attributes Attributes = new Attributes();
        public float PosX;
        public float PosY;
        public float Width;
        public float Height;
        public float ScaleX = 1f;
        public float ScaleY = 1f;
        public int Rank = 0;
        public string AttackSoundId = string.Empty;
        public string DefendSoundId = string.Empty;
        public string HitSoundId = string.Empty;
        public string DefeatSoundId = string.Empty;
        public bool IsDefending { get; set; }
        public event Action? onDeath;
        public virtual string SpriteId => string.Empty;
        public virtual string HitSpriteId => string.Empty;
        public virtual string AttackSpriteId => string.Empty;
        public virtual string SpecialAttackSpriteId => string.Empty;
        public virtual string DefendSpriteId => string.Empty;
        public virtual string YieldSpriteId => string.Empty;
        public virtual string WinSpriteId => string.Empty;
        public virtual string DefeatSpriteId => string.Empty;

        public Entity() 
        { 
        }

        public Entity(string name, int health, int maxHealth, Attributes attributes)
        {
            Name = name;
            Health = health;
            MaxHealth = maxHealth;
            Attributes = attributes;
        }

        public bool IsDefeated
        {
            get
            {
                return Health <= 0;
            }
        }

        public void Heal(int amount)
        {
            Health += amount;
            if (Health > MaxHealth) Health = MaxHealth;
        }

        public void TakeDamage(int amount)
        {
            if (IsDefending)
            {
                amount = (int)Math.Ceiling(amount * 0.5f); // 50% damage reduction, rounded up
            }
            Health -= amount;
            if (Health < 0) Health = 0;
            if (Health == 0) onDeath?.Invoke();
        }

        public virtual string GetActionSoundId(ActionType actionType)
        {
             switch(actionType)
            {
                case ActionType.Attack:
                    return AttackSoundId;
                default:
                    return string.Empty;
            };
        }

        public virtual string GetHitSoundId() => HitSoundId;
        public virtual ActionType DetermineAction(Entity target) => ActionType.Attack;
    }
}