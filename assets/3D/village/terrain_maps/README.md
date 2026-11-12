# Beaverford terrain maps

Three maps of the village terrain, sampled directly from the `Terrain` mesh in `beaverford_village_godot_fixes.blend` (terrain identical to numbered copy 14). Buildings, props, markers, hedges, bridges, collision and all reference collections are left out. So are the separate objects `Terrain_SoilSides`, `Terrain_RetainWall_*` and `Terrain_ChannelStones_Painted`.

**These are data maps, not colours.** Pixel values are metres or coverage amounts, with no sRGB curve and no gamma information in the files.

## Files

| File | Format | Contents |
|---|---|---|
| `terrain_height.exr` | 1024 × 1024, one channel named `R`, 32-bit float, ZIP (lossless) | Terrain height in **metres**, relative to the base ground level of 0 m (Blender Z = 0, Godot Y = 0). Negative below base level. Sampled at each pixel centre. |
| `terrain_height_16bit.png` | 1024 × 1024, 16-bit greyscale | Preview of the same heights. Black = −1.000000 m, white = +1.569107 m. |
| `terrain_road_mask.png` | 1024 × 1024, 8-bit greyscale | White where the terrain uses `M_Road`: roads, paths, the square and their bevels. Black elsewhere. |
| `terrain_riverbed_mask.png` | 1024 × 1024, 8-bit greyscale | River depth on `M_Riverbed` (stream, moat and feeder channel): grey level = depth below the base ground level, white = 1.000 m, the deepest point. Black outside the riverbed. |
| `terrain_maps.json` | JSON | Image size, metres per pixel, height range, `Water_Level` height, image corners in Blender and Godot coordinates, and the list of filled pixels. |
| `terrain_maps_preview.png` | 2048 × 2048, colour | Alignment check only: road in sandy yellow and riverbed in blue over a shaded heightmap, with building outlines. Not for use in shaders. |

Both maps are anti-aliased from 8 × 8 samples per pixel, written linearly as 0–255. In the road mask, each pixel is the fraction of samples on `M_Road`. In the riverbed map, each pixel is the average depth of its samples, with samples off the riverbed counting as 0, so depth fades to black over the one-pixel edge.

Riverbed depth in metres is `value * 1.0` when read as 0–1 in a shader (`value / 255` from raw bytes). Depth is measured from 0 m, not from the water. Water depth is `depth - 0.3`, since `Water_Level` is 0.3 m below base ground; negative means that spot is above the water. Riverbed depth runs from 0.08 m at the shallowest edge to 1.00 m. A value above 0 means riverbed. At 8 bits, one grey step is about 4 mm. For exact heights use `terrain_height.exr`.

## Coverage and orientation

- The maps cover the full terrain site: Blender X −50 to +50 m, Y −50 to +50 m, 100 × 100 m.
- One pixel is 0.09765625 m (100 / 1024).
- The top of the image is north (+Y in Blender, −Z in Godot). The left edge is west (−X).
- All three maps line up pixel for pixel.

## Pixels and world positions in Godot

Godot coordinates are (X, Z, −Y) of Blender's (X, Y, Z), the same as the exported level.

Pixel (`col`, `row`), counted from the top-left corner, has its centre at:

```
x = -50 + (col + 0.5) * 0.09765625
z = -50 + (row + 0.5) * 0.09765625
y = height from terrain_height.exr
```

World position to texture UV:

```
u = (x + 50) / 100
v = (z + 50) / 100
```

| Image corner | Blender (X, Y) | Godot (x, z) |
|---|---|---|
| Top left (north-west) | (−50, +50) | (−50, −50) |
| Top right (north-east) | (+50, +50) | (+50, −50) |
| Bottom left (south-west) | (−50, −50) | (−50, +50) |
| Bottom right (south-east) | (+50, −50) | (+50, +50) |

These are the outer edges of the image. The centres of the corner pixels are half a pixel inside them, and are also listed in `terrain_maps.json`.

The water height (`Water_Level`) is −0.3 m.

## Using the maps in Godot

Do **not** use `source_color` on these textures, because that would apply an sRGB conversion to data.

```glsl
uniform sampler2D terrain_height : filter_linear, repeat_disable;
uniform sampler2D road_mask : filter_linear, repeat_disable;
uniform sampler2D riverbed_mask : filter_linear, repeat_disable;

// in a shader, with world_pos as the world position:
vec2 map_uv = (world_pos.xz + 50.0) / 100.0;
float height_m = texture(terrain_height, map_uv).r;
float road = texture(road_mask, map_uv).r;
float river_depth_m = texture(riverbed_mask, map_uv).r;   // 0 outside the riverbed, 1.0 = 1 m deep
float water_depth_m = river_depth_m - 0.3;
```

Import settings for all of them:

- **Compress > Mode: Lossless.** Don't use lossy or VRAM compression, which would change the values.
- **Detect 3D > Compress To: Disabled**, so Godot doesn't switch them to VRAM compression later.
- **Mipmaps > Generate: off**, unless a shader needs them at a distance.
- For the EXR, leave **HDR as sRGB** off and **Clamp Exposure** off.

**Use the EXR for real heights.** Godot loads it as single-channel float (`FORMAT_RF`) and keeps negative values. Godot reduces a 16-bit PNG to 8-bit on import, so `terrain_height_16bit.png` is for viewing and other tools only. To convert its 16-bit values outside Godot: `height = -1.0 + value / 65535 * 2.569106511771679`.

Between pixel centres, linear filtering is an approximation. It was within 4.5 mm for 99% of 2,000 random points, and at most 18 mm, at sharp bank edges.

## The well opening

The terrain mesh has a real hole for the well shaft at Blender (−10, −6), Godot (−10, 6).

- In the height maps, the **83 pixels** with no surface (rows 568–578, columns 404–414) are filled from the ground around the opening, which is level at −0.0006 to 0 m. Each pixel is listed in `terrain_maps.json` under `filled_well_opening`.
- Both masks stay **black** in the opening, since no terrain material exists there. That leaves a small black disc in the road mask, in the middle of the square.
