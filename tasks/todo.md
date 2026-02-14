# Task: Progression System Refactor

## Objectives
- [x] Implement Hybrid XP Curve Engine (Options 1 & 2)
  - [x] Create `XpCurveConfig` and `XpCurveGenerator` POCOs
  - [x] Update `ProgressionLogic` to use new curve generation
  - [x] Add overrides support
- [x] Implement Dynamic Perk Pools (Option 3)
  - [x] Create `PerkPool` and `PerkSelector` POCOs
  - [x] Add `PerkPool` configuration to `ProgressionSchedule`
  - [x] Update `LevelUpPresenter` to use `PerkSelector`
- [x] Implement Simulation Harness
  - [x] Create `ProgressionSimulator`
  - [x] Verify with unit tests
- [x] Integration & Cleanup
  - [x] Remove `LevelReward.ExperienceRequired` usage
  - [x] Verify all unit tests pass

## Verification
- [x] All unit tests passed (including new `XpCurveGeneratorTests`, `PerkSelectorTests`, `ProgressionSimulatorTests`)
- [x] Codebase builds successfully
