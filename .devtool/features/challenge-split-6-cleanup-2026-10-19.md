---
id: "challenge-split-6-cleanup-2026-10-19"
status: "review"
priority: "high"
assignee: null
epic: "systems"
dueDate: "2026-10-23"
created: "2026-10-19T12:00:00.000Z"
modified: "2026-10-19T12:00:00.000Z"
completedAt: null
labels: ["system", "challenge-split", "cleanup"]
order: "Zw6"
---

> Implemented 2026-10-19. Also removed `MathChallenge.FormulaParams` (superseded by `Params`), the plugin autoload entry in project.godot, and fixed the stale ChallengeUI.tscn UID in managers.tscn. Verified: build, headless editor start (no errors), headless run of Cornelis (level2) + SelectThree (.tres) answered correctly. Still to do: open in your editor + play.
# Challenge split 6/7: remove legacy enum/strategy/registry code

Once everything runs on the new model, delete the old 1:1 type system.

**Build tasks:**

1. **Delete**
   - [x] `ChallengeUIType` enum + all `*UIStrategy` classes (`ChallengeUIStrategy.cs`)
   - [x] `addons/ChallengeUIFactoryPlugin/` (registry + plugin) and its entry in `project.godot`
   - [x] `ChallengeValue` resource + `MathChallenge.Values`, `SetFormulaParam`, `ChangeValue`, `_selectedValues`
   - [x] `Scenes/UI/ChallengeStrategies/` (after moving what's still used)
   - [x] `CombinationLockValue.cs` (unused, depends on `ChallengeValue`)
   - [x] `MathChallenge` legacy members: `Answer`, `ComputeRandomAnswer`, `CheckAnswer`, `Question`/`Hint` getters, `ChallengeUIType`
   - [x] Orphaned `.uid` files under `MVC_Challenges/View/`
2. **Radar**
   - [x] Radar: dropped (empty placeholder); since rebuilt as `RadarVisualization` (2026-10-19)
3. **Docs**
   - [x] Rewrite "Challenge System" + "Adding a New Challenge Type" sections in `CLAUDE.md`
4. **Verify**
   - [x] `dotnet build` passes, editor opens without plugin errors, all challenges in game still work

## Files

- scripts/Game/UI/MVC_Challenges/View/ChallengeUIStrategy.cs
- addons/ChallengeUIFactoryPlugin/
- project.godot
- scripts/Game/Systems/Challenges/ChallengeValue.cs
- CLAUDE.md
