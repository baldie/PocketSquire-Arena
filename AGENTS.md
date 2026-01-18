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
- `Assets/_Game/Scripts/Core/`  -> Pure C# logic (Framework agnostic).
- `Assets/_Game/Scripts/Unity/` -> MonoBehaviours & 
Unity-specific code.
- `Assets/_Game/Art/` -> Art assets.
- `Assets/_Game/Audio/` -> Audio assets.
- `Assets/_Game/Scenes/` -> Scene assets.
- `Assets/_Game/Prefabs/` -> Prefab assets.
- `Assets/_Game/Scripts/Core/`  -> Pure C# logic (Framework agnostic).
- `Assets/_Game/Scripts/Unity/` -> MonoBehaviours & 
Unity-specific code.
- `tests/unit/`           -> NUnit tests for Core logic.
- `tests/`                -> Playwright integration tests.
- `scripts/`              -> Node.js automation scripts.
- `.github/workflows/`    -> CI/CD definitions.

## Landing the Plane (Session Completion)

**When ending a work session**, you MUST complete ALL steps below. Work is NOT complete until `git push` succeeds.

**MANDATORY WORKFLOW:**

1. **File issues for remaining work** - Create issues for anything that needs follow-up
2. **Run quality gates** (if code changed) - Tests, linters, builds
3. **Update issue status** - Close finished work, update in-progress items
4. **PUSH TO REMOTE** - This is MANDATORY:
   ```bash
   git pull --rebase
   bd sync
   git push
   git status  # MUST show "up to date with origin"
   ```
5. **Clean up** - Clear stashes, prune remote branches
6. **Verify** - All changes committed AND pushed
7. **Hand off** - Provide context for next session

**CRITICAL RULES:**
- Work is NOT complete until `git push` succeeds
- NEVER stop before pushing - that leaves work stranded locally
- NEVER say "ready to push when you are" - YOU must push
- If push fails, resolve and retry until it succeeds
