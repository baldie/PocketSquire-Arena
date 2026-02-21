import type { ImageSlot } from "../../types";

interface BatchGenerateModalProps {
    isOpen: boolean;
    progress: { current: number; total: number } | null;
    failedSlots: Array<{ entityName: string; slot: ImageSlot; error?: string }>;
    onRetryFailed: (entityName: string, slot: ImageSlot) => void;
    onClose: () => void;
}

export default function BatchGenerateModal({
    isOpen,
    progress,
    failedSlots,
    onRetryFailed,
    onClose,
}: BatchGenerateModalProps) {
    if (!isOpen) return null;

    const isInProgress = progress !== null && progress.current < progress.total;
    const percent = progress ? Math.round((progress.current / progress.total) * 100) : 100;

    return (
        <div
            className="fixed inset-0 bg-black/70 flex items-center justify-center z-50"
            role="dialog"
            aria-modal="true"
            aria-label="Batch generation progress"
        >
            <div className="bg-gray-800 rounded-xl shadow-2xl p-6 w-full max-w-lg border border-gray-600">
                <div className="flex items-center justify-between mb-4">
                    <h2 className="text-lg font-semibold text-white">⚡ Batch Generate</h2>
                    <button
                        onClick={onClose}
                        disabled={isInProgress}
                        className="text-gray-400 hover:text-white disabled:opacity-40 disabled:cursor-not-allowed text-xl"
                        aria-label="Close"
                    >
                        ✕
                    </button>
                </div>

                {progress && (
                    <div className="mb-4">
                        <div className="flex justify-between text-sm text-gray-400 mb-1">
                            <span>{isInProgress ? "Generating..." : "Complete"}</span>
                            <span>{progress.current} / {progress.total}</span>
                        </div>
                        <div className="w-full bg-gray-700 rounded-full h-3">
                            <div
                                className="bg-indigo-500 h-3 rounded-full transition-all duration-300"
                                style={{ width: `${percent}%` }}
                            />
                        </div>
                    </div>
                )}

                {failedSlots.length > 0 && (
                    <div className="mt-4">
                        <h3 className="text-sm font-medium text-red-400 mb-2">
                            Failed ({failedSlots.length})
                        </h3>
                        <div className="space-y-1 max-h-48 overflow-y-auto">
                            {failedSlots.map(({ entityName, slot, error }, i) => (
                                <div
                                    key={i}
                                    className="flex items-center justify-between bg-gray-900 rounded p-2"
                                >
                                    <div>
                                        <span className="text-sm text-gray-300">{entityName} / {slot}</span>
                                        {error && <p className="text-xs text-red-400 mt-0.5">{error}</p>}
                                    </div>
                                    <button
                                        onClick={() => onRetryFailed(entityName, slot)}
                                        disabled={isInProgress}
                                        className="text-xs px-2 py-1 bg-indigo-600 hover:bg-indigo-700 disabled:opacity-40 disabled:cursor-not-allowed text-white rounded transition-colors"
                                    >
                                        Retry
                                    </button>
                                </div>
                            ))}
                        </div>
                    </div>
                )}

                {!isInProgress && (
                    <button
                        onClick={onClose}
                        className="mt-4 w-full px-4 py-2 bg-gray-700 hover:bg-gray-600 text-white rounded-lg transition-colors"
                    >
                        Close
                    </button>
                )}
            </div>
        </div>
    );
}
