# Perk System — Implementation PRD

> **Status**: Ready for implementation  
> **Last updated**: 2026-03-04  
> **Location**: This file lives in `tasks/` alongside `todo.md` and `lessons.md`

---

## How to Use This Document

This PRD is **self-contained**. It was produced by analyzing the full codebase and contains everything you need to implement the arena perk system. Follow these rules:

1. **Follow the phases in order** (0 → 1 → 2 → 3 → 4 → 5). Each phase depends on the previous.
2. **Create `tasks/todo.md`** with checklist items for the current phase before starting.
3. **Run `npm run test:unit` after every phase** to verify before moving on.
4. **Follow existing patterns exactly** — code samples below are modeled on actual code from this project.
5. **Read `tasks/lessons.md`** before starting — it has critical notes about DOTween, Input handling, etc.
6. **Never modify `Core/LevelUp/Perk.cs`** — arena perks are a separate system.
7. **All new Core files go in `Assets/_Game/Scripts/Core/Perks/`** (new folder).
8. **All new test files go in `tests/unit/`** using the existing NUnit project.

### Key Commands
```powershell
# Run unit tests
npm run test:unit

# Run build validation
npm run test:build
```

### Project Conventions
- **Namespace**: `PocketSquire.Arena.Core.Perks` for new core files
- **Nullable**: Always add `#nullable enable` at top of files using `?` types
- **No `Input.Get...`**: Use `GameInput` wrapper (see `tasks/lessons.md`)
- **Console, not Debug**: Core/ files use `Console.WriteLine` (framework-agnostic). Only Unity/ files use `Debug.Log`.
- **Newtonsoft.Json**: All JSON serialization uses `Newtonsoft.Json`, NOT `System.Text.Json`
- **PowerShell**: Terminal is PowerShell — use `;` not `&&` to chain commands

---

## Background: Three Separate Perk Systems

This project has **three distinct perk/buff systems**. Do NOT confuse them:

| System | Purpose | Location | Scope |
|--------|---------|----------|-------|
| **Level-Up Perks** | Chosen on level-up from `PerkPool`. Permanent stat upgrades. | `Core/LevelUp/Perk.cs`, `PerkPool.cs`, `PerkSelector.cs` | Permanent |
| **Run Power-Ups** | Card-selection between arena battles. Attribute mods, loot mods. | `Core/PowerUps/PowerUp.cs` (12+ files) | Single run |
| **Arena Perks** ← THIS PRD | Purchased from town vendors with gold. Combat triggers & passives. | `Core/Perks/` (NEW) | Permanent (purchased), runtime effects |

**DO NOT modify** the Level-Up Perk system or the Power-Up system. Arena Perks coexist alongside them.

---

## Existing Code You Must Understand

### IMerchandise Interface
`ArenaPerk` must implement this for shop integration:
```csharp
// File: Assets/_Game/Scripts/Core/IMerchandise.cs
namespace PocketSquire.Arena.Core
{
    public interface IMerchandise
    {
        string DisplayName { get; }
        string Description { get; }
        int Price { get; }
    }
}
```

### GameWorld.cs — Data Loading Pattern
**All JSON loading follows this exact pattern** (`File.ReadAllText` + Newtonsoft). Copy this pattern for arena perks:
```csharp
// File: Assets/_Game/Scripts/Core/GameWorld.cs (current state)
// Static class with Load() that calls private LoadX() methods
// Each loader accepts optional rootPath for unit testing
private static void LoadMonsters(string? rootPath = null)
{
    string root = rootPath ?? Environment.CurrentDirectory;
    string filePath = Path.Combine(root, "Assets/_Game/Data/monsters.json");
    if (!File.Exists(filePath))
        throw new FileNotFoundException($"Monster data file not found at: {filePath}");
    string jsonContent = File.ReadAllText(filePath);
    var monsters = JsonConvert.DeserializeObject<List<Monster>>(jsonContent);
    AllMonsters.Clear();
    if (monsters != null) AllMonsters.AddRange(monsters);
}
```
**IMPORTANT**: `arena_perks.json` is wrapped in `{ "perks": [...], "metadata": {...} }` — NOT a raw array. You need a wrapper class for deserialization (see §1.7).

### Player.cs — Current Perk Fields
```csharp
// These fields exist and must NOT be changed:
public HashSet<string> UnlockedPerks { get; set; } = new();  // Level-up perk IDs
// You ADD new fields for arena perks (see §4)
```

### Entity.cs — Current State (No Mana)
```csharp
// File: Assets/_Game/Scripts/Core/Entity.cs (current — no Mana fields)
public int Health;
public int MaxHealth;
public Attributes Attributes = new Attributes();
// You will ADD: Mana, MaxMana, RestoreMana(), SpendMana()
```

### AttackAction.cs — Current State (No Hit/Miss)
```csharp
// Currently: damage always lands, no hit/miss/crit
public int Damage { get; }  // Calculated in constructor
public void ApplyEffect()
{
    Target.TakeDamage(Damage);
}
// You will ADD: DidHit, IsCrit, hit/miss/crit calculations
```

### ShopController.cs — Current Perk Purchase Flow
The shop currently uses **ScriptableObject PerkNodes** assigned in the Unity editor on `LocationData`:
```csharp
// LocationData.cs has:
[SerializeField] private List<PerkNode> shopPerkNodes = new List<PerkNode>();

// ShopController.Open() iterates these:
foreach (var perkNode in location.ShopPerkNodes)
{
    if (ownedPerks.Contains(perkNode.Id)) continue;
    var corePerk = perkNode.ToCorePerk();
    CreateMerchandiseRow(corePerk, perkNode.Icon, () => OnPerkPurchased(perkNode));
}
```
For arena perks, you'll add a **new code path** that queries `GameWorld.GetArenaPerksByVendor()` at runtime.

### PlayerClass.cs — Tier Comments
```csharp
public enum ClassName
{
    // Tier 0
    Squire,
    // Tier 1
    SpellCaster, Bowman, Fighter,
    // Tier 2
    Mage, Druid, Archer, Hunter, Warrior,
    // Tier 3
    Wizard, Archdruid, Marksman, Ranger, Knight,
    // Prestige Classes
    Sorcerer, Warden, Sniper, Sentinel, Paladin
}
```

### Test Project Structure
```
tests/unit/PocketSquire.Arena.Tests.csproj
  - TargetFramework: net10.0
  - Includes: ../../Assets/_Game/Scripts/Core/**/*.cs (glob)
  - Uses: NUnit 4.3.2, Newtonsoft.Json 13.0.3
  - NO Unity dependencies — pure C# only
```

### arena_perks.json — Sample Entries
```json
{
  "perks": [
    {
      "id": "keen_eye",
      "name": "Keen Eye",
      "description": "Increases hit chance by 5%",
      "icon": "keen_eye.png",
      "type": "Passive",
      "soldBy": "Shopkeeper",
      "cost": 100,
      "tier": 0,
      "prerequisites": { "minLevel": 1 }
    },
    {
      "id": "second_wind",
      "name": "Second Wind",
      "description": "When HP drops below 30%, restore 15% max HP once per battle",
      "icon": "second_wind.png",
      "type": "Triggered",
      "soldBy": "Shopkeeper",
      "cost": 200,
      "tier": 0,
      "prerequisites": { "minLevel": 3 },
      "event": "HPBelowThreshold",
      "threshold": 30,
      "perkTarget": "Player",
      "procPercent": 100,
      "effect": "RestoreHP",
      "value": 15,
      "isPercent": true,
      "oncePerBattle": true
    },
    {
      "id": "warriors_resolve",
      "name": "Warrior's Resolve",
      "description": "Each consecutive hit increases damage by 3% (stacks up to 5 times)",
      "icon": "warriors_resolve.png",
      "type": "Triggered",
      "soldBy": "FightersBlacksmith",
      "cost": 300,
      "tier": 1,
      "prerequisites": { "class": "Fighter", "minLevel": 5 },
      "event": "PlayerHitMonster",
      "perkTarget": "Player",
      "procPercent": 100,
      "effect": "StackDamageBuff",
      "value": 3,
      "maxStacks": 5,
      "resetOn": "PlayerMissedMonster"
    },
    {
      "id": "tactical_retreat",
      "name": "Tactical Retreat",
      "description": "Increases yield success by 30% and restores 10% HP on successful yield",
      "type": "Triggered",
      "soldBy": "ArcheryTrainer",
      "cost": 250,
      "tier": 2,
      "prerequisites": { "minLevel": 10 },
      "event": "PlayerYieldedSuccessfully",
      "perkTarget": "Player",
      "procPercent": 100,
      "effect": "YieldBonus",
      "yieldChanceBonus": 30,
      "hpRestore": 10,
      "isPercent": true
    }
  ],
  "metadata": {
    "version": "2.0",
    "totalPerks": 30
  }
}
```
**Note**: The metadata says 30 perks but the array contains 28. Trust the array, not the metadata.

---

## Decisions (Locked In)

| Decision | Choice | Details |
|----------|--------|---------|
| Hit/miss system | **Build it** | Dex + Luck based. Phase 0 prerequisite. |
| MP system | **Add to Entity** | `Mana`/`MaxMana` on Entity. Casters get non-zero values from `classes.json`. |
| Active perk cap | **Class-tier-based** | Tier 0→2 slots, T1→4, T2→6, T3→8, Prestige→10 |
| Perk loadout UI | **Separate effort** | NOT part of this PRD. User will implement PlayerMenu tab independently. |
| Vendor mapping | **Runtime query** | `GameWorld.GetArenaPerksByVendor(vendor)` minus player-owned. Add `VendorType?` to `LocationData`. |
| ArenaPerk namespace | `PocketSquire.Arena.Core.Perks` | New namespace, separate from `Core.LevelUp` |

---

## Phase 0: Prerequisites — Hit/Miss + MP

### 0.1 Entity: Add Mana
**File**: `Assets/_Game/Scripts/Core/Entity.cs` (MODIFY)

Add after `MaxHealth`:
```csharp
public int Mana;
public int MaxMana;

public void RestoreMana(int amount)
{
    Mana = Math.Min(Mana + amount, MaxMana);
}

public bool SpendMana(int amount)
{
    if (Mana < amount) return false;
    Mana -= amount;
    return true;
}
```

### 0.2 classes.json: Add Mana Fields
**File**: `Assets/_Game/Data/classes.json` (MODIFY)

Add `"mana": 0, "maxMana": 0` to each class template. SpellCaster+ classes get non-zero values. Use your judgment on values (e.g., SpellCaster: 20, Mage: 40, Wizard: 80, etc.).

### 0.3 AttackAction: Hit/Miss/Crit
**File**: `Assets/_Game/Scripts/Core/AttackAction.cs` (MODIFY)

**Requirements**:
- Add `public bool DidHit { get; }` and `public bool IsCrit { get; }` properties
- Add `Random` parameter to constructor (default `new Random()`) for testability
- Calculate hit chance in constructor (NOT in `ApplyEffect` — follow existing `Damage` pattern)
- If miss: set `Damage = 0`, `DidHit = false`, skip `TakeDamage()` in `ApplyEffect()`
- If crit: multiply damage by 1.5x, set `IsCrit = true`

**Formulas**:
```csharp
// Hit chance: base 80%, ±2% per relative Dexterity
int hitChance = 80 + (attacker.Attributes.Dexterity - target.Attributes.Dexterity) * 2;
hitChance = Math.Clamp(hitChance, 5, 99);
DidHit = rng.Next(100) < hitChance;

// Crit chance: base 5%, +1% per Luck above 5 (only if hit landed)
int critChance = 5 + Math.Max(0, attacker.Attributes.Luck - 5);
critChance = Math.Clamp(critChance, 1, 50);
IsCrit = DidHit && rng.Next(100) < critChance;

// Damage
if (!DidHit) Damage = 0;
else if (IsCrit) Damage = (int)(baseDamage * 1.5f);
else Damage = baseDamage;
```

### 0.4 SpecialAttackAction: Same Hit/Miss/Crit
**File**: `Assets/_Game/Scripts/Core/SpecialAttackAction.cs` (MODIFY)

Apply same pattern as AttackAction.

### 0.5 ActionQueueProcessor: Show Miss/Crit Text
**File**: `Assets/_Game/Scripts/Unity/ActionQueueProcessor.cs` (MODIFY)

In `ProcessAction()`, after the hit effects block (~line 146), add logic:
- If `AttackAction.DidHit == false`: show "MISS" text on target using `ShowNumberEffect` with a unique color (e.g., gray)
- If `AttackAction.IsCrit == true`: show damage in a different color (e.g., yellow/gold) or add a crit indicator

### 0.6 Tests
**File**: `tests/unit/AttackActionTests.cs` (NEW)

Test with seeded `Random` for deterministic results:
- Hit lands with high dexterity advantage → DidHit = true
- Miss with low dexterity → DidHit = false, Damage = 0
- Crit with high luck → IsCrit = true, Damage = 1.5x
- Edge cases: clamp at 5% min, 99% max hit chance

**Verify**: `npm run test:unit`

---

## Phase 1: Core Data & Loading

### 1.1-1.4 Enum Files (NEW)
**Location**: `Assets/_Game/Scripts/Core/Perks/`

Create four enum files. All use namespace `PocketSquire.Arena.Core.Perks`:

**`ArenaPerkType.cs`**:
```csharp
namespace PocketSquire.Arena.Core.Perks
{
    public enum ArenaPerkType { Passive, Triggered }
}
```

**`VendorType.cs`**:
```csharp
namespace PocketSquire.Arena.Core.Perks
{
    public enum VendorType { Shopkeeper, Wizard, FightersBlacksmith, ArcheryTrainer }
}
```

**`PerkTriggerEvent.cs`** — include ALL events from the JSON:
```csharp
namespace PocketSquire.Arena.Core.Perks
{
    public enum PerkTriggerEvent
    {
        PlayerAttackedMonster,
        PlayerMissedMonster,
        PlayerHitMonster,
        PlayerUsedItem,
        PlayerDefended,
        PlayerAttemptedYield,
        PlayerYieldedSuccessfully,
        PlayerEnteredArena,
        BattleStarted,
        BattleWon,
        BattleLost,
        ConsecutiveHits,
        ConsecutiveWins,
        ConsecutiveDodges,
        ConsecutiveDefends,
        ConsecutiveItemUses,
        PurchasedItem,
        ReturnedHome,
        PlayerLeveledUp,
        HPBelowThreshold,
        MonsterMissedPlayer,
        SpecialAttackMissed,
        SpecialAttackLanded,
        SpecialAttackCooldownCompleted,
        PlayerTurnStarted,
        PlayerTurnEnded,
        MonsterTurnStarted,
        MonsterTurnEnded,
        MonsterAttackHitPlayer,
        WouldDie
    }
}
```

**`ArenaPerkEffectType.cs`** — separate from existing `LevelUp/PerkEffectType.cs` (which only has `None, InventoryExpansion`):
```csharp
namespace PocketSquire.Arena.Core.Perks
{
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
        ReduceDamage
    }
}
```

### 1.5 ArenaPerkPrerequisites (NEW)
**File**: `Assets/_Game/Scripts/Core/Perks/ArenaPerkPrerequisites.cs`

```csharp
#nullable enable
using System.Collections.Generic;
using Newtonsoft.Json;

namespace PocketSquire.Arena.Core.Perks
{
    public class ArenaPerkPrerequisites
    {
        [JsonProperty("minLevel")]
        public int MinLevel { get; set; } = 1;

        [JsonProperty("class")]
        public string? ClassName { get; set; }

        [JsonProperty("requiredPerks")]
        public List<string>? RequiredPerks { get; set; }
    }
}
```

### 1.6 ArenaPerk (NEW)
**File**: `Assets/_Game/Scripts/Core/Perks/ArenaPerk.cs`

Must implement `IMerchandise` (from `PocketSquire.Arena.Core`). Use `[JsonProperty]` on every field to match the JSON keys exactly. All properties should have `{ get; set; }` for Newtonsoft deserialization.

Key mapping gotchas:
- `"name"` → `DisplayName` (since `IMerchandise` uses `DisplayName`)
- `"cost"` → `Cost`, with `Price => Cost` for `IMerchandise.Price`
- `"soldBy"` → `Vendor` (type `VendorType`)
- `"event"` → `TriggerEvent` (type `PerkTriggerEvent?`, nullable because Passive perks don't have events)
- `"effect"` → `Effect` (type `ArenaPerkEffectType?`, nullable because some Passive perks have no explicit effect enum)

See the full class definition with all fields in the specification samples above.

### 1.7 ArenaPerkWrapper (NEW)
**File**: `Assets/_Game/Scripts/Core/Perks/ArenaPerkWrapper.cs`

```csharp
using System.Collections.Generic;
using Newtonsoft.Json;

namespace PocketSquire.Arena.Core.Perks
{
    public class ArenaPerkWrapper
    {
        [JsonProperty("perks")]
        public List<ArenaPerk> Perks { get; set; } = new List<ArenaPerk>();
    }
}
```

### 1.8 PlayerClass: GetTier() and GetMaxPerkSlots()
**File**: `Assets/_Game/Scripts/Core/PlayerClass.cs` (MODIFY)

Add two static methods:
```csharp
public static int GetTier(ClassName className)
{
    switch (className)
    {
        case ClassName.Squire: return 0;
        case ClassName.SpellCaster:
        case ClassName.Bowman:
        case ClassName.Fighter: return 1;
        case ClassName.Mage:
        case ClassName.Druid:
        case ClassName.Archer:
        case ClassName.Hunter:
        case ClassName.Warrior: return 2;
        case ClassName.Wizard:
        case ClassName.Archdruid:
        case ClassName.Marksman:
        case ClassName.Ranger:
        case ClassName.Knight: return 3;
        default: return 4; // Prestige
    }
}

/// <summary>
/// Tier 0→2, Tier 1→4, Tier 2→6, Tier 3→8, Prestige→10
/// </summary>
public static int GetMaxPerkSlots(ClassName className)
{
    return (GetTier(className) + 1) * 2;
}
```

### 1.9 GameWorld: LoadArenaPerks
**File**: `Assets/_Game/Scripts/Core/GameWorld.cs` (MODIFY)

1. Add `using PocketSquire.Arena.Core.Perks;`
2. Add property: `public static List<ArenaPerk> AllArenaPerks { get; set; } = new List<ArenaPerk>();`
3. Add `LoadArenaPerks(rootPath);` to the `Load()` method (after `LoadItems`)
4. Create `private static void LoadArenaPerks(string? rootPath = null)` following the exact `LoadMonsters` pattern, but deserializing to `ArenaPerkWrapper` instead of `List<T>` directly
5. Add helpers:
```csharp
public static ArenaPerk? GetArenaPerkById(string id)
    => AllArenaPerks.Find(p => p.Id == id);

public static List<ArenaPerk> GetArenaPerksByVendor(VendorType vendor)
    => AllArenaPerks.FindAll(p => p.Vendor == vendor);
```

### 1.10 Tests
**File**: `tests/unit/ArenaPerkLoadingTests.cs` (NEW)

- Test JSON loads successfully (28 perks)
- Test field mapping for a Passive perk (keen_eye)
- Test field mapping for a Triggered perk (second_wind)
- Test field mapping for a class-restricted perk (warriors_resolve)
- Test `GetArenaPerkById()` returns correct perk
- Test `GetArenaPerksByVendor()` returns correct filtered list
- Test `PlayerClass.GetTier()` for all tiers
- Test `PlayerClass.GetMaxPerkSlots()` returns correct slot counts

**Verify**: `npm run test:unit`

---

## Phase 2: Player Integration & State

### 2.1 ArenaPerkState (NEW)
**File**: `Assets/_Game/Scripts/Core/Perks/ArenaPerkState.cs`

Runtime state for one active perk. NOT serialized — rebuilt on load from `ActiveArenaPerkIds`.

```csharp
namespace PocketSquire.Arena.Core.Perks
{
    public class ArenaPerkState
    {
        public string PerkId { get; set; } = string.Empty;
        public int CurrentStacks { get; set; }
        public int RemainingDuration { get; set; }
        public bool HasTriggeredThisBattle { get; set; }
        public bool HasTriggeredThisRun { get; set; }
        public int ConsecutiveCounter { get; set; }
        public bool ConsumedThisBattle { get; set; }

        public void ResetForBattle()
        {
            CurrentStacks = 0;
            RemainingDuration = 0;
            HasTriggeredThisBattle = false;
            ConsecutiveCounter = 0;
            ConsumedThisBattle = false;
        }

        public void ResetForRun()
        {
            ResetForBattle();
            HasTriggeredThisRun = false;
        }
    }
}
```

### 2.2-2.4 Player Extensions
**File**: `Assets/_Game/Scripts/Core/Player.cs` (MODIFY)

Add `using PocketSquire.Arena.Core.Perks;` and `using Newtonsoft.Json;`

New fields:
```csharp
public HashSet<string> UnlockedArenaPerks { get; set; } = new();
public List<string> ActiveArenaPerkIds { get; set; } = new();

[JsonIgnore]
public Dictionary<string, ArenaPerkState> ArenaPerkStates { get; set; } = new();

[JsonIgnore]
public int MaxArenaPerkSlots => PlayerClass.GetMaxPerkSlots(Class);
```

New methods:
```csharp
public bool TryPurchaseArenaPerk(ArenaPerk perk)
{
    if (perk == null) throw new ArgumentNullException(nameof(perk));
    if (UnlockedArenaPerks.Contains(perk.Id)) return false;
    if (Gold < perk.Cost) return false;
    SpendGold(perk.Cost);
    UnlockedArenaPerks.Add(perk.Id);
    return true;
}

public bool TryActivateArenaPerk(string perkId)
{
    if (!UnlockedArenaPerks.Contains(perkId)) return false;
    if (ActiveArenaPerkIds.Contains(perkId)) return false;
    if (ActiveArenaPerkIds.Count >= MaxArenaPerkSlots) return false;
    ActiveArenaPerkIds.Add(perkId);
    ArenaPerkStates[perkId] = new ArenaPerkState { PerkId = perkId };
    return true;
}

public bool TryDeactivateArenaPerk(string perkId)
{
    if (!ActiveArenaPerkIds.Remove(perkId)) return false;
    ArenaPerkStates.Remove(perkId);
    return true;
}

public void InitializeArenaPerkStates()
{
    ArenaPerkStates.Clear();
    foreach (var id in ActiveArenaPerkIds)
        ArenaPerkStates[id] = new ArenaPerkState { PerkId = id };
}
```

### 2.5 GameState: Load Integration
**File**: `Assets/_Game/Scripts/Core/GameState.cs` (MODIFY)

In `LoadFromSaveData()`, add after the existing `Player.Inventory.UpdateCapacity(Player.UnlockedPerks);` line:
```csharp
Player.InitializeArenaPerkStates();
```

### 2.6 Tests
**File**: `tests/unit/ArenaPerkPlayerTests.cs` (NEW)

- Purchase succeeds (gold deducted, ID added)
- Purchase fails: already owned
- Purchase fails: insufficient gold
- Activate succeeds
- Activate fails: not owned
- Activate fails: cap reached (test with Squire = 2 slots)
- Activate fails: already active
- Cap changes when class changes (Squire→Fighter = 2→4)
- Deactivate succeeds
- Deactivate fails: not active
- `InitializeArenaPerkStates()` rebuilds state dict from IDs
- Save/load round-trip: `UnlockedArenaPerks` and `ActiveArenaPerkIds` survive

**Verify**: `npm run test:unit`

---

## Phase 3: Perk Processing & Battle Integration

### 3.1-3.2 PerkContext and PerkProcessResult (NEW)
**Files**: `Assets/_Game/Scripts/Core/Perks/PerkContext.cs`, `PerkProcessResult.cs`

**PerkContext**: data about the current action (damage, attacker, target, hit/crit, HP%, RNG)
**PerkProcessResult**: output of processing (modified damage, heal amount, survive fatal blow, guaranteed hit, etc.)

### 3.3 PerkProcessor (NEW)
**File**: `Assets/_Game/Scripts/Core/Perks/PerkProcessor.cs`

**Static class, stateless**. Two main methods:

1. `ProcessEvent(PerkTriggerEvent, Player, PerkContext)` → `PerkProcessResult`
   - Iterates player's active triggered perks matching the event
   - Checks limits (once per battle, once per run, consumed)
   - Checks thresholds and consecutive counters
   - Rolls proc chance using `context.Rng`
   - Applies effect, updates `ArenaPerkState`
   - Returns aggregated result

2. `GetPassiveModifiers(Player)` → `PerkProcessResult`
   - Iterates active passive perks
   - Aggregates hit chance bonus, crit bonus, damage multiplier, etc.
   - Called by `AttackAction` to modify hit/crit/damage calculations

### 3.4-3.12 Action Integration Points
Each action class calls `PerkProcessor` directly (NO event bus):

| File to Modify | Integration |
|----------------|-------------|
| `Core/AttackAction.cs` | Call `GetPassiveModifiers()` for hit/crit/damage. Call `ProcessEvent(PlayerAttackedMonster)`, then `PlayerHitMonster` or `PlayerMissedMonster`. Apply result to damage. |
| `Core/DefendAction.cs` | Call `ProcessEvent(PlayerDefended)` after setting `IsDefending = true` |
| `Core/ItemAction.cs` | Call `ProcessEvent(PlayerUsedItem)` after item effect |
| `Core/YieldAction.cs` | Call `ProcessEvent(PlayerAttemptedYield)`, use `YieldBonus` result |
| `Core/ChangeTurnsAction.cs` | Decrement `RemainingDuration` on all active perk states. Call `ProcessEvent(PlayerTurnStarted/Ended)` |
| `Core/WinAction.cs` | Call `ProcessEvent(BattleWon)`. Apply gold bonus from passive perks. |
| `Core/Entity.cs` `TakeDamage()` | Before `Health = 0`: check for `WouldDie` event → `SurviveFatalBlow` sets HP to 1 |
| `Core/Battle.cs` constructor | Call `ResetForBattle()` on all perk states |
| `Core/Run.cs` `Reset()` | Call `PerkProcessor.ResetPerksForEvent(player, "ReturnedHome")` |

**IMPORTANT**: Only process perks when `Actor is Player`. Don't trigger player perks for monster actions.

### 3.13 Tests
**File**: `tests/unit/PerkProcessorTests.cs` (NEW)

Test each effect type with mocked data:
- RestoreHP (flat and percentage)
- DoubleDamage + proc chance (seeded RNG)
- StackDamageBuff (stacks, max stacks, reset)
- SurviveFatalBlow (once per battle limit)
- Passive modifiers (hit chance, crit chance, damage buff)
- Duration tick-down on turn change
- OncePerBattle flag respected
- ConsecutiveCount logic
- Passive ReduceShopPrices / IncreaseGoldGain

**Verify**: `npm run test:unit`

---

## Phase 4: Shop Integration (Unity Layer)

### 4.1 LocationData: Add VendorType
**File**: `Assets/_Game/Scripts/Unity/Town/LocationData.cs` (MODIFY)

Add:
```csharp
using PocketSquire.Arena.Core.Perks;

[Header("Arena Perks")]
[SerializeField] private VendorType? vendorType;
public VendorType? VendorType => vendorType;
```

Then assign the correct `VendorType` to each location in the Unity Editor.

### 4.2-4.3 ShopController: Arena Perks
**File**: `Assets/_Game/Scripts/Unity/UI/ShopController.cs` (MODIFY)

In `Open()`, after the existing perk node loop, add:
```csharp
// Populate arena perks from runtime JSON data
if (location.VendorType.HasValue)
{
    var ownedArenaPerks = GameState.Player?.UnlockedArenaPerks ?? new HashSet<string>();
    var available = GameWorld.GetArenaPerksByVendor(location.VendorType.Value)
        .Where(p => !ownedArenaPerks.Contains(p.Id));

    foreach (var arenaPerk in available)
    {
        // TODO: icon lookup from GameAssetRegistry using perk.Icon
        CreateMerchandiseRow(arenaPerk, null, () => OnArenaPerkPurchased(arenaPerk),
            $"ArenaPerkRow_{arenaPerk.Id}");
    }
}
```

Add `OnArenaPerkPurchased(ArenaPerk)` method following the pattern of existing `OnPerkPurchased`.

### 4.4 ActionQueueProcessor: Perk Visual Feedback
**File**: `Assets/_Game/Scripts/Unity/ActionQueueProcessor.cs` (MODIFY)

Show text when a perk triggers (e.g., "Second Wind!" in green above the player). This is cosmetic and can be a simple `ShowNumberEffect` variant or a text popup.

### 4.5 (Optional) Buff Indicator HUD
**File**: `Assets/_Game/Scripts/Unity/UI/BuffIndicatorController.cs` (NEW, OPTIONAL)

Shows small icons for active perks with stack/duration counts in the arena scene. Low priority.

**Verify**: Manual testing in Unity Editor

---

## Phase 5: Polish

- Debug menu commands for perk testing (unlock all, clear all, toggle)
- Balance pass on perk values in `arena_perks.json`
- Fix metadata `totalPerks: 30` → actual count
- Performance check with 10 active perks all triggering

---

## File Checklist

### New Files
```
Assets/_Game/Scripts/Core/Perks/
├── ArenaPerk.cs
├── ArenaPerkEffectType.cs
├── ArenaPerkPrerequisites.cs
├── ArenaPerkState.cs
├── ArenaPerkType.cs
├── ArenaPerkWrapper.cs
├── PerkContext.cs
├── PerkProcessResult.cs
├── PerkProcessor.cs
├── PerkTriggerEvent.cs
└── VendorType.cs

tests/unit/
├── ArenaPerkLoadingTests.cs
├── ArenaPerkPlayerTests.cs
├── AttackActionTests.cs
└── PerkProcessorTests.cs
```

### Modified Files
```
Core/Entity.cs               — Add Mana, MaxMana, RestoreMana(), SpendMana()
Core/Player.cs               — Add arena perk fields + methods
Core/PlayerClass.cs           — Add GetTier(), GetMaxPerkSlots()
Core/GameWorld.cs             — Add LoadArenaPerks(), AllArenaPerks, helpers
Core/GameState.cs             — Call InitializeArenaPerkStates() on load
Core/AttackAction.cs          — Hit/miss/crit + perk integration
Core/SpecialAttackAction.cs   — Hit/miss/crit
Core/DefendAction.cs          — Perk trigger call
Core/ItemAction.cs            — Perk trigger call
Core/YieldAction.cs           — Perk trigger call
Core/ChangeTurnsAction.cs     — Duration tick-down + perk trigger
Core/WinAction.cs             — Perk trigger call
Core/Entity.cs                — WouldDie check in TakeDamage()
Core/Battle.cs                — Perk state reset in constructor
Core/Run.cs                   — Perk reset on run end
Unity/ActionQueueProcessor.cs — MISS/CRIT text + perk feedback
Unity/Town/LocationData.cs    — Add VendorType field
Unity/UI/ShopController.cs    — Arena perk shop integration
Data/classes.json              — Add mana fields
```
