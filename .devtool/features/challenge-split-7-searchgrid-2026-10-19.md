---
id: "challenge-split-7-searchgrid-2026-10-19"
status: "review"
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

> Implemented 2026-10-19. Also added `ChallengeRound.RandomFor(purpose)` (seed-scrambled random streams, now shared with RadarVisualization). Screenshot-verified layout.
# Challenge split 7/7: SearchGrid visualisation

First visualisation that overrides value generation. Params map to a location on a grid (e.g. column/row from formula results); the grid size constrains the generated values.

**Build tasks:**

1. **Resource**
   - [x] `SearchGridVisualization`: `Columns`/`Rows`, `XStart`/`XStep`/`YStart`/`YStep`, `XParam`/`YParam` (empty = seeded random inner line), optional `Background`, `MarkerLabel`
   - [x] Override `Generate`: deterministic reroll until the marker is on the grid (and on line crossings with `SnapToLines`)
2. **Widget**
   - [x] Draw grid with axis labels and a pulsing marker
3. **Verify**
   - [x] Test challenge with TextInput; values always land on the grid (300 seeds)
   - [ ] Dropdown input: not tested with the grid yet. Expect answer distractors of answer ± a few, not other grid lines (e.g. 70 → 67, 72); consider grid-aligned options

## Files

- scripts/Game/Systems/Challenges/Visualizations/SearchGridVisualization.cs (new)
- scripts/Game/UI/MVC_Challenges/View/ChallengeUIElements/Widgets/SearchGridWidget.cs
- Scenes/UI/ChallengeVisualizations/SearchGrid.tscn
