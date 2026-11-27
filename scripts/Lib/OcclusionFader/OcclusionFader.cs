using System.Collections.Generic;
using System.Linq;
using Godot;
using TnT.Extensions;

namespace TnT.Systems.OcclusionFade
{
    /// <summary>
    /// Applies the occlusion-fader shader to the MeshInstance3D, MultiMeshInstance3D and GridMap nodes of the scene it
    /// is in.
    /// Attach the script to the root node of your scene, or make a new node and place it anywhere in the scene.
    /// </summary>
    /// <remarks>
    /// <para>Scope: the scene this node belongs to (its topmost ancestor below the tree root), not the autoloads
    /// (e.g. the dialogue closeup's copy of a character model). Meshes under an <see cref="IOcclusionExempt"/> node,
    /// and meshes and GridMaps in the <c>occlusion_exclude</c> group, are left alone.</para>
    /// <para>Each <see cref="BaseMaterial3D"/> is replaced by one occlusion <see cref="ShaderMaterial"/> made for it
    /// (cached per source material: meshes sharing a material share the replacement). Its shader is built at run
    /// time from occlusion_material.gdshaderinc for the material's features (one shader per combination) and keeps
    /// what the game's materials use: albedo texture/colour, vertex colour, texture filter and repeat, roughness,
    /// metallic, specular, normal map, emission, AO, UV1 scale/offset, transparency (alpha, scissor, depth
    /// pre-pass), cull, blend, depth and shading modes. A material with a feature outside that list (see
    /// <c>UnsupportedFeature</c>: e.g. triplanar, rim, ORM) is kept as it is, with a warning, and doesn't fade.
    /// A surface without a material gets one shared default occlusion material. A <see cref="ShaderMaterial"/> (or any
    /// other non-<see cref="BaseMaterial3D"/>) is left as it is: it doesn't fade unless its shader opts in
    /// (<c>#include "res://scripts/Lib/OcclusionFader/occlusion.gdshaderinc"</c> and <c>OCCLUSION_DISCARD</c> at
    /// the start of <c>fragment()</c>, guarded by <c>if (!IN_SHADOW_PASS)</c>). A mesh's
    /// <see cref="GeometryInstance3D.MaterialOverride"/>, when set, is what renders, so that is the one replaced
    /// (the surface materials are then left alone). GridMaps get a converted copy of their MeshLibrary (one copy
    /// per library, shared by GridMaps using the same one).</para>
    /// <para>Meshes and GridMaps that enter the scene after this node's <c>_Ready</c> (e.g. spawned by a
    /// <c>DungeonPopulator</c>, or instanced later by a script) are converted as they enter the tree. A mesh whose
    /// <c>Mesh</c> or materials are assigned after it entered the tree is not seen again.</para>
    /// <para>In the editor (a tool script for that only) it converts nothing; with <see cref="ShowInEditor"/> it draws
    /// the cut region for the player in the edited scene (see <see cref="DrawEditorPreview"/>).</para>
    /// </remarks>
    [Tool]
    public partial class OcclusionFader : Node3D
    {
        /// <summary>
        /// How much the inside of a cut wall (back faces seen through the hole, double-sided materials only) is darkened:
        /// 0 = its own colour, 1 = black.
        /// </summary>
        [Export(PropertyHint.Range, "0,1,0.05")] private float InteriorDarkness { get; set; } = 0.5f;
        const string MaterialTemplatePath = "res://scripts/Lib/OcclusionFader/occlusion_material.gdshaderinc";
        Node3D _playerCharacter;
        Camera3D _camera;

        /// <summary>When the hole is open.</summary>
        public enum OpenMode
        {
            /// <summary>Opens when the line from the camera to the player is blocked, closes after <see cref="CloseDelay"/>.</summary>
            Animated,
            /// <summary>Always fully open (the cut is invisible when nothing stands between camera and player).</summary>
            AlwaysOpen,
        }
        /// <summary>
        /// Animated: open when something the fader converted blocks the view of the player. AlwaysOpen: for busy levels
        /// (e.g. a forest), where the animation would keep opening and closing.
        /// </summary>
        [Export] private OpenMode Mode { get; set; } = OpenMode.Animated;
        /// <summary>How fast the hole opens and closes, in units per second.</summary>
        [Export] private float FadeSpeed { get; set; } = 2.0f;
        /// <summary>
        /// Animated mode: the hole starts closing only after the view has been clear for this long (seconds), so a short
        /// gap (between two houses, past a tree) doesn't close and reopen it.
        /// </summary>
        [Export(PropertyHint.Range, "0,3,0.05,or_greater")] private float CloseDelay { get; set; } = 0.75f;
        /// <summary>
        /// Animated mode: the height above the player's feet the line of sight is tested from (the player's middle).
        /// </summary>
        [Export(PropertyHint.Range, "0,2,0.05")] private float SightHeight { get; set; } = 0.6f;
        /// <summary>
        /// Radius of the occlusion hole when fully open, in metres at the player's distance from the camera. The hole is a
        /// circle on screen around the player (occlusion.gdshaderinc), so it looks the same size at every depth.
        /// </summary>
        [Export] private float MaxRadius { get; set; } = 2.0f;
        /// <summary>
        /// How far above the player's origin the hole is centered.
        /// 0 = player at center of hole, ~1 = player at bottom of hole.
        /// </summary>
        [Export] private float VerticalOffset { get; set; } = 1.0f;
        /// <summary>
        /// Horizontal cut-off plane: nothing up to this height above the player's feet is cut (the ground the player
        /// walks on, the bottom of a wall in front of the player).
        /// </summary>
        [Export(PropertyHint.Range, "0,2,0.05,or_greater")] private float CutOffHeight { get; set; } = 0.1f;
        /// <summary>
        /// Vertical cut-off plane: it stands this far in front of the player's centre, towards the camera, and nothing
        /// behind it is cut. The player's capsule radius is 0.18.
        /// </summary>
        [Export(PropertyHint.Range, "0,3,0.05,or_greater")] private float CutOffFront { get; set; } = 0.2f;
        /// <summary>
        /// Editor only: draw the cut region (fully open) for the player in the edited scene: the cone from the player
        /// camera through the hole (cyan), the vertical cut-off plane (red) and the horizontal one (orange).
        /// </summary>
        [Export] private bool ShowInEditor { get; set; } = true;
        /// <summary>
        /// Meshes on any of these render layers (VisualInstance3D.Layers) are never made see-through, e.g. the ground.
        /// Read when a mesh is converted (the level's start, or when it enters the tree): changing a mesh's layers
        /// later has no effect. GridMaps have no render layers: exclude those with the <c>occlusion_exclude</c> group.
        /// </summary>
        [Export(PropertyHint.Layers3DRender)] private uint ExcludeLayers { get; set; } = 0;

        private float _currentRadius = 0.0f;
        // time since the view of the player was last blocked (Animated mode)
        private double _sinceBlocked = double.PositiveInfinity;
        // the converted nodes, tested for blocking the view
        readonly OcclusionLineOfSight _lineOfSight = new();

        private const string ExcludeGroup = "occlusion_exclude";

        // scene the fader works on; caches live as long as this fader (one level)
        Node _scope;
        readonly Dictionary<Material, ShaderMaterial> _materials = new();
        ShaderMaterial _plainMaterial;
        readonly Dictionary<MeshLibrary, MeshLibrary> _libraries = new();
        readonly Dictionary<Mesh, Mesh> _meshCopies = new(); // MultiMesh meshes with several materials

        public override void _Ready()
        {
            if (Engine.IsEditorHint())
                return; // only the preview runs in the editor (_Process)

            var target = GetTree().Root.FindObjectsByType<Node>(includeSelf: false).OfType<IOcclusionTarget>().FirstOrDefault();
            if (target == null)
            {
                GD.PushWarning($"{nameof(OcclusionFader)}: no {nameof(IOcclusionTarget)} in the scene, fading off.");
                SetProcess(false);
                return;
            }
            _playerCharacter = target.OcclusionBody;
            _camera = target.OcclusionCamera;

            _scope = this;
            while (_scope.GetParent() is Node parent && parent != GetTree().Root)
                _scope = parent;

            foreach (var meshInstance in _scope.FindObjectsByType<MeshInstance3D>(includeSelf: true))
                TryApply(meshInstance);
            foreach (var gridMap in _scope.FindObjectsByType<GridMap>(includeSelf: true))
                TryApply(gridMap);
            foreach (var multiMesh in _scope.FindObjectsByType<MultiMeshInstance3D>(includeSelf: true))
                TryApply(multiMesh);

            // later additions (populators, scripts instancing scenes); disconnected in _ExitTree
            GetTree().NodeAdded += OnNodeAdded;

            RenderingServer.GlobalShaderParameterSet("occlusion_voffset", VerticalOffset);
        }

        public override void _ExitTree()
        {
            if (_scope != null)
                GetTree().NodeAdded -= OnNodeAdded;
            _scope = null;
        }

        void OnNodeAdded(Node node)
        {
            if (_scope != null && (node is MeshInstance3D || node is GridMap || node is MultiMeshInstance3D)
                && _scope.IsAncestorOf(node))
                TryApply(node);
        }

        void TryApply(Node node)
        {
            if (IsExcluded(node))
                return;
            // what got an occlusion material also opens the hole when it blocks the view of the player
            if (node is MeshInstance3D meshInstance && ApplyToMeshInstance(meshInstance))
                _lineOfSight.Add(meshInstance);
            else if (node is GridMap gridMap && ApplyToGridMap(gridMap))
                _lineOfSight.Add(gridMap);
            else if (node is MultiMeshInstance3D multiMesh && ApplyToMultiMesh(multiMesh))
                _lineOfSight.Add(multiMesh);
        }

        // MultiMeshes (e.g. scattered trees): the cut is per pixel, so only the material changes. A material override,
        // or one material for all surfaces: the instance's MaterialOverride (no resource is changed). Otherwise this
        // instance gets its own copy of the MultiMesh with a converted copy of the mesh (one per source mesh).
        // True when something was converted.
        private bool ApplyToMultiMesh(MultiMeshInstance3D instance)
        {
            if (instance.MaterialOverride != null)
            {
                if (!TryConvert(instance.MaterialOverride, out var overrideMat)) return false;
                instance.MaterialOverride = overrideMat;
                return true;
            }

            var mesh = instance.Multimesh?.Mesh;
            if (mesh == null || mesh.GetSurfaceCount() == 0) return false;

            var materials = Enumerable.Range(0, mesh.GetSurfaceCount()).Select(mesh.SurfaceGetMaterial).ToList();
            if (materials.All(m => m == materials[0]))
            {
                if (!TryConvert(materials[0], out var mat)) return false;
                instance.MaterialOverride = mat;
                return true;
            }

            if (!_meshCopies.TryGetValue(mesh, out var converted))
            {
                converted = (Mesh)mesh.Duplicate();
                bool any = false;
                for (int s = 0; s < converted.GetSurfaceCount(); s++)
                    if (TryConvert(converted.SurfaceGetMaterial(s), out var mat))
                    {
                        converted.SurfaceSetMaterial(s, mat);
                        any = true;
                    }
                _meshCopies[mesh] = converted = any ? converted : null;
            }
            if (converted == null) return false;
            var multimesh = (MultiMesh)instance.Multimesh.Duplicate();
            multimesh.Mesh = converted;
            instance.Multimesh = multimesh;
            return true;
        }

        // Never converted: on one of ExcludeLayers (meshes; a GridMap has no render layers), in the exclude group
        // itself or below a node in it, or below an IOcclusionExempt node.
        bool IsExcluded(Node node)
        {
            if (node is VisualInstance3D visual && (visual.Layers & ExcludeLayers) != 0)
                return true;
            if (node.IsInGroup(ExcludeGroup))
                return true;
            for (var n = node.GetParent(); n != null; n = n.GetParent())
                if (n is IOcclusionExempt || n.IsInGroup(ExcludeGroup))
                    return true;
            return false;
        }

        public override void _Process(double delta)
        {
            if (Engine.IsEditorHint())
            {
                DrawEditorPreview();
                return;
            }

            float targetRadius;
            if (Mode == OpenMode.AlwaysOpen)
                targetRadius = MaxRadius;
            else
            {
                _sinceBlocked = IsViewBlocked() ? 0.0 : _sinceBlocked + delta;
                targetRadius = _sinceBlocked <= CloseDelay ? MaxRadius : 0.0f;
            }
            _currentRadius = Mathf.MoveToward(_currentRadius, targetRadius, (float)delta * FadeSpeed * MaxRadius);

            RenderingServer.GlobalShaderParameterSet("player_pos", _playerCharacter.GlobalPosition);
            RenderingServer.GlobalShaderParameterSet("occlusion_radius", _currentRadius);
            RenderingServer.GlobalShaderParameterSet("camera_pos", _camera.GlobalPosition);
            // set every frame, so they can be tuned in the remote inspector while the game runs
            RenderingServer.GlobalShaderParameterSet("occlusion_cutoff_height", CutOffHeight);
            RenderingServer.GlobalShaderParameterSet("occlusion_cutoff_front", CutOffFront);
        }

        // Does converted geometry block the line from the camera to the player's middle, inside the region the shader
        // cuts (above CutOffHeight, on the camera's side of the vertical plane)? Only that geometry can be cut away.
        private bool IsViewBlocked()
        {
            var feet = _playerCharacter.GlobalPosition;
            var cam = _camera.GlobalPosition;
            var toCamera = new Vector2(cam.X - feet.X, cam.Z - feet.Z);
            float length = toCamera.Length();
            var h = length > 1e-3f ? toCamera / length : Vector2.Zero;
            return _lineOfSight.IsBlocked(feet + Vector3.Up * SightHeight, cam, hit =>
                hit.Y > feet.Y + CutOffHeight
                && (length <= 1e-3f || new Vector2(hit.X - feet.X, hit.Z - feet.Z).Dot(h) > CutOffFront));
        }

        // True when something was converted.
        private bool ApplyToMeshInstance(MeshInstance3D meshInstance)
        {
            if (meshInstance.MaterialOverride != null)
            {
                // the override renders instead of every surface material
                if (!TryConvert(meshInstance.MaterialOverride, out var overrideMat)) return false;
                meshInstance.MaterialOverride = overrideMat;
                return true;
            }

            var mesh = meshInstance.Mesh;
            if (mesh == null) return false;

            bool any = false;
            for (int s = 0; s < mesh.GetSurfaceCount(); s++)
            {
                var existing = meshInstance.GetSurfaceOverrideMaterial(s)
                               ?? mesh.SurfaceGetMaterial(s);

                if (TryConvert(existing, out var mat))
                {
                    meshInstance.SetSurfaceOverrideMaterial(s, mat);
                    any = true;
                }
            }
            return any;
        }

        // True when the GridMap uses a converted library.
        private bool ApplyToGridMap(GridMap gridMap)
        {
            var source = gridMap.MeshLibrary;
            if (source == null) return false;
            if (_libraries.TryGetValue(source, out var converted) || _libraries.ContainsValue(source))
            {
                // the same library as another GridMap (or already this fader's copy)
                if (converted != null)
                    gridMap.MeshLibrary = converted;
                return true;
            }

            // a copy: the library (and its meshes) are shared resources, used by other scenes too
            converted = (MeshLibrary)source.Duplicate();
            foreach (int id in converted.GetItemList())
            {
                var mesh = converted.GetItemMesh(id);
                if (mesh == null) continue;

                mesh = (Mesh)mesh.Duplicate();
                for (int s = 0; s < mesh.GetSurfaceCount(); s++)
                {
                    if (TryConvert(mesh.SurfaceGetMaterial(s), out var mat))
                        mesh.SurfaceSetMaterial(s, mat);
                }
                converted.SetItemMesh(id, mesh);
            }
            _libraries[source] = converted;
            gridMap.MeshLibrary = converted;
            return true;
        }

        // The occlusion material for `source`, one per source material. False: keep `source` (a shader material, this
        // fader's own material already, or a BaseMaterial3D with a feature the template doesn't have).
        private bool TryConvert(Material source, out ShaderMaterial result)
        {
            result = null;
            switch (source)
            {
                case null:
                    result = _plainMaterial ??= MakeMaterial(new StandardMaterial3D());
                    return result != null;
                case BaseMaterial3D baseMaterial:
                    if (!_materials.TryGetValue(baseMaterial, out result))
                        _materials[baseMaterial] = result = MakeMaterial(baseMaterial); // null: not supported, kept
                    return result != null;
                default:
                    return false;
            }
        }

        // generated shaders by code: materials with the same compile-time features share one
        readonly Dictionary<string, Shader> _shaders = new();

        // A ShaderMaterial from occlusion_material.gdshaderinc that looks like `m` (null: `m` uses a feature the
        // template doesn't have; a warning names it).
        ShaderMaterial MakeMaterial(BaseMaterial3D m)
        {
            string unsupported = UnsupportedFeature(m);
            if (unsupported != null)
            {
                GD.PushWarning($"{nameof(OcclusionFader)}: material {MaterialName(m)} uses {unsupported}, which the " +
                               "occlusion shader doesn't support; it stays as it is and doesn't fade.");
                return null;
            }

            var modes = new List<string>
            {
                m.BlendMode switch
                {
                    BaseMaterial3D.BlendModeEnum.Add => "blend_add",
                    BaseMaterial3D.BlendModeEnum.Sub => "blend_sub",
                    BaseMaterial3D.BlendModeEnum.Mul => "blend_mul",
                    BaseMaterial3D.BlendModeEnum.PremultAlpha => "blend_premul_alpha",
                    _ => "blend_mix",
                },
                m.DepthDrawMode switch
                {
                    BaseMaterial3D.DepthDrawModeEnum.Always => "depth_draw_always",
                    BaseMaterial3D.DepthDrawModeEnum.Disabled => "depth_draw_never",
                    _ => "depth_draw_opaque",
                },
                m.CullMode switch
                {
                    BaseMaterial3D.CullModeEnum.Front => "cull_front",
                    BaseMaterial3D.CullModeEnum.Disabled => "cull_disabled",
                    _ => "cull_back",
                },
                m.DiffuseMode switch
                {
                    BaseMaterial3D.DiffuseModeEnum.Lambert => "diffuse_lambert",
                    BaseMaterial3D.DiffuseModeEnum.LambertWrap => "diffuse_lambert_wrap",
                    BaseMaterial3D.DiffuseModeEnum.Toon => "diffuse_toon",
                    _ => "diffuse_burley",
                },
                m.SpecularMode switch
                {
                    BaseMaterial3D.SpecularModeEnum.Toon => "specular_toon",
                    BaseMaterial3D.SpecularModeEnum.Disabled => "specular_disabled",
                    _ => "specular_schlick_ggx",
                },
            };
            if (m.ShadingMode == BaseMaterial3D.ShadingModeEnum.Unshaded) modes.Add("unshaded");
            if (m.ShadingMode == BaseMaterial3D.ShadingModeEnum.PerVertex) modes.Add("vertex_lighting");
            if (m.NoDepthTest) modes.Add("depth_test_disabled");
            if (m.DisableReceiveShadows) modes.Add("shadows_disabled");
            if (m.DisableAmbientLight) modes.Add("ambient_light_disabled");
            if (m.DisableFog) modes.Add("fog_disabled");
            if (m.Transparency == BaseMaterial3D.TransparencyEnum.AlphaDepthPrePass) modes.Add("depth_prepass_alpha");

            var defines = new List<string>
            {
                "OCC_FILTER " + m.TextureFilter switch
                {
                    BaseMaterial3D.TextureFilterEnum.Nearest => "filter_nearest",
                    BaseMaterial3D.TextureFilterEnum.Linear => "filter_linear",
                    BaseMaterial3D.TextureFilterEnum.NearestWithMipmaps => "filter_nearest_mipmap",
                    BaseMaterial3D.TextureFilterEnum.NearestWithMipmapsAnisotropic => "filter_nearest_mipmap_anisotropic",
                    BaseMaterial3D.TextureFilterEnum.LinearWithMipmapsAnisotropic => "filter_linear_mipmap_anisotropic",
                    _ => "filter_linear_mipmap",
                },
                "OCC_REPEAT " + (m.TextureRepeat ? "repeat_enable" : "repeat_disable"),
            };
            if (m.VertexColorUseAsAlbedo) defines.Add("OCC_VERTEX_COLOR");
            if (m.VertexColorUseAsAlbedo && m.VertexColorIsSrgb) defines.Add("OCC_VERTEX_COLOR_SRGB");
            if (m.NormalEnabled) defines.Add("OCC_NORMAL_MAP");
            if (m.EmissionEnabled) defines.Add("OCC_EMISSION");
            if (m.EmissionEnabled && m.EmissionOperator == BaseMaterial3D.EmissionOperatorEnum.Multiply)
                defines.Add("OCC_EMISSION_MULTIPLY");
            if (m.AOEnabled) defines.Add("OCC_AO");
            if (m.Transparency is BaseMaterial3D.TransparencyEnum.Alpha or BaseMaterial3D.TransparencyEnum.AlphaDepthPrePass)
                defines.Add("OCC_ALPHA");
            if (m.Transparency == BaseMaterial3D.TransparencyEnum.AlphaScissor) defines.Add("OCC_ALPHA_SCISSOR");
            // double-sided sources also darken their back faces seen through the hole
            if (m.CullMode != BaseMaterial3D.CullModeEnum.Back) defines.Add("OCC_DOUBLE_SIDED");

            string code = "shader_type spatial;\n"
                          + $"render_mode {string.Join(", ", modes)};\n"
                          + string.Concat(defines.Select(d => $"#define {d}\n"))
                          + $"#include \"{MaterialTemplatePath}\"\n";
            if (!_shaders.TryGetValue(code, out var shader))
                _shaders[code] = shader = new Shader { Code = code };

            var r = new ShaderMaterial { Shader = shader, RenderPriority = m.RenderPriority, NextPass = m.NextPass };
            r.SetShaderParameter("albedo", m.AlbedoColor);
            r.SetShaderParameter("texture_albedo", m.AlbedoTexture);
            r.SetShaderParameter("roughness", m.Roughness);
            r.SetShaderParameter("texture_roughness", m.RoughnessTexture);
            r.SetShaderParameter("roughness_texture_channel", Channel(m.RoughnessTextureChannel));
            r.SetShaderParameter("metallic", m.Metallic);
            r.SetShaderParameter("specular", m.MetallicSpecular);
            r.SetShaderParameter("texture_metallic", m.MetallicTexture);
            r.SetShaderParameter("metallic_texture_channel", Channel(m.MetallicTextureChannel));
            r.SetShaderParameter("uv1_scale", m.Uv1Scale);
            r.SetShaderParameter("uv1_offset", m.Uv1Offset);
            r.SetShaderParameter("alpha_scissor_threshold", m.AlphaScissorThreshold);
            r.SetShaderParameter("texture_normal", m.NormalTexture);
            r.SetShaderParameter("normal_scale", m.NormalScale);
            r.SetShaderParameter("emission", m.Emission);
            r.SetShaderParameter("emission_energy", m.EmissionEnergyMultiplier);
            r.SetShaderParameter("texture_emission", m.EmissionTexture);
            r.SetShaderParameter("texture_ambient_occlusion", m.AOTexture);
            r.SetShaderParameter("ao_texture_channel", Channel(m.AOTextureChannel));
            r.SetShaderParameter("ao_light_affect", m.AOLightAffect);
            r.SetShaderParameter("interior_darkness", InteriorDarkness);
            return r;
        }

        static Vector4 Channel(BaseMaterial3D.TextureChannel channel) => channel switch
        {
            BaseMaterial3D.TextureChannel.Green => new Vector4(0, 1, 0, 0),
            BaseMaterial3D.TextureChannel.Blue => new Vector4(0, 0, 1, 0),
            BaseMaterial3D.TextureChannel.Alpha => new Vector4(0, 0, 0, 1),
            BaseMaterial3D.TextureChannel.Grayscale => new Vector4(1f / 3f, 1f / 3f, 1f / 3f, 0),
            _ => new Vector4(1, 0, 0, 0),
        };

        // the first feature of `m` that occlusion_material.gdshaderinc doesn't reproduce, or null
        static string UnsupportedFeature(BaseMaterial3D m)
        {
            if (m is OrmMaterial3D) return "an ORM texture";
            if (m.Transparency == BaseMaterial3D.TransparencyEnum.AlphaHash) return "alpha hash transparency";
            if (m.BillboardMode != BaseMaterial3D.BillboardModeEnum.Disabled) return "billboard mode";
            if (m.Uv1Triplanar || m.UV2Triplanar) return "triplanar mapping";
            if (m.DetailEnabled) return "detail";
            if (m.HeightmapEnabled) return "a height map";
            if (m.RimEnabled) return "rim";
            if (m.ClearcoatEnabled) return "clearcoat";
            if (m.AnisotropyEnabled) return "anisotropy";
            if (m.SubsurfScatterEnabled) return "subsurface scattering";
            if (m.BacklightEnabled) return "backlight";
            if (m.RefractionEnabled) return "refraction";
            if (m.Grow) return "grow";
            if (m.ProximityFadeEnabled) return "proximity fade";
            if (m.DistanceFadeMode != BaseMaterial3D.DistanceFadeModeEnum.Disabled) return "distance fade";
            if (m.AOEnabled && m.AOOnUV2) return "AO on UV2";
            if (m.EmissionEnabled && m.EmissionOnUV2) return "emission on UV2";
            if (m.UsePointSize) return "point size";
            if (m.FixedSize) return "fixed size";
            return null;
        }

        static string MaterialName(Material m) =>
            !string.IsNullOrEmpty(m.ResourcePath) ? m.ResourcePath
            : !string.IsNullOrEmpty(m.ResourceName) ? m.ResourceName : m.ToString();

        // ---- editor preview ----

        const string PlayerScriptPath = "res://scripts/Game/Character/Player.cs";
        MeshInstance3D _preview;
        ImmediateMesh _previewMesh;
        Node3D _previewBody;
        Camera3D _previewCamera;

        /// <summary>
        /// Editor only. Draws the region the shader cuts when the hole is fully open (<see cref="MaxRadius"/>), for the
        /// player in the edited scene: the node with Player.cs, its character root (the feet) and its
        /// <c>PlayerCamera/Camera3D</c>. The camera aims at the feet, as <c>CameraLookAt</c> does in the game.
        /// Drawn in an internal child without owner, so nothing is saved into the scene.
        /// </summary>
        void DrawEditorPreview()
        {
            if (!ShowInEditor || !FindPreviewTarget())
            {
                if (_preview != null) _preview.Visible = false;
                return;
            }
            if (_preview == null || !IsInstanceValid(_preview))
            {
                _previewMesh = new ImmediateMesh();
                _preview = new MeshInstance3D
                {
                    Mesh = _previewMesh,
                    TopLevel = true,
                    CastShadow = GeometryInstance3D.ShadowCastingSetting.Off,
                    MaterialOverride = new StandardMaterial3D
                    {
                        ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded,
                        VertexColorUseAsAlbedo = true,
                        NoDepthTest = true,
                        DisableFog = true,
                    },
                };
                _preview.AddToGroup(ExcludeGroup); // never converted by a fader
                AddChild(_preview, false, InternalMode.Back);
            }
            _preview.Visible = true;
            _preview.GlobalTransform = Transform3D.Identity;

            Vector3 cam = _previewCamera.GlobalPosition;
            Vector3 feet = _previewBody.GlobalPosition;
            Vector3 center = feet + Vector3.Up * VerticalOffset;
            Vector3 forward = (feet - cam).Normalized();
            if (forward.IsZeroApprox() || Mathf.Abs(forward.Dot(Vector3.Up)) > 0.999f)
            {
                _preview.Visible = false;
                return;
            }
            Vector3 right = forward.Cross(Vector3.Up).Normalized();
            Vector3 up = right.Cross(forward);
            float centerDepth = (center - cam).Dot(forward);
            Vector3 lateral = center - cam - forward * centerDepth; // the hole's centre, off the view axis

            // a point of the circle (same on screen at every depth) at view depth `depth`, angle `a`
            Vector3 CirclePoint(float depth, float a) =>
                cam + forward * depth + (lateral + MaxRadius * (Mathf.Cos(a) * right + Mathf.Sin(a) * up)) * (depth / centerDepth);

            _previewMesh.ClearSurfaces();
            _previewMesh.SurfaceBegin(Mesh.PrimitiveType.Lines);

            // cone: circles from near the camera to past the player, lines from the camera
            var cyan = new Color(0.2f, 1f, 1f);
            const int segments = 32;
            float farDepth = centerDepth * 1.25f;
            foreach (float f in new[] { 0.25f, 0.5f, 0.75f, 1f, 1.25f })
                for (int i = 0; i < segments; i++)
                    Line(CirclePoint(centerDepth * f, Mathf.Tau * i / segments),
                         CirclePoint(centerDepth * f, Mathf.Tau * (i + 1) / segments), cyan);
            for (int i = 0; i < 8; i++)
                Line(cam, CirclePoint(farDepth, Mathf.Tau * i / 8), cyan);
            Line(center - right * 0.2f, center + right * 0.2f, cyan);
            Line(center - Vector3.Up * 0.2f, center + Vector3.Up * 0.2f, cyan);

            // cut-off planes: vertical one facing the camera, horizontal one at the feet (towards the camera)
            Vector3 toCamera = new Vector3(cam.X - feet.X, 0f, cam.Z - feet.Z);
            float camDistance = toCamera.Length();
            Vector3 h = toCamera / camDistance;
            Vector3 side = h.Cross(Vector3.Up).Normalized() * (MaxRadius + 1f);
            float bottom = feet.Y + CutOffHeight;
            float top = center.Y + MaxRadius + 1f;
            Vector3 front = new Vector3(feet.X, 0f, feet.Z) + h * CutOffFront;
            Rect(new Vector3(front.X, bottom, front.Z), side, Vector3.Up * (top - bottom), new Color(1f, 0.25f, 0.25f));
            Rect(new Vector3(front.X, bottom, front.Z), side, h * Mathf.Max(camDistance - CutOffFront, 0f),
                 new Color(1f, 0.6f, 0.1f));

            _previewMesh.SurfaceEnd();
        }

        void Line(Vector3 a, Vector3 b, Color color)
        {
            _previewMesh.SurfaceSetColor(color);
            _previewMesh.SurfaceAddVertex(a);
            _previewMesh.SurfaceSetColor(color);
            _previewMesh.SurfaceAddVertex(b);
        }

        // outline of the rectangle from `edge - side` to `edge + side`, extended by `extent`, with a few cross lines
        void Rect(Vector3 edge, Vector3 side, Vector3 extent, Color color)
        {
            Vector3 a = edge - side, b = edge + side, c = b + extent, d = a + extent;
            Line(a, b, color); Line(b, c, color); Line(c, d, color); Line(d, a, color);
            for (int i = 1; i < 4; i++)
                Line(a + extent * (i / 4f), b + extent * (i / 4f), color);
        }

        // the player in the edited scene (Player.cs is not a tool script, so it's found by its script path)
        bool FindPreviewTarget()
        {
            if (_previewBody != null && IsInstanceValid(_previewBody) && _previewBody.IsInsideTree()
                && _previewCamera != null && IsInstanceValid(_previewCamera) && _previewCamera.IsInsideTree())
                return true;
            _previewBody = null;
            _previewCamera = null;
            var root = GetTree().EditedSceneRoot;
            if (root == null)
                return false;
            foreach (var node in root.FindChildren("*", "", true, false).Prepend(root))
            {
                if ((node.GetScript().Obj as Script)?.ResourcePath != PlayerScriptPath)
                    continue;
                // the character root: the Player instance's root (Player.cs sits on a child of it)
                var body = (node.Owner != root ? node.Owner : null) as Node3D ?? node.GetParent() as Node3D;
                var camera = body?.GetNodeOrNull<Camera3D>("PlayerCamera/Camera3D");
                if (body == null || camera == null)
                    continue;
                _previewBody = body;
                _previewCamera = camera;
                return true;
            }
            return false;
        }
    }
}
