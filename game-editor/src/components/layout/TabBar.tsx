import { useAppContext } from "../../context/AppContext";

type TabId = "players" | "monsters" | "items";

export default function TabBar() {
    const { state, dispatch } = useAppContext();

    const tabs: Array<{ id: TabId; label: string; count: number }> = [
        { id: "players", label: "Players", count: 19 }, // Fixed 19 classes
        { id: "monsters", label: "Monsters", count: state.monsters.length },
        { id: "items", label: "Items", count: state.items.length },
    ];

    return (
        <nav className="flex border-b border-gray-700 bg-gray-900" aria-label="Main tabs">
            {tabs.map((tab) => {
                const isActive = state.activeTab === tab.id;
                return (
                    <button
                        key={tab.id}
                        id={`tab-${tab.id}`}
                        onClick={() => dispatch({ type: "SET_ACTIVE_TAB", payload: tab.id })}
                        className={`px-6 py-3 text-sm font-medium transition-colors border-b-2 ${isActive
                                ? "border-indigo-500 text-indigo-400 bg-gray-800"
                                : "border-transparent text-gray-400 hover:text-gray-200 hover:bg-gray-800"
                            }`}
                        aria-selected={isActive}
                        role="tab"
                    >
                        {tab.label} ({tab.count})
                    </button>
                );
            })}
        </nav>
    );
}
