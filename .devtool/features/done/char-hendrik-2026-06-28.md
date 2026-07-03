---
id: "char-hendrik-2026-06-28"
status: "done"
priority: "high"
assignee: null
epic: "characters"
dueDate: "2026-07-31"
created: "2026-06-28T12:00:00.000Z"
modified: "2026-07-03T09:34:49.784Z"
completedAt: "2026-07-03T09:34:49.784Z"
labels: ["character", "villager", "world"]
order: "a0l"
---
# Character: Hendrik (miller)

**Role:** Villager | **First appears:** I.2 | **Location:** Windmill

Grumpy and nostalgic. Everything was better before. Filed a complaint with the Wizard two weeks ago; heard nothing. Mobile NPC — moves around the windmill area.

**Build tasks:**

1. **Scene Setup**
   - \[x\] Create a new `.tscn` file that instances `Scenes/characters/character_base.tscn` as its root
   - \[x\] Rename the root node to `"hendrik"` (`"Hendrik"`)
2. **Character Data**
   - \[x\] Create `Character_Hendrik_CharacterData.tres` in `GameData/characterData/` (`Hendrik_CharacterData.tres`)
   - \[x\] Reference it via `_characterData` on the root node
   - \[x\] Set `PersistentId` to `"hendrik"` (`"Hendrik"`)
3. **3D Model**
   - \[x\] Pick or import a GLB model from `assets/3D/Mini Characters 1/Models/GLB format/` (`Farmer_A.glb`)
   - \[x\] Add it as a child node of the scene
   - \[x\] Set `CharacterController3D.VisualRoot` to `NodePath("../<model-node-name>")`
4. **Collision Shape**
   - \[x\] Add a `CapsuleShape3D` sub-resource with appropriate `radius` and `height`
   - \[x\] Apply it to `CollisionShape3D` with a transform offset (typically `y ≈ 0.35`) (`y = 0.4`)
5. **Animation Tree**
   - \[x\] Override `AnimationTree.root_node` → `../../<model-node-name>`
   - \[x\] Override `AnimationTree.anim_player` → `../../<model-node-name>/AnimationPlayer`
   - \[x\] Animations needed: idle, walk
6. **Closeup Camera**
   - \[x\] Put 3D model meshes on layer 20
   - \[x\] Move Closeup Camera to frame the face of the character (no override in scene, uses base default)

## Files

- GameData/characterData/Hendrik_CharacterData.tres
- Scenes/characters/hendrik.tscn