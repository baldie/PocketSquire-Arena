## Workflow Orchestration

### 1. Plan Mode Default
- Use plan mode for ANY non-trivial task (3+ steps) or architectural decisions, writing detailed specs upfront to reduce ambiguity.
- If something goes sideways or you find yourself in a loop, STOP and re-plan immediately – don't keep pushing.
- Apply plan mode to both implementation and verification phases.
- When running tests, use "npm run test:unit":
- To test the build, use "npm run test:build"

### 2. Subagent Strategy
- Use subagents liberally to offload research and analysis, keeping the main context window clean.
- For complex problems, increase compute power by deploying subagents.
- Assign one task per subagent for focused and efficient execution.

### 3. Self-Improvement Loop
- Immediately after any user correction, update `tasks/lessons.md` with the pattern and a rule to prevent recurrence.
- Ruthlessly iterate on lessons until the mistake rate drops.
- Review lessons at the start of each session for project-relevant context.

### 4. Verification Before Done
- Prove correctness before completion by running tests, checking logs, and demonstrating results.
- Diff behavior between main and changes when relevant to ensure quality.
- Review your work through the lens of: "Would a staff engineer approve this?"

### 5. Demand Elegance (Balanced)
- For non-trivial changes, pause to challenge your work and seek the most elegant solution.
- Replace hacky fixes with elegant implementations based on current system knowledge.
- Balance elegance with pragmatism: avoid over-engineering simple, obvious fixes.

### 6. Autonomous Bug Fixing
- Fix bug reports and failing CI tests autonomously without hand-holding or user context switching.
- Point at logs, errors, or failing tests, then resolve them directly.
- Take full ownership of the technical resolution to minimize user friction.

## 7. Task Management

1. **Plan First**: Write plan to `tasks/todo.md` with checkable items
2. **Verify Plan**: Check in before starting implementation
3. **Track Progress**: Mark items complete as you go
4. **Explain Changes**: High-level summary at each step
5. **Document Results**: Add review section to `tasks/todo.md`
6. **Capture Lessons**: Update `tasks/lessons.md` after corrections
7. **Remember lessons**: Read `tasks/lessons.md` at the start of each session

## 8. Core Principles

- **Simplicity First**: Make every change as simple as possible. Impact minimal code.
- **No Laziness**: Find root causes. No temporary fixes. Senior developer standards.
- **Minimal Impact**: Changes should only touch what's necessary. Avoid introducing bugs.
- **Campsite Rule**: If you make temporary scripts, clean them up before you leave.

## 9. Environment
- **Terminal** Powershell, so do not use "&&", use ";" instead.

# AGENT PROTOCOL

## 1. CONTEXT
- **Role:** Autonomous Unity Developer.
- **Project:** 2D Boss Rush Roguelite (Unity Editor) with RPG elements.
- **Stack:** C# v14, TDD, CI/CD.
- **Goal:** Move the "Bead String" forward. Simplicity is paramount.

## 2. STRICT GUIDELINES
- **Architecture:** Prefer simple OOP. Composition > Inheritance.
- **Dependencies:** NO new external dependencies without explicit approval.
- **Error Handling:** NO silent `try/catch`. Log all exceptions for observability.
- **Comments:** Explain "Why" (decision context), not "What" (syntax).
- **Testing:** Mock all API calls. Logic changes require unit test updates for POCO classes

## 3. DIRECTORY MAP
- `Assets/_Game/Scripts/Core/`  -> Pure C# logic (Framework agnostic).
- `Assets/_Game/Scripts/Unity/` -> MonoBehaviours & Unity-specific code.
- `Assets/_Game/Art/` -> Art assets.
- `Assets/_Game/Audio/` -> Audio assets.
- `Assets/_Game/Scenes/` -> Scene assets.
- `Assets/_Game/Prefabs/` -> Prefab assets.
- `tests/unit/`           -> NUnit tests for Core logic.
- `tests/`                -> Playwright integration tests.
- `scripts/`              -> Node.js automation scripts.
- `.github/workflows/`    -> CI/CD definitions.
- `game-editor/`          -> Web-based game editor.