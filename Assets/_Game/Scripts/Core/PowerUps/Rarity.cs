using System;

namespace PocketSquire.Arena.Core.PowerUps
{
    /// <summary>
    /// Rarity tiers for power-ups. Higher rarity = stronger scaling multiplier.
    /// </summary>
    [Serializable]
    public enum Rarity
    {
        Common = 0,
        Rare = 1,
        Epic = 2,
        Legendary = 3
    }
}
