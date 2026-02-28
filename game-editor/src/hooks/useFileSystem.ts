import { useCallback } from "react";
import { DEFAULT_TEMPLATES } from "../constants";
import { readJsonFile, writeJsonFile } from "../utils/fileSystem";
import type { ItemData, MonsterData, PlayerData, PromptTemplates } from "../types";
import { pushNotification, useAppContext } from "../context/AppContext";

export function useFileSystem() {
    const { dispatch } = useAppContext();

    const loadAllData = useCallback(
        async (handle: FileSystemDirectoryHandle) => {
            try {
                const players = await readJsonFile<PlayerData[]>(handle, ["Data", "classes.json"]);
                const monsters = await readJsonFile<MonsterData[]>(handle, ["Data", "monsters.json"]);
                const items = await readJsonFile<ItemData[]>(handle, ["Data", "items.json"]);

                let promptTemplates: PromptTemplates;
                try {
                    promptTemplates = await readJsonFile<PromptTemplates>(handle, ["Data", "promptTemplates.json"]);

                    // Migration: check if player.global is still a flat record instead of {m, f}
                    // We check if "idle" exists directly on player.global
                    // eslint-disable-next-line @typescript-eslint/no-explicit-any
                    const anyGlobals = promptTemplates.player.global as any;
                    if (typeof anyGlobals.idle === "string") {
                        const legacyGlobals = { ...anyGlobals };
                        const legacyOverrides = { ...promptTemplates.player.overrides };

                        promptTemplates.player.global = {
                            m: legacyGlobals,
                            f: DEFAULT_TEMPLATES.player.global.f
                        };

                        // Migrate existing overrides to male prefix if they don't have it
                        const newOverrides: typeof promptTemplates.player.overrides = {};
                        for (const [key, value] of Object.entries(legacyOverrides)) {
                            if (!key.startsWith("m_") && !key.startsWith("f_")) {
                                newOverrides[`m_${key}`] = value;
                            } else {
                                newOverrides[key] = value;
                            }
                        }
                        promptTemplates.player.overrides = newOverrides;

                        // Save migrated format immediately
                        await writeJsonFile(handle, ["Data", "promptTemplates.json"], promptTemplates);
                        console.log("[useFileSystem] Migrated promptTemplates to new gendered format.");
                    }

                } catch {
                    // Bootstrap: first run
                    promptTemplates = DEFAULT_TEMPLATES;
                    await writeJsonFile(handle, ["Data", "promptTemplates.json"], DEFAULT_TEMPLATES);
                }

                dispatch({
                    type: "LOAD_DATA",
                    payload: { players, monsters, items, promptTemplates },
                });
            } catch (err) {
                console.error("[useFileSystem] Failed to load game data:", err);
                pushNotification(
                    dispatch,
                    `Failed to load game data: ${err instanceof Error ? err.message : String(err)}`,
                    "error"
                );
            }
        },
        [dispatch]
    );

    const selectDirectory = useCallback(async () => {
        try {
            const handle = await window.showDirectoryPicker({ mode: "readwrite" });
            dispatch({ type: "SET_DIR_HANDLE", payload: handle });
            await loadAllData(handle);
        } catch (err) {
            if (err instanceof DOMException && err.name === "AbortError") return;
            console.error("[useFileSystem] Directory selection failed:", err);
            pushNotification(
                dispatch,
                `Could not open directory: ${err instanceof Error ? err.message : String(err)}`,
                "error"
            );
        }
    }, [dispatch, loadAllData]);

    return { selectDirectory, loadAllData };
}
