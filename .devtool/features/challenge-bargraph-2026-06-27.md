---
id: "challenge-bargraph-2026-06-27"
status: "review"
priority: "medium"
assignee: null
epic: "systems"
dueDate: "2026-11-27"
created: "2026-06-27T12:00:00.000Z"
modified: "2026-10-20T12:00:00.000Z"
completedAt: null
labels: ["math", "system"]
order: "a2"
---

> Implemented 2026-10-20 as `BarGraphVisualization` + `GraphBar`. Ranged bar params are narrowed to gridline values before rolling (rerolling would pass ~1 in 10 per bar), so their input options are gridline values too; an unknown bar is a full-height dashed slot with `?`. Screenshot-verified. Still to check: look and feel in-game.

# BarGraph visualization (procedural)

Procedural bar chart drawn from the round's values, so bar heights change per round instead of being a fixed picture. Player reads bar values and computes a result (difference, total, ratio). Used for consumption and output comparisons (e.g. veld04: daily water use per family member).

**Build tasks:**

1. **Resource**
   - [x] `BarGraphVisualization : ChallengeVisualization`: list of bars (label + param name), axis range/step, optional unit
   - [x] Param fields via `ParamFields` so they get the inspector dropdown
   - [x] `ImpliedConstraints()`: values within the axis range (and on the step if snapping)
2. **Widget**
   - [x] `BarGraphWidget : ChallengeVisualizationWidget<BarGraphVisualization>` drawing axes, gridlines, bars and labels (`ChallengeDrawing` helpers); unknown bars drawn as `?`
3. **Verify**
   - [x] Test challenge with `TextChallengeInput`; existing `ImageVisualization` challenges keep working

## Files

- scripts/Game/Systems/Challenges/Visualizations/BarGraphVisualization.cs (new)
- scripts/Game/UI/MVC_Challenges/View/ChallengeUIElements/Widgets/BarGraphWidget.cs (new)
- Scenes/UI/ChallengeVisualizations/BarGraph.tscn (new)
