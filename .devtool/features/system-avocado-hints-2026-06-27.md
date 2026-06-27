---
id: "system-avocado-hints-2026-06-27"
status: "backlog"
priority: "high"
assignee: null
epic: "systems"
dueDate: null
created: "2026-06-27T12:00:00.000Z"
modified: "2026-06-27T12:00:00.000Z"
completedAt: null
labels: ["system"]
order: "a5"
---
# Avocado hint system

Player-requested hint system for math and logic challenges. Hints are delivered as Mabel talking to Avocado, with Avocado's imagined responses appearing as thought bubbles — fiction never breaks. Hints are guiding questions ("What type of sum is this?" / "What is the height of x?"), not direct answers.

Rules: player-triggered only (no interruptions from Avocado). Hint content is challenge-specific and authored per `.tres` challenge resource. Integrate with `GameStateChallenge` — a dedicated input opens the hint bubble, which can be dismissed. Each challenge resource needs an optional hints array.
