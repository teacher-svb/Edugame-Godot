---
id: "challenge-split-3-text-slice-2026-10-19"
status: "done"
priority: "high"
assignee: null
epic: "systems"
dueDate: "2026-11-30"
created: "2026-10-19T12:00:00.000Z"
modified: "2026-10-19T12:00:00.000Z"
completedAt: "2026-10-19T12:00:00.000Z"
labels: ["math", "ui", "challenge-split"]
order: "Zw3"
---

> Implemented 2026-10-19. TextVisualization auto-renders `formula = answer` (`*`→`×`, `/`→`:`) unless `Template` is set. Smoke test passed with a Cornelis-identical challenge (5 - 3 = ?, 2 → correct, 7 → wrong). Still to do: play the actual Cornelis quest in-game.
# Challenge split 3/7: first vertical slice: TextVisualization + TextInput

End-to-end proof of the new architecture using the simplest pair. Migrate Cornelis' bridge challenge.

**Build tasks:**

1. **TextVisualization**
   - [x] `TextVisualization` resource + scene: renders formula with givens filled and unknowns as `?` (e.g. `5 - 3 = ?`), or empty when the question text already says it all
2. **TextInput**
   - [x] `TextInputResource` (or `TextChallengeInput`) + scene: one numeric `SpinBox` per unknown, labelled with the param name when more than one
3. **Migrate**
   - [x] Cornelis' bridge in `Scenes/levels/level2.tscn`: formula `x - y`, `x` fixed 5, `y` fixed 3, unknown `answer`, question `Hoeveel is {x} - {y}?`
4. **Verify**
   - [x] Play Cornelis quest: correct answer completes objective, wrong answer fails it

## Files

- scripts/Game/Systems/Challenges/Visualizations/TextVisualization.cs (new)
- scripts/Game/Systems/Challenges/Inputs/TextChallengeInput.cs (new)
- scripts/Game/UI/MVC_Challenges/View/ChallengeUIElements/Widgets/TextInputWidget.cs
- Scenes/UI/ChallengeVisualizations/Text.tscn (new)
- Scenes/UI/ChallengeInputs/TextInput.tscn (moved from ChallengeStrategies)
- Scenes/levels/level2.tscn
