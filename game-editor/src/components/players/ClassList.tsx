import { useEffect, useState } from "react";
import { useAppContext } from "../../context/AppContext";
import { PLAYER_CLASSES, PLAYER_SLOTS } from "../../constants";
import type { PlayerClassName } from "../../constants";
import { readImageAsDataUrl } from "../../utils/fileSystem";


function useClassCompletionCheck(
    dirHandle: FileSystemDirectoryHandle | null,
    saveStatus: string,
    activeTab: string
) {
    const [missingMap, setMissingMap] = useState<Record<string, number>>({});

    useEffect(() => {
        if (!dirHandle || saveStatus === "saving") return;
        let cancelled = false;

        async function check() {
            const map: Record<string, number> = {};
            for (const cls of PLAYER_CLASSES) {
                const clsLower = cls.toLowerCase();
                let missingCount = 0;
                const missingList: string[] = [];
                for (const gender of ["m", "f"]) {
                    for (const slot of PLAYER_SLOTS) {
                        const path = ["Art", "Player", `${gender}_${clsLower}_${slot}.png`];
                        const result = await readImageAsDataUrl(dirHandle!, path);
                        if (result === null) {
                            missingCount++;
                            missingList.push(`${gender}_${slot}`);
                        }
                    }
                }
                map[cls] = missingCount;
                if (missingCount > 0) {
                    console.warn(`[ClassList] Missing ${missingCount} images for ${cls}:`, missingList);
                }
            }
            if (!cancelled) setMissingMap(map);
        }

        void check();
        return () => { cancelled = true; };
    }, [dirHandle, saveStatus, activeTab]);

    return missingMap;
}

interface ClassListProps {
    onRefreshCompletion?: () => void;
}

export default function ClassList({ onRefreshCompletion: _ }: ClassListProps) {
    const { state, dispatch } = useAppContext();
    const missingMap = useClassCompletionCheck(state.dirHandle, state.saveStatus, state.activeTab);

    return (
        <div className="bg-gray-800 rounded-lg border border-gray-700 overflow-hidden">
            <div className="px-4 py-3 border-b border-gray-700">
                <h2 className="text-sm font-semibold text-gray-300">Classes ({PLAYER_CLASSES.length})</h2>
            </div>
            <ul role="listbox" aria-label="Player classes">
                {PLAYER_CLASSES.map((cls: PlayerClassName) => {
                    const isActive = state.activePlayerClass === cls;
                    const missingCount = missingMap[cls];
                    const isLoaded = missingCount !== undefined;

                    return (
                        <li key={cls}>
                            <button
                                id={`class-${cls.toLowerCase()}`}
                                onClick={() => dispatch({ type: "SET_ACTIVE_PLAYER_CLASS", payload: cls })}
                                className={`w-full flex items-center justify-between px-4 py-2.5 text-sm transition-colors ${isActive
                                    ? "bg-indigo-900/50 text-indigo-300"
                                    : "hover:bg-gray-700 text-gray-300"
                                    }`}
                                aria-selected={isActive}
                                role="option"
                            >
                                <span>{cls}</span>
                                {!isLoaded ? (
                                    <span className="text-gray-500">·</span>
                                ) : missingCount === 0 ? (
                                    <span className="text-green-500" title="All images complete">✓</span>
                                ) : (
                                    <span className="text-red-500 font-bold" title={`${missingCount} images missing`}>
                                        {missingCount}
                                    </span>
                                )}
                            </button>
                        </li>
                    );
                })}
            </ul>
        </div>
    );
}
