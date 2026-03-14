# AGENTS.md - Battle System

**Location:** `Assets/_Game/Scripts/Core/`

## Purpose

This folder owns the framework-agnostic battle model. The system is action-based and queue-driven:

- Core code creates `IGameAction` instances and mutates state through them.
- The Unity layer consumes the queue, plays presentation, and should not be the source of truth for battle state.
- After each action resolves, `Battle.DetermineNextAction()` decides what happens next until a win or loss action ends the loop.

## Core Invariants

### `IGameAction`

Every action exposes `Type`, `Actor`, `Target`, and `ApplyEffect()`.

- Put observable state changes in `ApplyEffect()`.
- Constructors may pre-resolve data that must stay stable while the action waits in the queue.
- If queue-time data is snapshotted, make sure delayed execution cannot make it misleading.

### `Battle`

`Battle` owns the combatants and current turn.

- `DetermineNextAction()` is the battle flow controller.
- `CurrentTurn = null` means the battle is over.
- Let the queue processor drive progression; do not short-circuit the flow from arbitrary callers.

### `Turn`

`Turn` is intentionally lightweight: `Actor` acts on `Target`, and `IsPlayerTurn` is derived from the acting entity.

## Responsibility Boundaries

### Combat math

Keep combat formulas in `CombatCalculator`.

- Damage, hit chance, crit chance, defense, and defend reduction belong there.
- If combat needs effective stats, read them through `CombatUtilities` so temporary modifiers stay centralized.
- If tuning changes, prefer shared helpers or constants over action-specific math.

### Entities

`Entity` owns shared combat state such as HP, mana, inventory, attributes, and defending state.

- `Player` adds progression, class-driven behavior, perks, and special-attack affordability.
- `Monster` adds AI-specific behavior and combat identity.
- `Entity.TakeDamage()` is the correct interception point for fatal-blow handling and similar last-second effects.

### Resources

- Mana is a caster-only concern. Physical classes should behave as if mana does not exist.
- Max health derives from class/base health plus Constitution. Recalculate when those inputs change, but avoid overwriting serialized state on load unless that load path is intentionally rebuilding it.

## Action Families

The current action set covers the full battle loop:

- Attack and special attack actions resolve shared combat flow, then fire the relevant perk events.
- Defend, item, and yield actions mutate state and publish their own trigger points.
- Turn-change actions advance the loop and handle upkeep such as perk durations and mana regen.
- Win and loss actions handle battle-end consequences and events.

Preserve this pattern when extending the system: core owns intent and state, Unity owns presentation, and cross-system hooks stay explicit.

## Attack Flow

Attack-style actions should keep this order of responsibility:

1. Gather perk or passive modifiers that affect resolution.
2. Use `CombatCalculator` to resolve the combat outcome.
3. In `ApplyEffect()`, publish hit/miss and follow-up events.
4. Route damage through `Entity.TakeDamage()` so defend logic and fatal-blow interception stay centralized.

## Extending The System

When adding a new action:

1. Add or update the `IGameAction` implementation in `Assets/_Game/Scripts/Core/`.
2. Extend `ActionType` if Unity needs a distinct presentation path.
3. Keep state mutation in `ApplyEffect()`.
4. Fire any needed `PerkTriggerEvent` values from the action itself.
5. Update Unity-side visuals in `ActionQueueProcessor` or the equivalent presentation hook.

## Common Mistakes

- Do not spread combat formulas across action classes.
- Do not mutate battle state from Unity presentation code.
- Do not call `Battle.DetermineNextAction()` from arbitrary code paths; let the queue lifecycle control it.
- Do not skip perk or event hooks just because an attack missed; misses are still meaningful state transitions.
