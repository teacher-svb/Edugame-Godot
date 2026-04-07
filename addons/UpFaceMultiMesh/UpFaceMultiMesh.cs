#if TOOLS
using Godot;
using System;

[Tool]
public partial class UpFaceMultiMesh : EditorPlugin
{
    public override void _EnterTree()
    {
        AddToolMenuItem("Populate Upward Faces", Callable.From(PopulateUpwardFaces));
    }

    public override void _ExitTree()
    {
        RemoveToolMenuItem("Populate Upward Faces");
    }

    private void PopulateUpwardFaces()
    {
        var selected = EditorInterface.Singleton.GetSelection().GetSelectedNodes();
        if (selected.Count == 0) return;

        // Expect selection: [MultiMeshInstance3D, MeshInstance3D]
        MultiMeshInstance3D mmi = null;
        MeshInstance3D sourceMesh = null;

        foreach (var node in selected)
        {
            if (node is MultiMeshInstance3D m) mmi = m;
            if (node is MeshInstance3D s) sourceMesh = s;
        }

        if (mmi == null || sourceMesh == null)
        {
            GD.PrintErr("Select both a MultiMeshInstance3D and a MeshInstance3D surface.");
            return;
        }

        PopulateOnUpwardFaces(mmi, sourceMesh, amount: 128, normalThreshold: 0.7f);
    }

    private void PopulateOnUpwardFaces(MultiMeshInstance3D mmi, MeshInstance3D surface, int amount, float normalThreshold)
    {
        var faces = surface.Mesh.GetFaces();
        var upFaces = new System.Collections.Generic.List<(Vector3, Vector3, Vector3)>();

        for (int i = 0; i < faces.Length - 2; i += 3)
        {
            var a = faces[i];
            var b = faces[i + 1];
            var c = faces[i + 2];
            var normal = (b - a).Cross(c - a).Normalized();
            var worldNormal = (surface.GlobalTransform.Basis * -normal).Normalized();
            if (worldNormal.Dot(Vector3.Up) > normalThreshold)
                upFaces.Add((a, b, c));
        }

        if (upFaces.Count == 0)
        {
            GD.PrintErr("No upward-facing faces found.");
            return;
        }

        var mm = new MultiMesh();
        mm.Mesh = mmi.Multimesh?.Mesh;
        mm.TransformFormat = MultiMesh.TransformFormatEnum.Transform3D;
        mm.InstanceCount = amount;

        var rng = new RandomNumberGenerator();
        rng.Randomize();

        for (int i = 0; i < amount; i++)
        {
            var face = upFaces[rng.RandiRange(0, upFaces.Count - 1)];
            var point = RandomPointOnTriangle(face.Item1, face.Item2, face.Item3, rng);
            point = surface.GlobalTransform * point;
            point = mmi.GlobalTransform.AffineInverse() * point;
            var scale = 1.0f + rng.RandfRange(-0.3f, 0.3f);
            var rotation = rng.RandfRange(0, Mathf.Tau);

            var t = Transform3D.Identity
                .Rotated(Vector3.Up, rotation)
                .Scaled(Vector3.One * scale);
            t.Origin = point;

            mm.SetInstanceTransform(i, t);
        }

        mmi.Multimesh = mm;
        GD.Print($"Populated {amount} instances on {upFaces.Count} upward faces.");
    }

    private Vector3 RandomPointOnTriangle(Vector3 a, Vector3 b, Vector3 c, RandomNumberGenerator rng)
    {
        float r1 = Mathf.Sqrt(rng.Randf());
        float r2 = rng.Randf();
        return a * (1 - r1) + b * (r1 * (1 - r2)) + c * (r1 * r2);
    }
}
#endif
