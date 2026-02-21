import { useState, useRef } from "react";
import { readAudioAsUrl } from "../../utils/audioUtils";
import { pushNotification } from "../../context/AppContext";
import { useAppContext } from "../../context/AppContext";

// Module-level singleton for the currently playing Audio instance,
// so stopping one clip stops any other that was playing.
let currentAudio: HTMLAudioElement | null = null;
let currentObjectUrl: string | null = null;

function releaseCurrentAudio() {
    if (currentAudio) {
        currentAudio.pause();
        currentAudio = null;
    }
    if (currentObjectUrl) {
        URL.revokeObjectURL(currentObjectUrl);
        currentObjectUrl = null;
    }
}

interface AudioButtonProps {
    soundId: string;
    dirHandle: FileSystemDirectoryHandle | null;
}

export default function AudioButton({ soundId, dirHandle }: AudioButtonProps) {
    const { dispatch } = useAppContext();
    const [isPlaying, setIsPlaying] = useState(false);
    const [isLoading, setIsLoading] = useState(false);
    // Track if THIS button's clip is the active one
    const myAudioRef = useRef<HTMLAudioElement | null>(null);

    // All hooks must be called before any conditional return
    if (!soundId.trim() || !dirHandle) return null;

    const handleClick = async () => {
        // If our clip is currently playing, stop it
        if (isPlaying && myAudioRef.current) {
            releaseCurrentAudio();
            myAudioRef.current = null;
            setIsPlaying(false);
            return;
        }

        // Stop any other clip that was playing
        releaseCurrentAudio();
        setIsPlaying(false);

        setIsLoading(true);
        try {
            const url = await readAudioAsUrl(dirHandle, soundId);
            if (!url) {
                pushNotification(dispatch, `Audio not found: "${soundId}"`, "error");
                setIsLoading(false);
                return;
            }

            const audio = new Audio(url);
            currentAudio = audio;
            currentObjectUrl = url;
            myAudioRef.current = audio;

            audio.onended = () => {
                setIsPlaying(false);
                myAudioRef.current = null;
                releaseCurrentAudio();
            };
            audio.onerror = () => {
                pushNotification(dispatch, `Failed to play audio: "${soundId}"`, "error");
                setIsPlaying(false);
                myAudioRef.current = null;
                releaseCurrentAudio();
            };

            await audio.play();
            setIsPlaying(true);
        } catch (err) {
            pushNotification(dispatch, `Audio error: ${err instanceof Error ? err.message : String(err)}`, "error");
        } finally {
            setIsLoading(false);
        }
    };

    return (
        <button
            type="button"
            onClick={() => void handleClick()}
            disabled={isLoading}
            title={isPlaying ? "Stop" : "Play"}
            aria-label={isPlaying ? "Stop audio" : "Play audio preview"}
            className="flex-shrink-0 w-7 h-7 flex items-center justify-center rounded bg-gray-600 hover:bg-gray-500 disabled:opacity-40 transition-colors text-xs text-white"
        >
            {isLoading ? "…" : isPlaying ? "■" : "▶"}
        </button>
    );
}
