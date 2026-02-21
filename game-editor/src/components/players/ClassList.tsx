import { useEffect, useState } from "react";
import { useAppContext } from "../../context/AppContext";
import { PLAYER_CLASSES, PLAYER_SLOTS } from "../../constants";
import type { PlayerClassName } from "../../constants";
import { readImageAsDataUrl } from "../../utils/fileSystem";


function useClassCompletionCheck(dirHandle: FileSystemDirectoryHandle | null) {
    const [completionMap, setCompletionMap] = useState<Record<string, boolean>>({});

    useEffect(() => {
        if (!dirHandle) return;
        let cancelled = false;

        async function check() {
            const map: Record<string, boolean> = {};
            for (const cls of PLAYER_CLASSES) {
                const clsLower = cls.toLowerCase();
                let allFound = true;
                for (const gender of ["m", "f"]) {
                    for (const slot of PLAYER_SLOTS) {
                        const path = ["Sprites", "Players", `player_${gender}_${clsLower}_${slot}.png`];
                        const result = await readImageAsDataUrl(dirHandle!, path);
                        if (result === null) { allFound = false; break; }
                    }
                    if (!allFound) break;
                }
                map[cls] = allFound;
            }
            if (!cancelled) setCompletionMap(map);
        }

        void check();
        return () => { cancelled = true; };
    }, [dirHandle]);

    return completionMap;
}

interface ClassListProps {
    onRefreshCompletion?: () => void;
}

export default function ClassList({ onRefreshCompletion: _ }: ClassListProps) {
    const { state, dispatch } = useAppContext();
    const completionMap = useClassCompletionCheck(state.dirHandle);

    return (
        <div className="bg-gray-800 rounded-lg border border-gray-700 overflow-hidden">
            <div className="px-4 py-3 border-b border-gray-700">
                <h2 className="text-sm font-semibold text-gray-300">Classes ({PLAYER_CLASSES.length})</h2>
            </div>
            <ul role="listbox" aria-label="Player classes">
                {PLAYER_CLASSES.map((cls: PlayerClassName) => {
                    const isActive = state.activePlayerClass === cls;
                    const isComplete = completionMap[cls];
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
                                <span title={isComplete ? "All images complete" : "Some images missing"}>
                                    {isComplete === true ? "✓" : isComplete === false ? "✗" : "·"}
                                </span>
                            </button>
                        </li>
                    );
                })}
            </ul>
        </div>
    );
}
