# Mabel's home: interior levels

Two game levels for Mabel's cottage, for a top-down RPG with a locked camera and 1 m grid movement. The player moves between them on the stairs, with a loading screen.

| File | Contents |
|---|---|
| `mabel_home_interior_ground.glb` | Ground floor: kitchen, living and dining room, staircase. Root `Interior_Ground` at the origin. |
| `mabel_home_interior_upper.glb` | Mabel's bedroom with the top of the stairs. Root `Interior_Upper` at x = 14 m, beside the ground floor. |
| `mabel_home_interior_props.json` | Every prop marker: floor, prop type, file, position, rotation (quaternion) and scale. |
| `mabel_home_interior_grid.json` | Each floor's size in tiles, its origin, walkable and blocked tiles, wall edges, interactive tiles, markers and the stairs link. |
| `textures/` | `T_MabelInteriorColorMap.png` (palette for all painted surfaces), `T_FloorBoards.png`, `T_CottageStone.png`. Keep this folder next to the two GLBs. |

Same export settings as the village (GLB, +Y up, 1 unit = 1 m). Put both level files at the origin and they sit side by side, as in Blender.

## Floors and grid

| Level | Tiles | Walkable | Origin (Godot) | Walls |
|---|---|---|---|---|
| Ground | 11 × 8 | 55 | (0, 0, 0) | 3.0 m |
| Bedroom | 7 × 6 | 27 | (14, 0, 0) | 2.6 m |

Tile `(col, row)` has its centre at `(origin_x + col + 0.5, 0, origin_z - (row + 0.5))`: columns run along Godot +X, rows along Godot -Z, and row 0 is the front wall facing the camera. Both floors have rounded corners like the cottage's thatch; the four corner tiles are blocked. There are no ceilings or roof.

`blocked_edges` in the grid file lists tile pairs separated by a wall without a blocked tile, around the stairwell upstairs. Doors are listed under `doors`, and the free tile in front of each bed, desk, hearth, sink, table, bookshelf, door and stair end under `interactive`.

## Cover objects

The front walls are cut down to a 0.4 m base so the camera can see in. The full-height pieces are separate nodes you can show or hide:

- `Front_Wall_Cover` (ground): front wall, rounded front corners, door frame and `Front_Door`.
- `Upper_Cover` (bedroom): front wall and corners, the walls around the stairwell, the bedroom door frame and `Bedroom_Door`.

Hide them while the player is inside; show them for an outside or cutaway view. The doors are separate nodes with their origin on the hinge, so they can swing.

## Markers

| Marker | Purpose |
|---|---|
| `SPAWN_front_door` | Arrival tile just inside the front door. |
| `SPAWN_bed` | Tile beside Mabel's bed. |
| `STAIRS_bottom` (ground), `STAIRS_top` (bedroom) | The walkable tiles at each end of the stairs. Stepping on one loads the other floor and places the player on the other marker. |
| `LIGHT_hearth`, `LIGHT_stove`, `LIGHT_candle_living`, `LIGHT_window_back`, `LIGHT_window_left`, `LIGHT_window_right`, `LIGHT_candle_bedroom`, `LIGHT_window_round` | Where to put lights. Window markers are turned so a SpotLight3D or DirectionalLight3D placed on them (shining along its -Z) points into the room. |
| `PROP_<type>_<nn>` | One empty per prop (see below). |

The hearth fire, stove glow and candle flames use the village's emissive `M_Glow_Lantern` material; the windows use `M_Glow_Window`. Blender preview lights are not exported.

## Props

Props are separate files in `../village/props/`, the same folder and format as the village props (origin at the bottom centre, front towards Blender -Y / Godot +Z). 21 new prop types were added there with their own palette `textures/T_MabelPropColorMap.png`; 7 village props are reused (`water_barrel`, `barrel_b`, `sack_a`, `sack_b`, `bucket`, `pot_plant`, `pot_flowers`).

```gdscript
var data = JSON.parse_string(FileAccess.get_file_as_string("res://godot_exports/mabel_interior/mabel_home_interior_props.json"))
for p in data.placements:
    var n: Node3D = (load("res://" + p.prop_file) as PackedScene).instantiate()
    add_child(n)
    n.position = Vector3(p.position[0], p.position[1], p.position[2])
    var q = p.rotation_quaternion_xyzw
    n.quaternion = Quaternion(q[0], q[1], q[2], q[3])
```

`position_in_floor` gives the same position relative to the floor root, if you load each floor as its own scene.

## Colours and import

No import script is needed. Painted surfaces get their colours from the palette textures (`T_MabelInteriorColorMap.png`, `T_MabelPropColorMap.png`), each colour a 4 × 4 block sampled with nearest filtering. Their `.import` files keep them lossless with no mipmaps. Tested in Godot 4.7 headless: no import errors or warnings, no vertex-colour surfaces, every prop lands exactly on its marker.

## Collision

Each level has a hidden collision mesh (`Ground_Collision`, `Upper_Collision`) that Godot imports as a `StaticBody3D` with a `ConcavePolygonShape3D`, so no import script is needed:

- floor slab with its top at y = 0, following the rounded outline;
- full-height walls; the front door is closed by a panel (use a trigger on the door tile to leave), the bedroom doorway is open;
- a box on every blocked tile from the grid file: the stairs and stairwell are solid, furniture and corner tiles are at least 0.9-1.0 m high.

The collider matches the grid file exactly: tested in Godot 4.7 with rays between all neighbouring tiles at 0.5 m height and down onto every tile centre.
