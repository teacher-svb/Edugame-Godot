---
id: "challenge-sequence-2026-06-27"
status: "backlog"
priority: "high"
assignee: null
epic: "systems"
dueDate: "2026-08-31"
created: "2026-06-27T12:00:00.000Z"
modified: "2026-06-27T12:00:00.000Z"
completedAt: null
labels: ["math", "system"]
order: "a1"
---
# Sequence challenge UI

Number series panel showing a sequence with one missing value. Player identifies the rule and inputs or selects the answer. Maps to M2+ challenges and diagnostic readouts.

Add `Sequence` to `ChallengeUIType` enum, implement `IChallengeUIStrategy`. Resource format: array of values with one slot marked as the missing entry; challenge answer is the value that belongs there.
