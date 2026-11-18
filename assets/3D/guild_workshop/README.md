# Engineers Guild hideout: Godot 4 export

The Guild's secret underground workshop (Acts III–IV): one hand-built level of 20 × 14 tiles of 1 m, in the village's painted low-poly style (stone cellar walls, flagstone floor, brass pipes and gears, warm oil lamps). Like the village, the export is split into the structure and the props:

| Folder | Holds |
|---|---|
| `level/guild_workshop.glb` | Floor, walls, pillars, entrance stairs and tunnel, locked door, everything fixed to a wall (boiler with steam pipes, gauge panel, "mana" pipe, forge, tool wall, oil lamps, drain grate), the collider, and all markers. |
| `level/guild_workshop_grid.json` | Tile grid: walkable / blocked tiles, stair heights, railing edges, interaction tiles, markers, the locked door and the exit. |
| `level/textures/` | `T_StoneWall`, `T_RoughStone`, `T_Brick`, `T_CellarFlags` (tiling) and the palette `T_GuildColorMap` (with its `.import` file). Keep it next to the GLB. |
| `props/prop_<type>.glb` | One file per new prop type (22), each at its own origin: bottom centre on (0, 0, 0), front towards Godot +Z. |
| `props/prop_placements.json` | Where every prop goes (28 placements). |
| `props/textures/` | The props palette `T_GuildPropColorMap` (with its `.import` file). |

The oil barrels reuse the village props `godot_exports/village/props/prop_barrel_a.glb` and `prop_barrel_b.glb`.

**Coordinates.** +Y up, 1 unit = 1 m, no extra scale. Place `guild_workshop.glb` at the origin. Tile (col, row) has its centre at `(col + 0.5, height, -(row + 0.5))`: columns run along +X, rows along -Z, and row 0 is the low front wall on the camera side.

## Layout

- **Entrance:** stairs along the left wall (tiles 0–3, rows 9–10) rise 2.0 m towards -X into a tunnel through the left wall. The tunnel runs 3 m further to the closed hidden door, out of view; that door leads up to the tavern's outside cellar hatch in the village (`PROP_cellar_hatch_01`, `prop_cellar_hatch.glb`, part of `STRUCT_tavern`): going down the hatch loads this level at `SPAWN_entrance`. The player arrives at `SPAWN_entrance` on the top of the stairs; a trigger at `EXIT_entrance` takes them out.
- **Middle:** the Mechanical Lantern build station (a big workbench under a gantry crane, tiles 8–11, rows 6–7) with free tiles all round, between four stone pillars.
- **Alfa (construction), front left:** two workbenches with vices under the tool wall, parts shelf, assembly table with a half-built cart, treadle lathe, drill press, coat stand.
- **Beta (materials), front right:** brick forge with bellows, firewood, oil barrels, shelf of jars, lamp test bench, wind test rig.
- **Gamma (documents), back right:** two blueprint shelves, basket of plans, chalkboard, plan chest, drafting table, rug.
- **Back wall:** boiler with brass steam pipes and a gauge-and-valve panel, crates.
- **Hints at the Wizard:** the locked door with a faded star-and-moon emblem (`DOOR_locked`), the pipe with a star-marked "mana" gauge that comes out of the floor and runs into the wall next to it, and the wizard hat on the coat stand.
- **Walls for the camera:** the front (south) wall is a permanent low wall (0.5 m) and the side walls step down to it in five stone-capped steps over their front 3 m, so nothing needs hiding while the player is inside.

## Markers

Empty nodes in `guild_workshop.glb`, children of `Workshop_Level`. A marker faces along its local -Z (Godot's forward), so an NPC or the player can copy its rotation. The same positions and rotations are listed in `guild_workshop_grid.json` under `markers`.

| Marker | Use |
|---|---|
| `SPAWN_entrance` | Arrival, top of the stairs (tile 0,9), facing into the room. |
| `EXIT_entrance` | In the tunnel mouth: trigger to leave the level, back out at the tavern's cellar hatch. |
| `STAIRS_bottom` | Bottom of the stairs (tile 4,9). |
| `NPC_alfa`, `NPC_beta`, `NPC_gamma` | Each mentor at their corner, facing their bench. |
| `STATION_lantern` | Where Mabel stands to build the lantern (tile 9,5), facing the bench. |
| `STATION_alfa`, `STATION_beta`, `STATION_gamma` | Mentor positions around the station for the build scene (IV.1). |
| `DOOR_locked` | In front of the locked door (tile 5,13). |
| `LIGHT_*` (15) | Light positions: 10 oil lamps on the pillars and walls, the hanging lamp over the station, forge, boiler fire door, drafting lamp, lamp test bench. Suggested: warm `OmniLight3D`s (colour about #FFB870), the forge and boiler more orange. |
| `PROP_<type>_<nn>` (28) | Prop positions (also in `prop_placements.json`). |

## Grid

`guild_workshop_grid.json`: 221 walkable tiles (the 8 stair tiles included, with their height under `heights`), 59 blocked. `blocked_edges` lists the railings along both sides of the stairs. `interactive` gives the free tile(s) in front of each bench, shelf, the station, the forge, the locked door and the bottom of the stairs. `exits.entrance_tunnel` marks the two top stair tiles that lead into the tunnel.

## Collision

`Workshop_Collision` imports as a `StaticBody3D` with a `ConcavePolygonShape3D` and no visible mesh (no import script needed):

- floor slab with its top at y = 0;
- every wall as a full-height box, including invisible walls above the low front wall and the stepped side walls;
- the stairs as a walkable ramp (tile heights in the grid file) with a 1 m railing on both sides; the tunnel floor, walls and the closed hidden door;
- a box on every blocked tile, at least 1 m high.

Tested in Godot 4.7 with down rays on every tile centre (walkable tiles hit their floor height, blocked tiles at least 0.85 m) and level rays between all neighbouring tiles (434 pairs): it matches the grid file. The only openings in the outer walls are the two top stair tiles into the tunnel.

## Colours and import

No import script is needed. Painted surfaces get their colours from the palette textures (`T_GuildColorMap`, 36 colours; `T_GuildPropColorMap`, 55 colours), each colour a 4 × 4 block sampled with nearest filtering; the `.import` files keep them lossless with no mipmaps. Flames and lamp glass use an emissive material. Tested in Godot 4.7 headless: no import errors, no vertex-colour surfaces, every prop lands exactly on its marker.

Size: level 5,635 triangles (+ 876 collider), props 96–2,500 triangles each.

## Putting it together

```gdscript
extends Node3D

func _ready() -> void:
    var level := (load("res://godot_exports/guild_workshop/level/guild_workshop.glb") as PackedScene).instantiate()
    add_child(level)
    var data = JSON.parse_string(FileAccess.get_file_as_string("res://godot_exports/guild_workshop/props/prop_placements.json"))
    for p in data.placements:
        var prop := (load("res://" + p.prop_file) as PackedScene).instantiate() as Node3D
        level.add_child(prop)
        prop.position = Vector3(p.position[0], p.position[1], p.position[2])
        var q = p.rotation_quaternion_xyzw
        prop.quaternion = Quaternion(q[0], q[1], q[2], q[3])
```

The paths assume the export folders sit under `res://godot_exports/`; adjust them to where you put the files. In the game project the files are in `res://assets/3D/guild_workshop/` (and the barrels in `res://assets/3D/village/props/`): replace `godot_exports/` with `assets/3D/` in the `prop_file` paths.
