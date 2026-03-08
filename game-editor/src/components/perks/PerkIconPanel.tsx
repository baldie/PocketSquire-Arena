import { useEffect, useState, useCallback, useRef } from "react";
import { useAppContext } from "../../context/AppContext";
import { readImageAsDataUrl } from "../../utils/fileSystem";
import { autoSave } from "../../utils/autoSave";
import { useImageGeneration } from "../../hooks/useImageGeneration";
import type { ArenaPerkData, PromptTemplates } from "../../types";

interface Props {
    perk: ArenaPerkData;
    onUpdate: (updated: ArenaPerkData) => void;
}

const DEFAULT_PROMPT = (name: string) =>
    `A fantasy RPG perk icon for '${name}', stylized badge design, pixel art, transparent background`;

export default function PerkIconPanel({ perk, onUpdate }: Props) {
    const { state, dispatch } = useAppContext();
    const { generateImage } = useImageGeneration();
    const [imageUrl, setImageUrl] = useState<string | null>(null);
    const [isGenerating, setIsGenerating] = useState(false);
    const [referenceImage, setReferenceImage] = useState<string | null>(null);
    const fileInputRef = useRef<HTMLInputElement>(null);

    // The prompt is the per-perk override stored in promptTemplates, falling back to the default.
    // This mirrors how ItemDetail manages overrides, so the generate hook picks it up automatically.
    const currentOverride = state.promptTemplates.item.overrides[perk.id]?.icon ?? "";
    const displayPrompt = currentOverride || DEFAULT_PROMPT(perk.name);

    // Reload image when perk id changes
    useEffect(() => {
        if (!state.dirHandle) return;
        let cancelled = false;
        readImageAsDataUrl(state.dirHandle, ["Art", "Perks", `${perk.id}.png`]).then((url) => {
            if (!cancelled) setImageUrl(url);
        }).catch(() => {
            if (!cancelled) setImageUrl(null);
        });
        return () => { cancelled = true; };
    }, [perk.id, state.dirHandle]);

    // Store the prompt as a promptTemplates override so generateImage picks it up automatically
    const handlePromptChange = useCallback((value: string) => {
        const updated: PromptTemplates = {
            ...state.promptTemplates,
            item: {
                ...state.promptTemplates.item,
                overrides: {
                    ...state.promptTemplates.item.overrides,
                    [perk.id]: {
                        ...state.promptTemplates.item.overrides[perk.id],
                        icon: value,
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
    }, [perk.id, state.promptTemplates, state.dirHandle, dispatch]);

    const handleGenerate = useCallback(async () => {
        if (!state.dirHandle) return;
        setIsGenerating(true);
        try {
            // generateImage reads the override from state.promptTemplates automatically
            const dataUrl = await generateImage(
                "icon",
                ["Art", "Perks", `${perk.id}.png`],
                { name: perk.name },
                "item",
                perk.id,
                referenceImage ?? undefined
            );
            if (dataUrl) {
                setImageUrl(dataUrl);
                // Only update the perk's icon field — parent owns saving the perk list
                onUpdate({ ...perk, icon: `${perk.id}.png` });
            }
        } finally {
            setIsGenerating(false);
        }
    }, [perk, state.dirHandle, generateImage, onUpdate, referenceImage]);

    const handleUpload = useCallback(async () => {
        if (!state.dirHandle) return;
        const dirHandle = state.dirHandle; // capture to avoid stale closure in callbacks
        try {
            const input = document.createElement("input");
            input.type = "file";
            input.accept = "image/png,image/jpeg,image/webp";
            input.onchange = () => {
                const file = input.files?.[0];
                if (!file) return;
                const reader = new FileReader();
                reader.onload = async () => {
                    const dataUrl = reader.result as string;
                    const base64 = dataUrl.split(",")[1];
                    const artDir = await dirHandle.getDirectoryHandle("Art", { create: true });
                    const perksDir = await artDir.getDirectoryHandle("Perks", { create: true });
                    const fileHandle = await perksDir.getFileHandle(`${perk.id}.png`, { create: true });
                    const writable = await fileHandle.createWritable();
                    const binaryStr = atob(base64);
                    const bytes = new Uint8Array(binaryStr.length);
                    for (let i = 0; i < binaryStr.length; i++) bytes[i] = binaryStr.charCodeAt(i);
                    await writable.write(bytes);
                    await writable.close();
                    setImageUrl(dataUrl);
                    // Only update icon field — parent owns saving the perk list
                    onUpdate({ ...perk, icon: `${perk.id}.png` });
                    dispatch({
                        type: "ADD_GENERATION_HISTORY",
                        payload: {
                            entityKey: perk.id,
                            slot: "icon",
                            prompt: "Manual upload",
                            timestamp: Date.now(),
                            imageDataUrl: dataUrl,
                        },
                    });
                };
                reader.readAsDataURL(file);
            };
            input.click();
        } catch (err) {
            console.error("[PerkIconPanel] Upload failed:", err);
        }
    }, [perk, state.dirHandle, onUpdate, dispatch]);

    return (
        <div className="space-y-3">
            {/* Icon display */}
            <div className="flex items-center justify-center w-32 h-32 rounded-lg border border-gray-600 bg-gray-700 overflow-hidden mx-auto">
                {imageUrl ? (
                    <img src={imageUrl} alt={perk.name} className="max-w-full max-h-full object-contain" />
                ) : (
                    <div className="flex flex-col items-center gap-1 text-gray-500">
                        <span className="text-2xl">✨</span>
                        <span className="text-xs text-center leading-tight px-2">
                            {perk.name.charAt(0).toUpperCase()}
                        </span>
                    </div>
                )}
            </div>

            {/* Prompt textarea — writes to promptTemplates override so generateImage uses it */}
            <div>
                <label className="block text-xs text-gray-500 mb-1">Image prompt</label>
                <textarea
                    value={displayPrompt}
                    onChange={(e) => handlePromptChange(e.target.value)}
                    rows={3}
                    className="w-full px-2 py-1.5 text-xs bg-gray-700 border border-gray-600 rounded text-gray-300 focus:outline-none focus:ring-1 focus:ring-indigo-500 resize-none"
                />
            </div>

            {/* Hidden file input for reference image */}
            <input
                type="file"
                accept="image/*"
                className="hidden"
                ref={fileInputRef}
                onChange={(e) => {
                    const file = e.target.files?.[0];
                    if (!file) return;
                    const reader = new FileReader();
                    reader.onloadend = () => setReferenceImage(reader.result as string);
                    reader.readAsDataURL(file);
                }}
            />

            {/* Buttons */}
            <div className="flex gap-2">
                {/* Reference image picker — mirrors PromptPanel pattern */}
                <button
                    onClick={() => fileInputRef.current?.click()}
                    disabled={!state.dirHandle}
                    className="px-2 py-1.5 text-xs bg-gray-600 hover:bg-gray-500 disabled:opacity-40 disabled:cursor-not-allowed text-white rounded-lg transition-colors flex items-center justify-center relative min-w-[2rem]"
                    title="Select reference image for generation"
                >
                    {referenceImage ? (
                        <div className="relative w-5 h-5">
                            <img src={referenceImage} alt="Reference" className="w-full h-full object-cover rounded border border-gray-500" />
                            <div
                                onClick={(e) => {
                                    e.stopPropagation();
                                    setReferenceImage(null);
                                    if (fileInputRef.current) fileInputRef.current.value = "";
                                }}
                                className="absolute -top-1.5 -right-1.5 bg-red-500 hover:bg-red-400 text-white rounded-full w-3.5 h-3.5 flex items-center justify-center text-[9px] cursor-pointer"
                            >
                                ×
                            </div>
                        </div>
                    ) : (
                        "🖼️"
                    )}
                </button>
                <button
                    id={`perk-generate-icon-${perk.id}`}
                    onClick={() => void handleGenerate()}
                    disabled={isGenerating || state.isGenerating || !state.dirHandle}
                    className="flex-1 py-1.5 text-xs bg-purple-700 hover:bg-purple-600 disabled:opacity-40 disabled:cursor-not-allowed text-white rounded-lg transition-colors"
                >
                    {isGenerating ? "Generating…" : "🎨 Generate Icon"}
                </button>
                <button
                    id={`perk-upload-icon-${perk.id}`}
                    onClick={() => void handleUpload()}
                    disabled={!state.dirHandle}
                    className="flex-1 py-1.5 text-xs bg-gray-600 hover:bg-gray-500 disabled:opacity-40 disabled:cursor-not-allowed text-white rounded-lg transition-colors"
                >
                    📁 Upload
                </button>
            </div>
        </div>
    );
}
