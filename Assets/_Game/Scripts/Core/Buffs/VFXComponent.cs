#nullable enable
using System;

namespace PocketSquire.Arena.Core.Buffs
{
    /// <summary>
    /// Handles spawning and despawning visual effects (particles, auras, etc.) on the target.
    /// The actual VFX spawning is handled by the Unity layer - this just stores the effect ID.
    /// </summary>
    [Serializable]
    public class VFXComponent : IBuffComponent
    {
        public string EffectId { get; set; } = string.Empty;

        public VFXComponent()
        {
        }

        public VFXComponent(string effectId)
        {
            EffectId = effectId;
        }

        public void OnApply(Entity target)
        {
            // VFX spawning is handled by the Unity layer
            // This core logic just stores the effect ID
            // Unity can subscribe to buff events or query active buffs to spawn VFX
        }

        public void OnTick(Entity target, float deltaTime)
        {
            // VFX doesn't need periodic updates in core logic
        }

        public void OnRemove(Entity target)
        {
            // VFX despawning is handled by the Unity layer
            // The Unity layer can detect when a buff is removed and despawn the effect
        }
    }
}
