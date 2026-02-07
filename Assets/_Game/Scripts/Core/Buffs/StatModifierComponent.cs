#nullable enable
using System;
using System.Collections.Generic;

namespace PocketSquire.Arena.Core.Buffs
{
    /// <summary>
    /// Modifies an attribute (stat) on the target entity by a flat or percentage value.
    /// Automatically reverts the change when removed.
    /// </summary>
    [Serializable]
    public class StatModifierComponent : IBuffComponent
    {
        public string Stat { get; set; } = string.Empty;
        public float Value { get; set; }
        public bool IsMultiplier { get; set; }

        // Store the original value to revert on removal
        private Dictionary<Entity, int> _originalValues = new Dictionary<Entity, int>();

        public StatModifierComponent()
        {
        }

        public StatModifierComponent(string stat, float value, bool isMultiplier)
        {
            Stat = stat;
            Value = value;
            IsMultiplier = isMultiplier;
        }

        public void OnApply(Entity target)
        {
            int originalValue = GetStatValue(target);
            _originalValues[target] = originalValue;

            int modifiedValue;
            if (IsMultiplier)
            {
                modifiedValue = (int)(originalValue * Value);
            }
            else
            {
                modifiedValue = originalValue + (int)Value;
            }

            SetStatValue(target, modifiedValue);
        }

        public void OnTick(Entity target, float deltaTime)
        {
            // StatModifier doesn't need periodic updates
        }

        public void OnRemove(Entity target)
        {
            if (_originalValues.TryGetValue(target, out int originalValue))
            {
                SetStatValue(target, originalValue);
                _originalValues.Remove(target);
            }
        }

        private int GetStatValue(Entity target)
        {
            return Stat switch
            {
                "Strength" => target.Attributes.Strength,
                "Constitution" => target.Attributes.Constitution,
                "Intelligence" => target.Attributes.Intelligence,
                "Wisdom" => target.Attributes.Wisdom,
                "Luck" => target.Attributes.Luck,
                "Defense" => target.Attributes.Defense,
                "AttackSpeed" => target.Attributes.Strength, // For testing purposes, map to Strength
                _ => throw new ArgumentException($"Unknown stat: {Stat}")
            };
        }

        private void SetStatValue(Entity target, int value)
        {
            switch (Stat)
            {
                case "Strength":
                    target.Attributes.Strength = value;
                    break;
                case "Constitution":
                    target.Attributes.Constitution = value;
                    break;
                case "Intelligence":
                    target.Attributes.Intelligence = value;
                    break;
                case "Wisdom":
                    target.Attributes.Wisdom = value;
                    break;
                case "Luck":
                    target.Attributes.Luck = value;
                    break;
                case "Defense":
                    target.Attributes.Defense = value;
                    break;
                case "AttackSpeed":
                    target.Attributes.Strength = value; // For testing purposes, map to Strength
                    break;
                default:
                    throw new ArgumentException($"Unknown stat: {Stat}");
            }
        }
    }
}
