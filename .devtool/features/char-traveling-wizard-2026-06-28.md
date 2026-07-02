---
id: "char-traveling-wizard-2026-06-28"
status: "backlog"
priority: "medium"
assignee: null
epic: "characters"
dueDate: "2026-08-31"
created: "2026-06-28T12:00:00.000Z"
modified: "2026-06-28T12:00:00.000Z"
completedAt: null
labels: ["character", "wizard", "world"]
order: "chr-11"
---
# Character: Traveling Wizard

**Role:** Antagonist (Act II) | **Appears:** II.4 only | **Location:** Village (public square)

A traveling wizard who hears the rumors about Mabel. Tests her publicly, confirms she produces no mana, and declares her a fraud. Catalyst for Mabel's retreat and the Guild's letter.

**Build tasks:**

1. **Scene Setup**
   - [ ] Create a new `.tscn` file that instances `Scenes/characters/character_base.tscn` as its root
   - [ ] Rename the root node to `"traveling-wizard"`
2. **Character Data**
   - [ ] Create `Character_TravelingWizard_CharacterData.tres` in `GameData/characterData/`
   - [ ] Reference it via `_characterData` on the root node
   - [ ] Set `PersistentId` to `"traveling-wizard"`
3. **3D Model**
   - [ ] Pick or import a GLB model from `assets/3D/Mini Characters 1/Models/GLB format/` (distinct from elemental wizards and the Wizard)
   - [ ] Add it as a child node of the scene
   - [ ] Set `CharacterController3D.VisualRoot` to `NodePath("../<model-node-name>")`
4. **Collision Shape**
   - [ ] Add a `CapsuleShape3D` sub-resource with appropriate `radius` and `height`
   - [ ] Apply it to `CollisionShape3D` with a transform offset (typically `y ≈ 0.35`)
5. **Animation Tree**
   - [ ] Override `AnimationTree.root_node` → `../../<model-node-name>`
   - [ ] Override `AnimationTree.anim_player` → `../../<model-node-name>/AnimationPlayer`
   - [ ] Animations needed: idle, walk (arrives in square)
6. **Dialogue**
   - [ ] Set up dialogue nodes for II.4 Exposure sequence
