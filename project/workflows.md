# Workflows

Checklists and step-by-step guides for common development tasks.

---

## Creating a New NPC

1. **Scene Setup**
  - [ ] Create a new `.tscn` file that instances `Scenes/characters/character_base.tscn` as its root
  - [ ] Rename the root node to the NPC's name (e.g., `"mabel"`)
2. **Character Data**
  - [ ] Create a `Character_<name>_CharacterData.tres` in `GameData/characterData/`
  - [ ] Reference it via `_characterData` on the root node
  - [ ] Set `PersistentId` to a unique string matching the NPC name (e.g., `"mabel"`)
3. **3D Model**
  - [ ] Pick or import a GLB model from `assets/3D/Mini Characters 1/Models/GLB format/`
  - [ ] Add it as a child node of the scene (e.g., `character-female-c`)
  - [ ] Set `CharacterController3D.VisualRoot` to `NodePath("../<model-node-name>")`
4. **Collision Shape**
  - [ ] Add a `CapsuleShape3D` sub-resource with appropriate `radius` and `height`
  - [ ] Apply it to `CollisionShape3D` with a transform offset (typically `y ≈ 0.35`)
5. **Animation Tree**
  - [ ] Override `AnimationTree.root_node` → `../../<model-node-name>`
  - [ ] Override `AnimationTree.anim_player` → `../../<model-node-name>/AnimationPlayer`
6. **Closeup Camera**
  - [ ] Put 3D model meshes on layer 20
  - [ ] Move Closeup Camera to frame the face of the character

---

## Creating a New Quest

1. **Scene Quest Node Setup**
  - [ ] Add the Quest skeleton structure to the scene:
```
scene root/
└── Quests (type: Node)/
    └── <ActNum>_<ChapterNum>-<QuestName> (type: QuestNode, _quest = the quest resource)/
        └── <objective>-<state>-<what> (type: QuestEventListener)/
            ├── <Step> (type: IdleStep, PatrolStep, ...)
            └── ...
```
2. **Quest Resource**
  - [ ] Create a new Quest Resource `<actNum>.<chapterNum>-<questName>.tres` in `res://GameData/quests/`
  - [ ] Set ID to new GUID, set last 2 characters to 00
3. **Register to QuestManager**
  - [ ] Add quest to `Managers/QuestManager:Quests` in `res://Scenes/autoload/managers.tscn`
4. **Quest Objectives**
  - [ ] Add Quest Objectives to the Quest Resource objectives array
  - [ ] Add a start and complete text to each objective
  - [ ] Link the relevant characterdata to each objective
  - [ ] Set objective ID to Quest ID, last 2 characters as counter (01 for first objective, 02 for second, ...)
  - [ ] *optional* Add a progress text to each objective
5. **Quest Triggers**
  - [ ] Wire a signal to the `QuestNode` to start the first objective and to complete each objective (including the first). Pass the objective ID as extra call argument (Connect dialog → Advanced → Add Extra Call Argument). For example:
    - Player enters an area: `Area3D.body_entered` → `_OnPlayerStartObjective` / `_OnPlayerCompleteObjective`
    - Chained after a prior reaction finishes: `QuestEventListener.ReactionEnd` → `CompleteObjective`
    - Gated on item pickup: `ItemEventListener.OnListen` → `CompleteObjective` (set Unbind Signal Arguments to 1)
    - Challenge: `MathChallengeTrigger.ChallengeCompleted` → `CompleteObjective`, `ChallengeFailed` → `FailObjective`
  - Only the current objective in the expected state accepts a message; anything else logs a `QuestManager: ... ignored` warning
6. **Quest Reactions**
  - For each object/character that should react to an objective changing state
    - [ ] Add a `QuestEventListener` node under the `QuestNode`
      - [ ] Set `Event Channel` to `res://GameData/EventChannels/QuestEventChannel.tres`
      - [ ] Set `_checkObjectiveId` true
      - [ ] Set `_objectiveId` to the objective's ID
      - [ ] Set `_checkState` true, `_state` to the target `QuestState` (NOTSTARTED=0, INPROGRESS=1, COMPLETED=2, FAILED=3)
    - [ ] *optional* Add step nodes as children of the listener; they run top to bottom, each waiting for the previous one (drag to reorder):
      - `IdleStep`, `PatrolStep`, `FollowStep`, `PopStateStep`, `ChallengeStep`
      - `CallMethodStep` for any other method (awaited if it returns a `Task`)
    - [ ] *optional* Link the `ReactionEnd` Signal (fires after the last step) to the object/character
