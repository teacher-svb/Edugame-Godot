# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

This is an educational game built with **Godot 4.7** and **C# (.NET 8.0)**. The game features a sophisticated architecture with math challenges, quests, inventory systems, and a component-based character system.

## Build and Development Commands

## GitHub Operations
Use the gh CLI for all GitHub tasks, not MCP:
- `gh issue list / create / view`
- `gh pr create / list / merge`
- `gh repo view`
- `gh api` for anything else

### Building the Project
```bash
dotnet build
```
The project uses Godot.NET.Sdk/4.7.2 and requires .NET 8.0.

### Running the Game
Open the project in Godot Engine 4.7+ and press F5, or use:
```bash
godot --path . --editor  # Open in editor
godot --path .           # Run directly
```

### Dependencies
- **Godot Engine 4.7+** (with .NET support)
- **.NET 8.0 SDK**
- **NuGet packages:**
  - Newtonsoft.Json 13.0.3 (for save/load serialization)

### Project Structure
```
/scripts/Lib/         - Reusable framework code (state management, extensions, persistence)
/scripts/Game/        - Game-specific implementations
/Scenes/              - Godot scene files (.tscn)
/Scenes/autoload/     - Singleton managers scene
/assets/              - Game assets (sprites, music, fonts)
/GameData/challenges/ - Challenge resource files (.tres)
/GameData/quests/     - Quest resource files (.tres)
/GameData/themes/     - UI theme resources
/scripts/Trail/       - GPUTrail3D (trail effect)
```

## Core Architecture

### 1. Stack-Based State Management System

The game uses a **stack-based state machine** as its architectural foundation:

- **`StateManagerGame`** (autoloaded) - Global state coordinator that inherits from `AbstractStateStack`
- States can be pushed/popped like a navigation stack, maintaining state history
- **Base state:** the game boots into a base (start menu) state with no gameplay input, never popped; pause, inventory and Save don't work at the start menu. `StartPlay()` (called by `SaveLoadManager` once a loaded scene is bound) inserts Play directly above the base, once per stack, and hands the player its input actions (`Player.SetInputActions`). A level run on its own (F6, test rooms) has a `Player`, so the base state calls `StartPlay` itself.
- **Lifecycle:** `OnEnter` once, when a state first becomes the top; `OnPause` when another state is entered on top of it; `OnResume` (not a second `OnEnter`) when it is the top again; `OnExit` once when it is removed, also for a state that was never entered. Every game state that can be covered has an `OnResume` (the loader states, always on top, and the dev menu overlay have none) that re-asserts what a state above may have undone (pause, its input actions). Hooks run in `try/catch` (logged), so an exception doesn't freeze the stack. Pushes and pops requested during a transition wait until it is done.
- **Closing your own state:** `Pop(expected)` exits that state wherever it is (the task completes after its `OnExit`), or let it exit through `ExitOnNextUpdate`. Plain `Pop()` pops whatever is on top. E.g. `ShowChallenge(challenge, seed)` returns `false` (and shows nothing) while a challenge is open; `CloseChallenge()` closes the challenge state, not the top.
- **Scene loads** (`GameStateLoadingScreen`): `LoadScene`: after the fade to black and once the scene file loaded, the loader unwinds the stack below itself to the base (pause menu, inventory, Play... each `OnExit` runs, its UI hides), then changes the scene and exits itself. While a loader is on the stack, every push (`ShowMessage`, `ShowChallenge`, `OpenInventory`, ...) goes **below** it, so a message the new level pushes shows after the fade-out. `LoadLocation` (in-level teleport) only fades, moves the player and fades back: no unwind, the states stay. For both, a second load while loading is ignored with a warning. A failed load (unknown path or `uid://`, not a scene) is logged, fades back and unpauses; the old scene and its states stay.
- `StateManagerGame.IsIdle`: the base or Play is the active top (start menu or plain gameplay). `PushOverlay(state)` pushes a state that isn't a registered game state (the dev menu, 5d).
- Nine game states (`scripts/Game/StateManagementGame/`):
  - `GameStatePlay` - Normal gameplay
  - `GameStateChallenge` - Math challenge overlay
  - `GameStateMessage` - Character dialogue/notifications. One message box at a time: `StateManagerGame.ShowMessage` adds a message to the open box (`IsOpen` / `Enqueue`) instead of pushing a second message state; each message keeps its `onDismissed` and they all run in order once the box is closed (since 2026-11-24, `bug-message-queue-drops-commit`). When those callbacks open the next message box straight away (an objective's complete text → the next start text, a `MessageStep` → the line after it) and it is the new top, the box stays up and the tree paused, and the new box swaps the text as a next does (since 2026-11-25, `bug-quest-message-gap`). The speaker of each line (`CharacterData`) turns toward the player as it shows: `Character3D.TurnSpeakerToPlayer`, the nearest character with that data, never the player
  - `GameStateInventory` - Inventory screen
  - `GameStateLoadingScreen` - Scene transitions
  - `GameStateActCard` - Card between acts (`StateManagerGame.ShowActCard(title, subtitle, onDismissed)`): pauses, fades to black, closes on next/close after `MinSeconds`
  - `GameStateReading` - Reading panel for a longer text (`StateManagerGame.ShowReading(readableText, onClosed(readToEnd))`): pauses; next turns the page and closes on the last one, close closes on any page (see 5e)
  - `GameStatePauseMenu` - Pause menu
  - `GameStateSettings` - Settings screen (`StateManagerGame.OpenSettings(..., onClosed)`)

**Key abstractions** in `scripts/Lib/StateManagement/` (namespace `TnT.Systems.State`):
- `AbstractStateStack` - One step per frame when no transition runs: exit a state whose exit was requested, else activate a new top (pause the covered state, then enter or resume the new one), else update the top. `Push`, `Pop()`, `Pop(expected)`; protected `Insert`, `InsertBelow`, `ExitStatesBelow(anchor)`, `Top`, `ActiveState`, `Contains`. The bottom state is never popped
- `BaseState` - Encapsulates state behavior with OnEnter/OnExit/OnPause/OnResume/OnUpdate delegates and `ExitOnNextUpdate`; `IsEntered`
- `IStateObject<T>` - Factory pattern interface for state creation (`GetState(options)`)

### 2. Autoload/Singleton Pattern

The `Managers` scene (`Scenes/autoload/managers.tscn`, process mode Always) is autoloaded and contains all core systems as child nodes:
- `StateManagerGame` - State coordinator, with one child node per game state
- `SaveLoadManager` - Persistence system
- `InputManager` - Centralized input handling
- `QuestManager` - Quest orchestrator
- `DevShortcuts` - Dev checkpoints (5d)
- `UIManager` (`ManagerUI`: the shared `Next` / `Close` / `Open` input actions) - All UI controllers, on their CanvasLayers (section 3). The inventory is `InventoryController` / `InventoryModel` here
- `MusicManager` - Music

Most managers expose a static `Instance` property for global access.

### 3. MVC Pattern for All UI Systems

**Every UI subsystem** follows the Model-View-Controller pattern in `scripts/Game/UI/MVC_*` (namespace `TnT.EduGame.UI`):

```
MVC_[Feature]/
  ├── Controller/[Feature]Controller.cs  - Mediates between Model and View
  ├── View/[Feature]View.cs              - Visual presentation and animations
  └── Model/[Feature]Model.cs            - Business logic and data
```

New systems use the `Controller/View/Model/` subfolders. Small systems may be flat (the three files directly in `MVC_[Feature]/`): `MVC_Fade`, `MVC_Inventory`, `MVC_PauseMenu` and `MVC_Settings` are, and stay that way (moving them would break scene refs).

Implemented systems:
- MVC_Challenges - Math challenge UI
- MVC_Messages - Notification/dialogue. The speaker closeup copies the character's `VisualRoot` (with its `dialogCamera`) into a `SubViewport` with `own_world_3d`, its own `CloseupEnvironment` (ambient light + the town's tonemap; without it the unlit side renders pure black) and `CloseupLight` (aimed from the closeup camera for every new model); without its own world the copy stands in the level at the origin (`bug-closeup-model-in-level`, 2026-11-23). So a character's model **must hang under `VisualRoot`** (as in `dad.tscn`: `root_node = "../../<model>"`, no `CharacterAnimator.VisualRoot` override), or its portrait is empty (`bug-portrait-missing-model`). A line without a character scene (or one without a `CharacterController3D` / `VisualRoot`) hides the closeup (a warning, unless the path is empty); consecutive lines of the same speaker keep the model. The box's Next/Close buttons have `FocusMode = None` (mouse only; Space/Escape go through `next`/`close`) and are wired after the box has scaled in
- MVC_Inventory - Inventory UI. Items arrive over the pickup `ItemEventChannel`; a full inventory refuses the pickup (`ItemEventListener.Accepts` = `HasFreeSlot`, asked through `ItemEventChannel.CanDeliver`), so the item stays in the world. A world `Item` (`scripts/Game/Systems/InventorySystem/`) is picked up with E while the player is in its `_area` (empty: the first `Area3D` under it; see Interaction focus in 5e); optional `_quest` + `_availableFromObjectiveId`: not pickable and no prompt while that objective hasn't started (the avocado in `level1`: I.1 obj5). A picked-up item is saved (`WorldItemSaveData`, scene-scoped id), so it stays gone after a load and when the level is entered again
- MVC_Fade - Screen transitions
- MVC_PauseMenu - Pause menu. "Laden" is disabled without a save (refreshed on every show and after Save); "Opslaan" is disabled with a tooltip while a quest reaction holds up saving (section 5); "Afsluiten" quits through `CleanQuit` (section 9)
- MVC_Settings - Settings screen: values kept in `user://settings.cfg` (not in the save game), loaded and applied at start, written only when a value changed (a slider when the drag ends). Volumes go to the `Music` / `SFX` buses; text size scales every font size of the UI theme `GameData/themes/Notifications.tres` (text with its own size in a scene is not scaled). From the start menu (which hides meanwhile) or the pause menu
- MVC_StartMenu - Start menu: Load uses the real save (`SaveLoadManager.MainGameName`) and is refreshed every time the menu shows; "Afsluiten" quits through `CleanQuit`
- MVC_ActCard - Card between acts ("Einde van Akte I")
- MVC_Reading - Reading panel for letters and notices (R2)

Controllers are instantiated in `managers.tscn` with Model and View as child nodes.

**CanvasLayers** (under `UIManager`): the shared `CanvasLayer` (layer -1: messages, challenges, inventory, pause menu, settings), `ReadingLayer` (10), `ActCardLayer` (20), `FadeLayer` (100, above every game UI); the dev menu builds its own layer 128. No view writes a `CanvasLayer.Layer`: a UI that must draw above or below others gets its own layer node in `managers.tscn`.

**Tweens:** views animate with the `UITweens` extensions (`TnT.Systems.UIAnimation`: `FadeIn/Out`, `ScaleIn/Out`, `SlideIn/Out`, `Bump`, `HoverBounce`, ...). One running tween per control and slot; each helper returns `Task<bool>`: `true` when its tween finished, `false` when a new tween replaced it in its slot or it was killed (control freed). A view changes the control after the await only on `true` (`if (await this.FadeOut()) Visible = false;`): on `false` another animation owns the control now. A view hidden with `Visible = false` sets `Visible = true` before it fades in.

**Focus** (keyboard/controller): the start menu focuses "Laden" when a save exists, else "Nieuw spel"; the pause menu focuses its first enabled button on show, again on resume, and moves the focus when the focused button gets disabled. A menu releases its focus when hidden or covered (settings on top), so Space/Enter can't press a button behind the screen in use. The challenge view focuses the input (section 4).

### 4. Challenge System (Visualization × Input)

A challenge is split into **what is asked** (math), **how it is shown** (visualization) and **how the player answers** (input). Visualizations and inputs combine freely.

**Data** (`scripts/Game/Systems/Challenges/`):
- `MathChallenge` (Resource) - `Formula` (expression; its result is the implicit `answer`), `Params` (one `ParamSpec` per formula param), `Unknowns` (params the player supplies, may include `answer`), `QuestionText`/`HintText` (`{param}` placeholders), `Constraints`, `Visualization`, `Input`. "Extract Params" tool button syncs `Params` with the formula.
- Constraints: `MathChallenge.Constraints` is a list of rules, true/false expressions in the formula's own language: `answer >= 0`, `x > y`, `x % y = 0`, `answer % 10 = 0`, `x % 10 + y % 10 < 10` (no tens crossing), combined with `AND` / `OR` / `NOT`, plus the shorthand `distinct(x, y, z)`. `FormulaEvaluator` expands, validates (unknown names, bad syntax, not a comparison → clear error before rolling) and evaluates them. `ChallengeRound.Roll` rerolls until all hold; `OptionsFor` gives an authored pool as it is (shuffled; rules and `count` don't apply), otherwise the correct value plus distractors that keep the rules true (the other values as rolled). Visualizations add their own via `ImpliedConstraints()` (radar range, grid snap) as rule strings. Answers are always whole numbers (values are `int`); `Check()` only tests the formula (a result within 1e-9 of a whole number counts, for rounding in `/`). No rule means negative answers are allowed. `%` is DataTable's: between `+ -` and `* /` in precedence, left-associative, whole-number operands only; when DataTable fails on a `%` over a `/` result (`(a / b) % 5`), `FormulaEvaluator` works out every `%` in C# and evaluates again (a fractional operand gives a fractional remainder). `Roll` first validates the formula (`FormulaEvaluator.ValidateFormula`: a clear `ArgumentException` for bad syntax, a comparison instead of a calculation, or no formula). Resources with param fields derive from `ParamAwareResource` (visualizations, `GraphBar`). In the inspector, a visualization's param fields (e.g. `RadarVisualization.DistanceParam`) show a dropdown of the formula's params: `MathChallenge` passes the names down whenever `Params` or `Visualization` is assigned.
- `ParamSpec` (Resource) - how a param is rolled: fixed value → `Pool` → `[Min, Max]` range
- `ChallengeRound` - one rolled instance: every value incl. `answer`, deterministic per seed. `Check()` re-evaluates the formula (any valid solution counts), `OptionsFor()` gives input options (an authored pool as is, a fixed param or range: correct value + distractors), `Format()` fills placeholders (unknowns as `?`), `RandomFor(purpose)` gives separate replayable random streams from the seed (rolling, options, visualizations). Pure C#; parsing/evaluating formulas lives in `FormulaEvaluator` (`System.Data.DataTable`).
- `ChallengeVisualization` / `ChallengeInput` (abstract Resources, shared base `ChallengeElement`) - subclasses carry their own settings and a default scene (`DefaultScenePath`; the `Scene` export overrides it). `ChallengeVisualization.Generate()` is virtual so a visualization can constrain the rolled values. An empty `Visualization` field means `NoVisualization` (Null Object, via `MathChallenge.VisualizationOrNone`), so nothing needs a null check.
  - Visualizations: `TextVisualization`, `ImageVisualization` (picture + `ImageAnnotation`s at normalized positions with `{param}` text), `SearchGridVisualization` (labelled coordinate grid, optional map background, marker snapped to line crossings), `BarGraphVisualization` (one `GraphBar` per param on a labelled value axis; ranged bar params are narrowed to gridline values before rolling; an unknown bar shows as `?`), `LineGraphVisualization` (`Line` expression in a variable, e.g. `v - r * t`, clipped to the axes; optional `MarkX` point that must land on the grid), `RadarVisualization` (distance rings + blip at a param's distance; rerolls until it fits the range), `ConcentricCirclesVisualization` (rings outer → inner spell the formula; optional `Operators` pool randomizes operators per round), `KeyVisualization` (a `BarGraphVisualization` drawn as a key, one tooth per bar, depth 0..9; its `KeyWidget` doubles as input: the player cuts the unknown teeth; e.g. `GameData/challenges/key_code_4.tres`, place value `a*1000 + b*100 + c*10 + d`) `SequenceVisualization` (number series in a row of boxes with one `?`: `Term` an expression in `i` (term index from 0) over the params, `TermCount` and `MissingTerm` expressions too (negative = from the end), `MissingValue` the param the gap stands for; `Generate` checks that the missing term equals it, which is a check and not a rule, so dropdown distractors still work; e.g. `GameData/challenges/sequence_bell_times.tres`: formula `s + n * d`, Term `s + i * d`, `n + 1` terms, the last missing) (in `Visualizations/`)
  - Inputs: `TextChallengeInput`, `DropdownInput`, `CombinationLockInput`, `CogwheelInput` (in `Inputs/`)

**UI** (`scripts/Game/UI/MVC_Challenges/`): `ChallengeUI` has three slots: visualization (left), question text (right), input (bottom centre). `ChallengeUI.ShowRound(challenge, round)` fills them. Widgets derive from `ChallengeVisualizationWidget<TSettings>` / `ChallengeInputWidget<TSettings>` (or the non-generic bases when they need no settings) and get the challenge, round and their resource as typed `Settings` in `Init()` before `_Ready()`; a widget given the wrong resource type, or whose `Init` throws, is freed with a clear message. Inputs report `ValueAssigned(param, value)` / `ValueCleared(param)` (also when a text field is emptied); a visualization widget implementing `IChallengeInput` doubles as input and the `Input` slot is skipped. `IChallengeInput.Handles(param)` (default true) says which unknowns it takes: a self-input visualization that doesn't cover every unknown (e.g. a `KeyVisualization` with `answer` as unknown) logs an error and the challenge's `Input` (or a `TextChallengeInput`) is used instead. Each widget names a `FocusTarget` (text: the field, lock: the first wheel, key: itself), focused when the view opens; Enter submits wherever the focus is, while Submit is enabled (only when the round is complete and the view open; disabled again right after an answer). Hints: a round with `HintText` shows the hint toggle (the avocado button, or Q), which opens the hint under it; no hint, no toggle. Widget scenes live in `Scenes/UI/ChallengeVisualizations/` and `Scenes/UI/ChallengeInputs/`.

**Flow:** `MathChallengeTrigger` → `StateManagerGame.ShowChallenge(challenge, seed)` (false while another challenge is open) → `ChallengeModel` rolls the round → `Answered(bool)` → the trigger closes it with `CloseChallenge()` and emits `ChallengeCompleted` / `ChallengeFailed`. The trigger keeps its seed (persisted as `ChallengeSaveData`) until the challenge is solved, so retries show the same values; with `KeepSolvedSeed` (set by `CodeDoor`) it keeps it after solving too, so a clue keeps showing the solved code. A roll that throws is logged, the seed dropped, and nothing is shown (`RunChallengeAsync` returns false).

Challenge resources are stored as `.tres` files in `/GameData/challenges/` or inline in level scenes.

### 5. Quest System (Chain of Responsibility + Event-Driven)

**Architecture:**
- `Quest` (Resource) - Contains array of `QuestObjective`, manages progression. `State` is getter-only: the current objective's state, COMPLETED once every objective is
- `QuestManager` - Processes quest messages through chain of responsibility (`QuestProcessor.cs`; messages in `QuestMessage.cs`):
  - `QuestProcessor<QuestMessageStart>` - NOTSTARTED → INPROGRESS
  - `QuestProcessor<QuestMessageComplete>` - INPROGRESS → COMPLETED
  - `QuestProcessor<QuestMessageFail>` - INPROGRESS → NOTSTARTED
- **Transitions wait for the message:** a processor only *requests* the transition (`Quest.ObjectiveTransitionRequested`); `QuestManager` shows the objective's text with `StateManagerGame.ShowQuestMessage(objective, state, commit)` and the state changes when the player dismisses it (a fail without a fail text commits at once, without a box; an empty start or complete text still shows a box with only the icon). Until then the objective is **pending**: another Start/Complete/Fail for it is ignored with a warning, and `StartQuest` does nothing (safe to call repeatedly). A commit requested before a `Restore` (load, new game, dev jump) is dropped. After the commit: COMPLETED **starts the next objective automatically**; a fail puts the objective back to NOTSTARTED and **starts it again** (so its listeners, e.g. a challenge, run again)
- `QuestEventChannel` - Broadcasts each committed objective state change (`QuestManager.OnQuestUpdated`); the listeners subscribe to it
- `QuestEventListener` - Filters objective updates (id/state) and runs its child `QuestStep` nodes top to bottom, each awaited (Observer + Command). Steps (in `QuestSystem/Steps/`) hold their receiver and arguments as typed exports: `IdleStep`, `PatrolStep`, `FollowStep`, `PopStateStep`, `ChallengeStep`, `MessageStep` (a character's line, awaited until dismissed; skipped on a catch-up run, which has no objective), `ActCardStep` (the card between acts; skipped on a catch-up run), `ReadStep` (5e), `SaveGameStep` (autosave; skipped on a catch-up run), `FreeNodeStep` (removes a node if it's still there; safe to repeat, so fine for catch-up: e.g. `level1` `obj5-completed-AvocadoGone` removes the avocado once I.1 obj5 is completed, because a picked-up `Item` only frees itself and nothing is saved), plus generic `CallMethodStep` (any public method by reflection, awaited if it returns a `Task`; the overload is picked by arg count, then by arg types; string args convert to `StringName` and enum parameters, int args to enums). New step: subclass `QuestStep` with `[GlobalClass]`, implement `Task Run(QuestObjective)`. The base `QuestStep` is `[Tool]`; don't mark a step `[Tool]` itself when it exports a non-tool script type (`Character3D`, `MathChallengeTrigger`, ...): in the editor those nodes are plain placeholders and the typed export throws (`CallMethodStep` is `[Tool]` for its method dropdown; its target is a plain `Node`).
- **A reaction** emits `ReactionStart`, then `ReactionEnd` only when every step ran; a step throwing `OperationCanceledException` (a failed challenge) or any other exception (logged) stops the remaining steps and emits `ReactionCancelled` instead. Wire "complete the objective" to `ReactionEnd`. A reaction whose listener left the tree (scene change) stops quietly after its current step, without either signal.
- **Saving is refused while an objective's reaction runs** (`QuestEventListener` reactions, live or resumed): a save then would store the objective halfway, and a load doesn't replay it. `SaveLoadManager.CanSave` / `SaveGame()` returns false with a warning; the pause menu greys out "Opslaan" (`QuestStepListener.BlockingReactionsChanged`). `QuestsCompletedListener` reactions and catch-up runs don't count (a load replays them). `SaveGameStep` doesn't count its own reaction and waits for any other one to end; under a `QuestEventListener` it needs Resume After Load (it warns otherwise).
- **After a load** (catch-up and resume): a load raises no quest events. Both listener types catch up once the scene is ready and bound (`QuestManager.Bound`, raised after every node's `_Ready` and after the other bindings: `SaveLoadManager` binds the quests last), not in `_Ready`, so neither node order nor the bindings (characters at their saved positions, doors, challenge seeds) can overwrite a catch-up. Listeners run in tree order (their `_Ready` order). A level started on its own (F6) gets no `Bound`, so no catch-up.
  - `QuestEventListener._catchUpAfterLoad` (opt-in): a reaction's world change that a later objective needs is lost after a save made mid-quest. With the flag, a listener whose objective is already COMPLETED runs its steps once more without an objective (messages, cards, saves skip themselves) and without `ReactionStart`/`ReactionEnd`. Only for steps that are safe to repeat (show / open something); never for walks, challenges or toggles (e.g. `ToggleDoor`, which saves itself). Used on II.4 `obj3-init-Verdict` (paper and notice).
  - `QuestEventListener._resumeAfterLoad` (opt-in): for an INPROGRESS reaction that completes its own objective (`ReactionEnd` → `CompleteObjective`). If that objective is still INPROGRESS after the load (an old save made mid-reaction, or the level was left while it ran), the reaction runs again from its first step, with the objective and with signals. Set on every such `objN-init-*` listener in the levels.
- `QuestsCompletedListener` - Runs its child steps once all listed quests are completed, live or as a catch-up (quests finished in another scene or in a loaded save). The catch-up passes no objective: `MessageStep`, `ActCardStep`, `ReadStep` and `SaveGameStep` skip themselves; don't show a challenge or start a quest from it. `SaveLoadManager.LoadGame` / `NewGame` first call `QuestManager.Restore(saveData)` (states in memory set from the save, or all not started, without quest events), so the catch-ups see the loaded states and not those of the session that was running; wire `ReactionEnd` to open up the next quest, e.g. enable its trigger `Area3D` (`set_deferred("monitoring", true)`). These are the arrows in the GDD quest diagram. Both listeners share `QuestStepListener` (runs the steps, `ReactionStart`/`ReactionEnd`/`ReactionCancelled`)
- `QuestNode` - Quest root in a level; connect signals (e.g. `Area3D.body_entered` → `_OnPlayerStartObjective`) with the objective id as an extra call argument. Ignored quest messages (wrong objective/state, or a transition pending) log a warning. `FindObjective(id)` returns the live objective (as `QuestManager` holds it).
- `QuestStarter` (Area3D, `IInteractable`, since 2026-11-26) - the start area of a quest an **NPC gives**: while the player is inside, a billboarded key label (`Scenes/interactables/quest_prompt.tscn`, "E"; made automatically when `Prompt` is empty, `top_level`) floats `PromptHeight` (2.3 m) above `Giver` and follows them when they walk; the interact key (`pickup`, E, shared with items and readables through `InteractionFocus`, distance measured to the giver) starts `ObjectiveId` on `QuestNode`. Shown only while it can start: monitoring (arm it with `set_Monitoring` like a plain area), the objective NOTSTARTED and no transition pending. Replaces `body_entered` → `_OnPlayerStartObjective` on those areas; arrivals and discoveries without an NPC (chamber `StartArea`s, `I4_Arrival`, `III1_ArrivalArea`, the II.5 table, LookingForAvocado) and the II.1 shout across the moat stay plain areas. With E the player already stands at the NPC, so an obj1 "walk up to them" completes right after its start message (an `obj1-init` listener without steps, `ReactionEnd` → `CompleteObjective`), not from a close area. Test room: `Scenes/tests/quest_starter_test.tscn` (Hendrik, I.2 Hendrik obj1).

Quest resources are stored as `.tres` files in `/GameData/quests/`.

Quest states: NOTSTARTED, INPROGRESS, COMPLETED. The enum also has FAILED, but nothing sets it: a fail is NOTSTARTED + an automatic restart.

### 5b. Platforming Challenges (wizard school chambers)

Components in `scripts/Game/Systems/Platforming/` (namespace `TnT.EduGame.Platforming`):
- `ChamberSafety` (one per platforming level, `Instance`) - remembers the last safe GridMap cell the player stood on (item name starts with `room_` or `corr_pad_`) and respawns the player there after falling below `KillY` or a hazard hit (`Hit()`, with a short grace period)
- `ChallengeHazard` (Area3D) - while `Active` (keyed by animations), a player inside is hit
- `PushZone` (Area3D) - while `Active`, pushes the player back along its local Z, away from its centre, via `CharacterController3D.AddPush()` (sources add a push every physics frame)
- `CrumblingPlatform` (AnimatableBody3D) - shakes when the player lands on its `Sensor`, falls, comes back after `ResetTime`

**Dungeon tile names** (school and mana mine meshlibs, since 2026-11-21): the prefix is the godoodungeon tile set. `room_*` = room tile set, with `room_door` = the dungeon's **Door** item (school 54, mine 12); `corr_*` = corridor tile set (`corr_pad_*` bridge pads, `corr_chasm*` / `corr_rock*` the empty tile). A hand-built room (Built room box) uses only `room_*` tiles, `room_door` on the wall ring. Item numbers are kept from the old names, so the school meshlib has gaps (the godoowave-only tiles were removed); the export JSON `renamed_2026_11_21` maps old → new names.

The element challenge bridges (MeshLibrary items 55-61, `corr_<element>_<challenge>`) carry marker scenes `challenges/corr_<element>_<challenge>.tscn` in `assets/3D/village/interiors/school_interior/`, placed by godoodungeon. Element chambers `Scenes/levels/level3_wizard_<fire|water|earth>.tscn` use `school_tiles_<element>.meshlib` and `GameData/wfc/school_dungeon_{rooms,corridors}_<element>.tres` (godoodungeon: the tile sets must use the GridMap's MeshLibrary). These assets are generated by scripts in `Edugame-Design/.work/scripts/school_interior_scripts/` (`godot_element_meshlibs.gd`, `godot_challenge_markers.gd`) and `.../rotatable_tiles_game_scripts/` (`godot_meshlibs.gd`, `game_tilesets.py`).

**Key doors** (since 2026-11-21, placeholder geometry): locked doors are placed by the game, not by the tile set (godoodungeon has no lock feature; don't add one to the addon). `CodeDoor` (`scripts/Game/Systems/Doors/`) runs its `MathChallengeTrigger` when the player walks into its `Trigger`; solved, its leaves swing into the room and the open state is saved in `doorData` (`DoorSaveData`, bound in `SaveLoadManager`); its trigger keeps the solved seed (`KeepSolvedSeed`), so the clue keeps showing the code. `assets/3D/village/interiors/school_interior/doors/room_door_key.tscn` = key door (challenge `GameData/challenges/key_code_4.tres`, `KeyVisualization`), N-S build: stands on a `room_door` cell turned with it (outside to the north). `CodeClue` (`doors/code_clue_board.tscn`) shows `Text` (default `{answer}`) from its `Source` trigger's round, so door and clue share the trigger's `Seed`. Test room: `Scenes/tests/key_lock_test.tscn`. Placement in the dungeon chambers: root kanban cards `dungeon-pop-*`.

**Dungeon planning** (`scripts/Game/Systems/Dungeon/`, namespace `TnT.EduGame.Dungeon`, since 2026-11-21): pure C# over godoodungeon's `DungeonGraphData` (`DungeonGraph.StoredOn(gridMap).CoreGraph`), never inside the addon. `RoutePlanner.Plan(graph, roomRoles, seed)` → `DungeonRoute`: start and goal (a room box's `Data["role"]` = `start` / `goal`, else the graph's diameter ends, the seed picking which is the start), the main path (`ShortestPath`), and per room a `RoomInfo` (Start / Goal / Path / Side, the path room it hangs off, depth, hops from the start, dead end). Encounters: `DungeonEncounter` (abstract Resource, Strategy; `Pass` Gate/Filler, `Plan(EncounterContext, EncounterTarget)`) plans against `EncounterContext` (Builder: rooms by `RoomFilter`, free cells and doors handed out once, `RandomFor(purpose)` streams, collected `Placement`s). `RoomFilter` Resources (Specification): `RoleFilter`, `DeadEndFilter`, `HopsFromStartFilter`, `AllOfFilter` / `AnyOfFilter` / `NotFilter`. `RoomCellPicker.FreeCells`: the room's footprint (walls included: a wall tile is mostly floor) minus its door cells and the cell inside each door, filtered by the caller's free-floor check. `LockEncounter` (gate: `DoorScene`, `ClueScene`, `ClueCount`, `CluePlacement` PathRoomBefore / SideRoomBefore) locks the next path room's door of a path corridor (a shared door cell closes all its corridors), prefers locks that cut the goal off, and places its clues before it with the lock's seed; `ObjectEncounter` (filler: `Scene`) takes a free cell in a room its filter accepts. `DungeonRecipe` (Resource, one per chamber in `GameData/dungeon/`): `Slots` (`EncounterSlot`: an encounter at Anywhere / PathStep (negative = from the goal) / StartRoom / GoalRoom), a weighted `Pool`, budgets `Gates` and `Fillers`; slots first, then pool draws. `DungeonPopulator` (Node3D in the level: `Map`, `Recipe`, `Seed` 0 = the dungeon's seed, `FreeFloorTiles`, `Player`) plans in `_Ready` from the graph stored on the GridMap (`Populate(graph)` for a runtime generator; room roles come from the room box's live `Data["role"]`, falling back to the graph's copy, which Generate only refreshes), spawns under `RoomNN/Lock_n` / `Clue_n` / `Object_n`, sets the door's challenge `Seed` and the clue's `Source`, puts the player in the start room and calls `BindSubtree`. The player's spawn cells (the start room's centre: 1, 2 or 4 cells) are avoided (`EncounterContext.Avoid`: a pick that lands there draws again from its own stream, so other picks and the locks' codes stay as they were). A `RoutePlanner` failure (e.g. `start`/`goal` roles in unconnected rooms) is logged and planned again between the diameter ends; `Populate` clears `Route`, `Goal` and `Placements` before an early return. `DungeonRecipe.IdProblems()` (warned by the populator): an encounter without an `Id`, or different encounters of one kind sharing one (they would share random streams). Spawned scenes get their transform before entering the tree (a door's `AnimatableBody3D` leaves don't follow a parent moved afterwards); doors turn -Z to the door direction, clues and objects face +Z (the camera). Used in all four chambers (`level3_wizard_1` / `_fire` / `_water` / `_earth`), each with its own `GameData/dungeon/<chamber>_recipe.tres` (for now all the same: a key lock into the goal room + a pooled key lock, 2 gates). A chamber without a stored graph (generated before godoodungeon 1.1) only gets a warning: Generate it again. Test level: `level3_wizard_earth.tscn`.

**Goal rooms** (since 2026-11-22, placeholder geometry): each chamber has a hand-built `GoalRoom` (Node3D) with the `SparkTestPlatform` at its origin, the element's wizard at (1.3, 0, -2.2) and `ExitToNext` (`goal_portal.tscn`, a closed `LevelExit` whose portal `Visual` shows only while open) at (0, 0, -3.6). `DungeonPopulator.GoalRoom` moves it onto the `GoalAnchor` cell and turns its +Z out through the entrance (`GoalAnchorPicker`: the plain-floor cell (`GoalFloorTiles`) of the goal room farthest from the entrance door, ways in last, no door behind it); that cell and the one behind are reserved (`EncounterContext.Reserve`), so no lock, clue or object lands there. Built in the level rather than spawned so the quest (`wzrd2`-`wzrd4`) can wire to it in the editor. `SparkTestPlatform` (`scripts/Game/Systems/School/`, Area3D): the player stepping on it plays a fizzle (sparks, dying glow, keeps playing while a message pauses the game) and emits `Tested` (connect to `QuestNode.CompleteObjective` with the objective id); again after stepping off. Scenes: `assets/3D/village/interiors/school_interior/goal/`. Exits: `_1` → Fire → Water → Earth → forest (default spawn); the quest opens them. I.3 quest wiring per chamber (Fire, Water, Earth since 2026-11-22, `GameData/quests/I.3-WizardSchoolTrials-<El>.tres`; the Earth wizard gives the verdict, its exit leads to the forest): `StartSpawn/StartArea` (moves with the start spawn) starts obj1, `GoalRoom/ArrivalArea` (just in front of the platform) completes it, `SparkTestPlatform.Tested` completes obj2, `Quests/I_3-<El>/obj3-init-SendOn` (wizard idles) completes obj3, and `Quests/completed-OpenExit` (`QuestsCompletedListener` over the chamber's quest) opens `ExitToNext`, also after loading.

**Tile sets are one MeshLibrary item per shape, turned in the GridMap** (since 2026-11-20; school, forest, mana mine): every WfcTile is Rotation FourWay, and a tile's other directions are the same item at GridMap orientation 16 / 10 / 22 (base north turned to west / south / east). Marker scenes exist only in their N-S build; the generators place them turned with their cell. Each set's export JSON (`replaces`, `remap_old_items`) maps the old one-tile-per-rotation items.

### 5c. Level Exits and Spawn Points

Components in `scripts/Game/Systems/Levels/` (namespace `TnT.EduGame.Levels`, since 2026-11-21):
- `LevelExit` (Area3D) - the player walking in loads `TargetScene` and arrives at its spawn point `TargetSpawn`: a node path relative to the target scene's root (empty = the level's default one), With **no `TargetScene`** it teleports within the current level instead (`StateManagerGame.LoadLocation`, with the fade; usable again once the player has left it). Picked in the inspector from the **Spawn Point** dropdown, which lists the spawn points saved in `TargetScene` (or, without one, those in the edited scene) (`SpawnPoint.ListIn`, read from its `SceneState`; not those inside instanced sub-scenes). Saved as a string (`_targetSpawnPath`), not a `NodePath`: the editor rewrites `NodePath` properties when a node is moved or renamed. A path that no longer exists shows as `(missing)` with a configuration warning, and at run time falls back to the default spawn. `Enabled` / `Open()` / `Close()`: a quest opens a closed exit with a `CallMethodStep` under a `QuestsCompletedListener` (which also catches up after a scene load or a loaded save, so the exit needs no save data of its own). A player who arrives inside an exit is ignored until they step out. Signal `Used`.
- `StructureDoor` (`scripts/Game/Systems/Doors/`, since 2026-11-26) - a door of a village structure, under `beaverford.tscn` `Doors/<Doorway>` (16: e.g. `MabelHome_Door`, `School_Door`, `WizardTower_Door`, `Tavern_FrontDoor`). Closed at the start; each leaf has a collider in the GLB that swings with it, so a closed door blocks its doorway and any `LevelExit` behind it. Quests open it: `CallMethodStep` `Open` / `Close` / `Toggle` (put it under the `QuestsCompletedListener` that opens the exit behind it). Plays the GLB's `<Doorway>_Open` / `_Close` (90° inward, 0.58 s) on its own `DoorPlayer`, with copies that keep only its leaf tracks, and strips its leaf tracks from the structure's other animations (Godot's glTF import puts every animated node into every animation; without that the wizard tower's `Turn` held its door shut). Saved as `DoorSaveData` (bound in `SaveLoadManager.BindEntities`), restored in the open pose without the swing; `Open` on an open door does nothing (catch-ups). Wired: `level2` `Quests/I_2-completed-OpenSchool/OpenSchoolDoor` (wizard tower), `Quests/I_1-completed-OpenHomeDoor` (Mabel's door once I.1 is done, i.e. after she left the house), `level6` `Quests/II4-completed-OpenExitHome/OpenHomeDoor`.
- `SpawnPoint` (Marker3D) - `Default` (one per level). Renaming or moving a spawn point breaks the exits that point to it (they show a warning). Keep spawn points out of exit areas.
- A new scene's `uid://` (e.g. a `LevelExit.TargetScene`) is only known to the game after the editor has scanned the project: open the editor once, or headless `godot --headless --editor --quit`. Until then loading it fails with "Unrecognized UID" (logged): the loading screen fades back and the old scene stays. `SceneLoaded` passes the path as given (often the `uid://`), not the `res://` path.
- `StateManagerGame.LoadScene(path, NodePath spawnPath)` puts the player at the spawn point **after** the save data is bound. Without that, the position saved in the previous level wins, because the player's `CharacterSaveData` is shared by all levels. `LoadScene(path)` (load game, new game) keeps the saved position; `LoadScene(path, Vector3)` (`Transporter`) always moves the player there (the origin too), also after binding.
- Chambers: `DungeonPopulator.StartSpawn` moves the level's default spawn point to the start room.
- Act I route: home `level1` (`Spawns/front_door`) ↔ town `level2_beaverford-in-the-morning` (`Spawns/from_home`; `ExitToSchool` at the wizard tower door, opened by `Quests/I_2-completed-OpenSchool` once the four I.2 quests are done, after Mabel's line) → `level3_wizard_fire` (`StartSpawn`, the default). Forest `level5_forest`: `from_school` at the west exit, `ExitToHome` at the east exit → home. `WorldBounds` (StaticBody3D, 4 `WorldBoundaryShape3D` planes facing inwards on the edge of the 23 x 28 cell grid, x -44..48, z 0..112: no height or thickness to tunnel through) keeps the player on the map; the south exits run into it, so their `LevelExit` areas must stay inside the grid. I.4 (`GameData/quests/I.4-TheHeavyHeart.tres`, since 2026-11-22): `Spawns/from_school/I4_Arrival` starts obj1, `I4_PassSpring` (a 3 m band across the only way from the west to the east half, x 0.5..3.5, z 52..76, where the spur to the spring branches off) completes it; home `Spawns/front_door/I4_ArriveHome` completes obj2 (`Quests/I_4-The Heavy Heart` in `level1`). **End of Act I:** `level1` `Quests/I4-completed-EndOfActI` (`QuestsCompletedListener` over I.4) → `ActCardStep` ("Einde van Akte I" / "Welkom in Beaverford") → `SaveGameStep`. Act II starts from "I.4 completed": give the II.1 quest a `QuestsCompletedListener` over `I.4-TheHeavyHeart.tres` (no separate save flag). The chamber exits to the next level are `GoalRoom/ExitToNext` (closed until the chamber's I.3 quest opens it), see Goal rooms in 5b.
- `GuardedGate` (Node3D, since 2026-11-22) - a way guards keep closed: `Guards` stand in front of a `Block` (e.g. a StaticBody3D barrier), and walking into `BarkArea` shows `BarkText` from `BarkCharacter` (again after stepping out). `Unguard()` removes guards, block and bark area for good (from a `CallMethodStep` under a `QuestsCompletedListener`, which also catches up after loading, so no save data). Forest: `ManaSpringGate` at the mana mine entrance (`mine_C` cell (0, 9), world (2, 0, 38); tunnel mouth z ≈ 38.2), two `Scenes/characters/mana_guard.tscn` (Knight) at (0.6 / 3.4, 0, 39.8) facing south, a placeholder wooden barricade across the mouth, bark "Halt! Geen toegang tot de bron. Bevel van de tovenaar." Unguarded in IV.2 (`engi10`).
- Act II town: `Scenes/levels/level6_beaverford-act2.tscn` (since 2026-11-22), a separate level from the Act I town (`level2` is all Act I quest wiring). Same `beaverford.tscn`, morning environment and music; villagers under `Villagers/` (Cornelis on the moat-house side of the raised drawbridge at (27, 0.3, 8.5); Hendrik, Liesbeth and Mevrouw Bram at their Act I spots by the mill, bakery and school), `Markers/TownSquare` (by the noticeboard, for II.4) and `Markers/DrawbridgeSouthBank`, an empty `Quests` node, `Spawns/from_home` (default), `Exits/ExitToHome` (closed; opened at the end of II.4 by `wzrd05`, → home `Spawns/front_door`). **Act II route** (since 2026-11-22): home `level1` has two front-door exits on the same spot, `transporters/transporter_livingroom_level2` (→ Act I town) and `transporters/transporter_livingroom_act2` (→ Act II town `Spawns/from_home`, closed); `Quests/I4-completed-TownIsActII` (`QuestsCompletedListener` over I.4, also catches up after loading) closes the first and opens the second. **II.5** (`GameData/quests/II.5-TheMysteriousLetter.tres`, since 2026-11-23, in `level1`): `Quests/II4-completed-StayHome` (`QuestsCompletedListener` over II.4, placed after `I4-completed-TownIsActII` so its catch-up runs later) closes `transporter_livingroom_act2` (Mabel stays home; it stays closed after II.5: the way out is Act III's), shows `Letter` (paper with a gear seal on the dining table at (10, 0.77, -7.5); the `Livingroom` node is offset by (2, 0, -5), so its props' local positions are not world positions) and opens `Letter/Readable` (at the table's free west end, `GameData/readables/II.5-guild_letter.tres`, 3 pages, Letter look; it sends her to the old well on the square tonight, after twelve strokes of the school bell, to look for the gear sign on its rim), and arms `II5_TableArea` (3.5 x 2.8 m around the table, kept clear of `Spawns/front_door` so arriving home doesn't start the quest during the load). The table area starts obj1 (Mabel finds the letter; completes right away), `Letter/Readable.Read` completes obj2. **End of Act II** (`act2-4`, since 2026-11-23): `level1` `Quests/II5-completed-EndOfActII` (`QuestsCompletedListener` over II.5) → `ActCardStep` ("Einde van Akte II" / "Toveren zonder toverstaf") → `SaveGameStep`. Act III starts from "II.5 completed", like Act II from "I.4 completed". **Act III route** (`act3-1`, since 2026-11-23): a third front-door exit in `level1`, `transporters/transporter_livingroom_act3` (closed; same spot as the other two) → night town `Scenes/levels/level4_beaverford-at-night.tscn` `Spawns/from_home`; opened by `OpenActIIITown` in `Quests/II5-completed-EndOfActII` (after the card, before the autosave; also on the catch-up after loading). **Night town** `level4` (uid `cr3nwuawx7hye`): night environment, same `beaverford.tscn`, no villagers, music `route7-ramble-110`; `Drawbridge` held at the end of `Drawbridge_Lower` and `DrawbridgeNavLink` on (the bridge has been down since II.1); `Markers/OldWell` at the well (-10, 0.1, 6); `Spawns/from_home` (default, same spot as in `level6`); `Exits/ExitToHome` (closed). **III.1** (`GameData/quests/III.1-TheHiddenWorkshop.tres`, obj1 so far): `III1_ArrivalArea` (8 x 2 m band across the road 5 m north of the spawn) starts obj1 (Mabel counts the twelve bell strokes: the letter's cue, there is no bell sound), `III1_WellArea` (4 x 4 m around the well) completes it; both always monitoring (the level is only reachable after II.5). The lock and the way down follow in `act3-2`. `Drawbridge` (`AnimatedProp`, `scripts/Game/Systems/Levels/`: holds an `AnimationPlayer` at the end or start of `StartAnimation` in `_Ready`; quests call `Play(anim)` and `SetPose(anim, atEnd)` through `CallMethodStep` (both take a plain `string`). `Play`'s task ends when *that* animation finishes or another one replaces it (`Play` / `SetPose` of another animation); a looping animation is started with a warning and not waited for. **Don't call `SetPose(x)` while a `Play(x)` of the same animation is awaited:** that task then stays pending) holds the bridge raised (`Drawbridge_Raise` end); a quest's catch-up after loading wins whatever the node order, because catch-ups run from `QuestManager.Bound`. **II.1** (`GameData/quests/II.1-TheDesperateHelp-Cornelis.tres`, since 2026-11-22): `Quests/I4-completed-CornelisCalls` arms `CornelisShoutArea` (16 x 10 m south of the moat, starts obj1: Cornelis shouts), `CornelisBankArea` (at `Markers/DrawbridgeSouthBank`) completes obj1; obj2 (Mabel) runs the challenge (`Quests/II_1-The Desperate Help/MathChallengeTrigger`: `b + s - c`, drawbridge picture with bridge, supplies and counterweight labels, `TextChallengeInput`; was `CombinationLockInput` until 2026-11-23, see backlog `bug-combination-lock-input`); obj3 (Cornelis) plays `Drawbridge_Lower` and his amazed line (`MessageStep`), then completes. `Quests/II1-completed-BridgeDown` puts the bridge down again after loading (`SetPose`). The drawbridge's static collider has ramps at the deck ends (since 2026-11-22, `Edugame-Design` README "Ramps on the collider"), so characters without step-up walk over the lowered bridge. The town's navmesh was baked with the bridge half raised and doesn't cross the moat: `DrawbridgeNavLink` (`NavigationLink3D`, island (27, 0, 10.2) ↔ south bank (27, 0, 23.6), off) is switched on when II.1 is completed. **II.2** (`GameData/quests/II.2-TheRumorMill.tres`, since 2026-11-22): `Quests/II1-completed-CornelisToSquare` (`CallMethodStep` `set_Enabled` on `DrawbridgeNavLink`, then `PatrolStep` with one waypoint `Markers/CornelisRoute/2_Square`: Cornelis walks over the bridge to the square at 2 m/s (his `_speed`, like dad), about 15–20 s; on a catch-up he already stands there) and `Quests/II1-completed-ArmSquare` (arms `RumorSquareArea`, 8 x 8 m by the noticeboard, kept separate so a stuck walk can't block II.2). obj1 (Cornelis) starts on E in the square (`RumorSquareArea` is a `QuestStarter` with Cornelis as giver, since 2026-11-26): reactions from Hendrik, Liesbeth, Mevrouw Bram (`MessageStep`s, the villagers stay at their workplaces); obj2 (Mabel: "het was gewoon rekenen"): their requests for help, which lead into II.3. NPC agents (`character_base.tscn` `NavigationAgent3D`, since 2026-11-24): `path_height_offset` 0, path/target distance 0.4. A character's origin is at its feet, so a navmesh must be baked close to the floor: `cell_height` 0.1 (town `beaverford.tscn`, home `level1`); a coarse `cell_height` lifts the navmesh surface (home had 0.5: surface at 0.6 m) and the agent never reaches a path point. **II.3 Liesbeth** (`GameData/quests/II.3-TheBusyWeek-Liesbeth.tres`, since 2026-11-23; one of the three Busy Week jobs, any order): `Quests/II2-completed-ArmBakery` arms `LiesbethBakeryArea` (6 x 6 m around the bakery, a `QuestStarter` with Liesbeth as giver: E starts obj1, Liesbeth asks), `obj1-init-Greet` completes obj1 right after its start message (since 2026-11-26; was a 2.5 x 2.5 m `LiesbethScalesArea` at her); obj2 (Mabel) runs the challenge (`Quests/II_3-The Busy Week-Liesbeth/MathChallengeTrigger`: `a * b - c`, a 300–500 g flour per loaf, b 2–6 loaves, c 100–1000 g already on the scale, all flour in steps of 50, `answer > 0`; `scales-diagram.png` with two labels (recipe `{a} g`, scale `{c} g`; the loaves are only in the question), `TextChallengeInput`); obj3 (Liesbeth) says the scales balance again plus one `MessageStep`, then completes. **II.3 Hendrik** (`GameData/quests/II.3-TheBusyWeek-Hendrik.tres`, since 2026-11-23, same pattern): `Quests/II2-completed-ArmMill` arms `HendrikMillArea` (6 x 6 m around Hendrik on the mill platform, `QuestStarter`, E), `obj1-init-Greet` completes obj1; the challenge (`Quests/II_3-The Busy Week-Hendrik/MathChallengeTrigger`) is `a * b - c` on `ConcentricCirclesVisualization` with `Operators` [+, -, *], so each round picks its own operators (a 2–12, b 2–9, c 2–15, `answer > 0`; the hint is about × before + and −), `TextChallengeInput`. **II.3 Mevrouw Bram** (`GameData/quests/II.3-TheBusyWeek-MsBram.tres`, since 2026-11-23, same pattern): `Quests/II2-completed-ArmSchool` arms `MsBramSchoolArea` (6 x 6 m in front of the school, on her +z side, `QuestStarter`, E), `obj1-init-Greet` completes obj1; the challenge is the shared `GameData/challenges/sequence_bell_times.tres` (`SequenceVisualization` of the bell times in minutes past 8:00, last one missing, `DropdownInput`). All three II.3 jobs are armed by their own `II2-completed-Arm*` listener. **Busy Week gate** (`act2-3`, since 2026-11-23): `Quests/II3-completed-WizardArrives` (`QuestsCompletedListener` over the three II.3 quests) shows Mabel's line (live only), reveals the traveling wizard (`Villagers/TravelingWizard`, waiting hidden at (0, 0.4, 32) on the road with `collision_layer` 0, so he stands on the floor but the player walks through him; revealed with `Show` and `set_CollisionLayer` 2) and the `SparkTestPlatform` (-3.5, 0, 6; `Show`, `set_Monitoring`), then walks the wizard, Hendrik and Liesbeth to `Markers/Exposure/*` in the square (`PatrolStep`, not awaited; about 13 / 10 / 6 s). Mevrouw Bram stays at the school: her porch is a navmesh island, and the stream bridge (-10, 0, -22) has a deck step NPCs can't climb (backlog `world-stream-bridge-ramps`). `Quests/II3-completed-ArmExposure` arms `ExposureSquareArea` (8 x 8 m at the square) on its own. **II.4** (`GameData/quests/II.4-TheExposure.tres`, since 2026-11-23): the square area (`QuestStarter`, giver the traveling wizard: the E follows him while he walks in, and shows as soon as the area is armed) starts obj1 on E (the wizard; Cornelis cheers, then obj1 completes); obj2 (Mabel) is completed by `SparkTestPlatform.Tested` (the Act I fizzle); obj3 (the verdict: Hendrik, Liesbeth, Mevrouw Bram, the wizard) shows `Noticeboard/Paper` and opens `Noticeboard/Notice` (a `Readable` 1 m in front of the board at (-5.5, 0, -0.5), `GameData/readables/II.4-public_notice.tres`, 2 pages, Notice look), then completes; obj4 (Mabel) is completed by `Notice.Read`. `Quests/II4-completed-OpenExitHome` opens `Exits/ExitToHome` and keeps the notice up after loading. **Note:** `CallMethodStep` calls C# methods by reflection: use `Show`, `set_Monitoring`, `set_CollisionLayer` (C# names), not Godot's `show` / `set_deferred`.
- In-level teleports: home bedroom ↔ living room (`transporters/transporter_bedroom-livingroom` → `Spawns/from_bedroom`, back → `Spawns/from_livingroom`). `scripts/Game/Systems/Levels/Transporter.cs` is now only used by the guild workshop's placeholder `transporter_exit_village` (no target set yet; make it a `LevelExit` once the village scene exists). It moves the player to its destination's global position, once until the player has left its area (a scene load: once for good); a destination inside another transporter's area would send the player on.

### 5d. Dev Shortcuts (debug builds only)

`scripts/Game/Dev/` (namespace `TnT.EduGame.Dev`, since 2026-11-22): jump to a checkpoint instead of playing up to it. `DevShortcuts` (node in `managers.tscn`, frees itself in a release export) holds a list of `DevCheckpoint` resources (`GameData/dev/NN_<id>.tres`: `Id`, `Title`, `CompletedQuests`, `Scene`, `Spawn`, `TutorialDone`).
- **Backtick** `` ` `` (physical key under Esc) opens a menu of the checkpoints while `StateManagerGame.IsIdle` (only the start menu or plain gameplay: not over the inventory, a menu, dialogue, challenge, card or a load): number keys 1-9 (physical keys, so also on AZERTY), arrows + Enter or a click pick one; Escape or `` ` `` closes it. The menu is a state on the game stack (`PushOverlay`), so Play gets no input under it and Escape closes only the menu. **Not F9/F10:** a game run from the editor treats those as the editor's "Suspend/Resume Embedded Project" and "Next Frame" shortcuts, even in its own window, and freezes.
- **Launch argument** `--checkpoint=<id>`: on the command line after `--` (`godot --path . -- --checkpoint=ii2`), or in the editor under Project Settings > Editor > Run > Main Run Args as `-- --checkpoint=ii2`. It jumps right after the start menu appears.
- A jump builds a fresh `MyGameData` (only the listed quests completed, every objective; tutorial done if `TutorialDone`; empty inventory), sets the quests in memory to match, and loads `Scene` at `Spawn`. Everything else comes from the levels' `QuestsCompletedListener` catch-up, like loading a save. Nothing is written to disk until the game saves. The in-memory quests are set with `QuestManager.Restore` (as `LoadGame` does), so the new level's listeners see them.
- **Dev save:** a game started by a jump runs under the save name `dev` (`DevShortcuts.SaveName`): its autosaves and the pause menu's Save / Load use `user://dev.json`, never the real save. The start menu's Load uses the real save (`SaveLoadManager.MainGameName`); New Game starts under the real name again.
- Checkpoints: `i3-fire` (I.1 and the four I.2 quests done; the first wizard chamber `level3_wizard_fire` at its default spawn `StartSpawn`, so `StartArea` starts I.3 Fire obj1 as on a normal arrival), `act2-start` (Act I done; Act II town at `Spawns/bridge_approach`, on the road just outside `CornelisShoutArea`: spawning inside it would start II.1 during the load), `ii2` (+ II.1) and `ii3` (+ II.2), both at `Spawns/bridge` on the south bank, 3 m east of the deck (x 30.5): in the walking line, Cornelis (walking to the square after II.1) runs into the player and climbs them. `ii4` (+ the three II.3 jobs) at `Spawns/square_south` (-6, 0.3, 10), just south of the square: the wizard walks in and the square area is armed. `ii5` (+ II.4) at home, `level1` `Spawns/front_door`: the letter is on the table. `act3-start` (+ II.5) in the night town `level4` at `Spawns/from_home`. New one: a `.tres` in `GameData/dev/`, added to `DevShortcuts.Checkpoints`.
- Dev tooling: reads the backtick key directly and builds its own small UI, outside `InputManager` and MVC.

### 5e. Reading (R2, since 2026-11-23)

Longer texts (a letter, a notice) are read in a panel, not in dialogue boxes. `scripts/Game/Systems/Reading/` (namespace `TnT.EduGame.Reading`):
- `ReadableText` (Resource, `GameData/readables/`): `Title`, `Body` (paragraphs, BBCode; a line with only `---` starts a new page), `Look` (Plain / Letter / Notice: paper and ink colours).
- `Readable` (Area3D; reusable scene `Scenes/interactables/readable.tscn`: 2.5 m range, `Prompt` with "lees" + the interact key, shown only while it has the interaction focus): the interact key (`pickup`, E, the same as items) opens its `Text`. Signals `Opened`, `Closed(readToEnd)`, `Read` (closed after reaching the last page; connect it to `QuestNode.CompleteObjective` with the objective id). `Enabled` / `Open()` / `Close()`, `Interact()` opens it from code. `HasBeenRead` is not saved: the quest that needs it records it.
- **Interaction focus** (`scripts/Game/Systems/Interaction/`, namespace `TnT.EduGame.Interaction`): `Item` and `Readable` implement `IInteractable` and add themselves to the static `InteractionFocus` while the player is in their range. Only the nearest one that can be used now (the first to come in range on a tie) reacts to E, and one press is used by one interactable only. A readable's prompt shows only while it has the focus; an item's prompt also shows from afar, but hides while the player is in its range and another interactable has the focus. Key labels come from `InteractionFocus.KeyLabel` (the physical key on the headless display server).
- `ReadStep` (quest step): with `_readable` it waits for that thing's `Read` (closing early keeps waiting); with `_text` it opens the panel itself and waits until it is closed. Skipped on a catch-up run.
- UI: `MVC_Reading` (`ReadingController.Instance`, `ReadingModel` pages / `ReachedEnd`, `ReadingView` from `Scenes/UI/ReadingPanel.tscn`: dimmed game, an 820 x 560 sheet, title 34 px, body 26 px, page number and a hint line). `GameStateReading.Next()` / `Dismiss()` press next / close from code (tests).
- Test room: `Scenes/tests/reading_test.tscn` (a two-page notice and a one-page letter, `GameData/readables/test_*.tres`).

### 5f. Playground (since 2026-11-26)

`PlaythingSeat` (`scripts/Game/Systems/Playground/`, namespace `TnT.EduGame.Playground`; Area3D, `IInteractable`): a seat the player gets on with the interact key (E, through `InteractionFocus`). While the player is in its area, the "E" (`quest_prompt.tscn`) floats `PromptHeight` above `Seat`. E: the player is held (`CharacterStateManager.Hold`), walks to `Enter` (navigation agent, then straight, placed within 6 cm), turns to `Enter`'s +Z, and with movement off (`CharacterController3D.MovementEnabled = false`) jumps on an arc to `Seat * SitOffset` while `CharacterAnimator.Hold` plays `jump` → `fall` → `sit_down` (the tree goes on to `sit`). E again: `stand_up` (→ `idle`), a step back to `Enter`, then everything is released. One seat at a time; a hold replaced from elsewhere puts the player back at `Enter`. `Enter` / `Seat` are the GLB's `ENTER_*` / `SEAT_*` markers (`assets/3D/playground/README.md`). Scene: `Scenes/interactables/playground_swing.tscn` (two seats), used in `beaverford.tscn` `Playground/playground_swing`. With a `Swing` (`SwingPendulum`, since 2026-11-27: a pendulum in code on the seat pivot, no physics body and not the GLB clips): the model follows and tilts with the seat while the body stays put, the jump key pumps (once per half swing, up to `MaxAngle`), and E above `MinJumpOffAngle` jumps off forward on the next forward swing. While seated the E shows on the ground in front (`GetOffPromptOffset`), the pump key above the seat. Seesaw and merry-go-round have no seats yet.
- `sit_down`, `sit`, `stand_up`, `grab_onto` (KayKit Character Animations 1.1: Sit_Chair_Down / Idle / StandUp, Holding_A, from `assets/Animations/Source/Rig_Medium/Rig_Medium_Simulation.glb` / `_Tools.glb`; the pack itself is in `Edugame-Design/blend/sources/`) are in `character_base.tscn`'s library and state machine, so every Rig_Medium character has them.

### 6. Input Handling

**Centralized polling** through `InputManager` (autoloaded):
- Registers every node implementing `IInputActionable` (it lists its actions in `InputActions`): a tree scan in `_Ready`, then `SceneTree.NodeAdded` / `NodeRemoved` for nodes added or removed at runtime
- Polls all registered actions every frame. A poll that throws (e.g. an action naming an InputMap action that doesn't exist) is logged once and skipped (`InputManager.PollSafely`); the other actions are still polled

**`InputAction` / `InputAction2D` / `MultiInputAction` (Resources, base `InputActionBase`):**
- Unity-style properties: `Triggered`, `IsPressed`, `WasReleasedThisFrame`
- C# events `OnPressed` / `OnReleased` / `OnHeld`
- `Enabled` (off by default): a disabled action reads as not pressed. One bool per Resource (not counted), so each game state enables the actions it needs again in `OnResume`; Play enables its own on enter/resume and disables them on exit
- Maps to input action names in `project.godot`:
  - move_up, move_down, move_left, move_right (WASD/Arrows)
  - next (Space), jump (Space)
  - close (Escape), Pause (Escape, pause menu)
  - open (Tab, inventory), pickup (E, interact)

### 7. Persistence System (Generic Template Pattern)

**Core abstractions** in `scripts/Lib/Persistence/`:
- `SaveLoadSystem<T>` - Abstract base class generic over `GameData` type: `NewGame`, `SaveGame` (false, logged, when the file can't be written), `LoadGame(name)` / `ReloadGame()` (false, logged, nothing changed, when there is no readable save), `HasSave()` (the running game's name) / `HasSave(name)`, `MainGameName` (the game's own save name; the running game may use another, e.g. the dev save)
- `IDataService<T>` - File operations interface (`Save`, `TryLoad(name, out data)`, `Exists`, `Delete`, `DeleteAll`, `ListSaves`)
- `IBind<TData>` - Interface for saveable entities; they also implement `IUnique` (`PersistentId`)
- `ISaveable` - Interface for save data classes requiring `Id` and `IsNew`
- `FileDataService<T>` - JSON-based storage using Newtonsoft.Json in Godot's `user://` directory (`<name>.json`). **Atomic save:** written to `<name>.json.tmp`, then moved over the save, the previous save kept as `<name>.json.bak`; loading falls back to the `.bak` (with a warning) when the save is missing or can't be parsed
- `PersistentIds.SceneScoped(node)` - save key `"<current scene file>#<path relative to the current scene>"` (e.g. `res://Scenes/levels/level1.tscn#NavigationRegion3D/Livingroom/frontDoor`), unique across levels; read it at bind time, not in `_EnterTree`. Used by `Door`, `CodeDoor`, `MathChallengeTrigger`, `PlayerTutorial`, `Item`
- `GameData` and every save data class are **plain C# classes** (not Godot Resources, no Godot object fields), so the JSON holds only the game's own fields

**Implementation:**
- `SaveLoadManager` extends `SaveLoadSystem<MyGameData>`
- On scene load (`SceneLoaded`), binds characters, the tutorial, doors, code doors, challenges and world items, and the quests **last** (their `Bound` event runs the listeners' catch-ups, section 5), then calls `StartPlay`
- Entities implement `IBind<CharacterSaveData>` to restore state
- The inventory lives in the managers scene, not in a level: `SaveGame` copies it into `MyGameData.inventoryData`; `LoadGame` restores it (and the quests, `QuestManager.Restore`) before loading the level, `NewGame` clears both. `LoadGame` first checks that the saved level exists, so a broken save changes nothing
- Characters are saved **per level** (since 2026-11-22): the save key (`IUnique.PersistentId`) is `"<level scene path>#<PersistentId>"`, so a villager in two levels (Act I town `level2`, Act II town `level6`) keeps a separate position in each. Only `Character3D.SharedAcrossLevels` characters (the player, set in `Player_3D.tscn`) share one entry. `Character3D.PersistentId` stays the plain character id. Saves from before this lose the villagers' positions (they start at the scene's placement).
- Saves from before 2026-11-25 still load, but lose door open states, pending challenge seeds (scene-scoped keys since then) and the inventory.

### 8. Character System (Component-Based with Mediator)

**Key components:**
All in `scripts/Game/Character/` (namespace `TnT.EduGame.Characters`); the character states are in `scripts/Game/StateManagementCharacter/` (`TnT.EduGame.Characters.States`). There is no 2D character code.
- `Character3D` (CharacterBody3D, root of `Scenes/characters/character_base.tscn`) - Main logic component
  - Builds Stats and Attributes from its `CharacterData` (Resource) via the mediator pattern
  - Holds the `CharacterStateManager` (idle, move input, following via `NavigationAgent3D`, patrolling)
- `CharacterStateFollowing` keeps a distance (exports on its node in `character_base.tscn`): stops at `_followDistance` (1.5 m), walks again past `_resumeDistance` (2.25 m), eases its speed (`_acceleration`, slower within `_slowDownRange` of the stop, up to `_maxSpeedFactor` × the target's speed when behind). It sets the controller's `Speed` every frame and puts the original back on exit
- `CharacterStateManager` (an `AbstractStateStack`) - the base idle (never popped) with on top of it a timed idle, the player's move input, or **one** autonomous behaviour (patrol or follow). A new patrol/follow **replaces** a running one (the old one exits first and puts its speed and agent settings back) instead of stacking. Patrol and follow keep their per-push data in a session object. `PatrolAsync` returns `Task<bool>` that always completes when the patrol leaves the stack: `true` at the end of its route, `false` when it was stopped first (replaced, `PopStateStep`, unwound), never cancelled, so the quest reaction goes on. A waypoint it can't reach (the path ends short, or less than 0.25 m moved in 4 s: stuck) is skipped with a warning, so a non-looping patrol always ends. `IdleAsync(ms)` waits for its own idle, on a tree timer that stops while the game is paused. While a patrol or follow runs, idles and the player's move input are ignored
- `CharacterController3D` (implements `ICharacterController`) - Physics movement, jumping and rotation; owns the `Camera`. On pause (`NotificationPaused`) a character on the floor emits "idle" (`VisualRoot` and its AnimationTree run with process mode Always, so a paused "run" kept playing). `FaceTowards(worldPos, duration)` turns `VisualRoot` with a tween that also runs while paused; walking takes over. The jump is buffered for `_jumpBufferSeconds` (0.15 s): pressed in the air, it fires on landing within that time, else it is dropped; `MoveTo` clears it
- `CharacterAnimator` (implements `IMovementAnimator`) - Plays animations on movement state changes
- `Player` (in `Scenes/characters/Player_3D.tscn`) - Feeds input into its character; implements `IOcclusionTarget` for the `OcclusionFader`. Gets its input actions from `StateManagerGame.StartPlay` (`Player.SetInputActions(move, jump)`; no input until then); pushes its move input state when a key is held (also one pressed during a dialogue) and pops only that state. `Player.IsPlayer(body)` is the one check for areas that react to the player

**Stats/Attributes/Abilities** (`scripts/Game/Systems/StatModifiers/`, `scripts/Game/Character/Attributes/`, `scripts/Game/Systems/Abilities/`): **kept but unused.** Nothing in the game reads the stats, no `CharacterData` has abilities, and the ability triggers are never polled, so none of it is reachable (backlog card `idea-abilities-wiring-2026-11-25`). `Character3D` only ticks a mediator that has modifiers.
- `StatsMediator` - Processes stat modifiers dynamically (a modifier without a duration lasts until disposed)
- `AttributesMediator` - Manages derived attributes (health, stamina, etc.)
- Allows temporary buffs/debuffs via modifier pattern; an `Ability` keeps its uses and cooldown per character

### 9. Key Library Abstractions (`scripts/Lib/`)

**Extensions:**
- `NodeSearchExtensions` - Unity-style `FindObjectsByType<T>()`, `FindAnyObjectByType<T>()`, `FindAncestorOfType<T>()`
  - Every search walks the tree depth-first, in child order (no group fast path)
  - The `FindObjectsByType` / `FindAnyObjectByType` node overloads take a required `includeSelf` (`node.FindAnyObjectByType<T>(includeSelf: true)`: whether the start node itself is a candidate) and optional `recursive`; `FindAncestorOfType<T>(includeSelf = false)` has an optional `includeSelf` and no `recursive`; the `SceneTree` overloads search below the root window
- `VectorExtensions` - Vector math utilities (Snap, etc.; `ToVector2()` / `ToVector3()` map XY ↔ XY)
- `LinqExtensions` - Additional LINQ operations (Shuffle, PickRandom, GetPermutations, Rotate, Pad)

**Event System:**
- `EventChannel` (Resource) - Observable pattern implementation; listeners are called in registration order
- `EventListener` - Subscribes to EventChannels
- Used for quest updates and item pickups
- A listener is registered on its channel only while it is in the tree (`_EnterTree` / `_ExitTree`): channels are Resources and outlive the level

**Easings:**
- `Easings` - Async enumerable for smooth animations (`Easings.Animate(duration, ease)`), used by the challenge widgets (`Carrousel`, concentric rings). Menu and dialogue views use `UITweens` (section 3)

**OcclusionFader** (`TnT.Systems.OcclusionFade`, since the occlusion-1…6 cards of 2026-11-27): replaces the materials of the meshes, MultiMeshes and GridMaps in **its own scene** (not the autoloads) with occlusion materials that cut a hole around the `IOcclusionTarget` (the player). The cut (`occlusion.gdshaderinc`, macro `OCCLUSION_DISCARD`): a circle on screen around the player (`MaxRadius` m at the player's depth, same size at every depth), only on the camera's side of a vertical plane `CutOffFront` m in front of the player and above `CutOffHeight` over the feet; skipped in the shadow pass (cut walls keep their shadow). Each `BaseMaterial3D` gets a `ShaderMaterial` built at run time from `occlusion_material.gdshaderinc` for its features (one shader per feature combination; looks pixel-identical to the source); materials with unsupported features (triplanar, rim, ORM, ...) are kept with a warning; a `ShaderMaterial` source fades only if its shader opts in (`#include` + `OCCLUSION_DISCARD` inside `if (!IN_SHADOW_PASS)`). Double-sided sources darken back faces seen through the hole (`InteriorDarkness`). GridMaps get one converted MeshLibrary copy per library; MultiMeshes a `MaterialOverride`. Nodes added later (e.g. by a `DungeonPopulator`) are converted as they enter the tree. Skipped: render layers in `ExcludeLayers`, the `occlusion_exclude` group on the node or a parent, nodes under an `IOcclusionExempt` node. `Mode`: `Animated` opens the hole when converted geometry blocks the line from the camera to the player's middle (`OcclusionLineOfSight`: bounds + triangle BVH per mesh, no colliders; ~0.2 ms per frame in the town) and closes it `CloseDelay` s after the view is clear; `AlwaysOpen` (the forest) keeps it open. `[Tool]`: in the editor it only draws the cut region for the scene's player (`ShowInEditor`).

**`CleanQuit`** (`TnT.Systems`): `CleanQuit.Quit(GetTree())` frees the scene tree, runs the .NET finalizers while the engine is still up, then quits: a bare `GetTree().Quit()` can crash at exit after a longer session (a finalizer racing the engine shutdown). The Quit buttons use it, and `CleanQuit.Install` (from `StateManagerGame._Ready`) routes the window's close button through it. `--quit-after` still takes the engine's way.

## Coding Conventions and Patterns

### Namespacing
- `TnT.Systems.*` - Framework/library code in `scripts/Lib/` (e.g. `TnT.Systems.UIAnimation`, `TnT.Systems.Easing`, `TnT.Systems.State`, `TnT.Systems.Persistence`, `TnT.Systems.OcclusionFade`). **Lib never references `TnT.EduGame`**: when Lib needs something from the game, it defines an interface (e.g. `IOcclusionTarget`) or the game subscribes itself (e.g. `SaveLoadManager` subscribes to `SceneLoaded`)
- `TnT.EduGame.*` - Game-specific code in `scripts/Game/`, one namespace per folder: `.UI` (all `MVC_*`), `.Challenges`, `.Characters`, `.Characters.States`, `.GameStates`, `.Camera`, `.Levels`, `.Platforming`, `.Doors`, `.Dungeon`, `.Inventory`, `.QuestSystem`, `.Reading`, `.Dev`, ... Folder names that differ (`StateManagementGame` → `.GameStates`, `StateManagementCharacter` → `.Characters.States`, `InventorySystem` → `.Inventory`) stay as they are, because renaming folders breaks scene refs
- `TnT.Extensions` - Extension methods
- `TnT.Input` - Input system
- Not yet cleaned up: some files are still global or in bare `TnT.EduGame` (e.g. `Game/UI/Carrousel/` (global), `Game/Door.cs`, `Game/PlayerTutorial.cs`, `Game/Singletons/ManagerUI.cs`, `SaveLoadManager` / `MyGameData`, `Trail/GPUTrail3D.cs`, the stats/attributes/abilities code)

### Design Patterns in Use
1. **Singleton** - Managers expose static `Instance` property set in `_EnterTree()` or `_Ready()`
2. **Null Object** - `NoVisualization` for challenges without a visualization
3. **Chain of Responsibility** - Quest message processing
4. **Mediator** - Stats/Attributes system (kept but unused, section 8)
5. **MVC** - All UI systems
6. **Observer** - EventChannel/EventListener
7. **Template Method** - SaveLoadSystem abstract base
8. **Strategy** - Challenge visualizations/inputs (polymorphic Resources)

### Async/Await Usage
The codebase **heavily uses async/await** for:
- State transitions (OnEnter/OnExit/OnPause/OnResume in BaseState)
- UI animations (View classes, `UITweens`)
- Scene loading
- Always use `await` for state changes and animations
- **After every `await` in node code, check that the node is still there** (`IsInstanceValid(this)`, and `IsInsideTree()` where the scene may have changed) before touching it or its nodes, or cancel the work in `_ExitTree`: a level can be freed while a reaction, a spin or a path update is suspended (an `async void` would then throw `ObjectDisposedException`). In an `async void`, catch and log exceptions.
- **Wait with tree timers or signals, not `Task.Delay`:** `GetTree().CreateTimer(seconds, processAlways: false)` stops while the game is paused (`processAlways: true` for UI that runs while paused), or await the animation's own signal. `Task.Delay` ignores the pause (no `Task.Delay` is left in `scripts/`)

### Dual Event System
Code uses both **C# events** and **Godot Signals**:
- Signals for editor connections (inspector)
- C# events for code subscriptions
- Many classes expose both for the same event

### Resource-Based Configuration
Heavy use of **Godot Resources** (.tres files) for data-driven design:
- Quest definitions (`/GameData/quests/`)
- Challenge definitions (`/GameData/challenges/`)
- CharacterData, ItemData, etc.
- Allows content creation without code changes

### Reflection and Discovery
- `CallMethodStep` calls methods by name through reflection (`GodotReflectionUtils`): C# names, public methods only (section 5)
- `InputManager` discovers `IInputActionable` nodes as they enter the tree (section 6): be aware of initialization order when adding new discoverable types

## Working with Specific Systems

### Adding a New Challenge Visualization or Input
1. Create a Resource subclass of `ChallengeVisualization` (in `scripts/Game/Systems/Challenges/Visualizations/`) or `ChallengeInput` (in `.../Inputs/`) with `[GlobalClass, Tool]`, its settings as exports, and `DefaultScenePath`
2. Create the widget: a `ChallengeVisualizationWidget<YourVisualization>` / `ChallengeInputWidget<YourInput>` subclass that builds itself from `Settings`, `Challenge` and `Round` in `_Ready()`
3. Create the widget scene in `Scenes/UI/ChallengeVisualizations/` or `Scenes/UI/ChallengeInputs/` with the widget script on its root `Control`
4. Inputs: call `EmitValueAssigned` / `EmitValueCleared`; use `Round.Unknowns` and `Round.OptionsFor()`
5. Visualizations: render `Round.Givens`, hide unknowns; override `Generate()` to constrain values; implement `IChallengeInput` if the visualization is also the input
6. Pick the new type in the `Visualization` / `Input` field of a `MathChallenge`; no registration needed

### Adding a New Challenge
1. Create a `MathChallenge` resource, set `Formula` and press "Extract Params"
2. Configure each `ParamSpec` (fixed, pool or range), set `Unknowns`, `QuestionText`
3. Pick a `Visualization` (optional) and an `Input`. **Labels on a picture (`ImageAnnotation.Text`) are only a number with its unit** (`{b} kg`, `{c} t/min`, `+ {d} min`), never a sentence, a word in front of the number, or a question: what the number means goes in `QuestionText`. Long labels spill over the picture (`bug-image-labels-sentences`)
4. Reference it from a `MathChallengeTrigger` node

### Adding a New Quest
Full checklist: `project/workflows.md`. In short:
1. Create a `Quest` `.tres` in `/GameData/quests/` with its `QuestObjective`s, and register it on `QuestManager` in `managers.tscn`
2. In the level, add a `QuestNode` (with `_quest` set) under `Quests`
3. Wire signals to the `QuestNode` (`_OnPlayerStartObjective`, `CompleteObjective`, `FailObjective`, ...) with the objective id as extra call argument
4. Add `QuestEventListener`s under the `QuestNode` and give them step nodes as children for the reactions. An INPROGRESS reaction whose `ReactionEnd` completes its own objective gets `_resumeAfterLoad`
5. Start a quest from a trigger (an area the player walks into, armed by a listener; a `QuestStarter` with the NPC as `Giver` when an NPC gives the quest, so it starts on E), not from `_Ready`: `_Ready` runs during the scene load, before the save data and the quests are bound (a message pushed during a load waits below the loader and shows after the fade-out)

### Adding a New UI System
1. Create `MVC_[Feature]` directory in `scripts/Game/UI/`
2. Implement Controller, View, and Model classes
3. Controller mediates between Model (data/logic) and View (presentation)
4. View handles animations with `UITweens` (`Task<bool>`: change the control after the await only on `true`); release keyboard focus when hidden
5. Instantiate Controller in `managers.tscn` under `UIManager/CanvasLayer` (layer -1), or under its own `CanvasLayer` node under `UIManager` when it must draw above or below the others (reading 10, act card 20, fade 100; dev menu 128). Don't set a `CanvasLayer.Layer` from a view
6. Add Model and View as child nodes of Controller
7. A screen that pauses the game or takes input is a game state (next section)

### Adding a New Game State
1. Create class inheriting from `BaseGameState` in `scripts/Game/StateManagementGame/`, implementing `IStateObject<YourOptions>`
2. Implement `GetState(options)` returning a new configured `BaseState` per push (per-push data that two pushes must not share goes in a session object captured by its delegates, as in `GameStateMessage`)
3. Define OnEnter/OnExit async delegates for state lifecycle, and `OnResume` to re-assert what a state above may have undone (pause, input actions; `OnPause` if it must let go of something while covered)
4. OnUpdate delegate for frame-by-frame logic
5. ExitOnNextUpdate predicate for state transition conditions; to close it from outside, `Pop(thatState)`, never a plain `Pop()`
6. Add its node under `StateManagerGame` in `managers.tscn`: it auto-registers in `_Ready()`. Add a `StateManagerGame` method that pushes it through `PushState` (so it lands below a running loader). Never call `Push` directly: it is public on `AbstractStateStack`, but during a load it would put the state above the loader, on the black screen

### Working with Save Data
1. Define save data class implementing `ISaveable` with `Id` and `IsNew` properties: a **plain C# class** (no `Resource`, `[GlobalClass]`, `[Export]` or Godot object fields)
2. Add save data field to `MyGameData` class (and a `??= new()` in `LoadGame` for a list, for older saves)
3. Entities to be saved implement `IBind<YourSaveData>` and `IUnique` (`PersistentId`); a node in a level uses `PersistentIds.SceneScoped(this)` as a getter (read at bind time)
4. In `SaveLoadManager`, call `Bind<EntityType, SaveDataType>()` on scene load, before the quests (bound last)
   - List-type data (one entry per `PersistentId`: doors, challenges, world items, characters) is bound per subtree too: nodes added after the scene loaded (e.g. by a dungeon populator) call `SaveLoadManager.Instance.BindSubtree(root)` once in the tree; add new list types to `BindEntities`. Their node paths must be the same on every run. Binding twice adds nothing
5. Save data persists to `user://<name>.json` (with `.bak`); state that lives in the managers scene (like the inventory) is copied in by `SaveGame` and restored by `LoadGame` / `NewGame`
6. Saving is refused while an objective's quest reaction runs (`SaveLoadManager.CanSave`); code that saves checks the `bool` result

### Character and Movement
- Characters move through `CharacterController3D` (`ICharacterController`): `Move(Vector2 movement)` for input direction (turned by the body's global rotation), `MoveInWorld(Vector3 direction)` for a world-space direction (NPCs walking a navigation path; not turned by the body's rotation), `MoveTo(Vector3 position)` a **teleport** to a world position that stops the character (velocity, buffered jump, queued input and push dropped)
- The player's input goes through `Player` (actions from `StartPlay`); NPCs walk via their `CharacterStateManager` and `NavigationAgent3D` (patrol/follow use `MoveInWorld`)
- The facing turns `VisualRoot` toward the walk direction in its parent's space, keeping its scale
- The game camera is `scripts/Game/Camera/CameraLookAt.cs` on `Scenes/PlayerCamera.tscn`

## Important Notes

### Input System
- Never poll input directly in character scripts
- Always use `InputAction` resources registered with `InputManager`
- InputActions can be enabled/disabled dynamically for state-specific input

### Scene and Script Relationship
- Scripts attach to nodes in .tscn files via `script = ExtResource(...)`
- Managers scene composes all systems as node hierarchy
- UI Controllers instantiate View/Model as child nodes
- Resources (.tres) reference script classes via metadata
- Some scenes reference a script by path only, without a `uid` (e.g. the generated school challenge scenes: `ChallengeHazard`, `CrumblingPlatform`, `PushZone`, `CodeClue`). Move scripts through the editor's FileSystem dock, or `git mv` the `.cs` together with its `.cs.uid` and fix every `res://` path; then grep for leftover `res://scripts/...` strings. Also check `Edugame-Design/.work/scripts/` (its generators write script paths). Namespace renames are free: no scene, resource or save file stores a namespace

### Editor Tooling
- Use `[Tool]` attribute for scripts that run in editor
- `[ExportToolButton]` creates buttons in inspector
- `MathChallenge` "Extract Params" demonstrates the tool-button pattern
- `GPUTrail3D` (`scripts/Trail/`, `[Tool]`) keeps the materials saved with the node and builds them only when missing; at run time each trail copies its draw mesh and material. **A trail duplicated in the editor shares its saved mesh/material with the original** (the editor preview of both follows the last ramp): use "Make Unique" on `draw_pass_1` before giving the copy its own ramp/curve

### Common Pitfalls
1. **Singleton initialization order** - Most managers set `Instance` in `_EnterTree()`, some in `_Ready()` (`SaveLoadManager`, `InventoryController`, `MessageController`, `ChallengeController`); use another singleton from `_Ready()` cautiously
2. **State transitions** - The stack queues pushes and pops made during a transition; close a state with `Pop(thatState)` (or its `ExitOnNextUpdate`), never with a `Pop()` of whatever is on top
3. **Challenge widgets** - `Init()` runs before the widget enters the tree; build UI in `_Ready()`, not the constructor
4. **Async deadlocks** - Avoid blocking async calls; always await
5. **Async after free** - Check `IsInstanceValid` after every await in node code (see Async/Await Usage)
6. **Resource paths** - Use `res://` for project files, `user://` for save data
7. **Quitting** - Quit with `CleanQuit.Quit(GetTree())`, not `GetTree().Quit()`

## File Locations Reference

- State management framework: `scripts/Lib/StateManagement/`
- Game states implementation: `scripts/Game/StateManagementGame/`
- UI systems: `scripts/Game/UI/MVC_*/`
- Challenge system: `scripts/Game/Systems/Challenges/` (data), `scripts/Game/UI/MVC_Challenges/` (UI)
- Quest system: `scripts/Game/Systems/QuestSystem/`
- Character system: `scripts/Game/Character/`
- Persistence: `scripts/Lib/Persistence/`
- Input: `scripts/Lib/Input/`
- Extensions: `scripts/Lib/Extensions/`
- Managers scene: `Scenes/autoload/managers.tscn`
- Level scenes: `Scenes/levels/` (`level1` home, `level2_beaverford-in-the-morning` Act I town, `level3_wizard_*` chambers, `level5_forest`, `level6_beaverford-act2` Act II town, ...); test rooms in `Scenes/tests/`; start menu `Scenes/StartMenu.tscn`
