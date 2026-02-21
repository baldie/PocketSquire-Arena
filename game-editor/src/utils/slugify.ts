export function slugify(input: string): string {
    return input
        .toLowerCase()
        .replace(/\s/g, "_")
        .replace(/[^a-z0-9_]/g, "");
}
