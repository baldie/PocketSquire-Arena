import { describe, it, expect } from "vitest";
import { resolveTemplate } from "../utils/resolveTemplate";

describe("resolveTemplate", () => {
    it("replaces all known variables", () => {
        const result = resolveTemplate("A {class} {gender} attacking", { class: "warrior", gender: "male" });
        expect(result.resolved).toBe("A warrior male attacking");
        expect(result.unresolvedVars).toHaveLength(0);
    });

    it("reports unresolved variables", () => {
        const result = resolveTemplate("A {class} {gender} with {weapon}", { class: "mage" });
        expect(result.unresolvedVars).toContain("gender");
        expect(result.unresolvedVars).toContain("weapon");
    });

    it("returns empty unresolvedVars when template has no variables", () => {
        const result = resolveTemplate("A simple prompt", {});
        expect(result.unresolvedVars).toHaveLength(0);
    });

    it("handles duplicate variables in a single template", () => {
        const result = resolveTemplate("{class} vs {class}", { class: "knight" });
        expect(result.resolved).toBe("knight vs knight");
    });
});
