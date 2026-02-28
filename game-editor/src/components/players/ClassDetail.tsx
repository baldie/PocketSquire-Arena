import { useEffect, useState, useCallback } from "react";
import { useAppContext } from "../../context/AppContext";
import { PLAYER_CLASSES, PLAYER_CLASS_DESCRIPTIONS, PLAYER_SLOTS, GENDERS } from "../../constants";
import type { PlayerClassName } from "../../constants";
import { readImageAsDataUrl } from "../../utils/fileSystem";
import { getEffectiveTemplate, resolveTemplate } from "../../utils/resolveTemplate";
import { autoSave } from "../../utils/autoSave";
import { useImageGeneration } from "../../hooks/useImageGeneration";
import ImageSlotGrid from "../shared/ImageSlotGrid";
import PromptPanel from "../shared/PromptPanel";
import AudioInputField from "../shared/AudioInputField";
import type { Gender, ImageSlot, PlayerData, PlayerSlot } from "../../types";
import { slugify } from "../../utils/slugify";
import { deleteFile } from "../../utils/fileSystem";

function spritePath(gender: Gender, cls: string, slot: PlayerSlot): string[] {
    return ["Art", "Player", `${gender}_${cls.toLowerCase()}_${slot}.png`];
}

export default function ClassDetail() {
    const { state, dispatch } = useAppContext();
    const { generateImage } = useImageGeneration();
    const [imageUrls, setImageUrls] = useState<Partial<Record<ImageSlot, string | null>>>({});
    const [generatingSlot, setGeneratingSlot] = useState<ImageSlot | null>(null);

    const cls = state.activePlayerClass;
    const gender = state.activePlayerGender;

    // Find player data for this class+gender combo
    const playerKey = cls ? `${gender}_${cls.toLowerCase()}` : null;
    const playerIndex = cls
        ? state.players.findIndex((p: PlayerData) => p.name === playerKey)
        : -1;
    const player: PlayerData | null = playerIndex >= 0 ? state.players[playerIndex] : null;

    // Load images when class or gender changes
    useEffect(() => {
        if (!cls || !state.dirHandle) return;
        let cancelled = false;
        async function load() {
            const urls: Partial<Record<ImageSlot, string | null>> = {};
            for (const slot of PLAYER_SLOTS) {
                const path = spritePath(gender, cls!, slot);
                urls[slot] = await readImageAsDataUrl(state.dirHandle!, path);
            }
            if (!cancelled) setImageUrls(urls);
        }
        void load();
        return () => { cancelled = true; };
    }, [cls, gender, state.dirHandle]);

    // Live update when a new frame is generated (useful during batch generate)
    useEffect(() => {
        if (state.generationHistory.length === 0) return;
        const lastEntry = state.generationHistory[state.generationHistory.length - 1];
        if (lastEntry.entityKey === playerKey) {
            setImageUrls((prev) => ({ ...prev, [lastEntry.slot]: lastEntry.imageDataUrl }));
        }
    }, [state.generationHistory, playerKey]);

    const handleFieldBlur = useCallback(
        (field: keyof PlayerData, value: string | number) => {
            if (!cls || !state.dirHandle) return;
            const key = playerKey!;

            let updatedPlayer: PlayerData;
            if (player) {
                updatedPlayer = { ...player, [field]: value, class: cls };
            } else {
                // Create default entry
                updatedPlayer = {
                    name: key,
                    class: cls,
                    attackSoundId: "",
                    defendSoundId: "",
                    hitSoundId: "",
                    gender,
                    [field]: value,
                };
            }

            if (playerIndex >= 0) {
                dispatch({ type: "UPDATE_PLAYER", payload: { index: playerIndex, data: updatedPlayer } });
                void autoSave(state.dirHandle, "classes", state.players.map((p: PlayerData, i: number) => i === playerIndex ? updatedPlayer : p), (s) =>
                    dispatch({ type: "SET_SAVE_STATUS", payload: s })
                );
            } else {
                dispatch({ type: "UPDATE_PLAYER", payload: { index: state.players.length, data: updatedPlayer } });
                void autoSave(state.dirHandle, "classes", [...state.players, updatedPlayer], (s) =>
                    dispatch({ type: "SET_SAVE_STATUS", payload: s })
                );
            }
        },
        [cls, gender, player, playerIndex, playerKey, state.dirHandle, state.players, dispatch]
    );

    const getLocalOverride = (slot: ImageSlot): string => {
        if (!cls) return "";
        const key = playerKey!; // format: "m_squire"
        return state.promptTemplates.player.overrides[key]?.[slot as PlayerSlot] ?? "";
    };

    const handleOverrideChange = useCallback(
        (slot: ImageSlot, value: string) => {
            if (!cls || !state.dirHandle) return;
            const key = playerKey!; // format: "m_squire"
            const updated = {
                ...state.promptTemplates,
                player: {
                    ...state.promptTemplates.player,
                    overrides: {
                        ...state.promptTemplates.player.overrides,
                        [key]: {
                            ...state.promptTemplates.player.overrides[key],
                            [slot]: value,
                        },
                    },
                },
            };
            dispatch({ type: "UPDATE_PROMPT_TEMPLATES", payload: updated });
            void autoSave(state.dirHandle, "promptTemplates", updated, (s) =>
                dispatch({ type: "SET_SAVE_STATUS", payload: s })
            );
        },
        [cls, playerKey, state.dirHandle, state.promptTemplates, dispatch]
    );

    const handleGenerate = useCallback(
        async (slot: ImageSlot, referenceDataUrl?: string) => {
            if (!cls || !state.dirHandle) return;
            setGeneratingSlot(slot);
            const variables = {
                class: cls,
                gender: gender === "m" ? "male" : "female",
            };
            const path = spritePath(gender, cls, slot as PlayerSlot);
            try {
                const dataUrl = await generateImage(slot, path, variables, "player", playerKey!, referenceDataUrl);
                if (dataUrl) {
                    setImageUrls((prev) => ({ ...prev, [slot]: dataUrl }));
                }
            } finally {
                setGeneratingSlot(null);
            }
        },
        [cls, gender, playerKey, state.dirHandle, generateImage]
    );

    const handleDeleteSlot = useCallback(
        async (slot: ImageSlot) => {
            if (!cls || !state.dirHandle) return;
            const path = spritePath(gender, cls, slot as PlayerSlot);
            try {
                await deleteFile(state.dirHandle, path);
                setImageUrls((prev) => ({ ...prev, [slot]: null }));
            } catch (err) {
                console.error("Failed to delete image", err);
                alert("Failed to delete image file. Is it locked?");
            }
        },
        [cls, gender, state.dirHandle]
    );

    const activeSlot = state.activeSlot as PlayerSlot | null;

    if (!cls) {
        return (
            <div className="flex-1 flex items-center justify-center text-gray-500 text-sm">
                Select a class to edit
            </div>
        );
    }

    const localOverride = activeSlot ? getLocalOverride(activeSlot) : "";
    const effectiveTemplate = activeSlot
        ? getEffectiveTemplate(state.promptTemplates, "player", playerKey!, activeSlot)
        : "";
    const variables = { class: cls, gender: gender === "m" ? "male" : "female" };
    const { resolved: resolvedPrompt, unresolvedVars } = activeSlot
        ? resolveTemplate(effectiveTemplate, variables)
        : { resolved: "", unresolvedVars: [] };

    return (
        <div className="flex-1 overflow-y-auto p-4 space-y-4">
            {/* Header */}
            <div className="flex items-start justify-between">
                <div>
                    <h2 className="text-lg font-bold text-white">{cls}</h2>
                    <p className="text-sm text-gray-400 mt-0.5">{cls ? PLAYER_CLASS_DESCRIPTIONS[cls as keyof typeof PLAYER_CLASS_DESCRIPTIONS] : ""}</p>
                </div>
                {/* Gender toggle */}
                <div className="flex gap-1 p-1 bg-gray-700 rounded-lg">
                    {GENDERS.map((g: Gender) => (
                        <button
                            key={g}
                            id={`gender-${g}`}
                            onClick={() => dispatch({ type: "SET_ACTIVE_PLAYER_GENDER", payload: g })}
                            className={`px-4 py-1.5 text-sm font-medium rounded-md transition-colors ${gender === g
                                ? "bg-indigo-600 text-white"
                                : "text-gray-400 hover:text-white"
                                }`}
                        >
                            {g === "m" ? "M" : "F"}
                        </button>
                    ))}
                </div>
            </div>

            {/* Player data form */}
            <div className="bg-gray-800 rounded-lg p-4 border border-gray-700 space-y-3">
                <h3 className="text-sm font-semibold text-gray-300">Class Data</h3>
                <div className="grid grid-cols-3 gap-3">
                    {(["attackSoundId", "defendSoundId", "hitSoundId"] as const).map((f) => (
                        <div key={f}>
                            <label className="block text-xs text-gray-500 mb-1">
                                {f.replace("SoundId", " Sound")}
                            </label>
                            <AudioInputField
                                key={`${cls}-${gender}-${f}`}
                                id={`player-${f}`}
                                defaultValue={player?.[f] ?? ""}
                                onBlur={(val) => handleFieldBlur(f, val)}
                                dirHandle={state.dirHandle}
                                category="Player"
                            />
                        </div>
                    ))}
                </div>
            </div>

            {/* Copy prompts from */}
            <div className="flex items-center gap-2">
                <label className="text-sm text-gray-400 whitespace-nowrap">Copy prompts from:</label>
                <select
                    id="copy-prompts-from"
                    className="px-3 py-1.5 text-sm bg-gray-700 border border-gray-600 rounded text-white focus:outline-none"
                    defaultValue=""
                    onChange={(e) => {
                        const srcClass = e.target.value as PlayerClassName;
                        if (!srcClass || !cls) return;
                        const srcKey = `${gender}_${slugify(srcClass)}`;
                        const dstKey = playerKey!;
                        const srcOverrides = state.promptTemplates.player.overrides[srcKey] ?? {};
                        const updated = {
                            ...state.promptTemplates,
                            player: {
                                ...state.promptTemplates.player,
                                overrides: {
                                    ...state.promptTemplates.player.overrides,
                                    [dstKey]: { ...srcOverrides },
                                },
                            },
                        };
                        dispatch({ type: "UPDATE_PROMPT_TEMPLATES", payload: updated });
                        if (state.dirHandle) {
                            void autoSave(state.dirHandle, "promptTemplates", updated, (s) =>
                                dispatch({ type: "SET_SAVE_STATUS", payload: s })
                            );
                        }
                        (e.target as HTMLSelectElement).value = "";
                    }}
                >
                    <option value="">— select class —</option>
                    {PLAYER_CLASSES.filter((c) => c !== cls).map((c) => (
                        <option key={c} value={c}>{c}</option>
                    ))}
                </select>
            </div>

            {/* Image grid */}
            <ImageSlotGrid
                slots={PLAYER_SLOTS}
                imageDataUrls={imageUrls}
                activeSlot={state.activeSlot}
                generatingSlot={generatingSlot}
                onSlotClick={(slot) => dispatch({ type: "SET_ACTIVE_SLOT", payload: slot })}
                onDeleteSlot={handleDeleteSlot}
            />

            {/* Prompt panel */}
            {activeSlot && (
                <PromptPanel
                    localOverride={localOverride}
                    resolvedPrompt={resolvedPrompt}
                    unresolvedVars={unresolvedVars}
                    onOverrideChange={(v) => handleOverrideChange(activeSlot, v)}
                    onGenerate={(refUrl) => void handleGenerate(activeSlot, refUrl)}
                    isGenerating={state.isGenerating}
                />
            )}
        </div>
    );
}
