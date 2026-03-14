#nullable enable

namespace PocketSquire.Arena.Core.Perks
{
    /// <summary>
    /// Aggregated output of one PerkProcessor call.
    /// Callers inspect these flags and apply them AFTER the core action logic.
    /// </summary>
    public class PerkProcessResult
    {
        // --- Damage modifications ---
        public int BonusDamageFlat { get; set; }
        public float DamageMultiplier { get; set; } = 1f;
        public bool ShouldDoubleDamage { get; set; }
        public bool GuaranteedHit { get; set; }
        public bool NullifyDamage { get; set; }

        // --- Healing ---
        public int HealAmount { get; set; }
        public int RestoreMpAmount { get; set; }

        // --- Survival ---
        public bool SurviveFatalBlow { get; set; }

        // --- Passive stat bonuses (accumulated across all passive perks) ---
        public int HitChanceBonusPercent { get; set; }
        public int CritChanceBonusPercent { get; set; }
        public float DamageBuffMultiplier { get; set; } = 1f;
        public float DamageReductionMultiplier { get; set; } = 1f;
        public float ReduceShopPrices { get; set; } = 1f;
        public float GoldGainMultiplier { get; set; } = 1f;
        public int KeepMoneyPercent { get; set; }

        // --- Yield ---
        public int YieldChanceBonus { get; set; }

        // --- Status text (for UI feedback) ---
        public string? TriggeredPerkName { get; set; }

        public bool HasAnyEffect =>
            BonusDamageFlat != 0 || DamageMultiplier != 1f || ShouldDoubleDamage ||
            GuaranteedHit || NullifyDamage || SurviveFatalBlow ||
            HealAmount > 0 || RestoreMpAmount > 0 ||
            HitChanceBonusPercent != 0 || CritChanceBonusPercent != 0 ||
            YieldChanceBonus != 0 || KeepMoneyPercent != 0;
    }
}
