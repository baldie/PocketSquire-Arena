# Author Agent Protocol

## 1. Workflow
1. **Start**: Run `npm run task:start -- "Description"` to track your work.
2. **Develop**: Write code in `Assets/Scripts/`.
3. **Verify**:
   - Run `npm run test:unit` (Fast C# logic check)
   - Run `npm run test:integration` (Slow Playwright check)
4. **Finish**: Run `npm run task:submit` (Automatically handles PR, bead closing, and syncing).

## 2. Boundaries
- **Arena Domain**: Focus on `Assets/Scripts/Core` and `Assets/Scripts/Arena`.
- **Constraint**: Do not modify beads created by other agents.

## 3. Definition of Done
- Unit tests pass.
- `npm run task:submit` executes successfully.
