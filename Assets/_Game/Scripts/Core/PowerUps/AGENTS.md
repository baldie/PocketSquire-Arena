# AGENTS.md - Power-Ups System

**Location:** `Assets/_Game/Scripts/Core/PowerUps/`

## Purpose

Power-ups are run-scoped upgrades offered after arena victories. They are distinct from:

- Perks, which are purchased and persist across runs.
- Stat points, which are permanent progression choices.

Power-ups belong to the current `Run`, not the base `Player`, and should be treated as temporary modifiers layered on top of core progression.

## Mental Model

The system is component-driven:

- A `PowerUp` wraps a single `PowerUpComponent`.
- The component defines the actual effect.
- A `UniqueKey` identifies the effect family for duplicate detection and rank-up handling.

Current power-ups mainly fall into four buckets:

- Attribute modifiers that improve the player for the run.
- Loot modifiers that change rewards.
- Utility effects that apply after battle.
- Monster debuffs that apply at battle start.

## Core Invariants

- Power-ups are generated at runtime, not authored as static JSON content.
- Offers should contain distinct choices.
- Re-selecting the same power-up family should rank it up instead of storing duplicate effects.
- Rank and rarity are behavioral inputs, not cosmetic labels.

## Ownership Boundaries

### `PowerUpCollection`

`Run.PowerUps` is the source of truth for the current run.

- It owns acquisition, duplicate handling, and lookups.
- It is also the main API for applying battle-start debuffs, post-battle utility effects, and reward modifiers.

Call into the collection instead of re-implementing ownership or rank logic at the call site.

### `PowerUpFactory`

`PowerUpFactory` owns offer generation.

- It should produce the offer set and apply weighting rules.
- Context such as arena level, player luck, health state, and already-owned power-ups belongs in the generation input.
- New power-up families should usually be introduced by extending the template list, not by rewriting the factory flow.

### Scaling

Keep scaling logic centralized.

- Rank, rarity, and arena progression should flow through shared scaling helpers.
- If numbers need retuning, change the scaling rules in one place instead of hardcoding adjustments into components or callers.

### Effective player stats

Power-ups should not permanently mutate base player progression data.

- Use `PlayerWithPowerUps` or `CombatUtilities` to read effective run-time attributes.
- Treat run bonuses as overlays, not writes into `Player.Attributes`.

## Integration Points

Power-ups affect the game by being read at the right time:

- Battle setup applies monster debuffs.
- Combat math reads effective player attributes.
- Reward resolution reads gold or XP modifiers.
- Post-battle flow applies utility effects.

This system is intentionally pull-based: owning a power-up does nothing unless the relevant caller reads and applies it.

## Generation Guidance

When changing offer generation:

- Keep the contract simple and predictable: generate a small set of distinct choices.
- Let generation context influence weighting, not downstream mutation.
- Exclude or gracefully handle options that are already maxed out.
- Keep a safe fallback path so offer generation never fails outright.

## Extending The System

When adding a new power-up type:

1. Decide whether it fits an existing `PowerUpComponent` subclass.
2. If not, add a new component type with the behavior and metadata it needs.
3. Add a new factory or template entry so it can appear in offers.
4. Wire its effect into the correct gameplay read point.
5. If it changes how effective stats are computed, keep that logic centralized.

## Persistence

Power-ups are run-scoped. Do not assume they survive outside the current run unless explicit run persistence is added.

If persistence is introduced later, keep subtype serialization and collection reconstruction as first-class design concerns rather than bolting them on informally.

## Common Mistakes

- Do not write power-up bonuses into permanent player stats.
- Do not pre-apply battle-start debuffs if battle setup already owns that responsibility.
- Do not bypass `PowerUpCollection.Add()` with manual duplicate checks; rank-up behavior should stay centralized.
- Do not treat rank or rarity as display-only fields if scaling depends on them.
- Do not cache effective-attribute wrappers across contexts where arena level or owned power-ups may have changed.
