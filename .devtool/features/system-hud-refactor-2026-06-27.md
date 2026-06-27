---
id: "system-hud-refactor-2026-06-27"
status: "backlog"
priority: "high"
assignee: null
epic: "systems"
dueDate: null
created: "2026-06-27T12:00:00.000Z"
modified: "2026-06-27T12:00:00.000Z"
completedAt: null
labels: ["system"]
order: "a4"
---
# HUD stats/actions refactor

Combine the stats view and actions view into a single unified HUD. Both panels must be accessible from a single action menu input. Active quest info displayed during exploration.

Currently the stats view and actions view are separate. Refactor `MVC_ActionsMenu` (or introduce a combined controller) so one input toggles between or overlays both. Review in context of `GameStatePlay` — the HUD lives there.
