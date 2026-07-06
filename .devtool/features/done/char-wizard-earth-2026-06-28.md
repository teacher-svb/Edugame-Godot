---
id: "char-wizard-earth-2026-06-28"
status: "done"
priority: "high"
assignee: null
epic: "characters"
dueDate: "2026-07-31"
created: "2026-06-28T12:00:00.000Z"
modified: "2026-07-03T09:49:09.318Z"
completedAt: "2026-07-03T09:49:09.318Z"
labels: ["character", "wizard", "world"]
order: "a0U"
---
# Character: Earth Wizard (elemental)

**Role:** NPC | **Appears:** I.3 only | **Location:** Wizard School — Earth chamber

Delivers the final verdict: no mana. Mabel is not admitted to Wizard School.

**Build tasks:**

1. **Scene Setup**
   - \[x\] Create a new `.tscn` file that instances `Scenes/characters/character_base.tscn` as its root
   - \[x\] Rename the root node to `"wizard-earth"` (`"EarthWizard"`)
2. **Character Data**
   - \[x\] Create `Character_WizardEarth_CharacterData.tres` in `GameData/characterData/` (`EarthWizard_CharacterData.tres`)
   - \[x\] Reference it via `_characterData` on the root node
   - \[x\] Set `PersistentId` to `"wizard-earth"` (`"earthwizard"`)
3. **3D Model**
   - \[x\] Pick or import an earth/stone-themed GLB model from `assets/3D/Mini Characters 1/Models/GLB format/` (`Mage.glb`, alt texture B)
   - \[x\] Add it as a child node of the scene
   - \[x\] Set `CharacterController3D.VisualRoot` to `NodePath("../<model-node-name>")`
4. **Collision Shape**
   - \[x\] Add a `CapsuleShape3D` sub-resource with appropriate `radius` and `height`
   - \[x\] Apply it to `CollisionShape3D` with a transform offset (typically `y ≈ 0.35`)
5. **Animation Tree**
   - \[x\] Override `AnimationTree.root_node` → `../../<model-node-name>`
   - \[x\] Override `AnimationTree.anim_player` → `../../<model-node-name>/AnimationPlayer`
   - \[x\] Animations needed: idle (stationary on platform) (`Idle_A`, autoplay)
6. **Closeup Camera**
   - \[x\] Put 3D model meshes on layer 20
   - \[x\] Move Closeup Camera to frame the face of the character (no override in scene, uses base default)

## Files

- GameData/characterData/EarthWizard_CharacterData.tres
- Scenes/characters/wizard_earth.tscn