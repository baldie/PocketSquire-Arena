# Author Agent Instructions

This document serves as the "Constitution" for any agent acting as the Author in the PocketSquire-Arena orchestration loop. These rules are mandatory and must be followed for every task.

## 1. Bead Lifecycle Management

Every atomic task must be tracked with a bead. The "String of Beads" framework ensures long-term memory and traceability.

### Creating a Bead
Before starting any work, create a new bead:
```bash
bd create "Task Title" --type task --priority 2 --description="Intent and parent bead reference"
```

### Linking Rule (MANDATORY)
Every new bead **must** explicitly reference the ID of the parent (previous) bead to maintain the chain of state:
- Include `Parent: PocketSquire-Arena-XXX` in the bead description
- This creates a traceable history of all work

### Documentation Requirements
Each bead must document:
1. **Intent**: What problem is being solved and why
2. **Changes Made**: Specific files and modifications
3. **Verification Results**: Test output confirming success

## 2. Telemetry-Driven Development

All C# logic changes must be verified through the Playwright telemetry infrastructure.

### Verification Process
1. Make C# changes in `Assets/Scripts/`
2. Build WebGL: Unity builds automatically or via CI
3. Run verification:
   ```bash
   npx playwright test
   ```

### Test Infrastructure
- **Test file**: `tests/tracer.spec.ts`
- **Pattern**: C# code emits telemetry via `Application.ExternalEval()`
- **Capture**: `window.__telemetryLogs` collects all console output
- **Assert**: Playwright verifies expected output appears

### Completion Rule (MANDATORY)
**No bead can be marked as "Complete" unless the corresponding Playwright test passes locally.**

If adding new functionality, extend `tracer.spec.ts` or create new test files following the same pattern.

## 3. Submission Protocol

### Pull Request Requirements
All work must be submitted via Pull Request with:
1. **Title**: Clear description of the change
2. **Bead ID**: Include `Bead: PocketSquire-Arena-XXX` in PR description
3. **Summary**: Intent, changes, and verification results

### Commit Message Format
Include the bead ID in commit messages:
```
<type>: <description> [Bead: PocketSquire-Arena-XXX]

<optional body with details>

Co-Authored-By: Claude <noreply@anthropic.com>
```

Examples:
- `feat: add health tracking system [Bead: PocketSquire-Arena-abc]`
- `fix: resolve null reference in combat [Bead: PocketSquire-Arena-xyz]`

## 4. Interaction with Reviewer

A separate **Reviewer Agent** will audit all submitted work.

### Review Process
1. Author submits PR with bead reference
2. Reviewer audits code quality, tests, and documentation
3. Reviewer approves or requests changes

### Handling Change Requests
If the Reviewer requests changes:
1. **Create a sub-bead** documenting the fix:
   ```bash
   bd create "Fix: <description>" --type task --priority 2 --description="Parent: PocketSquire-Arena-XXX (original task)"
   ```
2. Implement the requested changes
3. Run verification tests
4. Update the PR
5. Close the sub-bead when approved

## 5. Workflow Summary

```
1. bd create "Task" --description="Parent: <previous-bead>"
2. Implement changes
3. npx playwright test (MUST PASS)
4. git commit -m "type: description [Bead: XXX]"
5. git push && create PR
6. Await Reviewer approval
7. bd close <bead-id>
8. bd sync
```

---

**Parent Bead**: `PocketSquire-Arena-c4t` (Initialize Playwright Validation)
**Current Bead**: `PocketSquire-Arena-g9y` (Establish Author Agent Persona)
