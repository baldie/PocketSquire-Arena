using System.Collections.Generic;
using Newtonsoft.Json;

namespace PocketSquire.Arena.Core.Perks
{
    // arena_perks.json is wrapped in { "perks": [...] } — not a raw array.
    public class PerkWrapper
    {
        [JsonProperty("perks")]
        public List<Perk> Perks { get; set; } = new List<Perk>();
    }
}
