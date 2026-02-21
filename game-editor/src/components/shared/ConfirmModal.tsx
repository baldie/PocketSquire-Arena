interface ConfirmModalProps {
    isOpen: boolean;
    message: string;
    onConfirm: () => void;
    onCancel: () => void;
}

export default function ConfirmModal({ isOpen, message, onConfirm, onCancel }: ConfirmModalProps) {
    if (!isOpen) return null;

    return (
        <div
            className="fixed inset-0 bg-black/70 flex items-center justify-center z-50"
            role="dialog"
            aria-modal="true"
        >
            <div className="bg-gray-800 rounded-xl shadow-2xl p-6 w-full max-w-sm border border-gray-600">
                <p className="text-white mb-6 text-center">{message}</p>
                <div className="flex gap-3">
                    <button
                        id="confirm-cancel-btn"
                        onClick={onCancel}
                        className="flex-1 px-4 py-2 bg-gray-700 hover:bg-gray-600 text-white rounded-lg transition-colors"
                    >
                        Cancel
                    </button>
                    <button
                        id="confirm-ok-btn"
                        onClick={onConfirm}
                        className="flex-1 px-4 py-2 bg-red-600 hover:bg-red-700 text-white rounded-lg transition-colors font-medium"
                    >
                        Confirm
                    </button>
                </div>
            </div>
        </div>
    );
}
