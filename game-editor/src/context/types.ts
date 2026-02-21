import type {
    Gender,
    GenerationHistoryEntry,
    ImageSlot,
    ItemData,
    MonsterData,
    Notification,
    PlayerData,
    PromptTemplates,
    SaveStatus,
} from "../types";
import type { PlayerClassName } from "../constants";

export interface AppState {
    players: PlayerData[];
    monsters: MonsterData[];
    items: ItemData[];
    promptTemplates: PromptTemplates;
    activeTab: "classes" | "monsters" | "items";
    activePlayerClass: PlayerClassName | null;
    activePlayerGender: Gender;
    activeMonsterIndex: number | null;
    activeItemIndex: number | null;
    activeSlot: ImageSlot | null;
    saveStatus: SaveStatus;
    generationHistory: GenerationHistoryEntry[];
    isGenerating: boolean;
    generationProgress: { current: number; total: number } | null;
    dirHandle: FileSystemDirectoryHandle | null;
    notifications: Notification[];
}

export type AppAction =
    | { type: "SET_DIR_HANDLE"; payload: FileSystemDirectoryHandle }
    | { type: "LOAD_DATA"; payload: { players: PlayerData[]; monsters: MonsterData[]; items: ItemData[]; promptTemplates: PromptTemplates } }
    | { type: "SET_ACTIVE_TAB"; payload: AppState["activeTab"] }
    | { type: "SET_ACTIVE_PLAYER_CLASS"; payload: PlayerClassName | null }
    | { type: "SET_ACTIVE_PLAYER_GENDER"; payload: Gender }
    | { type: "SET_ACTIVE_MONSTER"; payload: number | null }
    | { type: "SET_ACTIVE_ITEM"; payload: number | null }
    | { type: "SET_ACTIVE_SLOT"; payload: ImageSlot | null }
    | { type: "UPDATE_PLAYER"; payload: { index: number; data: PlayerData } }
    | { type: "UPDATE_MONSTER"; payload: { index: number; data: MonsterData } }
    | { type: "ADD_MONSTER"; payload: MonsterData }
    | { type: "DELETE_MONSTER"; payload: number }
    | { type: "UPDATE_ITEM"; payload: { index: number; data: ItemData } }
    | { type: "ADD_ITEM"; payload: ItemData }
    | { type: "DELETE_ITEM"; payload: number }
    | { type: "UPDATE_PROMPT_TEMPLATES"; payload: PromptTemplates }
    | { type: "SET_SAVE_STATUS"; payload: SaveStatus }
    | { type: "ADD_GENERATION_HISTORY"; payload: GenerationHistoryEntry }
    | { type: "SET_GENERATING"; payload: boolean }
    | { type: "SET_GENERATION_PROGRESS"; payload: { current: number; total: number } | null }
    | { type: "ADD_NOTIFICATION"; payload: Notification }
    | { type: "DISMISS_NOTIFICATION"; payload: string }; // payload = notification id
