#nullable enable
using System;
using System.Collections.Generic;

namespace PocketSquire.Arena.Core.Buffs
{
    /// <summary>
    /// Container for a buff that holds a collection of buff components.
    /// </summary>
    [Serializable]
    public class Buff
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public float Duration { get; set; }
        public List<IBuffComponent> Components { get; set; } = new List<IBuffComponent>();

        public Buff()
        {
        }

        public Buff(string id, string name, float duration)
        {
            Id = id;
            Name = name;
            Duration = duration;
        }

        /// <summary>
        /// Apply all components of this buff to the target entity.
        /// </summary>
        public void Apply(Entity target)
        {
            foreach (var component in Components)
            {
                component.OnApply(target);
            }
        }

        /// <summary>
        /// Tick all components of this buff (for periodic effects).
        /// </summary>
        public void Tick(Entity target, float deltaTime)
        {
            foreach (var component in Components)
            {
                component.OnTick(target, deltaTime);
            }
        }

        /// <summary>
        /// Remove all components of this buff from the target entity.
        /// </summary>
        public void Remove(Entity target)
        {
            foreach (var component in Components)
            {
                component.OnRemove(target);
            }
        }
    }
}
