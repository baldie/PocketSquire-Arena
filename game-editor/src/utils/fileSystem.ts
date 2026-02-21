// Navigates nested directory handles and reads/writes JSON + image files.

async function navigateToDir(
    dirHandle: FileSystemDirectoryHandle,
    segments: string[]
): Promise<FileSystemDirectoryHandle> {
    let current = dirHandle;
    for (const segment of segments) {
        current = await current.getDirectoryHandle(segment, { create: false });
    }
    return current;
}

async function navigateToDirCreate(
    dirHandle: FileSystemDirectoryHandle,
    segments: string[]
): Promise<FileSystemDirectoryHandle> {
    let current = dirHandle;
    for (const segment of segments) {
        current = await current.getDirectoryHandle(segment, { create: true });
    }
    return current;
}

export async function readJsonFile<T>(
    dirHandle: FileSystemDirectoryHandle,
    pathSegments: string[]
): Promise<T> {
    const dirs = pathSegments.slice(0, -1);
    const filename = pathSegments[pathSegments.length - 1];
    const parentDir = dirs.length > 0 ? await navigateToDir(dirHandle, dirs) : dirHandle;
    const fileHandle = await parentDir.getFileHandle(filename, { create: false });
    const file = await fileHandle.getFile();
    const text = await file.text();
    return JSON.parse(text) as T;
}

export async function writeJsonFile(
    dirHandle: FileSystemDirectoryHandle,
    pathSegments: string[],
    data: unknown
): Promise<void> {
    const dirs = pathSegments.slice(0, -1);
    const filename = pathSegments[pathSegments.length - 1];
    const parentDir = dirs.length > 0 ? await navigateToDirCreate(dirHandle, dirs) : dirHandle;

    // 1. Read current file contents for backup
    let currentContents = "";
    try {
        const existingHandle = await parentDir.getFileHandle(filename, { create: false });
        const existingFile = await existingHandle.getFile();
        currentContents = await existingFile.text();
    } catch {
        // File doesn't exist yet — no backup needed
    }

    // 2. Write backup
    if (currentContents) {
        const bakHandle = await parentDir.getFileHandle(filename + ".bak", { create: true });
        const bakWritable = await bakHandle.createWritable();
        await bakWritable.write(currentContents);
        await bakWritable.close();
    }

    // 3. Write new JSON
    const newHandle = await parentDir.getFileHandle(filename, { create: true });
    const writable = await newHandle.createWritable();
    await writable.write(JSON.stringify(data, null, 4));
    await writable.close();
}

export async function writeImageFile(
    dirHandle: FileSystemDirectoryHandle,
    pathSegments: string[],
    base64Data: string
): Promise<void> {
    const dirs = pathSegments.slice(0, -1);
    const filename = pathSegments[pathSegments.length - 1];
    const parentDir = dirs.length > 0 ? await navigateToDirCreate(dirHandle, dirs) : dirHandle;

    // Convert base64 to binary
    const binaryStr = atob(base64Data);
    const bytes = new Uint8Array(binaryStr.length);
    for (let i = 0; i < binaryStr.length; i++) {
        bytes[i] = binaryStr.charCodeAt(i);
    }

    const fileHandle = await parentDir.getFileHandle(filename, { create: true });
    const writable = await fileHandle.createWritable();
    await writable.write(bytes);
    await writable.close();
}

export async function readImageAsDataUrl(
    dirHandle: FileSystemDirectoryHandle,
    pathSegments: string[]
): Promise<string | null> {
    try {
        const dirs = pathSegments.slice(0, -1);
        const filename = pathSegments[pathSegments.length - 1];
        const parentDir = dirs.length > 0 ? await navigateToDir(dirHandle, dirs) : dirHandle;
        const fileHandle = await parentDir.getFileHandle(filename, { create: false });
        const file = await fileHandle.getFile();
        return new Promise((resolve, reject) => {
            const reader = new FileReader();
            reader.onload = () => resolve(reader.result as string);
            reader.onerror = reject;
            reader.readAsDataURL(file);
        });
    } catch (err) {
        if (err instanceof DOMException && err.name === "NotFoundError") {
            return null;
        }
        // Propagate unexpected errors
        throw err;
    }
}

export async function deleteFile(
    dirHandle: FileSystemDirectoryHandle,
    pathSegments: string[]
): Promise<void> {
    try {
        const dirs = pathSegments.slice(0, -1);
        const filename = pathSegments[pathSegments.length - 1];
        const parentDir = dirs.length > 0 ? await navigateToDir(dirHandle, dirs) : dirHandle;
        await parentDir.removeEntry(filename);
    } catch (err) {
        if (err instanceof DOMException && err.name === "NotFoundError") {
            return; // Nothing to delete
        }
        throw err;
    }
}
