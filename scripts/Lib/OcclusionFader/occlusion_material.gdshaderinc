// Template of OcclusionFader's replacement materials: the parts of Godot's StandardMaterial3D the game uses, plus the
// occlusion cut. OcclusionFader builds one shader per feature combination at run time:
//     shader_type spatial;
//     render_mode <from the source material: blend, depth draw, cull, diffuse, specular, shading, ...>;
//     #define OCC_...            (the compile-time features below)
//     #include "res://scripts/Lib/OcclusionFader/occlusion_material.gdshaderinc"
// and copies the source material's values into the uniforms (same names as StandardMaterial3D's own shader).
//
// Defines: OCC_FILTER / OCC_REPEAT (sampler hints, e.g. filter_nearest_mipmap / repeat_enable), OCC_VERTEX_COLOR,
// OCC_VERTEX_COLOR_SRGB, OCC_NORMAL_MAP, OCC_EMISSION, OCC_EMISSION_MULTIPLY, OCC_AO, OCC_ALPHA, OCC_ALPHA_SCISSOR,
// OCC_DOUBLE_SIDED (darkens back faces seen through the hole).

#include "res://scripts/Lib/OcclusionFader/occlusion.gdshaderinc"

#ifndef OCC_FILTER
#define OCC_FILTER filter_linear_mipmap
#endif
#ifndef OCC_REPEAT
#define OCC_REPEAT repeat_enable
#endif

uniform vec4 albedo : source_color = vec4(1.0);
uniform sampler2D texture_albedo : source_color, hint_default_white, OCC_FILTER, OCC_REPEAT;
uniform float roughness : hint_range(0.0, 1.0) = 1.0;
uniform sampler2D texture_roughness : hint_default_white, OCC_FILTER, OCC_REPEAT;
uniform vec4 roughness_texture_channel = vec4(1.0, 0.0, 0.0, 0.0);
uniform float metallic : hint_range(0.0, 1.0) = 0.0;
uniform float specular : hint_range(0.0, 1.0) = 0.5;
uniform sampler2D texture_metallic : hint_default_white, OCC_FILTER, OCC_REPEAT;
uniform vec4 metallic_texture_channel = vec4(1.0, 0.0, 0.0, 0.0);
uniform vec3 uv1_scale = vec3(1.0);
uniform vec3 uv1_offset = vec3(0.0);
#ifdef OCC_ALPHA_SCISSOR
uniform float alpha_scissor_threshold : hint_range(0.0, 1.0) = 0.5;
#endif
#ifdef OCC_NORMAL_MAP
uniform sampler2D texture_normal : hint_roughness_normal, OCC_FILTER, OCC_REPEAT;
uniform float normal_scale = 1.0;
#endif
#ifdef OCC_EMISSION
uniform vec4 emission : source_color = vec4(0.0, 0.0, 0.0, 1.0);
uniform float emission_energy = 1.0;
uniform sampler2D texture_emission : source_color, hint_default_black, OCC_FILTER, OCC_REPEAT;
#endif
#ifdef OCC_AO
uniform sampler2D texture_ambient_occlusion : hint_default_white, OCC_FILTER, OCC_REPEAT;
uniform vec4 ao_texture_channel = vec4(1.0, 0.0, 0.0, 0.0);
uniform float ao_light_affect = 0.0;
#endif
#ifdef OCC_DOUBLE_SIDED
// back faces seen through the hole (the inside of a cut wall) are darkened by this much: 0 = own colour, 1 = black
uniform float interior_darkness : hint_range(0.0, 1.0) = 0.5;
#endif

void vertex() {
    UV = UV * uv1_scale.xy + uv1_offset.xy;
#if defined(OCC_VERTEX_COLOR) && defined(OCC_VERTEX_COLOR_SRGB)
    if (!OUTPUT_IS_SRGB) {
        COLOR.rgb = mix(pow((COLOR.rgb + vec3(0.055)) * (1.0 / (1.0 + 0.055)), vec3(2.4)),
                        COLOR.rgb * (1.0 / 12.92), lessThan(COLOR.rgb, vec3(0.04045)));
    }
#endif
}

void fragment() {
    // not in the shadow pass: VIEW_MATRIX is the light's there, and cut-away walls still cast their shadows
    if (!IN_SHADOW_PASS) {
        OCCLUSION_DISCARD
    }

    vec2 base_uv = UV;
    vec4 albedo_tex = texture(texture_albedo, base_uv);
#ifdef OCC_VERTEX_COLOR
    albedo_tex *= COLOR;
#endif
    ALBEDO = albedo.rgb * albedo_tex.rgb;

    float metallic_tex = dot(texture(texture_metallic, base_uv), metallic_texture_channel);
    METALLIC = metallic_tex * metallic;
    SPECULAR = specular;
    float roughness_tex = dot(texture(texture_roughness, base_uv), roughness_texture_channel);
    ROUGHNESS = roughness_tex * roughness;

#ifdef OCC_NORMAL_MAP
    NORMAL_MAP = texture(texture_normal, base_uv).rgb;
    NORMAL_MAP_DEPTH = normal_scale;
#endif
#ifdef OCC_EMISSION
    vec3 emission_tex = texture(texture_emission, base_uv).rgb;
#ifdef OCC_EMISSION_MULTIPLY
    EMISSION = (emission.rgb * emission_tex) * emission_energy;
#else
    EMISSION = (emission.rgb + emission_tex) * emission_energy;
#endif
#endif
#if defined(OCC_ALPHA) || defined(OCC_ALPHA_SCISSOR)
    ALPHA = albedo.a * albedo_tex.a;
#endif
#ifdef OCC_ALPHA_SCISSOR
    ALPHA_SCISSOR_THRESHOLD = alpha_scissor_threshold;
#endif
#ifdef OCC_AO
    AO = dot(texture(texture_ambient_occlusion, base_uv), ao_texture_channel);
    AO_LIGHT_AFFECT = ao_light_affect;
#endif
#ifdef OCC_DOUBLE_SIDED
    if (!FRONT_FACING)
        ALBEDO *= 1.0 - interior_darkness * OCCLUSION_INTERIOR;
#endif
}
