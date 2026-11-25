using System.Collections.Generic;
using System.Linq;
using Godot;
using TnT.Extensions;

namespace TnT.Systems.OcclusionFade
{
    /// <summary>
    /// Applies the occlusion-fader shader to the MeshInstance3D and GridMap nodes of the scene it is in.
    /// Attach the script to the root node of your scene, or make a new node and place it anywhere in the scene.
    /// </summary>
    /// <remarks>
    /// <para>Scope: the scene this node belongs to (its topmost ancestor below the tree root), not the autoloads
    /// (e.g. the dialogue closeup's copy of a character model). Meshes under an <see cref="IOcclusionExempt"/> node,
    /// and meshes and GridMaps in the <c>occlusion_exclude</c> group, are left alone.</para>
    /// <para>Each <see cref="BaseMaterial3D"/> is replaced by one occlusion <see cref="ShaderMaterial"/> made for it
    /// (cached per source material: meshes sharing a material share the replacement), which keeps only its albedo
    /// texture and colour (normal maps, transparency, emission etc. are not carried over). A surface without a
    /// material gets one shared plain (white) occlusion material. A <see cref="ShaderMaterial"/> (or any other
    /// non-<see cref="BaseMaterial3D"/>) is left as it is: it doesn't fade. A mesh's
    /// <see cref="GeometryInstance3D.MaterialOverride"/>, when set, is what renders, so that is the one replaced
    /// (the surface materials are then left alone). GridMaps get a converted copy of their MeshLibrary (one copy
    /// per library, shared by GridMaps using the same one).</para>
    /// <para>Meshes and GridMaps that enter the scene after this node's <c>_Ready</c> (e.g. spawned by a
    /// <c>DungeonPopulator</c>, or instanced later by a script) are converted as they enter the tree. A mesh whose
    /// <c>Mesh</c> or materials are assigned after it entered the tree is not seen again.</para>
    /// </remarks>
    public partial class OcclusionFader : Node3D
    {
        /// <summary>The occlusion fader shader (occlusionFader.gdshader).</summary>
        [Export] private Shader OcclusionShader { get; set; }
        Node3D _playerCharacter;
        Camera3D _camera;
        RayCast3D _playerRayCast;
        /// <summary>How fast the hole opens and closes, in units per second.</summary>
        [Export] private float FadeSpeed { get; set; } = 2.0f;
        /// <summary>Maximum radius of the occlusion hole when fully open. Should match hole_radius in the shader.</summary>
        [Export] private float MaxRadius { get; set; } = 2.0f;
        /// <summary>
        /// How far above the player's origin the hole is centered.
        /// 0 = player at center of hole, ~1 = player at bottom of hole.
        /// </summary>
        [Export] private float VerticalOffset { get; set; } = 1.0f;

        private float _currentRadius = 0.0f;

        private const string ExcludeGroup = "occlusion_exclude";

        // scene the fader works on; caches live as long as this fader (one level)
        Node _scope;
        readonly Dictionary<Material, ShaderMaterial> _materials = new();
        ShaderMaterial _plainMaterial;
        readonly Dictionary<MeshLibrary, MeshLibrary> _libraries = new();

        public override void _Ready()
        {
            var target = GetTree().Root.FindObjectsByType<Node>(includeSelf: false).OfType<IOcclusionTarget>().FirstOrDefault();
            if (target == null)
            {
                GD.PushWarning($"{nameof(OcclusionFader)}: no {nameof(IOcclusionTarget)} in the scene, fading off.");
                SetProcess(false);
                return;
            }
            _playerCharacter = target.OcclusionBody;
            _camera = target.OcclusionCamera;
            _playerRayCast = target.OcclusionRaycast;

            _scope = this;
            while (_scope.GetParent() is Node parent && parent != GetTree().Root)
                _scope = parent;

            foreach (var meshInstance in _scope.FindObjectsByType<MeshInstance3D>(includeSelf: true))
                TryApply(meshInstance);
            foreach (var gridMap in _scope.FindObjectsByType<GridMap>(includeSelf: true))
                TryApply(gridMap);

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
            if (_scope != null && (node is MeshInstance3D || node is GridMap) && _scope.IsAncestorOf(node))
                TryApply(node);
        }

        void TryApply(Node node)
        {
            if (node.IsInGroup(ExcludeGroup))
                return;
            if (node is MeshInstance3D meshInstance && !IsExempt(meshInstance))
                ApplyToMeshInstance(meshInstance);
            else if (node is GridMap gridMap)
                ApplyToGridMap(gridMap);
        }

        public override void _Process(double delta)
        {
            _playerRayCast.TargetPosition = _playerRayCast.ToLocal(_camera.GlobalPosition);

            float targetRadius = IsCollidingWithOccluder() ? MaxRadius : 0.0f;
            _currentRadius = Mathf.MoveToward(_currentRadius, targetRadius, (float)delta * FadeSpeed * MaxRadius);

            RenderingServer.GlobalShaderParameterSet("player_pos", _playerCharacter.GlobalPosition);
            RenderingServer.GlobalShaderParameterSet("occlusion_radius", _currentRadius);
            RenderingServer.GlobalShaderParameterSet("camera_pos", _camera.GlobalPosition);
        }

        private static bool IsExempt(Node node)
        {
            for (var n = node.GetParent(); n != null; n = n.GetParent())
                if (n is IOcclusionExempt) return true;
            return false;
        }

        private bool IsCollidingWithOccluder()
        {
            if (!_playerRayCast.IsColliding()) return false;
            var node = _playerRayCast.GetCollider() as Node;
            while (node != null)
            {
                if (node.IsInGroup(ExcludeGroup)) return false;
                node = node.GetParent();
            }
            return true;
        }

        private void ApplyToMeshInstance(MeshInstance3D meshInstance)
        {
            if (meshInstance.MaterialOverride != null)
            {
                // the override renders instead of every surface material
                if (TryConvert(meshInstance.MaterialOverride, out var overrideMat))
                    meshInstance.MaterialOverride = overrideMat;
                return;
            }

            var mesh = meshInstance.Mesh;
            if (mesh == null) return;

            for (int s = 0; s < mesh.GetSurfaceCount(); s++)
            {
                var existing = meshInstance.GetSurfaceOverrideMaterial(s)
                               ?? mesh.SurfaceGetMaterial(s);

                if (TryConvert(existing, out var mat))
                    meshInstance.SetSurfaceOverrideMaterial(s, mat);
            }
        }

        private void ApplyToGridMap(GridMap gridMap)
        {
            var source = gridMap.MeshLibrary;
            if (source == null) return;
            if (_libraries.TryGetValue(source, out var converted) || _libraries.ContainsValue(source))
            {
                // the same library as another GridMap (or already this fader's copy)
                if (converted != null)
                    gridMap.MeshLibrary = converted;
                return;
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
        }

        // The occlusion material for `source`, one per source material. False: keep `source` (a shader material,
        // or this fader's own material already).
        private bool TryConvert(Material source, out ShaderMaterial result)
        {
            result = null;
            switch (source)
            {
                case null:
                    result = _plainMaterial ??= new ShaderMaterial { Shader = OcclusionShader };
                    return true;
                case BaseMaterial3D baseMaterial:
                    if (!_materials.TryGetValue(baseMaterial, out result))
                    {
                        result = new ShaderMaterial { Shader = OcclusionShader };
                        result.SetShaderParameter("albedo_texture", baseMaterial.AlbedoTexture);
                        result.SetShaderParameter("albedo_color", baseMaterial.AlbedoColor);
                        _materials[baseMaterial] = result;
                    }
                    return true;
                default:
                    return false;
            }
        }
    }
}
