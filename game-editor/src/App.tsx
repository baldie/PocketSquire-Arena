
import { AppProvider } from "./context/AppContext";
import { useAppContext } from "./context/AppContext";
import Header from "./components/layout/Header";
import TabBar from "./components/layout/TabBar";
import PlayersTab from "./components/players/PlayersTab";
import MonstersTab from "./components/monsters/MonstersTab";
import ItemsTab from "./components/items/ItemsTab";
import NotificationToast from "./components/shared/NotificationToast";
import { useFileSystem } from "./hooks/useFileSystem";

function DirectoryPrompt() {
  const { selectDirectory } = useFileSystem();
  return (
    <div className="fixed inset-0 bg-gray-950 flex items-center justify-center z-40">
      <div className="text-center max-w-md p-8 space-y-6">
        <div className="text-5xl mb-4">🎮</div>
        <h2 className="text-2xl font-bold text-white">Game Editor</h2>
        <p className="text-gray-400">
          Select your <code className="text-indigo-400">Assets/_Game/</code> directory to begin.
          This is where your game data (JSON) and sprites live.
        </p>
        <button
          id="select-dir-btn"
          onClick={() => void selectDirectory()}
          className="px-6 py-3 bg-indigo-600 hover:bg-indigo-700 text-white font-semibold rounded-xl transition-colors text-lg"
        >
          📁 Select Game Directory
        </button>
        <p className="text-xs text-gray-600">
          Requires Chrome 86+ (File System Access API)
        </p>
      </div>
    </div>
  );
}

function AppContent() {
  const { state } = useAppContext();

  if (!state.dirHandle) {
    return <DirectoryPrompt />;
  }

  return (
    <div className="flex flex-col h-screen bg-gray-900 text-gray-100">
      <NotificationToast />
      <Header />
      <TabBar />
      <main className="flex-1 overflow-hidden">
        {state.activeTab === "players" && <PlayersTab />}
        {state.activeTab === "monsters" && <MonstersTab />}
        {state.activeTab === "items" && <ItemsTab />}
      </main>
    </div>
  );
}

export default function App() {
  return (
    <AppProvider>
      <AppContent />
    </AppProvider>
  );
}
