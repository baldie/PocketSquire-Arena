import type { Gender, ItemSlot, MonsterSlot, PlayerSlot, PromptTemplates } from "./types";

export const PLAYER_CLASSES = [
    "Squire", "SpellCaster", "Bowman", "Fighter", "Mage", "Druid",
    "Archer", "Hunter", "Warrior", "Wizard", "Archdruid", "Marksman",
    "Ranger", "Knight", "Sorcerer", "Warden", "Sniper", "Sentinel", "Paladin"
] as const;

export type PlayerClassName = typeof PLAYER_CLASSES[number];

export const PLAYER_CLASS_DESCRIPTIONS: Record<PlayerClassName, string> = {
    Squire: "A young knight in training, balanced in attack and defense.",
    SpellCaster: "A novice magic user, focusing on basic elemental spells.",
    Bowman: "An entry-level ranged attacker wielding a basic bow.",
    Fighter: "A melee combatant prioritizing pure physical strength.",
    Mage: "An intermediate spellcaster with access to diverse magical abilities.",
    Druid: "A nature-wielding adept who can harness the earth's power.",
    Archer: "A seasoned combatant skilled in striking from a distance.",
    Hunter: "A survivalist who excels at tracking and trapping foes.",
    Warrior: "A heavily armed frontline combatant built for endurance.",
    Wizard: "A learned scholar of the arcane, mastering complex spells.",
    Archdruid: "A master of nature manipulation and elemental synthesis.",
    Marksman: "A precise ranged combatant capable of devastating long-shots.",
    Ranger: "A versatile survivor blending ranged combat with woodland magic.",
    Knight: "A noble warrior encased in heavy armor, protecting the weak.",
    Sorcerer: "A wielder of innate magical potential, unleashing raw arcane power.",
    Warden: "A steadfast guardian trained to protect nature and allies alike.",
    Sniper: "A specialist in high-damage, single-target ranged strikes.",
    Sentinel: "An impenetrable frontline defender with unmatched resilience.",
    Paladin: "A holy crusader combining divine magic with martial prowess.",
};

export const PLAYER_SLOTS: PlayerSlot[] = [
    "idle", "attack", "defend", "hit", "defeat", "battle", "win", "item"
];

export const MONSTER_SLOTS: MonsterSlot[] = [
    "battle", "attack", "special_attack", "hit", "defend"
];

export const ITEM_SLOTS: ItemSlot[] = ["icon"];

export const GENDERS: Gender[] = ["m", "f"];

export const DEFAULT_TEMPLATES: PromptTemplates = {
    player: {
        global: {
            m: {
                idle: "A full-body fantasy RPG sprite of a {class} male, idle stance, pixel art, transparent background",
                attack: "A full-body fantasy RPG sprite of a {class} male, attacking with weapon, pixel art, transparent background",
                defend: "A full-body fantasy RPG sprite of a {class} male, defensive guard stance, pixel art, transparent background",
                hit: "A full-body fantasy RPG sprite of a {class} male, recoiling from a hit, pixel art, transparent background",
                defeat: "A full-body fantasy RPG sprite of a {class} male, collapsed in defeat, pixel art, transparent background",
                battle: "A full-body fantasy RPG sprite of a {class} male, standard battle stance, pixel art, transparent background",
                win: "A full-body fantasy RPG sprite of a {class} male, celebrating victory, pixel art, transparent background",
                item: "A full-body fantasy RPG sprite of a {class} male, using an item, pixel art, transparent background",
            },
            f: {
                idle: "Make this character a girl with short, straight, blonde hair. Keep the expression the same. White background",
                attack: "Make this character a girl with short, straight, blonde hair. Keep the expression the same. White background",
                defend: "Make this character a girl with short, straight, blonde hair. Keep the expression the same. White background",
                hit: "Make this character a girl with short, straight, blonde hair. Keep the expression the same. White background",
                defeat: "Make this character a girl with short, straight, blonde hair. Keep the expression the same. White background",
                battle: "Make this character a girl with short, straight, blonde hair. Keep the expression the same. White background",
                win: "Make this character a girl with short, straight, blonde hair. Keep the expression the same. White background",
                item: "Make this character a girl with short, straight, blonde hair. Keep the expression the same. White background",
            }
        },
        overrides: {},
    },
    monster: {
        global: {
            battle: "A fantasy monster named {name} (rank {rank}), idle battle stance, pixel art, transparent background",
            attack: "A fantasy monster named {name} (rank {rank}), lunging to attack, pixel art, transparent background",
            special_attack: "A fantasy monster named {name} (rank {rank}), unleashing a special attack, pixel art, transparent background",
            hit: "A fantasy monster named {name} (rank {rank}), recoiling from damage, pixel art, transparent background",
            defend: "A fantasy monster named {name} (rank {rank}), bracing in defense, pixel art, transparent background",
        },
        overrides: {},
    },
    item: {
        global: {
            icon: "A fantasy RPG item icon of {name}, pixel art style, 64x64, transparent background",
        },
        overrides: {},
    },
};
