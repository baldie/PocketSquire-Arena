import { useCallback, useState } from "react";
import { useAppContext } from "../../context/AppContext";
import {
    PERK_TYPES,
    VENDOR_TYPES,
    PERK_EFFECT_TYPES,
    PERK_TRIGGER_EVENTS,
    PERK_TARGETS,
    PERK_TIERS,
    PLAYER_CLASSES,
} from "../../constants";
import { autoSave } from "../../utils/autoSave";
import ConfirmModal from "../shared/ConfirmModal";
import PerkIconPanel from "./PerkIconPanel";
import type {
    ArenaPerkData,
    ArenaPerkEffectType,
    ArenaPerkType,
    PerkTarget,
    PerkTriggerEvent,
    VendorType,
} from "../../types";

// Consecutive trigger events that unlock the consecutiveCount field
const CONSECUTIVE_EVENTS: PerkTriggerEvent[] = [
    "ConsecutiveHits", "ConsecutiveWins", "ConsecutiveDodges",
    "ConsecutiveDefends", "ConsecutiveItemUses",
];

// Stack effects that unlock the maxStacks field
const STACK_EFFECTS: ArenaPerkEffectType[] = ["StackDamageBuff", "StackDodgeBuff"];

function isFieldRelevant(perk: ArenaPerkData, field: string): boolean {
    const triggered = perk.type === "Triggered";
    switch (field) {
        case "event": return triggered;
        case "perkTarget": return triggered;
        case "procPercent": return triggered;
        case "threshold": return triggered && perk.event === "HPBelowThreshold";
        case "consecutiveCount": return triggered && CONSECUTIVE_EVENTS.includes(perk.event as PerkTriggerEvent);
        case "maxStacks": return triggered && STACK_EFFECTS.includes(perk.effect as ArenaPerkEffectType);
        case "resetOn": return triggered && (perk.maxStacks ?? 0) > 0;
        case "duration": return triggered;
        case "oncePerBattle": return triggered;
        case "oncePerRun": return triggered;
        case "consumeOnUse": return triggered;
        case "yieldChanceBonus": return perk.effect === "YieldBonus";
        case "hpRestore": return perk.effect === "YieldBonus";
        case "value": return !!perk.effect;
        case "isPercent": return !!perk.effect;
        default: return true;
    }
}

// Small reusable label+field wrapper
function Field({ label, children }: { label: string; children: React.ReactNode }) {
    return (
        <div>
            <label className="block text-xs text-gray-500 mb-1">{label}</label>
            {children}
        </div>
    );
}

const inputCls = "w-full px-2 py-1.5 text-sm bg-gray-700 border border-gray-600 rounded text-white focus:outline-none focus:ring-1 focus:ring-indigo-500";
const selectCls = "w-full px-2 py-1.5 text-sm bg-gray-700 border border-gray-600 rounded text-white focus:outline-none";
const checkLabelCls = "flex items-center gap-2 text-sm text-gray-300 cursor-pointer";

export default function PerkDetail() {
    const { state, dispatch } = useAppContext();
    const [confirmDelete, setConfirmDelete] = useState(false);

    const perk = state.arenaPerks.find((p) => p.id === state.activePerkId) ?? null;

    const saveAll = useCallback(
        (updatedPerks: ArenaPerkData[]) => {
            if (!state.dirHandle) return;
            void autoSave(state.dirHandle, "arena_perks", updatedPerks, (s) =>
                dispatch({ type: "SET_SAVE_STATUS", payload: s })
            );
        },
        [state.dirHandle, dispatch]
    );

    const handleUpdate = useCallback(
        (updated: ArenaPerkData) => {
            dispatch({ type: "UPDATE_PERK", payload: { id: updated.id, data: updated } });
            saveAll(state.arenaPerks.map((p) => (p.id === updated.id ? updated : p)));
        },
        [dispatch, saveAll, state.arenaPerks]
    );

    const handleField = useCallback(
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
        (field: keyof ArenaPerkData, value: any) => {
            if (!perk) return;
            let updated: ArenaPerkData = { ...perk, [field]: value };

            // Clear triggered-only fields when switching to Passive
            if (field === "type" && value === "Passive") {
                updated = {
                    ...updated,
                    event: null,
                    perkTarget: null,
                    procPercent: 100,
                    threshold: null,
                    consecutiveCount: undefined,
                    maxStacks: undefined,
                    resetOn: null,
                    duration: undefined,
                    oncePerBattle: false,
                    oncePerRun: false,
                    consumeOnUse: false,
                };
            }
            handleUpdate(updated);
        },
        [perk, handleUpdate]
    );

    const handleDelete = () => {
        if (!perk) return;
        setConfirmDelete(false);
        dispatch({ type: "DELETE_PERK", payload: perk.id });
        if (state.dirHandle) {
            saveAll(state.arenaPerks.filter((p) => p.id !== perk.id));
        }
    };

    const handleDuplicate = () => {
        if (!perk) return;
        dispatch({ type: "DUPLICATE_PERK", payload: perk.id });
        const copy: ArenaPerkData = { ...perk, id: `${perk.id}_copy`, icon: undefined };
        if (state.dirHandle) {
            saveAll([...state.arenaPerks, copy]);
        }
    };

    if (!perk) {
        return (
            <div className="flex-1 flex items-center justify-center text-gray-500 text-sm h-full">
                <div className="text-center space-y-2">
                    <div className="text-4xl">⚡</div>
                    <p>Select a perk from the list, or click + New Perk to create one.</p>
                </div>
            </div>
        );
    }

    return (
        <div className="h-full overflow-y-auto flex flex-col p-4 gap-4">
            {/* Header */}
            <div className="flex items-start justify-between gap-4">
                <div>
                    <input
                        id="perk-name"
                        type="text"
                        defaultValue={perk.name}
                        key={`name-${perk.id}`}
                        onBlur={(e) => handleField("name", e.target.value)}
                        className="text-lg font-bold bg-transparent border-b border-transparent hover:border-gray-600 focus:border-indigo-500 text-white focus:outline-none w-full transition-colors"
                    />
                    <span className="text-xs text-gray-500 font-mono">id: {perk.id}</span>
                </div>
                <div className="flex gap-2 flex-shrink-0">
                    <button
                        id="duplicate-perk-btn"
                        onClick={handleDuplicate}
                        className="px-3 py-1.5 text-sm bg-gray-700 hover:bg-gray-600 text-gray-300 rounded-lg transition-colors"
                    >
                        ⧉ Duplicate
                    </button>
                    <button
                        id="delete-perk-btn"
                        onClick={() => setConfirmDelete(true)}
                        className="px-3 py-1.5 text-sm bg-red-700 hover:bg-red-600 text-white rounded-lg transition-colors"
                    >
                        🗑 Delete
                    </button>
                </div>
            </div>

            {/* Icon + Core Fields */}
            <div className="grid grid-cols-3 gap-4">
                {/* Icon panel (left 1/3) */}
                <div className="bg-gray-800 border border-gray-700 rounded-lg p-3">
                    <PerkIconPanel perk={perk} onUpdate={handleUpdate} />
                </div>

                {/* Core fields (right 2/3) */}
                <div className="col-span-2 bg-gray-800 border border-gray-700 rounded-lg p-4 space-y-3">
                    <h3 className="text-xs font-semibold text-gray-400 uppercase tracking-wider pb-1 border-b border-gray-700">
                        Core Fields
                    </h3>

                    <Field label="Description">
                        <textarea
                            id="perk-description"
                            defaultValue={perk.description}
                            key={`desc-${perk.id}`}
                            onBlur={(e) => handleField("description", e.target.value)}
                            rows={3}
                            className={`${inputCls} resize-none`}
                        />
                    </Field>

                    <div className="grid grid-cols-2 gap-3">
                        <Field label="Type">
                            <select
                                id="perk-type"
                                value={perk.type}
                                onChange={(e) => handleField("type", e.target.value as ArenaPerkType)}
                                className={selectCls}
                            >
                                {PERK_TYPES.map((t) => <option key={t} value={t}>{t}</option>)}
                            </select>
                        </Field>

                        <Field label="Sold By">
                            <select
                                id="perk-sold-by"
                                value={perk.soldBy}
                                onChange={(e) => handleField("soldBy", e.target.value as VendorType)}
                                className={selectCls}
                            >
                                {VENDOR_TYPES.map((v) => <option key={v} value={v}>{v}</option>)}
                            </select>
                        </Field>

                        <Field label="Cost (gold)">
                            <input
                                id="perk-cost"
                                type="number"
                                min={0}
                                step={50}
                                defaultValue={perk.cost}
                                key={`cost-${perk.id}`}
                                onBlur={(e) => handleField("cost", Number(e.target.value))}
                                className={inputCls}
                            />
                        </Field>

                        <Field label="Tier">
                            <select
                                id="perk-tier"
                                value={perk.tier}
                                onChange={(e) => handleField("tier", Number(e.target.value))}
                                className={selectCls}
                            >
                                {PERK_TIERS.map((t) => <option key={t} value={t}>{t}</option>)}
                            </select>
                        </Field>

                        <Field label="Effect">
                            <select
                                id="perk-effect"
                                value={perk.effect ?? ""}
                                onChange={(e) => handleField("effect", e.target.value || null)}
                                className={selectCls}
                            >
                                <option value="">— none —</option>
                                {PERK_EFFECT_TYPES.map((ef) => <option key={ef} value={ef}>{ef}</option>)}
                            </select>
                        </Field>

                        {isFieldRelevant(perk, "value") && (
                            <Field label="Value">
                                <input
                                    id="perk-value"
                                    type="number"
                                    defaultValue={perk.value ?? 0}
                                    key={`val-${perk.id}`}
                                    onBlur={(e) => handleField("value", Number(e.target.value))}
                                    className={inputCls}
                                />
                            </Field>
                        )}
                    </div>

                    {isFieldRelevant(perk, "isPercent") && (
                        <label className={checkLabelCls}>
                            <input
                                id="perk-is-percent"
                                type="checkbox"
                                checked={perk.isPercent ?? false}
                                onChange={(e) => handleField("isPercent", e.target.checked)}
                                className="w-4 h-4 rounded border-gray-600 bg-gray-700 text-indigo-600"
                            />
                            Value is a percentage
                        </label>
                    )}

                    {isFieldRelevant(perk, "yieldChanceBonus") && (
                        <Field label="Yield Chance Bonus (%)">
                            <input
                                id="perk-yield-bonus"
                                type="number"
                                defaultValue={perk.yieldChanceBonus ?? 0}
                                key={`yield-${perk.id}`}
                                onBlur={(e) => handleField("yieldChanceBonus", Number(e.target.value))}
                                className={inputCls}
                            />
                        </Field>
                    )}

                    {isFieldRelevant(perk, "hpRestore") && (
                        <Field label="HP Restore">
                            <input
                                id="perk-hp-restore"
                                type="number"
                                defaultValue={perk.hpRestore ?? 0}
                                key={`hpr-${perk.id}`}
                                onBlur={(e) => handleField("hpRestore", Number(e.target.value))}
                                className={inputCls}
                            />
                        </Field>
                    )}
                </div>
            </div>

            {/* Triggered-only fields */}
            {perk.type === "Triggered" && (
                <div className="bg-gray-800 border border-gray-700 rounded-lg p-4 space-y-3">
                    <h3 className="text-xs font-semibold text-gray-400 uppercase tracking-wider pb-1 border-b border-gray-700">
                        Triggered Fields
                    </h3>

                    <div className="grid grid-cols-2 gap-3">
                        <Field label="Trigger Event">
                            <select
                                id="perk-event"
                                value={perk.event ?? ""}
                                onChange={(e) => handleField("event", e.target.value as PerkTriggerEvent || null)}
                                className={selectCls}
                            >
                                <option value="">— none —</option>
                                {PERK_TRIGGER_EVENTS.map((ev) => <option key={ev} value={ev}>{ev}</option>)}
                            </select>
                        </Field>

                        <Field label="Perk Target">
                            <select
                                id="perk-target"
                                value={perk.perkTarget ?? ""}
                                onChange={(e) => handleField("perkTarget", e.target.value as PerkTarget || null)}
                                className={selectCls}
                            >
                                <option value="">— none —</option>
                                {PERK_TARGETS.map((t) => <option key={t} value={t}>{t}</option>)}
                            </select>
                        </Field>

                        <Field label="Proc Chance (%)">
                            <input
                                id="perk-proc-percent"
                                type="number"
                                min={1}
                                max={100}
                                defaultValue={perk.procPercent ?? 100}
                                key={`proc-${perk.id}`}
                                onBlur={(e) => handleField("procPercent", Number(e.target.value))}
                                className={inputCls}
                            />
                        </Field>

                        <Field label="Duration (turns, 0 = instant)">
                            <input
                                id="perk-duration"
                                type="number"
                                min={0}
                                defaultValue={perk.duration ?? 0}
                                key={`dur-${perk.id}`}
                                onBlur={(e) => handleField("duration", Number(e.target.value))}
                                className={inputCls}
                            />
                        </Field>

                        {isFieldRelevant(perk, "threshold") && (
                            <Field label="HP Threshold (%)">
                                <input
                                    id="perk-threshold"
                                    type="number"
                                    min={1}
                                    max={100}
                                    defaultValue={perk.threshold ?? ""}
                                    key={`thresh-${perk.id}`}
                                    onBlur={(e) => handleField("threshold", e.target.value ? Number(e.target.value) : null)}
                                    className={inputCls}
                                    placeholder="Leave blank if not HP-gated"
                                />
                            </Field>
                        )}

                        {isFieldRelevant(perk, "consecutiveCount") && (
                            <Field label="Consecutive Count">
                                <input
                                    id="perk-consecutive-count"
                                    type="number"
                                    min={0}
                                    defaultValue={perk.consecutiveCount ?? 0}
                                    key={`consec-${perk.id}`}
                                    onBlur={(e) => handleField("consecutiveCount", Number(e.target.value))}
                                    className={inputCls}
                                />
                            </Field>
                        )}

                        {isFieldRelevant(perk, "maxStacks") && (
                            <Field label="Max Stacks (0 = disabled)">
                                <input
                                    id="perk-max-stacks"
                                    type="number"
                                    min={0}
                                    defaultValue={perk.maxStacks ?? 0}
                                    key={`stacks-${perk.id}`}
                                    onBlur={(e) => handleField("maxStacks", Number(e.target.value))}
                                    className={inputCls}
                                />
                            </Field>
                        )}

                        {isFieldRelevant(perk, "resetOn") && (
                            <Field label="Reset On">
                                <select
                                    id="perk-reset-on"
                                    value={perk.resetOn ?? ""}
                                    onChange={(e) => handleField("resetOn", e.target.value as PerkTriggerEvent || null)}
                                    className={selectCls}
                                >
                                    <option value="">— none —</option>
                                    {PERK_TRIGGER_EVENTS.map((ev) => <option key={ev} value={ev}>{ev}</option>)}
                                </select>
                            </Field>
                        )}
                    </div>

                    <div className="flex flex-wrap gap-5 pt-1">
                        <label className={checkLabelCls}>
                            <input
                                id="perk-once-per-battle"
                                type="checkbox"
                                checked={perk.oncePerBattle ?? false}
                                onChange={(e) => handleField("oncePerBattle", e.target.checked)}
                                className="w-4 h-4 rounded border-gray-600 bg-gray-700 text-indigo-600"
                            />
                            Once per battle
                        </label>
                        <label className={checkLabelCls}>
                            <input
                                id="perk-once-per-run"
                                type="checkbox"
                                checked={perk.oncePerRun ?? false}
                                onChange={(e) => handleField("oncePerRun", e.target.checked)}
                                className="w-4 h-4 rounded border-gray-600 bg-gray-700 text-indigo-600"
                            />
                            Once per run
                        </label>
                        <label className={checkLabelCls}>
                            <input
                                id="perk-consume-on-use"
                                type="checkbox"
                                checked={perk.consumeOnUse ?? false}
                                onChange={(e) => handleField("consumeOnUse", e.target.checked)}
                                className="w-4 h-4 rounded border-gray-600 bg-gray-700 text-indigo-600"
                            />
                            Consume on use
                        </label>
                    </div>
                </div>
            )}

            {/* Prerequisites — flex-1 so it expands to fill remaining space */}
            <div className="bg-gray-800 border border-gray-700 rounded-lg p-4 flex flex-col flex-1 min-h-0 gap-3">
                <h3 className="text-xs font-semibold text-gray-400 uppercase tracking-wider pb-1 border-b border-gray-700 flex-shrink-0">
                    Prerequisites
                </h3>

                <div className="grid grid-cols-2 gap-3">
                    <Field label="Min Level">
                        <input
                            id="perk-min-level"
                            type="number"
                            min={1}
                            defaultValue={perk.prerequisites.minLevel}
                            key={`minlv-${perk.id}`}
                            onBlur={(e) =>
                                handleField("prerequisites", {
                                    ...perk.prerequisites,
                                    minLevel: Number(e.target.value),
                                })
                            }
                            className={inputCls}
                        />
                    </Field>

                    <Field label="Required Class (— any —)">
                        <select
                            id="perk-class"
                            value={perk.prerequisites.class ?? ""}
                            onChange={(e) =>
                                handleField("prerequisites", {
                                    ...perk.prerequisites,
                                    class: e.target.value || null,
                                })
                            }
                            className={selectCls}
                        >
                            <option value="">— any class —</option>
                            {PLAYER_CLASSES.map((c) => (
                                <option key={c} value={c}>{c}</option>
                            ))}
                        </select>
                    </Field>
                </div>

                {/* Required Perks multi-select — grows to fill all left-over space */}
                <div className="flex flex-col flex-1 min-h-0">
                    <label className="block text-xs text-gray-500 mb-1 flex-shrink-0">Required Perks</label>
                    <div className="border border-gray-600 rounded overflow-y-auto flex-1 min-h-0 bg-gray-700 p-2 space-y-1">
                        {state.arenaPerks
                            .filter((p) => p.id !== perk.id)
                            .map((candidate) => {
                                const isChecked = (perk.prerequisites.requiredPerks ?? []).includes(candidate.id);
                                return (
                                    <label
                                        key={candidate.id}
                                        className="flex items-center gap-2 text-xs text-gray-300 cursor-pointer hover:text-white"
                                    >
                                        <input
                                            type="checkbox"
                                            checked={isChecked}
                                            onChange={(e) => {
                                                const current = perk.prerequisites.requiredPerks ?? [];
                                                const updated = e.target.checked
                                                    ? [...current, candidate.id]
                                                    : current.filter((id) => id !== candidate.id);
                                                handleField("prerequisites", {
                                                    ...perk.prerequisites,
                                                    requiredPerks: updated.length > 0 ? updated : null,
                                                });
                                            }}
                                            className="w-3.5 h-3.5 rounded border-gray-600 bg-gray-600 text-indigo-600 flex-shrink-0"
                                        />
                                        <span className="font-medium">{candidate.name}</span>
                                        <span className="text-gray-500 font-mono">{candidate.id}</span>
                                    </label>
                                );
                            })}
                        {state.arenaPerks.length <= 1 && (
                            <p className="text-xs text-gray-500">No other perks available.</p>
                        )}
                    </div>
                </div>
            </div>

            <ConfirmModal
                isOpen={confirmDelete}
                message={`Delete "${perk.name}"? This cannot be undone.`}
                onConfirm={handleDelete}
                onCancel={() => setConfirmDelete(false)}
            />
        </div>
    );
}
