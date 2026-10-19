---
id: "challenge-split-1-model-2026-10-19"
status: "done"
priority: "high"
assignee: null
epic: "systems"
dueDate: "2026-11-30"
created: "2026-10-19T12:00:00.000Z"
modified: "2026-10-19T12:00:00.000Z"
completedAt: "2026-10-19T12:00:00.000Z"
labels: ["math", "system", "challenge-split"]
order: "Zw1"
---

> Implemented 2026-10-19; round logic verified with a scratch console test (22 checks). Still to check in the Godot editor: `Visualization`/`Input` exports show a subtype picker (no concrete subclasses exist until task 3), and "Extract Params" fills `Params`. Open question for task 5: pool params are sampled with replacement, so SelectThree can roll e.g. 1+1+1.
# Challenge split 1/7: challenge data model + seeded round generation

First step of splitting challenge UI into **visualisation** × **input**. Pure data/logic, no UI changes yet.

`MathChallenge` becomes: `Formula` (expression, result is implicit `answer`), `Params` (`ParamSpec[]`), `Unknowns` (param names the player supplies, may include `answer`), `QuestionText`, `HintText`, `Visualization` (`ChallengeVisualization`), `Input` (`ChallengeInput`). A roll produces a `ChallengeRound` holding values for every param + `answer`, generated from a seed. Rolling all values first then hiding unknowns guarantees an integer solution exists.

**Build tasks:**

1. **ParamSpec resource**
   - [x] `ParamSpec` (Resource): `Name`, `Min`, `Max`, optional `Pool` (int[]), optional `Fixed` value
   - [x] Auto-populate `Params` from formula via existing "Extract Params" tool button
2. **Abstract bases**
   - [x] `ChallengeVisualization` (abstract Resource): `PackedScene Scene`, virtual `ChallengeRound Generate(MathChallenge, int seed)` with default: sample each `ParamSpec` (fixed → pool → range), compute `answer`
   - [x] `ChallengeInput` (abstract Resource): `PackedScene Scene`
   - [x] Export both on `MathChallenge` so the inspector shows a subtype picker
3. **ChallengeRound**
   - [x] Holds `Seed`, `Values` (param → int incl. `answer`), `Unknowns`, `Givens`
   - [x] `OptionsFor(param)`: pool if authored, otherwise correct value + seeded distractors
   - [x] `Check(Dictionary<string,int> playerValues)`: substitute givens + player values, re-evaluate formula, compare to `answer` (any valid solution accepted)
4. **Question/hint templating**
   - [x] `{param}` / `{answer}` placeholders filled from round; unknowns render as `?`
5. **Verify**
   - [x] `dotnet build` passes
   - [x] Same seed → identical round; different seed → different values within spec ranges

## Files

- scripts/Game/Systems/Challenges/MathChallenge.cs
- scripts/Game/Systems/Challenges/ParamSpec.cs (new)
- scripts/Game/Systems/Challenges/ChallengeRound.cs (new)
- scripts/Game/Systems/Challenges/ChallengeVisualization.cs (new)
- scripts/Game/Systems/Challenges/ChallengeInput.cs (new)
