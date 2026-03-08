import { useState, useEffect } from "react";
import { useAppContext } from "../../context/AppContext";
import { readImageAsDataUrl } from "../../utils/fileSystem";
import type { ArenaPerkData, VendorType, GenerationHistoryEntry } from "../../types";
import NewPerkModal from "./NewPerkModal";

const VENDOR_COLORS: Record<VendorType, string> = {
    Shopkeeper: "bg-amber-600",
    Wizard: "bg-purple-600",
    FightersBlacksmith: "bg-red-700",
    ArcheryTrainer: "bg-green-700",
};

const VENDOR_SHORT: Record<VendorType, string> = {
    Shopkeeper: "Shop",
    Wizard: "Wiz",
    FightersBlacksmith: "Smith",
    ArcheryTrainer: "Archer",
};

// Loads and displays the on-disk icon for a perk row, falling back to the sparkle emoji.
function PerkIcon({ perk, dirHandle, generationHistory }: { perk: ArenaPerkData; dirHandle: FileSystemDirectoryHandle | null; generationHistory: GenerationHistoryEntry[] }) {
    const [imageUrl, setImageUrl] = useState<string | null>(null);

    useEffect(() => {
        if (!perk.icon || !dirHandle) {
            setImageUrl(null);
            return;
        }
        let cancelled = false;
        readImageAsDataUrl(dirHandle, ["Art", "Perks", `${perk.id}.png`]).then((url) => {
            if (!cancelled) setImageUrl(url);
        }).catch(() => {
            if (!cancelled) setImageUrl(null);
        });
        return () => { cancelled = true; };
    }, [perk.id, perk.icon, dirHandle]);

    // Live update when an image is generated or uploaded
    useEffect(() => {
        if (generationHistory.length === 0) return;
        const lastEntry = generationHistory[generationHistory.length - 1];
        if (lastEntry.entityKey === perk.id && lastEntry.slot === "icon") {
            setImageUrl(lastEntry.imageDataUrl);
        }
    }, [generationHistory, perk.id]);

    if (imageUrl) {
        return <img src={imageUrl} alt={perk.name} className="w-4 h-4 object-contain flex-shrink-0 rounded" />;
    }
    return <span className="w-4 h-4 flex-shrink-0 text-xs flex items-center justify-center text-gray-500">✨</span>;
}

export default function PerkList() {
    const { state, dispatch } = useAppContext();
    const [search, setSearch] = useState("");
    const [groupByVendor, setGroupByVendor] = useState(false);
    const [showModal, setShowModal] = useState(false);

    const filtered = state.arenaPerks.filter((p) => {
        const q = search.toLowerCase();
        return p.name.toLowerCase().includes(q) || p.id.toLowerCase().includes(q);
    });

    const sorted = [...filtered].sort((a, b) => a.name.localeCompare(b.name));

    const renderPerkRow = (perk: ArenaPerkData) => {
        const isActive = state.activePerkId === perk.id;
        return (
            <li key={perk.id}>
                <button
                    id={`perk-${perk.id}`}
                    onClick={() => dispatch({ type: "SET_ACTIVE_PERK", payload: perk.id })}
                    className={`w-full flex items-center gap-2 px-3 py-2 text-sm transition-colors border-b border-gray-700/50 ${
                        isActive
                            ? "bg-indigo-900/50 text-indigo-300"
                            : "hover:bg-gray-700 text-gray-300"
                    }`}
                    role="option"
                    aria-selected={isActive}
                    title={perk.id}
                >
                    {/* Icon: real image if available, sparkle fallback */}
                    <PerkIcon perk={perk} dirHandle={state.dirHandle} generationHistory={state.generationHistory} />

                    <span className="flex-1 truncate font-medium text-left">{perk.name}</span>

                    {/* Vendor badge */}
                    <span
                        className={`flex-shrink-0 px-1.5 py-0.5 rounded text-[10px] font-semibold text-white ${VENDOR_COLORS[perk.soldBy] ?? "bg-gray-600"}`}
                        title={perk.soldBy}
                    >
                    {VENDOR_SHORT[perk.soldBy]}
                    </span>

                    {/* Type badge */}
                    <span
                        className={`flex-shrink-0 w-4 h-4 rounded text-[9px] font-bold flex items-center justify-center ${
                            perk.type === "Triggered" ? "bg-indigo-700 text-indigo-200" : "bg-gray-600 text-gray-300"
                        }`}
                        title={perk.type}
                    >
                        {perk.type === "Triggered" ? "T" : "P"}
                    </span>
                </button>
            </li>
        );
    };

    const renderGrouped = () => {
        const vendors: VendorType[] = ["Shopkeeper", "Wizard", "FightersBlacksmith", "ArcheryTrainer"];
        return vendors.map((vendor) => {
            const group = sorted.filter((p) => p.soldBy === vendor);
            if (group.length === 0) return null;
            return (
                <div key={vendor}>
                    <div className="px-3 py-1 text-xs font-semibold text-gray-500 uppercase tracking-wider sticky top-0 bg-gray-800 border-b border-gray-700">
                        {vendor}
                    </div>
                    <ul role="listbox" aria-label={vendor}>
                        {group.map(renderPerkRow)}
                    </ul>
                </div>
            );
        });
    };

    return (
        <div className="flex flex-col h-full bg-gray-800 border-r border-gray-700">
            {/* Search */}
            <div className="p-2 border-b border-gray-700 space-y-1">
                <input
                    id="perk-search"
                    type="search"
                    value={search}
                    onChange={(e) => setSearch(e.target.value)}
                    placeholder="Search perks…"
                    className="w-full px-2 py-1.5 text-sm bg-gray-700 border border-gray-600 rounded text-white placeholder-gray-500 focus:outline-none focus:ring-1 focus:ring-indigo-500"
                />
                <label className="flex items-center gap-2 text-xs text-gray-400 cursor-pointer select-none">
                    <input
                        id="perk-group-by-vendor"
                        type="checkbox"
                        checked={groupByVendor}
                        onChange={(e) => setGroupByVendor(e.target.checked)}
                        className="w-3.5 h-3.5 rounded border-gray-600 bg-gray-700 text-indigo-600"
                    />
                    Group by vendor
                </label>
            </div>

            {/* List */}
            <div className="flex-1 overflow-y-auto">
                {state.arenaPerks.length === 0 ? (
                    <p className="px-4 py-6 text-sm text-gray-500 text-center">
                        No perks yet. Click + New Perk to get started.
                    </p>
                ) : sorted.length === 0 ? (
                    <p className="px-4 py-6 text-sm text-gray-500 text-center">No results.</p>
                ) : groupByVendor ? (
                    renderGrouped()
                ) : (
                    <ul role="listbox" aria-label="Perks">
                        {sorted.map(renderPerkRow)}
                    </ul>
                )}
            </div>

            {/* Add button */}
            <div className="p-2 border-t border-gray-700">
                <button
                    id="add-perk-btn"
                    onClick={() => setShowModal(true)}
                    disabled={!state.dirHandle}
                    className="w-full py-1.5 text-sm bg-indigo-700 hover:bg-indigo-600 disabled:opacity-40 disabled:cursor-not-allowed text-white font-medium rounded-lg transition-colors"
                >
                    + New Perk
                </button>
            </div>

            {showModal && <NewPerkModal onClose={() => setShowModal(false)} />}
        </div>
    );
}
