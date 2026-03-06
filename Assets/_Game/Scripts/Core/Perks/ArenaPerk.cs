#nullable enable
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace PocketSquire.Arena.Core.Perks
{
    /// <summary>
    /// Represents a purchasable arena perk loaded from arena_perks.json.
    /// Implements IMerchandise so it can be displayed in ShopController with no extra plumbing.
    /// </summary>
    public class ArenaPerk : PocketSquire.Arena.Core.IMerchandise
    {
        [JsonProperty("id")]
        public string Id { get; set; } = string.Empty;

        // IMerchandise.DisplayName maps to JSON "name"
        [JsonProperty("name")]
        public string DisplayName { get; set; } = string.Empty;

        [JsonProperty("description")]
        public string Description { get; set; } = string.Empty;

        [JsonProperty("icon")]
        public string? Icon { get; set; }

        [JsonProperty("type")]
        [JsonConverter(typeof(StringEnumConverter))]
        public ArenaPerkType PerkType { get; set; }

        // IMerchandise.Price delegates to Cost
        [JsonProperty("cost")]
        public int Cost { get; set; }
        public int Price => Cost;

        [JsonProperty("soldBy")]
        [JsonConverter(typeof(StringEnumConverter))]
        public VendorType Vendor { get; set; }

        [JsonProperty("tier")]
        public int Tier { get; set; }

        [JsonProperty("prerequisites")]
        public ArenaPerkPrerequisites? Prerequisites { get; set; }

        // Triggered-perk fields — nullable because Passive perks omit them
        [JsonProperty("event")]
        [JsonConverter(typeof(StringEnumConverter))]
        public PerkTriggerEvent? TriggerEvent { get; set; }

        [JsonProperty("effect")]
        [JsonConverter(typeof(StringEnumConverter))]
        public ArenaPerkEffectType? Effect { get; set; }

        [JsonProperty("perkTarget")]
        public string? PerkTarget { get; set; }

        [JsonProperty("procPercent")]
        public int ProcPercent { get; set; } = 100;

        [JsonProperty("value")]
        public int Value { get; set; }

        [JsonProperty("isPercent")]
        public bool IsPercent { get; set; }

        [JsonProperty("threshold")]
        public int? Threshold { get; set; }

        [JsonProperty("maxStacks")]
        public int MaxStacks { get; set; }

        [JsonProperty("consecutiveCount")]
        public int ConsecutiveCount { get; set; }

        [JsonProperty("duration")]
        public int Duration { get; set; }

        [JsonProperty("oncePerBattle")]
        public bool OncePerBattle { get; set; }

        [JsonProperty("oncePerRun")]
        public bool OncePerRun { get; set; }

        [JsonProperty("consumeOnUse")]
        public bool ConsumeOnUse { get; set; }

        [JsonProperty("resetOn")]
        [JsonConverter(typeof(StringEnumConverter))]
        public PerkTriggerEvent? ResetOn { get; set; }

        [JsonProperty("yieldChanceBonus")]
        public int YieldChanceBonus { get; set; }

        [JsonProperty("hpRestore")]
        public int HpRestore { get; set; }
    }
}
