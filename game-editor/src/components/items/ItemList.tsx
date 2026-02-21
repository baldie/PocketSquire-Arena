import { useEffect, useState } from "react";
import { useAppContext } from "../../context/AppContext";
import { readImageAsDataUrl } from "../../utils/fileSystem";
import type { ItemData } from "../../types";

export default function ItemList() {
    const { state, dispatch } = useAppContext();
    const [iconImages, setIconImages] = useState<Record<number, string | null>>({});

    useEffect(() => {
        if (!state.dirHandle) return;
        const dirHandle = state.dirHandle;
        let cancelled = false;

        async function load() {
            const map: Record<number, string | null> = {};
            for (let i = 0; i < state.items.length; i++) {
                map[i] = await readImageAsDataUrl(dirHandle, ["Art", "Items", `${state.items[i].sprite}.png`]);
            }
            if (!cancelled) setIconImages(map);
        }
        void load();
        return () => { cancelled = true; };
    }, [state.items, state.dirHandle]);

    return (
        <div className="bg-gray-800 rounded-lg border border-gray-700 overflow-hidden">
            {state.items.length === 0 ? (
                <p className="px-4 py-6 text-sm text-gray-500 text-center">No items yet. Add one!</p>
            ) : (
                <ul role="listbox" aria-label="Items">
                    {state.items.map((item: ItemData, idx: number) => {
                        const isActive = state.activeItemIndex === idx;
                        const img = iconImages[idx];
                        return (
                            <li key={item.id}>
                                <button
                                    id={`item-${idx}`}
                                    onClick={() => dispatch({ type: "SET_ACTIVE_ITEM", payload: idx })}
                                    className={`w-full flex items-center gap-3 px-4 py-2.5 text-sm transition-colors border-b border-gray-700/50 ${isActive ? "bg-indigo-900/50 text-indigo-300" : "hover:bg-gray-700 text-gray-300"
                                        }`}
                                    role="option"
                                    aria-selected={isActive}
                                >
                                    <div className="w-8 h-8 rounded overflow-hidden bg-gray-700 flex-shrink-0 flex items-center justify-center">
                                        {img ? (
                                            <img src={img} alt="" className="w-full h-full object-contain" />
                                        ) : (
                                            <span className="text-xs text-gray-500">⚗️</span>
                                        )}
                                    </div>
                                    <span className="truncate">{item.name}</span>
                                </button>
                            </li>
                        );
                    })}
                </ul>
            )}
        </div>
    );
}
