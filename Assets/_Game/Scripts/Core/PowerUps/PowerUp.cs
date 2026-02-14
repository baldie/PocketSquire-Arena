#nullable enable
using System;

namespace PocketSquire.Arena.Core.PowerUps
{
    /// <summary>
    /// Wrapper for a single power-up component. Represents a "card" the player can pick.
    /// </summary>
    [Serializable]
    public class PowerUp
    {
        public PowerUpComponent Component { get; private set; }

        public string DisplayName => Component.DisplayName;
        public string Description => Component.Description;
        public string UniqueKey => Component.UniqueKey;
        public PowerUpRank Rank => Component.Rank;

        public PowerUp(PowerUpComponent component)
        {
            Component = component ?? throw new ArgumentNullException(nameof(component));
        }

        /// <summary>
        /// Increments the rank of this power-up (up to Rank III).
        /// </summary>
        public void IncrementRank()
        {
            if (Component.Rank < PowerUpRank.III)
            {
                Component.Rank = (PowerUpRank)((int)Component.Rank + 1);
            }
        }

        /// <summary>
        /// Returns true if this power-up is at max rank.
        /// </summary>
        public bool IsMaxRank() => Component.Rank == PowerUpRank.III;
    }
}
