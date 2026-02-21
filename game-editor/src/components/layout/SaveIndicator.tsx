import type { SaveStatus } from "../../types";

interface SaveIndicatorProps {
    status: SaveStatus;
}

export default function SaveIndicator({ status }: SaveIndicatorProps) {
    if (status === "idle") return null;

    const config = {
        saving: { text: "Saving...", className: "text-yellow-400" },
        saved: { text: "Saved ✓", className: "text-green-400" },
        error: { text: "Save failed ✗", className: "text-red-400" },
    } as const;

    const { text, className } = config[status];
    return <span className={`text-sm font-medium ${className}`}>{text}</span>;
}
