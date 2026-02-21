// Augment global Window to include File System Access API
interface Window {
    showDirectoryPicker(options?: { mode?: "read" | "readwrite" }): Promise<FileSystemDirectoryHandle>;
}

// Allow importing image assets as URLs
declare module "*.png" {
    const url: string;
    export default url;
}
declare module "*.jpg" {
    const url: string;
    export default url;
}
declare module "*.svg" {
    const url: string;
    export default url;
}
