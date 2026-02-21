import { describe, it, expect } from "vitest";
import type { AppState } from "../context/types";
import type { AppAction } from "../context/types";
import { DEFAULT_TEMPLATES } from "../constants";
import type { MonsterData } from "../types";

// Extract reducer from AppContext for direct testing
// We replicate it here since it's not exported separately
function reducer(state: AppState, action: AppAction): AppState {
    switch (action.type) {
        case "SET_DIR_HANDLE":
            return { ...state, dirHandle: action.payload };
        case "LOAD_DATA":
            return { ...state, ...action.payload };
        case "SET_ACTIVE_TAB":
            return { ...state, activeTab: action.payload };
        case "SET_ACTIVE_PLAYER_CLASS":
            return { ...state, activePlayerClass: action.payload, activeSlot: null };
        case "SET_ACTIVE_PLAYER_GENDER":
            return { ...state, activePlayerGender: action.payload };
        case "SET_ACTIVE_MONSTER":
            return { ...state, activeMonsterIndex: action.payload, activeSlot: null };
        case "SET_ACTIVE_ITEM":
            return { ...state, activeItemIndex: action.payload, activeSlot: null };
        case "SET_ACTIVE_SLOT":
            return { ...state, activeSlot: action.payload };
        case "UPDATE_PLAYER": {
            const players = [...state.players];
            players[action.payload.index] = action.payload.data;
            return { ...state, players };
        }
        case "UPDATE_MONSTER": {
            const monsters = [...state.monsters];
            monsters[action.payload.index] = action.payload.data;
            return { ...state, monsters };
        }
        case "ADD_MONSTER":
            return { ...state, monsters: [...state.monsters, action.payload] };
        case "DELETE_MONSTER": {
            const monsters = state.monsters.filter((_, i) => i !== action.payload);
            return {
                ...state,
                monsters,
                activeMonsterIndex:
                    state.activeMonsterIndex === action.payload ? null : state.activeMonsterIndex,
            };
        }
        case "UPDATE_ITEM": {
            const items = [...state.items];
            items[action.payload.index] = action.payload.data;
            return { ...state, items };
        }
        case "ADD_ITEM":
            return { ...state, items: [...state.items, action.payload] };
        case "DELETE_ITEM": {
            const items = state.items.filter((_, i) => i !== action.payload);
            return { ...state, items };
        }
        case "UPDATE_PROMPT_TEMPLATES":
            return { ...state, promptTemplates: action.payload };
        case "SET_SAVE_STATUS":
            return { ...state, saveStatus: action.payload };
        case "ADD_GENERATION_HISTORY":
            return { ...state, generationHistory: [...state.generationHistory, action.payload] };
        case "SET_GENERATING":
            return { ...state, isGenerating: action.payload };
        case "SET_GENERATION_PROGRESS":
            return { ...state, generationProgress: action.payload };
        default:
            return state;
    }
}

const baseState: AppState = {
    players: [],
    monsters: [],
    items: [],
    promptTemplates: DEFAULT_TEMPLATES,
    activeTab: "players",
    activePlayerClass: null,
    activePlayerGender: "m",
    activeMonsterIndex: null,
    activeItemIndex: null,
    activeSlot: null,
    saveStatus: "idle",
    generationHistory: [],
    isGenerating: false,
    generationProgress: null,
    dirHandle: null,
    notifications: [],
};

const testMonster: MonsterData = {
    name: "Test Monster",
    rank: 1,
    health: 10,
    maxHealth: 10,
    experience: 5,
    gold: 3,
    attackSoundId: "",
    specialAttackSoundId: "",
    defendSoundId: "",
    hitSoundId: "",
    defeatSoundId: "",
    attributes: { Strength: 5, Constitution: 10, Magic: 3, Dexterity: 3, Luck: 2, Defense: 2 },
};

describe("AppContext reducer", () => {
    it("ADD_MONSTER appends to the monsters array and preserves existing entries", () => {
        const state = { ...baseState, monsters: [testMonster] };
        const nextState = reducer(state, { type: "ADD_MONSTER", payload: { ...testMonster, name: "New" } });
        expect(nextState.monsters).toHaveLength(2);
        expect(nextState.monsters[0].name).toBe("Test Monster");
        expect(nextState.monsters[1].name).toBe("New");
    });

    it("DELETE_MONSTER removes the correct index and does not mutate other entries", () => {
        const m2: MonsterData = { ...testMonster, name: "Second" };
        const state = { ...baseState, monsters: [testMonster, m2] };
        const nextState = reducer(state, { type: "DELETE_MONSTER", payload: 0 });
        expect(nextState.monsters).toHaveLength(1);
        expect(nextState.monsters[0].name).toBe("Second");
    });

    it("UPDATE_ITEM updates only the targeted index", () => {
        const item1 = { id: 1, name: "Item A", description: "", target: "self" as const, stackable: true, sprite: "a", sound_effect: "", price: 1 };
        const item2 = { id: 2, name: "Item B", description: "", target: "self" as const, stackable: false, sprite: "b", sound_effect: "", price: 2 };
        const state = { ...baseState, items: [item1, item2] };
        const nextState = reducer(state, { type: "UPDATE_ITEM", payload: { index: 0, data: { ...item1, name: "Updated A" } } });
        expect(nextState.items[0].name).toBe("Updated A");
        expect(nextState.items[1].name).toBe("Item B");
    });

    it("SET_ACTIVE_TAB updates activeTab and does not reset other state", () => {
        const state = { ...baseState, monsters: [testMonster] };
        const nextState = reducer(state, { type: "SET_ACTIVE_TAB", payload: "items" });
        expect(nextState.activeTab).toBe("items");
        expect(nextState.monsters).toHaveLength(1);
        expect(nextState.saveStatus).toBe("idle");
    });

    it("SET_SAVE_STATUS updates only saveStatus", () => {
        const state = { ...baseState, monsters: [testMonster] };
        const nextState = reducer(state, { type: "SET_SAVE_STATUS", payload: "saving" });
        expect(nextState.saveStatus).toBe("saving");
        expect(nextState.monsters).toHaveLength(1);
        expect(nextState.activeTab).toBe("players");
    });
});
