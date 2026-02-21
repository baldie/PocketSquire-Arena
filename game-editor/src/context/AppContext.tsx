import React, { createContext, useContext, useReducer, type Dispatch, type ReactNode } from "react";
import type { AppAction, AppState } from "./types";
import type { Notification, NotificationSeverity } from "../types";
import { DEFAULT_TEMPLATES } from "../constants";

const initialState: AppState = {
    players: [],
    monsters: [],
    items: [],
    promptTemplates: DEFAULT_TEMPLATES,
    activeTab: "classes",
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

const MAX_NOTIFICATIONS = 5;

function reducer(state: AppState, action: AppAction): AppState {
    switch (action.type) {
        case "SET_DIR_HANDLE":
            return { ...state, dirHandle: action.payload };
        case "LOAD_DATA":
            return {
                ...state,
                players: action.payload.players,
                monsters: action.payload.monsters,
                items: action.payload.items,
                promptTemplates: action.payload.promptTemplates,
            };
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
            return {
                ...state,
                items,
                activeItemIndex:
                    state.activeItemIndex === action.payload ? null : state.activeItemIndex,
            };
        }
        case "UPDATE_PROMPT_TEMPLATES":
            return { ...state, promptTemplates: action.payload };
        case "SET_SAVE_STATUS":
            return { ...state, saveStatus: action.payload };
        case "ADD_GENERATION_HISTORY":
            return {
                ...state,
                generationHistory: [...state.generationHistory, action.payload],
            };
        case "SET_GENERATING":
            return { ...state, isGenerating: action.payload };
        case "SET_GENERATION_PROGRESS":
            return { ...state, generationProgress: action.payload };
        case "ADD_NOTIFICATION": {
            // Keep only most recent MAX_NOTIFICATIONS notifications; newest at front
            const notifications = [action.payload, ...state.notifications].slice(0, MAX_NOTIFICATIONS);
            return { ...state, notifications };
        }
        case "DISMISS_NOTIFICATION":
            return {
                ...state,
                notifications: state.notifications.filter((n) => n.id !== action.payload),
            };
        default:
            return state;
    }
}

interface AppContextValue {
    state: AppState;
    dispatch: React.Dispatch<AppAction>;
}

const AppContext = createContext<AppContextValue | null>(null);

export function AppProvider({ children }: { children: ReactNode }) {
    const [state, dispatch] = useReducer(reducer, initialState);
    return (
        <AppContext.Provider value={{ state, dispatch }}>
            {children}
        </AppContext.Provider>
    );
}

export function useAppContext(): AppContextValue {
    const ctx = useContext(AppContext);
    if (!ctx) throw new Error("useAppContext must be used within AppProvider");
    return ctx;
}

/** Helper to push a notification from anywhere; returns the Notification so
 *  callers can store the id for early dismissal if desired. */
export function pushNotification(
    dispatch: Dispatch<AppAction>,
    message: string,
    severity: NotificationSeverity = "info"
): Notification {
    const notification: Notification = {
        id: `${Date.now()}-${Math.random().toString(36).slice(2)}`,
        message,
        severity,
        timestamp: Date.now(),
    };
    dispatch({ type: "ADD_NOTIFICATION", payload: notification });
    return notification;
}
