\# AGENTS.md — Battle System



\*\*Location:\*\* `Assets/\_Game/Scripts/Core/`



\---



\## Architecture Overview



The battle system is \*\*action-based and queue-driven\*\*. All game state changes happen in discrete `IGameAction` objects. The Unity layer (`ActionQueueProcessor`) consumes the queue and plays visuals/audio; it never mutates game state directly.



The flow is:

```

Player input → BattleManager creates IGameAction → ActionQueueProcessor enqueues it

→ ApplyEffect() mutates state → Battle.DetermineNextAction() returns the next action

→ repeat until Win/Lose

```



\---



\## Key Contracts



\### `IGameAction`

Every action implements three properties and one method:

\- `ActionType Type` — used by the Unity layer to look up sprites/audio

\- `Entity Actor` — who is performing the action

\- `Entity Target` — who is receiving the action

\- `void ApplyEffect()` — \*\*all game state changes live here\*\*



\*\*Rule:\*\* Constructors may snapshot or pre-resolve data that should stay stable while the action sits in the queue, but observable state changes belong in `ApplyEffect()`.



\### `Battle`

Owns `Player1`, `Player2`, and `CurrentTurn`. After each action completes, `Battle.DetermineNextAction(action)` decides what fires next — monster AI, turn changes, win/lose. Setting `CurrentTurn = null` signals the battle is over.



\### `Turn`

A lightweight struct. `Actor` is who acts, `Target` is who they act on. `IsPlayerTurn` is true when `Actor` is a `Player`.



\---



\## Action Reference



| Class | ActionType | What it does |

|---|---|---|

| `AttackAction` | Attack | Regular attack action built on the shared resolved-attack flow. Uses `CombatCalculator` and fires the relevant attack perk events. |

| `SpecialAttackAction` | SpecialAttack | Special attack built on the same shared resolved-attack flow, with higher damage / lower accuracy and optional mana spend for mana-using classes. |

| `DefendAction` | Defend | Sets `IsDefending = true` on actor. Fires `PlayerDefended` perk event. |

| `ItemAction` | Item | Applies item effect, removes item from inventory. Fires `PlayerUsedItem` perk event. |

| `YieldAction` | Yield | Fires `PlayerAttemptedYield` perk event. Unity layer handles the confirmation dialog. |

| `ChangeTurnsAction` | ChangeTurns | Flips the active turn. Ticks perk durations. Regenerates caster mana. Fires `PlayerTurnStarted`/`Ended` events. |

| `WinAction` | Win | Awards rewards, applies post-battle utility effects, advances arena rank, and fires `BattleWon`. |

| `LoseAction` | Lose | Fires `BattleLost` so loss-reactive perks can run. |



\---



\## Combat Math (`CombatCalculator.cs`)



All core combat formulas live in `CombatCalculator`. Keep damage, hit, crit, defense, and defend math centralized there.



The calculator determines attack style, base damage, hit chance, crit chance, defense reduction, and defend reduction. It also reads effective combat stats through `CombatUtilities`, so player power-up attribute bonuses are folded into combat math centrally rather than in individual action classes.



All tuning values are exposed as named constants in `CombatCalculator.cs`. Adjust those constants rather than spreading formula tweaks across multiple actions.



\---



\## Damage Pipeline (Attack)



```

shared attack constructor resolves outcome

&#x20;   → passive / pre-hit triggered perk modifiers are gathered

&#x20;   → `CombatCalculator` resolves damage, hit, crit, and defense reduction

&#x20;   → `ApplyEffect()` handles hit or miss events

&#x20;   → incoming monster-hit perks may reduce or nullify player damage

&#x20;   → `Entity.TakeDamage()` applies defend reduction and `WouldDie` handling

```



\---



\## Entities



`Entity` is the base class for both `Player` and `Monster`. It owns HP, mana, inventory, attributes, and the `IsDefending` flag.



\- `Player` adds: Level, Class, Gender, Perks, mana cost/regen helpers, `CanAffordSpecialAttack()`

\- `Monster` adds: `AttackStyle`, `DetermineAction()` (\~25% special attack), sprite ID conventions



`Entity.TakeDamage()` accepts an optional `wouldDieCheck` callback — this is how the Phoenix Heart perk intercepts a killing blow before HP reaches 0.



\---



\## Mana



Mana is \*\*only relevant for caster classes\*\*. Physical classes have `MaxMana = 0` and are never affected.



\- Defined per-class in `PlayerClass.GetManaProfile()`: pool size (from `classes.json`), cost per special, regen per turn

\- `ChangeTurnsAction` regenerates mana each turn for casters

\- `SpecialAttackAction` spends mana via `Player.TrySpendManaForSpecialAttack()` in `ApplyEffect()`

\- `BattleManager` greys out the special button if `!player.CanAffordSpecialAttack()`

\- The mana bar UI is hidden entirely for physical class players



\---



\## Health and CON



`MaxHealth = classBaseHP + (CON \* 4)`. Each class tier has a base HP value defined in `CombatCalculator.GetClassBaseHP()`. Call `player.RecalculateMaxHealth()` whenever CON changes (level up, class change). Do \*\*not\*\* call it on load — the serialized value is the ground truth.



\---



\## Adding a New Action



1\. Create a class implementing `IGameAction` in `Assets/\_Game/Scripts/Core/`

2\. Add the new value to the `ActionType` enum

3\. Resolve all randomness (hit/crit) in the constructor; mutate state in `ApplyEffect()`

4\. Fire relevant `PerkTriggerEvent` values at the appropriate moments in `ApplyEffect()`

5\. Handle the new `ActionType` in `ActionQueueProcessor.TriggerVisuals()` for Unity-side visuals



\---



\## Common Mistakes



\- \*\*Don't read mutable global state in constructors unless the action is intentionally snapshotting queue-time state.\*\* Some actions do capture actor/target or resolved combat results up front; if you do this, make sure delayed execution cannot make the snapshot invalid.

\- \*\*Don't put combat formulas outside `CombatCalculator`\*\* — the next developer will thank you.

\- \*\*Don't call `Battle.DetermineNextAction()` manually\*\* — it's called by `ActionQueueProcessor` after each action completes via the `OnActionComplete` event.

\- \*\*Perk events must be fired even on miss\*\* — many perks track consecutive misses or react to missed attacks. Don't skip perk calls when `!DidHit`.

