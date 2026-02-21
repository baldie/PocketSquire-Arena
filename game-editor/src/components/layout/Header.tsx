import { useState } from "react";
import { useAppContext } from "../../context/AppContext";
import SaveIndicator from "./SaveIndicator";
import { useFileSystem } from "../../hooks/useFileSystem";
import logoUrl from "../../../../Assets/_Game/Art/UI/pocket_squire_title.png";

export default function Header() {
    const { state } = useAppContext();
    const { selectDirectory } = useFileSystem();
    const [showSettings, setShowSettings] = useState(false);
    const [apiKey, setApiKey] = useState(() => localStorage.getItem("gemini_api_key") ?? "");

    const handleApiKeyChange = (value: string) => {
        setApiKey(value);
        localStorage.setItem("gemini_api_key", value);
    };

    return (
        <>
            <header className="flex items-center justify-between px-6 py-3 bg-gray-900 border-b border-gray-700 shadow-lg">
                <img
                    src={logoUrl}
                    alt="Pocket Squire"
                    className="h-10 w-auto object-contain"
                />
                <div className="flex items-center gap-4">
                    <SaveIndicator status={state.saveStatus} />
                    <button
                        id="settings-btn"
                        onClick={() => setShowSettings(true)}
                        className="p-2 text-gray-400 hover:text-white hover:bg-gray-700 rounded-lg transition-colors"
                        title="Settings"
                        aria-label="Open Settings"
                    >
                        ⚙️
                    </button>
                </div>
            </header>

            {showSettings && (
                <div
                    className="fixed inset-0 bg-black/70 flex items-center justify-center z-50"
                    role="dialog"
                    aria-modal="true"
                    aria-label="Settings"
                >
                    <div className="bg-gray-800 rounded-xl shadow-2xl p-6 w-full max-w-md border border-gray-600">
                        <div className="flex items-center justify-between mb-6">
                            <h2 className="text-lg font-semibold text-white">Settings</h2>
                            <button
                                onClick={() => setShowSettings(false)}
                                className="text-gray-400 hover:text-white text-xl leading-none"
                                aria-label="Close settings"
                            >
                                ✕
                            </button>
                        </div>

                        <div className="space-y-5">
                            <div>
                                <label className="block text-sm font-medium text-gray-300 mb-1">
                                    Gemini API Key
                                </label>
                                <input
                                    type="password"
                                    id="gemini-api-key"
                                    value={apiKey}
                                    onChange={(e) => handleApiKeyChange(e.target.value)}
                                    placeholder="AIza..."
                                    className="w-full px-3 py-2 bg-gray-700 border border-gray-600 rounded-lg text-white placeholder-gray-500 focus:outline-none focus:ring-2 focus:ring-indigo-500"
                                />
                                <p className="text-xs text-gray-500 mt-1">
                                    Stored in localStorage. Used for Imagen 3 image generation.
                                </p>
                            </div>

                            <div>
                                <label className="block text-sm font-medium text-gray-300 mb-1">
                                    Game Data Directory
                                </label>
                                {state.dirHandle ? (
                                    <p className="text-sm text-green-400 mb-2">✓ Directory selected</p>
                                ) : (
                                    <p className="text-sm text-yellow-400 mb-2">No directory selected</p>
                                )}
                                <button
                                    id="change-dir-btn"
                                    onClick={() => { void selectDirectory(); setShowSettings(false); }}
                                    className="w-full px-4 py-2 bg-indigo-600 hover:bg-indigo-700 text-white rounded-lg transition-colors font-medium"
                                >
                                    Change Directory
                                </button>
                            </div>
                        </div>
                    </div>
                </div>
            )}
        </>
    );
}
