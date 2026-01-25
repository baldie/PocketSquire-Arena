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
        public Attributes Attributes = new Attributes();
        public float PosX;
        public float PosY;
        public float Width;
        public float Height;
        public float ScaleX = 1f;
        public float ScaleY = 1f;
        public string AttackSoundId = string.Empty;
        public string DefendSoundId = string.Empty;
        public string HitSoundId = string.Empty;
        public bool IsDefending { get; set; }
        public event Action? onDeath;
        public virtual string SpriteId => string.Empty;
        public virtual string HitSpriteId => string.Empty;
        public virtual string AttackSpriteId => string.Empty;
        public virtual string DefendSpriteId => string.Empty;
        public virtual string YieldSpriteId => string.Empty;

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

        public bool IsDead
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

        public virtual string GetActionAnimationId(ActionType actionType)
        {
            switch(actionType)
            {
                case ActionType.Attack:
                    return "Attack";
                case ActionType.Defend:
                    return "Defend";
                case ActionType.Yield:
                    return "Yield";
                default:
                    return "Idle";
            };
        }
        public virtual float GetActionDuration(ActionType actionType)
        {
            return actionType == ActionType.Attack ? 1f : 0.5f;
        }
        public virtual string GetHitSoundId() => HitSoundId;
        public virtual string GetHitAnimationId() => "Hit"; // Default to "Hit" as per previous Monster implementation
        public virtual ActionType DetermineAction(Entity target) => ActionType.Attack;
    }
}