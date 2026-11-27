// Occlusion cut, shared by every shader that should open up when it hides the player.
// The globals are set each frame by OcclusionFader (scripts/Lib/OcclusionFader/OcclusionFader.cs).
//
// Opt in from a spatial shader:
//     #include "res://scripts/Lib/OcclusionFader/occlusion.gdshaderinc"
//     void fragment() {
//         if (!IN_SHADOW_PASS) {
//             OCCLUSION_DISCARD   // not in the shadow pass: VIEW_MATRIX is the light's there
//         }
//         ...
//     }
//
// What is cut: fragments inside a circle on screen around the player, on the camera's side of a vertical plane
// just in front of the player (facing the camera), and above the player's feet. Seen from the side: the beam from
// the camera to the player, ended by that vertical plane and cut off horizontally at the feet, so a wall between
// camera and player goes away down to foot level. The circle is the same on screen for every depth (a cone from
// the camera, not a world-space cylinder, which looks like a funnel in perspective and leaves the far side of a
// building standing). Its radius is in metres at the player's depth.

global uniform vec3 player_pos;
global uniform vec3 camera_pos;
global uniform float occlusion_radius;
global uniform float occlusion_voffset;
// fragments up to this high above the player's feet are kept (OcclusionFader.CutOffHeight)
global uniform float occlusion_cutoff_height;
// the vertical plane stands this far in front of the player's centre, towards the camera (OcclusionFader.CutOffFront)
global uniform float occlusion_cutoff_front;

// soft edge of the circle (metres at the player's depth), drawn with an ordered dither
uniform float occlusion_feather : hint_range(0.0, 2.0) = 0.2;

// The hole's centre in view space (its depth is -z).
vec3 occlusion_center(mat4 view_matrix) {
    return (view_matrix * vec4(player_pos + vec3(0.0, occlusion_voffset, 0.0), 1.0)).xyz;
}

// 1 inside the circle on screen, 0 outside, smooth across the feather. Any depth.
float occlusion_circle(vec3 view_vertex, vec3 center, mat4 projection_matrix) {
    if (occlusion_radius <= 0.0)
        return 0.0;
    float frag_depth = max(-view_vertex.z, 1e-4);
    // perspective: scale the fragment to the player's depth along its line of sight (same screen position);
    // orthographic (projection_matrix[3][3] == 1): screen position doesn't depend on depth
    bool ortho = projection_matrix[3][3] > 0.5;
    vec2 at_center_depth = ortho ? view_vertex.xy : view_vertex.xy * (-center.z / frag_depth);
    float dist = length(at_center_depth - center.xy);
    return 1.0 - smoothstep(occlusion_radius - occlusion_feather, occlusion_radius, dist);
}

// True on the camera's side of the vertical plane in front of the player. A camera straight above the player has
// no horizontal direction: then everything counts as in front.
bool occlusion_in_front(vec3 world_pos) {
    vec2 to_camera = camera_pos.xz - player_pos.xz;
    float len = length(to_camera);
    if (len < 1e-3)
        return true;
    return dot(world_pos.xz - player_pos.xz, to_camera / len) > occlusion_cutoff_front;
}

// How much a back face is an interior seen through the hole (the inside of a wall cut off at the feet): inside the
// circle and in front of the vertical plane. 0 elsewhere.
float occlusion_interior(vec3 view_vertex, vec3 world_pos, mat4 view_matrix, mat4 projection_matrix) {
    if (!occlusion_in_front(world_pos))
        return 0.0;
    return occlusion_circle(view_vertex, occlusion_center(view_matrix), projection_matrix);
}

// view_vertex: VERTEX in fragment() (view space); world_pos: the fragment in world space;
// view_matrix: VIEW_MATRIX; projection_matrix: PROJECTION_MATRIX; frag_coord: FRAGCOORD.xy
bool occlusion_cut(vec3 view_vertex, vec3 world_pos, mat4 view_matrix, mat4 projection_matrix, vec2 frag_coord) {
    if (occlusion_radius <= 0.0 || world_pos.y <= player_pos.y + occlusion_cutoff_height || -view_vertex.z <= 0.0)
        return false;
    if (!occlusion_in_front(world_pos))
        return false;

    float mask = 1.0 - occlusion_circle(view_vertex, occlusion_center(view_matrix), projection_matrix);
    const mat4 dither = mat4(
        vec4(0.0625, 0.5625, 0.1875, 0.6875),
        vec4(0.8125, 0.3125, 0.9375, 0.4375),
        vec4(0.25,   0.75,   0.125,  0.625),
        vec4(1.0,    0.5,    0.875,  0.375)
    );
    ivec2 cell = ivec2(frag_coord) % 4;
    return mask < dither[cell.x][cell.y];
}

// 0..1, see occlusion_interior; use on back faces (!FRONT_FACING) of double-sided shaders
#define OCCLUSION_INTERIOR occlusion_interior(VERTEX, (INV_VIEW_MATRIX * vec4(VERTEX, 1.0)).xyz, VIEW_MATRIX, PROJECTION_MATRIX)
#define OCCLUSION_DISCARD if (occlusion_cut(VERTEX, (INV_VIEW_MATRIX * vec4(VERTEX, 1.0)).xyz, VIEW_MATRIX, PROJECTION_MATRIX, FRAGCOORD.xy)) { discard; }
