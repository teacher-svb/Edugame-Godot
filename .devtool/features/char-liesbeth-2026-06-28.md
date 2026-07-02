---
id: "char-liesbeth-2026-06-28"
status: "in-progress"
priority: "high"
assignee: null
epic: "characters"
dueDate: "2026-07-31"
created: "2026-06-28T12:00:00.000Z"
modified: "2026-06-29T08:44:15.099Z"
completedAt: null
labels: ["character", "villager", "world"]
order: "Zz8"
---
# Character: Liesbeth (baker)

**Role:** Villager | **First appears:** I.2 | **Location:** Bakery

Warm and anxious. The village depends on her bread. Trusts the Wizard completely but running out of patience. Mobile NPC — moves within the bakery.

**Build tasks:**

1. **Scene Setup**
   - [x] Create a new `.tscn` file that instances `Scenes/characters/character_base.tscn` as its root
   - [ ] Rename the root node to `"liesbeth"`
2. **Character Data**
   - [ ] Create `Character_Liesbeth_CharacterData.tres` in `GameData/characterData/`
   - [ ] Reference it via `_characterData` on the root node
   - [ ] Set `PersistentId` to `"liesbeth"`
3. **3D Model**
   - [x] Pick or import a GLB model from `assets/3D/Mini Characters 1/Models/GLB format/` (`character-female-d.glb`)
   - [x] Add it as a child node of the scene
   - [x] Set `CharacterController3D.VisualRoot` to `NodePath("../<model-node-name>")`
4. **Collision Shape**
   - [x] Add a `CapsuleShape3D` sub-resource with appropriate `radius` and `height`
   - [x] Apply it to `CollisionShape3D` with a transform offset (typically `y ≈ 0.35`)
5. **Animation Tree**
   - [x] Override `AnimationTree.root_node` → `../../<model-node-name>`
   - [x] Override `AnimationTree.anim_player` → `../../<model-node-name>/AnimationPlayer`
   - [ ] Animations needed: idle, walk (within bakery)
6. **Dialogue**
   - [ ] Set up dialogue nodes for I.2, II.3, III.2/III.3 (Ghost + Night Shift), V.1/V.2 (Conversion + Lesson)