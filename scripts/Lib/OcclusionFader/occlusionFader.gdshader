// The occlusion material template with StandardMaterial3D's defaults (opaque, single-sided, albedo texture/colour,
// roughness, metallic): usable as a ready-made see-through material. OcclusionFader doesn't use this file: it builds
// its shaders from occlusion_material.gdshaderinc for each source material's features.
shader_type spatial;
render_mode blend_mix, depth_draw_opaque, cull_back, diffuse_burley, specular_schlick_ggx;

#include "res://scripts/Lib/OcclusionFader/occlusion_material.gdshaderinc"
