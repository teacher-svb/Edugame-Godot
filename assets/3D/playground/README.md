# Playground (swing, merry-go-round, seesaw)

Blender source: `blend/playground/playground.blend` (scripts in `.work/scripts/playground_scripts/`, run `playground_build.py`, then `playground_export.py`).
Sized for the game's characters: the player (Rogue, `Rig_Medium` at scale 0.525) is ~1.2 m tall with 0.21 m hips, Avocado (scale 1.5) ~0.56 m.

## Files
One file per plaything; no character files (the player and Avocado clips stay in the .blend; the last exported versions are in `blend/playground/versions/glb_character_clips/`, and `playground_export.py` writes them again with `CHARS = True`).

| File | What |
|---|---|
| `playground_swing.glb` | A-frame with two seats (a = red, left; b = blue, right). 1,660 tris |
| `playground_merry_go_round.glb` | Hand-driven disc, r 0.85 m, deck 0.22 m high, four handle bars. 1,044 tris |
| `playground_seesaw.glb` | 2.6 m beam on a 0.30 m pivot, red seat = player (−X), yellow = Avocado (+X). 444 tris |
| `textures/T_PlaygroundColorMap.png` | 32 × 32 palette (nearest, no mipmaps; `.import` included), shared by the three files |
| `playground.json` | Clip lengths, marker positions, collider names |

Every piece: origin at the bottom centre, front = Godot +Z, one palette surface per mesh, no vertex colours.
Colliders: `*-colonly` meshes (frame legs, merry-go-round base/deck cylinder, seesaw pivot block) import as `StaticBody3D` + concave shape. The moving parts (seats, disc, beam) have no collider.

## Markers (Node3D in the imported scene)
- `SEAT_*`: child of the moving part (seat pivot, disc, beam), so it swings / spins / tilts with it. A character sits here.
- `ENTER_*`: on the ground, static. Where a character stands to get on and off.
- Orientation: a marker's basis is the sitting character's own space: the player faces the marker's +Z (Godot). Avocado's model faces its own −X, so its seesaw markers are set up for that already.

## Clips (30 fps, in each file's AnimationPlayer). Godot drops the `_loop` suffix and turns looping on: `swing_a_loop` imports as `swing_a`
| Clip (imported name) | s | |
|---|---|---|
| `swing_a_start`, `swing_a` (loop), `swing_a_stop` | 5.2 / 2.6 / 5.2 | seat a: ramps up to ±32°, swings, ramps down to rest |
| `swing_b_start`, `swing_b`, `swing_b_stop` | same | seat b. Godot gives each swing clip a 1-key rest track for the other seat, so playing both seats at once needs an AnimationTree with filters or a second AnimationPlayer |
| `mgr_spin` (loop) | 4.0 | one turn counter-clockwise seen from above; set `speed_scale` for spin-up / slow-down |
| `seesaw` (loop) | 2.4 | ±11.5°; frame 0 = rest = player end on the ground |

Sequences: swing `swing_a_start` → `swing_a` → `swing_a_stop` (same for b); merry-go-round `mgr_spin` with `speed_scale` ramping up and down; seesaw `seesaw`, stopped at the end of a loop (rest).

## Check (2026-11-26)
Godot 4.7.2 headless, scratch project: import with no errors or warnings; each file has its clips, one `StaticBody3D`, and its `SEAT_` / `ENTER_` markers.

## Avocado seesaw clips (2026-11-27)
`characters/avocado_seesaw.glb`: the Avocado seesaw clips from the playground pass (the last export, `blend/playground/versions/glb_character_clips/avocado_seesaw.glb`, made against the current seesaw GLB): `avocado_seesaw_enter` (1.0 s, hops from in front of and below the seat onto it), `avocado_seesaw_loop` (2.4 s, one bounce cycle, frame 0 = rest; Godot names it `avocado_seesaw`), `avocado_seesaw_exit` (0.87 s). Avocado mesh without material (use the game's `avocado.glb` + these clips: tracks `Armature/Skeleton3D:*`), model at scale 1.5 directly under `SEAT_seesaw_avocado`, unturned (the clips are made for the raw model, which faces its own −X).
