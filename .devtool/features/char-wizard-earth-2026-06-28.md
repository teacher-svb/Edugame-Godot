---
id: "char-wizard-earth-2026-06-28"
status: "todo"
priority: "high"
assignee: null
epic: "characters"
dueDate: "2026-07-31"
created: "2026-06-28T12:00:00.000Z"
modified: "2026-06-28T12:00:00.000Z"
completedAt: null
labels: ["character", "wizard", "world"]
order: "chr-10"
---
# Character: Earth Wizard (elemental)

**Role:** NPC | **Appears:** I.3 only | **Location:** Wizard School — Earth chamber

Delivers the final verdict: no mana. Mabel is not admitted to Wizard School.

**Build tasks:**

1. **Scene Setup**
   - [ ] Create a new `.tscn` file that instances `Scenes/characters/character_base.tscn` as its root
   - [ ] Rename the root node to `"wizard-earth"`
2. **Character Data**
   - [ ] Create `Character_WizardEarth_CharacterData.tres` in `GameData/characterData/`
   - [ ] Reference it via `_characterData` on the root node
   - [ ] Set `PersistentId` to `"wizard-earth"`
3. **3D Model**
   - [ ] Pick or import an earth/stone-themed GLB model from `assets/3D/Mini Characters 1/Models/GLB format/`
   - [ ] Add it as a child node of the scene
   - [ ] Set `CharacterController3D.VisualRoot` to `NodePath("../<model-node-name>")`
4. **Collision Shape**
   - [ ] Add a `CapsuleShape3D` sub-resource with appropriate `radius` and `height`
   - [ ] Apply it to `CollisionShape3D` with a transform offset (typically `y ≈ 0.35`)
5. **Animation Tree**
   - [ ] Override `AnimationTree.root_node` → `../../<model-node-name>`
   - [ ] Override `AnimationTree.anim_player` → `../../<model-node-name>/AnimationPlayer`
   - [ ] Animations needed: idle (stationary on platform)
6. **Dialogue**
   - [ ] Set up dialogue nodes for I.3 Earth trial verdict sequence
