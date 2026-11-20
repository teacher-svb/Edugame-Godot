# Wizard school interior: WFC tiles

31 tiles of 4 × 4 m (plus the dungeon door and 7 challenge bridges) for generating the inside of the wizard school with the WFC generator: stone rooms floating over a dark chasm, joined by stone bridges, with platforming on the way. Low-poly and flat-shaded, coloured by one palette texture, in cool castle stone with blue and purple banners, gold trim and glowing blue runes. One GridMap layer: every floor is at y = 0.

| File | Contents |
|---|---|
| `school_<name>.glb` | One tile per shape, centre at the node origin, floor top at y = 0, in its base orientation. Turn it in the GridMap (around Y only, never mirror). |
| `school_moving_platform.glb` | The platform for the `mover_NS` tile (1.4 × 1.4 m, top at y = 0), moved by your game scene. |
| `school_tiles.json` | Per tile: item number, file, kind, rotation, sockets, weights, the old tiles it replaces, triangle counts; platforming sizes; the remap table for the old item numbers. |
| `textures/T_SchoolColorMap.png` | The palette (32 × 32 px, 31 colours, nearest filter) with its lossless `.import` file. Keep it next to the GLBs. |
| `textures/T_School<Fire\|Water\|Earth>ColorMap.png` | Element palettes for the trial chambers (see below), same layout. |

GLB, +Y up, 1 unit = 1 m, no vertex colours, no import script. Nodes per file: `Tile_<name>` → `<name>_Mesh` (surface `M_SchoolColors`, plus `M_SchoolGlow` for runes, flames and crystals: the same palette as emission) and `<name>_Collision` (StaticBody3D + ConcavePolygonShape3D; the chasm tiles have none).

## One tile per shape, turned in the GridMap

Since 2026-11-20 every shape is one MeshLibrary item, and every tile is **Rotation FourWay** in the WFC tile sets: godoowave and godoodungeon turn tiles themselves, and when you build a room by hand you turn the cell instead of looking for another tile.

Each shape is built with its **south side open** (a wall or gate on the north where it has one). Turned, the full build goes to every side: a wall turned to the south is the 1 m wall with merlons (the old low south curb is gone), and a gate turned to the south has the full arch. The palette shading is the same after any quarter turn, so a turned tile looks like one built that way. Only `stairs_S` still has its low curb and stairwell on the south side; it's a tile of its own.

godoowave counts quarter turns around +Y, counter-clockwise seen from above. A tile's base north side then faces:

| Base north faces | Quarter turns | GridMap orientation index |
|---|---|---|
| north | 0 | 0 |
| west | 1 | 16 |
| south | 2 | 10 |
| east | 3 | 22 |

Only these four orientations count: a tilted or mirrored cell is ignored by the generator.

**Old set.** Until 2026-11-20 every rotation was its own tile (78 tiles, items 0–115 with the chasm variants, the dungeon door and the challenges). `replaces` in the JSON lists, per tile, the old names it covers with their orientation index (for example `room_owoo` = `room_wooo` at 22), and `remap_old_items` maps every old item number to `[new item, orientation, keep]`; `keep` is true for the tiles that were already turned in the GridMap (the chasm socket variants and the dungeon door), whose cells keep their orientation. Every old tile was compared with its new tile at that orientation in Godot: the same floor, raised parts, barriers and openings everywhere.

## Sockets

N = −Z (top of the screen; the game camera looks north), E = +X, S = +Z, W = −X. In the WFC tile fields (base orientation, as in the JSON): N = `SocketNegZ`, E = `SocketPosX`, S = `SocketPosZ`, W = `SocketNegX`; up and down `ground`. Plain labels fit the same label; `x+` fits only `x−`. The labels don't depend on which way an edge is read, so they turn with the tile.

| Label | On | Fits |
|---|---|---|
| `room` | open sides inside a room | `room` |
| `wall+` | walls of rooms; closed sides of landings | a chasm side `wall−` only, so two rooms never touch wall to wall |
| `gate+` | gates of rooms and landings | a bridge end `gate−` only, so rooms connect over bridges, never by a doorway |
| `gate−` | bridge ends (straight, corner, broken, stones, steps, mover) | `gate+` |
| `flank+` | the sides of a bridge | a chasm side `flank−` only, so a bridge always has chasm on both sides |
| `void` | chasm against chasm; the door side of `door_N` / `stairs_S`; the map boundary | `void` |

The chasm is one mesh (`chasm_a`) with **23 socket variants**: every mix of `void` / `wall−` / `flank−` on its four sides, FourWay. The generator needs one MeshLibrary item per tile, so each variant is its own item (31–53) with the `chasm_a` mesh. `school_tiles.json` lists them under `chasm_variants`. Weight 0.5 per orientation.

Bridges can't join end to end, because that would also let two rooms meet gate to gate. A crossing longer than one cell goes bridge → landing → bridge, and the landings are the pads in between.

Rooms and landings are named after the sides of their base orientation, `room_<NESW>` / `landing_<NESW>`, with `o` = open (room), `w` = wall, `g` = gate. `room_gwow` has a gate on the north, a wall on the east, an open south and a wall on the west; turned, it covers the same shape facing any way.

## Tiles

Weights: the JSON `weight` is the tile's total; godoowave splits it over the tile's distinct rotations, which gives `weight_per_rotation` (the weights of the old set, so the mix is unchanged).

| Item | Tile | Base orientation | What it is | Weight (per rotation) |
|---|---|---|---|---|
| 0, 1 | `room_oooo_a`, `_b` | – | Open floor of a room (`_b` with a purple rug) | 1.3 |
| 2–6 | `room_oooo_circle`, `_dais`, `_pit`, `_pillar`, `_tiers` | – | Open floor with a glowing rune circle, a raised dais (+0.3 m) with a lectern, a 2 × 2 m pit into the chasm, a column with a blue fire, or two blocks (+0.3 / +0.6 m) with a small chest | 0.3–0.5 |
| 7 | `room_wooo` | wall north | Room edge | 4.2 (1.05) |
| 8 | `room_gooo` | gate north | Room edge with a gate | 2.1 (0.53) |
| 9 | `room_woow` | walls north and west | Room corner | 2.7 (0.68) |
| 10, 11 | `room_goow`, `room_gwoo` | gate north, wall west / east | Corner with a gate | 1.35 (0.34) |
| 12 | `room_wwow` | walls north, east, west | 3-wall end | 0.18 (0.05) |
| 13–15 | `room_wgow`, `room_gwow`, `room_wwog` | 3 closed sides, one a gate | 3-wall end with a gate | 0.09 (0.02) |
| 16–20 | `landing_gwww`, `_gwwg`, `_gwgw`, `_gwgg`, `_gggg` | gates as named | A bridge pad (2.6 m square, parapets, rune posts at the corners, a pier) with an arm to each of its 1–4 gates | 0.08–1.0 |
| 21 | `bridge_NS` | along north–south | Stone bridge, 2 m wide, low parapets, a pier into the dark | 3.0 (1.5) |
| 22 | `broken_NS` | along north–south | Broken bridge: **jump the gap** (1.15 / 1.25 m) | 0.7 (0.35) |
| 23 | `stones_NS` | along north–south | **Stepping stone**: a pillar 0.3 m higher, 1 m from each ledge | 0.6 (0.3) |
| 24 | `steps_NS` | along north–south | **Height step**: the middle of the bridge is raised 0.3 m | 0.6 (0.3) |
| 25 | `mover_NS` | along north–south | **Moving platform** crossing: ledges at both ends, 3.2 m gap, rune posts | 0.4 (0.2) |
| 26 | `bridge_NE` | north to east | Bridge corner | 1.4 (0.35) |
| 27, 28 | `chasm_a`, `_b` | – | The chasm: dark bottom at −6 m (`_b` with floating rocks and violet crystals); `chasm_a` also carries the socket variants | 0.4, 0.1 |
| 29 | `door_N` | door to the north | Level entrance or exit: an alcove sticking out of a room (walls on both sides, the room behind it) with a wooden door. Turn it for the east, west or south. | 0 (place by hand) |
| 30 | `stairs_S` | stairs down to the south | Level exit: stairs going down through a low curb | 0 (place by hand) |

The weights are a starting point. In tests on 12 × 9 they gave 2–5 separate rooms per seed. Rooms are always separate, but nothing guarantees every room gets a bridge, so some seeds have an island room: try another seed.

## Platforming

Tuned to the current player (jump velocity 3, speed 3, default gravity): a jump is about 0.46 m high and 1.8 m long.

- **Gaps** of 1.15–1.25 m on broken bridges. The chasm under them has no floor.
- **Height steps** are 0.3 m: the stepping stone, the raised bridge, the dais and the tiers. The player can't walk up them and has to jump.
- **Moving platforms:** `school_moving_platform.glb` should move along the tile's axis between the two ledges. With the tile centre at the origin, the platform centre goes from −0.9 m to +0.9 m along Z (the N–S build), top at y = 0. An `AnimatableBody3D` with an `AnimationPlayer` looping its position needs no script. Put that scene as the **Marker** of `mover_NS`: godoowave places markers turned with their cell, so one scene covers both directions.
- **Falling:** nothing catches the player in the chasm. Put an `Area3D` kill / respawn zone under the level, for example at y = −3.
- **Walls:** walls are 1 m with merlons on every side (`stairs_S` keeps a 0.45 m curb). Wall colliders go up to 1.35 m and curbs and bridge parapets to 0.95 m, so the player can't hop off a bridge or over a wall. The stairs in `stairs_S` collide as a ramp.

## Look and camera

Walls are 1 m with merlons on every side, also on the south side towards the camera: about as much as the mana mine's rock edge hides. Gates have the full arch (3.2 m) on every side; on the south side you look through the arch, and it hides about 1.5 m of floor behind it. Other tall parts are thin: the column and the door frames. Walls built on the north carry banners on their inner face, and east and west walls have blue-flame sconces; turned, they end up on other sides (a banner turned to the south faces away from the camera). The game's occlusion fader works on GridMaps if a tall part hides the player.

There are no lights in the tiles. The glow material only makes runes, flames and crystals bright, so add your own lights, for example a dim blue fill with point lights at the gates.

## Godot setup (suggested)

GridMap `cell_size` (4, 1, 4), `cell_center_y` off, one layer. WFC settings: all four side boundaries `void`, so there is chasm along the map edge. Every tile Rotation FourWay. Place `door_N` / `stairs_S` by hand on the edge row, turned so the door side faces out; the generator puts a room behind it.

## godoodungeon (generated rooms joined by bridges)

The same tiles also work with **godoodungeon**. It lays out rectangular rooms, solves each one with a room tile set, puts doors in the walls, and routes a corridor from door to door; here the corridors are the bridges. Every room is reachable.

`school_tiles.json` → `dungeon` lists both tile sets (item, rotation, weight, sockets); every tile FourWay:

| Tile set | Contents | Sockets |
|---|---|---|
| Rooms (10) | Floors (7 variants), the wall `room_wooo`, the corner `room_woow`, and the door | Walls face out with `outside+` (the room bounds supply `outside−`; put `outside−` in External Sockets), everything else `room` |
| Corridors (12) | Straight: bridge, broken, stepping stone, steps, moving platform, straight pad. Bend: bridge corner, corner pad. T: 3-gate pad. Cross: 4-gate pad. Empty: chasm a and b | Open sides `path`, closed sides `x` |

- **Door Tile:** `dungeon_door` (`school_dungeon_door.glb`, MeshLibrary item 54). It's a gatehouse built with its outside to the north, and godoodungeon turns it for each side. Two towers 1.45 m high with merlons, an arch, and blue sconces on the room side. Weight 0, so only the layout places it.
- **Room size:** walls included, so a room is at least 3 × 3 cells. 3–5 cells (12–20 m) suits the camera.
- **Settings:** Room Edge Socket `outside`, Path Socket `path`, Gap 3, Wall Clearance 1. Corridors keep a cell of chasm from the room walls, except at the doors.
- **Not used by godoodungeon:** the gated room tiles, the 3-wall pieces, the 1-gate pad, the chasm socket variants and `door_N` / `stairs_S`. Those belong to the plain WFC tile set.

## Element palettes (Fire, Water, Earth)

`textures/T_SchoolFireColorMap.png`, `T_SchoolWaterColorMap.png` and `T_SchoolEarthColorMap.png` are the palette for the three trial chambers (I.3): the same layout as `T_SchoolColorMap.png`, with other colours only for the banners and carpets and for the crystals (rune orbs on posts and above gates, wall sconce spikes, the rune circle, the crystals floating over the chasm). Stone and chasm stay the same; give each chamber its mood with lighting.

There are no element GLBs: the meshes are identical. In Godot, make one MeshLibrary per element from the base MeshLibrary, with the two palette materials (`M_SchoolColors`, `M_SchoolGlow`) pointing at the element palette instead (albedo, and emission for the glow material). The item numbers stay the same, so a chamber changes element by swapping its GridMap's MeshLibrary. `school_tiles.json` lists the palettes under `element_palettes`.

## Element challenges (I.3 trial chambers)

`school_<element>_<challenge>_NS.glb`: 7 extra bridge tiles, items 55–61 after the dungeon door, with the sockets of `bridge_NS`, built along north–south and turned in the GridMap (FourWay). Each holds the static part of an element-specific platforming challenge; the moving or dangerous parts are props, moved by marker scenes in the game (marker = tile name). godoowave places a marker turned with its cell, so one marker scene per challenge covers both directions. Each element chamber's corridor tile set uses only its own challenges. `school_tiles.json` → `challenges` lists the tiles, props and layout numbers.

| Element | Challenge | Tile | Prop(s) |
|---|---|---|---|
| Fire | **Flame jets**: three vent rows across the deck burst fire in turns; wait between rows | full deck, vents, nozzles | `school_flame_jet.glb` |
| Fire | **Sinking stones**: two stepping stones between end ledges sink and rise in turns | ledges, shafts below | `school_sinking_stone.glb` |
| Fire | **Flame bar**: a bar of fire sweeps around a pivot post on a round pad; jump over it | pad with curb, post | `school_flame_bar.glb` |
| Water | **Waterfall**: a curtain of water falls from an arch on a timer and pushes the player back | deck, arch, drain | `school_water_curtain.glb` |
| Water | **Drifting raft**: a raft drifts in from the side, lines up with the bridge, then drifts away | ledges, bollards | `school_raft.glb` |
| Earth | **Crumbling ledge**: three small blocks between end ledges fall after you land, then come back | ledges, broken piers | `school_crumble_block.glb` |
| Earth | **Falling rocks**: rocks drop on three spots after a growing shadow | cracked deck | `school_falling_rock.glb` |

Props are centred at their own origin with the walking surface at y = 0 (the flame and curtain props stand on y = 0), in the N–S frame of their tile.

## Checked (2026-11-20)

Godot 4.7 headless, fresh project: 0 import errors or warnings; 62 items (31 tiles, 23 chasm variants on the `chasm_a` mesh, the dungeon door, 7 challenges), 37 concave shapes. All 92 old tiles placed unturned next to their new tile at the `remap_old_items` orientation: 1,196 probes (centre, edge midpoints, corners) give the same floor / raised / barrier / void everywhere. Three generated maps (12 × 10, turned cells at the indices above): 986 probes, open passages have floor on both sides of every seam and every wall has its barrier.
