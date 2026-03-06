using System.Collections.Generic;
using Newtonsoft.Json;

namespace PocketSquire.Arena.Core.Perks
{
    // arena_perks.json is wrapped in { "perks": [...] } — not a raw array.
    public class ArenaPerkWrapper
    {
        [JsonProperty("perks")]
        public List<ArenaPerk> Perks { get; set; } = new List<ArenaPerk>();
    }
}
