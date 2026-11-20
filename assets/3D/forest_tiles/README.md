# Forest tiles for wave function collapse

18 ground tiles of 4 × 4 m for generating the forest level: forest floor, paths and a border bank that closes off the level. Same painted look as the village ground. Ground only: no trees, bushes or rocks; your MultiMesh scatter places all vegetation, using the scatter mask in the JSON file.

| File | Contents |
|---|---|
| `forest_<name>.glb` | One tile per shape, centre at the origin, in its base orientation. Turn it in the GridMap (around Y only, never mirror). |
| `forest_tiles.json` | Per tile: MeshLibrary item, file, sockets, weight, the old tiles it replaces, scatter mask, triangle counts. |
| `textures/` | `T_ForestFloor_BaseColor/Normal.png`, `T_ForestPath_BaseColor/Normal.png` (1024², one repeat per tile), `T_ForestColorMap.png` (palette of the old bushes and rocks, no longer used by the tiles), `T_MineColorMap.png` (mine entrance). Keep this folder next to the GLBs. |

Export settings as in the village: GLB, +Y up, 1 unit = 1 m. No import script is needed.

## Tiles

One MeshLibrary item per shape. The name ends with the base orientation; turn the cell for the other directions. Every tile is Rotation **FourWay** in the WFC tile set.

| Item | Tile | Base orientation | What it is | Weight (per rotation) |
|---|---|---|---|---|
| 0-3 | `floor_a` … `floor_d` | – | Forest floor, different bumps, tint and texture orientation | 6 (6) |
| 4 | `path_straight_NS` | path north-south | Straight path | 3 (1.5) |
| 5 | `path_curve_NE` | bend between north and east | Quarter-circle bend | 3.2 (0.8) |
| 6 | `path_t_NES` | path leaves north, east and south | T junction | 1.2 (0.3) |
| 7 | `path_cross` | – | Crossroads | 0.15 |
| 8 | `path_end_N` | path from the north | Path ending in a round clearing (3 m) | 0.8 (0.2) |
| 9-12 | `border_full`, `border_full_b`, `_c`, `_d` | – | Border bank all over, each with its own bumps and tint | 1 |
| 13 | `border_edge_N` | border along the north side | Border on one half, floor on the other | 4 (1) |
| 14 | `border_outer_NE` | border along north and east | Outer corner of the level | 2 (0.5) |
| 15 | `border_inner_NE` | border in the north-east corner only | Inner corner of the level | 2 (0.5) |
| 16 | `border_exit_N` | border edge, path out to the north | The way out of the level | 0.08 (0.02) |
| 17 | `border_path_NS` | solid border, path north-south | Leads an exit path to the map edge | 0.04 (0.02) |

N is Godot -Z, E is +X, S is +Z, W is -X. `weight` is the tile's total; godoowave splits it over the tile's distinct rotations, which gives the weight per rotation (the weights of the old set with one file per rotation, so the mix is unchanged). The weights are only a starting point.

## Turning cells

godoowave counts quarter turns around +Y, counter-clockwise seen from above. A tile's base north side then faces:

| Base north faces | Quarter turns | GridMap orientation index |
|---|---|---|
| north | 0 | 0 |
| west | 1 | 16 |
| south | 2 | 10 |
| east | 3 | 22 |

Only these four orientations count: a tilted or mirrored cell is ignored by the generator. For example `border_edge_N` at orientation 10 is a border along the south side, and `path_curve_NE` at orientation 22 bends between east and south.

**Old set.** Until 2026-11-20 every rotation was its own file (41 tiles). `replaces` in the JSON lists, per tile, the old names it covers with their orientation index, e.g. `path_curve_ES` = `path_curve_NE` at 22, `border_edge_W` = `border_edge_N` at 16. Use it to remap a GridMap painted with the old MeshLibrary. The turned tile has the same layout (corners, path, border) as the old file; only the small bumps and tint patches inside the tile differ, because the old rotated files had their own noise.

## Sockets

Each edge has a label, read **clockwise around the tile** seen from above: the north edge west → east, the east edge north → south, the south edge east → west, the west edge south → north. That reading stays the same however the tile is turned, so godoowave can turn the labels with the tile.

| Label | Edge | Fits |
|---|---|---|
| `FF` | Forest floor | `FF` |
| `FFP` | Forest floor with the 2 m path in the middle | `FFP` |
| `BB` | Border bank | `BB` |
| `BBP` | Border with a path through it | `BBP` |
| `FB+` | Half floor, half border: floor first (clockwise) | `FB-` |
| `FB-` | Half floor, half border: border first (clockwise) | `FB+` |

Two neighbours read their shared edge in opposite directions, so a half edge is one-sided: `FB+` fits `FB-`. In the WFC tile set: N = `SocketNegZ`, E = `SocketPosX`, S = `SocketPosZ`, W = `SocketNegX` (base orientation, as in the JSON); up and down `ground`.

For the map edge, treat cells outside the map as `BB`, so the level is closed by border. Where the player should leave, use `BBP` for that outside edge (pin the cell to a `border_exit` or `border_path` tile).

The tiles are built from their four corners (floor or border) plus the path sides. The two "diagonal" corner layouts (border in two opposite corners only) are not in the set, so WFC will avoid them. With a closed map edge and these weights, a test generator with the same rules (turned tiles, one-sided labels) solved 12 × 9 and 20 × 16 maps on its first attempt; about half of the cells were turned tiles.

## Ground, path and border

- **Heights.** The forest floor is at y = 0 on every tile edge, with gentle bumps (up to ±7 cm) inside a tile. Paths are 2 m wide and lie 0.08 m lower, with a 0.2 m soft bevel, as on the village roads. The border bank is 0.5 m high; paths through the border cut a gully through it.
- **Seams.** On every edge, height, normal and tint depend only on the edge's label, and nothing slopes across the edge, so matching tiles meet without a crack, crease or colour step, however they are turned. The textures repeat once per tile, and their outer band (about 0.2 m) is the same under all 8 rotations and mirrors. That lets each tile use its own texture orientation without a seam, so the repeat is less obvious.
- **No undergrowth.** Since 2026-11-14 the tiles carry no bushes or rocks; the border bank is bare ground. Scatter on mask value 2 to fill it.
- **Collision.** `<name>_Collision` imports as a `StaticBody3D` with a `ConcavePolygonShape3D` (0.25 m grid, 512 triangles). It follows the ground on floor and paths. On the border it rises into a wall about 1.8 m above the bank, so the player can only cross the border along a path.

Nodes per file: `Tile_<name>` (root) → `<name>_Ground`, `<name>_Collision`.

## Scatter mask

`scatter_mask` in the JSON gives each tile 16 × 16 cells of 0.25 m in its **base orientation**, rows north → south (Godot -Z → +Z), columns west → east:

| Value | Meaning |
|---|---|
| `0` | Keep clear: path and 0.6 m beside it |
| `1` | Forest floor |
| `2` | Border bank (place trees densely) |

For a turned cell, turn the point back into the tile's own frame first:

```gdscript
# masks[item] = scatter_mask of that MeshLibrary item (from the JSON)
func mask_at(gm: GridMap, world: Vector3, masks) -> int:
    var p := gm.to_local(world)
    var cell := gm.local_to_map(p)
    var q := gm.get_cell_item_basis(cell).inverse() * (p - gm.map_to_local(cell))    # tile frame, centre at 0
    var cx = clampi(int((q.x + 2.0) / 0.25), 0, 15)
    var cz = clampi(int((q.z + 2.0) / 0.25), 0, 15)
    return int(masks[gm.get_cell_item(cell)][cz][cx])
```

## Materials and import

| Material | Used on | Notes |
|---|---|---|
| `M_ForestFloor` | ground | `T_ForestFloor_*` × vertex colour (`COLOR_0`): darker shady bank, soil on its slope, trodden grass beside paths, soft patches. Roughness 0.95. |
| `M_ForestPath` | paths and their bevels | `T_ForestPath_*` × vertex colour. Roughness 0.9. |
| `M_ForestFloorFirst` | one small triangle | See below. |

**Vertex colours without an import script.** Godot 4.7 switches on *Vertex Color → Use as Albedo* for a surface only when the surface before it in the same mesh also has vertex colours, so the first surface misses out. Each `_Ground` mesh therefore starts with a single small floor triangle (`M_ForestFloorFirst`), placed where the tint is 1.0 (at a floor corner), so it looks like the rest. Tiles without a floor corner (`border_full` and its variants) get an extra small triangle 5 cm under the ground instead, out of sight. The floor and path surfaces after it get the flag automatically, as in the village terrain.

Ground textures use Godot's default import (VRAM compressed with mipmaps; normal maps are detected). Normal maps are OpenGL style (green up), scale 0.5.

Per tile: ground 2,048 triangles (2,049 on the `border_full` tiles), collider 512. All 18 GLBs: 1.8 MB.

## Tested

Godot 4.7 headless, fresh project (2026-11-20): no import errors or warnings; 18 items with one mesh and one `ConcavePolygonShape3D` each; the floor and path surfaces have *Use as Albedo* on (25 surfaces), the first-surface triangles not. Three WFC maps (12 × 9 twice, 20 × 16) with turned cells placed at the orientation indices above: 40,754 ray pairs 2 mm either side of every seam hit the colliders on both sides with no holes; the largest difference, 6.6 mm, is the slope of the border wall between points 4 mm apart. All 41 old tiles against the new tile at its `replaces` orientation: 296 probes (corners, and edge midpoints on floor, border or path) give the same ground type. Render: `.work/renders/forest_tiles/rot_v1_game_camera.png`.

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
