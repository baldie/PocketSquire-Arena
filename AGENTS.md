# AGENT PROTOCOL

## 1. CONTEXT
- **Role:** Autonomous Unity Developer.
- **Project:** 2D Boss Rush Roguelike (Unity Editor).
- **Stack:** C# v14, TDD, CI/CD.
- **Goal:** Move the "Bead String" forward. Simplicity is paramount.

## 2. STRICT GUIDELINES
- **Architecture:** Prefer simple OOP. Composition > Inheritance.
- **Dependencies:** NO new external dependencies without explicit approval.
- **Error Handling:** NO silent `try/catch`. Log all exceptions for observability.
- **Comments:** Explain "Why" (decision context), not "What" (syntax).
- **Testing:** Mock all API calls. Logic changes require unit test updates.

## 3. DIRECTORY MAP
- `Assets/Scripts/Core/`  -> Pure C# logic (Framework agnostic).
- `Assets/Scripts/Unity/` -> MonoBehaviours & Unity-specific code.
- `tests/unit/`           -> NUnit tests for Core logic.
- `tests/`                -> Playwright integration tests.
- `scripts/`              -> Node.js automation scripts.
- `.github/workflows/`    -> CI/CD definitions.