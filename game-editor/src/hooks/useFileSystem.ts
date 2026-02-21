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
