# Forest tiles for wave function collapse

41 ground tiles of 4 × 4 m for generating the forest level: forest floor, paths and a border of bushes and rocks that closes off the level. Same painted look as the village ground. Trees are not included; your MultiMesh scatter places them, using the scatter mask in the JSON file.

| File | Contents |
|---|---|
| `forest_<name>.glb` | One tile, centre at the origin. Every rotation is its own file, so never rotate or mirror a tile. |
| `forest_tiles.json` | Per tile: file, sockets, allowed neighbours, weight, scatter mask, triangle counts. |
| `textures/` | `T_ForestFloor_BaseColor/Normal.png`, `T_ForestPath_BaseColor/Normal.png` (1024², one repeat per tile), `T_ForestColorMap.png` (palette for bushes and rocks, with its `.import` file). Keep this folder next to the GLBs. |

Export settings as in the village: GLB, +Y up, 1 unit = 1 m. No import script is needed.

## Tiles

| Tiles | Variants | What it is | Weight each |
|---|---|---|---|
| `floor_a` … `floor_d` | 4 | Forest floor, different bumps, tint and texture orientation | 6 |
| `path_straight_NS`, `_EW` | 2 | Straight path | 1.5 |
| `path_curve_NE`, `_ES`, `_SW`, `_NW` | 4 | Quarter-circle bend between the two named sides | 0.8 |
| `path_t_NES`, `_ESW`, `_NSW`, `_NEW` | 4 | T junction, path leaves on the named sides | 0.3 |
| `path_cross` | 1 | Crossroads | 0.15 |
| `path_end_N`, `_E`, `_S`, `_W` | 4 | Path from the named side ending in a round clearing (3 m) | 0.2 |
| `border_full`, `border_full_b`, `_c`, `_d` | 4 | Border bank all over, different bush and rock layouts | 1 |
| `border_edge_N`, `_E`, `_S`, `_W` | 4 | Border along the named side, floor on the other half | 1 |
| `border_outer_NE`, `_ES`, `_SW`, `_NW` | 4 | Border along both named sides (outer corner of the level) | 0.5 |
| `border_inner_NE`, `_SE`, `_SW`, `_NW` | 4 | Border only in the named corner (inner corner of the level) | 0.5 |
| `border_exit_N`, `_E`, `_S`, `_W` | 4 | Border edge with a path through it to the named side: the way out of the level | 0.02 |
| `border_path_NS`, `_EW` | 2 | Solid border with a path through it, to lead an exit path to the map edge | 0.02 |

N is Godot -Z, E is +X, S is +Z, W is -X. The weights are only a starting point.

## Sockets

Each edge has a label. Two tiles may touch when the touching edges have the same label: my `east` == the neighbour's `west`, my `north` == the neighbour's `south`. `forest_tiles.json` also lists the allowed neighbours per side for every tile, if your WFC prefers lists.

| Label | Edge |
|---|---|
| `FF` | Forest floor |
| `FFP` | Forest floor with the 2 m path in the middle |
| `BB` | Border bank |
| `BBP` | Border with a path through it |
| `FB` / `BF` | Half floor, half border. Labels are read west → east on north and south edges and south → north on east and west edges, so `FB` means floor at the west/south end and border at the east/north end. |

For the map edge, treat cells outside the map as `BB`, so the level is closed by border. Where the player should leave, use `BBP` for that outside edge (pin the cell to a `border_exit` or `border_path` tile).

The tiles are built from their four corners (floor or border) plus the path sides. The two "diagonal" corner layouts (border in two opposite corners only) are not in the set, so WFC will avoid them. With a closed map edge and these weights, the test generator below solved 12 × 9 and 20 × 16 maps on its first attempt.

```gdscript
# grid[row][col] = tile name from your WFC, row 0 in the north
for row in grid.size():
    for col in grid[row].size():
        var t: Node3D = (load("res://godot_exports/forest_tiles/forest_%s.glb" % grid[row][col]) as PackedScene).instantiate()
        t.position = Vector3(col * 4 + 2, 0, row * 4 + 2)
        add_child(t)
```

## Ground, path and border

- **Heights.** The forest floor is at y = 0 on every tile edge, with gentle bumps (up to ±7 cm) inside a tile. Paths are 2 m wide and lie 0.08 m lower, with a 0.2 m soft bevel, as on the village roads. The border bank is 0.5 m high; paths through the border cut a gully through it.
- **Seams.** On every edge, height, normal and tint depend only on the edge's label, and nothing slopes across the edge, so matching tiles meet without a crack, crease or colour step. The textures repeat once per tile, and their outer band (about 0.2 m) is the same under all 8 rotations and mirrors. That lets each tile use its own texture orientation without a seam, so the repeat is less obvious.
- **Undergrowth.** Bushes and rocks on the border (`<name>_Undergrowth`) are low-poly and flat-shaded like the village bushes, coloured from the palette `T_ForestColorMap.png` (nearest filter). They can stick out 0.3 m past the tile edge but never over a path.
- **Collision.** `<name>_Collision` imports as a `StaticBody3D` with a `ConcavePolygonShape3D` (0.25 m grid, 512 triangles). It follows the ground on floor and paths. On the border it rises into a wall about 1.8 m above the bank, so the player can only cross the border along a path.

Nodes per file: `Tile_<name>` (root) → `<name>_Ground`, `<name>_Undergrowth` (border tiles only), `<name>_Collision`.

## Scatter mask

`scatter_mask` in the JSON gives each tile 16 × 16 cells of 0.25 m, rows north → south (Godot -Z → +Z), columns west → east:

| Value | Meaning |
|---|---|
| `0` | Keep clear: path and 0.6 m beside it |
| `1` | Forest floor |
| `2` | Border bank (place trees densely) |

```gdscript
func mask_at(world: Vector3, grid, tiles_by_name) -> int:
    var col = int(world.x / 4.0); var row = int(world.z / 4.0)
    var tile = tiles_by_name[grid[row][col]]
    var cx = clampi(int((world.x - col * 4.0) / 0.25), 0, 15)
    var cz = clampi(int((world.z - row * 4.0) / 0.25), 0, 15)
    return int(tile.scatter_mask[cz][cx])
```

## Materials and import

| Material | Used on | Notes |
|---|---|---|
| `M_ForestFloor` | ground | `T_ForestFloor_*` × vertex colour (`COLOR_0`): darker shady bank, soil on its slope, trodden grass beside paths, soft patches. Roughness 0.95. |
| `M_ForestPath` | paths and their bevels | `T_ForestPath_*` × vertex colour. Roughness 0.9. |
| `M_ForestFloorFirst` | one small triangle | See below. |
| `M_ForestColors` | bushes and rocks | Palette texture, no vertex colours. |

**Vertex colours without an import script.** Godot 4.7 switches on *Vertex Color → Use as Albedo* for a surface only when the surface before it in the same mesh also has vertex colours, so the first surface misses out. Each `_Ground` mesh therefore starts with a single small floor triangle (`M_ForestFloorFirst`), placed where the tint is 1.0 (at a floor corner) or under a bush, so it looks like the rest. The floor and path surfaces after it get the flag automatically, as in the village terrain.

Ground textures use Godot's default import (VRAM compressed with mipmaps; normal maps are detected). Normal maps are OpenGL style (green up), scale 0.5.

Per tile: ground 2,048 triangles, undergrowth 240–1,600 (bushes and rocks of 80 triangles each, like the village bushes), collider 512. All 41 GLBs: 5.9 MB.

## Tested

Godot 4.7 headless, fresh project, no script: no import errors or warnings; the floor and path surfaces of all 41 tiles have *Use as Albedo* on (62 surfaces); bushes and rocks have no vertex colours; 41 `StaticBody3D` + `ConcavePolygonShape3D`. Edge vertices compared straight from the imported meshes: within each of the 12 socket groups, positions and colours match and normals differ by at most 0.003 (Blender's custom-normal precision, about 0.2°). Two WFC maps (12 × 9 and 20 × 16) were assembled: rays at 12,784 points along the seams hit the colliders on both sides with no holes and no steps (the largest difference, 6 mm, is the slope of the border wall between points 6 mm apart). Renders: `.work/renders/forest_tiles/final_game_camera.png`, `final_top.png`.

## Mana mine entrance (3 × 3 tiles)

Nine extra tiles, `forest_mine_<NW|N|NE|W|C|E|SW|S|SE>.glb`, form one 12 × 12 m piece: a rocky hill with a timber-framed mine entrance, rails coming out of the dark tunnel, glowing mana crystals, an old mine cart, a lantern post and a crate, in a small clearing. Thicket bank on the west, north and east; the path enters from the south. The opening faces +Z (south), towards the camera. Data: `forest_mine.json`.

**Placing it.** The tiles are placed by hand and never generated (weight 0). Put them at their cell offsets from `mine_C` (in the JSON), without rotating them, and put `forest_border_exit_N` in the cell south of `mine_S`:

```
 NW   N   NE        row z-1
 W    C   E         row z
 SW   S   SE        row z+1
     exit_N         row z+2  (border_exit_N)
```

**Sockets.** Outer edges `BB`, so the thicket closes around it. The south edge of `mine_S` is `BBP`, a path through the bank, which only `border_exit_N` (or `border_path_NS`) has on its north edge. Inner edges are `MINE_<a>_<b>`, one label per seam (for example `MINE_C_S` between `mine_C` and `mine_S`), so a tile only fits its own neighbour. Up and down: `ground`, as on the other tiles. In the WFC generator's tile fields: N = `SocketNegZ`, E = `SocketPosX`, S = `SocketPosZ`, W = `SocketNegX`; Rotation None, Weight 0.

| Tile | N | E | S | W |
|---|---|---|---|---|
| `mine_NW` | BB | MINE_NW_N | MINE_NW_W | BB |
| `mine_N` | BB | MINE_N_NE | MINE_N_C | MINE_NW_N |
| `mine_NE` | BB | BB | MINE_NE_E | MINE_N_NE |
| `mine_W` | MINE_NW_W | MINE_W_C | MINE_W_SW | BB |
| `mine_C` | MINE_N_C | MINE_C_E | MINE_C_S | MINE_W_C |
| `mine_E` | MINE_NE_E | BB | MINE_E_SE | MINE_C_E |
| `mine_SW` | MINE_W_SW | MINE_SW_S | BB | BB |
| `mine_S` | MINE_C_S | MINE_S_SE | BBP | MINE_SW_S |
| `mine_SE` | MINE_E_SE | BB | BB | MINE_S_SE |

With paths on (path socket `FFP`), the `FFP` south edge of the hand-placed `border_exit_N` is a path end, so Generate joins the mine's path to the other exits.

**Nodes per file:** `Tile_mine_<pos>` → `mine_<pos>_Ground` (same ground materials as the other tiles, matching the forest tiles on every outer edge), `mine_<pos>_Mine` (rocks, timber, rails and props: `M_MineColors`, palette `textures/T_MineColorMap.png`, nearest filter, no vertex colours; crystals and the lantern: `M_ManaGlow`, the same palette as emission), `mine_<pos>_Collision` (StaticBody3D + ConcavePolygonShape3D). The rocks are cut at the tile edges, so each file only holds what lies in its own cell.

**Collision.** As on the border tiles, the bank and the hill are a wall about 1.8 m high; rocks, crystals, the cart, lantern and crate block the player. The tunnel can be walked into about 1.4 m deep, for a transporter trigger. The opening is 1.9 m wide and 2.25 m high, centred 0.15 m north of the centre of `mine_C`.

**Scatter mask** (in the JSON, as for the other tiles): 0 on the path, hill, rocks and props, 1 on the clearing's floor, 2 on the bank around it.
