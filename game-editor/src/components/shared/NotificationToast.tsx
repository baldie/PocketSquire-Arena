import { useEffect } from "react";
import { useAppContext } from "../../context/AppContext";
import type { Notification } from "../../types";

const SEVERITY_STYLES: Record<Notification["severity"], string> = {
    error: "bg-red-900/90 border-red-600 text-red-200",
    info: "bg-blue-900/90 border-blue-600 text-blue-200",
    success: "bg-green-900/90 border-green-600 text-green-200",
};

const SEVERITY_ICONS: Record<Notification["severity"], string> = {
    error: "✕",
    info: "ℹ",
    success: "✓",
};

const AUTO_DISMISS_MS = 5000;

function NotificationItem({ notification }: { notification: Notification }) {
    const { dispatch } = useAppContext();

    const dismiss = () => dispatch({ type: "DISMISS_NOTIFICATION", payload: notification.id });

    useEffect(() => {
        // Capture the id directly so the effect doesn't depend on the `dismiss` closure
        const id = notification.id;
        const timer = setTimeout(
            () => dispatch({ type: "DISMISS_NOTIFICATION", payload: id }),
            AUTO_DISMISS_MS
        );
        return () => clearTimeout(timer);
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [notification.id]);

    return (
        <div
            className={`flex items-start gap-3 px-4 py-3 rounded-lg border shadow-xl pointer-events-auto ${SEVERITY_STYLES[notification.severity]}`}
            role="alert"
        >
            <span className="text-sm font-bold mt-0.5">{SEVERITY_ICONS[notification.severity]}</span>
            <p className="flex-1 text-sm leading-snug">{notification.message}</p>
            <button
                onClick={dismiss}
                aria-label="Dismiss notification"
                className="opacity-60 hover:opacity-100 transition-opacity text-sm ml-2 mt-0.5"
            >
                ×
            </button>
        </div>
    );
}

export default function NotificationToast() {
    const { state } = useAppContext();

    if (state.notifications.length === 0) return null;

    return (
        <div
            className="fixed top-4 right-4 z-50 w-80 flex flex-col gap-2 pointer-events-none"
            aria-live="assertive"
        >
            {state.notifications.map((n: Notification) => (
                <NotificationItem key={n.id} notification={n} />
            ))}
        </div>
    );
}
