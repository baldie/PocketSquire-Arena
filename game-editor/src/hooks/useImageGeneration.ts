import { useCallback } from "react";
import { useAppContext } from "../context/AppContext";
import { writeImageFile } from "../utils/fileSystem";
import { resolveTemplate, getEffectiveTemplate } from "../utils/resolveTemplate";
import { autoSave } from "../utils/autoSave";
import type { ImageSlot, MonsterData, ItemData, PlayerData, GenerationHistoryEntry } from "../types";
import type { PlayerClassName } from "../constants";

const GEMINI_API_URL =
    "https://generativelanguage.googleapis.com/v1beta/models/imagen-3.0-generate-002:predict";

interface GeminiPrediction {
    bytesBase64Encoded: string;
    mimeType: string;
}

interface GeminiResponse {
    predictions?: GeminiPrediction[];
}

async function callGeminiImagen(apiKey: string, prompt: string): Promise<string> {
    let response: Response;
    try {
        response = await fetch(`${GEMINI_API_URL}?key=${apiKey}`, {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ instances: [{ prompt }], parameters: { sampleCount: 1 } }),
        });
    } catch {
        throw new Error("Network error. Check your connection and retry.");
    }

    if (response.status === 400) {
        const body = await response.json() as { error?: { message?: string } };
        if (body?.error?.message?.includes("SAFETY")) {
            throw new Error("Prompt was blocked by Imagen safety filters. Edit the prompt and try again.");
        }
        throw new Error(`Bad request: ${body?.error?.message ?? "Unknown error"}`);
    }
    if (response.status === 401) throw new Error("Invalid Gemini API key. Update it in Settings.");
    if (response.status === 429) throw new Error("Gemini rate limit reached. Wait a moment and retry.");
    if (!response.ok) throw new Error(`Gemini API error ${response.status}`);

    const data = await response.json() as GeminiResponse;
    if (!data.predictions || data.predictions.length === 0) {
        throw new Error("Gemini returned no image. Try a different prompt.");
    }

    return data.predictions[0].bytesBase64Encoded;
}

export function useImageGeneration() {
    const { state, dispatch } = useAppContext();

    const generateImage = useCallback(
        async (
            slot: ImageSlot,
            pathSegments: string[],
            variables: Parameters<typeof resolveTemplate>[1],
            entityType: "player" | "monster" | "item",
            entityKey: string
        ): Promise<string | null> => {
            const apiKey = localStorage.getItem("gemini_api_key");
            if (!apiKey || !state.dirHandle) return null;

            const template = getEffectiveTemplate(state.promptTemplates, entityType, entityKey, slot);
            const { resolved, unresolvedVars } = resolveTemplate(template, variables);
            if (unresolvedVars.length > 0) return null;

            dispatch({ type: "SET_GENERATING", payload: true });
            try {
                const base64 = await callGeminiImagen(apiKey, resolved);
                await writeImageFile(state.dirHandle, pathSegments, base64);

                const dataUrl = `data:image/png;base64,${base64}`;
                const entry: GenerationHistoryEntry = {
                    entityKey,
                    slot,
                    prompt: resolved,
                    timestamp: Date.now(),
                    imageDataUrl: dataUrl,
                };
                dispatch({ type: "ADD_GENERATION_HISTORY", payload: entry });

                // Auto-save prompt templates after successful generation
                if (state.dirHandle) {
                    await autoSave(
                        state.dirHandle,
                        "promptTemplates",
                        state.promptTemplates,
                        (status) => dispatch({ type: "SET_SAVE_STATUS", payload: status })
                    );
                }

                return dataUrl;
            } finally {
                dispatch({ type: "SET_GENERATING", payload: false });
            }
        },
        [state.dirHandle, state.promptTemplates, dispatch]
    );

    const batchGenerate = useCallback(
        async (
            entities: Array<{
                slot: ImageSlot;
                pathSegments: string[];
                variables: Parameters<typeof resolveTemplate>[1];
                entityType: "player" | "monster" | "item";
                entityKey: string;
                entityName: string;
            }>,
            onProgress: (current: number, total: number) => void,
            onFailure: (entityName: string, slot: ImageSlot, error: string) => void
        ) => {
            const apiKey = localStorage.getItem("gemini_api_key");
            if (!apiKey || !state.dirHandle) return;

            const dirHandle = state.dirHandle;
            dispatch({ type: "SET_GENERATING", payload: true });
            dispatch({ type: "SET_GENERATION_PROGRESS", payload: { current: 0, total: entities.length } });

            for (let i = 0; i < entities.length; i++) {
                const { slot, pathSegments, variables, entityType, entityKey, entityName } = entities[i];
                const template = getEffectiveTemplate(state.promptTemplates, entityType, entityKey, slot);
                const { resolved, unresolvedVars } = resolveTemplate(template, variables);

                if (unresolvedVars.length === 0) {
                    try {
                        const base64 = await callGeminiImagen(apiKey, resolved);
                        await writeImageFile(dirHandle, pathSegments, base64);
                        const dataUrl = `data:image/png;base64,${base64}`;
                        dispatch({
                            type: "ADD_GENERATION_HISTORY",
                            payload: { entityKey, slot, prompt: resolved, timestamp: Date.now(), imageDataUrl: dataUrl },
                        });
                    } catch (err) {
                        onFailure(entityName, slot, err instanceof Error ? err.message : "Unknown error");
                    }
                } else {
                    onFailure(entityName, slot, `Unresolved variables: ${unresolvedVars.join(", ")}`);
                }

                onProgress(i + 1, entities.length);
                dispatch({ type: "SET_GENERATION_PROGRESS", payload: { current: i + 1, total: entities.length } });

                // Throttle to avoid rate limits
                await new Promise<void>((r) => setTimeout(r, 300));
            }

            dispatch({ type: "SET_GENERATING", payload: false });
            dispatch({ type: "SET_GENERATION_PROGRESS", payload: null });
        },
        [state.dirHandle, state.promptTemplates, dispatch]
    );

    return { generateImage, batchGenerate };
}

export type { PlayerClassName, MonsterData, ItemData, PlayerData };
