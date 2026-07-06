---
id: "char-wizard-water-2026-06-28"
status: "done"
priority: "high"
assignee: null
epic: "characters"
dueDate: "2026-07-31"
created: "2026-06-28T12:00:00.000Z"
modified: "2026-07-03T09:30:25.461Z"
completedAt: "2026-07-03T09:30:25.461Z"
labels: ["character", "wizard", "world"]
order: "a2"
---
# Character: Water Wizard (elemental)

**Role:** NPC | **Appears:** I.3 only | **Location:** Wizard School — Water chamber

Presides over the Water trial. Tests Mabel, observes she produces no spark, sends her on.

**Build tasks:**

1. **Scene Setup**
   - \[x\] Create a new `.tscn` file that instances `Scenes/characters/character_base.tscn` as its root
   - \[x\] Rename the root node to `"wizard-water"` (still `"Character"`)
2. **Character Data**
   - \[x\] Create `Character_WizardWater_CharacterData.tres` in `GameData/characterData/` (`WaterWizard_CharacterData.tres`)
   - \[x\] Reference it via `_characterData` on the root node
   - \[x\] Set `PersistentId` to `"wizard-water"` (not set)
3. **3D Model**
   - \[x\] Pick or import a water-themed GLB model from `assets/3D/Mini Characters 1/Models/GLB format/` (`Mage.glb`)
   - \[x\] Add it as a child node of the scene
   - \[x\] Set `CharacterController3D.VisualRoot` to `NodePath("../<model-node-name>")`
4. **Collision Shape**
   - \[x\] Add a `CapsuleShape3D` sub-resource with appropriate `radius` and `height`
   - \[x\] Apply it to `CollisionShape3D` with a transform offset (typically `y ≈ 0.35`)
5. **Animation Tree**
   - \[x\] Override `AnimationTree.root_node` → `../../<model-node-name>`
   - \[x\] Override `AnimationTree.anim_player` → `../../<model-node-name>/AnimationPlayer`
   - \[x\] Animations needed: idle (stationary on platform)
6. **Closeup Camera**
   - \[x\] Put 3D model meshes on layer 20
   - \[x\] Move Closeup Camera to frame the face of the character (no override in scene, uses base default)

## Files

- GameData/characterData/WaterWizard_CharacterData.tres
- Scenes/characters/wizard_water.tscn