import { useEffect, useState, useCallback } from "react";
import { useAppContext } from "../../context/AppContext";
import { MONSTER_SLOTS } from "../../constants";
import { readImageAsDataUrl } from "../../utils/fileSystem";
import { getEffectiveTemplate, resolveTemplate } from "../../utils/resolveTemplate";
import { autoSave } from "../../utils/autoSave";
import { useImageGeneration } from "../../hooks/useImageGeneration";
import { slugify } from "../../utils/slugify";
import ImageSlotGrid from "../shared/ImageSlotGrid";
import PromptPanel from "../shared/PromptPanel";
import AttributeEditor from "../shared/AttributeEditor";
import ConfirmModal from "../shared/ConfirmModal";
import AudioButton from "../shared/AudioButton";
import type { Attributes, ImageSlot, MonsterData, MonsterSlot } from "../../types";

export default function MonsterDetail() {
    const { state, dispatch } = useAppContext();
    const { generateImage } = useImageGeneration();
    const [imageUrls, setImageUrls] = useState<Partial<Record<ImageSlot, string | null>>>({});
    const [generatingSlot, setGeneratingSlot] = useState<ImageSlot | null>(null);
    const [confirmDelete, setConfirmDelete] = useState(false);
    const [confirmDeleteImages, setConfirmDeleteImages] = useState(false);

    const idx = state.activeMonsterIndex;
    const monster = idx !== null ? state.monsters[idx] : null;

    useEffect(() => {
        if (monster === null || !state.dirHandle) return;
        let cancelled = false;
        async function load() {
            const slug = slugify(monster!.name);
            const urls: Partial<Record<ImageSlot, string | null>> = {};
            for (const slot of MONSTER_SLOTS) {
                urls[slot] = await readImageAsDataUrl(state.dirHandle!, ["Sprites", "Monsters", `${slug}_${slot}.png`]);
            }
            if (!cancelled) setImageUrls(urls);
        }
        void load();
        return () => { cancelled = true; };
    }, [monster, state.dirHandle]);

    const saveMonsters = useCallback(
        (updated: MonsterData[]) => {
            if (!state.dirHandle) return;
            void autoSave(state.dirHandle, "monsters", updated, (s) =>
                dispatch({ type: "SET_SAVE_STATUS", payload: s })
            );
        },
        [state.dirHandle, dispatch]
    );

    const handleFieldBlur = useCallback(
        (field: keyof MonsterData, value: string | number) => {
            if (idx === null || !monster) return;
            const updated = { ...monster, [field]: value };
            dispatch({ type: "UPDATE_MONSTER", payload: { index: idx, data: updated } });
            saveMonsters(state.monsters.map((m, i) => i === idx ? updated : m));
        },
        [idx, monster, dispatch, saveMonsters, state.monsters]
    );

    const handleAttrChange = useCallback(
        (attrs: Attributes) => {
            if (idx === null || !monster) return;
            const updated = { ...monster, attributes: attrs };
            dispatch({ type: "UPDATE_MONSTER", payload: { index: idx, data: updated } });
            saveMonsters(state.monsters.map((m, i) => i === idx ? updated : m));
        },
        [idx, monster, dispatch, saveMonsters, state.monsters]
    );

    const handleDuplicate = () => {
        if (!monster || !state.dirHandle) return;
        const copy: MonsterData = { ...monster, name: `${monster.name} (copy)` };
        dispatch({ type: "ADD_MONSTER", payload: copy });
        saveMonsters([...state.monsters, copy]);
    };

    const handleConfirmDelete = () => {
        if (idx === null) return;
        setConfirmDelete(false);
        dispatch({ type: "DELETE_MONSTER", payload: idx });
        saveMonsters(state.monsters.filter((_, i) => i !== idx));
        setConfirmDeleteImages(true);
    };

    const handleDeleteImages = async (doDelete: boolean) => {
        setConfirmDeleteImages(false);
        if (!doDelete || !monster || !state.dirHandle) return;
        const { deleteFile } = await import("../../utils/fileSystem");
        const slug = slugify(monster.name);
        for (const slot of MONSTER_SLOTS) {
            await deleteFile(state.dirHandle, ["Sprites", "Monsters", `${slug}_${slot}.png`]);
        }
    };

    const handleGenerate = useCallback(
        async (slot: ImageSlot) => {
            if (!monster || !state.dirHandle) return;
            setGeneratingSlot(slot);
            const slug = slugify(monster.name);
            const path = ["Sprites", "Monsters", `${slug}_${slot}.png`];
            const variables = { name: monster.name, rank: String(monster.rank) };
            try {
                const dataUrl = await generateImage(slot, path, variables, "monster", slug);
                if (dataUrl) setImageUrls((prev) => ({ ...prev, [slot]: dataUrl }));
            } finally {
                setGeneratingSlot(null);
            }
        },
        [monster, state.dirHandle, generateImage]
    );

    const activeSlot = state.activeSlot as MonsterSlot | null;

    // Check for same-name monsters (shared sprite pool warning)
    const hasSameNameWarning = monster
        ? state.monsters.filter((m) => m.name === monster.name).length > 1
        : false;

    if (!monster || idx === null) {
        return (
            <div className="flex-1 flex items-center justify-center text-gray-500 text-sm">
                Select a monster to edit
            </div>
        );
    }

    const globalTemplate = activeSlot
        ? state.promptTemplates.monster.global[activeSlot]
        : "";
    const slug = slugify(monster.name);
    const effectiveTemplate = activeSlot
        ? getEffectiveTemplate(state.promptTemplates, "monster", slug, activeSlot)
        : "";
    const variables = { name: monster.name, rank: String(monster.rank) };
    const { resolved: resolvedPrompt, unresolvedVars } = activeSlot
        ? resolveTemplate(effectiveTemplate, variables)
        : { resolved: "", unresolvedVars: [] };
    const localOverride = activeSlot
        ? state.promptTemplates.monster.overrides[slug]?.[activeSlot] ?? ""
        : "";

    const handleOverrideChange = (slot: ImageSlot, value: string) => {
        const updated = {
            ...state.promptTemplates,
            monster: {
                ...state.promptTemplates.monster,
                overrides: {
                    ...state.promptTemplates.monster.overrides,
                    [slug]: {
                        ...state.promptTemplates.monster.overrides[slug],
                        [slot]: value,
                    },
                },
            },
        };
        dispatch({ type: "UPDATE_PROMPT_TEMPLATES", payload: updated });
        if (state.dirHandle) {
            void autoSave(state.dirHandle, "promptTemplates", updated, (s) =>
                dispatch({ type: "SET_SAVE_STATUS", payload: s })
            );
        }
    };

    return (
        <div className="flex-1 overflow-y-auto p-4 space-y-4">
            {/* Header */}
            <div className="flex items-center justify-between">
                <h2 className="text-lg font-bold text-white">{monster.name}</h2>
                <div className="flex gap-2">
                    <button
                        id="duplicate-monster-btn"
                        onClick={handleDuplicate}
                        className="px-3 py-1.5 text-sm bg-gray-600 hover:bg-gray-500 text-white rounded-lg transition-colors"
                    >
                        Duplicate
                    </button>
                    <button
                        id="delete-monster-btn"
                        onClick={() => setConfirmDelete(true)}
                        className="px-3 py-1.5 text-sm bg-red-700 hover:bg-red-600 text-white rounded-lg transition-colors"
                    >
                        🗑 Delete
                    </button>
                </div>
            </div>

            {hasSameNameWarning && (
                <div className="text-xs text-yellow-400 bg-yellow-900/30 rounded px-3 py-2 border border-yellow-700/50">
                    ⚠️ Another monster shares this name — they share the same sprite pool.
                </div>
            )}

            {/* Form */}
            <div className="bg-gray-800 rounded-lg p-4 border border-gray-700 space-y-3">
                <div className="grid grid-cols-2 gap-3">
                    <div className="col-span-2">
                        <label className="block text-xs text-gray-500 mb-1">Name</label>
                        <input
                            id="monster-name"
                            type="text"
                            defaultValue={monster.name}
                            key={`name-${idx}`}
                            onBlur={(e) => handleFieldBlur("name", e.target.value)}
                            className="w-full px-2 py-1.5 text-sm bg-gray-700 border border-gray-600 rounded text-white focus:outline-none focus:ring-1 focus:ring-indigo-500"
                        />
                    </div>
                    {(["rank", "health", "maxHealth", "experience", "gold"] as const).map((f) => (
                        <div key={f}>
                            <label className="block text-xs text-gray-500 mb-1 capitalize">{f}</label>
                            <input
                                id={`monster-${f}`}
                                type="number"
                                min={1}
                                defaultValue={monster[f]}
                                key={`${f}-${idx}`}
                                onBlur={(e) => handleFieldBlur(f, parseInt(e.target.value, 10))}
                                className="w-full px-2 py-1.5 text-sm bg-gray-700 border border-gray-600 rounded text-white focus:outline-none focus:ring-1 focus:ring-indigo-500"
                            />
                        </div>
                    ))}
                </div>
                <div className="grid grid-cols-2 gap-3">
                    {(["attackSoundId", "specialAttackSoundId", "defendSoundId", "hitSoundId", "defeatSoundId"] as const).map((f) => (
                        <div key={f}>
                            <label className="block text-xs text-gray-500 mb-1">
                                {f.replace("SoundId", " Sound")}
                            </label>
                            <div className="flex gap-1">
                                <input
                                    id={`monster-${f}`}
                                    type="text"
                                    defaultValue={monster[f]}
                                    key={`${f}-${idx}`}
                                    onBlur={(e) => handleFieldBlur(f, e.target.value)}
                                    className="flex-1 min-w-0 px-2 py-1.5 text-sm bg-gray-700 border border-gray-600 rounded text-white focus:outline-none focus:ring-1 focus:ring-indigo-500"
                                />
                                {state.dirHandle && monster[f] && (
                                    <AudioButton soundId={monster[f]} dirHandle={state.dirHandle} />
                                )}
                            </div>
                        </div>
                    ))}
                </div>
            </div>

            <AttributeEditor attributes={monster.attributes} onChange={handleAttrChange} />

            <ImageSlotGrid
                slots={MONSTER_SLOTS}
                imageDataUrls={imageUrls}
                activeSlot={state.activeSlot}
                generatingSlot={generatingSlot}
                onSlotClick={(slot) => dispatch({ type: "SET_ACTIVE_SLOT", payload: slot })}
            />

            {activeSlot && (
                <PromptPanel
                    globalTemplate={globalTemplate}
                    localOverride={localOverride}
                    resolvedPrompt={resolvedPrompt}
                    unresolvedVars={unresolvedVars}
                    onOverrideChange={(v) => handleOverrideChange(activeSlot, v)}
                    onGenerate={() => void handleGenerate(activeSlot)}
                    onGenerateAll={() => {
                        void (async () => {
                            for (const slot of MONSTER_SLOTS) await handleGenerate(slot);
                        })();
                    }}
                    isGenerating={state.isGenerating}
                />
            )}

            <ConfirmModal
                isOpen={confirmDelete}
                message={`Delete "${monster.name}"?`}
                onConfirm={handleConfirmDelete}
                onCancel={() => setConfirmDelete(false)}
            />

            {/* Second confirm: delete images */}
            <ConfirmModal
                isOpen={confirmDeleteImages}
                message={`Also delete image files for "${monster.name}"?`}
                onConfirm={() => void handleDeleteImages(true)}
                onCancel={() => void handleDeleteImages(false)}
            />
        </div>
    );
}
