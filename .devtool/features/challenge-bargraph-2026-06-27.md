---
id: "challenge-bargraph-2026-06-27"
status: "backlog"
priority: "high"
assignee: null
epic: "systems"
dueDate: "2026-08-31"
created: "2026-06-27T12:00:00.000Z"
modified: "2026-06-27T12:00:00.000Z"
completedAt: null
labels: ["math", "system"]
order: "a2"
---
# BarGraph challenge UI

Static bar chart image alongside a numerical input field. Player reads bar values and computes a result (difference, total, ratio). Used for consumption and output comparisons.

Phase 1: static pre-authored image + TextInput (shares initial resource structure with TextInput and LineGraph). Phase 2 (later pass): replace with procedural graph renderer driven by resource parameters. Design the `.tres` resource format now to carry both a static image path **and** the procedural parameter fields — phase 2 upgrade must not break existing challenge resources.
