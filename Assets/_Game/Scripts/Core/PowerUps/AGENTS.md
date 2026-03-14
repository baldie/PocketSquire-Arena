\# AGENTS.md — Power-Ups System



\*\*Location:\*\* `Assets/\_Game/Scripts/Core/PowerUps/`



\---



\## What Power-Ups Are



Power-ups are \*\*procedurally generated, run-scoped upgrades\*\* presented as card choices after arena victories. They are distinct from Perks (purchased from vendors, persist across runs) and stat points (permanent, from level-up).



Key characteristics:

\- \*\*Exist only for the duration of one run.\*\* They are stored in `Run.PowerUps` (`PowerUpCollection`), not on `Player`.

\- \*\*Generated at runtime\*\*, not defined in JSON. The factory selects from templates, rolls rarity, and assigns rank.

\- \*\*Three choices offered at a time.\*\* The player picks one. Picking the same type twice upgrades its rank (I → II → III).

\- \*\*Apply their effects externally\*\* — they are wired into battle start, combat stat resolution, reward calculation, and loot selection flows rather than being stored permanently on `Player`.



\---



\## Component Architecture



Power-ups follow a \*\*component pattern\*\*. A `PowerUp` is a wrapper around a single `PowerUpComponent`. The component defines the actual effect.



```

PowerUp

&#x20; └── PowerUpComponent (abstract base)

&#x20;       ├── AttributeModifierComponent   — boosts a player stat

&#x20;       ├── LootModifierComponent        — increases gold or XP rewards

&#x20;       ├── UtilityComponent             — heals player after each battle

&#x20;       └── MonsterDebuffComponent       — reduces a monster stat at fight start

```



Each component type has a `UniqueKey` (e.g., `"ATTR\_STRENGTH"`, `"LOOT\_GOLD"`). This key is how `PowerUpCollection` identifies duplicates for rank-up handling.



\---



\## Rank and Rarity



Every power-up has two scaling axes:



\*\*Rank\*\* (`PowerUpRank`): I, II, III

\- Gained by picking the same power-up type again

\- Rank multipliers: I = 1.0×, II = 1.5×, III = 2.0×



\*\*Rarity\*\* (`Rarity`): Common, Rare, Epic, Legendary

\- Rolled at generation time based on player Luck stat

\- Rarity multipliers: Common = 1.0×, Rare = 1.5×, Epic = 2.0×, Legendary = 3.0×



\*\*Scaling formula\*\* (in `PowerUpScaling.ComputeValue()`):

```

value = BaseValue × RarityMultiplier × RankMultiplier × (1 + ln(arenaLevel + 1))

```



The `arenaLevel` factor gives power-ups that grow with the run's difficulty without becoming overwhelming (logarithmic, not linear).



All multiplier constants live in `PowerUpScaling.cs`. Change values there only.



\---



\## `PowerUpCollection`



Stored on `Run.PowerUps`. This is the authoritative list of what the player has for this run.



Key methods:

\- `Add(powerUp)` — if the player already owns this `UniqueKey`, increments rank instead of adding a duplicate

\- `Has(uniqueKey)` — check ownership

\- `GetRank(uniqueKey)` — returns current rank or null

\- `HasAtMaxRank(uniqueKey)` — returns true if at Rank III

\- `ApplyMonsterDebuffs(monster, arenaLevel)` — call this before each battle starts, passing the upcoming monster

\- `ApplyUtilityEffects(player, arenaLevel)` — call this after each battle ends

\- `GetGoldBonusPercent(arenaLevel)` / `GetXpBonusPercent(arenaLevel)` — read these in `WinAction` or wherever rewards are calculated



\---



\## `PowerUpFactory` — Generation



`PowerUpFactory.Generate(context, rng)` returns exactly 3 `PowerUp` choices. Never returns duplicates within the same offer.



\*\*`PowerUpGenerationContext`\*\* carries:

\- `ArenaLevel` — affects scaling and is passed to `ComputeValue`

\- `PlayerLuck` — shifts rarity distribution toward rarer tiers

\- `PlayerHealthPercent` — used for context-aware weighting (low HP boosts `UtilityComponent` heal weight)

\- `OwnedPowerUps` — used to determine rank-up vs new acquisition, and to exclude maxed-out types from the offer



\*\*Template system:\*\* `\_templates` is a static list of `ComponentTemplate` entries, each with a `UniqueKey`, a `BaseWeight`, and a `Factory` lambda. The factory creates the component with the rolled rarity and rank. To add a new power-up type, add an entry to this list — no other factory changes needed.



\*\*Context-aware weighting:\*\* Currently only one rule exists — `UTIL\_PARTIALHEAL` weight is tripled when player HP is below 25%. Add new contextual rules in `SelectWeightedTemplate()` following the same pattern.



\*\*Fallback:\*\* If 10 reroll attempts all fail (e.g., every template is maxed out), the factory returns a Single Coin — a flat +1 gold `LootModifierComponent`. This should be extremely rare in practice.



\---



\## Component Reference



\### `AttributeModifierComponent`

Boosts one of: Strength, Constitution, Magic, Dexterity, Luck, Defense.



Applied via `PlayerWithPowerUps.EffectiveAttributes` — it computes a modified `Attributes` struct without mutating the base `Player`. Use this wrapper class when passing player stats into combat calculations during a run.



`UniqueKey` format: `"ATTR\_STRENGTH"`, `"ATTR\_DEXTERITY"`, etc.



\### `LootModifierComponent`

Increases gold or XP rewards by a percentage (or flat +1 if `IsFlatBonus = true` for the fallback coin).



Read via `PowerUpCollection.GetGoldBonusPercent(arenaLevel)` and `GetXpBonusPercent(arenaLevel)`. Apply in `WinAction.ApplyEffect()`:

```

int goldGained = (int)(Target.Gold \* (1f + run.PowerUps.GetGoldBonusPercent(arenaLevel) / 100f));

```



`UniqueKey` format: `"LOOT\_GOLD"`, `"LOOT\_EXPERIENCE"`, `"COIN\_FALLBACK"`.



\### `UtilityComponent`

Currently only one type: `PartialHeal` — restores a percentage of max HP after each battle.



Normal recurring behavior is wired in `WinAction.ApplyEffect()` via `PowerUpCollection.ApplyUtilityEffects(player, arenaLevel)`.

Additionally, if the player selects a new utility power-up from the loot screen after a win, the newly selected power-up is also applied immediately in `LootScript` so the heal is felt before the next battle begins.



`UniqueKey` format: `"UTIL\_PARTIALHEAL"`.



\### `MonsterDebuffComponent`

Reduces one monster attribute at the start of a fight. The reduction has diminishing returns via `ComputeValue` and is floored at 1 (monsters can never have 0 in any stat).



Monster debuffs are currently wired inside the `Battle` constructor. Do not pre-apply them before creating `Battle`, or the monster will be debuffed twice.



`UniqueKey` format: `"DEBUFF\_STRENGTH"`, `"DEBUFF\_DEFENSE"`, etc.



\---



\## `PlayerWithPowerUps`



A \*\*non-mutating wrapper\*\* around `Player` that computes effective attributes with all `AttributeModifierComponent` bonuses applied. It does not change the base `Player.Attributes`.



Usage pattern:

```

var attrs = CombatUtilities.GetEffectiveAttributes(entity);

// Combat code should read effective attributes through CombatUtilities.
// Instantiate PlayerWithPowerUps directly only if you specifically need the wrapper.

```



The result is cached. Call `InvalidateCache()` if power-ups change mid-calculation (this should not happen in normal flow, but the method exists for safety).



\*\*Do not\*\* permanently write power-up bonuses into `Player.Attributes`. They are run-scoped and must not persist to saves.



\---



\## Adding a New Power-Up Type



1\. Decide if it fits an existing `PowerUpComponent` subclass. If yes, just add a template entry to `PowerUpFactory.\_templates`.

2\. If the effect is genuinely new, create a new subclass of `PowerUpComponent`:

&#x20;  - Implement all abstract properties: `UniqueKey`, `IconId`, `DisplayName`, `Description`

&#x20;  - Add the effect logic (apply to monster, apply to player, or expose a computed value)

&#x20;  - Add a new `PowerUpComponentType` enum value

3\. Add the template entry to `PowerUpFactory.\_templates` with an appropriate `BaseWeight`

4\. Wire the effect into the game loop — see the "where to call" notes in each component section above



\---



\## Persistence



Power-ups live on `Run`, which is \*\*not currently serialized to a save file\*\*. A run is started fresh each time. If run persistence is added in the future:

\- `PowerUpCollection` is `\[Serializable]` on the class but `\_powerUps` is a `Dictionary` — a custom JSON converter will be needed since Newtonsoft handles dictionaries but the abstract `PowerUpComponent` subtype requires a type discriminator

\- Until run saves are implemented, do not rely on power-up state surviving an app restart



\---



\## Common Mistakes



\- \*\*Don't mutate `Player.Attributes` with power-up values.\*\* Use `PlayerWithPowerUps` to compute effective stats for battle. The base player stats are a permanent record; run bonuses are transient.

\- \*\*Don't manually pre-apply `ApplyMonsterDebuffs` before creating `Battle`.\*\* The `Battle` constructor already applies monster debuffs once for the current run. Calling it yourself as well will double-debuff the monster.

\- \*\*Don't cache `PlayerWithPowerUps` across arena levels.\*\* `arenaLevel` affects scaling. Create a new instance for each battle.

\- \*\*`PowerUpCollection.Add()` handles rank-up automatically.\*\* Do not check `Has()` before calling `Add()` — the collection manages this itself. Just call `Add` with the chosen power-up.

\- \*\*`Rarity` and `Rank` are live inputs to `ComputeValue()`.\*\* They are not cosmetic-only fields. Mutating them after construction will change both behavior and display, so treat generated power-up components as immutable except for the intentional rank-up flow inside `PowerUpCollection.Add()`.

