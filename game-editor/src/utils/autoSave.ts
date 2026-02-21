import type { SaveStatus } from "../types";
import { writeJsonFile } from "./fileSystem";

export async function autoSave(
    dirHandle: FileSystemDirectoryHandle,
    entityType: "classes" | "monsters" | "items" | "promptTemplates",
    data: unknown,
    onStatusChange: (status: SaveStatus) => void
): Promise<void> {
    onStatusChange("saving");
    try {
        await writeJsonFile(dirHandle, ["Data", `${entityType}.json`], data);
        onStatusChange("saved");
        setTimeout(() => onStatusChange("idle"), 3000);
    } catch (err) {
        console.error(`[autoSave] Failed to save ${entityType}:`, err);
        onStatusChange("error");
    }
}
