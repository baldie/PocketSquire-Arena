#nullable enable
namespace PocketSquire.Arena.Core.Buffs
{
    /// <summary>
    /// Interface defining the lifecycle of a buff effect component.
    /// </summary>
    public interface IBuffComponent
    {
        /// <summary>
        /// Called when the buff is applied to the target entity.
        /// </summary>
        /// <param name="target">The entity receiving the buff</param>
        void OnApply(Entity target);

        /// <summary>
        /// Called periodically while the buff is active (optional - implement if needed).
        /// </summary>
        /// <param name="target">The entity with the buff</param>
        /// <param name="deltaTime">Time elapsed since last tick</param>
        void OnTick(Entity target, float deltaTime);

        /// <summary>
        /// Called when the buff is removed from the target entity.
        /// Should revert any changes made in OnApply.
        /// </summary>
        /// <param name="target">The entity losing the buff</param>
        void OnRemove(Entity target);
    }
}
