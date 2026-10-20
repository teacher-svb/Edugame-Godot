---
id: "challenge-sequence-2026-06-27"
status: "backlog"
priority: "high"
assignee: null
epic: "systems"
dueDate: "2026-11-06"
created: "2026-06-27T12:00:00.000Z"
modified: "2026-10-20T12:00:00.000Z"
completedAt: null
labels: ["math", "system"]
order: "a1"
---

# Sequence visualization

Number series panel showing a sequence with one missing value. Player identifies the rule and types or selects the answer. Maps to M2+ challenges (schl01/schl02 bell times) and diagnostic readouts.

**Build tasks:**

1. **Resource**
   - [ ] `SequenceVisualization : ChallengeVisualization`: number of terms, which term is missing; terms follow from the formula's params (e.g. start `a` and step `d`, formula `a + n * d` for the missing term)
   - [ ] Alternative worth checking first: let `TextVisualization` templates contain expressions (`{a + d}`) via `FormulaEvaluator`; a sequence then needs no new class
2. **Widget**
   - [ ] Row of term boxes, missing one shown as `?`
3. **Verify**
   - [ ] Test challenge with `TextChallengeInput` and `DropdownInput`; `answer >= 0` style rules keep terms sensible
