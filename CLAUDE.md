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
The project uses Godot.NET.Sdk/4.7.0 and requires .NET 8.0.

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
/quests/              - Quest resource files (.tres)
/themes/              - UI theme resources
```

## Core Architecture

### 1. Stack-Based State Management System

The game uses a **stack-based state machine** as its architectural foundation:

- **`StateManagerGame`** (autoloaded) - Global state coordinator that inherits from `AbstractStateStack`
- States can be pushed/popped like a navigation stack, maintaining state history
- Five main game states:
  - `GameStatePlay` - Normal gameplay
  - `GameStateChallenge` - Math challenge overlay
  - `GameStateMessage` - Character dialogue/notifications
  - `GameStateInventory` - Inventory screen
  - `GameStateLoadingScreen` - Scene transitions

**Key abstractions** in `scripts/Lib/StateManagement/`:
- `AbstractStateManager` - Manages state transitions with lifecycle (TransitionIn/TransitionOut)
- `AbstractStateStack` - Extends manager with Push/Pop semantics
- `BaseState` - Encapsulates state behavior with OnEnter/OnExit/OnUpdate delegates
- `IStateObject<T>` - Factory pattern interface for state creation

### 2. Autoload/Singleton Pattern

The `Managers` scene (`Scenes/autoload/managers.tscn`) is autoloaded and contains all core systems as child nodes:
- `StateManagerGame` - State coordinator
- `SaveLoadManager` - Persistence system
- `InputManager` - Centralized input handling
- `QuestManager` - Quest orchestrator
- `Inventory` - Player inventory container
- `UI` (CanvasLayer) - All UI controllers

Most managers expose a static `Instance` property for global access.

### 3. MVC Pattern for All UI Systems

**Every UI subsystem** follows strict Model-View-Controller pattern in `scripts/Game/UI/MVC_*`:

```
MVC_[Feature]/
  ├── Controller/[Feature]Controller.cs  - Mediates between Model and View
  ├── View/[Feature]View.cs              - Visual presentation and animations
  └── Model/[Feature]Model.cs            - Business logic and data
```

Implemented systems:
- MVC_Challenges - Math challenge UI
- MVC_Messages - Notification/dialogue
- MVC_Inventory - Inventory UI
- MVC_Fade - Screen transitions
- MVC_CharacterSelection - Character picker
- MVC_ActionsMenu - In-game action menus

Controllers are instantiated in `managers.tscn` with Model and View as child nodes.

### 4. Challenge System (Visualization × Input)

A challenge is split into **what is asked** (math), **how it is shown** (visualization) and **how the player answers** (input). Visualizations and inputs combine freely.

**Data** (`scripts/Game/Systems/Challenges/`):
- `MathChallenge` (Resource) - `Formula` (expression; its result is the implicit `answer`), `Params` (one `ParamSpec` per formula param), `Unknowns` (params the player supplies, may include `answer`), `QuestionText`/`HintText` (`{param}` placeholders), `AllowNegativeAnswer`, `DistinctValues`, `Visualization`, `Input`. "Extract Params" tool button syncs `Params` with the formula.
- `ParamSpec` (Resource) - how a param is rolled: fixed value → `Pool` → `[Min, Max]` range
- `ChallengeRound` - one rolled instance: every value incl. `answer`, deterministic per seed. `Check()` re-evaluates the formula (any valid solution counts), `OptionsFor()` gives input options (pool or correct value + distractors), `Format()` fills placeholders (unknowns as `?`). Pure C#, expressions via `System.Data.DataTable`.
- `ChallengeVisualization` / `ChallengeInput` (abstract Resources) - subclasses carry their own settings and a default scene (`Scene` export overrides it). `ChallengeVisualization.Generate()` is virtual so a visualization can constrain the rolled values.
  - Visualizations: `TextVisualization` (in `Visualizations/`)
  - Inputs: `TextChallengeInput`, `DropdownInput`, `CombinationLockInput`, `CogwheelInput` (in `Inputs/`)

**UI** (`scripts/Game/UI/MVC_Challenges/`): `ChallengeUI` has three slots: visualization (left), question text (right), input (bottom centre). `ChallengeUIFactory` builds them via `ChallengeUI.Builder`. Widgets derive from `ChallengeVisualizationWidget` / `ChallengeInputWidget` and get the challenge + round in `Init()` before `_Ready()`. Inputs report `ValueAssigned(param, value)` / `ValueCleared(param)`; a visualization widget implementing `IChallengeInput` doubles as input and the `Input` slot is skipped. Widget scenes live in `Scenes/UI/ChallengeVisualizations/` and `Scenes/UI/ChallengeInputs/`.

**Flow:** `MathChallengeTrigger` → `StateManagerGame.ShowChallenge(challenge, seed)` → `ChallengeModel` rolls the round → `Answered(bool)` signal. The trigger keeps its seed (persisted as `ChallengeSaveData`) until the challenge is solved, so retries show the same values.

Challenge resources are stored as `.tres` files in `/GameData/challenges/` or inline in level scenes.

### 5. Quest System (Chain of Responsibility + Event-Driven)

**Architecture:**
- `Quest` (Resource) - Contains array of `QuestObjective`, manages progression
- `QuestManager` - Processes quest messages through chain of responsibility:
  - `QuestProcessor<QuestMessageStart>` - NOTSTARTED → INPROGRESS
  - `QuestProcessor<QuestMessageComplete>` - INPROGRESS → COMPLETED
  - `QuestProcessor<QuestMessageFail>` - INPROGRESS → NOTSTARTED
- `QuestEventChannel` - Broadcasts events when objectives update, connected to `StateManagerGame.ShowQuestMessage()`

Quest resources are stored as `.tres` files in `/quests/`.

Quest states: NOTSTARTED, INPROGRESS, COMPLETED, FAILED

### 6. Input Handling

**Centralized polling** through `InputManager` (autoloaded):
- Scans scene tree for `InputAction` resources via reflection
- Polls all registered actions every frame
- Automatically tracks nodes added/removed at runtime

**`InputAction` (Resource):**
- Unity-style properties: `Triggered`, `IsPressed`, `WasReleasedThisFrame`
- Emits both Godot Signals and C# Events
- Maps to input action names in `project.godot`:
  - move_up, move_down, move_left, move_right (WASD/Arrows)
  - next (Space)
  - close (Escape)

### 7. Persistence System (Generic Template Pattern)

**Core abstractions** in `scripts/Lib/Persistence/`:
- `SaveLoadSystem<T>` - Abstract base class generic over `GameData` type
- `IDataService<T>` - File operations interface
- `IBind<TData>` - Interface for saveable entities requiring `UniqueId` property
- `ISaveable` - Interface for save data classes requiring `Id` and `IsNew`
- `FileDataService<T>` - JSON-based storage using Newtonsoft.Json, saves to Godot's `user://` directory

**Implementation:**
- `SaveLoadManager` extends `SaveLoadSystem<MyGameData>`
- On scene load, binds Character, Quest, and Door data
- Entities implement `IBind<CharacterSaveData>` to restore state

### 8. Character System (Component-Based with Mediator)

**Key components:**
- `Character` - Main logic component attached to `CharacterController2D`
  - Loads `CharacterData` (Resource) by ID
  - Manages Stats and Attributes via mediator pattern
  - Processes abilities via input actions
- `CharacterController2D` - Movement controller (CharacterBody2D)
  - Grid-based tile movement with interpolation
  - Raycast collision detection
- `CharacterAgent2D` - AI pathfinding component (separate from player)

**Stats/Attributes System:**
- `StatsMediator` - Processes stat modifiers dynamically
- `AttributeMediator` - Manages derived attributes (health, stamina, etc.)
- Allows temporary buffs/debuffs via modifier pattern

### 9. Key Library Abstractions (`scripts/Lib/`)

**Extensions:**
- `NodeSearchExtensions` - Unity-style `FindObjectsByType<T>()`, `FindAnyObjectByType<T>()`
  - Optimized with group-based fast path
  - Supports recursive and ancestor searches
- `VectorExtensions` - Vector math utilities (Snap, etc.)
- `LinqExtensions` - Additional LINQ operations (Shuffle, PickRandom, GetPermutations)

**Event System:**
- `EventChannel` (Resource) - Observable pattern implementation
- `EventListener` - Subscribes to EventChannels
- Used for quest updates and UI notifications

**Easings:**
- `Easings` - Async enumerable for smooth animations
- Used in UI View classes for fade in/out effects

## Coding Conventions and Patterns

### Namespacing
- `TnT.Systems.*` - Framework/library code
- `TnT.EduGame.*` - Game-specific code
- `TnT.Extensions` - Extension methods
- `TnT.Input` - Input system

### Design Patterns in Use
1. **Singleton** - Managers expose static `Instance` property set in `_EnterTree()` or `_Ready()`
2. **Factory + Builder** - Challenge UI system
3. **Chain of Responsibility** - Quest message processing
4. **Mediator** - Stats/Attributes system
5. **MVC** - All UI systems
6. **Observer** - EventChannel/EventListener
7. **Template Method** - SaveLoadSystem abstract base
8. **Strategy** - Challenge visualizations/inputs (polymorphic Resources)

### Async/Await Usage
The codebase **heavily uses async/await** for:
- State transitions (OnEnter/OnExit in BaseState)
- UI animations (View classes)
- Scene loading
- Always use `await` for state changes and animations

### Dual Event System
Code uses both **C# events** and **Godot Signals**:
- Signals for editor connections (inspector)
- C# events for code subscriptions
- Many classes expose both for the same event

### Resource-Based Configuration
Heavy use of **Godot Resources** (.tres files) for data-driven design:
- Quest definitions (`/quests/`)
- Challenge definitions (`/GameData/challenges/`)
- CharacterData, ItemData, etc.
- Allows content creation without code changes

### Reflection-Based Discovery
Runtime reflection is used in:
- `InputManager` - Discovers InputAction resources in scene tree
- Be aware of initialization order when adding new discoverable types

## Working with Specific Systems

### Adding a New Challenge Visualization or Input
1. Create a Resource subclass of `ChallengeVisualization` (in `scripts/Game/Systems/Challenges/Visualizations/`) or `ChallengeInput` (in `.../Inputs/`) with `[GlobalClass, Tool]`, its settings as exports, and `DefaultScenePath`
2. Create the widget: a `ChallengeVisualizationWidget` / `ChallengeInputWidget` subclass that builds itself from `Challenge` + `Round` in `_Ready()`
3. Create the widget scene in `Scenes/UI/ChallengeVisualizations/` or `Scenes/UI/ChallengeInputs/` with the widget script on its root `Control`
4. Inputs: call `EmitValueAssigned` / `EmitValueCleared`; use `Round.Unknowns` and `Round.OptionsFor()`
5. Visualizations: render `Round.Givens`, hide unknowns; override `Generate()` to constrain values; implement `IChallengeInput` if the visualization is also the input
6. Pick the new type in the `Visualization` / `Input` field of a `MathChallenge`; no registration needed

### Adding a New Challenge
1. Create a `MathChallenge` resource, set `Formula` and press "Extract Params"
2. Configure each `ParamSpec` (fixed, pool or range), set `Unknowns`, `QuestionText`
3. Pick a `Visualization` (optional) and an `Input`
4. Reference it from a `MathChallengeTrigger` node

### Adding a New Quest
1. Create `.tres` file in `/quests/` of type `Quest`
2. Define quest objectives as array of `QuestObjective`
3. Quest states transition through message processors:
   - Send `QuestMessageStart` to begin quest
   - Send `QuestMessageComplete` to complete
   - Send `QuestMessageFail` to fail
4. Subscribe to `QuestEventChannel` for UI updates

### Adding a New UI System
1. Create `MVC_[Feature]` directory in `scripts/Game/UI/`
2. Implement Controller, View, and Model classes
3. Controller mediates between Model (data/logic) and View (presentation)
4. View handles animations using `Easings` for smooth effects
5. Instantiate Controller in `managers.tscn` under UI CanvasLayer
6. Add Model and View as child nodes of Controller

### Adding a New Game State
1. Create class inheriting from `BaseGameState` in `scripts/Game/StateManagementGame/`
2. Implement `CreateState()` method returning configured `BaseState`
3. Define OnEnter/OnExit async delegates for state lifecycle
4. OnUpdate delegate for frame-by-frame logic
5. ExitOnNextUpdate predicate for state transition conditions
6. Auto-registers with `StateManagerGame` in `_Ready()`

### Working with Save Data
1. Define save data class implementing `ISaveable` with `Id` and `IsNew` properties
2. Add save data field to `MyGameData` class
3. Entities to be saved implement `IBind<YourSaveData>` with `UniqueId` property
4. In `SaveLoadManager`, call `Bind<EntityType, SaveDataType>()` on scene load
5. Save data persists to `user://` directory as JSON

### Character and Movement
- Player character uses `CharacterController2D` for grid-based movement
- Movement is tile-based with smooth interpolation
- Use `Move(Vector2I direction)` for relative movement
- Use `MoveTo(Vector2I targetTile)` for absolute positioning
- NPCs use `CharacterAgent2D` for pathfinding with NavigationAgent2D

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

### Editor Tooling
- Use `[Tool]` attribute for scripts that run in editor
- `[ExportToolButton]` creates buttons in inspector
- `MathChallenge` "Extract Params" demonstrates the tool-button pattern

### Common Pitfalls
1. **Singleton initialization order** - Managers initialize in `_EnterTree()`, use `_Ready()` cautiously
2. **State transitions** - Never transition states while another transition is in progress
3. **Challenge widgets** - `Init()` runs before the widget enters the tree; build UI in `_Ready()`, not the constructor
4. **Async deadlocks** - Avoid blocking async calls; always await
5. **Resource paths** - Use `res://` for project files, `user://` for save data

## File Locations Reference

- State management framework: `scripts/Lib/StateManagement/`
- Game states implementation: `scripts/Game/StateManagementGame/`
- UI systems: `scripts/Game/UI/MVC_*/`
- Challenge system: `scripts/Game/Systems/Challenges/` (data), `scripts/Game/UI/MVC_Challenges/` (UI)
- Quest system: `scripts/Game/Quests/`
- Character system: `scripts/Game/Character/`
- Persistence: `scripts/Lib/Persistence/`
- Input: `scripts/Lib/Input/`
- Extensions: `scripts/Lib/Extensions/`
- Managers scene: `Scenes/autoload/managers.tscn`
- Level scenes: `Scenes/Level1.tscn`, `Scenes/Level2.tscn`
