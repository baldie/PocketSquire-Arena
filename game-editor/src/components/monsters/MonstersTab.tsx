import { useState } from "react";
import { useAppContext } from "../../context/AppContext";
import { MONSTER_SLOTS } from "../../constants";
import { useImageGeneration } from "../../hooks/useImageGeneration";
import { slugify } from "../../utils/slugify";
import MonsterList from "./MonsterList";
import MonsterDetail from "./MonsterDetail";
import BatchGenerateModal from "../shared/BatchGenerateModal";
import type { ImageSlot, MonsterData } from "../../types";
import { autoSave } from "../../utils/autoSave";

const DEFAULT_MONSTER: MonsterData = {
    name: "New Monster",
    rank: 1,
    experience: 5,
    gold: 3,
    attackSoundId: "",
    specialAttackSoundId: "",
    defendSoundId: "",
    hitSoundId: "",
    defeatSoundId: "",
    attributes: { Strength: 5, Constitution: 10, Magic: 3, Dexterity: 3, Luck: 2, Defense: 2 },
};

export default function MonstersTab() {
    const { state, dispatch } = useAppContext();
    const { batchGenerate } = useImageGeneration();
    const [batchOpen, setBatchOpen] = useState(false);
    const [failedSlots, setFailedSlots] = useState<Array<{ entityName: string; slot: ImageSlot; error?: string }>>([]);

    const handleAddMonster = () => {
        if (!state.dirHandle) return;
        dispatch({ type: "ADD_MONSTER", payload: DEFAULT_MONSTER });
        dispatch({ type: "SET_ACTIVE_MONSTER", payload: state.monsters.length });
        void autoSave(state.dirHandle, "monsters", [...state.monsters, DEFAULT_MONSTER], (s) =>
            dispatch({ type: "SET_SAVE_STATUS", payload: s })
        );
    };

    const handleBatchGenerate = async () => {
        setBatchOpen(true);
        setFailedSlots([]);
        const entities: Parameters<typeof batchGenerate>[0] = state.monsters.flatMap((monster: MonsterData) =>
            MONSTER_SLOTS.map((slot) => ({
                slot,
                pathSegments: ["Art", "Monsters", `${slugify(monster.name)}_${slot}.png`],
                variables: { name: monster.name, rank: String(monster.rank) },
                entityType: "monster" as const,
                entityKey: slugify(monster.name),
                entityName: monster.name,
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
                <h2 className="text-sm text-gray-400">Monsters</h2>
                <div className="flex gap-2">
                    <button
                        id="batch-generate-monsters"
                        onClick={() => void handleBatchGenerate()}
                        disabled={state.isGenerating || !state.dirHandle}
                        className="px-3 py-1.5 text-sm bg-purple-600 hover:bg-purple-700 disabled:opacity-40 disabled:cursor-not-allowed text-white rounded-lg transition-colors"
                    >
                        ⚡ Batch Generate
                    </button>
                    <button
                        id="add-monster-btn"
                        onClick={handleAddMonster}
                        disabled={!state.dirHandle}
                        className="px-3 py-1.5 text-sm bg-green-700 hover:bg-green-600 disabled:opacity-40 disabled:cursor-not-allowed text-white rounded-lg transition-colors"
                    >
                        + New Monster
                    </button>
                </div>
            </div>
            <div className="flex flex-1 overflow-hidden">
                <div className="w-56 border-r border-gray-700 overflow-y-auto">
                    <MonsterList />
                </div>
                <div className="flex-1 overflow-y-auto">
                    <MonsterDetail />
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
