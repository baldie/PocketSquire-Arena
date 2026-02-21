import { useEffect, useState } from "react";
import { useAppContext } from "../../context/AppContext";
import { readImageAsDataUrl } from "../../utils/fileSystem";
import { slugify } from "../../utils/slugify";

export default function MonsterList() {
    const { state, dispatch } = useAppContext();
    const [battleImages, setBattleImages] = useState<Record<number, string | null>>({});

    useEffect(() => {
        if (!state.dirHandle) return;
        const dirHandle = state.dirHandle;
        let cancelled = false;

        async function load() {
            const map: Record<number, string | null> = {};
            for (let i = 0; i < state.monsters.length; i++) {
                const slug = slugify(state.monsters[i].name);
                map[i] = await readImageAsDataUrl(dirHandle, ["Art", "Monsters", `${slug}_battle.png`]);
            }
            if (!cancelled) setBattleImages(map);
        }
        void load();
        return () => { cancelled = true; };
    }, [state.monsters, state.dirHandle]);

    // Group by rank
    const byRank = state.monsters.reduce<Record<number, number[]>>((acc, m, i) => {
        const r = m.rank;
        if (!acc[r]) acc[r] = [];
        acc[r].push(i);
        return acc;
    }, {});
    const ranks = Object.keys(byRank)
        .map(Number)
        .sort((a, b) => a - b);

    const [collapsed, setCollapsed] = useState<Record<number, boolean>>({});

    return (
        <div className="bg-gray-800 rounded-lg border border-gray-700 overflow-hidden">
            {state.monsters.length === 0 ? (
                <p className="px-4 py-6 text-sm text-gray-500 text-center">No monsters yet. Add one!</p>
            ) : (
                <ul role="listbox" aria-label="Monsters">
                    {ranks.map((rank) => (
                        <li key={rank}>
                            <button
                                className="w-full flex items-center justify-between px-4 py-2.5 text-xs font-semibold text-gray-400 uppercase tracking-wide hover:bg-gray-700 transition-colors border-b border-gray-700"
                                onClick={() => setCollapsed((prev) => ({ ...prev, [rank]: !prev[rank] }))}
                            >
                                <span>Rank {rank} ({byRank[rank].length})</span>
                                <span>{collapsed[rank] ? "▶" : "▼"}</span>
                            </button>
                            {!collapsed[rank] && byRank[rank].map((idx) => {
                                const monster = state.monsters[idx];
                                const isActive = state.activeMonsterIndex === idx;
                                const img = battleImages[idx];
                                return (
                                    <button
                                        key={idx}
                                        id={`monster-${idx}`}
                                        onClick={() => dispatch({ type: "SET_ACTIVE_MONSTER", payload: idx })}
                                        className={`w-full flex items-center gap-3 px-4 py-2.5 text-sm transition-colors border-b border-gray-700/50 ${isActive ? "bg-indigo-900/50 text-indigo-300" : "hover:bg-gray-700 text-gray-300"
                                            }`}
                                        role="option"
                                        aria-selected={isActive}
                                    >
                                        <div className="w-8 h-8 rounded overflow-hidden bg-gray-700 flex-shrink-0 flex items-center justify-center">
                                            {img ? (
                                                <img src={img} alt="" className="w-full h-full object-contain" />
                                            ) : (
                                                <span className="text-xs text-gray-500">🐉</span>
                                            )}
                                        </div>
                                        <span className="truncate">{monster.name}</span>
                                    </button>
                                );
                            })}
                        </li>
                    ))}
                </ul>
            )}
        </div>
    );
}
