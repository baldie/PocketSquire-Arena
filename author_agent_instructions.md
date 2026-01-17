# Author Agent Protocol

## Workflow
1. **Start**: Run `npm run task:start -- "short-kebab-slug"`
   - *Constraint:* short-kebab-slug must be kebab-case, and have a max of 5 words.
2. **Develop**: Write code in `Assets/Scripts/`.
   - Implement changes + Unit Tests. Focus only on the task at hand.
3. **Verify**:
   - Run `npm run test:unit` (Fast C# logic check)
   - *Constraint:* Retries allowed. Task is incomplete until this passes.
4. **Finish**:
   - Run `npm run task:submit`
   - Output the generated PR link.