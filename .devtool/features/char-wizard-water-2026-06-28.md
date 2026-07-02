---
id: "char-wizard-water-2026-06-28"
status: "in-progress"
priority: "high"
assignee: null
epic: "characters"
dueDate: "2026-07-31"
created: "2026-06-28T12:00:00.000Z"
modified: "2026-07-02T13:28:53.903Z"
completedAt: null
labels: ["character", "wizard", "world"]
order: "Zz1"
---
# Character: Water Wizard (elemental)

**Role:** NPC | **Appears:** I.3 only | **Location:** Wizard School — Water chamber

Presides over the Water trial. Tests Mabel, observes she produces no spark, sends her on.

**Build tasks:**

1. **Scene Setup**
   - [ ] Create a new `.tscn` file that instances `Scenes/characters/character_base.tscn` as its root
   - [ ] Rename the root node to `"wizard-water"`
2. **Character Data**
   - [ ] Create `Character_WizardWater_CharacterData.tres` in `GameData/characterData/`
   - [ ] Reference it via `_characterData` on the root node
   - [ ] Set `PersistentId` to `"wizard-water"`
3. **3D Model**
   - [ ] Pick or import a water-themed GLB model from `assets/3D/Mini Characters 1/Models/GLB format/`
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
   - [ ] Set up dialogue nodes for I.3 Water trial sequence