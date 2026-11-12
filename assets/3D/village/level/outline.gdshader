// Inverted-hull outline for the Beaverford village (Godot 4).
// Use it as the "Next Pass" material of a mesh's surface material:
// the hull is drawn after the normal pass, pushed out along the vertex normals,
// with front faces culled, so only the dark rim behind the silhouette shows.
shader_type spatial;
render_mode unshaded, cull_front, depth_draw_opaque, diffuse_burley, specular_disabled, shadows_disabled;

// Matches the village's original outline colour, sRGB (58, 38, 30).
uniform vec4 outline_color : source_color = vec4(0.227, 0.149, 0.118, 1.0);
// Width in metres (object space). The original models used about 0.012-0.04 m on
// buildings and 0.004-0.008 m on small props.
uniform float outline_width : hint_range(0.0, 0.1, 0.001) = 0.02;

void vertex() {
	VERTEX += normalize(NORMAL) * outline_width;
}

void fragment() {
	ALBEDO = outline_color.rgb;
}
