---
id: "challenge-split-7-searchgrid-2026-10-19"
status: "todo"
priority: "high"
assignee: null
epic: "systems"
dueDate: "2026-11-30"
created: "2026-10-19T12:00:00.000Z"
modified: "2026-10-19T12:00:00.000Z"
completedAt: null
labels: ["math", "ui", "challenge-split"]
order: "Zw7"
---
# Challenge split 7/7: SearchGrid visualisation

First visualisation that overrides value generation. Params map to a location on a grid (e.g. column/row from formula results); the grid size constrains the generated values.

**Build tasks:**

1. **Resource**
   - [ ] `SearchGridVisualization`: `GridSize` (Vector2I), mapping of params → axis
   - [ ] Override `Generate` to clamp/sample within grid bounds, on top of `ParamSpec` ranges
2. **Widget**
   - [ ] Draw grid with axis labels; mark given location(s); unknown location shown as `?` or left for the input
3. **Verify**
   - [ ] Test challenge with TextInput and with Dropdown input; values always land inside the grid

## Files

- scripts/Game/Systems/Challenges/Visualizations/SearchGridVisualization.cs (new)
- scripts/Game/UI/MVC_Challenges/View/ChallengeUIElements/Widgets/SearchGridWidget.cs
- Scenes/UI/ChallengeVisualizations/SearchGrid.tscn
