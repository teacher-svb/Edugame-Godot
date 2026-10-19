---
id: "challenge-split-5-port-inputs-2026-10-19"
status: "review"
priority: "high"
assignee: null
epic: "systems"
dueDate: "2026-11-30"
created: "2026-10-19T12:00:00.000Z"
modified: "2026-10-19T12:00:00.000Z"
completedAt: null
labels: ["math", "ui", "challenge-split"]
order: "Zw5"
---

> Update 2026-10-19: CombinationLock now instances `Scenes/UI/carrousel.tscn` per unknown (overridable via `CombinationLockInput.WheelScene`), replacing the code-built `CombinationLockWheel`. Fixed carrousel.tscn: controller `_carrousel` was unset (null in `_Ready`) and the Carrousel anchors were zeroed (size 0).

> Implemented 2026-10-19. Added `MathChallenge.DistinctValues` (no two params share a value in roll or answer; on for SelectThree) and a `ValueCleared` event on the input contract (used by Cogwheel deselect). CombinationLockWheel rebuilt as ▲ / values / ▼ (its controller was wired before its buttons existed). Headless smoke test passed for all three inputs incl. SelectThree loaded from .tres. Still to check: look and feel in-game.
# Challenge split 5/7: port Dropdown, CombinationLock and Cogwheel inputs

Move the remaining input widgets onto the new `ChallengeInput` resource + `ChallengeRound` contract. Options come from `round.OptionsFor(param)` instead of `ChallengeValue.ParamName`. Migrate the SelectThree challenge.

**Build tasks:**

1. **Dropdown**
   - [x] `DropdownInput` resource + scene: one `OptionButton` per unknown, options from `OptionsFor`
2. **CombinationLock**
   - [x] `CombinationLockInput` resource + scene: one `CombinationLockWheel` per unknown, candidates from `OptionsFor` (fixes current bug where wheels get 0 candidates because values have no `ParamName`)
3. **Cogwheel**
   - [x] `CogwheelInput` resource + scene: buttons for shared pool, clicks assign to unknowns in order; allow deselect
4. **Migrate**
   - [x] `GameData/challenges/SelectThree_easy.tres`: formula `x+y+z`, unknowns `x,y,z`, each `ParamSpec.Pool = [1,2,4,8]`, CombinationLock input
5. **Verify**
   - [x] Each input works with a test challenge (via `scripts/test/ChallengeActivator.cs`)

## Files

- scripts/Game/Systems/Challenges/Inputs/ (new resources)
- scripts/Game/UI/MVC_Challenges/View/ChallengeUIElements/Widgets/{Dropdown,CombinationLock,Cogwheel}Widget.cs
- scripts/Game/UI/MVC_Challenges/View/ChallengeUIElements/CombinationLock/CombinationLockWheel.cs
- Scenes/UI/ChallengeInputs/ (new location)
- GameData/challenges/SelectThree_easy.tres
