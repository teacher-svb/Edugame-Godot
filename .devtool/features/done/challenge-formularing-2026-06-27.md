---
id: "challenge-formularing-2026-06-27"
status: "done"
priority: "high"
assignee: null
epic: "systems"
dueDate: "2026-12-11"
created: "2026-06-27T12:00:00.000Z"
modified: "2026-10-20T12:00:00.000Z"
completedAt: "2026-10-20T12:00:00.000Z"
labels: ["math", "system"]
order: "a0"
---

> Closed 2026-10-20: the rings are an animated way to *show* a challenge; the player doesn't spin or choose anything, so no interactive version is needed. Delivered as `ConcentricCirclesVisualization`.

# FormulaRing challenge UI

Concentric rings that spell out the challenge's formula, outer to inner, alternating values and operators. Each ring spins to its (seeded) option and locks in at the top; the centre shows the result, or `?` when the player has to find it. Used for the M4 challenges (engi12, engi13, engi14, schl04), paired with a regular input.

**Build tasks:**

1. **Resource**
   - [x] `ConcentricCirclesVisualization`: rings derived from the formula (`x + y - z` → 5 rings), `NumberSegments`, `SpinDuration`
   - [x] Optional `Operators` pool: operators picked per round from the seed, so the formula itself varies
2. **Widget**
   - [x] `ConcentricCirclesWidget`: staggered spin with ease-out, lock-in pulse, top marker and selection window, operators drawn larger for the pixel font
3. **Verify**
   - [x] Rings land exactly on the rolled values; answer checked through the input; retry shows the same spin (seeded)

## Files

- scripts/Game/Systems/Challenges/Visualizations/ConcentricCirclesVisualization.cs
- scripts/Game/UI/MVC_Challenges/View/ChallengeUIElements/Widgets/ConcentricCirclesWidget.cs
- Scenes/UI/ChallengeVisualizations/ConcentricCircles.tscn
