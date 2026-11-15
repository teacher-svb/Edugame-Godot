# Nature props

32 vegetation and nature props in the low-poly, flat-shaded style of the village and the forest tiles, for scattering with MultiMesh: 8 trees, 4 rocks, 8 bushes, 4 flowers and 8 grass clumps. Spring/summer colours, mostly greens.

| File | Contents |
|---|---|
| `nature_<name>.glb` | One prop: a single mesh with one surface. Origin at the bottom centre, +Y up, front towards +Z. |
| `nature_props.json` | Per prop: file, category, double-sided or not, height, radius, triangle count, and the trunk radius of trees. |
| `textures/T_NatureColorMap.png` | The palette that colours every prop (32 × 32 px, one 4 × 4 block per colour), with its `.import` file (lossless, no mipmaps). Keep this folder next to the GLBs. |

Same export settings as the village: GLB, +Y up, 1 unit = 1 m, no import script. All 32 GLBs: 0.8 MB.

## Props

Sizes are for scale 1, next to characters of about 1.1–1.4 m. Radius = the largest horizontal distance from the origin.

| Prop | Height (m) | Radius (m) | Triangles | Trunk radius (m) |
|---|---|---|---|---|
| `tree_oak` | 5.82 | 2.34 | 558 | 0.24 |
| `tree_oak_wide` | 5.22 | 2.70 | 651 | 0.29 |
| `tree_round` | 5.18 | 1.73 | 372 | 0.17 |
| `tree_round_tall` | 6.29 | 1.45 | 452 | 0.19 |
| `tree_birch` | 6.14 | 1.29 | 465 | 0.12 |
| `tree_pine` | 7.45 | 1.79 | 150 | 0.19 |
| `tree_pine_small` | 4.20 | 1.12 | 128 | 0.12 |
| `tree_poplar` | 6.75 | 0.91 | 532 | 0.16 |
| `rock_boulder` | 0.77 | 0.64 | 80 | |
| `rock_flat` | 0.24 | 0.66 | 80 | |
| `rock_cluster` | 0.47 | 0.69 | 240 | |
| `rock_mossy` | 0.79 | 0.75 | 160 | |
| `bush_round` | 0.90 | 0.79 | 240 | |
| `bush_wide` | 0.83 | 1.08 | 400 | |
| `bush_tall` | 1.42 | 0.76 | 400 | |
| `bush_small` | 0.54 | 0.48 | 160 | |
| `bush_dark` | 1.04 | 0.94 | 320 | |
| `bush_fern` | 0.60 | 0.86 | 77 | |
| `bush_flowering` | 0.96 | 0.90 | 864 | |
| `bush_berry` | 0.83 | 0.83 | 528 | |
| `flower_daisy` | 0.32 | 0.21 | 216 | |
| `flower_buttercup` | 0.29 | 0.16 | 253 | |
| `flower_bluebell` | 0.28 | 0.28 | 198 | |
| `flower_foxglove` | 0.57 | 0.25 | 246 | |
| `grass_short` | 0.20 | 0.21 | 48 | |
| `grass_tall` | 0.54 | 0.38 | 65 | |
| `grass_wide` | 0.32 | 0.24 | 27 | |
| `grass_thin` | 0.34 | 0.28 | 120 | |
| `grass_seedheads` | 0.60 | 0.29 | 88 | |
| `grass_dark` | 0.38 | 0.34 | 70 | |
| `grass_fresh` | 0.27 | 0.24 | 42 | |
| `grass_curly` | 0.24 | 0.46 | 70 | |

Trees: broadleaf canopies of lumpy blobs (oak, wide oak, round, tall round in blue-green, birch with a white trunk, poplar) and two conifers. Bushes: round, wide, tall, small, dark, a fern, one with pink and white blossoms and one with red berries. Flowers: daisies, buttercups, bluebells and a pink foxglove-like spike. Grass: short, tall, wide, thin, with seed heads, dark, fresh light green and curly.

Tree trunks start 5–6 cm below the origin so they sink into the ground without a seam; bushes and rocks are flat at the bottom.

## Using them with MultiMesh

A `MultiMesh` holds one mesh, so use one `MultiMeshInstance3D` per prop. To get the mesh as a resource: double-click the GLB, open **Meshes**, select the mesh, tick **Save to File** and pick a path (for example `nature_tree_oak.res`), then press **Reimport**. Assign that `.res` to the MultiMesh's **Mesh**. Reimporting the GLB later updates the saved mesh.

Suggested scatter settings (MultiMesh Scatter):

| Props | Alignment | Random yaw | Random scale | Min distance |
|---|---|---|---|---|
| Trees | 1 (upright, target +Y) | 360° | 0.85–1.15 | about 2 × radius × scale |
| Bushes, rocks | 0.8 | 360° | 0.8–1.2 | about the radius |
| Flowers, grass | 0.6–0.7, tilt a few degrees | 360° | 0.8–1.3 | 0.15–0.3 m |

The props have no collision (a MultiMesh has none). `trunk_radius_m` in the JSON is there for adding trunk colliders, for example a `CylinderShape3D` per tree instance.

## Materials and colour

Every surface takes its colour from `T_NatureColorMap.png`: each face's UVs point at the centre of one colour block, and the texture uses nearest filtering, so colours stay crisp. There are no vertex colours (the Godot 4.7 *Use as Albedo* rule does not apply). Two materials share the palette:

| Material | Used on | Culling |
|---|---|---|
| `M_NatureColors` | trees, rocks, bushes (except the fern) | back faces culled |
| `M_NatureColorsDS` | grass, flowers, fern | double-sided (glTF `doubleSided`), so single-sided blades show from both sides |

Light and shade are partly painted in (lighter faces towards the upper front right, darker underneath), on top of Godot's own lighting, as on the village bushes.

## Tested

Godot 4.7 headless, fresh project, no script: no import errors or warnings; every GLB has one `MeshInstance3D` with one surface, the palette texture with nearest filtering, no vertex colours, culling disabled exactly on the 13 double-sided props, and heights matching the JSON.
