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

        public bool IsBlocking { get; set; }
        public event Action? onDeath;

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
            if (IsBlocking)
            {
                amount = (int)Math.Ceiling(amount * 0.5f); // 50% damage reduction, rounded up
            }
            Health -= amount;
            if (Health < 0) Health = 0;
            if (Health == 0) onDeath?.Invoke();
        }
    }
}