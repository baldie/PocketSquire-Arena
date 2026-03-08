import PerkList from "./PerkList";
import PerkDetail from "./PerkDetail";

export default function PerksTab() {
    return (
        <div className="flex h-full overflow-hidden">
            {/* Left panel: fixed-width scrollable list */}
            <div className="w-64 flex-shrink-0 overflow-hidden flex flex-col border-r border-gray-700">
                <PerkList />
            </div>

            {/* Right panel: detail view */}
            <div className="flex-1 overflow-hidden">
                <PerkDetail />
            </div>
        </div>
    );
}
