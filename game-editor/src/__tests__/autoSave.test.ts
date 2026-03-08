import { describe, it, expect, vi, beforeEach, afterEach } from "vitest";
import { autoSave } from "../utils/autoSave";

// Mock writeJsonFile
vi.mock("../utils/fileSystem", () => ({
    writeJsonFile: vi.fn(),
}));

import { writeJsonFile } from "../utils/fileSystem";

describe("autoSave", () => {
    const mockDir = {} as FileSystemDirectoryHandle;

    beforeEach(() => {
        vi.useFakeTimers();
        vi.clearAllMocks();
    });

    afterEach(() => {
        vi.useRealTimers();
    });

    it("calls onStatusChange with 'saving' before write", async () => {
        const mock = vi.mocked(writeJsonFile);
        mock.mockResolvedValue(undefined);
        const cb = vi.fn();
        const savePromise = autoSave(mockDir, "monsters", [], cb);
        // 'saving' is called immediately (before debounce fires)
        expect(cb).toHaveBeenCalledWith("saving");
        // Flush debounce so the Promise resolves and we don't leak timers
        await vi.runAllTimersAsync();
        await savePromise;
    });

    it("calls onStatusChange with 'saved' after successful write", async () => {
        vi.mocked(writeJsonFile).mockResolvedValue(undefined);
        const cb = vi.fn();
        const savePromise = autoSave(mockDir, "items", [], cb);
        await vi.runAllTimersAsync();
        await savePromise;
        expect(cb).toHaveBeenCalledWith("saved");
    });

    it("calls onStatusChange with 'error' when writeJsonFile throws", async () => {
        vi.mocked(writeJsonFile).mockRejectedValue(new Error("disk full"));
        const cb = vi.fn();
        const savePromise = autoSave(mockDir, "classes", [], cb);
        await vi.runAllTimersAsync();
        await savePromise;
        expect(cb).toHaveBeenCalledWith("error");
    });

    it("calls onStatusChange with 'idle' 3 seconds after 'saved'", async () => {
        vi.mocked(writeJsonFile).mockResolvedValue(undefined);
        const cb = vi.fn();
        const savePromise = autoSave(mockDir, "monsters", [], cb);
        // Advance only the 1s debounce (triggers the write + 'saved' callback)
        await vi.advanceTimersByTimeAsync(1000);
        await savePromise;
        expect(cb).not.toHaveBeenCalledWith("idle");
        // Advance the 3s 'idle' reset timer
        await vi.advanceTimersByTimeAsync(3000);
        expect(cb).toHaveBeenCalledWith("idle");
    });
});
