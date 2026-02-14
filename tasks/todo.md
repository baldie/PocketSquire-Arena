# Task: Remove Extra Debug Logging in SaveIndicator.cs

## Plan
- [x] Remove `Debug.Log` calls from `SaveIndicator.cs`
- [x] Verify changes by compiling/checking for errors
- [x] Save game on quit from PauseMenu if in Town scene

## Progress
- [x] Remove `Debug.Log` in `Awake`
- [x] Remove `Debug.Log` in `OnEnable`
- [x] Remove `Debug.Log` in `OnDisable`
- [x] Remove `Debug.Log` in `HandleSaveStarted`
- [x] Remove `Debug.Log` in `HandleSaveEnded`
- [x] Remove `Debug.Log` in `SetVisible`
- [x] Implement save on quit in `PauseMenu.cs`
- [x] Verify save on quit

## Review
- Removed verbose logs from `SaveIndicator.cs`.
- Added save-on-quit logic to `PauseMenu.cs` specifically for the Town scene.
- Verified both scripts compile without errors.
