# Wizard school interior: dungeon tiles

29 tiles of 4 × 4 m for generating the inside of the wizard school with **godoodungeon**: stone rooms floating over a dark chasm, joined by stone bridges, with platforming on the way. Low-poly and flat-shaded, coloured by one palette texture, in cool castle stone with blue and purple banners, gold trim and glowing blue runes. One GridMap layer: every floor is at y = 0.

| File | Contents |
|---|---|
| `school_<name>.glb` | One tile per shape, centre at the node origin, floor top at y = 0, in its base orientation. Turn it in the GridMap (around Y only, never mirror). |
| `school_door_leaf.glb` | One door leaf of `room_door_locked`, origin at its hinge. Use it twice: the west leaf as is, the east leaf turned 180°. |
| `school_moving_platform.glb` | The platform for the `corr_bridge_mover` tile (1.4 × 1.4 m, top at y = 0), moved by your game scene. |
| `school_<prop>.glb` | The moving or dangerous parts of the element challenges (see below). |
| `school_tiles.json` | Per tile: item number, old name, file, tile set, role, sockets, weights, triangle counts; the godoodungeon tile sets; platforming and challenge sizes; the renames and removed items of 2026-11-21. |
| `textures/T_SchoolColorMap.png` | The palette (32 × 32 px, 31 colours, nearest filter) with its lossless `.import` file. Keep it next to the GLBs. |
| `textures/T_School<Fire\|Water\|Earth>ColorMap.png` | Element palettes for the trial chambers (see below), same layout. |

GLB, +Y up, 1 unit = 1 m, no vertex colours, no import script. Nodes per file: `Tile_<name>` → `<name>_Mesh` (surface `M_SchoolColors`, plus `M_SchoolGlow` for runes, flames and crystals: the same palette as emission) and `<name>_Collision` (StaticBody3D + ConcavePolygonShape3D; the chasm tiles have none).

## Names

A tile's name says which godoodungeon tile set it belongs to:

- **`room_*`**: the room tile set. Floors, `room_wall`, `room_corner`, and **`room_door`, the Door tile**. To build a room by hand (for a Built room box), use only `room_*` tiles, and put `room_door` on the wall ring, not in a corner. **`room_door_locked`** is the same doorway with a doorframe, for a door that opens with a code.
- **`corr_*`**: the corridor tile set. `corr_bridge*` straights, `corr_bend`, `corr_pad_*` bridge pads, `corr_chasm*` (the empty tile), and `corr_<element>_<challenge>` for the element chambers.

**Renamed and removed on 2026-11-21.** The tiles had names from the older godoowave set (`room_wooo`, `landing_gwgw`, `bridge_NS`, `dungeon_door`, …). `renamed_2026_11_21` in the JSON maps every old name to its new one. The godoowave-only tiles were removed: the gate rooms, the 3-wall rooms, the dead-end landing, the 23 chasm socket variants, and `door_N` / `stairs_S`. All other items **kept their numbers**, so the MeshLibrary has gaps (8, 10–16, 29–53; listed under `removed_items_2026_11_21`). GridMaps and tile sets that use item numbers keep working. The meshes are byte-identical to the previous export. The earlier GLBs are in `blend/school_interior/versions/glb_before_rename/`.

## One tile per shape, turned in the GridMap

Every shape is one MeshLibrary item and **Rotation FourWay** in the tile sets: godoodungeon turns tiles itself, and when you build a room by hand you turn the cell instead of looking for another tile.

Each shape is built with its **south side open**. Turned, the full build goes to every side: a wall turned to the south is the 1 m wall with merlons, and a door turned to the south has the full arch. The palette shading is the same after any quarter turn.

Base orientations: `room_wall` has its wall on the north side, `room_corner` on the north and west, and `room_door` its outside to the north. `corr_bridge*`, `corr_pad_straight` and the challenges run north–south. `corr_bend` opens north and east, `corr_pad_bend` north and west, and `corr_pad_t` north, south and west.

godoowave counts quarter turns around +Y, counter-clockwise seen from above. A tile's base north side then faces:

| Base north faces | Quarter turns | GridMap orientation index |
|---|---|---|
| north | 0 | 0 |
| west | 1 | 16 |
| south | 2 | 10 |
| east | 3 | 22 |

Only these four orientations count: a tilted or mirrored cell is ignored by the generator.

## Sockets

N = −Z (top of the screen; the game camera looks north), E = +X, S = +Z, W = −X. In the WFC tile fields (base orientation, as in the JSON): N = `SocketNegZ`, E = `SocketPosX`, S = `SocketPosZ`, W = `SocketNegX`; up and down `ground`. Plain labels fit the same label; `x+` fits only `x−`.

| Label | On | Fits |
|---|---|---|
| `room` | open sides inside a room | `room` |
| `outside+` | room walls, and the outside of `room_door` (Room Edge Socket `outside`) | `outside−`, which the room bounds supply: put `outside−` in the room set's External Sockets |
| `path` | open corridor sides (Path Socket) | `path` |
| `x` | closed corridor sides, and every side of the chasm | `x` |

## Tiles

Weights: the JSON `weight` is the tile's total; the generator splits it over the tile's distinct rotations, which gives `weight_per_rotation`. The game's tile sets may have their own tuned weights.

| Item | Tile | Was | Base orientation | What it is | Weight (per rotation) |
|---|---|---|---|---|---|
| 0, 1 | `room_floor`, `room_floor_b` | `room_oooo_a`, `_b` | – | Open floor of a room (`_b` with a purple rug) | 1.3 |
| 2–6 | `room_circle`, `room_dais`, `room_pit`, `room_pillar`, `room_tiers` | `room_oooo_<feature>` | – | Open floor with a glowing rune circle, a raised dais (+0.3 m) with a lectern, a 2 × 2 m pit into the chasm, a column with a blue fire, or two blocks (+0.3 / +0.6 m) with a small chest | 0.3–0.5 |
| 7 | `room_wall` | `room_wooo` | wall north | Room edge | 4.2 (1.05) |
| 9 | `room_corner` | `room_woow` | walls north and west | Room corner | 2.7 (0.68) |
| 17 | `corr_pad_bend` | `landing_gwwg` | arms north and west | Bridge pad (2.6 m square, parapets, rune posts at the corners, a pier) as a bend | 1.0 |
| 18 | `corr_pad_straight` | `landing_gwgw` | arms north and south | Bridge pad on a straight | 0.7 |
| 19 | `corr_pad_t` | `landing_gwgg` | arms north, south and west | Bridge pad as a T | 0.48 |
| 20 | `corr_pad_cross` | `landing_gggg` | four arms | Bridge pad as a crossing | 0.08 |
| 21 | `corr_bridge` | `bridge_NS` | along north–south | Stone bridge, 2 m wide, low parapets, a pier into the dark | 3.0 (1.5) |
| 22 | `corr_bridge_broken` | `broken_NS` | along north–south | Broken bridge: **jump the gap** (1.15 / 1.25 m) | 0.7 (0.35) |
| 23 | `corr_bridge_stones` | `stones_NS` | along north–south | **Stepping stone**: a pillar 0.3 m higher, 1 m from each ledge | 0.6 (0.3) |
| 24 | `corr_bridge_steps` | `steps_NS` | along north–south | **Height step**: the middle of the bridge is raised 0.3 m | 0.6 (0.3) |
| 25 | `corr_bridge_mover` | `mover_NS` | along north–south | **Moving platform** crossing: ledges at both ends, 3.2 m gap, rune posts | 0.4 (0.2) |
| 26 | `corr_bend` | `bridge_NE` | north to east | Bridge corner | 1.4 (0.35) |
| 27, 28 | `corr_chasm`, `corr_chasm_b` | `chasm_a`, `_b` | – | The chasm, the empty tile: dark bottom at −6 m (`_b` with floating rocks and violet crystals) | 0.4, 0.1 |
| 54 | `room_door` | `dungeon_door` | outside north | **The Door tile**: a gatehouse with two towers 1.45 m high with merlons, an arch, and blue sconces on the room side | 0 (the layout places it) |
| 62 | `room_door_locked` | — | outside north | **The Locked Door tile**: `room_door` with a stone threshold and hinge pins in the arch. The leaves are `school_door_leaf.glb`, in its marker scene | 0 (the layout places it) |
| 55–61 | `corr_<element>_<challenge>` | `<element>_<challenge>_NS` | along north–south | Element challenges (see below) | 0.7 (0.35) |

## godoodungeon setup

godoodungeon lays out rectangular rooms, solves each one with the room tile set, puts a door in the walls per connection, and routes a corridor from door to door; here the corridors are the bridges. Every room is reachable. `school_tiles.json` → `dungeon` lists both tile sets (item, rotation, weight, sockets); every tile FourWay.

| Tile set | Contents |
|---|---|
| Rooms (11) | The 7 floors, `room_wall`, `room_corner`, `room_door` and `room_door_locked` (both weight 0) |
| Corridors (12) | Straight: `corr_bridge`, `_broken`, `_stones`, `_steps`, `_mover`, `corr_pad_straight`. Bend: `corr_bend`, `corr_pad_bend`. T: `corr_pad_t`. Cross: `corr_pad_cross`. Empty: `corr_chasm`, `corr_chasm_b`. An element chamber adds its own `corr_<element>_*` tiles. |

- **Door:** `room_door`, MeshLibrary item 54. godoodungeon turns it for each side.
- **Locked Door:** `room_door_locked`, MeshLibrary item 62, weight 0, in the room set too. Its marker scene holds the two leaves: hinges at x = ±1 m and z = −1.725 m (Godot, the base north build), the east leaf turned 180°. Opening them means turning them about Y so they swing into the room (towards +Z), which keeps them inside the tile. `school_tiles.json` → `dungeon.locked_door`.
- **Room size:** walls included, so a room is at least 3 × 3 cells. 3–5 cells (12–20 m) suits the camera.
- **Settings:** GridMap `cell_size` (4, 1, 4), `cell_center_y` off, one layer. Room Edge Socket `outside`, Path Socket `path`, Gap 3, Wall Clearance 1. Corridors keep a cell of chasm from the room walls, except at the doors.

## Platforming

Tuned to the current player (jump velocity 3, speed 3, default gravity): a jump is about 0.46 m high and 1.8 m long.

- **Gaps** of 1.15–1.25 m on broken bridges. The chasm under them has no floor.
- **Height steps** are 0.3 m: the stepping stone, the raised bridge, the dais and the tiers. The player can't walk up them and has to jump.
- **Moving platforms:** `school_moving_platform.glb` should move along the tile's axis between the two ledges. With the tile centre at the origin, the platform centre goes from −0.9 m to +0.9 m along Z (the N–S build), top at y = 0. An `AnimatableBody3D` with an `AnimationPlayer` looping its position needs no script. Put that scene as the **Marker** of `corr_bridge_mover`: the generator places markers turned with their cell, so one scene covers both directions.
- **Falling:** nothing catches the player in the chasm. Put an `Area3D` kill / respawn zone under the level, for example at y = −3.
- **Walls:** walls are 1 m with merlons on every side. Wall colliders go up to 1.35 m and bridge parapets to 0.95 m, so the player can't hop off a bridge or over a wall.

## Look and camera

Walls are 1 m with merlons on every side, also on the south side towards the camera: about as much as the mana mine's rock edge hides. The door's arch is full height on every side; turned to the south you look through it, and it hides about 1.5 m of floor behind it. Walls built on the north carry banners on their inner face; turned, they end up on other sides (a banner turned to the south faces away from the camera). The game's occlusion fader works on GridMaps if a tall part hides the player.

There are no lights in the tiles. The glow material only makes runes, flames and crystals bright, so add your own lights, for example a dim blue fill with point lights at the doors.

## Element palettes (Fire, Water, Earth)

`textures/T_SchoolFireColorMap.png`, `T_SchoolWaterColorMap.png` and `T_SchoolEarthColorMap.png` are the palettes for the three trial chambers (I.3). They have the same layout as `T_SchoolColorMap.png`, with other colours only for the banners and carpets and for the crystals: rune orbs on posts and above doors, wall sconce spikes, the rune circle, and the crystals floating over the chasm. Stone and chasm stay the same; give each chamber its mood with lighting.

There are no element GLBs: the meshes are identical. In Godot, make one MeshLibrary per element from the base MeshLibrary, with the two palette materials (`M_SchoolColors`, `M_SchoolGlow`) pointing at the element palette instead (albedo, and emission for the glow material). The item numbers stay the same, so a chamber changes element by swapping its GridMap's MeshLibrary. `school_tiles.json` lists the palettes under `element_palettes`.

## Element challenges (I.3 trial chambers)

`school_corr_<element>_<challenge>.glb`: 7 extra bridge tiles, items 55–61 after `room_door`, with the sockets of `corr_bridge`, built along north–south and turned in the GridMap (FourWay). Each holds the static part of an element-specific platforming challenge. The moving or dangerous parts are props, moved by marker scenes in the game (`challenges/<tile name>.tscn`). The generator places a marker turned with its cell, so one marker scene per challenge covers both directions. Each element chamber's corridor tile set uses only its own challenges (`dungeon` → `element_corridor_tiles`). `school_tiles.json` → `challenges` lists the tiles, props and layout numbers.

| Element | Tile | Challenge | Static part | Prop(s) |
|---|---|---|---|---|
| Fire | `corr_fire_jets` | **Flame jets**: three vent rows across the deck burst fire in turns; wait between rows | full deck, vents, nozzles | `school_flame_jet.glb` |
| Fire | `corr_fire_sinking` | **Sinking stones**: two stepping stones between end ledges sink and rise in turns | ledges, shafts below | `school_sinking_stone.glb` |
| Fire | `corr_fire_flamebar` | **Flame bar**: a bar of fire sweeps around a pivot post on a round pad; jump over it | pad with curb, post | `school_flame_bar.glb` |
| Water | `corr_water_waterfall` | **Waterfall**: a curtain of water falls from an arch on a timer and pushes the player back | deck, arch, drain | `school_water_curtain.glb` |
| Water | `corr_water_raft` | **Drifting raft**: a raft drifts in from the side, lines up with the bridge, then drifts away | ledges, bollards | `school_raft.glb` |
| Earth | `corr_earth_crumble` | **Crumbling ledge**: three small blocks between end ledges fall after you land, then come back | ledges, broken piers | `school_crumble_block.glb` |
| Earth | `corr_earth_rocks` | **Falling rocks**: rocks drop on three spots after a growing shadow | cracked deck | `school_falling_rock.glb` |

Props are centred at their own origin with the walking surface at y = 0 (the flame and curtain props stand on y = 0), in the N–S frame of their tile.

## Checked

- **2026-11-20:** Godot 4.7 headless, fresh project: 0 import errors or warnings. Every old one-tile-per-rotation tile was compared with its new tile at the matching orientation: the same floor, raised parts, barriers and openings everywhere.
- **2026-11-21 (rename):** all 37 GLBs (29 tiles, the platform, 7 props) compared with the previous export: identical binary data and glTF structure apart from node names. The godoodungeon tile sets in the JSON equal the previous ones apart from names.
