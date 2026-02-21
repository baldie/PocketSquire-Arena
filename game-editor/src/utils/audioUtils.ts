/**
 * Resolves a sound ID (e.g. "Monsters/GenericAttack1") to an object URL
 * for audio playback from the game's Assets/_Game/Audio/ directory.
 * Returns null if the file is not found.
 */
export async function readAudioAsUrl(
    dirHandle: FileSystemDirectoryHandle,
    soundId: string
): Promise<string | null> {
    if (!soundId.trim()) return null;

    // soundId may be "Monsters/GenericAttack1" or just "GenericAttack1"
    const parts = soundId.split("/").filter(Boolean);
    const extensions = ["wav", "mp3", "ogg"];

    // Navigate to the Audio directory
    async function getHandle(): Promise<FileSystemFileHandle | null> {
        try {
            // Descend through Audio/ and any sub-path in the soundId
            let dir = dirHandle;
            dir = await dir.getDirectoryHandle("Audio", { create: false });
            for (let i = 0; i < parts.length - 1; i++) {
                dir = await dir.getDirectoryHandle(parts[i], { create: false });
            }
            const basename = parts[parts.length - 1];
            // Try each extension
            for (const ext of extensions) {
                try {
                    return await dir.getFileHandle(`${basename}.${ext}`, { create: false });
                } catch {
                    // not found with this extension, try next
                }
            }
            return null;
        } catch {
            return null;
        }
    }

    const fileHandle = await getHandle();
    if (!fileHandle) return null;

    const file = await fileHandle.getFile();
    return URL.createObjectURL(file);
}
