import { useState, useRef } from "react";

interface PromptPanelProps {
    globalTemplate: string;
    localOverride: string;
    resolvedPrompt: string;
    unresolvedVars: string[];
    onOverrideChange: (value: string) => void;
    onGenerate: (referenceDataUrl?: string) => void;
    onGenerateAll: () => void;
    isGenerating: boolean;
}

export default function PromptPanel({
    globalTemplate,
    localOverride,
    resolvedPrompt,
    unresolvedVars,
    onOverrideChange,
    onGenerate,
    onGenerateAll,
    isGenerating,
}: PromptPanelProps) {
    const [referenceImage, setReferenceImage] = useState<string | null>(null);
    const fileInputRef = useRef<HTMLInputElement>(null);

    const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        const file = e.target.files?.[0];
        if (!file) return;
        const reader = new FileReader();
        reader.onloadend = () => {
            setReferenceImage(reader.result as string);
        };
        reader.readAsDataURL(file);
    };

    const hasUnresolved = unresolvedVars.length > 0;
    const disabled = isGenerating || hasUnresolved;
    const apiKey = localStorage.getItem("gemini_api_key");
    const noApiKey = !apiKey;

    // Highlight unresolved vars in the resolved prompt
    const highlightedResolved = hasUnresolved
        ? resolvedPrompt.replace(/\{(\w+)\}/g, (match) => `【${match}】`)
        : resolvedPrompt;

    return (
        <div className="space-y-3 bg-gray-800 rounded-lg p-4 border border-gray-700">
            <h3 className="text-sm font-semibold text-gray-300">Prompt Panel</h3>

            <div>
                <label className="block text-xs text-gray-500 mb-1">Global Template (read-only)</label>
                <p className="text-xs text-gray-400 bg-gray-900 rounded p-2 leading-relaxed break-words">
                    {globalTemplate}
                </p>
            </div>

            <div>
                <label className="block text-xs text-gray-500 mb-1">
                    Local Override (empty = use global)
                </label>
                <textarea
                    id="prompt-override"
                    value={localOverride}
                    onChange={(e) => onOverrideChange(e.target.value)}
                    onBlur={(e) => onOverrideChange(e.target.value)}
                    rows={3}
                    placeholder="Leave blank to use the global template..."
                    className="w-full px-3 py-2 text-xs bg-gray-900 border border-gray-600 rounded text-gray-200 placeholder-gray-600 focus:outline-none focus:ring-1 focus:ring-indigo-500 resize-none"
                />
            </div>

            <div>
                <label className="block text-xs text-gray-500 mb-1">Resolved Prompt</label>
                <p
                    className={`text-xs rounded p-2 leading-relaxed break-words ${hasUnresolved ? "bg-red-950 text-red-300" : "bg-gray-900 text-gray-300"
                        }`}
                >
                    {highlightedResolved}
                </p>
                {hasUnresolved && (
                    <p className="text-xs text-red-400 mt-1">
                        Unresolved: {unresolvedVars.map((v) => <strong key={v}>{`{${v}}`}</strong>).reduce<React.ReactNode[]>((acc, el, i) => i === 0 ? [el] : [...acc, ", ", el], [])}
                    </p>
                )}
            </div>

            {noApiKey && (
                <p className="text-xs text-yellow-400">⚠️ Set your Gemini API key in Settings to generate images.</p>
            )}

            <div className="flex gap-2">
                <input
                    type="file"
                    accept="image/*"
                    className="hidden"
                    ref={fileInputRef}
                    onChange={handleFileChange}
                />
                <button
                    onClick={() => fileInputRef.current?.click()}
                    disabled={disabled || noApiKey}
                    className="px-3 py-2 text-sm bg-gray-700 hover:bg-gray-600 disabled:opacity-40 disabled:cursor-not-allowed text-gray-200 rounded-lg transition-colors flex items-center justify-center relative min-w-[3rem]"
                    title="Upload Reference Image"
                >
                    {referenceImage ? (
                        <div className="relative w-6 h-6">
                            <img src={referenceImage} alt="Reference" className="w-full h-full object-cover rounded border border-gray-500" />
                            <div
                                onClick={(e) => {
                                    e.stopPropagation();
                                    setReferenceImage(null);
                                    if (fileInputRef.current) fileInputRef.current.value = "";
                                }}
                                className="absolute -top-2 -right-2 bg-red-500 hover:bg-red-400 text-white rounded-full w-4 h-4 flex items-center justify-center text-[10px] cursor-pointer"
                            >
                                ×
                            </div>
                        </div>
                    ) : (
                        "🖼️"
                    )}
                </button>
                <button
                    id="generate-btn"
                    onClick={() => onGenerate(referenceImage || undefined)}
                    disabled={disabled || noApiKey}
                    className="flex-1 px-3 py-2 text-sm font-medium bg-indigo-600 hover:bg-indigo-700 disabled:opacity-40 disabled:cursor-not-allowed text-white rounded-lg transition-colors"
                >
                    {isGenerating ? "Generating..." : "Generate"}
                </button>
                <button
                    id="generate-all-btn"
                    onClick={onGenerateAll}
                    disabled={disabled || noApiKey}
                    className="px-3 py-2 text-sm font-medium bg-purple-600 hover:bg-purple-700 disabled:opacity-40 disabled:cursor-not-allowed text-white rounded-lg transition-colors"
                >
                    Generate All
                </button>
            </div>
        </div>
    );
}
