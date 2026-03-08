namespace PocketSquire.Arena.Core.Perks
{
    // Separate from LevelUp/PerkEffectType.cs — that only covers LevelUp perks.
    public enum ArenaPerkEffectType
    {
        RestoreHP,
        RestoreMP,
        DamageBuff,
        DamageReduction,
        StackDamageBuff,
        StackDodgeBuff,
        BonusDamage,
        DoubleDamage,
        GuaranteedHit,
        NullifyDamage,
        ReduceCooldown,
        RefundMPCost,
        IncreaseMaxHP,
        ApplyThorns,
        SurviveFatalBlow,
        YieldBonus,
        IncreaseHitChance,
        IncreaseCritChance,
        ReduceShopPrices,
        IncreaseGoldGain,
        ReduceDamage,
        KeepMoney
    }
}
