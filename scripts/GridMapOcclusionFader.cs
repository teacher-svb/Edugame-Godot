using Godot;
using System.Collections.Generic;

public partial class GridMapOcclusionFader : GridMap
{
    [Export] public Shader OcclusionShader { get; set; }
    [Export] public Node3D Player { get; set; }
    [Export] public Camera3D Camera { get; set; }
    [Export] public RayCast3D PlayerRayCast { get; set; }
    [Export] public float FadeSpeed { get; set; } = 2.0f; // units per second
    [Export] public float MaxRadius { get; set; } = 2.0f; // match hole_radius in shader

    private List<ShaderMaterial> _allMaterials = new();
    [Export] private float _currentRadius = 0.0f;

    public override void _Ready()
    {
        MeshLibrary = (MeshLibrary)MeshLibrary.Duplicate();
        foreach (int id in MeshLibrary.GetItemList())
        {
            var mesh = MeshLibrary.GetItemMesh(id);
            if (mesh == null) continue;
            MeshLibrary.SetItemMesh(id, ApplyMaterialOverride(mesh));
        }

        foreach (int id in MeshLibrary.GetItemList())
        {
            var mesh = MeshLibrary.GetItemMesh(id);
            if (mesh == null) continue;
            for (int s = 0; s < mesh.GetSurfaceCount(); s++)
            {
                if (mesh.SurfaceGetMaterial(s) is ShaderMaterial sm)
                    _allMaterials.Add(sm);
            }
        }
    }

    public override void _Process(double delta)
    {
        RenderingServer.GlobalShaderParameterSet("player_pos", Player.GlobalPosition);

        var camPos = Camera.GlobalPosition;
        foreach (var mat in _allMaterials)
            mat.SetShaderParameter("camera_pos", camPos);

        PlayerRayCast.TargetPosition = PlayerRayCast.ToLocal(camPos);

        float targetRadius = PlayerRayCast.IsColliding() ? MaxRadius : 0.0f;
        _currentRadius = Mathf.MoveToward(_currentRadius, targetRadius, (float)delta * FadeSpeed * MaxRadius);

        foreach (var mat in _allMaterials)
            mat.SetShaderParameter("hole_radius", _currentRadius);
    }

    private Mesh ApplyMaterialOverride(Mesh original)
    {
        var mesh = (Mesh)original.Duplicate();
        for (int s = 0; s < mesh.GetSurfaceCount(); s++)
        {
            var existingMat = mesh.SurfaceGetMaterial(s) as BaseMaterial3D;
            var mat = new ShaderMaterial();
            mat.Shader = OcclusionShader;
            if (existingMat != null)
            {
                mat.SetShaderParameter("albedo_texture", existingMat.AlbedoTexture);
                mat.SetShaderParameter("albedo_color", existingMat.AlbedoColor);
            }
            mesh.SurfaceSetMaterial(s, mat);
        }
        return mesh;
    }
}