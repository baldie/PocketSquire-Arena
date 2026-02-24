namespace PocketSquire.Arena.Core.LevelUp
{
    /// <summary>
    /// Describes the mechanical effect a Perk applies to the Player when unlocked.
    /// New effect types should be added here as the game grows.
    /// </summary>
    public enum PerkEffectType
    {
        /// <summary>No gameplay effect (cosmetic/tracking perks).</summary>
        None,

        /// <summary>Expands the player's inventory slot and stack capacity.</summary>
        InventoryExpansion,
    }
}
