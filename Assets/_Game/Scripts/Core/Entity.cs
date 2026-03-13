#nullable enable
using System;

namespace PocketSquire.Arena.Core
{
    [Serializable]
    public class Entity
    {

        public string Name = string.Empty;
        public int Health;
        public int MaxHealth;
        public int Mana;
        public int MaxMana;

        public void RestoreMana(int amount)
        {
            Mana = Math.Min(Mana + amount, MaxMana);
        }

        public bool SpendMana(int amount)
        {
            if (Mana < amount) return false;
            Mana -= amount;
            return true;
        }
        public int Experience;
        public int Gold;
        public Inventory Inventory = new();
        public Attributes Attributes = new Attributes();
        public int Rank = 0;
        public virtual string AttackSoundId { get; set; } = string.Empty;
        public virtual string DefendSoundId { get; set; } = string.Empty;
        public virtual string HitSoundId { get; set; } = string.Empty;
        public virtual string DefeatSoundId { get; set; } = string.Empty;
        public virtual string SpecialAttackSoundId { get; set; } = string.Empty;
        public bool IsDefending { get; set; }
        public event Action? onDeath;
        public virtual string SpriteId => string.Empty;
        public virtual string HitSpriteId => string.Empty;
        public virtual string AttackSpriteId => string.Empty;
        public virtual string SpecialAttackSpriteId => string.Empty;
        public virtual string DefendSpriteId => string.Empty;
        public virtual string BattleSpriteId => string.Empty;
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

        public void TakeDamage(int amount, Func<int, bool>? wouldDieCheck = null)
        {
            if (IsDefending)
            {
                amount = (int)Math.Ceiling(amount * (1f - CombatCalculator.CalculateDefendDamageReduction(this)));
            }

            // Allow a perk (e.g. Phoenix Heart) to intercept a killing blow before damage is applied.
            // The callback fires synchronously; if it returns true we survive with exactly 1 HP.
            if (amount >= Health && wouldDieCheck != null && wouldDieCheck(amount))
            {
                Health = 1;
                return;
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
                case ActionType.SpecialAttack:
                    return SpecialAttackSoundId;
                default:
                    return string.Empty;
            };
        }

        public virtual string GetHitSoundId() => HitSoundId;
        public virtual ActionType DetermineAction(Entity target) => ActionType.Attack;
    }
}
