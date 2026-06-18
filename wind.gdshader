shader_type spatial;
render_mode unshaded, cull_disabled, particle_trails;

void fragment() {
    ALBEDO = COLOR.rgb;
    ALPHA = COLOR.a;
}