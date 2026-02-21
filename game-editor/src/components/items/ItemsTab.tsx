import { useState } from "react";
import { useAppContext } from "../../context/AppContext";
import { ITEM_SLOTS } from "../../constants";
import { useImageGeneration } from "../../hooks/useImageGeneration";
import { slugify } from "../../utils/slugify";
import ItemList from "./ItemList";
import ItemDetail from "./ItemDetail";
import BatchGenerateModal from "../shared/BatchGenerateModal";
import type { ImageSlot, ItemData } from "../../types";
import { autoSave } from "../../utils/autoSave";

export default function ItemsTab() {
    const { state, dispatch } = useAppContext();
    const { batchGenerate } = useImageGeneration();
    const [batchOpen, setBatchOpen] = useState(false);
    const [failedSlots, setFailedSlots] = useState<Array<{ entityName: string; slot: ImageSlot; error?: string }>>([]);

    const handleAddItem = () => {
        if (!state.dirHandle) return;
        const nextId = state.items.length > 0 ? Math.max(...state.items.map((it) => it.id)) + 1 : 1;
        const newItem: ItemData = {
            id: nextId,
            name: "New Item",
            description: "",
            target: "self",
            stackable: true,
            sprite: "new_item",
            sound_effect: "",
            price: 1,
        };
        dispatch({ type: "ADD_ITEM", payload: newItem });
        dispatch({ type: "SET_ACTIVE_ITEM", payload: state.items.length });
        void autoSave(state.dirHandle, "items", [...state.items, newItem], (s) =>
            dispatch({ type: "SET_SAVE_STATUS", payload: s })
        );
    };

    const handleBatchGenerate = async () => {
        setBatchOpen(true);
        setFailedSlots([]);
        const entities: Parameters<typeof batchGenerate>[0] = state.items.flatMap((item) =>
            ITEM_SLOTS.map((slot) => ({
                slot,
                pathSegments: ["Sprites", "Items", `${item.sprite}.png`],
                variables: { name: item.name },
                entityType: "item" as const,
                entityKey: slugify(item.name),
                entityName: item.name,
            }))
        );
        await batchGenerate(
            entities,
            (current, total) => dispatch({ type: "SET_GENERATION_PROGRESS", payload: { current, total } }),
            (entityName, slot, error) => setFailedSlots((prev) => [...prev, { entityName, slot, error }])
        );
    };

    return (
        <div className="flex flex-col h-full">
            <div className="flex items-center justify-between px-4 py-2 border-b border-gray-700">
                <h2 className="text-sm text-gray-400">Items</h2>
                <div className="flex gap-2">
                    <button
                        id="batch-generate-items"
                        onClick={() => void handleBatchGenerate()}
                        disabled={state.isGenerating || !state.dirHandle}
                        className="px-3 py-1.5 text-sm bg-purple-600 hover:bg-purple-700 disabled:opacity-40 disabled:cursor-not-allowed text-white rounded-lg transition-colors"
                    >
                        ⚡ Batch Generate
                    </button>
                    <button
                        id="add-item-btn"
                        onClick={handleAddItem}
                        disabled={!state.dirHandle}
                        className="px-3 py-1.5 text-sm bg-green-700 hover:bg-green-600 disabled:opacity-40 disabled:cursor-not-allowed text-white rounded-lg transition-colors"
                    >
                        + New Item
                    </button>
                </div>
            </div>
            <div className="flex flex-1 overflow-hidden">
                <div className="w-52 border-r border-gray-700 overflow-y-auto">
                    <ItemList />
                </div>
                <div className="flex-1 overflow-y-auto">
                    <ItemDetail />
                </div>
            </div>
            <BatchGenerateModal
                isOpen={batchOpen}
                progress={state.generationProgress}
                failedSlots={failedSlots}
                onRetryFailed={() => { }}
                onClose={() => setBatchOpen(false)}
            />
        </div>
    );
}
