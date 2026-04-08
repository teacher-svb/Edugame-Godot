#if TOOLS
using Godot;
using System.Collections.Generic;

public class RandomSampler : ISampler
{
    private readonly List<(Vector3, Vector3, Vector3, float)> _faces;
    private readonly GeometryInstance3D _surface;

    public RandomSampler(List<(Vector3, Vector3, Vector3, float)> faces, GeometryInstance3D surface)
    {
        _faces = faces;
        _surface = surface;
    }

    public Vector3 Sample(RandomNumberGenerator rng)
    {
        var face = _faces[rng.RandiRange(0, _faces.Count - 1)];
        var point = RandomPointOnTriangle(face.Item1, face.Item2, face.Item3, rng);
        return _surface.GlobalTransform * point;
    }

    private Vector3 RandomPointOnTriangle(Vector3 a, Vector3 b, Vector3 c, RandomNumberGenerator rng)
    {
        float r1 = Mathf.Sqrt(rng.Randf());
        float r2 = rng.Randf();
        return a * (1 - r1) + b * (r1 * (1 - r2)) + c * (r1 * r2);
    }
}
#endif