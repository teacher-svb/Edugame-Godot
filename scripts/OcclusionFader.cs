using Godot;
using System.Collections.Generic;
using TnT.Extensions;

namespace TnT.EduGame
{
    /// <summary>
    /// Applies the occlusion-fader shader to all MeshInstance3D and GridMap nodes
    /// in the scene (or only those in the "occludable" group when UseGroup is true).
    /// Place this node anywhere in the scene tree.
    /// </summary>
    public partial class OcclusionFader : Node3D
    {
        [Export] public Shader OcclusionShader { get; set; }
        CharacterController3D _playerController;
        Camera3D _camera;
        RayCast3D _playerRayCast;
        [Export] public float FadeSpeed { get; set; } = 2.0f;
        [Export] public float MaxRadius { get; set; } = 2.0f;
        /// <summary>
        /// How far above the player's origin the hole is centered.
        /// 0 = player at center of hole, ~1 = player at bottom of hole.
        /// </summary>
        [Export] public float VerticalOffset { get; set; } = 1.0f;
        /// <summary>
        /// When true, only nodes in the "occludable" group receive the shader.
        /// When false, every MeshInstance3D and GridMap in the scene is affected.
        /// </summary>
        [Export] public bool UseGroup { get; set; } = false;

        private readonly List<ShaderMaterial> _allMaterials = new();
        private float _currentRadius = 0.0f;

        public override void _Ready()
        {
            _playerController = GetTree().FindAnyObjectByType<Player>().GetParent() as CharacterController3D;
            _camera = _playerController.Camera;
            _playerRayCast = _playerController.OcclusionRaycast;
            if (UseGroup)
            {
                foreach (var node in GetTree().GetNodesInGroup("occludable"))
                {
                    if (node is MeshInstance3D mi) ApplyToMeshInstance(mi);
                    else if (node is GridMap gm) ApplyToGridMap(gm);
                }
            }
            else
            {
                foreach (var mi in GetTree().FindObjectsByType<MeshInstance3D>())
                {
                    if (_playerController.IsAncestorOf(mi)) continue;
                    ApplyToMeshInstance(mi);
                }

                foreach (var gm in GetTree().FindObjectsByType<GridMap>())
                    ApplyToGridMap(gm);
            }

        }

        public override void _Process(double delta)
        {
            RenderingServer.GlobalShaderParameterSet("player_pos", _playerController.GlobalPosition);

            var camPos = _camera.GlobalPosition;
            foreach (var mat in _allMaterials)
            {
                mat.SetShaderParameter("camera_pos", camPos);
                mat.SetShaderParameter("vertical_offset", VerticalOffset);
            }

            _playerRayCast.TargetPosition = _playerRayCast.ToLocal(camPos);


            float targetRadius = _playerRayCast.IsColliding() ? MaxRadius : 0.0f;
            _currentRadius = Mathf.MoveToward(_currentRadius, targetRadius, (float)delta * FadeSpeed * MaxRadius);

            foreach (var mat in _allMaterials)
                mat.SetShaderParameter("hole_radius", _currentRadius);
        }

        // -------------------------------------------------------------------------

        // MeshInstance3D: use surface override so the original mesh asset is untouched
        private void ApplyToMeshInstance(MeshInstance3D meshInstance)
        {
            var mesh = meshInstance.Mesh;
            if (mesh == null) return;

            for (int s = 0; s < mesh.GetSurfaceCount(); s++)
            {
                var existing = meshInstance.GetSurfaceOverrideMaterial(s)
                               ?? mesh.SurfaceGetMaterial(s);

                var mat = BuildMaterial(existing as BaseMaterial3D);
                meshInstance.SetSurfaceOverrideMaterial(s, mat);
                _allMaterials.Add(mat);
            }
        }

        // GridMap: must duplicate the MeshLibrary and replace materials inside it
        private void ApplyToGridMap(GridMap gridMap)
        {
            gridMap.MeshLibrary = (MeshLibrary)gridMap.MeshLibrary.Duplicate();

            foreach (int id in gridMap.MeshLibrary.GetItemList())
            {
                var mesh = gridMap.MeshLibrary.GetItemMesh(id);
                if (mesh == null) continue;

                mesh = (Mesh)mesh.Duplicate();
                for (int s = 0; s < mesh.GetSurfaceCount(); s++)
                {
                    var mat = BuildMaterial(mesh.SurfaceGetMaterial(s) as BaseMaterial3D);
                    mesh.SurfaceSetMaterial(s, mat);
                    _allMaterials.Add(mat);
                }
                gridMap.MeshLibrary.SetItemMesh(id, mesh);
            }
        }

        private ShaderMaterial BuildMaterial(BaseMaterial3D source)
        {
            var mat = new ShaderMaterial { Shader = OcclusionShader };
            if (source != null)
            {
                mat.SetShaderParameter("albedo_texture", source.AlbedoTexture);
                mat.SetShaderParameter("albedo_color", source.AlbedoColor);
            }
            return mat;
        }
    }
}
