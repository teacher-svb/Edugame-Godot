---
id: "char-cornelis-2026-06-28"
status: "done"
priority: "high"
assignee: null
epic: "characters"
dueDate: "2026-07-31"
created: "2026-06-28T12:00:00.000Z"
modified: "2026-07-03T09:34:51.314Z"
completedAt: "2026-07-03T09:34:51.314Z"
labels: ["character", "villager", "world"]
order: "a0d"
---
# Character: Cornelis (villager)

**Role:** Villager | **First appears:** I.2 | **Location:** Moat house

Fussy, self-important. Convinced the moat makes him the safest man in Beaverford. Shouts from his window. Stationary NPC — always at or near his window.

**Build tasks:**

1. **Scene Setup**
   - \[x\] Create a new `.tscn` file that instances `Scenes/characters/character_base.tscn` as its root
   - \[x\] Rename the root node to `"cornelis"` (`"Cornelis"`)
2. **Character Data**
   - \[x\] Create `Character_Cornelis_CharacterData.tres` in `GameData/characterData/` (`Cornelis_CharacterData.tres`)
   - \[x\] Reference it via `_characterData` on the root node
   - \[x\] Set `PersistentId` to `"cornelis"` (`"Cornelis"`)
3. **3D Model**
   - \[x\] Pick or import a GLB model from `assets/3D/Mini Characters 1/Models/GLB format/` (`Engineer.glb`)
   - \[x\] Add it as a child node of the scene
   - \[x\] Set `CharacterController3D.VisualRoot` to `NodePath("../<model-node-name>")` (no `CharacterAnimator` override in scene)
4. **Collision Shape**
   - \[x\] Add a `CapsuleShape3D` sub-resource with appropriate `radius` and `height`
   - \[x\] Apply it to `CollisionShape3D` with a transform offset (typically `y ≈ 0.35`) (`y = 0.4`)
5. **Animation Tree**
   - \[x\] Override `AnimationTree.root_node` → `../../<model-node-name>`
   - \[x\] Override `AnimationTree.anim_player` → `../../<model-node-name>/AnimationPlayer`
   - \[x\] Animations needed: idle (window-shouting pose), no movement needed
6. **Closeup Camera**
   - \[x\] Put 3D model meshes on layer 20
   - \[x\] Move Closeup Camera to frame the face of the character (no override in scene, uses base default)

## Files

- GameData/characterData/Cornelis_CharacterData.tres
- Scenes/characters/cornelis.tscn