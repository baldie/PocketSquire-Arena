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


export type PlayerSlot = "idle" | "attack" | "defend" | "hit" | "defeat" | "yield" | "win" | "item";
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
        global: Record<PlayerSlot, string>;
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
