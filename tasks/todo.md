# Task: Fix Player Menu Escape Behavior

## Objectives
- [x] Prevent Pause Menu from opening when ESC is pressed to close Player Menu in Town Scene.
- [x] Ensure consistent input handling across all menus responding to ESC/Cancel.

## Plan
1. [x] Update `PlayerMenuController.cs`: Consume "Pause" when handling "Cancel" or "Inventory".
2. [x] Update `ConfirmationDialog.cs`: Consume "Pause" when handling "Cancel".
3. [x] Update `ItemSelectionDialog.cs`: Consume "Pause" when handling "Cancel".
4. [x] Update `PauseMenu.cs`: Consume "Pause" (and "Cancel" for good measure) when it closes a sub-menu instead of pausing.
5. [x] Update `ShopController.cs`: Consume "Cancel" and "Pause" (consistency).

## Verification
- [ ] PlayTown: Open Player Menu, press ESC -> Player Menu closes, Pause Menu does NOT open.
- [ ] PlayTown: Press ESC when no menu open -> Pause Menu opens.
- [ ] PlayTown: Open Shop, press ESC -> Shop closes, Pause Menu does NOT open. (Verify existing fix)
- [ ] PlayBattle: Open Item Selection, press ESC -> Dialog closes, Pause Menu does NOT open.
