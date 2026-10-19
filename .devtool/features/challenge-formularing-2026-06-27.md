---
id: "challenge-formularing-2026-06-27"
status: "backlog"
priority: "high"
assignee: null
epic: "systems"
dueDate: "2026-12-11"
created: "2026-06-27T12:00:00.000Z"
modified: "2026-10-19T12:00:00.000Z"
completedAt: null
labels: ["math", "system"]
order: "a0"
---

> Progress 2026-10-19: display version done as `ConcentricCirclesVisualization` (rings spin to a seeded option and lock in, centre shows result or `?`, optional random operators). Remaining for this task: interactive version where the player spins rings to build the equation for a fixed centre target. Make the widget implement `IChallengeInput` (the builder then skips the Input slot).

> Depends on the challenge-split tasks (challenge-split-1..6, 2026-10-19). Implement as a `ChallengeVisualization` resource (+ reuse existing inputs) instead of adding a `ChallengeUIType` enum value / `IChallengeUIStrategy`.
# FormulaRing challenge UI

Concentric spinning rings for equation construction. Rings alternate value/operator from outer to inner; centre shows a fixed target result. Ring count varies per challenge resource: 3 rings for `a op b = c`, 5 rings for `a op b op c = d`. Used for all M4 challenges (Great Conversion, Professor Mabel).

Add `FormulaRing` to `ChallengeUIType` enum, implement `IChallengeUIStrategy`, and register. Design the challenge `.tres` resource format to carry ring count and value/operator pools.
