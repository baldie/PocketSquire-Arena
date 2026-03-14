# AGENTS.md - Perks System

**Location:** `Assets/_Game/Scripts/Core/Perks/`

## Purpose

Perks are persistent upgrades the player acquires in town and activates into limited slots. They are distinct from:

- Power-ups, which are run-scoped and generated after arena wins.
- Stat points, which are permanent progression allocations.

Perk definitions live in `Assets/_Game/Data/arena_perks.json`, runtime state lives on `Player`, and effect resolution is centered in `PerkProcessor`.

## Mental Model

There are two perk categories:

- `Passive`: always-on modifiers that are read when a system asks for them.
- `Triggered`: effects that listen for a `PerkTriggerEvent` and rely on runtime state such as duration, stacks, or once-per-battle/run flags.

Treat perks as a mostly data-driven system with explicit wiring. Data defines what a perk wants to do, but code still decides when events fire, how effects are applied, and where returned results are consumed.

## Ownership Boundaries

### `PerkProcessor`

`PerkProcessor` is the central resolver.

- It is stateless; long-lived perk state does not live there.
- It reads `player.ActivePerks` and `Player.PerkStates`.
- It applies some immediate effects directly and returns a `PerkProcessResult` for callers to honor.

### `PerkState`

`PerkState` is runtime-only and exists per active perk.

- Use it for stacks, durations, consecutive counters, once-per-battle/run flags, and other transient activation state.
- Rebuild it from active perks after loading save data via `InitializePerkStates()`.

### `PerkProcessResult`

`PerkProcessResult` is the contract between perk resolution and callers.

- Use it for values the caller must still apply, such as combat modifiers, reward modifiers, or damage-interception flags.
- If Unity or another caller needs to react to a perk result, add a field here rather than bypassing the processor contract.

### `PerkContext`

Create a fresh `PerkContext` per `ProcessEvent()` call.

- It should carry only the action-specific data needed for gating or resolution.
- Do not reuse a context instance across separate events.

## Lifecycle

The high-level perk flow is:

1. Purchase adds a perk to owned/acquired data.
2. Activation moves it into the active set and creates runtime state.
3. Battle setup resets battle-scoped state.
4. Actions and turn changes fire events into `PerkProcessor`.
5. Returning home resets run-scoped state.
6. Loading a save must rebuild runtime state from active perks.

## Activation Rules

Keep activation checks in player-facing APIs such as `Player.CanActivatePerk()` and `TryActivatePerk()`, not in UI code.

Important boundaries:

- Slot limits are class-tier driven.
- Satchel-style perks affect inventory capacity, not perk slot count.
- Prerequisites such as level, class, or required perks should be enforced through the player/perk activation flow, not duplicated ad hoc.

## Event Processing

`ProcessEvent()` should stay predictable:

- Match the correct perk type and trigger event first.
- Then apply gating such as once-per-battle, once-per-run, consumed state, thresholds, streak requirements, and proc chance.
- Finally apply the perk effect and aggregate the result.

The exact set of events can evolve. What matters is that events are fired by the surrounding battle/action flow, not invented silently inside `PerkProcessor`.

## Wiring Rules

- Passive effects must be explicitly read by the systems that care about them.
- Triggered effects only matter if some caller fires the relevant `PerkTriggerEvent`.
- Adding a new perk effect often requires changes in more than one place: data, enum values if needed, processor handling, event firing, and caller-side consumption of the returned result.

## Data Guidance

The JSON schema should stay focused on durable concepts:

- Identity and vendor/source.
- Passive vs triggered behavior.
- Effect type and value data.
- Gating and activation rules such as thresholds, proc chance, and prerequisites.
- Optional runtime behaviors such as stacks, durations, streaks, or single-use flags.

If a new perk needs behavior that cannot be expressed with the existing schema, extend the code path deliberately instead of overloading unrelated fields.

## Extending The System

When adding a new perk:

1. Add or update the perk data in `arena_perks.json`.
2. Add enum values only if the effect or trigger is genuinely new.
3. Teach `PerkProcessor` how to apply the new effect.
4. Fire the corresponding trigger event from the correct battle/action location.
5. If the caller must honor the result, expose that through `PerkProcessResult`.

## Common Mistakes

- Do not treat `PerkProcessor` as stateful; runtime state belongs on the player.
- Do not fire `WouldDie` or similar late-stage events from arbitrary code paths if the damage pipeline already owns that hook.
- Do not call `GetPassiveModifiers()` repeatedly inside tight loops; resolve once per action or flow where possible.
- Do not forget to rebuild `PerkStates` after loading a player, or triggered perks will behave inconsistently.
- Do not duplicate activation or prerequisite checks in UI code when player APIs already own them.
