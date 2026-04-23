using Godot;

[GlobalClass]
public partial class NavigationPathDebugger : MeshInstance3D
{
    [Export] NavigationAgent3D _agent;
    [Export] Color _color = Colors.Red;
    [Export] float _sphereRadius = 0.1f;

    ImmediateMesh _mesh;
    StandardMaterial3D _material;

    public override void _Ready()
    {
        _mesh = new ImmediateMesh();
        Mesh = _mesh;

        _material = new StandardMaterial3D
        {
            AlbedoColor = _color,
            ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded,
            VertexColorUseAsAlbedo = true
        };
        MaterialOverride = _material;
    }

    public override void _Process(double delta)
    {
        _mesh.ClearSurfaces();

        var path = _agent.GetCurrentNavigationPath();
        var next = _agent.GetNextPathPosition();
        if (path.Length < 2) return;

        // Draw lines between path points
        _mesh.SurfaceBegin(Mesh.PrimitiveType.Lines);
        for (int i = 0; i < path.Length - 1; i++)
        {
            _mesh.SurfaceSetColor(_color);
            _mesh.SurfaceAddVertex(path[i] - GlobalPosition);
            _mesh.SurfaceSetColor(_color);
            _mesh.SurfaceAddVertex(path[i + 1] - GlobalPosition);
        }
        _mesh.SurfaceEnd();

        // Draw spheres at each waypoint
        _mesh.SurfaceBegin(Mesh.PrimitiveType.Triangles);
        foreach (var point in path)
        {
            DrawSphere(point - GlobalPosition, _sphereRadius, _color);
        }
        DrawSphere(next, .15f, Colors.Yellow);
        _mesh.SurfaceEnd();
    }

    private void DrawSphere(Vector3 center, float radius, Color color)
    {
        int segments = 24;
        for (int i = 0; i < segments; i++)
        {
            float lat0 = Mathf.Pi * (-0.5f + (float)i / segments);
            float lat1 = Mathf.Pi * (-0.5f + (float)(i + 1) / segments);
            float z0 = Mathf.Sin(lat0) * radius;
            float z1 = Mathf.Sin(lat1) * radius;
            float r0 = Mathf.Cos(lat0) * radius;
            float r1 = Mathf.Cos(lat1) * radius;

            for (int j = 0; j < segments; j++)
            {
                float lng0 = 2 * Mathf.Pi * (float)j / segments;
                float lng1 = 2 * Mathf.Pi * (float)(j + 1) / segments;

                Vector3 v0 = center + new Vector3(r0 * Mathf.Cos(lng0), z0, r0 * Mathf.Sin(lng0));
                Vector3 v1 = center + new Vector3(r0 * Mathf.Cos(lng1), z0, r0 * Mathf.Sin(lng1));
                Vector3 v2 = center + new Vector3(r1 * Mathf.Cos(lng0), z1, r1 * Mathf.Sin(lng0));
                Vector3 v3 = center + new Vector3(r1 * Mathf.Cos(lng1), z1, r1 * Mathf.Sin(lng1));

                _mesh.SurfaceSetColor(color);
                _mesh.SurfaceAddVertex(v0);
                _mesh.SurfaceAddVertex(v1);
                _mesh.SurfaceAddVertex(v2);

                _mesh.SurfaceSetColor(color);
                _mesh.SurfaceAddVertex(v1);
                _mesh.SurfaceAddVertex(v3);
                _mesh.SurfaceAddVertex(v2);
            }
        }
    }
}