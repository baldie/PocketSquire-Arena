#nullable enable

namespace PocketSquire.Arena.Core.Perks
{
    public class PerkActivationRequest
    {
        public string PerkToRemoveId { get; set; } = string.Empty;
        public Perk? PerkToActivate { get; set; }
    }
}
