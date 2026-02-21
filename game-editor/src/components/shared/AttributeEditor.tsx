import type { Attributes } from "../../types";

interface AttributeEditorProps {
    attributes: Attributes;
    onChange: (updated: Attributes) => void;
}

const ATTR_KEYS: Array<keyof Attributes> = [
    "Strength", "Constitution", "Magic", "Dexterity", "Luck", "Defense"
];

export default function AttributeEditor({ attributes, onChange }: AttributeEditorProps) {
    const handleBlur = (key: keyof Attributes, value: string) => {
        const num = parseInt(value, 10);
        if (!isNaN(num)) {
            onChange({ ...attributes, [key]: Math.max(1, Math.min(999, num)) });
        }
    };

    return (
        <div>
            <h4 className="text-sm font-medium text-gray-300 mb-2">Attributes</h4>
            <div className="grid grid-cols-3 gap-2">
                {ATTR_KEYS.map((key) => (
                    <div key={key}>
                        <label className="block text-xs text-gray-500 mb-0.5">{key}</label>
                        <input
                            id={`attr-${key.toLowerCase()}`}
                            type="number"
                            min={1}
                            max={999}
                            defaultValue={attributes[key]}
                            onBlur={(e) => handleBlur(key, e.target.value)}
                            className="w-full px-2 py-1.5 text-sm bg-gray-700 border border-gray-600 rounded text-white focus:outline-none focus:ring-1 focus:ring-indigo-500"
                        />
                    </div>
                ))}
            </div>
        </div>
    );
}
