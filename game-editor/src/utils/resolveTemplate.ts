import type { ImageSlot, PromptTemplates, ResolvedTemplate, TemplateVariables } from "../types";

export function resolveTemplate(template: string, variables: TemplateVariables): ResolvedTemplate {
    const unresolvedVars: string[] = [];
    const varKeys = Object.keys(variables) as Array<keyof TemplateVariables>;

    const resolved = template.replace(/\{(\w+)\}/g, (_match, key: string) => {
        const typedKey = key as keyof TemplateVariables;
        if (varKeys.includes(typedKey) && variables[typedKey] !== undefined) {
            return variables[typedKey] as string;
        }
        if (!unresolvedVars.includes(key)) {
            unresolvedVars.push(key);
        }
        return `{${key}}`;
    });

    return { resolved, unresolvedVars };
}

export function getEffectiveTemplate(
    templates: PromptTemplates,
    entityType: "player" | "monster" | "item",
    entityKey: string,
    slot: ImageSlot
): string {
    // Access overrides safely via index signature
    const entityTemplates = templates[entityType];
    const overrides = entityTemplates.overrides[entityKey];
    const slotStr = slot as string;
    const override: string | undefined = overrides ? (overrides as Record<string, string | undefined>)[slotStr] : undefined;
    if (override && override.trim() !== "") return override;
    return (entityTemplates.global as Record<string, string>)[slotStr] ?? "";
}
