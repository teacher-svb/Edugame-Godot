# Beaverford village: Godot 4 export (structures pass: buildings and bridges as separate files)

`beaverford_village.glb` is the village ground without foliage and without channel water, with an empty marker for every structure and prop: +Y up, 1 unit = 1 m, no extra scale. The 12 buildings and 2 bridges are separate files in `godot_exports/village/structures/`, the props in `godot_exports/village/props/`. Same export settings as before; the level GLB itself no longer holds any animation.

**Coordinates.** Map x runs east, map z runs south. In Godot: `X = map x − 50`, `Z = map z − 50`, `Y` = height. (In Blender: X = map x − 50, Y = 50 − map z.)

## Structures and markers (this export)

Everything is split into three kinds of file:

| Folder | Holds | Placed by |
|---|---|---|
| `godot_exports/village/level/beaverford_village.glb` | terrain, soil sides, retaining walls, stone steps (`Steps_*`, `Level_DoorSteps`), hedge, garden fence, flower bed edging, mooring posts, bakery apron, yard flagstones, `Water_Level`, and all markers | itself (place it at the origin) |
| `godot_exports/village/structures/*.glb` | one file per building and bridge, with its `structure_placements.json` and a shared `textures/` folder | `STRUCT_<name>` empties / `structure_placements.json` |
| `godot_exports/village/props/*.glb` | one file per prop type, with `prop_placements.json` | `PROP_<type>_<nn>` empties / `prop_placements.json` |

**Structure files.** `<root in snake_case>.glb`: `tavern`, `school`, `general_store`, `mabel_home`, `veld_home`, `moat_house`, `brick_house`, `narrow_house`, `bakery`, `windmill`, `well`, `wizard_tower`, `drawbridge`, `stream_bridge`. Each has one root node at the local origin, with no transform of its own. Under it are `<Building>_Static_<Material>` meshes and one node per moving part, with its pivot, same as before. Blender's -Y (front door side) is Godot's +Z. The textures are in `godot_exports/village/structures/textures/` and the GLBs reference them by relative path, so keep that folder next to the GLBs. Godot imports them with no manual fixes, and they aren't duplicated between files.

**Markers.** `Structure_Markers` holds one empty per structure, named `STRUCT_tavern`, `STRUCT_mabel_home`, and so on, at the structure root's transform. The same transforms are listed in `godot_exports/village/structures/structure_placements.json`, in the level's Godot coordinates (`position`, `rotation_quaternion_xyzw`, `scale`, plus `marker`, `structure`, `structure_file` and `animations`).

**Props and structures.** Each entry in `prop_placements.json` now also has `structure`: the `STRUCT_` marker it belongs to, or `null` for free-standing props (the noticeboard and boat). When set, `structure_file` and `relative_to_structure` (`position`, `rotation_quaternion_xyzw`, `scale`, relative to that structure's root) are also listed, so props can be parented to their building.

**Stream bridge collider.** `stream_bridge.glb` has a `StreamBridge_Collision-colonly` mesh (716 triangles), which Godot imports as a `StaticBody3D` with a `ConcavePolygonShape3D` and no visible mesh. It is one smooth deck following the arch of the planks (0.125 m at the ends, 0.424 m in the middle, in Blender world height) plus a solid wall along each rail up to the rail top, about 1 m above the deck. Each end runs 0.5 m further as a ramp that finishes just below the ground.

**Structure colliders.** Every structure file has a `<Root>_Collision-colonly` mesh, which Godot imports as a `StaticBody3D` with a `ConcavePolygonShape3D` and no visible mesh. They are simple solids: wall footprint up to about 2.8 m (5.6 m on the narrow house), a convex hull for the upper floors and roof, and boxes for free-standing posts. Behind every door there is an empty pocket: the door's full width and height, 1 m deep, closed at the back, for a door trigger or loading screen. The list of openings (world box, facing) is in `.work/scripts/beaverford_godot_structures_scripts/data/door_openings.json`. The drawbridge's deck has its own `Drawbridge_Deck_Collision-colonly`, parented to the deck, so it follows the raise and lower animations; `StaticBody3D` moved this way doesn't push the player, so use an `AnimatableBody3D` if riding it matters. Moving parts other than the deck, such as sails, signs, the windlass, and the tower's floating rocks, have no collision. The narrow house's outside stair is a walkable ramp up to its upper door; the windmill's outside stair is blocked. The tower's balcony door has no pocket because it turns with its level.

### Putting the village together in Godot

```gdscript
extends Node3D

func _apply(n: Node3D, t: Dictionary) -> void:
    n.position = Vector3(t.position[0], t.position[1], t.position[2])
    var q = t.rotation_quaternion_xyzw
    n.quaternion = Quaternion(q[0], q[1], q[2], q[3])
    n.scale = Vector3(t.scale[0], t.scale[1], t.scale[2])

func _ready() -> void:
    add_child((load("res://godot_exports/village/level/beaverford_village.glb") as PackedScene).instantiate())
    var structs := {}
    var sp = JSON.parse_string(FileAccess.get_file_as_string("res://godot_exports/village/structures/structure_placements.json"))
    for p in sp.placements:
        var n: Node3D = (load("res://godot_exports/village/structures/" + p.structure_file) as PackedScene).instantiate()
        add_child(n); _apply(n, p); structs[p.marker] = n
    var pl = JSON.parse_string(FileAccess.get_file_as_string("res://godot_exports/village/props/prop_placements.json"))
    for p in pl.placements:
        var n: Node3D = (load("res://godot_exports/village/props/" + p.prop_file) as PackedScene).instantiate()
        if p.structure:
            structs[p.structure].add_child(n); _apply(n, p.relative_to_structure)
        else:
            add_child(n); _apply(n, p)
```

Copy all three folders to your project (the paths above assume they sit at `res://`). You can also place the scenes yourself and use the `STRUCT_` / `PROP_` empties in the level only as a guide.

**Colours.** No import script is needed anywhere. Painted parts in the level, structures and props get their colours from palette textures (`T_LevelColorMap.png`, `T_StructureColorMap.png`, `T_PropColorMap.png`; see *Materials*); keep each `textures/` folder next to its GLBs. Only `M_Road`, `M_Terrain` and `M_Riverbed` still use vertex colours, for their tint, and Godot turns those on by itself (see *Ground materials*).

**Root positions** were checked against each structure's footprint. Roots sit at the bottom centre of the footprint and at floor level, with the front door facing -Y (+Z in Godot). `well`, `drawbridge` and `stream_bridge` have no front door: `well` and `drawbridge` keep their turned roots (90° and 180°).

## Fixes pass (this export)

- **Excluded from this GLB** (kept as excluded collections in the .blend): `Foliage_Reference`, `Water_Reference`, `Stones_Reference` (the 160 loose stones and riverbed stones), `Props_Reference` (the separated props), `Structures_Reference` (the separated buildings and bridges), `Level Helpers`, cameras and lights. `Scale References` was deleted.
- **Included:** `Prop_Markers` (43 empties), `Structure_Markers` (14 empties), `Water_Level`.
- **Tavern:** moved 1.40 m north; the terrace is flat at 0.5 m with a stone retaining wall on all four sides. The steps at the south wall are unchanged.
- **Houses:** `BrickHouse` is mirrored left to right and now stands at map (53, 69); `VeldHome` at map (28, 88).
- **Windmill:** planks run the same way on all 8 sides; the stone base has a dark backing; the door fits its frame and has a dark recess behind it.
- **Doors:** every door was checked through its swing; openings have a dark surface behind the leaf (2 triangles each). See `.work/progress/beaverford_godot_fixes_progress.md` for the list.

### Roofs (tile rows baked)

The gabled roofs (tavern, school, general store, brick house, narrow house, Veld home, bakery, well and the farm's slate roof) no longer model every tile. Each row is one low step in `<Building>_Static_RoofTile_Clay` / `_Slate`, using a shared tiling set (`M_RoofTile_Clay`, `M_RoofTile_Slate`): `T_RoofTile_<style>_BaseColor`, `_Normal` (OpenGL style, Y up) and `_ORM` (AO, roughness, metallic), 2048². Godot imports them with no manual fixes. Eave rows, gable-end tiles, half tiles and ridge tiles are still real geometry. The windmill cap and wizard tower roof are unchanged. Texture memory for both sets: about 134 MB uncompressed, roughly 22-34 MB VRAM-compressed.

### Prop markers and `prop_placements.json`

The props are separate files in `godot_exports/village/props/` (one GLB per prop type, origin at the bottom centre, front towards -Y in Blender / +Z in Godot). The level holds one empty per placed prop, named `PROP_<type>_<nn>`, and `godot_exports/village/props/prop_placements.json` lists the same placements in Godot coordinates.

```gdscript
var data = JSON.parse_string(FileAccess.get_file_as_string("res://godot_exports/village/props/prop_placements.json"))
for p in data.placements:
    var n: Node3D = (load("res://godot_exports/village/props/" + p.prop_file) as PackedScene).instantiate()
    add_child(n)
    n.position = Vector3(p.position[0], p.position[1], p.position[2])
    n.quaternion = Quaternion(p.rotation_quaternion_xyzw[0], p.rotation_quaternion_xyzw[1], p.rotation_quaternion_xyzw[2], p.rotation_quaternion_xyzw[3])
    n.scale = Vector3(p.scale[0], p.scale[1], p.scale[2])
```

The boat's marker is at the water height (Y = -0.3), so the boat can float. `prop_cellar_hatch` has two child door nodes with their own hinge pivots.

Prop colours come from `godot_exports/village/props/textures/T_PropColorMap.png` (shared by all prop files), so keep that folder next to the GLBs. No import script is needed.

## Drawbridge (this export)

The bakehouse hedge in this export is one continuous piece. The drawbridge was rebuilt as a working linkage:

- **Gantry:** posts, braces and crossbeam moved 1.92 m towards the moat, with the pivot directly above the deck's hinge. The pivot bracket sits on the crossbeam; the box is strapped over the beam's end.
- **Linkage:** the beam's chain ring is the same distance and direction from its pivot as the deck's ring is from the hinge, so beam and deck turn by the same angle and each chain keeps its exact length and direction. Iron rings at both ends; the end links pass through them.
- **Rope:** `Drawbridge_Rope` is gone. Hanging straight down from the box it would reach 16 cm below the ground at the raised position.
- **Landing:** the deck's free end now rests on the south bank. The fixed south deck lost its first plank, its stringers start 30 cm further back and its two nearest posts moved 26 cm.

**Angles** (deck above horizontal):

| Pose | Deck angle | Node rotation about local X |
|---|---|---|
| Level default (rest pose) | 35.0° | 0° |
| Lowered, resting on the bank | −0.51° | −35.52° |
| Raised | 67.8° | +32.78° |

The raised angle stops short of 75° because the deck's rails reach the gantry's knee braces at 67.97°.

### Playing the animations

The drawbridge is now its own file, `godot_exports/village/structures/drawbridge.glb`, with its own `AnimationPlayer`, so the tower and the bridge can play at the same time with no extra setup. `Drawbridge_Raise` runs from lowered to raised, `Drawbridge_Lower` the reverse: 4 s each with a slow start and gentle stop, not looping. Only the two rotations and two chain translations are animated; the box follows the beam.

```gdscript
@onready var bridge: AnimationPlayer = $drawbridge/AnimationPlayer   # path to your instance of drawbridge.glb

func _ready() -> void:
    # optional: start with the bridge down instead of the 35° level pose
    bridge.play("Drawbridge_Lower")
    bridge.seek(4.0, true)

func raise_bridge() -> void:
    bridge.play("Drawbridge_Raise")          # bridge.animation_finished fires after 4 s

func lower_bridge() -> void:
    bridge.play("Drawbridge_Lower")
```

Each animation starts from its own first pose, so start `Drawbridge_Raise` from the lowered pose or the bridge will snap to it. The GLB has no collision for the deck.

## Turning wizard tower

The wizard's tower is split into five level nodes. Levels 1–4 each turn slowly around their own (tilted) axis, alternating direction; the ground level and its door stay still, so the door keeps facing the path.

| Node | Turns | One turn | Triangles | Attached (children) |
|---|---|---|---|---|
| `WizardTower_Level_0` (ground level, door level) | no | — | 3,412 | — (`WizardTower_Door` stays under the root) |
| `WizardTower_Level_1` | clockwise seen from above | 60 s | 3,190 | `WizardTower_Crystal` |
| `WizardTower_Level_2` (balcony) | counterclockwise | 60 s | 4,443 | `WizardTower_BalconyDoor`, `WizardTower_Telescope` |
| `WizardTower_Level_3` (timber level) | clockwise | 60 s | 2,736 | — |
| `WizardTower_Level_4` (roof, chimney, star) | counterclockwise | 60 s | 1,210 | — |

- Every level is a direct child of `WizardTower`; levels are never parented to each other. Each level's origin is on its own centre axis at the bottom of the level, with local **+Y** along the level's tilt, so the animation is a pure rotation about local Y (6°/s, constant speed). Position and tilt never change.
- The yard (cauldron, mushrooms, standing stones, steps) is still `WizardTower_Static_Painted` / `WizardTower_Static_CauldronGlow`. The floating rocks stay under the root and don't turn.
- The old merged tower meshes `WizardTower_Static_TowerStone`, `_MagicGlow` and `_FloorBoards` are gone (their parts are now in the levels). All other node names are unchanged.

### Animation

- One animation, **`WizardTower_Turn-loop`** in `wizard_tower.glb`. Godot strips the `-loop` suffix and imports it as **`WizardTower_Turn`** with **Loop Mode = Linear**, length **60 s**, in the scene's `AnimationPlayer`.
- Linear interpolation, quaternion keys every 10° (0–60 s); the last key is the same pose as the first, so the loop is seamless. The key signs are made continuous after export, so no player can take the long way round between two keys.
- Godot bakes it at 30 fps on import (*Animation → FPS*). Measured in Godot 4.7: no axis wobble, at most 0.21° away from the exact constant-speed angle.

**Play it from code** (the `AnimationPlayer` is a direct child of the imported scene's root):

```gdscript
@onready var anim: AnimationPlayer = $wizard_tower/AnimationPlayer   # path to your instance of wizard_tower.glb

func _ready() -> void:
    anim.play("WizardTower_Turn")          # loops forever

func restart_tower() -> void:
    anim.stop()                            # stop() resets to the start
    anim.play("WizardTower_Turn")
```

**Autoplay (no code):** right-click the instanced village → *Editable Children*, select its `AnimationPlayer`, open the Animation panel, choose `WizardTower_Turn` and click **Autoplay on Load** (the "A" icon). It then starts from 0 s every time the scene loads. To restart it later, call `stop()` then `play("WizardTower_Turn")`, or `seek(0.0, true)` while it plays.

If you change this animation's settings in *Advanced Import Settings*, set its **Loop Mode = Linear** there as well: in the import test, an override for the animation came back with Loop Mode = None.

### Collision

`WizardTower_Collision-convcolonly` is one static, convex 16-sided prism (circumradius 2.80 m, ground to 15.7 m), a child of `WizardTower` (not of any level), so it never turns. Godot imports it as `WizardTower_Collision` (`StaticBody3D` + `CollisionShape3D` with a `ConvexPolygonShape3D`) and draws nothing. It sits inside the base wall (wall 2.84 m, plinth 3.0 m from the axis), with a flat side toward the door: its front is 9 cm behind the door's front face, so it covers neither the door steps nor the path.

### Joints

The levels already hover apart (at least 0.35 m between levels 0–3 at every moment of the turn). Changes so that nothing passes through anything while turning:

- **Crystal (level 1):** moved 0.10 m down its cradle axis; it used to touch the underside collar of level 2 at about 35 s. It still floats 5.7 cm above its cradle.
- **Level 3 → roof:** the roof is tilted about 15° against level 3, so level 3's top sweeps under the eave. 40 vertices at the tops of its timber posts and rails were pulled inward (by at most 2.8 cm) so they stay inside the roof all the way round, and the roof's flat underside disc was replaced by a ring-shaped soffit (+10 triangles) that clears level 3's top. Inside the roof, level 3's top and the sunk-in chimney base pass through each other between about 37 and 55 s; this is fully enclosed by the roof (checked) and never visible.
- **Faces that were hidden before:** the first Godot pass had deleted faces that the tower itself always covered. With the levels turning, some come into view; 484 of them (770 triangles; mostly level 3's timber frames, posts and rails under the roof's low side, plus window frames, moss, chimney rings and balcony details) were restored from the original model with their original colours. Duplicates of existing faces, faces below the ground and faces seen only from underneath were not restored.
- No braces or straps crossed between levels (the original model already had separated, hovering levels), so none were moved or removed.

Tower total: 16,659 → **17,439 triangles**, 14 → **24 surfaces** (12 → 15 nodes).

## Deeper roads (this export)

Roads, paths and the square (all `M_Road` faces) now sit **0.08 m** below the surrounding grass (was 0.05 m). The 0.2 m bevel keeps its width and takes up the extra height as one smooth slope. Road surfaces keep their slope along their length and stay level side to side. Where a road meets a door, step, bridge, deck, the well, the noticeboard or the retaining wall it keeps its original height, as before (the lowering fades in over the last 0.35–0.95 m). The 110 small flush stones that lay on the roads were removed (the road texture already has small stones); rocks and edge stones stay.

## Ridge-tile fix (this export)

Every tiled roof ridge now shows a continuous row of tiles. The ridge tile is a half-round cap with a wider socket at one end; in the original model only the Bakery and the Well placed the tiles so that each narrow end sits inside the next tile's socket. The other 12 ridges (School, Tavern main/dormers/porch, General Store main/dormer, Veld home, Narrow house main/dormer, Brick house both wings) used about twice that spacing, so roughly every other tile was missing, and their rows stopped 0.14–0.31 m short of the far end.

All 14 tiled ridges were rebuilt with the same rule: socket spacing 0.25 × tile scale (each tile overlaps the next by 0.04 × scale, about 14 % of its length), the row runs from gable to gable (2 cm past the roof) or into the roof, wall or ridge it meets, and the tile is stretched slightly along the ridge (0.96–1.09) so the row fits exactly. The hidden narrow end cap of every tile except the last was removed (124 instead of 140 triangles per tile). Node names, hierarchy, pivots and materials are unchanged.

## What changed since v2

- **Roads are part of the terrain mesh.** The roads, paths, door connections and the square are no longer painted into one big texture: they are cut into the single `Terrain` mesh, 5 cm lower than the grass, with a 0.2 m soft bevel along every edge and slightly irregular edges. Where a road meets a door, step, bridge, deck, the well or the retaining wall it stays at its old height (no step or gap).
- **Three ground materials with tiling textures** instead of the baked 4096² ground texture: `M_Road`, `M_Terrain`, `M_Riverbed` (see *Ground materials*).
- The old `M_Ground` and `T_VillageGround.png` are gone.

(v2 changes still apply: no foliage, no channel water, window fix, well in the square, noticeboard on the north side, drawbridge turned to the island.)

## What's in the GLB

| | |
|---|---|
| Nodes | 72: 14 meshes, 43 prop markers, 14 structure markers, 1 `Water_Level` |
| Surfaces (mesh × material) | 22 |
| Triangles | 115,911 |
| Materials | 5: `M_Road`, `M_Terrain`, `M_Riverbed`, `M_LevelColors`, `M_RoughStone` |
| Textures | 10 PNGs embedded (ground and stone), plus the palette `textures/T_LevelColorMap.png` as an external file |
| Animations | none; see `wizard_tower.glb` and `drawbridge.glb` |
| Not included | foliage, channel water, loose stones, separated props and structures, cameras, lights, helpers |

Structure files (`godot_exports/village/structures/`): GLB meshes and animations 13.7 MB, plus one shared `textures/` folder, 8.5 MB (15 PNGs).

Hierarchy of the structure files: each building is one root node at the file's origin, named `MabelHome`, `MoatHouse`, `Bakery`, `Windmill`, `Well`, `VeldHome`, `School`, `WizardTower`, `Tavern`, `GeneralStore`, `BrickHouse`, `NarrowHouse` and `Drawbridge` (same names as v1). Under each root:

- **`<Building>_Static_<Material>`**: all static parts, merged into one mesh per material, with identity transforms relative to the root.
- **Moving parts**: one node each, with its origin at the pivot.

| Moving part | Pivot |
|---|---|
| Doors (`*_Door`, `*_Door_Leaf0/1`, `*_BackDoor_*`, `*_UpperDoor_*`, `Tavern_CellarDoor0/1`, `Bakery_DoorFront/Right`, `WizardTower_BalconyDoor`) | Bottom of the hinge edge. Double doors hinge on their outer edges; single doors hinge opposite their knob or ring. Single doors without a knob were given the −X/−Y edge, so **check the hinge side before animating them.** |
| `Windmill_Sails` | Hub centre; rotate about local Y. |
| `Windmill_FanWheel` | Wheel centre. |
| `Tavern_HangingSign`, `GeneralStore_HangingSign` | Top centre, at the hanging point. |
| `School_Bell` | Top. |
| `Well_Windlass` | Axle centre. |
| `Well_Bucket` (bucket + hanging rope) | Top. |
| `Drawbridge_Deck` | Exactly on the hinge axis, on the island bank. Modelled 35.0° open (its rest pose); rotates about local X. |
| `Drawbridge_LiftBeam` | Exactly on the bracket pin, straight above the hinge; rotates about local X. |
| `Drawbridge_Counterweight` | Child of `Drawbridge_LiftBeam`; moves with it. |
| `Drawbridge_Chain_L`, `Drawbridge_Chain_R` | Top ring point of each chain; they only move, never rotate. |
| `WizardTower_Crystal`, `WizardTower_FloatingRock1..3`, `WizardTower_Telescope` | Their centres. The crystal is a child of `WizardTower_Level_1`; the telescope and the balcony door are children of `WizardTower_Level_2`, so they turn with their level. |
| `WizardTower_Level_0..4` | Each level's own centre axis, at the bottom of the level; turn about local Y (see *Turning wizard tower*). |

Other nodes, all with identity transforms:
- **`Terrain`**: the ground, one mesh with four surfaces (a one-triangle `M_Terrain`, then `M_Road`, `M_Terrain`, `M_Riverbed`; see *Ground materials*); `Terrain_SoilSides` (the site-edge cross-section), `Terrain_RetainWall_*`; `Level_*` meshes (hedge, door steps, garden fence, flower bed edging, apron, yard flags, drawbridge mooring post)
- `Water_Level` (empty, see below)
- `Steps_*`, `MoatHouseDetails_Painted` (mooring post); `StreamBridge` is in `godot_exports/village/structures/stream_bridge.glb`

### Root placement changes (v1 → v2, unchanged in v3)

| Root | v1 (map x, z, rotation) | v2 |
|---|---|---|
| `Well` | 51.5, 63.5, 90° | **40, 56, 90°** (centre of the square; Godot position (−10, 0, 6)) |
| `Drawbridge` | 77, 68.4, 0° | **77, 66.2, 180°** (Godot position (27, 0, 16.2), rotation Y −180°) |
| `Noticeboard_Painted` (mesh) | centred 40, 56 | **centred 44.5, 49.5**, facing south (Blender −Y, Godot +Z), node transform still identity |

The moat-house lantern (in `MoatHouseDetails_*`) moved 1.7 m north to map (78.75, 60.6), because the drawbridge gantry's brace now stands where it was.

## Ground materials

All three are Principled → glTF metallic-roughness materials. Base colour = texture × vertex colour (`COLOR_0`, linear, subtle large-scale tint), normal map, roughness map (glTF stores roughness in the green channel; Godot reads it from there automatically). Metallic 0.

| Material | Covers | Tile size | Base colour | Normal | Roughness |
|---|---|---|---|---|---|
| `M_Road` | roads, paths, door connections, the square and their bevels | **6 m** | `T_Road_BaseColor.png` | `T_Road_Normal.png` | `T_Road_Roughness.png` |
| `M_Terrain` | grass and open ground, channel banks above the riverbed line | **10 m** | `T_Terrain_BaseColor.png` | `T_Terrain_Normal.png` | `T_Terrain_Roughness.png` |
| `M_Riverbed` | stream, feeder and moat floors and lower banks, up to just above the water level | **6 m** | `T_Riverbed_BaseColor.png` | `T_Riverbed_Normal.png` | `T_Riverbed_Roughness.png` |

- Textures: 2048 × 2048 PNG, seamless on all sides, stylised/painted to match the buildings. Base colour is sRGB; normal and roughness are linear (non-colour).
- **Normal maps are OpenGL style (+Y up / green up)** — Godot's own convention, so no flipping is needed. Normal scale 0.5 (subtle).
- **UVs:** one UV set, world-planar from above: u = X / tile size, v = −Z / tile size (Godot axes). Roads and grass are plain planar; on the riverbed banks the UVs are "unrolled" along the channel profile so the steep banks don't stretch the texture (the seam sits on the riverbed/grass edge). The flat channel floors are plain planar.
- **Repeat:** the glTF samplers use the default REPEAT wrap, so Godot repeats the textures without any setting (checked: `texture_repeat` is on).
- **Vertex colours:** subtle multipliers (0.72–1.0). Roads: warm, 0.95–1.0 variation; **the square is a little cooler/greyer than the roads**; grass: large lighter/darker patches and a soil tint on the channel banks; riverbed: darker toward the floor, dry sandy stretches. Keep *Vertex Color → Is sRGB* **off**.
- **Road tint without an import script:** Godot 4.7 only switches on *Vertex Color → Use as Albedo* for a surface when the surface before it in the same mesh also has vertex colours, so the first one misses out. `Terrain` therefore starts with a one-triangle `M_Terrain` surface. It looks like any other grass and shares its material, and `M_Road` comes after it, so all four surfaces get the flag automatically. Keep that surface first if you edit the mesh.
- Tangents for the normal maps are generated by Godot on import (*Meshes → Ensure Tangents*, on by default).

### Texture memory (the 9 new ground maps)
| | per map | 9 maps |
|---|---|---|
| PNG on disk | 0.2–1.3 MB | 6.3 MB |
| GPU, uncompressed RGBA8 + mipmaps | 22.4 MB | 201 MB |
| GPU, VRAM-compressed (Godot's default for 3D textures: BC1/BC7/BC5 on desktop) | 2.8–5.6 MB | ≈ 34 MB |

This replaces the old 4096² ground texture (89.5 MB uncompressed / ≈ 11 MB compressed). If memory is tight, set the Import option *Size Limit* to 1024 (¼ of the memory); from the game camera the difference is hard to see.

## Water

The water surfaces are not in the GLB. Add your own water meshes with a shader at the original height:

- **Water surface height: Y = −0.3** (all channels). The `Water_Level` empty marks it at Godot (0, −0.3, −22) = map (50, 28), on the stream centreline.
- Channel bed: Y = −1.0 where the bed is flat (≥ 0.95 m from the bank); the banks rise to about +0.04 at the rim.

| Channel | Map x | Map z | Godot X | Godot Z |
|---|---|---|---|---|
| Stream (east–west, full width) | 0 – 100 | 26 – 30 | −50 – 50 | −24 – −20 |
| Feeder (stream → moat) | 76 – 78 | 30 – 41 | 26 – 28 | −20 – −9 |
| Moat, outer edge | 64 – 90 | 41 – 69.4 | 14 – 40 | −9 – 19.4 |
| Island (inside the moat) | 68 – 86 | 45 – 65.2 | 18 – 36 | −5 – 15.2 |

The moat is the outer rectangle minus the island: 4 m wide on the north, east and west sides, 4.2 m wide on the south side (z 65.2 – 69.4) where the drawbridge crosses. The site-edge soil cross-sections at map x 0 and x 100 follow the stream profile, so a water mesh can end flush with the site edge.

The moat-house boat (in `MoatHouseDetails_Painted`, map 70.6, 67.3) is still at its floating height (hull bottom Y −0.44), so it floats once the water is back.

Small water surfaces still included (part of props): Mabel's water butt (map 50.45, 90.85, Y 1.13), the moat-house water barrel (79.3, 54.0, Y 1.09), the tavern trough (5.4, 64.45, Y 0.95), and **the well water** `Well_Static_WaterSurface` (map 40, 56, top Y −1.65, inside the shaft).

## Windows

Each wall opening is a shallow blind pocket. In v1 the pocket's back face (wall material) lay exactly in the plane of the glass, so the two z-fought and the wall texture showed through up close. In v2 that duplicate back face is removed on every window; the glass quad (dark painted glass, `M_Glow_Window` or `M_Glow_Magic`) is now the only surface there. The wizard's balcony door is the exception: its glass is on the moving door, so the wall face behind it stays and sits 3 cm deeper.

## Materials

- **`M_StructureColors`** (structure files only): replaces `M_Painted` on the buildings and bridges. Base colour comes from `T_StructureColorMap.png`, 128×128, 375 colours, each a 4×4 block of texels; every corner of a face points at the centre of its colour's block, so a face shows exactly one colour. The texture has an `.import` file that keeps it lossless with no mipmaps, and the material samples it with nearest filtering. To recolour a building, edit that block in the PNG. Blocks may be shared by several parts.
- **`M_PropColors`** (prop files only): same approach as `M_StructureColors`, using `godot_exports/village/props/textures/T_PropColorMap.png`, 64×64, 106 colours.
- **`M_LevelColors`** (level only): same approach for the level's painted pieces (soil sides, retaining wall caps, steps, hedge, fence and so on), using `textures/T_LevelColorMap.png`, 64×64, 67 colours.
- **Textured:** `M_Brick`, `M_StoneWall`, `M_CottageStone`, `M_RoughStone`, `M_TowerStone`, `M_Boards`, `M_FloorBoards` and `M_Thatch` use tileable 2048² textures that repeat every 4.4–8 m.  The ground uses `M_Road` / `M_Terrain` / `M_Riverbed` (see *Ground materials*).
- **Water:** `M_Water` (alpha blend), only on the small props and the well water. `M_WaterSide` from v1 is gone.
- **Emissive:** `M_Glow_Window`, `M_Glow_Lantern`, `M_Glow_Cauldron`, `M_Glow_Magic` and `M_Glow_Crystal`. Their strength comes through `KHR_materials_emissive_strength`; tune it together with your Environment glow and tonemapping.
- **Double-sided:** all materials are exported double-sided, so Godot imports them with **Cull Mode = Disabled**.

### Toon look
glTF can't carry the original toon shading, so the materials are plain Standard materials with the original colours. For the toon look, on the imported materials (Advanced Import Settings → Materials → extract them, or use an import script):
- set `Diffuse Mode = Toon`, and
- set `Specular Mode = Toon` or `Disabled`,

or use your own toon shader as the material and keep the albedo texture and vertex colour.

## Outlines (`outline.gdshader`)

The GLB has no outline geometry. Add the outline in Godot as a second pass:

1. Create a `ShaderMaterial` with `outline.gdshader`, for example `outline_building.tres` (width about 0.02) and `outline_small.tres` (width about 0.005 for scatter).
2. On each surface material, set **Next Pass** to that ShaderMaterial.
3. Uniforms:
   - `outline_color`: default (58, 38, 30), the original line colour.
   - `outline_width`: in metres, in object space.

Notes:
- The hull grows along vertex normals. Flat-shaded hard edges have split normals, so a very wide outline can show small gaps at sharp corners.
- Each outlined surface costs one extra draw call. To save draws, skip the outline on scatter chunks.

## LODs

No manual LODs are included. Godot can generate them on import:

1. Select `beaverford_village.glb` in the FileSystem dock.
2. In the **Import** dock: Meshes → **Generate LODs** = On (the default in Godot 4). Click **Reimport**.
3. Tune the switch distance with Project Settings → Rendering → Mesh LOD → LOD Change → **Threshold Pixels** (default 1.0), or per instance with `lod_bias` on the MeshInstance3D.

## Draw calls (estimate)

There are 114 surfaces, so about **114 draw calls for the main colour pass** for the level with the whole site in view, plus one or more for each placed prop. A depth pre-pass, shadow cascades and outline next passes come on top, as in v1.

## Import test
Godot 4.7 headless import of this GLB, the 14 structure files and the 32 prop files in a temporary project with no import script: no errors or warnings. The level has 22 surfaces (115,911 triangles); the 4 `Terrain` surfaces with vertex colours all get *Use as Albedo* automatically. The 16 painted level surfaces, 56 structure surfaces and 34 prop surfaces use their palette textures, and colours sampled in Godot on all 26,868 painted level triangles match the originals exactly. `WizardTower_Turn`, `Drawbridge_Raise` and `Drawbridge_Lower` import in their own files.
