---
id: "challenge-linegraph-2026-06-27"
status: "todo"
priority: "medium"
assignee: null
epic: "systems"
dueDate: "2026-11-27"
created: "2026-06-27T12:00:00.000Z"
modified: "2026-10-20T12:00:00.000Z"
completedAt: null
labels: ["math", "system"]
order: "a3"
---

> **Still to do (checked 2026-10-20):** code, widget and scene are in place, but no level challenge uses this visualization yet and the in-game look-and-feel check is still open. Use it in a real challenge, play it in-game, then mark done.

> Implemented 2026-10-20 as `LineGraphVisualization`: `Line` expression in `Variable` (default `t`), drawn left to right and clipped to the axes; optional `MarkX` marker with dashed guides. Only the marked point is constrained (on the axes, on a gridline), so lines may run off the graph. Screenshot-verified with an oil-level line. Still to check: look and feel in-game.

# LineGraph visualization (procedural)

Procedural line graph drawn from the round's values. Player traces a value on a continuous graph (e.g. RPM over wind speed for hend04, oil level over days for lies04) and answers a question about the relationship.

**Build tasks:**

1. **Resource**
   - [x] `LineGraphVisualization : ChallengeVisualization`: x/y axis ranges and steps, axis titles, line defined by params (e.g. start value + slope, or a param per point)
   - [x] Param fields via `ParamFields` so they get the inspector dropdown
   - [x] `ImpliedConstraints()`: points within the axis ranges, readable on gridlines
2. **Widget**
   - [x] `LineGraphWidget : ChallengeVisualizationWidget<LineGraphVisualization>` drawing axes, gridlines and the line (`ChallengeDrawing` helpers); optional marker on the asked point
3. **Verify**
   - [x] Test challenge with `TextChallengeInput` and `DropdownInput`

## Files

- scripts/Game/Systems/Challenges/Visualizations/LineGraphVisualization.cs (new)
- scripts/Game/UI/MVC_Challenges/View/ChallengeUIElements/Widgets/LineGraphWidget.cs (new)
- Scenes/UI/ChallengeVisualizations/LineGraph.tscn (new)
