# Task: Migrate Legacy Input to Input System

## Objectives
- [x] Analyze codebase for `Input.` usage (Grep Search).
- [x] Verify `InputManager` wrapper availability and usage.
- [x] Replace `Input.GetButtonDown` in `SaveSlotSelector.cs` with `InputManager.GetButtonDown`.
- [x] Verify no other legacy usages in `Assets/_Game` and `Assets/Tests`.

## Verification
- [x] `SaveSlotSelector.cs` updated.
- [x] Clean grep search for `\bInput\b`.
