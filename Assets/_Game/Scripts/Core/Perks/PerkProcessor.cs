#nullable enable
using System;
using System.Collections.Generic;

namespace PocketSquire.Arena.Core.Perks
{
    /// <summary>
    /// Stateless, static processor for arena perks.
    /// Callers pass in the player and a context; this class reads active perk definitions
    /// from GameWorld and runtime state from Player.PerkStates, then returns a result.
    /// No event bus — action classes call ProcessEvent() directly.
    /// </summary>
    public static class PerkProcessor
    {
        // -------------------------------------------------------------------
        // Public API
        // -------------------------------------------------------------------

        /// <summary>
        /// Processes all triggered perks that subscribe to <paramref name="triggerEvent"/> for the player.
        /// Updates PerkState in-place and returns aggregated results.
        /// </summary>
        public static PerkProcessResult ProcessEvent(PerkTriggerEvent triggerEvent, Player player, PerkContext context)
        {
            var result = new PerkProcessResult();
            if (player == null) return result;

            foreach (var perk in player.ActivePerks)
            {
                if (perk == null || perk.PerkType != PerkType.Triggered) continue;
                var perkId = perk.Id;
                if (perk.TriggerEvent != triggerEvent) continue;

                // Also reset consecutive if we hit a ResetOn event match
                // (handled separately by the reset-event overload for cleanness)

                if (!player.PerkStates.TryGetValue(perkId, out var state))
                {
                    state = new PerkState { PerkId = perkId };
                    player.PerkStates[perkId] = state;
                }

                // Gate: once-per-battle
                if (perk.OncePerBattle && state.HasTriggeredThisBattle) continue;

                // Gate: once-per-run
                if (perk.OncePerRun && state.HasTriggeredThisRun) continue;

                // Gate: consumeOnUse (e.g. guaranteed hit used up)
                if (perk.ConsumeOnUse && state.ConsumedThisBattle) continue;

                // Gate: HP threshold (e.g. "below 30% HP")
                if (perk.Threshold.HasValue && context.PlayerHpPercent >= perk.Threshold.Value) continue;

                // Gate: consecutive count (e.g. "after 3 consecutive hits")
                if (perk.ConsecutiveCount > 0)
                {
                    state.ConsecutiveCounter++;
                    if (state.ConsecutiveCounter < perk.ConsecutiveCount) continue;
                    state.ConsecutiveCounter = 0; // reset after trigger
                }

                // Proc chance roll
                if (perk.ProcPercent < 100 && context.Rng.Next(100) >= perk.ProcPercent) continue;

                // Apply effect
                ApplyEffect(perk, state, context, result, player);

                // Mark state — only gate flags that are actually guarded by these conditions
                if (perk.OncePerBattle) state.HasTriggeredThisBattle = true;
                if (perk.OncePerRun)    state.HasTriggeredThisRun    = true;
                if (perk.ConsumeOnUse)  state.ConsumedThisBattle     = true;
                if (!string.IsNullOrEmpty(perk.DisplayName))
                    result.TriggeredPerkName = perk.DisplayName;
            }

            // Handle ResetOn events — iterate perks that reset when THIS event fires
            foreach (var perk in player.ActivePerks)
            {
                var perkId = perk.Id;
                if (perk?.ResetOn == triggerEvent)
                {
                    if (player.PerkStates.TryGetValue(perkId, out var state))
                        state.ConsecutiveCounter = 0;
                }
            }

            return result;
        }

        /// <summary>
        /// Aggregates passive modifiers from all active Passive perks for the player.
        /// Called by AttackAction in the constructor to adjust hit/crit/damage before resolution.
        /// </summary>
        public static PerkProcessResult GetPassiveModifiers(Player player)
        {
            var result = new PerkProcessResult();
            if (player == null) return result;

            foreach (var perk in player.ActivePerks)
            {
                var perkId = perk.Id;
                if (perk == null || perk.PerkType != PerkType.Passive || !perk.Effect.HasValue) continue;

                switch (perk.Effect.Value)
                {
                    case PerkEffectType.IncreaseHitChance:
                        result.HitChanceBonusPercent += perk.Value;
                        break;
                    case PerkEffectType.IncreaseCritChance:
                        result.CritChanceBonusPercent += perk.Value;
                        break;
                    case PerkEffectType.DamageBuff:
                        result.DamageBuffMultiplier *= (1f + perk.Value / 100f);
                        break;
                    case PerkEffectType.DamageReduction:
                        result.DamageReductionMultiplier *= (1f - perk.Value / 100f);
                        break;
                    case PerkEffectType.ReduceShopPrices:
                        result.ReduceShopPrices *= (1f - perk.Value / 100f);
                        break;
                    case PerkEffectType.IncreaseGoldGain:
                        result.GoldGainMultiplier *= (1f + perk.Value / 100f);
                        break;
                }
            }

            return result;
        }

        /// <summary>Resets all active perk states for a new battle.</summary>
        public static void ResetPerksForBattle(Player player)
        {
            if (player == null) return;
            foreach (var state in player.PerkStates.Values)
                state.ResetForBattle();
        }

        /// <summary>Resets all active perk states when the player returns home (run ends).</summary>
        public static void ResetPerksForRun(Player player)
        {
            if (player == null) return;
            foreach (var state in player.PerkStates.Values)
                state.ResetForRun();
        }

        /// <summary>Ticks down duration on all perk states. Call once per turn change.</summary>
        public static void TickDuration(Player player)
        {
            if (player == null) return;
            foreach (var state in player.PerkStates.Values)
            {
                if (state.RemainingDuration > 0)
                    state.RemainingDuration--;
            }
        }

        // -------------------------------------------------------------------
        // Private helpers
        // -------------------------------------------------------------------

        private static void ApplyEffect(Perk perk, PerkState state, PerkContext context, PerkProcessResult result, Player player)
        {
            if (!perk.Effect.HasValue) return;

            switch (perk.Effect.Value)
            {
                case PerkEffectType.RestoreHP:
                {
                    int healAmt = perk.IsPercent
                        ? (int)(player.MaxHealth * (perk.Value / 100f))
                        : perk.Value;
                    result.HealAmount += healAmt;
                    player.Heal(healAmt);
                    Console.WriteLine($"[Perk] {perk.DisplayName}: Healed player for {healAmt} HP.");
                    break;
                }
                case PerkEffectType.RestoreMP:
                {
                    result.RestoreMpAmount += perk.Value;
                    player.RestoreMana(perk.Value);
                    Console.WriteLine($"[Perk] {perk.DisplayName}: Restored {perk.Value} MP.");
                    break;
                }
                case PerkEffectType.DamageBuff:
                {
                    float mult = 1f + perk.Value / 100f;
                    result.DamageBuffMultiplier *= mult;
                    state.RemainingDuration = perk.Duration > 0 ? perk.Duration : 0;
                    Console.WriteLine($"[Perk] {perk.DisplayName}: Damage buff x{mult:F2} for {perk.Duration} turns.");
                    break;
                }
                case PerkEffectType.DamageReduction:
                {
                    float mult = 1f - perk.Value / 100f;
                    result.DamageReductionMultiplier *= mult;
                    state.RemainingDuration = perk.Duration > 0 ? perk.Duration : 0;
                    Console.WriteLine($"[Perk] {perk.DisplayName}: Damage reduction x{mult:F2} for {perk.Duration} turns.");
                    break;
                }
                case PerkEffectType.StackDamageBuff:
                {
                    if (state.CurrentStacks < perk.MaxStacks)
                        state.CurrentStacks++;
                    float mult = 1f + (perk.Value * state.CurrentStacks) / 100f;
                    result.DamageBuffMultiplier *= mult;
                    Console.WriteLine($"[Perk] {perk.DisplayName}: Stack {state.CurrentStacks}/{perk.MaxStacks}, buff x{mult:F2}.");
                    break;
                }
                case PerkEffectType.StackDodgeBuff:
                {
                    if (state.CurrentStacks < perk.MaxStacks)
                        state.CurrentStacks++;
                    result.HitChanceBonusPercent -= perk.Value * state.CurrentStacks; // Negative = harder to hit player
                    state.RemainingDuration = perk.Duration > 0 ? perk.Duration : 0;
                    Console.WriteLine($"[Perk] {perk.DisplayName}: Dodge stack {state.CurrentStacks}, dodge boost {perk.Value * state.CurrentStacks}%.");
                    break;
                }
                case PerkEffectType.BonusDamage:
                {
                    int bonus = perk.IsPercent
                        ? (int)(context.Damage * (perk.Value / 100f))
                        : perk.Value;
                    result.BonusDamageFlat += bonus;
                    Console.WriteLine($"[Perk] {perk.DisplayName}: +{bonus} bonus damage.");
                    break;
                }
                case PerkEffectType.DoubleDamage:
                    result.ShouldDoubleDamage = true;
                    Console.WriteLine($"[Perk] {perk.DisplayName}: Double damage!");
                    break;
                case PerkEffectType.GuaranteedHit:
                    result.GuaranteedHit = true;
                    Console.WriteLine($"[Perk] {perk.DisplayName}: Next attack guaranteed to hit.");
                    break;
                case PerkEffectType.NullifyDamage:
                    result.NullifyDamage = true;
                    Console.WriteLine($"[Perk] {perk.DisplayName}: Damage nullified!");
                    break;
                case PerkEffectType.SurviveFatalBlow:
                    result.SurviveFatalBlow = true;
                    // Health is set to 1 by Entity.TakeDamage when the wouldDieCheck callback returns true.
                    Console.WriteLine($"[Perk] {perk.DisplayName}: Survived fatal blow with 1 HP!");
                    break;
                case PerkEffectType.IncreaseMaxHP:
                {
                    player.MaxHealth += perk.Value;
                    Console.WriteLine($"[Perk] {perk.DisplayName}: MaxHP +{perk.Value}.");
                    break;
                }
                case PerkEffectType.YieldBonus:
                    result.YieldChanceBonus += perk.YieldChanceBonus;
                    if (perk.HpRestore > 0)
                    {
                        int healAmt = perk.IsPercent
                            ? (int)(player.MaxHealth * (perk.HpRestore / 100f))
                            : perk.HpRestore;
                        result.HealAmount += healAmt;
                        player.Heal(healAmt);
                    }
                    Console.WriteLine($"[Perk] {perk.DisplayName}: Yield bonus +{perk.YieldChanceBonus}%.");
                    break;
                case PerkEffectType.ReduceCooldown:
                    // Cooldown is a Unity-side concept; result carries the value for the Unity layer.
                    Console.WriteLine($"[Perk] {perk.DisplayName}: ReduceCooldown by {perk.Value}.");
                    break;
                case PerkEffectType.RefundMPCost:
                    result.RestoreMpAmount += perk.Value;
                    Console.WriteLine($"[Perk] {perk.DisplayName}: MP refunded.");
                    break;
                default:
                    Console.WriteLine($"[Perk] {perk.DisplayName}: Unhandled effect {perk.Effect.Value}.");
                    break;
            }
        }
    }
}
