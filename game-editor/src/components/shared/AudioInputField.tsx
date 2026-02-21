import { useState, useEffect } from "react";
import AudioButton from "./AudioButton";
import { checkAudioExists } from "../../utils/audioUtils";

interface AudioInputFieldProps {
    id: string;
    defaultValue: string;
    onBlur: (value: string) => void;
    dirHandle: FileSystemDirectoryHandle | null;
    category?: "Player" | "Monsters" | "Items";
}

export default function AudioInputField({ id, defaultValue, onBlur, dirHandle, category }: AudioInputFieldProps) {
    const [exists, setExists] = useState<boolean>(true);
    const [currentValue, setCurrentValue] = useState(defaultValue);

    // Initial check on mount or when defaultValue prop changes (though parent usually uses `key`)
    useEffect(() => {
        setCurrentValue(defaultValue);
    }, [defaultValue]);

    useEffect(() => {
        let mounted = true;
        async function verify() {
            if (!currentValue || !dirHandle) {
                if (mounted) setExists(true);
                return;
            }
            const found = await checkAudioExists(dirHandle, currentValue, category);
            if (mounted) setExists(found);
        }
        verify();
        return () => { mounted = false; };
    }, [currentValue, dirHandle, category]);

    const handleBlur = (e: React.FocusEvent<HTMLInputElement>) => {
        setCurrentValue(e.target.value);
        onBlur(e.target.value);
    };

    return (
        <div className="flex gap-1 items-center w-full">
            <input
                id={id}
                type="text"
                defaultValue={defaultValue}
                onBlur={handleBlur}
                className={`flex-1 min-w-0 px-2 py-1.5 text-sm bg-gray-700 border rounded text-white focus:outline-none focus:ring-1 focus:ring-indigo-500 ${exists ? "border-gray-600" : "border-red-500 focus:border-red-500 focus:ring-red-500"
                    }`}
                title={exists ? undefined : "Audio file not found"}
            />
            {dirHandle && currentValue && exists && (
                <AudioButton soundId={currentValue} dirHandle={dirHandle} category={category} />
            )}
        </div>
    );
}
