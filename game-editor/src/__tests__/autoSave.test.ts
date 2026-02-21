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
        await autoSave(mockDir, "monsters", [], cb);
        expect(cb).toHaveBeenCalledWith("saving");
    });

    it("calls onStatusChange with 'saved' after successful write", async () => {
        vi.mocked(writeJsonFile).mockResolvedValue(undefined);
        const cb = vi.fn();
        await autoSave(mockDir, "items", [], cb);
        expect(cb).toHaveBeenCalledWith("saved");
    });

    it("calls onStatusChange with 'error' when writeJsonFile throws", async () => {
        vi.mocked(writeJsonFile).mockRejectedValue(new Error("disk full"));
        const cb = vi.fn();
        await autoSave(mockDir, "classes", [], cb);
        expect(cb).toHaveBeenCalledWith("error");
    });

    it("calls onStatusChange with 'idle' 3 seconds after 'saved'", async () => {
        vi.mocked(writeJsonFile).mockResolvedValue(undefined);
        const cb = vi.fn();
        await autoSave(mockDir, "monsters", [], cb);
        expect(cb).not.toHaveBeenCalledWith("idle");
        vi.advanceTimersByTime(3000);
        expect(cb).toHaveBeenCalledWith("idle");
    });
});
