---
id: "char-gamma-2026-06-28"
status: "backlog"
priority: "medium"
assignee: null
epic: "characters"
dueDate: "2026-09-30"
created: "2026-06-28T12:00:00.000Z"
modified: "2026-06-28T12:00:00.000Z"
completedAt: null
labels: ["character", "guild", "world"]
order: "chr-14"
---
# Character: Gamma (Guild mentor — Knowledge)

**Role:** Guild mentor | **First appears:** III.1 | **Location:** Guild Workshop

Discipline: knowledge / documents. Provides access to blueprints and texts required as prerequisites for math challenges. Unlocks reading material before challenges that depend on it. Personality trait TBD.

**Build tasks:**

1. **Scene Setup**
   - [ ] Create a new `.tscn` file that instances `Scenes/characters/character_base.tscn` as its root
   - [ ] Rename the root node to `"gamma"`
2. **Character Data**
   - [ ] Create `Character_Gamma_CharacterData.tres` in `GameData/characterData/`
   - [ ] Reference it via `_characterData` on the root node
   - [ ] Set `PersistentId` to `"gamma"`
3. **3D Model**
   - [ ] Pick or import a GLB model from `assets/3D/Mini Characters 1/Models/GLB format/`
   - [ ] Add it as a child node of the scene
   - [ ] Set `CharacterController3D.VisualRoot` to `NodePath("../<model-node-name>")`
4. **Collision Shape**
   - [ ] Add a `CapsuleShape3D` sub-resource with appropriate `radius` and `height`
   - [ ] Apply it to `CollisionShape3D` with a transform offset (typically `y ≈ 0.35`)
5. **Animation Tree**
   - [ ] Override `AnimationTree.root_node` → `../../<model-node-name>`
   - [ ] Override `AnimationTree.anim_player` → `../../<model-node-name>/AnimationPlayer`
   - [ ] Animations needed: idle, walk (within workshop)
6. **Dialogue**
   - [ ] Set up dialogue nodes for III.1 introduction and blueprint-unlock interactions throughout Act III–V
