import type { SaveStatus } from "../types";
import { writeJsonFile } from "./fileSystem";

const pendingSaves = new Map<string, ReturnType<typeof setTimeout>>();

export async function autoSave(
    dirHandle: FileSystemDirectoryHandle,
    entityType: "classes" | "monsters" | "items" | "promptTemplates",
    data: unknown,
    onStatusChange: (status: SaveStatus) => void
): Promise<void> {
    // Clear existing pending save for this type
    if (pendingSaves.has(entityType)) {
        clearTimeout(pendingSaves.get(entityType)!);
    }

    onStatusChange("saving");

    return new Promise((resolve) => {
        const timeout = setTimeout(async () => {
            pendingSaves.delete(entityType);
            try {
                await writeJsonFile(dirHandle, ["Data", `${entityType}.json`], data);
                onStatusChange("saved");
                // Clear "saved" message after 3 seconds
                setTimeout(() => onStatusChange("idle"), 3000);
                resolve();
            } catch (err) {
                console.error(`[autoSave] Failed to save ${entityType}:`, err);
                onStatusChange("error");
                resolve();
            }
        }, 1000); // 1 second debounce
        pendingSaves.set(entityType, timeout);
    });
}
