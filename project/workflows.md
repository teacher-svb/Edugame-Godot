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

---
