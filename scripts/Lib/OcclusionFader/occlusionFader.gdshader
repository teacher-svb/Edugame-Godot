shader_type spatial;
render_mode depth_draw_opaque, cull_back;

uniform sampler2D albedo_texture : source_color, hint_default_white;
uniform vec4 albedo_color : source_color = vec4(1.0);
uniform float feather : hint_range(0.0, 5.0) = 0.2;
uniform float vertical_offset : hint_range(0.0, 3.0) = 2.0;

global uniform float occlusion_radius : hint_range(0.0, 10.0) = 1.0;

global uniform vec3 player_pos;
global uniform vec3 camera_pos;


void fragment() {
    vec3 world_pos = (INV_VIEW_MATRIX * vec4(VERTEX, 1.0)).xyz;

    // Shift the cylinder origin upward so the player sits at the bottom of the hole
    vec3 cylinder_origin = player_pos + vec3(0.0, vertical_offset, 0.0);

    // Cylinder axis from cylinder origin toward camera
    vec3 axis = normalize(camera_pos - cylinder_origin);

    // Project fragment onto the axis
    vec3 to_frag = world_pos - cylinder_origin;
    float proj = dot(to_frag, axis);

    // Perpendicular distance from the cylinder axis
    float dist = length(to_frag - proj * axis);

    // Only fade fragments between player and camera (proj > 0)
    float cam_dist = length(camera_pos - cylinder_origin);
	float in_range = step(-.2, proj) * step(proj, cam_dist);

    float mask = smoothstep(occlusion_radius - feather, occlusion_radius, dist);

    const mat4 dither_m = mat4(
        vec4(0.0625, 0.5625, 0.1875, 0.6875),
        vec4(0.8125, 0.3125, 0.9375, 0.4375),
        vec4(0.25,   0.75,   0.125,  0.625),
        vec4(1.0,    0.5,    0.875,  0.375)
    );

    ivec2 uv = ivec2(FRAGCOORD.xy) % 4;
	if (in_range > 0.5 && mask < dither_m[uv.x][uv.y] && world_pos.y > player_pos.y + .5) discard;

    vec4 tex = texture(albedo_texture, UV) * albedo_color;
    ALBEDO = tex.rgb;
}