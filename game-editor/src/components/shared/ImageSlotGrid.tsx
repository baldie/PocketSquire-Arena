import type { ImageSlot as ImageSlotType } from "../../types";
import ImageSlot from "./ImageSlot";

interface ImageSlotGridProps {
    slots: ImageSlotType[];
    imageDataUrls: Partial<Record<ImageSlotType, string | null>>;
    activeSlot: ImageSlotType | null;
    generatingSlot: ImageSlotType | null;
    onSlotClick: (slot: ImageSlotType) => void;
}

export default function ImageSlotGrid({
    slots,
    imageDataUrls,
    activeSlot,
    generatingSlot,
    onSlotClick,
}: ImageSlotGridProps) {
    return (
        <div className="grid grid-cols-4 gap-2">
            {slots.map((slot) => (
                <ImageSlot
                    key={slot}
                    slot={slot}
                    imageDataUrl={imageDataUrls[slot] ?? null}
                    isActive={activeSlot === slot}
                    isGenerating={generatingSlot === slot}
                    onClick={() => onSlotClick(slot)}
                />
            ))}
        </div>
    );
}
