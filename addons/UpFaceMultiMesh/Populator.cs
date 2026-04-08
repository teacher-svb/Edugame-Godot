#if TOOLS
using Godot;
using System.Collections.Generic;

public class PopulatorOptions
{
    public int Amount { get; set; } = 128;
    public float BaseScale { get; set; } = 1.0f;
    public float RandomScale { get; set; } = 0.3f;
    public float RandomRotationDeg { get; set; } = 360f;
    public float RandomTiltDeg { get; set; } = 0f;
    public float MinDistance { get; set; } = 0f;
}

public enum SamplerType
{
    Random,
    WeightedByArea
}

public class Populator
{
    private MultiMeshInstance3D _mmi;
    private GeometryInstance3D _surface;
    private PopulatorOptions _options = new();
    private SamplerType _sampler = SamplerType.WeightedByArea;
    private float _normalThreshold = 0.7f;

    public Populator SelectMultimesh(MultiMeshInstance3D mmi)
    {
        _mmi = mmi;
        return this;
    }

    public Populator SelectSurface(GeometryInstance3D surface)
    {
        _surface = surface;
        return this;
    }

    public Populator SetOptions(PopulatorOptions options)
    {
        _options = options;
        return this;
    }

    public Populator SetSampler(SamplerType sampler)
    {
        _sampler = sampler;
        return this;
    }

    public Populator SetNormalThreshold(float threshold)
    {
        _normalThreshold = threshold;
        return this;
    }

    public void Execute()
    {
        Validate();

        var mesh = GetSurfaceMesh(_surface);
        var upFaces = GetUpwardFaces(mesh, _surface, _normalThreshold);

        if (upFaces.Count == 0)
        {
            GD.PrintErr("No upward-facing faces found.");
            return;
        }

        var rng = new RandomNumberGenerator();
        rng.Randomize();

        var sampler = CreateSampler(upFaces);
        var candidates = GenerateCandidates(sampler, rng);

        if (_options.MinDistance > 0f)
            candidates = FilterCandidates(candidates);

        ApplyToMultiMesh(candidates);
        GD.Print($"Populated {candidates.Count} instances.");
    }

    // --- Validation ---

    private void Validate()
    {
        if (_mmi == null) throw new System.Exception("No MultiMeshInstance3D selected.");
        if (_surface == null) throw new System.Exception("No surface selected.");
        if (_mmi.Multimesh == null || _mmi.Multimesh.Mesh == null)
            throw new System.Exception("MultiMeshInstance3D has no mesh assigned.");
    }

    // --- Mesh extraction ---

    private Mesh GetSurfaceMesh(GeometryInstance3D node) => node switch
    {
        MeshInstance3D mi => mi.Mesh,
        CsgShape3D csg => csg.GetMeshes() is { Count: >= 2 } m ? m[1].Obj as Mesh : null,
        _ => null
    };

    // --- Face collection ---

    private List<(Vector3, Vector3, Vector3, float)> GetUpwardFaces(Mesh mesh, GeometryInstance3D surface, float threshold)
    {
        var result = new List<(Vector3, Vector3, Vector3, float)>();
        var faces = mesh.GetFaces();

        for (int i = 0; i < faces.Length - 2; i += 3)
        {
            var a = faces[i];
            var b = faces[i + 1];
            var c = faces[i + 2];
            var normal = (b - a).Cross(c - a).Normalized();
            var worldNormal = (surface.GlobalTransform.Basis * -normal).Normalized();

            if (worldNormal.Dot(Vector3.Up) > threshold)
            {
                float area = (b - a).Cross(c - a).Length() * 0.5f;
                result.Add((a, b, c, area));
            }
        }

        return result;
    }

    // --- Samplers ---

    private ISampler CreateSampler(List<(Vector3, Vector3, Vector3, float)> faces) => _sampler switch
    {
        SamplerType.Random => new RandomSampler(faces, _surface),
        SamplerType.WeightedByArea => new WeightedSampler(faces, _surface),
        _ => throw new System.Exception("Unknown sampler type.")
    };

    // --- Candidate generation ---

    private List<Transform3D> GenerateCandidates(ISampler sampler, RandomNumberGenerator rng)
    {
        var candidates = new List<Transform3D>();

        for (int i = 0; i < _options.Amount; i++)
        {
            var point = sampler.Sample(rng);
            point = _mmi.GlobalTransform.AffineInverse() * point;

            float scale = _options.BaseScale + rng.RandfRange(-_options.RandomScale, _options.RandomScale);
            float rotY = rng.RandfRange(0, Mathf.DegToRad(_options.RandomRotationDeg));
            float tiltAngle = rng.RandfRange(0, Mathf.DegToRad(_options.RandomTiltDeg));
            float tiltDir = rng.RandfRange(0, Mathf.Tau);

            var t = Transform3D.Identity;
            t = t.Rotated(Vector3.Up, rotY);
            t = t.Rotated(new Vector3(Mathf.Cos(tiltDir), 0, Mathf.Sin(tiltDir)), tiltAngle);
            t = t.Scaled(Vector3.One * scale);
            t.Origin = point;

            candidates.Add(t);
        }

        return candidates;
    }

    // --- Distance filtering ---

    private List<Transform3D> FilterCandidates(List<Transform3D> candidates)
    {
        var existingPoints = GetExistingWorldPositions();
        var kept = new List<Transform3D>();
        float minDistSq = _options.MinDistance * _options.MinDistance;

        foreach (var candidate in candidates)
        {
            var worldPos = _mmi.GlobalTransform * candidate.Origin;
            bool tooClose = false;

            foreach (var kept_t in kept)
            {
                if (worldPos.DistanceSquaredTo(_mmi.GlobalTransform * kept_t.Origin) < minDistSq)
                {
                    tooClose = true;
                    break;
                }
            }

            if (!tooClose)
            {
                foreach (var existing in existingPoints)
                {
                    if (worldPos.DistanceSquaredTo(existing) < minDistSq)
                    {
                        tooClose = true;
                        break;
                    }
                }
            }

            if (!tooClose)
                kept.Add(candidate);
        }

        return kept;
    }

    private List<Vector3> GetExistingWorldPositions()
    {
        var points = new List<Vector3>();
        var parent = _mmi.GetParent();
        if (parent == null) return points;

        foreach (var child in parent.GetChildren())
        {
            if (child == _mmi) continue;

            if (child is MultiMeshInstance3D other && other.Multimesh != null)
                for (int i = 0; i < other.Multimesh.InstanceCount; i++)
                    points.Add(other.GlobalTransform * other.Multimesh.GetInstanceTransform(i).Origin);

            if (child is MeshInstance3D mi)
                points.Add(mi.GlobalPosition);
        }

        return points;
    }

    // --- Apply result ---

    private void ApplyToMultiMesh(List<Transform3D> transforms)
    {
        var mm = new MultiMesh();
        mm.Mesh = _mmi.Multimesh.Mesh;
        mm.TransformFormat = MultiMesh.TransformFormatEnum.Transform3D;
        mm.InstanceCount = transforms.Count;

        for (int i = 0; i < transforms.Count; i++)
            mm.SetInstanceTransform(i, transforms[i]);

        _mmi.Multimesh = mm;
    }
}
#endif