import type { ImageSlot } from "../../types";

interface ImageSlotProps {
    slot: ImageSlot;
    imageDataUrl: string | null;
    isActive: boolean;
    isGenerating: boolean;
    onClick: () => void;
}

const SLOT_LABELS: Record<string, string> = {
    idle: "Idle", attack: "Attack", defend: "Defend", hit: "Hit",
    defeat: "Defeat", win: "Win", item: "Item",
    battle: "Battle", special_attack: "Special Atk", icon: "Icon",
};

export default function ImageSlot({ slot, imageDataUrl, isActive, isGenerating, onClick }: ImageSlotProps) {
    const label = SLOT_LABELS[slot] ?? slot;

    return (
        <button
            id={`slot-${slot}`}
            onClick={onClick}
            className={`relative flex flex-col items-center justify-center rounded-lg overflow-hidden transition-all border-2 aspect-square ${isActive
                ? "border-indigo-500 ring-2 ring-indigo-400/50"
                : "border-gray-600 hover:border-gray-400"
                }`}
            title={label}
            aria-label={`${label} slot${isActive ? " (active)" : ""}`}
        >
            {imageDataUrl ? (
                <img
                    src={imageDataUrl}
                    alt={label}
                    className="w-full h-full object-contain bg-gray-900 checkerboard"
                />
            ) : (
                <div className="flex flex-col items-center gap-1 p-2 bg-gray-800 w-full h-full justify-center">
                    <span className="text-lg">🖼️</span>
                    <span className="text-[10px] text-gray-400 text-center leading-tight">{label}</span>
                </div>
            )}

            {isGenerating && (
                <div className="absolute inset-0 bg-black/60 flex items-center justify-center">
                    <div className="w-5 h-5 border-2 border-indigo-400 border-t-transparent rounded-full animate-spin" />
                </div>
            )}
        </button>
    );
}
