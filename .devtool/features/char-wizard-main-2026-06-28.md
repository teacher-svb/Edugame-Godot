---
id: "char-wizard-main-2026-06-28"
status: "backlog"
priority: "medium"
assignee: null
epic: "characters"
dueDate: "2026-09-30"
created: "2026-06-28T12:00:00.000Z"
modified: "2026-06-28T12:00:00.000Z"
completedAt: null
labels: ["character", "wizard", "antagonist", "world"]
order: "chr-15"
---
# Character: The Wizard (antagonist)

**Role:** Antagonist | **First appears (in person):** IV.4 | **Location:** Mana Mine

Not a villain — genuinely believes magic is the natural order. Founded and controls both the magical system and the Guild. Uses Guild machines to extract the last mana from the earth. Cannot accept a world where knowledge is shared. Leaves alone at end of IV.5.

Referenced by reputation throughout Acts I–III; his house is visible from Act I. Only physically present in IV.4 and IV.5.

**Build tasks:**

1. **Scene Setup**
   - [ ] Create a new `.tscn` file that instances `Scenes/characters/character_base.tscn` as its root
   - [ ] Rename the root node to `"wizard"`
2. **Character Data**
   - [ ] Create `Character_Wizard_CharacterData.tres` in `GameData/characterData/`
   - [ ] Reference it via `_characterData` on the root node
   - [ ] Set `PersistentId` to `"wizard"`
3. **3D Model**
   - [ ] Pick or import a GLB model from `assets/3D/Mini Characters 1/Models/GLB format/` (distinct, imposing, not cartoonishly evil)
   - [ ] Add it as a child node of the scene
   - [ ] Set `CharacterController3D.VisualRoot` to `NodePath("../<model-node-name>")`
4. **Collision Shape**
   - [ ] Add a `CapsuleShape3D` sub-resource with appropriate `radius` and `height`
   - [ ] Apply it to `CollisionShape3D` with a transform offset (typically `y ≈ 0.35`)
5. **Animation Tree**
   - [ ] Override `AnimationTree.root_node` → `../../<model-node-name>`
   - [ ] Override `AnimationTree.anim_player` → `../../<model-node-name>/AnimationPlayer`
   - [ ] Animations needed: idle, walk, departure (IV.5 exit)
6. **Dialogue**
   - [ ] Set up dialogue nodes for IV.4 (Reveal) and IV.5 (Realization / departure)
