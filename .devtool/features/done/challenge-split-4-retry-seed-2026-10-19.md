---
id: "challenge-split-4-retry-seed-2026-10-19"
status: "done"
priority: "high"
assignee: null
epic: "systems"
dueDate: "2026-11-30"
created: "2026-10-19T12:00:00.000Z"
modified: "2026-10-19T12:00:00.000Z"
completedAt: "2026-10-19T12:00:00.000Z"
labels: ["math", "system", "challenge-split", "persistence"]
order: "Zw4"
---

> Implemented 2026-10-19. Headless smoke test passed: wrong answer → retry shows identical values; seed survives a JSON round-trip of MyGameData; correct answer clears the seed → next trigger rolls new values. Still to do in-game: fail Cornelis, retrigger, then save + reload and retrigger.
# Challenge split 4/7: keep same values after a failed attempt

After a wrong answer, retrying the same challenge must show the same round (same seed → same values). A new seed is rolled only once the challenge has been completed (or never, if it is one-shot).

**Build tasks:**

1. **Trigger owns the seed**
   - [x] `MathChallengeTrigger` rolls a seed on first trigger and reuses it on every retry
   - [x] Clear/reroll seed on `ChallengeCompleted`
2. **Persistence**
   - [x] Store seed in save data (`IBind` on trigger, `ChallengeSaveData`, keyed by node path) so a failed challenge shows the same values after save/load
3. **Verify**
   - [x] Fail Cornelis' challenge, retrigger → identical values
   - [x] Save + reload after failure → identical values

## Files

- scripts/Game/Systems/Challenges/MathChallengeTrigger.cs
- scripts/Game/Systems/Challenges/ChallengeSaveData.cs (new)
- scripts/Game/Systems/SaveLoadSystem/MyGameData.cs
- scripts/Game/Systems/SaveLoadSystem/SaveLoadManager.cs
