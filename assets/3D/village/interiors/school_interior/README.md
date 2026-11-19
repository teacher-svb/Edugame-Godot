# Wizard school interior: WFC tiles

78 tiles of 4 × 4 m for generating the inside of the wizard school with the WFC generator: stone rooms floating over a dark chasm, joined by stone bridges, with platforming on the way. Low-poly and flat-shaded, coloured by one palette texture, in cool castle stone with blue and purple banners, gold trim and glowing blue runes. One GridMap layer: every floor is at y = 0.

| File | Contents |
|---|---|
| `school_<name>.glb` | One tile, centre at the node origin, floor top at y = 0. Every rotation is its own tile, so never rotate one. |
| `school_moving_platform.glb` | The platform for the `mover_*` tiles (1.4 × 1.4 m, top at y = 0), moved by your game scene. |
| `school_tiles.json` | Per tile: item number, file, kind, sockets, weight, triangle counts; platforming sizes. |
| `textures/T_SchoolColorMap.png` | The palette (32 × 32 px, 31 colours, nearest filter) with its lossless `.import` file. Keep it next to the GLBs. |
| `textures/T_School<Fire\|Water\|Earth>ColorMap.png` | Element palettes for the trial chambers (see below), same layout. |

GLB, +Y up, 1 unit = 1 m, no vertex colours, no import script. Nodes per file: `Tile_<name>` → `<name>_Mesh` (surface `M_SchoolColors`, plus `M_SchoolGlow` for runes, flames and crystals: the same palette as emission) and `<name>_Collision` (StaticBody3D + ConcavePolygonShape3D; the chasm tiles have none).

## Sockets

N = −Z (top of the screen; the game camera looks north), E = +X, S = +Z, W = −X. In the WFC tile fields: N = `SocketNegZ`, E = `SocketPosX`, S = `SocketPosZ`, W = `SocketNegX`; up and down `ground`. Plain labels fit the same label; `x+` fits only `x−`.

| Label | On | Fits |
|---|---|---|
| `room` | open sides inside a room | `room` |
| `wall+` | walls and curbs of rooms; closed sides of landings | a chasm side `wall−` only, so two rooms never touch wall to wall |
| `gate+` | gates of rooms and landings | a bridge end `gate−` only, so rooms connect over bridges, never by a doorway |
| `gate−` | bridge ends (straight, corner, broken, stones, steps, mover) | `gate+` |
| `flank+` | the sides of a bridge | a chasm side `flank−` only, so a bridge always has chasm on both sides |
| `void` | chasm against chasm; the door side of `door_*` / `stairs_S`; the map boundary | `void` |

The chasm is one mesh (`chasm_a`) with **23 socket variants**: every mix of `void` / `wall−` / `flank−` on its four sides, with Rotation FourWay (quarter turns). `school_tiles.json` lists them under `chasm_variants`. Weight 0.5 per orientation.

Bridges can't join end to end, because that would also let two rooms meet gate to gate. A crossing longer than one cell goes bridge → landing → bridge, and the landings are the pads in between.

Rooms and landings are named after their sides, `room_<NESW>` / `landing_<NESW>`, with `o` = open (room), `w` = wall, `g` = gate. `room_owgw` has an open north, a wall on the east, a gate on the south and a wall on the west.

## Tiles

| Tiles | What it is | Weight |
|---|---|---|
| `room_oooo_a`, `_b` | Open floor of a room (`_b` with a purple rug) | 1.3 each |
| `room_oooo_circle`, `_dais`, `_pillar`, `_pit`, `_tiers` | Open floor with a glowing rune circle, a raised dais (+0.3 m) with a lectern, a column with a blue fire, a 2 × 2 m pit into the chasm, or two blocks (+0.3 / +0.6 m) with a small chest on top | 0.3–0.5 |
| `room_<NESW>` (36) | Room edge pieces: walls on 1–3 sides, at most one gate. No 1-wide corridors: rooms are at least 2 cells wide. | 1.05 (1 wall), 0.68 (2), 0.05 (3); half with a gate |
| `landing_<NESW>` (15) | A bridge pad (2.6 m square, parapets, rune posts at the corners, a pier) with an arm to each of its 1–4 gates: where bridges meet or turn | 0.04–0.35 |
| `bridge_NS`, `_EW` | Stone bridge, 2 m wide, low parapets, a pier into the dark | 1.5 |
| `bridge_NE`, `_ES`, `_SW`, `_WN` | Bridge corner | 0.35 |
| `broken_NS`, `_EW` | Broken bridge: **jump the gap** (1.15 / 1.25 m) | 0.35 |
| `stones_NS`, `_EW` | **Stepping stone**: a pillar 0.3 m higher, 1 m from each ledge | 0.3 |
| `steps_NS`, `_EW` | **Height step**: the middle of the bridge is raised 0.3 m | 0.3 |
| `mover_NS`, `_EW` | **Moving platform** crossing: ledges at both ends, 3.2 m gap, rune posts | 0.2 |
| `chasm_a`, `_b` | The chasm: dark bottom at −6 m (`_b` with floating rocks and violet crystals); `chasm_a` also carries the 23 socket variants | 0.4, 0.1 |
| `door_N`, `_E`, `_W`, `stairs_S` | Level entrance or exit: an alcove sticking out of a room (walls on both sides, the room behind it) with a wooden door, or stairs going down through the south curb. The door side is `void`, so it faces the map boundary. | 0 (place by hand) |

The weights are a starting point. In tests on 12 × 9 they gave 2–5 separate rooms per seed. Rooms are always separate, but nothing guarantees every room gets a bridge, so some seeds have an island room: try another seed.

## Platforming

Tuned to the current player (jump velocity 3, speed 3, default gravity): a jump is about 0.46 m high and 1.8 m long.

- **Gaps** of 1.15–1.25 m on broken bridges. The chasm under them has no floor.
- **Height steps** are 0.3 m: the stepping stone, the raised bridge, the dais and the tiers. The player can't walk up them and has to jump.
- **Moving platforms:** `school_moving_platform.glb` should move along the tile's axis between the two ledges. With the tile centre at the origin, the platform centre goes from −0.9 m to +0.9 m along Z (NS) or X (EW), top at y = 0. An `AnimatableBody3D` with an `AnimationPlayer` looping its position needs no script. Put that scene as the **Marker** of `mover_NS` / `mover_EW` in the WFC tile set, rotated 90° for EW, so every generated crossing gets one.
- **Falling:** nothing catches the player in the chasm. Put an `Area3D` kill / respawn zone under the level, for example at y = −3.
- **Walls and curbs:** walls are 1 m with merlons, south curbs 0.45 m. Their colliders go up to 1.35 m and 0.95 m, and bridge parapets to 0.95 m, so the player can't hop off a bridge or over a wall. The stairs in `stairs_S` collide as a ramp.

## Look and camera

Rooms keep low walls so the camera sees into them and onto the bridges behind. Tall parts are thin: gate arches (3.2 m), the column, the door frames. North walls carry banners on their inner face, and east and west walls have blue-flame sconces. The game's occlusion fader works on GridMaps if a tall part hides the player.

There are no lights in the tiles. The glow material only makes runes, flames and crystals bright, so add your own lights, for example a dim blue fill with point lights at the gates.

## Godot setup (suggested)

GridMap `cell_size` (4, 1, 4), `cell_center_y` off, one layer. WFC settings: all four side boundaries `void`, so there is chasm along the map edge. Place `door_*` / `stairs_S` by hand on the edge row, door side facing out; the generator puts a room behind it.

## godoodungeon (generated rooms joined by bridges)

The same tiles also work with **godoodungeon**. It lays out rectangular rooms, solves each one with a room tile set, puts doors in the walls, and routes a corridor from door to door; here the corridors are the bridges. Every room is reachable.

`school_tiles.json` → `dungeon` lists both tile sets (item, weight, sockets):

| Tile set | Contents | Sockets |
|---|---|---|
| Rooms (16) | Floors (7 variants), walls and corners in every direction, and the door | Walls face out with `outside+` (the room bounds supply `outside−`; put `outside−` in External Sockets), everything else `room` |
| Corridors (27) | Straight: bridge, broken, stepping stone, steps, moving platform, straight pad. Bend: bridge corners, corner pads. T: 3-gate pads. Cross: 4-gate pad. Empty: chasm | Open sides `path`, closed sides `x` |

- **Door Tile:** `dungeon_door` (`school_dungeon_door.glb`, MeshLibrary item 101). It's a gatehouse built with its outside to the north, and godoodungeon turns it for each side (Rotation FourWay). It's made to read in any direction: two towers 1.45 m high with merlons, an arch, and blue sconces on the room side. On the south side it rises above the low curb as a small gate tower. Weight 0, so only the layout places it.
- **Room size:** walls included, so a room is at least 3 × 3 cells. 3–5 cells (12–20 m) suits the camera.
- **Settings:** Room Edge Socket `outside`, Path Socket `path`, Gap 3, Wall Clearance 1. Corridors keep a cell of chasm from the room walls, except at the doors.
- **Not used by godoodungeon:** the gated room tiles, the 3-wall pieces, the 1-gate pads, the chasm socket variants and `door_*` / `stairs_S`. Those belong to the plain WFC tile set.

## Element palettes (Fire, Water, Earth)

`textures/T_SchoolFireColorMap.png`, `T_SchoolWaterColorMap.png` and `T_SchoolEarthColorMap.png` are the palette for the three trial chambers (I.3): the same layout as `T_SchoolColorMap.png`, with other colours only for the banners and carpets and for the crystals (rune orbs on posts and above gates, wall sconce spikes, the rune circle, the crystals floating over the chasm). Stone and chasm stay the same; give each chamber its mood with lighting.

There are no element GLBs: the meshes are identical. In Godot, make one MeshLibrary per element from the base MeshLibrary, with the two palette materials (`M_SchoolColors`, `M_SchoolGlow`) pointing at the element palette instead (albedo, and emission for the glow material). The item numbers stay the same, so a chamber changes element by swapping its GridMap's MeshLibrary. `school_tiles.json` lists the palettes under `element_palettes`.

## Element challenges (I.3 trial chambers)

`school_<element>_<challenge>_<NS|EW>.glb`: 14 extra bridge tiles, items 102–115 after the dungeon door, with the sockets of `bridge_NS` / `bridge_EW`. Each holds the static part of an element-specific platforming challenge; the moving or dangerous parts are props, moved by marker scenes in the game. Each element chamber's corridor tile set uses only its own challenges. `school_tiles.json` → `challenges` lists the tiles, props and layout numbers.

| Element | Challenge | Tile | Prop(s) |
|---|---|---|---|
| Fire | **Flame jets**: three vent rows across the deck burst fire in turns; wait between rows | full deck, vents, nozzles | `school_flame_jet.glb` |
| Fire | **Sinking stones**: two stepping stones between end ledges sink and rise in turns | ledges, shafts below | `school_sinking_stone.glb` |
| Fire | **Flame bar**: a bar of fire sweeps around a pivot post on a round pad; jump over it | pad with curb, post | `school_flame_bar.glb` |
| Water | **Waterfall**: a curtain of water falls from an arch on a timer and pushes the player back | deck, arch, drain | `school_water_curtain.glb` |
| Water | **Drifting raft**: a raft drifts in from the side, lines up with the bridge, then drifts away | ledges, bollards | `school_raft.glb` |
| Earth | **Crumbling ledge**: three small blocks between end ledges fall after you land, then come back | ledges, broken piers | `school_crumble_block.glb` |
| Earth | **Falling rocks**: rocks drop on three spots after a growing shadow | cracked deck | `school_falling_rock.glb` |

Props are centred at their own origin with the walking surface at y = 0 (the flame and curtain props stand on y = 0). EW tiles are the NS build turned 90° clockwise seen from above.
