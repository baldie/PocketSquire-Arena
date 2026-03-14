# AGENTS.md — Perks System

**Location:** `Assets/_Game/Scripts/Core/Perks/`

---

## What Perks Are

Perks are **persistent, purchasable upgrades** the player buys from vendors in town and equips to active slots. Their definitions live in `Assets/_Game/Data/arena_perks.json` and are loaded into `GameWorld.AllPerks`. Effect resolution is centered in `PerkProcessor`, while event wiring and activation rules live in the surrounding core and Unity flow.

Perks are distinct from **Power-Ups** (run-based procedural cards) and **stat points** (level-up allocations). See the PowerUps AGENTS.md for the other system.

---

## Two Perk Types

### `Passive`
Always-on modifiers. Read once per action via `PerkProcessor.GetPassiveModifiers(player)`. No trigger event, no state tracking. Examples: `keen_eye` (+hit chance), `battle_hardened` (damage reduction), `treasure_hunter` (+gold gain).

### `Triggered`
Fire in response to a specific `PerkTriggerEvent`. Have full state tracking (stacks, duration, once-per-battle flags). Examples: `second_wind` (heal at low HP), `warriors_resolve` (stack damage on consecutive hits), `phoenix_heart` (survive killing blow once per run).

---

## Data Model (`Perk.cs`)

Every perk is a flat JSON object. Key fields:

| Field | Purpose |
|---|---|
| `id` | Unique string key. Used everywhere as the canonical reference. |
| `type` | `"Passive"` or `"Triggered"` |
| `soldBy` | Which vendor sells it: `Shopkeeper`, `Wizard`, `FightersBlacksmith`, `ArcheryTrainer` |
| `event` | `PerkTriggerEvent` that activates this perk (Triggered only) |
| `effect` | `PerkEffectType` describing what happens when triggered |
| `value` | Numeric magnitude of the effect |
| `isPercent` | If true, `value` is treated as a percentage of max HP/damage rather than a flat amount |
| `procPercent` | 0–100 chance the effect fires when triggered. 100 = always. |
| `threshold` | HP% ceiling — perk only fires when player HP is below this value |
| `oncePerBattle` | Effect fires at most once per battle |
| `oncePerRun` | Effect fires at most once per run |
| `consumeOnUse` | Effect is consumed after firing (like a one-shot) |
| `maxStacks` | Maximum number of stacks for stack-based effects |
| `consecutiveCount` | How many consecutive triggers required before the effect fires |
| `resetOn` | `PerkTriggerEvent` that resets the consecutive counter |
| `duration` | How many turns a buff/debuff effect lasts |
| `prerequisites.minLevel` | Player must be at least this level to activate the perk |
| `prerequisites.class` | Class restriction checked by activation logic |
| `prerequisites.requiredPerks` | List of perk IDs the player must already own |

---

## Perk Lifecycle

```
Player buys perk (TryPurchasePerk)
    → added to Player.AcquiredPerks (owned, not yet active)

Player activates perk (TryActivatePerk)
    → added to Player.ActivePerks
    → PerkState created in Player.PerkStates[perkId]
    → Inventory capacity recalculated (satchel perks)

Battle starts
    → `Battle` setup resets battle-scoped perk state

Each action fires
    → PerkProcessor.ProcessEvent(triggerEvent, player, context)
    → PerkProcessor.GetPassiveModifiers(player) where passive reads are needed

Turn changes
    → PerkProcessor.TickDuration() — decrements RemainingDuration on all states

Run ends (player returns home)
    → PerkProcessor.ResetPerksForRun() — clears run-scoped state including HasTriggeredThisRun

Save load
    → `GameState.LoadFromSaveData()` rebuilds runtime perk state via `InitializePerkStates()`
```

---

## `PerkProcessor.cs` — Central Effect Resolution

`PerkProcessor` is a **stateless static class**. It reads active perk definitions and runtime state from the player and returns aggregated results while also applying certain immediate effects directly to the player. It never owns long-lived state itself.

### `ProcessEvent(triggerEvent, player, context)`
Iterates `player.ActivePerks`, finds Triggered perks matching `triggerEvent`, runs all gate checks, then calls `ApplyEffect()` for those that pass. Returns a `PerkProcessResult` aggregating all effects from this call.

**Gate checks run in this order:**
1. Perk type must be `Triggered`
2. `TriggerEvent` must match
3. `OncePerBattle` — skip if already triggered this battle
4. `OncePerRun` — skip if already triggered this run
5. `ConsumeOnUse` — skip if already consumed this battle
6. `Threshold` — skip if player HP% is not below threshold
7. `ConsecutiveCount` — increment counter; skip if not yet reached
8. `ProcPercent` — random roll; skip if fails

### `GetPassiveModifiers(player)`
Iterates `player.ActivePerks`, collects all Passive perk contributions into a single `PerkProcessResult`. It is used by combat resolution and other reward / economy flows that need passive modifiers.

---

## `PerkProcessResult`

The return value of any `PerkProcessor` call. Some effects are applied immediately inside `PerkProcessor` itself (for example healing, mana restore, or max-health changes), while other flags are returned for callers to honor during combat and reward resolution.

Key fields agents should know:
- `SurviveFatalBlow` — checked inside `Entity.TakeDamage()` callback
- `NullifyDamage` — checked in the attack action before calling `TakeDamage`
- `GuaranteedHit` — checked during shared attack resolution before hit roll
- `DamageBuffMultiplier` / `DamageReductionMultiplier` — applied to damage before `TakeDamage`
- `GoldGainMultiplier` — applied in `WinAction` when awarding gold
- `HitChanceBonusPercent` / `CritChanceBonusPercent` — added to calculated values in `CombatCalculator`

---

## `PerkState`

Runtime-only (never serialized). One exists per active perk in `Player.PerkStates`. Rebuilt from `Player.ActivePerks` by `InitializePerkStates()` on game load.

Fields: `CurrentStacks`, `RemainingDuration`, `HasTriggeredThisBattle`, `HasTriggeredThisRun`, `ConsecutiveCounter`, `ConsumedThisBattle`.

---

## `PerkContext`

Passed into every `ProcessEvent` call. Carries contextual data the processor needs:
- `Damage` — damage relevant to this action
- `PlayerHpPercent` — current HP as 0–100 integer (used for threshold checks)
- `DidHit`, `IsCrit` — attack resolution results
- `Rng` — injectable random for deterministic testing
- `Player`, `Target` — the entities involved

Always construct a fresh `PerkContext` per `ProcessEvent` call. Do not reuse across multiple calls.

---

## Event Wiring

Events are fired by the surrounding action flow, not by `PerkProcessor` itself.

Currently wired core events include:
- shared attack-resolution events such as `PlayerAttackedMonster`, `PlayerHitMonster`, `PlayerMissedMonster`, `MonsterMissedPlayer`, `MonsterAttackHitPlayer`, `SpecialAttackLanded`, `SpecialAttackMissed`, and `WouldDie`
- action-driven events such as `PlayerDefended`, `PlayerUsedItem`, `PlayerAttemptedYield`, `BattleWon`, `BattleLost`, `PlayerTurnStarted`, and `PlayerTurnEnded`

Other enum values may exist for future content, but they do nothing until some caller explicitly fires them.

---

## Perk Slot Limits

Slot count is driven by class tier: `(tier + 1) * 2`. Squire = 2 slots, Prestige = 10 slots. Satchel perks expand **inventory** capacity, not perk slots. These are separate limits.

`Player.CanActivatePerk()` / `TryActivatePerk()` enforce prerequisites, slot limits, and satchel conflict rules (only one satchel perk active at a time). Use the player APIs rather than duplicating those checks in UI code.

---

## Adding a New Perk

1. Add an entry to `arena_perks.json` following the existing schema
2. If the effect type is new, add a value to `PerkEffectType` enum
3. If the trigger event is new, add a value to `PerkTriggerEvent` enum and fire it from the appropriate action class
4. Add a `case` to `PerkProcessor.ApplyEffect()` for the new `PerkEffectType`
5. If it's a passive modifier, add a `case` to `PerkProcessor.GetPassiveModifiers()` as well
6. If it affects a value the Unity layer needs to display (e.g., shop price reduction), add a field to `PerkProcessResult` and read it in the Unity layer

Many perks are data-driven, but new behavior still requires the surrounding wiring to exist: a fired trigger event, a handled effect type, and any caller-side consumption of returned result fields.

---

## Common Mistakes

- **Don't read `perk.Id` before the null check.** The null guard `if (perk == null) continue` must come before any property access on the perk.
- **Don't create perks with both `oncePerBattle` and `consumeOnUse`.** They overlap in intent and the interaction is untested. Pick one.
- **Don't fire `WouldDie` directly** — it's fired via a callback inside `Entity.TakeDamage()`. Pass the callback from the attack action; don't call `ProcessEvent(WouldDie)` yourself.
- **`GetPassiveModifiers` is not cached.** It iterates all active perks on every call. Don't call it in a tight loop — call it once per action resolution and store the result.
- **`PerkStates` must be rebuilt after load.** After deserializing a Player, always call `InitializePerkStates()`. Without it, triggered perk gate checks will fail silently.