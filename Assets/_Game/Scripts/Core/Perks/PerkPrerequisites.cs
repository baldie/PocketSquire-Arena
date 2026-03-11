#nullable enable
using System.Collections.Generic;
using Newtonsoft.Json;

namespace PocketSquire.Arena.Core.Perks
{
    public class PerkPrerequisites
    {
        [JsonProperty("minLevel")]
        public int MinLevel { get; set; } = 1;

        [JsonProperty("class")]
        public string? ClassName { get; set; }

        [JsonProperty("requiredPerks")]
        public List<string>? RequiredPerks { get; set; }
    }
}
