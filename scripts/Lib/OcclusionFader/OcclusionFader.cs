using System.Linq;
using Godot;
using TnT.Extensions;

namespace TnT.Systems.OcclusionFade
{
    /// <summary>
    /// Applies the occlusion-fader shader to all MeshInstance3D and GridMap nodes in 
    /// the scene.
    /// Attach the script to the root node of your scene, or make a new node and place it anywhere in the scene.
    /// </summary>
    public partial class OcclusionFader : Node3D
    {
        /// <summary>The occlusion fader shader (occlusionFader.gdshader).</summary>
        [Export] private Shader OcclusionShader { get; set; }
        CharacterController3D _playerController;
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

        public override void _Ready()
        {
            _playerController = GetTree().FindAnyObjectByType<Player>().GetParent() as CharacterController3D;
            _camera = _playerController.Camera;
            _playerRayCast = _playerController.OcclusionRaycast;
            GetTree()
                .FindObjectsByType<MeshInstance3D>()
                .Where(n => n.FindAncestorOfType<CharacterController3D>() == null)
                .ForEach(ApplyToMeshInstance);

            foreach (var gm in GetTree().FindObjectsByType<GridMap>())
                ApplyToGridMap(gm);

            RenderingServer.GlobalShaderParameterSet("occlusion_voffset", VerticalOffset);
        }

        public override void _Process(double delta)
        {
            _playerRayCast.TargetPosition = _playerRayCast.ToLocal(_camera.GlobalPosition);

            float targetRadius = _playerRayCast.IsColliding() ? MaxRadius : 0.0f;
            _currentRadius = Mathf.MoveToward(_currentRadius, targetRadius, (float)delta * FadeSpeed * MaxRadius);

            RenderingServer.GlobalShaderParameterSet("player_pos", _playerController.GlobalPosition);
            RenderingServer.GlobalShaderParameterSet("occlusion_radius", _currentRadius);
            RenderingServer.GlobalShaderParameterSet("camera_pos", _camera.GlobalPosition);
        }

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
            }
        }

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
