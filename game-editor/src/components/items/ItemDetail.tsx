import { useEffect, useState, useCallback, useRef } from "react";
import { useAppContext } from "../../context/AppContext";
import { ITEM_SLOTS } from "../../constants";
import { readImageAsDataUrl } from "../../utils/fileSystem";
import { getEffectiveTemplate, resolveTemplate } from "../../utils/resolveTemplate";
import { autoSave } from "../../utils/autoSave";
import { useImageGeneration } from "../../hooks/useImageGeneration";
import { slugify } from "../../utils/slugify";
import ImageSlotGrid from "../shared/ImageSlotGrid";
import PromptPanel from "../shared/PromptPanel";
import ConfirmModal from "../shared/ConfirmModal";
import AudioInputField from "../shared/AudioInputField";
import type { ImageSlot, ItemData, ItemTarget } from "../../types";

export default function ItemDetail() {
    const { state, dispatch } = useAppContext();
    const { generateImage } = useImageGeneration();
    const [imageUrl, setImageUrl] = useState<string | null>(null);
    const [generatingSlot, setGeneratingSlot] = useState<ImageSlot | null>(null);
    const [confirmDelete, setConfirmDelete] = useState(false);
    const [priceError, setPriceError] = useState<string | null>(null);
    // Track whether user has manually edited the sprite field
    const spriteManuallyEdited = useRef(false);

    const idx = state.activeItemIndex;
    const item = idx !== null ? state.items[idx] : null;

    useEffect(() => {
        if (!item || !state.dirHandle) return;
        spriteManuallyEdited.current = false; // reset on item change
        let cancelled = false;
        readImageAsDataUrl(state.dirHandle, ["Art", "Items", `${item.sprite}.png`]).then((url) => {
            if (!cancelled) setImageUrl(url);
        });
        return () => { cancelled = true; };
    }, [item?.id, state.dirHandle]);

    const saveItems = useCallback(
        (updated: ItemData[]) => {
            if (!state.dirHandle) return;
            void autoSave(state.dirHandle, "items", updated, (s) =>
                dispatch({ type: "SET_SAVE_STATUS", payload: s })
            );
        },
        [state.dirHandle, dispatch]
    );

    const handleFieldBlur = useCallback(
        (field: keyof ItemData, value: string | number | boolean) => {
            if (idx === null || !item) return;
            if (field === "price") {
                const n = Number(value);
                if (!Number.isInteger(n) || n < 1) {
                    setPriceError("Price must be a positive integer.");
                    return;
                }
                setPriceError(null);
            }
            const updated = { ...item, [field]: value };
            dispatch({ type: "UPDATE_ITEM", payload: { index: idx, data: updated } });
            saveItems(state.items.map((it: ItemData, i: number) => i === idx ? updated : it));
        },
        [idx, item, dispatch, saveItems, state.items]
    );

    const handleNameBlur = useCallback(
        (name: string) => {
            if (idx === null || !item) return;
            const updates: Partial<ItemData> = { name };
            if (!spriteManuallyEdited.current) {
                updates.sprite = slugify(name);
            }
            const updated = { ...item, ...updates };
            dispatch({ type: "UPDATE_ITEM", payload: { index: idx, data: updated } });
            saveItems(state.items.map((it: ItemData, i: number) => i === idx ? updated : it));
        },
        [idx, item, dispatch, saveItems, state.items]
    );

    const handleConfirmDelete = () => {
        if (idx === null) return;
        setConfirmDelete(false);
        dispatch({ type: "DELETE_ITEM", payload: idx });
        saveItems(state.items.filter((_: ItemData, i: number) => i !== idx));
    };

    const handleGenerate = useCallback(
        async (slot: ImageSlot, referenceDataUrl?: string) => {
            if (!item || !state.dirHandle) return;
            setGeneratingSlot(slot);
            const path = ["Art", "Items", `${item.sprite}.png`];
            try {
                const dataUrl = await generateImage(slot, path, { name: item.name }, "item", slugify(item.name), referenceDataUrl);
                if (dataUrl) setImageUrl(dataUrl);
            } finally {
                setGeneratingSlot(null);
            }
        },
        [item, state.dirHandle, generateImage]
    );

    const activeSlot = state.activeSlot;

    if (!item || idx === null) {
        return (
            <div className="flex-1 flex items-center justify-center text-gray-500 text-sm">
                Select an item to edit
            </div>
        );
    }

    const slug = slugify(item.name);
    const globalTemplate = state.promptTemplates.item.global.icon;
    const effectiveTemplate = activeSlot
        ? getEffectiveTemplate(state.promptTemplates, "item", slug, activeSlot)
        : "";
    const { resolved: resolvedPrompt, unresolvedVars } = activeSlot
        ? resolveTemplate(effectiveTemplate, { name: item.name })
        : { resolved: "", unresolvedVars: [] };
    const localOverride = activeSlot
        ? state.promptTemplates.item.overrides[slug]?.icon ?? ""
        : "";

    const handleOverrideChange = (value: string) => {
        const updated = {
            ...state.promptTemplates,
            item: {
                ...state.promptTemplates.item,
                overrides: {
                    ...state.promptTemplates.item.overrides,
                    [slug]: { ...state.promptTemplates.item.overrides[slug], icon: value },
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
            <div className="flex items-center justify-between">
                <div>
                    <h2 className="text-lg font-bold text-white">{item.name}</h2>
                    <span className="text-xs text-gray-500">ID: {item.id}</span>
                </div>
                <button
                    id="delete-item-btn"
                    onClick={() => setConfirmDelete(true)}
                    className="px-3 py-1.5 text-sm bg-red-700 hover:bg-red-600 text-white rounded-lg transition-colors"
                >
                    🗑 Delete
                </button>
            </div>

            <div className="bg-gray-800 rounded-lg p-4 border border-gray-700 space-y-3">
                <div className="grid grid-cols-2 gap-3">
                    <div className="col-span-2">
                        <label className="block text-xs text-gray-500 mb-1">Name</label>
                        <input
                            id="item-name"
                            type="text"
                            defaultValue={item.name}
                            key={`name-${idx}`}
                            onBlur={(e) => handleNameBlur(e.target.value)}
                            className="w-full px-2 py-1.5 text-sm bg-gray-700 border border-gray-600 rounded text-white focus:outline-none focus:ring-1 focus:ring-indigo-500"
                        />
                    </div>

                    <div className="col-span-2">
                        <label className="block text-xs text-gray-500 mb-1">Description</label>
                        <textarea
                            id="item-description"
                            defaultValue={item.description}
                            key={`desc-${idx}`}
                            onBlur={(e) => handleFieldBlur("description", e.target.value)}
                            rows={2}
                            className="w-full px-2 py-1.5 text-sm bg-gray-700 border border-gray-600 rounded text-white focus:outline-none focus:ring-1 focus:ring-indigo-500 resize-none"
                        />
                    </div>

                    <div>
                        <label className="block text-xs text-gray-500 mb-1">Target</label>
                        <select
                            id="item-target"
                            defaultValue={item.target}
                            key={`target-${idx}`}
                            onBlur={(e) => handleFieldBlur("target", e.target.value as ItemTarget)}
                            className="w-full px-2 py-1.5 text-sm bg-gray-700 border border-gray-600 rounded text-white focus:outline-none"
                        >
                            <option value="self">Self</option>
                            <option value="enemy">Enemy</option>
                        </select>
                    </div>

                    <div className="flex items-center gap-2 pt-4">
                        <input
                            id="item-stackable"
                            type="checkbox"
                            defaultChecked={item.stackable}
                            key={`stackable-${idx}`}
                            onChange={(e) => handleFieldBlur("stackable", e.target.checked)}
                            className="w-4 h-4 rounded border-gray-600 bg-gray-700 text-indigo-600"
                        />
                        <label htmlFor="item-stackable" className="text-sm text-gray-300">Stackable</label>
                    </div>

                    <div>
                        <label className="block text-xs text-gray-500 mb-1">Sprite</label>
                        <input
                            id="item-sprite"
                            type="text"
                            defaultValue={item.sprite}
                            key={`sprite-${idx}`}
                            onChange={() => { spriteManuallyEdited.current = true; }}
                            onBlur={(e) => handleFieldBlur("sprite", e.target.value)}
                            className="w-full px-2 py-1.5 text-sm bg-gray-700 border border-gray-600 rounded text-white focus:outline-none focus:ring-1 focus:ring-indigo-500"
                        />
                        {/* Sprite preview — always shown; placeholder when image not found */}
                        <div className="mt-2 flex items-center justify-center w-16 h-16 rounded overflow-hidden border border-gray-600 bg-gray-700 checkerboard">
                            {imageUrl ? (
                                <img src={imageUrl} alt={item.name} className="max-w-full max-h-full object-contain" />
                            ) : (
                                <span className="text-xs text-gray-500 text-center leading-tight px-1">no<br />image</span>
                            )}
                        </div>
                    </div>

                    <div>
                        <label className="block text-xs text-gray-500 mb-1">Sound Effect</label>
                        <AudioInputField
                            id="item-sound-effect"
                            defaultValue={item.sound_effect}
                            key={`sound-${idx}`}
                            onBlur={(val: string) => handleFieldBlur("sound_effect", val)}
                            dirHandle={state.dirHandle}
                            category="Items"
                        />
                    </div>

                    <div>
                        <label className="block text-xs text-gray-500 mb-1">Price</label>
                        <input
                            id="item-price"
                            type="number"
                            min={1}
                            step={1}
                            defaultValue={item.price}
                            key={`price-${idx}`}
                            onBlur={(e) => handleFieldBlur("price", parseInt(e.target.value, 10))}
                            className={`w-full px-2 py-1.5 text-sm bg-gray-700 border rounded text-white focus:outline-none focus:ring-1 focus:ring-indigo-500 ${priceError ? "border-red-500" : "border-gray-600"
                                }`}
                        />
                        {priceError && <p className="text-xs text-red-400 mt-1">{priceError}</p>}
                    </div>
                </div>
            </div>

            <ImageSlotGrid
                slots={ITEM_SLOTS}
                imageDataUrls={{ icon: imageUrl }}
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
                    onOverrideChange={handleOverrideChange}
                    onGenerate={(refDataUrl) => void handleGenerate(activeSlot, refDataUrl)}
                    onGenerateAll={() => void handleGenerate("icon")}
                    isGenerating={state.isGenerating}
                />
            )}

            <ConfirmModal
                isOpen={confirmDelete}
                message={`Delete "${item.name}"?`}
                onConfirm={handleConfirmDelete}
                onCancel={() => setConfirmDelete(false)}
            />
        </div>
    );
}
