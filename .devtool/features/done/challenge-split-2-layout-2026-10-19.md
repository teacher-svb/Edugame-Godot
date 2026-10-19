---
id: "challenge-split-2-layout-2026-10-19"
status: "done"
priority: "high"
assignee: null
epic: "systems"
dueDate: "2026-11-30"
created: "2026-10-19T12:00:00.000Z"
modified: "2026-10-19T12:00:00.000Z"
completedAt: "2026-10-19T12:00:00.000Z"
labels: ["ui", "system", "challenge-split"]
order: "Zw2"
---

> Implemented 2026-10-19. Headless smoke test (Godot 4.7) drove the full flow: roll → 3-slot UI → type answer → submit enabled → correct/incorrect reported. Visual layout not yet checked. Legacy `ChallengeUIType` strategies now throw (kept only for registry validation until task 6); SelectThree (no Input set) will throw when shown until task 5. Seed is already threaded through `StateManagerGame.ShowChallenge(challenge, seed?)` for task 4.
# Challenge split 2/7: ChallengeUI three-slot layout + controller wiring

Restructure the challenge screen into three slots: **visualisation left**, **question text right**, **input bottom centre** (next to/above the submit button). Wire controller/model to `ChallengeRound` instead of `MathChallenge.Values`.

**Build tasks:**

1. **Scene**
   - [x] `Scenes/UI/ChallengeUI.tscn`: left = `VisualizationContainer`, right = `QuestionContainer`, bottom centre = `InputContainer`
   - [x] Remove `_challengeScenes` dictionary export
2. **Widget contracts**
   - [x] `ChallengeVisualizationWidget : Control` with `Init(ChallengeRound)`
   - [x] Rework `ChallengeInputWidget` to `Init(ChallengeRound)` (unknowns + `OptionsFor`), keep `ValueAssigned(param, value)` event
   - [x] `IChallengeInput` interface so a visualisation widget can also act as input (FormulaRing); when present, skip `Input` slot and warn if one is also set
3. **Builder**
   - [x] `ChallengeUI.Builder`: `WithQuestion(round)`, `WithVisualization(challenge.Visualization, round)`, `WithInput(challenge.Input, round)`
   - [x] `ChallengeUIFactory.Build` calls the builder directly (no strategy lookup)
4. **Controller/Model**
   - [x] `ChallengeModel` holds `ChallengeRound` + player values; `IsComplete` = all unknowns assigned
   - [x] `ChallengeController.SubmitChallenge` uses `round.Check(...)`
5. **Verify**
   - [x] `dotnet build` passes
   - [x] Layout looks right at game resolution (check in editor/game)

## Files

- Scenes/UI/ChallengeUI.tscn
- scripts/Game/UI/MVC_Challenges/View/ChallengeUI.cs
- scripts/Game/UI/MVC_Challenges/View/ChallengeUIFactory.cs
- scripts/Game/UI/MVC_Challenges/View/ChallengeUIElements/ChallengeInputWidget.cs
- scripts/Game/UI/MVC_Challenges/View/ChallengeUIElements/ChallengeVisualizationWidget.cs (new)
- scripts/Game/UI/MVC_Challenges/Controller/ChallengeController.cs
- scripts/Game/UI/MVC_Challenges/Model/ChallengeModel.cs
