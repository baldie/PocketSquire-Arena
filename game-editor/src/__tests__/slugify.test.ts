import { describe, it, expect } from "vitest";
import { slugify } from "../utils/slugify";

describe("slugify", () => {
    it("lowercases input", () => expect(slugify("Warrior")).toBe("warrior"));
    it("replaces spaces with underscores", () => expect(slugify("Mushroom Marauder")).toBe("mushroom_marauder"));
    it("strips special characters", () => expect(slugify("Hero-King!")).toBe("heroking"));
    it("handles multiple spaces", () => expect(slugify("A  B")).toBe("a__b"));
    it("handles already lowercase input", () => expect(slugify("spellcaster")).toBe("spellcaster"));
});
