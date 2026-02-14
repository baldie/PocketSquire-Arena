#nullable enable
using System.Collections.Generic;

namespace PocketSquire.Arena.Core.LevelUp
{
    /// <summary>
    /// A collection of perks that can be drawn from dynamically.
    /// Tag is used to identify the pool (e.g., "Tier1", "Combat").
    /// </summary>
    public class PerkPool
    {
        public string Tag { get; set; }
        public List<Perk> Perks { get; set; } = new List<Perk>();

        public PerkPool(string tag, List<Perk>? perks = null)
        {
            Tag = tag;
            Perks = perks ?? new List<Perk>();
        }
    }
}
