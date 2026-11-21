# Mana mine tiles (godoodungeon)

A tile set for the inside of the magical mana mine, made for **godoodungeon**: rooms are mine halls, corridors are rail tunnels. Underground mine, timber shoring, glowing blue mana crystals and natural stone pillars, in the same palette as the forest mine entrance (`forest_tiles/forest_mine_*`).

- 23 tiles, `mine_<name>.glb`, one per MeshLibrary item (the `item` numbers in `mana_mine_tiles.json`).
- **One item per shape, turned in the GridMap.** Each tile is built in one base orientation (below). Rotate the cell around Y to use it facing another way, by hand (the GridMap rotate keys) or by godoodungeon. Every tile is Rotation FourWay in the tile sets.
- Tile 4 × 4 m, one GridMap layer, floor at y = 0. Tile centre at the node origin; north = -Z (top of the screen).
- Each GLB holds one mesh with two materials: `M_ManaMineColors` (the palette) and `M_ManaMineGlow` (the same palette, emissive: crystals, lantern glass, the mana pool, crystal veins). It also has one collider, `*-colonly`, which Godot imports as a `StaticBody3D` with a `ConcavePolygonShape3D`.
- Palette `textures/T_ManaMineColorMap.png` (32 × 32, nearest filtering, lossless `.import`).

## The look
The rock is cut away at 1.0 m, like a cutaway of the mountain. Every closed tile edge is solid rock with a dark top at 1.0 m, and every open edge has the same profile: 2 m of floor in the middle, with rock faces on both sides. That's why any two tiles join without a seam, however they are turned. The rock faces are steep (about 70°), so the player can't climb them.

Every wall has the full timber shoring: posts, a header beam and plank lagging in some bays. On a wall turned to the south the posts and planks face away from the camera and the beam reads as a timber edge; it hides about 0.4 m more of the floor than bare rock would. The palette shading doesn't depend on direction, so a turned tile looks the same as one built that way.

## Tiles

| Item | Tile | Base orientation | What it is |
|---|---|---|---|
| 0-7 | `room_floor`, `room_rubble`, `room_crates`, `room_support`, `room_pillar`, `room_crystal`, `room_cart`, `room_pool` | open on all sides | Hall floor: plain, rubble, crates (crates, barrel, lamp), support (timber roof prop with a lantern), pillar (natural stone column, 3 m), crystal (big mana crystal cluster), cart (ore cart full of crystals on a short track), pool (glowing mana pool, not walkable) |
| 8, 9 | `room_wall`, `room_wall_b` | wall on the north side | Hall wall; two variants with different lagging, so a long wall doesn't repeat |
| 10 | `room_wall_vein` | wall on the north side | Hall wall with mana crystals growing from the rock |
| 11 | `room_corner` | walls on the north and west sides | Hall corner |
| 12 | `room_door` (was `mine_door`) | outside to the north | Timber portal in a wall; the rail track comes in from the corridor and ends just inside the hall |
| 13-16 | `corr_straight_frame`, `_plain`, `_crystal`, `_lamp` | open north and south | Rail tunnel: timber set across, plain, crystals, lantern post |
| 17 | `corr_bend` | open north and east | Curved track |
| 18 | `corr_t` | open north, east and south | T piece with points into the side tunnel |
| 19 | `corr_cross` | open on all sides | Crossing |
| 20-22 | `corr_rock`, `corr_rock_b`, `corr_rock_c` (were `rock_a`, `_b`, `_c`) | closed on all sides | Solid rock (the empty tile): plain, with a crystal outcrop, or with a glowing vein |

## Turning cells
godoowave counts quarter turns around +Y, counter-clockwise seen from above. A tile's base north side then faces:

| Base north faces | Quarter turns | GridMap orientation index |
|---|---|---|
| north | 0 | 0 |
| west | 1 | 16 |
| south | 2 | 10 |
| east | 3 | 22 |

Only these four orientations count: a tilted or mirrored cell is ignored by the generator. So a south wall is `room_wall` at orientation 10, and a south-east corner is `room_corner` at orientation 10 (north + west walls become south + east).

## godoodungeon setup
`mana_mine_tiles.json` → `godoodungeon` lists both tile sets with their items, weights, rotations and sockets.

1. Build a MeshLibrary from the GLBs, with item number = `item` (mesh plus its concave shape).
2. **Room tile set** (`WfcTileSet`): the `room_tiles`, using the same MeshLibrary as the GridMap. Set the sockets as in the JSON (base orientation): north = `SocketNegZ`, east = `SocketPosX`, south = `SocketPosZ`, west = `SocketNegX`. Add `outside-` to **External Sockets**. Every tile is Rotation **FourWay**; `mine_door` has weight 0 (the layout places it).
3. **Corridor tile set**: the `corridor_tiles`, also all FourWay. Open sides are `path` and closed sides are `x`. The `corr_rock*` tiles are the empty tiles.
4. Dialog: Room Edge Socket `outside`, Path Socket `path`, Door `room_door`. Suggested: room size 3 × 3 to 7 × 7, Gap 3, Margin 1, Wall Clearance 1, Max Doors Per Room 3.

**Weights.** `weight` in the JSON is the tile's total. godoowave splits it over the tile's distinct rotations (a wall has 4, a straight 2, a floor or rock 1), which gives `weight_per_rotation`. Those are the weights of the old set with one tile per rotation, so the mix is unchanged. Symmetric tiles are FourWay too: their rotations merge into one, and a cell you turned by hand is still read as that tile.

## Lighting
The tiles carry no lights. Use a dark environment with low ambient light, glow on, and a few blue `OmniLight3D`s near crystals and lanterns. For example, give the crystal, pool, lamp and T tiles a light scene as a godoodungeon **Marker**.

## Checked
- Godot 4.7, headless import: 0 errors and 0 warnings. 23 tiles, each with 1 mesh and 1 concave shape, no vertex colours, 14 tiles with an emissive surface.
- Turned in a GridMap with the orientation indices above, over 10 godoodungeon-like sample layouts: 7,194 downward rays all hit floor along every corridor and door path and rock on every closed edge.
- Height field seams over 20 sample layouts with turned tiles: max difference 1e-15 m.
- The tile set rules (as in `DungeonTileRules.cs`) were checked with the rotations expanded: one-sided `outside+`, a door with exactly one `outside+` face, straights both ways, bends, T pieces, a cross, and an empty tile with weight > 0.

## Names (2026-11-21)

A tile's name says which godoodungeon tile set it belongs to, the same scheme as the school tiles: `room_*` = room tile set (`room_door` is the Door tile), `corr_*` = corridor tile set (`corr_rock*` = the empty tile). Renamed on 2026-11-21: `mine_door` → `room_door`, `rock_a` / `rock_b` / `rock_c` → `corr_rock` / `corr_rock_b` / `corr_rock_c` (JSON `renamed_2026_11_21`). Item numbers and meshes are unchanged: all 23 GLBs are identical to the previous export apart from node names (previous export in `blend/mana_mine/versions/glb_before_rename/`).
