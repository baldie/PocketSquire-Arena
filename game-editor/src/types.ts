export type Gender = "m" | "f";
export type ItemTarget = "self" | "enemy";
export type SaveStatus = "idle" | "saving" | "saved" | "error";
export type NotificationSeverity = "error" | "info" | "success";

export interface Notification {
    id: string;
    message: string;
    severity: NotificationSeverity;
    timestamp: number;
}


export type PlayerSlot = "idle" | "attack" | "defend" | "hit" | "defeat" | "battle" | "win" | "item";
export type MonsterSlot = "battle" | "attack" | "special_attack" | "hit" | "defend";
export type ItemSlot = "icon";
export type ImageSlot = PlayerSlot | MonsterSlot | ItemSlot;

export interface Attributes {
    Strength: number;
    Constitution: number;
    Magic: number;
    Dexterity: number;
    Luck: number;
    Defense: number;
}

export interface PlayerData {
    name: string;
    class: string;
    attackSoundId: string;
    defendSoundId: string;
    hitSoundId: string;
    gender: Gender;
}

export interface MonsterData {
    name: string;
    rank: number;
    experience: number;
    gold: number;
    attackSoundId: string;
    specialAttackSoundId: string;
    defendSoundId: string;
    hitSoundId: string;
    defeatSoundId: string;
    attributes: Attributes;
}

export interface ItemData {
    id: number;
    name: string;
    description: string;
    target: ItemTarget;
    stackable: boolean;
    sprite: string;
    sound_effect: string;
    price: number;
}

export interface PromptTemplates {
    player: {
        global: {
            m: Record<PlayerSlot, string>;
            f: Record<PlayerSlot, string>;
        };
        overrides: Record<string, Partial<Record<PlayerSlot, string>>>;
    };
    monster: {
        global: Record<MonsterSlot, string>;
        overrides: Record<string, Partial<Record<MonsterSlot, string>>>;
    };
    item: {
        global: Record<ItemSlot, string>;
        overrides: Record<string, Partial<Record<ItemSlot, string>>>;
    };
}

export interface GenerationHistoryEntry {
    entityKey: string;
    slot: ImageSlot;
    prompt: string;
    timestamp: number;
    imageDataUrl: string;
}

export interface ResolvedTemplate {
    resolved: string;
    unresolvedVars: string[];
}

export interface TemplateVariables {
    class?: string;
    gender?: string;
    name?: string;
    rank?: string;
    slot?: string;
}

// ──────────────────────────────────────────────────────
// Arena Perks
// ──────────────────────────────────────────────────────

export type ArenaPerkType = "Passive" | "Triggered";
export type VendorType = "Shopkeeper" | "Wizard" | "FightersBlacksmith" | "ArcheryTrainer";
export type PerkTarget = "Player" | "Monster";

// Keep in sync with PerkTriggerEvent.cs
export type PerkTriggerEvent =
    | "PlayerAttackedMonster" | "PlayerMissedMonster" | "PlayerHitMonster"
    | "PlayerUsedItem" | "PlayerDefended" | "PlayerAttemptedYield"
    | "PlayerYieldedSuccessfully" | "PlayerEnteredArena" | "BattleStarted"
    | "BattleWon" | "BattleLost" | "ConsecutiveHits" | "ConsecutiveWins"
    | "ConsecutiveDodges" | "ConsecutiveDefends" | "ConsecutiveItemUses"
    | "PurchasedItem" | "ReturnedHome" | "PlayerLeveledUp" | "HPBelowThreshold"
    | "MonsterMissedPlayer" | "SpecialAttackMissed" | "SpecialAttackLanded"
    | "SpecialAttackCooldownCompleted" | "PlayerTurnStarted" | "PlayerTurnEnded"
    | "MonsterTurnStarted" | "MonsterTurnEnded" | "MonsterAttackHitPlayer" | "WouldDie";

// Keep in sync with ArenaPerkEffectType.cs
export type ArenaPerkEffectType =
    | "RestoreHP" | "RestoreMP" | "DamageBuff" | "DamageReduction"
    | "StackDamageBuff" | "StackDodgeBuff" | "BonusDamage" | "DoubleDamage"
    | "GuaranteedHit" | "NullifyDamage" | "ReduceCooldown" | "RefundMPCost"
    | "IncreaseMaxHP" | "ApplyThorns" | "SurviveFatalBlow" | "YieldBonus"
    | "IncreaseHitChance" | "IncreaseCritChance" | "ReduceShopPrices"
    | "IncreaseGoldGain" | "ReduceDamage";

export interface ArenaPerkPrerequisites {
    minLevel: number;
    class?: string | null;
    requiredPerks?: string[] | null;
}

export interface ArenaPerkData {
    id: string;
    name: string;
    description: string;
    icon?: string;
    type: ArenaPerkType;
    soldBy: VendorType;
    cost: number;
    tier: number; // 0-3
    prerequisites: ArenaPerkPrerequisites;
    event?: PerkTriggerEvent | null;
    effect?: ArenaPerkEffectType | null;
    perkTarget?: PerkTarget | null;
    procPercent?: number;
    value?: number;
    isPercent?: boolean;
    threshold?: number | null;
    maxStacks?: number;
    consecutiveCount?: number;
    duration?: number;
    oncePerBattle?: boolean;
    oncePerRun?: boolean;
    consumeOnUse?: boolean;
    resetOn?: PerkTriggerEvent | null;
    yieldChanceBonus?: number;
    hpRestore?: number;
}
