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
/GameData/quests/     - Quest resource files (.tres)
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
  - `GameStateActCard` - Card between acts (`StateManagerGame.ShowActCard(title, subtitle, onDismissed)`): pauses, fades to black, closes on next/close after `MinSeconds`
  - `GameStateReading` - Reading panel for a longer text (`StateManagerGame.ShowReading(readableText, onClosed(readToEnd))`): pauses; next turns the page and closes on the last one, close closes on any page (see 5e)

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
- MVC_ActCard - Card between acts ("Einde van Akte I")
- MVC_Reading - Reading panel for letters and notices (R2)

Controllers are instantiated in `managers.tscn` with Model and View as child nodes.

### 4. Challenge System (Visualization × Input)

A challenge is split into **what is asked** (math), **how it is shown** (visualization) and **how the player answers** (input). Visualizations and inputs combine freely.

**Data** (`scripts/Game/Systems/Challenges/`):
- `MathChallenge` (Resource) - `Formula` (expression; its result is the implicit `answer`), `Params` (one `ParamSpec` per formula param), `Unknowns` (params the player supplies, may include `answer`), `QuestionText`/`HintText` (`{param}` placeholders), `Constraints`, `Visualization`, `Input`. "Extract Params" tool button syncs `Params` with the formula.
- Constraints: `MathChallenge.Constraints` is a list of rules, true/false expressions in the formula's own language: `answer >= 0`, `x > y`, `x % y = 0`, `answer % 10 = 0`, `x % 10 + y % 10 < 10` (no tens crossing), combined with `AND` / `OR` / `NOT`, plus the shorthand `distinct(x, y, z)`. `FormulaEvaluator` expands, validates (unknown names, bad syntax, not a comparison → clear error before rolling) and evaluates them. `ChallengeRound.Roll` rerolls until all hold; `OptionsFor` only offers distractors that keep them true. Visualizations add their own via `ImpliedConstraints()` (radar range, grid snap) as rule strings. Answers are always whole numbers (values are `int`); `Check()` only tests the formula. No rule means negative answers are allowed. Resources with param fields derive from `ParamAwareResource` (visualizations, `GraphBar`). In the inspector, a visualization's param fields (e.g. `RadarVisualization.DistanceParam`) show a dropdown of the formula's params: `MathChallenge` passes the names down (`IParamFieldOwner`) whenever `Params` or `Visualization` is assigned.
- `ParamSpec` (Resource) - how a param is rolled: fixed value → `Pool` → `[Min, Max]` range
- `ChallengeRound` - one rolled instance: every value incl. `answer`, deterministic per seed. `Check()` re-evaluates the formula (any valid solution counts), `OptionsFor()` gives input options (pool or correct value + distractors), `Format()` fills placeholders (unknowns as `?`), `RandomFor(purpose)` gives separate replayable random streams from the seed (rolling, options, visualizations). Pure C#; parsing/evaluating formulas lives in `FormulaEvaluator` (`System.Data.DataTable`).
- `ChallengeVisualization` / `ChallengeInput` (abstract Resources, shared base `ChallengeElement`) - subclasses carry their own settings and a default scene (`DefaultScenePath`; the `Scene` export overrides it). `ChallengeVisualization.Generate()` is virtual so a visualization can constrain the rolled values. An empty `Visualization` field means `NoVisualization` (Null Object, via `MathChallenge.VisualizationOrNone`), so nothing needs a null check.
  - Visualizations: `TextVisualization`, `ImageVisualization` (picture + `ImageAnnotation`s at normalized positions with `{param}` text), `SearchGridVisualization` (labelled coordinate grid, optional map background, marker snapped to line crossings), `BarGraphVisualization` (one `GraphBar` per param on a labelled value axis; ranged bar params are narrowed to gridline values before rolling; an unknown bar shows as `?`), `LineGraphVisualization` (`Line` expression in a variable, e.g. `v - r * t`, clipped to the axes; optional `MarkX` point that must land on the grid), `RadarVisualization` (distance rings + blip at a param's distance; rerolls until it fits the range), `ConcentricCirclesVisualization` (rings outer → inner spell the formula; optional `Operators` pool randomizes operators per round), `KeyVisualization` (a `BarGraphVisualization` drawn as a key, one tooth per bar, depth 0..9; its `KeyWidget` doubles as input: the player cuts the unknown teeth; e.g. `GameData/challenges/key_code_4.tres`, place value `a*1000 + b*100 + c*10 + d`) `SequenceVisualization` (number series in a row of boxes with one `?`: `Term` an expression in `i` (term index from 0) over the params, `TermCount` and `MissingTerm` expressions too (negative = from the end), `MissingValue` the param the gap stands for; `Generate` checks that the missing term equals it, which is a check and not a rule, so dropdown distractors still work; e.g. `GameData/challenges/sequence_bell_times.tres`: formula `s + n * d`, Term `s + i * d`, `n + 1` terms, the last missing) (in `Visualizations/`)
  - Inputs: `TextChallengeInput`, `DropdownInput`, `CombinationLockInput`, `CogwheelInput` (in `Inputs/`)

**UI** (`scripts/Game/UI/MVC_Challenges/`): `ChallengeUI` has three slots: visualization (left), question text (right), input (bottom centre). `ChallengeUI.ShowRound(challenge, round)` fills them. Widgets derive from `ChallengeVisualizationWidget<TSettings>` / `ChallengeInputWidget<TSettings>` (or the non-generic bases when they need no settings) and get the challenge, round and their resource as typed `Settings` in `Init()` before `_Ready()`; a widget given the wrong resource type fails with a clear message. Inputs report `ValueAssigned(param, value)` / `ValueCleared(param)`; a visualization widget implementing `IChallengeInput` doubles as input and the `Input` slot is skipped. Widget scenes live in `Scenes/UI/ChallengeVisualizations/` and `Scenes/UI/ChallengeInputs/`.

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
- `QuestEventListener` - Filters objective updates (id/state) and runs its child `QuestStep` nodes top to bottom, each awaited (Observer + Command). Steps (in `QuestSystem/Steps/`) hold their receiver and arguments as typed exports: `IdleStep`, `PatrolStep`, `FollowStep`, `PopStateStep`, `ChallengeStep`, `MessageStep` (a character's line, awaited until dismissed; skipped on a catch-up run, which has no objective), `ActCardStep` (the card between acts; skipped on a catch-up run), `SaveGameStep` (autosave; skipped on a catch-up run), plus generic `CallMethodStep` (any method, awaited if it returns a `Task`). A step throwing `OperationCanceledException` stops the remaining steps. New step: subclass `QuestStep` with `[Tool, GlobalClass]`, implement `Task Run(QuestObjective)`.
- `QuestsCompletedListener` - Runs its child steps once all listed quests are completed (also checked on `_Ready`, for quests finished in another scene, and on `QuestManager.Bound`, because loading a save restores the quest states after `_Ready` without quest events; that catch-up run passes no objective and must not show anything); wire `ReactionEnd` to open up the next quest, e.g. enable its trigger `Area3D` (`set_deferred("monitoring", true)`). These are the arrows in the GDD quest diagram. Both listeners share `QuestStepListener` (runs the steps, `ReactionStart`/`ReactionEnd`)
- `QuestNode` - Quest root in a level; connect signals (e.g. `Area3D.body_entered` → `_OnPlayerStartObjective`) with the objective id as an extra call argument. Ignored quest messages (wrong objective/state) log a warning.

Quest resources are stored as `.tres` files in `/GameData/quests/`.

Quest states: NOTSTARTED, INPROGRESS, COMPLETED, FAILED

### 5b. Platforming Challenges (wizard school chambers)

Components in `scripts/Game/Systems/Platforming/` (namespace `TnT.EduGame.Platforming`):
- `ChamberSafety` (one per platforming level, `Instance`) - remembers the last safe GridMap cell the player stood on (item name starts with `room_` or `corr_pad_`) and respawns the player there after falling below `KillY` or a hazard hit (`Hit()`, with a short grace period)
- `ChallengeHazard` (Area3D) - while `Active` (keyed by animations), a player inside is hit
- `PushZone` (Area3D) - while `Active`, pushes the player back along its local Z, away from its centre, via `CharacterController3D.AddPush()` (sources add a push every physics frame)
- `CrumblingPlatform` (AnimatableBody3D) - shakes when the player lands on its `Sensor`, falls, comes back after `ResetTime`

**Dungeon tile names** (school and mana mine meshlibs, since 2026-11-21): the prefix is the godoodungeon tile set. `room_*` = room tile set, with `room_door` = the dungeon's **Door** item (school 54, mine 12); `corr_*` = corridor tile set (`corr_pad_*` bridge pads, `corr_chasm*` / `corr_rock*` the empty tile). A hand-built room (Built room box) uses only `room_*` tiles, `room_door` on the wall ring. Item numbers are kept from the old names, so the school meshlib has gaps (the godoowave-only tiles were removed); the export JSON `renamed_2026_11_21` maps old → new names.

The element challenge bridges (MeshLibrary items 55-61, `corr_<element>_<challenge>`) carry marker scenes `challenges/corr_<element>_<challenge>.tscn` in `assets/3D/village/interiors/school_interior/`, placed by godoodungeon. Element chambers `Scenes/levels/level3_wizard_<fire|water|earth>.tscn` use `school_tiles_<element>.meshlib` and `GameData/wfc/school_dungeon_{rooms,corridors}_<element>.tres` (godoodungeon: the tile sets must use the GridMap's MeshLibrary). These assets are generated by scripts in `Edugame-Design/.work/scripts/school_interior_scripts/` (`godot_element_meshlibs.gd`, `godot_challenge_markers.gd`) and `.../rotatable_tiles_game_scripts/` (`godot_meshlibs.gd`, `game_tilesets.py`).

**Key doors** (since 2026-11-21, placeholder geometry): locked doors are placed by the game, not by the tile set (godoodungeon has no lock feature; don't add one to the addon). `CodeDoor` (`scripts/Game/Systems/Doors/`) runs its `MathChallengeTrigger` when the player walks into its `Trigger`; solved, its leaves swing into the room and the open state is saved in `doorData` (`DoorSaveData`, bound in `SaveLoadManager`). `assets/3D/village/interiors/school_interior/doors/room_door_key.tscn` = key door (challenge `GameData/challenges/key_code_4.tres`, `KeyVisualization`), N-S build: stands on a `room_door` cell turned with it (outside to the north). `CodeClue` (`doors/code_clue_board.tscn`) shows `Text` (default `{answer}`) from its `Source` trigger's round, so door and clue share the trigger's `Seed`. Test room: `Scenes/tests/key_lock_test.tscn`. Placement in the dungeon chambers: root kanban cards `dungeon-pop-*`.

**Dungeon planning** (`scripts/Game/Systems/Dungeon/`, namespace `TnT.EduGame.Dungeon`, since 2026-11-21): pure C# over godoodungeon's `DungeonGraphData` (`DungeonGraph.StoredOn(gridMap).CoreGraph`), never inside the addon. `RoutePlanner.Plan(graph, roomRoles, seed)` → `DungeonRoute`: start and goal (a room box's `Data["role"]` = `start` / `goal`, else the graph's diameter ends, the seed picking which is the start), the main path (`ShortestPath`), and per room a `RoomInfo` (Start / Goal / Path / Side, the path room it hangs off, depth, hops from the start, dead end). Encounters: `DungeonEncounter` (abstract Resource, Strategy; `Pass` Gate/Filler, `Plan(EncounterContext, EncounterTarget)`) plans against `EncounterContext` (Builder: rooms by `RoomFilter`, free cells and doors handed out once, `RandomFor(purpose)` streams, collected `Placement`s). `RoomFilter` Resources (Specification): `RoleFilter`, `DeadEndFilter`, `HopsFromStartFilter`, `AllOfFilter` / `AnyOfFilter` / `NotFilter`. `RoomCellPicker.FreeCells`: the room's footprint (walls included: a wall tile is mostly floor) minus its door cells and the cell inside each door, filtered by the caller's free-floor check. `LockEncounter` (gate: `DoorScene`, `ClueScene`, `ClueCount`, `CluePlacement` PathRoomBefore / SideRoomBefore) locks the next path room's door of a path corridor (a shared door cell closes all its corridors), prefers locks that cut the goal off, and places its clues before it with the lock's seed; `ObjectEncounter` (filler: `Scene`) takes a free cell in a room its filter accepts. `DungeonRecipe` (Resource, one per chamber in `GameData/dungeon/`): `Slots` (`EncounterSlot`: an encounter at Anywhere / PathStep (negative = from the goal) / StartRoom / GoalRoom), a weighted `Pool`, budgets `Gates` and `Fillers`; slots first, then pool draws. `DungeonPopulator` (Node3D in the level: `Map`, `Recipe`, `Seed` 0 = the dungeon's seed, `FreeFloorTiles`, `Player`) plans in `_Ready` from the graph stored on the GridMap (`Populate(graph)` for a runtime generator; room roles come from the room box's live `Data["role"]`, falling back to the graph's copy, which Generate only refreshes), spawns under `RoomNN/Lock_n` / `Clue_n` / `Object_n`, sets the door's challenge `Seed` and the clue's `Source`, puts the player in the start room and calls `BindSubtree`. Spawned scenes get their transform before entering the tree (a door's `AnimatableBody3D` leaves don't follow a parent moved afterwards); doors turn -Z to the door direction, clues and objects face +Z (the camera). Used in all four chambers (`level3_wizard_1` / `_fire` / `_water` / `_earth`), each with its own `GameData/dungeon/<chamber>_recipe.tres` (for now all the same: a key lock into the goal room + a pooled key lock, 2 gates). A chamber without a stored graph (generated before godoodungeon 1.1) only gets a warning: Generate it again. Test level: `level3_wizard_earth.tscn`.

**Goal rooms** (since 2026-11-22, placeholder geometry): each chamber has a hand-built `GoalRoom` (Node3D) with the `SparkTestPlatform` at its origin, the element's wizard at (1.3, 0, -2.2) and `ExitToNext` (`goal_portal.tscn`, a closed `LevelExit` whose portal `Visual` shows only while open) at (0, 0, -3.6). `DungeonPopulator.GoalRoom` moves it onto the `GoalAnchor` cell and turns its +Z out through the entrance (`GoalAnchorPicker`: the plain-floor cell (`GoalFloorTiles`) of the goal room farthest from the entrance door, ways in last, no door behind it); that cell and the one behind are reserved (`EncounterContext.Reserve`), so no lock, clue or object lands there. Built in the level rather than spawned so the quest (`wzrd2`-`wzrd4`) can wire to it in the editor. `SparkTestPlatform` (`scripts/Game/Systems/School/`, Area3D): the player stepping on it plays a fizzle (sparks, dying glow, keeps playing while a message pauses the game) and emits `Tested` (connect to `QuestNode.CompleteObjective` with the objective id); again after stepping off. Scenes: `assets/3D/village/interiors/school_interior/goal/`. Exits: `_1` → Fire → Water → Earth → forest (default spawn); the quest opens them. I.3 quest wiring per chamber (Fire, Water, Earth since 2026-11-22, `GameData/quests/I.3-WizardSchoolTrials-<El>.tres`; the Earth wizard gives the verdict, its exit leads to the forest): `StartSpawn/StartArea` (moves with the start spawn) starts obj1, `GoalRoom/ArrivalArea` (just in front of the platform) completes it, `SparkTestPlatform.Tested` completes obj2, `Quests/I_3-<El>/obj3-init-SendOn` (wizard idles) completes obj3, and `Quests/completed-OpenExit` (`QuestsCompletedListener` over the chamber's quest) opens `ExitToNext`, also after loading.

**Tile sets are one MeshLibrary item per shape, turned in the GridMap** (since 2026-11-20; school, forest, mana mine): every WfcTile is Rotation FourWay, and a tile's other directions are the same item at GridMap orientation 16 / 10 / 22 (base north turned to west / south / east). Marker scenes exist only in their N-S build; the generators place them turned with their cell. Each set's export JSON (`replaces`, `remap_old_items`) maps the old one-tile-per-rotation items.

### 5c. Level Exits and Spawn Points

Components in `scripts/Game/Systems/Levels/` (namespace `TnT.EduGame.Levels`, since 2026-11-21):
- `LevelExit` (Area3D) - the player walking in loads `TargetScene` and arrives at its spawn point `TargetSpawn`: a node path relative to the target scene's root (empty = the level's default one), With **no `TargetScene`** it teleports within the current level instead (`StateManagerGame.LoadLocation`, with the fade; usable again once the player has left it). Picked in the inspector from the **Spawn Point** dropdown, which lists the spawn points saved in `TargetScene` (or, without one, those in the edited scene) (`SpawnPoint.ListIn`, read from its `SceneState`; not those inside instanced sub-scenes). Saved as a string (`_targetSpawnPath`), not a `NodePath`: the editor rewrites `NodePath` properties when a node is moved or renamed. A path that no longer exists shows as `(missing)` with a configuration warning, and at run time falls back to the default spawn. `Enabled` / `Open()` / `Close()`: a quest opens a closed exit with a `CallMethodStep` under a `QuestsCompletedListener` (which also catches up after a scene load or a loaded save, so the exit needs no save data of its own). A player who arrives inside an exit is ignored until they step out. Signal `Used`.
- `SpawnPoint` (Marker3D) - `Default` (one per level). Renaming or moving a spawn point breaks the exits that point to it (they show a warning). Keep spawn points out of exit areas.
- A new scene's `uid://` (e.g. a `LevelExit.TargetScene`) is only known to the game after the editor has scanned the project: open the editor once, or headless `godot --headless --editor --quit`. Until then loading it fails with "Unrecognized UID" and the loading screen hangs. `SceneLoaded` passes the path as given (often the `uid://`), not the `res://` path.
- `StateManagerGame.LoadScene(path, NodePath spawnPath)` puts the player at the spawn point **after** the save data is bound. Without that, the position saved in the previous level wins, because the player's `CharacterSaveData` is shared by all levels. `LoadScene(path, Vector3)` (load game, `Transporter`) keeps the saved position.
- Chambers: `DungeonPopulator.StartSpawn` moves the level's default spawn point to the start room.
- Act I route: home `level1` (`Spawns/front_door`) ↔ town `level2_beaverford-in-the-morning` (`Spawns/from_home`; `ExitToSchool` at the wizard tower door, opened by `Quests/I_2-completed-OpenSchool` once the four I.2 quests are done, after Mabel's line) → `level3_wizard_fire` (`StartSpawn`, the default). Forest `level5_forest`: `from_school` at the west exit, `ExitToHome` at the east exit → home. `WorldBounds` (StaticBody3D, 4 `WorldBoundaryShape3D` planes facing inwards on the edge of the 23 x 28 cell grid, x -44..48, z 0..112: no height or thickness to tunnel through) keeps the player on the map; the south exits run into it, so their `LevelExit` areas must stay inside the grid. I.4 (`GameData/quests/I.4-TheHeavyHeart.tres`, since 2026-11-22): `Spawns/from_school/I4_Arrival` starts obj1, `I4_PassSpring` (a 3 m band across the only way from the west to the east half, x 0.5..3.5, z 52..76, where the spur to the spring branches off) completes it; home `Spawns/front_door/I4_ArriveHome` completes obj2 (`Quests/I_4-The Heavy Heart` in `level1`). **End of Act I:** `level1` `Quests/I4-completed-EndOfActI` (`QuestsCompletedListener` over I.4) → `ActCardStep` ("Einde van Akte I" / "Welkom in Beaverford") → `SaveGameStep`. Act II starts from "I.4 completed": give the II.1 quest a `QuestsCompletedListener` over `I.4-TheHeavyHeart.tres` (no separate save flag). The chamber exits to the next level are `GoalRoom/ExitToNext` (closed until the chamber's I.3 quest opens it), see Goal rooms in 5b.
- `GuardedGate` (Node3D, since 2026-11-22) - a way guards keep closed: `Guards` stand in front of a `Block` (e.g. a StaticBody3D barrier), and walking into `BarkArea` shows `BarkText` from `BarkCharacter` (again after stepping out). `Unguard()` removes guards, block and bark area for good (from a `CallMethodStep` under a `QuestsCompletedListener`, which also catches up after loading, so no save data). Forest: `ManaSpringGate` at the mana mine entrance (`mine_C` cell (0, 9), world (2, 0, 38); tunnel mouth z ≈ 38.2), two `Scenes/characters/mana_guard.tscn` (Knight) at (0.6 / 3.4, 0, 39.8) facing south, a placeholder wooden barricade across the mouth, bark "Halt! Geen toegang tot de bron. Bevel van de tovenaar." Unguarded in IV.2 (`engi10`).
- Act II town: `Scenes/levels/level6_beaverford-act2.tscn` (since 2026-11-22), a separate level from the Act I town (`level2` is all Act I quest wiring). Same `beaverford.tscn`, morning environment and music; villagers under `Villagers/` (Cornelis on the moat-house side of the raised drawbridge at (27, 0.3, 8.5); Hendrik, Liesbeth and Mevrouw Bram at their Act I spots by the mill, bakery and school), `Markers/TownSquare` (by the noticeboard, for II.4) and `Markers/DrawbridgeSouthBank`, an empty `Quests` node, `Spawns/from_home` (default), `Exits/ExitToHome` (closed; opened at the end of II.4 by `wzrd05`, → home `Spawns/front_door`). **Act II route** (since 2026-11-22): home `level1` has two front-door exits on the same spot, `transporters/transporter_livingroom_level2` (→ Act I town) and `transporters/transporter_livingroom_act2` (→ Act II town `Spawns/from_home`, closed); `Quests/I4-completed-TownIsActII` (`QuestsCompletedListener` over I.4, also catches up after loading) closes the first and opens the second. **II.5** (`GameData/quests/II.5-TheMysteriousLetter.tres`, since 2026-11-23, in `level1`): `Quests/II4-completed-StayHome` (`QuestsCompletedListener` over II.4, placed after `I4-completed-TownIsActII` so its catch-up runs later) closes `transporter_livingroom_act2` (Mabel stays home; it stays closed after II.5: the way out is Act III's), shows `Letter` (paper with a gear seal on the dining table at (10, 0.77, -7.5); the `Livingroom` node is offset by (2, 0, -5), so its props' local positions are not world positions) and opens `Letter/Readable` (at the table's free west end, `GameData/readables/II.5-guild_letter.tres`, 3 pages, Letter look; it sends her to the old well on the square tonight, after twelve strokes of the school bell, to look for the gear sign on its rim), and arms `II5_TableArea` (3.5 x 2.8 m around the table, kept clear of `Spawns/front_door` so arriving home doesn't start the quest during the load). The table area starts obj1 (Mabel finds the letter; completes right away), `Letter/Readable.Read` completes obj2. **End of Act II** (`act2-4`, since 2026-11-23): `level1` `Quests/II5-completed-EndOfActII` (`QuestsCompletedListener` over II.5) → `ActCardStep` ("Einde van Akte II" / "Toveren zonder toverstaf") → `SaveGameStep`. Act III starts from "II.5 completed", like Act II from "I.4 completed". `Drawbridge` (`AnimatedProp`, `scripts/Game/Systems/Levels/`: holds an `AnimationPlayer` at the end or start of `StartAnimation` in `_Ready`; quests call `Play(anim)` (awaited until finished) and `SetPose(anim, atEnd)` through `CallMethodStep`, which is why both take a plain `string`: `CallMethodStep` can't convert a string arg to `StringName`) holds the bridge raised (`Drawbridge_Raise` end); it sits before `Quests`, so a quest's catch-up after loading wins. **II.1** (`GameData/quests/II.1-TheDesperateHelp-Cornelis.tres`, since 2026-11-22): `Quests/I4-completed-CornelisCalls` arms `CornelisShoutArea` (16 x 10 m south of the moat, starts obj1: Cornelis shouts), `CornelisBankArea` (at `Markers/DrawbridgeSouthBank`) completes obj1; obj2 (Mabel) runs the challenge (`Quests/II_1-The Desperate Help/MathChallengeTrigger`: `b + s - c`, drawbridge picture with bridge, supplies and counterweight labels, `CombinationLockInput`); obj3 (Cornelis) plays `Drawbridge_Lower` and his amazed line (`MessageStep`), then completes. `Quests/II1-completed-BridgeDown` puts the bridge down again after loading (`SetPose`). The drawbridge's static collider has ramps at the deck ends (since 2026-11-22, `Edugame-Design` README "Ramps on the collider"), so characters without step-up walk over the lowered bridge. The town's navmesh was baked with the bridge half raised and doesn't cross the moat: `DrawbridgeNavLink` (`NavigationLink3D`, island (27, 0, 10.2) ↔ south bank (27, 0, 23.6), off) is switched on when II.1 is completed. **II.2** (`GameData/quests/II.2-TheRumorMill.tres`, since 2026-11-22): `Quests/II1-completed-CornelisToSquare` (`CallMethodStep` `set_Enabled` on `DrawbridgeNavLink`, then `PatrolStep` with one waypoint `Markers/CornelisRoute/2_Square`: Cornelis walks over the bridge to the square at 2 m/s (his `_speed`, like dad), about 15–20 s; on a catch-up he already stands there) and `Quests/II1-completed-ArmSquare` (arms `RumorSquareArea`, 8 x 8 m by the noticeboard, kept separate so a stuck walk can't block II.2). obj1 (Cornelis) starts in the square: reactions from Hendrik, Liesbeth, Mevrouw Bram (`MessageStep`s, the villagers stay at their workplaces); obj2 (Mabel: "het was gewoon rekenen"): their requests for help, which lead into II.3. Cornelis' `NavigationAgent3D` is overridden in `level6` (`path_height_offset` 0, path/target distance 0.4): with `character_base`'s 0.75 offset an agent whose origin is at its feet never reaches a path point (backlog `bug-npc-nav-height-offset`). **II.3 Liesbeth** (`GameData/quests/II.3-TheBusyWeek-Liesbeth.tres`, since 2026-11-23; one of the three Busy Week jobs, any order): `Quests/II2-completed-ArmBakery` arms `LiesbethBakeryArea` (6 x 6 m around the bakery, starts obj1: Liesbeth asks), `LiesbethScalesArea` (2.5 x 2.5 m at Liesbeth) completes obj1; obj2 (Mabel) runs the challenge (`Quests/II_3-The Busy Week-Liesbeth/MathChallengeTrigger`: `a * b - c`, a 300–500 g flour per loaf, b 2–6 loaves, c 100–1000 g already on the scale, all flour in steps of 50, `answer > 0`; `scales-diagram.png` with three labels, `TextChallengeInput`); obj3 (Liesbeth) says the scales balance again plus one `MessageStep`, then completes. **II.3 Hendrik** (`GameData/quests/II.3-TheBusyWeek-Hendrik.tres`, since 2026-11-23, same pattern): `Quests/II2-completed-ArmMill` arms `HendrikMillArea` (6 x 6 m around Hendrik on the mill platform), `HendrikMillstoneArea` (2.5 x 2.5 m at Hendrik) completes obj1; the challenge (`Quests/II_3-The Busy Week-Hendrik/MathChallengeTrigger`) is `a * b - c` on `ConcentricCirclesVisualization` with `Operators` [+, -, *], so each round picks its own operators (a 2–12, b 2–9, c 2–15, `answer > 0`; the hint is about × before + and −), `TextChallengeInput`. **II.3 Mevrouw Bram** (`GameData/quests/II.3-TheBusyWeek-MsBram.tres`, since 2026-11-23, same pattern): `Quests/II2-completed-ArmSchool` arms `MsBramSchoolArea` (6 x 6 m in front of the school, on her +z side), `MsBramBellArea` (2.5 x 2.5 m at her) completes obj1; the challenge is the shared `GameData/challenges/sequence_bell_times.tres` (`SequenceVisualization` of the bell times in minutes past 8:00, last one missing, `DropdownInput`). All three II.3 jobs are armed by their own `II2-completed-Arm*` listener. **Busy Week gate** (`act2-3`, since 2026-11-23): `Quests/II3-completed-WizardArrives` (`QuestsCompletedListener` over the three II.3 quests) shows Mabel's line (live only), reveals the traveling wizard (`Villagers/TravelingWizard`, waiting hidden at (0, 0.4, 32) on the road with `collision_layer` 0, so he stands on the floor but the player walks through him; revealed with `Show` and `set_CollisionLayer` 2) and the `SparkTestPlatform` (-3.5, 0, 6; `Show`, `set_Monitoring`), then walks the wizard, Hendrik and Liesbeth to `Markers/Exposure/*` in the square (`PatrolStep`, not awaited; about 13 / 10 / 6 s). Mevrouw Bram stays at the school: her porch is a navmesh island, and the stream bridge (-10, 0, -22) has a deck step NPCs can't climb (backlog `world-stream-bridge-ramps`). Hendrik, Liesbeth and the wizard have the same `NavigationAgent3D` overrides as Cornelis. `Quests/II3-completed-ArmExposure` arms `ExposureSquareArea` (8 x 8 m at the square) on its own. **II.4** (`GameData/quests/II.4-TheExposure.tres`, since 2026-11-23): the square area starts obj1 (the wizard; Cornelis cheers, then obj1 completes); obj2 (Mabel) is completed by `SparkTestPlatform.Tested` (the Act I fizzle); obj3 (the verdict: Hendrik, Liesbeth, Mevrouw Bram, the wizard) shows `Noticeboard/Paper` and opens `Noticeboard/Notice` (a `Readable` 1 m in front of the board at (-5.5, 0, -0.5), `GameData/readables/II.4-public_notice.tres`, 2 pages, Notice look), then completes; obj4 (Mabel) is completed by `Notice.Read`. `Quests/II4-completed-OpenExitHome` opens `Exits/ExitToHome` and keeps the notice up after loading. **Note:** `CallMethodStep` calls C# methods by reflection: use `Show`, `set_Monitoring`, `set_CollisionLayer` (C# names), not Godot's `show` / `set_deferred`.
- In-level teleports: home bedroom ↔ living room (`transporters/transporter_bedroom-livingroom` → `Spawns/from_bedroom`, back → `Spawns/from_livingroom`). `scripts/test/Transporter.cs` is now only used by the guild workshop's placeholder `transporter_exit_village` (no target set yet; make it a `LevelExit` once the village scene exists).

### 5d. Dev Shortcuts (debug builds only)

`scripts/Game/Dev/` (namespace `TnT.EduGame.Dev`, since 2026-11-22): jump to a checkpoint instead of playing up to it. `DevShortcuts` (node in `managers.tscn`, frees itself in a release export) holds a list of `DevCheckpoint` resources (`GameData/dev/NN_<id>.tres`: `Id`, `Title`, `CompletedQuests`, `Scene`, `Spawn`, `TutorialDone`).
- **Backtick** `` ` `` (physical key under Esc) opens a menu of the checkpoints while nothing else is open (no dialogue, challenge, card): number keys 1-9, arrows + Enter or a click pick one; Escape or `` ` `` closes it. **Not F9/F10:** a game run from the editor treats those as the editor's "Suspend/Resume Embedded Project" and "Next Frame" shortcuts, even in its own window, and freezes.
- **Launch argument** `--checkpoint=<id>`: on the command line after `--` (`godot --path . -- --checkpoint=ii2`), or in the editor under Project Settings > Editor > Run > Main Run Args as `-- --checkpoint=ii2`. It jumps right after the start menu appears.
- A jump builds a fresh `MyGameData` (only the listed quests completed, every objective; tutorial done), sets the quests in memory to match, and loads `Scene` at `Spawn`. Everything else comes from the levels' `QuestsCompletedListener` catch-up, like loading a save. Nothing is written to disk until the game saves. Setting the in-memory quests matters: listeners check them on `_Ready`, before the save data is bound.
- Checkpoints: `act2-start` (Act I done; Act II town at `Spawns/bridge_approach`, on the road just outside `CornelisShoutArea`: spawning inside it would start II.1 during the load), `ii2` (+ II.1) and `ii3` (+ II.2), both at `Spawns/bridge` on the south bank, 3 m east of the deck (x 30.5): in the walking line, Cornelis (walking to the square after II.1) runs into the player and climbs them. `ii4` (+ the three II.3 jobs) at `Spawns/square_south` (-6, 0.3, 10), just south of the square: the wizard walks in and the square area is armed. `ii5` (+ II.4) at home, `level1` `Spawns/front_door`: the letter is on the table. New one: a `.tres` in `GameData/dev/`, added to `DevShortcuts.Checkpoints`.
- Dev tooling: reads the backtick key directly and builds its own small UI, outside `InputManager` and MVC.

### 5e. Reading (R2, since 2026-11-23)

Longer texts (a letter, a notice) are read in a panel, not in dialogue boxes. `scripts/Game/Systems/Reading/` (namespace `TnT.EduGame.Reading`):
- `ReadableText` (Resource, `GameData/readables/`): `Title`, `Body` (paragraphs, BBCode; a line with only `---` starts a new page), `Look` (Plain / Letter / Notice: paper and ink colours).
- `Readable` (Area3D; reusable scene `Scenes/interactables/readable.tscn`: 2.5 m range, `Prompt` with "lees" + the interact key, shown only while the player is in range): the interact key (`pickup`, E, the same as items) opens its `Text`. Signals `Opened`, `Closed(readToEnd)`, `Read` (closed after reaching the last page; connect it to `QuestNode.CompleteObjective` with the objective id). `Enabled` / `Open()` / `Close()`, `Interact()` opens it from code. `HasBeenRead` is not saved: the quest that needs it records it.
- `ReadStep` (quest step): with `_readable` it waits for that thing's `Read` (closing early keeps waiting); with `_text` it opens the panel itself and waits until it is closed. Skipped on a catch-up run.
- UI: `MVC_Reading` (`ReadingController.Instance`, `ReadingModel` pages / `ReachedEnd`, `ReadingView` from `Scenes/UI/ReadingPanel.tscn`: dimmed game, an 820 x 560 sheet, title 34 px, body 26 px, page number and a hint line). `GameStateReading.Next()` / `Dismiss()` press next / close from code (tests).
- Test room: `Scenes/tests/reading_test.tscn` (a two-page notice and a one-page letter, `GameData/readables/test_*.tres`).

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
- Characters are saved **per level** (since 2026-11-22): the save key (`IUnique.PersistentId`) is `"<level scene path>#<PersistentId>"`, so a villager in two levels (Act I town `level2`, Act II town `level6`) keeps a separate position in each. Only `Character3D.SharedAcrossLevels` characters (the player, set in `Player_3D.tscn`) share one entry. `Character3D.PersistentId` stays the plain character id. Saves from before this lose the villagers' positions (they start at the scene's placement).

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
- A listener is registered on its channel only while it is in the tree (`_EnterTree` / `_ExitTree`): channels are Resources and outlive the level

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
2. **Null Object** - `NoVisualization` for challenges without a visualization
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
- Quest definitions (`/GameData/quests/`)
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
2. Create the widget: a `ChallengeVisualizationWidget<YourVisualization>` / `ChallengeInputWidget<YourInput>` subclass that builds itself from `Settings`, `Challenge` and `Round` in `_Ready()`
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
Full checklist: `project/workflows.md`. In short:
1. Create a `Quest` `.tres` in `/GameData/quests/` with its `QuestObjective`s, and register it on `QuestManager` in `managers.tscn`
2. In the level, add a `QuestNode` (with `_quest` set) under `Quests`
3. Wire signals to the `QuestNode` (`_OnPlayerStartObjective`, `CompleteObjective`, `FailObjective`, ...) with the objective id as extra call argument
4. Add `QuestEventListener`s under the `QuestNode` and give them step nodes as children for the reactions
5. Never start a quest from `_Ready`: it runs mid scene load, and the quest message it pushes breaks the loading screen

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
   - List-type data (one entry per `PersistentId`: doors, challenges, characters) is bound per subtree too: nodes added after the scene loaded (e.g. by a dungeon populator) call `SaveLoadManager.Instance.BindSubtree(root)` once in the tree; add new list types to `BindEntities`. Their node paths must be the same on every run. Binding twice adds nothing
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
