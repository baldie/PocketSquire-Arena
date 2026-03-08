import { useState } from "react";
import { useAppContext } from "../../context/AppContext";
import { PERK_TYPES, VENDOR_TYPES } from "../../constants";
import { autoSave } from "../../utils/autoSave";
import { slugify } from "../../utils/slugify";
import type { ArenaPerkData, ArenaPerkType, VendorType } from "../../types";

interface Props {
    onClose: () => void;
}

export default function NewPerkModal({ onClose }: Props) {
    const { state, dispatch } = useAppContext();
    const [id, setId] = useState("");
    const [name, setName] = useState("");
    const [type, setType] = useState<ArenaPerkType>("Passive");
    const [soldBy, setSoldBy] = useState<VendorType>("Shopkeeper");
    const [idError, setIdError] = useState<string | null>(null);

    const handleIdBlur = () => {
        const clean = slugify(id);
        setId(clean);
        if (!clean) {
            setIdError("ID is required.");
        } else if (state.arenaPerks.some((p) => p.id === clean)) {
            setIdError("ID already exists.");
        } else {
            setIdError(null);
        }
    };

    const handleCreate = () => {
        const cleanId = slugify(id);
        if (!cleanId) { setIdError("ID is required."); return; }
        if (state.arenaPerks.some((p) => p.id === cleanId)) { setIdError("ID already exists."); return; }
        if (!name.trim()) return;

        const newPerk: ArenaPerkData = {
            id: cleanId,
            name: name.trim(),
            description: "",
            type,
            soldBy,
            cost: 100,
            tier: 0,
            prerequisites: { minLevel: 1 },
            procPercent: 100,
            value: 0,
        };

        dispatch({ type: "ADD_PERK", payload: newPerk });
        if (state.dirHandle) {
            const updatedPerks = [...state.arenaPerks, newPerk];
            void autoSave(state.dirHandle, "arena_perks", updatedPerks, (s) =>
                dispatch({ type: "SET_SAVE_STATUS", payload: s })
            );
        }
        onClose();
    };

    return (
        <div className="fixed inset-0 bg-black/60 flex items-center justify-center z-50" onClick={onClose}>
            <div
                className="bg-gray-800 border border-gray-600 rounded-xl p-6 w-96 space-y-4 shadow-2xl"
                onClick={(e) => e.stopPropagation()}
            >
                <h3 className="text-lg font-bold text-white">New Perk</h3>

                <div>
                    <label className="block text-xs text-gray-400 mb-1">ID (snake_case)</label>
                    <input
                        id="new-perk-id"
                        type="text"
                        value={id}
                        onChange={(e) => setId(e.target.value)}
                        onBlur={handleIdBlur}
                        placeholder="e.g. keen_eye"
                        className={`w-full px-3 py-2 text-sm bg-gray-700 border rounded text-white focus:outline-none focus:ring-1 focus:ring-indigo-500 ${
                            idError ? "border-red-500" : "border-gray-600"
                        }`}
                    />
                    {idError && <p className="text-xs text-red-400 mt-1">{idError}</p>}
                </div>

                <div>
                    <label className="block text-xs text-gray-400 mb-1">Name</label>
                    <input
                        id="new-perk-name"
                        type="text"
                        value={name}
                        onChange={(e) => setName(e.target.value)}
                        placeholder="e.g. Keen Eye"
                        className="w-full px-3 py-2 text-sm bg-gray-700 border border-gray-600 rounded text-white focus:outline-none focus:ring-1 focus:ring-indigo-500"
                    />
                </div>

                <div className="grid grid-cols-2 gap-3">
                    <div>
                        <label className="block text-xs text-gray-400 mb-1">Type</label>
                        <select
                            id="new-perk-type"
                            value={type}
                            onChange={(e) => setType(e.target.value as ArenaPerkType)}
                            className="w-full px-2 py-2 text-sm bg-gray-700 border border-gray-600 rounded text-white focus:outline-none"
                        >
                            {PERK_TYPES.map((t) => <option key={t} value={t}>{t}</option>)}
                        </select>
                    </div>
                    <div>
                        <label className="block text-xs text-gray-400 mb-1">Vendor</label>
                        <select
                            id="new-perk-vendor"
                            value={soldBy}
                            onChange={(e) => setSoldBy(e.target.value as VendorType)}
                            className="w-full px-2 py-2 text-sm bg-gray-700 border border-gray-600 rounded text-white focus:outline-none"
                        >
                            {VENDOR_TYPES.map((v) => <option key={v} value={v}>{v}</option>)}
                        </select>
                    </div>
                </div>

                <div className="flex gap-3 pt-2">
                    <button
                        id="new-perk-create-btn"
                        onClick={handleCreate}
                        disabled={!id || !name || !!idError}
                        className="flex-1 py-2 bg-indigo-600 hover:bg-indigo-700 disabled:opacity-40 disabled:cursor-not-allowed text-white text-sm font-medium rounded-lg transition-colors"
                    >
                        Create
                    </button>
                    <button
                        id="new-perk-cancel-btn"
                        onClick={onClose}
                        className="flex-1 py-2 bg-gray-700 hover:bg-gray-600 text-gray-300 text-sm rounded-lg transition-colors"
                    >
                        Cancel
                    </button>
                </div>
            </div>
        </div>
    );
}
