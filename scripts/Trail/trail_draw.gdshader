shader_type spatial;
render_mode unshaded, world_vertex_coords, cull_disabled;

uniform sampler2D color_ramp : repeat_disable, source_color, hint_default_white;
uniform sampler2D width_curve : repeat_disable, hint_default_white;

void vertex() {
	// MODEL_MATRIX columns store cached world-space edge points:
	//   [0],[1] = left edge (a), [2],[3] = right edge (b)
	vec3 a = mix(MODEL_MATRIX[1].xyz, MODEL_MATRIX[0].xyz, UV.x);
	vec3 b = mix(MODEL_MATRIX[2].xyz, MODEL_MATRIX[3].xyz, UV.x);

	// remap UV.x to 0..1 along trail length
	UV.x = (UV.x + INSTANCE_CUSTOM.w - 3.0) / (INSTANCE_CUSTOM.z - 1.0);

	float width = textureLod(width_curve, vec2(UV.x), 0.0).r;

	VERTEX = mix(a, b, (UV.y - 0.5) * width + 0.5);
}

void fragment() {
	vec4 c = textureLod(color_ramp, vec2(UV.x, 0.5), 0.0);
	ALBEDO = c.rgb;
	ALPHA = c.a;
}
