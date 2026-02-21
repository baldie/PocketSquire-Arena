import { useState } from "react";
import { useAppContext } from "../../context/AppContext";
import { PLAYER_SLOTS, GENDERS } from "../../constants";
import { useImageGeneration } from "../../hooks/useImageGeneration";
import { slugify } from "../../utils/slugify";
import { PLAYER_CLASSES } from "../../constants";

import ClassList from "./ClassList";
import ClassDetail from "./ClassDetail";
import BatchGenerateModal from "../shared/BatchGenerateModal";
import type { ImageSlot } from "../../types";

export default function PlayersTab() {
    const { state, dispatch } = useAppContext();
    const { batchGenerate } = useImageGeneration();
    const [batchOpen, setBatchOpen] = useState(false);
    const [failedSlots, setFailedSlots] = useState<Array<{ entityName: string; slot: ImageSlot; error?: string }>>([]);

    const handleBatchGenerate = async () => {
        setBatchOpen(true);
        setFailedSlots([]);

        const entities: Parameters<typeof batchGenerate>[0] = [];

        for (const cls of PLAYER_CLASSES) {
            for (const gender of GENDERS) {
                for (const slot of PLAYER_SLOTS) {
                    entities.push({
                        slot,
                        pathSegments: ["Art", "Player", `player_${gender}_${cls.toLowerCase()}_${slot}.png`],
                        variables: { class: cls, gender: gender === "m" ? "male" : "female" },
                        entityType: "player",
                        entityKey: slugify(cls),
                        entityName: `${cls} (${gender})`,
                    });
                }
            }
        }

        await batchGenerate(
            entities,
            (current, total) => dispatch({ type: "SET_GENERATION_PROGRESS", payload: { current, total } }),
            (entityName, slot, error) => {
                setFailedSlots((prev) => [...prev, { entityName, slot, error }]);
            }
        );
    };

    return (
        <div className="flex flex-col h-full">
            <div className="flex items-center justify-between px-4 py-2 border-b border-gray-700">
                <h2 className="text-sm text-gray-400">Player Classes</h2>
                <button
                    id="batch-generate-players"
                    onClick={() => void handleBatchGenerate()}
                    disabled={state.isGenerating || !state.dirHandle}
                    className="px-3 py-1.5 text-sm bg-purple-600 hover:bg-purple-700 disabled:opacity-40 disabled:cursor-not-allowed text-white rounded-lg transition-colors"
                >
                    ⚡ Batch Generate
                </button>
            </div>
            <div className="flex flex-1 overflow-hidden">
                <div className="w-52 border-r border-gray-700 overflow-y-auto">
                    <ClassList />
                </div>
                <div className="flex-1 overflow-y-auto">
                    <ClassDetail />
                </div>
            </div>

            <BatchGenerateModal
                isOpen={batchOpen}
                progress={state.generationProgress}
                failedSlots={failedSlots}
                onRetryFailed={(_entityName, _slot) => {
                    // Single retry: find and regenerate
                    // Implementation: just re-run batch for that item
                }}
                onClose={() => setBatchOpen(false)}
            />
        </div>
    );
}
